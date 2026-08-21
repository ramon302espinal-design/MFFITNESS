using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Orquestador dashboard de ventas (FASE 9.21).</summary>
    public class SalesDashboardService
    {
        private readonly SalesAnalyticsService _sales = new();
        private readonly SalesComparisonService _comparison = new();
        private readonly SalesSeriesTrendService _trend = new();
        private readonly SalesAccelerationService _accel = new();
        private readonly SalesForecastService _forecast = new();
        private readonly SalesStarMixService _stars = new();
        private readonly SalesStockRiskService _stock = new();
        private readonly SalesCapitalBridgeService _capital = new();
        private readonly SalesByProductService _byProduct = new();
        private readonly SalesByCategoryService _byCategory = new();

        public SalesDashboardReport GetReport(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            DateTime? asOf = null,
            int topLists = 5)
        {
            int top = topLists <= 0 ? 5 : topLists;
            SalesSummary summary = _sales.GetSummary(periodKind, asOf);

            SalesVariationReport? variations = null;
            SalesComparisonReport? cmp = _comparison.GetComparison(periodKind, asOf);
            if (cmp != null)
                variations = SalesVariationMath.FromComparison(cmp);

            SalesSeriesTrendReport trends = _trend.GetReport(periodKind, asOf);
            SalesAccelerationReport accel = _accel.GetReport(periodKind, asOf);
            SalesForecastReport forecast = _forecast.GetEstimate(periodKind, horizonDays: 30, asOf);
            SalesStarMixReport stars = _stars.GetReport(periodKind, asOf, top);
            SalesStockRiskReport stock = _stock.GetReport(periodKind, asOf);
            SalesCapitalBridgeReport capital = _capital.GetReport(periodKind, asOf);

            SalesByProductReport products = _byProduct.GetReport(periodKind, asOf, top);
            var topProducts = products.Products
                .OrderByDescending(r => r.RevenueTotal)
                .Take(top)
                .Select((r, i) => new SalesDashboardTopItem
                {
                    Rank = i + 1,
                    Name = r.ProductName,
                    Amount = r.RevenueTotal,
                    SharePct = r.RevenueSharePct
                })
                .ToList();

            SalesByCategoryReport categories = _byCategory.GetReport(periodKind, asOf);
            var topCategories = categories.Categories
                .OrderByDescending(r => r.RevenueTotal)
                .Take(top)
                .Select((r, i) => new SalesDashboardTopItem
                {
                    Rank = i + 1,
                    Name = r.CategoryName,
                    Amount = r.RevenueTotal,
                    SharePct = r.RevenueSharePct
                })
                .ToList();

            return SalesDashboardComposer.Build(
                periodKind,
                summary,
                variations,
                trends.Revenue,
                accel.Revenue,
                forecast,
                stars,
                stock,
                capital,
                topProducts,
                topCategories);
        }
    }
}
