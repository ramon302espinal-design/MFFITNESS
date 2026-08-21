using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>
    /// Orquestador Dashboard FASE 8.18 — una pasada de clasificación + tops etiquetados.
    /// </summary>
    public class ProductPerformanceDashboardService
    {
        private readonly ProductClassificationService _classification = new();

        public ProductPerformanceDashboardReport GetReport(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            DateTime? asOf = null,
            int topLists = 5)
        {
            ProductClassificationReport classification = _classification.GetReport(periodKind, asOf);
            return ProductPerformanceDashboardComposer.Build(classification, topLists);
        }
    }
}
