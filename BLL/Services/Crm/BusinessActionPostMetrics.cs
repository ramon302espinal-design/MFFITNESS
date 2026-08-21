using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato post-métricas / deltas (FASE 11.8).</summary>
    public static class BusinessActionPostMetricsPolicy
    {
        public const string Definition =
            "FASE 11.8: captura métricas post-acción vs Baseline. " +
            "Money/Count → variación %. Niveles % (margen) → puntos porcentuales (pp). " +
            "Lenguaje: 'se observó' — sin causalidad. Outcome = 11.9.";

        public const string PpRule =
            "profit.margin_pct 22→25 = +3 pp (no +13.6%). " +
            "Claves *_var_pct / *_change_pct ya son variaciones → delta relativo N/D o pp según catálogo.";

        public const string Deferred =
            "FASE 11 completa.";
    }

    /// <summary>Cálculo puro de deltas baseline → post (FASE 11.8).</summary>
    public static class BusinessActionMetricDeltaMath
    {
        /// <summary>
        /// True si el nivel % se compara en puntos porcentuales (margen, share, ROI nivel).
        /// False para dinero/conteo (variación relativa %) o claves ya-variación.
        /// </summary>
        public static bool UsesPercentagePoints(string metricKey)
        {
            if (string.IsNullOrWhiteSpace(metricKey))
                return false;

            string key = metricKey.Trim();
            if (key.Contains("_var_", StringComparison.OrdinalIgnoreCase)
                || key.Contains("_change_pct", StringComparison.OrdinalIgnoreCase)
                || key.EndsWith("_change_pp", StringComparison.OrdinalIgnoreCase)
                || key.EndsWith("_delta_pp", StringComparison.OrdinalIgnoreCase))
            {
                // Ya son deltas/variaciones: no tratar el valor como nivel de margen.
                return key.EndsWith("_pp", StringComparison.OrdinalIgnoreCase)
                    || key.Contains("_delta_pp", StringComparison.OrdinalIgnoreCase)
                    || key.Contains("_change_pp", StringComparison.OrdinalIgnoreCase);
            }

            DecisionMetricDescriptor? desc = DecisionMetricsCatalog.Find(key);
            if (desc?.Unit == DecisionMetricUnit.Percent)
                return true;

            return key.EndsWith("_pct", StringComparison.OrdinalIgnoreCase)
                || key.EndsWith(".margin", StringComparison.OrdinalIgnoreCase);
        }

        public static BusinessActionMetricDelta? Compute(
            string metricKey,
            string? label,
            decimal? before,
            decimal? after,
            string? unit = null)
        {
            if (string.IsNullOrWhiteSpace(metricKey))
                return null;
            if (!before.HasValue && !after.HasValue)
                return null;

            DecisionMetricDescriptor? desc = DecisionMetricsCatalog.Find(metricKey);
            bool isPp = UsesPercentagePoints(metricKey);
            decimal? change = isPp
                ? AbsoluteDelta(before, after)
                : RelativePct(before, after);

            return new BusinessActionMetricDelta
            {
                MetricKey = metricKey.Trim(),
                Label = string.IsNullOrWhiteSpace(label)
                    ? (desc?.DisplayName ?? metricKey.Trim())
                    : label.Trim(),
                Before = before,
                After = after,
                Change = change,
                IsPercentagePoints = isPp,
                Unit = unit ?? (desc == null ? null : UnitLabel(desc.Unit))
            };
        }

        public static IReadOnlyList<BusinessActionMetricDelta> Compare(
            BusinessActionBaseline? baseline,
            BusinessActionBaseline? post,
            IReadOnlyList<string>? preferredKeys = null)
        {
            if (baseline == null || !baseline.HasMetrics)
                return Array.Empty<BusinessActionMetricDelta>();
            if (post == null || !post.HasMetrics)
                return Array.Empty<BusinessActionMetricDelta>();

            var beforeMap = baseline.Metrics
                .GroupBy(m => m.MetricKey, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
            var afterMap = post.Metrics
                .GroupBy(m => m.MetricKey, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            IEnumerable<string> keys;
            if (preferredKeys is { Count: > 0 })
            {
                keys = preferredKeys
                    .Where(k => !string.IsNullOrWhiteSpace(k))
                    .Select(k => k.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                keys = beforeMap.Keys.Union(afterMap.Keys, StringComparer.OrdinalIgnoreCase);
            }

            var list = new List<BusinessActionMetricDelta>();
            foreach (string key in keys)
            {
                beforeMap.TryGetValue(key, out BusinessActionBaselineMetric? b);
                afterMap.TryGetValue(key, out BusinessActionBaselineMetric? a);
                if (b == null && a == null)
                    continue;

                BusinessActionMetricDelta? delta = Compute(
                    key,
                    a?.Label ?? b?.Label,
                    b?.Value,
                    a?.Value,
                    a?.Unit ?? b?.Unit);
                if (delta != null)
                    list.Add(delta);
            }

            return list;
        }

        /// <summary>Resumen soft sin causalidad (FASE 11.8 / 11.23).</summary>
        public static string BuildObservedSummary(IReadOnlyList<BusinessActionMetricDelta> deltas)
        {
            if (deltas == null || deltas.Count == 0)
            {
                return BusinessActionSoftLanguageGuard.EnsureObserved(
                    "Sin datos suficientes para comparar métricas post-acción.");
            }

            var parts = new List<string>();
            foreach (BusinessActionMetricDelta d in deltas.Take(6))
            {
                if (!d.Change.HasValue)
                {
                    parts.Add($"{d.Label}: N/D");
                    continue;
                }

                string sign = d.Change.Value > 0 ? "+" : "";
                string unit = d.IsPercentagePoints ? " pp" : "%";
                parts.Add($"{d.Label} {sign}{d.Change.Value:0.##}{unit}");
            }

            return BusinessActionSoftLanguageGuard.EnsureObserved(
                "Durante el período posterior se observó: " + string.Join("; ", parts) + ".");
        }

        public static decimal? RelativePct(decimal? before, decimal? after)
        {
            if (!before.HasValue || !after.HasValue)
                return null;
            if (before.Value == 0m)
                return null;
            return Math.Round(
                (after.Value - before.Value) / Math.Abs(before.Value) * 100m,
                4,
                MidpointRounding.AwayFromZero);
        }

        public static decimal? AbsoluteDelta(decimal? before, decimal? after)
        {
            if (!before.HasValue || !after.HasValue)
                return null;
            return Math.Round(after.Value - before.Value, 4, MidpointRounding.AwayFromZero);
        }

        private static string UnitLabel(DecisionMetricUnit unit) => unit switch
        {
            DecisionMetricUnit.Money => "Money",
            DecisionMetricUnit.Percent => "Percent",
            DecisionMetricUnit.Count => "Count",
            DecisionMetricUnit.Days => "Days",
            DecisionMetricUnit.Ratio => "Ratio",
            DecisionMetricUnit.Flag => "Flag",
            DecisionMetricUnit.EnumLabel => "Enum",
            _ => unit.ToString()
        };
    }

    /// <summary>
    /// Codec de deltas para DeltasPayload (FASE 11.8).
    /// D\tkey\tlabel\tbefore\tafter\tchange\tisPp\tunit
    /// </summary>
    public static class BusinessActionDeltaCodec
    {
        private const char Sep = '\t';

        public static string? Encode(IReadOnlyList<BusinessActionMetricDelta>? deltas)
        {
            if (deltas == null || deltas.Count == 0)
                return null;

            var lines = new List<string>(deltas.Count);
            foreach (BusinessActionMetricDelta d in deltas)
            {
                lines.Add(string.Join(Sep,
                    "D",
                    Sanitize(d.MetricKey),
                    Sanitize(d.Label),
                    Db(d.Before),
                    Db(d.After),
                    Db(d.Change),
                    d.IsPercentagePoints ? "1" : "0",
                    Sanitize(d.Unit)));
            }

            return string.Join('\n', lines);
        }

        public static IReadOnlyList<BusinessActionMetricDelta> Decode(string? payload)
        {
            if (string.IsNullOrWhiteSpace(payload))
                return Array.Empty<BusinessActionMetricDelta>();

            var list = new List<BusinessActionMetricDelta>();
            foreach (string line in payload.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                string[] p = line.Split(Sep);
                if (p.Length < 8 || p[0] != "D")
                    continue;

                list.Add(new BusinessActionMetricDelta
                {
                    MetricKey = p[1],
                    Label = p[2],
                    Before = ParseDec(p[3]),
                    After = ParseDec(p[4]),
                    Change = ParseDec(p[5]),
                    IsPercentagePoints = p[6] == "1",
                    Unit = string.IsNullOrEmpty(p[7]) ? null : p[7]
                });
            }

            return list;
        }

        private static string Sanitize(string? s)
        {
            if (string.IsNullOrEmpty(s))
                return "";
            return s.Replace('\t', ' ').Replace('\n', ' ').Replace('\r', ' ');
        }

        private static string Db(decimal? v)
            => v.HasValue
                ? v.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)
                : "";

        private static decimal? ParseDec(string s)
            => string.IsNullOrEmpty(s)
                ? null
                : decimal.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
    }
}
