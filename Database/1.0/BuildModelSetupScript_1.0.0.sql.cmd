@echo off
@echo Building the model setup script...
@echo.
copy CreateObjectsDbScript.sql + DatabaseVersion_1.0.0.sql PayrollEngine_ModelSetup_1.0.0.sql
@echo.
@echo Model setup script PayrollEngine_ModelSetup_1.0.0.sql created
@echo.
pause
