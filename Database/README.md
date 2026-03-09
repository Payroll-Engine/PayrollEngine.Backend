# PayrollEngine Database

## Overview

The PayrollEngine database is a SQL Server database that stores all tenant, regulation,
payroll, and result data. It is designed for multi-tenant operation where each tenant
represents an independent payroll client.

## Database Version

Current schema version: **0.9.6**

The schema version is stored in the `Version` table and verified by the Backend on startup.

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

### SQL Server Container

When running SQL Server in a container, set the server-level collation to match:

```
MSSQL_COLLATION=SQL_Latin1_General_CP1_CS_AS
```

Without this, `tempdb` operations (sorts, joins on temp tables) can produce collation
conflict errors.

## Parallel Payrun Processing and Deadlock Prevention

PayrollEngine processes employees in parallel (up to 16 threads). This creates two
categories of lock contention on result tables, each addressed by a different mechanism.

### Writer-Writer Deadlocks (solved by Bulk Insert Serialization)

When multiple threads perform `SqlBulkCopy` into the same result table (e.g.
`WageTypeCustomResult`), SQL Server acquires page-level X locks on both the clustered
index and non-clustered indexes. Different threads lock pages in different order,
causing classic cross-index page-lock deadlocks.

This is solved at the application level in `DbContext.BulkInsertAsync()` with a
`SemaphoreSlim(1, 1)` that serializes only the bulk insert phase. Employee calculations
remain fully parallel — only the I/O phase (milliseconds per employee) is serialized.

RCSI does **not** help with writer-writer deadlocks, as both sides hold exclusive locks.

### Reader-Writer Lock Waits (reduced by RCSI)

During a payrun, consolidated result queries (`GetConsolidatedWageTypeResults`,
`GetConsolidatedCollectorResults`) read historical data while other threads insert
new results. Without RCSI, these reads acquire shared locks that are blocked by
the exclusive locks from concurrent inserts — not a deadlock, but a wait that slows
the overall payrun.

RCSI eliminates these waits: reads use row versioning from `tempdb` instead of
shared locks, so they proceed without blocking and without blocking writers.
This also benefits external clients (reports, UI) querying data during a payrun.

## Read Committed Snapshot Isolation (RCSI)

RCSI is enabled to eliminate reader-writer lock waits during parallel payroll
processing (see above). It must be set immediately after `CREATE DATABASE`,
before any schema objects are created.

### Performance Impact

RCSI adds a small overhead on write operations: SQL Server stores a row version in
`tempdb` for every modified row (14 bytes per row plus the version copy). This typically
results in 2–5% overhead on INSERT/UPDATE-intensive workloads. For PayrollEngine,
the trade-off is clearly positive — the reduction in lock waits during parallel
processing far outweighs the `tempdb` cost.

To monitor `tempdb` usage after a payrun:

```sql
SELECT
    SUM(unallocated_extent_page_count) * 8 / 1024 AS free_mb,
    SUM(version_store_reserved_page_count) * 8 / 1024 AS version_store_mb
FROM sys.dm_db_file_space_usage;
```

With 40+ employees and 16 parallel threads, the version store typically stays in the
low single-digit MB range.

## Query Plan Stability

SQL Server's automatic statistics updates use a 20% row-change threshold before
recalculating histograms. With large datasets this threshold is rarely crossed,
causing the query optimizer to reuse stale plans (the *tipping point* problem:
the optimizer estimates too few rows and prefers an index seek + key lookup,
but actually does a full table scan).

PayrollEngine triggers two scenarios where statistics become stale quickly:

| Scenario | Cause | Trigger |
|---|---|---|
| Lookup bulk import | Thousands of `LookupValue` rows inserted at once | Automatic — `LookupSetRepository.CreateAsync` calls `UpdateStatisticsTargetedAsync` after `CreateBulkAsync` |
| Payrun result accumulation | Large load test or historical import writes many result rows | Explicit — call `ITenantRepository.UpdateStatisticsAsync` from setup scripts or after bulk imports |

