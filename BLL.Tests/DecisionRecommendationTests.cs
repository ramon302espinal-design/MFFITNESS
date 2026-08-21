using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 10.19 — motor de recomendaciones + lenguaje suave.</summary>
public class DecisionRecommendationTests
{
    private static DecisionEvent Ev(
        string type,
        DecisionEventArea area,
        DecisionEntityType entityType,
        string? entityId,
        string entityName,
        string? recommendation = null,
        DecisionPriority priority = DecisionPriority.High,
        DecisionSeverity severity = DecisionSeverity.High)
    {
        var draft = DecisionEventFactory.Create(
            type, area, entityType, entityId, entityName, "p",
            "UnitTest", type, "desc",
            recommendation: recommendation ?? string.Empty);

        return new DecisionEvent
        {
            EventId = draft.EventId,
            EventType = draft.EventType,
            Area = draft.Area,
            EntityType = draft.EntityType,
            EntityId = draft.EntityId,
            EntityName = draft.EntityName,
            PeriodKey = draft.PeriodKey,
            DetectedAt = draft.DetectedAt,
            CreatedAt = draft.CreatedAt,
            Severity = severity,
            Priority = priority,
            Title = draft.Title,
            Description = draft.Description,
            Reason = draft.Reason,
            Impact = draft.Impact,
            Recommendation = draft.Recommendation,
            Status = draft.Status,
            Source = draft.Source,
            Fingerprint = draft.Fingerprint,
            Evidence = draft.Evidence,
            MetricKeys = draft.MetricKeys
        };
    }

    [Fact]
    public void SoftLanguage_Accepts_Allowed_Verbs()
    {
        Assert.True(DecisionSoftLanguageGuard.IsCompliant("Revisar costos y mix."));
        Assert.True(DecisionSoftLanguageGuard.IsCompliant("Evaluar reposición según demanda."));
        Assert.True(DecisionSoftLanguageGuard.IsCompliant("Considerar diversificar el portafolio."));
        Assert.True(DecisionSoftLanguageGuard.IsCompliant("Analizar el impacto en margen."));
    }

    [Fact]
    public void SoftLanguage_Rejects_Forbidden()
    {
        Assert.True(DecisionSoftLanguageGuard.ContainsForbidden("Debe comprar 10 unidades ahora."));
        Assert.True(DecisionSoftLanguageGuard.ContainsForbidden("Comprar automáticamente el SKU."));
        Assert.True(DecisionSoftLanguageGuard.ContainsForbidden("Garantizamos recuperación total."));
        Assert.True(DecisionSoftLanguageGuard.ContainsForbidden("Vas a perder RD$25,000."));
        Assert.False(DecisionSoftLanguageGuard.IsCompliant("Ejecutar compra inmediata."));
    }

    [Fact]
    public void Ensure_Prepends_Soft_Verb_And_Strips_Forbidden()
    {
        string fixedText = DecisionSoftLanguageGuard.Ensure(
            "Debe comprar stock urgente para no perder ventas.");
        Assert.True(DecisionSoftLanguageGuard.StartsWithSoftVerb(fixedText));
        Assert.False(DecisionSoftLanguageGuard.ContainsForbidden(fixedText));
    }

    [Fact]
    public void Catalog_Covers_Core_EventTypes()
    {
        Assert.NotNull(DecisionRecommendationCatalog.Find("inv.stockout_risk"));
        Assert.NotNull(DecisionRecommendationCatalog.Find("capital.at_risk"));
        Assert.NotNull(DecisionRecommendationCatalog.Find("product.growth_opportunity"));
        Assert.True(DecisionRecommendationCatalog.Find("product.growth_opportunity")!.IsOpportunity);
        Assert.Contains("No comprar automáticamente",
            DecisionRecommendationCatalog.DefaultPolicyReminders[0]);
    }

    [Fact]
    public void FromEvent_Uses_Catalog_And_Is_Compliant()
    {
        DecisionEvent e = Ev(
            "capital.at_risk", DecisionEventArea.Capital,
            DecisionEntityType.Product, "42", "Proteína X");

        DecisionRecommendation rec = DecisionRecommendationComposer.FromEvent(e);
        Assert.Equal(DecisionRecommendationVerb.Revisar, rec.Verb);
        Assert.Contains("Proteína X", rec.Headline);
        Assert.True(rec.SoftLanguageCompliant);
        Assert.Contains(rec.SuggestedChecks, c => c.Contains("At-risk", StringComparison.OrdinalIgnoreCase)
            || c.Length > 0);
        Assert.False(rec.IsOpportunity);
    }

    [Fact]
    public void FromEvent_Opportunity_Flag()
    {
        DecisionRecommendation rec = DecisionRecommendationComposer.FromEvent(
            Ev("product.growth_opportunity", DecisionEventArea.Product,
                DecisionEntityType.Product, "1", "SKU-Z"));

        Assert.True(rec.IsOpportunity);
        Assert.Equal(DecisionRecommendationVerb.Evaluar, rec.Verb);
    }

