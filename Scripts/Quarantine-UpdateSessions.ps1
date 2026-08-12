<#
.SYNOPSIS
  Archiva sesiones de update terminales seguras (lab / higiene local).

.DESCRIPTION
  Mueve Completed, Blocked, Failed y FailedRecovered a
  %LocalAppData%\MFFITNESS\updates\sessions\_quarantine\<timestamp>\

  NUNCA mueve FailedRecoveryRequired ni RecoveryRequired (bloqueo intencional de UI).

.EXAMPLE
  .\Scripts\Quarantine-UpdateSessions.ps1
  .\Scripts\Quarantine-UpdateSessions.ps1 -WhatIf
#>
[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [string] $SessionsDir = ''
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($SessionsDir)) {
    $SessionsDir = Join-Path $env:LOCALAPPDATA 'MFFITNESS\updates\sessions'
}

if (!(Test-Path $SessionsDir)) {
    Write-Host "No hay carpeta de sesiones: $SessionsDir"
    exit 0
}

$safe = @('completed', 'blocked', 'failed', 'failedrecovered')
$stamp = Get-Date -Format 'yyyyMMddHHmmss'
$quarantine = Join-Path $SessionsDir "_quarantine\$stamp"
$moved = 0
$kept = 0

Get-ChildItem $SessionsDir -Filter '*.json' -File | ForEach-Object {
    try {
        $j = Get-Content $_.FullName -Raw | ConvertFrom-Json
        $status = [string]$j.Status
    }
    catch {
        Write-Host "SKIP (JSON inválido): $($_.Name)" -ForegroundColor Yellow
        $kept++
        return
    }

    if ($safe -contains $status.ToLowerInvariant()) {
        if ($PSCmdlet.ShouldProcess($_.FullName, "Quarantine ($status)")) {
            if (!(Test-Path $quarantine)) {
                New-Item -ItemType Directory -Force -Path $quarantine | Out-Null
            }
            Move-Item -LiteralPath $_.FullName -Destination (Join-Path $quarantine $_.Name) -Force
            Write-Host "MOVED $($_.Name) [$status]"
            $moved++
        }
    }
    else {
        Write-Host "KEEP  $($_.Name) [$status] (no se archiva)" -ForegroundColor Cyan
        $kept++
    }
}

Write-Host ""
Write-Host "Quarantine: moved=$moved kept=$kept"
if ($moved -gt 0) { Write-Host "Destino: $quarantine" }
