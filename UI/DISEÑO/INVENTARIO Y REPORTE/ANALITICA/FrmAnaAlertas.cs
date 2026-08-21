using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BLL.Models.Crm;
using UI.Helpers;
using UI.Theme;

namespace UI
{
    /// <summary>
    /// Alertas CRM — FASE 10.25 Centro + FASE 11.18 enlace decisión/acción/resultado.
    /// Sin auto-acciones.
    /// </summary>
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmAnaAlertas : Form
    {
        public FrmAnaAlertas()
        {
            InitializeComponent();
            CrmVisualTokens.MarkClassic(this);
            Load += (_, _) => CargarDatos();
        }

        private void CargarDatos()
        {
            InventoryAlertReport? report = CrmInventoryUiBinder.TryLoadAlerts(out string? error, top: 25);
            SalesDashboardReport? sales = CrmSalesUiBinder.TryLoadDashboard(out _, ProfitPeriodKind.ThisMonth);
            InventoryCapitalHealthReport? health = CrmInventoryUiBinder.TryLoadHealth(out _);

            decimal frozen = health?.ImmobilizedCapital
                ?? report?.ImmobilizedCapital
                ?? 0m;

            DecisionCenterSnapshot snapshot = CrmDecisionUiBinder.BuildSnapshot(
                sales?.RevenueVariation?.VariationPct,
                sales?.ProfitVariation?.VariationPct,
                frozen);

            DecisionCenterReport? center = CrmDecisionUiBinder.TryLoadCenter(
                out string? centerError,
                ProfitPeriodKind.ThisMonth,
                snapshot);

            if (center == null && report == null && sales == null)
            {
                MessageBox.Show(
                    "No se pudieron cargar alertas.\n" + (error ?? centerError ?? ""),
                    "CRM Financiero",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (center != null)
            {
                DecisionCenterSummary s = center.Summary;

                lblCriticasValue.Text = CrmDecisionUiBinder.Count(s.CriticalCount);
                lblCriticasDesc.Text = "Cubo CRÍTICAS del Centro";

                lblImportantesValue.Text = CrmDecisionUiBinder.Count(s.ImportantCount);
                lblImportantesDesc.Text = "Cubo IMPORTANTES";

                lblAdvertenciasValue.Text = CrmDecisionUiBinder.Count(s.ReviewCount);
                lblAdvertenciasDesc.Text = "Cubo REVISAR";

                lblInformacionValue.Text = CrmDecisionUiBinder.Count(s.OpportunityCount);
                lblInformacionDesc.Text = "Cubo OPORTUNIDADES · ≠ auto-compra";

                int invArea = CrmDecisionUiBinder.CountEventsInAreas(
                    center,
                    DecisionEventArea.Inventory,
                    DecisionEventArea.Capital,
                    DecisionEventArea.Liquidity);
                lblAlertasdeinventarioValue.Text = CrmDecisionUiBinder.Count(invArea);
                lblAlertasdeinventarioDesc.Text = report != null
                    ? $"Centro inv/capital · legacy {CrmInventoryUiBinder.Count(report.TotalAlerts)}"
                    : "Eventos inventario/capital del Centro";

                int finArea = CrmDecisionUiBinder.CountEventsInAreas(
                    center,
                    DecisionEventArea.Sales,
                    DecisionEventArea.Profit,
                    DecisionEventArea.Margin,
                    DecisionEventArea.Roi);
                lblAlertasfinancierasValue.Text = CrmDecisionUiBinder.Count(finArea);
                lblAlertasfinancierasDesc.Text = sales?.RevenueVariation != null
                    ? $"Centro · var. ingresos {CrmSalesUiBinder.VariationDisplay(sales.RevenueVariation)}"
                    : "Eventos ventas/ganancia/margen/ROI";

                int rentArea = CrmDecisionUiBinder.CountEventsInAreas(
                    center,
                    DecisionEventArea.Profit,
                    DecisionEventArea.Margin,
                    DecisionEventArea.Roi,
                    DecisionEventArea.Product);
                lblAlertasderentabilidadValue.Text = CrmDecisionUiBinder.Count(rentArea);
                lblAlertasderentabilidadDesc.Text = "Rentabilidad / producto (Centro)";

                lblHistorialValue.Text = CrmDecisionUiBinder.Count(s.TotalEvents);
                lblHistorialDesc.Text = CrmDecisionUiBinder.FormatPriorityFeed(center, max: 8);
            }
            else
            {
                // Fallback legacy (FASE 7/9) si el Centro falla
                int salesAlerts = sales?.Alerts.Count ?? 0;

                lblCriticasValue.Text = CrmInventoryUiBinder.Count(report?.CriticalCount ?? 0);
                lblCriticasDesc.Text = "Prioridad crítica (inventario) · Centro N/D";

                lblImportantesValue.Text = CrmInventoryUiBinder.Count(report?.HighCount ?? 0);
                lblImportantesDesc.Text = "Prioridad alta (inventario)";

                lblAdvertenciasValue.Text = CrmInventoryUiBinder.Count(
                    (report?.MediumCount ?? 0) + salesAlerts);
                lblAdvertenciasDesc.Text = salesAlerts > 0
                    ? $"Media inv. + {CrmSalesUiBinder.Count(salesAlerts)} alertas ventas"
                    : "Prioridad media";

                lblInformacionValue.Text = CrmInventoryUiBinder.Count(report?.LowCount ?? 0);
                lblInformacionDesc.Text = "Prioridad baja";

                lblAlertasdeinventarioValue.Text = CrmInventoryUiBinder.Count(report?.TotalAlerts ?? 0);
                lblAlertasdeinventarioDesc.Text = report != null
                    ? $"Inmovilizado {CrmInventoryUiBinder.Money(report.ImmobilizedCapital)}"
                    : "Sin alertas inventario";

                lblAlertasfinancierasValue.Text = sales?.RevenueVariation != null
                    ? CrmSalesUiBinder.VariationDisplay(sales.RevenueVariation)
                    : CrmInventoryUiBinder.Pct(report?.FrozenSharePct);
                lblAlertasfinancierasDesc.Text = sales?.RevenueVariation != null
                    ? $"Var. ingresos · {CrmSalesUiBinder.TrendLabel(sales.RevenueTrend)}"
                    : "% capital congelado";

                int stockout = (report?.Alerts.Count(a => a.Kind == InventoryAlertKind.StockoutRisk) ?? 0)
                    + (sales?.StockoutRiskCount ?? 0);
                lblAlertasderentabilidadValue.Text = CrmInventoryUiBinder.Count(stockout);
                lblAlertasderentabilidadDesc.Text = "Riesgo de quiebre (inv. + ventas)";

                var sb = new StringBuilder();
                if (sales != null)
                {
                    foreach (SalesDashboardAlert a in sales.Alerts.Take(6))
                        sb.AppendLine($"{a.Icon} {a.Message}");
                }

                if (report != null)
                {
                    foreach (InventoryAlert a in report.Alerts.Take(8))
                    {
                        string who = string.IsNullOrEmpty(a.ProductName) ? a.Kind.ToString() : a.ProductName;
                        sb.AppendLine($"{a.Priority} · {who}: {a.Message}");
                    }
                }

                if (sb.Length == 0)
                    sb.Append("Sin alertas.");

                lblHistorialValue.Text = CrmInventoryUiBinder.Count(
                    (report?.TotalAlerts ?? 0) + salesAlerts);
                lblHistorialDesc.Text = sb.ToString().TrimEnd();
            }

            CargarPanelEnlace(center);
        }

        /// <summary>FASE 11.18 — lista prioritaria + VER DECISIÓN / ACCIÓN / RESULTADO.</summary>
        private void CargarPanelEnlace(DecisionCenterReport? center)
        {
            lstAlertasPrioridad.Items.Clear();
            IReadOnlyList<AlertDecisionLinkItem> links = CrmAlertLinkUiBinder.ToLinkItems(center);

            if (links.Count == 0)
            {
                lstAlertasPrioridad.Items.Add("Sin prioridades del Centro para enlazar.");
                txtEnlaceDetalle.Text =
                    "Cuando el Centro tenga prioridades, podrá ver decisión, acción y resultado asociados.";
                return;
            }

            foreach (AlertDecisionLinkItem item in links)
                lstAlertasPrioridad.Items.Add(item);

            lstAlertasPrioridad.SelectedIndex = 0;
            txtEnlaceDetalle.Text = CrmAlertLinkUiBinder.FormatDecisionView(
                lstAlertasPrioridad.SelectedItem as AlertDecisionLinkItem);
        }

        private AlertDecisionLinkItem? SeleccionEnlace()
            => lstAlertasPrioridad.SelectedItem as AlertDecisionLinkItem;

        private void btnVerDecision_Click(object? sender, EventArgs e)
            => txtEnlaceDetalle.Text = CrmAlertLinkUiBinder.FormatDecisionView(SeleccionEnlace());

        private void btnVerAccion_Click(object? sender, EventArgs e)
            => txtEnlaceDetalle.Text = CrmAlertLinkUiBinder.FormatActionsView(SeleccionEnlace());

        private void btnVerResultadoAlerta_Click(object? sender, EventArgs e)
            => txtEnlaceDetalle.Text = CrmAlertLinkUiBinder.FormatResultView(SeleccionEnlace());

        private void btnIrDecisiones_Click(object? sender, EventArgs e)
        {
            Control? c = Parent;
            while (c != null && c is not FrmCRMFinanciero)
                c = c.Parent;

            if (c is FrmCRMFinanciero crm)
                crm.MostrarDecisiones();
            else
                MessageBox.Show(
                    "Abra el módulo CRM Financiero para registrar/ver acciones.",
                    "Alertas",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
        }
    }
}
