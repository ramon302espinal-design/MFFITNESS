using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 7.12 — rankings de capital (contrato de criterios).</summary>
public class InventoryCapitalRankingTests
{
    [Fact]
    public void Rank_Kinds_Are_Distinct()
    {
        Assert.NotEqual(
            InventoryCapitalRankKind.ByTurnoverProxyDesc,
            InventoryCapitalRankKind.ByTurnoverProxyAsc);
        Assert.NotEqual(
            InventoryCapitalRankKind.ByImmobilizedCapitalDesc,
            InventoryCapitalRankKind.ByInventoryCapitalDesc);
    }

    [Fact]
    public void Policy_Rejects_Universal_Ranking()
    {
        Assert.Contains("no hay un único ranking", InventoryCapitalPolicy.RankingDefinition);
        Assert.Contains("AtRisk", InventoryCapitalPolicy.RankingDefinition);
    }

    [Fact]
    public void All_Brief_Criteria_Exist()
    {
        // Mayor capital, inmovilizado, riesgo, cobertura, rotación ↑↓, velocidad
        var kinds = Enum.GetValues<InventoryCapitalRankKind>();
        Assert.Contains(InventoryCapitalRankKind.ByInventoryCapitalDesc, kinds);
        Assert.Contains(InventoryCapitalRankKind.ByImmobilizedCapitalDesc, kinds);
        Assert.Contains(InventoryCapitalRankKind.ByAtRiskCapitalDesc, kinds);
        Assert.Contains(InventoryCapitalRankKind.ByDaysOfCoverDesc, kinds);
        Assert.Contains(InventoryCapitalRankKind.ByTurnoverProxyDesc, kinds);
        Assert.Contains(InventoryCapitalRankKind.ByTurnoverProxyAsc, kinds);
    }
}
