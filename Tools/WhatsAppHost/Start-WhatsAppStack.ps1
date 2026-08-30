#Requires -Version 5.1
<#
.SYNOPSIS
  Arranca WhatsAppHost (Kestrel :5088) + túnel HTTPS para desarrollo.

.EXAMPLE
  # Dev: host + ngrok efímero, actualiza configs automáticamente
  .\Start-WhatsAppStack.ps1 -Profile Dev

.EXAMPLE
  # Dev con dominio ngrok reservado (whatsapp.stack.config → NgrokDomain)
  .\Start-WhatsAppStack.ps1 -Profile Dev

.EXAMPLE
  # Solo Kestrel (URL ya fija en whatsapp.stack.config)
  .\Start-WhatsAppStack.ps1 -Profile Dev -NoTunnel
#>
param(
    [ValidateSet('Dev', 'Prod')]
    [string]$Profile = 'Dev',
    [switch]$FullHost,
    [switch]$NoTunnel,
    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'

function Get-RepoRoot {
    $here = $PSScriptRoot
    if (Test-Path (Join-Path $here 'WhatsAppHost.csproj')) { return $here }
    throw "Ejecute desde Tools\WhatsAppHost"
}

function Get-StackConfigPath {
    Join-Path $env:LOCALAPPDATA 'MFFITNESS\whatsapp.stack.config'
}

function Ensure-StackConfig {
    $path = Get-StackConfigPath
    $dir = Split-Path $path -Parent
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    if (-not (Test-Path $path)) {
        $example = Join-Path (Get-RepoRoot) '..\..\whatsapp.stack.config.example'
        $example = (Resolve-Path $example).Path
        Copy-Item $example $path
        Write-Host "Creado $path (edite NgrokDomain o WhatsAppPublicBaseUrl)" -ForegroundColor Yellow
    }
    return $path
}

function Read-StackSetting([string]$key) {
    $path = Get-StackConfigPath
    if (-not (Test-Path $path)) { return $null }
    [xml]$xml = Get-Content $path
    $node = $xml.configuration.appSettings.add | Where-Object { $_.key -eq $key } | Select-Object -First 1
    if ($node -and -not [string]::IsNullOrWhiteSpace($node.value)) { return $node.value.Trim() }
    return $null
}

function Set-StackSetting([string]$key, [string]$value) {
    $path = Get-StackConfigPath
    [xml]$xml = Get-Content $path
    $node = $xml.configuration.appSettings.add | Where-Object { $_.key -eq $key } | Select-Object -First 1
    if ($node) { $node.value = $value } else { throw "Clave $key no existe en $path" }
    $xml.Save($path)
}

function Set-AppConfigUrl([string]$configPath, [string]$url) {
    if (-not (Test-Path $configPath)) { return }
    [xml]$xml = Get-Content $configPath
    $node = $xml.configuration.appSettings.add | Where-Object { $_.key -eq 'WhatsAppPublicBaseUrl' } | Select-Object -First 1
    if ($node) {
        $node.value = $url
        $xml.Save($configPath)
        Write-Host "  App.config: $configPath" -ForegroundColor DarkGray
    }
}

function Sync-PublicUrl([string]$url) {
    $url = $url.TrimEnd('/')
    Set-StackSetting 'WhatsAppPublicBaseUrl' $url
    $root = (Get-RepoRoot).Parent.Parent.FullName
    Set-AppConfigUrl (Join-Path $root 'UI\App.config') $url
    Set-AppConfigUrl (Join-Path (Get-RepoRoot) 'App.config') $url
    $uiDll = Join-Path $root "UI\bin\Debug\net8.0-windows\UI.dll.config"
    if (Test-Path $uiDll) { Set-AppConfigUrl $uiDll $url }
    $hostDll = Join-Path (Get-RepoRoot) 'bin\Debug\net8.0\WhatsAppHost.dll.config'
    if (Test-Path $hostDll) { Set-AppConfigUrl $hostDll $url }
    return $url
}

function Stop-Port([int]$Port) {
    Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue |
        ForEach-Object { Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue }
}

function Wait-Health([string]$baseUrl, [int]$seconds = 30) {
    $deadline = (Get-Date).AddSeconds($seconds)
    while ((Get-Date) -lt $deadline) {
        try {
            $r = Invoke-RestMethod "$baseUrl/health" -TimeoutSec 3
            return $r
        } catch { Start-Sleep -Milliseconds 500 }
    }
    throw "Kestrel no respondió en $baseUrl/health"
}

function Resolve-HostExe {
    param([string]$Config = 'Debug')
    $root = Get-RepoRoot
    $exe = Join-Path $root "bin\$Config\net8.0\WhatsAppHost.exe"
    if (-not (Test-Path $exe) -and -not $SkipBuild) {
        Write-Host "Compilando WhatsAppHost ($Config)..." -ForegroundColor Cyan
        dotnet build (Join-Path $root 'WhatsAppHost.csproj') -c $Config --nologo | Out-Null
    }
    if (-not (Test-Path $exe)) { throw "No se encontró $exe" }
    return $exe
}

function Start-NgrokTunnel([int]$Port, [string]$Domain) {
    Get-Process ngrok -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 1
    if ([string]::IsNullOrWhiteSpace($Domain)) {
        Write-Host "Ngrok (URL efímera) -> puerto $Port" -ForegroundColor Cyan
        $ngrok = Start-Process -FilePath 'ngrok' -ArgumentList @('http', "$Port", '--log=stdout') -PassThru -WindowStyle Minimized
    } else {
        Write-Host "Ngrok dominio fijo: $Domain -> $Port" -ForegroundColor Cyan
        $ngrok = Start-Process -FilePath 'ngrok' -ArgumentList @('http', "--url=$Domain", "$Port", '--log=stdout') -PassThru -WindowStyle Minimized
    }
    for ($i = 0; $i -lt 45; $i++) {
        Start-Sleep -Seconds 1
        try {
            $tunnels = Invoke-RestMethod 'http://127.0.0.1:4040/api/tunnels' -TimeoutSec 2
            $https = $tunnels.tunnels | Where-Object { $_.public_url -like 'https://*' } | Select-Object -First 1
            if ($https) { return @{ Pid = $ngrok.Id; Url = $https.public_url.TrimEnd('/') } }
        } catch { }
    }
    throw 'Ngrok no entregó URL HTTPS. ¿ngrok config add-authtoken?'
}

function Start-CloudflaredQuick([int]$Port) {
    Get-Process cloudflared -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    $log = Join-Path $env:TEMP 'mff-cloudflared.log'
    if (Test-Path $log) { Remove-Item $log -Force }
    $cf = Start-Process -FilePath 'cloudflared' -ArgumentList @('tunnel', '--url', "http://127.0.0.1:$Port", '--logfile', $log) -PassThru -WindowStyle Minimized
    for ($i = 0; $i -lt 60; $i++) {
        Start-Sleep -Seconds 1
        if (Test-Path $log) {
            $line = Get-Content $log -Tail 20 -ErrorAction SilentlyContinue | Where-Object { $_ -match 'https://.*\.trycloudflare\.com' } | Select-Object -Last 1
            if ($line -match '(https://[^\s]+\.trycloudflare\.com)') {
                return @{ Pid = $cf.Id; Url = $Matches[1].TrimEnd('/') }
            }
        }
    }
    throw 'Cloudflared no entregó URL. Revise $env:TEMP\mff-cloudflared.log'
}

# --- Main ---
$repoRoot = Get-RepoRoot
$stackPath = Ensure-StackConfig
$port = [int](Read-StackSetting 'WhatsAppMediaListenPort')
if ($port -le 0) { $port = 5088 }

$fixedUrl = Read-StackSetting 'WhatsAppPublicBaseUrl'
$ngrokDomain = Read-StackSetting 'NgrokDomain'
if ($env:MFF_NGROK_DOMAIN) { $ngrokDomain = $env:MFF_NGROK_DOMAIN.Trim() }
$provider = Read-StackSetting 'TunnelProvider'
if (-not $provider) { $provider = 'Ngrok' }

$cfg = if ($Profile -eq 'Prod') { 'Release' } else { 'Debug' }
$hostExe = Resolve-HostExe -Config $cfg
$hostDir = Split-Path $hostExe -Parent
$hostArgs = if ($FullHost) { @() } else { @('--media-only') }

Write-Host ""
Write-Host "=== MFFITNESS WhatsApp Stack [$Profile] ===" -ForegroundColor Green

Stop-Port -Port $port
Get-Process -Name 'WhatsAppHost' -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 1

Write-Host "Iniciando WhatsAppHost $($hostArgs -join ' ')..." -ForegroundColor Cyan
$hostProc = Start-Process -FilePath $hostExe -ArgumentList $hostArgs -PassThru -WorkingDirectory $hostDir -WindowStyle Minimized
$localBase = "http://127.0.0.1:$port"
$health = Wait-Health $localBase
Write-Host "Kestrel OK en $localBase" -ForegroundColor Green

$publicUrl = $fixedUrl
$tunnelPid = $null

if (-not $NoTunnel -and [string]::IsNullOrWhiteSpace($publicUrl)) {
    switch -Regex ($provider) {
        '^Cloudflared$' {
            $tunnel = Start-CloudflaredQuick -Port $port
            $publicUrl = $tunnel.Url
            $tunnelPid = $tunnel.Pid
        }
        default {
            $tunnel = Start-NgrokTunnel -Port $port -Domain $ngrokDomain
            $publicUrl = $tunnel.Url
            $tunnelPid = $tunnel.Pid
        }
    }
    $publicUrl = Sync-PublicUrl $publicUrl
    Write-Host "URL pública sincronizada: $publicUrl" -ForegroundColor Green
}
elseif (-not [string]::IsNullOrWhiteSpace($publicUrl)) {
    Write-Host "URL fija (config): $publicUrl" -ForegroundColor Green
    if (-not $NoTunnel -and $provider -eq 'Ngrok' -and $ngrokDomain) {
        $tunnel = Start-NgrokTunnel -Port $port -Domain $ngrokDomain
        $tunnelPid = $tunnel.Pid
    }
}
else {
    Write-Host "Sin túnel (-NoTunnel). Solo local $localBase" -ForegroundColor Yellow
}

$webhookPath = '/webhook/twilio/whatsapp'
Write-Host ""
Write-Host "LISTO - deje estos procesos abiertos" -ForegroundColor Green
Write-Host "  Host PID    : $($hostProc.Id)"
if ($tunnelPid) { Write-Host "  Tunnel PID  : $tunnelPid" }
Write-Host "  Local       : $localBase/health"
if ($publicUrl) {
    Write-Host "  Público     : $publicUrl/health"
    Write-Host "  Twilio POST : $publicUrl$webhookPath" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Twilio Console -> WhatsApp Sender -> When a message comes in:" -ForegroundColor Cyan
    Write-Host "  $publicUrl$webhookPath"
}
Write-Host "  Stack config: $stackPath"
Write-Host ""
