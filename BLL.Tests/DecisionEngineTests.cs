using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 10.8 — motor base de reglas (pipeline).</summary>
public class DecisionEngineTests
{
    private sealed class ScriptedRule : IDecisionRule
    {
        private readonly DecisionRuleCandidate[] _candidates;
        public ScriptedRule(string id, params DecisionRuleCandidate[] candidates)
        {
            RuleId = id;
            _candidates = candidates;
        }

        public string RuleId { get; }

        public IEnumerable<DecisionRuleCandidate> Evaluate(DecisionRuleContext context)
            => _candidates;
    }

    private static DecisionRuleCandidate Decline(decimal varPct, string? entityId = null)
        => new()
        {
            RuleId = "test.sales.decline",
            EventType = "sales.strong_decline",
            Area = DecisionEventArea.Sales,
            EntityType = entityId == null ? DecisionEntityType.Portfolio : DecisionEntityType.Product,
            EntityId = entityId,
            EntityName = entityId == null ? "" : "Prod " + entityId,
            PeriodKey = "Last30Days",
            Title = "Caída de ventas",
            Description = $"Ingresos {varPct:N0}%",
            Reason = "Variación material vs período comparable",
            Recommendation = "Revisar mezcla y demanda.",
            Source = "UnitTest",
            Materiality = new DecisionMaterialityInput { VariationPct = varPct },
            ImpactAssessment = new DecisionImpactAssessment
            {
                Sales = DecisionImpactLevel.High,
                Financial = DecisionImpactLevel.High
            },
            Urgency = DecisionUrgencyLevel.Medium,
            Evidence =
            [
                new DecisionEvidenceFact
                {
                    Key = "revenue_var",
                    Label = "Var. ingresos",
                    ValueText = $"{varPct:N2}%",
                    MetricKey = "sales.revenue_var_pct"
                }
            ],
            MetricKeys = ["sales.revenue_var_pct"]
        };

    [Fact]
    public void Noise_Variation_Is_Suppressed_By_Materiality()
    {
        var engine = new DecisionEngine();
        var report = engine.Run(
            [new ScriptedRule("r1", Decline(1.5m))],
            new DecisionRuleContext());

        Assert.Equal(1, report.CandidatesConsidered);
        Assert.Equal(0, report.EmittedCount);
        Assert.Equal(1, report.SuppressedByMateriality);
    }

    [Fact]
    public void Strong_Decline_Emits_With_Severity_And_Priority()
    {
        // TEST 1
        var engine = new DecisionEngine();
        var report = engine.Evaluate([Decline(-30m)]);

        Assert.Equal(1, report.EmittedCount);
        DecisionEvent e = report.Events[0];
        Assert.Equal("sales.strong_decline", e.EventType);
        Assert.True(e.Severity >= DecisionSeverity.High);
        Assert.NotEqual(DecisionPriority.Unspecified, e.Priority);
        Assert.Contains("Revisar", e.Recommendation);
        Assert.False(string.IsNullOrWhiteSpace(e.Fingerprint));
    }

    [Fact]
    public void Cross_Signal_Emits_Even_If_Flat_Variation()
    {
        // TEST 2
        var c = new DecisionRuleCandidate
        {
            RuleId = "test.cross",
            EventType = "sales.rev_up_profit_down",
            Area = DecisionEventArea.Sales,
            PeriodKey = "ThisMonth",
            Title = "Ingresos↑ Ganancia↓",
            Description = "Contradicción",
            Recommendation = "Revisar costos y mezcla.",
            Materiality = new DecisionMaterialityInput
            {
                VariationPct = 1m,
                CrossSignal = true
            },
            ImpactAssessment = new DecisionImpactAssessment
            {
                Sales = DecisionImpactLevel.Medium,
                Financial = DecisionImpactLevel.High
            }
        };

        var report = new DecisionEngine().Evaluate([c]);
        Assert.Equal(1, report.EmittedCount);
        Assert.Equal(DecisionEventArea.Sales, report.Events[0].Area);
    }

    [Fact]
    public void Duplicate_Fingerprint_Keeps_Single_Active()
    {
        // TEST 8
        var engine = new DecisionEngine();
        var report = engine.Evaluate([Decline(-30m), Decline(-32m)]);

        Assert.Equal(2, report.CandidatesConsidered);
        Assert.Equal(1, report.EmittedCount);
        Assert.Equal(1, report.SuppressedByDuplicate);
    }

    [Fact]
    public void Insufficient_Data_Does_Not_Emit_Advanced_Alert()
    {
        // TEST 7
        var c = new DecisionRuleCandidate
        {
            RuleId = "test.insuf",
            EventType = "product.insufficient_data",
            Area = DecisionEventArea.Product,
            EntityType = DecisionEntityType.Product,
            EntityId = "9",
            PeriodKey = "asOf",
            Title = "Nuevo",
            Description = "Sin historial",
            Materiality = new DecisionMaterialityInput
            {
                InsufficientData = true,
                VariationPct = -40m,
                CapitalAmount = 50_000m
            }
        };

        var report = new DecisionEngine().Evaluate([c]);
        Assert.Equal(0, report.EmittedCount);
        Assert.Equal(1, report.SuppressedByMateriality);
    }

