using System.Text.RegularExpressions;
using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato de recomendaciones (FASE 10.19 / brief §106).</summary>
    public static class DecisionRecommendationPolicy
    {
        public const string Definition =
            "FASE 10.19: DecisionRecommendationComposer produce recomendaciones estructuradas. " +
            "DETECTA / ANALIZA / RECOMIENDA — el usuario DECIDE. Sin auto-compra ni mutación.";

        public const string SoftLanguage =
            "Verbos permitidos: Revisar, Evaluar, Considerar, Analizar. " +
            "Sin garantías ni órdenes irreversibles.";

        public const string Deferred =
            "FASE 10 completa.";
    }

    /// <summary>
    /// Guard de lenguaje suave. Rechaza órdenes irreversibles / garantías.
    /// </summary>
    public static class DecisionSoftLanguageGuard
    {
        private static readonly string[] SoftVerbs =
        [
            "revisar", "evaluar", "considerar", "analizar"
        ];

        private static readonly Regex Forbidden = new(
            @"auto[\s\-]?compr|" +
            @"comprar\s+autom|" +
            @"liquidar\s+autom|" +
            @"debe\s+compr|" +
            @"ejecutar\s+compra|" +
            @"orden(ar|e)\s+compra|" +
            @"garantiz|" +
            @"vas\s+a\s+perder|" +
            @"perder[áa]\s+seguro|" +
            @"certeza|" +
            @"probabilidad\s+\d",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public static bool StartsWithSoftVerb(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;
            string t = text.Trim();
            foreach (string v in SoftVerbs)
            {
                if (t.StartsWith(v, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        public static bool ContainsForbidden(string? text)
            => !string.IsNullOrWhiteSpace(text) && Forbidden.IsMatch(text);

        public static bool IsCompliant(string? text)
            => !string.IsNullOrWhiteSpace(text)
               && StartsWithSoftVerb(text)
               && !ContainsForbidden(text);

        public static DecisionRecommendationVerb DetectVerb(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return DecisionRecommendationVerb.Revisar;

            string t = text.Trim();
            if (t.StartsWith("evaluar", StringComparison.OrdinalIgnoreCase))
                return DecisionRecommendationVerb.Evaluar;
            if (t.StartsWith("considerar", StringComparison.OrdinalIgnoreCase))
                return DecisionRecommendationVerb.Considerar;
            if (t.StartsWith("analizar", StringComparison.OrdinalIgnoreCase))
                return DecisionRecommendationVerb.Analizar;
            return DecisionRecommendationVerb.Revisar;
        }

        public static string VerbText(DecisionRecommendationVerb verb) => verb switch
        {
            DecisionRecommendationVerb.Evaluar => "Evaluar",
            DecisionRecommendationVerb.Considerar => "Considerar",
            DecisionRecommendationVerb.Analizar => "Analizar",
            _ => "Revisar"
        };

        /// <summary>
        /// Normaliza texto: quita prohibidos y asegura verbo suave al inicio.
        /// </summary>
        public static string Ensure(
            string? text,
            DecisionRecommendationVerb preferred = DecisionRecommendationVerb.Revisar,
            string? fallbackBody = null)
        {
            string body = string.IsNullOrWhiteSpace(text)
                ? (fallbackBody ?? string.Empty)
                : text.Trim();

            if (ContainsForbidden(body))
            {
                // Reescribir conservando intención suave
                body = Forbidden.Replace(body, "revisar manualmente");
                body = Regex.Replace(body, @"\s{2,}", " ").Trim(' ', '.', ',');
            }

            if (string.IsNullOrWhiteSpace(body))
                body = VerbText(preferred) + " la señal detectada antes de decidir.";

            if (!StartsWithSoftVerb(body))
                body = VerbText(preferred) + ": " + body.TrimStart(':', ' ', '-');

            return body.Trim();
        }
    }
}
