namespace CORE.Update
{
    /// <summary>
    /// Solicitud de instalación. El paquete debe haber sido verificado (FASE 8).
    /// </summary>
    public sealed record UpdateInstallRequest
    {
        public required UpdateManifest Manifest { get; init; }

        /// <summary>Ruta absoluta al ZIP ya descargado.</summary>
        public required string PackagePath { get; init; }

        /// <summary>SHA256 esperado (del manifest / FASE 8). Se recalcula antes de instalar.</summary>
        public required string ExpectedSha256 { get; init; }

        /// <summary>True solo si FASE 8 devolvió SuccessVerified.</summary>
        public bool PackageVerified { get; init; }

        /// <summary>Carpeta de instalación del POS (donde está UI.exe).</summary>
        public required string InstallDirectory { get; init; }

        /// <summary>Nombre del proceso/exe del POS (por defecto UI.exe).</summary>
        public string UiExecutableName { get; init; } = "UI.exe";

        /// <summary>Timeout para cierre graceful de UI.exe.</summary>
        public TimeSpan UiCloseTimeout { get; init; } = TimeSpan.FromSeconds(30);

        /// <summary>Si true, intenta iniciar UI.exe tras instalación exitosa.</summary>
        public bool StartApplicationAfterInstall { get; init; } = true;

        public string? StagingDirectory { get; init; }
        public string? SnapshotDirectory { get; init; }
    }
}
