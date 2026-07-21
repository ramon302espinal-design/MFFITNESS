using System;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BLL
{
    /// <summary>
    /// Normaliza texto para ContentVariables de Twilio/WhatsApp (error 21656).
    /// </summary>
    internal static class WhatsAppContentVariableHelper
    {
        private static readonly Regex EspaciosExcesivos = new(@"\s{5,}", RegexOptions.Compiled);
        private static readonly Regex PrefijoMffitness = new(
            @"^MFFITNESS\s*[-–—:]\s*",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly JsonSerializerOptions JsonOpciones = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public static string Sanitizar(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return "Notificacion de MFFITNESS.";

            string normalizado = texto.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(normalizado.Length);

            foreach (char c in normalizado)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                    continue;

                if (c == '\r' || c == '\n' || c == '\t')
                    sb.Append(' ');
                else if (c == '\'' || c == '`')
                    sb.Append('\u2019');
                else if (c == '"' || c == '\\' || c == '{' || c == '}' || c == '$')
                    continue;
                else if (c < 32)
                    continue;
                else if (EsEmojiOSimbolo(c))
                    continue;
                else
                    sb.Append(c);
            }

            string limpio = EspaciosExcesivos.Replace(sb.ToString(), "    ");

            while (limpio.Contains("  ", StringComparison.Ordinal))
                limpio = limpio.Replace("  ", " ", StringComparison.Ordinal);

            limpio = limpio.Trim();
            return string.IsNullOrEmpty(limpio) ? "Notificacion de MFFITNESS." : limpio;
        }

        private static bool EsEmojiOSimbolo(char c)
        {
            // Rangos comunes de emoji/symbol sin regex Unicode avanzado
            return c >= 0x2600 && c <= 0x27BF
                || c >= 0x1F300 && c <= 0x1FAFF;
        }

        public static string PrepararCuerpoPlantilla(string mensaje, string? nombreCliente)
        {
            string cuerpo = Sanitizar(mensaje);
            cuerpo = PrefijoMffitness.Replace(cuerpo, string.Empty);

            if (!string.IsNullOrWhiteSpace(nombreCliente))
            {
                string nombre = Regex.Escape(nombreCliente.Trim());
                cuerpo = Regex.Replace(cuerpo, $@"^Hola\s+{nombre}\s*[,!]?\s*", string.Empty, RegexOptions.IgnoreCase);
                cuerpo = Regex.Replace(cuerpo, $@"^Estimado\s+{nombre}\s*[,!]?\s*", string.Empty, RegexOptions.IgnoreCase);
                cuerpo = Regex.Replace(cuerpo, $@"^Gracias\s+{nombre}\s*[,!]?\s*", string.Empty, RegexOptions.IgnoreCase);
                cuerpo = Regex.Replace(cuerpo, $@"^FELICIDADES\s+{nombre}\s*[,!]?\s*", string.Empty, RegexOptions.IgnoreCase);
            }

            cuerpo = cuerpo.Trim();
            return string.IsNullOrEmpty(cuerpo) ? "Notificacion de MFFITNESS." : cuerpo;
        }

        public static string Serializar(string variableKey, string mensaje)
        {
            string clave = string.IsNullOrWhiteSpace(variableKey) ? "1" : variableKey.Trim();
            string valor = Sanitizar(mensaje);

            if (valor.Length > 900)
            {
                valor = valor[..900].TrimEnd();
                if (!valor.EndsWith('.'))
                    valor += ".";
            }

            return JsonSerializer.Serialize(
                new System.Collections.Generic.Dictionary<string, string> { [clave] = valor },
                JsonOpciones);
        }

        /// <summary>
        /// Variables plantilla twilio/media (Meta-compliant):
        /// media = https://dominio/{{1}}
        /// body  = texto fijo con {{2}}..{{6}} (nunca empieza ni termina en variable).
        /// </summary>
        public static string SerializarFacturaMedia(
            string pathSuffixTrasDominio,
            string plan,
            string monto,
            string fechaPago,
            string fechaVence,
            string numeroRecibo)
        {
            string path = (pathSuffixTrasDominio ?? string.Empty).Trim().TrimStart('/');
            if (string.IsNullOrWhiteSpace(path))
                path = "factura_sample.pdf";

            return JsonSerializer.Serialize(
                new System.Collections.Generic.Dictionary<string, string>
                {
                    ["1"] = path,
                    ["2"] = Cortar(Sanitizar(plan), 80),
                    ["3"] = Cortar(Sanitizar(monto), 40),
                    ["4"] = Cortar(Sanitizar(fechaPago), 40),
                    ["5"] = Cortar(Sanitizar(fechaVence), 40),
                    ["6"] = Cortar(Sanitizar(numeroRecibo), 40)
                },
                JsonOpciones);
        }

        /// <summary>Compat: cuerpo libre → se usa como plan/detalle en {{2}}.</summary>
        public static string SerializarFacturaMedia(string pathSuffixTrasDominio, string cuerpo)
        {
            string detalle = Sanitizar(cuerpo);
            if (detalle.Length > 80)
                detalle = detalle[..80].TrimEnd() + ".";

            return SerializarFacturaMedia(
                pathSuffixTrasDominio,
                plan: detalle,
                monto: "Ver PDF",
                fechaPago: DateTime.Now.ToString("dd/MM/yyyy"),
                fechaVence: DateTime.Now.ToString("dd/MM/yyyy"),
                numeroRecibo: "MF-0");
        }

        private static string Cortar(string valor, int max)
        {
            if (string.IsNullOrEmpty(valor))
                return "N/D";
            if (valor.Length <= max)
                return valor;
            return valor[..max].TrimEnd() + ".";
        }
    }
}
