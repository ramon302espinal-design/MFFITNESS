using BLL.Models.Crm;
using System.Globalization;

namespace BLL.Services.Crm
{
    /// <summary>Contrato de variaciones y señales cruzadas (FASE 9.5).</summary>
    public static class SalesVariationPolicy
    {
        public const string Formula =
            "VARIACIÓN % = (Current − Previous) / Previous × 100. Previous = 0 → N/D.";

        public const string FlatBand =
            "Flat si |variación| ≤ FlatBandPct (default 2%). No es tendencia multi-punto (9.14).";

        public const string StrongBand =
            "Strong si |variación| ≥ StrongBandPct (default 15%). Mild entre Flat y Strong.";

        public const string CrossSignals =
            "Señales: Ingresos↑+Ganancia↓ (§50); Ingresos↑+Margen↓ (§51). " +
            "No ejecutan acciones — solo alertan.";

        public const string Display =
            "UI: '+20.00 %' / '−5.00 %' / 'N/D'. No inventar porcentaje.";
    }

    public sealed class SalesVariationThresholds
    {
        public static SalesVariationThresholds Default { get; } = new();

        public decimal FlatBandPct { get; init; } = 2m;
        public decimal StrongBandPct { get; init; } = 15m;
    }

    /// <summary>Clasificación pura de variaciones (FASE 9.5).</summary>
    public static class SalesVariationMath
    {
        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-DO");

        public static SalesVariationLabel Label(
            decimal? variationPct,
            SalesVariationThresholds? thresholds = null)
        {
            SalesVariationThresholds t = thresholds ?? SalesVariationThresholds.Default;

            if (!variationPct.HasValue)
            {
                return new SalesVariationLabel
                {
                    VariationPct = null,
                    Direction = SalesVariationDirection.NoComparableBase,
                    Strength = SalesVariationStrength.None,
                    Display = "N/D"
                };
            }

            decimal v = variationPct.Value;
            SalesVariationDirection dir;
            if (Math.Abs(v) <= t.FlatBandPct)
                dir = SalesVariationDirection.Flat;
            else if (v > 0)
                dir = SalesVariationDirection.Up;
            else
                dir = SalesVariationDirection.Down;

            SalesVariationStrength strength = SalesVariationStrength.None;
            if (dir is SalesVariationDirection.Up or SalesVariationDirection.Down)
            {
                strength = Math.Abs(v) >= t.StrongBandPct
                    ? SalesVariationStrength.Strong
                    : SalesVariationStrength.Mild;
            }

            string sign = v > 0 ? "+" : string.Empty;
            return new SalesVariationLabel
            {
                VariationPct = v,
                Direction = dir,
                Strength = strength,
                Display = sign + v.ToString("N2", Cultura) + " %"
            };
        }

        public static SalesVariationLabel FromDelta(
            SalesMetricDelta delta,
            SalesVariationThresholds? thresholds = null)
            => Label(delta.VariationPct, thresholds);

        public static IReadOnlyList<SalesCrossSignal> DetectCrossSignals(
            SalesMetricDelta revenue,
            SalesMetricDelta profit,
            SalesMetricDelta? margin = null)
        {
            var list = new List<SalesCrossSignal>();

            if (IsUp(revenue) && IsDown(profit))
            {
                list.Add(new SalesCrossSignal
                {
                    Kind = SalesCrossSignalKind.RevenueUpProfitDown,
                    Message = "Crecimiento de ventas sin crecimiento de ganancia " +
                              $"(ingresos {Label(revenue.VariationPct).Display}, " +
                              $"ganancia {Label(profit.VariationPct).Display})"
                });
            }

            if (margin != null && IsUp(revenue) && IsDown(margin))
            {
                list.Add(new SalesCrossSignal
                {
                    Kind = SalesCrossSignalKind.RevenueUpMarginDown,
                    Message = "Ventas ↑ con margen ↓ " +
                              $"(ingresos {Label(revenue.VariationPct).Display}, " +
                              $"margen {Label(margin.VariationPct).Display})"
                });
            }

            if (IsDown(revenue) && IsUp(profit))
            {
                list.Add(new SalesCrossSignal
                {
                    Kind = SalesCrossSignalKind.RevenueDownProfitUp,
                    Message = "Ingresos ↓ con ganancia ↑ " +
                              "(revisar mix / costos / ticket)"
                });
            }

            return list;
        }

        public static SalesVariationReport FromComparison(
            SalesComparisonReport comparison,
            SalesVariationThresholds? thresholds = null)
        {
            SalesVariationThresholds t = thresholds ?? SalesVariationThresholds.Default;
            SalesMetricDelta? margin = comparison.Margin;

            return new SalesVariationReport
            {
                Revenue = FromDelta(comparison.Revenue, t),
                RealizedProfit = FromDelta(comparison.RealizedProfit, t),
                Units = FromDelta(comparison.Units, t),
                Transactions = FromDelta(comparison.Transactions, t),
                Ticket = FromDelta(comparison.Ticket, t),
                Margin = margin == null ? null : FromDelta(margin, t),
                CrossSignals = DetectCrossSignals(
                    comparison.Revenue,
                    comparison.RealizedProfit,
                    margin)
            };
        }

        private static bool IsUp(SalesMetricDelta d)
            => d.VariationPct.HasValue && d.VariationPct.Value > SalesVariationThresholds.Default.FlatBandPct;

        private static bool IsDown(SalesMetricDelta d)
            => d.VariationPct.HasValue && d.VariationPct.Value < -SalesVariationThresholds.Default.FlatBandPct;
    }
}
