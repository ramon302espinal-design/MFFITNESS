namespace CORE.Update
{
    /// <summary>
    /// Contrato de una release disponible.
    /// Evolucionable: hoy es local; mañana puede mapearse desde GitHub Releases.
    /// </summary>
    public sealed record UpdateManifest
    {
        public string AppVersion { get; init; } = string.Empty;
        public int TargetDbVersion { get; init; }
        public string MinAppVersion { get; init; } = string.Empty;
        public string PackageName { get; init; } = string.Empty;
        public string PackageSha256 { get; init; } = string.Empty;
        public DateTime ReleaseDate { get; init; }
        public string? ReleaseNotesUrl { get; init; }
    }
}
