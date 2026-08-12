namespace DL.Migrations
{
    public sealed class MigrationDefinition
    {
        public int Version { get; init; }
        public string Name { get; init; } = string.Empty;
        public string FilePath { get; init; } = string.Empty;
        public string FileName => Path.GetFileName(FilePath);
    }
}
