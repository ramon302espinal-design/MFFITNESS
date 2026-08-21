namespace BLL.Models.Crm
{
    /// <summary>Tipo de hito en timeline decisión→acción→resultado (FASE 11.11).</summary>
    public enum BusinessActionTimelineStepKind
    {
        Unspecified = 0,
        DecisionDetected = 1,
        DecisionResolved = 2,
        ActionRegistered = 3,
        ActionStarted = 4,
        BaselineCaptured = 5,
        ActionCompleted = 6,
        EvaluationWindowReady = 7,
        PostMetricsCaptured = 8,
        OutcomeEvaluated = 9,
        CapitalImpactNoted = 10,
        ActionCancelled = 11,
        ActionNoResult = 12
    }

    /// <summary>Hito ordenado por fecha.</summary>
    public sealed class BusinessActionTimelineStep
    {
        public BusinessActionTimelineStepKind Kind { get; init; }
        public DateTime AtUtc { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Detail { get; init; } = string.Empty;
        public string? Actor { get; init; }
    }

    /// <summary>Timeline completa de un ciclo (FASE 11.11).</summary>
    public sealed class BusinessActionTimeline
    {
        public Guid ActionId { get; init; }
        public Guid? DecisionEventId { get; init; }
        public long? DecisionHistoryId { get; init; }
        public string ActionDescription { get; init; } = string.Empty;
        public BusinessActionType ActionType { get; init; }
        public BusinessActionStatus ActionStatus { get; init; }
        public BusinessActionOutcome? Outcome { get; init; }
        public DateTime? SpanFrom { get; init; }
        public DateTime? SpanTo { get; init; }
        public IReadOnlyList<BusinessActionTimelineStep> Steps { get; init; }
            = Array.Empty<BusinessActionTimelineStep>();
        public string SummaryLabel { get; init; } = string.Empty;
    }

    /// <summary>Stats de carga timeline (FASE 11.21 — sin N consultas por acción).</summary>
    public sealed class BusinessActionTimelineLoadStats
    {
        public int ActionStoreCalls { get; init; }
        public int DecisionStoreCalls { get; init; }
        public int ActionsLoaded { get; init; }
        public int DecisionsPrefetched { get; init; }
        public long ElapsedMs { get; init; }
        public string PolicyNote { get; init; } = string.Empty;
    }

    /// <summary>Lote de timelines + métricas de consulta (FASE 11.21).</summary>
    public sealed class BusinessActionTimelineBatch
    {
        public IReadOnlyList<BusinessActionTimeline> Items { get; init; }
            = Array.Empty<BusinessActionTimeline>();
        public BusinessActionTimelineLoadStats Stats { get; init; } = new();
    }
}
