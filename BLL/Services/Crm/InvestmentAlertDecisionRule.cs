using System.Globalization;
using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato alertas de inversiones (FASE 10.17).</summary>
    public static class InvestmentAlertRulePolicy
    {
        public const string Definition =
            "FASE 10.17: FrozenCapital inversión (FIFO) y ROI inversión débil. " +
            "≠ InventoryCapital / ImmobilizedCapital de producto. " +
            "Consume InvestmentCapitalBridge + InvestmentSummary (SSOT). " +
            "No liquidar / cerrar automáticamente.";

        public const string Frozen =
            "invst.frozen_capital = capital atrapado material en inversión activa.";

        public const string RoiWeak =
            "invst.roi_weak = ROI realizado &lt; 0 o IsLoss (con capital material y costo confiable).";
    }

    /// <summary>Composición pura alertas de inversión (FASE 10.17).</summary>
    public static class InvestmentAlertRuleComposer
    {
        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-DO");

        public const int DefaultMaxAlerts = 10;

        /// <summary>Reutiliza MinMaterialCapital FASE 7.8 (RD$1,000).</summary>
        public static decimal MinMaterialCapital { get; } =
            InventoryHealthThresholds.Default.MinMaterialCapital;

        public static IReadOnlyList<DecisionRuleCandidate> FromTrappedCapital(
            InvestmentCapitalBridgeReport? bridge,
            string periodKey,
            int max = DefaultMaxAlerts)
        {
            if (bridge == null)
                return Array.Empty<DecisionRuleCandidate>();

            return bridge.Investments
                .Where(r => r.TrappedCapital >= MinMaterialCapital)
                .OrderByDescending(r => r.TrappedCapital)
                .ThenBy(r => r.Summary.Name, StringComparer.OrdinalIgnoreCase)
                .Take(Math.Max(1, max))
                .Select(r => BuildFrozen(r, periodKey))
                .ToList();
        }

        public static IReadOnlyList<DecisionRuleCandidate> FromSummaries(
            IEnumerable<InvestmentSummary>? summaries,
            string periodKey,
            int max = DefaultMaxAlerts)
        {
            if (summaries == null)
                return Array.Empty<DecisionRuleCandidate>();

            return summaries
                .Where(IsRoiWeak)
                .OrderBy(s => s.RoiRealizedPct ?? 0m)
                .ThenByDescending(s => s.CapitalInvested)
                .ThenBy(s => s.Name, StringComparer.OrdinalIgnoreCase)
                .Take(Math.Max(1, max))
                .Select(s => BuildRoiWeak(s, periodKey))
                .ToList();
        }

        public static bool IsRoiWeak(InvestmentSummary s)
        {
            if (!s.HasReliableCost)
                return false;
            if (s.CapitalInvested < MinMaterialCapital)
                return false;
            if (s.Status is InvestmentStatus.Planificada)
                return false;

            if (s.IsLoss)
                return true;

            return s.RoiRealizedPct.HasValue && s.RoiRealizedPct.Value < 0m;
        }

        private static DecisionRuleCandidate BuildFrozen(
            InvestmentTrappedCapitalRow row,
            string periodKey)
        {
            InvestmentSummary s = row.Summary;
            return new DecisionRuleCandidate
            {
                RuleId = "invst.alerts.frozen",
                EventType = "invst.frozen_capital",
                Area = DecisionEventArea.Investment,
                EntityType = DecisionEntityType.Investment,
                EntityId = s.InvestmentId.ToString(CultureInfo.InvariantCulture),
                EntityName = s.Name,
                PeriodKey = periodKey,
                Title = "Frozen capital de inversión",
                Description =
                    $"{s.Name}: capital atrapado {row.TrappedCapital.ToString("N2", Cultura)} "
                    + $"(FIFO). Productos Frozen/Critical vinculados: {row.ProductsFrozenOrCritical}.",
                Reason =
                    "FrozenCapital de inversión ≠ InventoryCapital global del producto. "
                    + "Capital restante atribuible a esta inversión.",
                Recommendation =
                    "Revisar productos vinculados y plan de recuperación — no liquidar automáticamente.",
                Source = "InvestmentCapitalBridgeService",
                Materiality = new DecisionMaterialityInput
                {
                    CapitalAmount = row.TrappedCapital
                },
                ImpactAssessment = new DecisionImpactAssessment
                {
                    Capital = row.TrappedCapital >= 10_000m
                        ? DecisionImpactLevel.Critical
                        : DecisionImpactLevel.High,
                    Liquidity = DecisionImpactLevel.High,
                    // TEST 11: no asumir crítico solo por frozen si aún hay recuperación
                    ProductStillSelling = s.RecoveryPct.HasValue && s.RecoveryPct.Value > 0m
                },
                Urgency = row.ProductsFrozenOrCritical > 0
                    ? DecisionUrgencyLevel.High
                    : DecisionUrgencyLevel.Medium,
                ProductStillSelling = s.RecoveryPct.HasValue && s.RecoveryPct.Value > 0m,
                Evidence =
                [
                    new DecisionEvidenceFact
                    {
                        Key = "frozen_capital",
                        Label = "Frozen (inv.)",
                        ValueText = row.TrappedCapital.ToString("N2", Cultura),
                        MetricKey = "invst.frozen_capital"
                    },
                    new DecisionEvidenceFact
                    {
                        Key = "invested",
                        Label = "Capital invertido",
                        ValueText = s.CapitalInvested.ToString("N2", Cultura),
                        MetricKey = "invst.capital_invested"
                    },
                    new DecisionEvidenceFact
                    {
                        Key = "products_frozen",
                        Label = "SKUs Frozen/Critical",
                        ValueText = row.ProductsFrozenOrCritical.ToString(Cultura)
                    }
                ],
                MetricKeys = ["invst.frozen_capital", "invst.capital_invested", "invst.status"]
            };
        }

        private static DecisionRuleCandidate BuildRoiWeak(
            InvestmentSummary s,
            string periodKey)
        {
            string roiText = s.RoiRealizedPct.HasValue
                ? s.RoiRealizedPct.Value.ToString("N2", Cultura) + " %"
                : "N/D";

            return new DecisionRuleCandidate
            {
                RuleId = "invst.alerts.roi_weak",
                EventType = "invst.roi_weak",
                Area = DecisionEventArea.Investment,
                EntityType = DecisionEntityType.Investment,
                EntityId = s.InvestmentId.ToString(CultureInfo.InvariantCulture),
                EntityName = s.Name,
                PeriodKey = periodKey,
                Title = "ROI de inversión débil",
                Description =
                    $"{s.Name}: ROI realizado {roiText}"
                    + (s.IsLoss ? " (pérdida)." : "."),
                Reason =
                    "ROI de inversión FASE 6 ≠ ROI de línea de venta. "
                    + "Señal de deterioro/pérdida — no cerrar automáticamente.",
                Recommendation =
                    "Revisar recuperación y productos vinculados — no liquidar automáticamente.",
                Source = "InvestmentService",
                Materiality = new DecisionMaterialityInput
                {
                    CapitalAmount = s.CapitalInvested,
                    VariationPct = s.RoiRealizedPct
                },
                ImpactAssessment = new DecisionImpactAssessment
                {
                    Financial = DecisionImpactLevel.High,
                    Capital = DecisionImpactLevel.Medium
                },
                Urgency = DecisionUrgencyLevel.Medium,
                Evidence =
                [
                    new DecisionEvidenceFact
                    {
                        Key = "roi_realized",
                        Label = "ROI realizado",
                        ValueText = roiText,
                        MetricKey = "invst.roi_realized_pct"
                    },
                    new DecisionEvidenceFact
                    {
                        Key = "realized_profit",
                        Label = "Ganancia inv.",
                        ValueText = s.RealizedProfit.ToString("N2", Cultura),
                        MetricKey = "invst.realized_profit"
                    },
                    new DecisionEvidenceFact
                    {
                        Key = "status",
                        Label = "Estado",
                        ValueText = s.Status.ToString(),
                        MetricKey = "invst.status"
                    }
                ],
                MetricKeys = ["invst.roi_realized_pct", "invst.realized_profit", "invst.status"]
            };
        }
    }

    /// <summary>Regla de dominio inversiones (FASE 10.17).</summary>
    public sealed class InvestmentAlertDecisionRule : IDecisionRule
    {
        private readonly Func<InvestmentCapitalBridgeReport?> _loadBridge;
        private readonly Func<IReadOnlyList<InvestmentSummary>> _loadSummaries;

        public string RuleId => "invst.alerts.v1";

        public InvestmentAlertDecisionRule()
            : this(null, null)
        {
        }

        public InvestmentAlertDecisionRule(
            Func<InvestmentCapitalBridgeReport?>? loadBridge,
            Func<IReadOnlyList<InvestmentSummary>>? loadSummaries)
        {
            if (loadBridge != null)
                _loadBridge = loadBridge;
            else
            {
                var svc = new InvestmentCapitalBridgeService();
                _loadBridge = () => svc.GetTrappedCapitalReport();
            }

            if (loadSummaries != null)
                _loadSummaries = loadSummaries;
            else
            {
                var svc = new InvestmentService();
                _loadSummaries = () => svc.List()
                    .Select(i => svc.GetSummary(i.Id))
                    .ToList();
            }
        }

        public IEnumerable<DecisionRuleCandidate> Evaluate(DecisionRuleContext context)
        {
            string periodKey = string.IsNullOrWhiteSpace(context.PeriodKey)
                ? SalesAlertRuleComposer.PeriodKey(context.PeriodKind, context.AsOf)
                : context.PeriodKey;

            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (DecisionRuleCandidate c in InvestmentAlertRuleComposer.FromTrappedCapital(
                         context.Analytics != null
                         ? context.Analytics.TrappedCapital
                         : _loadBridge(), periodKey))
            {
                string fp = DecisionFingerprint.Compute(
                    c.Area, c.EventType, c.EntityType, c.EntityId, periodKey);
                if (seen.Add(fp))
                    yield return c;
            }

            foreach (DecisionRuleCandidate c in InvestmentAlertRuleComposer.FromSummaries(
                         context.Analytics != null
                         ? context.Analytics.InvestmentSummaries
                         : _loadSummaries(), periodKey))
            {
                string fp = DecisionFingerprint.Compute(
                    c.Area, c.EventType, c.EntityType, c.EntityId, periodKey);
                if (seen.Add(fp))
                    yield return c;
            }
        }
    }
}
