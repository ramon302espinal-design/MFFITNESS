using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato participación / concentración (FASE 9.12).</summary>
    public static class SalesSharePolicy
    {
        public const string Definition =
            "PARTICIPACIÓN % = Monto ítem / Total × 100. " +
            "Ej.: producto RD$100k / total RD$500k = 20%. Total ≤ 0 → N/D.";

        public const string Concentration =
            "CONCENTRACIÓN = Σ participación de Top N. " +
            "Ej.: Top 5 = 70% → alta concentración. No es automáticamente malo; es estratégico. " +
            "Pareto 80/20 formal = 9.13.";

        public const string Metrics =
            "Participación se calcula por métrica etiquetada: Ingresos | Ganancia | Unidades. " +
            "No mezclar.";
    }

    /// <summary>Composición pura de participación (FASE 9.12).</summary>
    public static class SalesShareComposer
    {
        public static SalesShareReport FromAmounts(
            IEnumerable<(string Name, int? ProductId, int? CategoryId, decimal Amount)> items,
            SalesShareMetric metric,
            ProfitPeriodKind periodKind,
            int topN = 5)
        {
            var list = items
                .Where(i => i.Amount > 0)
                .OrderByDescending(i => i.Amount)
                .ThenBy(i => i.Name)
                .ToList();

            decimal total = list.Sum(i => i.Amount);
            int n = topN <= 0 ? 5 : topN;

            var rows = new List<SalesShareRow>(list.Count);
            decimal cumulative = 0m;
            int rank = 0;
            foreach (var i in list)
            {
                rank++;
                decimal? share = SalesAnalyticsMath.SharePct(i.Amount, total);
                if (share.HasValue)
                    cumulative = InventoryFinancialMath.RoundPct(cumulative + share.Value);

                rows.Add(new SalesShareRow
                {
                    Rank = rank,
                    Name = i.Name,
                    ProductId = i.ProductId,
                    CategoryId = i.CategoryId,
                    Amount = InventoryFinancialMath.RoundMoney(i.Amount),
                    SharePct = share,
                    CumulativeSharePct = share.HasValue ? cumulative : null
                });
            }

            decimal? topShare = total <= 0
                ? null
                : SalesAnalyticsMath.SharePct(
                    list.Take(n).Sum(i => i.Amount),
                    total);

            return new SalesShareReport
            {
                PeriodKind = periodKind,
                MetricLabel = metric switch
                {
                    SalesShareMetric.RealizedProfit => "Ganancia",
                    SalesShareMetric.Units => "Unidades",
                    _ => "Ingresos"
                },
                TotalAmount = InventoryFinancialMath.RoundMoney(total),
                ItemCount = rows.Count,
                TopN = n,
                TopNSharePct = topShare,
                Items = rows
            };
        }

        public static SalesShareReport FromProducts(
            IReadOnlyList<SalesProductRow> products,
            SalesShareMetric metric,
            ProfitPeriodKind periodKind,
            int topN = 5)
        {
            return FromAmounts(
                products.Select(p => (
                    p.ProductName,
                    (int?)p.ProductId,
                    (int?)null,
                    metric switch
                    {
                        SalesShareMetric.RealizedProfit => p.RealizedProfit,
                        SalesShareMetric.Units => p.UnitsSold,
                        _ => p.RevenueTotal
                    })),
                metric,
                periodKind,
                topN);
        }

        public static SalesShareReport FromCategories(
            IReadOnlyList<SalesCategoryRow> categories,
            SalesShareMetric metric,
            ProfitPeriodKind periodKind,
            int topN = 5)
        {
            return FromAmounts(
                categories.Select(c => (
                    c.CategoryName,
                    (int?)null,
                    c.CategoryId,
                    metric switch
                    {
                        SalesShareMetric.RealizedProfit => c.RealizedProfit,
                        SalesShareMetric.Units => c.UnitsSold,
                        _ => c.RevenueTotal
                    })),
                metric,
                periodKind,
                topN);
        }
    }
}
