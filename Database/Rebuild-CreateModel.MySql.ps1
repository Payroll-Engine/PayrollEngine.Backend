# Rebuild-CreateModel.MySql.ps1
# Rebuilds Create-Model.mysql.sql in correct dependency order:
#
#   1. DATABASE   -- CREATE DATABASE, USE, globals
#   2. TABLES     -- CREATE TABLE IF NOT EXISTS
#   3. INDEXES    -- CREATE [UNIQUE] INDEX (without IF NOT EXISTS - not supported in MySQL)
#   4. FUNCTIONS  -- from Persistence.MySql\Functions\*.mysql.sql
#   5. PROCS      -- from Persistence.MySql\StoredProcedures\*.mysql.sql
#   6. VERSION    -- INSERT INTO Version
#
# Strategy:
#   Static sections (Database, Tables, Indexes) are extracted from the existing
#   Create-Model.mysql.sql. Functions and SPs are loaded from individual source
#   files. Splitting uses SQL object patterns as reliable anchors.
#
# Usage:
#   .\Rebuild-CreateModel.MySql.ps1
#   .\Rebuild-CreateModel.MySql.ps1 -Verbose
#   .\Rebuild-CreateModel.MySql.ps1 -DryRun
#
# Run before: CreateModel.MySql.cmd

