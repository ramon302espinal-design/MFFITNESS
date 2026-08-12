using CORE;
using CORE.Update;

namespace BLL.Update
{
    /// <summary>
    /// Hooks inyectables para smoke tests (caja, procesos, filesystem).
    /// </summary>
    public sealed record UpdateInstallerHooks
    {
        public Func<bool>? IsCajaAbierta { get; init; }
        public Func<bool>? IsMigrationRunning { get; init; }
        public Func<bool>? HasCriticalOperation { get; init; }
        public IUpdateProcessController? ProcessController { get; init; }
        public IUpdateApplicationLauncher? ApplicationLauncher { get; init; }
        public Func<UpdateInstallRequest, UpdatePackageVerifier.VerifyResult>? VerifyPackage { get; init; }
        public Func<string, string, UpdateBinarySnapshotService.SnapshotResult>? CreateSnapshot { get; init; }
        public Func<string, string, UpdatePackageExtractor.ExtractResult>? ExtractPackage { get; init; }
        public Func<string, string, UpdateBinaryInstaller.InstallResult>? InstallFiles { get; init; }
        public Func<UpdateSnapshotInfo, string, IReadOnlyList<string>?, UpdateBinarySnapshotService.RestoreResult>? RestoreSnapshot { get; init; }
        public Func<string, UpdateManifest, UpdateHealthCheckService.HealthCheckResult>? RunHealthCheck { get; init; }
        public Func<string, UpdateManifest, (bool Ok, string? Actual, string? Error)>? VerifyInstalledVersion { get; init; }
        /// <summary>Hook de prueba: lanza al copiar el archivo relativo indicado.</summary>
        public Action<string>? BeforeFileCopy { get; init; }
    }

    /// <summary>
    /// Instalador seguro: staging → validación → snapshot → cierre UI → copia → recovery ante fallo → health-check.
    /// Ejecutado desde UpdateManager.exe (proceso externo).
    /// </summary>
    public sealed class UpdateInstaller
    {
        private readonly UpdateInstallerHooks _hooks;
        private readonly Action<string> _log;

        public UpdateInstaller(UpdateInstallerHooks? hooks = null, Action<string>? log = null)
        {
            _hooks = hooks ?? new UpdateInstallerHooks();
            _log = log ?? MigrationLog.Write;
        }

