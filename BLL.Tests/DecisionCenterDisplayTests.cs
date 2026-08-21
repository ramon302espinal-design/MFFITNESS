using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 10.24 — textos Dashboard del Centro (sin UI).</summary>
public class DecisionCenterDisplayTests
{
    [Fact]
    public void DashboardLines_Prefer_Priorities_Then_Snapshot()
    {
        var report = new DecisionCenterReport
        {
            Summary = new DecisionCenterSummary
            {
                Headline = "HOY · 1 críticos · 0 importantes · 0 a revisar · 1 oportunidades",
                CriticalCount = 1,
                OpportunityCount = 1,
                SnapshotLines = ["Ventas -5% (variación del período)", "Capital congelado: RD$1,000 (expuesto, no pérdida garantizada)"]
            },
            PrioritiesToday =
            [
                new DecisionCenterPriorityItem
                {
                    Rank = 1,
                    Bucket = DecisionCenterBucket.Critical,
                    Title = "Revisar Star",
                    Recommendation = "Evaluar reposición — no comprar automáticamente.",
                    Priority = DecisionPriority.Critical,
                    Severity = DecisionSeverity.Critical
                }
            ]
        };

        var lines = DecisionCenterDisplay.DashboardLines(report, maxLines: 3);
        Assert.Equal(3, lines.Count);
        Assert.Contains("CRÍTICA", lines[0]);
        Assert.Contains("Star", lines[0]);
        Assert.Contains("Ventas", lines[1]);
        Assert.Contains("Capital", lines[2]);
    }

    [Fact]
    public void DashboardLines_Fallback_To_Headline_When_Empty()
    {
        var report = new DecisionCenterReport
        {
            Summary = new DecisionCenterSummary
            {
                Headline = "HOY · 0 críticos · 0 importantes · 0 a revisar · 0 oportunidades"
            }
        };

        var lines = DecisionCenterDisplay.DashboardLines(report, 3);
        Assert.StartsWith("HOY", lines[0]);
        Assert.Equal(string.Empty, lines[1]);
        Assert.Equal(string.Empty, lines[2]);
    }

    [Fact]
    public void PriorityLine_Uses_Soft_Recommendation_Snippet()
    {
        string line = DecisionCenterDisplay.PriorityLine(new DecisionCenterPriorityItem
        {
            Rank = 2,
            Bucket = DecisionCenterBucket.Opportunity,
            Title = "Evaluar Rising",
            Recommendation = "Evaluar oportunidad de crecimiento / cobertura."
        });

        Assert.StartsWith("2. [OPORTUNIDAD]", line);
        Assert.Contains("Evaluar", line);
    }

    [Fact]
    public void CountEventsInAreas_Filters()
    {
        var engine = new DecisionEngine().Evaluate(
        [
            new DecisionRuleCandidate
            {
                RuleId = "t",
                EventType = "inv.stockout_risk",
                Area = DecisionEventArea.Inventory,
                EntityType = DecisionEntityType.Product,
                EntityId = "1",
                EntityName = "A",
                PeriodKey = "p",
                Title = "Q",
                Description = "d",
                Recommendation = "Evaluar reposición — no comprar automáticamente.",
                Materiality = new DecisionMaterialityInput { TimeSensitiveStockout = true },
                ImpactAssessment = new DecisionImpactAssessment
                {
                    Inventory = DecisionImpactLevel.Critical
                },
                TimeSensitiveStockout = true,
                Urgency = DecisionUrgencyLevel.Immediate,
                RequiresImmediateReview = true
            },
            new DecisionRuleCandidate
            {
                RuleId = "t2",
                EventType = "sales.strong_decline",
                Area = DecisionEventArea.Sales,
                EntityType = DecisionEntityType.Portfolio,
                PeriodKey = "p",
                Title = "S",
                Description = "d",
                Recommendation = "Revisar demanda.",
                Materiality = new DecisionMaterialityInput { VariationPct = -20m },
                ImpactAssessment = new DecisionImpactAssessment
                {
                    Sales = DecisionImpactLevel.High
                },
                Urgency = DecisionUrgencyLevel.High
            }
        ]);

        var center = DecisionCenterComposer.Compose(engine);
        Assert.Equal(1, DecisionCenterDisplay.CountEventsInAreas(
            center, DecisionEventArea.Inventory));
        Assert.Equal(1, DecisionCenterDisplay.CountEventsInAreas(
            center, DecisionEventArea.Sales));
        Assert.Equal(0, DecisionCenterDisplay.CountEventsInAreas(
            center, DecisionEventArea.Investment));
    }

    [Fact]
    public void SourceMap_Includes_Decision_Binder()
    {
        Assert.NotNull(DecisionSourceMap.Find("CrmDecisionUiBinder"));
        Assert.Contains("completa", DecisionCenterPolicy.Deferred);
    }
}
