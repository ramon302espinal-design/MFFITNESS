using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato de acciones de negocio (FASE 11.2).</summary>
    public static class BusinessActionPolicy
    {
        public const string Definition =
            "FASE 11.2: contratos ActionType / Status / Outcome / Confidence. " +
            "Closed-loop: DECISIÓN → ACCIÓN → RESULTADO → EVALUACIÓN. " +
            "El sistema RECOMIENDA y REGISTRA; el usuario EJECUTA. " +
            "≠ DecisionResolutionAction (cerrar evento FASE 10).";

        public const string NoAutomation =
            "PROHIBIDO: cambiar precios, comprar, vender, promocionar o mutar inventario. " +
            "Solo registro manual de lo que el usuario ya hizo o planea hacer.";

        public const string Causality =
            "Lenguaje: 'Después de / Durante el período / Se observó…' (FASE 11.23). " +
            "Nunca 'la acción causó' ni garantías. Sin ML en FASE 11 (§87).";

        public const string OutcomeRules =
            "Exitosa / Parcial / No efectiva / Sin datos. " +
            "Cancelada ⇒ no clasificar como Exitosa (TEST 8). " +
            "Sin baseline o datos ⇒ InsufficientData (TEST 7/12).";

        public const string Deferred =
            "FASE 11 completa.";
    }

    /// <summary>Catálogo de tipos de acción (FASE 11.2) — sin I/O ni umbrales.</summary>
    public static class BusinessActionCatalog
    {
        public static IReadOnlyList<BusinessActionTypeDescriptor> All { get; } = Build();

        public static BusinessActionTypeDescriptor? Find(BusinessActionType type)
            => All.FirstOrDefault(d => d.Type == type);

        public static BusinessActionTypeDescriptor? FindByCode(string code)
            => All.FirstOrDefault(d =>
                string.Equals(d.Code, code, StringComparison.OrdinalIgnoreCase));

        public static string DisplayName(BusinessActionType type)
            => Find(type)?.DisplayName ?? type.ToString();

        public static string StatusLabel(BusinessActionStatus status) => status switch
        {
            BusinessActionStatus.Pending => "PENDIENTE",
            BusinessActionStatus.InProgress => "EN PROCESO",
            BusinessActionStatus.Completed => "COMPLETADA",
            BusinessActionStatus.Cancelled => "CANCELADA",
            BusinessActionStatus.NoResult => "SIN RESULTADO",
            _ => "—"
        };

        public static string OutcomeLabel(BusinessActionOutcome outcome) => outcome switch
        {
            BusinessActionOutcome.Successful => "EXITOSA",
            BusinessActionOutcome.Partial => "PARCIAL",
            BusinessActionOutcome.Ineffective => "NO EFECTIVA",
            BusinessActionOutcome.InsufficientData => "SIN DATOS",
            _ => "—"
        };

        public static string OutcomeGlyph(BusinessActionOutcome outcome) => outcome switch
        {
            BusinessActionOutcome.Successful => "🟢",
            BusinessActionOutcome.Partial => "🟡",
            BusinessActionOutcome.Ineffective => "🔴",
            BusinessActionOutcome.InsufficientData => "⚪",
            _ => "·"
        };

        public static string ConfidenceLabel(BusinessActionConfidence confidence) => confidence switch
        {
            BusinessActionConfidence.High => "ALTA",
            BusinessActionConfidence.Medium => "MEDIA",
            BusinessActionConfidence.Low => "BAJA",
            _ => "—"
        };

        /// <summary>Cancelada / Sin resultado no entran a tasa de éxito.</summary>
        public static bool IsEvaluable(BusinessActionStatus status)
            => status == BusinessActionStatus.Completed;

        /// <summary>Outcome válido solo si la acción es evaluable.</summary>
        public static bool CanAssignOutcome(BusinessActionStatus status, BusinessActionOutcome outcome)
        {
            if (outcome == BusinessActionOutcome.Unspecified)
                return true;
            if (!IsEvaluable(status))
                return false;
            return outcome is BusinessActionOutcome.Successful
                or BusinessActionOutcome.Partial
                or BusinessActionOutcome.Ineffective
                or BusinessActionOutcome.InsufficientData;
        }

        private static IReadOnlyList<BusinessActionTypeDescriptor> Build()
            =>
            [
                D(BusinessActionType.Promotion, "promotion", "Promoción",
                    "Campaña promocional / descuento temporal.", impliesPos: true),
                D(BusinessActionType.PriceChange, "price_change", "Cambio de precio",
                    "Ajuste de PrecioVenta u oferta — solo registro.", impliesPos: true),
                D(BusinessActionType.Replenishment, "replenishment", "Reabastecimiento",
                    "Reposición de stock — el usuario compra/ingresa; el CRM no compra.", impliesPos: true),
                D(BusinessActionType.StockReduction, "stock_reduction", "Reducción de stock",
                    "Liquidación / salida de inventario.", impliesPos: true),
                D(BusinessActionType.MixChange, "mix_change", "Cambio de mix",
                    "Cambio de mezcla de productos o surtido.", impliesPos: false),
                D(BusinessActionType.Campaign, "campaign", "Campaña",
                    "Campaña comercial o de comunicación.", impliesPos: false),
                D(BusinessActionType.PurchasePause, "purchase_pause", "Pausa de compra",
                    "Decisión de no reponer / pausar compras.", impliesPos: false),
                D(BusinessActionType.CostReview, "cost_review", "Revisión de costos",
                    "Análisis de costos — sin mutar histórico de costo.", impliesPos: false),
                D(BusinessActionType.MarginReview, "margin_review", "Revisión de margen",
                    "Análisis de margen / descuentos.", impliesPos: false),
                D(BusinessActionType.StrategyChange, "strategy_change", "Cambio de estrategia",
                    "Cambio de estrategia comercial o de salida.", impliesPos: false),
                D(BusinessActionType.Other, "other", "Otra",
                    "Acción no catalogada — describir en notas.", impliesPos: false)
            ];

        private static BusinessActionTypeDescriptor D(
            BusinessActionType type,
            string code,
            string name,
            string description,
            bool impliesPos)
            => new()
            {
                Type = type,
                Code = code,
                DisplayName = name,
                Description = description,
                ImpliesManualPosChange = impliesPos
            };
    }
}
