namespace BLL.Models.Crm
{
    /// <summary>Señal de aprendizaje contextual (FASE 11.14 / brief §52–54).</summary>
    public enum BusinessActionLearningSignalKind
    {
        /// <summary>⚠️ PROBLEMA RECURRENTE</summary>
        RecurrentProblem = 1,
        /// <summary>⚠️ ACCIÓN POCO EFECTIVA (patrón histórico)</summary>
        IneffectiveActionPattern = 2,
        /// <summary>🟢 ACCIÓN HISTÓRICAMENTE EFECTIVA</summary>
        HistoricallyEffectiveAction = 3
    }

    /// <summary>Agregados por entidad (producto / categoría / …).</summary>
    public sealed class BusinessActionEntityLearningStats
    {
        public DecisionEntityType EntityType { get; init; }
        public string? EntityId { get; init; }
        public string EntityName { get; init; } = string.Empty;

        public int TotalCount { get; init; }
        public int ClassifiedCount { get; init; }
        public int SuccessfulCount { get; init; }
        public int PartialCount { get; init; }
        public int IneffectiveCount { get; init; }

        public decimal? SuccessRatePct { get; init; }
        public decimal? PartialRatePct { get; init; }
        public decimal? FailureRatePct { get; init; }

        public IReadOnlyList<BusinessActionTypeLearningStats> ByActionType { get; init; }
            = Array.Empty<BusinessActionTypeLearningStats>();

        public string Summary { get; init; } = string.Empty;
    }

    /// <summary>Agregados por problema (EventType de decisión o Area).</summary>
    public sealed class BusinessActionProblemLearningStats
    {
        /// <summary>Clave estable: EventType o "area:Capital".</summary>
        public string ProblemKey { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public DecisionEventArea? Area { get; init; }

        public int LinkedActionCount { get; init; }
        public int DistinctDecisionCount { get; init; }
        public int ClassifiedCount { get; init; }
        public int SuccessfulCount { get; init; }
        public int PartialCount { get; init; }
        public int IneffectiveCount { get; init; }

        public decimal? SuccessRatePct { get; init; }

        public IReadOnlyList<BusinessActionTypeLearningStats> ByActionType { get; init; }
            = Array.Empty<BusinessActionTypeLearningStats>();

        /// <summary>Tipo con mejor tasa histórica (≥2 clasificadas); null si no aplica.</summary>
        public BusinessActionType? BestHistoricalActionType { get; init; }
        public string? BestHistoricalHint { get; init; }

        public string Summary { get; init; } = string.Empty;
    }

    public sealed class BusinessActionLearningSignal
    {
        public BusinessActionLearningSignalKind Kind { get; init; }
        public string Key { get; init; } = string.Empty;
        public string Label { get; init; } = string.Empty;
        public int OccurrenceCount { get; init; }
        public int? ClassifiedCount { get; init; }
        public decimal? RatePct { get; init; }
        /// <summary>Mensaje suave; nunca "funcionará".</summary>
        public string Message { get; init; } = string.Empty;
    }

    /// <summary>Learning contextual: entidad + problema + señales (FASE 11.14).</summary>
    public sealed class BusinessActionContextualLearning
    {
        public DateTime? FromUtc { get; init; }
        public DateTime? ToUtc { get; init; }
        public DateTime GeneratedAtUtc { get; init; }

        public IReadOnlyList<BusinessActionEntityLearningStats> ByEntity { get; init; }
            = Array.Empty<BusinessActionEntityLearningStats>();

        public IReadOnlyList<BusinessActionProblemLearningStats> ByProblem { get; init; }
            = Array.Empty<BusinessActionProblemLearningStats>();

        public IReadOnlyList<BusinessActionLearningSignal> Signals { get; init; }
            = Array.Empty<BusinessActionLearningSignal>();

        public string Caution { get; init; } = string.Empty;
        public string Narrative { get; init; } = string.Empty;
    }
}
