using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Clasificación pura de tendencia MoM (FASE 8.11). Sin I/O.</summary>
    public static class ProductTrendMath
    {
        public static decimal? ChangePct(decimal current, decimal previous)
        {
            if (previous == 0)
                return current == 0 ? null : 100m;
            return InventoryFinancialMath.RoundPct((current - previous) / previous * 100m);
        }

        public static ProductTrendDirection Classify(
            decimal current,
            decimal previous,
            ProductTrendThresholds? thresholds = null)
        {
            ProductTrendThresholds t = thresholds ?? ProductTrendThresholds.Default;

            if (current <= 0 && previous <= 0)
                return ProductTrendDirection.InsufficientData;

            if (previous <= 0 && current > 0)
                return ProductTrendDirection.Growing;

            if (previous > 0 && current <= 0)
                return ProductTrendDirection.Declining;

            decimal? pct = ChangePct(current, previous);
            if (!pct.HasValue)
                return ProductTrendDirection.InsufficientData;

            if (Math.Abs(pct.Value) <= t.StableBandPct)
                return ProductTrendDirection.Stable;

            return pct.Value > 0
                ? ProductTrendDirection.Growing
                : ProductTrendDirection.Declining;
        }

        public static ProductTrendRow Compose(
            int productId,
            string productName,
            string category,
            int unitsCurrent,
            int unitsPrevious,
            decimal revenueCurrent,
            decimal revenuePrevious,
            ProductTrendThresholds? thresholds = null)
        {
            var unitsTrend = Classify(unitsCurrent, unitsPrevious, thresholds);
            var revenueTrend = Classify(revenueCurrent, revenuePrevious, thresholds);

            return new ProductTrendRow
            {
                ProductId = productId,
                ProductName = productName,
                Category = category,
                UnitsCurrent = unitsCurrent,
                UnitsPrevious = unitsPrevious,
                RevenueCurrent = revenueCurrent,
                RevenuePrevious = revenuePrevious,
                UnitsChangePct = ChangePct(unitsCurrent, unitsPrevious),
                RevenueChangePct = ChangePct(revenueCurrent, revenuePrevious),
                UnitsTrend = unitsTrend,
                RevenueTrend = revenueTrend,
                PrimaryTrend = unitsTrend,
                Acceleration = ProductAccelerationKind.Unknown
            };
        }

        /// <summary>Une filas de P&amp;L actual vs previo por ProductId.</summary>
        public static IReadOnlyList<ProductTrendRow> ComposeAll(
            IEnumerable<ProfitGroupRow> current,
            IEnumerable<ProfitGroupRow> previous,
            ProductTrendThresholds? thresholds = null)
        {
            var cur = current
                .Where(p => p.ProductId.HasValue)
                .GroupBy(p => p.ProductId!.Value)
                .ToDictionary(g => g.Key, g => g.First());

            var prev = previous
                .Where(p => p.ProductId.HasValue)
                .GroupBy(p => p.ProductId!.Value)
                .ToDictionary(g => g.Key, g => g.First());

            var ids = cur.Keys.Union(prev.Keys);
            var rows = new List<ProductTrendRow>();

            foreach (int id in ids)
            {
                cur.TryGetValue(id, out ProfitGroupRow? c);
                prev.TryGetValue(id, out ProfitGroupRow? p);

                string name = c?.ProductName ?? p?.ProductName ?? id.ToString();
                rows.Add(Compose(
                    id,
                    name,
                    category: string.Empty,
                    unitsCurrent: c?.UnitsSold ?? 0,
                    unitsPrevious: p?.UnitsSold ?? 0,
                    revenueCurrent: c?.RevenueTotal ?? 0m,
                    revenuePrevious: p?.RevenueTotal ?? 0m,
                    thresholds));
            }

            return rows
                .OrderBy(r => r.ProductName, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Resuelve par (actual, base) para tendencia. Null si el preset no tiene par definido.
        /// </summary>
        public static (ProfitPeriodRange Current, ProfitPeriodRange Previous)? TryResolvePeriodPair(
            ProfitPeriodKind kind,
            DateTime? asOf = null)
        {
            DateTime d = (asOf ?? DateTime.Today).Date;
            ProfitPeriodRange current = ProfitAnalyticsService.ResolvePeriod(kind, asOf);

            return kind switch
            {
                ProfitPeriodKind.ThisMonth => (
                    current,
                    ProfitAnalyticsService.ResolvePeriod(ProfitPeriodKind.PreviousMonth, asOf)),

                ProfitPeriodKind.Today => (
                    current,
                    ProfitAnalyticsService.ResolvePeriod(ProfitPeriodKind.Yesterday, asOf)),

                ProfitPeriodKind.Last7Days => (
                    current,
                    new ProfitPeriodRange(d.AddDays(-13), d.AddDays(-6))),

                ProfitPeriodKind.Last14Days => (
                    current,
                    new ProfitPeriodRange(d.AddDays(-27), d.AddDays(-13))),

                ProfitPeriodKind.Last30Days => (
                    current,
                    new ProfitPeriodRange(d.AddDays(-59), d.AddDays(-29))),

                ProfitPeriodKind.ThisQuarter => (
                    current,
                    ResolvePreviousQuarter(d)),

                ProfitPeriodKind.ThisSemester => (
                    current,
                    ResolvePreviousSemester(d)),

                ProfitPeriodKind.ThisYear => (
                    current,
                    ProfitAnalyticsService.ResolvePeriod(ProfitPeriodKind.PreviousYear, asOf)),

                _ => null
            };
        }

        private static ProfitPeriodRange ResolvePreviousQuarter(DateTime d)
        {
            int quarterIndex = (d.Month - 1) / 3;
            int startMonth = quarterIndex * 3 + 1;
            var thisQ = new DateTime(d.Year, startMonth, 1);
            return new ProfitPeriodRange(thisQ.AddMonths(-3), thisQ);
        }

        private static ProfitPeriodRange ResolvePreviousSemester(DateTime d)
        {
            int startMonth = d.Month <= 6 ? 1 : 7;
            var thisH = new DateTime(d.Year, startMonth, 1);
            return new ProfitPeriodRange(thisH.AddMonths(-6), thisH);
        }
    }
}
