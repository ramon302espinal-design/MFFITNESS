using System;
using System.Linq;
using System.Windows.Forms;
using BLL.Models.Crm;
using UI.Helpers;
using UI.Theme;

namespace UI
{
    /// <summary>
    /// Ganancias — FASE 5.11: ProfitAnalyticsService (mes actual + potencial).
    /// </summary>
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmAnaGanancias : Form
    {
        public FrmAnaGanancias()
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
                    "No se pudieron cargar ganancias.\n" + (error ?? ""),
                    "CRM Financiero",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var products = CrmProfitUiBinder.TryLoadByProduct(ProfitPeriodKind.ThisMonth, out _, top: 1);
            var categories = CrmProfitUiBinder.TryLoadByCategory(ProfitPeriodKind.ThisMonth, out _, top: 1);

            lblGananciarealizadaValue.Text = CrmProfitUiBinder.Money(month.RealizedProfit);
            lblGananciarealizadaDesc.Text = month.HasReliableRealizedProfit
                ? $"Mes · cobertura {CrmProfitUiBinder.Pct(month.CostCoveragePct)}"
                : "Mes · sin COGS snapshot confiable";

            lblGananciapotencialValue.Text = CrmProfitUiBinder.Money(month.PotentialProfit);
            lblGananciapotencialDesc.Text = "Inventario actual (no es realizada)";

            ProfitGroupRow? topProd = products?.FirstOrDefault();
            lblGananciaporproductoValue.Text = topProd != null
                ? CrmProfitUiBinder.Money(topProd.RealizedProfit)
                : "—";
            lblGananciaporproductoDesc.Text = topProd != null
                ? topProd.GroupName
                : "Sin ventas en el mes";

            lblGananciaporinversionValue.Text = CrmProfitUiBinder.Pct(month.RoiPct);
            lblGananciaporinversionDesc.Text = "ROI mes ventas = ganancia / COGS";

            var invByProfit = CrmInvestmentUiBinder.TryRanking(InvestmentRankKind.ByRealizedProfit, out _, top: 1);
            InvestmentRankRow? topInv = invByProfit?.FirstOrDefault();
            if (topInv != null)
            {
                lblGananciaporinversionValue.Text = CrmInvestmentUiBinder.Money(topInv.Summary.RealizedProfit);
                lblGananciaporinversionDesc.Text =
                    $"Inv. {topInv.Summary.Name} · ROI {CrmInvestmentUiBinder.Pct(topInv.Summary.RoiRealizedPct)}";
            }

            ProfitGroupRow? topCat = categories?.FirstOrDefault();
            lblGananciaporcategoriaValue.Text = topCat != null
                ? CrmProfitUiBinder.Money(topCat.RealizedProfit)
                : "—";
            lblGananciaporcategoriaDesc.Text = topCat?.GroupName ?? "Sin datos";

            lblGraficoValue.Text = "Realizada vs potencial";
            lblGraficoDesc.Text =
                $"Realizada {CrmProfitUiBinder.Money(month.RealizedProfit)} · " +
                $"Potencial {CrmProfitUiBinder.Money(month.PotentialProfit)} · " +
                $"Margen {CrmProfitUiBinder.Pct(month.MarginPct)}";
        }
    }
}
