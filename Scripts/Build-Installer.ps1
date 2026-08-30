<#
.SYNOPSIS
  Genera el instalador MFFITNESS-Setup.exe (Inno Setup) o paquete Instalar.cmd.

.EXAMPLE
  .\Scripts\Build-Installer.ps1
  .\Scripts\Build-Installer.ps1 -SkipInno
#>
[CmdletBinding()]
param(
    [string] $RepoRoot = '',

    [switch] $SkipPublish,

    [switch] $SkipInno
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = Split-Path -Parent $PSScriptRoot
}

$publishDir = Join-Path $RepoRoot 'artifacts\pos'
$installerDir = Join-Path $RepoRoot 'artifacts\installer'
$issFile = Join-Path $RepoRoot 'Scripts\Installer\MFFITNESS.iss'
$publishScript = Join-Path $RepoRoot 'Scripts\Publish-Pos.ps1'

Write-Host "=== Build Installer MFFITNESS ===" -ForegroundColor Cyan

if (-not $SkipPublish) {
    & $publishScript -Configuration Release -OutputDir $publishDir -RepoRoot $RepoRoot
    if ($LASTEXITCODE -ne 0) { throw "Publish-Pos falló" }
}

$uiExe = Join-Path $publishDir 'UI.exe'
if (-not (Test-Path $uiExe)) { throw "Falta $uiExe. Ejecute Publish-Pos primero." }

$ver = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($uiExe)
$appVersion = if ($ver.ProductVersion) { $ver.ProductVersion.Split('+')[0] } else { '1.0.0' }

New-Item -ItemType Directory -Force -Path $installerDir | Out-Null

# Paquete alternativo (sin Inno Setup)
$packageDir = Join-Path $installerDir 'package'
if (Test-Path $packageDir) { Remove-Item -Recurse -Force $packageDir }
Copy-Item -Path $publishDir -Destination $packageDir -Recurse
Copy-Item (Join-Path $RepoRoot 'Scripts\Install-MFFITNESS.ps1') $packageDir -Force
Copy-Item (Join-Path $RepoRoot 'Scripts\Uninstall-MFFITNESS.ps1') $packageDir -Force

$instalarCmd = Join-Path $packageDir 'Instalar-MFFITNESS.cmd'
@'
@echo off
title Instalar MFFITNESS POS
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Install-MFFITNESS.ps1" -SourceDir "%~dp0" -SkipPublish -RepoRoot "%~dp0"
pause
'@ | Set-Content -Path $instalarCmd -Encoding ASCII

Write-Host "Paquete manual: $packageDir" -ForegroundColor Green
Write-Host "  Ejecute: Instalar-MFFITNESS.cmd" -ForegroundColor Green

if ($SkipInno) { return }

$isccCandidates = @(
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "$env:ProgramFiles\Inno Setup 6\ISCC.exe",
    (Join-Path $env:LOCALAPPDATA 'Programs\Inno Setup 6\ISCC.exe'),
    (Get-Command ISCC.exe -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source)
) | Where-Object { $_ -and (Test-Path $_) }

$iscc = $isccCandidates | Select-Object -First 1
if (-not $iscc) {
    Write-Host ""
    Write-Host "Inno Setup 6 no encontrado. Solo se genero el paquete manual." -ForegroundColor Yellow
    Write-Host "Instale Inno Setup: https://jrsoftware.org/isdl.php" -ForegroundColor Yellow
    Write-Host "Luego re-ejecute: .\Scripts\Build-Installer.ps1 -SkipPublish" -ForegroundColor Yellow
    return
}

Write-Host "Compilando Setup.exe con Inno Setup..." -ForegroundColor Cyan
& $iscc "/DAppVersion=$appVersion" $issFile
if ($LASTEXITCODE -ne 0) { throw "ISCC falló ($LASTEXITCODE)" }

$setupExe = Get-ChildItem $installerDir -Filter 'MFFITNESS-Setup-*.exe' | Sort-Object LastWriteTime -Descending | Select-Object -First 1
Write-Host ""
Write-Host "Instalador listo:" -ForegroundColor Green
Write-Host "  $($setupExe.FullName)" -ForegroundColor Green
Write-Host "  Versión: $appVersion"
Write-Host ""
Write-Host "En la PC del gym: doble clic en MFFITNESS-Setup-*.exe"
