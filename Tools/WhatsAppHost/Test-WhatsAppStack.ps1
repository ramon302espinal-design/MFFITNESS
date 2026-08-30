#Requires -Version 5.1
<#
.SYNOPSIS
  Diagnóstico del stack WhatsApp (local + público + webhook).
#>
param(
    [int]$Port = 5088
)

$ErrorActionPreference = 'Continue'
$ok = $true

function Test-Endpoint([string]$Label, [string]$Url) {
    try {
        $r = Invoke-RestMethod $Url -TimeoutSec 8
        Write-Host "[OK] $Label" -ForegroundColor Green
        Write-Host "     $Url" -ForegroundColor DarkGray
        if ($r.webhook) { Write-Host "     webhook: $($r.webhook)" -ForegroundColor DarkGray }
        return $true
    } catch {
        Write-Host "[FAIL] $Label — $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "     $Url" -ForegroundColor DarkGray
        return $false
    }
}

Write-Host ""
Write-Host "=== Test WhatsApp Stack ===" -ForegroundColor Cyan

$local = "http://127.0.0.1:$Port"
if (-not (Test-Endpoint 'Kestrel local' "$local/health")) {
    $ok = $false
    Write-Host "  -> Arranque: .\Start-WhatsAppStack.ps1 -Profile Dev" -ForegroundColor Yellow
}

$stackPath = Join-Path $env:LOCALAPPDATA 'MFFITNESS\whatsapp.stack.config'
$publicUrl = $null
if (Test-Path $stackPath) {
    [xml]$xml = Get-Content $stackPath
    $node = $xml.configuration.appSettings.add | Where-Object { $_.key -eq 'WhatsAppPublicBaseUrl' } | Select-Object -First 1
    if ($node -and $node.value) { $publicUrl = $node.value.Trim().TrimEnd('/') }
}

if ($env:WHATSAPP_PUBLIC_BASE_URL) {
    $publicUrl = $env:WHATSAPP_PUBLIC_BASE_URL.Trim().TrimEnd('/')
}

if ([string]::IsNullOrWhiteSpace($publicUrl)) {
    Write-Host "[WARN] WhatsAppPublicBaseUrl no configurada" -ForegroundColor Yellow
    Write-Host "  Archivo: $stackPath" -ForegroundColor DarkGray
    $ok = $false
} else {
    if (-not (Test-Endpoint 'HTTPS público' "$publicUrl/health")) { $ok = $false }
    $webhook = "$publicUrl/webhook/twilio/whatsapp"
    Write-Host ""
    Write-Host "Twilio webhook (POST):" -ForegroundColor Cyan
    Write-Host "  $webhook"
}

Write-Host ""
if ($ok) { Write-Host "Stack SALUDABLE" -ForegroundColor Green }
else { Write-Host "Stack INCOMPLETO — revise config y procesos" -ForegroundColor Yellow }
Write-Host ""

exit $(if ($ok) { 0 } else { 1 })
