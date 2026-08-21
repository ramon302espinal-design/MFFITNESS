using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>
    /// Tendencias MoM por producto (FASE 8.11). Reutiliza ProfitAnalytics. Sin score.
    /// </summary>
    public class ProductTrendService
    {
        private readonly ProfitAnalyticsService _profit = new();

        public ProductTrendReport GetTrends(
            ProfitPeriodKind currentKind = ProfitPeriodKind.ThisMonth,
            DateTime? asOf = null,
            ProductTrendThresholds? thresholds = null)
        {
            var pair = ProductTrendMath.TryResolvePeriodPair(currentKind, asOf);
            if (pair == null)
            {
                return new ProductTrendReport
                {
                    CurrentPeriodKind = currentKind,
                    Rows = Array.Empty<ProductTrendRow>()
                };
            }

            var (currentRange, previousRange) = pair.Value;
            var currentRows = _profit.GetByProduct(currentRange.From, currentRange.ToExclusive);
            var previousRows = _profit.GetByProduct(previousRange.From, previousRange.ToExclusive);

            IReadOnlyList<ProductTrendRow> rows = ProductTrendMath.ComposeAll(
                currentRows, previousRows, thresholds);

            return new ProductTrendReport
            {
                CurrentPeriodKind = currentKind,
                CurrentFrom = currentRange.From,
                CurrentToExclusive = currentRange.ToExclusive,
                PreviousFrom = previousRange.From,
                PreviousToExclusive = previousRange.ToExclusive,
                ProductCount = rows.Count,
                GrowingCount = rows.Count(r => r.PrimaryTrend == ProductTrendDirection.Growing),
                StableCount = rows.Count(r => r.PrimaryTrend == ProductTrendDirection.Stable),
                DecliningCount = rows.Count(r => r.PrimaryTrend == ProductTrendDirection.Declining),
                InsufficientCount = rows.Count(r => r.PrimaryTrend == ProductTrendDirection.InsufficientData),
                Rows = rows
            };
        }

        public IReadOnlyList<ProductTrendRow> GetGrowing(
            ProfitPeriodKind currentKind = ProfitPeriodKind.ThisMonth,
            int top = 10,
            DateTime? asOf = null)
            => GetTrends(currentKind, asOf).Rows
                .Where(r => r.PrimaryTrend == ProductTrendDirection.Growing)
                .OrderByDescending(r => r.UnitsChangePct ?? 0m)
                .Take(top <= 0 ? 10 : top)
                .ToList();

        public IReadOnlyList<ProductTrendRow> GetDeclining(
            ProfitPeriodKind currentKind = ProfitPeriodKind.ThisMonth,
            int top = 10,
            DateTime? asOf = null)
            => GetTrends(currentKind, asOf).Rows
                .Where(r => r.PrimaryTrend == ProductTrendDirection.Declining)
                .OrderBy(r => r.UnitsChangePct ?? 0m)
                .Take(top <= 0 ? 10 : top)
                .ToList();
    }
}
