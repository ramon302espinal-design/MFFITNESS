namespace BLL.Models.Crm
{
    /// <summary>Cubo visual del Centro (FASE 10.20 / brief §103·§112).</summary>
    public enum DecisionCenterBucket
    {
        Critical = 1,
        Important = 2,
        Review = 3,
        Opportunity = 4
    }

    /// <summary>Resumen ejecutivo del día.</summary>
    public sealed class DecisionCenterSummary
    {
        public int CriticalCount { get; init; }
        public int ImportantCount { get; init; }
        public int ReviewCount { get; init; }
        public int OpportunityCount { get; init; }

        public int TotalGroups { get; init; }
        public int TotalEvents { get; init; }
        public int TotalRecommendations { get; init; }

        /// <summary>Ej. "HOY · 2 críticos · 4 importantes · 3 oportunidades".</summary>
        public string Headline { get; init; } = string.Empty;

        /// <summary>Líneas KPI opcionales (ventas/ganancia/capital) — sin garantías.</summary>
        public IReadOnlyList<string> SnapshotLines { get; init; }
            = Array.Empty<string>();
    }

    /// <summary>Ítem de "PRIORIDADES DE HOY" (brief §104).</summary>
    public sealed class DecisionCenterPriorityItem
    {
        public int Rank { get; init; }
        public DecisionCenterBucket Bucket { get; init; }

        public string Title { get; init; } = string.Empty;
        public string WhyItMatters { get; init; } = string.Empty;
        public string Recommendation { get; init; } = string.Empty;

        public DecisionPriority Priority { get; init; }
        public DecisionSeverity Severity { get; init; }

        public string? GroupId { get; init; }
        public Guid? PrimaryEventId { get; init; }
        public string? EntityName { get; init; }
        public DecisionEntityType EntityType { get; init; }

        public IReadOnlyList<string> SubSignals { get; init; }
            = Array.Empty<string>();

        public bool IsOpportunity { get; init; }
    }

    /// <summary>
    /// Snapshot opcional para enriquecer el resumen (sin I/O en el composer).
    /// </summary>
    public sealed class DecisionCenterSnapshot
    {
        public decimal? SalesVariationPct { get; init; }
        public decimal? ProfitVariationPct { get; init; }
        public decimal? FrozenCapitalAmount { get; init; }
        public string? FrozenCapitalLabel { get; init; }
    }

    /// <summary>Reporte del Centro de decisiones (FASE 10.20).</summary>
    public sealed class DecisionCenterReport
    {
        public DateTime GeneratedAt { get; init; }
        public string PeriodKey { get; init; } = string.Empty;

        public DecisionCenterSummary Summary { get; init; } = new();

        /// <summary>Cola corta: qué revisar primero (máx. configurado).</summary>
        public IReadOnlyList<DecisionCenterPriorityItem> PrioritiesToday { get; init; }
            = Array.Empty<DecisionCenterPriorityItem>();

        public IReadOnlyList<DecisionGroup> Groups { get; init; }
            = Array.Empty<DecisionGroup>();

        public IReadOnlyList<DecisionRecommendation> Recommendations { get; init; }
            = Array.Empty<DecisionRecommendation>();

        /// <summary>Reporte crudo del motor (trazabilidad).</summary>
        public DecisionEngineReport? Engine { get; init; }

        public DecisionCenterPriorityItem? TopPriority
            => PrioritiesToday.Count > 0 ? PrioritiesToday[0] : null;

        public string PolicyNote { get; init; } = string.Empty;
    }
}
