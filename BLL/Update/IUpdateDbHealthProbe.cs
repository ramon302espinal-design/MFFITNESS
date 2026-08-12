namespace BLL.Update
{
    public sealed class UpdateDbHealthProbeResult
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public int? SchemaVersion { get; init; }
        public bool SqlConnected { get; init; }
        public bool SchemaVersionExists { get; init; }
        public bool MatchesTarget { get; init; }
        public bool NoPendingUntilTarget { get; init; }
        public bool IntegrityQueryOk { get; init; }
    }

    public interface IUpdateDbHealthProbe
    {
        UpdateDbHealthProbeResult Probe(int targetDbVersion, string? migrationsDirectory);
    }
}
