# PayrollEngine Database

## Overview

The PayrollEngine database stores all tenant, regulation, payroll, and result data.
It is designed for multi-tenant operation where each tenant represents an independent payroll client.

## Schema Version

Current schema version: **0.9.6**

The schema version is stored in the `Version` table and verified by the Backend on startup.

## Schema

| Category          | Count | Description                                                   |
|-------------------|------:|---------------------------------------------------------------|
| Tables            |    65 | Domain tables including audit tables for regulation objects   |
| Functions         |     7 | Attribute queries, localization, cluster matching             |
| Stored Procedures |    44 | CRUD, derived objects, consolidated results, maintenance      |
| Indexes           |  100+ | Unique constraints, foreign key indexes, query optimization   |

## Key Domain Tables

| Table                | Description                                           |
|----------------------|-------------------------------------------------------|
| `Tenant`             | Root entity, represents a payroll client              |
| `Regulation`         | Payroll regulation (country/company-specific rules)   |
| `Employee`           | Employee master data                                  |
| `Division`           | Organizational unit within a tenant                   |
| `Payroll`            | Payroll configuration with regulation layers          |
| `PayrollLayer`       | Links regulations to a payroll with priority          |
| `WageType`           | Wage type definition with calculation scripts         |
| `Collector`          | Aggregates wage type results                          |
| `Case` / `CaseField` | Data entry definitions with validation scripts        |
| `Lookup`             | Reference data tables used in calculations            |
| `Payrun`             | Payrun definition (calculation trigger)               |
| `PayrunJob`          | Execution record of a payrun                          |
| `PayrollResult`      | Per-employee result set for a payrun job              |
| `WageTypeResult`     | Individual wage type calculation result               |
| `CollectorResult`    | Aggregated collector result                           |
| `Report`             | Report definition with parameters and templates       |
| `Script`             | Compiled script assemblies (cached)                   |
| `Version`            | Database schema version tracking                      |

---

# Common

## Statistics

SQL Server's automatic statistics updates use a 20% row-change threshold before
recalculating histograms. With large datasets this threshold is rarely crossed,
causing the query optimizer to reuse stale plans.

PayrollEngine triggers two scenarios where statistics become stale quickly:

| Scenario               | Cause                                                | Trigger                                                                                           |
|------------------------|------------------------------------------------------|---------------------------------------------------------------------------------------------------|
| Lookup bulk import     | Thousands of `LookupValue` rows inserted at once     | Automatic — `LookupSetRepository.CreateAsync` calls `UpdateStatisticsTargetedAsync` after bulk insert |
| Payrun result accumulation | Large load test or historical import             | Explicit — call `ITenantRepository.UpdateStatisticsAsync` from setup scripts or after bulk imports |

Two stored procedures are provided:

| Procedure                   | Scope                    | Use                                       |
|-----------------------------|--------------------------|-------------------------------------------|
| `UpdateStatistics`          | All 65 tables            | Manual maintenance, post-migration        |
| `UpdateStatisticsTargeted`  | 11 high-volume tables    | Automatic trigger after bulk imports      |

**Targeted tables** (`UpdateStatisticsTargeted`):
`LookupValue`, `PayrollResult`, `WageTypeResult`, `WageTypeCustomResult`,
`CollectorResult`, `CollectorCustomResult`, `PayrunResult`,
`GlobalCaseValue`, `NationalCaseValue`, `CompanyCaseValue`, `EmployeeCaseValue`

Both procedures are exposed via:
- `IDbContext.UpdateStatisticsAsync()` — full rebuild
- `IDbContext.UpdateStatisticsTargetedAsync()` — targeted rebuild
- `ITenantRepository.UpdateStatisticsAsync(context)` — repository-level entry point

---

# SQL Server

## Requirements

- **SQL Server** 2019 or later (including Azure SQL)
- **Collation:** `SQL_Latin1_General_CP1_CS_AS` (case-sensitive)
- **Isolation Level:** `READ_COMMITTED_SNAPSHOT ON` (RCSI)

## Collation

The database uses `SQL_Latin1_General_CP1_CS_AS` (case-sensitive, accent-sensitive).
This is required because regulation names, wage type names, collector names, and case
field names serve as cross-object references and must be matched case-sensitively.

Case values (e.g. employee street, city) are stored as JSON inside `NVARCHAR(MAX)` columns
and extracted at query time via SQL functions. For user-facing filtering, the Backend
applies `COLLATE SQL_Latin1_General_CP1_CI_AS` at query level on text attribute values.

When running SQL Server in a container, set the server-level collation to match:

```
MSSQL_COLLATION=SQL_Latin1_General_CP1_CS_AS
```

