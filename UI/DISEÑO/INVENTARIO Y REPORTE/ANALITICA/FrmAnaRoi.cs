using System;
using System.Linq;
using System.Windows.Forms;
using BLL.Models.Crm;
using UI.Helpers;
using UI.Theme;

namespace UI
{
    /// <summary>
    /// ROI — FASE 5.11: ProfitAnalyticsService (sin lógica en el Form).
    /// </summary>
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmAnaRoi : Form
    {
        public FrmAnaRoi()
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
                    "No se pudo cargar ROI.\n" + (error ?? ""),
                    "CRM Financiero",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var topRoi = CrmProfitUiBinder.TryLoadByProduct(ProfitPeriodKind.ThisMonth, out _, top: 50)
                ?.Where(r => r.HasReliableRealizedProfit && r.RoiPct.HasValue)
                .OrderByDescending(r => r.RoiPct)
                .FirstOrDefault();

            lblROIgeneralValue.Text = CrmProfitUiBinder.Pct(month.RoiPct);
            lblROIgeneralDesc.Text = "Mes · ganancia / COGS";

            lblROIrealizadoValue.Text = CrmProfitUiBinder.Money(month.RealizedProfit);
            lblROIrealizadoDesc.Text = "Ganancia realizada del mes";

            // Proyectado = potencial / capital inventario, no capital congelado clasificado
            decimal? roiProyectado = month.InventoryCapital > 0
                ? Math.Round(month.PotentialProfit / month.InventoryCapital * 100m, 2)
                : null;
            lblROIproyectadoValue.Text = CrmProfitUiBinder.Pct(roiProyectado);
            lblROIproyectadoDesc.Text = "Potencial / capital inventario";

            lblROIporproductoValue.Text = topRoi != null
                ? CrmProfitUiBinder.Pct(topRoi.RoiPct)
                : "N/D";
            lblROIporproductoDesc.Text = topRoi?.GroupName ?? "Sin producto con ROI";

            // ROI por inversión (FASE 6): ganancia / capital invertido — distinto del ROI/COGS
            var invRank = CrmInvestmentUiBinder.TryRanking(InvestmentRankKind.ByRoiRealized, out _, top: 1);
            InvestmentRankRow? topInv = invRank?.FirstOrDefault();
            if (topInv != null)
            {
                lblROIporinversionValue.Text = CrmInvestmentUiBinder.Pct(topInv.Summary.RoiRealizedPct);
                lblROIporinversionDesc.Text = $"Inversión: {topInv.Summary.Name} (≠ ROI/COGS)";
            }
            else
            {
                lblROIporinversionValue.Text = "N/D";
                lblROIporinversionDesc.Text = "Sin inversiones con capital (mig. 0009)";
            }

            lblGraficoValue.Text = "ROI ventas vs inversión";
            lblGraficoDesc.Text =
                $"Ventas {CrmProfitUiBinder.Pct(month.RoiPct)} · " +
                $"Inv. {CrmInvestmentUiBinder.Pct(topInv?.Summary.RoiRealizedPct)}";

            CrmDomainHintUiBinder.Apply(
                lblCrmHint,
                null,
                DecisionEventArea.Roi,
                DecisionEventArea.Investment,
                DecisionEventArea.Profit);
        }
    }
}
