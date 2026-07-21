# Facturas PDF + WhatsApp

## Producción: Supabase Storage (recomendado)
1. Bucket público `FACTURAS`
2. Credenciales: `%LocalAppData%\MFFITNESS\supabase.secrets.config`
3. `UI\App.config`: `SupabaseStorageEnabled=true`, `SupabaseUrl`, `SupabaseBucketFacturas=FACTURAS`
4. URL pública:
   `https://{proyecto}.supabase.co/storage/v1/object/public/FACTURAS/factura_{pagoId}.pdf`

Con Supabase configurado, la UI **no** arranca Kestrel/Ngrok. WhatsAppHost solo corre el loop de recordatorios.

## Fallback (solo si NO hay Supabase)
Kestrel local (`WhatsAppHost --media-only` en `:5088`) + URL pública HTTPS (`WhatsAppPublicBaseUrl`).

El script histórico de túnel Ngrok está en:
`Tools\WhatsAppHost\archive\Start-MediaTunnel.ps1`

No usarlo en producción si Supabase está activo.

## Config
- Twilio: `%LocalAppData%\MFFITNESS\twilio.secrets.config`
- Supabase: `%LocalAppData%\MFFITNESS\supabase.secrets.config`
- PDF locales: `%LocalAppData%\MFFITNESS\Facturas\factura_{pagoId}.pdf`

## Envío
- Plantilla genérica: `TwilioContentSidGenerico`
- PDF adjunto siempre: plantilla `twilio/media` + `TwilioContentSidFactura` (Approved en Meta)
- Sin plantilla media: adjunto libre solo en ventana 24h
