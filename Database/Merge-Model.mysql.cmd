@echo off
setlocal
powershell -ExecutionPolicy Bypass -File "%~dp0Merge-Model.mysql.ps1"
endlocal
