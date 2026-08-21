namespace BLL.Models.Crm
{
    /// <summary>Registro persistido de historial (FASE 10.21 / brief §108).</summary>
    public sealed class DecisionHistoryRecord
    {
        public long Id { get; init; }
        public Guid EventId { get; init; }
        public string Fingerprint { get; init; } = string.Empty;
        public string EventType { get; init; } = string.Empty;
        public DecisionEventArea Area { get; init; }
        public DecisionEntityType EntityType { get; init; }
        public string? EntityId { get; init; }
        public string EntityName { get; init; } = string.Empty;
        public string? PeriodKey { get; init; }
        public DecisionSeverity Severity { get; init; }
        public DecisionPriority Priority { get; init; }
        public DecisionEventStatus Status { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Reason { get; init; } = string.Empty;
        public string Impact { get; init; } = string.Empty;
        public string Recommendation { get; init; } = string.Empty;
        public string Source { get; init; } = string.Empty;
        public string? GroupKey { get; init; }
        public DateTime DetectedAt { get; init; }
        public DateTime CreatedAt { get; init; }

        /// <summary>FASE 10.22 — null hasta resolución.</summary>
        public DateTime? ResolvedAt { get; init; }
        public string? ResolvedBy { get; init; }
        public string? ResolutionNote { get; init; }
    }

    /// <summary>Filtro de consulta de historial.</summary>
    public sealed class DecisionHistoryQuery
    {
        public DateTime? FromUtc { get; init; }
        public DateTime? ToUtc { get; init; }
        public DecisionEventStatus? Status { get; init; }
        public string? EventType { get; init; }
        public string? Fingerprint { get; init; }
        public DecisionEntityType? EntityType { get; init; }
        public string? EntityId { get; init; }
        public int Top { get; init; } = 100;
    }

    /// <summary>Resultado de captura (persistencia de un run del motor).</summary>
    public sealed class DecisionHistoryCaptureResult
    {
        public int Considered { get; init; }
        public int Inserted { get; init; }
        public int SkippedActiveDuplicate { get; init; }
        public IReadOnlyList<long> InsertedIds { get; init; } = Array.Empty<long>();
        public string PolicyNote { get; init; } = string.Empty;
    }

    /// <summary>
    /// Resultado de reconciliar: fingerprints abiertos ausentes del run actual → Resolved (TEST 9).
    /// Solo estado de historial — no muta stock/caja.
    /// </summary>
    public sealed class DecisionHistoryReconcileResult
    {
        public int OpenConsidered { get; init; }
        public int ResolvedAbsent { get; init; }
        public int StillPresent { get; init; }
        public IReadOnlyList<long> ResolvedIds { get; init; } = Array.Empty<long>();
        public string PolicyNote { get; init; } = string.Empty;
    }

    /// <summary>Señal de recurrencia (brief §110) — preparación, sin UI.</summary>
    public sealed class DecisionRecurrenceSignal
    {
        public string EventType { get; init; } = string.Empty;
        public string? EntityId { get; init; }
        public int OccurrenceCount { get; init; }
        public DateTime FirstDetectedAt { get; init; }
        public DateTime LastDetectedAt { get; init; }
        public bool IsRecurrent { get; init; }
        public string Message { get; init; } = string.Empty;
    }

    /// <summary>Métricas básicas del historial (brief §109 prep).</summary>
    public sealed class DecisionHistoryMetrics
    {
        public int GeneratedCount { get; init; }
        public int CriticalCount { get; init; }
        public int ActiveCount { get; init; }
        public int ResolvedCount { get; init; }
        public int IgnoredCount { get; init; }
        public double? AvgResolutionHours { get; init; }
    }
}
