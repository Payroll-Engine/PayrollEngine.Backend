@echo off
echo dump database create object schema script

REM --- script all DDL objects ---
REM script generated by the toll mssql-scripter
REM further info https://github.com/microsoft/mssql-scripter
call SqlScripter --script-create --exclude-use-database --exclude-extended-properties -f ../Database/ModelCreate.sql
echo formatting script
call SqlFormatter ..\Database\ModelCreate.sql
