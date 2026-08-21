using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Composición pura capital ↔ clase (FASE 8.17).</summary>
    public static class ProductCapitalPerformanceComposer
    {
        public static ProductCapitalClassRow Compose(
            ProductPerformanceRow performance,
            ProductClassificationRow classification)
        {
            return new ProductCapitalClassRow
            {
                ProductId = performance.ProductId,
                ProductName = performance.ProductName,
                Category = performance.Category,
                Class = classification.Class,
                Reasons = classification.Reasons,
                HealthStatus = performance.HealthStatus,
                Stock = performance.Stock,
                InventoryCapital = performance.InventoryCapital,
                ImmobilizedCapital = performance.ImmobilizedCapital,
                PotentialProfit = performance.PotentialProfit,
                IdleDays = performance.IdleDays,
                RealizedProfit = performance.RealizedProfit,
                MarginPct = performance.MarginPct,
                RoiPct = performance.RoiPct
            };
        }

        public static IReadOnlyList<ProductCapitalClassBucket> BuildBuckets(
            IReadOnlyList<ProductCapitalClassRow> products)
        {
            return products
                .GroupBy(p => p.Class)
                .OrderBy(g => (int)g.Key)
                .Select(g => new ProductCapitalClassBucket
                {
                    Class = g.Key,
                    ProductCount = g.Count(),
                    InventoryCapital = InventoryFinancialMath.RoundMoney(
                        g.Sum(p => p.InventoryCapital)),
                    ImmobilizedCapital = InventoryFinancialMath.RoundMoney(
                        g.Sum(p => p.ImmobilizedCapital)),
                    PeriodProfit = InventoryFinancialMath.RoundMoney(
                        g.Sum(p => p.RealizedProfit))
                })
                .ToList();
        }

        public static ProductCapitalPerformanceReport BuildReport(
            IReadOnlyList<ProductCapitalClassRow> products,
            ProfitPeriodKind periodKind,
            int? topImmobilized = 15)
        {
            IReadOnlyList<ProductCapitalClassBucket> buckets = BuildBuckets(products);

            decimal Cap(ProductPerformanceClass c)
                => buckets.FirstOrDefault(b => b.Class == c)?.InventoryCapital ?? 0m;

            var topImm = products
                .Where(p => p.IsImmobilized)
                .OrderByDescending(p => p.ImmobilizedCapital)
                .ThenByDescending(p => p.IdleDays ?? 0)
                .ThenBy(p => p.ProductName)
                .Take(topImmobilized ?? products.Count)
                .ToList();

            return new ProductCapitalPerformanceReport
            {
                PeriodKind = periodKind,
                TotalInventoryCapital = InventoryFinancialMath.RoundMoney(
                    products.Sum(p => p.InventoryCapital)),
                TotalImmobilizedCapital = InventoryFinancialMath.RoundMoney(
                    products.Sum(p => p.ImmobilizedCapital)),
                StarCapital = Cap(ProductPerformanceClass.Star),
                OpportunityCapital = Cap(ProductPerformanceClass.Opportunity),
                CriticalClassCapital = Cap(ProductPerformanceClass.Critical),
                SlowCapital = Cap(ProductPerformanceClass.Slow),
                HealthyCapital = Cap(ProductPerformanceClass.Healthy),
                NewCapital = Cap(ProductPerformanceClass.New),
                Buckets = buckets,
                TopImmobilized = topImm,
                Products = products
                    .OrderByDescending(p => p.InventoryCapital)
                    .ThenBy(p => p.ProductName)
                    .ToList()
            };
        }
    }
}
