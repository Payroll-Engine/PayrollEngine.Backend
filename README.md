<h1>Payroll Engine Backend</h1>

## Open API
The Payroll Engine API supports the [Open API](https://www.openapis.org/) specification and describes the interface to the [Swagger](https://swagger.io/) tool. The document [REST Service Endpoints](https://github.com/Payroll-Engine/PayrollEngine/blob/main/Documents/PayrollRestServicesEndpoints.pdf) document describes the available endpoints.

> Payroll Engine [swagger.json](docs/swagger.json)

## API Versioning
In the first 1.0 release of the REST API, no version header is required in the HTTP request. For future version changes, the HTTP header **X-Version** with the version number must be present.

## API Conent Type
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

## OData Queries
Most `GET` endpoints provide parameters for sorting and filtering data. The backend uses a subset of the query protocol [OData] (https://learn.microsoft.com/en-us/odata/) for this purpose.

### Basic rules
- Field/column name is not case-sensitive
- Enum values resolved by case-insesitive name

### Supported features
- `top`
- `skip`
- `select` (only on db level)
- `orderby`
- filter
  - `Or`
  - `And`
  - `Equal`
  - `NotEqual`
  - `GreaterThan`
  - `GreaterThanOrEqual`
  - `LessThan`
  - `LessThanOrEqual`
  - group filter terms with `()`
  - supported functions
    - `startswith` (string)
    - `endswith` (string)
    - `contains` (string)
    - `year` (datetime)
    - `month` (datetime)
    - `day` (datetime)
    - `hour` (datetime)
    - `minute` (datetime)
    - `date` (datetime)
    - `time` (datetime)

### Unsupported query features
- `expand`
- `search`
- filter
    - `Add`
    - `Subtract`
    - `Multiply`
    - `Divide`
    - `Modulo`
    - `Has`
  - all other functions
- lambda operators

Further information
- [OData v4](https://docs.oasis-open.org/odata/odata/v4.01/odata-v4.01-part1-protocol.html)
- [OData Getting Started Tutorial](https://www.odata.org/getting-started/basic-tutorial)

## Database
The database model is managed in T-SQL scripts. Adjustments to the model are made in SQL Server Management Studio and exported to files.

### Database Script Export
Steps to build the T-SQL **create** script:
1. Run the `Db.ExportModelCreate.cmd` command
    - start the [mssql-scripter](https://github.com/microsoft/mssql-scripter)
    - start the SQL code [formatter](https://github.com/TaoK/PoorMansTSqlFormatter)
2. Manual editing of [ModelCreate.sql](../Database/Current/ModelCreate.sql)
    - remove the `CREATE DATABASE` and `ALTER DATABASE` statements from the top (up to the `BuildAttributeQuery` function)
    - remove the `ALTER DATABASE` and `SET READ_WRITE` statements from the bottom

Steps to build the T-SQL **drop** script:
1. Run the `Db.ExportModelDrop.cmd` command
    - start the [mssql-scripter](https://github.com/microsoft/mssql-scripter)
    - start the SQL code [formatter](https://github.com/TaoK/PoorMansTSqlFormatter)
2. Manual editing of [ModelDrop.sql](../Database/Current/ModelDrop.sql)
    - remove the `USE [PayrollEngine]` statements from the top
    - remove the `USE [Master]` and `DROP DATABASE [PayrollEngine]` statements from the bottom

### Database Script Import
Commands to import SQL script files into the database:
- `Db.ModelCreate.cmd` - create the database
- `Db.ModelDrop.cmd` - drop the database
- `Db.ModelUpdate.cmd` - update the database: first drop and then create

### Filtered index
To support index on nullable fields, the SQL index requieres a filter condition. For example the range value on the lookup value:
```sql
CREATE UNIQUE NONCLUSTERED INDEX [IX_LookupValue.UniqueRangeValuePerLookup] ON [dbo].[LookupValue] (
  [RangeValue] ASC,
  [LookupId] ASC )
WHERE ([RangeValue] IS NOT NULL)
```

> Please note: the condition of a filtered index is not visible in SSMS.

See also:
- [How do I create a unique constraint that also allows nulls?](https://stackoverflow.com/a/767702)
- [Create filtered index](https://docs.microsoft.com/en-us/sql/relational-databases/indexes/create-filtered-indexes)

### Multi-column index
This also means that if you have a multi-column index across several columns, having a single-column index against the first column in the multi-column index is redundant and superfluous – the multi-column index can be used just as easily in queries only constraining that one, left-most column.

See also:
- [Multi-column index](https://www.celerity.com/how-to-design-sql-indexes/)

## C# Script Compiler
The business logic defined by the business in C# is compiled into binary files (assemblies) by the backend using [Roslyn](https://github.com/dotnet/roslyn). This procedure has a positive effect on the runtime performance, so that even extensive calculations can be performed sufficiently quickly. At runtime, the backend keeps the assemblies in a cache. To optimize memory usage, unused assemblies are periodically deleted (application setting `AssemblyCacheTimeout`).

## Developer Guidelines
### New object guidelines
Backend:
- Create an SQL table in [Microsoft SQL Server Management Studio]
- Add table and columns to [Persistence.Sql.DbShema.cs]
- Export database schema (create & delete)
- Create domain object including the repo interface in [Domain.Model]
- Create SQL repository class in [Persitence.SqlServer]
- Create an application service in [Apllication]
- Create MVC API object including the repo interface in [Api.Model]
- Create domain/API map type in [Api.Map]
- Create internal MVC controller in [Api.Controller]
- Create public MVC controller in [Backend.Controller]

> Duplicate these steps for audit objects

Client services:
- Create client object in [Client.Model]
- Update client JSON schema (build of PayrollEngine.Client.Core)

### New object field guidelines
Backend:
- Add table and columns to [Persistence.Sql.DbShema.cs]
- Export database schema (create & delete)
- Add field to the domain object in [Domain.Model]
- add field to the API object in [Api.Model]

> Duplicate these steps for audit objects

Client services:
- Create client object in [Client.Model]
- Update client JSON schema (build of PayrollEngine.Client.Core)

## Third party components
- Object mapping with [AutoMapper](https://github.com/AutoMapper/AutoMapper/) - licence `MIT`
- OpenAPI with [Swashbuggle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/) - licence `MIT`
- Database query builder with [SqlKata](https://github.com/sqlkata/querybuilder/) - licence `MIT`
- Database object mapping with [Dapper](https://github.com/DapperLib/Dapper/) - licence `Apache 2.0`
- Logging with [Serilog](https://github.com/serilog/serilog/) - licence `Apache 2.0`
- Tests with [xunit](https://github.com/xunit) - licence `Apache 2.0`
