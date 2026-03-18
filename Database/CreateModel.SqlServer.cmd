@echo off
echo Creating SQL Server database...
sqlcmd -S localhost -E -i "%~dp0Create-Model.sql"
echo Done.
