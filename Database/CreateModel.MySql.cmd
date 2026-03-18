@echo off
setlocal
powershell -ExecutionPolicy Bypass -File "%~dp0CreateModel.MySql.ps1"
endlocal