Without this, `tempdb` operations (sorts, joins on temp tables) can produce collation conflict errors.

## Read Committed Snapshot Isolation (RCSI)

RCSI is enabled to eliminate reader-writer lock waits during parallel payroll processing.
It must be set immediately after `CREATE DATABASE`, before any schema objects are created.

RCSI adds a small overhead on write operations: SQL Server stores a row version in
`tempdb` for every modified row (14 bytes per row plus the version copy). This typically
results in 2–5% overhead on INSERT/UPDATE-intensive workloads. For PayrollEngine,
the trade-off is clearly positive.

```sql
-- Verify database settings
SELECT name, collation_name, is_read_committed_snapshot_on AS rcsi
FROM sys.databases
WHERE name = 'PayrollEngine';
```

## Parallel Payrun Processing and Deadlock Prevention

PayrollEngine processes employees in parallel (up to 16 threads), creating two categories of lock contention.

### Writer-Writer Deadlocks (solved by Bulk Insert Serialization)

When multiple threads perform `SqlBulkCopy` into the same result table, SQL Server acquires
page-level X locks on both the clustered index and non-clustered indexes in different order,
causing cross-index page-lock deadlocks.

Solved at application level in `DbContext.BulkInsertAsync()` with a `SemaphoreSlim(1, 1)`
that serializes only the bulk insert phase. Employee calculations remain fully parallel.

### Reader-Writer Lock Waits (reduced by RCSI)

During a payrun, consolidated result queries read historical data while other threads insert
new results. RCSI eliminates these waits via row versioning from `tempdb`.

## Scripts and Files

| File                | Purpose                                                                         |
|---------------------|---------------------------------------------------------------------------------|
| `Create-Model.sql`  | Creates the database (if not exists) and all schema objects                     |
| `Update-Model.sql`  | Migrates from the previous version (idempotent)                                 |
| `Drop-Model.sql`    | Drops all schema objects (for development/reset only)                           |
| `DbVersion.json`    | Version config for `Generate-DbUpdate.ps1` (OldVersion/NewVersion/Descriptions)|
| `History\v*\`       | Immutable snapshots per release                                                 |

### SQL Source Files

The authoritative source for functions and stored procedures is in the `Persistence.SqlServer` project:

- `Persistence.SqlServer/Functions/` — 7 SQL functions
- `Persistence.SqlServer/StoredProcedures/` — 44 stored procedures

`Create-Model.sql` is generated from these individual files.

### Create-Model.sql Structure

```sql
-- #region DATABASE
USE [master];                   -- database setup (master context)
  CREATE DATABASE ...
  ALTER DATABASE ... RCSI
-- #endregion DATABASE

USE [PayrollEngine];

-- #region DB_SCRIPTS
  CREATE FUNCTION ...
  CREATE TABLE ...
  ...
-- #endregion DB_SCRIPTS

-- VERSION_SET
  INSERT INTO dbo.[Version] ...
```

### Update-Model.sql Structure

```sql
USE [PayrollEngine];
GO
-- VERSION_CHECK (fails with RAISERROR + SET NOEXEC ON on version mismatch)

BEGIN TRANSACTION
GO
-- schema changes (ALTER TABLE, DROP/CREATE PROCEDURE ...)

-- VERSION_SET
INSERT INTO dbo.[Version] ...
GO
COMMIT TRANSACTION
GO
SET NOEXEC OFF
```

## DevOps Scripts

PowerShell scripts in `devops/scripts`:

| Script                  | Purpose                                                                         |
|-------------------------|---------------------------------------------------------------------------------|
| `Export-DbScript.ps1`   | Export live database schema to a SQL file                                       |
| `Format-DbScript.ps1`   | Reorder DDL objects by dependency, normalize GO                                 |
| `Compare-DbScript.ps1`  | Diff two formatted scripts, generate delta SQL                                  |
| `Generate-DbUpdate.ps1` | Full pipeline: reads `DbVersion.json`, diffs history vs. current, writes `Update-Model.sql` |

### Workflow: Update Script Creation

```powershell
cd Backend\Database

# 1. Generate Update-Model.sql
..\..\devops\scripts\Generate-DbUpdate.ps1

# 2. Review Update-Model.sql, check for '-- TODO' comments

# 3. After release: snapshot into History
mkdir History\v0.9.6
copy Create-Model.sql  History\v0.9.6\
copy Update-Model.sql  History\v0.9.6\
copy Drop-Model.sql    History\v0.9.6\
copy DbVersion.json    History\v0.9.6\

