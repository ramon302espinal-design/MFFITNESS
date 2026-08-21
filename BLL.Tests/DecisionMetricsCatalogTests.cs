using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 10.2 — mapa de métricas disponibles.</summary>
public class DecisionMetricsCatalogTests
{
    [Fact]
    public void Catalog_Is_Non_Empty_And_Unique_Keys()
    {
        Assert.True(DecisionMetricsCatalog.All.Count >= 40);
        var keys = DecisionMetricsCatalog.All.Select(m => m.Key).ToList();
        Assert.Equal(keys.Count, keys.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void Separation_Covered_Sales_Vs_Units_Vs_Revenue()
    {
        Assert.NotNull(DecisionMetricsCatalog.Find("sales.revenue"));
        Assert.NotNull(DecisionMetricsCatalog.Find("sales.units"));
        Assert.NotNull(DecisionMetricsCatalog.Find("sales.transactions"));
        Assert.NotEqual(
            DecisionMetricsCatalog.Find("sales.revenue")!.Notes,
            DecisionMetricsCatalog.Find("sales.transactions")!.Notes);
    }

    [Fact]
    public void Capital_Kinds_Are_Separated()
    {
        var inv = DecisionMetricsCatalog.Find("capital.inventory")!;
        var imm = DecisionMetricsCatalog.Find("capital.immobilized")!;
        var frz = DecisionMetricsCatalog.Find("invst.frozen_capital")!;
        Assert.Contains("PrecioCompra", inv.Notes);
        Assert.Contains("Frozen", imm.Notes);
        Assert.Contains("≠ InventoryCapital", frz.Notes);
    }

    [Fact]
    public void Forecast_Is_Estimate_Not_Certainty()
    {
        var f = DecisionMetricsCatalog.Find("forecast.base_revenue")!;
        Assert.Contains("ESTIMACIÓN", f.Notes);
        Assert.Contains("certeza", f.Notes);
    }

    [Fact]
    public void Comparable_Metrics_Flagged()
    {
        var varRev = DecisionMetricsCatalog.Find("sales.revenue_var_pct")!;
        Assert.True(varRev.RequiresComparablePeriod);
        Assert.False(DecisionMetricsCatalog.Find("sales.revenue")!.RequiresComparablePeriod);
    }

    [Fact]
    public void Source_Services_Include_Canonical_Ssot()
    {
        var sources = DecisionMetricsCatalog.SourceServices;
        Assert.Contains("SalesAnalyticsService", sources);
        Assert.Contains("ProfitAnalyticsService", sources);
        Assert.Contains("InventoryFinancialService", sources);
        Assert.Contains("InvestmentService", sources);
        Assert.Contains("ProductClassificationService", sources);
        Assert.Contains("SalesForecastService", sources);
    }

    [Fact]
    public void ByArea_Sales_Has_Variation()
    {
        var sales = DecisionMetricsCatalog.ByArea(DecisionMetricArea.Sales);
        Assert.Contains(sales, m => m.Key == "sales.revenue_var_pct");
    }

    [Fact]
    public void Policy_No_Engine_Yet()
    {
        Assert.Contains("NO recalcular", DecisionMetricsPolicy.Definition);
        Assert.Contains("10.8+", DecisionMetricsPolicy.NoEngineYet);
        Assert.Contains("InventoryCapital ≠ ImmobilizedCapital", DecisionMetricsPolicy.Separation);
    }
}
