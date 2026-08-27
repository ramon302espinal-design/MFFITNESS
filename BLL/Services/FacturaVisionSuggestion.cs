using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BLL.Services
{
    /// <summary>
    /// Sugerencia de gasto desde OCR de factura. Solo propuesta; no registra caja.
    /// Reglas: monto = TOTAL A PAGAR; concepto = comercio + líneas (desc/cant/precio/subtotal).
    /// </summary>
    public sealed class FacturaVisionSuggestion
    {
        public const int ConceptoMaxLength = 1000;

        public string? Concepto { get; set; }
        public decimal? Monto { get; set; }
        public string? RawResponse { get; set; }

        public bool EsUtil =>
            !string.IsNullOrWhiteSpace(Concepto) && Monto is > 0;

        public static FacturaVisionSuggestion? TryParse(string? jsonOrText)
        {
            if (string.IsNullOrWhiteSpace(jsonOrText))
                return null;

            string text = jsonOrText.Trim();
            int start = text.IndexOf('{');
            int end = text.LastIndexOf('}');
            if (start >= 0 && end > start)
                text = text[start..(end + 1)];

            try
            {
                using var doc = JsonDocument.Parse(text);
                var root = doc.RootElement;
                var s = new FacturaVisionSuggestion { RawResponse = jsonOrText };

                // Monto: SOLO total a pagar (nunca subtotal de línea).
                s.Monto = SanitizeMonto(
                    ReadDecimal(root, "monto", "total", "totalPagar", "totalAPagar", "importe", "amount"));

                string? conceptoDirecto = ReadString(
                    root, "concepto", "concept", "detalle", "resumen");
                string? comercio = ReadString(
                    root, "comercio", "proveedor", "empresa", "titulo", "nombreComercial", "razonSocial");

                string? compuesto = ComponerConceptoDesdeLineas(comercio, root);
                string? elegido = !string.IsNullOrWhiteSpace(conceptoDirecto)
                    ? conceptoDirecto
                    : compuesto;

                // Si el modelo dio concepto corto sin líneas, preferir compuesto si es más rico.
                if (!string.IsNullOrWhiteSpace(compuesto)
                    && (string.IsNullOrWhiteSpace(elegido) || compuesto.Length > elegido.Length + 20))
                {
                    elegido = compuesto;
                }

                s.Concepto = SanitizeConcepto(elegido);
                return s;
            }
            catch
            {
                return new FacturaVisionSuggestion { RawResponse = jsonOrText };
            }
        }

        private static string? ComponerConceptoDesdeLineas(string? comercio, JsonElement root)
        {
            if (!root.TryGetProperty("lineas", out JsonElement lineas)
                && !root.TryGetProperty("productos", out lineas)
                && !root.TryGetProperty("items", out lineas))
            {
                return string.IsNullOrWhiteSpace(comercio) ? null : comercio.Trim();
            }

            if (lineas.ValueKind != JsonValueKind.Array || lineas.GetArrayLength() == 0)
                return string.IsNullOrWhiteSpace(comercio) ? null : comercio.Trim();

            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(comercio))
                sb.Append(comercio.Trim());

            int n = 0;
            foreach (JsonElement item in lineas.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                    continue;

                string? desc = ReadString(item, "descripcion", "desc", "producto", "nombre", "detalle");
                if (string.IsNullOrWhiteSpace(desc))
                    continue;

                decimal? cant = ReadDecimal(item, "cantidad", "cant", "qty", "unidades");
                decimal? precio = ReadDecimal(item, "precio", "precioUnitario", "unitario", "punit");
                decimal? sub = ReadDecimal(item, "subtotal", "importe", "totalLinea", "monto");

                if (sb.Length > 0)
                    sb.AppendLine();

                sb.Append("- ").Append(desc.Trim());
                if (cant is > 0)
                    sb.Append(" x").Append(FormatNum(cant.Value));
                if (precio is > 0)
                    sb.Append(" @ ").Append(FormatNum(precio.Value));
                if (sub is > 0)
                    sb.Append(" = ").Append(FormatNum(sub.Value));

                n++;
                if (n >= 25 || sb.Length >= ConceptoMaxLength - 40)
                    break;
            }

            return sb.Length == 0 ? null : sb.ToString();
        }

        private static string FormatNum(decimal v) =>
            v.ToString("0.##", CultureInfo.InvariantCulture);

        /// <summary>Conserva saltos de línea; limita longitud para DetalleCaja.Concepto.</summary>
        public static string? SanitizeConcepto(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            string cleaned = raw.Replace("\r\n", "\n").Replace('\r', '\n').Trim();
            cleaned = Regex.Replace(cleaned, @"[ \t]+", " ");
            cleaned = Regex.Replace(cleaned, @"\n{3,}", "\n\n");
            if (cleaned.Length == 0)
                return null;
            if (cleaned.Length > ConceptoMaxLength)
                cleaned = cleaned[..ConceptoMaxLength].TrimEnd();
            return cleaned;
        }

        public static decimal? SanitizeMonto(decimal? raw)
        {
            if (raw is null)
                return null;

            decimal m = Math.Round(raw.Value, 2, MidpointRounding.AwayFromZero);
            if (m <= 0 || m > 10_000_000m)
                return null;
            return m;
        }

        private static string? ReadString(JsonElement root, params string[] names)
        {
            foreach (string name in names)
            {
                if (!root.TryGetProperty(name, out JsonElement el))
                    continue;

                if (el.ValueKind == JsonValueKind.String)
                {
                    string? v = el.GetString();
                    if (!string.IsNullOrWhiteSpace(v))
                        return v.Trim();
                }
            }
            return null;
        }

        private static decimal? ReadDecimal(JsonElement root, params string[] names)
        {
            foreach (string name in names)
            {
                if (!root.TryGetProperty(name, out JsonElement el))
                    continue;

                if (el.ValueKind == JsonValueKind.Number && el.TryGetDecimal(out decimal d))
                    return d;

                if (el.ValueKind == JsonValueKind.String)
                {
                    string? s = el.GetString();
                    if (string.IsNullOrWhiteSpace(s))
                        continue;

                    s = s.Replace("RD$", "", StringComparison.OrdinalIgnoreCase)
                        .Replace("$", "")
                        .Replace(" ", "")
                        .Trim();

                    if (decimal.TryParse(
                            s.Replace(',', '.'),
                            NumberStyles.Any,
                            CultureInfo.InvariantCulture,
                            out decimal parsed))
                        return parsed;

                    if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.GetCultureInfo("es-DO"), out parsed))
                        return parsed;
                }
            }
            return null;
        }
    }
}
