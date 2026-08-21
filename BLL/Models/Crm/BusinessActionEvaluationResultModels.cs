namespace BLL.Models.Crm
{
    /// <summary>Solicitud de evaluación de resultado (FASE 11.9).</summary>
    public sealed class BusinessActionEvaluateRequest
    {
        public Guid ActionId { get; init; }
        public DateTime? AsOfUtc { get; init; }
        public string? Actor { get; init; }
        public string? Notes { get; init; }

        /// <summary>Permite evaluar aunque la ventana aún no venció.</summary>
        public bool AllowBeforeWindowEnd { get; init; }

        /// <summary>Override manual (usuario). Null = clasificación automática.</summary>
        public BusinessActionOutcome? OverrideOutcome { get; init; }

        public BusinessActionConfidence? OverrideConfidence { get; init; }
    }

    /// <summary>Resultado de evaluación (sugerencia o persistida).</summary>
    public sealed class BusinessActionEvaluationResult
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public BusinessActionRecord? Record { get; init; }
        public BusinessActionOutcome Outcome { get; init; }
        public BusinessActionConfidence Confidence { get; init; }
        public string Summary { get; init; } = string.Empty;
        public int FavorableCount { get; init; }
        public int UnfavorableCount { get; init; }
        public int NeutralOrUnknownCount { get; init; }
        public bool UsedOverride { get; init; }
        public BusinessActionEvaluationWindow? Window { get; init; }
        /// <summary>Capital liberado / incrementos observados (FASE 11.10).</summary>
        public BusinessActionObservedCapitalImpact? CapitalImpact { get; init; }
    }
}
