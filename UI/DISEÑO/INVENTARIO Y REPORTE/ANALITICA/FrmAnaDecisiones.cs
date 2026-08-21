using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BLL.Models.Crm;
using UI.Helpers;
using UI.Theme;

namespace UI
{
    /// <summary>
    /// Decisiones CRM — FASE 10.25/10.28: Centro + captura opcional de historial.
    /// KPIs de contexto vía binders. Sin auto-acciones.
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
        }
    }
}
