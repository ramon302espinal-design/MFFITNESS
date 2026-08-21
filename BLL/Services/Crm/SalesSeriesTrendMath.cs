using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato tendencia multi-punto (FASE 9.14).</summary>
    public static class SalesSeriesTrendPolicy
    {
        public const string Definition =
            "FASE 9.14: clasificar serie ≥ MinPoints (default 4): " +
            "Growing / Stable / Declining / Volatile / InsufficientData. " +
            "NO usar solo diferencia entre 2 días. ≠ ProductTrend MoM FASE 8.";

        public const string Volatile =
            "VOLÁTIL si CV% ≥ VolatileCvPct aunque la pendiente sea positiva/negativa. " +
            "Ej.: 20k, 80k, 15k, 90k.";

        public const string Slope =
            "Pendiente = regresión lineal simple sobre índices 0..n-1, " +
            "expresada como % del promedio por paso.";
    }

    public sealed class SalesSeriesTrendThresholds
    {
        public static SalesSeriesTrendThresholds Default { get; } = new();

        public int MinPoints { get; init; } = 4;
        public decimal StableSlopePct { get; init; } = 2m;
        public decimal VolatileCvPct { get; init; } = 40m;
    }

    /// <summary>Clasificación pura de series (FASE 9.14).</summary>
    public static class SalesSeriesTrendMath
    {
        public static SalesSeriesTrendResult Classify(
            IReadOnlyList<decimal> values,
            SalesSeriesTrendThresholds? thresholds = null)
        {
            SalesSeriesTrendThresholds t = thresholds ?? SalesSeriesTrendThresholds.Default;

            if (values == null || values.Count < t.MinPoints)
            {
                return new SalesSeriesTrendResult
                {
                    Kind = SalesSeriesTrendKind.InsufficientData,
                    PointCount = values?.Count ?? 0,
                    Reason = $"Se requieren ≥ {t.MinPoints} puntos (hay {values?.Count ?? 0})"
                };
            }

            decimal avg = values.Average();
            decimal? cv = CoefficientOfVariationPct(values, avg);
            decimal? slopePct = SlopePerStepPct(values, avg);

            if (cv.HasValue && cv.Value >= t.VolatileCvPct)
            {
                return new SalesSeriesTrendResult
                {
                    Kind = SalesSeriesTrendKind.Volatile,
                    PointCount = values.Count,
                    SlopePerStepPct = slopePct,
                    CoefficientOfVariationPct = cv,
                    Reason = $"CV {cv:N0}% ≥ {t.VolatileCvPct:N0}% — no clasificar solo por pendiente"
                };
            }

            if (!slopePct.HasValue || avg == 0)
            {
                return new SalesSeriesTrendResult
                {
                    Kind = SalesSeriesTrendKind.InsufficientData,
                    PointCount = values.Count,
                    SlopePerStepPct = slopePct,
                    CoefficientOfVariationPct = cv,
                    Reason = "Sin base para pendiente"
                };
            }

            SalesSeriesTrendKind kind;
            string reason;
            if (Math.Abs(slopePct.Value) <= t.StableSlopePct)
            {
                kind = SalesSeriesTrendKind.Stable;
                reason = $"Pendiente {slopePct:N1}%/paso dentro de banda estable ±{t.StableSlopePct:N0}%";
            }
            else if (slopePct.Value > 0)
            {
                kind = SalesSeriesTrendKind.Growing;
                reason = $"Pendiente +{slopePct:N1}%/paso en {values.Count} puntos";
            }
            else
            {
                kind = SalesSeriesTrendKind.Declining;
                reason = $"Pendiente {slopePct:N1}%/paso en {values.Count} puntos";
            }

            return new SalesSeriesTrendResult
            {
                Kind = kind,
                PointCount = values.Count,
                SlopePerStepPct = slopePct,
                CoefficientOfVariationPct = cv,
                Reason = reason
            };
        }

        public static decimal? CoefficientOfVariationPct(IReadOnlyList<decimal> values, decimal? mean = null)
        {
            if (values == null || values.Count < 2)
                return null;
            decimal avg = mean ?? values.Average();
            if (avg == 0)
                return null;

            decimal sumSq = values.Sum(v => (v - avg) * (v - avg));
            decimal variance = sumSq / values.Count;
            decimal std = (decimal)Math.Sqrt((double)variance);
            return InventoryFinancialMath.RoundPct(std / Math.Abs(avg) * 100m);
        }

        /// <summary>Pendiente OLS × 100 / promedio (cambio relativo aprox. por paso).</summary>
        public static decimal? SlopePerStepPct(IReadOnlyList<decimal> values, decimal? mean = null)
        {
            if (values == null || values.Count < 2)
                return null;

            int n = values.Count;
            decimal avgY = mean ?? values.Average();
            if (avgY == 0)
                return null;

            decimal avgX = (n - 1) / 2m;
            decimal num = 0m;
            decimal den = 0m;
            for (int i = 0; i < n; i++)
            {
                decimal dx = i - avgX;
                num += dx * (values[i] - avgY);
                den += dx * dx;
            }

            if (den == 0)
                return null;

            decimal slope = num / den;
            return InventoryFinancialMath.RoundPct(slope / Math.Abs(avgY) * 100m);
        }

        public static SalesSeriesTrendReport FromDays(
            IReadOnlyList<ProfitDayRow> days,
            ProfitPeriodKind periodKind,
            SalesSeriesTrendThresholds? thresholds = null)
        {
            var operating = days.Where(d => d.TransactionCount > 0).OrderBy(d => d.Date).ToList();
            return new SalesSeriesTrendReport
            {
                PeriodKind = periodKind,
                SeriesLabel = "Días con operación",
                Revenue = Classify(operating.Select(d => d.RevenueTotal).ToList(), thresholds),
                RealizedProfit = Classify(operating.Select(d => d.RealizedProfit).ToList(), thresholds),
                Units = Classify(operating.Select(d => (decimal)d.UnitsSold).ToList(), thresholds),
                Transactions = Classify(operating.Select(d => (decimal)d.TransactionCount).ToList(), thresholds)
            };
        }
    }
}
