namespace BLL.Models.Crm
{
    /// <summary>Alertas de dashboard de ventas (FASE 9.21 / §62).</summary>
    public enum SalesDashboardAlertKind
    {
        StrongGrowth = 1,
        StrongDecline = 2,
        Deceleration = 3,
        SalesDownStockUp = 4,
        RevenueUpProfitDown = 5,
        RevenueUpMarginDown = 6,
        RoiDown = 7,
        StockoutRisk = 8,
        GrowthOpportunity = 9
    }

    public sealed class SalesDashboardAlert
    {
        public SalesDashboardAlertKind Kind { get; init; }
        public string Icon { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
    }

    public sealed class SalesDashboardTopItem
    {
        public int Rank { get; init; }
        public string Name { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public decimal? SharePct { get; init; }
    }

    /// <summary>
    /// Snapshot para FrmAnaDashboard / FrmAnaVentas (FASE 9.21).
    /// Sin lógica en Forms — solo consumo.
    /// </summary>
    public sealed class SalesDashboardReport
    {
        public ProfitPeriodKind PeriodKind { get; init; }

        public int TransactionCount { get; init; }
        public int UnitsSold { get; init; }
        public decimal RevenueTotal { get; init; }
        public decimal RealizedProfit { get; init; }
        public decimal? MarginPct { get; init; }
        public decimal? AverageTicket { get; init; }

        public SalesVariationLabel? RevenueVariation { get; init; }
        public SalesVariationLabel? ProfitVariation { get; init; }
        public SalesVariationLabel? TicketVariation { get; init; }

        public SalesSeriesTrendKind RevenueTrend { get; init; }
        public SalesAccelerationKind RevenueAcceleration { get; init; }

        public decimal? ForecastBaseRevenue { get; init; }
        public SalesForecastConfidence ForecastConfidence { get; init; }
        public string ForecastNote { get; init; } = string.Empty;

        public int StarCount { get; init; }
        public decimal? StarRevenueSharePct { get; init; }

        public int StockoutRiskCount { get; init; }
        public int CapitalRiskCount { get; init; }

        public IReadOnlyList<SalesDashboardAlert> Alerts { get; init; }
            = Array.Empty<SalesDashboardAlert>();

        public IReadOnlyList<SalesDashboardTopItem> TopProducts { get; init; }
            = Array.Empty<SalesDashboardTopItem>();

        public IReadOnlyList<SalesDashboardTopItem> TopCategories { get; init; }
            = Array.Empty<SalesDashboardTopItem>();
    }
}
