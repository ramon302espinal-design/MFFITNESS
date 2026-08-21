using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Tendencias multi-punto de ventas (FASE 9.14).</summary>
    public class SalesSeriesTrendService
    {
        private readonly ProfitAnalyticsService _profit = new();

        public SalesSeriesTrendReport GetReport(
            ProfitPeriodKind periodKind = ProfitPeriodKind.Last30Days,
            DateTime? asOf = null,
            SalesSeriesTrendThresholds? thresholds = null)
        {
            IReadOnlyList<ProfitDayRow> days = _profit.GetByDay(periodKind, asOf);
            return SalesSeriesTrendMath.FromDays(days, periodKind, thresholds);
        }
    }
}
