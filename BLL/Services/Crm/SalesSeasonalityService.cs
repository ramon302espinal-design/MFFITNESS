using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Estacionalidad y comparación YoY (FASE 9.16).</summary>
    public class SalesSeasonalityService
    {
        private readonly SalesAnalyticsService _sales = new();
        private readonly ProfitAnalyticsService _profit = new();

        public SalesSeasonalityReport GetSameMonthYoY(DateTime? asOf = null)
        {
            DateTime d = (asOf ?? DateTime.Today).Date;
            var (currentRange, priorRange) = SalesSeasonalityMath.ResolveSameMonthYoY(d);

            SalesSummary current = _sales.GetSummary(currentRange.From, currentRange.ToExclusive);
            SalesSummary prior = _sales.GetSummary(priorRange.From, priorRange.ToExclusive);

            SalesComparisonReport? sequential = null;
            var momPair = ProductTrendMath.TryResolvePeriodPair(ProfitPeriodKind.ThisMonth, d);
            if (momPair != null)
            {
                SalesSummary momPrev = _sales.GetSummary(
                    momPair.Value.Previous.From, momPair.Value.Previous.ToExclusive);
                sequential = SalesComparisonComposer.Build(
                    ProfitPeriodKind.ThisMonth,
                    momPair.Value.Current,
                    momPair.Value.Previous,
                    current,
                    momPrev);
            }

            IReadOnlyList<ProfitDayRow> days = _profit.GetByDay(ProfitPeriodKind.Last30Days, d);
            var dow = SalesSeasonalityMath.BuildDayOfWeekProfile(days);

            return SalesSeasonalityMath.Compose(
                SalesSeasonalityMode.SameMonthYoY,
                d,
                currentRange,
                priorRange,
                current,
                prior,
                sequential,
                dow);
        }

        public SalesSeasonalityReport GetSameWeekYoY(DateTime? asOf = null)
        {
            DateTime d = (asOf ?? DateTime.Today).Date;
            var (currentRange, priorRange) = SalesSeasonalityMath.ResolveSameWeekYoY(d);
            return BuildSimple(SalesSeasonalityMode.SameWeekYoY, d, currentRange, priorRange);
        }

        public SalesSeasonalityReport GetSameCalendarDayYoY(DateTime? asOf = null)
        {
            DateTime d = (asOf ?? DateTime.Today).Date;
            var (currentRange, priorRange) = SalesSeasonalityMath.ResolveSameCalendarDayYoY(d);
            return BuildSimple(SalesSeasonalityMode.SameCalendarDayYoY, d, currentRange, priorRange);
        }

        private SalesSeasonalityReport BuildSimple(
            SalesSeasonalityMode mode,
            DateTime asOf,
            ProfitPeriodRange currentRange,
            ProfitPeriodRange priorRange)
        {
            SalesSummary current = _sales.GetSummary(currentRange.From, currentRange.ToExclusive);
            SalesSummary prior = _sales.GetSummary(priorRange.From, priorRange.ToExclusive);
            return SalesSeasonalityMath.Compose(
                mode, asOf, currentRange, priorRange, current, prior);
        }
    }
}
