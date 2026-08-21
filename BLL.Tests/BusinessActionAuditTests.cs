using BLL.Models.Crm;
using BLL.Services.Crm;
using CORE;

namespace BLL.Tests;

/// <summary>FASE 11.12 — Auditoría de acciones + actor Sesión.</summary>
public class BusinessActionAuditTests : IDisposable
{
    public BusinessActionAuditTests()
    {
        Sesion.CerrarSesion();
    }

    public void Dispose()
    {
        Sesion.CerrarSesion();
    }

    [Fact]
    public void ActorResolver_Prefers_Explicit_Then_Sesion()
    {
        Assert.Null(BusinessActionActorResolver.ResolveName(null));

        Sesion.Iniciar(7, "ana", 1, "Admin", []);
        Assert.Equal("ana", BusinessActionActorResolver.ResolveName(null));
        Assert.Equal(7, BusinessActionActorResolver.ResolveUserId(null));

        Assert.Equal("bob", BusinessActionActorResolver.ResolveName("bob"));
        Assert.Null(BusinessActionActorResolver.ResolveUserId("bob")); // distinto de Sesion
    }

    [Fact]
    public void Register_Start_Complete_Writes_Audit_Trail()
    {
        var actionStore = new InMemoryBusinessActionStore();
        var auditStore = new InMemoryBusinessActionAuditStore();
        var svc = new BusinessActionService(actionStore, auditStore);
        var audit = new BusinessActionAuditService(auditStore);

        var reg = svc.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.Campaign,
            Description = "Campaña",
            CreatedBy = "ana"
        });
        Assert.True(svc.Start(reg.Record!.ActionId, "ana").Success);
        Assert.True(svc.Complete(reg.Record.ActionId, "bob", "cerrado").Success);

        IReadOnlyList<BusinessActionAuditEntry> trail = audit.ForAction(reg.Record.ActionId);
        Assert.Equal(3, trail.Count);
        Assert.Contains(trail, e => e.AuditAction == BusinessActionAuditAction.Register && e.Actor == "ana");
        Assert.Contains(trail, e => e.AuditAction == BusinessActionAuditAction.Start);
        Assert.Contains(trail, e => e.AuditAction == BusinessActionAuditAction.Complete && e.Actor == "bob");
    }

    [Fact]
    public void Register_Uses_Sesion_When_No_CreatedBy()
    {
        Sesion.Iniciar(3, "caja1", 2, "Cajero", []);

        var actionStore = new InMemoryBusinessActionStore();
        var auditStore = new InMemoryBusinessActionAuditStore();
        var svc = new BusinessActionService(actionStore, auditStore);

        var reg = svc.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.Other,
            Description = "Sin CreatedBy"
        });

        BusinessActionAuditEntry entry = auditStore.Query(new BusinessActionAuditQuery
        {
            ActionId = reg.Record!.ActionId
        }).Single();

        Assert.Equal("caja1", entry.Actor);
        Assert.Equal(3, entry.ActorUserId);
    }

    [Fact]
    public void Evaluate_Writes_Audit_With_Outcome()
    {
        var actionStore = new InMemoryBusinessActionStore();
        var auditStore = new InMemoryBusinessActionAuditStore();
        var actions = new BusinessActionService(actionStore, auditStore);
        var eval = new BusinessActionEvaluationService(actionStore, auditStore);

        var completed = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);
        var baseline = BusinessActionBaselineComposer.FromMetricValues(
            new BusinessActionBaselineCaptureRequest(),
            new Dictionary<string, decimal?> { ["sales.revenue"] = 100m });

        var reg = actions.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.Campaign,
            Description = "Camp",
            Baseline = baseline,
            StartImmediately = true,
            CreatedBy = "ana"
        });
        actions.Complete(reg.Record!.ActionId, atUtc: completed, evaluationDays: 3);
        actions.CapturePostMetrics(new BusinessActionPostMetricsRequest
        {
            ActionId = reg.Record.ActionId,
            CapturedAt = completed.AddDays(3),
            MetricValues = new Dictionary<string, decimal?> { ["sales.revenue"] = 130m }
        });

        Assert.True(eval.Evaluate(new BusinessActionEvaluateRequest
        {
            ActionId = reg.Record.ActionId,
            AsOfUtc = completed.AddDays(3),
            Actor = "ana"
        }).Success);

        Assert.Contains(
            auditStore.Query(new BusinessActionAuditQuery { ActionId = reg.Record.ActionId }),
            e => e.AuditAction == BusinessActionAuditAction.Evaluate
                && e.Outcome == BusinessActionOutcome.Successful
                && e.Actor == "ana");
    }

    [Fact]
    public void Cancel_Is_Audited_Not_Successful()
    {
        var actionStore = new InMemoryBusinessActionStore();
        var auditStore = new InMemoryBusinessActionAuditStore();
        var svc = new BusinessActionService(actionStore, auditStore);

        var reg = svc.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.Promotion,
            Description = "Promo",
            CreatedBy = "ana"
        });
        Assert.True(svc.Cancel(reg.Record!.ActionId, "ana", "abortada").Success);

        BusinessActionAuditEntry cancel = auditStore.Query(new BusinessActionAuditQuery
        {
            ActionId = reg.Record.ActionId,
            AuditAction = BusinessActionAuditAction.Cancel
        }).Single();

        Assert.Equal(BusinessActionStatus.Cancelled, cancel.NewStatus);
        Assert.Null(cancel.Outcome);
    }

    [Fact]
    public void Policy_And_SourceMap()
    {
        Assert.Contains("11.12", BusinessActionAuditPolicy.Definition);
        Assert.Contains("Sesion", BusinessActionAuditPolicy.ActorRule);
        Assert.Contains("completa", BusinessActionAuditPolicy.Deferred);
        Assert.NotNull(DecisionSourceMap.Find("BusinessActionAuditService"));
    }
}
