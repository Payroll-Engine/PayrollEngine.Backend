# Payroll Engine Backend
👉 This application is part of the [Payroll Engine](https://github.com/Payroll-Engine/PayrollEngine/wiki).

## Open API
The Payroll Engine API supports the [Open API](https://www.openapis.org/) specification and describes the interface to the [Swagger](https://swagger.io/) tool. The document [REST Service Endpoints](https://github.com/Payroll-Engine/PayrollEngine/blob/main/Documents/PayrollRestServicesEndpoints.pdf) document describes the available endpoints.

> Payroll Engine [swagger.json](docs/swagger.json)

## API Versioning
In the first 1.0 release of the REST API, no version header is required in the HTTP request. For future version changes, the HTTP header **X-Version** with the version number must be present.

## API Content Type
The Payroll REST API supports HTTP requests in `JSON` format.

## Backend Server
To run the backend server, the web host must support the execution of .NET Core applications. For local development, [IIS Express](https://learn.microsoft.com/en-us/iis/extensions/introduction-to-iis-express/iis-express-overview) serves as the host in two execution variants:
- [CLI dotnet command](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet) using the command inside the binary folder:
```
start "" dotnet PayrollEngine.Backend.Server.dll --urls=https://localhost:44354/
```
- Visual Studio solution `PayrollEngine.Backend.sln` using the debugger.

## Application Settings
The server configuration file `Backend.Server\appsetings.json` contains the following settings:
| Setting      | Description            | Default |
|:--|:--|:--|
| `StartupCulture` | The culture of the backend process (string) | System culture |
| `LogHttpRequests` | Log http requestd to log file (bool) | `false` |
| `InitializeScriptCompiler` | Initialize the script compiler to reduce first run time (bool) | `false` |
| `DbTransactionTimeout` | Database transaction timeout (timespan) | 10 minutes |
| `DbCommandTimeout` | Database command timeout (seconds) | 2 minutes |
| `WebhookTimeout` | Webhook timeout (timespan) | 1 minute |
| `Serilog` | Serilog settings | file and console log with [Serilog](https://serilog.net/) |
| `FunctionLogTimeout` | Timeout for tracking long function executions (timespan) | off |
| `AssemblyCacheTimeout` | Timeout for cached assemblies (timespan) | 30 minutes |
| `VisibleControllers` | Name of visible API controllers (string[]) <sup>1) 2)</sup> | all |
| `HiddenControllers` | Name of hidden API controllers (string[]) <sup>1) 2)</sup> | none |

<sup>1)</sup> Wildcard support for `*` and `?`<br />
<sup>2)</sup> `HiddenControllers` setting cannot be combined with `VisibleControllers` setting

> It is recommended that you save the application settings within your local [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets).

## Application Logs
Under Windows, the backend server stores its logs in the system folder `%ProgramData%\Backend\logs`.

## C# Script Compiler
The business logic defined by the business in C# is compiled into binary files (assemblies) by the backend using [Roslyn](https://github.com/dotnet/roslyn). This procedure has a positive effect on the runtime performance, so that even extensive calculations can be performed sufficiently quickly. At runtime, the backend keeps the assemblies in a cache. To optimize memory usage, unused assemblies are periodically deleted (application setting `AssemblyCacheTimeout`).

## Further documents
- [OData](OData.md) queries
- [Database](Database.md) Management
- [Developer Guidelines](Dev-Guidelines.md)

## Third party components
- Object mapping with [Mapperly](https://github.com/riok/mapperly/) - licence `Apache 2.0`
- OpenAPI with [Swashbuggle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/) - licence `MIT`
- Database query builder with [SqlKata](https://github.com/sqlkata/querybuilder/) - licence `MIT`
- Database object mapping with [Dapper](https://github.com/DapperLib/Dapper/) - licence `Apache 2.0`
- Logging with [Serilog](https://github.com/serilog/serilog/) - licence `Apache 2.0`
- Tests with [xunit](https://github.com/xunit) - licence `Apache 2.0`
