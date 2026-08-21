using System.Linq;
using System.Windows.Forms;
using BLL.Models.Crm;
using UI.Helpers;
using UI.Theme;

namespace UI
{
    /// <summary>
    /// Tendencias — FASE 8 MoM producto + FASE 9.24 serie multi-punto / aceleración / forecast.
    /// </summary>
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmAnaTendencias : Form
    {
        public FrmAnaTendencias()
        {
            InitializeComponent();
            CrmVisualTokens.MarkClassic(this);
            Load += (_, _) => CargarDatos();
        }

        private void CargarDatos()
        {
            ProductTrendReport? trends = CrmProductPerformanceUiBinder.TryLoadTrends(out string? error);
            SalesSeriesTrendReport? series = CrmSalesUiBinder.TryLoadSeriesTrend(out _);
            SalesAccelerationReport? accel = CrmSalesUiBinder.TryLoadAcceleration(out _);
            SalesForecastReport? forecast = CrmSalesUiBinder.TryLoadForecast(out _);
            SalesDashboardReport? salesDash = CrmSalesUiBinder.TryLoadDashboard(out _, ProfitPeriodKind.Last30Days);
            ProfitSummary? month = CrmProfitUiBinder.TryLoadThisMonth(out _);
            InventoryCapitalHealthReport? health = CrmInventoryUiBinder.TryLoadHealth(out string? invError);
            ProductPerformanceDashboardReport? dash =
                CrmProductPerformanceUiBinder.TryLoadDashboard(out _, topLists: 5);

            if (trends == null && series == null && month == null && health == null)
            {
                MessageBox.Show(
                    "No se pudieron cargar tendencias.\n" + (error ?? invError ?? ""),
                    "CRM Financiero",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Panel Ventas = tendencia serie ingresos (FASE 9.14)
            lblVentasTitle.Text = "Tendencia ingresos";
            if (series != null)
            {
                lblVentasValue.Text = CrmSalesUiBinder.TrendLabel(series.Revenue.Kind);
                lblVentasDesc.Text =
                    $"{series.Revenue.Reason} · {CrmSalesUiBinder.Count(series.Revenue.PointCount)} pts";
            }
            else
            {
                lblVentasValue.Text = "N/D";
                lblVentasDesc.Text = "Sin serie multi-punto";
            }

            // Panel Ganancias = aceleración (FASE 9.15)
            lblGananciasTitle.Text = "Aceleración";
            if (accel != null)
            {
                lblGananciasValue.Text = CrmSalesUiBinder.AccelerationLabel(accel.Revenue.Kind);
                lblGananciasDesc.Text = accel.Revenue.Reason;
            }
            else
            {
                lblGananciasValue.Text = "N/D";
                lblGananciasDesc.Text = "Sin aceleración";
            }

            // Panel ROI = forecast estimación
            lblROITitle.Text = "Forecast (est.)";
            if (forecast?.HasEstimate == true)
            {
                lblROIValue.Text = CrmSalesUiBinder.Money(forecast.Base.EstimatedRevenue);
                lblROIDesc.Text =
                    $"Bajo {CrmSalesUiBinder.Money(forecast.Low.EstimatedRevenue)} · " +
                    $"Alto {CrmSalesUiBinder.Money(forecast.High.EstimatedRevenue)} · " +
                    CrmSalesUiBinder.ConfidenceLabel(forecast.Confidence);
            }
            else
            {
                lblROIValue.Text = "N/D";
                lblROIDesc.Text = "Estimación no disponible";
            }

            ProductTrendRow? topGrow = trends?.Rows
                .Where(r => r.PrimaryTrend == ProductTrendDirection.Growing)
                .OrderByDescending(r => r.UnitsChangePct ?? 0m)
                .FirstOrDefault();
            ProductTrendRow? topDrop = trends?.Rows
                .Where(r => r.PrimaryTrend == ProductTrendDirection.Declining)
                .OrderBy(r => r.UnitsChangePct ?? 0m)
                .FirstOrDefault();

            lblProductosTitle.Text = "Productos MoM";
            if (trends != null)
            {
                lblProductosValue.Text =
                    $"{CrmProductPerformanceUiBinder.Count(trends.GrowingCount)}↑ / " +
                    $"{CrmProductPerformanceUiBinder.Count(trends.DecliningCount)}↓";
                lblProductosDesc.Text = topGrow != null
                    ? $"Top ↑ {topGrow.ProductName} ({CrmProductPerformanceUiBinder.Pct(topGrow.UnitsChangePct)}) · primaria=uds"
                    : $"Estables {CrmProductPerformanceUiBinder.Count(trends.StableCount)} · Insuf. {CrmProductPerformanceUiBinder.Count(trends.InsufficientCount)}";
            }
            else
            {
                lblProductosValue.Text = "—";
                lblProductosDesc.Text = "Sin MoM producto";
            }

            decimal invCapital = health?.InventoryCapitalTotal ?? month?.InventoryCapital ?? 0m;
            decimal immobilized = dash?.TotalImmobilizedCapital
                ?? health?.ImmobilizedCapital
                ?? 0m;

            lblInventarioValue.Text = CrmInventoryUiBinder.Money(invCapital);
            lblInventarioDesc.Text = dash != null
                ? $"Estrella {CrmInventoryUiBinder.Money(dash.StarCapital)} · " +
                  $"Opp {CrmInventoryUiBinder.Money(dash.OpportunityCapital)}"
                : "Capital inventario (snapshot)";

            lblCapitalValue.Text = CrmInventoryUiBinder.Money(immobilized);
            lblCapitalDesc.Text = salesDash != null
                ? $"Capital riesgo ventas {CrmSalesUiBinder.Count(salesDash.CapitalRiskCount)} · " +
                  $"Quiebre {CrmSalesUiBinder.Count(salesDash.StockoutRiskCount)}"
                : "Inmovilizado Frozen∪Critical";

            lblPeriodoTitle.Text = "Período";
            lblPeriodoValue.Text = "30d + MoM";
            lblPeriodoDesc.Text = salesDash?.RevenueVariation != null
                ? $"Ingresos {CrmSalesUiBinder.VariationDisplay(salesDash.RevenueVariation)} · " +
                  $"Ganancia {CrmSalesUiBinder.VariationDisplay(salesDash.ProfitVariation)}"
                : "Serie FASE 9 + MoM FASE 8";

            if (topDrop != null)
            {
                lblGraficoValue.Text = topDrop.ProductName;
                lblGraficoDesc.Text =
                    $"Mayor caída uds {CrmProductPerformanceUiBinder.Pct(topDrop.UnitsChangePct)} · " +
                    $"≠ aceleración serie ({CrmSalesUiBinder.AccelerationLabel(accel?.Revenue.Kind ?? SalesAccelerationKind.InsufficientData)})";
            }
            else if (dash != null)
            {
                lblGraficoValue.Text = CrmProductPerformanceUiBinder.Count(dash.CriticalCount);
                lblGraficoDesc.Text =
                    $"Críticos FASE 8 · Estrellas {CrmProductPerformanceUiBinder.Count(dash.StarCount)}";
            }
            else
            {
                lblGraficoValue.Text = "—";
                lblGraficoDesc.Text = "Sin señal de caída MoM";
            }
        }
    }
}
