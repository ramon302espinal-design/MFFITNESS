using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>
    /// Orquestador de variaciones (FASE 9.5). Reutiliza SalesComparisonService.
    /// </summary>
    public class SalesVariationService
    {
        private readonly SalesComparisonService _comparison = new();

        public SalesVariationReport? GetVariations(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            DateTime? asOf = null,
            SalesVariationThresholds? thresholds = null)
        {
            SalesComparisonReport? cmp = _comparison.GetComparison(periodKind, asOf);
            if (cmp == null)
                return null;
            return SalesVariationMath.FromComparison(cmp, thresholds);
        }
    }
}
