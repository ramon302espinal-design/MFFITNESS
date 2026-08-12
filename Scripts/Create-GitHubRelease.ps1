<#
.SYNOPSIS
  Crea un GitHub Release con ZIP + manifest.json (assets de update).

.DESCRIPTION
  Por defecto hace dry-run. Usa -Push para crear el release de verdad.

.EXAMPLE
  .\Scripts\Create-GitHubRelease.ps1
  .\Scripts\Create-GitHubRelease.ps1 -Push
#>
[CmdletBinding()]
param(
    [string] $PackageDir = '',
    [string] $Tag = '',
    [string] $Title = '',
    [switch] $Push,
    [switch] $Draft
)

$ErrorActionPreference = 'Stop'
$RepoRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($PackageDir)) {
    $PackageDir = Join-Path $RepoRoot 'artifacts\update-package'
}

$manifestPath = Join-Path $PackageDir 'manifest.json'
if (!(Test-Path $manifestPath)) { throw "Falta manifest.json en $PackageDir" }

$manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
$zipPath = Join-Path $PackageDir $manifest.packageName
if (!(Test-Path $zipPath)) { throw "Falta ZIP $($manifest.packageName)" }

if ([string]::IsNullOrWhiteSpace($Tag)) { $Tag = "v$($manifest.appVersion)" }
if ([string]::IsNullOrWhiteSpace($Title)) { $Title = "MFFITNESS $($manifest.appVersion)" }

$notes = @"
## MFFITNESS POS $($manifest.appVersion)

- TargetDbVersion: $($manifest.targetDbVersion)
- MinAppVersion: $($manifest.minAppVersion)
- Package: $($manifest.packageName)
- SHA256: $($manifest.packageSha256)

### Assets requeridos
- ``$($manifest.packageName)``
- ``manifest.json``

### Notas
- ``UpdateManager.exe`` NO va en el ZIP (se despliega solo en install inicial).
- Actualización vía UI → Buscar actualizaciones, o ``UpdateManager.exe --request``.
"@

Write-Host "=== Create-GitHubRelease ===" -ForegroundColor Cyan
Write-Host "Tag:   $Tag"
Write-Host "Title: $Title"
Write-Host "ZIP:   $zipPath"
Write-Host "Manifest: $manifestPath"
Write-Host "Draft: $Draft"
Write-Host "Push:  $Push"

if (-not $Push) {
    Write-Host ""
    Write-Host "Dry-run OK. Para publicar:" -ForegroundColor Yellow
    Write-Host "  .\Scripts\Create-GitHubRelease.ps1 -Push"
    exit 0
}

gh auth status | Out-Host
$ghArgs = @('release', 'create', $Tag, $zipPath, $manifestPath, '--title', $Title, '--notes', $notes)
if ($Draft) { $ghArgs += '--draft' }

& gh @ghArgs
if ($LASTEXITCODE -ne 0) { throw "gh release create falló ($LASTEXITCODE)" }
Write-Host "Release creado: $Tag" -ForegroundColor Green
