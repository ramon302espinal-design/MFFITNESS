using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Integración ventas ↔ productos estrella / mix de clase (FASE 9.20).</summary>
    public class SalesStarMixService
    {
        private readonly ProductClassificationService _classification = new();

        public SalesStarMixReport GetReport(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            DateTime? asOf = null,
            int topStars = 10)
        {
            ProductClassificationReport classification = _classification.GetReport(periodKind, asOf);
            return SalesStarMixMath.BuildReport(classification.Rows, periodKind, topStars);
        }
    }
}
