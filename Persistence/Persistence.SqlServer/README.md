# Payroll Engine Persistence SQL Server
👉 This application is part of the [Payroll Engine](https://github.com/Payroll-Engine/PayrollEngine/wiki).

## Tipping Point
The SQL Server Tipping Point is the performance threshold where the Query Optimizer decides that a Non-Clustered Index Seek with Bookmark Lookups is more expensive than a full Table or Clustered Index Scan. This typically occurs when a query is expected to return between 25% and 33% of the table's data pages, prompting the engine to switch to sequential I/O for better efficiency. It primarily affects non-covering indexes and can be avoided by creating Covering Indexes that include all required columns. 

The Soted Procedure `UpdateStatistics` performs a full scan of all database tables.
```sql
DECLARE @sql nvarchar(MAX) = N'';
SELECT @sql += N'UPDATE STATISTICS ' + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name) + N' WITH FULLSCAN; '
FROM sys.tables t
	JOIN sys.schemas s ON t.schema_id = s.schema_id;

EXEC sp_executesql @sql;
```

Further info:
- [The Tipping Point Query Answers](https://www.sqlskills.com/blogs/kimberly/the-tipping-point-query-answers/)
- [How to Think Like the SQL Server Engine: What’s the Tipping Point?](https://www.brentozar.com/archive/2019/10/how-to-think-like-the-sql-server-engine-whats-the-tipping-point/)
