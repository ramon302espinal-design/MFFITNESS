using System.Text.RegularExpressions;

namespace BLL.Services.Crm
{
    /// <summary>Contrato soft language / causalidad acciones (FASE 11.23 / brief §86).</summary>
    public static class BusinessActionSoftLanguagePolicy
    {
        public const string Definition =
            "FASE 11.23: no afirmar causalidad automática. " +
            "Preferir 'Después de…' / 'Durante el período posterior…' / 'Se observó…'. " +
            "Histórico ≠ garantía. Sin ML (§87).";

        public const string AllowedOpeners =
            "Después de · Durante el período · Se observó · Clasificación sugerida · Información histórica";

        public const string Forbidden =
            "causó · la acción liberó/generó/produjo · garantizamos · certeza · funcionará (como promesa)";

        public const string Deferred =
            "FASE 11 completa.";
    }

    /// <summary>
    /// Guard de lenguaje suave para resultados/aprendizaje de acciones (FASE 11.23).
    /// Distinto de <see cref="DecisionSoftLanguageGuard"/> (recomendaciones FASE 10).
    /// </summary>
    public static class BusinessActionSoftLanguageGuard
    {
        private static readonly string[] SoftOpeners =
        [
            "después de",
            "durante el período",
            "durante el periodo",
            "se observó",
            "se observaron",
            "clasificación sugerida",
            "clasificación:",
            "información histórica",
            "las acciones de tipo",
            "sin datos",
            "sin deltas",
            "no se observó",
            "no se atribuye",
            "problema",
            "entre tipos",
            "revisar",
            "evaluar",
            "considerar"
        ];

        private static readonly Regex Forbidden = new(
            @"\bcaus[oó]|" +
            @"la\s+acci[oó]n\s+(liber[oó]|gener[oó]|produjo|provoc[oó]|origin[oó])|" +
            @"gracias\s+a\s+la\s+acci|" +
            @"debido\s+a\s+la\s+acci|" +
            @"garantizamos|" +
            @"garantizado\s+que|" +
            @"con\s+certeza|" +
            @"certeza\s+de|" +
            @"asegura(mos|rá|ra)\s+resultado|" +
            @"auto[\s\-]?ejecut",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public static bool StartsWithSoftOpener(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;
            string t = text.Trim();
            foreach (string o in SoftOpeners)
            {
                if (t.StartsWith(o, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        public static bool ContainsForbidden(string? text)
            => !string.IsNullOrWhiteSpace(text) && Forbidden.IsMatch(text);

        public static bool IsCompliant(string? text)
            => !string.IsNullOrWhiteSpace(text)
               && !ContainsForbidden(text)
               && StartsWithSoftOpener(text);

        /// <summary>
        /// Normaliza narrativa observada: quita frases causales y antepone opener soft si falta.
        /// </summary>
        public static string EnsureObserved(string? text, string? fallback = null)
        {
            string body = string.IsNullOrWhiteSpace(text)
                ? (fallback ?? string.Empty)
                : text.Trim();

            if (ContainsForbidden(body))
            {
                body = Forbidden.Replace(body, "se observó un cambio");
                body = Regex.Replace(
                    body,
                    @"\b(funcionará|funcionara)\b",
                    "se repetirá necesariamente",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                body = Regex.Replace(body, @"\s{2,}", " ").Trim(' ', '.', ',');
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                return fallback
                    ?? "Durante el período posterior se observaron cambios; no se atribuye causalidad automática.";
            }

            if (!StartsWithSoftOpener(body))
                body = "Se observó: " + body.TrimStart(':', ' ', '-');

            // Segunda pasada por si el rewrite reintrodujo algo
            if (ContainsForbidden(body))
                body = "Durante el período posterior se observaron cambios. No se atribuye causalidad automática.";

            return body.Trim();
        }

        /// <summary>Para hints históricos: nunca promesa de futuro.</summary>
        public static string EnsureHistoricalHint(string? text)
        {
            string body = EnsureObserved(
                text,
                "Información histórica; no es una garantía futura.");

            if (body.Contains("funcionará", StringComparison.OrdinalIgnoreCase)
                || body.Contains("funcionara", StringComparison.OrdinalIgnoreCase))
            {
                body = Regex.Replace(
                    body,
                    @"\bno\s+es\s+una\s+garant[ií]a\s+de\s+que\s+funcionará\.?",
                    "no es una garantía de resultados futuros.",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                body = Regex.Replace(
                    body,
                    @"\bfuncionará\b|\bfuncionara\b",
                    "se repita",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            }

            if (!body.Contains("garantía", StringComparison.OrdinalIgnoreCase)
                && !body.Contains("garantia", StringComparison.OrdinalIgnoreCase))
            {
                body = body.TrimEnd('.') + ". Información histórica; no es una garantía futura.";
            }

            return body.Trim();
        }
    }
}
