namespace BLL.Models.Crm
{
    /// <summary>Bucket de mix ventas por clase FASE 8 (FASE 9.20 / §53).</summary>
    public sealed class SalesClassMixBucket
    {
        public ProductPerformanceClass Class { get; init; }
        public int ProductCount { get; init; }

        public decimal RevenueTotal { get; init; }
        public decimal RealizedProfit { get; init; }
        public int UnitsSold { get; init; }

        public decimal? RevenueSharePct { get; init; }
        public decimal? ProfitSharePct { get; init; }
        public decimal? UnitsSharePct { get; init; }
    }

    /// <summary>Producto estrella en contexto de ventas (FASE 9.20).</summary>
    public sealed class SalesStarContributionRow
    {
        public int Rank { get; init; }
        public int ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;

        public decimal RevenueTotal { get; init; }
        public decimal RealizedProfit { get; init; }
        public int UnitsSold { get; init; }
        public decimal? RevenueSharePct { get; init; }

        public bool FlagStockoutRisk { get; init; }
        public ProductTrendDirection? Trend { get; init; }
    }

    /// <summary>Mix categoría (FASE 9.20 / §54) — participación etiquetada.</summary>
    public sealed class SalesCategoryMixRow
    {
        public int Rank { get; init; }
        public string CategoryName { get; init; } = string.Empty;
        public decimal RevenueTotal { get; init; }
        public decimal? RevenueSharePct { get; init; }
    }

    /// <summary>Informe integración ventas ↔ estrellas / clases (FASE 9.20).</summary>
    public sealed class SalesStarMixReport
    {
        public ProfitPeriodKind PeriodKind { get; init; }

        public decimal TotalRevenue { get; init; }
        public decimal TotalRealizedProfit { get; init; }
        public int TotalUnits { get; init; }

        public int StarCount { get; init; }
        public decimal StarRevenue { get; init; }
        public decimal? StarRevenueSharePct { get; init; }
        public decimal? StarProfitSharePct { get; init; }

        public decimal? HealthyRevenueSharePct { get; init; }
        public decimal? SlowRevenueSharePct { get; init; }
        public decimal? CriticalRevenueSharePct { get; init; }

        public IReadOnlyList<SalesClassMixBucket> ClassBuckets { get; init; }
            = Array.Empty<SalesClassMixBucket>();

        public IReadOnlyList<SalesStarContributionRow> TopStars { get; init; }
            = Array.Empty<SalesStarContributionRow>();

        public IReadOnlyList<SalesStarContributionRow> StarsWithStockoutRisk { get; init; }
            = Array.Empty<SalesStarContributionRow>();

        public IReadOnlyList<SalesCategoryMixRow> CategoryMix { get; init; }
            = Array.Empty<SalesCategoryMixRow>();

        public string Caution { get; init; } = string.Empty;
    }
}
