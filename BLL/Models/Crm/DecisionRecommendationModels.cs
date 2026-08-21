namespace BLL.Models.Crm
{
    /// <summary>Verbo suave permitido (FASE 10.19 / brief §106).</summary>
    public enum DecisionRecommendationVerb
    {
        Revisar = 1,
        Evaluar = 2,
        Considerar = 3,
        Analizar = 4
    }

    /// <summary>
    /// Recomendación estructurada. Detecta/analiza — NUNCA ejecuta.
    /// </summary>
    public sealed class DecisionRecommendation
    {
        public string RecommendationId { get; init; } = string.Empty;

        /// <summary>EventId del evento (si aplica).</summary>
        public Guid? EventId { get; init; }

        /// <summary>GroupId del grupo (si aplica).</summary>
        public string? GroupId { get; init; }

        public string? EventType { get; init; }
        public DecisionRecommendationVerb Verb { get; init; }

        /// <summary>Título corto para UI / cola de prioridades.</summary>
        public string Headline { get; init; } = string.Empty;

        /// <summary>Texto suave completo (Revisar/Evaluar/…).</summary>
        public string Body { get; init; } = string.Empty;

        /// <summary>Chequeos sugeridos (no acciones irreversibles).</summary>
        public IReadOnlyList<string> SuggestedChecks { get; init; }
            = Array.Empty<string>();

        /// <summary>Recordatorios de política (no auto-compra, etc.).</summary>
        public IReadOnlyList<string> PolicyReminders { get; init; }
            = Array.Empty<string>();

        public bool IsOpportunity { get; init; }

        /// <summary>true si Body pasa el SoftLanguageGuard.</summary>
        public bool SoftLanguageCompliant { get; init; }

        /// <summary>Texto listo para binder / DecisionEvent.Recommendation.</summary>
        public string DisplayText =>
            string.IsNullOrWhiteSpace(Body) ? Headline : Body;
    }
}
