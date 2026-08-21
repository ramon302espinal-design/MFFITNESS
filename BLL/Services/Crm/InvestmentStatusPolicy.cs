using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>
    /// Contrato de estados y transiciones de inversión (FASE 6.2 / 6.11).
    /// </summary>
    public static class InvestmentStatusPolicy
    {
        public const string RoiDefinition =
            "RoiInvestment = RealizedProfit / CapitalInvested × 100. No usar ventas ni capital recuperado como denominador.";

        public const string RecoveredDefinition =
            "CapitalRecovered = Σ COGS de unidades vendidas atribuibles a las ENTRADAS de la inversión.";

        public static string Describe(InvestmentStatus status) => status switch
        {
            InvestmentStatus.Planificada =>
                "Sin capital materializado (sin ENTRADAS asignadas). No cuenta como capital invertido real.",
            InvestmentStatus.Activa =>
                "Capital colocado; hay pendiente de recuperar y/o stock atribuible.",
            InvestmentStatus.Recuperada =>
                "Capital recuperado (COGS) ≥ capital invertido. Puede quedar inventario.",
            InvestmentStatus.Cerrada =>
                "Operación cerrada (manual o sin stock atribuible). Resultado histórico estable.",
            InvestmentStatus.ConPerdida =>
                "Ganancia realizada < 0. Visible; no ocultar.",
            _ => "Estado desconocido."
        };

        /// <summary>
        /// Transiciones permitidas. Cerrada no se reabre automáticamente.
        /// </summary>
        public static bool CanTransition(InvestmentStatus from, InvestmentStatus to)
        {
            if (from == to)
                return true;

            return (from, to) switch
            {
                (InvestmentStatus.Planificada, InvestmentStatus.Activa) => true,
                (InvestmentStatus.Planificada, InvestmentStatus.Cerrada) => true,
                (InvestmentStatus.Activa, InvestmentStatus.Recuperada) => true,
                (InvestmentStatus.Activa, InvestmentStatus.Cerrada) => true,
                (InvestmentStatus.Activa, InvestmentStatus.ConPerdida) => true,
                (InvestmentStatus.Recuperada, InvestmentStatus.Cerrada) => true,
                (InvestmentStatus.Recuperada, InvestmentStatus.Activa) => true,
                (InvestmentStatus.Recuperada, InvestmentStatus.ConPerdida) => true,
                (InvestmentStatus.ConPerdida, InvestmentStatus.Activa) => true,
                (InvestmentStatus.ConPerdida, InvestmentStatus.Cerrada) => true,
                (InvestmentStatus.ConPerdida, InvestmentStatus.Recuperada) => true,
                _ => false
            };
        }

        /// <summary>
        /// Estado sugerido por métricas (FASE 6.11).
        /// Con pérdida + stock restante → Activa (IsLoss en resumen); ConPerdida al agotar/cerrar.
        /// </summary>
        public static InvestmentStatus SuggestStatus(InvestmentSummary s)
        {
            if (s.CapitalInvested <= 0)
                return InvestmentStatus.Planificada;

            bool depleted = s.FrozenCapital <= 0 && s.CapitalPending <= 0;
            bool loss = s.IsLoss || s.RealizedProfit < 0;

            if (s.CloseDate.HasValue)
                return loss ? InvestmentStatus.ConPerdida : InvestmentStatus.Cerrada;

            if (depleted)
                return loss ? InvestmentStatus.ConPerdida : InvestmentStatus.Cerrada;

            if (s.CapitalRecovered >= s.CapitalInvested)
                return InvestmentStatus.Recuperada;

            return InvestmentStatus.Activa;
        }
    }
}
