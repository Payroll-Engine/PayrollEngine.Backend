# PayrollEngine Database

## Overview

The PayrollEngine database stores all tenant, regulation, payroll, and result data.
It is designed for multi-tenant operation where each tenant represents an independent payroll client.

## Database Providers

The provider is selected with `PayrollServerConfiguration:DbProvider` in the Backend `appsettings.json`.

| Provider   | `DbProvider`          | Minimum version | Create script            | Update script            |
|------------|-----------------------|-----------------|--------------------------|--------------------------|
| SQL Server | `SqlServer` (default) | 2019, Azure SQL | `Create-Model.sql`       | `Update-Model.sql`       |
| MySQL      | `MySql`               | 8.0 (8.4 LTS)   | `Create-Model.mysql.sql` | `Update-Model.mysql.sql` |
| PostgreSQL | `Postgres`            | 14              | `Create-Model.pg.sql`    | — (first release 1.1.0)  |

## Schema Version

Current schema version: **1.1.0**

The schema version is stored in the `Version` table. On startup, the Backend verifies:

| Check     | Rule                                                                 |
|-----------|----------------------------------------------------------------------|
| Version   | Latest `Version` entry must be **1.0.0 or newer**                    |
| Collation | Database collation must match the provider default (or `DbCollation`) |

| Provider   | Default collation              |
|------------|--------------------------------|
| SQL Server | `SQL_Latin1_General_CP1_CS_AS` |
| MySQL      | `utf8mb4_unicode_ci`           |
| PostgreSQL | `en_US.UTF-8`                  |

## Schema

| Category          | Count | Description                                                   |
|-------------------|------:|---------------------------------------------------------------|
| Tables            |    65 | Domain tables including audit tables for regulation objects   |
| Functions         |   7–8 | Attribute queries, localization, cluster matching             |
| Stored Procedures |    44 | Derived objects, case values, results, deletion, maintenance  |
| Indexes           | 64–80 | Unique constraints, foreign key indexes, query optimization   |

Functions and indexes differ slightly per provider. SQL Server has an additional inline
table-valued function (`GetDerivedRegulations`); in PostgreSQL, most stored procedures
are implemented as table-returning functions.

## Key Domain Tables

| Table                | Description                                           |
|----------------------|-------------------------------------------------------|
| `Tenant`             | Root entity, represents a payroll client              |
| `Regulation`         | Payroll regulation (country/company-specific rules)   |
| `RegulationShare`    | Cross-tenant regulation access                        |
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

## Version History

