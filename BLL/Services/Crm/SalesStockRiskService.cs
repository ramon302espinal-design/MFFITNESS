using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Riesgo de quiebre y señales stock↔ventas (FASE 9.18).</summary>
    public class SalesStockRiskService
    {
        private readonly InventoryFinancialService _inventory = new();
        private readonly ProductTrendService _trends = new();

        public SalesStockRiskReport GetReport(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            DateTime? asOf = null,
            SalesStockRiskThresholds? thresholds = null)
        {
            SalesStockRiskThresholds t = thresholds ?? SalesStockRiskThresholds.Default;
            InventoryFinancialSummary inv = _inventory.GetInventoryFinancials(null, null, asOf);

            Dictionary<int, ProductTrendDirection> trendById = new();
            ProductTrendReport trendReport = _trends.GetTrends(periodKind, asOf);
            foreach (ProductTrendRow row in trendReport.Rows)
                trendById[row.ProductId] = row.PrimaryTrend;

            var signals = inv.Rows.Select(r =>
            {
                ProductTrendDirection? tdir = trendById.TryGetValue(r.ProductId, out ProductTrendDirection tr)
                    ? tr
                    : null;
                return SalesStockRiskMath.FromInventory(r, tdir, t);
            });

            return SalesStockRiskMath.Compose(signals, periodKind, t);
        }
    }
}
