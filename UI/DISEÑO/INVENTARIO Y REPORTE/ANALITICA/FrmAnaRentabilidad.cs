using System.Windows.Forms;
using BLL.Models.Crm;
using UI.Helpers;
using UI.Theme;

namespace UI
{
    /// <summary>
    /// Rentabilidad — FASE 5.11: margen y ROI del mes (paneles existentes).
    /// </summary>
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmAnaRentabilidad : Form
    {
        public FrmAnaRentabilidad()
        {
            InitializeComponent();
            CrmVisualTokens.MarkClassic(this);
            Load += (_, _) => CargarDatos();
        }

        private void CargarDatos()
        {
            ProfitSummary? month = CrmProfitUiBinder.TryLoadThisMonth(out string? error);
            if (month == null)
            {
                MessageBox.Show(
                    "No se pudo cargar rentabilidad.\n" + (error ?? ""),
                    "CRM Financiero",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            lblHeaderLocal.Text = "Rentabilidad";

            lblReservavisualTitle.Text = "Margen";
            lblReservavisualValue.Text = CrmProfitUiBinder.Pct(month.MarginPct);
            lblReservavisualDesc.Text =
                $"Ganancia / ingreso con costo · {CrmProfitUiBinder.Money(month.RealizedProfit)}";

            lblSinusoensidebarTitle.Text = "ROI";
            lblSinusoensidebarValue.Text = CrmProfitUiBinder.Pct(month.RoiPct);
            lblSinusoensidebarDesc.Text =
                $"Ganancia / COGS · Ventas {CrmProfitUiBinder.Money(month.RevenueTotal)}";
        }
    }
}
