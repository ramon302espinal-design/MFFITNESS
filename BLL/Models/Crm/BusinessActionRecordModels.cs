namespace BLL.Models.Crm
{
    /// <summary>
    /// Impacto esperado declarado por el usuario (FASE 11.3 / brief §10).
    /// Sin garantías monetarias inventadas.
    /// </summary>
    public sealed class BusinessActionExpectedImpact
    {
        /// <summary>Ej. "Reducir capital congelado." — objetivo cualitativo.</summary>
        public string Summary { get; init; } = string.Empty;

        /// <summary>Métricas que se espera mover (claves SSOT / DecisionMetricsCatalog).</summary>
        public IReadOnlyList<string> TargetMetricKeys { get; init; }
            = Array.Empty<string>();

        /// <summary>Notas opcionales; no usar "ganaremos RD$X" sin base.</summary>
        public string? Notes { get; init; }
    }

    /// <summary>
    /// Impacto real observado (FASE 11.3 shell; cálculo = 11.8–11.9).
    /// Lenguaje: "incremento observado" — no causalidad automática.
    /// </summary>
    public sealed class BusinessActionActualImpact
    {
        public BusinessActionOutcome Outcome { get; init; }
        public BusinessActionConfidence Confidence { get; init; }

        /// <summary>Resumen observado, p.ej. "Durante el período posterior, ventas +38%."</summary>
        public string Summary { get; init; } = string.Empty;

        public string? Notes { get; init; }

        /// <summary>Llenado en 11.8+ (antes/después por métrica).</summary>
        public IReadOnlyList<BusinessActionMetricDelta> Deltas { get; init; }
            = Array.Empty<BusinessActionMetricDelta>();
    }

    /// <summary>Delta de una métrica (placeholder 11.3; comparación 11.8).</summary>
    public sealed class BusinessActionMetricDelta
    {
        public string MetricKey { get; init; } = string.Empty;
        public string Label { get; init; } = string.Empty;
        public decimal? Before { get; init; }
        public decimal? After { get; init; }
        /// <summary>Variación relativa % o puntos (según IsPercentagePoints).</summary>
        public decimal? Change { get; init; }
        /// <summary>True = puntos porcentuales (margen 22→25 = +3 pp), no +3%.</summary>
        public bool IsPercentagePoints { get; init; }
        public string? Unit { get; init; }
    }

    /// <summary>
    /// ActionRecord — registro manual de acción de negocio (FASE 11.3 / brief §5·§7).
    /// No muta POS. Persistencia = 11.4.
    /// </summary>
    public sealed class BusinessActionRecord
    {
        public Guid ActionId { get; init; }

        /// <summary>DecisionEvent.EventId vinculado (opcional).</summary>
        public Guid? DecisionEventId { get; init; }

        /// <summary>Id de historial CRM si ya persistió el evento.</summary>
        public long? DecisionHistoryId { get; init; }

        public BusinessActionType ActionType { get; init; }
        public DecisionEventArea Area { get; init; }
        public DecisionEntityType EntityType { get; init; }
        public string? EntityId { get; init; }
        public string EntityName { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;
        /// <summary>¿Por qué? (brief §5).</summary>
        public string? Reason { get; init; }
        public string? Notes { get; init; }

        public decimal? QuantityInvolved { get; init; }
        public decimal? CapitalInvolved { get; init; }

        public DateTime CreatedAt { get; init; }
        public string? CreatedBy { get; init; }

        public BusinessActionStatus Status { get; init; }

        /// <summary>Inicio efectivo de la acción (brief §18).</summary>
        public DateTime? StartedAt { get; init; }

        /// <summary>Días de ventana de evaluación (ej. 14).</summary>
        public int? EvaluationDays { get; init; }

        public DateTime? EvaluationDueAt { get; init; }

        public DateTime? CompletedAt { get; init; }
        public string? CompletedBy { get; init; }

        public BusinessActionExpectedImpact? ExpectedImpact { get; init; }
        public BusinessActionActualImpact? ActualImpact { get; init; }

        /// <summary>Snapshot SSOT pre-acción (FASE 11.6).</summary>
        public BusinessActionBaseline? Baseline { get; init; }
    }
}
