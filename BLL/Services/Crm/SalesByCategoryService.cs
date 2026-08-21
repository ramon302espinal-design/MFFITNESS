using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Ventas por categoría + tendencia agregada (FASE 9.11).</summary>
    public class SalesByCategoryService
    {
        private readonly ProfitAnalyticsService _profit = new();

        public SalesByCategoryReport GetReport(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            DateTime? asOf = null,
            int? top = null)
        {
            IReadOnlyList<ProfitGroupRow> categories = _profit.GetByCategory(periodKind, asOf, top);

            var pair = ProductTrendMath.TryResolvePeriodPair(periodKind, asOf);
            IReadOnlyList<ProfitGroupRow> curProducts = Array.Empty<ProfitGroupRow>();
            IReadOnlyList<ProfitGroupRow> prevProducts = Array.Empty<ProfitGroupRow>();

            if (pair != null)
            {
                curProducts = _profit.GetByProduct(pair.Value.Current.From, pair.Value.Current.ToExclusive);
                prevProducts = _profit.GetByProduct(pair.Value.Previous.From, pair.Value.Previous.ToExclusive);
            }

            return SalesByCategoryComposer.Build(
                categories, curProducts, prevProducts, periodKind);
        }
    }
}
