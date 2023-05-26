@echo off
call DbQuery PayrollEngine_DatabaseSetup_1.0.0.sql "server=localhost; Integrated Security=SSPI;TrustServerCertificate=True;"
pause