Each release is snapshotted into `History\v<version>\` (immutable).
The update script of the current version migrates from the previous release only —
it aborts if the database is not at the expected previous version.

| Folder             | Content                                                   |
|--------------------|-----------------------------------------------------------|
| `History\v1.1.0\`  | SQL Server, MySQL and PostgreSQL scripts of release 1.1.0 |
| `History\v1.0.0\`  | SQL Server and MySQL scripts of release 1.0.0             |
| `History\v0.9.x\`  | Earlier releases                                          |

To migrate across several releases, run the update scripts of each version in order.

## Statistics

Query optimizers rely on table statistics. With large datasets, automatic statistics
updates may lag behind (SQL Server, for example, uses a 20% row-change threshold),
causing the optimizer to reuse stale plans.

PayrollEngine triggers two scenarios where statistics become stale quickly:

| Scenario                   | Cause                                            | Trigger                                                                                           |
|----------------------------|--------------------------------------------------|---------------------------------------------------------------------------------------------------|
| Lookup bulk import         | Thousands of `LookupValue` rows inserted at once | Automatic — `LookupSetRepository.CreateAsync` calls `UpdateStatisticsTargetedAsync` after bulk insert |
| Payrun result accumulation | Large load test or historical import             | Explicit — call `ITenantRepository.UpdateStatisticsAsync` from setup scripts or after bulk imports |

Two stored procedures are provided:

| Procedure                   | Scope                    | Use                                       |
|-----------------------------|--------------------------|-------------------------------------------|
| `UpdateStatistics`          | All 65 tables            | Manual maintenance, post-migration        |
| `UpdateStatisticsTargeted`  | 11 high-volume tables    | Automatic trigger after bulk imports      |

| Provider   | Implementation                     |
|------------|------------------------------------|
| SQL Server | `UPDATE STATISTICS ... WITH FULLSCAN` |
| MySQL      | `ANALYZE TABLE`                    |
| PostgreSQL | `ANALYZE`                          |

**Targeted tables** (`UpdateStatisticsTargeted`):
`LookupValue`, `PayrollResult`, `WageTypeResult`, `WageTypeCustomResult`,
`CollectorResult`, `CollectorCustomResult`, `PayrunResult`,
`GlobalCaseValue`, `NationalCaseValue`, `CompanyCaseValue`, `EmployeeCaseValue`

Both procedures are exposed via:
- `IDbContext.UpdateStatisticsAsync()` — full rebuild
- `IDbContext.UpdateStatisticsTargetedAsync()` — targeted rebuild
- `ITenantRepository.UpdateStatisticsAsync(context)` — repository-level entry point

## Bulk Insert

Payrun results are written in bulk via `IDbContext.BulkInsertAsync()`:

| Provider   | Implementation                                                   |
|------------|------------------------------------------------------------------|
| SQL Server | `SqlBulkCopy`, serialized per process (see deadlock prevention)  |
| MySQL      | Multi-row `INSERT`, batches of 500 rows                          |
| PostgreSQL | Multi-row `INSERT`, batches of 500 rows                          |

## Docker

| Provider   | Location                                   | Content                                         |
|------------|--------------------------------------------|-------------------------------------------------|
| SQL Server | `PayrollEngine/docker-compose.yml`         | Full stack (database, init, Backend, WebApp)    |
| MySQL      | `Database/docker-mysql/`                   | Database only — create and update scenarios     |
| PostgreSQL | `Database/docker-pg/`                      | Database only — create scenario                 |

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

| File                                | Purpose                                                                          |
|-------------------------------------|----------------------------------------------------------------------------------|
| `Create-Model.sql`                  | Creates the database (if not exists) and all schema objects                      |
| `Update-Model.sql`                  | Migrates from the previous version (guarded by version check)                    |
| `Drop-Model.sql`                    | Drops all schema objects (for development/reset only)                            |
| `DbVersion.json`                    | Version config for `Generate-DbUpdate.ps1` (OldVersion/NewVersion/Descriptions) |
| `CreateModel.SqlServer.cmd`         | Runs `Create-Model.sql` against `localhost` (Windows authentication)             |
| `UpdateModel.SqlServer.cmd`         | Runs `Update-Model.sql` against `localhost` (Windows authentication)             |
| `DropModel.SqlServer.cmd`           | Drops the `PayrollEngine` database on `localhost` (Windows authentication)       |
| `Rebuild-CreateModel.SqlServer.ps1` | Rebuilds `Create-Model.sql` from the source files in dependency order            |
| `GenerateUpdateModel.SqlServer.cmd` | Generates `Update-Model.sql` via `Generate-DbUpdate.ps1`                         |

### SQL Source Files

The authoritative source for functions and stored procedures is in the `Persistence.SqlServer` project:

- `Persistence.SqlServer/Functions/` — 8 SQL functions
- `Persistence.SqlServer/StoredProcedures/` — 44 stored procedures

`Create-Model.sql` is generated from these individual files with `Rebuild-CreateModel.SqlServer.ps1`.

### Create-Model.sql Structure

```sql
-- #region DATABASE
USE [master];                   -- database setup (master context)
  CREATE DATABASE ...
  ALTER DATABASE ... RCSI
-- #endregion DATABASE

USE [PayrollEngine];

-- #region DB_SCRIPTS
  CREATE TABLE ...
  CREATE INDEX ...
  ALTER TABLE ... ADD CONSTRAINT ...
  CREATE FUNCTION ...           -- after tables (inline TVFs bind eagerly)
  CREATE PROCEDURE ...
-- #endregion DB_SCRIPTS

-- VERSION_SET
  INSERT INTO dbo.[Version] ...
```

### Update-Model.sql Structure

```sql
USE [PayrollEngine];
GO
SET XACT_ABORT ON
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

PowerShell scripts in `devops/scripts` of the `PayrollEngine` repository:

| Script                        | Purpose                                                                         |
|-------------------------------|---------------------------------------------------------------------------------|
| `Export-DbScript.ps1`         | Export live database schema to a SQL file                                       |
| `Format-DbScript.ps1`         | Reorder DDL objects by dependency, normalize GO                                 |
| `Compare-DbScript.ps1`        | Diff two formatted scripts, generate delta SQL                                  |
| `Generate-DbUpdate.ps1`       | Full pipeline: reads `DbVersion.json`, diffs history vs. current, writes `Update-Model.sql` |
| `Generate-DbUpdate.mysql.ps1` | MySQL variant, writes `Update-Model.mysql.sql`                                  |

