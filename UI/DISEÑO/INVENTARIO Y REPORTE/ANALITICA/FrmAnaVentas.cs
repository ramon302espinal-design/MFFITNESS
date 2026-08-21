using System.Linq;
using System.Windows.Forms;
using BLL.Models.Crm;
using UI.Helpers;
using UI.Theme;

namespace UI
{
    /// <summary>
    /// Ventas — FASE 9.24: KPIs, variación, ticket, tops, tendencia, forecast (estimación).
    /// Sin lógica financiera en el Form (solo binders).
    /// </summary>
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmAnaVentas : Form
    {
        public FrmAnaVentas()
        {
            InitializeComponent();
            CrmVisualTokens.MarkClassic(this);
            Load += (_, _) => CargarDatos();
        }

        private void CargarDatos()
        {
            SalesDashboardReport? dash = CrmSalesUiBinder.TryLoadDashboard(
                out string? error, ProfitPeriodKind.ThisMonth, topLists: 5);

            if (dash == null)
            {
                MessageBox.Show(
                    "No se pudieron cargar ventas.\n" + (error ?? ""),
                    "CRM Financiero",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Ingresos (≠ tickets)
            lblVentasTitle.Text = "Ingresos";
            lblVentasValue.Text = CrmSalesUiBinder.Money(dash.RevenueTotal);
            lblVentasDesc.Text =
                $"Var. {CrmSalesUiBinder.VariationDisplay(dash.RevenueVariation)} · " +
                $"{CrmSalesUiBinder.Count(dash.TransactionCount)} tickets";

            lblUnidadesTitle.Text = "Unidades";
            lblUnidadesValue.Text = CrmSalesUiBinder.Count(dash.UnitsSold);
            lblUnidadesDesc.Text = "Unidades ≠ tickets ≠ ingresos";

            lblGananciaTitle.Text = "Ganancia";
            lblGananciaValue.Text = CrmSalesUiBinder.Money(dash.RealizedProfit);
            lblGananciaDesc.Text =
                $"Var. {CrmSalesUiBinder.VariationDisplay(dash.ProfitVariation)}";

            lblMargenTitle.Text = "Margen";
            lblMargenValue.Text = CrmSalesUiBinder.Pct(dash.MarginPct);
            lblMargenDesc.Text = "Ganancia / ingresos (si COGS confiable)";

            lblPeriodoTitle.Text = "Período";
            lblPeriodoValue.Text = CrmSalesUiBinder.PeriodLabel(dash.PeriodKind);
            lblPeriodoDesc.Text =
                $"Ticket {CrmSalesUiBinder.Money(dash.AverageTicket ?? 0m)} · " +
                $"Var. ticket {CrmSalesUiBinder.VariationDisplay(dash.TicketVariation)}";

            SalesDashboardTopItem? topProd = dash.TopProducts.FirstOrDefault();
            lblProductosTitle.Text = "Top producto";
            lblProductosValue.Text = topProd != null
                ? CrmSalesUiBinder.Money(topProd.Amount)
                : "—";
            lblProductosDesc.Text = topProd != null
                ? $"{topProd.Name} · {CrmSalesUiBinder.Pct(topProd.SharePct)} ingresos"
                : "Sin productos en el período";

            SalesDashboardTopItem? topCat = dash.TopCategories.FirstOrDefault();
            lblCajerosTitle.Text = "Top categoría";
            lblCajerosValue.Text = topCat != null
                ? CrmSalesUiBinder.Money(topCat.Amount)
                : "—";
            lblCajerosDesc.Text = topCat != null
                ? $"{topCat.Name} · {CrmSalesUiBinder.Pct(topCat.SharePct)}"
                : "Sin categorías";

            lblHorarioTitle.Text = "Forecast 30d";
            lblHorarioValue.Text = dash.ForecastBaseRevenue.HasValue
                ? CrmSalesUiBinder.Money(dash.ForecastBaseRevenue.Value)
                : "N/D";
            lblHorarioDesc.Text =
                $"ESTIMACIÓN · confianza {CrmSalesUiBinder.ConfidenceLabel(dash.ForecastConfidence)} · " +
                $"estrellas {CrmSalesUiBinder.Count(dash.StarCount)} ({CrmSalesUiBinder.Pct(dash.StarRevenueSharePct)})";

            lblGraficoTitle.Text = "Tendencia";
            lblGraficoValue.Text = CrmSalesUiBinder.TrendLabel(dash.RevenueTrend);
            string alertPreview = dash.Alerts.Count > 0
                ? dash.Alerts[0].Message
                : "Sin alertas de ventas";
            lblGraficoDesc.Text =
                $"Acel. {CrmSalesUiBinder.AccelerationLabel(dash.RevenueAcceleration)} · " +
                $"Quiebre {CrmSalesUiBinder.Count(dash.StockoutRiskCount)} · {alertPreview}";

            // FASE 11.20 — hint Centro / acciones (sin lógica en Form)
            CrmDomainHintUiBinder.Apply(
                lblCrmHint,
                CrmDomainHintUiBinder.TryBuildSnapshotFromSales(dash),
                DecisionEventArea.Sales,
                DecisionEventArea.Profit,
                DecisionEventArea.Trend);
        }
    }
}
