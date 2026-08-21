namespace BLL.Services.Crm
{
    /// <summary>Contrato PRODUCTO EN RIESGO (FASE 8.15). Clase = Critical.</summary>
    public static class ProductRiskPolicy
    {
        public const string Definition =
            "RIESGO (Critical) = capital en peligro de no rotar / perder valor. " +
            "≠ todo Frozen. ≠ quiebre de stock (problema opuesto). Sin score ponderado.";

        public const string Rules =
            "Critical si: HealthStatus Critical; o Frozen/inmovilizado con potencial&lt;0; " +
            "o inmovilizado con idle ≥ umbral y capital material; " +
            "o capital material + Declining fuerte + impacto de ganancia bajo/nulo.";

        public const string NotStockout =
            "FlagStockoutRisk solo NO clasifica Critical (es falta de inventario). " +
            "Puede anotarse como alerta operativa aparte.";

        public const string VsFrozen =
            "Frozen sin agravantes → Slow (observar). Critical exige agravante de riesgo.";

        public const string Explainability =
            "Reasons detallan por qué es riesgo. UI: 🔴 Producto en riesgo.";
    }

    public sealed class ProductRiskThresholds
    {
        public static ProductRiskThresholds Default { get; } = new();

        public int CriticalIdleDays { get; init; } = 60;
        public decimal MinMaterialCapital { get; init; } = 1_000m;
        public decimal StrongDeclinePct { get; init; } = -25m;
        public decimal MaxWeakProfit { get; init; } = 1_000m;
    }
}