        public UpdateInstallationResult Install(UpdateInstallRequest request)
        {
            var state = new UpdateInstallationResult
            {
                ExpectedAppVersion = request.Manifest.AppVersion,
                TargetDbVersion = request.Manifest.TargetDbVersion
            };

            UpdateSnapshotInfo? activeSnapshot = null;

            try
            {
                _log("install inicio");

                if (string.IsNullOrWhiteSpace(request.InstallDirectory))
                    return Fail(UpdateInstallationStage.Checking, "InstallDirectory vacío.", state);

                string uiPath = Path.Combine(request.InstallDirectory, request.UiExecutableName);

                var packageVerify = ResolveVerifyPackage(request);
                if (!packageVerify.Success)
                    return Fail(UpdateInstallationStage.Checking, packageVerify.Message, state);

                _log("install paquete verificado");

                bool? cajaAbierta;
                try
                {
                    cajaAbierta = ResolveCajaAbierta();
                    state = state with { CajaAbierta = cajaAbierta };
                    _log($"install caja: {(cajaAbierta == true ? "ABIERTA" : "CERRADA")}");
                }
                catch (Exception ex)
                {
                    _log($"install error caja: {ex.Message}");
                    return Fail(UpdateInstallationStage.Preparing,
                        "No se pudo determinar el estado de caja. Instalación cancelada (fail closed).",
                        state);
                }

                if (cajaAbierta == true)
                {
                    return UpdateInstallationResult.CreateBlocked(
                        UpdateInstallationStage.Blocked,
                        "Caja abierta. La instalación requiere caja cerrada.",
                        cajaAbierta);
                }

                if (ResolveMigrationRunning())
                {
                    return UpdateInstallationResult.CreateBlocked(
                        UpdateInstallationStage.Blocked,
                        "Migración en ejecución. Instalación bloqueada.",
                        cajaAbierta);
                }

                if (ResolveCriticalOperation())
                {
                    return UpdateInstallationResult.CreateBlocked(
                        UpdateInstallationStage.Blocked,
                        "Operación crítica activa. Instalación bloqueada.",
                        cajaAbierta);
                }

                string stagingDir = request.StagingDirectory
                    ?? Path.Combine(UpdateDownloadStorage.CarpetaStaging, Guid.NewGuid().ToString("N"));

                string snapshotRoot = request.SnapshotDirectory ?? UpdateDownloadStorage.CarpetaSnapshots;

                _log("install extrayendo a staging");
                var extract = ResolveExtract(request.PackagePath, stagingDir);
                if (!extract.Success)
                {
                    return Fail(UpdateInstallationStage.Extracting, extract.Message, state with { StagingPath = stagingDir });
                }

                state = state with
                {
                    StagingPath = extract.StagingDirectory,
                    PackageExtracted = true
                };

                _log("install validando paquete extraído");
                var zipValidation = UpdateZipPathValidator.ValidateArchive(request.PackagePath);
                if (!zipValidation.IsValid)
                    return Fail(UpdateInstallationStage.ValidatingPackage, zipValidation.Message, state);

                _log("install creando snapshot");
                var snapshot = ResolveSnapshot(request.InstallDirectory, snapshotRoot);
                if (!snapshot.Success || snapshot.Snapshot == null)
                {
                    return Fail(UpdateInstallationStage.Preparing, snapshot.Message, state);
                }

                activeSnapshot = snapshot.Snapshot;
                state = state with
                {
                    SnapshotCreated = true,
                    SnapshotPath = snapshot.Snapshot.SnapshotDirectory
                };

                _log("install solicitando cierre UI");
                if (ResolveProcessController().IsProcessRunning(uiPath))
                {
                    bool requested = ResolveProcessController().RequestGracefulClose(uiPath);
                    if (!requested)
                    {
                        return Fail(UpdateInstallationStage.StoppingApplication,
                            "No se pudo solicitar cierre graceful de UI.exe.",
                            state);
                    }

                    bool exited = ResolveProcessController().WaitForExit(uiPath, request.UiCloseTimeout);
                    if (!exited)
                    {
                        return Fail(UpdateInstallationStage.StoppingApplication,
                            "UI.exe no terminó dentro del timeout. Instalación abortada (sin Kill automático).",
                            state);
                    }
                }

                _log("install UI cerrada");

                _log("install copiando archivos");
                var install = ResolveInstall(stagingDir, request.InstallDirectory);
                if (!install.Success)
                {
                    state = state with { InstalledFiles = install.InstalledFiles };
                    _log($"install copia fallida en {install.FailedOnFile ?? "?"}: {install.Message}");
                    return RecoverOrFail(install.Message, activeSnapshot, request.InstallDirectory, state);
                }

                state = state with
                {
                    FilesInstalled = true,
                    InstalledFiles = install.InstalledFiles
                };

                _log("install verificando archivos");
                if (!UpdateBinaryInstaller.VerifyRequiredFiles(request.InstallDirectory, out string? missing))
                {
                    return RecoverOrFail(missing ?? "Verificación fallida.", activeSnapshot, request.InstallDirectory, state);
                }

                string? actualVersion;
                string? versionError;
                bool versionOk;
                if (_hooks.VerifyInstalledVersion != null)
                {
                    (versionOk, actualVersion, versionError) = _hooks.VerifyInstalledVersion(request.InstallDirectory, request.Manifest);
                }
                else if (!UpdateBinaryInstaller.VerifyAppVersion(
                             request.InstallDirectory,
                             request.Manifest.AppVersion,
                             out actualVersion,
                             out versionError))
                {
                    versionOk = false;
                }
                else
                {
                    versionOk = true;
                }

                if (!versionOk)
                {
                    state = state with { InstalledAppVersion = actualVersion };
                    return RecoverOrFail(versionError ?? "Versión incorrecta.", activeSnapshot, request.InstallDirectory, state);
                }

                state = state with { InstalledAppVersion = actualVersion };

                _log("install health-check");
                var health = ResolveHealthCheck(request.InstallDirectory, request.Manifest);
                if (!health.Success)
                {
                    return RecoverOrFail(health.Message, activeSnapshot, request.InstallDirectory, state);
                }

                state = state with { HealthCheckPassed = true, InstalledAppVersion = health.InstalledAppVersion };

                if (request.StartApplicationAfterInstall)
                {
                    _log("install iniciando UI");
                    if (!ResolveLauncher().Start(uiPath, out string? startError))
                    {
                        // Binarios ya instalados y verificados; fallo de arranque no revierte (FASE 10).
                        return Fail(UpdateInstallationStage.StartingApplication,
                            "Arranque fallido: " + (startError ?? "desconocido"),
                            state);
                    }

                    if (!ResolveLauncher().WaitForStartup(uiPath, TimeSpan.FromSeconds(15)))
                    {
                        return Fail(UpdateInstallationStage.StartingApplication,
                            "UI.exe no confirmó arranque dentro del timeout.",
                            state);
                    }

                    state = state with { ApplicationStarted = true };
                }

                _log("install completada");
                return UpdateInstallationResult.CreateCompleted("Instalación completada.", state);
            }
            catch (Exception ex)
            {
                _log($"install error: {ex.Message}");
                if (activeSnapshot != null && state.SnapshotCreated)
                    return RecoverOrFail(ex.Message, activeSnapshot, request.InstallDirectory, state);

                return Fail(UpdateInstallationStage.Failed, ex.Message, state);
            }
        }

