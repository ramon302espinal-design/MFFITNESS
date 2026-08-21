using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>
    /// Puente FASE 6 ↔ FASE 8 (8.16): productos de inversión + Star/Opportunity/Risk + P&L.
    /// </summary>
    public class InvestmentProductPerformanceBridgeService
    {
        private readonly InvestmentService _investments = new();
        private readonly ProductPerformanceService _performance = new();
        private readonly ProductTrendService _trends = new();

        public InvestmentPerformanceBridgeReport GetReport(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            DateTime? asOf = null)
        {
            ProductPerformanceReport perfReport = _performance.GetReport(periodKind, asOf);
            ProductTrendReport trendReport = _trends.GetTrends(periodKind, asOf);

            var perfById = perfReport.Rows.ToDictionary(r => r.ProductId);
            var trendById = trendReport.Rows.ToDictionary(r => r.ProductId);

            var classById = new Dictionary<int, ProductClassificationRow>();
            foreach (ProductPerformanceRow p in perfReport.Rows)
            {
                trendById.TryGetValue(p.ProductId, out ProductTrendRow? tr);
                classById[p.ProductId] = ProductClassificationMath.Classify(p, tr);
            }

            var invRows = new List<InvestmentPerformanceBridgeRow>();
            foreach (Investment inv in _investments.List())
            {
                InvestmentSummary summary = _investments.GetSummary(inv.Id);
                IReadOnlyList<InvestmentProductRow> linked = _investments.GetProducts(inv.Id);
                if (linked.Count == 0)
                    continue;

                var products = new List<InvestmentProductPerformanceRow>(linked.Count);
                foreach (InvestmentProductRow lp in linked)
                {
                    perfById.TryGetValue(lp.ProductId, out ProductPerformanceRow? perf);
                    classById.TryGetValue(lp.ProductId, out ProductClassificationRow? cls);
                    products.Add(InvestmentProductPerformanceComposer.Compose(
                        lp.ProductId,
                        lp.ProductName,
                        lp.CapitalAssigned,
                        cls,
                        perf));
                }

                invRows.Add(InvestmentProductPerformanceComposer.BuildInvestmentRow(
                    summary, products));
            }

            invRows = invRows
                .OrderByDescending(r => r.StarsCount)
                .ThenByDescending(r => r.LinkedPeriodProfit)
                .ThenByDescending(r => r.Summary.CapitalInvested)
                .ToList();

            for (int i = 0; i < invRows.Count; i++)
            {
                var r = invRows[i];
                invRows[i] = new InvestmentPerformanceBridgeRow
                {
                    Rank = i + 1,
                    Summary = r.Summary,
                    ProductsLinked = r.ProductsLinked,
                    StarsCount = r.StarsCount,
                    OpportunityCount = r.OpportunityCount,
                    RiskCount = r.RiskCount,
                    LinkedPeriodProfit = r.LinkedPeriodProfit,
                    Products = r.Products
                };
            }

            return new InvestmentPerformanceBridgeReport
            {
                PeriodKind = periodKind,
                InvestmentCount = invRows.Count,
                TotalStarsAcrossInvestments = invRows.Sum(r => r.StarsCount),
                TotalRisksAcrossInvestments = invRows.Sum(r => r.RiskCount),
                TotalLinkedPeriodProfit = InventoryFinancialMath.RoundMoney(
                    invRows.Sum(r => r.LinkedPeriodProfit)),
                Investments = invRows
            };
        }
    }
}
