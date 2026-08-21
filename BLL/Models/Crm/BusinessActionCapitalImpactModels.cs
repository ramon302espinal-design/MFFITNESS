namespace BLL.Models.Crm
{
    /// <summary>
    /// Lectura cautelosa de capital liberado / incrementos observados (FASE 11.10).
    /// Montos = diferencia absoluta Before→After. No implica que la acción “causó” el cambio.
    /// </summary>
    public sealed class BusinessActionObservedCapitalImpact
    {
        /// <summary>Reducción observada de capital inmovilizado (Before−After si ↓).</summary>
        public decimal? LiberatedImmobilized { get; init; }

        /// <summary>Reducción observada de capital en riesgo.</summary>
        public decimal? LiberatedAtRisk { get; init; }

        /// <summary>Reducción observada de capital inventario (si ↓).</summary>
        public decimal? LiberatedInventoryCapital { get; init; }

        /// <summary>Suma de liberaciones de capital reportadas (solo tramos ↓).</summary>
        public decimal? TotalLiberatedCapital { get; init; }

        /// <summary>Incremento observado de ingresos (After−Before si ↑).</summary>
        public decimal? ObservedRevenueIncrease { get; init; }

        /// <summary>Incremento observado de ganancia realizada.</summary>
        public decimal? ObservedProfitIncrease { get; init; }

        /// <summary>Cambio de margen en pp (After−Before); puede ser negativo.</summary>
        public decimal? ObservedMarginChangePp { get; init; }

        public bool HasAnySignal { get; init; }

        /// <summary>Narrativa soft — “se observó…”, nunca “liberó/causó”.</summary>
        public string Narrative { get; init; } = string.Empty;

        public string Caution { get; init; } = string.Empty;
    }
}
