using BLL;
using CORE;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Text;
using Twilio.Security;

namespace WhatsAppHost
{
    /// <summary>
    /// POST /webhook/twilio/status — estados de entrega. No responde con mensaje al miembro.
    /// </summary>
    internal static class TwilioStatusWebhook
    {
        public static async Task<IResult> HandleAsync(HttpContext context)
        {
            if (!HttpMethods.IsPost(context.Request.Method))
                return Results.StatusCode(StatusCodes.Status405MethodNotAllowed);

            WhatsAppStackSecrets.InvalidateCache();

            IFormCollection form;
            try
            {
                form = await context.Request.ReadFormAsync(context.RequestAborted);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{Ahora()}] Status: form invalido: {ex.Message}");
                return Results.BadRequest();
            }

            var parametros = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (string key in form.Keys)
                parametros[key] = form[key].ToString();

            string? messageSid = parametros.GetValueOrDefault("MessageSid");
            string? messageStatus = parametros.GetValueOrDefault("MessageStatus");
            if (string.IsNullOrWhiteSpace(messageSid) || string.IsNullOrWhiteSpace(messageStatus))
                return Results.Ok();

            if (!ValidarFirma(context, parametros))
            {
                Console.WriteLine($"[{Ahora()}] Status: firma Twilio invalida sid={messageSid}");
                return Results.Unauthorized();
            }

            try
            {
                bool aplicado = new ChatBLL().AplicarEstadoEntregaTwilio(
                    messageSid,
                    messageStatus,
                    parametros.GetValueOrDefault("ErrorCode"),
                    parametros.GetValueOrDefault("ErrorMessage"));

                Console.WriteLine($"[{Ahora()}] Status sid={messageSid} status={messageStatus} aplicado={aplicado}");
                return Results.Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{Ahora()}] Status ERROR sid={messageSid}: {ex.Message}");
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        private static bool ValidarFirma(HttpContext context, IDictionary<string, string> parametros)
        {
            if (!TwilioSettings.ValidarFirmaWebhook)
                return true;

            string? publicUrl = TwilioSettings.StatusCallbackPublicUrl;
            if (string.IsNullOrWhiteSpace(publicUrl))
            {
                Console.WriteLine($"[{Ahora()}] Status: sin URL publica HTTPS; firma omitida (dev).");
                return true;
            }

            string authToken = TwilioSettings.AuthToken;
            if (string.IsNullOrWhiteSpace(authToken))
            {
                Console.WriteLine($"[{Ahora()}] Status: sin AuthToken; firma omitida.");
                return true;
            }

            string? signature = context.Request.Headers["X-Twilio-Signature"];
            if (string.IsNullOrWhiteSpace(signature))
                return false;

            var validator = new RequestValidator(authToken);
            return validator.Validate(publicUrl, parametros, signature);
        }

        private static string Ahora() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
