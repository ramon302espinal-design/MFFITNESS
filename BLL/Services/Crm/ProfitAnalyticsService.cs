using BLL.Models.Crm;
using DL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BLL.Services.Crm
{
    /// <summary>
    /// Motor de ganancias (FASE 5.5–5.9).
    /// UI no calcula: consume este servicio.
    /// Potencial/capital → InventoryFinancialService.
    /// Anulaciones/devoluciones → <see cref="ProfitVoidAndReturnPolicy"/>.
    /// </summary>
    public class ProfitAnalyticsService
    {
        private readonly CrmProfitAnalyticsDAL dal = new();
        private readonly InventoryFinancialService inventory = new();

        /// <summary>Texto de política 5.9 para UI/auditoría (sin lógica financiera extra).</summary>
        public string GetVoidAndReturnPolicyNote()
            => ProfitVoidAndReturnPolicy.DescribeForUi();

        public ProfitSummary GetProfitSummary(
            DateTime? periodFrom = null,
            DateTime? periodToExclusive = null)
        {
            DataSet ds = dal.ObtenerAgregadoPeriodo(periodFrom, periodToExclusive);
            DataRow header = RequireSingleRow(ds.Tables["Header"], "Header");
            DataRow detail = RequireSingleRow(ds.Tables["Detail"], "Detail");

            int linesWithCost = GetInt(detail, "LinesWithCost");
            int linesWithoutCost = GetInt(detail, "LinesWithoutCost");
            int linesTotal = linesWithCost + linesWithoutCost;

            decimal revenueWithCost = RoundMoney(GetDecimal(detail, "RevenueWithCost"));
            decimal cogs = RoundMoney(GetDecimal(detail, "Cogs"));
            decimal realized = RoundMoney(GetDecimal(detail, "RealizedProfit"));
            bool reliable = linesWithCost > 0;

            decimal? margin = reliable
                ? InventoryFinancialMath.MarginPct(realized, revenueWithCost)
                : null;
            decimal? roi = reliable
                ? InventoryFinancialMath.RoiPct(realized, cogs)
                : null;

            decimal? coverage = linesTotal > 0
                ? InventoryFinancialMath.RoundPct(linesWithCost * 100m / linesTotal)
                : null;

            PotentialValueReport potential = inventory.GetPotentialValueReport();
            decimal inventoryCapital = inventory.GetInventoryCapitalTotal();
            decimal frozenClassified = inventory.GetFrozenCapitalTotal();

            return new ProfitSummary
            {
                PeriodFrom = periodFrom,
                PeriodToExclusive = periodToExclusive,
                TransactionCount = GetInt(header, "TransactionCount"),
                SalesHeaderTotal = RoundMoney(GetDecimal(header, "SalesHeaderTotal")),
                CollectedAtSale = RoundMoney(GetDecimal(header, "CollectedAtSale")),
                ReceivableAtSale = RoundMoney(GetDecimal(header, "ReceivableAtSale")),
                UnitsSold = GetInt(detail, "UnitsSold"),
                RevenueTotal = RoundMoney(GetDecimal(detail, "RevenueTotal")),
                RevenueWithCost = revenueWithCost,
                Cogs = reliable ? cogs : 0m,
                RealizedProfit = reliable ? realized : 0m,
                MarginPct = margin,
                RoiPct = roi,
                LinesWithCost = linesWithCost,
                LinesWithoutCost = linesWithoutCost,
                CostCoveragePct = coverage,
                HasReliableRealizedProfit = reliable,
                PotentialProfit = potential.TotalPotentialProfit,
                PotentialSalesValue = potential.TotalPotentialSalesValue,
                FrozenCapital = frozenClassified,
                InventoryCapital = inventoryCapital
            };
        }

        public ProfitSummary GetProfitSummary()
            => GetProfitSummary(null, null);

        public ProfitSummary GetForPeriod(
            ProfitPeriodKind kind,
            DateTime? asOf = null,
            DateTime? customFrom = null,
            DateTime? customToExclusive = null)
        {
            ProfitPeriodRange range = ResolvePeriod(kind, asOf, customFrom, customToExclusive);
            return GetProfitSummary(range.From, range.ToExclusive);
        }

        public ProfitSummary GetToday(DateTime? asOf = null)
            => GetForPeriod(ProfitPeriodKind.Today, asOf);

        public ProfitSummary GetThisMonth(DateTime? asOf = null)
            => GetForPeriod(ProfitPeriodKind.ThisMonth, asOf);

        public ProfitSummary GetThisYear(DateTime? asOf = null)
            => GetForPeriod(ProfitPeriodKind.ThisYear, asOf);

        public decimal? GetMarginPct(
            DateTime? periodFrom = null,
            DateTime? periodToExclusive = null)
            => GetProfitSummary(periodFrom, periodToExclusive).MarginPct;

        public decimal? GetRoiPct(
            DateTime? periodFrom = null,
            DateTime? periodToExclusive = null)
            => GetProfitSummary(periodFrom, periodToExclusive).RoiPct;

        /// <summary>Ganancia por producto en el período (ordenado por ganancia desc).</summary>
        public IReadOnlyList<ProfitGroupRow> GetByProduct(
            DateTime? periodFrom = null,
            DateTime? periodToExclusive = null,
            int? top = null)
        {
            DataTable table = dal.ObtenerPorProducto(periodFrom, periodToExclusive);
            return MapGroupRows(table, isProduct: true, top);
        }

        public IReadOnlyList<ProfitGroupRow> GetByProduct(ProfitPeriodKind kind, DateTime? asOf = null, int? top = null)
        {
            ProfitPeriodRange range = ResolvePeriod(kind, asOf);
            return GetByProduct(range.From, range.ToExclusive, top);
        }

        /// <summary>Ganancia por categoría en el período.</summary>
        public IReadOnlyList<ProfitGroupRow> GetByCategory(
            DateTime? periodFrom = null,
            DateTime? periodToExclusive = null,
            int? top = null)
        {
            DataTable table = dal.ObtenerPorCategoria(periodFrom, periodToExclusive);
            return MapGroupRows(table, isProduct: false, top);
        }

        public IReadOnlyList<ProfitGroupRow> GetByCategory(ProfitPeriodKind kind, DateTime? asOf = null, int? top = null)
        {
            ProfitPeriodRange range = ResolvePeriod(kind, asOf);
            return GetByCategory(range.From, range.ToExclusive, top);
        }

        /// <summary>Ganancia por día + acumulado (ordenado cronológico).</summary>
        public IReadOnlyList<ProfitDayRow> GetByDay(
            DateTime? periodFrom = null,
            DateTime? periodToExclusive = null)
        {
            DataTable table = dal.ObtenerPorDia(periodFrom, periodToExclusive);
            var rows = new List<ProfitDayRow>(table.Rows.Count);
            decimal cumulative = 0m;

            foreach (DataRow raw in table.Rows)
            {
                int linesWithCost = GetInt(raw, "LinesWithCost");
                int linesWithoutCost = GetInt(raw, "LinesWithoutCost");
                decimal revenueWithCost = RoundMoney(GetDecimal(raw, "RevenueWithCost"));
                decimal cogs = RoundMoney(GetDecimal(raw, "Cogs"));
                decimal realized = RoundMoney(GetDecimal(raw, "RealizedProfit"));
                bool reliable = linesWithCost > 0;
                decimal realizedShown = reliable ? realized : 0m;
                cumulative = RoundMoney(cumulative + realizedShown);

                rows.Add(new ProfitDayRow
                {
                    Date = Convert.ToDateTime(raw["SaleDate"]).Date,
                    TransactionCount = GetInt(raw, "TransactionCount"),
                    UnitsSold = GetInt(raw, "UnitsSold"),
                    RevenueTotal = RoundMoney(GetDecimal(raw, "RevenueTotal")),
                    RevenueWithCost = revenueWithCost,
                    Cogs = reliable ? cogs : 0m,
                    RealizedProfit = realizedShown,
                    MarginPct = reliable
                        ? InventoryFinancialMath.MarginPct(realized, revenueWithCost)
                        : null,
                    RoiPct = reliable
                        ? InventoryFinancialMath.RoiPct(realized, cogs)
                        : null,
                    CumulativeRealizedProfit = cumulative,
                    LinesWithCost = linesWithCost,
                    LinesWithoutCost = linesWithoutCost,
                    HasReliableRealizedProfit = reliable
                });
            }

            return rows;
        }

        public IReadOnlyList<ProfitDayRow> GetByDay(ProfitPeriodKind kind, DateTime? asOf = null)
        {
            ProfitPeriodRange range = ResolvePeriod(kind, asOf);
            return GetByDay(range.From, range.ToExclusive);
        }

        /// <summary>Resuelve [From, ToExclusive) según preset. Custom usa customFrom/customToExclusive.</summary>
        public static ProfitPeriodRange ResolvePeriod(
            ProfitPeriodKind kind,
            DateTime? asOf = null,
            DateTime? customFrom = null,
            DateTime? customToExclusive = null)
        {
            DateTime d = (asOf ?? DateTime.Today).Date;

            return kind switch
            {
                ProfitPeriodKind.AllTime => new ProfitPeriodRange(null, null),
                ProfitPeriodKind.Today => new ProfitPeriodRange(d, d.AddDays(1)),
                ProfitPeriodKind.Yesterday => new ProfitPeriodRange(d.AddDays(-1), d),
                ProfitPeriodKind.Last7Days => new ProfitPeriodRange(d.AddDays(-6), d.AddDays(1)),
                ProfitPeriodKind.Last14Days => new ProfitPeriodRange(d.AddDays(-13), d.AddDays(1)),
                ProfitPeriodKind.Last30Days => new ProfitPeriodRange(d.AddDays(-29), d.AddDays(1)),
                ProfitPeriodKind.ThisMonth => new ProfitPeriodRange(
                    new DateTime(d.Year, d.Month, 1),
                    new DateTime(d.Year, d.Month, 1).AddMonths(1)),
                ProfitPeriodKind.PreviousMonth => ResolvePreviousMonth(d),
                ProfitPeriodKind.ThisQuarter => ResolveThisQuarter(d),
                ProfitPeriodKind.ThisSemester => ResolveThisSemester(d),
                ProfitPeriodKind.ThisYear => new ProfitPeriodRange(
                    new DateTime(d.Year, 1, 1),
                    new DateTime(d.Year, 1, 1).AddYears(1)),
                ProfitPeriodKind.PreviousYear => new ProfitPeriodRange(
                    new DateTime(d.Year - 1, 1, 1),
                    new DateTime(d.Year, 1, 1)),
                ProfitPeriodKind.Custom => new ProfitPeriodRange(customFrom, customToExclusive),
                _ => new ProfitPeriodRange(null, null)
            };
        }

        private static ProfitPeriodRange ResolvePreviousMonth(DateTime d)
        {
            var thisMonth = new DateTime(d.Year, d.Month, 1);
            return new ProfitPeriodRange(thisMonth.AddMonths(-1), thisMonth);
        }

        /// <summary>Q1=Ene–Mar, Q2=Abr–Jun, Q3=Jul–Sep, Q4=Oct–Dic.</summary>
        private static ProfitPeriodRange ResolveThisQuarter(DateTime d)
        {
            int quarterIndex = (d.Month - 1) / 3;
            int startMonth = quarterIndex * 3 + 1;
            var from = new DateTime(d.Year, startMonth, 1);
            return new ProfitPeriodRange(from, from.AddMonths(3));
        }

        /// <summary>H1=Ene–Jun, H2=Jul–Dic.</summary>
        private static ProfitPeriodRange ResolveThisSemester(DateTime d)
        {
            int startMonth = d.Month <= 6 ? 1 : 7;
            var from = new DateTime(d.Year, startMonth, 1);
            return new ProfitPeriodRange(from, from.AddMonths(6));
        }

        private static IReadOnlyList<ProfitGroupRow> MapGroupRows(DataTable table, bool isProduct, int? top)
        {
            var mapped = new List<ProfitGroupRow>(table.Rows.Count);

            foreach (DataRow raw in table.Rows)
            {
                int linesWithCost = GetInt(raw, "LinesWithCost");
                int linesWithoutCost = GetInt(raw, "LinesWithoutCost");
                decimal revenueWithCost = RoundMoney(GetDecimal(raw, "RevenueWithCost"));
                decimal cogs = RoundMoney(GetDecimal(raw, "Cogs"));
                decimal realized = RoundMoney(GetDecimal(raw, "RealizedProfit"));
                bool reliable = linesWithCost > 0;
                decimal realizedShown = reliable ? realized : 0m;

                mapped.Add(new ProfitGroupRow
                {
                    ProductId = isProduct ? GetInt(raw, "ProductId") : null,
                    ProductName = isProduct ? GetString(raw, "ProductName") : null,
                    CategoryId = raw.Table.Columns.Contains("CategoryId")
                        ? GetInt(raw, "CategoryId")
                        : null,
                    GroupName = isProduct
                        ? GetString(raw, "ProductName")
                        : GetString(raw, "CategoryName"),
                    UnitsSold = GetInt(raw, "UnitsSold"),
                    RevenueTotal = RoundMoney(GetDecimal(raw, "RevenueTotal")),
                    RevenueWithCost = revenueWithCost,
                    Cogs = reliable ? cogs : 0m,
                    RealizedProfit = realizedShown,
                    MarginPct = reliable
                        ? InventoryFinancialMath.MarginPct(realized, revenueWithCost)
                        : null,
                    RoiPct = reliable
                        ? InventoryFinancialMath.RoiPct(realized, cogs)
                        : null,
                    TransactionCount = raw.Table.Columns.Contains("TransactionCount")
                        ? GetInt(raw, "TransactionCount")
                        : 0,
                    LinesWithCost = linesWithCost,
                    LinesWithoutCost = linesWithoutCost,
                    HasReliableRealizedProfit = reliable,
                    IsLoss = reliable && realizedShown < 0m
                });
            }

            // Reordenar por ganancia mostrada (fiable) para ranking estable.
            mapped = mapped
                .OrderByDescending(r => r.RealizedProfit)
                .ThenByDescending(r => r.RevenueTotal)
                .ThenBy(r => r.GroupName)
                .ToList();

            decimal totalProfit = mapped.Sum(r => r.RealizedProfit);

            var ranked = new List<ProfitGroupRow>(mapped.Count);
            int rank = 0;
            foreach (ProfitGroupRow r in mapped)
            {
                rank++;
                ranked.Add(new ProfitGroupRow
                {
                    Rank = rank,
                    ProductId = r.ProductId,
                    ProductName = r.ProductName,
                    CategoryId = r.CategoryId,
                    GroupName = r.GroupName,
                    UnitsSold = r.UnitsSold,
                    RevenueTotal = r.RevenueTotal,
                    RevenueWithCost = r.RevenueWithCost,
                    Cogs = r.Cogs,
                    RealizedProfit = r.RealizedProfit,
                    MarginPct = r.MarginPct,
                    RoiPct = r.RoiPct,
                    TransactionCount = r.TransactionCount,
                    ProfitSharePct = totalProfit > 0
                        ? InventoryFinancialMath.RoundPct(r.RealizedProfit / totalProfit * 100m)
                        : null,
                    LinesWithCost = r.LinesWithCost,
                    LinesWithoutCost = r.LinesWithoutCost,
                    HasReliableRealizedProfit = r.HasReliableRealizedProfit,
                    IsLoss = r.IsLoss
                });
            }

            if (top.HasValue && top.Value > 0)
                return ranked.Take(top.Value).ToList();

            return ranked;
        }

        private static DataRow RequireSingleRow(DataTable? table, string name)
        {
            if (table == null || table.Rows.Count == 0)
                throw new InvalidOperationException($"Agregado {name} vacío.");
            return table.Rows[0];
        }

        private static decimal RoundMoney(decimal v)
            => InventoryFinancialMath.RoundMoney(v);

        private static int GetInt(DataRow row, string col)
            => row[col] == DBNull.Value ? 0 : Convert.ToInt32(row[col]);

        private static decimal GetDecimal(DataRow row, string col)
            => row[col] == DBNull.Value ? 0m : Convert.ToDecimal(row[col]);

        private static string GetString(DataRow row, string col)
            => row[col] == DBNull.Value ? string.Empty : Convert.ToString(row[col]) ?? string.Empty;
    }
}
