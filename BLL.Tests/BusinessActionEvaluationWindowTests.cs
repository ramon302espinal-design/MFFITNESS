using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 11.7 — Completar acción + ventana de evaluación.</summary>
public class BusinessActionEvaluationWindowTests
{
    private static BusinessActionService Svc()
        => new(new InMemoryBusinessActionStore());

    [Fact]
    public void Complete_Reanchors_Window_From_CompletedAt()
    {
        var created = new DateTime(2026, 8, 1, 12, 0, 0, DateTimeKind.Utc);
        var completed = new DateTime(2026, 8, 10, 12, 0, 0, DateTimeKind.Utc);
        var svc = Svc();

        var reg = svc.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.Promotion,
            Description = "Promo",
            CreatedAt = created,
            EvaluationDays = 14
        });

        Assert.Equal(created.AddDays(14), reg.Record!.EvaluationDueAt);

        var done = svc.Complete(reg.Record.ActionId, "ana", atUtc: completed);
        Assert.True(done.Success);
        Assert.Equal(BusinessActionStatus.Completed, done.Record!.Status);
        Assert.Equal(completed, done.Record.CompletedAt);
        Assert.Equal(14, done.Record.EvaluationDays);
        Assert.Equal(completed.AddDays(14), done.Record.EvaluationDueAt);
        Assert.NotNull(done.EvaluationWindow);
        Assert.Equal(BusinessActionEvaluationPhase.InWindow, done.EvaluationWindow!.Phase);
    }

    [Fact]
    public void Complete_With_Custom_Days_Overrides_Window()
    {
        var at = new DateTime(2026, 8, 20, 8, 0, 0, DateTimeKind.Utc);
        var svc = Svc();
        var reg = svc.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.PriceChange,
            Description = "Precio",
            EvaluationDays = 14,
            StartImmediately = true
        });

        var done = svc.Complete(reg.Record!.ActionId, evaluationDays: 7, atUtc: at);
        Assert.Equal(7, done.Record!.EvaluationDays);
        Assert.Equal(at.AddDays(7), done.Record.EvaluationDueAt);
    }

    [Fact]
    public void Resolve_Ready_When_AsOf_Past_Due()
    {
        var completed = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);
        var svc = Svc();
        var reg = svc.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.Campaign,
            Description = "Camp",
            StartImmediately = true
        });
        svc.Complete(reg.Record!.ActionId, atUtc: completed, evaluationDays: 10);

        DateTime mid = completed.AddDays(5);
        Assert.Equal(
            BusinessActionEvaluationPhase.InWindow,
            svc.GetEvaluationWindow(reg.Record.ActionId, mid)!.Phase);

        DateTime after = completed.AddDays(10);
        BusinessActionEvaluationWindow ready = svc.GetEvaluationWindow(reg.Record.ActionId, after)!;
        Assert.Equal(BusinessActionEvaluationPhase.Ready, ready.Phase);
        Assert.True(ready.IsReady);
    }

    [Fact]
    public void ListReadyForEvaluation_Excludes_InWindow_And_Evaluated()
    {
        var svc = Svc();
        var t0 = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);

        var a = svc.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.Promotion,
            Description = "Ready",
            StartImmediately = true
        });
        svc.Complete(a.Record!.ActionId, atUtc: t0, evaluationDays: 5);

        var b = svc.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.StockReduction,
            Description = "Still window",
            StartImmediately = true
        });
        svc.Complete(b.Record!.ActionId, atUtc: t0.AddDays(20), evaluationDays: 14);

        var c = svc.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.Other,
            Description = "Evaluated",
            StartImmediately = true
        });
        svc.Complete(
            c.Record!.ActionId,
            atUtc: t0,
            evaluationDays: 5,
            actualImpact: new BusinessActionActualImpact
            {
                Outcome = BusinessActionOutcome.Partial,
                Confidence = BusinessActionConfidence.Medium,
                Summary = "Observado parcial."
            });

        DateTime asOf = t0.AddDays(10);
        IReadOnlyList<BusinessActionRecord> ready = svc.ListReadyForEvaluation(asOf);
        Assert.Single(ready);
        Assert.Equal(a.Record.ActionId, ready[0].ActionId);

        IReadOnlyList<BusinessActionRecord> inWin = svc.ListInEvaluationWindow(asOf);
        Assert.Single(inWin);
        Assert.Equal(b.Record.ActionId, inWin[0].ActionId);
    }

    [Fact]
    public void SetEvaluationWindow_On_Completed_Uses_CompletedAt_Anchor()
    {
        var completed = new DateTime(2026, 8, 5, 0, 0, 0, DateTimeKind.Utc);
        var svc = Svc();
        var reg = svc.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.Replenishment,
            Description = "Repo",
            EvaluationDays = 14
        });
        svc.Complete(reg.Record!.ActionId, atUtc: completed);

        var set = svc.SetEvaluationWindow(new BusinessActionEvaluationWindowRequest
        {
            ActionId = reg.Record.ActionId,
            EvaluationDays = 21
        });

        Assert.True(set.Success);
        Assert.Equal(21, set.Record!.EvaluationDays);
        Assert.Equal(completed.AddDays(21), set.Record.EvaluationDueAt);
    }

    [Fact]
    public void Cancelled_Is_NotApplicable()
    {
        var svc = Svc();
        var reg = svc.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.MixChange,
            Description = "Mix"
        });
        svc.Cancel(reg.Record!.ActionId);

        Assert.Equal(
            BusinessActionEvaluationPhase.NotApplicable,
            svc.GetEvaluationWindow(reg.Record.ActionId)!.Phase);
    }

    [Fact]
    public void Planned_Phase_Before_Complete()
    {
        var svc = Svc();
        var reg = svc.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.MarginReview,
            Description = "Revisar margen",
            EvaluationDays = 14
        });

        BusinessActionEvaluationWindow w = BusinessActionEvaluationWindowMath.Resolve(reg.Record!);
        Assert.Equal(BusinessActionEvaluationPhase.Planned, w.Phase);
        Assert.Equal(14, w.EvaluationDays);
    }

    [Fact]
    public void Policy_And_SourceMap()
    {
        Assert.Contains("11.7", BusinessActionEvaluationWindowPolicy.Definition);
        Assert.Contains("completa", BusinessActionEvaluationWindowPolicy.Deferred);
        Assert.Contains("11.7", BusinessActionServicePolicy.Definition);
        Assert.NotNull(DecisionSourceMap.Find("BusinessActionEvaluationWindowMath"));
    }
}
