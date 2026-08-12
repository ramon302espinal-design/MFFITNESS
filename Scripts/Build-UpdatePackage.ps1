<#
.SYNOPSIS
  Publica el POS y genera el paquete de actualización (ZIP + manifest.json).

.DESCRIPTION
  1) Publish-Pos.ps1 → artifacts/pos (incluye UpdateManager.exe)
  2) UpdatePackageBuilder → artifacts/update-package/
       MFFITNESS-{version}.zip   (SIN UpdateManager.exe)
       manifest.json
       package-files.txt

.EXAMPLE
  .\Scripts\Build-UpdatePackage.ps1
  .\Scripts\Build-UpdatePackage.ps1 -AppVersion 1.1.0 -MinAppVersion 1.0.0 -TargetDbVersion 4
#>
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',

    [string] $AppVersion = '',
    [string] $MinAppVersion = '1.0.0',
    [int] $TargetDbVersion = 0,
    [string] $ReleaseNotesUrl = '',
    [switch] $SkipPublish,
    [switch] $IncludePdbs
)

$ErrorActionPreference = 'Stop'
$RepoRoot = Split-Path -Parent $PSScriptRoot
$publishDir = Join-Path $RepoRoot 'artifacts\pos'
$packageDir = Join-Path $RepoRoot 'artifacts\update-package'
$builderProj = Join-Path $RepoRoot 'Tools\UpdatePackageBuilder\UpdatePackageBuilder.csproj'
$publishScript = Join-Path $PSScriptRoot 'Publish-Pos.ps1'

Write-Host "=== MFFITNESS Build-UpdatePackage ===" -ForegroundColor Cyan

if (-not $SkipPublish) {
    & $publishScript -Configuration $Configuration -OutputDir $publishDir -RepoRoot $RepoRoot
    if ($LASTEXITCODE -ne 0) { throw "Publish-Pos falló" }
}
elseif (!(Test-Path (Join-Path $publishDir 'UI.exe'))) {
    throw "SkipPublish=true pero no existe publish en $publishDir. Ejecuta Publish-Pos.ps1 primero."
}

$builderArgs = @(
    'run', '--project', $builderProj, '-c', $Configuration, '--',
    '--source', $publishDir,
    '--out', $packageDir,
    '--min-app-version', $MinAppVersion
)

if (![string]::IsNullOrWhiteSpace($AppVersion)) {
    $builderArgs += @('--app-version', $AppVersion)
}
if ($TargetDbVersion -gt 0) {
    $builderArgs += @('--target-db-version', "$TargetDbVersion")
}
if (![string]::IsNullOrWhiteSpace($ReleaseNotesUrl)) {
    $builderArgs += @('--release-notes-url', $ReleaseNotesUrl)
}
if ($IncludePdbs) {
    $builderArgs += '--include-pdbs'
}

Write-Host "Generando ZIP + manifest..."
dotnet @builderArgs
if ($LASTEXITCODE -ne 0) { throw "UpdatePackageBuilder falló ($LASTEXITCODE)" }

Write-Host ""
Write-Host "Paquete listo en: $packageDir" -ForegroundColor Green
Write-Host "  - Sube ZIP + manifest.json como assets de GitHub Release"
Write-Host "  - NO subas UpdateManager.exe dentro del ZIP (ya excluido)"
Write-Host "  - El despliegue inicial SÍ debe copiar toda la carpeta UpdateManager\ (junto al layout de UI)"
