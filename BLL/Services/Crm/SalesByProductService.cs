using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>
    /// Ventas por producto + tendencia + clase FASE 8 (FASE 9.10).
    /// </summary>
    public class SalesByProductService
    {
        private readonly ProfitAnalyticsService _profit = new();
        private readonly ProductTrendService _trends = new();
        private readonly ProductClassificationService _classification = new();

        public SalesByProductReport GetReport(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            DateTime? asOf = null,
            int? top = null)
        {
            IReadOnlyList<ProfitGroupRow> products = _profit.GetByProduct(periodKind, asOf, top);
            ProductTrendReport trendReport = _trends.GetTrends(periodKind, asOf);
            ProductClassificationReport classReport = _classification.GetReport(periodKind, asOf);

            var trends = trendReport.Rows.ToDictionary(t => t.ProductId);
            var classes = classReport.Rows.ToDictionary(c => c.ProductId);

            return SalesByProductComposer.Build(products, trends, classes, periodKind);
        }
    }
}
