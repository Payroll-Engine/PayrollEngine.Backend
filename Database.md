# Database
The database model is managed in T-SQL scripts. Adjustments to the model are made in SQL Server Management Studio and exported to files.

## Database Script Export
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

## Database Script Import
Commands to import SQL script files into the database:
- `Db.ModelCreate.cmd` - create the database
- `Db.ModelDrop.cmd` - drop the database
- `Db.ModelUpdate.cmd` - update the database: first drop and then create

## Filtered index
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

## Multi-column index
This also means that if you have a multi-column index across several columns, having a single-column index against the first column in the multi-column index is redundant and superfluous – the multi-column index can be used just as easily in queries only constraining that one, left-most column.

See also:
- [Multi-column index](https://www.celerity.com/how-to-design-sql-indexes/)

