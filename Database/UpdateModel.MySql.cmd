@echo off
setlocal

set CONTAINER=pe-poc
set PASSWORD=poc123
set DB=PayrollEngine
set DB_DIR=%~dp0

echo Copying update script to container...
docker cp "%DB_DIR%Update-Model.mysql.sql" %CONTAINER%:/tmp/Update-Model.mysql.sql

echo Updating MySQL database...
docker exec -i %CONTAINER% mysql -uroot -p%PASSWORD% %DB% -e "source /tmp/Update-Model.mysql.sql"

echo Done.
endlocal
