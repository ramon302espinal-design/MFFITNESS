namespace BLL.Models.Crm
{
    /// <summary>Ventas por producto + tendencia + clase FASE 8 (FASE 9.10).</summary>
    public sealed class SalesProductRow
    {
        public int Rank { get; init; }
        public int ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;

        public int UnitsSold { get; init; }
        public decimal RevenueTotal { get; init; }
        public decimal RealizedProfit { get; init; }
        public decimal? MarginPct { get; init; }
        public decimal? RoiPct { get; init; }

        public int TransactionCount { get; init; }
        public decimal? AverageTicket { get; init; }
        public decimal? UnitsPerTransaction { get; init; }

        /// <summary>Participación % sobre ingresos del período (9.12 formaliza; aquí base).</summary>
        public decimal? RevenueSharePct { get; init; }

        public ProductTrendDirection? Trend { get; init; }
        public decimal? UnitsChangePct { get; init; }

        public ProductPerformanceClass? PerformanceClass { get; init; }
    }

    public sealed class SalesByProductReport
    {
        public ProfitPeriodKind PeriodKind { get; init; }
        public int ProductCount { get; init; }
        public decimal TotalRevenue { get; init; }
        public decimal TotalRealizedProfit { get; init; }

        public IReadOnlyList<SalesProductRow> Products { get; init; }
            = Array.Empty<SalesProductRow>();
    }
}
