namespace BLL.Models.Crm
{
    /// <summary>
    /// Comparación de dos períodos equivalentes (FASE 9.4).
    /// Variación % = 9.2/9.5; aquí el empaquetado current vs previous.
    /// </summary>
    public sealed class SalesMetricDelta
    {
        public decimal Current { get; init; }
        public decimal Previous { get; init; }

        /// <summary>Null si Previous = 0 (N/D / SIN BASE COMPARABLE).</summary>
        public decimal? VariationPct { get; init; }

        public bool HasComparableBase => VariationPct.HasValue;
    }

    public sealed class SalesComparisonReport
    {
        public ProfitPeriodKind PeriodKind { get; init; }
        public DateTime? CurrentFrom { get; init; }
        public DateTime? CurrentToExclusive { get; init; }
        public DateTime? PreviousFrom { get; init; }
        public DateTime? PreviousToExclusive { get; init; }

        public SalesSummary Current { get; init; } = null!;
        public SalesSummary Previous { get; init; } = null!;

        public SalesMetricDelta Revenue { get; init; } = null!;
        public SalesMetricDelta RealizedProfit { get; init; } = null!;
        public SalesMetricDelta Units { get; init; } = null!;
        public SalesMetricDelta Transactions { get; init; } = null!;
        public SalesMetricDelta Ticket { get; init; } = null!;

        /// <summary>Solo puntos de margen confiables; si falta base → VariationPct null.</summary>
        public SalesMetricDelta? Margin { get; init; }

        public bool HasComparablePeriod { get; init; }
    }
}
