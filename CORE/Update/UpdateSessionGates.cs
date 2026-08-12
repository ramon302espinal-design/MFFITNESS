namespace CORE.Update
{
    /// <summary>
    /// Resultado de los pre-gates evaluados antes de side effects.
    /// </summary>
    public sealed class UpdateSessionGates
    {
        public bool ManifestValid { get; set; }
        public bool PackageVerified { get; set; }
        public bool Sha256RecalculatedOk { get; set; }
        public bool PackageNameMatches { get; set; }
        public bool CurrentAppLessThanTarget { get; set; }
        public bool CurrentAppMeetsMin { get; set; }
        public bool CurrentDbLessOrEqualTarget { get; set; }
        public bool CurrentDbAtLeastOne { get; set; }
        public bool CajaCerrada { get; set; }
        public bool NoConcurrentMigration { get; set; }
        public bool NoCriticalOperation { get; set; }
        public bool SufficientDiskSpace { get; set; }
        public bool UpdateManagerNotInPackage { get; set; }
        public bool MigrationsDirectoryOk { get; set; }

        /// <summary>True solo si todos los gates aplicables pasaron.</summary>
        public bool AllPassed { get; set; }

        public string? FailedGate { get; set; }
        public string? FailureReason { get; set; }
    }
}
