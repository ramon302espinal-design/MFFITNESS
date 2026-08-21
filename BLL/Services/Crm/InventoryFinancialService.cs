using BLL.Models.Crm;
using DL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BLL.Services.Crm
{
    /// <summary>
    /// Capa financiera de inventario (FASE 4.5).
    /// UI no calcula: consume este servicio.
    /// </summary>
    public class InventoryFinancialService
    {
        private readonly CrmInventoryFinancialDAL dal = new();

        public InventoryFinancialSummary GetInventoryFinancials(
            DateTime? salesFrom = null,
            DateTime? salesToExclusive = null,
            DateTime? asOf = null,
            int? velocityWindowDays = null)
        {
            DateTime refDate = (asOf ?? DateTime.Today).Date;
            int windowDays = velocityWindowDays is > 0
                ? velocityWindowDays.Value
                : InventoryFinancialMath.DefaultVelocityWindowDays;
            DateTime velocityFrom = refDate.AddDays(-(windowDays - 1));
            DateTime velocityToExclusive = refDate.AddDays(1);

            DataTable table = dal.ObtenerBaseFinanciera(
                salesFrom, salesToExclusive, velocityFrom, velocityToExclusive);

            var rows = new List<InventoryFinancialRow>(table.Rows.Count);
            foreach (DataRow raw in table.Rows)
                rows.Add(MapRow(raw, refDate, windowDays));

            return BuildSummary(rows, windowDays);
        }

        public InventoryFinancialSummary GetInventoryFinancials()
            => GetInventoryFinancials(null, null, null, null);

        /// <summary>
        /// Capital en inventario (FASE 7.2) + ranking por producto.
        /// Excluye stock≤0 y costo no definido. No es capital congelado clasificado (7.9).
        /// </summary>
        public InventoryCapitalReport GetInventoryCapitalReport(DateTime? asOf = null, int? top = null)
        {
            InventoryFinancialSummary full = GetInventoryFinancials(null, null, asOf);

            var withCapital = full.Rows
                .Where(r => r.Stock > 0 && !r.FlagNoCost && r.InventoryCapital > 0)
                .OrderByDescending(r => r.InventoryCapital)
                .ThenBy(r => r.ProductName)
                .ToList();

            decimal total = RoundMoney(withCapital.Sum(r => r.InventoryCapital));

            IEnumerable<InventoryFinancialRow> source = withCapital;
            if (top.HasValue && top.Value > 0)
                source = withCapital.Take(top.Value);

            int rank = 0;
            var items = source.Select(r =>
            {
                rank++;
                return new InventoryCapitalItem
                {
                    Rank = rank,
                    ProductId = r.ProductId,
                    ProductName = r.ProductName,
                    Category = r.Category,
                    Stock = r.Stock,
                    UnitCost = r.UnitCost,
                    InventoryCapital = r.InventoryCapital,
                    SharePct = total > 0
                        ? RoundPct(r.InventoryCapital / total * 100m)
                        : null,
                    DaysWithoutSale = r.DaysWithoutSale,
                    FlagNoRotation = r.FlagNoRotation,
                    FlagNoCost = r.FlagNoCost
                };
            }).ToList();

            return new InventoryCapitalReport
            {
                TotalInventoryCapital = total,
                ProductsWithInventoryCapital = withCapital.Count,
                ProductsExcludedNoCostWithStock = full.Rows.Count(r => r.Stock > 0 && r.FlagNoCost),
                ProductsExcludedNoStock = full.Rows.Count(r => r.Stock <= 0),
                ProductsNegativeStock = full.ProductsNegativeStock,
                Items = items
            };
        }

        public decimal GetInventoryCapitalTotal(DateTime? asOf = null)
            => GetInventoryCapitalReport(asOf).TotalInventoryCapital;

        /// <summary>
        /// Buckets de capital por salud (FASE 7.9).
        /// </summary>
        public InventoryCapitalHealthReport GetInventoryCapitalHealthReport(
            DateTime? asOf = null,
            int? frozenTop = null)
        {
            InventoryFinancialSummary full = GetInventoryFinancials(null, null, asOf);
            FrozenCapitalReport frozen = BuildFrozenReport(full, frozenTop);

            return new InventoryCapitalHealthReport
            {
                InventoryCapitalTotal = full.InventoryCapitalTotal,
                HealthyCapital = full.HealthyCapital,
                SlowCapital = full.SlowCapital,
                NewCapital = full.NewCapital,
                FrozenStatusCapital = full.FrozenStatusCapital,
                CriticalCapital = full.CriticalCapital,
                ImmobilizedCapital = full.FrozenCapitalTotal,
                ImmobilizedSharePct = full.FrozenSharePct,
                Frozen = frozen
            };
        }

        /// <summary>
        /// Capital congelado clasificado (FASE 7.9): solo Frozen + Critical.
        /// </summary>
        public FrozenCapitalReport GetFrozenCapitalReport(DateTime? asOf = null, int? top = null)
        {
            InventoryFinancialSummary full = GetInventoryFinancials(null, null, asOf);
            return BuildFrozenReport(full, top);
        }

        /// <summary>Σ capital Frozen + Critical.</summary>
        public decimal GetFrozenCapitalTotal(DateTime? asOf = null)
            => GetFrozenCapitalReport(asOf).TotalFrozenCapital;

        /// <summary>
        /// Capital en riesgo + liberable + simulaciones de descuento (FASE 7.10).
        /// No modifica precios ni stock.
        /// </summary>
        public InventoryRiskReport GetInventoryRiskReport(DateTime? asOf = null)
        {
            InventoryFinancialSummary full = GetInventoryFinancials(null, null, asOf);

            var atRiskRows = full.Rows
                .Where(IsAtRiskRow)
                .Where(r => r.InventoryCapital > 0)
                .ToList();

            decimal atRiskCapital = RoundMoney(atRiskRows.Sum(r => r.InventoryCapital));
            decimal liberableSales = RoundMoney(atRiskRows.Sum(r => r.PotentialSalesValue));
            decimal inventoryTotal = full.InventoryCapitalTotal;
            decimal immobilized = full.FrozenCapitalTotal;

            var scenarios = InventoryFinancialMath.DefaultLiquidationDiscounts
                .Select(d => InventoryFinancialMath.SimulateLiquidationFromTotals(
                    atRiskCapital, liberableSales, d))
                .ToList();

            return new InventoryRiskReport
            {
                InventoryCapitalTotal = inventoryTotal,
                ImmobilizedCapital = immobilized,
                AtRiskCapital = atRiskCapital,
                AtRiskShareOfInventoryPct = InventoryFinancialMath.FrozenShareOfInventoryPct(
                    atRiskCapital, inventoryTotal),
                AtRiskShareOfImmobilizedPct = InventoryFinancialMath.FrozenShareOfInventoryPct(
                    atRiskCapital, immobilized),
                LiberableSalesValueAtList = liberableSales,
                LiberableCapitalAtCost = atRiskCapital,
                ProductsAtRisk = atRiskRows.Count,
                LiquidationScenarios = scenarios
            };
        }

        /// <summary>
        /// En riesgo: Critical, o Frozen con ganancia potencial negativa.
        /// Congelado sin pérdida latente no cuenta automáticamente como riesgo.
        /// </summary>
        private static bool IsAtRiskRow(InventoryFinancialRow r)
        {
            if (r.HealthStatus == InventoryHealthStatus.Critical)
                return true;

            if (r.HealthStatus == InventoryHealthStatus.Frozen && r.PotentialProfit < 0)
                return true;

            return false;
        }

        /// <summary>
        /// Rankings de capital (FASE 7.12). Criterios separados; sin ranking universal.
        /// </summary>
        public IReadOnlyList<InventoryCapitalRankRow> GetCapitalRanking(
            InventoryCapitalRankKind kind,
            int top = 10,
            DateTime? asOf = null)
        {
            if (top <= 0)
                top = 10;

            InventoryFinancialSummary full = GetInventoryFinancials(null, null, asOf);
            IEnumerable<InventoryFinancialRow> source = full.Rows.Where(r => r.Stock > 0);

            source = kind switch
            {
                InventoryCapitalRankKind.ByImmobilizedCapitalDesc =>
                    source.Where(r => r.HealthStatus is InventoryHealthStatus.Frozen
                        or InventoryHealthStatus.Critical),
                InventoryCapitalRankKind.ByAtRiskCapitalDesc =>
                    source.Where(IsAtRiskRow),
                InventoryCapitalRankKind.ByDaysOfCoverDesc =>
                    source.Where(r => r.DaysOfCover.HasValue),
                InventoryCapitalRankKind.ByTurnoverProxyDesc =>
                    source.Where(r => r.TurnoverProxy.HasValue),
                InventoryCapitalRankKind.ByTurnoverProxyAsc =>
                    source.Where(r => r.TurnoverProxy.HasValue && r.InventoryCapital > 0),
                InventoryCapitalRankKind.ByUnitsPerDayDesc =>
                    source.Where(r => r.UnitsPerDay.HasValue && r.UnitsPerDay.Value > 0),
                InventoryCapitalRankKind.ByIdleDaysDesc =>
                    source.Where(r => r.IdleDays.HasValue),
                InventoryCapitalRankKind.ByPotentialProfitDesc =>
                    source.Where(r => r.PotentialProfit != 0 || r.InventoryCapital > 0),
                _ => source.Where(r => r.InventoryCapital > 0)
            };

            IOrderedEnumerable<InventoryFinancialRow> ordered = kind switch
            {
                InventoryCapitalRankKind.ByInventoryCapitalDesc =>
                    source.OrderByDescending(r => r.InventoryCapital).ThenBy(r => r.ProductName),
                InventoryCapitalRankKind.ByImmobilizedCapitalDesc =>
                    source.OrderByDescending(r => r.InventoryCapital).ThenBy(r => r.ProductName),
                InventoryCapitalRankKind.ByAtRiskCapitalDesc =>
                    source.OrderByDescending(r => r.InventoryCapital).ThenBy(r => r.ProductName),
                InventoryCapitalRankKind.ByIdleDaysDesc =>
                    source.OrderByDescending(r => r.IdleDays).ThenByDescending(r => r.InventoryCapital)
                        .ThenBy(r => r.ProductName),
                InventoryCapitalRankKind.ByDaysOfCoverDesc =>
                    source.OrderByDescending(r => r.DaysOfCover).ThenByDescending(r => r.InventoryCapital)
                        .ThenBy(r => r.ProductName),
                InventoryCapitalRankKind.ByTurnoverProxyDesc =>
                    source.OrderByDescending(r => r.TurnoverProxy).ThenBy(r => r.ProductName),
                InventoryCapitalRankKind.ByTurnoverProxyAsc =>
                    source.OrderBy(r => r.TurnoverProxy).ThenByDescending(r => r.InventoryCapital)
                        .ThenBy(r => r.ProductName),
                InventoryCapitalRankKind.ByUnitsPerDayDesc =>
                    source.OrderByDescending(r => r.UnitsPerDay).ThenBy(r => r.ProductName),
                InventoryCapitalRankKind.ByPotentialProfitDesc =>
                    source.OrderByDescending(r => r.PotentialProfit).ThenBy(r => r.ProductName),
                _ => source.OrderByDescending(r => r.InventoryCapital).ThenBy(r => r.ProductName)
            };

            int rank = 0;
            return ordered.Take(top).Select(r =>
            {
                rank++;
                var (label, value) = FormatRankMetric(kind, r);
                return new InventoryCapitalRankRow
                {
                    Rank = rank,
                    Kind = kind,
                    Row = r,
                    MetricLabel = label,
                    MetricValue = value
                };
            }).ToList();
        }

        private static (string Label, decimal? Value) FormatRankMetric(
            InventoryCapitalRankKind kind,
            InventoryFinancialRow r)
            => kind switch
            {
                InventoryCapitalRankKind.ByInventoryCapitalDesc =>
                    ($"Capital {r.InventoryCapital:N2}", r.InventoryCapital),
                InventoryCapitalRankKind.ByImmobilizedCapitalDesc =>
                    ($"Inmovilizado {r.InventoryCapital:N2} ({r.HealthStatus})", r.InventoryCapital),
                InventoryCapitalRankKind.ByAtRiskCapitalDesc =>
                    ($"Riesgo {r.InventoryCapital:N2}", r.InventoryCapital),
                InventoryCapitalRankKind.ByIdleDaysDesc =>
                    ($"Idle {r.IdleDays} d", r.IdleDays),
                InventoryCapitalRankKind.ByDaysOfCoverDesc =>
                    ($"Cobertura {r.DaysOfCover:N1} d", r.DaysOfCover),
                InventoryCapitalRankKind.ByTurnoverProxyDesc =>
                    ($"Turnover proxy {r.TurnoverProxy:N2}", r.TurnoverProxy),
                InventoryCapitalRankKind.ByTurnoverProxyAsc =>
                    ($"Turnover proxy {r.TurnoverProxy:N2}", r.TurnoverProxy),
                InventoryCapitalRankKind.ByUnitsPerDayDesc =>
                    ($"{r.UnitsPerDay:N2} uds/día", r.UnitsPerDay),
                InventoryCapitalRankKind.ByPotentialProfitDesc =>
                    ($"Potencial {r.PotentialProfit:N2}", r.PotentialProfit),
                _ => ($"Capital {r.InventoryCapital:N2}", r.InventoryCapital)
            };

        private static FrozenCapitalReport BuildFrozenReport(InventoryFinancialSummary full, int? top)
        {
            var immobilized = full.Rows
                .Where(r => r.HealthStatus is InventoryHealthStatus.Frozen or InventoryHealthStatus.Critical)
                .Where(r => r.InventoryCapital > 0)
                .OrderByDescending(r => r.InventoryCapital)
                .ThenBy(r => r.ProductName)
                .ToList();

            decimal frozenTotal = RoundMoney(immobilized.Sum(r => r.InventoryCapital));
            decimal inventoryTotal = full.InventoryCapitalTotal;

            IEnumerable<InventoryFinancialRow> source = immobilized;
            if (top.HasValue && top.Value > 0)
                source = immobilized.Take(top.Value);

            int rank = 0;
            var items = source.Select(r =>
            {
                rank++;
                return new FrozenCapitalItem
                {
                    Rank = rank,
                    ProductId = r.ProductId,
                    ProductName = r.ProductName,
                    Category = r.Category,
                    Stock = r.Stock,
                    UnitCost = r.UnitCost,
                    FrozenCapital = r.InventoryCapital,
                    HealthStatus = r.HealthStatus,
                    SharePct = frozenTotal > 0
                        ? RoundPct(r.InventoryCapital / frozenTotal * 100m)
                        : null,
                    IdleDays = r.IdleDays,
                    DaysWithoutSale = r.DaysWithoutSale,
                    FlagNoRotation = r.FlagNoRotation,
                    FlagNoCost = r.FlagNoCost,
                    FlagNeverSold = r.FlagNeverSold
                };
            }).ToList();

            return new FrozenCapitalReport
            {
                TotalFrozenCapital = frozenTotal,
                TotalInventoryCapital = inventoryTotal,
                FrozenSharePct = InventoryFinancialMath.FrozenShareOfInventoryPct(frozenTotal, inventoryTotal),
                FrozenStatusCapital = full.FrozenStatusCapital,
                CriticalCapital = full.CriticalCapital,
                ProductsWithFrozenCapital = immobilized.Count,
                ProductsExcludedNoCostWithStock = full.Rows.Count(r => r.Stock > 0 && r.FlagNoCost),
                ProductsExcludedNoStock = full.Rows.Count(r => r.Stock <= 0),
                ProductsNegativeStock = full.ProductsNegativeStock,
                Items = items
            };
        }

        /// <summary>
        /// Valor potencial de venta + ganancia potencial (FASE 4.7).
        /// Ganancia potencial solo cuando hay stock, costo y precio válidos.
        /// Nunca se presenta como ganancia realizada.
        /// </summary>
        public PotentialValueReport GetPotentialValueReport(DateTime? asOf = null, int? top = null)
        {
            InventoryFinancialSummary full = GetInventoryFinancials(null, null, asOf);

            var eligible = full.Rows
                .Where(r => r.Stock > 0 && !r.FlagNoCost && !r.FlagNoPrice)
                .OrderByDescending(r => r.PotentialProfit)
                .ThenByDescending(r => r.PotentialSalesValue)
                .ThenBy(r => r.ProductName)
                .ToList();

            decimal totalProfit = RoundMoney(eligible.Sum(r => r.PotentialProfit));
            decimal totalSalesValue = RoundMoney(eligible.Sum(r => r.PotentialSalesValue));
            decimal totalFrozen = RoundMoney(eligible.Sum(r => r.InventoryCapital));

            IEnumerable<InventoryFinancialRow> source = eligible;
            if (top.HasValue && top.Value > 0)
                source = eligible.Take(top.Value);

            int rank = 0;
            var items = source.Select(r =>
            {
                rank++;
                return new PotentialValueItem
                {
                    Rank = rank,
                    ProductId = r.ProductId,
                    ProductName = r.ProductName,
                    Category = r.Category,
                    Stock = r.Stock,
                    UnitCost = r.UnitCost,
                    SalePrice = r.SalePrice,
                    InventoryCapital = r.InventoryCapital,
                    FrozenCapital = r.InventoryCapital,
                    PotentialSalesValue = r.PotentialSalesValue,
                    PotentialProfit = r.PotentialProfit,
                    PotentialProfitSharePct = totalProfit > 0
                        ? RoundPct(r.PotentialProfit / totalProfit * 100m)
                        : null,
                    FlagNoCost = r.FlagNoCost,
                    FlagNoPrice = r.FlagNoPrice
                };
            }).ToList();

            return new PotentialValueReport
            {
                TotalPotentialSalesValue = totalSalesValue,
                TotalPotentialProfit = totalProfit,
                TotalInventoryCapital = totalFrozen,
                TotalFrozenCapital = totalFrozen,
                ProductsWithPotentialProfit = eligible.Count,
                ProductsExcludedNoPriceWithStock = full.Rows.Count(r => r.Stock > 0 && r.FlagNoPrice),
                ProductsExcludedNoCostWithStock = full.Rows.Count(r => r.Stock > 0 && r.FlagNoCost),
                ProductsExcludedNoStock = full.Rows.Count(r => r.Stock <= 0),
                Items = items
            };
        }

        public decimal GetPotentialProfitTotal(DateTime? asOf = null)
            => GetPotentialValueReport(asOf).TotalPotentialProfit;

        public decimal GetPotentialSalesValueTotal(DateTime? asOf = null)
            => GetPotentialValueReport(asOf).TotalPotentialSalesValue;

        private static InventoryFinancialRow MapRow(DataRow raw, DateTime refDate, int velocityWindowDays)
        {
            int stock = GetInt(raw, "Stock");
            decimal unitCost = GetDecimal(raw, "UnitCost");
            decimal salePrice = GetDecimal(raw, "SalePrice");
            int unitsSold = GetInt(raw, "UnitsSold");
            decimal revenue = GetDecimal(raw, "Revenue");
            decimal cogs = GetDecimal(raw, "Cogs");
            decimal realized = GetDecimal(raw, "RealizedProfit");
            int linesWithCost = GetInt(raw, "LinesWithCost");
            int linesWithoutCost = GetInt(raw, "LinesWithoutCost");
            DateTime? lastSale = GetDateTime(raw, "LastSaleDate");
            DateTime? firstSale = GetDateTime(raw, "FirstSaleDate");
            DateTime? firstEntry = GetDateTime(raw, "FirstEntryDate");
            DateTime? latestEntry = GetDateTime(raw, "LatestEntryDate");
            int unitsVelocity = GetInt(raw, "UnitsSoldVelocity");
            decimal cogsVelocity = GetDecimal(raw, "CogsVelocity");

            bool flagNoCost = unitCost <= 0;
            bool flagNoPrice = salePrice <= 0;
            bool flagNoStock = stock == 0;
            bool flagNegative = stock < 0;
            bool flagUncosted = linesWithoutCost > 0;
            bool reliableRealized = linesWithCost > 0;
            bool flagNoRotation = stock > 0 && unitsSold == 0;
            bool flagNoEntryHistory = !firstEntry.HasValue;

            decimal inventoryCost = InventoryFinancialMath.InventoryCapital(stock, unitCost);
            decimal potentialSales = InventoryFinancialMath.PotentialSalesValue(stock, salePrice);
            decimal potentialProfit = InventoryFinancialMath.PotentialProfit(stock, unitCost, salePrice);

            decimal? margin = null;
            decimal? roi = null;
            if (reliableRealized)
            {
                margin = InventoryFinancialMath.MarginPct(realized, revenue);
                roi = InventoryFinancialMath.RoiPct(realized, cogs);
            }

            int? daysSinceFirstEntry = InventoryFinancialMath.DaysSince(firstEntry, refDate);
            int? daysSinceLatestEntry = InventoryFinancialMath.DaysSince(latestEntry, refDate);
            var idle = InventoryFinancialMath.ResolveIdle(lastSale, firstEntry, refDate);
            var velocity = InventoryFinancialMath.ResolveVelocity(unitsVelocity, velocityWindowDays);
            decimal? turnoverProxy = InventoryFinancialMath.TurnoverProxy(cogsVelocity, inventoryCost);
            decimal? unitTurnover = InventoryFinancialMath.UnitTurnoverProxy(unitsVelocity, stock);
            decimal? daysOfCover = InventoryFinancialMath.DaysOfCover(stock, velocity.UnitsPerDay);
            bool flagOverstock = daysOfCover.HasValue
                && daysOfCover.Value >= InventoryFinancialMath.DefaultOverstockCoverDays;
            int stockMin = GetInt(raw, "StockMinimo");
            bool flagStockout = stock >= 0
                && stockMin > 0
                && stock <= stockMin
                && velocity.UnitsPerDay.HasValue
                && velocity.UnitsPerDay.Value > 0;

            InventoryHealthStatus health = InventoryHealthClassifier.Classify(
                stock,
                inventoryCost,
                potentialProfit,
                idle.Kind,
                idle.IdleDays,
                daysSinceFirstEntry,
                daysOfCover,
                velocity.UnitsPerDay);

            return new InventoryFinancialRow
            {
                ProductId = GetInt(raw, "ProductId"),
                ProductName = GetString(raw, "ProductName"),
                Category = GetString(raw, "Category"),
                Activo = GetBool(raw, "Activo"),
                Stock = stock,
                StockMinimo = stockMin,
                UnitCost = unitCost,
                SalePrice = salePrice,
                InventoryCost = inventoryCost,
                InventoryCapital = inventoryCost,
                PotentialSalesValue = potentialSales,
                PotentialProfit = potentialProfit,
                UnitsSold = unitsSold,
                Revenue = revenue,
                Cogs = reliableRealized ? cogs : 0m,
                RealizedProfit = reliableRealized ? realized : 0m,
                MarginPct = margin,
                RoiPct = roi,
                FirstSaleDate = firstSale,
                LastSaleDate = lastSale,
                DaysWithoutSale = idle.DaysWithoutSale,
                FlagNeverSold = idle.Kind == InventoryIdleKind.NeverSold,
                IdleKind = idle.Kind,
                IdleDays = idle.IdleDays,
                VelocityWindowDays = velocityWindowDays,
                UnitsSoldInVelocityWindow = unitsVelocity,
                UnitsPerDay = velocity.UnitsPerDay,
                UnitsPerWeek = velocity.UnitsPerWeek,
                UnitsPerMonth = velocity.UnitsPerMonth,
                CogsInVelocityWindow = InventoryFinancialMath.RoundMoney(cogsVelocity),
                TurnoverProxy = turnoverProxy,
                UnitTurnoverProxy = unitTurnover,
                DaysOfCover = daysOfCover,
                FlagOverstock = flagOverstock,
                FlagStockoutRisk = flagStockout,
                HealthStatus = health,
                FirstEntryDate = firstEntry,
                LatestEntryDate = latestEntry,
                DaysSinceFirstEntry = daysSinceFirstEntry,
                DaysSinceLatestEntry = daysSinceLatestEntry,
                FlagNoEntryHistory = flagNoEntryHistory,
                FlagNoCost = flagNoCost,
                FlagNoPrice = flagNoPrice,
                FlagNoStock = flagNoStock,
                FlagNegativeStock = flagNegative,
                FlagNoRotation = flagNoRotation,
                FlagUncostedSales = flagUncosted,
                HasReliableRealizedProfit = reliableRealized
            };
        }

        private static InventoryFinancialSummary BuildSummary(List<InventoryFinancialRow> rows, int velocityWindowDays)
        {
            decimal capitalTotal = InventoryFinancialMath.RoundMoney(rows.Sum(r => r.InventoryCapital));
            decimal cogsVelTotal = InventoryFinancialMath.RoundMoney(rows.Sum(r => r.CogsInVelocityWindow));
            decimal frozenStatus = SumCapital(rows, InventoryHealthStatus.Frozen);
            decimal critical = SumCapital(rows, InventoryHealthStatus.Critical);
            decimal immobilizedCapital = InventoryFinancialMath.RoundMoney(frozenStatus + critical);

            return new InventoryFinancialSummary
            {
                ProductCount = rows.Count,
                ProductsWithStock = rows.Count(r => r.Stock > 0),
                ProductsNoCost = rows.Count(r => r.FlagNoCost),
                ProductsNoPrice = rows.Count(r => r.FlagNoPrice),
                ProductsNegativeStock = rows.Count(r => r.FlagNegativeStock),
                ProductsNoRotation = rows.Count(r => r.FlagNoRotation),
                ProductsNoEntryHistory = rows.Count(r => r.FlagNoEntryHistory),
                ProductsNeverSold = rows.Count(r => r.FlagNeverSold),
                VelocityWindowDays = velocityWindowDays,
                CogsInVelocityWindowTotal = cogsVelTotal,
                TurnoverProxy = InventoryFinancialMath.TurnoverProxy(cogsVelTotal, capitalTotal),
                ProductsOverstock = rows.Count(r => r.FlagOverstock),
                ProductsStockoutRisk = rows.Count(r => r.FlagStockoutRisk),
                ProductsHealthy = rows.Count(r => r.HealthStatus == InventoryHealthStatus.Healthy),
                ProductsSlow = rows.Count(r => r.HealthStatus == InventoryHealthStatus.Slow),
                ProductsFrozen = rows.Count(r => r.HealthStatus == InventoryHealthStatus.Frozen),
                ProductsCritical = rows.Count(r => r.HealthStatus == InventoryHealthStatus.Critical),
                ProductsNew = rows.Count(r => r.HealthStatus == InventoryHealthStatus.New),
                InventoryCapitalTotal = capitalTotal,
                HealthyCapital = SumCapital(rows, InventoryHealthStatus.Healthy),
                SlowCapital = SumCapital(rows, InventoryHealthStatus.Slow),
                NewCapital = SumCapital(rows, InventoryHealthStatus.New),
                FrozenStatusCapital = frozenStatus,
                CriticalCapital = critical,
                FrozenCapitalTotal = immobilizedCapital,
                FrozenSharePct = InventoryFinancialMath.FrozenShareOfInventoryPct(immobilizedCapital, capitalTotal),
                PotentialSalesValueTotal = InventoryFinancialMath.RoundMoney(rows.Sum(r => r.PotentialSalesValue)),
                PotentialProfitTotal = InventoryFinancialMath.RoundMoney(rows.Sum(r => r.PotentialProfit)),
                RevenueTotal = InventoryFinancialMath.RoundMoney(rows.Sum(r => r.Revenue)),
                CogsTotal = InventoryFinancialMath.RoundMoney(rows.Where(r => r.HasReliableRealizedProfit).Sum(r => r.Cogs)),
                RealizedProfitTotal = InventoryFinancialMath.RoundMoney(rows.Where(r => r.HasReliableRealizedProfit).Sum(r => r.RealizedProfit)),
                Rows = rows
            };
        }

        private static decimal SumCapital(List<InventoryFinancialRow> rows, InventoryHealthStatus status)
            => InventoryFinancialMath.RoundMoney(
                rows.Where(r => r.HealthStatus == status).Sum(r => r.InventoryCapital));

        private static decimal RoundMoney(decimal v) => InventoryFinancialMath.RoundMoney(v);

        private static decimal RoundPct(decimal v) => InventoryFinancialMath.RoundPct(v);

        private static int GetInt(DataRow row, string col)
            => row[col] == DBNull.Value ? 0 : Convert.ToInt32(row[col]);

        private static decimal GetDecimal(DataRow row, string col)
            => row[col] == DBNull.Value ? 0m : Convert.ToDecimal(row[col]);

        private static bool GetBool(DataRow row, string col)
            => row[col] != DBNull.Value && Convert.ToBoolean(row[col]);

        private static string GetString(DataRow row, string col)
            => row[col] == DBNull.Value ? string.Empty : Convert.ToString(row[col]) ?? string.Empty;

        private static DateTime? GetDateTime(DataRow row, string col)
            => row[col] == DBNull.Value ? null : Convert.ToDateTime(row[col]);
    }
}
