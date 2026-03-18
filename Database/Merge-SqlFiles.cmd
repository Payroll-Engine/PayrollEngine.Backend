@echo off
:: Merge-SqlFiles.cmd
:: Merges all *.sql files in a directory into a single output file.
::
:: Usage:
::   Merge-SqlFiles.cmd <SourceDir> <OutputFile>
::
:: Example:
::   Merge-SqlFiles.cmd ..\Persistence\Persistence.MySql\StoredProcedures\GetResults merged.sql

setlocal

if "%~1"=="" (
    echo Usage: Merge-SqlFiles.cmd ^<SourceDir^> ^<OutputFile^>
    exit /b 1
)
if "%~2"=="" (
    echo Usage: Merge-SqlFiles.cmd ^<SourceDir^> ^<OutputFile^>
    exit /b 1
)

set SOURCE_DIR=%~1
set OUTPUT=%~2

if not exist "%SOURCE_DIR%" (
    echo ERROR: Directory not found: %SOURCE_DIR%
    exit /b 1
)

:: Delete existing output
if exist "%OUTPUT%" del "%OUTPUT%"

:: Merge all *.sql files
copy /b /y "%SOURCE_DIR%\*.sql" "%OUTPUT%" > nul

echo Merged files from %SOURCE_DIR% into %OUTPUT%
dir "%SOURCE_DIR%\*.sql" /b
echo.
echo Output: %OUTPUT%

endlocal
