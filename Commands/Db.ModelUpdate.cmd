@echo off
echo recreate database from scripts
echo.

call Db.ModelDrop
call Db.ModelCreate
pause