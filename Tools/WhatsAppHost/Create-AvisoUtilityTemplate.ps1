# Crea la plantilla Twilio Content de TEXTO categoria UTILITY para avisos de cuenta.
#
# Por que hace falta:
#   La plantilla actual "aviso_mffitness_general" (HX4b6b...) quedo categorizada como
#   MARKETING en Meta. Desde el 01/04/2025 Meta bloquea marketing hacia numeros +1
#   con el error 63049, por eso los avisos (desactivacion, deudas, vencimientos)
#   salian "sent" en Twilio pero nunca llegaban al cliente.
#
# Esta plantilla es transaccional (UTILITY):
#   - Texto fijo abundante que describe un aviso de cuenta.
#   - Variables SIEMPRE en medio del cuerpo (nunca al inicio ni al final).
#   - Sin lenguaje promocional.
#
# Variables:
#   {{1}} miembro   {{2}} asunto   {{3}} detalle   {{4}} fecha
#
# Uso:
#   powershell -ExecutionPolicy Bypass -File .\Create-AvisoUtilityTemplate.ps1

param(
    [string]$Nombre = "mff_aviso_cuenta_v2",
    [string]$Texto = ""
)

$ErrorActionPreference = "Stop"

$FriendlyName = $Nombre
$WhatsAppName = $Nombre

# v2: se evita el saludo inicial y cualquier invitacion, porque Meta lee esos
# patrones como marketing. Todo el cuerpo describe un registro ya ocurrido.
$Body = if ($Texto) { $Texto } else {
    "Notificacion de tu cuenta en MFFITNESS. Este mensaje confirma un movimiento " + `
    "registrado en tu ficha de miembro. Miembro: {{1}}. Movimiento: {{2}}. " + `
    "Detalle del registro: {{3}}. Fecha y hora del registro: {{4}}. " + `
    "Conserva este aviso como constancia del movimiento en tu cuenta. " + `
    "Para revisar el detalle completo o corregir cualquier dato, presentate en la recepcion de MFFITNESS."
}

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
    if ($apiSid -and $apiSec) { return @{ User = $apiSid; Pass = $apiSec; Mode = "ApiKey" } }
    if ($tok) { return @{ User = $sid; Pass = $tok; Mode = "AuthToken" } }
    throw "Faltan ApiKey o AuthToken en twilio.secrets.config"
}

Write-Host "=== Plantilla Meta UTILITY (aviso de cuenta) ==="
Write-Host "Body: $Body"

$creds = Get-TwilioCreds
$basic = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $creds.User, $creds.Pass)))
$headers = @{
    Authorization  = "Basic $basic"
    "Content-Type" = "application/json"
}

$bodyObj = @{
    friendly_name = $FriendlyName
    language      = "es"
    variables     = @{
        "1" = "Juan Perez"
        "2" = "Membresia desactivada"
        "3" = "Tu membresia fue desactivada. Motivo: solicitud del miembro."
        "4" = "16/08/2026 10:15 AM"
    }
    types         = @{
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
    name     = $WhatsAppName
    category = "UTILITY"
} | ConvertTo-Json -Compress

Write-Host "Solicitando aprobacion WhatsApp UTILITY..."
try {
    $appr = Invoke-RestMethod -Method Post `
        -Uri "https://content.twilio.com/v1/Content/$hx/ApprovalRequests/whatsapp" `
        -Headers $headers `
        -Body $approval
    Write-Host "Approval status: $($appr.status) categoria: $($appr.category)"
}
catch {
    Write-Host "Solicitud manual en Console si falla auto:"
    Write-Host "  Messaging > Content Template Builder > $hx"
    Write-Host $_.Exception.Message
}

Write-Host ""
Write-Host "LISTO. Añade el SID a TwilioContentSidAvisoUtility (varios separados por ';'):"
Write-Host "  $hx"
Write-Host "El sistema usa sola la primera plantilla que Meta apruebe."
