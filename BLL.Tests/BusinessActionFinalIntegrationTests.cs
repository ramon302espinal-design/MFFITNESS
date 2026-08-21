using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>
/// FASE 11.24 — integración final closed-loop (brief §88).
/// Pipeline en memoria: Decisión→Acción→Resultado→Evaluación→Aprendizaje.
/// </summary>
public class BusinessActionFinalIntegrationTests
{
    [Fact]
    public void ClosedLoop_SalesUp_ProfitUp_Completes_Checklist()
    {
        var actions = new InMemoryBusinessActionStore();
        var decisions = new InMemoryDecisionHistoryStore();
        var audit = new InMemoryBusinessActionAuditStore();
        var svc = new BusinessActionIntegrationService(actions, decisions, audit);

        var at = new DateTime(2026, 8, 1, 10, 0, 0, DateTimeKind.Utc);
        BusinessActionClosedLoopResult result = svc.RunClosedLoop(new BusinessActionClosedLoopRequest
        {
            DecisionEventType = "sales.decline",
            DecisionTitle = "Caída de ventas",
            Area = DecisionEventArea.Sales,
            ActionType = BusinessActionType.Campaign,
            ActionDescription = "Campaña de recuperación",
            Actor = "ana",
            AtUtc = at,
            EvaluationDays = 7,
            BaselineMetrics = new Dictionary<string, decimal?>
            {
                ["sales.revenue"] = 100m,
                ["profit.realized"] = 20m
            },
            PostMetrics = new Dictionary<string, decimal?>
            {
                ["sales.revenue"] = 130m,
                ["profit.realized"] = 35m
            },
            ExpectedMetricKeys = ["sales.revenue", "profit.realized"]
        });

        Assert.True(result.Success);
        Assert.True(result.SoftLanguageOk);
        Assert.NotNull(result.Decision);
        Assert.NotNull(result.Action);
        Assert.Equal(BusinessActionOutcome.Successful, result.Evaluation!.Outcome);
        Assert.NotNull(result.Timeline);
        Assert.Contains(result.Timeline!.Steps, s => s.Kind == BusinessActionTimelineStepKind.DecisionDetected);
        Assert.Contains(result.Timeline.Steps, s => s.Kind == BusinessActionTimelineStepKind.ActionRegistered);
        Assert.Contains(result.Timeline.Steps, s => s.Kind == BusinessActionTimelineStepKind.OutcomeEvaluated);
        Assert.NotNull(result.LearningByType);
        Assert.True(result.Checklist.Count >= 8);
        Assert.Contains(result.Checklist, c => c.StartsWith("1 ", StringComparison.Ordinal));
        Assert.Contains(result.Checklist, c => c.StartsWith("2 ", StringComparison.Ordinal));
        Assert.Contains(result.Checklist, c => c.StartsWith("4 ", StringComparison.Ordinal));
        Assert.Contains(result.Checklist, c => c.Contains("Soft language", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("11.25", result.PolicyNote);
        Assert.False(BusinessActionSoftLanguageGuard.ContainsForbidden(result.Evaluation.Summary));
    }

    [Fact]
    public void ClosedLoop_Without_Metrics_Is_InsufficientData_Still_Operational()
    {
        var svc = new BusinessActionIntegrationService(
            new InMemoryBusinessActionStore(),
            new InMemoryDecisionHistoryStore());

        BusinessActionClosedLoopResult result = svc.RunClosedLoop(new BusinessActionClosedLoopRequest
        {
            ActionType = BusinessActionType.Other,
            ActionDescription = "Sin baseline",
            AtUtc = new DateTime(2026, 8, 10, 0, 0, 0, DateTimeKind.Utc)
        });

        Assert.True(result.Success);
        Assert.Equal(BusinessActionOutcome.InsufficientData, result.Evaluation!.Outcome);
        Assert.Contains(result.Checklist, c => c.Contains("SIN DATOS", StringComparison.OrdinalIgnoreCase)
            || c.StartsWith("4 ", StringComparison.Ordinal));
    }

    [Fact]
    public void PhasePolicy_ClosedLoop_Operational_And_Formally_Complete()
    {
        Assert.Equal("11.25", BusinessActionPhasePolicy.Stage);
        Assert.True(BusinessActionPhasePolicy.ClosedLoopOperational);
        Assert.True(BusinessActionPhasePolicy.IsComplete);
        Assert.Contains("completa", BusinessActionPhasePolicy.Deferred);
        Assert.Contains("11.25", BusinessActionPhasePolicy.Definition);
        Assert.Contains("Alerta→Decisión→Acción", BusinessActionPhasePolicy.Definition);
        Assert.Contains("FrmReportes", BusinessActionPhasePolicy.SuccessCriteria);
        Assert.Contains("completa", BusinessActionIntegrationService.StatusBanner(), StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(DecisionSourceMap.Find("BusinessActionIntegrationService"));
        Assert.NotNull(DecisionSourceMap.Find("BusinessActionPhasePolicy"));
        Assert.Contains("NO TOCAR", DecisionSourceMap.Find("FrmReportes")!.MustNot);
        Assert.Contains("completa", BusinessActionPolicy.Deferred);
        Assert.Contains("completa", BusinessActionSoftLanguagePolicy.Deferred);
        Assert.Contains("completa", BusinessActionBriefPolicy.Deferred);
        Assert.Contains("completa", BusinessActionTimelinePolicy.Deferred);
    }
}
