# Merge-Model.mysql.ps1
# Replaces the Functions + Stored Procedures section in Create-Model.mysql.sql
# with the current content of all *.mysql.sql source files.
#
# Start marker: separator line whose NEXT line starts with "-- FUNCTIONS"
# End marker:   separator line whose NEXT line starts with "-- VERSION RECORD"
#
# Usage:  .\Merge-Model.mysql.ps1

param(
    [string]$BaseDir = (Join-Path $PSScriptRoot "..")
)

$Target = Join-Path $PSScriptRoot "Create-Model.mysql.sql"
$FnDir  = Join-Path $BaseDir "Persistence\Persistence.MySql\Functions"
$SpDir  = Join-Path $BaseDir "Persistence\Persistence.MySql\StoredProcedures"

# --------------------------------------------------------------------------
# 1. Read lines, find section boundaries
# --------------------------------------------------------------------------
if (-not (Test-Path $Target)) { Write-Error "Not found: $Target"; exit 1 }

$lines = [System.IO.File]::ReadAllLines($Target, [System.Text.Encoding]::UTF8)

$startIdx = -1
$endIdx   = -1

for ($i = 0; $i -lt $lines.Count; $i++) {
    $l = $lines[$i].Trim()
    if ($l -match '^--\s*=+\s*$') {
        $next = if (($i + 1) -lt $lines.Count) { $lines[$i+1].Trim() } else { '' }
        if ($startIdx -lt 0 -and $next -match '^-- FUNCTIONS') {
            $startIdx = $i
        }
        if ($startIdx -ge 0 -and $endIdx -lt 0 -and $next -match '^-- VERSION RECORD') {
            $endIdx = $i
            break
        }
    }
}

if ($startIdx -lt 0) { Write-Error "Start marker '-- FUNCTIONS' not found."; exit 1 }
if ($endIdx   -lt 0) { Write-Error "End marker '-- VERSION RECORD' not found."; exit 1 }

Write-Host "  Start line    : $($startIdx + 1)"
Write-Host "  End line      : $($endIdx + 1)"

# --------------------------------------------------------------------------
# 2. Build new routines block from source files
# --------------------------------------------------------------------------
$fnFiles = Get-ChildItem $FnDir -Filter "*.mysql.sql" | Sort-Object Name
$spFiles = Get-ChildItem $SpDir -Filter "*.mysql.sql" | Sort-Object Name
Write-Host "  Functions     : $($fnFiles.Count)"
Write-Host "  Stored Procs  : $($spFiles.Count)"

$newLines = [System.Collections.Generic.List[string]]::new()

$newLines.Add("-- =============================================================================")
$newLines.Add("-- FUNCTIONS ($($fnFiles.Count))")
$newLines.Add("-- DELIMITER `$`$ avoids conflicts with `$ in JSON PATH expressions")
$newLines.Add("-- =============================================================================")

foreach ($f in $fnFiles) {
    $newLines.Add("")
    $newLines.Add("-- $($f.Name)")
    foreach ($fl in [System.IO.File]::ReadAllLines($f.FullName, [System.Text.Encoding]::UTF8)) {
        $newLines.Add($fl)
    }
}

$newLines.Add("")
$newLines.Add("-- =============================================================================")
$newLines.Add("-- STORED PROCEDURES ($($spFiles.Count))")
$newLines.Add("-- =============================================================================")

foreach ($f in $spFiles) {
    $newLines.Add("")
    $newLines.Add("-- $($f.Name)")
    foreach ($fl in [System.IO.File]::ReadAllLines($f.FullName, [System.Text.Encoding]::UTF8)) {
        $newLines.Add($fl)
    }
}

$newLines.Add("")

# --------------------------------------------------------------------------
# 3. Assemble and write (UTF-8 without BOM)
# --------------------------------------------------------------------------
$result = [System.Collections.Generic.List[string]]::new()
for ($i = 0; $i -lt $startIdx; $i++) { $result.Add($lines[$i]) }
foreach ($l in $newLines)             { $result.Add($l) }
for ($i = $endIdx; $i -lt $lines.Count; $i++) { $result.Add($lines[$i]) }

[System.IO.File]::WriteAllLines($Target, $result, (New-Object System.Text.UTF8Encoding $false))

Write-Host ""
Write-Host "Done: $Target  ($($result.Count) lines)"
