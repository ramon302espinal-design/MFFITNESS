using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 10.3 — mapa fuentes de verdad (SSOT).</summary>
public class DecisionSourceMapTests
{
    [Fact]
    public void Owners_Include_Core_Analytics()
    {
        var names = DecisionSourceMap.Owners.Select(o => o.ServiceName).ToHashSet();
        Assert.Contains("ProfitAnalyticsService", names);
        Assert.Contains("SalesAnalyticsService", names);
        Assert.Contains("InventoryFinancialService", names);
        Assert.Contains("InvestmentService", names);
        Assert.Contains("SalesForecastService", names);
    }

    [Fact]
    public void Metric_Catalog_Sources_Resolve_In_Map()
    {
        var missing = new List<string>();
        foreach (string source in DecisionMetricsCatalog.SourceServices)
        {
            if (DecisionSourceMap.Find(source) == null)
                missing.Add(source);
        }

        Assert.True(missing.Count == 0,
            "Fuentes del catálogo 10.2 sin entrada SSOT: " + string.Join(", ", missing));
    }

    [Fact]
    public void ResolveOwnerForMetric_Sales_Revenue()
    {
        Assert.Equal(
            "SalesAnalyticsService",
            DecisionSourceMap.ResolveOwnerForMetric("sales.revenue"));
    }

    [Fact]
    public void Ui_Consumers_Must_Not_Recalculate()
    {
        var binders = DecisionSourceMap.UiConsumers
            .Where(u => u.ServiceName.Contains("Binder", StringComparison.Ordinal));
        Assert.All(binders, b =>
            Assert.True(
                b.MustNot.Contains("Cálculo", StringComparison.OrdinalIgnoreCase)
                || b.MustNot.Contains("Recalcular", StringComparison.OrdinalIgnoreCase)
                || b.MustNot.Contains("Mutar", StringComparison.OrdinalIgnoreCase)
                || b.MustNot.Contains("Inventar", StringComparison.OrdinalIgnoreCase)
                || b.MustNot.Contains("Score", StringComparison.OrdinalIgnoreCase),
                b.ServiceName + ": " + b.MustNot));
    }

    [Fact]
    public void FrmReportes_Is_Do_Not_Touch()
    {
        var r = DecisionSourceMap.Find("FrmReportes")!;
        Assert.Equal(DecisionSourceRole.UiConsumer, r.Role);
        Assert.Contains("NO TOCAR", r.MustNot);
    }

    [Fact]
    public void Inventory_Alert_Is_Composer_Not_Sales_Owner()
    {
        var a = DecisionSourceMap.Find("InventoryAlertService")!;
        Assert.Equal(DecisionSourceRole.Composer, a.Role);
        Assert.Contains("No ventas", a.MustNot);
    }

    [Fact]
    public void Policy_Forbids_Recalculation_In_Ui()
    {
        Assert.Contains("NO recalculan", DecisionSourcePolicy.Definition);
        Assert.Contains("DecisionEngine", DecisionSourcePolicy.Flow);
        Assert.Contains("consolidar", DecisionSourcePolicy.ExistingSignals);
    }

    [Fact]
    public void Service_Names_Are_Unique()
    {
        var names = DecisionSourceMap.All.Select(s => s.ServiceName).ToList();
        Assert.Equal(names.Count, names.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }
}