param(
    [string]$BaseDir = (Join-Path $PSScriptRoot ".."),
    [switch]$DryRun,
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$DbDir  = $PSScriptRoot
$FnDir  = Join-Path $BaseDir "Persistence\Persistence.MySql\Functions"
$SpDir  = Join-Path $BaseDir "Persistence\Persistence.MySql\StoredProcedures"
$Source = Join-Path $DbDir "Create-Model.mysql.sql"
$Target = Join-Path $DbDir "Create-Model.mysql.sql"

# =============================================================================
# 1. Read existing file
# =============================================================================
Write-Host "Reading: $Source"
$raw     = [System.IO.File]::ReadAllText($Source, [System.Text.Encoding]::UTF8)
$content = $raw.Replace("`r`n", "`n").Replace("`r", "`n")

# =============================================================================
# 2. Locate section boundaries
# =============================================================================

# DATABASE + TABLES + INDEXES = everything before the first DELIMITER $$ (start of Functions/SPs)
# "DELIMITER $$" is the reliable boundary between static DDL and routines in MySQL
$delimiterMarker = "`nDELIMITER `$`$"
$routinesStart = $content.IndexOf($delimiterMarker)
if ($routinesStart -lt 0) { throw "Cannot locate 'DELIMITER `$`$' in source file - routines boundary not found" }

$staticRaw = $content.Substring(0, $routinesStart).TrimEnd()

# VERSION = last INSERT INTO `Version` block (after all DELIMITER ;)
# Find last occurrence of the separator before the version INSERT
$versionInsertMarker = "INSERT INTO ``Version``"
$versionPos = $content.LastIndexOf($versionInsertMarker)
if ($versionPos -lt 0) {
    $versionInsertMarker = 'INSERT INTO `Version`'
    $versionPos = $content.LastIndexOf($versionInsertMarker)
}
if ($versionPos -lt 0) { throw "Cannot locate VERSION INSERT in source file" }

# Walk back to the separator line above the version block
$sepLine       = "-- ============================================================================="
$versionSepPos = $content.LastIndexOf($sepLine, $versionPos)
if ($versionSepPos -lt 0) { $versionSepPos = $versionPos }
$versionSection = $content.Substring($versionSepPos).TrimEnd()

# Validate static section
if ($staticRaw -notmatch 'CREATE TABLE IF NOT EXISTS') {
    throw "VALIDATION FAILED: No 'CREATE TABLE IF NOT EXISTS' in static section"
}
if ($staticRaw -match '(?m)^DELIMITER|(?m)^CREATE FUNCTION|(?m)^CREATE PROCEDURE') {
    throw "VALIDATION FAILED: Routines found in static section - boundary detection failed"
}

$tableCount = ([regex]::Matches($staticRaw, 'CREATE TABLE IF NOT EXISTS')).Count
$indexCount = ([regex]::Matches($staticRaw, 'CREATE (UNIQUE )?INDEX')).Count
Write-Host "  [OK] DATABASE+TABLES+INDEXES: $tableCount tables, $indexCount indexes"
Write-Host "  [OK] VERSION section:         $($versionSection.Length) chars"

# =============================================================================
# 3. Load Functions
# =============================================================================
Write-Host "Loading Functions from: $FnDir"
$fnFiles = Get-ChildItem $FnDir -Filter "*.mysql.sql" | Sort-Object Name
if ($fnFiles.Count -eq 0) { throw "No Function files found in $FnDir" }
Write-Host "  Functions: $($fnFiles.Count)"

$sep = "-- ============================================================================="
$fnSb = [System.Text.StringBuilder]::new()
[void]$fnSb.AppendLine()
[void]$fnSb.AppendLine($sep)
[void]$fnSb.AppendLine("-- FUNCTIONS ($($fnFiles.Count))")
[void]$fnSb.AppendLine("-- DELIMITER `$`$ avoids conflicts with `$ in JSON PATH expressions")
[void]$fnSb.AppendLine($sep)
[void]$fnSb.AppendLine()
foreach ($f in $fnFiles) {
    if ($Verbose) { Write-Host "    + $($f.Name)" }
    [void]$fnSb.AppendLine("-- $($f.Name)")
    [void]$fnSb.AppendLine([System.IO.File]::ReadAllText($f.FullName, [System.Text.Encoding]::UTF8).Trim())
    [void]$fnSb.AppendLine()
}

# =============================================================================
# 4. Load Stored Procedures
# =============================================================================
Write-Host "Loading Stored Procedures from: $SpDir"
# Reorder so callee SPs precede callers.
# Known dependency: DeleteAllCaseValues calls the four sub-Delete SPs.
$spFilesAll = Get-ChildItem $SpDir -Filter "*.mysql.sql" | Sort-Object Name
if ($spFilesAll.Count -eq 0) { throw "No SP files found in $SpDir" }

$spDepFirst = @(
    'DeleteAllCompanyCaseValues.mysql.sql',
    'DeleteAllEmployeeCaseValues.mysql.sql',
    'DeleteAllGlobalCaseValues.mysql.sql',
    'DeleteAllNationalCaseValues.mysql.sql'
)
$spOrdered = [System.Collections.Generic.List[System.IO.FileInfo]]::new()
foreach ($dep in $spDepFirst) {
    $f = $spFilesAll | Where-Object { $_.Name -eq $dep }
    if ($f) { $spOrdered.Add($f) }
}
foreach ($f in $spFilesAll) {
    if ($spDepFirst -notcontains $f.Name) { $spOrdered.Add($f) }
}
$spFiles = $spOrdered
if ($spFiles.Count -eq 0) { throw "No SP files found in $SpDir" }
Write-Host "  Stored Procedures: $($spFiles.Count)"

$spSb = [System.Text.StringBuilder]::new()
[void]$spSb.AppendLine()
[void]$spSb.AppendLine($sep)
[void]$spSb.AppendLine("-- STORED PROCEDURES ($($spFiles.Count))")
[void]$spSb.AppendLine($sep)
[void]$spSb.AppendLine()
foreach ($f in $spFiles) {
    if ($Verbose) { Write-Host "    + $($f.Name)" }
    [void]$spSb.AppendLine("-- $($f.Name)")
    [void]$spSb.AppendLine([System.IO.File]::ReadAllText($f.FullName, [System.Text.Encoding]::UTF8).Trim())
    [void]$spSb.AppendLine()
}

# =============================================================================
# 5. Assemble in dependency order
# =============================================================================
$out = [System.Text.StringBuilder]::new(2000000)

# 1+2+3. DATABASE + TABLES + INDEXES
[void]$out.AppendLine($staticRaw)

# 4. FUNCTIONS
[void]$out.Append($fnSb.ToString())

# 5. STORED PROCS
[void]$out.Append($spSb.ToString())

# 6. VERSION
[void]$out.AppendLine()
[void]$out.AppendLine($versionSection)

$newContent = $out.ToString()

# =============================================================================
# 6. Validate assembly order
# =============================================================================
Write-Host ""
Write-Host "Validating assembly order..."

$norm     = $newContent.Replace("`r`n", "`n").Replace("`r", "`n")
$posTable = $norm.IndexOf("CREATE TABLE IF NOT EXISTS")
$posIndex = $norm.IndexOf("CREATE INDEX ")
if ($posIndex -lt 0) { $posIndex = $norm.IndexOf("CREATE UNIQUE INDEX ") }
$posFn    = $norm.IndexOf("CREATE FUNCTION")
$posSp    = $norm.IndexOf("CREATE PROCEDURE")
$posVer   = $norm.IndexOf("INSERT INTO")

$checks = @(
    @{ Label = "Tables  before Indexes";   Ok = ($posIndex -lt 0 -or ($posTable -gt 0 -and $posTable -lt $posIndex)) }
    @{ Label = "Tables  before Functions"; Ok = ($posTable -gt 0 -and $posFn -gt 0 -and $posTable -lt $posFn) }
    @{ Label = "Functions before Procs";   Ok = ($posFn   -gt 0 -and $posSp  -gt 0 -and $posFn   -lt $posSp) }
    @{ Label = "Procs    before Version";  Ok = ($posSp   -gt 0 -and $posVer  -gt 0 -and $posSp   -lt $posVer) }
)

$ok = $true
foreach ($c in $checks) {
    $icon = if ($c.Ok) { "[OK]" } else { "[FAIL]" }
    Write-Host "  $icon $($c.Label)"
    if (-not $c.Ok) { $ok = $false }
}

if (-not $ok) { throw "Assembly order validation failed - file not written." }
Write-Host "  [OK] Total size: $($newContent.Length) chars"

# =============================================================================
# 7. Write (UTF-8 without BOM)
# =============================================================================
if ($DryRun) {
    Write-Host ""
    Write-Host "[DRY RUN] No file written."
} else {
    [System.IO.File]::WriteAllText(
        $Target,
        $newContent,
        (New-Object System.Text.UTF8Encoding $false))
    Write-Host ""
    Write-Host "Done: $Target"
    Write-Host "  Tables    : $tableCount"
    Write-Host "  Indexes   : $indexCount"
    Write-Host "  Functions : $($fnFiles.Count)"
    Write-Host "  Procs     : $($spFiles.Count)"
}
