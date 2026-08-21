using BLL.Models.Crm;
using BLL.Services.Crm;

namespace BLL.Tests;

/// <summary>FASE 10.4 — modelo DecisionEvent + fingerprint.</summary>
public class DecisionEventTests
{
    [Fact]
    public void Catalog_Has_Unique_Codes_And_Covers_Domains()
    {
        var codes = DecisionEventCatalog.All.Select(t => t.Code).ToList();
        Assert.Equal(codes.Count, codes.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.True(codes.Count >= 20);

        Assert.Contains(DecisionEventCatalog.All, t => t.Area == DecisionEventArea.Sales);
        Assert.Contains(DecisionEventCatalog.All, t => t.Area == DecisionEventArea.Capital);
        Assert.Contains(DecisionEventCatalog.All, t => t.Area == DecisionEventArea.Investment);
        Assert.Contains(DecisionEventCatalog.All, t => t.Area == DecisionEventArea.Forecast);
    }

    [Fact]
    public void Fingerprint_Is_Stable_And_Case_Insensitive_On_Ids()
    {
        string a = DecisionFingerprint.Compute(
            DecisionEventArea.Sales, "sales.strong_decline",
            DecisionEntityType.Product, "42", "ThisMonth|2026-08");
        string b = DecisionFingerprint.Compute(
            DecisionEventArea.Sales, "Sales.Strong_Decline",
            DecisionEntityType.Product, " 42 ", "thismonth|2026-08");
        Assert.Equal(a, b);
    }

    [Fact]
    public void Fingerprint_Differs_By_Entity_Or_Period()
    {
        string p1 = DecisionFingerprint.Compute(
            DecisionEventArea.Capital, "capital.at_risk",
            DecisionEntityType.Product, "1", "Last30Days");
        string p2 = DecisionFingerprint.Compute(
            DecisionEventArea.Capital, "capital.at_risk",
            DecisionEntityType.Product, "2", "Last30Days");
        string p3 = DecisionFingerprint.Compute(
            DecisionEventArea.Capital, "capital.at_risk",
            DecisionEntityType.Product, "1", "ThisMonth");
        Assert.NotEqual(p1, p2);
        Assert.NotEqual(p1, p3);
    }

    [Fact]
    public void Duplicate_Active_Same_Fingerprint_Detected()
    {
        var e1 = DecisionEventFactory.Create(
            "sales.strong_decline", DecisionEventArea.Sales,
            DecisionEntityType.Portfolio, null, "",
            "ThisMonth|2026-08", "UnitTest",
            "Caída", "Ingresos ↓");
        var e2 = DecisionEventFactory.Create(
            "sales.strong_decline", DecisionEventArea.Sales,
            DecisionEntityType.Portfolio, null, "",
            "ThisMonth|2026-08", "UnitTest",
            "Caída otra vez", "Ingresos ↓");

        Assert.Equal(e1.Fingerprint, e2.Fingerprint);
        Assert.True(DecisionFingerprint.IsDuplicateActive(e1, e2));
        Assert.NotEqual(e1.EventId, e2.EventId);
    }

    [Fact]
    public void Resolved_Does_Not_Block_As_Duplicate_Active()
    {
        var active = DecisionEventFactory.Create(
            "inv.stockout_risk", DecisionEventArea.Inventory,
            DecisionEntityType.Product, "9", "Prod",
            "asOf:2026-08-20", "UnitTest", "Quiebre", "Stock bajo");
        var resolved = DecisionEventFactory.Create(
            "inv.stockout_risk", DecisionEventArea.Inventory,
            DecisionEntityType.Product, "9", "Prod",
            "asOf:2026-08-20", "UnitTest", "Quiebre", "Stock bajo",
            status: DecisionEventStatus.Resolved);

        Assert.Equal(active.Fingerprint, resolved.Fingerprint);
        Assert.False(DecisionFingerprint.IsDuplicateActive(active, resolved));
    }

    [Fact]
    public void Factory_Leaves_Severity_And_Priority_Unspecified()
    {
        var e = DecisionEventFactory.Create(
            "product.insufficient_data", DecisionEventArea.Product,
            DecisionEntityType.Product, "1", "Nuevo",
            null, "UnitTest", "Datos insuficientes", "New",
            status: DecisionEventStatus.InsufficientData);

        Assert.Equal(DecisionSeverity.Unspecified, e.Severity);
        Assert.Equal(DecisionPriority.Unspecified, e.Priority);
        Assert.Equal(DecisionEventStatus.InsufficientData, e.Status);
        Assert.False(string.IsNullOrWhiteSpace(e.Fingerprint));
    }

    [Fact]
    public void Evidence_Is_Reference_Snapshot_Not_Full_Inventory()
    {
        var e = DecisionEventFactory.Create(
            "capital.at_risk", DecisionEventArea.Capital,
            DecisionEntityType.Product, "7", "X",
            "Last30Days", "UnitTest",
            "Capital en riesgo", "Ventas↓ + stock",
            evidence:
            [
                new DecisionEvidenceFact
                {
                    Key = "revenue_var_pct",
                    Label = "Var. ingresos",
                    ValueText = "-32%",
                    MetricKey = "sales.revenue_var_pct"
                },
                new DecisionEvidenceFact
                {
                    Key = "inventory_capital",
                    Label = "Capital inventario",
                    ValueText = "25000",
                    MetricKey = "capital.inventory"
                }
            ],
            metricKeys: ["sales.revenue_var_pct", "capital.inventory"],
            recommendation: "Revisar estrategia de salida.");

        Assert.Equal(2, e.Evidence.Count);
        Assert.Contains("Revisar", e.Recommendation);
        Assert.All(e.MetricKeys, k => Assert.NotNull(DecisionMetricsCatalog.Find(k)));
    }

    [Fact]
    public void Policy_Defers_Engine_And_Forbids_Auto_Actions()
    {
        Assert.Contains("NUNCA auto-comprar", DecisionEventPolicy.Definition);
        Assert.Contains("no duplicar", DecisionEventPolicy.FingerprintRule);
        Assert.Contains("10.5", DecisionEventPolicy.Deferred);
        Assert.Contains("Revisar/Evaluar", DecisionEventPolicy.SoftLanguage);
    }

    [Fact]
    public void Legacy_Inventory_Signals_Mapped()
    {
        Assert.NotNull(DecisionEventCatalog.Find("capital.frozen"));
        Assert.Contains("FrozenCapital", DecisionEventCatalog.Find("capital.frozen")!.LegacySignal!);
        Assert.NotNull(DecisionEventCatalog.Find("sales.rev_up_profit_down"));
    }
}
