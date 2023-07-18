@echo off

rem query tool
set query=PayrollDbQuery
if not "%PayrollDbQuery%" == "" set query=%PayrollDbQuery%

echo query database drop object script
call %query% Query ../Database/Current/VersionCreate.sql /verbose
