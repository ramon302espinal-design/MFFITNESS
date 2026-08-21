using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato dashboard de ventas (FASE 9.21).</summary>
    public static class SalesDashboardPolicy
    {
        public const string Definition =
            "FASE 9.21: Dashboard consume KPIs de ventas, variación, tendencia, " +
            "aceleración, forecast (estimación), mix estrellas y alertas §62. " +
            "Sin lógica financiera en Forms.";

        public const string Alerts =
            "Alertas: crecimiento/caída fuerte, desaceleración, ventas↓+stock↑, " +
            "ventas↑+ganancia↓, ventas↑+margen↓, ROI↓, quiebre, oportunidad. " +
            "No ejecutan acciones.";

        public const string Forecast =
            "Forecast en dashboard = escenario base etiquetado ESTIMACIÓN. Nunca certeza.";
    }

    /// <summary>Composición pura de snapshot dashboard (FASE 9.21).</summary>
    public static class SalesDashboardComposer
    {
        public static IReadOnlyList<SalesDashboardAlert> BuildAlerts(
            SalesVariationReport? variations,
            SalesAccelerationResult? acceleration,
            SalesStockRiskReport? stockRisk,
            SalesCapitalBridgeReport? capital,
            SalesStarMixReport? starMix)
        {
            var alerts = new List<SalesDashboardAlert>();

            if (variations != null)
            {
                if (variations.Revenue.Direction == SalesVariationDirection.Up
                    && variations.Revenue.Strength == SalesVariationStrength.Strong)
                {
                    alerts.Add(Alert(
                        SalesDashboardAlertKind.StrongGrowth,
                        "📈",
                        $"Crecimiento fuerte de ingresos ({variations.Revenue.Display})"));
                }

                if (variations.Revenue.Direction == SalesVariationDirection.Down
                    && variations.Revenue.Strength == SalesVariationStrength.Strong)
                {
                    alerts.Add(Alert(
                        SalesDashboardAlertKind.StrongDecline,
                        "📉",
                        $"Caída fuerte de ingresos ({variations.Revenue.Display})"));
                }

                foreach (SalesCrossSignal cross in variations.CrossSignals)
                {
                    if (cross.Kind == SalesCrossSignalKind.RevenueUpProfitDown)
                    {
                        alerts.Add(Alert(
                            SalesDashboardAlertKind.RevenueUpProfitDown,
                            "⚠️",
                            cross.Message));
                    }
                    else if (cross.Kind == SalesCrossSignalKind.RevenueUpMarginDown)
                    {
                        alerts.Add(Alert(
                            SalesDashboardAlertKind.RevenueUpMarginDown,
                            "⚠️",
                            cross.Message));
                    }
                }
            }

            if (acceleration?.Kind == SalesAccelerationKind.Decelerating)
            {
                alerts.Add(Alert(
                    SalesDashboardAlertKind.Deceleration,
                    "⚠️",
                    $"Desaceleración: {acceleration.Reason}"));
            }

            if (stockRisk != null && stockRisk.StockoutRiskCount > 0)
            {
                alerts.Add(Alert(
                    SalesDashboardAlertKind.StockoutRisk,
                    "🔴",
                    $"Riesgo de quiebre en {stockRisk.StockoutRiskCount} producto(s)"));
            }

            if (stockRisk != null && stockRisk.CapitalRiskCount > 0)
            {
                alerts.Add(Alert(
                    SalesDashboardAlertKind.SalesDownStockUp,
                    "⚠️",
                    $"Ventas ↓ + stock alto / capital en riesgo ({stockRisk.CapitalRiskCount})"));
            }

            if (capital != null && capital.RevenueUpRoiDownCount > 0)
            {
                alerts.Add(Alert(
                    SalesDashboardAlertKind.RoiDown,
                    "⚠️",
                    $"Ventas ↑ + ROI ↓ en {capital.RevenueUpRoiDownCount} producto(s)"));
            }

            if (stockRisk != null && stockRisk.ReplenishmentOpportunityCount > 0)
            {
                alerts.Add(Alert(
                    SalesDashboardAlertKind.GrowthOpportunity,
                    "🟡",
                    $"Oportunidad de reabastecimiento ({stockRisk.ReplenishmentOpportunityCount})"));
            }
            else if (starMix != null
                     && starMix.StarsWithStockoutRisk.Count > 0)
            {
                alerts.Add(Alert(
                    SalesDashboardAlertKind.GrowthOpportunity,
                    "🟡",
                    $"Estrella(s) con riesgo de quiebre — reabastecer ({starMix.StarsWithStockoutRisk.Count})"));
            }

            return alerts;
        }

        public static SalesDashboardReport Build(
            ProfitPeriodKind periodKind,
            SalesSummary summary,
            SalesVariationReport? variations = null,
            SalesSeriesTrendResult? revenueTrend = null,
            SalesAccelerationResult? revenueAcceleration = null,
            SalesForecastReport? forecast = null,
            SalesStarMixReport? starMix = null,
            SalesStockRiskReport? stockRisk = null,
            SalesCapitalBridgeReport? capital = null,
            IReadOnlyList<SalesDashboardTopItem>? topProducts = null,
            IReadOnlyList<SalesDashboardTopItem>? topCategories = null)
        {
            return new SalesDashboardReport
            {
                PeriodKind = periodKind,
                TransactionCount = summary.TransactionCount,
                UnitsSold = summary.UnitsSold,
                RevenueTotal = summary.RevenueTotal,
                RealizedProfit = summary.RealizedProfit,
                MarginPct = summary.MarginPct,
                AverageTicket = summary.AverageTicket,
                RevenueVariation = variations?.Revenue,
                ProfitVariation = variations?.RealizedProfit,
                TicketVariation = variations?.Ticket,
                RevenueTrend = revenueTrend?.Kind ?? SalesSeriesTrendKind.InsufficientData,
                RevenueAcceleration = revenueAcceleration?.Kind ?? SalesAccelerationKind.InsufficientData,
                ForecastBaseRevenue = forecast?.HasEstimate == true
                    ? forecast.Base.EstimatedRevenue
                    : null,
                ForecastConfidence = forecast?.Confidence
                    ?? SalesForecastConfidence.InsufficientData,
                ForecastNote = forecast?.LanguageNote ?? SalesForecastPolicy.Language,
                StarCount = starMix?.StarCount ?? 0,
                StarRevenueSharePct = starMix?.StarRevenueSharePct,
                StockoutRiskCount = stockRisk?.StockoutRiskCount ?? 0,
                CapitalRiskCount = stockRisk?.CapitalRiskCount ?? capital?.CapitalRiskCount ?? 0,
                Alerts = BuildAlerts(variations, revenueAcceleration, stockRisk, capital, starMix),
                TopProducts = topProducts ?? Array.Empty<SalesDashboardTopItem>(),
                TopCategories = topCategories ?? Array.Empty<SalesDashboardTopItem>()
            };
        }

        private static SalesDashboardAlert Alert(
            SalesDashboardAlertKind kind, string icon, string message)
            => new() { Kind = kind, Icon = icon, Message = message };
    }
}
