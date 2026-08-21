namespace BLL.Models.Crm
{
    /// <summary>Agregados de aprendizaje por tipo de acción (FASE 11.13).</summary>
    public sealed class BusinessActionTypeLearningStats
    {
        public BusinessActionType ActionType { get; init; }
        public string DisplayName { get; init; } = string.Empty;

        /// <summary>Total de registros del tipo (cualquier estado).</summary>
        public int TotalCount { get; init; }
        public int PendingCount { get; init; }
        public int InProgressCount { get; init; }
        public int CompletedCount { get; init; }
        public int CancelledCount { get; init; }
        public int NoResultCount { get; init; }

        /// <summary>Completadas con Outcome Exitosa/Parcial/No efectiva.</summary>
        public int ClassifiedCount { get; init; }
        public int SuccessfulCount { get; init; }
        public int PartialCount { get; init; }
        public int IneffectiveCount { get; init; }
        public int InsufficientDataCount { get; init; }
        public int UnspecifiedOutcomeCount { get; init; }

        /// <summary>Successful / Classified (null si Classified=0).</summary>
        public decimal? SuccessRatePct { get; init; }
        /// <summary>Partial / Classified.</summary>
        public decimal? PartialRatePct { get; init; }
        /// <summary>Ineffective / Classified.</summary>
        public decimal? FailureRatePct { get; init; }

        /// <summary>Resumen suave; sin causalidad ni garantía futura.</summary>
        public string Summary { get; init; } = string.Empty;
    }

    /// <summary>Resumen de aprendizaje del negocio (FASE 11.13 — por tipo).</summary>
    public sealed class BusinessActionLearningSummary
    {
        public DateTime? FromUtc { get; init; }
        public DateTime? ToUtc { get; init; }
        public DateTime GeneratedAtUtc { get; init; }

        public int TotalActions { get; init; }
        public int ClassifiedActions { get; init; }
        public decimal? OverallSuccessRatePct { get; init; }
        public decimal? OverallPartialRatePct { get; init; }
        public decimal? OverallFailureRatePct { get; init; }

        public IReadOnlyList<BusinessActionTypeLearningStats> ByType { get; init; }
            = Array.Empty<BusinessActionTypeLearningStats>();

        public string Caution { get; init; } = string.Empty;
        public string Narrative { get; init; } = string.Empty;
    }

    public sealed class BusinessActionLearningQuery
    {
        public DateTime? FromUtc { get; init; }
        public DateTime? ToUtc { get; init; }
        public BusinessActionType? ActionType { get; init; }
        /// <summary>Máximo de acciones a leer del store (cap defensivo).</summary>
        public int Top { get; init; } = 500;
    }
}
