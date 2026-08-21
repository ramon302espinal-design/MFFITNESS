using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Ticket promedio + comparación (FASE 9.9).</summary>
    public class SalesTicketService
    {
        private readonly SalesAnalyticsService _sales = new();

        public SalesTicketReport GetReport(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            DateTime? asOf = null)
        {
            ProfitPeriodRange currentRange = ProfitAnalyticsService.ResolvePeriod(periodKind, asOf);
            SalesSummary current = _sales.GetSummary(periodKind, asOf);

            var pair = ProductTrendMath.TryResolvePeriodPair(periodKind, asOf);
            if (pair == null)
            {
                return SalesTicketComposer.Build(
                    periodKind, current, previous: null, currentRange, previousRange: null);
            }

            SalesSummary previous = _sales.GetSummary(
                pair.Value.Previous.From, pair.Value.Previous.ToExclusive);

            return SalesTicketComposer.Build(
                periodKind,
                current,
                previous,
                pair.Value.Current,
                pair.Value.Previous);
        }
    }
}
