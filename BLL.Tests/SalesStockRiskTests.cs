using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 9.18 — riesgo de quiebre / señales stock↔ventas.</summary>
public class SalesStockRiskTests
{
    [Fact]
    public void Demand_Exceeds_Stock_Is_Stockout()
    {
        // vel 2 uds/día × 30 = 60 > stock 10
        var row = SalesStockRiskMath.Classify(
            1, "Whey", "Prot",
            stock: 10, stockMinimo: 5,
            unitsPerDay: 2m, turnoverProxy: 1m,
            flagStockoutRisk: false, flagOverstock: false, isImmobilized: false);

        Assert.Equal(SalesStockSignalKind.StockoutRisk, row.PrimarySignal);
        Assert.True(row.DemandExceedsStock);
        Assert.Equal(60m, row.ProjectedDemandUnits);
        Assert.Contains("QUIEBRE", row.DisplayLabel);
    }

    [Fact]
    public void Fase7_FlagStockoutRisk_Maps()
    {
        var row = SalesStockRiskMath.Classify(
            1, "Creatina", "Prot",
            stock: 3, stockMinimo: 5,
            unitsPerDay: 0.2m, turnoverProxy: null,
            flagStockoutRisk: true, flagOverstock: false, isImmobilized: false);

        Assert.Contains(SalesStockSignalKind.StockoutRisk, row.Signals);
        Assert.True(row.FlagStockoutRisk);
    }

    [Fact]
    public void Replenishment_Growing_Low_Stock_High_Velocity()
    {
        var row = SalesStockRiskMath.Classify(
            1, "Barrita", "Snack",
            stock: 4, stockMinimo: 10,
            unitsPerDay: 1.5m, turnoverProxy: 2m,
            flagStockoutRisk: true, flagOverstock: false, isImmobilized: false,
            trend: ProductTrendDirection.Growing);

        Assert.Contains(SalesStockSignalKind.ReplenishmentOpportunity, row.Signals);
        Assert.Contains("NO compra automática", row.Reason);
        // Primary sigue siendo quiebre (prioridad)
        Assert.Equal(SalesStockSignalKind.StockoutRisk, row.PrimarySignal);
    }

    [Fact]
    public void Capital_Risk_Declining_Overstock()
    {
        var row = SalesStockRiskMath.Classify(
            1, "Lento", "Acc",
            stock: 200, stockMinimo: 5,
            unitsPerDay: 1m, turnoverProxy: 0.1m,
            flagStockoutRisk: false, flagOverstock: true, isImmobilized: true,
            trend: ProductTrendDirection.Declining);

        Assert.Equal(SalesStockSignalKind.CapitalRisk, row.PrimarySignal);
        Assert.Contains("CAPITAL", row.DisplayLabel);
    }

    [Fact]
    public void Healthy_Growth()
    {
        // stock 30 / 1 ud/día = 30d cobertura saludable
        var row = SalesStockRiskMath.Classify(
            1, "Estrella", "Prot",
            stock: 30, stockMinimo: 5,
            unitsPerDay: 1m, turnoverProxy: 1m,
            flagStockoutRisk: false, flagOverstock: false, isImmobilized: false,
            trend: ProductTrendDirection.Growing);

        Assert.Equal(SalesStockSignalKind.HealthyGrowth, row.PrimarySignal);
        Assert.Equal(30m, row.DaysOfCover);
    }

    [Fact]
    public void Cover_Reuses_Fase7_Math()
    {
        Assert.Equal(
            InventoryFinancialMath.DaysOfCover(60, 2m),
            SalesStockRiskMath.Classify(
                1, "X", "", 60, 0, 2m, null, false, false, false).DaysOfCover);
    }

    [Fact]
    public void Compose_Counts_And_Policy()
    {
        var rows = new[]
        {
            SalesStockRiskMath.Classify(1, "A", "", 5, 10, 2m, 1m, true, false, false,
                ProductTrendDirection.Growing),
            SalesStockRiskMath.Classify(2, "B", "", 30, 5, 1m, 1m, false, false, false,
                ProductTrendDirection.Growing)
        };

        var report = SalesStockRiskMath.Compose(rows, ProfitPeriodKind.ThisMonth);
        Assert.True(report.StockoutRiskCount >= 1);
        Assert.True(report.HealthyGrowthCount >= 1);
        Assert.Contains("NO ejecutar compra automática", report.PolicyNote);
    }

    [Fact]
    public void Policy_Integrates_Fase7()
    {
        Assert.Contains("DaysOfCover", SalesStockRiskPolicy.Definition);
        Assert.Contains("FlagStockoutRisk", SalesStockRiskPolicy.Stockout);
        Assert.Contains("NO ejecutar compra automática", SalesStockRiskPolicy.Replenishment);
        Assert.Contains("congelado", SalesStockRiskPolicy.Capital);
    }
}
