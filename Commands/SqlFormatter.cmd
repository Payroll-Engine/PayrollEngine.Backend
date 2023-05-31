@ECHO off

REM --- arguments ---
if "%~1"=="" goto help

REM Poormans SQL Formatter
REM https://github.com/TaoK/PoorMansTSqlFormatter
REM is: indentString
REM tc: trailingCommas on
REM bjo: breakJoinOnSections on
REM b: backup off
call %~dp0\SqlFormatter\SqlFormatter.exe %1 /is:"\s\s" /st:2 /tc /bjo /b-
goto exit

:help
echo.
echo usage: SqlFormatter FileOrFolder
echo.
goto exit

:exit
