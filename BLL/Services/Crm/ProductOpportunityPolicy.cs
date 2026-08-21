namespace BLL.Services.Crm
{
    /// <summary>Contrato PRODUCTO OPORTUNIDAD (FASE 8.14).</summary>
    public static class ProductOpportunityPolicy
    {
        public const string Definition =
            "OPORTUNIDAD = demanda Growing + eficiencia confiable + capital bajo/moderado + no inmovilizado. " +
            "≠ estrella actual. ≠ señal automática de compra. Sin score ponderado.";

        public const string GrowthRule =
            "Requiere PrimaryTrend = Growing (unidades MoM). Sin tendencia Growing → no Opportunity.";

        public const string EfficiencyRule =
            "Margen % ≥ umbral o ROI % ≥ umbral (confiables). Umbrales pueden ser menores que Star.";

        public const string CapitalRule =
            "Capital inventario ≤ MaxInventoryCapital y stock ≤ MaxStock (moderado). " +
            "Alto capital con crecimiento = revisar aparte (no Opportunity automática).";

        public const string VsStar =
            "Star tiene prioridad en el clasificador. Opportunity es emergente / reinversión potencial.";

        public const string NoAutoBuy =
            "Genera señal de oportunidad. NO recomienda comprar automáticamente.";
    }

    public sealed class ProductOpportunityThresholds
    {
        public static ProductOpportunityThresholds Default { get; } = new();

        public decimal MinMarginPct { get; init; } = 12m;
        public decimal MinRoiPct { get; init; } = 15m;
        public decimal MaxInventoryCapital { get; init; } = 25_000m;
        public int MaxStock { get; init; } = 80;
    }
}
