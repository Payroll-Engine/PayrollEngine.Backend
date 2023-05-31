@echo off
echo recreate database from scripts
echo.

call DbObjects.Drop
call DbObjects.Create
pause