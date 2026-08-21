using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato ventas por categoría (FASE 9.11).</summary>
    public static class SalesByCategoryPolicy
    {
        public const string Definition =
            "FASE 9.11: por categoría — Unidades, Ingresos, Ganancia, Margen, ROI, " +
            "Transacciones, Ticket, participación, tendencia agregada. " +
            "Ingresos ≠ ganancia (mejor categoría por ingreso puede ≠ mejor por ganancia).";

        public const string TrendNote =
            "Tendencia de categoría = MoM de Σ unidades de productos de la categoría " +
            "(agregado simple). Capital por categoría = 9.19 / FASE 7.";
    }

    /// <summary>Composición pura ventas×categoría (FASE 9.11).</summary>
    public static class SalesByCategoryComposer
    {
        public static SalesCategoryRow Compose(
            ProfitGroupRow category,
            decimal totalRevenue,
            decimal totalProfit,
            ProductTrendDirection? trend,
            decimal? unitsChangePct,
            int rank)
        {
            int txns = category.TransactionCount;
            return new SalesCategoryRow
            {
                Rank = rank,
                CategoryId = category.CategoryId,
                CategoryName = category.GroupName,
                UnitsSold = category.UnitsSold,
                RevenueTotal = category.RevenueTotal,
                RealizedProfit = category.RealizedProfit,
                MarginPct = category.MarginPct,
                RoiPct = category.RoiPct,
                TransactionCount = txns,
                AverageTicket = SalesAnalyticsMath.AverageTicket(category.RevenueTotal, txns),
                RevenueSharePct = SalesAnalyticsMath.SharePct(category.RevenueTotal, totalRevenue),
                ProfitSharePct = SalesAnalyticsMath.SharePct(category.RealizedProfit, totalProfit),
                Trend = trend,
                UnitsChangePct = unitsChangePct
            };
        }

        /// <summary>
        /// Agrega tendencia MoM por categoría sumando unidades current/previous de productos.
        /// </summary>
        public static (ProductTrendDirection Direction, decimal? ChangePct) CategoryTrend(
            int? categoryId,
            IReadOnlyList<ProfitGroupRow> productsCurrent,
            IReadOnlyList<ProfitGroupRow> productsPrevious)
        {
            if (!categoryId.HasValue)
                return (ProductTrendDirection.InsufficientData, null);

            int cur = productsCurrent
                .Where(p => p.CategoryId == categoryId)
                .Sum(p => p.UnitsSold);
            int prev = productsPrevious
                .Where(p => p.CategoryId == categoryId)
                .Sum(p => p.UnitsSold);

            return (
                ProductTrendMath.Classify(cur, prev),
                ProductTrendMath.ChangePct(cur, prev));
        }

        public static SalesByCategoryReport Build(
            IReadOnlyList<ProfitGroupRow> categories,
            IReadOnlyList<ProfitGroupRow>? productsCurrent = null,
            IReadOnlyList<ProfitGroupRow>? productsPrevious = null,
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth)
        {
            decimal totalRevenue = categories.Sum(c => c.RevenueTotal);
            decimal totalProfit = categories.Sum(c => c.RealizedProfit);
            productsCurrent ??= Array.Empty<ProfitGroupRow>();
            productsPrevious ??= Array.Empty<ProfitGroupRow>();

            var rows = new List<SalesCategoryRow>(categories.Count);
            int rank = 0;
            foreach (ProfitGroupRow c in categories
                         .OrderByDescending(x => x.RevenueTotal)
                         .ThenByDescending(x => x.RealizedProfit)
                         .ThenBy(x => x.GroupName))
            {
                rank++;
                var (dir, pct) = CategoryTrend(c.CategoryId, productsCurrent, productsPrevious);
                rows.Add(Compose(c, totalRevenue, totalProfit, dir, pct, rank));
            }

            return new SalesByCategoryReport
            {
                PeriodKind = periodKind,
                CategoryCount = rows.Count,
                TotalRevenue = InventoryFinancialMath.RoundMoney(totalRevenue),
                TotalRealizedProfit = InventoryFinancialMath.RoundMoney(totalProfit),
                Categories = rows
            };
        }
    }
}
