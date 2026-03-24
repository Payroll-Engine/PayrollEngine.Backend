# Rebuild-CreateModel.SqlServer.ps1
# Rebuilds Create-Model.sql in correct dependency order:
#
#   1. DATABASE    -- CREATE DATABASE, collation, RCSI
#   2. Tables      -- CREATE TABLE
#   3. Indexes     -- CREATE [UNIQUE] [NONCLUSTERED] INDEX
#   4. Constraints -- ALTER TABLE ... ADD CONSTRAINT (DEFAULT, CHECK, FK)
#   5. Functions   -- Inline TVFs from Persistence.SqlServer\Functions\*.sql
#   6. Procs       -- Stored Procedures from Persistence.SqlServer\StoredProcedures\*.sql
#   7. Version     -- INSERT INTO Version
#
# Why this order matters:
#   SQL Server Inline-TVFs resolve object references at CREATE time (eager binding).
#   GetDerivedRegulations references RegulationShare via EXISTS.
#   => Tables MUST exist before Functions are created.
#
# Strategy:
#   Split DB_SCRIPTS block into individual object blocks using the SSMS header comment
#   pattern "/****** Object: <type> ". Classify each block as Table, Function, or SP.
#   ALTER TABLE blocks (Constraints, Defaults, FKs) are treated as a separate class.
#   Functions and SPs are replaced entirely from the individual source files.
#
# Usage:
#   .\Rebuild-CreateModel.SqlServer.ps1
#   .\Rebuild-CreateModel.SqlServer.ps1 -Verbose
#   .\Rebuild-CreateModel.SqlServer.ps1 -DryRun   (validate only, no file written)
#
# Run before: CreateModel.SqlServer.cmd

