namespace BLL.Models.Crm
{
    /// <summary>
    /// Nivel de confianza cualitativo (FASE 9.17).
    /// NO es probabilidad numérica.
    /// </summary>
    public enum SalesForecastConfidence
    {
        InsufficientData = 0,
        Low = 1,
        Medium = 2,
        High = 3
    }

    /// <summary>Un escenario de proyección (estimación, no certeza).</summary>
    public sealed class SalesForecastScenario
    {
        public string Key { get; init; } = string.Empty;
        public string Label { get; init; } = string.Empty;
        public decimal EstimatedRevenue { get; init; }
        public decimal? EstimatedProfit { get; init; }
    }

    /// <summary>Reporte de forecast / estimación (FASE 9.17).</summary>
    public sealed class SalesForecastReport
    {
        public ProfitPeriodKind SourcePeriodKind { get; init; }
        public int HorizonDays { get; init; }

        public int OperatingDaysUsed { get; init; }
        public decimal? HistoricalDailyAverageRevenue { get; init; }
        public decimal? HistoricalMarginPct { get; init; }

        public SalesSeriesTrendKind TrendUsed { get; init; }
        public decimal TrendAdjustmentFactor { get; init; }

        /// <summary>Promedio diario × horizonte (antes de tendencia).</summary>
        public decimal? SimpleProjectionRevenue { get; init; }

        public SalesForecastScenario Low { get; init; } = null!;
        public SalesForecastScenario Base { get; init; } = null!;
        public SalesForecastScenario High { get; init; } = null!;

        public SalesForecastConfidence Confidence { get; init; }
        public string ConfidenceReason { get; init; } = string.Empty;

        /// <summary>Siempre dejar claro: estimación / escenario, no certeza.</summary>
        public string LanguageNote { get; init; } = string.Empty;

        public bool HasEstimate => HistoricalDailyAverageRevenue.HasValue && OperatingDaysUsed > 0;
    }
}
