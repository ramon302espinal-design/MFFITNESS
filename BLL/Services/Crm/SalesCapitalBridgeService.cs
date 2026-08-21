using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Puente ventas ↔ capital inventario (FASE 9.19).</summary>
    public class SalesCapitalBridgeService
    {
        private readonly ProductPerformanceService _performance = new();
        private readonly ProductTrendService _trends = new();
        private readonly ProfitAnalyticsService _profit = new();
        private readonly SalesStockRiskService _stockRisk = new();

        public SalesCapitalBridgeReport GetReport(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            DateTime? asOf = null)
        {
            ProductPerformanceReport perf = _performance.GetReport(periodKind, asOf);
            ProductTrendReport trends = _trends.GetTrends(periodKind, asOf);
            SalesStockRiskReport stock = _stockRisk.GetReport(periodKind, asOf);

            var trendById = trends.Rows.ToDictionary(r => r.ProductId, r => r);
            var stockById = stock.Rows.ToDictionary(r => r.ProductId, r => r.PrimarySignal);

            Dictionary<int, ProfitGroupRow> previousById = new();
            var pair = ProductTrendMath.TryResolvePeriodPair(periodKind, asOf);
            if (pair != null)
            {
                foreach (ProfitGroupRow p in _profit.GetByProduct(
                             pair.Value.Previous.From, pair.Value.Previous.ToExclusive))
                {
                    if (p.ProductId.HasValue)
                        previousById[p.ProductId.Value] = p;
                }
            }

            var rows = new List<SalesCapitalBridgeRow>(perf.Rows.Count);
            foreach (ProductPerformanceRow p in perf.Rows)
            {
                previousById.TryGetValue(p.ProductId, out ProfitGroupRow? prev);
                decimal? revCh = prev != null
                    ? ProductTrendMath.ChangePct(p.RevenueTotal, prev.RevenueTotal)
                    : null;
                decimal? profitCh = prev != null
                    ? ProductTrendMath.ChangePct(p.RealizedProfit, prev.RealizedProfit)
                    : null;
                decimal? roiCh = prev != null && p.RoiPct.HasValue && prev.RoiPct.HasValue
                    ? InventoryFinancialMath.RoundPct(p.RoiPct.Value - prev.RoiPct.Value)
                    : null;

                ProductTrendDirection? trend = trendById.TryGetValue(p.ProductId, out ProductTrendRow? tr)
                    ? tr.PrimaryTrend
                    : null;
                SalesStockSignalKind? stockSig = stockById.TryGetValue(p.ProductId, out SalesStockSignalKind ss)
                    ? ss
                    : null;

                rows.Add(SalesCapitalBridgeMath.Compose(
                    p, revCh, profitCh, roiCh, trend, stockSig));
            }

            return SalesCapitalBridgeMath.BuildReport(rows, periodKind);
        }
    }
}
