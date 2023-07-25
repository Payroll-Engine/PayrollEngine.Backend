<h1>Payroll Engine Backend</h1>

## Open API
The Payroll Engine API supports the [Open API](https://www.openapis.org/) specification and describes the interface to the [Swagger](https://swagger.io/) tool. The document [REST Service Endpoints](https://github.com/Payroll-Engine/PayrollEngine/blob/main/Documents/PayrollRestServicesEndpoints.pdf) describes the available endpoints.

> Payroll Engine [swagger.json](docs/swagger.json)

## API Versioning
In the first version 1.0 of the REST API, no version header is required in the HTTP request. For future version changes, the HTTP header **X-Version** must be present with the version number.

## API Conent Type
The Payroll REST API supports HTTP requests in `JSON` format.

## Web Application Server
To run the backend server, the web host must support the execution of .NET Core applications. For local development, [IIS Express](https://learn.microsoft.com/en-us/iis/extensions/introduction-to-iis-express/iis-express-overview) serves as host in two execution variants:
- [CLI dotnet command](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet) using the command within the binary folder:
```
start "" dotnet PayrollEngine.Backend.Server.dll --urls=https://localhost:44354/
```

- Visual Studio Solution `PayrollEngine.Backend.sln` using the debugger.

## Application Settings
The server configuration `Backend.Server\appsetings.json` contains the following settings:
| Setting      | Description            | Default |
|:--|:--|:--|
| `StartupCulture` | The backend process culture (string) | System culture |
| `LogHttpRequests` | Log http request to the log file (bool) | `false` |
| `InitializeScriptCompiler` | Initialize the script compiler to reduce first execution time (bool) | `false` |
| `DbTransactionTimeout` | Database transaction timeout (timespan) | 10 minutes |
| `DbCommandTimeout` | Database command timeout (seconds) | 2 minutes |
| `WebhookTimeout` | Webhook timeout (timespan) | 1 minute |
| `Serilog` | Serilog settings | file and console log with [Serilog](https://serilog.net/) |
| `FunctionLogTimeout` | Timeout to track long function exections (timespan) | off |
| `AssemblyCacheTimeout` | Timeout for cached assemblies (timespan) | 30 minuts |
| `VisibleControllers` | Name of visible API controllers (string[]) <sup>1) 2)</sup> | all |
| `HiddenControllers` | Name of hidden API controllers (string[]) <sup>1) 2)</sup> | none |

<sup>1)</sup> Wildcard support wildcard support `*` and `?`<br />
<sup>2)</sup> `HiddenControllers` setting can not combined with `VisibleControllers` setting

> It is recommended to save the server settings within your local [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets).

## Database Scripts
Das Datenbankmodell wird in T-SQL Scripts geführt. Anpassungen am Modell erfolgen im SQL-Sever management Studio und werden in Dateien exportiert.

### Database Script Export
Steps to build the T-SQL **create** script:
1. Start command `Db.ExportModelCreate.cmd`
    - starts the [mssql-scripter](https://github.com/microsoft/mssql-scripter)
    - start the SQL code [formatter](https://github.com/TaoK/PoorMansTSqlFormatter)
2. Manual edit of [ModelCreate.sql](../Database/Current/ModelCreate.sql)
    - remove `CREATE DATABASE` and `ALTER DATABASE` statements from the top (until the function `BuildAttributeQuery`)
    - remove `ALTER DATABASE` and `SET READ_WRITE` statements from the bottom

Steps to build the T-SQL **drop** script:
1. Start command `Db.ExportModelDrop.cmd`
    - starts the [mssql-scripter](https://github.com/microsoft/mssql-scripter)
    - start the SQL code [formatter](https://github.com/TaoK/PoorMansTSqlFormatter)
2. Manual edit of [ModelDrop.sql](../Database/Current/ModelDrop.sql)
    - remove `USE [PayrollEngine]` statements from the top
    - remove `USE [Master]` and `DROP DATABASE [PayrollEngine]` statements from the bottom

### Database Script Import
Commands to import t-SQL script files to the database:
- `Db.ModelCreate.cmd` - created the datatabase
- `Db.ModelDrop.cmd` - drop the datatabase
- `Db.ModelUpdate.cmd` - updet the datatabase: frist drop tand the create

## C# Script Compiler
The business logic defined by the business in C# is compiled into binary files (assemblies) by the backend using [Roslyn](https://github.com/dotnet/roslyn). This procedure has a positive effect on runtime performance, so that even extensive calculations can be performed sufficiently quickly.
At runtime, the backend keeps the assemblies in a cache. For memory optimization, unused assemblies are periodically deleted.
