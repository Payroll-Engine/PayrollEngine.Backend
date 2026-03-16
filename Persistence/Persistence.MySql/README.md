# PayrollEngine.Persistence.MySql

MySQL 8.0+ persistence backend for the Payroll Engine.
Implements `IDbContext` using `MySqlConnector` and Dapper.

## Status

Complete — all phases delivered and validated in MySQL 8.4.8 LTS.
- 65 tables, ~60 indexes, 7 functions, 30 stored procedures
- Build: clean (net10.0)
- DDL validation: 65/65 tables, 37/37 routines

## Requirements

- MySQL 8.0.13+ (8.4 LTS recommended)
- `utf8mb4_unicode_ci` collation
- `MySqlConnector` NuGet package (2.4.0+)

## Database Setup

```bash
# Create schema
mysql -uroot -p PayrollEngine < Database/Create-Model.mysql.sql

# Update existing 0.9.5 schema
mysql -uroot -p PayrollEngine < Database/Update-Model.mysql.sql

# Drop schema
mysql -uroot -p PayrollEngine < Database/Drop-Model.mysql.sql
```

## Connection String

```
Server=localhost;Port=3306;Database=PayrollEngine;User=payroll;Password=...;CharSet=utf8mb4;
```

## Key Differences to SQL Server

| T-SQL | MySQL |
|---|---|
| `NVARCHAR(MAX)` | `LONGTEXT` |
| `DATETIME2(7)` | `DATETIME(6)` |
| `VARBINARY(MAX)` | `LONGBLOB` |
| `BIT` | `TINYINT(1)` |
| `IDENTITY(1,1)` | `AUTO_INCREMENT` |
| `[dbo].[Table]` | `` `Table` `` (backtick for reserved words) |
| `Binary` column name | `` `Binary` `` -- reserved keyword in MySQL 8.4 |
| Inline TVF `RETURNS TABLE` | Inline CTE in each SP |
| `##GlobalTempTable` | `CREATE TEMPORARY TABLE` (session-scoped) |
| `sp_executesql @sql` | `PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;` |
| `OPENJSON(@j) WHERE [key]=N` | `JSON_TABLE(@j, '$[*]' COLUMNS ...)` |
| `JSON_VALUE(col, '$.key')` | `JSON_UNQUOTE(JSON_EXTRACT(col, '$.key'))` |
| `$."de-CH"` (hyphenated keys) | `JSON_EXTRACT(col, '$."de-CH"')` — quoted key syntax required |
| `BEGIN TRY / END CATCH` | `DECLARE EXIT HANDLER FOR SQLEXCEPTION` |
| `OPTION (RECOMPILE)` | removed |
| `UPDATE STATISTICS ... WITH FULLSCAN` | `ANALYZE TABLE` |

## PoC Results

All 4 critical blockers validated (35/35 tests passed):
- Blocker 1: Inline TVF -> CTE ✅
- Blocker 2: `OPENJSON` WHILE-Loop -> `JSON_TABLE` ✅
- Blocker 3: `##GlobalTempTable` -> Session `TEMPORARY TABLE` ✅
- Blocker 4: `sp_executesql` -> `PREPARE/EXECUTE` ✅

See `PayrollEngine.Private/database/poc/Results.md` for details.
