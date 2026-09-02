<#
.SYNOPSIS
  Instala MFFITNESS POS en esta PC (modo instalable, no portable suelto).

.DESCRIPTION
  Copia el publish (UI.exe + WhatsAppHost + migraciones) a:
    %LocalAppData%\Programs\MFFITNESS
  Crea accesos directos Escritorio/Inicio, launcher Production y entrada en
  Configuracion -> Aplicaciones -> Instaladas (desinstalar).

.EXAMPLE
  .\Scripts\Install-MFFITNESS.ps1
  .\Scripts\Install-MFFITNESS.ps1 -SourceDir .\artifacts\pos -Silent
#>
[CmdletBinding()]
param(
    [string] $SourceDir = '',

    [string] $InstallDir = '',

    [switch] $SkipPublish,

    [switch] $Silent,

    [switch] $NoDesktopShortcut,

    [string] $RepoRoot = ''
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = Split-Path -Parent $PSScriptRoot
}

if ([string]::IsNullOrWhiteSpace($InstallDir)) {
    $InstallDir = Join-Path $env:LOCALAPPDATA 'Programs\MFFITNESS'
}

if ([string]::IsNullOrWhiteSpace($SourceDir)) {
    $SourceDir = Join-Path $RepoRoot 'artifacts\pos'
}

$InstallDir = [System.IO.Path]::GetFullPath($InstallDir)
$SourceDir = [System.IO.Path]::GetFullPath($SourceDir)
$publishScript = Join-Path $RepoRoot 'Scripts\Publish-Pos.ps1'
$desk = [Environment]::GetFolderPath('Desktop')
$startMenu = Join-Path $env:APPDATA 'Microsoft\Windows\Start Menu\Programs'
$uninstallKey = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\MFFITNESS'

function Write-Step([string]$Msg, [ConsoleColor]$Color = [ConsoleColor]::Cyan) {
    if (-not $Silent) { Write-Host $Msg -ForegroundColor $Color }
}

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
    if ($IconPath -and (Test-Path $IconPath)) {
        $s.IconLocation = "$IconPath,0"
    }
    $s.Description = $Description
    $s.Save()
}

Write-Step '=== Instalador MFFITNESS POS ==='
Write-Step "Origen:  $SourceDir"
Write-Step "Destino: $InstallDir"

if (-not $SkipPublish) {
    if (-not (Test-Path $publishScript)) {
        throw 'No se encontro Publish-Pos.ps1'
    }
    Write-Step 'Publicando Release...'
    & $publishScript -Configuration Release -OutputDir $SourceDir -RepoRoot $RepoRoot
    if ($LASTEXITCODE -ne 0) { throw "Publish-Pos fallo ($LASTEXITCODE)" }
}
elseif (-not (Test-Path (Join-Path $SourceDir 'UI.exe'))) {
    throw "SkipPublish=true pero falta UI.exe en $SourceDir"
}

# Cerrar app en ejecucion para reemplazar archivos
Get-Process -Name 'UI', 'WhatsAppHost' -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 1

