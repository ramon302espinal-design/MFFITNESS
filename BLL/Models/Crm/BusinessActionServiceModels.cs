namespace BLL.Models.Crm
{
    /// <summary>Solicitud de registro de acción (FASE 11.5 / TEST 1–2).</summary>
    public sealed class BusinessActionRegisterRequest
    {
        public BusinessActionType ActionType { get; init; }
        public string Description { get; init; } = string.Empty;
        public DecisionEventArea Area { get; init; } = DecisionEventArea.Operations;
        public DecisionEntityType EntityType { get; init; } = DecisionEntityType.Portfolio;
        public string? EntityId { get; init; }
        public string? EntityName { get; init; }
        public Guid? DecisionEventId { get; init; }
        public long? DecisionHistoryId { get; init; }
        public string? Reason { get; init; }
        public string? Notes { get; init; }
        public decimal? QuantityInvolved { get; init; }
        public decimal? CapitalInvolved { get; init; }
        public string? CreatedBy { get; init; }
        public BusinessActionExpectedImpact? ExpectedImpact { get; init; }
        public int? EvaluationDays { get; init; } = 14;
        public DateTime? CreatedAt { get; init; }
        /// <summary>Si true, queda EN PROCESO al registrar.</summary>
        public bool StartImmediately { get; init; }

        /// <summary>Baseline pre-capturado (FASE 11.6). Alternativa: CaptureBaseline*.</summary>
        public BusinessActionBaseline? Baseline { get; init; }

        /// <summary>Si true, captura baseline desde Analytics o MetricValues al registrar.</summary>
        public bool CaptureBaseline { get; init; }

        public DecisionAnalyticsBundle? Analytics { get; init; }

        public IReadOnlyDictionary<string, decimal?>? MetricValues { get; init; }

        public ProfitPeriodKind? BaselinePeriodKind { get; init; }
    }

    /// <summary>Solicitud de captura/attach de baseline (FASE 11.6).</summary>
    public sealed class BusinessActionBaselineRequest
    {
        public Guid ActionId { get; init; }
        public BusinessActionBaseline? Baseline { get; init; }
        public DecisionAnalyticsBundle? Analytics { get; init; }
        public IReadOnlyDictionary<string, decimal?>? MetricValues { get; init; }
        public ProfitPeriodKind? PeriodKind { get; init; }
        public DateTime? CapturedAt { get; init; }
        public IReadOnlyList<string>? MetricKeys { get; init; }
    }

    /// <summary>Cambio de estado de acción.</summary>
    public sealed class BusinessActionStatusRequest
    {
        public Guid ActionId { get; init; }
        public BusinessActionStatus TargetStatus { get; init; }
        public string? Actor { get; init; }
        public string? Notes { get; init; }
        public DateTime? AtUtc { get; init; }
        public BusinessActionActualImpact? ActualImpact { get; init; }

        /// <summary>
        /// Al Completar: días de ventana (default = actuales o 14). Recalcula DueAt desde AtUtc.
        /// </summary>
        public int? EvaluationDays { get; init; }
    }

    /// <summary>Ajuste de ventana sin cambiar estado (FASE 11.7).</summary>
    public sealed class BusinessActionEvaluationWindowRequest
    {
        public Guid ActionId { get; init; }
        /// <summary>Días &gt; 0. Ancla: CompletedAt si Completada; si no, CreatedAt.</summary>
        public int EvaluationDays { get; init; }
        public DateTime? AtUtc { get; init; }
    }

    public sealed class BusinessActionServiceResult
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public BusinessActionRecord? Record { get; init; }
        public BusinessActionStatus? PreviousStatus { get; init; }
        public BusinessActionStatus? NewStatus { get; init; }
        public long? PersistenceId { get; init; }
        public BusinessActionEvaluationWindow? EvaluationWindow { get; init; }
        /// <summary>Deltas calculados en CapturePostMetrics (FASE 11.8).</summary>
        public IReadOnlyList<BusinessActionMetricDelta>? Deltas { get; init; }
    }

    public sealed class BusinessActionQuery
    {
        public BusinessActionStatus? Status { get; init; }
        public BusinessActionType? ActionType { get; init; }
        public Guid? DecisionEventId { get; init; }
        public DecisionEntityType? EntityType { get; init; }
        public string? EntityId { get; init; }
        public DateTime? FromUtc { get; init; }
        public DateTime? ToUtc { get; init; }
        public int Top { get; init; } = 100;

        /// <summary>Si true, solo acciones Ready (ventana vencida, sin outcome) — FASE 11.7.</summary>
        public bool ReadyForEvaluationOnly { get; init; }

        /// <summary>Si true, solo Completadas aún InWindow.</summary>
        public bool InEvaluationWindowOnly { get; init; }

        public DateTime? AsOfUtc { get; init; }
    }

    /// <summary>Captura post-métricas vs baseline (FASE 11.8). No clasifica Outcome.</summary>
    public sealed class BusinessActionPostMetricsRequest
    {
        public Guid ActionId { get; init; }
        public DecisionAnalyticsBundle? Analytics { get; init; }
        public IReadOnlyDictionary<string, decimal?>? MetricValues { get; init; }
        public ProfitPeriodKind? PeriodKind { get; init; }
        public DateTime? CapturedAt { get; init; }
        public IReadOnlyList<string>? MetricKeys { get; init; }
        public string? Notes { get; init; }
        /// <summary>Si true, permite capturar aunque la ventana aún no venció.</summary>
        public bool AllowBeforeWindowEnd { get; init; }
    }
}
