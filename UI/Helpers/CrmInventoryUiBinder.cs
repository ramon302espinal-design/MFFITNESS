using System.Globalization;
using BLL.Models.Crm;
using BLL.Services.Crm;

namespace UI.Helpers
{
    /// <summary>
    /// Formato y carga segura de datos CRM inventario (FASE 4.10 / 7.14).
    /// Sin SQL en Forms: solo consume servicios BLL.
    /// </summary>
    public static class CrmInventoryUiBinder
    {
        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-DO");

        public static string Money(decimal value)
            => "RD$ " + value.ToString("N2", Cultura);

        public static string Pct(decimal value)
            => value.ToString("N2", Cultura) + " %";

        public static string Pct(decimal? value)
            => value.HasValue ? Pct(value.Value) : "N/D";

        public static string Count(int value)
            => value.ToString("N0", Cultura);

        public static string HealthLabel(InventoryHealthStatus status) => status switch
        {
            InventoryHealthStatus.New => "Nuevo",
            InventoryHealthStatus.Healthy => "Saludable",
            InventoryHealthStatus.Slow => "Lento",
            InventoryHealthStatus.Frozen => "Congelado",
            InventoryHealthStatus.Critical => "Crítico",
            _ => "N/D"
        };

        public static InventoryFinancialSummary? TryLoadSummary(out string? error)
        {
            try
            {
                error = null;
                return new InventoryFinancialService().GetInventoryFinancials();
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static InventoryCapitalHealthReport? TryLoadHealth(out string? error)
        {
            try
            {
                error = null;
                return new InventoryFinancialService().GetInventoryCapitalHealthReport(frozenTop: 15);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static FrozenCapitalReport? TryLoadFrozen(out string? error)
        {
            try
            {
                error = null;
                return new InventoryFinancialService().GetFrozenCapitalReport(top: 15);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static InventoryRiskReport? TryLoadRisk(out string? error)
        {
            try
            {
                error = null;
                return new InventoryFinancialService().GetInventoryRiskReport();
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static InventoryAlertReport? TryLoadAlerts(out string? error, int top = 20)
        {
            try
            {
                error = null;
                return new InventoryAlertService().GetAlerts(top: top);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static IReadOnlyList<InventoryCapitalRankRow>? TryLoadCapitalRanking(
            InventoryCapitalRankKind kind,
            out string? error,
            int top = 5)
        {
            try
            {
                error = null;
                return new InventoryFinancialService().GetCapitalRanking(kind, top);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static string RankHeadline(InventoryCapitalRankRow? row)
        {
            if (row == null)
                return "—";
            return row.Row.ProductName;
        }

        public static string RankMetric(InventoryCapitalRankRow? row)
        {
            if (row == null)
                return "Sin datos";
            return row.MetricLabel;
        }

        public static PotentialValueReport? TryLoadPotential(out string? error)
        {
            try
            {
                error = null;
                return new InventoryFinancialService().GetPotentialValueReport(top: 15);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static InventoryFinancialValidationReport? TryValidate(out string? error)
        {
            try
            {
                error = null;
                return new InventoryFinancialValidationService().Validate();
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Buckets FASE 7.8/7.14: New→Estrella slot, Healthy, Slow, Critical(+Frozen count in score).
        /// </summary>
        public static (int Nuevos, int Saludables, int Lentos, int Criticos, int Salud0a100) HealthBuckets(
            InventoryFinancialSummary summary)
        {
            int nuevos = summary.ProductsNew;
            int saludables = summary.ProductsHealthy;
            int lentos = summary.ProductsSlow;
            int criticos = summary.ProductsCritical + summary.ProductsFrozen;

            int withStock = Math.Max(1, summary.ProductsWithStock);
            decimal healthyShare = (decimal)(nuevos + saludables) / withStock;
            int salud = (int)Math.Clamp(Math.Round(healthyShare * 100m), 0, 100);
            salud -= Math.Min(40, summary.ProductsCritical * 10);
            salud -= Math.Min(25, summary.ProductsFrozen * 3);
            salud = Math.Clamp(salud, 0, 100);

            return (nuevos, saludables, lentos, criticos, salud);
        }

        /// <summary>LEGACY: preferir <see cref="HealthBuckets"/>.</summary>
        public static (int Estrella, int Buenos, int Lentos, int Criticos, int Salud0a100) ProvisionalHealthBuckets(
            InventoryFinancialSummary summary)
            => HealthBuckets(summary);

        public static decimal AvgIdleDays(InventoryFinancialSummary summary)
        {
            var days = summary.Rows
                .Where(r => r.Stock > 0 && r.IdleDays.HasValue)
                .Select(r => r.IdleDays!.Value)
                .ToList();
            if (days.Count == 0)
                return 0m;
            return Math.Round((decimal)days.Average(), 0, MidpointRounding.AwayFromZero);
        }

        public static decimal AvgDaysWithoutSale(InventoryFinancialSummary summary)
            => AvgIdleDays(summary);
    }
}