Two stored procedures are provided:

| Procedure | Scope | Timeout | Use |
|---|---|---|---|
| `UpdateStatistics` | All 65 tables | 600 s | Manual maintenance, post-migration |
| `UpdateStatisticsTargeted` | 11 high-volume tables (see below) | 120 s | Automatic trigger after bulk imports |

**Targeted tables** (`UpdateStatisticsTargeted`):
- `LookupValue` — bulk-imported during regulation setup
- `PayrollResult`, `WageTypeResult`, `WageTypeCustomResult`, `CollectorResult`, `CollectorCustomResult`, `PayrunResult` — accumulate per payrun job
- `GlobalCaseValue`, `NationalCaseValue`, `CompanyCaseValue`, `EmployeeCaseValue` — grow with case change history

```sql
-- Full rebuild (all tables, use after migrations or large data imports)
EXEC UpdateStatistics;

-- Targeted rebuild (high-volume tables only, use after bulk imports)
EXEC UpdateStatisticsTargeted;
```

Both procedures are registered in `Procedures.cs` and exposed via:
- `IDbContext.UpdateStatisticsAsync()` — full rebuild, 600 s timeout
- `IDbContext.UpdateStatisticsTargetedAsync()` — targeted rebuild, 120 s timeout
- `ITenantRepository.UpdateStatisticsAsync(context)` — repository-level entry point for application code

## Schema

| Category            | Count | Description                                               |
|---------------------|------:|-----------------------------------------------------------|
| Tables              |    65 | Domain tables including audit tables for regulation objects |
| Functions           |     8 | Attribute queries, localization, derived regulation lookup  |
| Stored Procedures   |    43 | CRUD operations, derived objects, consolidated results, maintenance |
| Indexes             |  100+ | Unique constraints, foreign key indexes, query optimization |

## Scripts and Files

| File                | Purpose                                                         |
|---------------------|-----------------------------------------------------------------|
| `Create-Model.sql`  | Creates the database (if not exists) and all schema objects     |
| `Update-Model.sql`  | Migrates from the previous version (idempotent)                 |
| `Drop-Model.sql`    | Drops all schema objects (for development/reset only)           |
| `DbVersion.json`    | Version config for `Compose-DbScript.ps1` (OldVersion/NewVersion/Descriptions) |
| `History\v*\`      | Immutable snapshots of `Create-Model.sql`, `Update-Model.sql`, `Drop-Model.sql` and `DbVersion.json` per release |

### Create-Model.sql

Creates the database if it does not exist, sets collation and RCSI, then creates all
schema objects (functions, tables, indexes, defaults, foreign keys, stored procedures).
The script runs in `master` context first (for `CREATE DATABASE` and `ALTER DATABASE`),
then switches to `USE [PayrollEngine]` for schema creation.

**Structure (C# region style):**

```sql
-- #region DATABASE
USE [master];                   -- database setup (master context)
  CREATE DATABASE ...
  ALTER DATABASE ... RCSI
-- #endregion DATABASE

USE [PayrollEngine];            -- context switch (outside regions)

-- #region DB_SCRIPTS
  CREATE FUNCTION ...
  CREATE TABLE ...
  ...
-- #endregion DB_SCRIPTS

-- #region VERSION_SET          -- managed by Compose-DbScript.ps1
  INSERT INTO dbo.[Version] ...
-- #endregion VERSION_SET
```

**Important:** This script is for initial setup only. It is not idempotent — running it
against an existing database will produce "already exists" errors for all objects.

### Update-Model.sql

Migrates from a specific previous version to the current version.

**Structure (C# region style):**

```sql
USE [PayrollEngine];            -- context switch (before regions, safe to run from master)
GO

