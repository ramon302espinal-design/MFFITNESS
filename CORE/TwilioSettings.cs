using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace CORE
{
    public static class TwilioSettings
    {
        private const string NumeroSandboxTwilio = "+14155238886";

        public static bool WhatsAppHabilitado =>
            bool.TryParse(ConfigurationManager.AppSettings["TwilioWhatsAppEnabled"], out bool habilitado)
                ? habilitado
                : true;

        /// <summary>
        /// Si false, FrmPresentacion no arranca el timer (usar WhatsAppHost 24/7).
        /// </summary>
        public static bool TimerUIHabilitado =>
            !bool.TryParse(ConfigurationManager.AppSettings["TwilioTimerUIHabilitado"], out bool habilitado)
            || habilitado;

        public static bool ModoProduccion =>
            bool.TryParse(ConfigurationManager.AppSettings["TwilioModoProduccion"], out bool produccion)
                ? produccion
                : !AppConfig.ModoPrueba;

        public static string AccountSid =>
            LeerCredencial("TWILIO_ACCOUNT_SID", "TwilioAccountSid");

        public static string AuthToken =>
            LeerCredencial("TWILIO_AUTH_TOKEN", "TwilioAuthToken");

        /// <summary>API Key SID (SK...). Preferida frente a AuthToken si esta configurada.</summary>
        public static string ApiKeySid =>
            LeerCredencial("TWILIO_API_KEY_SID", "TwilioApiKeySid");

        /// <summary>API Key Secret. Usar con ApiKeySid + AccountSid.</summary>
        public static string ApiKeySecret =>
            LeerCredencial("TWILIO_API_KEY_SECRET", "TwilioApiKeySecret");

        public static bool UsaApiKey =>
            !string.IsNullOrWhiteSpace(ApiKeySid)
            && !string.IsNullOrWhiteSpace(ApiKeySecret)
            && !string.IsNullOrWhiteSpace(AccountSid);

        public static string PhoneNumber =>
            ConfigurationManager.AppSettings["TwilioPhoneNumber"] ?? string.Empty;

        /// <summary>Numero origen SMS (E.164). Fallback: TwilioPhoneNumber.</summary>
        public static string SmsFromNumber
        {
            get
            {
                string v = LeerOpcional("TWILIO_SMS_FROM", "TwilioSmsPhoneNumber");
                return string.IsNullOrWhiteSpace(v) ? PhoneNumber : v;
            }
        }

        /// <summary>Numero origen voz (E.164). Fallback: TwilioPhoneNumber.</summary>
        public static string VoiceFromNumber
        {
            get
            {
                string v = LeerOpcional("TWILIO_VOICE_FROM", "TwilioVoicePhoneNumber");
                return string.IsNullOrWhiteSpace(v) ? PhoneNumber : v;
            }
        }

        /// <summary>
        /// URL publica TwiML para llamadas (ej. bin de TwiML o webhook propio).
        /// Obligatoria para EnviarLlamadaVozAsync.
        /// </summary>
        public static string VoiceTwimlUrl =>
            LeerOpcional("TWILIO_VOICE_TWIML_URL", "TwilioVoiceTwimlUrl");

        public static int DiasRecordatorioDeuda =>
            int.TryParse(ConfigurationManager.AppSettings["DiasRecordatorioDeuda"], out int dias) && dias > 0
                ? dias
                : 5;

        public static int DiasRecordatorioMembresia =>
            int.TryParse(ConfigurationManager.AppSettings["DiasRecordatorioMembresia"], out int dias) && dias > 0
                ? dias
                : 10;

        public static int DiasRecordatorioMembresiaUrgente =>
            int.TryParse(ConfigurationManager.AppSettings["DiasRecordatorioMembresiaUrgente"], out int dias) && dias >= 0
                ? dias
                : 3;

        public static int IntervaloAutomatizacionMinutos =>
            int.TryParse(ConfigurationManager.AppSettings["IntervaloAutomatizacionMinutos"], out int minutos) && minutos > 0
                ? minutos
                : 30;

        public static int MaxReintentosMensajes =>
            int.TryParse(ConfigurationManager.AppSettings["TwilioMaxReintentosMensajes"], out int max) && max > 0
                ? max
                : 5;

        public static int MaxIntentosPorMensaje =>
            int.TryParse(ConfigurationManager.AppSettings["TwilioMaxIntentosPorMensaje"], out int max) && max > 0
                ? max
                : 3;

        /// <summary>
        /// Plantilla utilitaria aprobada en Twilio (cuerpo: {{1}}).
        /// Obligatoria para entrega en produccion fuera de ventana 24h.
        /// </summary>
        public static string ContentSidGenerico =>
            ConfigurationManager.AppSettings["TwilioContentSidGenerico"] ?? string.Empty;

        /// <summary>
        /// Nombre de la variable en la plantilla Twilio (ej. "1" para {{1}}).
        /// </summary>
        public static string ContentVariableKey =>
            string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TwilioContentVariableKey"])
                ? "1"
                : ConfigurationManager.AppSettings["TwilioContentVariableKey"]!.Trim();

        /// <summary>
        /// Plantilla de texto categoria UTILITY para avisos de cuenta
        /// (desactivacion, vencimientos, deudas). Variables:
        /// {{1}} miembro, {{2}} asunto, {{3}} detalle, {{4}} fecha.
        /// Meta bloquea con error 63049 las plantillas categorizadas MARKETING,
        /// por eso los avisos transaccionales deben ir por esta.
        /// </summary>
        public static string ContentSidAvisoUtility =>
            ConfigurationManager.AppSettings["TwilioContentSidAvisoUtility"]?.Trim() ?? string.Empty;

        /// <summary>
        /// Candidatos separados por ';': Meta puede tardar o rechazar una redaccion,
        /// asi que se declara mas de una y el cliente usa la primera aprobada.
        /// </summary>
        public static IReadOnlyList<string> ContentSidsAvisoUtility =>
            ContentSidAvisoUtility
                .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(sid => sid.Trim())
                .Where(sid => sid.Length > 0)
                .ToArray();

        public static bool UsaPlantillaAvisoUtility =>
            ContentSidsAvisoUtility.Count > 0;

        /// <summary>
        /// Plantilla ContentSid tipo twilio/media (PDF) aprobada en Meta.
        /// media: https://TU-DOMINIO-NGROK/{{1}}  body: "Comprobante MFFITNESS. {{2}}"
        /// Si esta vacia, se intenta adjunto libre o link en plantilla generica.
        /// </summary>
        public static string ContentSidFactura =>
            ConfigurationManager.AppSettings["TwilioContentSidFactura"]?.Trim() ?? string.Empty;

        public static bool UsaPlantillaFacturaMedia =>
            !string.IsNullOrWhiteSpace(ContentSidFactura);

        public static bool ExigirEntregaConfirmada =>
            !bool.TryParse(ConfigurationManager.AppSettings["TwilioExigirEntregaConfirmada"], out bool exigir)
            || exigir;

        public static bool PermitirBodyEnProduccion =>
            bool.TryParse(ConfigurationManager.AppSettings["TwilioPermitirBodyEnProduccion"], out bool permitir)
            && permitir;

        /// <summary>
        /// Si true, intenta enviar el PDF como adjunto libre (Body+MediaUrl).
        /// En WhatsApp Business solo funciona dentro de la ventana 24h del cliente.
        /// Por defecto false: se envia el link del PDF dentro de la plantilla ContentSid.
        /// </summary>
        public static bool AdjuntarPdfLibre =>
            bool.TryParse(ConfigurationManager.AppSettings["WhatsAppAdjuntarPdfLibre"], out bool adjuntar)
            && adjuntar;

        public static int IntentosConsultaEstado =>
            int.TryParse(ConfigurationManager.AppSettings["TwilioIntentosConsultaEstado"], out int intentos) && intentos > 0
                ? intentos
                : 6;

        public static int MilisegundosEntreConsultasEstado =>
            int.TryParse(ConfigurationManager.AppSettings["TwilioMilisegundosEntreConsultasEstado"], out int ms) && ms > 0
                ? ms
                : 2000;

        /// <summary>
        /// URL publica HTTPS para que Twilio descargue el PDF (fallback sin Supabase).
        /// Con Supabase, FacturaStorage usa la URL del bucket y esta clave puede quedar vacia.
        /// </summary>
        public static string PublicBaseUrl
        {
            get
            {
                string? env = Environment.GetEnvironmentVariable("WHATSAPP_PUBLIC_BASE_URL");
                if (!string.IsNullOrWhiteSpace(env))
                    return env.Trim().TrimEnd('/');

                string? stack = WhatsAppStackSecrets.PublicBaseUrlOverride;
                if (!string.IsNullOrWhiteSpace(stack))
                    return stack.Trim().TrimEnd('/');

                return ConfigurationManager.AppSettings["WhatsAppPublicBaseUrl"]?.Trim().TrimEnd('/')
                       ?? string.Empty;
            }
        }

        /// <summary>
        /// URL local donde escucha Kestrel (WhatsAppHost).
        /// </summary>
        public static string MediaListenUrl =>
            ConfigurationManager.AppSettings["WhatsAppMediaListenUrl"]?.Trim()
            ?? "http://127.0.0.1:5088";

        /// <summary>Ruta POST del webhook inbound (Kestrel WhatsAppHost).</summary>
        public static string WebhookInboundPath =>
            string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TwilioWebhookInboundPath"])
                ? "/webhook/twilio/whatsapp"
                : ConfigurationManager.AppSettings["TwilioWebhookInboundPath"]!.Trim();

        /// <summary>
        /// Valida X-Twilio-Signature en el webhook. Requiere WhatsAppPublicBaseUrl HTTPS.
        /// </summary>
        public static bool ValidarFirmaWebhook =>
            !bool.TryParse(ConfigurationManager.AppSettings["TwilioWebhookValidarFirma"], out bool validar)
            || validar;

        public static string? WebhookPublicUrl
        {
            get
            {
                string? env = Environment.GetEnvironmentVariable("TWILIO_WEBHOOK_PUBLIC_URL");
                if (!string.IsNullOrWhiteSpace(env))
                    return env.Trim().TrimEnd('/');

                string baseUrl = PublicBaseUrl;
                if (string.IsNullOrWhiteSpace(baseUrl))
                    return null;

                string path = WebhookInboundPath.StartsWith('/')
                    ? WebhookInboundPath
                    : "/" + WebhookInboundPath;

                return baseUrl + path;
            }
        }

        /// <summary>Estado del webhook inbound para la UI de chat.</summary>
        public static string? ObtenerEstadoWebhookInbound()
        {
            if (string.IsNullOrWhiteSpace(WebhookPublicUrl))
                return "Webhook inbound: falta WhatsAppPublicBaseUrl (HTTPS) — ver tooltip ℹ";

            return null;
        }

        /// <summary>Instrucciones para configurar recepción de mensajes (tooltip UI).</summary>
        public static string ObtenerAyudaWebhookInbound()
        {
            string url = WebhookPublicUrl ?? $"{PublicBaseUrl}{WebhookInboundPath}";
            return
                "Para RECIBIR mensajes de WhatsApp:\r\n\r\n" +
                "1. WhatsAppHost corriendo (puerto 5088).\r\n" +
                "2. Túnel HTTPS (dev: .\\Start-WhatsAppStack.ps1 -Profile Dev)\r\n" +
                "   prod: .\\Install-WhatsAppStack.ps1\r\n" +
                "3. URL pública en %LocalAppData%\\MFFITNESS\\whatsapp.stack.config\r\n" +
                "   clave WhatsAppPublicBaseUrl (o App.config UI)\r\n" +
                "   (o variable WHATSAPP_PUBLIC_BASE_URL)\r\n" +
                "4. Twilio Console → Messaging → WhatsApp Sender\r\n" +
                "   When a message comes in (POST):\r\n" +
                $"   {url}\r\n\r\n" +
                "Reinicie la app tras guardar App.config.";
        }

        public static bool WebhookInboundConfigurado =>
            !string.IsNullOrWhiteSpace(WebhookPublicUrl);

        /// <summary>
        /// Si false, FrmChat solo recibe/muestra mensajes (sin envío manual).
        /// App.config → ChatEnvioManualHabilitado (default: false).
        /// </summary>
        public static bool ChatEnvioManualHabilitado =>
            bool.TryParse(ConfigurationManager.AppSettings["ChatEnvioManualHabilitado"], out bool habilitado)
            && habilitado;

        public static bool UsaPlantillaContent =>
            !string.IsNullOrWhiteSpace(ContentSidGenerico);

        public static bool CredencialesConfiguradas =>
            !string.IsNullOrWhiteSpace(AccountSid)
            && !string.IsNullOrWhiteSpace(PhoneNumber)
            && (UsaApiKey || !string.IsNullOrWhiteSpace(AuthToken));

        /// <summary>
        /// Usuario/password para HTTP Basic (Content API, etc.).
        /// Con API Key: Sid de la key + Secret. Con AuthToken: AccountSid + AuthToken.
        /// </summary>
        public static (string User, string Password) CredencialesHttpBasicas =>
            UsaApiKey
                ? (ApiKeySid, ApiKeySecret)
                : (AccountSid, AuthToken);

        public static bool EsNumeroSandbox =>
            NormalizarNumero(PhoneNumber) == NormalizarNumero(NumeroSandboxTwilio);

        public static bool ConfiguracionListaParaProduccion =>
            CredencialesConfiguradas && ModoProduccion && !EsNumeroSandbox;

        public static string? ObtenerAdvertenciaConfiguracion()
        {
            if (!WhatsAppHabilitado)
                return null;

            if (!CredencialesConfiguradas)
                return "WhatsApp: faltan credenciales Twilio. Configure TWILIO_ACCOUNT_SID / TWILIO_AUTH_TOKEN " +
                       $"o el archivo {TwilioLocalSecrets.RutaArchivoSecretos} (AccountSid + AuthToken o ApiKey).";

            if (ModoProduccion && EsNumeroSandbox)
                return "WhatsApp en modo produccion pero TwilioPhoneNumber sigue siendo el sandbox (+14155238886). " +
                       "Actualice App.config con su numero WhatsApp Business aprobado en Twilio.";

            if (ModoProduccion && !UsaPlantillaContent && !PermitirBodyEnProduccion)
                return "WhatsApp produccion: falta TwilioContentSidGenerico. Cree en Twilio una plantilla utilitaria " +
                       "con cuerpo {{1}}, apruebela en Meta y pegue el ContentSid (HX...) en App.config.";

            return null;
        }

        /// <summary>
        /// Prioridad: variable de entorno → archivo local (%LocalAppData%) → App.config.
        /// </summary>
        private static string LeerCredencial(string envName, string appSettingKey)
        {
            string? env = Environment.GetEnvironmentVariable(envName);
            if (!string.IsNullOrWhiteSpace(env))
                return env.Trim();

            string? local = TwilioLocalSecrets.Get(appSettingKey);
            if (!string.IsNullOrWhiteSpace(local))
                return local.Trim();

            return ConfigurationManager.AppSettings[appSettingKey]?.Trim() ?? string.Empty;
        }

        private static string LeerOpcional(string envName, string appSettingKey)
        {
            string? env = Environment.GetEnvironmentVariable(envName);
            if (!string.IsNullOrWhiteSpace(env))
                return env.Trim();

            return ConfigurationManager.AppSettings[appSettingKey]?.Trim() ?? string.Empty;
        }

        private static string NormalizarNumero(string numero)
        {
            if (string.IsNullOrWhiteSpace(numero))
                return string.Empty;

            numero = numero.Trim();
            if (!numero.StartsWith("+"))
                numero = "+" + numero.TrimStart('+');

            return numero;
        }
    }
}