    [Fact]
    public void Stockout_Emits_High_Priority()
    {
        // TEST 4 conceptual
        var c = new DecisionRuleCandidate
        {
            RuleId = "test.stockout",
            EventType = "product.star_stockout",
            Area = DecisionEventArea.Product,
            EntityType = DecisionEntityType.Product,
            EntityId = "1",
            EntityName = "Estrella",
            PeriodKey = "asOf",
            Title = "Estrella con riesgo de quiebre",
            Description = "Demanda > stock",
            Recommendation = "Evaluar reposición.",
            Materiality = new DecisionMaterialityInput
            {
                CapitalAmount = 100m,
                TimeSensitiveStockout = true
            },
            ImpactAssessment = new DecisionImpactAssessment
            {
                Inventory = DecisionImpactLevel.Critical,
                Sales = DecisionImpactLevel.High
            },
            TimeSensitiveStockout = true,
            Urgency = DecisionUrgencyLevel.Immediate
        };

        var report = new DecisionEngine().Evaluate([c]);
        Assert.Equal(1, report.EmittedCount);
        Assert.True(report.Events[0].Priority >= DecisionPriority.High);
    }

    [Fact]
    public void Events_Ordered_By_Priority_Then_Severity()
    {
        var low = new DecisionRuleCandidate
        {
            RuleId = "a",
            EventType = "trend.deceleration",
            Area = DecisionEventArea.Trend,
            PeriodKey = "p",
            Title = "Desaceleración",
            Description = "Frena",
            Materiality = new DecisionMaterialityInput { VariationPct = 10m },
            ImpactAssessment = new DecisionImpactAssessment { Sales = DecisionImpactLevel.Medium },
            Urgency = DecisionUrgencyLevel.Low
        };

        var high = new DecisionRuleCandidate
        {
            RuleId = "b",
            EventType = "capital.at_risk",
            Area = DecisionEventArea.Capital,
            EntityType = DecisionEntityType.Product,
            EntityId = "2",
            PeriodKey = "p",
            Title = "Capital en riesgo",
            Description = "Ventas↓ stock↑",
            Materiality = new DecisionMaterialityInput
            {
                CapitalAmount = 25_000m,
                VariationPct = -32m
            },
            ImpactAssessment = new DecisionImpactAssessment
            {
                Capital = DecisionImpactLevel.Critical,
                Sales = DecisionImpactLevel.High
            },
            Urgency = DecisionUrgencyLevel.High,
            RequiresImmediateReview = true
        };

        var report = new DecisionEngine().Evaluate([low, high]);
        Assert.True(report.EmittedCount >= 2);
        Assert.Equal("capital.at_risk", report.Primary!.EventType);
        Assert.True((int)report.Primary.Priority >= (int)report.Events[^1].Priority);
    }

    [Fact]
    public void BuiltIn_Registry_Includes_All_Domain_Rules_Through_Investment()
    {
        Assert.Contains(DecisionRuleRegistry.BuiltIn, r => r.RuleId == "sales.alerts.v1");
        Assert.Contains(DecisionRuleRegistry.BuiltIn, r => r.RuleId == "profit.alerts.v1");
        Assert.Contains(DecisionRuleRegistry.BuiltIn, r => r.RuleId == "roi.alerts.v1");
        Assert.Contains(DecisionRuleRegistry.BuiltIn, r => r.RuleId == "inv.alerts.v1");
        Assert.Contains(DecisionRuleRegistry.BuiltIn, r => r.RuleId == "capital.alerts.v1");
        Assert.Contains(DecisionRuleRegistry.BuiltIn, r => r.RuleId == "product.alerts.v1");
        Assert.Contains(DecisionRuleRegistry.BuiltIn, r => r.RuleId == "trend.alerts.v1");
        Assert.Contains(DecisionRuleRegistry.BuiltIn, r => r.RuleId == "forecast.alerts.v1");
        Assert.Contains(DecisionRuleRegistry.BuiltIn, r => r.RuleId == "invst.alerts.v1");
        Assert.Equal(9, DecisionRuleRegistry.BuiltIn.Count);
    }

    [Fact]
    public void Policy_Forbids_Auto_Actions()
    {
        Assert.Contains("Nunca auto-compra", DecisionEnginePolicy.Definition);
        Assert.Contains("10.9", DecisionEnginePolicy.DomainRules);
        Assert.Contains("Revisar/Evaluar", DecisionEnginePolicy.SoftLanguage);
    }

    [Fact]
    public void Different_Entities_Are_Not_Duplicates()
    {
        var report = new DecisionEngine().Evaluate(
        [
            Decline(-30m, "1"),
            Decline(-30m, "2")
        ]);
        Assert.Equal(2, report.EmittedCount);
        Assert.Equal(0, report.SuppressedByDuplicate);
    }
}
