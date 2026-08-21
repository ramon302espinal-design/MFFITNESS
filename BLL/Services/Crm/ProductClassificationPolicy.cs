namespace BLL.Services.Crm
{
    /// <summary>Contrato de clasificación de productos (FASE 8.12–8.13).</summary>
    public static class ProductClassificationPolicy
    {
        public const string TaxonomyDefinition =
            "Clases: New, Healthy, Opportunity, Slow, Critical, InsufficientData, Star. " +
            "Star = checklist 8.13. Sin score ponderado.";

        public const string PriorityOrder =
            "Orden: InsufficientData → Critical → New → Slow → Star(8.13) → Opportunity → Healthy.";

        public const string CriticalRule =
            "Critical / riesgo (FASE 8.15): ver ProductRiskPolicy. " +
            "Health Critical; Frozen+potencial&lt;0; idle largo inmovilizado; " +
            "capital material + Declining/idle + ganancia débil. Stockout solo ≠ Critical.";

        public const string SlowRule =
            "Slow: HealthStatus Slow o Frozen (sin agravantes Critical), o Declining fuerte " +
            "con capital material.";

        public const string NewRule =
            "New: HealthStatus New (gracia post-entrada FASE 7). No penalizar como Slow/Critical.";

        public const string OpportunityRule =
            "Opportunity (FASE 8.14): Growing + margen/ROI ≥ umbral + capital/stock moderados + " +
            "no inmovilizado. Ver ProductOpportunityPolicy. ≠ estrella ≠ auto-compra.";

        public const string HealthyRule =
            "Healthy: HealthStatus Healthy (o actividad estable) sin señales adversas ni Star.";

        public const string StarRule =
            "Star (FASE 8.13): checklist impacto + eficiencia + bajo riesgo. " +
            "≠ más vendido. Ver ProductStarPolicy. Sin score ponderado.";

        public const string ExplainabilityNote =
            "Cada clasificación incluye Reasons en texto. Sin 'Score 87' opaco.";
    }

    public sealed class ProductClassificationThresholds
    {
        public static ProductClassificationThresholds Default { get; } = new();

        public int CriticalIdleDays { get; init; } = 60;
        public decimal MinMaterialCapital { get; init; } = 1_000m;
        public decimal StrongDeclinePct { get; init; } = -25m;
    }
}
