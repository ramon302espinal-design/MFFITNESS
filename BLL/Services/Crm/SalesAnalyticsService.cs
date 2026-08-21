using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>
    /// Orquestador de ventas (FASE 9.2). Reutiliza ProfitAnalytics — sin SQL duplicado.
    /// </summary>
    public class SalesAnalyticsService
    {
        private readonly ProfitAnalyticsService _profit = new();

        public SalesSummary GetSummary(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            DateTime? asOf = null,
            DateTime? customFrom = null,
            DateTime? customToExclusive = null)
        {
            ProfitSummary profit = _profit.GetForPeriod(
                periodKind, asOf, customFrom, customToExclusive);
            return SalesAnalyticsComposer.FromProfitSummary(profit, periodKind);
        }

        public SalesSummary GetSummary(
            DateTime? periodFrom,
            DateTime? periodToExclusive)
        {
            ProfitSummary profit = _profit.GetProfitSummary(periodFrom, periodToExclusive);
            return SalesAnalyticsComposer.FromProfitSummary(profit, ProfitPeriodKind.Custom);
        }

        /// <summary>Promedios/medianas diarias del período (FASE 9.6).</summary>
        public SalesDailyStatsReport GetDailyStats(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            DateTime? asOf = null)
        {
            ProfitPeriodRange range = ProfitAnalyticsService.ResolvePeriod(periodKind, asOf);
            IReadOnlyList<ProfitDayRow> days = _profit.GetByDay(periodKind, asOf);
            return SalesSeriesStatsComposer.FromDays(
                days, periodKind, range.From, range.ToExclusive);
        }

        public string GetPolicyNote()
            => SalesAnalyticsPolicy.SeparationRule + " " + SalesAnalyticsPolicy.VoidReturnNote;
    }
}
