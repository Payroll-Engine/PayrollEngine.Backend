@echo off
echo Generating SQL Server Update-Model.sql...
cd /d "%~dp0"
set "DBDIR=%~dp0"
if "%DBDIR:~-1%"=="\" set "DBDIR=%DBDIR:~0,-1%"
powershell -ExecutionPolicy Bypass -File "..\..\PayrollEngine\devops\scripts\Generate-DbUpdate.ps1" ^
    -DatabaseDir "%DBDIR%"
echo.
echo Done. Review Update-Model.sql and add any missing ALTER TABLE statements.
