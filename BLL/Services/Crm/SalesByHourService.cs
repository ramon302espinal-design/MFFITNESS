using BLL.Models.Crm;
using DL;
using System.Data;

namespace BLL.Services.Crm
{
    /// <summary>Ventas por hora + picos de demanda (FASE 9.8).</summary>
    public class SalesByHourService
    {
        private readonly CrmProfitAnalyticsDAL _dal = new();

        public SalesByHourReport GetReport(
            ProfitPeriodKind periodKind = ProfitPeriodKind.ThisMonth,
            DateTime? asOf = null)
        {
            ProfitPeriodRange range = ProfitAnalyticsService.ResolvePeriod(periodKind, asOf);
            DataTable table = _dal.ObtenerPorHora(range.From, range.ToExclusive);

            var hours = new List<SalesHourRow>(table.Rows.Count);
            foreach (DataRow raw in table.Rows)
            {
                int linesWithCost = GetInt(raw, "LinesWithCost");
                bool reliable = linesWithCost > 0;
                decimal profit = InventoryFinancialMath.RoundMoney(GetDecimal(raw, "RealizedProfit"));

                hours.Add(SalesByHourComposer.Compose(
                    hour: GetInt(raw, "SaleHour"),
                    transactions: GetInt(raw, "TransactionCount"),
                    units: GetInt(raw, "UnitsSold"),
                    revenue: InventoryFinancialMath.RoundMoney(GetDecimal(raw, "RevenueTotal")),
                    profit: profit,
                    reliableProfit: reliable));
            }

            return SalesByHourComposer.Build(hours, periodKind, range.From, range.ToExclusive);
        }

        private static int GetInt(DataRow row, string col)
            => row[col] == DBNull.Value ? 0 : Convert.ToInt32(row[col]);

        private static decimal GetDecimal(DataRow row, string col)
            => row[col] == DBNull.Value ? 0m : Convert.ToDecimal(row[col]);
    }
}
