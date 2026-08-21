using System.Globalization;
using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato alertas de producto (FASE 10.14).</summary>
    public static class ProductAlertRulePolicy
    {
        public const string Definition =
            "FASE 10.14: estrella+quiebre, oportunidad, clase crítica. " +
            "Consume ProductClassification + SalesStarMix + SalesStockRisk (SSOT). " +
            "Sin score. InsufficientData/New → NO alerta avanzada (TEST 7/13).";

        public const string StarStockout =
            "product.star_stockout = Star con FlagStockoutRisk (SalesStarMix).";

        public const string Opportunity =
            "product.growth_opportunity = clase Opportunity o HealthyGrowth stock↔ventas.";

        public const string Critical =
            "product.critical_class = ProductPerformanceClass.Critical (≠ stockout solo).";
    }

    /// <summary>Composición pura alertas de producto (FASE 10.14).</summary>
    public static class ProductAlertRuleComposer
    {
        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-DO");

        public const int DefaultMaxProductAlerts = 10;

        public static IReadOnlyList<DecisionRuleCandidate> FromStarMix(
            SalesStarMixReport? mix,
            string periodKey,
            int max = DefaultMaxProductAlerts)
        {
            if (mix == null || mix.StarsWithStockoutRisk.Count == 0)
                return Array.Empty<DecisionRuleCandidate>();

            return mix.StarsWithStockoutRisk
                .OrderByDescending(s => s.RevenueTotal)
                .ThenBy(s => s.ProductName, StringComparer.OrdinalIgnoreCase)
                .Take(Math.Max(1, max))
                .Select(s => new DecisionRuleCandidate
                {
                    RuleId = "product.alerts.star_stockout",
                    EventType = "product.star_stockout",
                    Area = DecisionEventArea.Product,
                    EntityType = DecisionEntityType.Product,
                    EntityId = s.ProductId.ToString(CultureInfo.InvariantCulture),
                    EntityName = s.ProductName,
                    PeriodKey = periodKey,
                    Title = "Estrella con riesgo de quiebre",
                    Description =
                        $"{s.ProductName}: producto estrella con riesgo de quiebre " +
                        $"(ingresos {s.RevenueTotal.ToString("N2", Cultura)}).",
                    Reason = "Demanda alta en SKU estrella con stock crítico — priorizar revisión.",
                    Recommendation = "Evaluar reposición — no comprar automáticamente.",
                    Source = "SalesStarMixService",
                    Materiality = new DecisionMaterialityInput
                    {
                        TimeSensitiveStockout = true,
                        OpportunitySignal = false
                    },
                    ImpactAssessment = new DecisionImpactAssessment
                    {
                        Inventory = DecisionImpactLevel.Critical,
                        Sales = DecisionImpactLevel.High,
                        Financial = DecisionImpactLevel.High
                    },
                    Urgency = DecisionUrgencyLevel.Immediate,
                    TimeSensitiveStockout = true,
                    RequiresImmediateReview = true,
                    Evidence =
                    [
                        new DecisionEvidenceFact
                        {
                            Key = "revenue",
                            Label = "Ingresos",
                            ValueText = s.RevenueTotal.ToString("N2", Cultura),
                            MetricKey = "sales.revenue"
                        },
                        new DecisionEvidenceFact
                        {
                            Key = "class",
                            Label = "Clase",
                            ValueText = "Star",
                            MetricKey = "product.class"
                        }
                    ],
                    MetricKeys = ["product.is_star", "inv.flag_stockout", "stock.signal"]
                })
                .ToList();
        }

        public static IReadOnlyList<DecisionRuleCandidate> FromClassification(
            ProductClassificationReport? report,
            string periodKey,
            int max = DefaultMaxProductAlerts)
        {
            if (report == null)
                return Array.Empty<DecisionRuleCandidate>();

            var list = new List<DecisionRuleCandidate>();

            // Critical class
            list.AddRange(report.Rows
                .Where(r => r.Class == ProductPerformanceClass.Critical)
                .OrderByDescending(r => r.Performance?.InventoryCapital ?? 0m)
                .ThenBy(r => r.ProductName, StringComparer.OrdinalIgnoreCase)
                .Take(Math.Max(1, max))
                .Select(r => BuildCritical(r, periodKey)));

            // Opportunity class
            list.AddRange(report.Rows
                .Where(r => r.Class == ProductPerformanceClass.Opportunity)
                .OrderByDescending(r => r.Performance?.RevenueTotal ?? 0m)
                .ThenBy(r => r.ProductName, StringComparer.OrdinalIgnoreCase)
                .Take(Math.Max(1, max))
                .Select(r => BuildOpportunity(
                    r.ProductId, r.ProductName, periodKey,
                    "Clase Opportunity (FASE 8)",
                    r.Performance?.RevenueTotal ?? 0m,
                    r.Performance?.InventoryCapital ?? 0m,
                    "ProductClassificationService")));

            // InsufficientData / New → explícitamente NO generar candidatos de alerta avanzada
            return list;
        }

        public static IReadOnlyList<DecisionRuleCandidate> FromStockRiskGrowth(
            SalesStockRiskReport? stock,
            string periodKey,
            int max = DefaultMaxProductAlerts)
        {
            if (stock == null)
                return Array.Empty<DecisionRuleCandidate>();

            return stock.Rows
                .Where(r => r.PrimarySignal == SalesStockSignalKind.HealthyGrowth
                            || r.Signals.Contains(SalesStockSignalKind.HealthyGrowth))
                .OrderByDescending(r => r.UnitsPerDay ?? 0m)
                .ThenBy(r => r.ProductName, StringComparer.OrdinalIgnoreCase)
                .Take(Math.Max(1, max))
                .Select(r => BuildOpportunity(
                    r.ProductId, r.ProductName, periodKey,
                    string.IsNullOrWhiteSpace(r.Reason)
                        ? "Crecimiento saludable con stock adecuado."
                        : r.Reason,
                    revenue: 0m,
                    capital: 0m,
                    source: "SalesStockRiskService",
                    cover: r.DaysOfCover))
                .ToList();
        }

        /// <summary>
        /// TEST 7/13: New / InsufficientData no deben pasar el gate como alerta avanzada.
        /// </summary>
        public static bool ShouldSuppressAdvancedAlert(ProductPerformanceClass cls)
            => cls is ProductPerformanceClass.InsufficientData
                or ProductPerformanceClass.New;

        private static DecisionRuleCandidate BuildCritical(
            ProductClassificationRow r,
            string periodKey)
        {
            decimal capital = r.Performance?.InventoryCapital ?? 0m;
            return new DecisionRuleCandidate
            {
                RuleId = "product.alerts.critical",
                EventType = "product.critical_class",
                Area = DecisionEventArea.Product,
                EntityType = DecisionEntityType.Product,
                EntityId = r.ProductId.ToString(CultureInfo.InvariantCulture),
                EntityName = r.ProductName,
                PeriodKey = periodKey,
                Title = "Producto clase crítica",
                Description =
                    $"{r.ProductName}: clasificado Critical. "
                    + (r.Reasons.Count > 0 ? string.Join("; ", r.Reasons.Take(2)) : "Riesgo de performance."),
                Reason = "Clase Critical FASE 8 — distinto de solo stockout operativo.",
                Recommendation = "Revisar capital, tendencia y estrategia de salida.",
                Source = "ProductClassificationService",
                Materiality = new DecisionMaterialityInput
                {
                    CapitalAmount = capital > 0 ? capital : 1_000m
                },
                ImpactAssessment = new DecisionImpactAssessment
                {
                    Capital = DecisionImpactLevel.High,
                    Sales = DecisionImpactLevel.High,
                    Inventory = DecisionImpactLevel.Medium,
                    ProductStillSelling = r.Trend is ProductTrendDirection.Growing
                        or ProductTrendDirection.Stable
                },
                Urgency = DecisionUrgencyLevel.High,
                ProductStillSelling = r.Trend is ProductTrendDirection.Growing
                    or ProductTrendDirection.Stable,
                Evidence =
                [
                    new DecisionEvidenceFact
                    {
                        Key = "class",
                        Label = "Clase",
                        ValueText = "Critical",
                        MetricKey = "product.class"
                    },
                    new DecisionEvidenceFact
                    {
                        Key = "capital",
                        Label = "Capital",
                        ValueText = capital.ToString("N2", Cultura),
                        MetricKey = "capital.inventory"
                    }
                ],
                MetricKeys = ["product.class", "capital.inventory"]
            };
        }

        private static DecisionRuleCandidate BuildOpportunity(
            int productId,
            string name,
            string periodKey,
            string reason,
            decimal revenue,
            decimal capital,
            string source,
            decimal? cover = null)
            => new()
            {
                RuleId = "product.alerts.opportunity",
                EventType = "product.growth_opportunity",
                Area = DecisionEventArea.Product,
                EntityType = DecisionEntityType.Product,
                EntityId = productId.ToString(CultureInfo.InvariantCulture),
                EntityName = name,
                PeriodKey = periodKey,
                Title = "Oportunidad de crecimiento",
                Description = $"{name}: {reason}",
                Reason = reason,
                Recommendation = "Evaluar oportunidad de crecimiento / cobertura.",
                Source = source,
                Materiality = new DecisionMaterialityInput
                {
                    OpportunitySignal = true,
                    VariationPct = 20m,
                    CapitalAmount = capital > 0 ? capital : null
                },
                ImpactAssessment = new DecisionImpactAssessment
                {
                    Sales = DecisionImpactLevel.Medium,
                    Inventory = DecisionImpactLevel.Low
                },
                Urgency = DecisionUrgencyLevel.Medium,
                OpportunityWindow = true,
                Evidence =
                [
                    new DecisionEvidenceFact
                    {
                        Key = "revenue",
                        Label = "Ingresos",
                        ValueText = revenue > 0 ? revenue.ToString("N2", Cultura) : "N/D",
                        MetricKey = "sales.revenue"
                    },
                    new DecisionEvidenceFact
                    {
                        Key = "cover",
                        Label = "Cobertura",
                        ValueText = cover.HasValue
                            ? cover.Value.ToString("N0", Cultura) + " d"
                            : "N/D",
                        MetricKey = "inv.days_of_cover"
                    }
                ],
                MetricKeys = ["product.class", "stock.signal"]
            };
    }

    /// <summary>Regla de dominio productos (FASE 10.14).</summary>
    public sealed class ProductAlertDecisionRule : IDecisionRule
    {
        private readonly Func<ProfitPeriodKind, DateTime?, ProductClassificationReport?> _loadClass;
        private readonly Func<ProfitPeriodKind, DateTime?, SalesStarMixReport?> _loadStars;
        private readonly Func<ProfitPeriodKind, DateTime?, SalesStockRiskReport?> _loadStock;

        public string RuleId => "product.alerts.v1";

        public ProductAlertDecisionRule()
            : this(null, null, null)
        {
        }

        public ProductAlertDecisionRule(
            Func<ProfitPeriodKind, DateTime?, ProductClassificationReport?>? loadClass,
            Func<ProfitPeriodKind, DateTime?, SalesStarMixReport?>? loadStars,
            Func<ProfitPeriodKind, DateTime?, SalesStockRiskReport?>? loadStock)
        {
            if (loadClass != null)
                _loadClass = loadClass;
            else
            {
                var svc = new ProductClassificationService();
                _loadClass = (k, a) => svc.GetReport(k, a);
            }

            if (loadStars != null)
                _loadStars = loadStars;
            else
            {
                var svc = new SalesStarMixService();
                _loadStars = (k, a) => svc.GetReport(k, a);
            }

            if (loadStock != null)
                _loadStock = loadStock;
            else
            {
                var svc = new SalesStockRiskService();
                _loadStock = (k, a) => svc.GetReport(k, a);
            }
        }

        public IEnumerable<DecisionRuleCandidate> Evaluate(DecisionRuleContext context)
        {
            string periodKey = string.IsNullOrWhiteSpace(context.PeriodKey)
                ? SalesAlertRuleComposer.PeriodKey(context.PeriodKind, context.AsOf)
                : context.PeriodKey;

            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (DecisionRuleCandidate c in ProductAlertRuleComposer.FromStarMix(
                         context.Analytics != null
                         ? context.Analytics.StarMix
                         : _loadStars(context.PeriodKind, context.AsOf),
                         periodKey))
            {
                string fp = DecisionFingerprint.Compute(
                    c.Area, c.EventType, c.EntityType, c.EntityId, periodKey);
                if (seen.Add(fp))
                    yield return c;
            }

            foreach (DecisionRuleCandidate c in ProductAlertRuleComposer.FromClassification(
                         context.Analytics != null
                         ? context.Analytics.ProductClassification
                         : _loadClass(context.PeriodKind, context.AsOf),
                         periodKey))
            {
                string fp = DecisionFingerprint.Compute(
                    c.Area, c.EventType, c.EntityType, c.EntityId, periodKey);
                if (seen.Add(fp))
                    yield return c;
            }

            foreach (DecisionRuleCandidate c in ProductAlertRuleComposer.FromStockRiskGrowth(
                         context.Analytics != null
                         ? context.Analytics.StockRisk
                         : _loadStock(context.PeriodKind, context.AsOf),
                         periodKey))
            {
                string fp = DecisionFingerprint.Compute(
                    c.Area, c.EventType, c.EntityType, c.EntityId, periodKey);
                if (seen.Add(fp))
                    yield return c;
            }
        }
    }
}
