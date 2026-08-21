using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Forecast / estimación de ventas (FASE 9.17).</summary>
    public class SalesForecastService
    {
        private readonly ProfitAnalyticsService _profit = new();
        private readonly SalesAnalyticsService _sales = new();

        public SalesForecastReport GetEstimate(
            ProfitPeriodKind sourcePeriodKind = ProfitPeriodKind.Last30Days,
            int horizonDays = 30,
            DateTime? asOf = null,
            SalesForecastThresholds? thresholds = null)
        {
            IReadOnlyList<ProfitDayRow> days = _profit.GetByDay(sourcePeriodKind, asOf);
            SalesSummary summary = _sales.GetSummary(sourcePeriodKind, asOf);
            decimal? margin = summary.HasReliableRealizedProfit ? summary.MarginPct : null;

            return SalesForecastMath.FromDays(
                days,
                horizonDays,
                margin,
                sourcePeriodKind,
                thresholds);
        }
    }
}
