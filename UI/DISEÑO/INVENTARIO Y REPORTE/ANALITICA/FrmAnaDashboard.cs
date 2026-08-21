using System;
using System.Linq;
using System.Windows.Forms;
using BLL.Models.Crm;
using UI.Helpers;
using UI.Theme;

namespace UI
{
    /// <summary>
    /// Dashboard CRM — FASE 7–10 + FASE 11.17 contadores de acciones.
    /// Sin lógica financiera: solo binders BLL.
    /// </summary>
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmAnaDashboard : Form
    {
        public FrmAnaDashboard()
        {
            InitializeComponent();
            CrmVisualTokens.MarkClassic(this);
            Load += (_, _) => CargarDatos();
            btnDecisionVer.Enabled = true;
            btnDecisionVer.Click += (_, _) => NavegarADecisiones();
            btnAccionesVer.Click += (_, _) => NavegarADecisiones();
        }

        private void NavegarADecisiones()
        {
            Control? c = Parent;
            while (c != null && c is not FrmCRMFinanciero)
                c = c.Parent;

            if (c is FrmCRMFinanciero crm)
                crm.MostrarDecisiones();
        }

        private void CargarDatos()
        {
            ProfitSummary? profit = CrmProfitUiBinder.TryLoadThisMonth(out string? error);
            InventoryFinancialSummary? summary = CrmInventoryUiBinder.TryLoadSummary(out string? invError);
            InventoryCapitalHealthReport? health = CrmInventoryUiBinder.TryLoadHealth(out _);
            ProductPerformanceDashboardReport? dash =
                CrmProductPerformanceUiBinder.TryLoadDashboard(out _, topLists: 5);
            SalesDashboardReport? salesDash =
                CrmSalesUiBinder.TryLoadDashboard(out _, ProfitPeriodKind.ThisMonth, topLists: 5);

            if (profit == null && summary == null && health == null && dash == null)
            {
                MessageBox.Show(
                    "No se pudieron cargar los datos financieros.\n" + (error ?? invError ?? ""),
                    "CRM Financiero",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            FrozenCapitalReport? frozen = health?.Frozen
                ?? CrmInventoryUiBinder.TryLoadFrozen(out _);
            PotentialValueReport? potential = CrmInventoryUiBinder.TryLoadPotential(out _);
            InventoryRiskReport? risk = CrmInventoryUiBinder.TryLoadRisk(out _);
            InventoryAlertReport? alerts = CrmInventoryUiBinder.TryLoadAlerts(out _, top: 8);

            decimal inventoryCapital = health?.InventoryCapitalTotal
                ?? summary?.InventoryCapitalTotal
                ?? profit?.InventoryCapital
                ?? 0m;
            decimal frozenCapital = health?.ImmobilizedCapital
                ?? frozen?.TotalFrozenCapital
                ?? profit?.FrozenCapital
                ?? summary?.FrozenCapitalTotal
                ?? 0m;
            decimal? frozenShare = health?.ImmobilizedSharePct
                ?? frozen?.FrozenSharePct
                ?? summary?.FrozenSharePct;

            decimal inventarioValor = potential?.TotalPotentialSalesValue
                ?? profit?.PotentialSalesValue
                ?? summary?.PotentialSalesValueTotal
                ?? 0m;
            decimal ventas = salesDash?.RevenueTotal
                ?? profit?.RevenueTotal
                ?? summary?.RevenueTotal
                ?? 0m;
            decimal ganancia = salesDash?.RealizedProfit
                ?? profit?.RealizedProfit
                ?? summary?.RealizedProfitTotal
                ?? 0m;
            decimal? roiGlobal = profit?.RoiPct;
            if (!roiGlobal.HasValue && summary != null && summary.CogsTotal > 0)
                roiGlobal = Math.Round(summary.RealizedProfitTotal / summary.CogsTotal * 100m, 2);

            decimal? margen = salesDash?.MarginPct ?? profit?.MarginPct;

            // KPI principal: capital EN INVENTARIO (no el congelado)
            lblKpiCapVal.Text = CrmInventoryUiBinder.Money(inventoryCapital);
            lblKpiCapDelta.Text = frozenShare.HasValue
                ? $"Congelado {CrmInventoryUiBinder.Pct(frozenShare)} · {CrmInventoryUiBinder.Count(frozen?.ProductsWithFrozenCapital ?? 0)} prod."
                : "Capital a costo";

            lblKpiInvVal.Text = CrmInventoryUiBinder.Money(inventarioValor);
            lblKpiInvDelta.Text = inventoryCapital > 0 && inventarioValor > 0
                ? CrmInventoryUiBinder.Pct(inventoryCapital / inventarioValor * 100m) + " a costo"
                : "Valor a PVP";

            lblKpiVenVal.Text = CrmInventoryUiBinder.Money(ventas);
            lblKpiVenDelta.Text = salesDash?.RevenueVariation != null
                ? $"Mes · var. {CrmSalesUiBinder.VariationDisplay(salesDash.RevenueVariation)} · " +
                  $"{CrmSalesUiBinder.TrendLabel(salesDash.RevenueTrend)}"
                : profit != null
                    ? $"Mes · {CrmInventoryUiBinder.Count(profit.TransactionCount)} ventas"
                    : "Ingresos";

            lblKpiGanVal.Text = CrmInventoryUiBinder.Money(ganancia);
            lblKpiGanDelta.Text = salesDash?.ProfitVariation != null
                ? $"Var. {CrmSalesUiBinder.VariationDisplay(salesDash.ProfitVariation)}"
                : profit?.HasReliableRealizedProfit == true
                    ? "Realizada (mes)"
                    : "Sin COGS confiable";

            lblKpiRoiVal.Text = CrmProfitUiBinder.Pct(roiGlobal);
            lblKpiRoiDelta.Text = "Ganancia / COGS";

            if (dash != null)
            {
                lblEstrellaTitle.Text = "ESTRELLA";
                lblBuenosTitle.Text = "SALUDABLE";
                lblLentosTitle.Text = "LENTOS";
                lblCriticosTitle.Text = "CRÍTICOS";
                lblEstrellaVal.Text = CrmProductPerformanceUiBinder.Count(dash.StarCount);
                lblBuenosVal.Text = CrmProductPerformanceUiBinder.Count(dash.HealthyCount);
                lblLentosVal.Text = CrmProductPerformanceUiBinder.Count(dash.SlowCount);
                lblCriticosVal.Text = CrmProductPerformanceUiBinder.Count(dash.CriticalCount);
                lblHealthScore.Text =
                    $"{dash.PortfolioHealthScore} / 100 · Opp {CrmProductPerformanceUiBinder.Count(dash.OpportunityCount)}";
            }
            else if (summary != null)
            {
                var (nuevos, saludables, lentos, criticos, salud) =
                    CrmInventoryUiBinder.HealthBuckets(summary);
                lblEstrellaTitle.Text = "NUEVOS";
                lblBuenosTitle.Text = "SALUDABLE";
                lblLentosTitle.Text = "LENTOS";
                lblCriticosTitle.Text = "FRÍO/CRÍT.";
                lblEstrellaVal.Text = CrmInventoryUiBinder.Count(nuevos);
                lblBuenosVal.Text = CrmInventoryUiBinder.Count(saludables);
                lblLentosVal.Text = CrmInventoryUiBinder.Count(lentos);
                lblCriticosVal.Text = CrmInventoryUiBinder.Count(criticos);
                lblHealthScore.Text = $"{salud} / 100";
            }

            // Panel capital congelado = clasificado Frozen+Critical
            lblFrozenValor.Text = CrmInventoryUiBinder.Money(frozenCapital);
            int prodFrozen = frozen?.ProductsWithFrozenCapital
                ?? ((summary?.ProductsFrozen ?? 0) + (summary?.ProductsCritical ?? 0));
            lblFrozenProductos.Text = $"{CrmInventoryUiBinder.Count(prodFrozen)} productos";

            decimal avgDays = summary != null
                ? CrmInventoryUiBinder.AvgIdleDays(summary)
                : 0m;
            lblFrozenDias.Text = avgDays > 0
                ? $"{avgDays:0} días prom. idle"
                : "Sin historial de idle";

            if (frozenShare.HasValue)
            {
                lblFrozenPct.Text = $"{CrmInventoryUiBinder.Pct(frozenShare)} del capital inventario";
                progressFrozen.Value = (int)Math.Clamp(Math.Round(frozenShare.Value), 0, 100);
            }
            else if (inventarioValor > 0)
            {
                decimal pct = Math.Round(frozenCapital / inventarioValor * 100m, 2);
                lblFrozenPct.Text = $"{CrmInventoryUiBinder.Pct(pct)} del valor a PVP";
                progressFrozen.Value = (int)Math.Clamp(Math.Round(pct), 0, 100);
            }
            else
            {
                lblFrozenPct.Text = "Sin base de comparación";
                progressFrozen.Value = 0;
            }

            lblProfitGanancia.Text = "Ganancia realizada: " + CrmInventoryUiBinder.Money(ganancia);
            lblProfitMargen.Text = "Margen: " + CrmProfitUiBinder.Pct(margen);
            lblProfitRoi.Text = salesDash?.ForecastBaseRevenue.HasValue == true
                ? "Forecast (est.): " + CrmSalesUiBinder.Money(salesDash.ForecastBaseRevenue.Value) +
                  " · " + CrmSalesUiBinder.ConfidenceLabel(salesDash.ForecastConfidence)
                : "ROI: " + CrmProfitUiBinder.Pct(roiGlobal);
            lblGanRealizada.Text = "REALIZADA " + CrmInventoryUiBinder.Money(ganancia);
            lblGanPotencial.Text = "POTENCIAL " + CrmInventoryUiBinder.Money(
                potential?.TotalPotentialProfit
                ?? profit?.PotentialProfit
                ?? summary?.PotentialProfitTotal
                ?? 0m);

            lblCapInvertido.Text = "En inventario: " + CrmInventoryUiBinder.Money(inventoryCapital);
            if (dash != null)
            {
                lblCapRecuperado.Text =
                    $"Estrella {CrmInventoryUiBinder.Money(dash.StarCapital)} · " +
                    $"Opp {CrmInventoryUiBinder.Money(dash.OpportunityCapital)}";
                lblCapPendiente.Text =
                    $"Lento {CrmInventoryUiBinder.Money(dash.SlowCapital)} · " +
                    $"Crítico {CrmInventoryUiBinder.Money(dash.CriticalClassCapital)}";
            }
            else if (health != null)
            {
                lblCapRecuperado.Text =
                    $"Saludable {CrmInventoryUiBinder.Money(health.HealthyCapital)} · " +
                    $"Lento {CrmInventoryUiBinder.Money(health.SlowCapital)}";
                lblCapPendiente.Text =
                    $"Congelado {CrmInventoryUiBinder.Money(health.FrozenStatusCapital)} · " +
                    $"Crítico {CrmInventoryUiBinder.Money(health.CriticalCapital)}";
            }
            else
            {
                lblCapRecuperado.Text = "COGS vendido: " + CrmInventoryUiBinder.Money(
                    profit?.Cogs ?? summary?.CogsTotal ?? 0m);
                lblCapPendiente.Text = "Congelado clasificado: " + CrmInventoryUiBinder.Money(frozenCapital);
            }

            lblCapInventario.Text = "Valor PVP: " + CrmInventoryUiBinder.Money(inventarioValor);
            lblCapCaja.Text = risk != null
                ? "En riesgo: " + CrmInventoryUiBinder.Money(risk.AtRiskCapital)
                : profit != null
                    ? "Cobrado en venta: " + CrmProfitUiBinder.Money(profit.CollectedAtSale)
                    : "Productos c/stock: " + CrmInventoryUiBinder.Count(summary?.ProductsWithStock ?? 0);

            lstTop.Items.Clear();
            if (dash != null)
            {
                foreach (ProductClassificationRow s in dash.TopStars.Take(3))
                    lstTop.Items.Add($"★ {s.ProductName}");
                foreach (ProductPerformanceRankRow r in dash.TopProfit.Take(5 - lstTop.Items.Count))
                    lstTop.Items.Add(CrmProductPerformanceUiBinder.FormatRankLine(r));
            }
            else
            {
                var topProducts = CrmProfitUiBinder.TryLoadByProduct(ProfitPeriodKind.ThisMonth, out _, top: 5);
                if (topProducts != null && topProducts.Count > 0)
                {
                    foreach (var row in topProducts)
                        lstTop.Items.Add($"{row.Rank}. {row.GroupName}");
                }
                else if (summary != null)
                {
                    foreach (var row in summary.Rows
                                 .Where(r => r.UnitsSold > 0)
                                 .OrderByDescending(r => r.RealizedProfit)
                                 .Take(5)
                                 .Select((r, i) => $"{i + 1}. {r.ProductName}"))
                        lstTop.Items.Add(row);
                }
            }

            lstWatch.Items.Clear();
            if (dash != null && (dash.TopRisks.Count > 0 || dash.TopOpportunities.Count > 0))
            {
                foreach (ProductClassificationRow r in dash.TopRisks.Take(3))
                    lstWatch.Items.Add(CrmProductPerformanceUiBinder.FormatClassLine(r));
                foreach (ProductClassificationRow o in dash.TopOpportunities.Take(5 - lstWatch.Items.Count))
                    lstWatch.Items.Add(CrmProductPerformanceUiBinder.FormatClassLine(o));
            }
            else if (alerts != null && alerts.Alerts.Count > 0)
            {
                foreach (InventoryAlert a in alerts.Alerts.Take(5))
                {
                    string name = string.IsNullOrEmpty(a.ProductName) ? a.Kind.ToString() : a.ProductName;
                    lstWatch.Items.Add($"{a.Priority}: {name}");
                }
            }
            else if (summary != null)
            {
                foreach (var row in summary.Rows
                             .Where(r => r.HealthStatus is InventoryHealthStatus.Frozen
                                 or InventoryHealthStatus.Critical)
                             .OrderByDescending(r => r.InventoryCapital)
                             .Take(5)
                             .Select(r => $"{CrmInventoryUiBinder.HealthLabel(r.HealthStatus)}: {r.ProductName}"))
                    lstWatch.Items.Add(row);
            }

            if (lstTop.Items.Count == 0)
                lstTop.Items.Add("(Sin ventas con datos)");
            if (lstWatch.Items.Count == 0)
                lstWatch.Items.Add("(Sin alertas de capital)");

            // FASE 10.24 — Centro de decisiones (binder; sin lógica financiera en el Form)
            var decisionSnapshot = new DecisionCenterSnapshot
            {
                SalesVariationPct = salesDash?.RevenueVariation?.VariationPct,
                ProfitVariationPct = salesDash?.ProfitVariation?.VariationPct,
                FrozenCapitalAmount = frozenCapital > 0 ? frozenCapital : null,
                FrozenCapitalLabel = "Capital congelado"
            };

            IReadOnlyList<string> decisionLines = CrmDecisionUiBinder.TryLoadDashboardDecisionLines(
                out _,
                out DecisionCenterReport? center,
                ProfitPeriodKind.ThisMonth,
                decisionSnapshot,
                maxLines: 3);

            lblDecision1.Text = decisionLines.Count > 0 ? decisionLines[0] : string.Empty;
            lblDecision2.Text = decisionLines.Count > 1 ? decisionLines[1] : string.Empty;
            lblDecision3.Text = decisionLines.Count > 2 ? decisionLines[2] : string.Empty;

            if (center != null)
            {
                pnlDecisions.Text =
                    $"Centro de decisiones · {CrmDecisionUiBinder.Count(center.Summary.CriticalCount)} críticas · " +
                    $"{CrmDecisionUiBinder.Count(center.Summary.OpportunityCount)} oportunidades";
            }
            else
            {
                pnlDecisions.Text = "Centro de decisiones";
            }

            // FASE 11.17 — Contadores de acciones (binder; sin lógica en el Form)
            BusinessActionDashboardCounters? actions =
                CrmBusinessActionUiBinder.TryLoadDashboardCounters(out string? actionsErr);

            if (actions == null)
            {
                pnlActions.Text = "Acciones de negocio";
                lblAccPendientes.Text = "Pendientes: —";
                lblAccEnProceso.Text = "En proceso: —";
                lblAccCompletadas.Text = "Completadas: —";
                lblAccExitosas.Text = "Exitosas (histórico): —";
                lblAccImpacto.Text = string.IsNullOrWhiteSpace(actionsErr)
                    ? "Impacto observado: no disponible (migración 0012+)"
                    : $"Impacto observado: {actionsErr}";
            }
            else
            {
                pnlActions.Text = CrmBusinessActionUiBinder.FormatDashboardTitle(actions);
                lblAccPendientes.Text =
                    $"Pendientes: {CrmBusinessActionUiBinder.Count(actions.Pending)}";
                lblAccEnProceso.Text =
                    $"En proceso: {CrmBusinessActionUiBinder.Count(actions.InProgress)}";
                lblAccCompletadas.Text =
                    $"Completadas: {CrmBusinessActionUiBinder.Count(actions.Completed)}";
                lblAccExitosas.Text =
                    $"Exitosas (histórico): {CrmBusinessActionUiBinder.Count(actions.Successful)}";
                lblAccImpacto.Text = "Impacto observado: " + actions.ImpactHint
                    + " · " + CrmBusinessActionUiBinder.ClosedLoopStatusLine();
            }
        }
    }
}
