# PayrollEngine MySQL — Docker Setup

## Structure

```
Database\docker-mysql\
  docker-compose.yml          ← Create: current schema (Create-Model.mysql.sql)
  docker-compose.update.yml   ← Update: previous release schema as migration baseline
  Docker.Compose.*.cmd        ← Shortcut scripts (see Scripts)
```

The init script is mounted directly from `Database\` — there are no copies to maintain.
On first start (empty volume), the container runs it via the `mysql` client
(`DELIMITER` blocks and `CREATE DATABASE` are handled by the script).

## Scenarios

| Scenario | Project | Container | Port | Initial schema |
|---|---|---|---|---|
| Create | `pe-full` | `pe-mysql` | 3306 | `Create-Model.mysql.sql` (current) |
| Update | `pe-upd` | `pe-mysql-upd` | 3307 | `History\v1.1.0\Create-Model.mysql.sql` |

Each project (`-p`) gets its own volume, so both scenarios can run side by side.

### Scripts

| Script | Scenario | Action |
|---|---|---|
| `Docker.Compose.Up.cmd` | Create | Start `pe-full` |
| `Docker.Compose.Down.cmd` | Create | Stop `pe-full` and delete its volume |
| `Docker.Compose.Update.Up.cmd` | Update | Start `pe-upd` with the previous release schema |
| `Docker.Compose.Update.Migrate.cmd` | Update | Apply `Update-Model.mysql.sql` to `pe-mysql-upd` |
| `Docker.Compose.Update.Down.cmd` | Update | Stop `pe-upd` and delete its volume |

The scripts can be run from any directory.

### Create — Full Setup

```powershell
cd C:\Shared\PayrollEngine\Repos\PayrollEngine.Backend\Database\docker-mysql
docker compose -p pe-full up -d --wait
```

### Update — Migration from Previous Release

```powershell
cd C:\Shared\PayrollEngine\Repos\PayrollEngine.Backend\Database\docker-mysql

# 1. Start with the previous release schema
docker compose -p pe-upd -f docker-compose.yml -f docker-compose.update.yml up -d --wait

# 2. Import regulations and test data using the previous release backend (port 3307)

# 3. Migrate to the current schema
cmd /c "docker exec -i pe-mysql-upd mysql --comments -uroot -ppoc123 PayrollEngine < ..\Update-Model.mysql.sql"
```

Step 1 and 3 are also available as `Docker.Compose.Update.Up.cmd` and `Docker.Compose.Update.Migrate.cmd`.

Use `cmd /c` with `<` redirection — piping via PowerShell `Get-Content` may alter encoding.
`--comments` keeps routine comments, matching the full setup (the container init uses it too).

Step 2 is what makes the update test meaningful: it verifies the migration against existing data,
not just an empty schema.

`Update-Model.mysql.sql` aborts if the schema is not at the expected previous version.

> **Release maintenance:** after each release, update the `History\vX.Y.Z` path in
> `docker-compose.update.yml` to the new previous release.

## Verification

```powershell
docker exec pe-mysql mysql -uroot -ppoc123 -e "SELECT COUNT(*) AS tables FROM information_schema.TABLES WHERE TABLE_SCHEMA='PayrollEngine' AND TABLE_TYPE='BASE TABLE'; SELECT ROUTINE_TYPE, COUNT(*) AS count FROM information_schema.ROUTINES WHERE ROUTINE_SCHEMA='PayrollEngine' GROUP BY ROUTINE_TYPE; SELECT MajorVersion, MinorVersion, SubVersion FROM PayrollEngine.Version ORDER BY Id DESC LIMIT 1;"
```

Use `pe-mysql-upd` for the update scenario.

### Schema Comparison (Create vs. Update)

```powershell
cmd /c "docker exec pe-mysql mysqldump -uroot -ppoc123 --no-data --routines --skip-dump-date PayrollEngine > %TEMP%\schema-full.sql"
cmd /c "docker exec pe-mysql-upd mysqldump -uroot -ppoc123 --no-data --routines --skip-dump-date PayrollEngine > %TEMP%\schema-upd.sql"
git diff --no-index --ignore-all-space $env:TEMP\schema-full.sql $env:TEMP\schema-upd.sql
```

Differences in `AUTO_INCREMENT=` values are expected. Any other difference indicates a gap in
`Update-Model.mysql.sql`.

## Behaviour

| Situation | Behaviour |
|---|---|
| First `up` (empty volume) | Init script runs automatically |
| Subsequent `up` (volume present) | Init script is **not** re-run |
| Reset | `down -v` deletes the volume, next `up` re-initializes |

## Reset

```powershell
docker compose -p pe-full down -v
docker compose -p pe-upd -f docker-compose.yml -f docker-compose.update.yml down -v
```

## Configuration

Defaults (overridable via `.env` or environment variables):

| Variable | Default (Create) | Default (Update) |
|---|---|---|
| `MYSQL_IMAGE` | `mysql:8.0` | `mysql:8.0` |
| `MYSQL_CONTAINER` | `pe-mysql` | `pe-mysql-upd` |
| `MYSQL_PORT` | `3306` | `3307` |
| `MYSQL_ROOT_PASSWORD` | `poc123` | `poc123` |

Use `MYSQL_IMAGE=mysql:8.4` to test against MySQL 8.4 LTS.

## Connection String

`appsettings.Development.json` entry:

```json
"ConnectionStrings": {
  "PayrollDatabaseConnection": "Server=localhost;Port=3306;Database=PayrollEngine;User=root;Password=poc123;CharSet=utf8mb4;"
},
"PayrollServerConfiguration": {
  "DbProvider": "MySql"
}
```

Use `Port=3307` for the update scenario.
