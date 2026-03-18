param(
    [string]$Container = "pe-poc",
    [string]$Password  = "poc123",
    [string]$Database  = "PayrollEngine"
)

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$SqlFile   = Join-Path $ScriptDir "Create-Model.mysql.sql"
$TmpBat    = Join-Path $env:TEMP "pe_mysql_create.bat"

Write-Host "Dropping existing database (if any)..."
docker exec -i $Container mysql -uroot "-p$Password" -e "DROP DATABASE IF EXISTS ${Database};"

Write-Host "Creating schema (tables, indexes, functions, stored procedures)..."

# Write a temp .bat that uses native CMD redirect -- no PowerShell string expansion of $
$bat = "@echo off`r`ndocker exec -i $Container mysql -uroot -p$Password < `"$SqlFile`""
[System.IO.File]::WriteAllText($TmpBat, $bat, [System.Text.Encoding]::ASCII)

cmd /c $TmpBat
$exitCode = $LASTEXITCODE
Remove-Item $TmpBat -ErrorAction SilentlyContinue

if ($exitCode -ne 0) {
    Write-Error "ERROR: Schema creation failed (exit code $exitCode)."
    exit 1
}

Write-Host ""
Write-Host "Verifying..."
docker exec -i $Container mysql -uroot "-p$Password" -e "SELECT COUNT(*) AS tables FROM information_schema.TABLES WHERE TABLE_SCHEMA='$Database' AND TABLE_TYPE='BASE TABLE'; SELECT ROUTINE_TYPE, COUNT(*) AS count FROM information_schema.ROUTINES WHERE ROUTINE_SCHEMA='$Database' GROUP BY ROUTINE_TYPE; SELECT MajorVersion, MinorVersion, SubVersion FROM ${Database}.Version ORDER BY Id DESC LIMIT 1;"

Write-Host ""
Write-Host "Done."
