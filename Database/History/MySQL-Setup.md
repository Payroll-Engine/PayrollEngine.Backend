# PayrollEngine MySQL — Setup & Update

## Scenarios

### Create a new database

```powershell
docker exec -i pe-poc mysql -uroot -ppoc123 -e "DROP DATABASE IF EXISTS PayrollEngine;"; Get-Content "C:\Shared\PayrollEngine\Repos\PayrollEngine.Backend\Database\Create-Model.mysql.sql" | docker exec -i pe-poc mysql -uroot -ppoc123; Get-Content "C:\Shared\PayrollEngine\Repos\PayrollEngine.Backend\Persistence\Persistence.MySql\Functions\Functions.mysql.sql" | docker exec -i pe-poc mysql -uroot -ppoc123 PayrollEngine; Get-Content "C:\Shared\PayrollEngine\Repos\PayrollEngine.Backend\Persistence\Persistence.MySql\StoredProcedures\GetDerived.mysql.sql" | docker exec -i pe-poc mysql -uroot -ppoc123 PayrollEngine; Get-Content "C:\Shared\PayrollEngine\Repos\PayrollEngine.Backend\Persistence\Persistence.MySql\StoredProcedures\GetCaseValues.mysql.sql" | docker exec -i pe-poc mysql -uroot -ppoc123 PayrollEngine; Get-Content "C:\Shared\PayrollEngine\Repos\PayrollEngine.Backend\Persistence\Persistence.MySql\StoredProcedures\GetLookupRangeValue.mysql.sql" | docker exec -i pe-poc mysql -uroot -ppoc123 PayrollEngine; Get-Content "C:\Shared\PayrollEngine\Repos\PayrollEngine.Backend\Persistence\Persistence.MySql\StoredProcedures\Remaining.mysql.sql" | docker exec -i pe-poc mysql -uroot -ppoc123 PayrollEngine
```

### Update an existing database

```powershell
Get-Content "C:\Shared\PayrollEngine\Repos\PayrollEngine.Backend\Database\Update-Model.mysql.sql" | docker exec -i pe-poc mysql -uroot -ppoc123 PayrollEngine
```

### Drop the database

```powershell
docker exec -i pe-poc mysql -uroot -ppoc123 -e "DROP DATABASE IF EXISTS PayrollEngine;"
```

---

## Lessons Learned

### PowerShell input redirect

PowerShell does not support the `<` input redirect. Use `Get-Content` piped to `docker exec` instead:

```powershell
# Wrong — does not work in PowerShell
docker exec -i pe-poc mysql -uroot -ppoc123 < script.sql

# Correct
Get-Content "script.sql" | docker exec -i pe-poc mysql -uroot -ppoc123 PayrollEngine
```

### Password must be hardcoded

PowerShell variables are not reliably interpolated inside the `-p` argument. Pass the password directly without a space:

```powershell
# Wrong — variable interpolation may silently fail
$pw = "poc123"
docker exec -i pe-poc mysql -uroot -p$pw PayrollEngine

# Correct
docker exec -i pe-poc mysql -uroot -ppoc123 PayrollEngine
```

### Drop = DROP DATABASE

`Drop-Model.mysql.sql` only drops tables, stored procedures and functions — the database itself remains. For a complete reset, drop the database directly:

```powershell
docker exec -i pe-poc mysql -uroot -ppoc123 -e "DROP DATABASE IF EXISTS PayrollEngine;"
```

### Script execution order

`Create-Model.mysql.sql` contains only tables and indexes. Functions must be applied **before** stored procedures because `GetDerived.mysql.sql` depends on `IsMatchingCluster` and `BuildAttributeQuery`.

| Step | File | Content |
|---|---|---|
| 1 | `Database/Create-Model.mysql.sql` | 65 tables, ~60 indexes |
| 2 | `Functions/Functions.mysql.sql` | 7 functions |
| 3 | `StoredProcedures/GetDerived.mysql.sql` | 13 derived SPs |
| 4 | `StoredProcedures/GetCaseValues.mysql.sql` | 8 CaseValue pivot SPs |
| 5 | `StoredProcedures/GetLookupRangeValue.mysql.sql` | 1 SP |
| 6 | `StoredProcedures/Remaining.mysql.sql` | `GetPayrollResultValues`, `GetEmployeeCaseValuesByTenant`, `Delete*`, `UpdateStatistics` |

---

## Verification

```powershell
docker exec -i pe-poc mysql -uroot -ppoc123 -e "SELECT COUNT(*) AS tables FROM information_schema.TABLES WHERE TABLE_SCHEMA='PayrollEngine' AND TABLE_TYPE='BASE TABLE'; SELECT COUNT(*) AS routines FROM information_schema.ROUTINES WHERE ROUTINE_SCHEMA='PayrollEngine'; SELECT MajorVersion, MinorVersion, SubVersion FROM PayrollEngine.Version ORDER BY Id DESC LIMIT 1;"
```

Expected values:

| Metric | Expected |
|---|---|
| Tables | 65 |
| Routines | ~37 |
| Version | 0.9.6 |

---

## Container Info (PoC)

| Parameter | Value |
|---|---|
| Container | `pe-poc` |
| Image | `mysql:8.0` |
| Port | `3306` |
| Root password | `poc123` (`MYSQL_ROOT_PASSWORD`) |

Retrieve the password from the container:
```powershell
docker inspect pe-poc --format "{{range .Config.Env}}{{println .}}{{end}}" | Select-String "MYSQL_ROOT_PASSWORD"
```

---

## Docker Setup with Init Scripts

For CI/CD or clean-room environments, a `docker-compose.yml` is provided that
automatically initializes the database on first start using init scripts.

Location: `Database/docker/`

```
docker/
  docker-compose.yml   — Container definition (mysql:8.0, healthcheck, volume)
  Prepare-Init.cmd     — Copies SQL scripts into init/ in the correct order
  README.md            — Full usage instructions
  init/                — Generated: SQL scripts copied by Prepare-Init.cmd (01–06)
```

### Quickstart

```cmd
cd C:\Shared\PayrollEngine\Repos\PayrollEngine.Backend\Database\docker

:: 1. Copy scripts into init/ (run once, and after any script update)
Prepare-Init.cmd

:: 2. Start container
docker compose up -d
```

### Behavior

| Situation | Behavior |
|---|---|
| First `docker compose up` (empty volume) | Init scripts run automatically |
| Subsequent `docker compose up` | Init scripts are **skipped** |
| Full reset | `docker compose down -v` then `docker compose up -d` |

### Configuration

Defaults can be overridden via a `.env` file next to `docker-compose.yml`:

| Variable | Default |
|---|---|
| `MYSQL_ROOT_PASSWORD` | `poc123` |
| `MYSQL_PORT` | `3306` |

### When to use which approach

| Approach | Use case |
|---|---|
| `CreateModel.MySql.cmd` | Local development — manual reset on demand |
| `docker compose` + init scripts | CI/CD — clean database per pipeline run |

> **Note:** The `init/` directory contains generated copies of the SQL scripts.
> Do not edit them directly. Run `Prepare-Init.cmd` after any script update.
