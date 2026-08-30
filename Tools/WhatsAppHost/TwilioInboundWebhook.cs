using BLL;
using BLL.Models;
using CORE;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Text;
using Twilio.Security;

namespace WhatsAppHost
{
    internal static class TwilioInboundWebhook
    {
        public static async Task<IResult> HandleAsync(HttpContext context)
        {
            if (!HttpMethods.IsPost(context.Request.Method))
                return Results.StatusCode(StatusCodes.Status405MethodNotAllowed);

            IFormCollection form;
            try
            {
                form = await context.Request.ReadFormAsync(context.RequestAborted);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{Ahora()}] Webhook: form invalido: {ex.Message}");
                return Results.BadRequest();
            }

            var parametros = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (string key in form.Keys)
                parametros[key] = form[key].ToString();

            if (!EsMensajeEntrante(parametros, out string? from, out string? body, out string? messageSid, out int numMedia))
                return Results.Ok();

            if (!ValidarFirma(context, parametros))
            {
                Console.WriteLine($"[{Ahora()}] Webhook: firma Twilio invalida desde {from}");
                return Results.Unauthorized();
            }

            var mediaUrls = ExtraerMediaUrls(form, numMedia);
            string? profileName = parametros.GetValueOrDefault("ProfileName");

            try
            {
                var chatBll = new ChatBLL();
                ChatInboundResult resultado = chatBll.ProcesarMensajeEntrante(
                    from!,
                    body,
                    messageSid!,
                    numMedia,
                    mediaUrls,
                    profileName);

                if (resultado.Duplicado)
                    return TwimlVacio();

                if (!resultado.Exito)
                {
                    Console.WriteLine($"[{Ahora()}] Webhook: {resultado.Detalle} ({resultado.Telefono})");
                    return TwimlVacio();
                }

                Console.WriteLine($"[{Ahora()}] Webhook IN #{resultado.MensajeId} cliente={resultado.ClienteId} sid={messageSid}");
                return TwimlVacio();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{Ahora()}] Webhook ERROR: {ex.Message}");
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        private static bool EsMensajeEntrante(
            IReadOnlyDictionary<string, string> parametros,
            out string? from,
            out string? body,
            out string? messageSid,
            out int numMedia)
        {
            from = parametros.GetValueOrDefault("From");
            body = parametros.GetValueOrDefault("Body");
            messageSid = parametros.GetValueOrDefault("MessageSid")
                           ?? parametros.GetValueOrDefault("SmsMessageSid");

            numMedia = int.TryParse(parametros.GetValueOrDefault("NumMedia"), out int n) && n > 0 ? n : 0;

            bool tieneContenido = !string.IsNullOrWhiteSpace(body) || numMedia > 0;
            bool esWhatsApp = !string.IsNullOrWhiteSpace(from)
                              && from.Contains("whatsapp:", StringComparison.OrdinalIgnoreCase);

            return tieneContenido && esWhatsApp && !string.IsNullOrWhiteSpace(messageSid);
        }

        private static List<string> ExtraerMediaUrls(IFormCollection form, int numMedia)
        {
            var urls = new List<string>();
            for (int i = 0; i < numMedia; i++)
            {
                string key = $"MediaUrl{i}";
                if (form.TryGetValue(key, out var value))
                {
                    string url = value.ToString();
                    if (!string.IsNullOrWhiteSpace(url))
                        urls.Add(url);
                }
            }

            return urls;
        }

        private static bool ValidarFirma(HttpContext context, IDictionary<string, string> parametros)
        {
            if (!TwilioSettings.ValidarFirmaWebhook)
                return true;

            string? publicUrl = TwilioSettings.WebhookPublicUrl;
            if (string.IsNullOrWhiteSpace(publicUrl))
            {
                Console.WriteLine($"[{Ahora()}] Webhook: sin URL publica; firma omitida (dev).");
                return true;
            }

            string authToken = TwilioSettings.AuthToken;
            if (string.IsNullOrWhiteSpace(authToken))
            {
                Console.WriteLine($"[{Ahora()}] Webhook: sin AuthToken; firma omitida.");
                return true;
            }

            string? signature = context.Request.Headers["X-Twilio-Signature"];
            if (string.IsNullOrWhiteSpace(signature))
                return false;

            var validator = new RequestValidator(authToken);
            return validator.Validate(publicUrl, parametros, signature);
        }

        private static IResult TwimlVacio()
        {
            const string twiml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Response></Response>";
            return Results.Content(twiml, "text/xml", Encoding.UTF8);
        }

        private static string Ahora() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
