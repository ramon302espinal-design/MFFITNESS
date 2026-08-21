using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 10.23 — auditoría append-only.</summary>
public class DecisionAuditTests
{
    private static DecisionRuleCandidate Cand() => new()
    {
        RuleId = "r",
        EventType = "capital.at_risk",
        Area = DecisionEventArea.Capital,
        EntityType = DecisionEntityType.Product,
        EntityId = "42",
        EntityName = "SKU",
        PeriodKey = "p",
        Title = "Riesgo",
        Description = "x",
        Recommendation = "Revisar estrategia de salida.",
        Materiality = new DecisionMaterialityInput { CapitalAmount = 8_000m },
        ImpactAssessment = new DecisionImpactAssessment
        {
            Capital = DecisionImpactLevel.High
        },
        Urgency = DecisionUrgencyLevel.High
    };

    [Fact]
    public void Capture_Writes_Detected_Audit()
    {
        var auditStore = new InMemoryDecisionAuditStore();
        var histStore = new InMemoryDecisionHistoryStore();
        var hist = new DecisionHistoryService(histStore, auditStore);
        var audit = new DecisionAuditService(auditStore);

        var engine = new DecisionEngine().Evaluate([Cand()]);
        hist.Capture(engine);

        var entries = audit.ForEvent(engine.Events[0].EventId);
        Assert.Single(entries);
        Assert.Equal(DecisionAuditAction.Detected, entries[0].Action);
        Assert.Equal("capital.at_risk", entries[0].EventType);
        Assert.NotNull(entries[0].HistoryId);
    }

    [Fact]
    public void Resolve_Writes_Resolve_Audit_With_Actor()
    {
        var auditStore = new InMemoryDecisionAuditStore();
        var histStore = new InMemoryDecisionHistoryStore();
        var hist = new DecisionHistoryService(histStore, auditStore);
        var res = new DecisionResolutionService(histStore, auditStore);
        var audit = new DecisionAuditService(auditStore);

        var engine = new DecisionEngine().Evaluate([Cand()]);
        hist.Capture(engine);
        Guid id = engine.Events[0].EventId;
        Assert.True(res.Resolve(id, "ana", "cerrado").Success);

        var entries = audit.ForEvent(id);
        Assert.Equal(2, entries.Count);
        Assert.Contains(entries, e => e.Action == DecisionAuditAction.Detected);
        DecisionAuditEntry resolve = entries.First(e => e.Action == DecisionAuditAction.Resolve);
        Assert.Equal("ana", resolve.Actor);
        Assert.Equal(DecisionEventStatus.Active, resolve.PreviousStatus);
        Assert.Equal(DecisionEventStatus.Resolved, resolve.NewStatus);
        Assert.Equal("cerrado", resolve.Note);
    }

    [Fact]
    public void Full_Lifecycle_Audit_Trail()
    {
        var auditStore = new InMemoryDecisionAuditStore();
        var histStore = new InMemoryDecisionHistoryStore();
        var hist = new DecisionHistoryService(histStore, auditStore);
        var res = new DecisionResolutionService(hist, auditStore);
        var audit = new DecisionAuditService(auditStore);

        var engine = new DecisionEngine().Evaluate([Cand()]);
        hist.Capture(engine);
        Guid id = engine.Events[0].EventId;

        res.StartReview(id, "ana");
        res.Reopen(id, "ana");
        res.Ignore(id, "ana", "ruido");

        var trail = audit.ForEvent(id);
        Assert.Equal(4, trail.Count);
        // Ordered newest first
        Assert.Equal(DecisionAuditAction.Ignore, trail[0].Action);
        Assert.Equal(DecisionAuditAction.Reopen, trail[1].Action);
        Assert.Equal(DecisionAuditAction.StartReview, trail[2].Action);
        Assert.Equal(DecisionAuditAction.Detected, trail[3].Action);
        Assert.Equal("ana", trail[1].Actor); // reopen keeps actor in audit
    }

    [Fact]
    public void Duplicate_Capture_Audits_Suppression()
    {
        var auditStore = new InMemoryDecisionAuditStore();
        var histStore = new InMemoryDecisionHistoryStore();
        var hist = new DecisionHistoryService(histStore, auditStore);
        var audit = new DecisionAuditService(auditStore);

        var e1 = new DecisionEngine().Evaluate([Cand()]);
        hist.Capture(e1);
        var e2 = new DecisionEngine().Evaluate([Cand()]);
        hist.Capture(e2);

        var suppressed = audit.GetAudit(new DecisionAuditQuery
        {
            Action = DecisionAuditAction.DuplicateSuppressed
        });
        Assert.Single(suppressed);
        Assert.Equal(e2.Events[0].Fingerprint, suppressed[0].Fingerprint);
    }

    [Fact]
    public void Query_Filters_By_Action()
    {
        var auditStore = new InMemoryDecisionAuditStore();
        var histStore = new InMemoryDecisionHistoryStore();
        var hist = new DecisionHistoryService(histStore, auditStore);
        var res = new DecisionResolutionService(histStore, auditStore);
        var audit = new DecisionAuditService(auditStore);

        var engine = new DecisionEngine().Evaluate([Cand()]);
        hist.Capture(engine);
        res.Ignore(engine.Events[0].EventId, "x");

        Assert.Single(audit.GetAudit(new DecisionAuditQuery { Action = DecisionAuditAction.Ignore }));
        Assert.Single(audit.GetAudit(new DecisionAuditQuery { Action = DecisionAuditAction.Detected }));
    }

    [Fact]
    public void Policy_And_SourceMap()
    {
        Assert.Contains("10.23", DecisionAuditPolicy.Definition);
        Assert.Contains("append-only", DecisionAuditPolicy.Definition);
        Assert.Contains("completa", DecisionAuditPolicy.Deferred);

        var s = DecisionSourceMap.Find("DecisionAuditService");
        Assert.NotNull(s);
        Assert.Contains("10.23", s!.Phase);
    }
}