        private UpdateInstallationResult RecoverOrFail(
            string installError,
            UpdateSnapshotInfo snapshot,
            string installDirectory,
            UpdateInstallationResult state)
        {
            _log("install recovery: restaurando binarios desde snapshot");

            var restore = ResolveRestore(snapshot, installDirectory, state.InstalledFiles);
            state = state with
            {
                RecoveryAttempted = true,
                RecoverySucceeded = restore.Success,
                RecoveredFiles = restore.RestoredFiles,
                RecoveryFailedFiles = restore.FailedFiles,
                FilesInstalled = false
            };

            if (restore.Success)
            {
                _log("install recovery OK (FailedRecovered)");
                return UpdateInstallationResult.CreateFailedRecovered(installError, state);
            }

            _log("install recovery FAILED (FailedRecoveryRequired)");
            return UpdateInstallationResult.CreateFailedRecoveryRequired(installError, restore.Message, state);
        }

        private UpdatePackageVerifier.VerifyResult ResolveVerifyPackage(UpdateInstallRequest request) =>
            _hooks.VerifyPackage?.Invoke(request) ?? UpdatePackageVerifier.VerifyPackage(request);

        private bool? ResolveCajaAbierta() =>
            _hooks.IsCajaAbierta?.Invoke() ?? new CajaBLL().ObtenerEstadoCaja();

        private bool ResolveMigrationRunning() =>
            _hooks.IsMigrationRunning?.Invoke() ?? false;

        private bool ResolveCriticalOperation() =>
            _hooks.HasCriticalOperation?.Invoke() ?? false;

        private IUpdateProcessController ResolveProcessController() =>
            _hooks.ProcessController ?? new UpdateProcessController();

        private IUpdateApplicationLauncher ResolveLauncher() =>
            _hooks.ApplicationLauncher ?? new UpdateApplicationLauncher();

        private UpdateBinarySnapshotService.SnapshotResult ResolveSnapshot(string installDir, string snapshotRoot) =>
            _hooks.CreateSnapshot?.Invoke(installDir, snapshotRoot)
            ?? UpdateBinarySnapshotService.CreateSnapshot(installDir, snapshotRoot);

        private UpdatePackageExtractor.ExtractResult ResolveExtract(string zipPath, string stagingDir) =>
            _hooks.ExtractPackage?.Invoke(zipPath, stagingDir)
            ?? UpdatePackageExtractor.ExtractToStaging(zipPath, stagingDir);

        private UpdateBinaryInstaller.InstallResult ResolveInstall(string stagingDir, string installDir)
        {
            if (_hooks.InstallFiles != null)
                return _hooks.InstallFiles(stagingDir, installDir);

            return UpdateBinaryInstaller.InstallFromStaging(stagingDir, installDir, _hooks.BeforeFileCopy);
        }

        private UpdateBinarySnapshotService.RestoreResult ResolveRestore(
            UpdateSnapshotInfo snapshot,
            string installDirectory,
            IReadOnlyList<string> modifiedFiles) =>
            _hooks.RestoreSnapshot?.Invoke(snapshot, installDirectory, modifiedFiles)
            ?? UpdateBinarySnapshotService.RestoreFromSnapshot(snapshot, installDirectory, modifiedFiles);

        private UpdateHealthCheckService.HealthCheckResult ResolveHealthCheck(string installDir, UpdateManifest manifest) =>
            _hooks.RunHealthCheck?.Invoke(installDir, manifest)
            ?? UpdateHealthCheckService.Run(installDir, manifest);

        private static UpdateInstallationResult Fail(
            UpdateInstallationStage stage,
            string error,
            UpdateInstallationResult partial) =>
            UpdateInstallationResult.CreateFailed(stage, error, partial);
    }
}
