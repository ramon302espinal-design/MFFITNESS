namespace BLL.Models.Crm
{
    /// <summary>
    /// Snapshot mínimo de métricas SSOT antes de la acción (FASE 11.6 / brief §19·§63).
    /// No copia el sistema completo — solo claves relevantes.
    /// </summary>
    public sealed class BusinessActionBaseline
    {
        public DateTime CapturedAt { get; init; }

        public ProfitPeriodKind? PeriodKind { get; init; }

        public DecisionEntityType EntityType { get; init; }

        public string? EntityId { get; init; }

        /// <summary>Origen del snapshot (ej. SalesCapitalBridgeService).</summary>
        public string? SourceNote { get; init; }

        public IReadOnlyList<BusinessActionBaselineMetric> Metrics { get; init; }
            = Array.Empty<BusinessActionBaselineMetric>();

        public bool HasMetrics => Metrics.Count > 0;
    }

    /// <summary>Una métrica del baseline (clave catálogo + valor + fuente).</summary>
    public sealed class BusinessActionBaselineMetric
    {
        public string MetricKey { get; init; } = string.Empty;
        public string Label { get; init; } = string.Empty;
        public decimal? Value { get; init; }
        public string? Unit { get; init; }
        /// <summary>Servicio / bridge SSOT que aportó el valor.</summary>
        public string Source { get; init; } = string.Empty;
    }

    /// <summary>Contexto de captura (sin I/O).</summary>
    public sealed record BusinessActionBaselineCaptureRequest
    {
        public DecisionEntityType EntityType { get; init; } = DecisionEntityType.Portfolio;
        public string? EntityId { get; init; }
        public ProfitPeriodKind? PeriodKind { get; init; }
        public DateTime? CapturedAt { get; init; }

        /// <summary>
        /// Claves a capturar. Null/vacío → set mínimo por defecto (ventas/margen/stock/capital).
        /// </summary>
        public IReadOnlyList<string>? MetricKeys { get; init; }
    }
}
