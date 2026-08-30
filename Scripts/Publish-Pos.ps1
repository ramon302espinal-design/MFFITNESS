<#
.SYNOPSIS
  Publica el POS (UI) + UpdateManager en un layout de instalación unificado.

.DESCRIPTION
  Salida por defecto: artifacts/pos/
    UI.exe
    UpdateManager\UpdateManager.exe   (runtime aislado)
    *.dll (sin .pdb en Release)
    Database/Migrations/*.sql
    Resources/IMG_1722.ico

  Para instalación en esta PC usa Deploy-Pos.ps1 (LocalAppData + acceso directo).

.EXAMPLE
  .\Scripts\Publish-Pos.ps1
  .\Scripts\Publish-Pos.ps1 -Configuration Release -OutputDir D:\Deploy\MFFITNESS
#>
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',

    [string] $OutputDir = '',

    [string] $RepoRoot = '',

    # Incluir símbolos .pdb (solo diagnóstico). Por defecto NO en Release.
    [switch] $IncludePdbs
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = Split-Path -Parent $PSScriptRoot
}

if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot 'artifacts\pos'
}

$OutputDir = [System.IO.Path]::GetFullPath($OutputDir)

# OneDrive / Escritorio: malo para OTA (file locks + sync). Avisar fuerte.
$normalized = $OutputDir.Replace('/', '\')
if ($normalized -match '\\OneDrive\\' -or $normalized -match '\\Escritorio\\' -or $normalized -match '\\Desktop\\') {
    Write-Host ""
    Write-Host "ADVERTENCIA: destino bajo OneDrive/Escritorio." -ForegroundColor Yellow
    Write-Host "  Las actualizaciones OTA pueden fallar por bloqueo de archivos en sync." -ForegroundColor Yellow
    Write-Host "  Preferir: .\Scripts\Deploy-Pos.ps1 -> %LocalAppData%\Programs\MFFITNESS" -ForegroundColor Yellow
    Write-Host ""
}

$uiProj = Join-Path $RepoRoot 'UI\UI.csproj'
$umProj = Join-Path $RepoRoot 'Update\UpdateManager\UpdateManager.csproj'
$migrations = Join-Path $RepoRoot 'Database\Migrations'

if (!(Test-Path $uiProj)) { throw "No se encontró UI.csproj en $uiProj" }
if (!(Test-Path $umProj)) { throw "No se encontró UpdateManager.csproj en $umProj" }

Write-Host "=== MFFITNESS Publish-Pos ===" -ForegroundColor Cyan
Write-Host "Repo:    $RepoRoot"
Write-Host "Config:  $Configuration"
Write-Host "Output:  $OutputDir"

if (Test-Path $OutputDir) {
    Write-Host "Limpiando output previo..."
    Remove-Item -Recurse -Force $OutputDir
}
New-Item -ItemType Directory -Path $OutputDir | Out-Null

$publishArgs = @('-c', $Configuration, '-o', $OutputDir, '--nologo')
if ($Configuration -eq 'Release' -and -not $IncludePdbs) {
    $publishArgs += @('-p:DebugType=None', '-p:DebugSymbols=false')
}

Write-Host "Publicando UI..."
dotnet publish $uiProj @publishArgs
if ($LASTEXITCODE -ne 0) { throw "dotnet publish UI falló ($LASTEXITCODE)" }

$umOut = Join-Path $OutputDir 'UpdateManager'
if (Test-Path $umOut) { Remove-Item -Recurse -Force $umOut }
New-Item -ItemType Directory -Path $umOut | Out-Null

$umArgs = @('-c', $Configuration, '-o', $umOut, '--nologo')
if ($Configuration -eq 'Release' -and -not $IncludePdbs) {
    $umArgs += @('-p:DebugType=None', '-p:DebugSymbols=false')
}

Write-Host "Publicando UpdateManager -> UpdateManager\ (runtime aislado)..."
dotnet publish $umProj @umArgs
if ($LASTEXITCODE -ne 0) { throw "dotnet publish UpdateManager falló ($LASTEXITCODE)" }

$whProj = Join-Path $RepoRoot 'Tools\WhatsAppHost\WhatsAppHost.csproj'
$whOut = Join-Path $OutputDir 'WhatsAppHost'
if (Test-Path $whProj) {
    Write-Host "Publicando WhatsAppHost -> WhatsAppHost\ ..."
    $whArgs = @('-c', $Configuration, '-o', $whOut, '--nologo')
    if ($Configuration -eq 'Release' -and -not $IncludePdbs) {
        $whArgs += @('-p:DebugType=None', '-p:DebugSymbols=false')
    }
    dotnet publish $whProj @whArgs
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish WhatsAppHost falló ($LASTEXITCODE)" }
    $stackExample = Join-Path $RepoRoot 'whatsapp.stack.config.example'
    if (Test-Path $stackExample) {
        Copy-Item $stackExample (Join-Path $OutputDir 'whatsapp.stack.config.example') -Force
    }
}

# ngrok portable embebido (Tools\Bundled\ngrok\ngrok.exe)
$bundledNgrok = Join-Path $RepoRoot 'Tools\Bundled\ngrok\ngrok.exe'
$ngrokOut = Join-Path $OutputDir 'Tools\ngrok'
if (Test-Path $bundledNgrok) {
    New-Item -ItemType Directory -Force -Path $ngrokOut | Out-Null
    Copy-Item $bundledNgrok (Join-Path $ngrokOut 'ngrok.exe') -Force
    Write-Host "ngrok embebido -> Tools\ngrok\ngrok.exe" -ForegroundColor Green
}
else {
    Write-Host "AVISO: no hay Tools\Bundled\ngrok\ngrok.exe. El instalador usara ngrok del sistema." -ForegroundColor Yellow
}

$migOut = Join-Path $OutputDir 'Database\Migrations'
New-Item -ItemType Directory -Force -Path $migOut | Out-Null
if (Test-Path $migrations) {
    Copy-Item (Join-Path $migrations '*.sql') $migOut -Force
}

# Higiene: quitar PDB residuales / XML de doc / .deps no necesarios no se tocan.
if ($Configuration -eq 'Release' -and -not $IncludePdbs) {
    Get-ChildItem $OutputDir -Recurse -Include '*.pdb' -File -ErrorAction SilentlyContinue |
        Remove-Item -Force -ErrorAction SilentlyContinue
}

$required = @(
    (Join-Path $OutputDir 'UI.exe'),
    (Join-Path $umOut 'UpdateManager.exe'),
    (Join-Path $OutputDir 'BLL.dll'),
    (Join-Path $OutputDir 'DL.dll'),
    (Join-Path $OutputDir 'DTO.dll'),
    (Join-Path $OutputDir 'CORE.dll')
)
foreach ($path in $required) {
    if (!(Test-Path $path)) {
        throw "Falta archivo requerido tras publish: $path"
    }
}

$icoOut = Join-Path $OutputDir 'Resources\IMG_1722.ico'
if (!(Test-Path $icoOut)) {
    foreach ($candidate in @(
            (Join-Path $RepoRoot 'UI\Resources\IMG_1722.ico'),
            (Join-Path $RepoRoot 'IMG_1722.ico')
        )) {
        if (Test-Path $candidate) {
            New-Item -ItemType Directory -Force -Path (Split-Path $icoOut) | Out-Null
            Copy-Item $candidate $icoOut -Force
            Write-Host "Icono copiado a Resources\IMG_1722.ico" -ForegroundColor Yellow
            break
        }
    }
    if (!(Test-Path $icoOut)) {
        Write-Host "ADVERTENCIA: no se encontró IMG_1722.ico." -ForegroundColor Yellow
    }
}

$rootUm = Join-Path $OutputDir 'UpdateManager.exe'
if (Test-Path $rootUm) { Remove-Item -Force $rootUm }

# POS instalado / Release: siempre [MF CYBER DB] via appsettings.Local.json.
# Desarrollo en VS sigue con perfil "UI (Development)" (MFFITNESS_ENVIRONMENT=Development).
$localSettings = Join-Path $OutputDir 'appsettings.Local.json'
if ($Configuration -eq 'Release') {
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
    Write-Host "appsettings.Local.json -> Production + [MF CYBER DB]" -ForegroundColor Green

    $launcher = Join-Path $OutputDir 'Start-MFFITNESS.cmd'
    @'
@echo off
set MFFITNESS_ENVIRONMENT=Production
set DOTNET_ENVIRONMENT=Production
start "" /D "%~dp0" "%~dp0UI.exe"
'@ | Set-Content -Path $launcher -Encoding ASCII
    Write-Host "Start-MFFITNESS.cmd -> fuerza Production" -ForegroundColor Green
}
elseif (Test-Path $localSettings) {
    Remove-Item -Force $localSettings
}

$migCount = @(Get-ChildItem $migOut -Filter '*.sql' -ErrorAction SilentlyContinue).Count
$totalMb = [math]::Round(((Get-ChildItem $OutputDir -Recurse -File | Measure-Object -Sum Length).Sum / 1MB), 1)

Write-Host ""
Write-Host "Publish OK" -ForegroundColor Green
Write-Host "  UI.exe en:              $OutputDir"
Write-Host "  UpdateManager.exe en:   $umOut"
Write-Host "  Icono:                  $(if (Test-Path $icoOut) { $icoOut } else { 'NO' })"
Write-Host "  Migraciones SQL: $migCount"
Write-Host "  Tamano: ${totalMb} MB"
Write-Host "  Despliegue en PC: .\Scripts\Deploy-Pos.ps1"
