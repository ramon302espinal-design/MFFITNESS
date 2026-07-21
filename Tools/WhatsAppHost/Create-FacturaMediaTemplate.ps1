# Crea plantilla Twilio Content twilio/media (PDF) Meta-compliant para comprobante MFFITNESS.
#
# Por que rechazo Meta la anterior (HXf4b6...):
#  1) El body TERMINABA en {{2}}  → Meta/Twilio lo prohíben.
#  2) Cuerpo casi vacío + 1 variable → parece spam / sin utilidad clara.
#  3) Sample media "preview unavailable" → Meta no pudo validar el PDF.
#
# Esta versión:
#  - Body UTILITY con texto fijo abundante (recibo de membresía).
#  - Variables {{2}}..{{6}} EN MEDIO del texto (nunca al inicio ni al final).
#  - {{1}} solo en la URL del PDF (Supabase FACTURAS).
#  - Sample PDF público verificado antes de crear.
#
# Uso:
#   powershell -ExecutionPolicy Bypass -File .\Create-FacturaMediaTemplate.ps1

$ErrorActionPreference = "Stop"

$FriendlyName = "mff_comprobante_membresia_v2"
$WhatsAppName = "mff_comprobante_membresia_v2"  # lowercase, guiones bajos

$uiCfg = Join-Path (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent) "UI\App.config"
[xml]$cfg = Get-Content $uiCfg
$supa = ($cfg.SelectNodes("//add[@key='SupabaseUrl']") | Select-Object -First 1).value
$bucket = ($cfg.SelectNodes("//add[@key='SupabaseBucketFacturas']") | Select-Object -First 1).value
if ([string]::IsNullOrWhiteSpace($bucket)) { $bucket = "FACTURAS" }
if ([string]::IsNullOrWhiteSpace($supa)) { throw "Falta SupabaseUrl en UI\App.config" }

$Domain = ($supa.TrimEnd('/') + "/storage/v1/object/public/" + $bucket.Trim('/'))
$SamplePath = "factura_sample.pdf"
$SampleMedia = "$Domain/$SamplePath"

# Body Meta-compliant: NO empieza ni termina con variable; NO variables juntas; sin marketing.
$Body = "Hola. Confirmamos la recepcion de tu pago de membresia en MFFITNESS. Plan: {{2}}. Monto pagado: {{3}}. Fecha de pago: {{4}}. Vigencia hasta: {{5}}. Numero de recibo: {{6}}. Adjuntamos el comprobante en PDF para tu archivo personal. Si no reconoces este pago, contacta a recepcion del gimnasio."

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

function Get-SupabaseKey {
    $path = Join-Path $env:LOCALAPPDATA "MFFITNESS\supabase.secrets.config"
    if (-not (Test-Path $path)) { throw "Falta $path" }
    [xml]$x = Get-Content $path
    $map = @{}
    foreach ($a in $x.SelectNodes("//add")) {
        if ($a.GetAttribute("key")) { $map[$a.GetAttribute("key")] = $a.GetAttribute("value") }
    }
    $key = $map["SupabaseKey"]
    if (-not $key) { $key = $map["SupabasePublishableKey"] }
    if (-not $key) { $key = $map["Key"] }
    if (-not $key) { throw "supabase.secrets.config sin key" }
    $url = $map["SupabaseUrl"]; if (-not $url) { $url = $map["Url"] }
    if (-not $url) { $url = $script:supa }
    return @{ Url = $url.TrimEnd('/'); Key = $key }
}

function Ensure-SamplePdf {
    Write-Host "Verificando sample: $SampleMedia"
    try {
        $r = Invoke-WebRequest $SampleMedia -UseBasicParsing -TimeoutSec 25
        $ct = $r.Headers["Content-Type"]
        if ($r.StatusCode -eq 200 -and $r.RawContentLength -gt 500) {
            Write-Host "Sample OK ($($r.RawContentLength) bytes, Content-Type=$ct)"
            return
        }
    } catch {
        Write-Host "Sample no accesible: $($_.Exception.Message)"
    }

    $localDir = Join-Path $env:LOCALAPPDATA "MFFITNESS\Facturas"
    $localPdf = Get-ChildItem $localDir -Filter "factura_*.pdf" -ErrorAction SilentlyContinue |
        Sort-Object Length -Descending |
        Select-Object -First 1
    if (-not $localPdf) { throw "No hay PDF local en $localDir para usar como sample Meta." }

    Write-Host "Subiendo sample desde $($localPdf.Name) ($([math]::Round($localPdf.Length/1KB)) KB)..."
    $supa = Get-SupabaseKey
    $uploadUri = "$($supa.Url)/storage/v1/object/$bucket/$SamplePath"
    $bytes = [System.IO.File]::ReadAllBytes($localPdf.FullName)
    Invoke-RestMethod -Method Post -Uri $uploadUri `
        -Headers @{
            Authorization = "Bearer $($supa.Key)"
            apikey = $supa.Key
            "Content-Type" = "application/pdf"
            "x-upsert" = "true"
        } `
        -Body $bytes | Out-Null

    Start-Sleep -Seconds 1
    $r2 = Invoke-WebRequest $SampleMedia -UseBasicParsing -TimeoutSec 25
    if ($r2.StatusCode -ne 200 -or $r2.RawContentLength -lt 500) {
        throw "Sample subido pero GET publico fallo (HTTP $($r2.StatusCode), $($r2.RawContentLength) bytes)."
    }
    Write-Host "Sample publico OK ($($r2.RawContentLength) bytes)"
}

Write-Host "=== Plantilla Meta UTILITY (comprobante PDF) ==="
Write-Host "Dominio: $Domain"
Write-Host "Body: $Body"
Ensure-SamplePdf

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
        "1" = $SamplePath
        "2" = "Mensual Premium"
        "3" = "RD 2500.00"
        "4" = "13/07/2026"
        "5" = "13/08/2026"
        "6" = "MF-9001"
    }
    types = @{
        "twilio/media" = @{
            body = $Body
            media = @("$Domain/{{1}}")
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
    Write-Host "Solicitud manual en Console si falla auto:"
    Write-Host "  Messaging > Content Template Builder > $hx"
    Write-Host $_.Exception.Message
}

Write-Host ""
Write-Host "LISTO. Cuando Meta marque Approved, usa este ContentSid:"
Write-Host "  TwilioContentSidFactura = $hx"
Write-Host ""
Write-Host "IMPORTANTE: elimina o ignora la plantilla rechazada anterior (HXf4b6d14ce5d8a46d3f9be76f260a4a5f)."
