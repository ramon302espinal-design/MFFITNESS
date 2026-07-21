# Twilio — credenciales y uso (MFFITNESS)

## Stack real (no duplicar)
- NuGet `Twilio` en `BLL`
- **WhatsApp (único canal en uso):** `WhatsAppTwilioClient` + `MensajeAutomaticoBLL`
- Config: `CORE\TwilioSettings` + `%LocalAppData%\MFFITNESS\twilio.secrets.config`
- Host 24/7 (recordatorios): `Tools\WhatsAppHost`
- Facturas PDF: Supabase Storage (`FACTURAS`) → URL pública a Twilio

No hay fachada SMS/voz en código. Si se necesitan en el futuro, se añaden sobre el mismo `TwilioSettings` sin inventar un segundo cliente WhatsApp.

## Qué copiar desde Twilio Console
1. **Account SID** (`AC...`) — Account → Dashboard  
2. **API Key SID** (`SK...`) + **API Key Secret** — Account → API keys & tokens → Create API key  
   (preferido; el Auth Token primario no hace falta si usas API Key)  
3. Opcional: **Auth Token** — solo si no usas API Key  
4. **WhatsApp From** — Messaging → WhatsApp senders → `TwilioPhoneNumber` en App.config  
5. Plantillas Content (Meta Approved):
   - `TwilioContentSidGenerico` — avisos texto `{{1}}`
   - `TwilioContentSidFactura` — PDF `twilio/media` (opcional; ver script `Tools\WhatsAppHost\Create-FacturaMediaTemplate.ps1`)

## Dónde colocarlas
| Dato | Dónde |
|------|--------|
| Account SID, ApiKey SID/Secret (o AuthToken) | `%LocalAppData%\MFFITNESS\twilio.secrets.config` |
| Números públicos, ContentSid, flags | `UI\App.config` (sin secretos) |
| Env (opcional) | `TWILIO_ACCOUNT_SID`, `TWILIO_API_KEY_SID`, `TWILIO_API_KEY_SECRET` |

Plantilla: `twilio.secrets.config.example` en la raíz del repo.

## Ejemplo de uso (WhatsApp)
```csharp
var wa = new BLL.WhatsAppTwilioClient();
var r = wa.Enviar(
    "+18095551234",
    "Comprobante de pago MFFITNESS.",
    mediaUrl: "https://.../storage/v1/object/public/FACTURAS/factura_1.pdf");
// r.Entregado, r.Detalle, r.MessageSid
```

Ops facturas: `DOCS\WHATSAPP_FACTURA_OPS.md`
