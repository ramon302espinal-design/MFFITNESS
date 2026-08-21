namespace BLL.Models.Crm
{
    /// <summary>Ventas por categoría (FASE 9.11).</summary>
    public sealed class SalesCategoryRow
    {
        public int Rank { get; init; }
        public int? CategoryId { get; init; }
        public string CategoryName { get; init; } = string.Empty;

        public int UnitsSold { get; init; }
        public decimal RevenueTotal { get; init; }
        public decimal RealizedProfit { get; init; }
        public decimal? MarginPct { get; init; }
        public decimal? RoiPct { get; init; }

        public int TransactionCount { get; init; }
        public decimal? AverageTicket { get; init; }

        public decimal? RevenueSharePct { get; init; }
        public decimal? ProfitSharePct { get; init; }

        public ProductTrendDirection? Trend { get; init; }
        public decimal? UnitsChangePct { get; init; }
    }

    public sealed class SalesByCategoryReport
    {
        public ProfitPeriodKind PeriodKind { get; init; }
        public int CategoryCount { get; init; }
        public decimal TotalRevenue { get; init; }
        public decimal TotalRealizedProfit { get; init; }

        public IReadOnlyList<SalesCategoryRow> Categories { get; init; }
            = Array.Empty<SalesCategoryRow>();
    }
}
