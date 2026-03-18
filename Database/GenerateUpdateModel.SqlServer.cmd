@echo off
echo Generating SQL Server Update-Model.sql...
cd /d "%~dp0"
powershell -ExecutionPolicy Bypass -File "..\..\..\..\PayrollEngine\devops\scripts\Generate-DbUpdate.ps1" ^
    -DatabaseDir "%~dp0"
echo.
echo Done. Review Update-Model.sql and add any missing ALTER TABLE statements.