-- #region VERSION_CHECK        -- managed by Compose-DbScript.ps1
SET XACT_ABORT ON
DECLARE @MajorVersion int ...
IF @MajorVersion <> x ... BEGIN
    RAISERROR(...)
    SET NOEXEC ON               -- suppresses all subsequent batches
END
-- #endregion VERSION_CHECK

BEGIN TRANSACTION               -- opened only if VERSION_CHECK passed (NOEXEC skips it)
GO

-- #region DB_SCRIPTS           -- managed by Compose-DbScript.ps1
USE [PayrollEngine];
-- schema changes (ALTER TABLE, DROP/CREATE PROCEDURE ...)
-- #endregion DB_SCRIPTS

-- #region VERSION_SET          -- managed by Compose-DbScript.ps1
INSERT INTO dbo.[Version] (MajorVersion, MinorVersion, SubVersion, ...)
GO
COMMIT TRANSACTION
GO
SET NOEXEC OFF                  -- re-enables execution (no-op on success path)
-- #endregion VERSION_SET
```

The version check at the top ensures the script is only applied to the expected
source version. On a version mismatch `SET NOEXEC ON` suppresses all subsequent
batches — no transaction is opened and no changes are applied. Any unexpected
error during migration triggers an automatic rollback via `SET XACT_ABORT ON`.

### SQL Source Files

The authoritative source for functions and stored procedures is in the
`Persistence.SqlServer` project:

- `Persistence.SqlServer/Functions/` — 8 SQL functions
- `Persistence.SqlServer/StoredProcedures/` — 41 stored procedures

`ModelCreate.sql` is generated from these individual files.

---

## DevOps Scripts

PowerShell scripts in
[`devops/scripts`](https://github.com/Payroll-Engine/PayrollEngine/tree/main/devops/scripts)
support the database script lifecycle. See the devops
[README](https://github.com/Payroll-Engine/PayrollEngine/blob/main/devops/scripts/README.md)
for setup and global usage.

| Script                   | Purpose                                                            |
|--------------------------|--------------------------------------------------------------------|
| `Export-DbScript.ps1`    | Export live database schema to a SQL file                          |
| `Format-DbScript.ps1`    | Reorder DDL objects by dependency, normalize GO                    |
| `Compare-DbScript.ps1`   | Diff two formatted scripts, generate delta SQL                     |
| `Generate-DbUpdate.ps1`  | Format + Compare in one step — generate `ModelUpdate.sql` from two files |
| `Compose-DbScript.ps1`   | Inject version-check and version-set blocks from `DbVersion.json`  |

### Workflow: DB Merge

Transfers schema changes made directly in SQL Server back into `ModelCreate.sql`.
Use this when objects were modified in SSMS and the script must reflect the current
database state.

```powershell
$env:PayrollDatabaseConnection = 'Server=.;Database=PayrollEngine;Integrated Security=True;'

# 1. Export current database schema
Export-DbScript.ps1 -Mode Create -TargetFile Snapshot.sql

# 2. Generate delta (what exists in DB but differs from script)
Generate-DbUpdate.ps1 `
    -BaselineFile ModelCreate.sql `
    -CurrentFile  Snapshot.sql `
    -TargetFile   Delta.sql
```

Review `Delta.sql` and manually apply the relevant changes to `ModelCreate.sql`.

> **Version entry:** `ModelCreate.sql` contains a manually maintained
> `INSERT INTO [Version]` block at the end of the file. Update it when the schema
> version changes:
>
> ```sql
> INSERT INTO [Version] (MajorVersion, MinorVersion, SubVersion, [Owner], [Description])
> VALUES (0, 9, 6, CURRENT_USER, 'Payroll Engine: Full setup v0.9.6')
> ```

### Workflow: Update Script Creation

