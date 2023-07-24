<h1>Payroll Engine Backend</h1>

## Open API
The Payroll Engine API supports the [Open API](https://www.openapis.org/) specification and describes the interface to the [Swagger](https://swagger.io/) tool. The document [REST Service Endpoints](https://github.com/Payroll-Engine/PayrollEngine/blob/main/Documents/PayrollRestServicesEndpoints.pdf) describes the available endpoints.

> Payroll Engine [swagger.json](docs/swagger.json)

## API Versioning
In the first version 1.0 of the REST API, no version header is required in the HTTP request. For future version changes, the HTTP header **X-Version** must be present with the version number.

## API Conent Type
The Payroll REST API supports HTTP requests in `JSON` format.

## Application Settings
The server configuration `Backend.Server\appsetings.json` contains the following settings:
| Setting      | Description            | Default |
|:--|:--|:--|
| `StartupCulture` | The backend process culture (string) | System culture |
| `LogHttpRequests` | Log http request to the log file (bool) | `false` |
| `InitializeScriptCompiler` | Initialize the script compiler to reduce first execution time (bool) | `false` |
| `TransactionTimeout` | Database transaction timeout (timespan) | 10 minutes |
| `DbCommandTimeout` | Database command timeout (seconds) | 2 minutes |
| `WebhookTimeout` | Webhook timeout (timespan) | 1 minute |
| `Serilog` | Serilog settings | file and console log |
| `FunctionLogTimeout` | Timeout to track long function exections (timespan) | off |
| `AssemblyCacheTimeout` | Timeout for cached assemblies (timespan) | 30 minuts |
| `VisibleControllers` | Name of visible API controllers (string[]) <sup>1) 2)</sup> | all |
| `HiddenControllers` | Name of hidden API controllers (string[]) <sup>1) 2)</sup> | none |

<sup>1)</sup> Wildcard support wildcard support `*` and `?`<br />
<sup>2)</sup> `HiddenControllers` setting can not combined with `VisibleControllers` setting

> It is recommended to save the server settings within your local [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets).

## Script Compiler
The business logic defined by the business in C# is compiled into binary files (assemblies) by the backend using [Roslyn](https://github.com/dotnet/roslyn). This procedure has a positive effect on runtime performance, so that even extensive calculations can be performed sufficiently quickly.
At runtime, the backend keeps the assemblies in a cache. For memory optimization, unused assemblies are periodically deleted.
