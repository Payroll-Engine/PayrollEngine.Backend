@echo off
@echo Building the database setup script...
@echo.
copy  /B CreateDefaultDatabase.sql + CreateObjectsDbScript.sql + DatabaseVersion_1.0.0.sql PayrollEngine_DatabaseSetup_1.0.0.sql
@echo.
@echo Database setup script PayrollEngine_DatabaseSetup_1.0.0.sql created
@echo.
pause
