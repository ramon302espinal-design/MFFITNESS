using System.Globalization;
using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Contrato del Centro de decisiones (FASE 10.20).</summary>
    public static class DecisionCenterPolicy
    {
        public const string Definition =
            "FASE 10.20: DecisionCenter convierte Engine+Groups+Recommendations en " +
            "resumen ejecutivo y PRIORIDADES DE HOY. El usuario decide — sin auto-acciones.";

        public const string AntiFatigue =
            "No saturar: pocas prioridades (default 5), no 100 alertas técnicas (brief §102).";

        public const string Buckets =
            "CRÍTICAS / IMPORTANTES / REVISAR / OPORTUNIDADES (brief §103·§112).";

        public const string Deferred =
            "FASE 10 completa.";

        public const int DefaultMaxPriorities = 5;
    }

    /// <summary>
    /// Composer puro del Centro. Sin I/O. Sin side-effects.
    /// </summary>
    public static class DecisionCenterComposer
    {
        public static DecisionCenterReport Compose(
            DecisionEngineReport engine,
            DecisionCenterSnapshot? snapshot = null,
            string? periodKey = null,
            DateTime? generatedAt = null,
            int maxPriorities = DecisionCenterPolicy.DefaultMaxPriorities)
        {
            ArgumentNullException.ThrowIfNull(engine);

            int max = maxPriorities <= 0
                ? DecisionCenterPolicy.DefaultMaxPriorities
                : maxPriorities;

            IReadOnlyList<DecisionGroup> groups = engine.Groups;
            IReadOnlyList<DecisionRecommendation> recs = engine.Recommendations;

            var bucketByGroup = new Dictionary<string, DecisionCenterBucket>(StringComparer.Ordinal);
            int critical = 0, important = 0, review = 0, opportunity = 0;

            foreach (DecisionGroup g in groups)
            {
                DecisionRecommendation? rec = FindGroupRec(recs, g.GroupId);
                DecisionCenterBucket bucket = Classify(g, rec);
                bucketByGroup[g.GroupId] = bucket;

                switch (bucket)
                {
                    case DecisionCenterBucket.Critical: critical++; break;
                    case DecisionCenterBucket.Important: important++; break;
                    case DecisionCenterBucket.Opportunity: opportunity++; break;
                    default: review++; break;
                }
            }

            List<DecisionCenterPriorityItem> priorities = BuildPriorities(
                groups, recs, bucketByGroup, max);

            var summary = new DecisionCenterSummary
            {
                CriticalCount = critical,
                ImportantCount = important,
                ReviewCount = review,
                OpportunityCount = opportunity,
                TotalGroups = groups.Count,
                TotalEvents = engine.EmittedCount,
                TotalRecommendations = recs.Count,
                Headline = BuildHeadline(critical, important, review, opportunity),
                SnapshotLines = BuildSnapshotLines(snapshot)
            };

            return new DecisionCenterReport
            {
                GeneratedAt = generatedAt ?? DateTime.UtcNow,
                PeriodKey = periodKey ?? string.Empty,
                Summary = summary,
                PrioritiesToday = priorities,
                Groups = groups,
                Recommendations = recs,
                Engine = engine,
                PolicyNote = DecisionCenterPolicy.Definition
                    + " " + DecisionCenterPolicy.AntiFatigue
            };
        }

        public static DecisionCenterBucket Classify(
            DecisionGroup group,
            DecisionRecommendation? recommendation)
        {
            bool opportunity = recommendation?.IsOpportunity == true
                || group.Events.Any(e =>
                    e.EventType.Contains("opportunity", StringComparison.OrdinalIgnoreCase)
                    || (e.EventType.Contains("growth", StringComparison.OrdinalIgnoreCase)
                        && !e.EventType.Contains("decline", StringComparison.OrdinalIgnoreCase)));

            // Oportunidad gana el cubo visual aunque la prioridad sea alta
            if (opportunity
                && group.Priority != DecisionPriority.Critical
                && group.Severity != DecisionSeverity.Critical)
            {
                return DecisionCenterBucket.Opportunity;
            }

            if (group.Priority == DecisionPriority.Critical
                || group.Severity == DecisionSeverity.Critical)
                return DecisionCenterBucket.Critical;

            if (group.Priority == DecisionPriority.High
                || group.Severity == DecisionSeverity.High)
                return DecisionCenterBucket.Important;

            if (opportunity)
                return DecisionCenterBucket.Opportunity;

            return DecisionCenterBucket.Review;
        }

        public static string BucketDisplayName(DecisionCenterBucket b) => b switch
        {
            DecisionCenterBucket.Critical => "CRÍTICAS",
            DecisionCenterBucket.Important => "IMPORTANTES",
            DecisionCenterBucket.Opportunity => "OPORTUNIDADES",
            _ => "REVISAR"
        };

        private static List<DecisionCenterPriorityItem> BuildPriorities(
            IReadOnlyList<DecisionGroup> groups,
            IReadOnlyList<DecisionRecommendation> recs,
            IReadOnlyDictionary<string, DecisionCenterBucket> buckets,
            int max)
        {
            // Orden: Critical → Important → Opportunity → Review; dentro Priority↓ Severity↓
            int BucketRank(DecisionCenterBucket b) => b switch
            {
                DecisionCenterBucket.Critical => 4,
                DecisionCenterBucket.Important => 3,
                DecisionCenterBucket.Opportunity => 2,
                _ => 1
            };

            var ordered = groups
                .OrderByDescending(g => BucketRank(buckets[g.GroupId]))
                .ThenByDescending(g => (int)g.Priority)
                .ThenByDescending(g => (int)g.Severity)
                .ThenByDescending(g => g.EventCount)
                .Take(max)
                .ToList();

            var items = new List<DecisionCenterPriorityItem>(ordered.Count);
            int rank = 0;
            foreach (DecisionGroup g in ordered)
            {
                rank++;
                DecisionRecommendation? rec = FindGroupRec(recs, g.GroupId);
                DecisionCenterBucket bucket = buckets[g.GroupId];
                bool isOpp = bucket == DecisionCenterBucket.Opportunity;

                string title = BuildPriorityTitle(g, rec, bucket);
                string why = !string.IsNullOrWhiteSpace(g.Summary)
                    ? g.Summary
                    : (g.Primary?.Description ?? g.Primary?.Title ?? "Señal relevante detectada.");
                string recommendation = !string.IsNullOrWhiteSpace(g.Recommendation)
                    ? g.Recommendation
                    : rec?.DisplayText
                      ?? DecisionSoftLanguageGuard.Ensure("Revisar la señal antes de decidir.");

                items.Add(new DecisionCenterPriorityItem
                {
                    Rank = rank,
                    Bucket = bucket,
                    Title = title,
                    WhyItMatters = why,
                    Recommendation = recommendation,
                    Priority = g.Priority,
                    Severity = g.Severity,
                    GroupId = g.GroupId,
                    PrimaryEventId = g.Primary?.EventId,
                    EntityName = g.EntityName,
                    EntityType = g.EntityType,
                    SubSignals = g.Events
                        .Select(e => e.EventType)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Take(6)
                        .ToList(),
                    IsOpportunity = isOpp
                });
            }

            return items;
        }

        private static string BuildPriorityTitle(
            DecisionGroup g,
            DecisionRecommendation? rec,
            DecisionCenterBucket bucket)
        {
            string verb = bucket == DecisionCenterBucket.Opportunity ? "Evaluar" : "Revisar";
            string target = !string.IsNullOrWhiteSpace(g.EntityName)
                ? g.EntityName
                : (!string.IsNullOrWhiteSpace(g.Title) ? g.Title : "señal");

            if (rec != null && !string.IsNullOrWhiteSpace(rec.Headline))
                return rec.Headline;

            return $"{verb} {target}";
        }

        private static string BuildHeadline(int critical, int important, int review, int opportunity)
        {
            return string.Create(CultureInfo.InvariantCulture,
                $"HOY · {critical} críticos · {important} importantes · {review} a revisar · {opportunity} oportunidades");
        }

        private static IReadOnlyList<string> BuildSnapshotLines(DecisionCenterSnapshot? snapshot)
        {
            if (snapshot == null)
                return Array.Empty<string>();

            var lines = new List<string>(3);
            if (snapshot.SalesVariationPct.HasValue)
            {
                decimal v = snapshot.SalesVariationPct.Value;
                string sign = v > 0 ? "+" : string.Empty;
                lines.Add($"Ventas {sign}{v.ToString("0.#", CultureInfo.InvariantCulture)}% (variación del período)");
            }

            if (snapshot.ProfitVariationPct.HasValue)
            {
                decimal v = snapshot.ProfitVariationPct.Value;
                string sign = v > 0 ? "+" : string.Empty;
                lines.Add($"Ganancia {sign}{v.ToString("0.#", CultureInfo.InvariantCulture)}% (variación del período)");
            }

            if (snapshot.FrozenCapitalAmount.HasValue)
            {
                string label = string.IsNullOrWhiteSpace(snapshot.FrozenCapitalLabel)
                    ? "Capital congelado"
                    : snapshot.FrozenCapitalLabel.Trim();
                lines.Add(
                    $"{label}: RD${snapshot.FrozenCapitalAmount.Value.ToString("N0", CultureInfo.InvariantCulture)} (expuesto, no pérdida garantizada)");
            }

            return lines;
        }

        private static DecisionRecommendation? FindGroupRec(
            IReadOnlyList<DecisionRecommendation> recs,
            string groupId)
            => recs.FirstOrDefault(r =>
                string.Equals(r.GroupId, groupId, StringComparison.Ordinal));
    }

    /// <summary>
    /// Fachada del Centro: ejecuta el motor y compone el reporte de UI.
    /// </summary>
    public sealed class DecisionCenterService
    {
        private readonly DecisionEngine _engine;

        public DecisionCenterService(DecisionEngine? engine = null)
        {
            _engine = engine ?? new DecisionEngine();
        }

        /// <summary>Desde reporte ya evaluado (tests / binders).</summary>
        public DecisionCenterReport FromEngine(
            DecisionEngineReport engine,
            DecisionCenterSnapshot? snapshot = null,
            string? periodKey = null,
            int maxPriorities = DecisionCenterPolicy.DefaultMaxPriorities)
            => DecisionCenterComposer.Compose(engine, snapshot, periodKey, maxPriorities: maxPriorities);

        /// <summary>Ejecuta reglas inyectadas (sin DB propia).</summary>
        public DecisionCenterReport Run(
            IReadOnlyList<IDecisionRule> rules,
            DecisionRuleContext? context = null,
            DecisionCenterSnapshot? snapshot = null,
            int maxPriorities = DecisionCenterPolicy.DefaultMaxPriorities,
            bool preloadAnalytics = false,
            DecisionAnalyticsBundleHooks? analyticsHooks = null)
        {
            DecisionRuleContext ctx = context ?? new DecisionRuleContext();
            if (preloadAnalytics)
                ctx = DecisionAnalyticsBundleLoader.EnsureAnalytics(ctx, analyticsHooks);

            DecisionEngineReport engine = _engine.Run(rules, ctx);
            return DecisionCenterComposer.Compose(
                engine,
                snapshot,
                periodKey: ctx.PeriodKey,
                maxPriorities: maxPriorities);
        }

        /// <summary>
        /// Reglas built-in: precarga métricas agregadas UNA vez (FASE 10.27),
        /// luego evalúa reglas en memoria.
        /// </summary>
        public DecisionCenterReport RunBuiltIn(
            DecisionRuleContext? context = null,
            DecisionCenterSnapshot? snapshot = null,
            int maxPriorities = DecisionCenterPolicy.DefaultMaxPriorities,
            DecisionAnalyticsBundleHooks? analyticsHooks = null)
            => Run(
                DecisionRuleRegistry.BuiltIn,
                context,
                snapshot,
                maxPriorities,
                preloadAnalytics: true,
                analyticsHooks);
    }
}
