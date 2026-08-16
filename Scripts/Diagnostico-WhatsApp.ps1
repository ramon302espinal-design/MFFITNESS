# Diagnostico de entrega WhatsApp (Twilio + Meta).
#
# Responde la pregunta "el mensaje salio pero, llego?":
#   - Estado FINAL de los ultimos mensajes (Twilio marca "sent" antes de que Meta lo rechace).
#   - Categoria de cada plantilla: Meta bloquea las MARKETING con el error 63049.
#
# No imprime credenciales.
#
# Uso:
#   powershell -ExecutionPolicy Bypass -File .\Diagnostico-WhatsApp.ps1
#   powershell -ExecutionPolicy Bypass -File .\Diagnostico-WhatsApp.ps1 -ContentSid HX...

param(
    [string]$ContentSid = ""
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Net.Http
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

function Get-TwilioCreds {
    $sid = $env:TWILIO_ACCOUNT_SID
    $apiSid = $env:TWILIO_API_KEY_SID
    $apiSec = $env:TWILIO_API_KEY_SECRET
    $tok = $env:TWILIO_AUTH_TOKEN

    $path = Join-Path $env:LOCALAPPDATA "MFFITNESS\twilio.secrets.config"
    if (Test-Path $path) {
        [xml]$x = Get-Content $path
        $map = @{}
        foreach ($a in $x.SelectNodes("//add")) {
            if ($a.GetAttribute("key")) { $map[$a.GetAttribute("key")] = $a.GetAttribute("value") }
        }
        if (-not $sid) { $sid = $map["TwilioAccountSid"] }
        if (-not $apiSid) { $apiSid = $map["TwilioApiKeySid"] }
        if (-not $apiSec) { $apiSec = $map["TwilioApiKeySecret"] }
        if (-not $tok) { $tok = $map["TwilioAuthToken"] }
    }

    if (-not $sid) { throw "Falta Twilio Account SID." }
    if ($apiSid -and $apiSec) { return @{ Sid = $sid; User = $apiSid; Pass = $apiSec } }
    if ($tok) { return @{ Sid = $sid; User = $sid; Pass = $tok } }
    throw "Faltan ApiKey o AuthToken en twilio.secrets.config"
}

$creds = Get-TwilioCreds
$http = New-Object System.Net.Http.HttpClient
$http.Timeout = [TimeSpan]::FromSeconds(30)
$basic = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $creds.User, $creds.Pass)))
$http.DefaultRequestHeaders.Authorization =
New-Object System.Net.Http.Headers.AuthenticationHeaderValue('Basic', $basic)

function Get-Json($url) {
    $r = $http.GetAsync($url).GetAwaiter().GetResult()
    if (-not $r.IsSuccessStatusCode) { return $null }
    return $r.Content.ReadAsStringAsync().GetAwaiter().GetResult() | ConvertFrom-Json
}

Write-Host "=== PLANTILLAS (categoria UTILITY = Meta entrega; MARKETING = 63049) ==="
$uiCfg = Join-Path (Split-Path $PSScriptRoot -Parent) "UI\App.config"
[xml]$cfg = Get-Content $uiCfg
$sids = @()
foreach ($k in @('TwilioContentSidAvisoUtility', 'TwilioContentSidFactura', 'TwilioContentSidGenerico')) {
    $v = ($cfg.SelectNodes("//add[@key='$k']") | Select-Object -First 1).value
    if ($v) { $sids += , @($k, $v) }
}
if ($ContentSid) { $sids += , @('ContentSidConsultado', $ContentSid.Trim()) }

foreach ($par in $sids) {
    $clave = $par[0]
    $hx = $par[1]
    $c = Get-Json "https://content.twilio.com/v1/Content/$hx"
    $a = Get-Json "https://content.twilio.com/v1/Content/$hx/ApprovalRequests"
    $nombre = if ($c) { $c.friendly_name } else { "(no encontrada)" }
    $estado = if ($a) { "$($a.whatsapp.status) / $($a.whatsapp.category)" } else { "(sin datos)" }
    Write-Host ("{0,-32} {1,-36} {2}" -f $clave, $nombre, $estado)
    if ($clave -eq 'ContentSidConsultado' -and $c) {
        $tipo = ($c.types | Get-Member -MemberType NoteProperty).Name | Select-Object -First 1
        $body = $c.types.$tipo.body
        Write-Host ("  Tipo: {0}" -f $tipo)
        Write-Host ("  Body: {0}" -f $body)
    }
}

Write-Host ""
Write-Host "=== ULTIMOS MENSAJES (estado final real) ==="
$data = Get-Json "https://api.twilio.com/2010-04-01/Accounts/$($creds.Sid)/Messages.json?PageSize=30"
if (-not $data) { Write-Host "No se pudo consultar la lista de mensajes."; exit 1 }

foreach ($m in $data.messages) {
    $tipo = if ($m.num_media -and [int]$m.num_media -gt 0) { "PDF  " } else { "TEXTO" }
    $err = if ($m.error_code) { "ERROR $($m.error_code) $($m.error_message)" } else { "" }
    Write-Host ("{0} | {1} | {2,-12} | {3} {4}" -f $m.date_sent, $tipo, $m.status, $m.to, $err)
}

Write-Host ""
Write-Host "Guia rapida de errores:"
Write-Host "  63016 = mensaje libre fuera de la ventana 24h  -> debe ir por plantilla aprobada."
Write-Host "  63019 = Twilio no pudo descargar el PDF        -> revisar bucket FACTURAS en Supabase."
Write-Host "  63024 = numero de destino invalido             -> corregir el telefono del miembro."
Write-Host "  63049 = Meta bloqueo plantilla MARKETING       -> usar plantilla categoria UTILITY."
