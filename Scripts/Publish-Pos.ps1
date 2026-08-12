<#
.SYNOPSIS
  Publica el POS (UI) + UpdateManager.exe en un layout de instalación unificado.

.DESCRIPTION
  Salida por defecto: artifacts/pos/
    UI.exe
    UpdateManager\UpdateManager.exe   (runtime aislado; NO en la raíz)
    *.dll
    Database/Migrations/*.sql
    Resources/...

  UpdateManager vive en subcarpeta propia para no bloquear DLLs del install
  durante updates. Nunca va dentro del ZIP de update.

.EXAMPLE
  .\Scripts\Publish-Pos.ps1
  .\Scripts\Publish-Pos.ps1 -Configuration Release -OutputDir D:\Deploy\MFFITNESS
#>
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',

    [string] $OutputDir = '',

    [string] $RepoRoot = ''
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = Split-Path -Parent $PSScriptRoot
}

if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot 'artifacts\pos'
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

Write-Host "Publicando UI..."
dotnet publish $uiProj -c $Configuration -o $OutputDir --nologo
if ($LASTEXITCODE -ne 0) { throw "dotnet publish UI falló ($LASTEXITCODE)" }

# UpdateManager en SUBCARPETA propia para no bloquear DLLs del install al actualizar.
$umOut = Join-Path $OutputDir 'UpdateManager'
if (Test-Path $umOut) { Remove-Item -Recurse -Force $umOut }
New-Item -ItemType Directory -Path $umOut | Out-Null

Write-Host "Publicando UpdateManager → UpdateManager\ (runtime aislado)..."
dotnet publish $umProj -c $Configuration -o $umOut --nologo
if ($LASTEXITCODE -ne 0) { throw "dotnet publish UpdateManager falló ($LASTEXITCODE)" }

# Garantizar migraciones (UI.csproj ya copia, reforzamos)
$migOut = Join-Path $OutputDir 'Database\Migrations'
New-Item -ItemType Directory -Force -Path $migOut | Out-Null
if (Test-Path $migrations) {
    Copy-Item (Join-Path $migrations '*.sql') $migOut -Force
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

# No dejar UpdateManager.exe suelto en la raíz del install (evita locks).
$rootUm = Join-Path $OutputDir 'UpdateManager.exe'
if (Test-Path $rootUm) { Remove-Item -Force $rootUm }

$migCount = @(Get-ChildItem $migOut -Filter '*.sql' -ErrorAction SilentlyContinue).Count
Write-Host ""
Write-Host "Publish OK" -ForegroundColor Green
Write-Host "  UI.exe en:              $OutputDir"
Write-Host "  UpdateManager.exe en:   $umOut"
Write-Host "  Migraciones SQL: $migCount"
Write-Host "  Siguiente: .\Scripts\Build-UpdatePackage.ps1"
