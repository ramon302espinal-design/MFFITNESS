namespace CORE.Update
{
    /// <summary>
    /// Objetivo local de actualización (sin GitHub Releases todavía).
    /// </summary>
    public sealed class UpdateTarget
    {
        public string TargetAppVersion { get; init; } = "1.0.0";
        public int TargetDbVersion { get; init; } = 1;

        /// <summary>Opcional: carpeta de migraciones (pruebas / UpdateManager).</summary>
        public string? MigrationsDirectory { get; init; }
    }
}
