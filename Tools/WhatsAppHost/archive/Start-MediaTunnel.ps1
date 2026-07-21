# Arranca WhatsAppHost (media) + Ngrok — FALLBACK si Supabase no esta activo.
# Requisitos:
#   1) ngrok config add-authtoken TU_TOKEN
#   2) Haber compilado WhatsAppHost (Debug)
#   3) Definir dominio fijo Ngrok abajo (o variable MFF_NGROK_DOMAIN)

$ErrorActionPreference = "Stop"
# .../Tools/WhatsAppHost -> repo root
$repo = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
if (-not (Test-Path (Join-Path $repo "Tools\WhatsAppHost\WhatsAppHost.csproj"))) {
    $repo = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}

$FixedDomain = if ($env:MFF_NGROK_DOMAIN) { $env:MFF_NGROK_DOMAIN.Trim() } else { "" }
if ([string]::IsNullOrWhiteSpace($FixedDomain)) {
    Write-Host "ERROR: Defina su dominio Ngrok en `$FixedDomain o env MFF_NGROK_DOMAIN."
    Write-Host "Preferido: usar Supabase Storage (sin Ngrok)."
    exit 1
}
$hostExe = Join-Path $repo "Tools\WhatsAppHost\bin\Debug\net10.0\WhatsAppHost.exe"
$hostCfg = Join-Path $repo "Tools\WhatsAppHost\App.config"
$uiCfg   = Join-Path $repo "UI\App.config"
$hostDllCfg = Join-Path $repo "Tools\WhatsAppHost\bin\Debug\net10.0\WhatsAppHost.dll.config"

if (-not (Test-Path $hostExe)) {
    Write-Host "Compilando WhatsAppHost..."
    dotnet build (Join-Path $repo "Tools\WhatsAppHost\WhatsAppHost.csproj") -c Debug --nologo
}

Get-Process ngrok -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Get-NetTCPConnection -LocalPort 5088 -ErrorAction SilentlyContinue |
    ForEach-Object { Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue }
Start-Sleep -Seconds 1

Write-Host "Iniciando WhatsAppHost --media-only..."
$hostProc = Start-Process -FilePath $hostExe -ArgumentList "--media-only" -PassThru -WorkingDirectory (Split-Path $hostExe)

Start-Sleep -Seconds 2
try {
    $health = Invoke-RestMethod "http://127.0.0.1:5088/health" -TimeoutSec 5
    Write-Host "Kestrel OK:" ($health | ConvertTo-Json -Compress)
} catch {
    Write-Host "ERROR: Kestrel no responde en 5088. $($_.Exception.Message)"
    exit 1
}

Write-Host "Iniciando ngrok dominio fijo -> 5088..."
$ngrokProc = Start-Process -FilePath "ngrok" -ArgumentList @("http","--url=$FixedDomain","5088","--log=stdout") -PassThru -WindowStyle Minimized

$publicUrl = $null
for ($i = 0; $i -lt 40; $i++) {
    Start-Sleep -Seconds 1
    try {
        $tunnels = Invoke-RestMethod "http://127.0.0.1:4040/api/tunnels" -TimeoutSec 2
        $https = $tunnels.tunnels | Where-Object { $_.public_url -like "https://*" } | Select-Object -First 1
        if ($https) {
            $publicUrl = $https.public_url.TrimEnd('/')
            break
        }
    } catch { }
}

if (-not $publicUrl) {
    Write-Host "Ngrok no entrego URL. WhatsAppHost PID $($hostProc.Id) sigue corriendo."
    exit 2
}

function Set-PublicBaseUrl([string]$configPath, [string]$url) {
    if (-not (Test-Path $configPath)) { return }
    [xml]$xml = Get-Content $configPath
    $node = $xml.configuration.appSettings.add | Where-Object { $_.key -eq "WhatsAppPublicBaseUrl" }
    if ($node) {
        $node.value = $url
        $xml.Save($configPath)
        Write-Host "Actualizado: $configPath"
    }
}

Set-PublicBaseUrl $hostCfg $publicUrl
Set-PublicBaseUrl $uiCfg $publicUrl
if (Test-Path $hostDllCfg) { Set-PublicBaseUrl $hostDllCfg $publicUrl }
$uiDll = Join-Path $repo "UI\bin\Debug\net10.0-windows\UI.dll.config"
if (Test-Path $uiDll) { Set-PublicBaseUrl $uiDll $publicUrl }

Write-Host ""
Write-Host "LISTO"
Write-Host "  Public URL : $publicUrl"
Write-Host "  Health     : $publicUrl/health"
Write-Host "  Factura    : $publicUrl/media/factura/{pagoId}.pdf"
Write-Host "  Host PID   : $($hostProc.Id)  Ngrok PID: $($ngrokProc.Id)"
Write-Host "Deja estos procesos abiertos al cobrar."
