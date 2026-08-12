namespace CORE.Update
{
    public sealed class UpdateSnapshotFileEntry
    {
        public string RelativePath { get; init; } = string.Empty;
        public long SizeBytes { get; init; }
        public string Sha256 { get; init; } = string.Empty;
    }

    public sealed class UpdateSnapshotInfo
    {
        public string SnapshotId { get; init; } = string.Empty;
        public string SnapshotDirectory { get; init; } = string.Empty;
        public DateTimeOffset CreatedAtUtc { get; init; }
        public string AppVersion { get; init; } = string.Empty;
        public string InformationalVersion { get; init; } = string.Empty;
        public string InstallDirectory { get; init; } = string.Empty;
        public IReadOnlyList<UpdateSnapshotFileEntry> Files { get; init; } = Array.Empty<UpdateSnapshotFileEntry>();
    }
}
