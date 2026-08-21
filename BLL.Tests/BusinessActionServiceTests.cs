using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 11.5 — ActionService (registro + estados).</summary>
public class BusinessActionServiceTests
{
    private static BusinessActionService Svc()
        => new(new InMemoryBusinessActionStore());

    [Fact]
    public void Test01_Decision_Can_Register_Action()
    {
        var svc = Svc();
        Guid decisionId = Guid.NewGuid();

        BusinessActionServiceResult r = svc.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.Promotion,
            Description = "Promoción 2x1",
            DecisionEventId = decisionId,
            Area = DecisionEventArea.Capital,
            EntityType = DecisionEntityType.Product,
            EntityId = "42",
            EntityName = "SKU-X",
            Reason = "Ventas ↓ 32%",
            ExpectedImpact = BusinessActionRecordFactory.Expected("Reducir capital congelado.")
        });

        Assert.True(r.Success);
        Assert.Equal(BusinessActionStatus.Pending, r.Record!.Status);
        Assert.Equal(decisionId, r.Record.DecisionEventId);
        Assert.NotNull(svc.Get(r.Record.ActionId));
    }

    [Fact]
    public void Test02_Register_Stores_User_Date_Type()
    {
        var at = new DateTime(2026, 8, 20, 10, 0, 0, DateTimeKind.Utc);
        var r = Svc().Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.StockReduction,
            Description = "Liquidar exceso",
            CreatedBy = "ana",
            CreatedAt = at
        });

        Assert.True(r.Success);
        Assert.Equal("ana", r.Record!.CreatedBy);
        Assert.Equal(at, r.Record.CreatedAt);
        Assert.Equal(BusinessActionType.StockReduction, r.Record.ActionType);
        Assert.True(r.PersistenceId > 0);
    }

    [Fact]
    public void Start_Then_Complete_Sets_Actors()
    {
        var svc = Svc();
        var reg = svc.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.Campaign,
            Description = "Campaña redes",
            CreatedBy = "ana"
        });

        Assert.True(svc.Start(reg.Record!.ActionId, "ana").Success);
        BusinessActionRecord? mid = svc.Get(reg.Record.ActionId);
        Assert.Equal(BusinessActionStatus.InProgress, mid!.Status);
        Assert.NotNull(mid.StartedAt);

        Assert.True(svc.Complete(reg.Record.ActionId, "bob", "cerrado en piso").Success);
        BusinessActionRecord done = svc.Get(reg.Record.ActionId)!;
        Assert.Equal(BusinessActionStatus.Completed, done.Status);
        Assert.Equal("bob", done.CompletedBy);
        Assert.Contains("cerrado", done.Notes);
    }

    [Fact]
    public void Test08_Cancelled_Cannot_Be_Successful()
    {
        var svc = Svc();
        var reg = svc.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.Promotion,
            Description = "Promo cancelada"
        });

        var bad = svc.ChangeStatus(new BusinessActionStatusRequest
        {
            ActionId = reg.Record!.ActionId,
            TargetStatus = BusinessActionStatus.Cancelled,
            ActualImpact = new BusinessActionActualImpact
            {
                Outcome = BusinessActionOutcome.Successful,
                Summary = "no debería"
            }
        });

        Assert.False(bad.Success);
        Assert.Contains("Outcome", bad.Message, StringComparison.OrdinalIgnoreCase);

        Assert.True(svc.Cancel(reg.Record.ActionId, "ana", "abortada").Success);
        Assert.Equal(BusinessActionStatus.Cancelled, svc.Get(reg.Record.ActionId)!.Status);
        Assert.False(BusinessActionCatalog.IsEvaluable(BusinessActionStatus.Cancelled));
    }

    [Fact]
    public void Terminal_Cannot_Transition_Again()
    {
        var svc = Svc();
        var reg = svc.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.Other,
            Description = "x",
            StartImmediately = true
        });

        Assert.True(svc.Complete(reg.Record!.ActionId).Success);
        Assert.False(svc.Cancel(reg.Record.ActionId).Success);
    }

    [Fact]
    public void List_Filters_By_Status_And_Decision()
    {
        var svc = Svc();
        Guid d = Guid.NewGuid();
        svc.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.Promotion,
            Description = "A",
            DecisionEventId = d
        });
        svc.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.PriceChange,
            Description = "B"
        });

        Assert.Single(svc.List(new BusinessActionQuery { DecisionEventId = d }));
        Assert.Single(svc.List(new BusinessActionQuery { ActionType = BusinessActionType.PriceChange }));
    }

    [Fact]
    public void Policy_And_SourceMap()
    {
        Assert.Contains("TEST 8", BusinessActionServicePolicy.Definition);
        Assert.Contains("completa", BusinessActionServicePolicy.Deferred);
        Assert.Contains("11.8", BusinessActionServicePolicy.Definition);
        Assert.NotNull(DecisionSourceMap.Find("BusinessActionService"));
        Assert.NotNull(DecisionSourceMap.Find("BusinessActionBaselineComposer"));
    }
}
