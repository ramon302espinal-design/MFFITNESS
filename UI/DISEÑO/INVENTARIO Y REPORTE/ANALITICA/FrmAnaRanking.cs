using System.Linq;
using System.Windows.Forms;
using BLL.Models.Crm;
using UI.Helpers;
using UI.Theme;

namespace UI
{
    /// <summary>
    /// Ranking CRM — FASE 8.20: una métrica por panel (sin ranking universal / sin score).
    /// </summary>
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmAnaRanking : Form
    {
        public FrmAnaRanking()
        {
            InitializeComponent();
            CrmVisualTokens.MarkClassic(this);
            Load += (_, _) => CargarDatos();
        }

        private void CargarDatos()
        {
            var units = RankTop(ProductPerformanceMetricKind.UnitsSold);
            var revenue = RankTop(ProductPerformanceMetricKind.Revenue);
            var profit = RankTop(ProductPerformanceMetricKind.RealizedProfit);
            var margin = RankTop(ProductPerformanceMetricKind.MarginPct);
            var roi = RankTop(ProductPerformanceMetricKind.RoiPct);
            var turnover = RankTop(ProductPerformanceMetricKind.TurnoverProxy);
            var capital = RankTop(ProductPerformanceMetricKind.InventoryCapital);
            var immobilized = RankTop(ProductPerformanceMetricKind.ImmobilizedCapital);

            if (units == null && profit == null && immobilized == null && capital == null)
            {
                MessageBox.Show(
                    "No se pudo cargar ranking de performance.",
                    "CRM Financiero",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            lblFiltrosValue.Text = "Mes · 1 métrica";
            lblFiltrosDesc.Text = immobilized != null
                ? $"Congelado: {immobilized.Row.ProductName} · {CrmProductPerformanceUiBinder.RankMetric(immobilized)}"
                : capital != null
                    ? $"Mayor capital: {capital.Row.ProductName}"
                    : "Criterios separados (no hay “mejor” único)";

            lblRankingValue.Text = CrmProductPerformanceUiBinder.RankHeadline(units);
            lblRankingDesc.Text = units != null
                ? $"TOP UNIDADES · {CrmProductPerformanceUiBinder.RankMetric(units)}"
                : "Sin ventas en período";

            lblScoreValue.Text = CrmProductPerformanceUiBinder.RankHeadline(profit);
            lblScoreDesc.Text = profit != null
                ? $"TOP GANANCIA · {CrmProductPerformanceUiBinder.RankMetric(profit)}"
                : "Sin ganancia confiable";

            lblVentasValue.Text = revenue != null
                ? CrmProductPerformanceUiBinder.Money(revenue.MetricValue ?? 0m)
                : "—";
            lblVentasDesc.Text = revenue != null
                ? $"TOP INGRESOS: {revenue.Row.ProductName}"
                : "Sin ingresos";

            lblGananciaValue.Text = margin != null
                ? CrmProductPerformanceUiBinder.Pct(margin.MetricValue)
                : "—";
            lblGananciaDesc.Text = margin != null
                ? $"TOP MARGEN: {margin.Row.ProductName}"
                : "Sin margen confiable";

            lblROIValue.Text = roi != null
                ? CrmProductPerformanceUiBinder.Pct(roi.MetricValue)
                : "—";
            lblROIDesc.Text = roi != null
                ? $"TOP ROI producto: {roi.Row.ProductName} (≠ inv.)"
                : "Sin ROI confiable";

            lblRotacionValue.Text = CrmProductPerformanceUiBinder.RankHeadline(turnover);
            lblRotacionDesc.Text = turnover != null
                ? $"TOP ROTACIÓN PROXY · {CrmProductPerformanceUiBinder.RankMetric(turnover)}"
                : "Sin turnover proxy";
        }

        private static ProductPerformanceRankRow? RankTop(ProductPerformanceMetricKind kind)
            => CrmProductPerformanceUiBinder.TryLoadRanking(kind, out _, top: 1)?.FirstOrDefault();
    }
}
