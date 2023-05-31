@echo off
echo dump database drop object data script

REM --- script all DDL objects ---
REM database connection from the environment variable MSSQL_SCRIPTER_CONNECTION_STRING
REM further info https://github.com/microsoft/mssql-scripter/blob/dev/doc/usage_guide.md#environment-variables
call SqlScripter --script-drop -f ../Database/Current/DropObjectsDbScript.sql
echo formatting script
call SqlFormatter ..\Database\Current\DropObjectsDbScript.sql