### Workflow: Update Script Creation

```powershell
cd PayrollEngine.Backend\Database

# 1. Generate update scripts
.\GenerateUpdateModel.SqlServer.cmd
.\GenerateUpdateModel.MySql.cmd

# 2. Review the update scripts, check for '-- TODO' comments,
#    test the migration against the previous release (see docker-mysql/README.md)

# 3. After release: snapshot into History
mkdir History\v1.1.0
copy Create-Model.sql        History\v1.1.0\
copy Update-Model.sql        History\v1.1.0\
copy Drop-Model.sql          History\v1.1.0\
copy Create-Model.mysql.sql  History\v1.1.0\
copy Update-Model.mysql.sql  History\v1.1.0\
copy Drop-Model.mysql.sql    History\v1.1.0\
copy Create-Model.pg.sql     History\v1.1.0\
copy DbVersion.json          History\v1.1.0\

# 4. Advance DbVersion.json for next cycle
#    OldVersion = "1.1.0", NewVersion = "1.1.1"
```

## Docker

The full stack in the `PayrollEngine` repository (`docker-compose.yml`, `docker-compose.ghcr.yml`)
runs SQL Server with a `db-init` container that creates the database and executes `Create-Model.sql`.

---

# MySQL

## Requirements

- **MySQL** 8.0 or later (8.4 LTS recommended)
- **Docker** (recommended for local development)
- **Character set:** `utf8mb4`, **Collation:** `utf8mb4_unicode_ci`

## Configuration

```json
"PayrollServerConfiguration": {
  "DbProvider": "MySql"
},
"ConnectionStrings": {
  "PayrollDatabaseConnection": "Server=localhost;Port=3306;Database=PayrollEngine;User=root;Password=...;CharSet=utf8mb4;"
}
```

## Scripts and Files

| File                             | Purpose                                                                  |
|----------------------------------|--------------------------------------------------------------------------|
| `Create-Model.mysql.sql`         | Creates the database, all tables, indexes, functions, and stored procedures |
| `Update-Model.mysql.sql`         | Migrates from the previous version (guarded by version check)            |
| `Drop-Model.mysql.sql`           | Drops all schema objects                                                 |
| `Rebuild-CreateModel.MySql.ps1`  | Rebuilds `Create-Model.mysql.sql` from the source files in dependency order |
| `CreateModel.MySql.ps1/.cmd`     | Drops and recreates the database from `Create-Model.mysql.sql`           |
| `UpdateModel.MySql.cmd`          | Runs `Update-Model.mysql.sql` in the container                           |
| `DropModel.MySql.cmd`            | Drops the database in the container                                      |
| `GenerateUpdateModel.MySql.cmd`  | Generates `Update-Model.mysql.sql` via `Generate-DbUpdate.mysql.ps1`     |
| `Merge-MySql-Routines.cmd`       | Merges all routine source files into `MySql-Routines.merged.sql` (review aid) |

The container helper scripts default to the container `pe-poc` (password `poc123`).
For the Docker Compose setup, pass the container name: `.\CreateModel.MySql.ps1 -Container pe-mysql`.

### SQL Source Files

The authoritative source for functions and stored procedures is in the `Persistence.MySql` project:

- `Persistence.MySql/Functions/` — 7 MySQL functions (`*.mysql.sql`)
- `Persistence.MySql/StoredProcedures/` — 44 MySQL stored procedures (`*.mysql.sql`)

`Create-Model.mysql.sql` embeds these files inline. Changes to a source file must also be
applied to `Update-Model.mysql.sql` — otherwise migrated databases keep the old routine.

## Development Workflow

```powershell
# 1. After editing any Function or StoredProcedure source file:
.\Rebuild-CreateModel.MySql.ps1

# 2. Recreate the database:
.\CreateModel.MySql.ps1 -Container pe-mysql
```

`CreateModel.MySql.ps1` drops the existing `PayrollEngine` database and recreates it
from `Create-Model.mysql.sql`, then prints a verification summary:

