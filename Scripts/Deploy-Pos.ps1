<#
.SYNOPSIS
  Despliegue recomendado del POS en esta PC (instalación local).

.DESCRIPTION
  Atajo de Install-MFFITNESS.ps1 (publish Release + instalar + accesos directos).

.EXAMPLE
  .\Scripts\Deploy-Pos.ps1
  .\Scripts\Deploy-Pos.ps1 -CleanDesktopFolder
#>
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',

    [string] $InstallDir = '',

    [switch] $SkipPublish,

    [switch] $CleanDesktopFolder,

    [string] $RepoRoot = ''
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = Split-Path -Parent $PSScriptRoot
}

$installScript = Join-Path $RepoRoot 'Scripts\Install-MFFITNESS.ps1'
if (-not (Test-Path $installScript)) {
    throw "Falta Install-MFFITNESS.ps1"
}

$sourceDir = Join-Path $RepoRoot 'artifacts\pos'

& $installScript `
    -RepoRoot $RepoRoot `
    -SourceDir $sourceDir `
    -InstallDir $(if ($InstallDir) { $InstallDir } else { Join-Path $env:LOCALAPPDATA 'Programs\MFFITNESS' }) `
    -SkipPublish:$SkipPublish `
    -Silent

if ($CleanDesktopFolder) {
    $desk = [Environment]::GetFolderPath('Desktop')
    $legacyDesktopApp = Join-Path $desk 'MFFITNESS'
    if (Test-Path $legacyDesktopApp) {
        Write-Host "Eliminando carpeta portable legacy: $legacyDesktopApp" -ForegroundColor Yellow
        Remove-Item -LiteralPath $legacyDesktopApp -Recurse -Force
    }
}

Write-Host ""
Write-Host "Deploy-Pos completado (modo instalable)." -ForegroundColor Green
