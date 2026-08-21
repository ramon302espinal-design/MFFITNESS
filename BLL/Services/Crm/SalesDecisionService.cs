using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Señales de decisión de ventas para FrmAnaDecisiones (FASE 9.22).</summary>
    public class SalesDecisionService
    {
        private readonly SalesVariationService _variations = new();
        private readonly SalesShareService _share = new();
        private readonly SalesStockRiskService _stock = new();
        private readonly ProductTrendService _trends = new();

        public SalesDecisionReport GetReport(
            ProfitPeriodKind periodKind = ProfitPeriodKind.Last30Days,
            DateTime? asOf = null)
        {
            string periodLabel = SalesDecisionMath.PeriodLabel(periodKind);
            SalesVariationReport? variations = _variations.GetVariations(periodKind, asOf);
            SalesShareReport share = _share.GetProductShare(periodKind, SalesShareMetric.Revenue, topN: 3, asOf);
            SalesStockRiskReport stock = _stock.GetReport(periodKind, asOf);
            ProductTrendReport trends = _trends.GetTrends(periodKind, asOf);
            var trendById = trends.Rows.ToDictionary(t => t.ProductId);

            SalesDecisionSignal? growthCover = null;
            foreach (SalesStockSignalRow row in stock.Rows
                         .Where(r => r.DaysOfCover.HasValue && r.DaysOfCover.Value <= 14m)
                         .OrderBy(r => r.DaysOfCover))
            {
                if (!trendById.TryGetValue(row.ProductId, out ProductTrendRow? tr))
                    continue;
                if (tr.PrimaryTrend != ProductTrendDirection.Growing || !tr.UnitsChangePct.HasValue)
                    continue;
                growthCover = SalesDecisionMath.FromGrowthWithLowCover(
                    row.ProductName, tr.UnitsChangePct, row.DaysOfCover);
                if (growthCover != null)
                    break;
            }

            return SalesDecisionMath.Build(
                periodKind,
                new SalesDecisionSignal?[]
                {
                    SalesDecisionMath.FromRevenueVariation(variations?.Revenue, periodLabel),
                    SalesDecisionMath.FromConcentration(share.TopNSharePct, share.TopN),
                    growthCover,
                    SalesDecisionMath.FromRevenueUpMarginDown(
                        variations?.Revenue.VariationPct,
                        variations?.Margin?.VariationPct),
                    SalesDecisionMath.FromStockout(stock.StockoutRiskCount)
                });
        }
    }
}