```
tables: 65
FUNCTION: 7
PROCEDURE: 44
MajorVersion: 1, MinorVersion: 0, SubVersion: 1
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

## Docker

Docker Compose files for the create and the update scenario are provided in `Database/docker-mysql/`.
The init script is mounted directly and runs automatically on first start.

```powershell
cd Database\docker-mysql
docker compose -p pe-full up -d --wait
```

See `Database/docker-mysql/README.md` for the update scenario, schema comparison and configuration.

## MySQL-Specific Notes

### Case sensitivity
`utf8mb4_unicode_ci` is case-insensitive. Unlike SQL Server and PostgreSQL, name
comparisons in MySQL do not distinguish upper and lower case.

### CAST in JSON_TABLE
MySQL requires `CAST(... AS SIGNED)` for integer values extracted from `JSON_TABLE`.
`CAST(... AS INT)` is not supported before MySQL 8.0.17.

### DELIMITER $$
All functions and stored procedures use `DELIMITER $$` to avoid conflicts with
`$` characters in JSON PATH expressions (e.g. `'$[*]'`). `DELIMITER` is a `mysql` client
command — run the scripts with the `mysql` client, not via a driver.

### PowerShell redirection
PowerShell does not support `<` input redirection, and piping via `Get-Content` may alter
the encoding. Use `cmd /c` for script execution:

```powershell
cmd /c "docker exec -i pe-mysql mysql --comments -uroot -ppoc123 PayrollEngine < Update-Model.mysql.sql"
```

### Reserved words
The following column names require backtick quoting in MySQL:
`` `User` ``, `` `Key` ``, `` `Order` ``, `` `Schema` ``, `` `Binary` ``, `` `Case` ``

---

# PostgreSQL

## Requirements

- **PostgreSQL** 14 or later (Docker setup uses 17)
- **Encoding:** `UTF8`, **Locale/Collation:** `en_US.UTF-8`

## Configuration

```json
"PayrollServerConfiguration": {
  "DbProvider": "Postgres"
},
"ConnectionStrings": {
  "PayrollDatabaseConnection": "Host=localhost;Port=5432;Database=PayrollEngine;Username=payroll;Password=...;"
}
```

With `DbProvider=Postgres`, the Backend enables the Npgsql switch
`Npgsql.EnableLegacyTimestampBehavior` on startup, so non-UTC `DateTime` values are accepted.

## Scripts and Files

| File                  | Purpose                                                                  |
|-----------------------|--------------------------------------------------------------------------|
| `Create-Model.pg.sql` | Creates all tables, indexes, functions, and stored procedures            |

The database itself is not created by the script — create it beforehand with the required
locale (the Docker setup does this via `POSTGRES_DB` and `POSTGRES_INITDB_ARGS`).

PostgreSQL support starts with schema version 1.1.0 (snapshot `History\v1.1.0\Create-Model.pg.sql`).
An update script will be provided from the next release onwards.

### SQL Source Files

The authoritative source for functions and stored procedures is in the `Persistence.Postgres` project:

- `Persistence.Postgres/Functions/` — 7 functions (`*.pg.sql`)
- `Persistence.Postgres/StoredProcedures/` — 44 stored procedures (`*.pg.sql`)

There is no rebuild script yet: changes to a source file must also be applied to
`Create-Model.pg.sql`, and `docker-pg/Prepare-Init.cmd` must be re-run.

## Verification

```sql
SELECT COUNT(*) AS tables
FROM information_schema.tables
WHERE table_schema = 'public' AND table_type = 'BASE TABLE';

SELECT routine_type, COUNT(*) AS count
FROM information_schema.routines
WHERE routine_schema = 'public'
GROUP BY routine_type;

SELECT "MajorVersion", "MinorVersion", "SubVersion"
FROM "Version" ORDER BY "Id" DESC LIMIT 1;
```

Expected: 65 tables, 40 functions and 11 procedures (PostgreSQL routine types), version 1.1.0.

## Docker

A Docker Compose setup is provided in `Database/docker-pg/`:

```powershell
cd Database\docker-pg
Prepare-Init.cmd
docker compose up -d
```

See `Database/docker-pg/README.md` for details.

## PostgreSQL-Specific Notes

### Quoted identifiers
All table and column names are created double-quoted (e.g. `"WageType"."Name"`) to preserve
the PascalCase names used by the C# query layer. Unquoted identifiers are folded to lower case
by PostgreSQL — every reference in functions and procedures must be quoted as well.

### Case sensitivity
The default deterministic collation compares case-sensitively, matching the SQL Server behavior.

### Functions vs. procedures
Routines returning result sets are implemented as table-returning functions (`RETURNS TABLE`);
routines without results (deletion, statistics) are procedures (`CALL`).
