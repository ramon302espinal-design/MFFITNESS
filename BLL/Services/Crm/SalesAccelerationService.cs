using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Aceleración / desaceleración de ventas (FASE 9.15).</summary>
    public class SalesAccelerationService
    {
        private readonly ProfitAnalyticsService _profit = new();

        public SalesAccelerationReport GetReport(
            ProfitPeriodKind periodKind = ProfitPeriodKind.Last30Days,
            DateTime? asOf = null,
            SalesAccelerationThresholds? thresholds = null)
        {
            IReadOnlyList<ProfitDayRow> days = _profit.GetByDay(periodKind, asOf);
            return SalesAccelerationMath.FromDays(days, periodKind, thresholds);
        }
    }
}
