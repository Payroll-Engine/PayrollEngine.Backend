@echo off

@echo Publishing database scripts to %target%...
@echo.
pushd ..\Database\Current\
copy /B DefaultDatabaseCreate.sql ..\Public\SetupDefaultDatabase.sql
copy /B ModelCreate.sql + VersionCreate.sql ..\Public\SetupModel.sql
popd
pushd ..\Public\
dir
pause
popd
