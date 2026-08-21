using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>
    /// Comparaciones de ventas período vs equivalente anterior (FASE 9.4).
    /// </summary>
    public class SalesComparisonService
    {
        private readonly SalesAnalyticsService _sales = new();

        public SalesComparisonReport? GetComparison(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            DateTime? asOf = null)
        {
            var pair = ProductTrendMath.TryResolvePeriodPair(periodKind, asOf);
            if (pair == null)
                return null;

            SalesSummary current = _sales.GetSummary(
                pair.Value.Current.From, pair.Value.Current.ToExclusive);
            // Re-tag period kind on summaries for clarity
            current = WithKind(current, periodKind);

            SalesSummary previous = _sales.GetSummary(
                pair.Value.Previous.From, pair.Value.Previous.ToExclusive);
            previous = WithKind(previous, ProfitPeriodKind.Custom);

            return SalesComparisonComposer.Build(
                periodKind,
                pair.Value.Current,
                pair.Value.Previous,
                current,
                previous);
        }

        private static SalesSummary WithKind(SalesSummary s, ProfitPeriodKind kind)
            => new()
            {
                PeriodFrom = s.PeriodFrom,
                PeriodToExclusive = s.PeriodToExclusive,
                PeriodKind = kind,
                TransactionCount = s.TransactionCount,
                UnitsSold = s.UnitsSold,
                RevenueTotal = s.RevenueTotal,
                SalesHeaderTotal = s.SalesHeaderTotal,
                RealizedProfit = s.RealizedProfit,
                Cogs = s.Cogs,
                RevenueWithCost = s.RevenueWithCost,
                MarginPct = s.MarginPct,
                RoiPct = s.RoiPct,
                AverageTicket = s.AverageTicket,
                UnitsPerTransaction = s.UnitsPerTransaction,
                HasReliableRealizedProfit = s.HasReliableRealizedProfit,
                CostCoveragePct = s.CostCoveragePct,
                CollectedAtSale = s.CollectedAtSale
            };
    }
}
