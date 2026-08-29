<#
.SYNOPSIS
  Fase 12 - verifica paridad DEV vs PROD (SchemaVersion + integridad financiera).

.EXAMPLE
  .\Scripts\Verify-DevProdParity.ps1
#>
[CmdletBinding()]
param(
    [string] $ServerInstance = '(localdb)\MSSQLLocalDB',
    [string] $DevDatabase = 'MF_CYBER_DB_DEV',
    [string] $ProdDatabase = 'MF CYBER DB'
)

$ErrorActionPreference = 'Stop'
$RepoRoot = Split-Path -Parent $PSScriptRoot
$GateSql = Join-Path $RepoRoot 'Scripts\Fase12-PreRelease-Gate.sql'

function Invoke-Gate {
    param([string] $Database)
    Write-Host "=== $Database ===" -ForegroundColor Cyan
    if (!(Test-Path $GateSql)) { throw "No existe $GateSql" }
    sqlcmd -S $ServerInstance -d $Database -E -i $GateSql -W
    if ($LASTEXITCODE -ne 0) { throw "Gate fallo en $Database" }
}

Write-Host '=== MFFITNESS Verify-DevProdParity (Fase 12) ===' -ForegroundColor Cyan

$devVer = sqlcmd -S $ServerInstance -d $DevDatabase -E -h -1 -Q "SET NOCOUNT ON; SELECT ISNULL(MAX(Version),0) FROM SchemaVersion"
$prodVer = sqlcmd -S $ServerInstance -d $ProdDatabase -E -h -1 -Q "SET NOCOUNT ON; SELECT ISNULL(MAX(Version),0) FROM SchemaVersion"
$devVer = ($devVer | Where-Object { $_ -match '^\s*\d+\s*$' } | Select-Object -First 1).Trim()
$prodVer = ($prodVer | Where-Object { $_ -match '^\s*\d+\s*$' } | Select-Object -First 1).Trim()

Write-Host "SchemaVersion DEV=$devVer  PROD=$prodVer"
if ($devVer -ne $prodVer) {
    Write-Host 'DESALINEADO: migraciones pendientes en una de las bases.' -ForegroundColor Red
    exit 1
}

Invoke-Gate -Database $DevDatabase
Invoke-Gate -Database $ProdDatabase

$uiExe = Join-Path $RepoRoot 'artifacts\pos\UI.exe'
if (Test-Path $uiExe) {
    $ver = (Get-Item $uiExe).VersionInfo.ProductVersion
    Write-Host "Publish POS: UI.exe $ver" -ForegroundColor Green
}
else {
    Write-Host 'Publish POS: no hay artifacts\pos - ejecute Publish-Pos.ps1 antes del release' -ForegroundColor Yellow
}

Write-Host ''
Write-Host "Paridad F12: OK SchemaVersion=$devVer gates PASS en ambas BD" -ForegroundColor Green
