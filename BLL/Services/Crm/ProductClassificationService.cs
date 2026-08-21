using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>
    /// Orquestador de clasificación (FASE 8.12). Une performance + tendencia.
    /// </summary>
    public class ProductClassificationService
    {
        private readonly ProductPerformanceService _performance = new();
        private readonly ProductTrendService _trends = new();

        public ProductClassificationReport GetReport(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            DateTime? asOf = null,
            ProductClassificationThresholds? thresholds = null)
        {
            ProductPerformanceReport perf = _performance.GetReport(periodKind, asOf);
            ProductTrendReport trendReport = _trends.GetTrends(periodKind, asOf);
            var trendById = trendReport.Rows.ToDictionary(t => t.ProductId);

            var rows = new List<ProductClassificationRow>(perf.Rows.Count);
            foreach (ProductPerformanceRow p in perf.Rows)
            {
                trendById.TryGetValue(p.ProductId, out ProductTrendRow? tr);
                rows.Add(ProductClassificationMath.Classify(p, tr, thresholds));
            }

            rows = rows
                .OrderBy(r => r.ProductName, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            return new ProductClassificationReport
            {
                PeriodKind = periodKind,
                ProductCount = rows.Count,
                NewCount = rows.Count(r => r.Class == ProductPerformanceClass.New),
                HealthyCount = rows.Count(r => r.Class == ProductPerformanceClass.Healthy),
                OpportunityCount = rows.Count(r => r.Class == ProductPerformanceClass.Opportunity),
                SlowCount = rows.Count(r => r.Class == ProductPerformanceClass.Slow),
                CriticalCount = rows.Count(r => r.Class == ProductPerformanceClass.Critical),
                StarCount = rows.Count(r => r.Class == ProductPerformanceClass.Star),
                InsufficientCount = rows.Count(r => r.Class == ProductPerformanceClass.InsufficientData),
                Rows = rows
            };
        }

        public IReadOnlyList<ProductClassificationRow> GetByClass(
            ProductPerformanceClass cls,
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            int top = 20,
            DateTime? asOf = null)
            => GetReport(periodKind, asOf).Rows
                .Where(r => r.Class == cls)
                .Take(top <= 0 ? 20 : top)
                .ToList();

        public IReadOnlyList<ProductClassificationRow> GetStars(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            int top = 20,
            DateTime? asOf = null)
            => GetByClass(ProductPerformanceClass.Star, periodKind, top, asOf);

        public IReadOnlyList<ProductClassificationRow> GetOpportunities(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            int top = 20,
            DateTime? asOf = null)
            => GetByClass(ProductPerformanceClass.Opportunity, periodKind, top, asOf);

        public IReadOnlyList<ProductClassificationRow> GetRisks(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            int top = 20,
            DateTime? asOf = null)
            => GetByClass(ProductPerformanceClass.Critical, periodKind, top, asOf);
    }
}
