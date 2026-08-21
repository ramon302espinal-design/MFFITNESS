namespace BLL.Models.Crm
{
    /// <summary>Clasificación de materialidad (FASE 10.7).</summary>
    public enum DecisionMaterialityKind
    {
        /// <summary>Ruido / banda flat / capital insignificante — no alertar.</summary>
        NotMaterial = 0,

        /// <summary>Relevante para DecisionEvent.</summary>
        Material = 1,

        /// <summary>Materialidad fuerte (p.ej. variación ≥ StrongBand / capital crítico).</summary>
        Strong = 2
    }

    /// <summary>Entrada cualitativa/numérica para evaluar materialidad.</summary>
    public sealed class DecisionMaterialityInput
    {
        /// <summary>Variación % (ingresos, uds, etc.). null = no aplica.</summary>
        public decimal? VariationPct { get; init; }

        /// <summary>Capital involucrado (RD$). null = no aplica.</summary>
        public decimal? CapitalAmount { get; init; }

        /// <summary>% inmovilizado / frozen share. null = no aplica.</summary>
        public decimal? ImmobilizedSharePct { get; init; }

        /// <summary>Señal cruzada (§50/§51) — material aunque cada pierna sea mild.</summary>
        public bool CrossSignal { get; init; }

        /// <summary>Quiebre con demanda — no silenciar por capital bajo.</summary>
        public bool TimeSensitiveStockout { get; init; }

        /// <summary>Oportunidad explícita (crecimiento fuerte + stock OK).</summary>
        public bool OpportunitySignal { get; init; }

        /// <summary>TEST 7/13 — no alerta avanzada.</summary>
        public bool InsufficientData { get; init; }
    }

    /// <summary>Resultado de materialidad (anti-fatiga).</summary>
    public sealed class DecisionMaterialityResult
    {
        public bool IsMaterial { get; init; }
        public bool ShouldEmitAlert { get; init; }
        public DecisionMaterialityKind Kind { get; init; }
        public string Reason { get; init; } = string.Empty;

        /// <summary>Puente opcional hacia DecisionSeverityResolver (10.5).</summary>
        public DecisionImpactLevel SuggestedImpact { get; init; }
    }
}
