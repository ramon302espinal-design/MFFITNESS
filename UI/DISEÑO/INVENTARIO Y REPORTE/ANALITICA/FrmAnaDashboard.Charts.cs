using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BLL.Models.Crm;
using BLL.Services.Crm;
using UI.Helpers;

namespace UI
{
    public partial class FrmAnaDashboard
    {
        private IReadOnlyList<CrmDashboardChartPaint.Segment> _capitalSegments = Array.Empty<CrmDashboardChartPaint.Segment>();
        private IReadOnlyList<decimal> _trendValues = Array.Empty<decimal>();
        private string _trendTitle = "Tendencia";
        private string _trendSubtitle = string.Empty;

        private void btnDecisionVer_Click(object? sender, EventArgs e) => NavegarADecisiones();

        private void btnAccionesVer_Click(object? sender, EventArgs e) => NavegarADecisiones();

        private void btnVerDetalleCapital_Click(object? sender, EventArgs e) => NavegarACapitalCongelado();

        private void cmbTrendMetric_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (DesignMode)
                return;
            ActualizarGraficoTendencias();
        }

        private void cmbTrendPeriod_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (DesignMode)
                return;
            ActualizarGraficoTendencias();
        }

        private void lstTop_DoubleClick(object? sender, EventArgs e) => NavegarAProductosEstrella();

        private void lstWatch_DoubleClick(object? sender, EventArgs e) => NavegarAAlertas();

        private void pnlTrends_DoubleClick(object? sender, EventArgs e) => NavegarATendencias();

        private void pnlChartCapital_Paint(object? sender, PaintEventArgs e)
        {
            CrmDashboardChartPaint.PaintStackedBar(
                e,
                pnlChartCapital.ClientRectangle,
                _capitalSegments,
                "Sin datos de distribución de capital");
        }

        private void pnlChartTrends_Paint(object? sender, PaintEventArgs e)
        {
            CrmDashboardChartPaint.PaintSparkline(
                e,
                pnlChartTrends.ClientRectangle,
                _trendValues,
                _trendTitle,
                Color.FromArgb(49, 130, 206),
                _trendSubtitle,
                "Sin ventas en el período seleccionado");
        }

        private void ActualizarGraficoCapital(InventoryCapitalHealthReport? health, ProductPerformanceDashboardReport? dash)
        {
            if (health != null)
            {
                _capitalSegments = new[]
                {
                    Seg("Saludable", health.HealthyCapital, Color.FromArgb(56, 161, 105)),
                    Seg("Lento", health.SlowCapital, Color.FromArgb(214, 158, 46)),
                    Seg("Nuevo", health.NewCapital, Color.FromArgb(49, 130, 206)),
                    Seg("Congelado", health.FrozenStatusCapital, Color.FromArgb(237, 137, 54)),
                    Seg("Crítico", health.CriticalCapital, Color.FromArgb(229, 62, 62))
                };
            }
            else if (dash != null)
            {
                _capitalSegments = new[]
                {
                    Seg("Estrella", dash.StarCapital, Color.FromArgb(56, 161, 105)),
                    Seg("Oportunidad", dash.OpportunityCapital, Color.FromArgb(49, 130, 206)),
                    Seg("Lento", dash.SlowCapital, Color.FromArgb(214, 158, 46)),
                    Seg("Crítico", dash.CriticalClassCapital, Color.FromArgb(229, 62, 62))
                };
            }
            else
            {
                _capitalSegments = Array.Empty<CrmDashboardChartPaint.Segment>();
            }

            pnlChartCapital.Invalidate();
        }

        private void ActualizarGraficoTendencias()
        {
            ResolveTrendPeriod(out DateTime? from, out DateTime? toExclusive, out string periodLabel);
            IReadOnlyList<ProfitDayRow> days = new ProfitAnalyticsService().GetByDay(from, toExclusive);
            IReadOnlyList<ProfitDayRow> series = AggregateTrendPoints(days, cmbTrendPeriod.SelectedIndex);

            string metric = cmbTrendMetric.SelectedItem?.ToString() ?? "Ventas";
            _trendValues = ExtractMetricSeries(series, metric);
            _trendTitle = $"{metric} · {periodLabel}";
            _trendSubtitle = series.Count > 0
                ? $"{series.Count} pts · Σ {CrmSalesUiBinder.Money(_trendValues.Sum())}"
                : "Sin operaciones en el rango";

            pnlChartTrends.Invalidate();
        }

        private static CrmDashboardChartPaint.Segment Seg(string label, decimal value, Color color)
            => new() { Label = label, Value = value, Color = color };

        private void ResolveTrendPeriod(
            out DateTime? from,
            out DateTime? toExclusive,
            out string periodLabel)
        {
            DateTime d = DateTime.Today;
            toExclusive = d.AddDays(1);

            switch (cmbTrendPeriod.SelectedIndex)
            {
                case 1:
                    from = new DateTime(d.Year, d.Month, 1).AddMonths(-5);
                    periodLabel = "Últimos 6 meses";
                    break;
                case 2:
                    from = new DateTime(d.Year, 1, 1);
                    periodLabel = "Este año";
                    break;
                default:
                    from = new DateTime(d.Year, d.Month, 1).AddMonths(-11);
                    periodLabel = "Últimos 12 meses";
                    break;
            }
        }

        private static IReadOnlyList<ProfitDayRow> AggregateTrendPoints(
            IReadOnlyList<ProfitDayRow> days,
            int periodIndex)
        {
            if (days == null || days.Count == 0)
                return Array.Empty<ProfitDayRow>();

            if (periodIndex == 0 || periodIndex == 1)
            {
                return days
                    .GroupBy(d => new DateTime(d.Date.Year, d.Date.Month, 1))
                    .OrderBy(g => g.Key)
                    .Select(g => new ProfitDayRow
                    {
                        Date = g.Key,
                        TransactionCount = g.Sum(x => x.TransactionCount),
                        UnitsSold = g.Sum(x => x.UnitsSold),
                        RevenueTotal = g.Sum(x => x.RevenueTotal),
                        RevenueWithCost = g.Sum(x => x.RevenueWithCost),
                        Cogs = g.Sum(x => x.Cogs),
                        RealizedProfit = g.Sum(x => x.RealizedProfit),
                        HasReliableRealizedProfit = g.Any(x => x.HasReliableRealizedProfit),
                        MarginPct = g.Sum(x => x.RevenueWithCost) > 0
                            ? g.Sum(x => x.RealizedProfit) / g.Sum(x => x.RevenueWithCost) * 100m
                            : null,
                        RoiPct = g.Sum(x => x.Cogs) > 0
                            ? g.Sum(x => x.RealizedProfit) / g.Sum(x => x.Cogs) * 100m
                            : null
                    })
                    .ToList();
            }

            return days.OrderBy(d => d.Date).ToList();
        }

        private static IReadOnlyList<decimal> ExtractMetricSeries(IReadOnlyList<ProfitDayRow> series, string metric)
        {
            if (series == null || series.Count == 0)
                return Array.Empty<decimal>();

            return metric switch
            {
                "Ganancia" => series.Select(d => d.RealizedProfit).ToList(),
                "ROI" => series.Select(d => d.RoiPct ?? 0m).ToList(),
                "Margen" => series.Select(d => d.MarginPct ?? 0m).ToList(),
                "Capital" => series.Select(d => d.Cogs).ToList(),
                _ => series.Select(d => d.RevenueTotal).ToList()
            };
        }

        private FrmCRMFinanciero? ResolverShellCrm()
        {
            Control? c = Parent;
            while (c != null && c is not FrmCRMFinanciero)
                c = c.Parent;
            return c as FrmCRMFinanciero;
        }

        private void NavegarADecisiones() => ResolverShellCrm()?.MostrarDecisiones();

        private void NavegarACapitalCongelado() => ResolverShellCrm()?.MostrarCapitalCongelado();

        private void NavegarAAlertas() => ResolverShellCrm()?.MostrarAlertas();

        private void NavegarAProductosEstrella() => ResolverShellCrm()?.MostrarProductosEstrella();

        private void NavegarATendencias() => ResolverShellCrm()?.MostrarTendencias();
    }
}