# 4. Advance DbVersion.json for next cycle
#    OldVersion = "0.9.6", NewVersion = "0.9.7"
```

---

# MySQL

## Requirements

- **MySQL** 8.0 or later (8.4 LTS recommended)
- **Docker** (recommended for local development)
- **Character set:** `utf8mb4`, **Collation:** `utf8mb4_unicode_ci`

## Scripts and Files

| File                         | Purpose                                                    |
|------------------------------|------------------------------------------------------------|
| `Create-Model.mysql.sql`     | Creates the database, all tables, indexes, functions, and stored procedures |
| `Drop-Model.mysql.sql`       | Drops all schema objects                                   |
| `Merge-Model.mysql.ps1/.cmd` | Rebuilds the routines section from source files            |
| `CreateModel.MySql.ps1/.cmd` | Drops and recreates the database from `Create-Model.mysql.sql` |

### SQL Source Files

The authoritative source for functions and stored procedures is in the `Persistence.MySql` project:

- `Persistence.MySql/Functions/` — 7 MySQL functions (`*.mysql.sql`)
- `Persistence.MySql/StoredProcedures/` — 44 MySQL stored procedures (`*.mysql.sql`)

`Create-Model.mysql.sql` embeds these files inline. Use `Merge-Model.mysql.cmd` to
rebuild the routines section from the current source files after any changes.

## Development Workflow

```cmd
:: 1. After editing any Function or StoredProcedure source file:
Database\Merge-Model.mysql.cmd

:: 2. Recreate the database:
Database\CreateModel.MySql.cmd
```

`Merge-Model.mysql.cmd` replaces the routines block in `Create-Model.mysql.sql`
(between `-- FUNCTIONS` and `-- VERSION RECORD`) with the current content of all
`*.mysql.sql` source files, alphabetically ordered.

`CreateModel.MySql.cmd` drops the existing `PayrollEngine` database and recreates it
from `Create-Model.mysql.sql`, then prints a verification summary:

```
tables: 65
FUNCTION: 7
PROCEDURE: 44
MajorVersion: 0, MinorVersion: 9, SubVersion: 6
```

## Container Setup (PoC / Local Development)

| Parameter  | Value                      |
|------------|----------------------------|
| Container  | `pe-poc`                   |
| Image      | `mysql:8.0`                |
| Port       | `3306`                     |
| Password   | `poc123`                   |

```cmd
:: Test connection
docker exec -it pe-poc mysql -uroot -ppoc123 -e "SELECT 1;"
```

## Verification

```sql
SELECT COUNT(*) AS tables
FROM information_schema.TABLES
WHERE TABLE_SCHEMA = 'PayrollEngine' AND TABLE_TYPE = 'BASE TABLE';

SELECT ROUTINE_TYPE, COUNT(*) AS count
FROM information_schema.ROUTINES
WHERE ROUTINE_SCHEMA = 'PayrollEngine'
GROUP BY ROUTINE_TYPE;

SELECT MajorVersion, MinorVersion, SubVersion
FROM PayrollEngine.Version ORDER BY Id DESC LIMIT 1;
```

## Docker Compose (CI/CD)

A `docker-compose.yml` is provided in `Database/docker/` for clean-room environments
that initializes the database automatically on first start.

```
docker/
  docker-compose.yml    Container definition (mysql:8.0, healthcheck, volume)
  Prepare-Init.cmd      Copies Create-Model.mysql.sql into init/
  init/                 Generated: init script (run once on empty volume)
```

```cmd
cd Database\docker
Prepare-Init.cmd        :: run once, and after any script update
docker compose up -d
```

| Situation                                | Behavior                            |
|------------------------------------------|-------------------------------------|
| First `docker compose up` (empty volume) | Init script runs automatically      |
| Subsequent `docker compose up`           | Init script is skipped              |
| Full reset                               | `docker compose down -v` then `up`  |

| Variable              | Default  |
|-----------------------|----------|
| `MYSQL_ROOT_PASSWORD` | `poc123` |
| `MYSQL_PORT`          | `3306`   |

## MySQL-Specific Notes

### CAST in JSON_TABLE
MySQL requires `CAST(... AS SIGNED)` for integer values extracted from `JSON_TABLE`.
`CAST(... AS INT)` is not supported before MySQL 8.0.17.

### DELIMITER $$
All functions and stored procedures use `DELIMITER $$` to avoid conflicts with
`$` characters in JSON PATH expressions (e.g. `'$[*]'`).

### Reserved words
The following column names require backtick quoting in MySQL:
`` `User` ``, `` `Key` ``, `` `Order` ``, `` `Schema` ``, `` `Binary` ``, `` `Case` ``
