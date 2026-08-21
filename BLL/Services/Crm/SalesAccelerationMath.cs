using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato aceleración / desaceleración (FASE 9.15).</summary>
    public static class SalesAccelerationPolicy
    {
        public const string Definition =
            "FASE 9.15: aceleración ≠ crecimiento. " +
            "Clasificar con ≥ MinChangeCount variaciones % consecutivas " +
            "(requiere ≥ MinChangeCount+1 valores absolutos). " +
            "Accelerating / Decelerating / Steady / InsufficientData.";

        public const string GrowthVsAcceleration =
            "CRECIMIENTO = dirección de la serie (9.14 Growing). " +
            "ACELERACIÓN = las tasas de variación aumentan (ej. +20 → +25 → +35). " +
            "DESACELERACIÓN = tasas bajan aunque sigan positivas (ej. +40 → +20 → +5).";

        public const string ProductBridge =
            "ProductAccelerationKind (FASE 8) queda Unknown en MoM de 2 puntos; " +
            "este motor cubre series multi-período de ventas.";
    }

    public sealed class SalesAccelerationThresholds
    {
        public static SalesAccelerationThresholds Default { get; } = new();

        /// <summary>Mínimo de variaciones % consecutivas (default 3 → 4 buckets).</summary>
        public int MinChangeCount { get; init; } = 3;

        /// <summary>|Last−First| ≤ banda → Steady (puntos porcentuales).</summary>
        public decimal SteadyDeltaPp { get; init; } = 5m;
    }

    /// <summary>Clasificación pura de aceleración (FASE 9.15). Sin I/O.</summary>
    public static class SalesAccelerationMath
    {
        public static SalesAccelerationResult ClassifyFromChangePcts(
            IReadOnlyList<decimal> changePcts,
            SalesAccelerationThresholds? thresholds = null)
        {
            SalesAccelerationThresholds t = thresholds ?? SalesAccelerationThresholds.Default;

            if (changePcts == null || changePcts.Count < t.MinChangeCount)
            {
                return new SalesAccelerationResult
                {
                    Kind = SalesAccelerationKind.InsufficientData,
                    ChangeCount = changePcts?.Count ?? 0,
                    ChangePcts = changePcts?.ToList() ?? new List<decimal>(),
                    Reason = $"Se requieren ≥ {t.MinChangeCount} variaciones % (hay {changePcts?.Count ?? 0})"
                };
            }

            decimal first = changePcts[0];
            decimal last = changePcts[changePcts.Count - 1];
            decimal delta = InventoryFinancialMath.RoundPct(last - first);
            var snapshot = changePcts.ToList();

            SalesAccelerationKind kind;
            string reason;
            if (Math.Abs(delta) <= t.SteadyDeltaPp)
            {
                kind = SalesAccelerationKind.Steady;
                reason = $"Δ tasas {delta:N1} pp dentro de banda ±{t.SteadyDeltaPp:N0} pp";
            }
            else if (delta > 0)
            {
                kind = SalesAccelerationKind.Accelerating;
                reason = $"Tasas {first:N0}% → {last:N0}% (Δ +{delta:N1} pp) — acelera";
            }
            else
            {
                kind = SalesAccelerationKind.Decelerating;
                reason = $"Tasas {first:N0}% → {last:N0}% (Δ {delta:N1} pp) — desacelera " +
                         "(puede seguir creciendo)";
            }

            return new SalesAccelerationResult
            {
                Kind = kind,
                ChangeCount = changePcts.Count,
                FirstChangePct = InventoryFinancialMath.RoundPct(first),
                LastChangePct = InventoryFinancialMath.RoundPct(last),
                AccelerationDeltaPp = delta,
                ChangePcts = snapshot,
                Reason = reason
            };
        }

        /// <summary>
        /// A partir de valores absolutos ordenados: calcula variaciones % consecutivas
        /// (Previous=0 → se omite ese paso; si quedan &lt; MinChangeCount → InsufficientData).
        /// </summary>
        public static SalesAccelerationResult ClassifyFromValues(
            IReadOnlyList<decimal> values,
            SalesAccelerationThresholds? thresholds = null)
        {
            if (values == null || values.Count < 2)
            {
                return ClassifyFromChangePcts(Array.Empty<decimal>(), thresholds);
            }

            var changes = new List<decimal>();
            for (int i = 1; i < values.Count; i++)
            {
                // Previous=0 → sin base comparable (N/D): no inventar +100% aquí.
                if (values[i - 1] == 0)
                    continue;

                decimal? pct = ProductTrendMath.ChangePct(values[i], values[i - 1]);
                if (pct.HasValue)
                    changes.Add(pct.Value);
            }

            return ClassifyFromChangePcts(changes, thresholds);
        }

        /// <summary>Agrega días con operación en buckets semanales (lunes→domingo).</summary>
        public static IReadOnlyList<decimal> WeeklyTotals(
            IReadOnlyList<ProfitDayRow> days,
            Func<ProfitDayRow, decimal> selector)
        {
            var operating = days
                .Where(d => d.TransactionCount > 0)
                .OrderBy(d => d.Date)
                .ToList();

            if (operating.Count == 0)
                return Array.Empty<decimal>();

            return operating
                .GroupBy(d => StartOfWeek(d.Date))
                .OrderBy(g => g.Key)
                .Select(g => g.Sum(selector))
                .ToList();
        }

        public static SalesAccelerationReport FromDays(
            IReadOnlyList<ProfitDayRow> days,
            ProfitPeriodKind periodKind,
            SalesAccelerationThresholds? thresholds = null)
        {
            return new SalesAccelerationReport
            {
                PeriodKind = periodKind,
                SeriesLabel = "Semanas con operación (ingresos)",
                Revenue = ClassifyFromValues(
                    WeeklyTotals(days, d => d.RevenueTotal), thresholds),
                RealizedProfit = ClassifyFromValues(
                    WeeklyTotals(days, d => d.RealizedProfit), thresholds),
                Units = ClassifyFromValues(
                    WeeklyTotals(days, d => (decimal)d.UnitsSold), thresholds),
                Transactions = ClassifyFromValues(
                    WeeklyTotals(days, d => (decimal)d.TransactionCount), thresholds)
            };
        }

        private static DateTime StartOfWeek(DateTime date)
        {
            // Lunes como inicio (ISO-ish) para buckets estables.
            int diff = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            return date.Date.AddDays(-diff);
        }
    }
}
