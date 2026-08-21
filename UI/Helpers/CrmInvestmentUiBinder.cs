using System.Globalization;
using BLL.Models.Crm;
using BLL.Services.Crm;

namespace UI.Helpers
{
    /// <summary>
    /// Carga segura del motor de inversiones (FASE 6.14).
    /// Forms solo muestran; sin SQL ni fórmulas.
    /// </summary>
    public static class CrmInvestmentUiBinder
    {
        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-DO");

        public static string Money(decimal value)
            => "RD$ " + value.ToString("N2", Cultura);

        public static string Pct(decimal? value)
            => value.HasValue
                ? value.Value.ToString("N2", Cultura) + " %"
                : "N/D";

        public static string Count(int value)
            => value.ToString("N0", Cultura);

        public static IReadOnlyList<Investment>? TryList(out string? error)
        {
            try
            {
                error = null;
                return new InvestmentService().List();
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static InvestmentSummary? TrySummary(int id, out string? error)
        {
            try
            {
                error = null;
                return new InvestmentService().GetSummary(id);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static IReadOnlyList<InvestmentSummary>? TryLoadAllSummaries(out string? error)
        {
            try
            {
                error = null;
                var svc = new InvestmentService();
                return svc.List().Select(i => svc.GetSummary(i.Id)).ToList();
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static IReadOnlyList<InvestmentRankRow>? TryRanking(
            InvestmentRankKind kind,
            out string? error,
            int top = 5)
        {
            try
            {
                error = null;
                return new InvestmentService().GetRanking(kind, top);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static string StatusLabel(InvestmentStatus status) => status switch
        {
            InvestmentStatus.Planificada => "Planificada",
            InvestmentStatus.Activa => "Activa",
            InvestmentStatus.Recuperada => "Recuperada",
            InvestmentStatus.Cerrada => "Cerrada",
            InvestmentStatus.ConPerdida => "Con pérdida",
            _ => status.ToString()
        };
    }
}
