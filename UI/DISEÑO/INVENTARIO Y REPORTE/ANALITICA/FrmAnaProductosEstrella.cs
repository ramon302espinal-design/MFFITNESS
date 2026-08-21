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
    /// Productos estrella — FASE 8.20: checklist impacto + eficiencia + bajo riesgo (sin score).
    /// </summary>
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmAnaProductosEstrella : Form
    {
        public FrmAnaProductosEstrella()
        {
            InitializeComponent();
            CrmVisualTokens.MarkClassic(this);
            Load += (_, _) => CargarDatos();
        }

        private void CargarDatos()
        {
            ProductPerformanceDashboardReport? dash =
                CrmProductPerformanceUiBinder.TryLoadDashboard(out string? error, topLists: 8);
            IReadOnlyList<ProductClassificationRow>? stars =
                CrmProductPerformanceUiBinder.TryLoadStars(out _, top: 10);
            IReadOnlyList<ProductClassificationRow>? opps =
                CrmProductPerformanceUiBinder.TryLoadOpportunities(out _, top: 3);
            SalesStarMixReport? mix = CrmSalesUiBinder.TryLoadStarMix(out _);

            if (dash == null && stars == null)
            {
                MessageBox.Show(
                    "No se pudieron cargar productos estrella.\n" + (error ?? ""),
                    "CRM Financiero",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            int starCount = dash?.StarCount ?? stars?.Count ?? mix?.StarCount ?? 0;
            lblProductosestrellaValue.Text = CrmProductPerformanceUiBinder.Count(starCount);
            lblProductosestrellaDesc.Text = mix?.StarRevenueSharePct.HasValue == true
                ? $"Mix ingresos estrellas {CrmSalesUiBinder.Pct(mix.StarRevenueSharePct)} · " +
                  $"Opp {CrmProductPerformanceUiBinder.Count(dash?.OpportunityCount ?? opps?.Count ?? 0)}"
                : $"Checklist 3 pilares · Opp {CrmProductPerformanceUiBinder.Count(dash?.OpportunityCount ?? opps?.Count ?? 0)}";

            ProductClassificationRow? topStar = stars?.FirstOrDefault();
            var topUnits = dash?.TopUnits.FirstOrDefault();
            lblCrecimientoValue.Text = topStar?.ProductName
                ?? CrmProductPerformanceUiBinder.RankHeadline(topUnits);
            lblCrecimientoDesc.Text = topStar != null
                ? "Top estrella (≠ solo más vendido)"
                : topUnits != null
                    ? $"Sin estrellas · top uds: {CrmProductPerformanceUiBinder.RankMetric(topUnits)}"
                    : "Sin estrellas ni ventas";

            decimal starProfit = topStar?.Performance?.RealizedProfit ?? 0m;
            lblGananciaValue.Text = topStar != null
                ? CrmProductPerformanceUiBinder.Money(starProfit)
                : CrmProductPerformanceUiBinder.Money(dash?.TopProfit.FirstOrDefault()?.MetricValue ?? 0m);
            lblGananciaDesc.Text = topStar != null
                ? "Ganancia realizada de la estrella"
                : "Top ganancia (sin estrella aún)";

            lblROIValue.Text = topStar != null
                ? CrmProductPerformanceUiBinder.Pct(topStar.Performance?.RoiPct)
                : CrmProductPerformanceUiBinder.Pct(dash?.TopRoi.FirstOrDefault()?.MetricValue);
            lblROIDesc.Text = "ROI producto (≠ ROI inversión)";

            lblRotacionValue.Text = topStar != null
                ? (topStar.Performance?.TurnoverProxy?.ToString("N2") ?? "—")
                : CrmProductPerformanceUiBinder.RankHeadline(dash?.TopTurnover.FirstOrDefault());
            lblRotacionDesc.Text = topStar != null
                ? $"Turnover proxy · {topStar.Performance?.UnitsPerDay:N2} ud/día"
                : "Top rotación proxy";

            lblScoreValue.Text = CrmProductPerformanceUiBinder.Count(dash?.PortfolioHealthScore ?? 0);
            lblScoreDesc.Text = "Salud portafolio 0–100 (explicable · no score de producto)";

            var sb = new StringBuilder();
            if (stars != null && stars.Count > 0)
            {
                foreach (ProductClassificationRow s in stars.Take(6))
                    sb.AppendLine("★ " + CrmProductPerformanceUiBinder.ExplainStar(s));
            }
            else
            {
                sb.AppendLine("Sin productos que cumplan los 3 pilares este mes.");
                if (opps != null)
                {
                    foreach (ProductClassificationRow o in opps.Take(3))
                        sb.AppendLine("· Opp: " + o.ProductName);
                }
            }

            lblTablaValue.Text = CrmProductPerformanceUiBinder.Count(stars?.Count ?? 0);
            lblTablaDesc.Text = sb.ToString().TrimEnd();
        }
    }
}
