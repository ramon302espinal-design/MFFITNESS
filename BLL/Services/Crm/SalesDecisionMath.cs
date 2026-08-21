using System.Globalization;
using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato Centro de decisiones ventas (FASE 9.22).</summary>
    public static class SalesDecisionPolicy
    {
        public const string Definition =
            "FASE 9.22 / §63: generar señales narrativas para FrmAnaDecisiones. " +
            "Ejemplos: variación de ventas, concentración, cobertura vs crecimiento, margen. " +
            "NO ejecutar acciones automáticamente.";

        public const string Language =
            "Mensajes en lenguaje de negocio (es-DO). Porcentajes con formato local. " +
            "Forecast/estimación no se presentan como certeza.";
    }

    /// <summary>Composición pura de señales de decisión (FASE 9.22).</summary>
    public static class SalesDecisionMath
    {
        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-DO");

        public static string FormatPct(decimal value)
        {
            string sign = value > 0 ? "+" : string.Empty;
            return sign + value.ToString("N0", Cultura) + "%";
        }

        public static SalesDecisionSignal? FromRevenueVariation(
            SalesVariationLabel? revenue,
            string periodLabel = "el período")
        {
            if (revenue == null || !revenue.VariationPct.HasValue)
                return null;
            if (revenue.Direction is SalesVariationDirection.Flat
                or SalesVariationDirection.NoComparableBase)
                return null;

            decimal pct = revenue.VariationPct.Value;
            bool up = revenue.Direction == SalesVariationDirection.Up;
            return new SalesDecisionSignal
            {
                Code = up ? "SalesGrowth" : "SalesDecline",
                Severity = revenue.Strength == SalesVariationStrength.Strong
                    ? SalesDecisionSeverity.Watch
                    : SalesDecisionSeverity.Info,
                Title = up ? "Crecimiento de ventas" : "Caída de ventas",
                Message = up
                    ? $"Las ventas aumentaron {FormatPct(pct)} durante {periodLabel}."
                    : $"Las ventas cayeron {FormatPct(pct)} durante {periodLabel}.",
                Priority = revenue.Strength == SalesVariationStrength.Strong ? 80 : 50
            };
        }

        public static SalesDecisionSignal? FromConcentration(
            decimal? topNSharePct,
            int topN,
            decimal thresholdPct = 50m)
        {
            if (!topNSharePct.HasValue || topNSharePct.Value < thresholdPct || topN <= 0)
                return null;

            return new SalesDecisionSignal
            {
                Code = "GrowthConcentration",
                Severity = SalesDecisionSeverity.Watch,
                Title = "Concentración de ventas",
                Message = topN <= 3
                    ? $"El crecimiento / volumen está concentrado en {topN} productos " +
                      $"({topNSharePct.Value.ToString("N0", Cultura)}% de ingresos)."
                    : $"Alta concentración: Top {topN} = {topNSharePct.Value.ToString("N0", Cultura)}% de ingresos.",
                Priority = 70
            };
        }

        public static SalesDecisionSignal? FromGrowthWithLowCover(
            string productName,
            decimal? unitsChangePct,
            decimal? daysOfCover)
        {
            if (string.IsNullOrWhiteSpace(productName))
                return null;
            if (!unitsChangePct.HasValue || unitsChangePct.Value <= 0)
                return null;
            if (!daysOfCover.HasValue || daysOfCover.Value > 14m)
                return null;

            return new SalesDecisionSignal
            {
                Code = "GrowthLowCover",
                Severity = SalesDecisionSeverity.Action,
                Title = "Crecimiento con poca cobertura",
                Message =
                    $"El producto {productName} crece {FormatPct(unitsChangePct.Value)} " +
                    $"pero tiene solo {daysOfCover.Value.ToString("N0", Cultura)} días de cobertura.",
                Priority = 95
            };
        }

        public static SalesDecisionSignal? FromRevenueUpMarginDown(
            decimal? revenueChangePct,
            decimal? marginChangePct)
        {
            if (!revenueChangePct.HasValue || !marginChangePct.HasValue)
                return null;
            if (revenueChangePct.Value <= 2m || marginChangePct.Value >= -2m)
                return null;

            return new SalesDecisionSignal
            {
                Code = "RevenueUpMarginDown",
                Severity = SalesDecisionSeverity.Watch,
                Title = "Ventas sin margen",
                Message =
                    $"Las ventas aumentaron {FormatPct(revenueChangePct.Value)} " +
                    $"pero el margen cayó {FormatPct(marginChangePct.Value)}.",
                Priority = 85
            };
        }

        public static SalesDecisionSignal? FromStockout(int stockoutCount)
        {
            if (stockoutCount <= 0)
                return null;
            return new SalesDecisionSignal
            {
                Code = "StockoutRisk",
                Severity = SalesDecisionSeverity.Action,
                Title = "Riesgo de quiebre",
                Message = stockoutCount == 1
                    ? "Hay 1 producto con riesgo de quiebre de stock."
                    : $"Hay {stockoutCount} productos con riesgo de quiebre de stock.",
                Priority = 90
            };
        }

        public static SalesDecisionReport Build(
            ProfitPeriodKind periodKind,
            IEnumerable<SalesDecisionSignal?> candidates)
        {
            var signals = candidates
                .Where(s => s != null)
                .Cast<SalesDecisionSignal>()
                .OrderByDescending(s => s.Priority)
                .ThenBy(s => s.Title)
                .ToList();

            return new SalesDecisionReport
            {
                PeriodKind = periodKind,
                Signals = signals,
                Primary = signals.FirstOrDefault(),
                PolicyNote = SalesDecisionPolicy.Definition
            };
        }

        public static string PeriodLabel(ProfitPeriodKind kind)
            => kind switch
            {
                ProfitPeriodKind.Last30Days => "los últimos 30 días",
                ProfitPeriodKind.Last14Days => "los últimos 14 días",
                ProfitPeriodKind.Last7Days => "los últimos 7 días",
                ProfitPeriodKind.ThisMonth => "este mes",
                ProfitPeriodKind.ThisQuarter => "este trimestre",
                _ => "el período"
            };
    }
}
