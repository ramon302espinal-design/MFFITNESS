using System.Globalization;
using System.Linq;
using BLL.Models.Crm;
using BLL.Services.Crm;

namespace UI.Helpers
{
    /// <summary>
    /// Binder del Centro de decisiones (FASE 10.24).
    /// Forms solo formatean — no recalculan métricas ni ejecutan acciones.
    /// </summary>
    public static class CrmDecisionUiBinder
    {
        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-DO");

        public static string Count(int value)
            => value.ToString("N0", Cultura);

        public static string Headline(DecisionCenterSummary? summary)
            => summary == null
                ? "Centro no disponible"
                : DecisionCenterDisplay.SummaryLine(summary);

        public static string PriorityLine(DecisionCenterPriorityItem? item)
            => item == null ? string.Empty : DecisionCenterDisplay.PriorityLine(item);

        public static string BucketLabel(DecisionCenterBucket bucket)
            => DecisionCenterDisplay.BucketPrefix(bucket);

        /// <summary>
        /// Carga el Centro con reglas built-in. No persiste historial (eso es explícito).
        /// </summary>
        public static DecisionCenterReport? TryLoadCenter(
            out string? error,
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            DecisionCenterSnapshot? snapshot = null,
            int maxPriorities = DecisionCenterPolicy.DefaultMaxPriorities,
            DateTime? asOf = null)
        {
            error = null;
            try
            {
                var ctx = new DecisionRuleContext
                {
                    PeriodKind = periodKind,
                    AsOf = asOf,
                    PeriodKey = SalesAlertRuleComposer.PeriodKey(periodKind, asOf)
                };

                return new DecisionCenterService().RunBuiltIn(
                    ctx,
                    snapshot,
                    maxPriorities);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        /// <summary>Líneas listas para lblDecision1..N del Dashboard.</summary>
        public static IReadOnlyList<string> TryLoadDashboardDecisionLines(
            out string? error,
            out DecisionCenterReport? report,
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            DecisionCenterSnapshot? snapshot = null,
            int maxLines = 3)
        {
            report = TryLoadCenter(out error, periodKind, snapshot, maxPriorities: Math.Max(maxLines, 5));
            if (report == null)
            {
                return
                [
                    "Centro de decisiones no disponible.",
                    string.IsNullOrWhiteSpace(error) ? string.Empty : error!,
                    string.Empty
                ];
            }

            return DecisionCenterDisplay.DashboardLines(report, maxLines);
        }

        public static int CountEventsInAreas(DecisionCenterReport? report, params DecisionEventArea[] areas)
            => report == null ? 0 : DecisionCenterDisplay.CountEventsInAreas(report, areas);

        public static string FormatPriorityFeed(DecisionCenterReport? report, int max = 8)
        {
            if (report == null || report.PrioritiesToday.Count == 0)
                return "Sin prioridades del Centro.";

            var sb = new System.Text.StringBuilder();
            foreach (DecisionCenterPriorityItem p in report.PrioritiesToday.Take(max))
                sb.AppendLine(DecisionCenterDisplay.PriorityLine(p, maxLen: 140));
            return sb.ToString().TrimEnd();
        }

        public static DecisionCenterSnapshot BuildSnapshot(
            decimal? salesVarPct,
            decimal? profitVarPct,
            decimal? frozenCapital)
            => new()
            {
                SalesVariationPct = salesVarPct,
                ProfitVariationPct = profitVarPct,
                FrozenCapitalAmount = frozenCapital is > 0 ? frozenCapital : null,
                FrozenCapitalLabel = "Capital congelado"
            };

        /// <summary>
        /// Hint de prioridad por área (Forms de dominio). Vacío si no hay señal.
        /// </summary>
        public static string TryFormatAreaHint(
            out string? error,
            DecisionCenterSnapshot? snapshot = null,
            params DecisionEventArea[] areas)
        {
            DecisionCenterReport? center = TryLoadCenter(out error, snapshot: snapshot);
            return DecisionIntegrationService.AreaPriorityHint(center, areas);
        }

        /// <summary>
        /// Persistencia opcional: Capture + ReconcileAbsent (no muta stock/caja).
        /// Falla silenciosa si SQL/migración no disponible.
        /// </summary>
        public static DecisionIntegrationResult? TryCaptureAndReconcile(
            out string? error,
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            DecisionCenterSnapshot? snapshot = null,
            DateTime? asOf = null)
        {
            error = null;
            try
            {
                var ctx = new DecisionRuleContext
                {
                    PeriodKind = periodKind,
                    AsOf = asOf,
                    PeriodKey = SalesAlertRuleComposer.PeriodKey(periodKind, asOf)
                };

                var history = new DecisionHistoryService();
                return new DecisionIntegrationService().RunAndPersist(
                    history, ctx, snapshot);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }
    }
}
