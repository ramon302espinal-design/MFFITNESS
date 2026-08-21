using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>
    /// Puente FASE 7 ↔ FASE 8 (8.17): capital por clase de performance + top congelado etiquetado.
    /// </summary>
    public class ProductCapitalPerformanceBridgeService
    {
        private readonly ProductPerformanceService _performance = new();
        private readonly ProductTrendService _trends = new();

        public ProductCapitalPerformanceReport GetReport(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            DateTime? asOf = null,
            int? topImmobilized = 15)
        {
            ProductPerformanceReport perfReport = _performance.GetReport(periodKind, asOf);
            ProductTrendReport trendReport = _trends.GetTrends(periodKind, asOf);
            var trendById = trendReport.Rows.ToDictionary(r => r.ProductId);

            var products = new List<ProductCapitalClassRow>(perfReport.Rows.Count);
            foreach (ProductPerformanceRow p in perfReport.Rows)
            {
                trendById.TryGetValue(p.ProductId, out ProductTrendRow? tr);
                ProductClassificationRow cls = ProductClassificationMath.Classify(p, tr);
                products.Add(ProductCapitalPerformanceComposer.Compose(p, cls));
            }

            return ProductCapitalPerformanceComposer.BuildReport(
                products, periodKind, topImmobilized);
        }
    }
}
