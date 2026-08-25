<#
.SYNOPSIS
  Despliegue recomendado del POS en esta PC (producción local).

.DESCRIPTION
  1) Publish-Pos → %LocalAppData%\Programs\MFFITNESS
     (fuera de OneDrive: seguro para OTA / file replace)
  2) appsettings.Local.json + Start-MFFITNESS.cmd → [MF CYBER DB]
  3) Acceso directo Escritorio/Inicio → Start-MFFITNESS.cmd
  4) Opcional: limpia carpeta antigua en Escritorio\MFFITNESS (anti-patrón)

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

    # Elimina Escritorio\MFFITNESS si existe (deja solo el .lnk)
    [switch] $CleanDesktopFolder,

    [string] $RepoRoot = ''
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = Split-Path -Parent $PSScriptRoot
}

if ([string]::IsNullOrWhiteSpace($InstallDir)) {
    $InstallDir = Join-Path $env:LOCALAPPDATA 'Programs\MFFITNESS'
}

$InstallDir = [System.IO.Path]::GetFullPath($InstallDir)
$publishScript = Join-Path $PSScriptRoot 'Publish-Pos.ps1'
$desk = [Environment]::GetFolderPath('Desktop')
$startMenu = Join-Path $env:APPDATA 'Microsoft\Windows\Start Menu\Programs'
$icoRel = 'Resources\IMG_1722.ico'

Write-Host "=== MFFITNESS Deploy-Pos ===" -ForegroundColor Cyan
Write-Host "Install: $InstallDir"
Write-Host "Desktop: $desk"

if (-not $SkipPublish) {
    & $publishScript -Configuration $Configuration -OutputDir $InstallDir -RepoRoot $RepoRoot
    if ($LASTEXITCODE -ne 0) { throw "Publish-Pos falló ($LASTEXITCODE)" }
}
elseif (!(Test-Path (Join-Path $InstallDir 'UI.exe'))) {
    throw "SkipPublish=true pero no existe UI.exe en $InstallDir"
}

$exe = Join-Path $InstallDir 'UI.exe'
$ico = Join-Path $InstallDir $icoRel
if (!(Test-Path $exe)) { throw "Falta UI.exe tras publish" }

# Garantiza Production en el layout instalado (también con -SkipPublish).
$localSettings = Join-Path $InstallDir 'appsettings.Local.json'
@'
{
  "Database": {
    "DefaultEnvironment": "Production"
  }
}
'@ | Set-Content -Path $localSettings -Encoding UTF8
Write-Host "BD instalada: [MF CYBER DB] (appsettings.Local.json → Production)" -ForegroundColor Green

# Launcher: fuerza env Production (funciona aunque falte Local / AppConfig antiguo).
$launcher = Join-Path $InstallDir 'Start-MFFITNESS.cmd'
@'
@echo off
set MFFITNESS_ENVIRONMENT=Production
set DOTNET_ENVIRONMENT=Production
start "" /D "%~dp0" "%~dp0UI.exe"
'@ | Set-Content -Path $launcher -Encoding ASCII
Write-Host "Launcher: $launcher" -ForegroundColor Green

function New-AppShortcut {
    param(
        [string] $LinkPath,
        [string] $Target,
        [string] $WorkDir,
        [string] $IconPath,
        [string] $Description
    )
    $w = New-Object -ComObject WScript.Shell
    $s = $w.CreateShortcut($LinkPath)
    $s.TargetPath = $Target
    $s.WorkingDirectory = $WorkDir
    if (Test-Path $IconPath) {
        $s.IconLocation = "$IconPath,0"
    }
    $s.Description = $Description
    $s.Save()
}

# Escritorio / Inicio → launcher (Production), no UI.exe directo.
$deskLnk = Join-Path $desk 'MFFITNESS.lnk'
New-AppShortcut -LinkPath $deskLnk -Target $launcher -WorkDir $InstallDir -IconPath $ico -Description 'MFFITNESS POS ([MF CYBER DB])'
Write-Host "Acceso directo Escritorio: $deskLnk" -ForegroundColor Green

New-Item -ItemType Directory -Force -Path $startMenu | Out-Null
$startLnk = Join-Path $startMenu 'MFFITNESS.lnk'
New-AppShortcut -LinkPath $startLnk -Target $launcher -WorkDir $InstallDir -IconPath $ico -Description 'MFFITNESS POS ([MF CYBER DB])'
Write-Host "Acceso directo Inicio:     $startLnk" -ForegroundColor Green

# Limpiar anti-patrón: app completa en Escritorio/OneDrive
$legacyDesktopApp = Join-Path $desk 'MFFITNESS'
if (Test-Path $legacyDesktopApp) {
    if ($CleanDesktopFolder) {
        Write-Host "Eliminando carpeta legacy en Escritorio: $legacyDesktopApp" -ForegroundColor Yellow
        Remove-Item -LiteralPath $legacyDesktopApp -Recurse -Force
    }
    else {
        Write-Host ""
        Write-Host "AVISO: existe carpeta completa en Escritorio (OneDrive):" -ForegroundColor Yellow
        Write-Host "  $legacyDesktopApp" -ForegroundColor Yellow
        Write-Host "  Re-ejecuta con -CleanDesktopFolder para borrarla (deja el .lnk)." -ForegroundColor Yellow
    }
}

$ver = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($exe)
Write-Host ""
Write-Host "Deploy OK" -ForegroundColor Green
Write-Host "  App:     $exe"
Write-Host "  Versión: $($ver.ProductVersion)"
Write-Host "  Producto:$($ver.FileDescription)"
Write-Host "  Abre desde el acceso directo MFFITNESS del Escritorio."