    [Fact]
    public void FromGroup_Multiple_Signals_One_Narrative()
    {
        // brief §99 / TEST 10
        var group = new DecisionGroup
        {
            GroupId = "g1",
            GroupKey = "1|42",
            Title = "Proteína X",
            EntityType = DecisionEntityType.Product,
            EntityId = "42",
            EntityName = "Proteína X",
            Priority = DecisionPriority.Critical,
            Severity = DecisionSeverity.Critical,
            Events =
            [
                Ev("sales.strong_decline", DecisionEventArea.Sales,
                    DecisionEntityType.Product, "42", "Proteína X",
                    priority: DecisionPriority.High),
                Ev("profit.decline", DecisionEventArea.Profit,
                    DecisionEntityType.Product, "42", "Proteína X"),
                Ev("capital.at_risk", DecisionEventArea.Capital,
                    DecisionEntityType.Product, "42", "Proteína X",
                    priority: DecisionPriority.Critical, severity: DecisionSeverity.Critical)
            ],
            Primary = null
        };
        // Primary for ordering inside FromGroup uses Events order by priority when ApplyToGroup;
        // FromGroup uses g.Primary ?? first
        group = new DecisionGroup
        {
            GroupId = group.GroupId,
            GroupKey = group.GroupKey,
            Title = group.Title,
            EntityType = group.EntityType,
            EntityId = group.EntityId,
            EntityName = group.EntityName,
            Priority = group.Priority,
            Severity = group.Severity,
            Events = group.Events,
            Primary = group.Events[2]
        };

        DecisionRecommendation rec = DecisionRecommendationComposer.FromGroup(group);
        Assert.Equal("g1", rec.GroupId);
        Assert.Contains("3 señales", rec.Body);
        Assert.Contains("Proteína X", rec.Body);
        Assert.True(rec.SoftLanguageCompliant);
        Assert.Contains("estrategia conjunta", rec.Body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Engine_Fills_Recommendations_And_Group_Narrative()
    {
        var candidates = new[]
        {
            new DecisionRuleCandidate
            {
                RuleId = "t1",
                EventType = "inv.stockout_risk",
                Area = DecisionEventArea.Inventory,
                EntityType = DecisionEntityType.Product,
                EntityId = "9",
                EntityName = "Star",
                PeriodKey = "p",
                Title = "Quiebre",
                Description = "Stock bajo",
                Recommendation = "Comprar automáticamente 50 unidades.",
                Materiality = new DecisionMaterialityInput { TimeSensitiveStockout = true },
                ImpactAssessment = new DecisionImpactAssessment
                {
                    Inventory = DecisionImpactLevel.Critical
                },
                TimeSensitiveStockout = true,
                Urgency = DecisionUrgencyLevel.Immediate
            },
            new DecisionRuleCandidate
            {
                RuleId = "t2",
                EventType = "product.star_stockout",
                Area = DecisionEventArea.Product,
                EntityType = DecisionEntityType.Product,
                EntityId = "9",
                EntityName = "Star",
                PeriodKey = "p",
                Title = "Estrella quiebre",
                Description = "Star",
                Recommendation = "Evaluar reposición — no comprar automáticamente.",
                Materiality = new DecisionMaterialityInput { TimeSensitiveStockout = true },
                ImpactAssessment = new DecisionImpactAssessment
                {
                    Inventory = DecisionImpactLevel.Critical,
                    Sales = DecisionImpactLevel.High
                },
                TimeSensitiveStockout = true,
                Urgency = DecisionUrgencyLevel.Immediate
            }
        };

        var report = new DecisionEngine().Evaluate(candidates);
        Assert.Equal(2, report.EmittedCount);
        Assert.Equal(1, report.GroupCount);
        Assert.True(report.RecommendationCount >= 1);
        Assert.False(string.IsNullOrWhiteSpace(report.PrimaryGroup!.Recommendation));
        Assert.All(report.Events, e =>
            Assert.True(DecisionSoftLanguageGuard.IsCompliant(e.Recommendation)));
        Assert.All(report.Recommendations, r => Assert.True(r.SoftLanguageCompliant));
        Assert.DoesNotContain(report.Events,
            e => DecisionSoftLanguageGuard.ContainsForbidden(e.Recommendation));
    }

    [Fact]
    public void Policy_Mentions_Soft_Language()
    {
        Assert.Contains("10.19", DecisionRecommendationPolicy.Definition);
        Assert.Contains("Revisar", DecisionRecommendationPolicy.SoftLanguage);
        Assert.Contains("completa", DecisionRecommendationPolicy.Deferred);
    }
}
