# Developer Guidelines

## New object guidelines
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

## New object field guidelines
Backend:
- Add table and columns to [Persistence.Sql.DbShema.cs]
- Export database schema (create & delete)
- Add field to the domain object in [Domain.Model]
- add field to the API object in [Api.Model]

> Duplicate these steps for audit objects

Client services:
- Create client object in [Client.Model]
- Update client JSON schema (build of PayrollEngine.Client.Core)
