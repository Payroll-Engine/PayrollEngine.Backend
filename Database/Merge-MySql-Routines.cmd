@echo off
:: Merge-MySql-Routines.cmd
:: Merges all MySQL Functions and Stored Procedures into a single output file.
::
:: Output: Database\MySql-Routines.merged.sql
::
:: Usage: Merge-MySql-Routines.cmd

setlocal

set BASE_DIR=%~dp0..
set FN_DIR=%BASE_DIR%\Persistence\Persistence.MySql\Functions
set SP_DIR=%BASE_DIR%\Persistence\Persistence.MySql\StoredProcedures
set OUTPUT=%~dp0MySql-Routines.merged.sql

if exist "%OUTPUT%" del "%OUTPUT%"

echo -- ============================================================= >> "%OUTPUT%"
echo -- MySQL Functions >> "%OUTPUT%"
echo -- ============================================================= >> "%OUTPUT%"

for %%f in ("%FN_DIR%\*.mysql.sql") do (
    echo. >> "%OUTPUT%"
    echo -- %%~nxf >> "%OUTPUT%"
    type "%%f" >> "%OUTPUT%"
    echo. >> "%OUTPUT%"
)

echo. >> "%OUTPUT%"
echo -- ============================================================= >> "%OUTPUT%"
echo -- MySQL Stored Procedures >> "%OUTPUT%"
echo -- ============================================================= >> "%OUTPUT%"

for %%f in ("%SP_DIR%\*.mysql.sql") do (
    echo. >> "%OUTPUT%"
    echo -- %%~nxf >> "%OUTPUT%"
    type "%%f" >> "%OUTPUT%"
    echo. >> "%OUTPUT%"
)

echo Done: %OUTPUT%
endlocal