param(
    [string]$BaseDir = (Join-Path $PSScriptRoot ".."),
    [switch]$DryRun,
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$DbDir  = $PSScriptRoot
$FnDir  = Join-Path $BaseDir "Persistence\Persistence.SqlServer\Functions"
$SpDir  = Join-Path $BaseDir "Persistence\Persistence.SqlServer\StoredProcedures"
$Source = Join-Path $DbDir "Create-Model.sql"
$Target = Join-Path $DbDir "Create-Model.sql"

# =============================================================================
# 1. Read source file
# =============================================================================
Write-Host "Reading: $Source"
$raw     = [System.IO.File]::ReadAllText($Source, [System.Text.Encoding]::UTF8)
$content = $raw.Replace("`r`n", "`n").Replace("`r", "`n")

# =============================================================================
# 2. Extract fixed sections by region markers
# =============================================================================

function Get-Region([string]$text, [string]$start, [string]$end) {
    $s = $text.IndexOf($start)
    $e = $text.IndexOf($end)
    if ($s -lt 0) { throw "Region marker not found: '$start'" }
    if ($e -lt 0) { throw "Region marker not found: '$end'" }
    return $text.Substring($s, $e - $s + $end.Length).TrimEnd()
}

$dbSection      = Get-Region $content "-- #region DATABASE"    "-- #endregion DATABASE"
$versionSection = Get-Region $content "-- #region VERSION_SET" "-- #endregion VERSION_SET"

Write-Host "  [OK] DATABASE section : $($dbSection.Length) chars"
Write-Host "  [OK] VERSION section  : $($versionSection.Length) chars"

# =============================================================================
# 3. Extract DB_SCRIPTS block and split into individual object blocks
#
#    Each object in an SSMS-exported script is preceded by a header comment:
#    /****** Object:  <ObjectType> [dbo].[Name]  ...  ******/
#    We use this as the split boundary.
# =============================================================================

$dbScriptsStart = $content.IndexOf("-- #region DB_SCRIPTS")
$dbScriptsEnd   = $content.IndexOf("-- #endregion DB_SCRIPTS")
if ($dbScriptsStart -lt 0) { throw "Region marker not found: '-- #region DB_SCRIPTS'" }
if ($dbScriptsEnd   -lt 0) { throw "Region marker not found: '-- #endregion DB_SCRIPTS'" }

$dbScripts = $content.Substring(
    $dbScriptsStart + "-- #region DB_SCRIPTS".Length,
    $dbScriptsEnd   - $dbScriptsStart - "-- #region DB_SCRIPTS".Length
).Trim()

# Split on SSMS object header pattern -- keep the delimiter with each block
$headerPattern = [regex]'/\*{6} Object:'
$positions = @($headerPattern.Matches($dbScripts) | ForEach-Object { $_.Index })

# Collect blocks: everything before first header is the preamble (USE statement etc.)
$preamble = if ($positions.Count -gt 0) {
    $dbScripts.Substring(0, $positions[0]).Trim()
} else { $dbScripts.Trim() }

$blocks = @()
for ($i = 0; $i -lt $positions.Count; $i++) {
    $start  = $positions[$i]
    $length = if ($i + 1 -lt $positions.Count) { $positions[$i+1] - $start } else { $dbScripts.Length - $start }
    $blocks += $dbScripts.Substring($start, $length).Trim()
}

Write-Host "  [OK] DB_SCRIPTS blocks: $($blocks.Count) objects"

# =============================================================================
# 4. Classify blocks by SSMS object type keyword
# =============================================================================

$tablePreamble  = [System.Text.StringBuilder]::new()   # USE [PayrollEngine] GO etc.
$tableBlocks    = [System.Collections.Generic.List[string]]::new()
$constraintBlocks = [System.Collections.Generic.List[string]]::new()  # ALTER TABLE ... ADD CONSTRAINT

# Functions and SPs come from source files -- we only count them here for reporting
$fnBlockCount = 0
$spBlockCount = 0

# The preamble contains USE [PayrollEngine]; GO
if ($preamble) {
    [void]$tablePreamble.AppendLine($preamble)
    [void]$tablePreamble.AppendLine()
}

foreach ($block in $blocks) {
    # Determine type from the SSMS header comment
    if ($block -match '/\*{6} Object:\s+UserDefinedFunction') {
        $fnBlockCount++
        if ($Verbose) { 
            $name = if ($block -match '\[dbo\]\.\[([^\]]+)\]') { $Matches[1] } else { '?' }
            Write-Host "    [Function] $name"
        }
    }
    elseif ($block -match '/\*{6} Object:\s+StoredProcedure') {
        $spBlockCount++
        if ($Verbose) {
            $name = if ($block -match '\[dbo\]\.\[([^\]]+)\]') { $Matches[1] } else { '?' }
            Write-Host "    [SP      ] $name"
        }
    }
    elseif ($block -match '/\*{6} Object:\s+Table') {
        # Table block may contain the CREATE TABLE AND trailing ALTER TABLE ... ADD CONSTRAINT blocks
        # Split those out so constraints can be placed after all tables
        $alterPos = $block.IndexOf("`nALTER TABLE")
        if ($alterPos -gt 0) {
            $tableBlocks.Add($block.Substring(0, $alterPos).TrimEnd())
            $constraintBlocks.Add($block.Substring($alterPos).Trim())
        } else {
            $tableBlocks.Add($block)
        }
        if ($Verbose) {
            $name = if ($block -match '\[dbo\]\.\[([^\]]+)\]') { $Matches[1] } else { '?' }
            Write-Host "    [Table   ] $name"
        }
    }
    else {
        # Other object types (Index as standalone, etc.) -- attach to constraints block
        $constraintBlocks.Add($block)
        if ($Verbose) { Write-Host "    [Other   ] (non-TVF/SP/Table block, added to constraints)" }
    }
}

Write-Host "  [OK] Tables     : $($tableBlocks.Count)"
Write-Host "  [OK] Constraints: $($constraintBlocks.Count) blocks"
Write-Host "  [OK] Functions  : $fnBlockCount (from DB_SCRIPTS, will be replaced from source files)"
Write-Host "  [OK] Procs      : $spBlockCount (from DB_SCRIPTS, will be replaced from source files)"

if ($tableBlocks.Count -eq 0) { throw "VALIDATION FAILED: No Table blocks found in DB_SCRIPTS" }

# =============================================================================
# 5. Load Functions from individual source files
# =============================================================================
Write-Host "Loading Functions from: $FnDir"
$fnFiles = Get-ChildItem $FnDir -Filter "*.sql" | Sort-Object Name
if ($fnFiles.Count -eq 0) { throw "No Function files found in $FnDir" }

$fnSb = [System.Text.StringBuilder]::new()
foreach ($f in $fnFiles) {
    if ($Verbose) { Write-Host "    + $($f.Name)" }
    [void]$fnSb.AppendLine([System.IO.File]::ReadAllText($f.FullName, [System.Text.Encoding]::UTF8).Trim())
    [void]$fnSb.AppendLine()
}
Write-Host "  [OK] Functions loaded: $($fnFiles.Count)"

# =============================================================================
# 6. Load Stored Procedures from individual source files
# =============================================================================
Write-Host "Loading Stored Procedures from: $SpDir"
# Load all SP files, then reorder so callee SPs precede callers.
# Known dependency: DeleteAllCaseValues calls the four sub-Delete SPs.
# Sort alphabetically first, then move DeleteAllCaseValues after its dependencies.
$spFilesAll = Get-ChildItem $SpDir -Filter "*.sql" | Sort-Object Name
if ($spFilesAll.Count -eq 0) { throw "No SP files found in $SpDir" }

$spDepFirst = @(
    'DeleteAllCompanyCaseValues.sql',
    'DeleteAllEmployeeCaseValues.sql',
    'DeleteAllGlobalCaseValues.sql',
    'DeleteAllNationalCaseValues.sql'
)
# Build ordered list: dependency files first (in defined order), then all others alphabetically
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

$spSb = [System.Text.StringBuilder]::new()
foreach ($f in $spFiles) {
    if ($Verbose) { Write-Host "    + $($f.Name)" }
    [void]$spSb.AppendLine([System.IO.File]::ReadAllText($f.FullName, [System.Text.Encoding]::UTF8).Trim())
    [void]$spSb.AppendLine()
}
Write-Host "  [OK] Procs loaded     : $($spFiles.Count)"

# =============================================================================
# 7. Assemble in correct dependency order
# =============================================================================
$sep = "-- ============================================================================="
$out = [System.Text.StringBuilder]::new(2000000)

# 1. DATABASE
[void]$out.AppendLine($dbSection)
[void]$out.AppendLine()

# USE [PayrollEngine] -- must come before all objects.
# In the original SSMS export this sits inside the first Function block and
# is lost when Functions are discarded. Insert it explicitly here.
[void]$out.AppendLine("USE [PayrollEngine];")
[void]$out.AppendLine("GO")
[void]$out.AppendLine()

# Open DB_SCRIPTS region
[void]$out.AppendLine("-- #region DB_SCRIPTS")
[void]$out.AppendLine()

# 2. TABLES
[void]$out.AppendLine($sep)
[void]$out.AppendLine("-- Tables ($($tableBlocks.Count))")
[void]$out.AppendLine($sep)
[void]$out.AppendLine()
foreach ($b in $tableBlocks) {
    [void]$out.AppendLine($b)
    [void]$out.AppendLine()
}

# 3. INDEXES + CONSTRAINTS (ALTER TABLE ... ADD CONSTRAINT)
if ($constraintBlocks.Count -gt 0) {
    [void]$out.AppendLine($sep)
    [void]$out.AppendLine("-- Indexes and Constraints ($($constraintBlocks.Count))")
    [void]$out.AppendLine($sep)
    [void]$out.AppendLine()
    foreach ($b in $constraintBlocks) {
        [void]$out.AppendLine($b)
        [void]$out.AppendLine()
    }
}

# 4. FUNCTIONS (after Tables -- TVF eager binding)
[void]$out.AppendLine($sep)
[void]$out.AppendLine("-- Functions ($($fnFiles.Count))")
[void]$out.AppendLine($sep)
[void]$out.AppendLine()
[void]$out.Append($fnSb.ToString())

# 5. STORED PROCS
[void]$out.AppendLine($sep)
[void]$out.AppendLine("-- Stored Procedures ($($spFiles.Count))")
[void]$out.AppendLine($sep)
[void]$out.AppendLine()
[void]$out.Append($spSb.ToString())

# Close DB_SCRIPTS region
[void]$out.AppendLine()
[void]$out.AppendLine("-- #endregion DB_SCRIPTS")
[void]$out.AppendLine()

# 6. VERSION
[void]$out.AppendLine($versionSection)
[void]$out.AppendLine()

$newContent = $out.ToString()

# =============================================================================
# 8. Validate assembly order
# =============================================================================
Write-Host ""
Write-Host "Validating assembly order..."

$norm     = $newContent.Replace("`r`n", "`n").Replace("`r", "`n")
$posTable = $norm.IndexOf("CREATE TABLE")
$posFn    = $norm.IndexOf("CREATE FUNCTION")
$posSp    = $norm.IndexOf("CREATE PROCEDURE")
$posVer   = $norm.IndexOf("INSERT INTO")

$posUse = $norm.IndexOf("USE [PayrollEngine]")

$checks = @(
    @{ Label = "USE PayrollEngine present";  Ok = ($posUse -gt 0) }
    @{ Label = "USE      before Tables";     Ok = ($posUse -gt 0 -and $posTable -gt 0 -and $posUse -lt $posTable) }
    @{ Label = "Tables   before Functions";  Ok = ($posTable -gt 0 -and $posFn -gt 0 -and $posTable -lt $posFn) }
    @{ Label = "Functions before Procs";     Ok = ($posFn   -gt 0 -and $posSp  -gt 0 -and $posFn   -lt $posSp)  }
    @{ Label = "Procs    before Version";    Ok = ($posSp   -gt 0 -and $posVer  -gt 0 -and $posSp   -lt $posVer) }
)

$ok = $true
foreach ($c in $checks) {
    $icon = if ($c.Ok) { "[OK]" } else { "[FAIL]" }
    Write-Host "  $icon $($c.Label)"
    if (-not $c.Ok) { $ok = $false }
}
if (-not $ok) { throw "Assembly order validation failed - file not written." }
Write-Host "  [OK] Total output: $($newContent.Length) chars"

# =============================================================================
# 9. Write (UTF-8 without BOM, CRLF)
# =============================================================================
if ($DryRun) {
    Write-Host ""
    Write-Host "[DRY RUN] Validation passed. No file written."
} else {
    [System.IO.File]::WriteAllText(
        $Target,
        $newContent,
        (New-Object System.Text.UTF8Encoding $false))
    Write-Host ""
    Write-Host "Done: $Target"
    Write-Host "  Tables      : $($tableBlocks.Count)"
    Write-Host "  Constraints : $($constraintBlocks.Count)"
    Write-Host "  Functions   : $($fnFiles.Count)"
    Write-Host "  Procs       : $($spFiles.Count)"
}
