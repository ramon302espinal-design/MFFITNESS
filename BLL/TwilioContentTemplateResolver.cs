using CORE;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace BLL
{
    /// <summary>
    /// Consulta la plantilla Twilio Content API para obtener claves de variables ({{1}}, etc.).
    /// </summary>
    internal static class TwilioContentTemplateResolver
    {
        private static string? _variableKeyCache;
        private static string? _contentSidCache;

        public static string ObtenerClaveVariable(string contentSid)
        {
            if (string.IsNullOrWhiteSpace(contentSid))
                return TwilioSettings.ContentVariableKey;

            if (_variableKeyCache != null
                && string.Equals(_contentSidCache, contentSid, StringComparison.OrdinalIgnoreCase))
            {
                return _variableKeyCache;
            }

            string? detectada = IntentarDetectarDesdeApi(contentSid);
            _contentSidCache = contentSid;
            _variableKeyCache = detectada ?? TwilioSettings.ContentVariableKey;
            return _variableKeyCache;
        }

        private static string? IntentarDetectarDesdeApi(string contentSid)
        {
            if (!TwilioSettings.CredencialesConfiguradas)
                return null;

            try
            {
                using var client = new HttpClient();
                var (user, password) = TwilioSettings.CredencialesHttpBasicas;
                string credentials = Convert.ToBase64String(
                    Encoding.ASCII.GetBytes($"{user}:{password}"));
                client.DefaultRequestHeaders.Add("Authorization", $"Basic {credentials}");

                string json = client.GetAsync($"https://content.twilio.com/v1/Content/{contentSid}")
                    .Result.Content.ReadAsStringAsync().Result;

                if (string.IsNullOrWhiteSpace(json))
                    return null;

                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("types", out var types))
                    return null;

                foreach (var typeProp in types.EnumerateObject())
                {
                    if (!typeProp.Value.TryGetProperty("variables", out var variables))
                        continue;

                    foreach (var variable in variables.EnumerateObject())
                    {
                        if (!string.IsNullOrWhiteSpace(variable.Name))
                            return variable.Name;
                    }
                }

                // Fallback: buscar {{n}} en el body
                string body = types.ToString();
                int idx = body.IndexOf("{{", StringComparison.Ordinal);
                while (idx >= 0)
                {
                    int end = body.IndexOf("}}", idx + 2, StringComparison.Ordinal);
                    if (end > idx)
                    {
                        string nombre = body.Substring(idx + 2, end - idx - 2).Trim();
                        if (!string.IsNullOrWhiteSpace(nombre))
                            return nombre;
                    }

                    idx = body.IndexOf("{{", idx + 2, StringComparison.Ordinal);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TwilioContentTemplateResolver: {ex.Message}");
            }

            return null;
        }
    }
}
