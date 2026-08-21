using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Ventas por día + mejor/peor (FASE 9.7).</summary>
    public class SalesByDayService
    {
        private readonly ProfitAnalyticsService _profit = new();

        public SalesByDayReport GetReport(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            DateTime? asOf = null)
        {
            ProfitPeriodRange range = ProfitAnalyticsService.ResolvePeriod(periodKind, asOf);
            IReadOnlyList<ProfitDayRow> days = _profit.GetByDay(periodKind, asOf);
            return SalesByDayComposer.Build(days, periodKind, range.From, range.ToExclusive);
        }
    }
}
