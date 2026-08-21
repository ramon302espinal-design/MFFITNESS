using System.Globalization;
using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato alertas de inventario operativo (FASE 10.12). Capital = 10.13.</summary>
    public static class InventoryAlertRulePolicy
    {
        public const string Definition =
            "FASE 10.12: stockout / overstock / never-sold / reposición. " +
            "Consume InventoryAlertService + SalesStockRisk (SSOT). " +
            "No auto-compra. Capital crítico/congelado = FASE 10.13.";

        public const string Mapping =
            "NeverSold/Overstock/StockoutRisk ← InventoryAlertKind. " +
            "Replenishment ← SalesStockSignalKind.ReplenishmentOpportunity.";

        public const string AntiFatigue =
            "Máx. MaxProductAlerts por tipo de evento (default 10).";
    }

    /// <summary>Composición pura alertas inventario (FASE 10.12).</summary>
    public static class InventoryAlertRuleComposer
    {
        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-DO");

        public const int DefaultMaxProductAlerts = 10;

        public static IReadOnlyList<DecisionRuleCandidate> FromInventoryAlerts(
            InventoryAlertReport? report,
            string periodKey,
            int maxPerKind = DefaultMaxProductAlerts)
        {
            if (report == null || report.Alerts.Count == 0)
                return Array.Empty<DecisionRuleCandidate>();

            var list = new List<DecisionRuleCandidate>();

            list.AddRange(TakeKind(
                report.Alerts,
                InventoryAlertKind.StockoutRisk,
                maxPerKind,
                a => BuildFromAlert(
                    a, periodKey,
                    "inv.stockout_risk",
                    "Riesgo de quiebre",
                    "Stock bajo con demanda activa.",
                    "Evaluar reposición — no comprar automáticamente.",
                    timeSensitive: true,
                    DecisionImpactLevel.Critical,
                    DecisionUrgencyLevel.Immediate)));

            list.AddRange(TakeKind(
                report.Alerts,
                InventoryAlertKind.Overstock,
                maxPerKind,
                a => BuildFromAlert(
                    a, periodKey,
                    "inv.overstock",
                    "Sobreinventario",
                    "Cobertura elevada respecto a la demanda.",
                    "Revisar compras futuras y estrategia de salida.",
                    timeSensitive: false,
                    DecisionImpactLevel.Medium,
                    DecisionUrgencyLevel.Low)));

            list.AddRange(TakeKind(
                report.Alerts,
                InventoryAlertKind.NeverSold,
                maxPerKind,
                a => BuildFromAlert(
                    a, periodKey,
                    "inv.never_sold",
                    "Nunca vendido",
                    "Producto con stock sin ventas registradas (post-gracia).",
                    "Revisar visibilidad, precio o descontinuación.",
                    timeSensitive: false,
                    DecisionImpactLevel.High,
                    DecisionUrgencyLevel.Medium)));

            return list;
        }

        public static IReadOnlyList<DecisionRuleCandidate> FromStockRisk(
            SalesStockRiskReport? report,
            string periodKey,
            int maxPerKind = DefaultMaxProductAlerts)
        {
            if (report == null)
                return Array.Empty<DecisionRuleCandidate>();

            // Reposición: solo este kind en 10.12 (stockout ya viene de InventoryAlert)
            return report.Rows
                .Where(r => r.PrimarySignal == SalesStockSignalKind.ReplenishmentOpportunity
                            || r.Signals.Contains(SalesStockSignalKind.ReplenishmentOpportunity))
                .OrderBy(r => r.DaysOfCover ?? decimal.MaxValue)
                .ThenByDescending(r => r.UnitsPerDay ?? 0m)
                .Take(Math.Max(1, maxPerKind))
                .Select(r => new DecisionRuleCandidate
                {
                    RuleId = "inv.alerts.replenishment",
                    EventType = "inv.replenishment",
                    Area = DecisionEventArea.Inventory,
                    EntityType = DecisionEntityType.Product,
                    EntityId = r.ProductId.ToString(CultureInfo.InvariantCulture),
                    EntityName = r.ProductName,
                    PeriodKey = periodKey,
                    Title = "Oportunidad de reposición",
                    Description =
                        $"{r.ProductName}: cobertura {FormatDays(r.DaysOfCover)}, " +
                        $"demanda proy. {FormatDec(r.ProjectedDemandUnits)} uds.",
                    Reason = string.IsNullOrWhiteSpace(r.Reason)
                        ? "Crecimiento con stock bajo — revisar reposición (no auto-compra)."
                        : r.Reason,
                    Recommendation = "Evaluar reposición según demanda — sin compra automática.",
                    Source = "SalesStockRiskService",
                    Materiality = new DecisionMaterialityInput
                    {
                        TimeSensitiveStockout = r.DemandExceedsStock || r.FlagStockoutRisk,
                        OpportunitySignal = true,
                        VariationPct = 20m // fuerza banda fuerte para oportunidad
                    },
                    ImpactAssessment = new DecisionImpactAssessment
                    {
                        Inventory = DecisionImpactLevel.High,
                        Sales = DecisionImpactLevel.Medium
                    },
                    Urgency = DecisionUrgencyLevel.High,
                    OpportunityWindow = true,
                    TimeSensitiveStockout = r.DemandExceedsStock,
                    Evidence =
                    [
                        new DecisionEvidenceFact
                        {
                            Key = "days_of_cover",
                            Label = "Días cobertura",
                            ValueText = FormatDays(r.DaysOfCover),
                            MetricKey = "inv.days_of_cover"
                        },
                        new DecisionEvidenceFact
                        {
                            Key = "stock",
                            Label = "Stock",
                            ValueText = r.Stock.ToString(Cultura),
                            MetricKey = "inv.stock"
                        }
                    ],
                    MetricKeys = ["inv.days_of_cover", "stock.projected_demand", "inv.stock"]
                })
                .ToList();
        }

        private static IEnumerable<DecisionRuleCandidate> TakeKind(
            IReadOnlyList<InventoryAlert> alerts,
            InventoryAlertKind kind,
            int max,
            Func<InventoryAlert, DecisionRuleCandidate> map)
            => alerts
                .Where(a => a.Kind == kind && a.ProductId.HasValue)
                .OrderByDescending(a => a.Priority)
                .ThenByDescending(a => a.CapitalAmount ?? 0m)
                .ThenBy(a => a.ProductName, StringComparer.OrdinalIgnoreCase)
                .Take(Math.Max(1, max))
                .Select(map);

        private static DecisionRuleCandidate BuildFromAlert(
            InventoryAlert a,
            string periodKey,
            string eventType,
            string title,
            string reason,
            string recommendation,
            bool timeSensitive,
            DecisionImpactLevel inventoryImpact,
            DecisionUrgencyLevel urgency)
        {
            decimal capital = a.CapitalAmount ?? 0m;
            return new DecisionRuleCandidate
            {
                RuleId = "inv.alerts." + eventType.Replace('.', '_'),
                EventType = eventType,
                Area = DecisionEventArea.Inventory,
                EntityType = DecisionEntityType.Product,
                EntityId = a.ProductId!.Value.ToString(CultureInfo.InvariantCulture),
                EntityName = a.ProductName,
                PeriodKey = periodKey,
                Title = title,
                Description = string.IsNullOrWhiteSpace(a.Message)
                    ? $"{a.ProductName}: {title}"
                    : a.Message,
                Reason = reason,
                Recommendation = recommendation,
                Source = "InventoryAlertService",
                Materiality = new DecisionMaterialityInput
                {
                    CapitalAmount = capital > 0 ? capital : null,
                    TimeSensitiveStockout = timeSensitive
                },
                ImpactAssessment = new DecisionImpactAssessment
                {
                    Inventory = inventoryImpact,
                    Capital = capital >= 10_000m
                        ? DecisionImpactLevel.High
                        : capital >= 1_000m
                            ? DecisionImpactLevel.Medium
                            : DecisionImpactLevel.Low
                },
                Urgency = urgency,
                TimeSensitiveStockout = timeSensitive,
                RequiresImmediateReview = timeSensitive,
                Evidence =
                [
                    new DecisionEvidenceFact
                    {
                        Key = "capital",
                        Label = "Capital",
                        ValueText = capital.ToString("N2", Cultura),
                        MetricKey = "capital.inventory"
                    },
                    new DecisionEvidenceFact
                    {
                        Key = "days_of_cover",
                        Label = "Cobertura",
                        ValueText = FormatDays(a.DaysOfCover),
                        MetricKey = "inv.days_of_cover"
                    },
                    new DecisionEvidenceFact
                    {
                        Key = "idle_days",
                        Label = "Idle",
                        ValueText = a.IdleDays?.ToString(Cultura) ?? "N/D",
                        MetricKey = "inv.idle_days"
                    }
                ],
                MetricKeys = ["inv.stock", "inv.days_of_cover", "capital.inventory"]
            };
        }

        private static string FormatDays(decimal? d)
            => d.HasValue ? d.Value.ToString("N0", Cultura) + " d" : "N/D";

        private static string FormatDec(decimal? d)
            => d.HasValue ? d.Value.ToString("N1", Cultura) : "N/D";
    }

    /// <summary>Regla de dominio inventario operativo (FASE 10.12).</summary>
    public sealed class InventoryAlertDecisionRule : IDecisionRule
    {
        private readonly Func<DateTime?, InventoryAlertReport?> _loadAlerts;
        private readonly Func<ProfitPeriodKind, DateTime?, SalesStockRiskReport?> _loadStockRisk;

        public string RuleId => "inv.alerts.v1";

        public InventoryAlertDecisionRule()
            : this(null, null)
        {
        }

        public InventoryAlertDecisionRule(
            Func<DateTime?, InventoryAlertReport?>? loadAlerts,
            Func<ProfitPeriodKind, DateTime?, SalesStockRiskReport?>? loadStockRisk)
        {
            if (loadAlerts != null)
                _loadAlerts = loadAlerts;
            else
            {
                var svc = new InventoryAlertService();
                _loadAlerts = a => svc.GetAlerts(a);
            }

            if (loadStockRisk != null)
                _loadStockRisk = loadStockRisk;
            else
            {
                var svc = new SalesStockRiskService();
                _loadStockRisk = (k, a) => svc.GetReport(k, a);
            }
        }

        public IEnumerable<DecisionRuleCandidate> Evaluate(DecisionRuleContext context)
        {
            string periodKey = string.IsNullOrWhiteSpace(context.PeriodKey)
                ? SalesAlertRuleComposer.PeriodKey(context.PeriodKind, context.AsOf)
                : context.PeriodKey;

            InventoryAlertReport? alerts = context.Analytics != null
                ? context.Analytics.InventoryAlerts
                : _loadAlerts(context.AsOf);
            SalesStockRiskReport? stock = context.Analytics != null
                ? context.Analytics.StockRisk
                : _loadStockRisk(context.PeriodKind, context.AsOf);

            foreach (DecisionRuleCandidate c in InventoryAlertRuleComposer.FromInventoryAlerts(
                         alerts, periodKey))
                yield return c;

            foreach (DecisionRuleCandidate c in InventoryAlertRuleComposer.FromStockRisk(
                         stock, periodKey))
                yield return c;
        }
    }
}
