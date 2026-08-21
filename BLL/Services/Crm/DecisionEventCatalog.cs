using System.Globalization;
using System.Text;
using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato de eventos de decisión (FASE 10.4).</summary>
    public static class DecisionEventPolicy
    {
        public const string Definition =
            "FASE 10.4: DecisionEvent = 'algo relevante ocurrió'. " +
            "Detectar / explicar / priorizar (luego) — NUNCA auto-comprar ni mutar stock/caja.";

        public const string FingerprintRule =
            "Fingerprint = Area|EventType|EntityType|EntityId|PeriodKey. " +
            "Misma huella + Active ⇒ no duplicar (TEST 8).";

        public const string Deferred =
            "FASE 10 completa." +
            "Materiality = DecisionMaterialityEvaluator (10.7) · Engine = DecisionEngine (10.8) · " +
            "Domain rules = 10.9+ · Recommendations = 10.19 · Persistence = 10.21+.";

        public const string SoftLanguage =
            "Recommendation usa Revisar/Evaluar — no órdenes irreversibles.";
    }

    /// <summary>
    /// Catálogo de tipos de evento (arquitectura). Sin umbrales ni motor.
    /// </summary>
    public static class DecisionEventCatalog
    {
        public static IReadOnlyList<DecisionEventTypeDescriptor> All { get; } = Build();

        public static DecisionEventTypeDescriptor? Find(string code)
            => All.FirstOrDefault(t =>
                string.Equals(t.Code, code, StringComparison.OrdinalIgnoreCase));

        public static IReadOnlyList<DecisionEventTypeDescriptor> ByArea(DecisionEventArea area)
            => All.Where(t => t.Area == area).ToList();

        private static IReadOnlyList<DecisionEventTypeDescriptor> Build()
        {
            var list = new List<DecisionEventTypeDescriptor>();

            void Add(string code, string name, DecisionEventArea area, string notes, string? legacy = null)
                => list.Add(new DecisionEventTypeDescriptor
                {
                    Code = code,
                    DisplayName = name,
                    Area = area,
                    Notes = notes,
                    LegacySignal = legacy
                });

            // --- VENTAS ---
            Add("sales.strong_decline", "Caída fuerte de ventas", DecisionEventArea.Sales,
                "Var. ingresos material ↓", "SalesDashboardAlertKind.StrongDecline");
            Add("sales.strong_growth", "Crecimiento fuerte de ventas", DecisionEventArea.Sales,
                "Var. ingresos material ↑", "SalesDashboardAlertKind.StrongGrowth");
            Add("sales.rev_up_profit_down", "Ingresos↑ Ganancia↓", DecisionEventArea.Sales,
                "Señal cruzada §50", "SalesDashboardAlertKind.RevenueUpProfitDown");
            Add("sales.rev_up_margin_down", "Ingresos↑ Margen↓", DecisionEventArea.Sales,
                "Señal cruzada §51", "SalesDashboardAlertKind.RevenueUpMarginDown");
            Add("sales.concentration", "Concentración de ingresos", DecisionEventArea.Sales,
                "Top N / Pareto — estratégico, no automáticamente malo");

            // --- GANANCIA / MARGEN ---
            Add("profit.decline", "Deterioro de ganancia", DecisionEventArea.Profit,
                "Var. ganancia material ↓");
            Add("margin.deterioration", "Deterioro de margen", DecisionEventArea.Margin,
                "Ingresos↑ o planos + margen↓");

            // --- ROI ---
            Add("roi.deterioration", "Deterioro de ROI línea", DecisionEventArea.Roi,
                "≠ ROI inversión FASE 6", "SalesDashboardAlertKind.RoiDown");
            Add("roi.rev_up_roi_down", "Ventas↑ ROI↓", DecisionEventArea.Roi,
                "§52 capital sin retorno proporcional");

            // --- INVENTARIO ---
            Add("inv.stockout_risk", "Riesgo de quiebre", DecisionEventArea.Inventory,
                "DaysOfCover / Stock≤mín + velocidad", "InventoryAlertKind.StockoutRisk");
            Add("inv.overstock", "Sobreinventario", DecisionEventArea.Inventory,
                "Cobertura alta", "InventoryAlertKind.Overstock");
            Add("inv.never_sold", "Nunca vendido", DecisionEventArea.Inventory,
                "Idle / NeverSold", "InventoryAlertKind.NeverSold");
            Add("inv.replenishment", "Reposición sugerida (revisar)", DecisionEventArea.Inventory,
                "SalesStockSignalKind.Replenishment · no auto-compra");

            // --- CAPITAL ---
            Add("capital.critical", "Capital crítico", DecisionEventArea.Capital,
                "Health Critical", "InventoryAlertKind.CriticalCapital");
            Add("capital.frozen", "Capital congelado (producto)", DecisionEventArea.Capital,
                "≠ FrozenCapital inversión", "InventoryAlertKind.FrozenCapital");
            Add("capital.at_risk", "Capital en riesgo", DecisionEventArea.Capital,
                "Ventas↓ + stock alto / AtRiskLoss", "InventoryAlertKind.AtRiskLoss");
            Add("capital.high_immobilized_share", "% inmovilizado alto", DecisionEventArea.Capital,
                "FrozenShare%", "InventoryAlertKind.HighImmobilizedShare");
            Add("capital.slow", "Capital lento", DecisionEventArea.Capital,
                "SlowCapital", "InventoryAlertKind.SlowCapital");

            // --- PRODUCTO ---
            Add("product.star_stockout", "Estrella con riesgo de quiebre", DecisionEventArea.Product,
                "Class=Star + stockout/replenish");
            Add("product.growth_opportunity", "Oportunidad de crecimiento", DecisionEventArea.Product,
                "Growing + stock saludable", "SalesDashboardAlertKind.GrowthOpportunity");
            Add("product.critical_class", "Producto clase crítica", DecisionEventArea.Product,
                "ProductClassification Critical");
            Add("product.insufficient_data", "Datos insuficientes", DecisionEventArea.Product,
                "New / Insufficient · no alerta avanzada (TEST 7/13)");

            // --- TENDENCIA / FORECAST ---
            Add("trend.deceleration", "Desaceleración", DecisionEventArea.Trend,
                "≠ caída · creciendo pero frenando", "SalesDashboardAlertKind.Deceleration");
            Add("trend.volatile", "Serie volátil", DecisionEventArea.Trend,
                "CV alto · SalesSeriesTrend Volatile");
            Add("forecast.low_confidence", "Forecast baja confianza", DecisionEventArea.Forecast,
                "ESTIMACIÓN · nunca certeza");

            // --- INVERSIÓN / LIQUIDEZ ---
            Add("invst.frozen_capital", "Frozen capital inversión", DecisionEventArea.Investment,
                "FIFO FASE 6 · ≠ InventoryCapital");
            Add("invst.roi_weak", "ROI inversión débil", DecisionEventArea.Investment,
                "Revisar · no liquidar auto");
            Add("liq.immobilized_pressure", "Presión de liquidez (inmovilizado)", DecisionEventArea.Liquidity,
                "Share inmovilizado alto");

            // --- OPERACIONES ---
            Add("ops.hour_data_unreliable", "Datos por hora no confiables", DecisionEventArea.Operations,
                "HourDataReliable=false");

            return list;
        }
    }

    /// <summary>Huella estable para deduplicar eventos activos.</summary>
    public static class DecisionFingerprint
    {
        public static string Compute(
            DecisionEventArea area,
            string eventType,
            DecisionEntityType entityType,
            string? entityId,
            string? periodKey)
        {
            static string Norm(string? s)
            {
                if (string.IsNullOrWhiteSpace(s))
                    return "_";
                return s.Trim().ToUpperInvariant();
            }

            var sb = new StringBuilder(96);
            sb.Append(((int)area).ToString(CultureInfo.InvariantCulture));
            sb.Append('|');
            sb.Append(Norm(eventType));
            sb.Append('|');
            sb.Append(((int)entityType).ToString(CultureInfo.InvariantCulture));
            sb.Append('|');
            sb.Append(Norm(entityId));
            sb.Append('|');
            sb.Append(Norm(periodKey));
            return sb.ToString();
        }

        public static string Compute(DecisionEvent e)
            => Compute(e.Area, e.EventType, e.EntityType, e.EntityId, e.PeriodKey);

        /// <summary>
        /// Misma huella + ambos Active ⇒ duplicado (no emitir segunda copia).
        /// </summary>
        public static bool IsDuplicateActive(DecisionEvent a, DecisionEvent b)
            => a.Status == DecisionEventStatus.Active
               && b.Status == DecisionEventStatus.Active
               && string.Equals(a.Fingerprint, b.Fingerprint, StringComparison.Ordinal);
    }

    /// <summary>Factory mínima de DecisionEvent (sin reglas de negocio).</summary>
    public static class DecisionEventFactory
    {
        public static DecisionEvent Create(
            string eventType,
            DecisionEventArea area,
            DecisionEntityType entityType,
            string? entityId,
            string entityName,
            string? periodKey,
            string source,
            string title,
            string description,
            DateTime? detectedAt = null,
            IReadOnlyList<DecisionEvidenceFact>? evidence = null,
            IReadOnlyList<string>? metricKeys = null,
            DecisionEventStatus status = DecisionEventStatus.Active,
            string reason = "",
            string impact = "",
            string recommendation = "")
        {
            DateTime at = detectedAt ?? DateTime.UtcNow;
            string fp = DecisionFingerprint.Compute(area, eventType, entityType, entityId, periodKey);

            return new DecisionEvent
            {
                EventId = Guid.NewGuid(),
                EventType = eventType,
                Area = area,
                EntityType = entityType,
                EntityId = entityId,
                EntityName = entityName ?? string.Empty,
                PeriodKey = periodKey,
                DetectedAt = at,
                CreatedAt = at,
                Title = title ?? string.Empty,
                Description = description ?? string.Empty,
                Reason = reason ?? string.Empty,
                Impact = impact ?? string.Empty,
                Recommendation = recommendation ?? string.Empty,
                Status = status,
                Source = source ?? string.Empty,
                Fingerprint = fp,
                Evidence = evidence ?? Array.Empty<DecisionEvidenceFact>(),
                MetricKeys = metricKeys ?? Array.Empty<string>()
            };
        }
    }
}
