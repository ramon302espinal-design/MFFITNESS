namespace BLL.Services.Crm
{
    /// <summary>
    /// Contrato PRODUCTO ESTRELLA (FASE 8.13). Checklist de pilares — no score ponderado.
    /// </summary>
    public static class ProductStarPolicy
    {
        public const string Definition =
            "ESTRELLA = impacto + eficiencia + bajo riesgo (tres pilares). " +
            "≠ más vendido solo. ≠ Opportunity. Sin pesos ni score numérico en 8.13.";

        public const string ImpactPillar =
            "IMPACTO: ganancia realizada ≥ umbral, o ingresos ≥ umbral, o unidades ≥ umbral " +
            "(HasReliableRealizedProfit para ganancia).";

        public const string EfficiencyPillar =
            "EFICIENCIA: margen % ≥ umbral o ROI producto % ≥ umbral (confiables). " +
            "Y señal de rotación (UnitsPerDay&gt;0 o TurnoverProxy&gt;0 o ventas en período).";

        public const string RiskPillar =
            "BAJO RIESGO: no Critical/Frozen/Slow inmovilizado; no potencial&lt;0; " +
            "tendencia no Declining fuerte. StockoutRisk no bloquea estrella " +
            "(alerta de reabastecimiento, §49).";

        public const string Exclusions =
            "Excluye: New, InsufficientData, Critical, sin actividad ni inventario.";

        public const string Explainability =
            "Reasons listan cada pilar cumplido. UI debe poder explicar por qué es estrella.";
    }

    /// <summary>Umbrales del checklist estrella (FASE 8.13). Ajustables; no son pesos.</summary>
    public sealed class ProductStarThresholds
    {
        public static ProductStarThresholds Default { get; } = new();

        public decimal MinRealizedProfit { get; init; } = 5_000m;
        public decimal MinRevenue { get; init; } = 20_000m;
        public int MinUnitsSold { get; init; } = 30;

        public decimal MinMarginPct { get; init; } = 15m;
        public decimal MinRoiPct { get; init; } = 20m;

        public decimal StrongDeclinePct { get; init; } = -25m;
    }
}
