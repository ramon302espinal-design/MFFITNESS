using System.Diagnostics;
using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato performance del Decision Engine (FASE 10.27 / brief §88).</summary>
    public static class DecisionPerformancePolicy
    {
        public const string Definition =
            "FASE 10.27: obtener métricas agregadas UNA vez por run; " +
            "evaluar reglas en memoria. NO N consultas por alerta.";

        public const string SharedSources =
            "SalesVariation · SalesShare · InventoryAlerts · CapitalBridge · " +
            "ProductClassification · StarMix · StockRisk · Acceleration · SeriesTrend · " +
            "Forecast · TrappedCapital · InvestmentSummaries";

        public const string Deferred =
            "FASE 10 completa.";
    }

    /// <summary>
    /// Hooks opcionales (tests) — cada función se invoca como máximo una vez por Load.
    /// </summary>
    public sealed class DecisionAnalyticsBundleHooks
    {
        public Func<ProfitPeriodKind, DateTime?, SalesVariationReport?>? LoadSalesVariation { get; init; }
        public Func<ProfitPeriodKind, DateTime?, SalesShareReport?>? LoadSalesShare { get; init; }
        public Func<DateTime?, InventoryAlertReport?>? LoadInventoryAlerts { get; init; }
        public Func<ProfitPeriodKind, DateTime?, SalesCapitalBridgeReport?>? LoadCapitalBridge { get; init; }
        public Func<ProfitPeriodKind, DateTime?, ProductClassificationReport?>? LoadProductClassification { get; init; }
        public Func<ProfitPeriodKind, DateTime?, SalesStarMixReport?>? LoadStarMix { get; init; }
        public Func<ProfitPeriodKind, DateTime?, SalesStockRiskReport?>? LoadStockRisk { get; init; }
        public Func<ProfitPeriodKind, DateTime?, SalesAccelerationReport?>? LoadAcceleration { get; init; }
        public Func<ProfitPeriodKind, DateTime?, SalesSeriesTrendReport?>? LoadSeriesTrend { get; init; }
        public Func<ProfitPeriodKind, DateTime?, SalesForecastReport?>? LoadForecast { get; init; }
        public Func<InvestmentCapitalBridgeReport?>? LoadTrappedCapital { get; init; }
        public Func<IReadOnlyList<InvestmentSummary>?>? LoadInvestmentSummaries { get; init; }
    }

    /// <summary>Carga SSOT una vez por run (FASE 10.27).</summary>
    public static class DecisionAnalyticsBundleLoader
    {
        public static DecisionAnalyticsBundle Load(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            DateTime? asOf = null,
            DecisionAnalyticsBundleHooks? hooks = null)
        {
            var sw = Stopwatch.StartNew();
            var sources = new List<string>(12);
            int calls = 0;

            T? Once<T>(string name, Func<T?> load) where T : class
            {
                calls++;
                sources.Add(name);
                try { return load(); }
                catch { return null; }
            }

            hooks ??= new DecisionAnalyticsBundleHooks();

            SalesVariationReport? variations = Once("SalesVariation", () =>
                hooks.LoadSalesVariation != null
                    ? hooks.LoadSalesVariation(periodKind, asOf)
                    : new SalesVariationService().GetVariations(periodKind, asOf));

            SalesShareReport? share = Once("SalesShare", () =>
                hooks.LoadSalesShare != null
                    ? hooks.LoadSalesShare(periodKind, asOf)
                    : new SalesShareService().GetProductShare(
                        periodKind, SalesShareMetric.Revenue, topN: 3, asOf));

            InventoryAlertReport? invAlerts = Once("InventoryAlerts", () =>
                hooks.LoadInventoryAlerts != null
                    ? hooks.LoadInventoryAlerts(asOf)
                    : new InventoryAlertService().GetAlerts(asOf));

            SalesCapitalBridgeReport? bridge = Once("CapitalBridge", () =>
                hooks.LoadCapitalBridge != null
                    ? hooks.LoadCapitalBridge(periodKind, asOf)
                    : new SalesCapitalBridgeService().GetReport(periodKind, asOf));

            ProductClassificationReport? classification = Once("ProductClassification", () =>
                hooks.LoadProductClassification != null
                    ? hooks.LoadProductClassification(periodKind, asOf)
                    : new ProductClassificationService().GetReport(periodKind, asOf));

            SalesStarMixReport? stars = Once("StarMix", () =>
                hooks.LoadStarMix != null
                    ? hooks.LoadStarMix(periodKind, asOf)
                    : new SalesStarMixService().GetReport(periodKind, asOf));

            SalesStockRiskReport? stock = Once("StockRisk", () =>
                hooks.LoadStockRisk != null
                    ? hooks.LoadStockRisk(periodKind, asOf)
                    : new SalesStockRiskService().GetReport(periodKind, asOf));

            SalesAccelerationReport? accel = Once("Acceleration", () =>
                hooks.LoadAcceleration != null
                    ? hooks.LoadAcceleration(periodKind, asOf)
                    : new SalesAccelerationService().GetReport(periodKind, asOf));

            SalesSeriesTrendReport? series = Once("SeriesTrend", () =>
                hooks.LoadSeriesTrend != null
                    ? hooks.LoadSeriesTrend(periodKind, asOf)
                    : new SalesSeriesTrendService().GetReport(periodKind, asOf));

            SalesForecastReport? forecast = Once("Forecast", () =>
                hooks.LoadForecast != null
                    ? hooks.LoadForecast(periodKind, asOf)
                    : new SalesForecastService().GetEstimate(periodKind, horizonDays: 30, asOf));

            InvestmentCapitalBridgeReport? trapped = Once("TrappedCapital", () =>
                hooks.LoadTrappedCapital != null
                    ? hooks.LoadTrappedCapital()
                    : new InvestmentCapitalBridgeService().GetTrappedCapitalReport());

            IReadOnlyList<InvestmentSummary>? summaries = Once("InvestmentSummaries", () =>
            {
                if (hooks.LoadInvestmentSummaries != null)
                    return hooks.LoadInvestmentSummaries();
                var svc = new InvestmentService();
                return svc.List().Select(i => svc.GetSummary(i.Id)).ToList();
            });

            sw.Stop();

            return new DecisionAnalyticsBundle
            {
                PeriodKind = periodKind,
                AsOf = asOf,
                SalesVariation = variations,
                SalesShare = share,
                InventoryAlerts = invAlerts,
                CapitalBridge = bridge,
                ProductClassification = classification,
                StarMix = stars,
                StockRisk = stock,
                Acceleration = accel,
                SeriesTrend = series,
                Forecast = forecast,
                TrappedCapital = trapped,
                InvestmentSummaries = summaries,
                Stats = new DecisionAnalyticsLoadStats
                {
                    ServiceCalls = calls,
                    SourcesLoaded = sources,
                    ElapsedMs = sw.ElapsedMilliseconds,
                    PolicyNote = DecisionPerformancePolicy.Definition
                }
            };
        }

        /// <summary>Enriquece el contexto con bundle si aún no tiene Analytics.</summary>
        public static DecisionRuleContext EnsureAnalytics(
            DecisionRuleContext context,
            DecisionAnalyticsBundleHooks? hooks = null)
        {
            ArgumentNullException.ThrowIfNull(context);
            if (context.Analytics != null)
                return context;

            DecisionAnalyticsBundle bundle = Load(context.PeriodKind, context.AsOf, hooks);
            return WithAnalytics(context, bundle);
        }

        public static DecisionRuleContext WithAnalytics(
            DecisionRuleContext context,
            DecisionAnalyticsBundle analytics)
            => new()
            {
                PeriodKind = context.PeriodKind,
                AsOf = context.AsOf,
                PeriodKey = context.PeriodKey,
                Bag = context.Bag,
                Analytics = analytics
            };
    }
}
