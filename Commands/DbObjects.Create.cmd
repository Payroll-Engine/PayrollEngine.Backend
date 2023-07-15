@echo off

rem query tool
set query=PayrollDbQuery
if not "%PayrollDbQuery%" == "" set query=%PayrollDbQuery%

@echo off
echo query database create object script
call %query% Query ../Database/Current/CreateObjectsDbScript.sql
