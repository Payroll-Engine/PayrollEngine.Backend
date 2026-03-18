@echo off
:: =============================================================================
:: GenerateUpdateModel.MySql.cmd
::
:: MySQL has no automated diff generator. Update-Model.mysql.sql must be
:: maintained manually based on the changes made to Create-Model.mysql.sql
:: and the StoredProcedures files.
::
:: Checklist for a new release:
::   1. For each changed table: add ALTER TABLE statements
::   2. For each changed SP/function: add DROP + CREATE block
::   3. Update the VERSION_SET block at the end
::   4. Test against a v(OldVersion) database before release
:: =============================================================================

echo.
echo MySQL Update Script - Manual Maintenance Required
echo =================================================
echo.
echo Update-Model.mysql.sql must be maintained manually.
echo.
echo Checklist:
echo   1. ALTER TABLE for each changed table column
echo   2. DROP + CREATE for each changed SP or function
echo   3. Update VERSION_SET block at the end of the file
echo   4. Test against a database on the previous version
echo.
echo Opening Update-Model.mysql.sql for editing...
echo.

if exist "%~dp0Update-Model.mysql.sql" (
    start "" notepad "%~dp0Update-Model.mysql.sql"
) else (
    echo File not found: Update-Model.mysql.sql
    echo Create it manually based on Update-Model.sql as a reference.
)
