<#
.SYNOPSIS
  Prepara (y opcionalmente ejecuta) un update E2E de lab: baseline 1.0.0 → package 1.1.0.

.DESCRIPTION
  1. Copia artifacts/baselines/1.0.0 → artifacts/lab-pos
  2. Escribe request.json apuntando al ZIP 1.1.0
  3. Con -Execute: lanza UpdateManager.exe --request (TOCA LocalDB real si hay migraciones pendientes)

.EXAMPLE
  .\Scripts\Lab-RunUpdate.ps1
  .\Scripts\Lab-RunUpdate.ps1 -Execute
#>
[CmdletBinding()]
param(
    [string] $BaselineDir = '',
    [string] $PackageDir = '',
    [string] $LabDir = '',
    [switch] $Execute,
    [switch] $SkipCajaWarning
)

$ErrorActionPreference = 'Stop'
$RepoRoot = Split-Path -Parent $PSScriptRoot

if ([string]::IsNullOrWhiteSpace($BaselineDir)) {
    $BaselineDir = Join-Path $RepoRoot 'artifacts\baselines\1.0.0'
}
if ([string]::IsNullOrWhiteSpace($PackageDir)) {
    $PackageDir = Join-Path $RepoRoot 'artifacts\update-package'
}
if ([string]::IsNullOrWhiteSpace($LabDir)) {
    $LabDir = Join-Path $RepoRoot 'artifacts\lab-pos'
}

$manifestPath = Join-Path $PackageDir 'manifest.json'
if (!(Test-Path $manifestPath)) { throw "Falta manifest.json. Ejecuta Build-UpdatePackage.ps1 primero." }
if (!(Test-Path (Join-Path $BaselineDir 'UI.exe'))) { throw "Falta baseline 1.0.0 en $BaselineDir" }

$umCandidate = @(
    (Join-Path $BaselineDir 'UpdateManager\UpdateManager.exe'),
    (Join-Path $BaselineDir 'UpdateManager.exe'),
    (Join-Path $RepoRoot 'artifacts\pos\UpdateManager\UpdateManager.exe'),
    (Join-Path $RepoRoot 'artifacts\pos\UpdateManager.exe')
) | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not $umCandidate) {
    throw "No se encontró UpdateManager.exe (baseline ni artifacts/pos). Ejecuta Publish-Pos.ps1."
}

$manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
$zipPath = Join-Path $PackageDir $manifest.packageName
if (!(Test-Path $zipPath)) { throw "ZIP no encontrado: $zipPath" }

Write-Host "=== MFFITNESS Lab-RunUpdate ===" -ForegroundColor Cyan
Write-Host "Baseline: $($BaselineDir)"
Write-Host "Target:   App $($manifest.appVersion) / DB $($manifest.targetDbVersion)"
Write-Host "Lab dir:  $LabDir"

if (Test-Path $LabDir) { Remove-Item -Recurse -Force $LabDir }
New-Item -ItemType Directory -Path $LabDir | Out-Null
robocopy $BaselineDir $LabDir /E /NFL /NDL /NJH /NJS /nc /ns /np | Out-Null

$requests = Join-Path $LabDir '_lab'
New-Item -ItemType Directory -Force -Path $requests | Out-Null
$requestPath = Join-Path $requests 'request.json'

$request = [ordered]@{
    manifest = [ordered]@{
        appVersion        = $manifest.appVersion
        targetDbVersion   = [int]$manifest.targetDbVersion
        minAppVersion     = $manifest.minAppVersion
        packageName       = $manifest.packageName
        packageSha256     = $manifest.packageSha256
        releaseDate       = $manifest.releaseDate
        releaseNotesUrl   = $manifest.releaseNotesUrl
    }
    packagePath                  = (Resolve-Path $zipPath).Path
    expectedSha256               = $manifest.packageSha256
    packageVerified              = $true
    installDirectory             = (Resolve-Path $LabDir).Path
    uiExecutableName             = 'UI.exe'
    startApplicationAfterInstall = $false
}

$request | ConvertTo-Json -Depth 6 | Set-Content -Path $requestPath -Encoding UTF8

Write-Host ""
Write-Host "Request preparado:" -ForegroundColor Green
Write-Host "  $requestPath"
Write-Host ""
Write-Host "IMPORTANTE:" -ForegroundColor Yellow
Write-Host "  - UpdateManager usará la BD LocalDB configurada en DBHelper (MF CYBER DB)."
Write-Host "  - Si SchemaVersion < TargetDbVersion, aplicará migraciones REALES."
Write-Host "  - Caja debe estar CERRADA."
Write-Host "  - startApplicationAfterInstall=false (lab no abre UI automáticamente)."

if (-not $Execute) {
    Write-Host ""
    Write-Host "Dry-run OK. Para ejecutar:" -ForegroundColor Cyan
    Write-Host "  .\Scripts\Lab-RunUpdate.ps1 -Execute"
    Write-Host "o manualmente:"
    Write-Host "  & '$LabDir\UpdateManager.exe' --request '$requestPath'"
    exit 0
}

if (-not $SkipCajaWarning) {
    $ans = Read-Host "¿Caja CERRADA y confirmas ejecutar update REAL sobre LocalDB? (yes/no)"
    if ($ans -ne 'yes') {
        Write-Host "Abortado por el usuario."
        exit 2
    }
}

$um = $umCandidate
# Si el manager del baseline está DENTRO de lab-pos, las DLLs del install se bloquean.
# Preferir siempre un runtime fuera del InstallDirectory cuando sea posible.
$preferredUm = Join-Path $RepoRoot 'artifacts\pos\UpdateManager\UpdateManager.exe'
if (Test-Path $preferredUm) {
    $um = $preferredUm
}
elseif ((Split-Path -Parent $um) -eq (Resolve-Path $LabDir).Path) {
    Write-Host "ADVERTENCIA: UpdateManager está en el mismo folder que se va a actualizar (posible file lock)." -ForegroundColor Yellow
}

Write-Host "Ejecutando UpdateManager desde: $um" -ForegroundColor Cyan
& $um --request $requestPath
$code = $LASTEXITCODE
Write-Host "ExitCode=$code"
Write-Host "Revisa App FileVersion en lab-pos\UI.exe y SchemaVersion en BD."
exit $code
