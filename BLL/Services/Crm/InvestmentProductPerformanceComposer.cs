using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Composición pura inversión↔performance (FASE 8.16).</summary>
    public static class InvestmentProductPerformanceComposer
    {
        public static InvestmentProductPerformanceRow Compose(
            int productId,
            string productName,
            decimal capitalAssigned,
            ProductClassificationRow? classification,
            ProductPerformanceRow? performance)
        {
            return new InvestmentProductPerformanceRow
            {
                ProductId = productId,
                ProductName = productName,
                CapitalAssignedInInvestment = capitalAssigned,
                Class = classification?.Class ?? ProductPerformanceClass.InsufficientData,
                Reasons = classification?.Reasons ?? Array.Empty<string>(),
                UnitsSold = performance?.UnitsSold ?? 0,
                RevenueTotal = performance?.RevenueTotal ?? 0m,
                RealizedProfit = performance?.RealizedProfit ?? 0m,
                MarginPct = performance?.MarginPct,
                RoiPct = performance?.RoiPct,
                ProductInventoryCapital = performance?.InventoryCapital ?? 0m
            };
        }

        public static InvestmentPerformanceBridgeRow BuildInvestmentRow(
            InvestmentSummary summary,
            IReadOnlyList<InvestmentProductPerformanceRow> products,
            int rank = 0)
        {
            return new InvestmentPerformanceBridgeRow
            {
                Rank = rank,
                Summary = summary,
                ProductsLinked = products.Count,
                StarsCount = products.Count(p => p.IsStar),
                OpportunityCount = products.Count(p => p.IsOpportunity),
                RiskCount = products.Count(p => p.IsRisk),
                LinkedPeriodProfit = InventoryFinancialMath.RoundMoney(
                    products.Sum(p => p.RealizedProfit)),
                Products = products
                    .OrderByDescending(p => p.IsStar)
                    .ThenByDescending(p => p.RealizedProfit)
                    .ThenByDescending(p => p.CapitalAssignedInInvestment)
                    .ToList()
            };
        }
    }
}
