using BLL.Models.Crm;

namespace BLL.Services.Crm
{
    /// <summary>Textos de presentación del Centro (FASE 10.24) — sin I/O.</summary>
    public static class DecisionCenterDisplay
    {
        public static string BucketPrefix(DecisionCenterBucket bucket) => bucket switch
        {
            DecisionCenterBucket.Critical => "CRÍTICA",
            DecisionCenterBucket.Important => "IMPORTANTE",
            DecisionCenterBucket.Opportunity => "OPORTUNIDAD",
            _ => "REVISAR"
        };

        public static string PriorityLine(DecisionCenterPriorityItem item, int maxLen = 110)
        {
            ArgumentNullException.ThrowIfNull(item);
            string line = $"{item.Rank}. [{BucketPrefix(item.Bucket)}] {item.Title}";
            if (!string.IsNullOrWhiteSpace(item.Recommendation))
            {
                string rec = item.Recommendation.Trim();
                if (rec.Length > 48)
                    rec = rec[..45] + "…";
                line += " — " + rec;
            }

            if (maxLen > 20 && line.Length > maxLen)
                return line[..(maxLen - 1)] + "…";
            return line;
        }

        public static string SummaryLine(DecisionCenterSummary summary)
        {
            ArgumentNullException.ThrowIfNull(summary);
            return string.IsNullOrWhiteSpace(summary.Headline)
                ? "HOY · sin señales"
                : summary.Headline;
        }

        public static IReadOnlyList<string> DashboardLines(
            DecisionCenterReport report,
            int maxLines = 3)
        {
            ArgumentNullException.ThrowIfNull(report);
            if (maxLines <= 0)
                return Array.Empty<string>();

            var lines = new List<string>(maxLines);
            foreach (DecisionCenterPriorityItem p in report.PrioritiesToday.Take(maxLines))
                lines.Add(PriorityLine(p));

            if (lines.Count == 0)
                lines.Add(SummaryLine(report.Summary));

            foreach (string snap in report.Summary.SnapshotLines)
            {
                if (lines.Count >= maxLines)
                    break;
                lines.Add(snap);
            }

            while (lines.Count < maxLines)
                lines.Add(string.Empty);

            return lines;
        }

        public static int CountEventsInAreas(
            DecisionCenterReport report,
            params DecisionEventArea[] areas)
        {
            ArgumentNullException.ThrowIfNull(report);
            if (areas.Length == 0)
                return 0;

            var set = areas.ToHashSet();
            IEnumerable<DecisionEvent> events = report.Engine?.Events
                ?? report.Groups.SelectMany(g => g.Events);
            return events.Count(e => set.Contains(e.Area));
        }
    }
}
