using System;
using System.Linq;
using System.Windows.Forms;
using BLL.Models.Crm;
using UI.Helpers;
using UI.Theme;

namespace UI
{
    /// <summary>
    /// Analítica inventario / rentabilidad — FASE 4.10.
    /// </summary>
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmAnaliticaInventario : Form
    {
        public FrmAnaliticaInventario()
        {
            InitializeComponent();
            CrmVisualTokens.MarkClassic(this);
            Load += (_, _) => CargarDatos();
        }

        private void CargarDatos()
        {
            InventoryFinancialSummary? summary = CrmInventoryUiBinder.TryLoadSummary(out string? error);
            if (summary == null)
            {
                MessageBox.Show(
                    "No se pudo cargar analítica de inventario.\n" + (error ?? ""),
                    "CRM Financiero",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            lblValordelinventarioValue.Text = CrmInventoryUiBinder.Money(summary.InventoryCapitalTotal);
            lblValordelinventarioDesc.Text = "Inventario a costo (capital total)";

            lblGananciapotencialValue.Text = CrmInventoryUiBinder.Money(summary.PotentialProfitTotal);
            lblGananciapotencialDesc.Text = "Valor PVP − costo del stock";

            lblMargenValue.Text = summary.RevenueTotal > 0
                ? CrmInventoryUiBinder.Pct(
                    Math.Round(summary.RealizedProfitTotal / summary.RevenueTotal * 100m, 2))
                : "N/D";
            lblMargenDesc.Text = "Margen sobre ingreso con snapshot";

            // Rotación simplificada: productos con venta / productos con stock
            int withStock = Math.Max(1, summary.ProductsWithStock);
            int rotating = summary.Rows.Count(r => r.Stock > 0 && r.UnitsSold > 0);
            decimal rotPct = Math.Round((decimal)rotating / withStock * 100m, 2);
            lblRotacionValue.Text = CrmInventoryUiBinder.Pct(rotPct);
            lblRotacionDesc.Text = $"{rotating}/{summary.ProductsWithStock} c/stock vendieron";

            lblCapitalinmovilizadoValue.Text = CrmInventoryUiBinder.Money(summary.FrozenCapitalTotal);
            lblCapitalinmovilizadoDesc.Text = summary.FrozenSharePct.HasValue
                ? $"Clasificado Frozen+Critical · {CrmInventoryUiBinder.Pct(summary.FrozenSharePct.Value)}"
                : $"Frozen: {summary.ProductsFrozen} · Críticos: {summary.ProductsCritical}";

            var top = summary.Rows
                .Where(r => r.InventoryCapital > 0)
                .OrderByDescending(r => r.InventoryCapital)
                .Take(5)
                .Select(r =>
                    $"{r.ProductName}: {CrmInventoryUiBinder.Money(r.InventoryCapital)} " +
                    $"[{CrmInventoryUiBinder.HealthLabel(r.HealthStatus)}]");
            lblTabladeproductosValue.Text = "Mayor capital";
            lblTabladeproductosDesc.Text = top.Any()
                ? string.Join(" · ", top)
                : "Sin productos con capital calculable";
        }
    }
}
