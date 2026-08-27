using System.Text.Json;

namespace BLL.Services
{
    public sealed class ProductoVisionSuggestion
    {
        public string? Nombre { get; set; }
        public string? Categoria { get; set; }
        public string? Descripcion { get; set; }
        public decimal? PrecioCompraEstimado { get; set; }
        public decimal? PrecioVentaEstimado { get; set; }
        public string? RawResponse { get; set; }
        public string? CodigoBarra { get; set; }

        public static string? TryParseCodigoBarra(string? jsonOrText)
        {
            ProductoVisionSuggestion? s = TryParse(jsonOrText);
            if (s == null)
                return null;

            if (!string.IsNullOrWhiteSpace(s.CodigoBarra))
                return s.CodigoBarra;

            return null;
        }

        public static ProductoVisionSuggestion? TryParse(string? jsonOrText)
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
                var s = new ProductoVisionSuggestion { RawResponse = jsonOrText };

                s.Nombre = CleanNombreEmpaque(ReadString(root, "nombre", "name", "producto", "product"));
                s.Categoria = ReadString(root, "categoria", "category", "categoriaSugerida");
                s.Descripcion = ReadString(root, "descripcion", "description");
                s.CodigoBarra = CleanCodigoBarra(ReadString(root, "codigoBarra", "codigo", "barcode", "ean", "upc"));
                s.PrecioCompraEstimado = ReadDecimal(root, "precioCompra", "precioCompraEstimado", "cost");
                s.PrecioVentaEstimado = ReadDecimal(root, "precioVenta", "precioVentaEstimado", "price");
                return s;
            }
            catch
            {
                return new ProductoVisionSuggestion
                {
                    Nombre = TruncateLine(jsonOrText),
                    RawResponse = jsonOrText
                };
            }
        }

        /// <summary>Normaliza espacios y quita ruido típico de empaque (conteos, QR).</summary>
        private static string? CleanNombreEmpaque(string? nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return null;

            string cleaned = System.Text.RegularExpressions.Regex.Replace(nombre.Trim(), @"\s+", " ");
            cleaned = System.Text.RegularExpressions.Regex.Replace(
                cleaned,
                @"\b\d+\s*(bars?|unidades?|uds?|pcs?|pack)\b",
                "",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            cleaned = System.Text.RegularExpressions.Regex.Replace(
                cleaned,
                @"\b(box\s*tops?|qr(\s*code)?)\b",
                "",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"\s{2,}", " ").Trim(' ', '-', ',');
            return cleaned.Length == 0 ? null : cleaned;
        }

        private static string? CleanCodigoBarra(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            string digits = new string(raw.Where(char.IsDigit).ToArray());
            return digits.Length >= 4 ? digits : raw.Trim();
        }

        private static string? ReadString(JsonElement root, params string[] names)
        {
            foreach (string name in names)
            {
                if (root.TryGetProperty(name, out JsonElement el)
                    && el.ValueKind == JsonValueKind.String)
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

                if (el.ValueKind == JsonValueKind.String
                    && decimal.TryParse(
                        el.GetString()?.Replace(',', '.'),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out decimal parsed))
                    return parsed;
            }
            return null;
        }

        private static string TruncateLine(string s)
        {
            s = s.Replace('\r', ' ').Replace('\n', ' ').Trim();
            return s.Length <= 80 ? s : s[..80];
        }
    }
}
