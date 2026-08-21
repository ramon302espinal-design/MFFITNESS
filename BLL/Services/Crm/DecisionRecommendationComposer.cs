using System.Globalization;
using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>
    /// Motor de recomendaciones (FASE 10.19). Puro, sin I/O, sin side-effects.
    /// </summary>
    public static class DecisionRecommendationComposer
    {
        public static DecisionRecommendation FromEvent(DecisionEvent e)
        {
            DecisionRecommendationTemplate? tpl = DecisionRecommendationCatalog.Find(e.EventType);
            DecisionRecommendationTemplate fallback = tpl
                ?? DecisionRecommendationCatalog.Fallback(e.Area);

            DecisionRecommendationVerb verb = tpl?.Verb
                ?? (DecisionSoftLanguageGuard.StartsWithSoftVerb(e.Recommendation)
                    ? DecisionSoftLanguageGuard.DetectVerb(e.Recommendation)
                    : fallback.Verb);

            string preferredBody = !string.IsNullOrWhiteSpace(e.Recommendation)
                ? e.Recommendation
                : fallback.Body;

            string body = DecisionSoftLanguageGuard.Ensure(
                preferredBody, verb, fallback.Body);

            string headline = BuildEventHeadline(e, fallback.Headline, verb);

            return new DecisionRecommendation
            {
                RecommendationId = StableId("evt", e.EventId.ToString("N")),
                EventId = e.EventId,
                EventType = e.EventType,
                Verb = DecisionSoftLanguageGuard.DetectVerb(body),
                Headline = headline,
                Body = body,
                SuggestedChecks = fallback.SuggestedChecks,
                PolicyReminders = DecisionRecommendationCatalog.DefaultPolicyReminders,
                IsOpportunity = fallback.IsOpportunity
                    || e.EventType.Contains("opportunity", StringComparison.OrdinalIgnoreCase)
                    || e.EventType.Contains("growth", StringComparison.OrdinalIgnoreCase),
                SoftLanguageCompliant = DecisionSoftLanguageGuard.IsCompliant(body)
            };
        }

        public static DecisionRecommendation FromGroup(DecisionGroup g)
        {
            if (g.Events.Count == 1)
            {
                DecisionRecommendation single = FromEvent(g.Events[0]);
                return new DecisionRecommendation
                {
                    RecommendationId = StableId("grp", g.GroupId),
                    GroupId = g.GroupId,
                    EventId = single.EventId,
                    EventType = single.EventType,
                    Verb = single.Verb,
                    Headline = single.Headline,
                    Body = single.Body,
                    SuggestedChecks = single.SuggestedChecks,
                    PolicyReminders = single.PolicyReminders,
                    IsOpportunity = single.IsOpportunity,
                    SoftLanguageCompliant = single.SoftLanguageCompliant
                };
            }

            DecisionEvent? primary = g.Primary ?? g.Events.FirstOrDefault();
            DecisionRecommendationVerb verb = DecisionRecommendationVerb.Revisar;
            bool opportunity = g.Events.Any(e =>
                e.EventType.Contains("opportunity", StringComparison.OrdinalIgnoreCase)
                || e.EventType.Contains("growth", StringComparison.OrdinalIgnoreCase));

            if (opportunity)
                verb = DecisionRecommendationVerb.Evaluar;

            string entity = !string.IsNullOrWhiteSpace(g.EntityName)
                ? g.EntityName
                : g.Title;

            string types = string.Join(", ",
                g.Events.Select(e => e.EventType).Distinct(StringComparer.OrdinalIgnoreCase).Take(5));

            string body = DecisionSoftLanguageGuard.Ensure(
                $"{DecisionSoftLanguageGuard.VerbText(verb)} {entity}: " +
                $"{g.EventCount} señales relacionadas ({types}). " +
                "Considerar una estrategia conjunta antes de nuevas compras o inversiones.",
                verb);

            string headline = opportunity
                ? $"Evaluar oportunidad — {entity}"
                : $"Revisar {entity} ({g.EventCount} señales)";

            var checks = new List<string>();
            foreach (DecisionEvent e in g.Events.Take(4))
            {
                DecisionRecommendationTemplate? t = DecisionRecommendationCatalog.Find(e.EventType);
                if (t != null)
                    checks.AddRange(t.SuggestedChecks.Take(1));
            }

            if (checks.Count == 0)
                checks.Add("Evidencia de cada subseñal");

            return new DecisionRecommendation
            {
                RecommendationId = StableId("grp", g.GroupId),
                GroupId = g.GroupId,
                EventId = primary?.EventId,
                EventType = primary?.EventType,
                Verb = DecisionSoftLanguageGuard.DetectVerb(body),
                Headline = headline,
                Body = body,
                SuggestedChecks = checks.Distinct(StringComparer.OrdinalIgnoreCase).Take(6).ToList(),
                PolicyReminders = DecisionRecommendationCatalog.DefaultPolicyReminders,
                IsOpportunity = opportunity,
                SoftLanguageCompliant = DecisionSoftLanguageGuard.IsCompliant(body)
            };
        }

        /// <summary>Aplica Body suave al campo Recommendation del evento.</summary>
        public static DecisionEvent ApplyToEvent(DecisionEvent e)
        {
            DecisionRecommendation rec = FromEvent(e);
            if (string.Equals(e.Recommendation, rec.Body, StringComparison.Ordinal))
                return e;

            return CloneEvent(e, rec.Body);
        }

        /// <summary>Adjunta Recommendation narrativa al grupo + eventos suavizados.</summary>
        public static DecisionGroup ApplyToGroup(DecisionGroup g)
        {
            List<DecisionEvent> events = g.Events.Select(ApplyToEvent).ToList();
            DecisionRecommendation rec = FromGroup(new DecisionGroup
            {
                GroupId = g.GroupId,
                GroupKey = g.GroupKey,
                Title = g.Title,
                Summary = g.Summary,
                EntityType = g.EntityType,
                EntityId = g.EntityId,
                EntityName = g.EntityName,
                Severity = g.Severity,
                Priority = g.Priority,
                Events = events,
                Primary = events.Count > 0
                    ? events.OrderByDescending(x => (int)x.Priority)
                        .ThenByDescending(x => (int)x.Severity)
                        .First()
                    : null
            });

            return new DecisionGroup
            {
                GroupId = g.GroupId,
                GroupKey = g.GroupKey,
                Title = g.Title,
                Summary = g.Summary,
                EntityType = g.EntityType,
                EntityId = g.EntityId,
                EntityName = g.EntityName,
                Severity = g.Severity,
                Priority = g.Priority,
                Events = events,
                Primary = events.Count > 0
                    ? events.OrderByDescending(x => (int)x.Priority)
                        .ThenByDescending(x => (int)x.Severity)
                        .First()
                    : null,
                Recommendation = rec.DisplayText
            };
        }

        public static IReadOnlyList<DecisionRecommendation> ComposeReport(
            IReadOnlyList<DecisionEvent> events,
            IReadOnlyList<DecisionGroup> groups)
        {
            var list = new List<DecisionRecommendation>();

            // Preferir recomendaciones de grupo (TEST 10 / no saturar)
            foreach (DecisionGroup g in groups)
                list.Add(FromGroup(g));

            // Eventos huérfanos (no deberían quedar si Group cubre todo)
            var covered = new HashSet<Guid>(
                groups.SelectMany(g => g.Events).Select(e => e.EventId));

            foreach (DecisionEvent e in events)
            {
                if (!covered.Contains(e.EventId))
                    list.Add(FromEvent(e));
            }

            return list;
        }

        private static string BuildEventHeadline(
            DecisionEvent e,
            string templateHeadline,
            DecisionRecommendationVerb verb)
        {
            if (!string.IsNullOrWhiteSpace(e.EntityName)
                && e.EntityType is DecisionEntityType.Product or DecisionEntityType.Investment)
            {
                return $"{DecisionSoftLanguageGuard.VerbText(verb)} {e.EntityName}";
            }

            return string.IsNullOrWhiteSpace(templateHeadline)
                ? DecisionSoftLanguageGuard.VerbText(verb) + " señal"
                : templateHeadline;
        }

        private static DecisionEvent CloneEvent(DecisionEvent e, string recommendation)
            => new()
            {
                EventId = e.EventId,
                EventType = e.EventType,
                Area = e.Area,
                EntityType = e.EntityType,
                EntityId = e.EntityId,
                EntityName = e.EntityName,
                PeriodKey = e.PeriodKey,
                DetectedAt = e.DetectedAt,
                CreatedAt = e.CreatedAt,
                Severity = e.Severity,
                Priority = e.Priority,
                Title = e.Title,
                Description = e.Description,
                Reason = e.Reason,
                Impact = e.Impact,
                Recommendation = recommendation,
                Status = e.Status,
                Source = e.Source,
                Fingerprint = e.Fingerprint,
                Evidence = e.Evidence,
                MetricKeys = e.MetricKeys
            };

        private static string StableId(string prefix, string key)
        {
            unchecked
            {
                int hash = 17;
                foreach (char c in prefix + "|" + key)
                    hash = hash * 31 + c;
                return prefix + "_" + ((uint)hash).ToString("X8", CultureInfo.InvariantCulture);
            }
        }
    }
}
