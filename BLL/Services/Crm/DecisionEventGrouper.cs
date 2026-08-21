using System.Globalization;
using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato de agrupación (FASE 10.18).</summary>
    public static class DecisionGroupPolicy
    {
        public const string Definition =
            "FASE 10.18: agrupar DecisionEvents relacionados para no saturar al usuario. " +
            "Misma entidad (producto/inversión) = un grupo. " +
            "Portafolio: temas SalesProfit / TrendForecast. TEST 10.";

        public const string Rule =
            "Priority/Severity del grupo = máximo de miembros. " +
            "Primary = líder por Priority ↓ Severity ↓. " +
            "No fusiona fingerprints distintos — solo presenta juntos.";

        public const string Deferred =
            "FASE 10 completa.";
    }

    /// <summary>Agrupa eventos relacionados (puro, sin I/O).</summary>
    public static class DecisionEventGrouper
    {
        public static IReadOnlyList<DecisionGroup> Group(IEnumerable<DecisionEvent>? events)
        {
            if (events == null)
                return Array.Empty<DecisionGroup>();

            var list = events.Where(e => e != null).ToList();
            if (list.Count == 0)
                return Array.Empty<DecisionGroup>();

            var buckets = new Dictionary<string, List<DecisionEvent>>(StringComparer.Ordinal);

            foreach (DecisionEvent e in list)
            {
                string key = ResolveGroupKey(e);
                if (!buckets.TryGetValue(key, out List<DecisionEvent>? bucket))
                {
                    bucket = new List<DecisionEvent>();
                    buckets[key] = bucket;
                }

                bucket.Add(e);
            }

            return buckets
                .Select(kv => BuildGroup(kv.Key, kv.Value))
                .OrderByDescending(g => (int)g.Priority)
                .ThenByDescending(g => (int)g.Severity)
                .ThenByDescending(g => g.EventCount)
                .ThenBy(g => g.Title, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Product/Investment con Id → entidad.
        /// Portfolio → tema por área (SalesProfit, CapInv, TrendForecast, Other).
        /// </summary>
        public static string ResolveGroupKey(DecisionEvent e)
        {
            if (e.EntityType is DecisionEntityType.Product or DecisionEntityType.Investment
                or DecisionEntityType.Category
                && !string.IsNullOrWhiteSpace(e.EntityId))
            {
                return string.Create(CultureInfo.InvariantCulture,
                    $"{(int)e.EntityType}|{e.EntityId.Trim().ToUpperInvariant()}");
            }

            string theme = ResolvePortfolioTheme(e.Area);
            string period = string.IsNullOrWhiteSpace(e.PeriodKey) ? "_" : e.PeriodKey.Trim().ToUpperInvariant();
            return $"PORTFOLIO|{theme}|{period}";
        }

        public static string ResolvePortfolioTheme(DecisionEventArea area) => area switch
        {
            DecisionEventArea.Sales or DecisionEventArea.Profit or DecisionEventArea.Margin
                or DecisionEventArea.Roi => "SALES_PROFIT",
            DecisionEventArea.Inventory or DecisionEventArea.Capital
                or DecisionEventArea.Liquidity => "CAPITAL_INV",
            DecisionEventArea.Trend or DecisionEventArea.Forecast => "TREND_FORECAST",
            DecisionEventArea.Investment => "INVESTMENT",
            DecisionEventArea.Product => "PRODUCT",
            _ => "OTHER"
        };

        private static DecisionGroup BuildGroup(string groupKey, List<DecisionEvent> members)
        {
            List<DecisionEvent> ordered = members
                .OrderByDescending(e => (int)e.Priority)
                .ThenByDescending(e => (int)e.Severity)
                .ThenBy(e => e.Title, StringComparer.OrdinalIgnoreCase)
                .ToList();

            DecisionEvent primary = ordered[0];
            DecisionPriority maxPri = ordered.Max(e => e.Priority);
            DecisionSeverity maxSev = ordered.Max(e => e.Severity);

            string title;
            string summary;
            if ((primary.EntityType is DecisionEntityType.Product or DecisionEntityType.Investment)
                && !string.IsNullOrWhiteSpace(primary.EntityName))
            {
                title = primary.EntityName;
                summary = ordered.Count == 1
                    ? primary.Title
                    : $"{ordered.Count} señales relacionadas sobre {primary.EntityName}.";
            }
            else
            {
                title = ThemeDisplayName(ResolvePortfolioTheme(primary.Area));
                summary = ordered.Count == 1
                    ? primary.Title
                    : $"{ordered.Count} señales de {title.ToLowerInvariant()}.";
            }

            return new DecisionGroup
            {
                GroupId = StableId(groupKey),
                GroupKey = groupKey,
                Title = title,
                Summary = summary,
                EntityType = primary.EntityType,
                EntityId = primary.EntityId,
                EntityName = primary.EntityName,
                Severity = maxSev,
                Priority = maxPri,
                Events = ordered,
                Primary = primary
            };
        }

        private static string ThemeDisplayName(string theme) => theme switch
        {
            "SALES_PROFIT" => "Ventas y rentabilidad",
            "CAPITAL_INV" => "Capital e inventario",
            "TREND_FORECAST" => "Tendencia y forecast",
            "INVESTMENT" => "Inversiones",
            "PRODUCT" => "Productos",
            _ => "Otras señales"
        };

        private static string StableId(string groupKey)
        {
            // Id determinista corto (no crypto) para UI/tests
            unchecked
            {
                int hash = 17;
                foreach (char c in groupKey)
                    hash = hash * 31 + c;
                return "grp_" + ((uint)hash).ToString("X8", CultureInfo.InvariantCulture);
            }
        }
    }
}
