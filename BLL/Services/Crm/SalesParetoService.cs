using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Pareto de ventas (FASE 9.13) — datos reales, no 80/20 fijo.</summary>
    public class SalesParetoService
    {
        private readonly SalesShareService _share = new();

        public SalesParetoReport GetProductPareto(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            SalesShareMetric metric = SalesShareMetric.Revenue,
            decimal targetCumulativePct = 80m,
            DateTime? asOf = null)
        {
            SalesShareReport share = _share.GetProductShare(
                periodKind, metric, topN: 5, asOf);
            return SalesParetoComposer.FromShareReport(share, targetCumulativePct);
        }

        public SalesParetoReport GetCategoryPareto(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            SalesShareMetric metric = SalesShareMetric.Revenue,
            decimal targetCumulativePct = 80m,
            DateTime? asOf = null)
        {
            SalesShareReport share = _share.GetCategoryShare(
                periodKind, metric, topN: 5, asOf);
            return SalesParetoComposer.FromShareReport(share, targetCumulativePct);
        }
    }
}