Write-Step 'Copiando archivos...'
if (Test-Path $InstallDir) {
    # Conservar datos de usuario fuera del arbol de la app (%LocalAppData%\MFFITNESS)
    Remove-Item -LiteralPath $InstallDir -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $InstallDir | Out-Null
Copy-Item -Path (Join-Path $SourceDir '*') -Destination $InstallDir -Recurse -Force

$exe = Join-Path $InstallDir 'UI.exe'
if (-not (Test-Path $exe)) { throw 'Instalacion incompleta: falta UI.exe' }

# Production + [MF CYBER DB]
$localSettings = Join-Path $InstallDir 'appsettings.Local.json'
@'
{
  "Database": {
    "DefaultEnvironment": "Production",
    "ConnectionStrings": {
      "Production": "Server=(localdb)\\MSSQLLocalDB;Database=MF CYBER DB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
    }
  }
}
'@ | Set-Content -Path $localSettings -Encoding UTF8

$launcher = Join-Path $InstallDir 'Start-MFFITNESS.cmd'
@'
@echo off
set MFFITNESS_ENVIRONMENT=Production
set DOTNET_ENVIRONMENT=Production
start "" /D "%~dp0" "%~dp0UI.exe"
'@ | Set-Content -Path $launcher -Encoding ASCII

# Scripts de desinstalacion dentro de la carpeta instalada
$uninstallSrc = Join-Path $SourceDir 'Uninstall-MFFITNESS.ps1'
if (-not (Test-Path $uninstallSrc)) {
    $uninstallSrc = Join-Path $RepoRoot 'Scripts\Uninstall-MFFITNESS.ps1'
}
if (Test-Path $uninstallSrc) {
    Copy-Item $uninstallSrc (Join-Path $InstallDir 'Uninstall-MFFITNESS.ps1') -Force
}

$ver = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($exe)
$iconForShortcut = if (Test-Path $exe) { $exe } else { $null }

New-Item -ItemType Directory -Force -Path $startMenu | Out-Null
$startLnk = Join-Path $startMenu 'MFFITNESS.lnk'
New-AppShortcut -LinkPath $startLnk -Target $launcher -WorkDir $InstallDir `
    -IconPath $iconForShortcut -Description 'MFFITNESS POS'

if (-not $NoDesktopShortcut) {
    $deskLnk = Join-Path $desk 'MFFITNESS.lnk'
    New-AppShortcut -LinkPath $deskLnk -Target $launcher -WorkDir $InstallDir `
        -IconPath $iconForShortcut -Description 'MFFITNESS POS'
    Write-Step "Acceso directo Escritorio: $deskLnk"
}

# Registro Windows -> Agregar o quitar programas
$uninstallCmd = "powershell.exe -NoProfile -ExecutionPolicy Bypass -File `"$(Join-Path $InstallDir 'Uninstall-MFFITNESS.ps1')`""
New-Item -Path $uninstallKey -Force | Out-Null
Set-ItemProperty -Path $uninstallKey -Name 'DisplayName' -Value 'MFFITNESS POS'
Set-ItemProperty -Path $uninstallKey -Name 'DisplayVersion' -Value $ver.ProductVersion
Set-ItemProperty -Path $uninstallKey -Name 'Publisher' -Value 'MFFITNESS'
Set-ItemProperty -Path $uninstallKey -Name 'InstallLocation' -Value $InstallDir
Set-ItemProperty -Path $uninstallKey -Name 'UninstallString' -Value $uninstallCmd
Set-ItemProperty -Path $uninstallKey -Name 'DisplayIcon' -Value $exe
Set-ItemProperty -Path $uninstallKey -Name 'NoModify' -Value 1 -Type DWord
Set-ItemProperty -Path $uninstallKey -Name 'NoRepair' -Value 1 -Type DWord

Write-Step ''
Write-Step 'Instalacion completada.' -Color Green
Write-Step "  App:     $exe"
Write-Step "  Version: $($ver.ProductVersion)"
if (Test-Path (Join-Path $InstallDir 'WhatsAppHost\WhatsAppHost.exe')) {
    Write-Step '  WhatsAppHost: incluido'
} else {
    Write-Host '  AVISO: WhatsAppHost no incluido - reinstale con Publish-Pos completo.' -ForegroundColor Yellow
}
Write-Step '  Desinstalar: Configuracion -> Aplicaciones -> MFFITNESS POS'
Write-Step "  O ejecute: $(Join-Path $InstallDir 'Uninstall-MFFITNESS.ps1')"

if (-not $Silent) {
    $launch = Read-Host 'Abrir MFFITNESS ahora? (S/N)'
    if ($launch -match '^[sSyY]') {
        Start-Process -FilePath $launcher -WorkingDirectory $InstallDir
    }
}
