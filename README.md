# Payroll Engine Backend

> Part of the [Payroll Engine](https://github.com/Payroll-Engine/PayrollEngine) open-source payroll automation framework.
> Full documentation at [payrollengine.org](https://payrollengine.org).

The Backend is the ASP.NET Core REST API server at the core of the Payroll Engine. It exposes the full payroll object model over HTTP, compiles and executes C# payroll scripts via Roslyn, processes payrun jobs asynchronously, and persists all results in a SQL Server database.

---

## Contents

- [Prerequisites](#prerequisites)
- [Setup](#setup)
- [Open API](#open-api)
- [Authentication](#authentication)
- [Application Settings](#application-settings)
  - [General](#general)
  - [Swagger / OpenAPI](#swagger--openapi)
  - [Authentication](#authentication-1)
  - [Audit Trail](#audit-trail)
  - [Scripting & Compilation](#scripting--compilation)
  - [Database & Timeouts](#database--timeouts)
  - [Payrun Processing](#payrun-processing)
  - [Tenant Isolation](#tenant-isolation)
  - [CORS](#cors)
  - [Rate Limiting](#rate-limiting)
  - [Configuration Examples](#configuration-examples)
  - [Database Connection String](#database-connection-string)
- [Application Logs](#application-logs)
- [C# Script Compiler](#c-script-compiler)
- [Docker Support](#docker-support)
- [Commands](#commands)
- [Solution Projects](#solution-projects)
- [Migration to 1.0](#migration-to-10)
- [Further Documents](#further-documents)
- [Third Party Components](#third-party-components)

---

## Prerequisites

| Requirement | Minimum |
|:--|:--|
| [.NET](https://dotnet.microsoft.com/download) | 10.0 |
| [SQL Server](https://www.microsoft.com/sql-server) | 2019 (or Azure SQL) — or MySQL 8.4 LTS |
| Database collation | `SQL_Latin1_General_CP1_CS_AS` (SQL Server) · `utf8mb4_unicode_ci` (MySQL) |
| Database isolation | `READ_COMMITTED_SNAPSHOT ON` (SQL Server only) |

---

## Setup

### 1. Create the database

**SQL Server** — run the provided script:

```cmd
Commands\Db.ModelCreate.cmd
```

Or use Docker Compose (recommended for first-time setup):
see [Container Setup](https://payrollengine.org/setup/container-setup).

Verify database settings after creation:

```sql
SELECT name, collation_name, is_read_committed_snapshot_on AS rcsi
FROM sys.databases WHERE name = 'PayrollEngine';
```

**MySQL 8.4 LTS** — run the provided script:

```cmd
Database\CreateModel.MySql.cmd
```

This executes `Create-Model.mysql.sql`, which contains tables, indexes, functions, and stored procedures in the correct dependency order.

### 2. Configure the connection string

**SQL Server** — set the connection string via environment variable (recommended):

```bash
# Windows
set PayrollDatabaseConnection=Server=localhost;Database=PayrollEngine;Integrated Security=SSPI;

# Docker / Linux
export PayrollDatabaseConnection="Server=localhost;Database=PayrollEngine;User Id=sa;Password=...;TrustServerCertificate=True;"
```

**MySQL** — set connection string and provider:

```bash
set PayrollDatabaseConnection=Server=localhost;Port=3306;Database=PayrollEngine;User=root;Password=...;CharSet=utf8mb4;
set PayrollServerConfiguration__DbProvider=MySql
```

Or add to `appsettings.json` (for local development only):

```json
{
  "ConnectionStrings": {
    "PayrollDatabaseConnection": "Server=localhost;Database=PayrollEngine;Integrated Security=SSPI;"
  }
}
```

> Store sensitive configuration in [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) for local development.

### 3. Start the server

```shell
# Run from the binary output folder
dotnet PayrollEngine.Backend.Server.dll --urls=https://localhost:44354/

# Run from the project folder (Backend.Server/)
dotnet run --urls=https://localhost:44354/

# Visual Studio
Open PayrollEngine.Backend.sln and start with the debugger
```

The API is accessible at the configured URL. When `EnableSwagger` is set to `true`, the Swagger UI is available at `/swagger`.

---

## Open API

The Payroll Engine API supports the [Open API](https://www.openapis.org/) specification and describes the interface to the [Swagger](https://swagger.io/) tool. The document [REST Service Endpoints](https://github.com/Payroll-Engine/PayrollEngine/blob/main/Documents/PayrollRestServicesEndpoints.pdf) describes the available endpoints.

> Payroll Engine [swagger.json](docs/swagger.json)

### Key Endpoints

| Endpoint | Method | Description |
|:--|:--|:--|
| `.../payruns/jobs` | POST | Start a payrun job (asynchronous, returns HTTP 202 with location header) |
| `.../payruns/jobs/preview` | POST | Synchronous single-employee payrun preview without persisting results |
| `.../employees/bulk` | POST | Bulk employee creation via `SqlBulkCopy` for high-throughput import |

### API Versioning

Starting with the 1.0 release, no version header is required in the HTTP request. Future major versions will require the HTTP header **X-Version** with the version number.

### API Content Type

The Payroll REST API supports HTTP requests in `JSON` format.

---

## Authentication

The API supports three authentication modes, configured via `Authentication:Mode` in `appsettings.json`:

**None** — No authentication. All requests are accepted. Development/internal use only.

**ApiKey** — Static API key. The client must send the key in the `Api-Key` HTTP header. The key is resolved in the following order:
1. Environment variable `PayrollApiKey`
2. Configuration value `Authentication:ApiKey` in `appsettings.json`

**OAuth** — OAuth 2.0 / JWT Bearer token. Requires `Authority` and `Audience` configuration. The client must send a valid Bearer token in the `Authorization` header. Authority and audience are validated at startup to prevent token confusion.

> When authentication is active and Swagger is enabled, Swagger UI requires the corresponding credentials.

---

## Application Settings

The server configuration file `appsettings.json` contains the following settings:

### General

| Setting                    | Description                                                 | Type       | Default        |
|:--|:--|:--|:--|
| `StartupCulture`           | Culture of the backend process (RFC 4646)                   | string     | System culture |
| `HttpsRedirection`         | Enable HTTPS redirection                                    | bool       | false          |
| `LogHttpRequests`          | Log HTTP requests to log file                               | bool       | false          |
| `Serilog`                  | Logger settings                                             | [Serilog](https://serilog.net/) | file and console log |

### Swagger / OpenAPI

| Setting                    | Description                                                 | Type       | Default        |
|:--|:--|:--|:--|
| `EnableSwagger`            | Enable Swagger UI and JSON endpoint <sup>1)</sup>           | bool       | false          |
| `DarkTheme`                | Use Swagger dark theme                                      | bool       | false          |
| `VisibleControllers`       | Visible API controllers <sup>2) 3)</sup>                    | string[]   | all            |
| `HiddenControllers`        | Hidden API controllers <sup>2) 3)</sup>                     | string[]   | none           |
| `XmlCommentFileNames`      | XML documentation files for Swagger                         | string[]   | none           |

### Authentication

| Setting                              | Description                                       | Type       | Default  |
|:--|:--|:--|:--|
| `Authentication:Mode`                | Authentication mode: `None`, `ApiKey`, `OAuth`    | enum       | None     |
| `Authentication:ApiKey`              | Static API key (only for mode `ApiKey`)           | string     | none     |
| `Authentication:OAuth:Authority`     | Token authority / issuer URL                      | string     | none     |
| `Authentication:OAuth:Audience`      | Expected audience claim                           | string     | none     |
| `Authentication:OAuth:RequireHttpsMetadata` | Require HTTPS metadata discovery            | bool       | true     |
| `Authentication:OAuth:ClientSecret`  | OAuth client secret for Swagger UI token flow <sup>9)</sup> | string     | none     |

### Audit Trail

| Setting                    | Description                                                 | Type       | Default  |
|:--|:--|:--|:--|
| `AuditTrail:Script`        | Audit trail for scripts                                     | bool       | false    |
| `AuditTrail:Lookup`        | Audit trail for lookups and lookup values <sup>4)</sup>     | bool       | false    |
| `AuditTrail:Input`         | Audit trail for cases, case fields and case relations        | bool       | false    |
| `AuditTrail:Payrun`        | Audit trail for collectors and wage types                   | bool       | false    |
| `AuditTrail:Report`        | Audit trail for reports, templates and parameters           | bool       | false    |

### Scripting & Compilation

| Setting                    | Description                                                 | Type       | Default        |
|:--|:--|:--|:--|
| `InitializeScriptCompiler` | Initialize Roslyn at startup to reduce first execution time | bool       | false          |
| `DumpCompilerSources`      | Store compiler source files to disk <sup>5)</sup>           | bool       | false          |
| `ScriptSafetyAnalysis`     | Static safety analysis of scripts during compilation <sup>8)</sup> | bool | false          |
| `AssemblyCacheTimeout`     | Timeout for cached script assemblies                        | timespan   | 30 minutes     |

### Database & Timeouts

| Setting                    | Description                                                 | Type       | Default        |
|:--|:--|:--|:--|
| `DbCollation`              | Expected database collation, verified on startup <sup>10)</sup> | string  | SQL_Latin1_General_CP1_CS_AS |
| `DbCommandTimeout`         | Database command timeout                                    | timespan   | 2 minutes      |
| `DbTransactionTimeout`     | Database transaction timeout                                | timespan   | 10 minutes     |
| `WebhookTimeout`           | Webhook HTTP request timeout                                | timespan   | 1 minute       |
| `FunctionLogTimeout`       | Timeout threshold for logging long function executions <sup>11)</sup> | timespan   | off            |

### Payrun Processing

| Setting                    | Description                                                 | Type       | Default        |
|:--|:--|:--|:--|
| `MaxParallelEmployees`     | Parallelism for employee processing <sup>6)</sup>           | string     | `0` (auto, ProcessorCount) |
| `MaxParallelPersist`       | Parallelism for result persistence (SemaphoreSlim) <sup>12)</sup> | int   | `2`            |
| `MaxRetroPayrunPeriods`    | Maximum retro payrun periods per employee <sup>7)</sup>     | int        | 0 (unlimited)  |
| `LogEmployeeTiming`        | Log employee processing timing                              | bool       | false          |

### Tenant Isolation

> ⚠️ **Regulation Sharing is disabled by default.** The `RegulationShare` controller is hidden and cross-tenant access is blocked until `TenantIsolationLevel` is explicitly set to `Consolidation` or higher.

| Setting | Description | Type | Default |
|:--|:--|:--|:--|
| `TenantIsolationLevel` | Server-wide cross-tenant access policy <sup>13)</sup> | enum | `None` |

Allowed values:

| Value | Description |
|:--|:--|
| `None` | Default. Single-tenant mode. Filter fully transparent — all requests permitted. `RegulationShare` controller hidden. |
| <nobr>`Consolidation`</nobr> | Single-tenant HTTP mode. Filter fully transparent. Enables `ExecuteConsolidatedQuery` in report scripts only (not HTTP cross-tenant access). |
| `Read` | Cross-tenant read access via HTTP (GET and read-only POST without `Auth-Tenant`). |
| `Write` | Full cross-tenant HTTP access. `Auth-Tenant` header must **not** be sent (returns `400` if present). |

> At `None` and `Consolidation` the filter is fully transparent — all requests pass through regardless of headers. The level check only activates at `Read` and `Write`. Standard clients that do not send `Auth-Tenant` (e.g. PE Console) are never blocked in single-tenant deployments.

### CORS

| Setting                              | Description                                       | Type       | Default                         |
|:--|:--|:--|:--|
| `Cors:AllowedOrigins`                | Allowed origins (empty = CORS inactive)           | string[]   | [] (inactive)                   |
| `Cors:AllowedMethods`                | Allowed HTTP methods                              | string[]   | GET, POST, PUT, DELETE, PATCH, OPTIONS |
| `Cors:AllowedHeaders`                | Allowed request headers                           | string[]   | Content-Type, Authorization, Api-Key, Auth-Tenant |
| `Cors:AllowCredentials`              | Include credentials in cross-origin requests      | bool       | false                           |
| `Cors:PreflightMaxAgeSeconds`        | Preflight response cache duration                 | int        | 600                             |

### Rate Limiting

| Setting                              | Description                                       | Type       | Default        |
|:--|:--|:--|:--|
| `RateLimiting:Global:PermitLimit`    | Max requests per window (0 = inactive)            | int        | 0 (inactive)   |
| `RateLimiting:Global:WindowSeconds`  | Time window in seconds                            | int        | 60             |
| `RateLimiting:PayrunJobStart:PermitLimit` | Max payrun job starts per window             | int        | 0 (inactive)   |
| `RateLimiting:PayrunJobStart:WindowSeconds` | Time window in seconds                     | int        | 60             |

<sup>1)</sup> Should be disabled in production to prevent exposing the full API surface and OAuth client credentials.<br />
<sup>2)</sup> Wildcard support for `*` and `?`.<br />
<sup>3)</sup> `HiddenControllers` cannot be combined with `VisibleControllers`.<br />
<sup>4)</sup> Audit trail is not supported on bulk lookup values import.<br />
<sup>5)</sup> Stores compilation scripts to the `ScriptDump` folder. Analysis feature only.<br />
<sup>6)</sup> Values: `0` (or empty) = auto (ProcessorCount, default), `off` or `-1` = sequential (no parallelism), `half` = ProcessorCount/2, `max` = ProcessorCount, `1`–`N` = explicit thread count.<br />
<sup>7)</sup> Safety guard against runaway retro calculations. 0 = no limit.<br />
<sup>8)</sup> When enabled, scripts are checked for banned API usage (`System.IO`, `System.Net`, `System.Diagnostics`, `System.Reflection`, etc.) before the assembly is emitted. Adds ~300 ms per compilation. Enable to harden script execution against unauthorized BCL access.<br />
<sup>9)</sup> Used exclusively by Swagger UI to obtain tokens for the interactive API explorer. Not required for production API authentication.<br />
<sup>10)</sup> Verified on startup before the schema version check. Prevents silent data integrity issues from mismatched collation.<br />
<sup>11)</sup> When set, functions exceeding this duration are logged at Warning level. Useful for identifying slow scripts or database operations.<br />
<sup>12)</sup> Controls how many employees can persist their results concurrently. `1` = fully serialized (no deadlocks), `2` = default (load-tested best balance), `4+` = no measurable gain over `2`. Failed persists are retried up to 3 times with backoff; a job abort occurs only if all retries are exhausted.<br />
<sup>13)</sup> Controls whether cross-tenant access is permitted. Must be set to `Consolidation` or higher to activate Regulation Sharing. Only elevate above `None` when the deployment explicitly requires cross-tenant access — a consolidation-only setup does **not** require `Read` or `Write`.

> It is recommended that you save the application settings within your local [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets).

### Configuration Examples

Minimal development configuration:
```json
{
  "StartupCulture": "de-CH",
  "EnableSwagger": true,
  "Authentication": {
    "Mode": "None"
  }
}
```

Production configuration with OAuth, CORS and rate limiting:
```json
{
  "EnableSwagger": false,
  "HttpsRedirection": true,
  "Authentication": {
    "Mode": "OAuth",
    "OAuth": {
      "Authority": "https://login.example.com/realms/payroll",
      "Audience": "payroll-api",
      "RequireHttpsMetadata": true
    }
  },
  "AuditTrail": {
    "Script": true,
    "Input": true,
    "Payrun": true
  },
  "DbCommandTimeout": "00:03:00",
  "DbTransactionTimeout": "00:15:00",
  "MaxParallelEmployees": "half",
  "MaxRetroPayrunPeriods": 24,
  "Cors": {
    "AllowedOrigins": [ "https://app.example.com" ],
    "AllowCredentials": true
  },
  "RateLimiting": {
    "Global": {
      "PermitLimit": 200,
      "WindowSeconds": 60
    },
    "PayrunJobStart": {
      "PermitLimit": 5,
      "WindowSeconds": 60
    }
  }
}
```

### Database Connection String

The backend database connection string is determined by the following priority:

1. Environment variable `PayrollDatabaseConnection`.
2. Program configuration file `appsettings.json`.

> In Docker, the connection string uses the ASP.NET Core hierarchical key format: `ConnectionStrings__PayrollDatabaseConnection`. This is the standard mechanism for overriding nested configuration values via environment variables.

---

## Application Logs

The backend server stores its logs in the application folder `logs`.

---

## C# Script Compiler

The business logic defined by the business in C# is compiled into binary files (assemblies) by the backend using [Roslyn](https://github.com/dotnet/roslyn). This procedure has a positive effect on the runtime performance, so that even extensive calculations can be performed sufficiently quickly. At runtime, the backend keeps the assemblies in a cache. To optimize memory usage, unused assemblies are periodically deleted (application setting `AssemblyCacheTimeout`).

You can use the `InitializeScriptCompiler` application setting to start the Roslyn engine when the application starts, thereby eliminating the runtime delay.

To perform a more in-depth analysis, set the `DumpCompilerSources` application setting to force the C# script compiler to save the source scripts of the compilation as disk files. These files are stored in the `ScriptDump` folder within the application folder, ordered by function type and dump date.

When `ScriptSafetyAnalysis` is enabled, user scripts are statically checked for banned API usage before the assembly is emitted. See footnote 8 in the application settings for details.

---

## Docker Support

The recommended way to run the Backend is as part of the full Docker Compose stack.
See the [Container Setup](https://payrollengine.org/setup/container-setup) documentation.

> ⚠️ The examples below use sample credentials for local development only. Never use these values in production.

### Pre-built image (ghcr.io)

Pull and run the pre-built image:
```bash
docker run -p 5001:8080 \
  -e ASPNETCORE_URLS="http://+:8080" \
  -e ConnectionStrings__PayrollDatabaseConnection="Server=host.docker.internal;Database=PayrollEngine;User Id=sa;Password=PayrollStrongPass789;TrustServerCertificate=True;" \
  ghcr.io/payroll-engine/payrollengine.backend:latest
```

Verify API is accessible at http://localhost:5001

### Build from source (development)

```bash
docker build -t payroll-backend .
docker run -p 5001:8080 \
  -e ASPNETCORE_URLS="http://+:8080" \
  -e ConnectionStrings__PayrollDatabaseConnection="Server=host.docker.internal;Database=PayrollEngine;User Id=sa;Password=PayrollStrongPass789;TrustServerCertificate=True;" \
  payroll-backend
```

---

## Commands

Helper scripts in the `Commands` folder:

| Command | Description |
|:--|:--|
| `Db.ExportModelCreate.cmd` | Export the live database schema to `Database\Create-Model.sql` <sup>1)</sup> |
| `Db.ExportModelDrop.cmd` | Export the live database drop script to `Database\Drop-Model.sql` <sup>1)</sup> |
| `Db.ModelCreate.cmd` | Execute `Create-Model.sql` against the database |
| `Db.ModelDrop.cmd` | Execute `Drop-Model.sql` against the database |
| `Db.ModelUpdate.cmd` | Execute drop then create (full schema reset) |
| `Db.Publish.cmd` | Create the combined setup script `SetupModel.sql` |
| `Db.VersionCreate.cmd` | Insert the version record via `VersionCreate.sql` |
| `DotNet.Swagger.Install.cmd` | Install the Swashbuckle CLI tool (`dotnet-swagger`) |
| `Swagger.Build.cmd` | Generate `docs/swagger.json` from the running backend |
| `SqlScripter.cmd` | Script the live database to a SQL file via `mssql-scripter` |
| `SqlFormatter.cmd` | Format a raw SQL export via `PoorMansTSqlFormatter` |
| `Domian.Model.Unit.Tests.cmd` | Run the domain model unit tests |

<sup>1)</sup> After export, manually remove the `CREATE DATABASE` / `ALTER DATABASE` block at the top and the `SET READ_WRITE` statement at the bottom before committing the file.

---

## Solution Projects

| Name                                  | Type       | Description                                       |
|:--|:--|:--|
| `PayrollEngine.Domain.Model`          | Library    | Domain objects and repositories                   |
| `PayrollEngine.Domain.Scripting`      | Library    | Scripting services                                |
| `PayrollEngine.Domain.Application`    | Library    | Application service                               |
| `PayrollEngine.Persistence`           | Library    | Repository implementations                        |
| `PayrollEngine.Persistence.SqlServer` | Library    | SQL Server persistence implementation             |
| `PayrollEngine.Persistence.MySql`     | Library    | MySQL 8.4 LTS persistence implementation (preview) |
| `PayrollEngine.Api.Model`             | Library    | REST API data transfer objects                    |
| `PayrollEngine.Api.Core`              | Library    | REST core services (query, filter, serialization) |
| `PayrollEngine.Api.Map`               | Library    | Mapping between REST and domain objects           |
| `PayrollEngine.Api.Controller`        | Library    | REST controllers (business logic per resource)    |
| `PayrollEngine.Backend.Controller`    | Library    | ASP.NET routing controllers (HTTP routing and model binding) |
| `PayrollEngine.Backend.Server`        | Exe        | Web application server with REST API              |

---

## Migration to 1.0

### Breaking Change: PayrunJobInvocation

`PayrunJobInvocation` has been refactored from id-based to name/identifier-based references:
- **Removed**: `PayrunId` and `UserId` properties from API and domain models
- **Required**: `PayrunName` and `UserIdentifier` are now the required fields

Clients must update payrun job invocations to use name-based references instead of numeric ids.

### Async Payrun Job Processing

Payrun jobs are now processed asynchronously by default via a background queue:
- Job is pre-created and persisted before enqueue
- Returns HTTP 202 with a location header for status polling
- Webhook notification on job completion or abort
- Bounded channel with backpressure (capacity: 100)

---

## Further Documents

- [OData](OData.md) — query syntax reference
- [Database](Database.md) — schema, scripts, and maintenance
- [Developer Guidelines](Dev-Guidelines.md) — adding new objects and fields

---

## Third Party Components

- Object mapping with [Mapperly](https://github.com/riok/mapperly/) — license `Apache 2.0`
- OpenAPI with [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/) — license `MIT`
- Database query builder with [SqlKata](https://github.com/sqlkata/querybuilder/) — license `MIT`
- Database object mapping with [Dapper](https://github.com/DapperLib/Dapper/) — license `Apache 2.0`
- Logging with [Serilog](https://github.com/serilog/serilog/) — license `Apache 2.0`
- Tests with [xunit](https://github.com/xunit) — license `Apache 2.0`
