using System.Windows.Forms;
using BLL.Models.Crm;
using BLL.Services.Crm;

namespace UI.Helpers
{
    /// <summary>
    /// Hints suaves para Forms de dominio analítico (FASE 11.20).
    /// Una línea · sin saturar · sin lógica financiera en el Form · sin mutar POS.
    /// </summary>
    public static class CrmDomainHintUiBinder
    {
        public const string Policy =
            "FASE 11.20: hints en Forms de dominio (Ventas/Ganancias/Inventario/Capital/ROI/Inversiones). " +
            "Revisar/Evaluar/Considerar — nunca auto-acción ni causalidad.";

        public const string Deferred =
            "FASE 11 completa.";

        /// <summary>
        /// Hint combinado: prioridad del Centro + acciones abiertas (si hay).
        /// Vacío si no hay señal — el Form no inventa texto.
        /// </summary>
        public static string TryFormatDomainHint(
            out string? error,
            DecisionCenterSnapshot? snapshot = null,
            params DecisionEventArea[] areas)
        {
            error = null;
            if (areas.Length == 0)
                return string.Empty;

            string decisionHint = CrmDecisionUiBinder.TryFormatAreaHint(out error, snapshot, areas);

            int openActions = 0;
            try
            {
                var counters = CrmBusinessActionUiBinder.TryLoadDashboardCounters(out _);
                if (counters != null)
                    openActions = counters.Pending + counters.InProgress;
            }
            catch
            {
                // best-effort
            }

            if (string.IsNullOrWhiteSpace(decisionHint) && openActions <= 0)
                return string.Empty;

            if (string.IsNullOrWhiteSpace(decisionHint))
            {
                return openActions > 0
                    ? $"Acciones abiertas: {CrmBusinessActionUiBinder.Count(openActions)} · ver Decisiones (solo registro)."
                    : string.Empty;
            }

            if (openActions <= 0)
                return "Centro · " + Truncate(decisionHint, 110);

            return Truncate(
                $"Centro · {decisionHint} · Acciones abiertas: {CrmBusinessActionUiBinder.Count(openActions)}",
                140);
        }

        /// <summary>Aplica hint a un Label; limpia si no hay señal.</summary>
        public static void Apply(
            Label? target,
            DecisionCenterSnapshot? snapshot,
            params DecisionEventArea[] areas)
        {
            if (target == null)
                return;

            string hint = TryFormatDomainHint(out _, snapshot, areas);
            target.Text = string.IsNullOrWhiteSpace(hint)
                ? string.Empty
                : hint;
            target.Visible = !string.IsNullOrWhiteSpace(hint);
        }

        public static DecisionCenterSnapshot? TryBuildSnapshotFromSales(
            SalesDashboardReport? sales,
            decimal? frozenCapital = null)
        {
            if (sales == null && frozenCapital is null or <= 0)
                return null;

            return CrmDecisionUiBinder.BuildSnapshot(
                sales?.RevenueVariation?.VariationPct,
                sales?.ProfitVariation?.VariationPct,
                frozenCapital);
        }

        private static string Truncate(string text, int max)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= max)
                return text;
            return text[..(max - 1)] + "…";
        }
    }
}
