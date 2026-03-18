@echo off
:: =============================================================================
:: Prepare-Init.cmd
:: Copies all MySQL scripts into docker\init\ in the correct execution order.
:: Run this once before 'docker compose up' or after any script update.
:: =============================================================================
setlocal

set DB_DIR=%~dp0..
set SP_DIR=%~dp0..\..\Persistence\Persistence.MySql\StoredProcedures
set FN_DIR=%~dp0..\..\Persistence\Persistence.MySql\Functions
set INIT_DIR=%~dp0init

echo Copying MySQL init scripts to %INIT_DIR%...

copy /Y "%DB_DIR%\Create-Model.mysql.sql"             "%INIT_DIR%\01-Create-Model.mysql.sql"
copy /Y "%FN_DIR%\Functions.mysql.sql"                "%INIT_DIR%\02-Functions.mysql.sql"
copy /Y "%SP_DIR%\GetDerived.mysql.sql"               "%INIT_DIR%\03-GetDerived.mysql.sql"
copy /Y "%SP_DIR%\GetCaseValues.mysql.sql"            "%INIT_DIR%\04-GetCaseValues.mysql.sql"
copy /Y "%SP_DIR%\GetLookupRangeValue.mysql.sql"      "%INIT_DIR%\05-GetLookupRangeValue.mysql.sql"
copy /Y "%SP_DIR%\Remaining.mysql.sql"                "%INIT_DIR%\06-Remaining.mysql.sql"
copy /Y "%SP_DIR%\GetResults.mysql.sql"               "%INIT_DIR%\07-GetResults.mysql.sql"

echo Done. Run 'docker compose up -d' to start the container.
endlocal
