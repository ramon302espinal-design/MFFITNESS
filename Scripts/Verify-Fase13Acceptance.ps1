<#
.SYNOPSIS
  Fase 13 - verificacion final sinergia 100% en DEV y PROD.

.EXAMPLE
  .\Scripts\Verify-Fase13Acceptance.ps1
#>
[CmdletBinding()]
param(
    [string] $ServerInstance = '(localdb)\MSSQLLocalDB',
    [string] $DevDatabase = 'MF_CYBER_DB_DEV',
    [string] $ProdDatabase = 'MF CYBER DB'
)

$ErrorActionPreference = 'Stop'
$RepoRoot = Split-Path -Parent $PSScriptRoot
$Sql = Join-Path $RepoRoot 'Scripts\Fase13-AceptacionFinal.sql'

function Invoke-F13 {
    param([string] $Database)
    Write-Host ""
    Write-Host "========== $Database ==========" -ForegroundColor Cyan
    if (!(Test-Path $Sql)) { throw "No existe $Sql" }
    sqlcmd -S $ServerInstance -d $Database -E -i $Sql -W
    if ($LASTEXITCODE -ne 0) { throw "F13 fallo en $Database" }
}

Write-Host '=== MFFITNESS Fase 13 - Aceptacion Final ===' -ForegroundColor Cyan

Invoke-F13 -Database $DevDatabase
Invoke-F13 -Database $ProdDatabase

Write-Host ""
Write-Host '--- Checklist manual (operador) ---' -ForegroundColor Yellow
Write-Host '13.1 UI: Venta contado -> verificar Home ingresos, Caja, Historial ventas, Reportes mismo dia'
Write-Host '13.2 UI: Venta financiada con pago inicial -> Deudas, Historial deudas, Caja, Historial ventas, Home'
Write-Host '13.3 UI: Membresia financiada -> mismas 5 pantallas + Estado clientes'
Write-Host '13.4 UI: Abonar deuda -> saldo baja en Deudas, Historial, Caja, Home'
Write-Host '13.5 UI: Editar pago inicial -> historial + caja + grids alineados'
Write-Host '13.6 UI: Revertir pago -> todo coherente'
Write-Host '13.7 UI: Export PDF historial deudas = grid en pantalla'
Write-Host '13.8 UI: CRM reportes POS tras cobro sin cambiar periodo'
Write-Host '13.10 UI: Atajos P/C/E/D/H/R/I/M + buscadores sin pantallas stale'
Write-Host ""
Write-Host 'Fase 13: revision AUTO completada. Complete items MANUAL para sinergia 100%.' -ForegroundColor Green
