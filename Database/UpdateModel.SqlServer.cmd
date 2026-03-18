@echo off
echo Updating SQL Server database...
sqlcmd -S localhost -E -i "%~dp0Update-Model.sql"
echo Done.
