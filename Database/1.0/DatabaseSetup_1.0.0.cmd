@echo off

rem query tool
set query=PayrollDbQuery
if not "%PayrollDbQuery%" == "" set query=%PayrollDbQuery%

call %query% Query PayrollEngine_DatabaseSetup_1.0.0.sql "server=localhost; Integrated Security=SSPI;TrustServerCertificate=True;"
pause
