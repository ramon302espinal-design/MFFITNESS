# Crea plantilla Twilio Content (texto) Meta-compliant:
# - La variable {{1}} queda en medio, no al inicio ni al final.
# - Incluye contexto fijo de servicio y un cierre fijo.
# - Sirve para avisos operativos de membresía, deuda y cuenta.
#
# Sustituye a HX4b6bbb98799fc7a7fe02187bceb46ecb cuyo body fijo era:
#   "Estimado cliente de Mffitness, le enviamos una notificación importante sobre su cuenta: {{1}}"
#
# Uso:
#   powershell -ExecutionPolicy Bypass -File .\Create-AvisoGenericoTemplate.ps1
#
# Cuando Meta marque Approved, actualiza App.config:
#   TwilioContentSidGenerico = HX...

$ErrorActionPreference = "Stop"

$FriendlyName = "mff_aviso_cliente_v4"
$WhatsAppName = "mff_aviso_cliente_v4"
$Body = "Hola! Estimado cliente de Mf Fitness: {{1}} Para mas informacion o asistencia, comuniquese con la recepcion de Mf Fitness."
$Sample = "Le informamos que su membresia vence el 20/08/2026. Puede pasar por recepcion para renovarla."

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
        if (-not $sid) { $sid = $map["TwilioAccountSid"]; if (-not $sid) { $sid = $map["AccountSid"] } }
        if (-not $apiSid) { $apiSid = $map["TwilioApiKeySid"]; if (-not $apiSid) { $apiSid = $map["ApiKeySid"] } }
        if (-not $apiSec) { $apiSec = $map["TwilioApiKeySecret"]; if (-not $apiSec) { $apiSec = $map["ApiKeySecret"] } }
        if (-not $tok) { $tok = $map["TwilioAuthToken"]; if (-not $tok) { $tok = $map["AuthToken"] } }
    }

    if (-not $sid) { throw "Falta Twilio Account SID." }
    if ($apiSid -and $apiSec) { return @{ User = $apiSid; Pass = $apiSec; Mode = "ApiKey"; AccountSid = $sid } }
    if ($tok) { return @{ User = $sid; Pass = $tok; Mode = "AuthToken"; AccountSid = $sid } }
    throw "Faltan ApiKey o AuthToken en twilio.secrets.config"
}

Write-Host "=== Plantilla aviso generico (encabezado corto) ==="
Write-Host "Body: $Body"

$creds = Get-TwilioCreds
Write-Host "Auth: $($creds.Mode)"
$basic = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $creds.User, $creds.Pass)))
$headers = @{
    Authorization = "Basic $basic"
    "Content-Type" = "application/json"
}

$bodyObj = @{
    friendly_name = $FriendlyName
    language = "es"
    variables = @{
        "1" = $Sample
    }
    types = @{
        "twilio/text" = @{
            body = $Body
        }
    }
}

$json = $bodyObj | ConvertTo-Json -Depth 8 -Compress
Write-Host "Creando Content Template..."
$created = Invoke-RestMethod -Method Post -Uri "https://content.twilio.com/v1/Content" -Headers $headers -Body $json
$hx = $created.sid
Write-Host "Creada: $hx"

$approval = @{
    name = $WhatsAppName
    category = "UTILITY"
} | ConvertTo-Json -Compress

Write-Host "Solicitando aprobacion WhatsApp UTILITY..."
try {
    $appr = Invoke-RestMethod -Method Post `
        -Uri "https://content.twilio.com/v1/Content/$hx/ApprovalRequests/whatsapp" `
        -Headers $headers `
        -Body $approval
    Write-Host "Approval status: $($appr.status)"
} catch {
    Write-Host "Solicitud automatica fallo (puedes pedirla en Console):"
    Write-Host "  Messaging > Content Template Builder > $hx"
    Write-Host $_.Exception.Message
}

Write-Host ""
Write-Host "LISTO. Cuando Meta marque Approved, cambia en UI\App.config (y Tools\WhatsAppHost\App.config):"
Write-Host "  TwilioContentSidGenerico = $hx"
Write-Host ""
Write-Host "La plantilla vieja HX4b6bbb98799fc7a7fe02187bceb46ecb se deja de usar al actualizar el SID."
