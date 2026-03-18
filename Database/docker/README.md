# PayrollEngine MySQL — Docker Setup

## Struktur

```
Database\docker\
  docker-compose.yml   ← Container-Definition
  Prepare-Init.cmd     ← Kopiert Scripts in init\ (einmalig / nach Updates)
  init\
    01-Create-Model.mysql.sql        ← wird beim ersten Start ausgeführt
    02-Functions.mysql.sql
    03-GetDerived.mysql.sql
    04-GetCaseValues.mysql.sql
    05-GetLookupRangeValue.mysql.sql
    06-Remaining.mysql.sql
```

## Schnellstart

```cmd
:: 1. Init-Scripts vorbereiten (einmalig / nach Script-Updates)
cd C:\Shared\PayrollEngine\Repos\PayrollEngine.Backend\Database\docker
Prepare-Init.cmd

:: 2. Container starten
docker compose up -d

:: 3. Bereit prüfen
docker compose ps
```

## Verhalten

| Situation | Verhalten |
|---|---|
| Erstes `docker compose up` (leeres Volume) | Init-Scripts werden automatisch ausgeführt |
| Weiteres `docker compose up` (Volume vorhanden) | Init-Scripts werden **nicht** erneut ausgeführt |
| Manueller Reset | Volume löschen + neu starten |

## Manueller Reset (vollständig)

```cmd
docker compose down -v
docker compose up -d
```

`-v` löscht das Volume `pe-mysql-data` — die DB wird beim nächsten Start neu initialisiert.

## Konfiguration

Standardwerte (über `.env` überschreibbar):

| Variable | Default |
|---|---|
| `MYSQL_ROOT_PASSWORD` | `poc123` |
| `MYSQL_PORT` | `3306` |

`.env`-Beispiel:
```
MYSQL_ROOT_PASSWORD=MeinPasswort
MYSQL_PORT=3307
```

## Verbindung

```
Server=localhost;Port=3306;Database=PayrollEngine;User=root;Password=poc123;CharSet=utf8mb4;
```

## Init-Scripts neu einlesen (nach Script-Änderungen)

```cmd
:: Scripts aktualisieren
Prepare-Init.cmd

:: Volume löschen und Container neu starten
docker compose down -v
docker compose up -d
```
