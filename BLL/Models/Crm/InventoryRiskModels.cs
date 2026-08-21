namespace BLL.Models.Crm
{
    /// <summary>
    /// Escenario de liquidación SIMULADO (FASE 7.10). No modifica PrecioVenta.
    /// </summary>
    public sealed class LiquidationScenarioResult
    {
        /// <summary>Descuento aplicado (0 = precio lista).</summary>
        public decimal DiscountPct { get; init; }

        public decimal SimulatedUnitPrice { get; init; }

        /// <summary>Ingreso si se vende todo el stock del alcance a precio simulado.</summary>
        public decimal SimulatedRevenue { get; init; }

        /// <summary>Capital a costo del alcance (lo que se liberaría del inventario).</summary>
        public decimal CapitalAtCost { get; init; }

        /// <summary>Ingreso − capital (puede ser pérdida).</summary>
        public decimal ProfitOrLoss { get; init; }

        /// <summary>
        /// Capital liberable ≈ CapitalAtCost (vuelve a liquidez si se vende).
        /// Distinto de CapitalRecovered (FASE 6, ya ocurrido).
        /// </summary>
        public decimal CapitalLiberable { get; init; }
    }

    /// <summary>
    /// Capital en riesgo + liberable + simulaciones (FASE 7.10).
    /// </summary>
    public sealed class InventoryRiskReport
    {
        public decimal InventoryCapitalTotal { get; init; }

        /// <summary>Frozen + Critical (inmovilizado clasificado).</summary>
        public decimal ImmobilizedCapital { get; init; }

        /// <summary>
        /// En riesgo: Critical + Frozen con ganancia potencial &lt; 0.
        /// No todo congelado está perdido.
        /// </summary>
        public decimal AtRiskCapital { get; init; }

        public decimal? AtRiskShareOfInventoryPct { get; init; }
        public decimal? AtRiskShareOfImmobilizedPct { get; init; }

        /// <summary>Valor PVP del stock en riesgo (liberable a precio lista).</summary>
        public decimal LiberableSalesValueAtList { get; init; }

        /// <summary>Capital a costo del stock en riesgo.</summary>
        public decimal LiberableCapitalAtCost { get; init; }

        public int ProductsAtRisk { get; init; }

        /// <summary>Escenarios 0/−5/−10/−20/−30/−50 % sobre el alcance en riesgo.</summary>
        public IReadOnlyList<LiquidationScenarioResult> LiquidationScenarios { get; init; }
            = Array.Empty<LiquidationScenarioResult>();
    }
}
