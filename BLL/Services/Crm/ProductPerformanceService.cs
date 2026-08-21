using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>
    /// Orquestador de performance de producto (FASE 8.2–8.3).
    /// Reutiliza ProfitAnalytics + InventoryFinancial. Rankings por métrica explícita.
    /// </summary>
    public class ProductPerformanceService
    {
        private readonly ProfitAnalyticsService _profit = new();
        private readonly InventoryFinancialService _inventory = new();

        public ProductPerformanceReport GetReport(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            DateTime? asOf = null,
            DateTime? customFrom = null,
            DateTime? customToExclusive = null)
        {
            ProfitPeriodRange range = ProfitAnalyticsService.ResolvePeriod(
                periodKind, asOf, customFrom, customToExclusive);

            IReadOnlyList<ProfitGroupRow> periodRows = periodKind == ProfitPeriodKind.Custom
                ? _profit.GetByProduct(range.From, range.ToExclusive)
                : _profit.GetByProduct(periodKind, asOf);

            InventoryFinancialSummary inv = _inventory.GetInventoryFinancials(
                null, null, asOf);

            IReadOnlyList<ProductPerformanceRow> rows =
                ProductPerformanceComposer.ComposeAll(periodRows, inv.Rows);

            return BuildReport(periodKind, range, rows);
        }

        public ProductPerformanceReport GetReport(
            DateTime? periodFrom,
            DateTime? periodToExclusive,
            DateTime? asOf = null)
        {
            var periodRows = _profit.GetByProduct(periodFrom, periodToExclusive);
            InventoryFinancialSummary inv = _inventory.GetInventoryFinancials(
                null, null, asOf);

            IReadOnlyList<ProductPerformanceRow> rows =
                ProductPerformanceComposer.ComposeAll(periodRows, inv.Rows);

            var range = new ProfitPeriodRange(periodFrom, periodToExclusive);
            return BuildReport(ProfitPeriodKind.Custom, range, rows);
        }

        /// <summary>
        /// Ranking por métrica (8.3–8.10: todos los ProductPerformanceMetricKind).
        /// </summary>
        public IReadOnlyList<ProductPerformanceRankRow> GetRanking(
            ProductPerformanceMetricKind kind,
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            int top = 10,
            DateTime? asOf = null)
        {
            ProductPerformanceReport report = GetReport(periodKind, asOf);
            return ProductPerformanceRanker.Rank(report.Rows, kind, top);
        }

        public IReadOnlyList<ProductPerformanceRankRow> GetTopUnits(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            int top = 10,
            DateTime? asOf = null)
            => GetRanking(ProductPerformanceMetricKind.UnitsSold, periodKind, top, asOf);

        public IReadOnlyList<ProductPerformanceRankRow> GetTopRevenue(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            int top = 10,
            DateTime? asOf = null)
            => GetRanking(ProductPerformanceMetricKind.Revenue, periodKind, top, asOf);

        public IReadOnlyList<ProductPerformanceRankRow> GetTopProfit(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            int top = 10,
            DateTime? asOf = null)
            => GetRanking(ProductPerformanceMetricKind.RealizedProfit, periodKind, top, asOf);

        public IReadOnlyList<ProductPerformanceRankRow> GetTopMargin(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            int top = 10,
            DateTime? asOf = null)
            => GetRanking(ProductPerformanceMetricKind.MarginPct, periodKind, top, asOf);

        public IReadOnlyList<ProductPerformanceRankRow> GetTopRoi(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            int top = 10,
            DateTime? asOf = null)
            => GetRanking(ProductPerformanceMetricKind.RoiPct, periodKind, top, asOf);

        public IReadOnlyList<ProductPerformanceRankRow> GetTopTurnover(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            int top = 10,
            DateTime? asOf = null)
            => GetRanking(ProductPerformanceMetricKind.TurnoverProxy, periodKind, top, asOf);

        public IReadOnlyList<ProductPerformanceRankRow> GetTopVelocity(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            int top = 10,
            DateTime? asOf = null)
            => GetRanking(ProductPerformanceMetricKind.UnitsPerDay, periodKind, top, asOf);

        public IReadOnlyList<ProductPerformanceRankRow> GetTopInventoryCapital(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            int top = 10,
            DateTime? asOf = null)
            => GetRanking(ProductPerformanceMetricKind.InventoryCapital, periodKind, top, asOf);

        public IReadOnlyList<ProductPerformanceRankRow> GetTopImmobilized(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            int top = 10,
            DateTime? asOf = null)
            => GetRanking(ProductPerformanceMetricKind.ImmobilizedCapital, periodKind, top, asOf);

        public IReadOnlyList<ProductPerformanceRankRow> GetTopPotential(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            int top = 10,
            DateTime? asOf = null)
            => GetRanking(ProductPerformanceMetricKind.PotentialProfit, periodKind, top, asOf);

        private static ProductPerformanceReport BuildReport(
            ProfitPeriodKind kind,
            ProfitPeriodRange range,
            IReadOnlyList<ProductPerformanceRow> rows)
        {
            return new ProductPerformanceReport
            {
                PeriodKind = kind,
                PeriodFrom = range.From,
                PeriodToExclusive = range.ToExclusive,
                ProductCount = rows.Count,
                ProductsWithPeriodActivity = rows.Count(r => r.HasPeriodActivity),
                ProductsWithImmobilizedCapital = rows.Count(r => r.IsImmobilized),
                TotalUnitsSold = rows.Sum(r => r.UnitsSold),
                TotalRevenue = InventoryFinancialMath.RoundMoney(rows.Sum(r => r.RevenueTotal)),
                TotalRealizedProfit = InventoryFinancialMath.RoundMoney(
                    rows.Sum(r => r.RealizedProfit)),
                TotalInventoryCapital = InventoryFinancialMath.RoundMoney(
                    rows.Sum(r => r.InventoryCapital)),
                TotalImmobilizedCapital = InventoryFinancialMath.RoundMoney(
                    rows.Sum(r => r.ImmobilizedCapital)),
                Rows = rows
            };
        }
    }
}
