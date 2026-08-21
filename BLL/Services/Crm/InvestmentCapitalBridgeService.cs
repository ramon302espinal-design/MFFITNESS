using BLL.Models.Crm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL.Services.Crm
{
    /// <summary>
    /// Puente FASE 6 ↔ FASE 7 (7.13): capital atrapado por inversión + salud de productos.
    /// No recalcula FIFO; reutiliza InvestmentService + InventoryFinancialService.
    /// </summary>
    public class InvestmentCapitalBridgeService
    {
        private readonly InvestmentService _investments = new();
        private readonly InventoryFinancialService _inventory = new();

        public InvestmentCapitalBridgeReport GetTrappedCapitalReport(
            DateTime? asOf = null,
            int? topInvestments = null)
        {
            InventoryCapitalHealthReport health = _inventory.GetInventoryCapitalHealthReport(asOf);
            Dictionary<int, InventoryFinancialRow> byProduct = _inventory
                .GetInventoryFinancials(null, null, asOf)
                .Rows
                .ToDictionary(r => r.ProductId);

            IReadOnlyList<InvestmentRankRow> ranked = _investments.GetRanking(
                InvestmentRankKind.ByFrozenCapitalDesc,
                top: topInvestments,
                onlyReliable: true);

            var rows = new List<InvestmentTrappedCapitalRow>();
            foreach (InvestmentRankRow inv in ranked.Where(r => r.Summary.FrozenCapital > 0
                                                              || r.Summary.CapitalInvested > 0))
            {
                IReadOnlyList<InvestmentProductRow> products = _investments.GetProducts(inv.Summary.InvestmentId);
                var healthRows = new List<InvestmentProductHealthRow>();
                int frozenCrit = 0;

                foreach (InvestmentProductRow p in products)
                {
                    byProduct.TryGetValue(p.ProductId, out InventoryFinancialRow? invRow);
                    var status = invRow?.HealthStatus ?? InventoryHealthStatus.InsufficientData;
                    if (status is InventoryHealthStatus.Frozen or InventoryHealthStatus.Critical)
                        frozenCrit++;

                    healthRows.Add(new InvestmentProductHealthRow
                    {
                        ProductId = p.ProductId,
                        ProductName = p.ProductName,
                        CapitalAssignedInInvestment = p.CapitalAssigned,
                        ProductInventoryCapital = invRow?.InventoryCapital ?? 0m,
                        HealthStatus = status,
                        IdleDays = invRow?.IdleDays,
                        FlagNeverSold = invRow?.FlagNeverSold ?? false,
                        FlagStockoutRisk = invRow?.FlagStockoutRisk ?? false
                    });
                }

                rows.Add(new InvestmentTrappedCapitalRow
                {
                    Rank = inv.Rank,
                    Summary = inv.Summary,
                    TrappedCapital = inv.Summary.FrozenCapital,
                    ProductsLinked = products.Count,
                    ProductsFrozenOrCritical = frozenCrit,
                    Products = healthRows
                        .OrderByDescending(h => h.CapitalAssignedInInvestment)
                        .ToList()
                });
            }

            // Re-rank only those with trapped > 0 first for the report total focus
            var withTrap = rows
                .OrderByDescending(r => r.TrappedCapital)
                .ThenByDescending(r => r.Summary.CapitalInvested)
                .ToList();
            for (int i = 0; i < withTrap.Count; i++)
            {
                withTrap[i] = new InvestmentTrappedCapitalRow
                {
                    Rank = i + 1,
                    Summary = withTrap[i].Summary,
                    TrappedCapital = withTrap[i].TrappedCapital,
                    ProductsLinked = withTrap[i].ProductsLinked,
                    ProductsFrozenOrCritical = withTrap[i].ProductsFrozenOrCritical,
                    Products = withTrap[i].Products
                };
            }

            return new InvestmentCapitalBridgeReport
            {
                TotalTrappedCapital = InventoryFinancialMath.RoundMoney(
                    withTrap.Sum(r => r.TrappedCapital)),
                GlobalImmobilizedCapital = health.ImmobilizedCapital,
                InvestmentsWithTrappedCapital = withTrap.Count(r => r.TrappedCapital > 0),
                Investments = withTrap
            };
        }
    }
}
