using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato baseline de acción (FASE 11.6).</summary>
    public static class BusinessActionBaselinePolicy
    {
        public const string Definition =
            "FASE 11.6: snapshot mínimo (ventas/margen/stock/capital) antes de la acción. " +
            "Claves del DecisionMetricsCatalog. Sin inventar datos. Sin mutar POS.";

        public const string MinimalSet =
            "sales.revenue · sales.units · profit.realized · profit.margin_pct · " +
            "inv.stock · capital.inventory · capital.immobilized · capital.at_risk";

        public const string Deferred =
            "FASE 11 completa.";
    }

    /// <summary>
    /// Compone baseline desde valores inyectados o SalesCapitalBridge (SSOT FASE 9.19).
    /// Sin I/O propio.
    /// </summary>
    public static class BusinessActionBaselineComposer
    {
        public const string CapitalBridgeSource = "SalesCapitalBridgeService";

        public static readonly IReadOnlyList<string> DefaultMetricKeys =
        [
            "sales.revenue",
            "sales.units",
            "profit.realized",
            "profit.margin_pct",
            "inv.stock",
            "capital.inventory",
            "capital.immobilized",
            "capital.at_risk"
        ];

        /// <summary>Desde diccionario clave→valor (tests / callers con hooks).</summary>
        public static BusinessActionBaseline FromMetricValues(
            BusinessActionBaselineCaptureRequest request,
            IReadOnlyDictionary<string, decimal?> values,
            string? sourceNote = null)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(values);

            IReadOnlyList<string> keys = ResolveKeys(request.MetricKeys);
            var metrics = new List<BusinessActionBaselineMetric>();
            string source = string.IsNullOrWhiteSpace(sourceNote) ? "InjectedValues" : sourceNote.Trim();

            foreach (string key in keys)
            {
                if (!values.TryGetValue(key, out decimal? value))
                    continue;
                metrics.Add(BuildMetric(key, value, source));
            }

            return Finish(request, metrics, source);
        }

        /// <summary>
        /// Desde DecisionAnalyticsBundle.CapitalBridge — no inventa si falta el bridge.
        /// </summary>
        public static BusinessActionBaseline FromAnalytics(
            BusinessActionBaselineCaptureRequest request,
            DecisionAnalyticsBundle? analytics)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (analytics?.CapitalBridge == null)
            {
                return Finish(
                    request,
                    Array.Empty<BusinessActionBaselineMetric>(),
                    "Sin SalesCapitalBridge en Analytics (datos insuficientes).");
            }

            return FromCapitalBridge(request, analytics.CapitalBridge, analytics.PeriodKind);
        }

        /// <summary>Desde reporte SalesCapitalBridge (SSOT).</summary>
        public static BusinessActionBaseline FromCapitalBridge(
            BusinessActionBaselineCaptureRequest request,
            SalesCapitalBridgeReport bridge,
            ProfitPeriodKind? periodKind = null)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(bridge);

            IReadOnlyList<string> keys = ResolveKeys(request.MetricKeys);
            var wanted = new HashSet<string>(keys, StringComparer.OrdinalIgnoreCase);
            var metrics = new List<BusinessActionBaselineMetric>();

            if (request.EntityType == DecisionEntityType.Product
                && !string.IsNullOrWhiteSpace(request.EntityId)
                && int.TryParse(request.EntityId.Trim(), out int productId))
            {
                SalesCapitalBridgeRow? row = bridge.Rows
                    .FirstOrDefault(r => r.ProductId == productId);

                if (row == null)
                {
                    return Finish(
                        request with { PeriodKind = request.PeriodKind ?? periodKind ?? bridge.PeriodKind },
                        Array.Empty<BusinessActionBaselineMetric>(),
                        $"{CapitalBridgeSource}: producto {productId} no encontrado.");
                }

                AddIf(wanted, metrics, "sales.revenue", row.RevenueTotal, CapitalBridgeSource);
                AddIf(wanted, metrics, "profit.realized", row.RealizedProfit, CapitalBridgeSource);
                AddIf(wanted, metrics, "profit.margin_pct", row.MarginPct, CapitalBridgeSource);
                AddIf(wanted, metrics, "inv.stock", row.Stock, CapitalBridgeSource);
                AddIf(wanted, metrics, "capital.inventory", row.InventoryCapital, CapitalBridgeSource);
                AddIf(wanted, metrics, "capital.immobilized", row.ImmobilizedCapital, CapitalBridgeSource);
            }
            else
            {
                // Portafolio / agregado
                AddIf(wanted, metrics, "sales.revenue", bridge.TotalRevenue, CapitalBridgeSource);
                AddIf(wanted, metrics, "profit.realized", bridge.TotalRealizedProfit, CapitalBridgeSource);
                AddIf(wanted, metrics, "capital.inventory", bridge.TotalInventoryCapital, CapitalBridgeSource);
                AddIf(wanted, metrics, "capital.immobilized", bridge.TotalImmobilizedCapital, CapitalBridgeSource);
                AddIf(wanted, metrics, "capital.at_risk", bridge.CapitalAtRisk, CapitalBridgeSource);

                if (wanted.Contains("inv.stock"))
                {
                    int stockSum = bridge.Rows.Sum(r => r.Stock);
                    AddIf(wanted, metrics, "inv.stock", stockSum, CapitalBridgeSource);
                }

                if (wanted.Contains("profit.margin_pct"))
                {
                    // Solo si hay margen confiable a nivel fila — no inventar margen de portafolio.
                    // Usar promedio ponderado por revenue de filas con MarginPct.
                    decimal? margin = PortfolioMarginPct(bridge);
                    if (margin.HasValue)
                        AddIf(wanted, metrics, "profit.margin_pct", margin, CapitalBridgeSource);
                }
            }

            var req = request with
            {
                PeriodKind = request.PeriodKind ?? periodKind ?? bridge.PeriodKind
            };
            return Finish(req, metrics, CapitalBridgeSource);
        }

        private static decimal? PortfolioMarginPct(SalesCapitalBridgeReport bridge)
        {
            decimal rev = 0m;
            decimal profit = 0m;
            bool any = false;
            foreach (SalesCapitalBridgeRow row in bridge.Rows)
            {
                if (!row.MarginPct.HasValue)
                    continue;
                // Reconstruir contribución aproximada: solo filas con margen conocido.
                // Preferir suma profit/revenue de esas filas (coherente con SSOT del row).
                rev += row.RevenueTotal;
                profit += row.RealizedProfit;
                any = true;
            }

            if (!any || rev <= 0m)
                return null;
            return Math.Round(profit / rev * 100m, 4, MidpointRounding.AwayFromZero);
        }

        private static void AddIf(
            HashSet<string> wanted,
            List<BusinessActionBaselineMetric> metrics,
            string key,
            decimal? value,
            string source)
        {
            if (!wanted.Contains(key))
                return;
            // Incluir null explícito solo si el caller lo pidió vía FromMetricValues;
            // aquí omitimos nulls (no inventar).
            if (!value.HasValue)
                return;
            metrics.Add(BuildMetric(key, value, source));
        }

        private static void AddIf(
            HashSet<string> wanted,
            List<BusinessActionBaselineMetric> metrics,
            string key,
            decimal value,
            string source)
            => AddIf(wanted, metrics, key, (decimal?)value, source);

        private static void AddIf(
            HashSet<string> wanted,
            List<BusinessActionBaselineMetric> metrics,
            string key,
            int value,
            string source)
            => AddIf(wanted, metrics, key, (decimal)value, source);

        internal static BusinessActionBaselineMetric BuildMetric(
            string key,
            decimal? value,
            string source)
        {
            DecisionMetricDescriptor? desc = DecisionMetricsCatalog.Find(key);
            return new BusinessActionBaselineMetric
            {
                MetricKey = key,
                Label = desc?.DisplayName ?? key,
                Value = value,
                Unit = desc == null ? null : UnitLabel(desc.Unit),
                Source = source
            };
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

        private static IReadOnlyList<string> ResolveKeys(IReadOnlyList<string>? keys)
        {
            if (keys == null || keys.Count == 0)
                return DefaultMetricKeys;
            return keys.Where(k => !string.IsNullOrWhiteSpace(k))
                .Select(k => k.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static BusinessActionBaseline Finish(
            BusinessActionBaselineCaptureRequest request,
            IReadOnlyList<BusinessActionBaselineMetric> metrics,
            string sourceNote)
            => new()
            {
                CapturedAt = request.CapturedAt ?? DateTime.UtcNow,
                PeriodKind = request.PeriodKind,
                EntityType = request.EntityType,
                EntityId = string.IsNullOrWhiteSpace(request.EntityId)
                    ? null
                    : request.EntityId.Trim(),
                SourceNote = sourceNote,
                Metrics = metrics
            };
    }

    /// <summary>
    /// Codec texto compacto para BaselinePayload (sin JSON obligatorio).
    /// Formato v1 (campos separados por TAB):
    /// H\tiso\tperiod\tentityType\tentityId\tsourceNote
    /// M\tkey\tlabel\tvalue\tunit\tsource
    /// </summary>
    public static class BusinessActionBaselineCodec
    {
        public const string VersionTag = "H";
        private const char Sep = '\t';

        public static string? Encode(BusinessActionBaseline? baseline)
        {
            if (baseline == null)
                return null;

            var lines = new List<string>
            {
                string.Join(Sep,
                    VersionTag,
                    baseline.CapturedAt.ToUniversalTime().ToString("o"),
                    baseline.PeriodKind.HasValue ? ((int)baseline.PeriodKind.Value).ToString() : "",
                    ((int)baseline.EntityType).ToString(),
                    Sanitize(baseline.EntityId),
                    Sanitize(baseline.SourceNote))
            };

            foreach (BusinessActionBaselineMetric m in baseline.Metrics)
            {
                lines.Add(string.Join(Sep,
                    "M",
                    Sanitize(m.MetricKey),
                    Sanitize(m.Label),
                    m.Value.HasValue
                        ? m.Value.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)
                        : "",
                    Sanitize(m.Unit),
                    Sanitize(m.Source)));
            }

            return string.Join('\n', lines);
        }

        public static BusinessActionBaseline? Decode(string? payload)
        {
            if (string.IsNullOrWhiteSpace(payload))
                return null;

            string[] lines = payload.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (lines.Length == 0)
                return null;

            string[] h = lines[0].Split(Sep);
            if (h.Length < 6 || !string.Equals(h[0], VersionTag, StringComparison.Ordinal))
                return null;

            DateTime captured = DateTime.Parse(
                h[1],
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.RoundtripKind);

            ProfitPeriodKind? period = string.IsNullOrEmpty(h[2])
                ? null
                : (ProfitPeriodKind)int.Parse(h[2]);

            var metrics = new List<BusinessActionBaselineMetric>();
            for (int i = 1; i < lines.Length; i++)
            {
                string[] p = lines[i].Split(Sep);
                if (p.Length < 6 || p[0] != "M")
                    continue;

                decimal? value = string.IsNullOrEmpty(p[3])
                    ? null
                    : decimal.Parse(p[3], System.Globalization.CultureInfo.InvariantCulture);

                metrics.Add(new BusinessActionBaselineMetric
                {
                    MetricKey = p[1],
                    Label = p[2],
                    Value = value,
                    Unit = EmptyToNull(p[4]),
                    Source = p[5]
                });
            }

            return new BusinessActionBaseline
            {
                CapturedAt = captured,
                PeriodKind = period,
                EntityType = (DecisionEntityType)int.Parse(h[3]),
                EntityId = EmptyToNull(h[4]),
                SourceNote = EmptyToNull(h[5]),
                Metrics = metrics
            };
        }

        private static string Sanitize(string? s)
        {
            if (string.IsNullOrEmpty(s))
                return "";
            return s.Replace('\t', ' ').Replace('\n', ' ').Replace('\r', ' ');
        }

        private static string? EmptyToNull(string s)
            => string.IsNullOrEmpty(s) ? null : s;
    }
}
