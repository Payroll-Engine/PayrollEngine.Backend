@echo off
echo Dropping SQL Server database...
sqlcmd -S localhost -E -Q "DROP DATABASE IF EXISTS [PayrollEngine];"
echo Done.
