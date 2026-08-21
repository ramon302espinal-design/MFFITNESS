using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BLL.Models.Crm;
using UI.Helpers;
using UI.Theme;

namespace UI
{
    /// <summary>
    /// Capital congelado — FASE 7.14: buckets de salud + inmovilizado clasificado.
    /// </summary>
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmAnaCapitalCongelado : Form
    {
        public FrmAnaCapitalCongelado()
        {
            InitializeComponent();
            CrmVisualTokens.MarkClassic(this);
            Load += (_, _) => CargarDatos();
        }

        private void CargarDatos()
        {
            InventoryCapitalHealthReport? health = CrmInventoryUiBinder.TryLoadHealth(out string? error);
            FrozenCapitalReport? frozen = health?.Frozen
                ?? CrmInventoryUiBinder.TryLoadFrozen(out error);
            InventoryFinancialSummary? summary = CrmInventoryUiBinder.TryLoadSummary(out _);
            InventoryRiskReport? risk = CrmInventoryUiBinder.TryLoadRisk(out _);

            if (frozen == null && health == null)
            {
                MessageBox.Show(
                    "No se pudo cargar capital congelado.\n" + (error ?? ""),
                    "CRM Financiero",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            decimal immobilized = health?.ImmobilizedCapital ?? frozen!.TotalFrozenCapital;
            decimal? share = health?.ImmobilizedSharePct ?? frozen?.FrozenSharePct;

            lblCapitalcongeladoValue.Text = CrmInventoryUiBinder.Money(immobilized);
            var invSummaries = CrmInvestmentUiBinder.TryLoadAllSummaries(out _);
            decimal invFrozen = invSummaries?.Sum(s => s.FrozenCapital) ?? 0m;
            string shareTxt = CrmInventoryUiBinder.Pct(share);
            lblCapitalcongeladoDesc.Text = invFrozen > 0
                ? $"Clasificado {shareTxt} · Inv.etiquetado {CrmInvestmentUiBinder.Money(invFrozen)}"
                : $"Frozen+Critical · {shareTxt} del inventario";

            if (share.HasValue)
            {
                lblPorcentajeValue.Text = shareTxt;
                lblPorcentajeDesc.Text = "% capital inventario clasificado congelado";
            }
            else
            {
                lblPorcentajeValue.Text = "N/D";
                lblPorcentajeDesc.Text = "Sin base para % congelado";
            }

            lblProductosafectadosValue.Text = CrmInventoryUiBinder.Count(
                frozen?.ProductsWithFrozenCapital ?? 0);
            lblProductosafectadosDesc.Text = "Productos Frozen/Critical";

            decimal avgDays = summary != null ? CrmInventoryUiBinder.AvgIdleDays(summary) : 0m;
            lblDiasinmovilizadoValue.Text = avgDays > 0 ? $"{avgDays:0}" : "—";
            lblDiasinmovilizadoDesc.Text = "Promedio idle (c/stock)";

            if (health != null)
            {
                lblProductoscriticosValue.Text = CrmInventoryUiBinder.Money(health.CriticalCapital);
                lblProductoscriticosDesc.Text =
                    $"Crítico · Saludable {CrmInventoryUiBinder.Money(health.HealthyCapital)}";
                lblProductoslentosValue.Text = CrmInventoryUiBinder.Money(health.SlowCapital);
                lblProductoslentosDesc.Text = risk != null
                    ? $"Lento · En riesgo {CrmInventoryUiBinder.Money(risk.AtRiskCapital)}"
                    : "Capital lento";
            }
            else if (summary != null)
            {
                lblProductoscriticosValue.Text = CrmInventoryUiBinder.Count(summary.ProductsCritical);
                lblProductoscriticosDesc.Text = "Productos críticos";
                lblProductoslentosValue.Text = CrmInventoryUiBinder.Count(summary.ProductsSlow);
                lblProductoslentosDesc.Text = "Productos lentos";
            }

            var sb = new StringBuilder();
            if (health != null)
            {
                sb.AppendLine(
                    $"Inventario {CrmInventoryUiBinder.Money(health.InventoryCapitalTotal)} · " +
                    $"S {CrmInventoryUiBinder.Money(health.HealthyCapital)} · " +
                    $"L {CrmInventoryUiBinder.Money(health.SlowCapital)} · " +
                    $"F {CrmInventoryUiBinder.Money(health.FrozenStatusCapital)} · " +
                    $"C {CrmInventoryUiBinder.Money(health.CriticalCapital)}");
            }

            foreach (FrozenCapitalItem item in (frozen?.Items ?? Array.Empty<FrozenCapitalItem>()).Take(8))
            {
                sb.AppendLine(
                    $"#{item.Rank} [{item.HealthStatus}] {item.ProductName}: " +
                    $"{CrmInventoryUiBinder.Money(item.FrozenCapital)}" +
                    (item.SharePct.HasValue ? $" ({CrmInventoryUiBinder.Pct(item.SharePct.Value)})" : ""));
            }

            if (sb.Length == 0)
                sb.Append("Sin capital congelado clasificado.");

            lblTablaValue.Text = "Buckets + top";
            lblTablaDesc.Text = sb.ToString().TrimEnd();
        }
    }
}
