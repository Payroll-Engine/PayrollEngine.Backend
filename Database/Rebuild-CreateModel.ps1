# Rebuild-CreateModel.ps1
# Rebuilds Create-Model.mysql.sql from:
#   - static part (tables + indexes) extracted from the existing file
#   - all Functions from Persistence.MySql\Functions\*.mysql.sql
#   - all Stored Procedures from Persistence.MySql\StoredProcedures\*.mysql.sql
#
# Usage: .\Rebuild-CreateModel.ps1
# Run before: CreateModel.MySql.cmd

param(
    [string]$BaseDir = (Join-Path $PSScriptRoot "..")
)

$DbDir  = $PSScriptRoot
$FnDir  = Join-Path $BaseDir "Persistence\Persistence.MySql\Functions"
$SpDir  = Join-Path $BaseDir "Persistence\Persistence.MySql\StoredProcedures"
$Target = Join-Path $DbDir   "Create-Model.mysql.sql"

# --------------------------------------------------------------------------
# 1. Extract static part (everything before the routines section marker)
# --------------------------------------------------------------------------
Write-Host "Reading static part from $Target ..."
$existing = [System.IO.File]::ReadAllText($Target, [System.Text.Encoding]::UTF8)

# The marker line that starts the routines block (has typo "PROCEDURS" — keep as anchor)
$marker = "-- FUNCTIONS AND STORED PROCEDURS"
$pos = $existing.IndexOf($marker)
if ($pos -lt 0) {
    # Fallback: try correct spelling
    $marker = "-- FUNCTIONS AND STORED PROCEDURES"
    $pos = $existing.IndexOf($marker)
}
if ($pos -lt 0) {
    Write-Error "ERROR: Routines section marker not found in Create-Model.mysql.sql. Aborting."
    exit 1
}

# Walk back to the start of the separator comment block (the === line before the marker)
$sepLine = "-- ============================================================================="
$sepPos  = $existing.LastIndexOf($sepLine, $pos)
if ($sepPos -lt 0) { $sepPos = $pos }

$staticPart = $existing.Substring(0, $sepPos).TrimEnd() + "`r`n"
Write-Host "  Static part: $($staticPart.Length) chars"

# --------------------------------------------------------------------------
# 2. Build routines block from individual source files
# --------------------------------------------------------------------------
$sb = [System.Text.StringBuilder]::new(300000)

$null = $sb.AppendLine("")
$null = $sb.AppendLine("-- =============================================================================")
$null = $sb.AppendLine("-- FUNCTIONS (7)")
$null = $sb.AppendLine("-- DELIMITER `$`$ avoids conflicts with `$ in JSON PATH expressions")
$null = $sb.AppendLine("-- =============================================================================")
$null = $sb.AppendLine("")

$fnFiles = Get-ChildItem $FnDir -Filter "*.mysql.sql" | Sort-Object Name
Write-Host "  Functions: $($fnFiles.Count)"
foreach ($f in $fnFiles) {
    $null = $sb.AppendLine("-- $($f.Name)")
    $null = $sb.AppendLine([System.IO.File]::ReadAllText($f.FullName, [System.Text.Encoding]::UTF8).TrimEnd())
    $null = $sb.AppendLine("")
}

$null = $sb.AppendLine("")
$null = $sb.AppendLine("-- =============================================================================")
$null = $sb.AppendLine("-- STORED PROCEDURES (44)")
$null = $sb.AppendLine("-- =============================================================================")
$null = $sb.AppendLine("")

$spFiles = Get-ChildItem $SpDir -Filter "*.mysql.sql" | Sort-Object Name
Write-Host "  Stored Procedures: $($spFiles.Count)"
foreach ($f in $spFiles) {
    $null = $sb.AppendLine("-- $($f.Name)")
    $null = $sb.AppendLine([System.IO.File]::ReadAllText($f.FullName, [System.Text.Encoding]::UTF8).TrimEnd())
    $null = $sb.AppendLine("")
}

# --------------------------------------------------------------------------
# 3. VERSION RECORD
# --------------------------------------------------------------------------
$versionRecord = @"

-- =============================================================================
-- VERSION RECORD
-- =============================================================================

INSERT INTO ``Version`` (Created, MajorVersion, MinorVersion, SubVersion, Owner, Description)
VALUES (NOW(6), 0, 9, 6, CURRENT_USER(), 'Payroll Engine: Full setup v0.9.6 (MySQL)');

SELECT 'PayrollEngine MySQL schema v0.9.6 created successfully.' AS Result;
"@

# --------------------------------------------------------------------------
# 4. Assemble and write (UTF-8 without BOM)
# --------------------------------------------------------------------------
$newContent = $staticPart + $sb.ToString() + $versionRecord
[System.IO.File]::WriteAllText($Target, $newContent, (New-Object System.Text.UTF8Encoding $false))

Write-Host ""
Write-Host "Done: $Target"
Write-Host "  Total size: $($newContent.Length) chars"
Write-Host "  Functions : $($fnFiles.Count)"
Write-Host "  SPs       : $($spFiles.Count)"
