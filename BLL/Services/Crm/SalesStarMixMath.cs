using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato mix ventas ↔ estrellas / clases (FASE 9.20).</summary>
    public static class SalesStarMixPolicy
    {
        public const string Definition =
            "FASE 9.20 / §53: % de ingresos (y ganancia/unidades) por clase FASE 8 — " +
            "Estrella, Saludable, Oportunidad, Lento, Crítico, Nuevo. " +
            "Reutiliza ProductClassification — no reclasificar en ventas.";

        public const string Stars =
            "Estrellas impulsan ventas: StarRevenueSharePct + TopStars. " +
            "StockoutRisk en estrella = alerta de reabastecimiento (9.18), no quita la clase.";

        public const string Category =
            "§54 mix de categorías = participación de ingresos por categoría (paralelo al mix de clase).";

        public const string Separation =
            "Clase ≠ ranking de ingresos solo. Un top seller puede no ser Star. " +
            "Ingresos ≠ ganancia ≠ unidades en el mix.";
    }

    /// <summary>Composición pura mix clase/estrella (FASE 9.20).</summary>
    public static class SalesStarMixMath
    {
        public static SalesClassMixBucket Bucket(
            ProductPerformanceClass cls,
            IEnumerable<ProductClassificationRow> rows,
            decimal totalRevenue,
            decimal totalProfit,
            int totalUnits)
        {
            var list = rows.Where(r => r.Class == cls).ToList();
            decimal rev = list.Sum(r => r.Performance?.RevenueTotal ?? 0m);
            decimal profit = list.Sum(r => r.Performance?.RealizedProfit ?? 0m);
            int units = list.Sum(r => r.Performance?.UnitsSold ?? 0);

            return new SalesClassMixBucket
            {
                Class = cls,
                ProductCount = list.Count,
                RevenueTotal = InventoryFinancialMath.RoundMoney(rev),
                RealizedProfit = InventoryFinancialMath.RoundMoney(profit),
                UnitsSold = units,
                RevenueSharePct = SalesAnalyticsMath.SharePct(rev, totalRevenue),
                ProfitSharePct = SalesAnalyticsMath.SharePct(profit, totalProfit),
                UnitsSharePct = SalesAnalyticsMath.SharePct(units, totalUnits)
            };
        }

        public static IReadOnlyList<SalesClassMixBucket> BuildClassBuckets(
            IReadOnlyList<ProductClassificationRow> rows,
            decimal totalRevenue,
            decimal totalProfit,
            int totalUnits)
        {
            var classes = new[]
            {
                ProductPerformanceClass.Star,
                ProductPerformanceClass.Healthy,
                ProductPerformanceClass.Opportunity,
                ProductPerformanceClass.Slow,
                ProductPerformanceClass.Critical,
                ProductPerformanceClass.New,
                ProductPerformanceClass.InsufficientData
            };

            return classes
                .Select(c => Bucket(c, rows, totalRevenue, totalProfit, totalUnits))
                .Where(b => b.ProductCount > 0 || b.RevenueTotal > 0)
                .ToList();
        }

        public static IReadOnlyList<SalesStarContributionRow> BuildTopStars(
            IReadOnlyList<ProductClassificationRow> rows,
            decimal totalRevenue,
            int top = 10)
        {
            var stars = rows
                .Where(r => r.Class == ProductPerformanceClass.Star && r.Performance != null)
                .OrderByDescending(r => r.Performance!.RevenueTotal)
                .ThenBy(r => r.ProductName)
                .Take(top <= 0 ? 10 : top)
                .ToList();

            var result = new List<SalesStarContributionRow>(stars.Count);
            int rank = 0;
            foreach (ProductClassificationRow r in stars)
            {
                rank++;
                ProductPerformanceRow p = r.Performance!;
                result.Add(new SalesStarContributionRow
                {
                    Rank = rank,
                    ProductId = r.ProductId,
                    ProductName = r.ProductName,
                    Category = r.Category,
                    RevenueTotal = p.RevenueTotal,
                    RealizedProfit = p.RealizedProfit,
                    UnitsSold = p.UnitsSold,
                    RevenueSharePct = SalesAnalyticsMath.SharePct(p.RevenueTotal, totalRevenue),
                    FlagStockoutRisk = p.FlagStockoutRisk,
                    Trend = r.Trend
                });
            }

            return result;
        }

        public static IReadOnlyList<SalesCategoryMixRow> BuildCategoryMix(
            IReadOnlyList<ProductClassificationRow> rows,
            decimal totalRevenue)
        {
            return rows
                .Where(r => r.Performance != null)
                .GroupBy(r => string.IsNullOrWhiteSpace(r.Category) ? "(Sin categoría)" : r.Category)
                .Select(g => new
                {
                    Name = g.Key,
                    Revenue = g.Sum(x => x.Performance!.RevenueTotal)
                })
                .OrderByDescending(x => x.Revenue)
                .ThenBy(x => x.Name)
                .Select((x, i) => new SalesCategoryMixRow
                {
                    Rank = i + 1,
                    CategoryName = x.Name,
                    RevenueTotal = InventoryFinancialMath.RoundMoney(x.Revenue),
                    RevenueSharePct = SalesAnalyticsMath.SharePct(x.Revenue, totalRevenue)
                })
                .ToList();
        }

        public static SalesStarMixReport BuildReport(
            IReadOnlyList<ProductClassificationRow> rows,
            ProfitPeriodKind periodKind,
            int topStars = 10)
        {
            decimal totalRev = rows.Sum(r => r.Performance?.RevenueTotal ?? 0m);
            decimal totalProfit = rows.Sum(r => r.Performance?.RealizedProfit ?? 0m);
            int totalUnits = rows.Sum(r => r.Performance?.UnitsSold ?? 0);

            IReadOnlyList<SalesClassMixBucket> buckets =
                BuildClassBuckets(rows, totalRev, totalProfit, totalUnits);

            decimal? NullableShare(ProductPerformanceClass c)
            {
                SalesClassMixBucket? b = buckets.FirstOrDefault(x => x.Class == c);
                return b?.RevenueSharePct;
            }

            var top = BuildTopStars(rows, totalRev, topStars);
            var starBucket = buckets.FirstOrDefault(b => b.Class == ProductPerformanceClass.Star);

            return new SalesStarMixReport
            {
                PeriodKind = periodKind,
                TotalRevenue = InventoryFinancialMath.RoundMoney(totalRev),
                TotalRealizedProfit = InventoryFinancialMath.RoundMoney(totalProfit),
                TotalUnits = totalUnits,
                StarCount = starBucket?.ProductCount ?? 0,
                StarRevenue = starBucket?.RevenueTotal ?? 0m,
                StarRevenueSharePct = starBucket?.RevenueSharePct,
                StarProfitSharePct = starBucket?.ProfitSharePct,
                HealthyRevenueSharePct = NullableShare(ProductPerformanceClass.Healthy),
                SlowRevenueSharePct = NullableShare(ProductPerformanceClass.Slow),
                CriticalRevenueSharePct = NullableShare(ProductPerformanceClass.Critical),
                ClassBuckets = buckets,
                TopStars = top,
                StarsWithStockoutRisk = top.Where(s => s.FlagStockoutRisk).ToList(),
                CategoryMix = BuildCategoryMix(rows, totalRev),
                Caution = SalesStarMixPolicy.Separation
            };
        }
    }
}
