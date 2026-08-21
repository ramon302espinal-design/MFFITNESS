using System.Globalization;
using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato alertas ROI de línea (FASE 10.11). ≠ ROI inversión FASE 6.</summary>
    public static class RoiAlertRulePolicy
    {
        public const string Definition =
            "FASE 10.11: ROI de producto (ganancia/COGS o puente 9.19). " +
            "≠ ROI de inversión FASE 6. Consume SalesCapitalBridge (SSOT).";

        public const string Section52 =
            "roi.rev_up_roi_down = Ventas↑ + ROI↓ (§52) — capital sin retorno proporcional.";

        public const string Deterioration =
            "roi.deterioration = caída fuerte de ROI en pp (StrongBand) sin exigir ventas↑.";

        public const string AntiFatigue =
            "Máximo MaxProductAlerts productos por corrida (default 10), orden |ΔROI| luego capital.";
    }

    /// <summary>Composición pura de candidatos ROI (FASE 10.11).</summary>
    public static class RoiAlertRuleComposer
    {
        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-DO");

        public const int DefaultMaxProductAlerts = 10;

        public static IReadOnlyList<DecisionRuleCandidate> FromBridge(
            SalesCapitalBridgeReport? bridge,
            string periodKey,
            int maxProductAlerts = DefaultMaxProductAlerts,
            decimal strongBandPp = 15m)
        {
            if (bridge == null || bridge.Rows.Count == 0)
                return Array.Empty<DecisionRuleCandidate>();

            var scored = new List<(DecisionRuleCandidate Candidate, decimal AbsRoi, decimal Capital)>();

            foreach (SalesCapitalBridgeRow row in bridge.Rows)
            {
                bool revUpRoiDown = row.Signals.Any(s =>
                    s.Kind == SalesCapitalSignalKind.RevenueUpRoiDown);

                bool strongRoiDown = row.RoiChangePct.HasValue
                    && row.RoiChangePct.Value <= -strongBandPp;

                if (!revUpRoiDown && !strongRoiDown)
                    continue;

                if (revUpRoiDown)
                {
                    scored.Add((
                        BuildRevUpRoiDown(row, periodKey),
                        Abs(row.RoiChangePct),
                        row.InventoryCapital));
                }
                else
                {
                    scored.Add((
                        BuildDeterioration(row, periodKey),
                        Abs(row.RoiChangePct),
                        row.InventoryCapital));
                }
            }

            return scored
                .OrderByDescending(x => x.AbsRoi)
                .ThenByDescending(x => x.Capital)
                .ThenBy(x => x.Candidate.EntityName, StringComparer.OrdinalIgnoreCase)
                .Take(Math.Max(1, maxProductAlerts))
                .Select(x => x.Candidate)
                .ToList();
        }

        private static DecisionRuleCandidate BuildRevUpRoiDown(
            SalesCapitalBridgeRow row,
            string periodKey)
        {
            string roiText = FormatPp(row.RoiChangePct);
            string revText = FormatPct(row.RevenueChangePct);

            return new DecisionRuleCandidate
            {
                RuleId = "roi.alerts.rev_up_roi_down",
                EventType = "roi.rev_up_roi_down",
                Area = DecisionEventArea.Roi,
                EntityType = DecisionEntityType.Product,
                EntityId = row.ProductId.ToString(CultureInfo.InvariantCulture),
                EntityName = row.ProductName,
                PeriodKey = periodKey,
                Title = "Ventas↑ ROI↓",
                Description =
                    $"{row.ProductName}: ingresos {revText} con ROI {roiText} (pp).",
                Reason =
                    "Más actividad de ventas sin retorno proporcional de ROI de línea (§52). " +
                    "No es ROI de inversión FASE 6.",
                Recommendation = "Revisar capital invertido en el SKU, costos y precios.",
                Source = "SalesCapitalBridgeService",
                Materiality = new DecisionMaterialityInput
                {
                    VariationPct = row.RoiChangePct,
                    CapitalAmount = row.InventoryCapital,
                    CrossSignal = true
                },
                ImpactAssessment = new DecisionImpactAssessment
                {
                    Financial = DecisionImpactLevel.High,
                    Sales = DecisionImpactLevel.Medium,
                    Capital = row.InventoryCapital >= 10_000m
                        ? DecisionImpactLevel.High
                        : DecisionImpactLevel.Medium
                },
                Urgency = DecisionUrgencyLevel.Medium,
                Evidence = Evidence(row, roiText, revText),
                MetricKeys = ["roi.flag_rev_up_roi_down", "roi.product_change_pp", "capital.inventory"]
            };
        }

        private static DecisionRuleCandidate BuildDeterioration(
            SalesCapitalBridgeRow row,
            string periodKey)
        {
            string roiText = FormatPp(row.RoiChangePct);

            return new DecisionRuleCandidate
            {
                RuleId = "roi.alerts.deterioration",
                EventType = "roi.deterioration",
                Area = DecisionEventArea.Roi,
                EntityType = DecisionEntityType.Product,
                EntityId = row.ProductId.ToString(CultureInfo.InvariantCulture),
                EntityName = row.ProductName,
                PeriodKey = periodKey,
                Title = "Deterioro de ROI",
                Description = $"{row.ProductName}: ROI de línea {roiText} (pp).",
                Reason = "Caída fuerte del ROI de producto (puntos porcentuales). ≠ ROI inversión.",
                Recommendation = "Revisar margen, COGS y mix del producto.",
                Source = "SalesCapitalBridgeService",
                Materiality = new DecisionMaterialityInput
                {
                    VariationPct = row.RoiChangePct,
                    CapitalAmount = row.InventoryCapital
                },
                ImpactAssessment = new DecisionImpactAssessment
                {
                    Financial = DecisionImpactLevel.High,
                    Capital = DecisionImpactLevel.Medium
                },
                Urgency = DecisionUrgencyLevel.Medium,
                Evidence = Evidence(row, roiText, FormatPct(row.RevenueChangePct)),
                MetricKeys = ["roi.product_change_pp", "roi.product_pct"]
            };
        }

        private static IReadOnlyList<DecisionEvidenceFact> Evidence(
            SalesCapitalBridgeRow row,
            string roiText,
            string revText)
            =>
            [
                new DecisionEvidenceFact
                {
                    Key = "roi_change_pp",
                    Label = "Δ ROI (pp)",
                    ValueText = roiText,
                    MetricKey = "roi.product_change_pp"
                },
                new DecisionEvidenceFact
                {
                    Key = "revenue_change_pct",
                    Label = "Var. ingresos",
                    ValueText = revText,
                    MetricKey = "sales.revenue_var_pct"
                },
                new DecisionEvidenceFact
                {
                    Key = "inventory_capital",
                    Label = "Capital inventario",
                    ValueText = row.InventoryCapital.ToString("N2", Cultura),
                    MetricKey = "capital.inventory"
                }
            ];

        private static decimal Abs(decimal? v) => v.HasValue ? Math.Abs(v.Value) : 0m;

        private static string FormatPp(decimal? pp)
        {
            if (!pp.HasValue)
                return "N/D";
            string sign = pp.Value > 0 ? "+" : string.Empty;
            return sign + pp.Value.ToString("N2", Cultura) + " pp";
        }

        private static string FormatPct(decimal? pct)
        {
            if (!pct.HasValue)
                return "N/D";
            string sign = pct.Value > 0 ? "+" : string.Empty;
            return sign + pct.Value.ToString("N2", Cultura) + " %";
        }
    }

    /// <summary>Regla de dominio ROI línea (FASE 10.11).</summary>
    public sealed class RoiAlertDecisionRule : IDecisionRule
    {
        private readonly Func<ProfitPeriodKind, DateTime?, SalesCapitalBridgeReport?> _loadBridge;

        public string RuleId => "roi.alerts.v1";

        public RoiAlertDecisionRule()
            : this(null)
        {
        }

        public RoiAlertDecisionRule(
            Func<ProfitPeriodKind, DateTime?, SalesCapitalBridgeReport?>? loadBridge)
        {
            if (loadBridge != null)
            {
                _loadBridge = loadBridge;
            }
            else
            {
                var svc = new SalesCapitalBridgeService();
                _loadBridge = (k, a) => svc.GetReport(k, a);
            }
        }

        public IEnumerable<DecisionRuleCandidate> Evaluate(DecisionRuleContext context)
        {
            string periodKey = string.IsNullOrWhiteSpace(context.PeriodKey)
                ? SalesAlertRuleComposer.PeriodKey(context.PeriodKind, context.AsOf)
                : context.PeriodKey;

            SalesCapitalBridgeReport? bridge = context.Analytics != null
                ? context.Analytics.CapitalBridge
                : _loadBridge(context.PeriodKind, context.AsOf);
            return RoiAlertRuleComposer.FromBridge(bridge, periodKey);
        }
    }
}