Creates `ModelUpdate.sql` for a release and snapshots both scripts into `History\`.

```powershell
# 1. Generate migration delta from the previous release snapshot
Generate-DbUpdate.ps1 `
    -BaselineFile History\v0.9.5\ModelCreate.sql `
    -CurrentFile  ModelCreate.sql `
    -TargetFile   ModelUpdate.sql

# 2. Review ModelUpdate.sql:
#    - Replace any '-- TODO' table comments with correct ALTER TABLE statements

# 3. Inject version-check and version-set blocks
Compose-DbScript.ps1 -ConfigFile DbVersion.json `
    -UpdateScriptSource ModelUpdate.sql `
    -UpdateScriptTarget ModelUpdate.Composed.sql

# 4. After release: snapshot all three scripts into History
#    (do this once the release is tagged)
mkdir History\v0.9.6
copy Create-Model.sql  History\v0.9.6\Create-Model.sql
copy Update-Model.sql  History\v0.9.6\Update-Model.sql
copy Drop-Model.sql    History\v0.9.6\Drop-Model.sql
copy DbVersion.json    History\v0.9.6\DbVersion.json

# 4. Advance DbVersion.json for the next release cycle
#    OldVersion = "0.9.6", NewVersion = "0.9.7"
```

**`Compose-DbScript.ps1`** calls `Generate-DbUpdate.ps1` internally — no manual
diff step required. Use `-DryRun` to preview and `-KeepTemp` to retain the
intermediate delta file for inspection.

### History Structure

```
Database\
  Create-Model.sql         ← current HEAD (always latest)
  Update-Model.sql         ← delta: previous release → HEAD
  Drop-Model.sql           ← current HEAD
  DbVersion.json           ← version config for Compose-DbScript.ps1

  History\
    v0.9.5\
      Create-Model.sql     ← immutable snapshot, frozen at release
      Update-Model.sql     ← delta v0.9.4 → v0.9.5
      Drop-Model.sql       ← drop script for this schema version
      DbVersion.json       ← version config used for this release
    v0.9.6\
      Create-Model.sql
      Update-Model.sql
      Drop-Model.sql
      DbVersion.json
```

History snapshots are committed to Git at release time and never modified afterwards.
Formatted and temp files (`*.Formatted.sql`, `*_Delta_*.sql`)
are not committed — add them to `.gitignore`.

---

## Database Creation

The database is created **outside** of `Create-Model.sql` in production deployments.
In the CI/CD pipeline, the `db-init` container handles database creation, collation
verification, and RCSI activation before running the schema scripts.

For local development, the `#region DATABASE` block at the top of `Create-Model.sql`
handles database creation with collation and RCSI.

```sql
-- Verify database settings
SELECT
    name,
    collation_name,
    is_read_committed_snapshot_on AS rcsi
FROM sys.databases
WHERE name = 'PayrollEngine';
```

## Key Domain Tables

| Table                | Description                                          |
|----------------------|------------------------------------------------------|
| `Tenant`             | Root entity, represents a payroll client              |
| `Regulation`         | Payroll regulation (country/company-specific rules)   |
| `Employee`           | Employee master data                                  |
| `Division`           | Organizational unit within a tenant                   |
| `Payroll`            | Payroll configuration with regulation layers          |
| `PayrollLayer`       | Links regulations to a payroll with priority           |
| `WageType`           | Wage type definition with calculation scripts         |
| `Collector`          | Aggregates wage type results                          |
| `Case` / `CaseField` | Data entry definitions with validation scripts       |
| `Lookup`             | Reference data tables used in calculations            |
| `Payrun`             | Payrun definition (calculation trigger)               |
| `PayrunJob`          | Execution record of a payrun                          |
| `PayrollResult`      | Per-employee result set for a payrun job              |
| `WageTypeResult`     | Individual wage type calculation result               |
| `CollectorResult`    | Aggregated collector result                           |
| `Report`             | Report definition with parameters and templates       |
| `Script`             | Compiled script assemblies (cached)                   |
| `Version`            | Database schema version tracking                      |
