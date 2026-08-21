using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>
/// FASE 11.22 — batería del brief §80 (TEST 1–12).
/// Sin BD: ActionService + Evaluation + Learning en memoria. Sin mutar POS.
/// </summary>
public class BusinessActionBriefTests
{
    private static (BusinessActionService Actions, BusinessActionEvaluationService Eval,
        InMemoryBusinessActionStore ActionStore, InMemoryDecisionHistoryStore DecisionStore)
        Svc()
    {
        var actions = new InMemoryBusinessActionStore();
        var decisions = new InMemoryDecisionHistoryStore();
        return (
            new BusinessActionService(actions),
            new BusinessActionEvaluationService(actions),
            actions,
            decisions);
    }

    private static BusinessActionRecord ReadyWithMetrics(
        BusinessActionService actions,
        Dictionary<string, decimal?> before,
        Dictionary<string, decimal?> after,
        BusinessActionType type = BusinessActionType.Promotion,
        Guid? decisionEventId = null,
        string? createdBy = "ana")
    {
        var completed = new DateTime(2026, 8, 1, 12, 0, 0, DateTimeKind.Utc);
        var baseline = BusinessActionBaselineComposer.FromMetricValues(
            new BusinessActionBaselineCaptureRequest { CapturedAt = completed.AddDays(-1) },
            before);

        var reg = actions.Register(new BusinessActionRegisterRequest
        {
            ActionType = type,
            Description = "Acción brief",
            DecisionEventId = decisionEventId,
            CreatedBy = createdBy,
            Baseline = baseline,
            StartImmediately = true,
            ExpectedImpact = BusinessActionRecordFactory.Expected(
                "Mejorar métricas observadas.",
                before.Keys.ToList())
        });

        Assert.True(reg.Success);
        actions.Complete(reg.Record!.ActionId, atUtc: completed, evaluationDays: 7);
        actions.CapturePostMetrics(new BusinessActionPostMetricsRequest
        {
            ActionId = reg.Record.ActionId,
            CapturedAt = completed.AddDays(7),
            MetricValues = after
        });

        return actions.Get(reg.Record.ActionId)!;
    }

