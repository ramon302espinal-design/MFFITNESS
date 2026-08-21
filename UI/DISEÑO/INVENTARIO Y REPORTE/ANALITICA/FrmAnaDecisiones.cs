using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BLL.Models.Crm;
using UI.Helpers;
using UI.Theme;

namespace UI
{
    /// <summary>
    /// Decisiones CRM — FASE 10.25 Centro + FASE 11.15 REGISTRAR + 11.16 VER RESULTADO.
    /// KPIs y registro vía binders. Sin auto-acciones / sin mutar POS.
    /// </summary>
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmAnaDecisiones : Form
    {
        public FrmAnaDecisiones()
        {
            InitializeComponent();
            CrmVisualTokens.MarkClassic(this);
            Load += (_, _) => CargarDatos();
        }

        private void CargarDatos()
        {
            var summaries = CrmInvestmentUiBinder.TryLoadAllSummaries(out _);
            InventoryCapitalHealthReport? health = CrmInventoryUiBinder.TryLoadHealth(out _);
            InventoryRiskReport? risk = CrmInventoryUiBinder.TryLoadRisk(out _);
            ProductPerformanceDashboardReport? dash =
                CrmProductPerformanceUiBinder.TryLoadDashboard(out _, topLists: 5);
            SalesDashboardReport? salesDash =
                CrmSalesUiBinder.TryLoadDashboard(out _, ProfitPeriodKind.ThisMonth);

            decimal immobilized = dash?.TotalImmobilizedCapital
                ?? health?.ImmobilizedCapital
                ?? risk?.ImmobilizedCapital
                ?? 0m;

            DecisionCenterSnapshot snapshot = CrmDecisionUiBinder.BuildSnapshot(
                salesDash?.RevenueVariation?.VariationPct,
                salesDash?.ProfitVariation?.VariationPct,
                immobilized);

            DecisionCenterReport? center = CrmDecisionUiBinder.TryLoadCenter(
                out _,
                ProfitPeriodKind.ThisMonth,
                snapshot,
                maxPriorities: 8);

            // Persistencia best-effort (migración 0010/0011). No bloquea UI.
            _ = CrmDecisionUiBinder.TryCaptureAndReconcile(out _, snapshot: snapshot);

            // ——— Contexto inversiones / capital (binders existentes) ———
            if (summaries == null || summaries.Count == 0)
            {
                lblInversionesValue.Text = "—";
                lblInversionesDesc.Text = "Sin inversiones / migración 0009";
            }
            else
            {
                var best = summaries
                    .Where(s => s.RoiRealizedPct.HasValue)
                    .OrderByDescending(s => s.RoiRealizedPct)
                    .FirstOrDefault();

                int activas = summaries.Count(s =>
                    s.Status is InvestmentStatus.Activa or InvestmentStatus.Recuperada);

                lblInversionesValue.Text = best != null
                    ? CrmInvestmentUiBinder.Pct(best.RoiRealizedPct)
                    : CrmInvestmentUiBinder.Count(activas);

                lblInversionesDesc.Text = best != null
                    ? $"Mejor ROI: {best.Name} · {CrmInvestmentUiBinder.Count(activas)} activas"
                    : $"{CrmInvestmentUiBinder.Count(activas)} activas/recuperadas";
            }

            lblCapitalcongeladoValue.Text = CrmInventoryUiBinder.Money(immobilized);
            lblCapitalcongeladoDesc.Text = health != null
                ? $"Inmovilizado · {CrmInventoryUiBinder.Pct(health.ImmobilizedSharePct)} inventario"
                : "Frozen + Critical";

            lblPreciosValue.Text = "Sim.";
            lblPreciosDesc.Text = "Liquidación = simulación · no muta PrecioVenta";

            // ——— Centro de decisiones (FASE 10.25) ———
            if (center != null)
            {
                DecisionCenterSummary s = center.Summary;

                lblDecisionesValue.Text = CrmDecisionUiBinder.Count(s.TotalGroups);
                lblDecisionesDesc.Text = CrmDecisionUiBinder.Headline(s);

                lblRiesgosValue.Text = CrmDecisionUiBinder.Count(s.CriticalCount);
                lblRiesgosDesc.Text =
                    $"{CrmDecisionUiBinder.Count(s.ImportantCount)} importantes · " +
                    $"{CrmDecisionUiBinder.Count(s.ReviewCount)} a revisar";

                lblOportunidadesValue.Text = CrmDecisionUiBinder.Count(s.OpportunityCount);
                lblOportunidadesDesc.Text = "Oportunidades del Centro · ≠ auto-compra";

                if (center.TopPriority != null)
                {
                    lblReposicionValue.Text = center.TopPriority.Title;
                    if (lblReposicionValue.Text.Length > 28)
                        lblReposicionValue.Text = lblReposicionValue.Text[..25] + "…";
                    lblReposicionDesc.Text =
                        $"{CrmDecisionUiBinder.BucketLabel(center.TopPriority.Bucket)} · " +
                        center.TopPriority.Recommendation + " · sin auto-acción";
                }
                else
                {
                    lblReposicionValue.Text = "—";
                    lblReposicionDesc.Text = "Sin prioridad activa hoy";
                }
            }
            else
            {
                // Fallback legacy si el Centro no carga
                IReadOnlyList<ProductClassificationRow>? opps =
                    CrmProductPerformanceUiBinder.TryLoadOpportunities(out _, top: 5);
                IReadOnlyList<ProductClassificationRow>? risks8 =
                    CrmProductPerformanceUiBinder.TryLoadRisks(out _, top: 5);
                SalesDecisionReport? salesDecisions = CrmSalesUiBinder.TryLoadDecisions(out _);

                if (risks8 != null && risks8.Count > 0)
                {
                    lblRiesgosValue.Text = risks8[0].ProductName;
                    lblRiesgosDesc.Text = "Fallback FASE 8 · Centro no disponible";
                }
                else
                {
                    lblRiesgosValue.Text = "—";
                    lblRiesgosDesc.Text = "Sin riesgos / Centro no disponible";
                }

                if (opps != null && opps.Count > 0)
                {
                    lblOportunidadesValue.Text = opps[0].ProductName;
                    lblOportunidadesDesc.Text = "Fallback Opp · Centro no disponible";
                }
                else
                {
                    lblOportunidadesValue.Text = "—";
                    lblOportunidadesDesc.Text = "Sin oportunidades";
                }

                lblDecisionesValue.Text = salesDecisions != null
                    ? CrmSalesUiBinder.Count(salesDecisions.SignalCount)
                    : "—";
                lblDecisionesDesc.Text = salesDecisions?.Primary?.Message ?? "Centro no disponible";

                lblReposicionValue.Text = salesDecisions?.Primary?.Title ?? "—";
                lblReposicionDesc.Text = salesDecisions?.Primary != null
                    ? salesDecisions.Primary.Message + " · sin auto-acción"
                    : "Sin decisión prioritaria";
            }

            CargarPanelAcciones();
        }

        /// <summary>FASE 11.15 — panel REGISTRAR ACCIÓN (vía binder).</summary>
        private void CargarPanelAcciones()
        {
            (int pending, int inProgress, int completed) =
                CrmBusinessActionUiBinder.TryCountOpen(out string? countErr);

            int open = pending + inProgress;
            lblAccionesValue.Text = countErr != null
                ? "—"
                : CrmBusinessActionUiBinder.Count(open);
            lblAccionesDesc.Text = countErr != null
                ? "Acciones no disponibles (migración 0012+)"
                : CrmBusinessActionUiBinder.OpenSummaryLine(pending, inProgress, completed)
                  + " · " + CrmBusinessActionUiBinder.ClosedLoopStatusLine();

            if (cmbTipoAccion.DataSource == null)
            {
                cmbTipoAccion.DataSource = CrmBusinessActionUiBinder.TypeChoices().ToList();
                cmbTipoAccion.DisplayMember = nameof(BusinessActionTypeChoice.Display);
            }

            cmbDecisionVinculo.DataSource = null;
            cmbDecisionVinculo.DataSource = CrmBusinessActionUiBinder.TryLoadDecisionLinks(out _).ToList();
            cmbDecisionVinculo.DisplayMember = nameof(DecisionLinkChoice.Display);
            if (cmbDecisionVinculo.Items.Count > 0)
                cmbDecisionVinculo.SelectedIndex = 0;

            RefrescarListaAcciones();
        }

        private void RefrescarListaAcciones()
        {
            lstAccionesRecientes.Items.Clear();
            IReadOnlyList<BusinessActionRecord>? recent =
                CrmBusinessActionUiBinder.TryListRecent(out string? err, top: 12);

            if (recent == null)
            {
                lstAccionesRecientes.Items.Add(
                    string.IsNullOrWhiteSpace(err)
                        ? "Sin historial de acciones."
                        : $"No se pudo cargar: {err}");
                txtResultadoDetalle.Text = "Acciones no disponibles.";
                return;
            }

            if (recent.Count == 0)
            {
                lstAccionesRecientes.Items.Add("Sin acciones registradas aún.");
                txtResultadoDetalle.Text = "Sin acciones. Registre una para ver resultado/impacto.";
                return;
            }

            foreach (BusinessActionListItem item in CrmBusinessActionUiBinder.ToListItems(recent))
                lstAccionesRecientes.Items.Add(item);
        }

        private BusinessActionRecord? AccionSeleccionada()
            => lstAccionesRecientes.SelectedItem as BusinessActionListItem is { } item
                ? item.Record
                : null;

        private void btnRegistrarAccion_Click(object? sender, EventArgs e)
        {
            if (cmbTipoAccion.SelectedItem is not BusinessActionTypeChoice typeChoice)
            {
                MessageBox.Show(
                    "Seleccione un tipo de acción.",
                    "Registrar acción",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            string desc = txtDescAccion.Text?.Trim() ?? string.Empty;
            DecisionLinkChoice? link = cmbDecisionVinculo.SelectedItem as DecisionLinkChoice;

            BusinessActionServiceResult? result = CrmBusinessActionUiBinder.TryRegister(
                out string? error,
                typeChoice.Type,
                desc,
                decisionEventId: link?.EventId,
                decisionHistoryId: link?.HistoryId,
                startImmediately: chkIniciarAccion.Checked);

            if (result == null || !result.Success)
            {
                MessageBox.Show(
                    error ?? result?.Message ?? "No se pudo registrar la acción.",
                    "Registrar acción",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            txtDescAccion.Clear();
            chkIniciarAccion.Checked = false;
            CargarPanelAcciones();

            MessageBox.Show(
                result.Message + "\n\nRecordatorio: el CRM solo registra; usted ejecuta en el POS.",
                "Acción registrada",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void btnMarcarCompletada_Click(object? sender, EventArgs e)
        {
            BusinessActionRecord? selected = AccionSeleccionada();
            if (selected == null)
            {
                MessageBox.Show(
                    "Seleccione una acción de la lista.",
                    "Completar acción",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            BusinessActionServiceResult? result = CrmBusinessActionUiBinder.TryComplete(
                out string? error,
                selected.ActionId);

            if (result == null || !result.Success)
            {
                MessageBox.Show(
                    error ?? result?.Message ?? "No se pudo completar.",
                    "Completar acción",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            CargarPanelAcciones();
            txtResultadoDetalle.Text = CrmBusinessActionUiBinder.FormatImpactReport(result.Record);
            MessageBox.Show(
                result.Message + "\n\nLuego puede VER RESULTADO cuando haya deltas/evaluación.",
                "Acción completada",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void btnVerResultado_Click(object? sender, EventArgs e)
        {
            BusinessActionRecord? selected = AccionSeleccionada();
            if (selected == null)
            {
                MessageBox.Show(
                    "Seleccione una acción de la lista.",
                    "Ver resultado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            BusinessActionRecord? fresh =
                CrmBusinessActionUiBinder.TryGet(selected.ActionId, out _) ?? selected;

            // Si ya está Completada con deltas y sin Outcome, intentar evaluar (best-effort).
            if (fresh.Status == BusinessActionStatus.Completed
                && fresh.ActualImpact?.Deltas is { Count: > 0 }
                && (fresh.ActualImpact.Outcome is BusinessActionOutcome.Unspecified
                    or BusinessActionOutcome.InsufficientData))
            {
                BusinessActionEvaluationResult? eval =
                    CrmBusinessActionUiBinder.TryEvaluate(out _, fresh.ActionId);
                if (eval is { Success: true, Record: not null })
                    fresh = eval.Record;
            }

            txtResultadoDetalle.Text = CrmBusinessActionUiBinder.FormatImpactReport(fresh);
        }

        private void btnIniciarAccion_Click(object? sender, EventArgs e)
        {
            BusinessActionRecord? selected = AccionSeleccionada();
            if (selected == null)
            {
                MessageBox.Show("Seleccione una acción.", "Iniciar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            BusinessActionServiceResult? result =
                CrmBusinessActionUiBinder.TryStart(out string? error, selected.ActionId);
            if (result == null || !result.Success)
            {
                MessageBox.Show(error ?? result?.Message ?? "No se pudo iniciar.", "Iniciar",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CargarPanelAcciones();
            txtResultadoDetalle.Text = result.Message + "\n" + CrmBusinessActionUiBinder.NoPosMutation;
        }

        private void btnCancelarAccion_Click(object? sender, EventArgs e)
        {
            BusinessActionRecord? selected = AccionSeleccionada();
            if (selected == null)
            {
                MessageBox.Show("Seleccione una acción.", "Cancelar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show(
                    "¿Cancelar esta acción? (no se evaluará como Exitosa)",
                    "Cancelar acción",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            BusinessActionServiceResult? result =
                CrmBusinessActionUiBinder.TryCancel(out string? error, selected.ActionId);
            if (result == null || !result.Success)
            {
                MessageBox.Show(error ?? result?.Message ?? "No se pudo cancelar.", "Cancelar",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CargarPanelAcciones();
            txtResultadoDetalle.Text = result.Message;
        }

        private void btnVerTimeline_Click(object? sender, EventArgs e)
        {
            BusinessActionRecord? selected = AccionSeleccionada();
            if (selected == null)
            {
                MessageBox.Show("Seleccione una acción.", "Timeline", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            txtResultadoDetalle.Text = CrmBusinessActionUiBinder.FormatTimeline(selected.ActionId);
        }
    }
}
