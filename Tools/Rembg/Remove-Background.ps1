<#
.SYNOPSIS
  Remove image background using the MFFITNESS rembg venv.
.PARAMETER InputPath
  Source image path.
.PARAMETER OutputPath
  Destination PNG path (with alpha). Parent directory is created if needed.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$InputPath,

    [Parameter(Mandatory = $true)]
    [string]$OutputPath
)

$ErrorActionPreference = "Stop"

$venvPath = Join-Path $env:LOCALAPPDATA "MFFITNESS\rembg-venv"
$rembgExe = Join-Path $venvPath "Scripts\rembg.exe"
$venvPython = Join-Path $venvPath "Scripts\python.exe"

if (-not (Test-Path $rembgExe)) {
    throw "rembg not found at '$rembgExe'. Run Tools\Rembg\Install-Rembg.ps1 first."
}

if (-not (Test-Path -LiteralPath $InputPath)) {
    throw "Input file not found: $InputPath"
}

$outDir = Split-Path -Parent $OutputPath
if ($outDir -and -not (Test-Path $outDir)) {
    New-Item -ItemType Directory -Path $outDir -Force | Out-Null
}

# Ensure PNG extension for alpha output
$ext = [System.IO.Path]::GetExtension($OutputPath)
if ([string]::IsNullOrWhiteSpace($ext)) {
    $OutputPath = "$OutputPath.png"
} elseif ($ext.ToLowerInvariant() -ne ".png") {
    Write-Warning "Output should be PNG for alpha; got extension '$ext'."
}

Write-Host "rembg: $InputPath -> $OutputPath"
& $rembgExe i $InputPath $OutputPath
if ($LASTEXITCODE -ne 0) {
    throw "rembg failed (exit $LASTEXITCODE)"
}

if (-not (Test-Path -LiteralPath $OutputPath)) {
    throw "rembg reported success but output missing: $OutputPath"
}

Write-Host "Done: $OutputPath"
