using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 8.16 — puente inversión / performance de productos.</summary>
public class InvestmentProductPerformanceBridgeTests
{
    [Fact]
    public void Compose_Marks_Star_And_Profit()
    {
        var classification = new ProductClassificationRow
        {
            ProductId = 1,
            ProductName = "P1",
            Class = ProductPerformanceClass.Star,
            Reasons = new[] { "Impacto", "Eficiencia", "Bajo riesgo" }
        };
        var perf = ProductPerformanceComposer.Compose(
            new ProfitGroupRow
            {
                ProductId = 1,
                ProductName = "P1",
                GroupName = "P1",
                UnitsSold = 50,
                RevenueTotal = 100_000m,
                RealizedProfit = 35_000m,
                HasReliableRealizedProfit = true
            },
            null);

        var row = InvestmentProductPerformanceComposer.Compose(
            1, "P1", capitalAssigned: 20_000m, classification, perf);

        Assert.True(row.IsStar);
        Assert.False(row.IsRisk);
        Assert.Equal(20_000m, row.CapitalAssignedInInvestment);
        Assert.Equal(35_000m, row.RealizedProfit);
    }

    [Fact]
    public void BuildInvestmentRow_Counts_Stars_Risks_Profit()
    {
        var summary = new InvestmentSummary
        {
            InvestmentId = 9,
            Name = "Lote A",
            CapitalInvested = 100_000m,
            FrozenCapital = 10_000m
        };

        var products = new[]
        {
            new InvestmentProductPerformanceRow
            {
                ProductId = 1,
                ProductName = "Star",
                Class = ProductPerformanceClass.Star,
                RealizedProfit = 20_000m,
                CapitalAssignedInInvestment = 30_000m
            },
            new InvestmentProductPerformanceRow
            {
                ProductId = 2,
                ProductName = "Risk",
                Class = ProductPerformanceClass.Critical,
                RealizedProfit = -500m,
                CapitalAssignedInInvestment = 40_000m
            },
            new InvestmentProductPerformanceRow
            {
                ProductId = 3,
                ProductName = "Opp",
                Class = ProductPerformanceClass.Opportunity,
                RealizedProfit = 3_000m,
                CapitalAssignedInInvestment = 5_000m
            }
        };

        var row = InvestmentProductPerformanceComposer.BuildInvestmentRow(summary, products, rank: 1);
        Assert.Equal(1, row.StarsCount);
        Assert.Equal(1, row.OpportunityCount);
        Assert.Equal(1, row.RiskCount);
        Assert.Equal(22_500m, row.LinkedPeriodProfit);
        Assert.Equal("Star", row.Products[0].ProductName);
    }

    [Fact]
    public void Policy_Separates_Assigned_From_Global_Capital()
    {
        Assert.Contains("≠", InvestmentProductPerformancePolicy.Definition);
        Assert.Contains("FIFO", InvestmentProductPerformancePolicy.Definition);
        Assert.Contains("StarsCount", InvestmentProductPerformancePolicy.Question);
        Assert.Contains("No recalcula FIFO", InvestmentProductPerformancePolicy.NoRecalcFifo);
    }
}
