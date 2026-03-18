@echo off
setlocal

set CONTAINER=pe-poc
set PASSWORD=poc123
set DB=PayrollEngine

echo Dropping MySQL database...
docker exec -i %CONTAINER% mysql -uroot -p%PASSWORD% -e "DROP DATABASE IF EXISTS %DB%;"

echo Done.
endlocal
