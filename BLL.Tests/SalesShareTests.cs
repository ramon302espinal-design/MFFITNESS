using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 9.12 — participación y concentración.</summary>
public class SalesShareTests
{
    [Fact]
    public void Brief_Example_20_Percent_Share()
    {
        var report = SalesShareComposer.FromAmounts(
            new[]
            {
                ("A", (int?)1, (int?)null, 100_000m),
                ("B", 2, null, 400_000m)
            },
            SalesShareMetric.Revenue,
            ProfitPeriodKind.ThisMonth,
            topN: 1);

        Assert.Equal(20.00m, report.Items.First(i => i.Name == "A").SharePct);
        Assert.Equal(80.00m, report.Items.First(i => i.Name == "B").SharePct);
        Assert.Equal("B", report.Items[0].Name);
    }

    [Fact]
    public void Top5_Concentration_70_Percent()
    {
        var items = new List<(string, int?, int?, decimal)>();
        for (int i = 1; i <= 5; i++)
            items.Add(($"T{i}", i, null, 14_000m)); // 5×14k = 70k
        for (int i = 6; i <= 10; i++)
            items.Add(($"O{i}", i, null, 6_000m)); // 5×6k = 30k

        var report = SalesShareComposer.FromAmounts(
            items, SalesShareMetric.Revenue, ProfitPeriodKind.Last30Days, topN: 5);

        Assert.Equal(70.00m, report.TopNSharePct);
        Assert.Equal(5, report.TopN);
        Assert.Equal(100.00m, report.Items[^1].CumulativeSharePct);
    }

    [Fact]
    public void Zero_Total_No_Share()
    {
        var report = SalesShareComposer.FromAmounts(
            Array.Empty<(string, int?, int?, decimal)>(),
            SalesShareMetric.Revenue,
            ProfitPeriodKind.Today);

        Assert.Equal(0, report.ItemCount);
        Assert.Null(report.TopNSharePct);
    }

    [Fact]
    public void Metric_Label_Separates_Revenue_And_Profit()
    {
        var products = new[]
        {
            new SalesProductRow { ProductId = 1, ProductName = "X", RevenueTotal = 10m, RealizedProfit = 90m, UnitsSold = 1 }
        };

        var byRev = SalesShareComposer.FromProducts(products, SalesShareMetric.Revenue, ProfitPeriodKind.ThisMonth);
        var byProfit = SalesShareComposer.FromProducts(products, SalesShareMetric.RealizedProfit, ProfitPeriodKind.ThisMonth);

        Assert.Equal("Ingresos", byRev.MetricLabel);
        Assert.Equal("Ganancia", byProfit.MetricLabel);
        Assert.Equal(10m, byRev.TotalAmount);
        Assert.Equal(90m, byProfit.TotalAmount);
    }

    [Fact]
    public void Policy_Documents_Concentration()
    {
        Assert.Contains("20%", SalesSharePolicy.Definition);
        Assert.Contains("Top N", SalesSharePolicy.Concentration);
        Assert.Contains("9.13", SalesSharePolicy.Concentration);
        Assert.Contains("No mezclar", SalesSharePolicy.Metrics);
    }
}
