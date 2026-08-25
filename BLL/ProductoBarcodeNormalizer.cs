using System.Linq;
using System.Text;

namespace BLL
{
    /// <summary>
    /// Normaliza y valida lecturas de escáner HID (teclado).
    /// Rechaza QR/URL y basura de teclado ES/US mal configurado.
    /// </summary>
    public static class ProductoBarcodeNormalizer
    {
        public static string? Normalize(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            var sb = new StringBuilder(raw.Length);
            foreach (char c in raw.Trim())
            {
                if (char.IsControl(c) || char.IsWhiteSpace(c))
                    continue;
                sb.Append(c);
            }

            string code = sb.ToString();
            return code.Length == 0 ? null : code;
        }

        /// <summary>
        /// True mientras el usuario escanea (evita filtrar la lista POS en cada tecla).
        /// </summary>
        public static bool LooksLikeBarcodeInProgress(string? raw)
        {
            string? normalized = Normalize(raw);
            if (normalized == null)
                return false;

            if (IsRejectedContent(normalized))
                return true;

            return IsNumericBarcode(normalized) || IsAlphanumericBarcode(normalized);
        }

        /// <summary>
        /// Solo acepta EAN/UPC numérico (4–14 dígitos) o código interno alfanumérico corto.
        /// </summary>
        public static bool TryNormalizeBarcode(string? raw, out string? code)
        {
            code = null;
            string? normalized = Normalize(raw);
            if (normalized == null)
                return false;

            if (IsRejectedContent(normalized))
                return false;

            if (IsNumericBarcode(normalized))
            {
                code = normalized;
                return true;
            }

            if (IsAlphanumericBarcode(normalized))
            {
                code = normalized.ToUpperInvariant();
                return true;
            }

            return false;
        }

        private static bool IsRejectedContent(string s)
        {
            string lower = s.ToLowerInvariant();

            // QR / URL (típico cuando el lector lee QR o teclado ES recibe wedge US: httpñ...)
            if (lower.Contains("http") || lower.Contains("www") || lower.Contains("ftp"))
                return true;
            if (lower.Contains(".com") || lower.Contains(".net") || lower.Contains(".org"))
                return true;
            if (lower.Contains(".to") || lower.Contains(".io") || lower.Contains("://"))
                return true;

            if (s.Length < 4 || s.Length > 32)
                return true;

            int simbolosRaros = s.Count(c =>
                !char.IsLetterOrDigit(c) && c != '-' && c != '.');

            // EAN puro no lleva símbolos; muchos = QR/URL corrupto
            if (simbolosRaros >= 2)
                return true;

            return false;
        }

        private static bool IsNumericBarcode(string s) =>
            s.Length >= 4 && s.Length <= 14 && s.All(char.IsDigit);

        private static bool IsAlphanumericBarcode(string s) =>
            s.Length >= 4 && s.Length <= 32
            && s.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '.');
    }
}
