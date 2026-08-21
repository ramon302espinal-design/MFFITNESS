namespace BLL.Models.Crm
{
    /// <summary>
    /// Señales de dashboard de performance (FASE 8.18).
    /// Buckets = clasificación FASE 8; tops = una métrica por lista.
    /// </summary>
    public sealed class ProductPerformanceDashboardReport
    {
        public ProfitPeriodKind PeriodKind { get; init; }

        public int StarCount { get; init; }
        public int HealthyCount { get; init; }
        public int OpportunityCount { get; init; }
        public int SlowCount { get; init; }
        public int CriticalCount { get; init; }
        public int NewCount { get; init; }
        public int InsufficientCount { get; init; }

        /// <summary>0–100 orientativo (explicable). No es score de producto.</summary>
        public int PortfolioHealthScore { get; init; }

        public decimal StarCapital { get; init; }
        public decimal OpportunityCapital { get; init; }
        public decimal CriticalClassCapital { get; init; }
        public decimal SlowCapital { get; init; }
        public decimal TotalImmobilizedCapital { get; init; }

        public IReadOnlyList<ProductClassificationRow> TopStars { get; init; }
            = Array.Empty<ProductClassificationRow>();

        public IReadOnlyList<ProductClassificationRow> TopOpportunities { get; init; }
            = Array.Empty<ProductClassificationRow>();

        public IReadOnlyList<ProductClassificationRow> TopRisks { get; init; }
            = Array.Empty<ProductClassificationRow>();

        public IReadOnlyList<ProductPerformanceRankRow> TopUnits { get; init; }
            = Array.Empty<ProductPerformanceRankRow>();

        public IReadOnlyList<ProductPerformanceRankRow> TopProfit { get; init; }
            = Array.Empty<ProductPerformanceRankRow>();

        public IReadOnlyList<ProductPerformanceRankRow> TopRoi { get; init; }
            = Array.Empty<ProductPerformanceRankRow>();

        public IReadOnlyList<ProductPerformanceRankRow> TopMargin { get; init; }
            = Array.Empty<ProductPerformanceRankRow>();

        public IReadOnlyList<ProductPerformanceRankRow> TopTurnover { get; init; }
            = Array.Empty<ProductPerformanceRankRow>();
    }
}
