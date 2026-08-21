using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Composición pura de señales de dashboard (FASE 8.18).</summary>
    public static class ProductPerformanceDashboardComposer
    {
        public static int PortfolioHealthScore(
            int star, int healthy, int opportunity, int neu,
            int slow, int critical, int insufficient)
        {
            int classifiable = star + healthy + opportunity + neu + slow + critical;
            if (classifiable <= 0)
                return insufficient > 0 ? 0 : 50;

            int positive = star + healthy + opportunity + neu;
            int score = (int)Math.Round(positive * 100m / classifiable);
            score -= Math.Min(40, critical * 10);
            score -= Math.Min(15, slow * 2);
            return (int)Math.Clamp(score, 0, 100);
        }

        public static ProductPerformanceDashboardReport Build(
            ProductClassificationReport classification,
            int topLists = 5)
        {
            int top = topLists <= 0 ? 5 : topLists;

            var perfRows = classification.Rows
                .Where(r => r.Performance != null)
                .Select(r => r.Performance!)
                .ToList();

            var capitalRows = classification.Rows
                .Where(r => r.Performance != null)
                .Select(r => ProductCapitalPerformanceComposer.Compose(r.Performance!, r))
                .ToList();

            ProductCapitalPerformanceReport capital =
                ProductCapitalPerformanceComposer.BuildReport(
                    capitalRows, classification.PeriodKind, topImmobilized: top);

            var stars = classification.Rows
                .Where(r => r.Class == ProductPerformanceClass.Star)
                .OrderByDescending(r => r.Performance?.RealizedProfit ?? 0m)
                .Take(top)
                .ToList();

            var opps = classification.Rows
                .Where(r => r.Class == ProductPerformanceClass.Opportunity)
                .OrderByDescending(r => r.Performance?.RealizedProfit ?? 0m)
                .Take(top)
                .ToList();

            var risks = classification.Rows
                .Where(r => r.Class == ProductPerformanceClass.Critical)
                .OrderByDescending(r => r.Performance?.ImmobilizedCapital
                    ?? r.Performance?.InventoryCapital ?? 0m)
                .Take(top)
                .ToList();

            return new ProductPerformanceDashboardReport
            {
                PeriodKind = classification.PeriodKind,
                StarCount = classification.StarCount,
                HealthyCount = classification.HealthyCount,
                OpportunityCount = classification.OpportunityCount,
                SlowCount = classification.SlowCount,
                CriticalCount = classification.CriticalCount,
                NewCount = classification.NewCount,
                InsufficientCount = classification.InsufficientCount,
                PortfolioHealthScore = PortfolioHealthScore(
                    classification.StarCount,
                    classification.HealthyCount,
                    classification.OpportunityCount,
                    classification.NewCount,
                    classification.SlowCount,
                    classification.CriticalCount,
                    classification.InsufficientCount),
                StarCapital = capital.StarCapital,
                OpportunityCapital = capital.OpportunityCapital,
                CriticalClassCapital = capital.CriticalClassCapital,
                SlowCapital = capital.SlowCapital,
                TotalImmobilizedCapital = capital.TotalImmobilizedCapital,
                TopStars = stars,
                TopOpportunities = opps,
                TopRisks = risks,
                TopUnits = ProductPerformanceRanker.Rank(
                    perfRows, ProductPerformanceMetricKind.UnitsSold, top),
                TopProfit = ProductPerformanceRanker.Rank(
                    perfRows, ProductPerformanceMetricKind.RealizedProfit, top),
                TopRoi = ProductPerformanceRanker.Rank(
                    perfRows, ProductPerformanceMetricKind.RoiPct, top),
                TopMargin = ProductPerformanceRanker.Rank(
                    perfRows, ProductPerformanceMetricKind.MarginPct, top),
                TopTurnover = ProductPerformanceRanker.Rank(
                    perfRows, ProductPerformanceMetricKind.TurnoverProxy, top)
            };
        }
    }
}
