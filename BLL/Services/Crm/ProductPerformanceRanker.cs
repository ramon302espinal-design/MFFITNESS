using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>
    /// Rankings de performance por UNA métrica (FASE 8.3–8.5). Sin score compuesto.
    /// </summary>
    public static class ProductPerformanceRanker
    {
        /// <summary>
        /// 8.3 UnitsSold · 8.4 Revenue · 8.5 RealizedProfit. Otros = 8.6+.
        /// </summary>
        public static IReadOnlyList<ProductPerformanceRankRow> Rank(
            IEnumerable<ProductPerformanceRow> source,
            ProductPerformanceMetricKind kind,
            int top = 10)
        {
            if (top <= 0)
                top = 10;

            return kind switch
            {
                ProductPerformanceMetricKind.UnitsSold => RankUnitsSold(source, top),
                ProductPerformanceMetricKind.Revenue => RankRevenue(source, top),
                ProductPerformanceMetricKind.RealizedProfit => RankProfit(source, top),
                ProductPerformanceMetricKind.MarginPct => RankMargin(source, top),
                ProductPerformanceMetricKind.RoiPct => RankRoi(source, top),
                ProductPerformanceMetricKind.TurnoverProxy => RankTurnover(source, top),
                ProductPerformanceMetricKind.UnitsPerDay => RankUnitsPerDay(source, top),
                ProductPerformanceMetricKind.InventoryCapital => RankInventoryCapital(source, top),
                ProductPerformanceMetricKind.ImmobilizedCapital => RankImmobilized(source, top),
                ProductPerformanceMetricKind.PotentialProfit => RankPotential(source, top),
                _ => throw new NotSupportedException(
                    $"Ranking {kind} no reconocido.")
            };
        }

        private static IReadOnlyList<ProductPerformanceRankRow> RankUnitsSold(
            IEnumerable<ProductPerformanceRow> source,
            int top)
        {
            var ordered = source
                .Where(r => r.UnitsSold > 0)
                .OrderByDescending(r => r.UnitsSold)
                .ThenByDescending(r => r.RevenueTotal)
                .ThenBy(r => r.ProductName, StringComparer.CurrentCultureIgnoreCase);

            return TakeRanked(ordered, top, ProductPerformanceMetricKind.UnitsSold,
                r => $"{r.UnitsSold:N0} uds",
                r => r.UnitsSold);
        }

        private static IReadOnlyList<ProductPerformanceRankRow> RankRevenue(
            IEnumerable<ProductPerformanceRow> source,
            int top)
        {
            var ordered = source
                .Where(r => r.RevenueTotal > 0)
                .OrderByDescending(r => r.RevenueTotal)
                .ThenByDescending(r => r.UnitsSold)
                .ThenBy(r => r.ProductName, StringComparer.CurrentCultureIgnoreCase);

            return TakeRanked(ordered, top, ProductPerformanceMetricKind.Revenue,
                r => $"Ingresos {r.RevenueTotal:N2}",
                r => r.RevenueTotal);
        }

        private static IReadOnlyList<ProductPerformanceRankRow> RankProfit(
            IEnumerable<ProductPerformanceRow> source,
            int top)
        {
            var ordered = source
                .Where(r => r.HasReliableRealizedProfit)
                .OrderByDescending(r => r.RealizedProfit)
                .ThenByDescending(r => r.RevenueTotal)
                .ThenBy(r => r.ProductName, StringComparer.CurrentCultureIgnoreCase);

            return TakeRanked(ordered, top, ProductPerformanceMetricKind.RealizedProfit,
                r => $"Ganancia {r.RealizedProfit:N2}",
                r => r.RealizedProfit);
        }

        private static IReadOnlyList<ProductPerformanceRankRow> RankMargin(
            IEnumerable<ProductPerformanceRow> source,
            int top)
        {
            var ordered = source
                .Where(r => r.HasReliableRealizedProfit && r.MarginPct.HasValue)
                .OrderByDescending(r => r.MarginPct)
                .ThenByDescending(r => r.RealizedProfit)
                .ThenBy(r => r.ProductName, StringComparer.CurrentCultureIgnoreCase);

            return TakeRanked(ordered, top, ProductPerformanceMetricKind.MarginPct,
                r => $"Margen {r.MarginPct:N2} %",
                r => r.MarginPct);
        }

        private static IReadOnlyList<ProductPerformanceRankRow> RankRoi(
            IEnumerable<ProductPerformanceRow> source,
            int top)
        {
            var ordered = source
                .Where(r => r.HasReliableRealizedProfit
                    && r.RoiPct.HasValue
                    && r.Cogs > 0)
                .OrderByDescending(r => r.RoiPct)
                .ThenByDescending(r => r.RealizedProfit)
                .ThenBy(r => r.ProductName, StringComparer.CurrentCultureIgnoreCase);

            return TakeRanked(ordered, top, ProductPerformanceMetricKind.RoiPct,
                r => $"ROI {r.RoiPct:N2} %",
                r => r.RoiPct);
        }

        private static IReadOnlyList<ProductPerformanceRankRow> RankTurnover(
            IEnumerable<ProductPerformanceRow> source,
            int top)
        {
            var ordered = source
                .Where(r => r.TurnoverProxy.HasValue)
                .OrderByDescending(r => r.TurnoverProxy)
                .ThenByDescending(r => r.UnitsPerDay ?? 0m)
                .ThenBy(r => r.ProductName, StringComparer.CurrentCultureIgnoreCase);

            return TakeRanked(ordered, top, ProductPerformanceMetricKind.TurnoverProxy,
                r => $"Turnover proxy {r.TurnoverProxy:N2}",
                r => r.TurnoverProxy);
        }

        private static IReadOnlyList<ProductPerformanceRankRow> RankUnitsPerDay(
            IEnumerable<ProductPerformanceRow> source,
            int top)
        {
            var ordered = source
                .Where(r => r.UnitsPerDay.HasValue && r.UnitsPerDay.Value > 0)
                .OrderByDescending(r => r.UnitsPerDay)
                .ThenByDescending(r => r.UnitsSold)
                .ThenBy(r => r.ProductName, StringComparer.CurrentCultureIgnoreCase);

            return TakeRanked(ordered, top, ProductPerformanceMetricKind.UnitsPerDay,
                r => $"{r.UnitsPerDay:N2} uds/día",
                r => r.UnitsPerDay);
        }

        private static IReadOnlyList<ProductPerformanceRankRow> RankInventoryCapital(
            IEnumerable<ProductPerformanceRow> source,
            int top)
        {
            var ordered = source
                .Where(r => r.InventoryCapital > 0)
                .OrderByDescending(r => r.InventoryCapital)
                .ThenByDescending(r => r.Stock)
                .ThenBy(r => r.ProductName, StringComparer.CurrentCultureIgnoreCase);

            return TakeRanked(ordered, top, ProductPerformanceMetricKind.InventoryCapital,
                r => $"Capital {r.InventoryCapital:N2}",
                r => r.InventoryCapital);
        }

        private static IReadOnlyList<ProductPerformanceRankRow> RankImmobilized(
            IEnumerable<ProductPerformanceRow> source,
            int top)
        {
            var ordered = source
                .Where(r => r.IsImmobilized && r.ImmobilizedCapital > 0)
                .OrderByDescending(r => r.ImmobilizedCapital)
                .ThenByDescending(r => r.IdleDays ?? 0)
                .ThenBy(r => r.ProductName, StringComparer.CurrentCultureIgnoreCase);

            return TakeRanked(ordered, top, ProductPerformanceMetricKind.ImmobilizedCapital,
                r => $"Congelado {r.ImmobilizedCapital:N2} ({r.HealthStatus})",
                r => r.ImmobilizedCapital);
        }

        private static IReadOnlyList<ProductPerformanceRankRow> RankPotential(
            IEnumerable<ProductPerformanceRow> source,
            int top)
        {
            var ordered = source
                .Where(r => r.HasInventorySnapshot && r.InventoryCapital > 0)
                .OrderByDescending(r => r.PotentialProfit)
                .ThenByDescending(r => r.InventoryCapital)
                .ThenBy(r => r.ProductName, StringComparer.CurrentCultureIgnoreCase);

            return TakeRanked(ordered, top, ProductPerformanceMetricKind.PotentialProfit,
                r => $"Potencial {r.PotentialProfit:N2}",
                r => r.PotentialProfit);
        }

        private static IReadOnlyList<ProductPerformanceRankRow> TakeRanked(
            IEnumerable<ProductPerformanceRow> ordered,
            int top,
            ProductPerformanceMetricKind kind,
            Func<ProductPerformanceRow, string> label,
            Func<ProductPerformanceRow, decimal?> value)
        {
            int rank = 0;
            return ordered.Take(top).Select(r =>
            {
                rank++;
                return new ProductPerformanceRankRow
                {
                    Rank = rank,
                    Kind = kind,
                    Row = r,
                    MetricLabel = label(r),
                    MetricValue = value(r)
                };
            }).ToList();
        }
    }
}
