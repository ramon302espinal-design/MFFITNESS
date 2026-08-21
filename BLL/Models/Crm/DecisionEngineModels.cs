namespace BLL.Models.Crm
{
    /// <summary>
    /// Candidato crudo de una regla (FASE 10.8). Aún no es DecisionEvent.
    /// </summary>
    public sealed class DecisionRuleCandidate
    {
        public string RuleId { get; init; } = string.Empty;
        public string EventType { get; init; } = string.Empty;
        public DecisionEventArea Area { get; init; }
        public DecisionEntityType EntityType { get; init; } = DecisionEntityType.Portfolio;
        public string? EntityId { get; init; }
        public string EntityName { get; init; } = string.Empty;
        public string? PeriodKey { get; init; }

        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Reason { get; init; } = string.Empty;
        public string Impact { get; init; } = string.Empty;
        public string Recommendation { get; init; } = string.Empty;
        public string Source { get; init; } = string.Empty;

        public DecisionMaterialityInput Materiality { get; init; } = new();
        public DecisionImpactAssessment ImpactAssessment { get; init; } = new();

        /// <summary>Urgencia/flags; Severity se rellena tras materialidad+impacto.</summary>
        public DecisionUrgencyLevel Urgency { get; init; } = DecisionUrgencyLevel.None;
        public bool RequiresImmediateReview { get; init; }
        public bool TimeSensitiveStockout { get; init; }
        public bool OpportunityWindow { get; init; }
        public bool ProductStillSelling { get; init; }

        public IReadOnlyList<DecisionEvidenceFact> Evidence { get; init; }
            = Array.Empty<DecisionEvidenceFact>();

        public IReadOnlyList<string> MetricKeys { get; init; }
            = Array.Empty<string>();
    }

    /// <summary>Contexto de evaluación del motor (sin I/O propio).</summary>
    public sealed class DecisionRuleContext
    {
        public ProfitPeriodKind PeriodKind { get; init; } = ProfitPeriodKind.Last30Days;
        public DateTime? AsOf { get; init; }
        public string PeriodKey { get; init; } = string.Empty;

        /// <summary>
        /// Bag opcional para reglas/tests (ej. variation pct inyectada).
        /// El motor base no consulta DB — reglas de dominio (10.9+) cargan SSOT.
        /// </summary>
        public IReadOnlyDictionary<string, object?> Bag { get; init; }
            = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// FASE 10.27 — métricas agregadas precargadas (una vez por run).
        /// Si está presente, las reglas built-in no vuelven a consultar SSOT.
        /// </summary>
        public DecisionAnalyticsBundle? Analytics { get; init; }
    }

    /// <summary>Resultado del pipeline del Decision Engine.</summary>
    public sealed class DecisionEngineReport
    {
        public IReadOnlyList<DecisionEvent> Events { get; init; }
            = Array.Empty<DecisionEvent>();

        /// <summary>Eventos agrupados (FASE 10.18).</summary>
        public IReadOnlyList<DecisionGroup> Groups { get; init; }
            = Array.Empty<DecisionGroup>();

        /// <summary>Recomendaciones estructuradas (FASE 10.19).</summary>
        public IReadOnlyList<DecisionRecommendation> Recommendations { get; init; }
            = Array.Empty<DecisionRecommendation>();

        public int CandidatesConsidered { get; init; }
        public int EmittedCount { get; init; }
        public int SuppressedByMateriality { get; init; }
        public int SuppressedByDuplicate { get; init; }
        public int GroupCount => Groups.Count;
        public int RecommendationCount => Recommendations.Count;

        public string PolicyNote { get; init; } = string.Empty;

        /// <summary>Primera por prioridad (cola "qué revisar primero").</summary>
        public DecisionEvent? Primary => Events.Count > 0 ? Events[0] : null;

        /// <summary>Grupo líder (si hay Groups).</summary>
        public DecisionGroup? PrimaryGroup => Groups.Count > 0 ? Groups[0] : null;

        /// <summary>Recomendación líder (grupo primario o primera).</summary>
        public DecisionRecommendation? PrimaryRecommendation
            => Recommendations.Count > 0 ? Recommendations[0] : null;
    }
}
