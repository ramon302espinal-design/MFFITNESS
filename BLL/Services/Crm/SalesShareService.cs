using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Participación y concentración Top N (FASE 9.12).</summary>
    public class SalesShareService
    {
        private readonly SalesByProductService _products = new();
        private readonly SalesByCategoryService _categories = new();

        public SalesShareReport GetProductShare(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            SalesShareMetric metric = SalesShareMetric.Revenue,
            int topN = 5,
            DateTime? asOf = null)
        {
            SalesByProductReport report = _products.GetReport(periodKind, asOf);
            return SalesShareComposer.FromProducts(report.Products, metric, periodKind, topN);
        }

        public SalesShareReport GetCategoryShare(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            SalesShareMetric metric = SalesShareMetric.Revenue,
            int topN = 5,
            DateTime? asOf = null)
        {
            SalesByCategoryReport report = _categories.GetReport(periodKind, asOf);
            return SalesShareComposer.FromCategories(report.Categories, metric, periodKind, topN);
        }
    }
}