    // TEST 1: Crear decisión → debe permitir registrar acción.
    [Fact]
    public void Test01_Decision_Allows_Registering_Action()
    {
        var (actions, _, _, decisions) = Svc();
        Guid eventId = Guid.NewGuid();
        long historyId = decisions.Append(new DecisionHistoryRecord
        {
            EventId = eventId,
            EventType = "sales.decline",
            Title = "Caída ventas",
            Fingerprint = "brief-t1",
            DetectedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            Status = DecisionEventStatus.Active
        });

        var r = actions.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.Campaign,
            Description = "Respuesta a decisión",
            DecisionEventId = eventId,
            DecisionHistoryId = historyId
        });

        Assert.True(r.Success);
        Assert.Equal(eventId, r.Record!.DecisionEventId);
        Assert.Equal(historyId, r.Record.DecisionHistoryId);
        Assert.Equal(BusinessActionStatus.Pending, r.Record.Status);
    }

    // TEST 2: Registrar acción → usuario, fecha, tipo.
    [Fact]
    public void Test02_Register_Stores_User_Date_Type()
    {
        var (actions, _, _, _) = Svc();
        var at = new DateTime(2026, 8, 21, 9, 30, 0, DateTimeKind.Utc);

        var r = actions.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.PriceChange,
            Description = "Ajuste PVP",
            CreatedBy = "carmen",
            CreatedAt = at
        });

        Assert.True(r.Success);
        Assert.Equal("carmen", r.Record!.CreatedBy);
        Assert.Equal(at, r.Record.CreatedAt);
        Assert.Equal(BusinessActionType.PriceChange, r.Record.ActionType);
    }

    // TEST 3: Completar acción → debe permitir registrar resultado.
    [Fact]
    public void Test03_Complete_Allows_Registering_Result()
    {
        var (actions, eval, _, _) = Svc();
        var rec = ReadyWithMetrics(
            actions,
            new Dictionary<string, decimal?> { ["sales.revenue"] = 100m, ["profit.realized"] = 40m },
            new Dictionary<string, decimal?> { ["sales.revenue"] = 120m, ["profit.realized"] = 50m });

        Assert.Equal(BusinessActionStatus.Completed, rec.Status);
        Assert.NotNull(rec.ActualImpact?.Deltas);
        Assert.NotEmpty(rec.ActualImpact!.Deltas);

        var result = eval.Evaluate(new BusinessActionEvaluateRequest { ActionId = rec.ActionId });
        Assert.True(result.Success);
        Assert.NotNull(result.Record!.ActualImpact);
        Assert.NotEqual(BusinessActionOutcome.Unspecified, result.Record.ActualImpact!.Outcome);
    }

    // TEST 4: Comparar baseline vs resultado → variación.
    [Fact]
    public void Test04_Baseline_Vs_Result_Computes_Variation()
    {
        var (actions, _, _, _) = Svc();
        var rec = ReadyWithMetrics(
            actions,
            new Dictionary<string, decimal?> { ["sales.revenue"] = 200m },
            new Dictionary<string, decimal?> { ["sales.revenue"] = 260m });

        BusinessActionMetricDelta delta = Assert.Single(
            rec.ActualImpact!.Deltas.Where(d => d.MetricKey == "sales.revenue"));

        Assert.Equal(200m, delta.Before);
        Assert.Equal(260m, delta.After);
        Assert.Equal(30m, delta.Change); // variación relativa %
        Assert.False(delta.IsPercentagePoints);
    }

    // TEST 5: Ventas ↑ Ganancia ↑ → EXITOSA.
    [Fact]
    public void Test05_Sales_Up_Profit_Up_Is_Successful()
    {
        var (actions, eval, _, _) = Svc();
        var rec = ReadyWithMetrics(
            actions,
            new Dictionary<string, decimal?>
            {
                ["sales.revenue"] = 100m,
                ["profit.realized"] = 20m
            },
            new Dictionary<string, decimal?>
            {
                ["sales.revenue"] = 130m,
                ["profit.realized"] = 35m
            });

        var result = eval.Evaluate(new BusinessActionEvaluateRequest { ActionId = rec.ActionId });
        Assert.True(result.Success);
        Assert.Equal(BusinessActionOutcome.Successful, result.Record!.ActualImpact!.Outcome);
        Assert.Equal("EXITOSA", BusinessActionCatalog.OutcomeLabel(result.Record.ActualImpact.Outcome));
    }

    // TEST 6: Ventas ↑ Ganancia ↓ → PARCIAL.
    [Fact]
    public void Test06_Sales_Up_Profit_Down_Is_Partial()
    {
        var (actions, eval, _, _) = Svc();
        var rec = ReadyWithMetrics(
            actions,
            new Dictionary<string, decimal?>
            {
                ["sales.revenue"] = 100m,
                ["profit.realized"] = 40m
            },
            new Dictionary<string, decimal?>
            {
                ["sales.revenue"] = 140m,
                ["profit.realized"] = 25m
            });

        var result = eval.Evaluate(new BusinessActionEvaluateRequest { ActionId = rec.ActionId });
        Assert.True(result.Success);
        Assert.Equal(BusinessActionOutcome.Partial, result.Record!.ActualImpact!.Outcome);
        Assert.Equal("PARCIAL", BusinessActionCatalog.OutcomeLabel(result.Record.ActualImpact.Outcome));
    }

    // TEST 7: Sin datos → SIN DATOS.
    [Fact]
    public void Test07_No_Data_Is_InsufficientData()
    {
        var (actions, eval, _, _) = Svc();
        var completed = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);
        var reg = actions.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.Other,
            Description = "Sin métricas",
            StartImmediately = true
        });
        actions.Complete(reg.Record!.ActionId, atUtc: completed, evaluationDays: 7);

        var result = eval.Evaluate(new BusinessActionEvaluateRequest
        {
            ActionId = reg.Record.ActionId,
            AsOfUtc = completed.AddDays(7),
            AllowBeforeWindowEnd = true
        });
        Assert.True(result.Success);
        Assert.Equal(BusinessActionOutcome.InsufficientData, result.Record!.ActualImpact!.Outcome);
        Assert.Equal("SIN DATOS", BusinessActionCatalog.OutcomeLabel(result.Record.ActualImpact.Outcome));
    }

    // TEST 8: Acción cancelada → no evaluar como exitosa.
    [Fact]
    public void Test08_Cancelled_Not_Evaluated_As_Successful()
    {
        var (actions, eval, _, _) = Svc();
        var reg = actions.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.Promotion,
            Description = "Promo abortada",
            StartImmediately = true
        });

        Assert.True(actions.Cancel(reg.Record!.ActionId, "ana", "cliente desistió").Success);

        var blocked = actions.ChangeStatus(new BusinessActionStatusRequest
        {
            ActionId = reg.Record.ActionId,
            TargetStatus = BusinessActionStatus.Cancelled,
            ActualImpact = new BusinessActionActualImpact
            {
                Outcome = BusinessActionOutcome.Successful,
                Summary = "no"
            }
        });
        Assert.False(blocked.Success);

        var evalResult = eval.Evaluate(new BusinessActionEvaluateRequest { ActionId = reg.Record.ActionId });
        Assert.False(evalResult.Success);
        Assert.Contains("Cancelad", evalResult.Message, StringComparison.OrdinalIgnoreCase);
    }

    // TEST 9: Acción repetida → debe conservar historial.
    [Fact]
    public void Test09_Repeated_Action_Keeps_History()
    {
        var (actions, _, store, _) = Svc();
        Guid eventId = Guid.NewGuid();

        for (int i = 0; i < 3; i++)
        {
            Assert.True(actions.Register(new BusinessActionRegisterRequest
            {
                ActionType = BusinessActionType.Promotion,
                Description = $"Promo #{i + 1}",
                DecisionEventId = eventId,
                CreatedBy = "ana"
            }).Success);
        }

        IReadOnlyList<BusinessActionRecord> linked = actions.List(new BusinessActionQuery
        {
            DecisionEventId = eventId,
            Top = 50
        });
        Assert.Equal(3, linked.Count);
        Assert.Equal(3, store.Query(new BusinessActionQuery { DecisionEventId = eventId, Top = 50 }).Count);
        Assert.Equal(3, linked.Select(a => a.ActionId).Distinct().Count());
    }

    // TEST 10: Problema recurrente → debe detectarse.
    [Fact]
    public void Test10_Recurrent_Problem_Is_Detected()
    {
        var (_, _, actionStore, decisionStore) = Svc();
        var learning = new BusinessActionLearningService(actionStore, decisionStore);
        string entityId = "42";
        var baseAt = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);

        for (int i = 0; i < 3; i++)
        {
            Guid eventId = Guid.NewGuid();
            decisionStore.Append(new DecisionHistoryRecord
            {
                EventId = eventId,
                EventType = "capital.immobilized",
                Title = "Capital inmovilizado",
                Fingerprint = $"rec-{i}",
                EntityType = DecisionEntityType.Product,
                EntityId = entityId,
                Area = DecisionEventArea.Capital,
                DetectedAt = baseAt.AddDays(i * 10),
                CreatedAt = baseAt.AddDays(i * 10),
                Status = DecisionEventStatus.Active
            });

            actionStore.Append(new BusinessActionRecord
            {
                ActionId = Guid.NewGuid(),
                ActionType = BusinessActionType.StockReduction,
                Status = BusinessActionStatus.Completed,
                Area = DecisionEventArea.Capital,
                EntityType = DecisionEntityType.Product,
                EntityId = entityId,
                EntityName = "SKU-42",
                DecisionEventId = eventId,
                Description = "Liquidar",
                CreatedAt = baseAt.AddDays(i * 10 + 1),
                CompletedAt = baseAt.AddDays(i * 10 + 2),
                ActualImpact = new BusinessActionActualImpact
                {
                    Outcome = BusinessActionOutcome.Partial,
                    Confidence = BusinessActionConfidence.Low,
                    Summary = "obs"
                }
            });
        }

        BusinessActionContextualLearning ctx = learning.GetContextual(minOccurrences: 3);
        Assert.Contains(ctx.Signals, s =>
            s.Kind == BusinessActionLearningSignalKind.RecurrentProblem
            || (s.Message?.Contains("recurrent", StringComparison.OrdinalIgnoreCase) ?? false)
            || (s.Message?.Contains("Recurrente", StringComparison.OrdinalIgnoreCase) ?? false));
    }

    // TEST 11: Acción históricamente efectiva → histórico, NO garantía.
    [Fact]
    public void Test11_Historical_Effectiveness_Is_Hint_Not_Guarantee()
    {
        var records = new List<BusinessActionRecord>();
        Guid e1 = Guid.NewGuid();
        Guid e2 = Guid.NewGuid();
        Guid e3 = Guid.NewGuid();

        foreach (Guid eid in new[] { e1, e2, e3 })
        {
            records.Add(new BusinessActionRecord
            {
                ActionId = Guid.NewGuid(),
                ActionType = BusinessActionType.Promotion,
                Status = BusinessActionStatus.Completed,
                Area = DecisionEventArea.Sales,
                EntityType = DecisionEntityType.Product,
                EntityId = "7",
                EntityName = "Estrella",
                DecisionEventId = eid,
                Description = "Promo",
                CreatedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow,
                ActualImpact = new BusinessActionActualImpact
                {
                    Outcome = BusinessActionOutcome.Successful,
                    Confidence = BusinessActionConfidence.Medium,
                    Summary = "obs"
                }
            });
        }

        var decisionsByEvent = new Dictionary<Guid, DecisionHistoryRecord>
        {
            [e1] = new DecisionHistoryRecord
            {
                EventId = e1, EventType = "sales.decline", EntityId = "7",
                Title = "Caída", DetectedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow,
                Status = DecisionEventStatus.Active, Area = DecisionEventArea.Sales
            },
            [e2] = new DecisionHistoryRecord
            {
                EventId = e2, EventType = "sales.decline", EntityId = "7",
                Title = "Caída", DetectedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow,
                Status = DecisionEventStatus.Active, Area = DecisionEventArea.Sales
            },
            [e3] = new DecisionHistoryRecord
            {
                EventId = e3, EventType = "sales.decline", EntityId = "7",
                Title = "Caída", DetectedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow,
                Status = DecisionEventStatus.Active, Area = DecisionEventArea.Sales
            }
        };

        var byProblem = BusinessActionContextualLearningComposer.ComposeByProblem(records, decisionsByEvent);
        BusinessActionProblemLearningStats problem = Assert.Single(byProblem);
        Assert.NotNull(problem.BestHistoricalHint);
        Assert.Contains("históric", problem.BestHistoricalHint!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no es una garantía", problem.BestHistoricalHint!, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("funcionará", problem.BestHistoricalHint!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("histórica", BusinessActionContextualLearningPolicy.Caution, StringComparison.OrdinalIgnoreCase);
    }

    // TEST 12: Baseline inexistente → no calcular impacto artificial.
    [Fact]
    public void Test12_Missing_Baseline_Does_Not_Invent_Impact()
    {
        var (actions, eval, _, _) = Svc();
        var completed = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);
        var reg = actions.Register(new BusinessActionRegisterRequest
        {
            ActionType = BusinessActionType.MarginReview,
            Description = "Sin baseline",
            StartImmediately = true
        });
        actions.Complete(reg.Record!.ActionId, atUtc: completed, evaluationDays: 7);

        var post = actions.CapturePostMetrics(new BusinessActionPostMetricsRequest
        {
            ActionId = reg.Record.ActionId,
            CapturedAt = completed.AddDays(7),
            MetricValues = new Dictionary<string, decimal?> { ["sales.revenue"] = 999m }
        });
        Assert.True(post.Success);
        Assert.Equal(BusinessActionOutcome.InsufficientData, post.Record!.ActualImpact!.Outcome);
        Assert.Empty(post.Deltas ?? Array.Empty<BusinessActionMetricDelta>());

        var result = eval.Evaluate(new BusinessActionEvaluateRequest
        {
            ActionId = reg.Record.ActionId,
            AsOfUtc = completed.AddDays(7)
        });
        Assert.True(result.Success);
        Assert.Equal(BusinessActionOutcome.InsufficientData, result.Record!.ActualImpact!.Outcome);
        Assert.Null(actions.Get(reg.Record.ActionId)!.Baseline);
    }

    [Fact]
    public void Policy_And_SourceMap()
    {
        Assert.Contains("11.22", BusinessActionBriefPolicy.Definition);
        Assert.Contains("completa", BusinessActionBriefPolicy.Deferred);
        Assert.Contains("SoftLanguageGuard", BusinessActionBriefPolicy.SoftLanguage);
        Assert.Contains("TEST 1", BusinessActionBriefPolicy.Definition);
        Assert.NotNull(DecisionSourceMap.Find("BusinessActionBriefPolicy"));
    }
}
