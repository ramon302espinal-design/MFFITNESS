using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 11.23 — Soft language / causalidad (brief §86).</summary>
public class BusinessActionSoftLanguageTests
{
    [Fact]
    public void Accepts_Allowed_Openers()
    {
        Assert.True(BusinessActionSoftLanguageGuard.IsCompliant(
            "Durante el período posterior se observó: ingresos +10%."));
        Assert.True(BusinessActionSoftLanguageGuard.IsCompliant(
            "Después de la acción se observó reducción de capital inmovilizado."));
        Assert.True(BusinessActionSoftLanguageGuard.IsCompliant(
            "Se observó un cambio de margen de +2 pp."));
        Assert.True(BusinessActionSoftLanguageGuard.IsCompliant(
            "Información histórica; no es una garantía futura."));
        Assert.True(BusinessActionSoftLanguageGuard.IsCompliant(
            "Clasificación sugerida: EXITOSA. Sin afirmar causalidad."));
    }

    [Fact]
    public void Rejects_Causal_Claims()
    {
        Assert.True(BusinessActionSoftLanguageGuard.ContainsForbidden(
            "La acción causó todo el crecimiento."));
        Assert.True(BusinessActionSoftLanguageGuard.ContainsForbidden(
            "La acción liberó RD$50,000 de capital."));
        Assert.True(BusinessActionSoftLanguageGuard.ContainsForbidden(
            "Gracias a la acción aumentaron las ventas."));
        Assert.True(BusinessActionSoftLanguageGuard.ContainsForbidden(
            "Debido a la acción el margen subió."));
        Assert.True(BusinessActionSoftLanguageGuard.ContainsForbidden(
            "Garantizamos recuperación total."));
        Assert.False(BusinessActionSoftLanguageGuard.IsCompliant(
            "La acción causó el incremento."));
    }

    [Fact]
    public void EnsureObserved_Rewrites_Forbidden_And_Adds_Opener()
    {
        string fixedText = BusinessActionSoftLanguageGuard.EnsureObserved(
            "La acción causó el crecimiento de ingresos.");
        Assert.False(BusinessActionSoftLanguageGuard.ContainsForbidden(fixedText));
        Assert.True(BusinessActionSoftLanguageGuard.StartsWithSoftOpener(fixedText));
        Assert.DoesNotContain("causó", fixedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EnsureHistoricalHint_Never_Promises_Future()
    {
        string hint = BusinessActionSoftLanguageGuard.EnsureHistoricalHint(
            "Las acciones de tipo Promoción funcionará siempre.");
        Assert.DoesNotContain("funcionará", hint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("garantía", hint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("históric", hint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Evaluation_And_Capital_Narratives_Are_Compliant()
    {
        var deltas = new[]
        {
            BusinessActionMetricDeltaMath.Compute("sales.revenue", null, 100m, 120m)!,
            BusinessActionMetricDeltaMath.Compute("profit.realized", null, 20m, 30m)!
        };

        string observed = BusinessActionMetricDeltaMath.BuildObservedSummary(deltas);
        Assert.True(BusinessActionSoftLanguageGuard.IsCompliant(observed));

        string eval = BusinessActionEvaluationMath.BuildEvaluationSummary(
            BLL.Models.Crm.BusinessActionOutcome.Successful, deltas, fav: 2, unfav: 0);
        Assert.False(BusinessActionSoftLanguageGuard.ContainsForbidden(eval));
        Assert.DoesNotContain("causó", eval, StringComparison.OrdinalIgnoreCase);

        var capital = BusinessActionCapitalImpactComposer.FromDeltas(deltas);
        Assert.False(BusinessActionSoftLanguageGuard.ContainsForbidden(capital.Narrative));
        Assert.DoesNotContain("causó", capital.Narrative, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Policy_And_SourceMap()
    {
        Assert.Contains("11.23", BusinessActionSoftLanguagePolicy.Definition);
        Assert.Contains("completa", BusinessActionSoftLanguagePolicy.Deferred);
        Assert.Contains("Se observó", BusinessActionSoftLanguagePolicy.AllowedOpeners);
        Assert.Contains("causó", BusinessActionSoftLanguagePolicy.Forbidden);
        Assert.NotNull(DecisionSourceMap.Find("BusinessActionSoftLanguageGuard"));
        Assert.Contains("11.23", BusinessActionPolicy.Causality);
        Assert.Contains("completa", BusinessActionPolicy.Deferred);
    }
}
