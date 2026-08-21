using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 7.13 — puente inversión / capital atrapado.</summary>
public class InvestmentCapitalBridgeTests
{
    [Fact]
    public void Rank_Kind_Trapped_Desc_Exists()
    {
        Assert.Equal(8, (int)InvestmentRankKind.ByFrozenCapitalDesc);
    }

    [Fact]
    public void Policy_Separates_Fifo_From_Classified()
    {
        Assert.Contains("FIFO", InventoryCapitalPolicy.InvestmentBridgeDefinition);
        Assert.Contains("≠", InventoryCapitalPolicy.InvestmentBridgeDefinition);
        Assert.Contains("ByFrozenCapitalDesc", InventoryCapitalPolicy.InvestmentBridgeDefinition);
    }

    [Fact]
    public void Trapped_Is_Not_Same_Concept_As_Global_Immobilized()
    {
        // Documented contract: investment frozen is FIFO pool; global is health-classified.
        Assert.Contains("etiquetadas", InventoryCapitalPolicy.InvestmentBridgeDefinition);
    }
}
