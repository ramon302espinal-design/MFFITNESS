using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Plantilla de recomendación por EventType (FASE 10.19).</summary>
    public sealed class DecisionRecommendationTemplate
    {
        public string EventType { get; init; } = string.Empty;
        public DecisionRecommendationVerb Verb { get; init; }
            = DecisionRecommendationVerb.Revisar;
        public string Headline { get; init; } = string.Empty;
        public string Body { get; init; } = string.Empty;
        public IReadOnlyList<string> SuggestedChecks { get; init; }
            = Array.Empty<string>();
        public bool IsOpportunity { get; init; }
    }

    /// <summary>Catálogo de plantillas suaves por tipo de evento.</summary>
    public static class DecisionRecommendationCatalog
    {
        public static IReadOnlyList<string> DefaultPolicyReminders { get; } =
        [
            "No comprar automáticamente.",
            "No mutar stock, caja ni inversiones desde esta alerta.",
            "El usuario decide."
        ];

        private static readonly IReadOnlyDictionary<string, DecisionRecommendationTemplate> ByType
            = Build();

        public static DecisionRecommendationTemplate? Find(string? eventType)
        {
            if (string.IsNullOrWhiteSpace(eventType))
                return null;
            return ByType.TryGetValue(eventType.Trim().ToLowerInvariant(), out DecisionRecommendationTemplate? t)
                ? t
                : null;
        }

        public static DecisionRecommendationTemplate Fallback(DecisionEventArea area)
            => area switch
            {
                DecisionEventArea.Forecast => T(
                    "_fallback.forecast", DecisionRecommendationVerb.Revisar,
                    "Revisar estimación",
                    "Revisar el escenario como estimación — sin tratarlo como certeza.",
                    ["Confianza del modelo", "Datos de respaldo"]),
                DecisionEventArea.Product => T(
                    "_fallback.product", DecisionRecommendationVerb.Evaluar,
                    "Evaluar producto",
                    "Evaluar desempeño, capital y cobertura del producto.",
                    ["Clasificación", "Capital", "Tendencia"]),
                _ => T(
                    "_fallback", DecisionRecommendationVerb.Revisar,
                    "Revisar señal",
                    "Revisar la señal detectada y su impacto antes de decidir.",
                    ["Evidencia", "Impacto", "Contexto del período"])
            };

        private static IReadOnlyDictionary<string, DecisionRecommendationTemplate> Build()
        {
            var map = new Dictionary<string, DecisionRecommendationTemplate>(StringComparer.OrdinalIgnoreCase);

            void Add(DecisionRecommendationTemplate t)
                => map[t.EventType] = t;

            Add(T("sales.strong_decline", DecisionRecommendationVerb.Revisar,
                "Revisar caída de ventas",
                "Revisar productos, precios y demanda del período.",
                ["Variación de ingresos", "Top productos", "Categorías afectadas"]));
            Add(T("sales.strong_growth", DecisionRecommendationVerb.Evaluar,
                "Evaluar crecimiento",
                "Evaluar cobertura de stock en productos que impulsan el crecimiento.",
                ["Cobertura", "Capacidad de reposición", "Margen del crecimiento"],
                opportunity: true));
            Add(T("sales.rev_up_profit_down", DecisionRecommendationVerb.Revisar,
                "Revisar ingresos↑ ganancia↓",
                "Revisar costos, mezcla de productos y descuentos.",
                ["COGS", "Mix", "Descuentos"]));
            Add(T("sales.rev_up_margin_down", DecisionRecommendationVerb.Revisar,
                "Revisar ingresos↑ margen↓",
                "Revisar mezcla, costos y descuentos.",
                ["Margen %", "Mix", "Descuentos"]));
            Add(T("sales.concentration", DecisionRecommendationVerb.Revisar,
                "Revisar concentración",
                "Revisar diversificación y riesgo de dependencia.",
                ["Share top productos", "Pareto"]));

            Add(T("profit.decline", DecisionRecommendationVerb.Revisar,
                "Revisar deterioro de ganancia",
                "Revisar costos, mezcla de productos y descuentos.",
                ["Ganancia realizada", "Margen", "Mix"]));
            Add(T("margin.deterioration", DecisionRecommendationVerb.Revisar,
                "Revisar deterioro de margen",
                "Revisar mezcla de productos, costos y descuentos.",
                ["Margen %", "COGS", "Precios"]));

            Add(T("roi.deterioration", DecisionRecommendationVerb.Revisar,
                "Revisar ROI de línea",
                "Revisar margen, COGS y mix del producto.",
                ["ROI línea", "Capital invertido", "Margen"]));
            Add(T("roi.rev_up_roi_down", DecisionRecommendationVerb.Revisar,
                "Revisar ventas↑ ROI↓",
                "Revisar capital invertido en el SKU, costos y precios.",
                ["ROI", "Capital", "Ventas"]));

            Add(T("inv.stockout_risk", DecisionRecommendationVerb.Evaluar,
                "Evaluar riesgo de quiebre",
                "Evaluar reposición según demanda — sin compra automática.",
                ["Days of cover", "Velocidad", "Stock mínimo"]));
            Add(T("inv.overstock", DecisionRecommendationVerb.Revisar,
                "Revisar sobreinventario",
                "Revisar compras futuras y estrategia de salida.",
                ["Cobertura", "Rotación", "Capital inmovilizado"]));
            Add(T("inv.never_sold", DecisionRecommendationVerb.Revisar,
                "Revisar never sold",
                "Revisar visibilidad, precio o descontinuación.",
                ["Días sin venta", "Capital expuesto"]));
            Add(T("inv.replenishment", DecisionRecommendationVerb.Evaluar,
                "Evaluar reposición",
                "Evaluar reposición según demanda — sin compra automática.",
                ["Demanda", "Cobertura", "Lead time"]));

            Add(T("capital.critical", DecisionRecommendationVerb.Revisar,
                "Revisar capital crítico",
                "Revisar estrategia de salida antes de nueva compra.",
                ["Capital inmovilizado", "Rotación", "Tendencia"]));
            Add(T("capital.frozen", DecisionRecommendationVerb.Revisar,
                "Revisar capital congelado",
                "Revisar rotación, precio o liquidación simulada.",
                ["Frozen capital", "Idle days", "Escenarios"]));
            Add(T("capital.at_risk", DecisionRecommendationVerb.Revisar,
                "Revisar capital en riesgo",
                "Revisar descuentos simulados y prioridad de salida.",
                ["At-risk", "Ventas ↓", "Stock"]));
            Add(T("capital.high_immobilized_share", DecisionRecommendationVerb.Revisar,
                "Revisar % inmovilizado",
                "Revisar productos Frozen/Critical y plan de liberación de capital.",
                ["Frozen share %", "Top SKUs"]));
            Add(T("capital.slow", DecisionRecommendationVerb.Revisar,
                "Revisar capital lento",
                "Revisar rotación y estrategia comercial del SKU.",
                ["Velocidad", "Cobertura", "Capital"]));

            Add(T("product.star_stockout", DecisionRecommendationVerb.Evaluar,
                "Evaluar estrella con quiebre",
                "Evaluar reposición — no comprar automáticamente.",
                ["Clasificación estrella", "Stock", "Demanda"]));
            Add(T("product.growth_opportunity", DecisionRecommendationVerb.Evaluar,
                "Evaluar oportunidad",
                "Evaluar oportunidad de crecimiento / cobertura.",
                ["Tendencia", "Margen", "Cobertura"],
                opportunity: true));
            Add(T("product.critical_class", DecisionRecommendationVerb.Revisar,
                "Revisar clase crítica",
                "Revisar capital, tendencia y estrategia de salida.",
                ["Clasificación", "Capital", "Tendencia"]));

            Add(T("trend.deceleration", DecisionRecommendationVerb.Revisar,
                "Revisar desaceleración",
                "Revisar momentum y no confundir con declive absoluto.",
                ["Pendiente reciente", "Nivel absoluto"]));
            Add(T("trend.volatile", DecisionRecommendationVerb.Revisar,
                "Revisar volatilidad",
                "Revisar con cautela; evitar decisiones solo por pendiente.",
                ["Variabilidad", "Ventana"]));
            Add(T("forecast.low_confidence", DecisionRecommendationVerb.Revisar,
                "Revisar forecast (estimación)",
                "Revisar el forecast como escenario; no decidir compras solo con esta estimación.",
                ["Confianza", "Datos históricos"]));

            Add(T("invst.frozen_capital", DecisionRecommendationVerb.Revisar,
                "Revisar frozen capital inversión",
                "Revisar productos vinculados y plan de recuperación — no liquidar automáticamente.",
                ["Frozen capital inversión", "Productos vinculados"]));
            Add(T("invst.roi_weak", DecisionRecommendationVerb.Revisar,
                "Revisar ROI inversión",
                "Revisar recuperación y productos vinculados — no liquidar automáticamente.",
                ["ROI inversión", "Recuperación"]));

            return map;
        }

        private static DecisionRecommendationTemplate T(
            string eventType,
            DecisionRecommendationVerb verb,
            string headline,
            string body,
            string[] checks,
            bool opportunity = false)
            => new()
            {
                EventType = eventType,
                Verb = verb,
                Headline = headline,
                Body = body,
                SuggestedChecks = checks,
                IsOpportunity = opportunity
            };
        }
}
