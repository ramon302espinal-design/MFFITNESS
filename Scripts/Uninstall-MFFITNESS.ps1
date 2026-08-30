<#
.SYNOPSIS
  Desinstala MFFITNESS POS de esta PC.
#>
[CmdletBinding()]
param(
    [string] $InstallDir = '',

    [switch] $Silent
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($InstallDir)) {
    $InstallDir = Join-Path $env:LOCALAPPDATA 'Programs\MFFITNESS'
}

$InstallDir = [System.IO.Path]::GetFullPath($InstallDir)
$desk = [Environment]::GetFolderPath('Desktop')
$startMenu = Join-Path $env:APPDATA 'Microsoft\Windows\Start Menu\Programs'
$uninstallKey = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\MFFITNESS'

if (-not $Silent) {
    Write-Host "Desinstalando MFFITNESS desde: $InstallDir" -ForegroundColor Yellow
    $confirm = Read-Host "¿Continuar? (S/N)"
    if ($confirm -notmatch '^[sSyY]') { return }
}

Get-Process -Name 'UI', 'WhatsAppHost', 'ngrok' -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 1

foreach ($lnk in @(
        (Join-Path $desk 'MFFITNESS.lnk'),
        (Join-Path $startMenu 'MFFITNESS.lnk')
    )) {
    if (Test-Path $lnk) { Remove-Item -LiteralPath $lnk -Force }
}

if (Test-Path $InstallDir) {
    Remove-Item -LiteralPath $InstallDir -Recurse -Force
}

if (Test-Path $uninstallKey) {
    Remove-Item -LiteralPath $uninstallKey -Force
}

# Datos de usuario (%LocalAppData%\MFFITNESS) se conservan: facturas, secretos, logs.
if (-not $Silent) {
    Write-Host ""
    Write-Host "MFFITNESS desinstalado." -ForegroundColor Green
    Write-Host "Datos conservados en: $(Join-Path $env:LOCALAPPDATA 'MFFITNESS')" -ForegroundColor DarkGray
    Write-Host "(twilio.secrets.config, facturas, whatsapp.stack.config)"
}
