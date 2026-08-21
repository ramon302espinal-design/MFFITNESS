using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Composición pura ProfitSummary → SalesSummary (FASE 9.2).</summary>
    public static class SalesAnalyticsComposer
    {
        public static SalesSummary FromProfitSummary(
            ProfitSummary profit,
            ProfitPeriodKind periodKind = ProfitPeriodKind.Custom)
        {
            return new SalesSummary
            {
                PeriodFrom = profit.PeriodFrom,
                PeriodToExclusive = profit.PeriodToExclusive,
                PeriodKind = periodKind,
                TransactionCount = profit.TransactionCount,
                UnitsSold = profit.UnitsSold,
                RevenueTotal = profit.RevenueTotal,
                SalesHeaderTotal = profit.SalesHeaderTotal,
                RealizedProfit = profit.RealizedProfit,
                Cogs = profit.Cogs,
                RevenueWithCost = profit.RevenueWithCost,
                MarginPct = profit.MarginPct,
                RoiPct = profit.RoiPct,
                AverageTicket = SalesAnalyticsMath.AverageTicket(
                    profit.RevenueTotal, profit.TransactionCount),
                UnitsPerTransaction = SalesAnalyticsMath.UnitsPerTransaction(
                    profit.UnitsSold, profit.TransactionCount),
                HasReliableRealizedProfit = profit.HasReliableRealizedProfit,
                CostCoveragePct = profit.CostCoveragePct,
                CollectedAtSale = profit.CollectedAtSale
            };
        }
    }
}
