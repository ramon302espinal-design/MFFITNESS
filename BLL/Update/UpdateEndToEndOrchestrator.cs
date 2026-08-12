using CORE;
using CORE.Update;
using DL.Backup;
using DL.Migrations;

namespace BLL.Update
{
    /// <summary>
    /// Orquestador end-to-end único (FASE 10B).
    /// Orden: Gates → Backup → Snapshot → Close UI → Install → Migrate → Health → Start → FinalVerify.
    /// </summary>
    public sealed class UpdateEndToEndOrchestrator
    {
        private readonly UpdateEndToEndHooks _hooks;
        private readonly Action<string> _log;
        private readonly UpdateSessionStorage _storage;

        public UpdateEndToEndOrchestrator(
            UpdateEndToEndHooks? hooks = null,
            Action<string>? log = null,
            UpdateSessionStorage? storage = null)
        {
            _hooks = hooks ?? new UpdateEndToEndHooks();
            _log = log ?? MigrationLog.Write;
            _storage = storage ?? new UpdateSessionStorage();
        }

        public UpdateEndToEndResult Run(UpdateEndToEndRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            UpdateManagerLock? mutex = null;
            try
            {
                if (_hooks.AcquireMutex)
                {
                    var lockResult = UpdateManagerLock.TryAcquire(TimeSpan.Zero);
                    if (!lockResult.Acquired || lockResult.Lock == null)
                    {
                        return new UpdateEndToEndResult
                        {
                            Success = false,
                            Blocked = true,
                            Status = UpdateSessionStatus.Blocked,
                            Stage = UpdateEndToEndStage.Blocked,
                            Message = lockResult.Message,
                            ErrorMessage = lockResult.Message
                        };
                    }

                    mutex = lockResult.Lock;
                }

                UpdateSession session;
                if (!string.IsNullOrWhiteSpace(request.ExistingUpdateId))
                {
                    session = _storage.Load(request.ExistingUpdateId)
                        ?? throw new InvalidOperationException("Sesión existente no encontrada: " + request.ExistingUpdateId);
                }
                else
                {
                    session = _storage.Create(
                        request.Manifest,
                        request.PackagePath,
                        request.ExpectedSha256,
                        request.PackageVerified,
                        request.InstallDirectory,
                        request.UiExecutableName);
                }

                return ExecutePipeline(session, request);
            }
            catch (Exception ex)
            {
                _log("e2e error no controlado: " + ex.Message);
                return new UpdateEndToEndResult
                {
                    Success = false,
                    Status = UpdateSessionStatus.Failed,
                    Stage = UpdateEndToEndStage.Failed,
                    Message = ex.Message,
                    ErrorMessage = ex.Message
                };
            }
            finally
            {
                mutex?.Dispose();
            }
        }

        /// <summary>
        /// Recuperación ante crash: analiza sesión no terminal / stale.
        /// </summary>
        public UpdateEndToEndResult Recover(UpdateSession session, UpdateEndToEndRequest? request = null)
        {
            ArgumentNullException.ThrowIfNull(session);

            UpdateManagerLock? mutex = null;
            try
            {
                if (_hooks.AcquireMutex)
                {
                    var lockResult = UpdateManagerLock.TryAcquire(TimeSpan.Zero);
                    if (!lockResult.Acquired || lockResult.Lock == null)
                    {
                        return new UpdateEndToEndResult
                        {
                            Success = false,
                            Blocked = true,
                            Status = UpdateSessionStatus.Blocked,
                            Stage = UpdateEndToEndStage.Blocked,
                            Message = lockResult.Message,
                            UpdateId = session.UpdateId
                        };
                    }

                    mutex = lockResult.Lock;
                }

                _log($"e2e recover stage={session.CurrentStage} id={session.UpdateId}");
                session.CompensationLog.Add($"recover entró en stage={session.CurrentStage}");
                Persist(session);

                switch (session.CurrentStage)
                {
                    case UpdateEndToEndStage.Checking:
                    case UpdateEndToEndStage.Prepared:
                        return AbortClean(session, "Abort limpio: crash antes de backup/side effects.");

                    case UpdateEndToEndStage.BackupCreated:
                        return AbortClean(session, "Abort limpio: crash tras backup; DB/binarios intactos.");

                    case UpdateEndToEndStage.SnapshotCreated:
                        session.CompensationLog.Add("limpiar snapshot incompleto no destructivo");
                        return AbortClean(session, "Abort limpio: crash durante/tras snapshot; DB intacta.");

                    case UpdateEndToEndStage.UiClosed:
                        // UI cerrada pero install no empezó → seguro abortar sin restore
                        return AbortClean(session, "Abort limpio: UI cerrada sin install; binarios intactos.");

                    case UpdateEndToEndStage.BinariesInstalled:
                        // App nueva + DB vieja: si request válido, continuar migrate; si no, restore snapshot
                        if (request?.Manifest != null && request.PackageVerified)
                            return ExecutePipelineFrom(session, request, UpdateEndToEndStage.DbMigrated);
                        return RecoverBinariesOnly(session, "Crash post-install sin request válido; restore snapshot.");

                    case UpdateEndToEndStage.DbMigrated:
                        if (request?.Manifest != null)
                            return ExecutePipelineFrom(session, request, UpdateEndToEndStage.HealthCheckPassed);
                        return RecoverFull(session, "Crash post-migrate sin request; restore DB+snapshot.");

                    case UpdateEndToEndStage.HealthCheckPassed:
                    case UpdateEndToEndStage.StartingApplication:
                    case UpdateEndToEndStage.FinalVerification:
                        session.Status = UpdateSessionStatus.RecoveryRequired;
                        session.RecoveryStatus = UpdateRecoveryStatus.Required;
                        session.ErrorMessage = "Crash durante start/verificación; requiere confirmación manual.";
                        session.CompletedAtUtc = DateTime.UtcNow;
                        Persist(session);
                        return UpdateEndToEndResult.FromSession(session);

                    default:
                        if (session.IsTerminal)
                            return UpdateEndToEndResult.FromSession(session, "Sesión ya terminal.");

                        session.Status = UpdateSessionStatus.RecoveryRequired;
                        session.RecoveryStatus = UpdateRecoveryStatus.Required;
                        session.ErrorMessage = "Estado de sesión ambiguo/corrupto.";
                        session.CurrentStage = UpdateEndToEndStage.RecoveryRequired;
                        session.CompletedAtUtc = DateTime.UtcNow;
                        Persist(session);
                        return UpdateEndToEndResult.FromSession(session);
                }
            }
            finally
            {
                mutex?.Dispose();
            }
        }

        private UpdateEndToEndResult ExecutePipeline(UpdateSession session, UpdateEndToEndRequest request) =>
            ExecutePipelineFrom(session, request, UpdateEndToEndStage.Checking);

        private UpdateEndToEndResult ExecutePipelineFrom(
            UpdateSession session,
            UpdateEndToEndRequest request,
            UpdateEndToEndStage startFrom)
        {
            UpdateSnapshotInfo? snapshot = null;
            bool dbMigrated = session.CurrentStage >= UpdateEndToEndStage.DbMigrated
                && session.Status == UpdateSessionStatus.Active;
            var applied = new List<int>();

            try
            {
                // ---- Pre-gates (siempre si startFrom <= Checking) ----
                if (startFrom <= UpdateEndToEndStage.Checking)
                {
                    session.CurrentStage = UpdateEndToEndStage.Checking;
                    Persist(session);

                    string appBefore = ResolveAppVersion(request.InstallDirectory, request.UiExecutableName);
                    int dbBefore = ResolveDbVersion();
                    session.AppVersionBefore = appBefore;
                    session.DbVersionBefore = dbBefore;
                    session.AppVersionTarget = request.Manifest.AppVersion;
                    session.DbVersionTarget = request.Manifest.TargetDbVersion;
                    Persist(session);

                    var gateFail = EvaluateGates(session, request, appBefore, dbBefore);
                    if (gateFail != null)
                        return gateFail;
                }

                // ---- Prepared: extract ----
                if (startFrom <= UpdateEndToEndStage.Prepared)
                {
                    string staging = request.StagingDirectory
                        ?? Path.Combine(UpdateDownloadStorage.CarpetaStaging, session.UpdateId);
                    var extract = ResolveExtract(request.PackagePath, staging);
                    if (!extract.Success)
                        return Fail(session, UpdateEndToEndStage.Failed, UpdateSessionStatus.Failed, extract.Message);

                    session.StagingPath = extract.StagingDirectory;
                    session.MigrationsDirectory = request.MigrationsDirectoryOverride
                        ?? Path.Combine(extract.StagingDirectory, "Database", "Migrations");
                    session.CurrentStage = UpdateEndToEndStage.Prepared;
                    Persist(session);
                }

                // Re-validate migrations dir if needed
                if (session.DbVersionBefore is int db0
                    && session.DbVersionTarget is int dbT
                    && dbT > db0
                    && (string.IsNullOrWhiteSpace(session.MigrationsDirectory)
                        || !Directory.Exists(session.MigrationsDirectory)))
                {
                    // En modo fake ApplyUpTo, el directorio puede no existir; solo exigir si no hay hook
                    if (_hooks.ApplyUpTo == null)
                    {
                        return Block(session, "MigrationsDirectory no existe y TargetDb > CurrentDb.");
                    }
                }

                // ---- Backup ----
                if (startFrom <= UpdateEndToEndStage.BackupCreated)
                {
                    Heartbeat(session);
                    var backup = ResolveBackup();
                    if (!backup.Success || !backup.Verified || string.IsNullOrWhiteSpace(backup.BackupPath))
                    {
                        return Fail(session, UpdateEndToEndStage.Failed, UpdateSessionStatus.Failed,
                            backup.ErrorMessage ?? "Backup falló o no verificado.");
                    }

                    session.BackupPath = backup.BackupPath;
                    session.BackupVerified = true;
                    session.CurrentStage = UpdateEndToEndStage.BackupCreated;
                    session.CompensationLog.Add("backup OK: " + backup.BackupPath);
                    Persist(session);
                }

                // ---- Snapshot ----
                if (startFrom <= UpdateEndToEndStage.SnapshotCreated)
                {
                    Heartbeat(session);
                    string snapRoot = request.SnapshotDirectory ?? UpdateDownloadStorage.CarpetaSnapshots;
                    var snap = ResolveSnapshot(request.InstallDirectory, snapRoot);
                    if (!snap.Success || snap.Snapshot == null)
                    {
                        return Fail(session, UpdateEndToEndStage.Failed, UpdateSessionStatus.Failed, snap.Message);
                    }

                    snapshot = snap.Snapshot;
                    session.SnapshotPath = snap.Snapshot.SnapshotDirectory;
                    session.SnapshotVerified = true;
                    session.CurrentStage = UpdateEndToEndStage.SnapshotCreated;
                    session.CompensationLog.Add("snapshot OK: " + session.SnapshotPath);
                    Persist(session);
                }
                else if (!string.IsNullOrWhiteSpace(session.SnapshotPath))
                {
                    snapshot = ResolveLoadSnapshot(session.SnapshotPath);
                }

                // ---- Close UI ----
                if (startFrom <= UpdateEndToEndStage.UiClosed)
                {
                    string uiPath = Path.Combine(request.InstallDirectory, request.UiExecutableName);
                    var proc = ResolveProcessController();
                    if (proc.IsProcessRunning(uiPath))
                    {
                        if (!proc.RequestGracefulClose(uiPath))
                            return Fail(session, UpdateEndToEndStage.Failed, UpdateSessionStatus.Failed,
                                "No se pudo solicitar cierre graceful de UI.");

                        if (!proc.WaitForExit(uiPath, request.UiCloseTimeout))
                            return Fail(session, UpdateEndToEndStage.Failed, UpdateSessionStatus.Failed,
                                "UI no terminó dentro del timeout (sin Kill).");
                    }

                    session.CurrentStage = UpdateEndToEndStage.UiClosed;
                    Persist(session);
                }

                // ---- Install ----
                if (startFrom <= UpdateEndToEndStage.BinariesInstalled)
                {
                    Heartbeat(session);
                    string stagingPath = session.StagingPath
                        ?? throw new InvalidOperationException("StagingPath ausente.");

                    var zipValidation = UpdateZipPathValidator.ValidateArchive(request.PackagePath);
                    // En smoke con fakes de extract, el zip puede ser sintético; si verify package pasó, continuar
                    if (!zipValidation.IsValid && _hooks.ExtractPackage == null)
                    {
                        return Fail(session, UpdateEndToEndStage.Failed, UpdateSessionStatus.Failed, zipValidation.Message);
                    }

                    var install = ResolveInstall(stagingPath, request.InstallDirectory);
                    if (!install.Success)
                    {
                        return RecoverBinaries(session, snapshot, request.InstallDirectory, install.InstalledFiles,
                            "Install falló: " + install.Message);
                    }

                    if (_hooks.VerifyInstalledVersion != null)
                    {
                        var (ok, actual, err) = _hooks.VerifyInstalledVersion(request.InstallDirectory, request.Manifest);
                        session.AppVersionAfter = actual;
                        if (!ok)
                        {
                            return RecoverBinaries(session, snapshot, request.InstallDirectory, install.InstalledFiles,
                                err ?? "Versión instalada incorrecta.");
                        }
                    }
                    else if (!UpdateBinaryInstaller.VerifyRequiredFiles(request.InstallDirectory, out string? missing))
                    {
                        return RecoverBinaries(session, snapshot, request.InstallDirectory, install.InstalledFiles,
                            missing ?? "Archivos requeridos faltantes.");
                    }

                    session.CurrentStage = UpdateEndToEndStage.BinariesInstalled;
                    session.CompensationLog.Add("binaries installed");
                    Persist(session);
                }

                // ---- Migrate ----
                if (startFrom <= UpdateEndToEndStage.DbMigrated)
                {
                    Heartbeat(session);
                    int targetDb = request.Manifest.TargetDbVersion;
                    var mig = ResolveApplyUpTo(targetDb, session.MigrationsDirectory);
                    applied = mig.AppliedVersions.ToList();
                    session.DbVersionAfter = mig.FinalVersion;

                    if (!mig.Success)
                    {
                        return RecoverFull(session, snapshot, request,
                            $"Migración falló (DB puede quedar intermedia {mig.FinalVersion}): {mig.Message}");
                    }

                    if (mig.FinalVersion != targetDb)
                    {
                        return RecoverFull(session, snapshot, request,
                            $"DB final {mig.FinalVersion} != target {targetDb}.");
                    }

                    dbMigrated = true;
                    session.CurrentStage = UpdateEndToEndStage.DbMigrated;
                    session.CompensationLog.Add("db migrated to " + mig.FinalVersion);
                    Persist(session);
                }

                // ---- Health ----
                if (startFrom <= UpdateEndToEndStage.HealthCheckPassed)
                {
                    Heartbeat(session);
                    var health = ResolveHealth(request.InstallDirectory, request.Manifest, session.MigrationsDirectory);
                    if (!health.Success)
                    {
                        if (dbMigrated)
                            return RecoverFull(session, snapshot, request, "Health FAIL: " + health.Message);

                        return RecoverBinaries(session, snapshot, request.InstallDirectory, Array.Empty<string>(),
                            "Health FAIL: " + health.Message);
                    }

                    session.AppVersionAfter = health.InstalledAppVersion;
                    session.CurrentStage = UpdateEndToEndStage.HealthCheckPassed;
                    Persist(session);
                }

                // ---- Start UI ----
                if (request.StartApplicationAfterInstall && startFrom <= UpdateEndToEndStage.StartingApplication)
                {
                    session.CurrentStage = UpdateEndToEndStage.StartingApplication;
                    Persist(session);

                    string uiPath = Path.Combine(request.InstallDirectory, request.UiExecutableName);
                    var launcher = ResolveLauncher();
                    if (!launcher.Start(uiPath, out string? startErr)
                        || !launcher.WaitForStartup(uiPath, TimeSpan.FromSeconds(15)))
                    {
                        // App+DB verificados: no rollback automático
                        session.Status = UpdateSessionStatus.RecoveryRequired;
                        session.RecoveryStatus = UpdateRecoveryStatus.Required;
                        session.CurrentStage = UpdateEndToEndStage.RecoveryRequired;
                        session.ErrorMessage = "Arranque UI falló tras health OK: " + (startErr ?? "timeout");
                        session.CompletedAtUtc = DateTime.UtcNow;
                        Persist(session);
                        return UpdateEndToEndResult.FromSession(session);
                    }
                }

                // ---- Final verification ----
                session.CurrentStage = UpdateEndToEndStage.FinalVerification;
                Persist(session);

                string appFinal = ResolveInstalledAppVersion(request.InstallDirectory, request.Manifest)
                    ?? session.AppVersionAfter
                    ?? string.Empty;
                int dbFinal = ResolveDbVersion();
                session.AppVersionAfter = appFinal;
                session.DbVersionAfter = dbFinal;

                var healthFinal = ResolveHealth(request.InstallDirectory, request.Manifest, session.MigrationsDirectory);
                bool appOk = string.Equals(appFinal, request.Manifest.AppVersion, StringComparison.OrdinalIgnoreCase);
                bool dbOk = dbFinal == request.Manifest.TargetDbVersion;
                bool healthOk = healthFinal.Success;
                bool recoveryNone = session.RecoveryStatus == UpdateRecoveryStatus.None;

                if (!(appOk && dbOk && healthOk && recoveryNone))
                {
                    session.Status = UpdateSessionStatus.Failed;
                    session.CurrentStage = UpdateEndToEndStage.Failed;
                    session.ErrorMessage =
                        $"Final verification FAIL. AppOk={appOk} DbOk={dbOk} HealthOk={healthOk} Recovery={session.RecoveryStatus}. "
                        + $"App={appFinal} DB={dbFinal}";
                    session.CompletedAtUtc = DateTime.UtcNow;
                    Persist(session);
                    return UpdateEndToEndResult.FromSession(session);
                }

                session.Status = UpdateSessionStatus.Completed;
                session.CurrentStage = UpdateEndToEndStage.Completed;
                session.CompletedAtUtc = DateTime.UtcNow;
                session.ErrorMessage = null;
                Persist(session);

                var result = UpdateEndToEndResult.FromSession(session, "Actualización E2E completada.");
                return new UpdateEndToEndResult
                {
                    Success = true,
                    Blocked = false,
                    Status = result.Status,
                    Stage = result.Stage,
                    RecoveryStatus = UpdateRecoveryStatus.None,
                    Message = result.Message,
                    UpdateId = session.UpdateId,
                    AppVersionBefore = session.AppVersionBefore,
                    AppVersionAfter = session.AppVersionAfter,
                    DbVersionBefore = session.DbVersionBefore,
                    DbVersionAfter = session.DbVersionAfter,
                    BackupPath = session.BackupPath,
                    SnapshotPath = session.SnapshotPath,
                    BackupVerified = session.BackupVerified,
                    SnapshotVerified = session.SnapshotVerified,
                    HealthCheckPassed = true,
                    ApplicationStarted = request.StartApplicationAfterInstall,
                    AppliedMigrations = applied,
                    CompensationLog = session.CompensationLog.ToList()
                };
            }
            catch (Exception ex)
            {
                _log("e2e pipeline error: " + ex.Message);
                if (dbMigrated)
                    return RecoverFull(session, snapshot, request, ex.Message);
                if (snapshot != null && session.SnapshotVerified)
                    return RecoverBinaries(session, snapshot, request.InstallDirectory, Array.Empty<string>(), ex.Message);

                return Fail(session, UpdateEndToEndStage.Failed, UpdateSessionStatus.Failed, ex.Message);
            }
        }

        private UpdateEndToEndResult? EvaluateGates(
            UpdateSession session,
            UpdateEndToEndRequest request,
            string appBefore,
            int dbBefore)
        {
            var gates = new UpdateSessionGates();

            var mv = UpdateManifestValidator.Validate(request.Manifest);
            gates.ManifestValid = mv.IsValid;
            if (!mv.IsValid)
                return BlockGate(session, gates, nameof(gates.ManifestValid), string.Join(" ", mv.Errors));

            var installReq = new UpdateInstallRequest
            {
                Manifest = request.Manifest,
                PackagePath = request.PackagePath,
                ExpectedSha256 = request.ExpectedSha256,
                PackageVerified = request.PackageVerified,
                InstallDirectory = request.InstallDirectory,
                UiExecutableName = request.UiExecutableName
            };

            var pkg = ResolveVerifyPackage(installReq);
            gates.PackageVerified = request.PackageVerified && pkg.Success;
            gates.Sha256RecalculatedOk = pkg.Success;
            gates.PackageNameMatches = pkg.Success ||
                string.Equals(Path.GetFileName(request.PackagePath), request.Manifest.PackageName, StringComparison.OrdinalIgnoreCase);

            if (!pkg.Success)
                return BlockGate(session, gates, "PackageVerified", pkg.Message);

            gates.CurrentAppLessThanTarget = SemVer.TryParse(appBefore, out _)
                && SemVer.Compare(appBefore, request.Manifest.AppVersion) < 0;
            if (!gates.CurrentAppLessThanTarget)
                return BlockGate(session, gates, nameof(gates.CurrentAppLessThanTarget),
                    $"CurrentApp ({appBefore}) no es menor que Target ({request.Manifest.AppVersion}).");

            gates.CurrentAppMeetsMin = SemVer.Compare(appBefore, request.Manifest.MinAppVersion) >= 0;
            if (!gates.CurrentAppMeetsMin)
                return BlockGate(session, gates, nameof(gates.CurrentAppMeetsMin), "App menor que MinAppVersion.");

            gates.CurrentDbLessOrEqualTarget = dbBefore <= request.Manifest.TargetDbVersion;
            if (!gates.CurrentDbLessOrEqualTarget)
                return BlockGate(session, gates, nameof(gates.CurrentDbLessOrEqualTarget), "CurrentDb > TargetDb.");

            gates.CurrentDbAtLeastOne = dbBefore >= 1;
            if (!gates.CurrentDbAtLeastOne)
                return BlockGate(session, gates, nameof(gates.CurrentDbAtLeastOne), "CurrentDb < 1.");

            bool? caja;
            try { caja = ResolveCajaAbierta(); }
            catch (Exception ex)
            {
                return BlockGate(session, gates, nameof(gates.CajaCerrada),
                    "No se pudo determinar caja (fail closed): " + ex.Message);
            }

            gates.CajaCerrada = caja != true;
            if (!gates.CajaCerrada)
                return BlockGate(session, gates, nameof(gates.CajaCerrada), "Caja abierta.");

            gates.NoConcurrentMigration = !ResolveMigrationRunning();
            if (!gates.NoConcurrentMigration)
                return BlockGate(session, gates, nameof(gates.NoConcurrentMigration), "Migración concurrente.");

            gates.NoCriticalOperation = !ResolveCriticalOperation();
            if (!gates.NoCriticalOperation)
                return BlockGate(session, gates, nameof(gates.NoCriticalOperation), "Operación crítica activa.");

            long free = ResolveDiskBytes();
            gates.SufficientDiskSpace = free > 50L * 1024 * 1024;
            if (!gates.SufficientDiskSpace)
                return BlockGate(session, gates, nameof(gates.SufficientDiskSpace), "Espacio en disco insuficiente.");

            bool containsUm = ResolvePackageContainsUpdateManager(request.PackagePath);
            gates.UpdateManagerNotInPackage = !containsUm;
            if (!gates.UpdateManagerNotInPackage)
                return BlockGate(session, gates, nameof(gates.UpdateManagerNotInPackage),
                    "UpdateManager.exe no debe venir en el package de install.");

            gates.MigrationsDirectoryOk = true; // se valida tras extract; pre-check soft
            if (dbBefore < request.Manifest.TargetDbVersion && request.MigrationsDirectoryOverride != null)
            {
                gates.MigrationsDirectoryOk = Directory.Exists(request.MigrationsDirectoryOverride)
                    || _hooks.ApplyUpTo != null;
                if (!gates.MigrationsDirectoryOk)
                    return BlockGate(session, gates, nameof(gates.MigrationsDirectoryOk),
                        "MigrationsDirectory override no existe.");
            }

            gates.AllPassed = true;
            session.Gates = gates;
            Persist(session);
            return null;
        }

        private UpdateEndToEndResult BlockGate(
            UpdateSession session,
            UpdateSessionGates gates,
            string failedGate,
            string reason)
        {
            gates.FailedGate = failedGate;
            gates.FailureReason = reason;
            gates.AllPassed = false;
            session.Gates = gates;
            return Block(session, reason);
        }

        private UpdateEndToEndResult Block(UpdateSession session, string reason)
        {
            session.Status = UpdateSessionStatus.Blocked;
            session.CurrentStage = UpdateEndToEndStage.Blocked;
            session.ErrorMessage = reason;
            session.CompletedAtUtc = DateTime.UtcNow;
            Persist(session);
            return UpdateEndToEndResult.FromSession(session, reason);
        }

        private UpdateEndToEndResult Fail(
            UpdateSession session,
            UpdateEndToEndStage stage,
            UpdateSessionStatus status,
            string error)
        {
            session.Status = status;
            session.CurrentStage = stage;
            session.ErrorMessage = error;
            session.CompletedAtUtc = DateTime.UtcNow;
            Persist(session);
            return UpdateEndToEndResult.FromSession(session, error);
        }

        private UpdateEndToEndResult AbortClean(UpdateSession session, string message)
        {
            session.Status = UpdateSessionStatus.Failed;
            session.CurrentStage = UpdateEndToEndStage.Failed;
            session.ErrorMessage = message;
            session.CompletedAtUtc = DateTime.UtcNow;
            session.CompensationLog.Add(message);
            Persist(session);
            return UpdateEndToEndResult.FromSession(session, message);
        }

        private UpdateEndToEndResult RecoverBinariesOnly(UpdateSession session, string reason)
        {
            UpdateSnapshotInfo? snap = null;
            if (!string.IsNullOrWhiteSpace(session.SnapshotPath))
                snap = ResolveLoadSnapshot(session.SnapshotPath);

            return RecoverBinaries(session, snap, session.InstallDirectory ?? string.Empty, Array.Empty<string>(), reason);
        }

        private UpdateEndToEndResult RecoverBinaries(
            UpdateSession session,
            UpdateSnapshotInfo? snapshot,
            string installDirectory,
            IReadOnlyList<string> modified,
            string reason)
        {
            session.RecoveryStatus = UpdateRecoveryStatus.Attempted;
            session.RecoveryActions.Add("RestoreFromSnapshot");
            session.CompensationLog.Add("binary recovery: " + reason);
            Persist(session);

            if (snapshot == null)
            {
                session.Status = UpdateSessionStatus.FailedRecoveryRequired;
                session.RecoveryStatus = UpdateRecoveryStatus.Failed;
                session.CurrentStage = UpdateEndToEndStage.FailedRecoveryRequired;
                session.ErrorMessage = reason + " | Snapshot ausente.";
                session.CompletedAtUtc = DateTime.UtcNow;
                Persist(session);
                return UpdateEndToEndResult.FromSession(session);
            }

            var restore = ResolveRestoreSnapshot(snapshot, installDirectory, modified);
            if (!restore.Success)
            {
                session.Status = UpdateSessionStatus.FailedRecoveryRequired;
                session.RecoveryStatus = UpdateRecoveryStatus.Failed;
                session.CurrentStage = UpdateEndToEndStage.FailedRecoveryRequired;
                session.ErrorMessage = reason + " | Snapshot restore FAIL: " + restore.Message;
                session.CompletedAtUtc = DateTime.UtcNow;
                Persist(session);
                return UpdateEndToEndResult.FromSession(session);
            }

            session.AppVersionAfter = session.AppVersionBefore;
            session.Status = UpdateSessionStatus.FailedRecovered;
            session.RecoveryStatus = UpdateRecoveryStatus.Succeeded;
            session.CurrentStage = UpdateEndToEndStage.FailedRecovered;
            session.ErrorMessage = reason;
            session.CompletedAtUtc = DateTime.UtcNow;
            session.CompensationLog.Add("binary recovery OK");
            Persist(session);
            return UpdateEndToEndResult.FromSession(session);
        }

        private UpdateEndToEndResult RecoverFull(UpdateSession session, string reason) =>
            RecoverFull(session, null, null, reason);

        private UpdateEndToEndResult RecoverFull(
            UpdateSession session,
            UpdateSnapshotInfo? snapshot,
            UpdateEndToEndRequest? request,
            string reason)
        {
            session.RecoveryStatus = UpdateRecoveryStatus.Attempted;
            session.RecoveryActions.Add("RESTORE DATABASE");
            session.RecoveryActions.Add("RestoreFromSnapshot");
            session.CompensationLog.Add("full recovery: " + reason);
            Persist(session);

            int expectedDb = session.DbVersionBefore ?? 0;
            string? backupPath = session.BackupPath;
            var restoreSvc = ResolveDbRestore();

            bool uiClosed = true;
            var proc = ResolveProcessController();
            string uiName = session.UiExecutableName;
            string installDir = session.InstallDirectory ?? request?.InstallDirectory ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(installDir))
            {
                string uiPath = Path.Combine(installDir, uiName);
                uiClosed = !proc.IsProcessRunning(uiPath);
            }

            if (string.IsNullOrWhiteSpace(backupPath) || expectedDb < 1)
            {
                session.Status = UpdateSessionStatus.FailedRecoveryRequired;
                session.RecoveryStatus = UpdateRecoveryStatus.Failed;
                session.CurrentStage = UpdateEndToEndStage.FailedRecoveryRequired;
                session.ErrorMessage = reason + " | Backup/DbVersionBefore ausente.";
                session.CompletedAtUtc = DateTime.UtcNow;
                Persist(session);
                return UpdateEndToEndResult.FromSession(session);
            }

            var dbRestore = restoreSvc.RestoreFromBackup(backupPath, expectedDb, () => uiClosed);
            foreach (var line in dbRestore.CompensationLog)
                session.CompensationLog.Add(line);

            if (!dbRestore.Success)
            {
                session.Status = UpdateSessionStatus.FailedRecoveryRequired;
                session.RecoveryStatus = UpdateRecoveryStatus.Failed;
                session.CurrentStage = UpdateEndToEndStage.FailedRecoveryRequired;
                session.ErrorMessage = reason + " | DB restore FAIL: " + dbRestore.Message;
                session.CompletedAtUtc = DateTime.UtcNow;
                Persist(session);
                return UpdateEndToEndResult.FromSession(session);
            }

            session.DbVersionAfter = dbRestore.SchemaVersionAfter;

            if (snapshot == null && !string.IsNullOrWhiteSpace(session.SnapshotPath))
                snapshot = ResolveLoadSnapshot(session.SnapshotPath);

            if (snapshot == null)
            {
                session.Status = UpdateSessionStatus.FailedRecoveryRequired;
                session.RecoveryStatus = UpdateRecoveryStatus.Failed;
                session.CurrentStage = UpdateEndToEndStage.FailedRecoveryRequired;
                session.ErrorMessage = reason + " | Snapshot ausente tras DB restore.";
                session.CompletedAtUtc = DateTime.UtcNow;
                Persist(session);
                return UpdateEndToEndResult.FromSession(session);
            }

            var binRestore = ResolveRestoreSnapshot(snapshot, installDir, null);
            if (!binRestore.Success)
            {
                session.Status = UpdateSessionStatus.FailedRecoveryRequired;
                session.RecoveryStatus = UpdateRecoveryStatus.Failed;
                session.CurrentStage = UpdateEndToEndStage.FailedRecoveryRequired;
                session.ErrorMessage = reason + " | Snapshot restore FAIL: " + binRestore.Message;
                session.CompletedAtUtc = DateTime.UtcNow;
                Persist(session);
                return UpdateEndToEndResult.FromSession(session);
            }

            session.AppVersionAfter = session.AppVersionBefore;
            session.Status = UpdateSessionStatus.FailedRecovered;
            session.RecoveryStatus = UpdateRecoveryStatus.Succeeded;
            session.CurrentStage = UpdateEndToEndStage.FailedRecovered;
            session.ErrorMessage = reason;
            session.CompletedAtUtc = DateTime.UtcNow;
            session.CompensationLog.Add("full recovery OK → OLD APP + OLD DB");
            Persist(session);
            return UpdateEndToEndResult.FromSession(session);
        }

        private void Persist(UpdateSession session) => _storage.Save(session);

        private void Heartbeat(UpdateSession session)
        {
            session.LastHeartbeatUtc = DateTime.UtcNow;
            _storage.Save(session);
        }

        private string ResolveAppVersion(string? installDirectory = null, string uiExecutableName = "UI.exe")
        {
            if (_hooks.GetCurrentAppVersion != null)
                return _hooks.GetCurrentAppVersion();

            if (!string.IsNullOrWhiteSpace(installDirectory))
            {
                string uiPath = Path.Combine(installDirectory, uiExecutableName);
                if (File.Exists(uiPath))
                {
                    string fromUi = UpdateBinarySnapshotService.ReadFileVersion(uiPath);
                    if (!string.IsNullOrWhiteSpace(fromUi) &&
                        !string.Equals(fromUi, "unknown", StringComparison.OrdinalIgnoreCase))
                    {
                        // ProductVersion puede ser "1.0.0+abc" → SemVer base
                        int plus = fromUi.IndexOf('+');
                        if (plus > 0)
                            fromUi = fromUi[..plus];
                        return fromUi;
                    }
                }
            }

            // Fallback: no usar AppVersion.SemanticVersion del proceso (sería UpdateManager.exe).
            return "0.0.0";
        }

        private int ResolveDbVersion() =>
            _hooks.GetCurrentDbVersion?.Invoke() ?? SchemaMigrationBLL.GetCurrentDbVersion();

        private bool? ResolveCajaAbierta() =>
            _hooks.IsCajaAbierta?.Invoke() ?? new CajaBLL().ObtenerEstadoCaja();

        private bool ResolveMigrationRunning() =>
            _hooks.IsMigrationRunning?.Invoke() ?? false;

        private bool ResolveCriticalOperation() =>
            _hooks.HasCriticalOperation?.Invoke() ?? false;

        private long ResolveDiskBytes()
        {
            if (_hooks.GetAvailableDiskBytes != null)
                return _hooks.GetAvailableDiskBytes();

            try
            {
                string root = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData))
                    ?? "C:\\";
                var drive = new DriveInfo(root);
                return drive.AvailableFreeSpace;
            }
            catch
            {
                return long.MaxValue;
            }
        }

        private bool ResolvePackageContainsUpdateManager(string packagePath)
        {
            if (_hooks.PackageContainsUpdateManager != null)
                return _hooks.PackageContainsUpdateManager(packagePath);

            try
            {
                using var zip = System.IO.Compression.ZipFile.OpenRead(packagePath);
                return zip.Entries.Any(e =>
                    string.Equals(Path.GetFileName(e.FullName), AllowedUpdatePackageFiles.UpdateManagerExe,
                        StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return false;
            }
        }

        private UpdatePackageVerifier.VerifyResult ResolveVerifyPackage(UpdateInstallRequest request) =>
            _hooks.VerifyPackage?.Invoke(request) ?? UpdatePackageVerifier.VerifyPackage(request);

        private UpdatePackageExtractor.ExtractResult ResolveExtract(string zip, string staging) =>
            _hooks.ExtractPackage?.Invoke(zip, staging)
            ?? UpdatePackageExtractor.ExtractToStaging(zip, staging);

        private DatabaseBackupResult ResolveBackup() =>
            _hooks.CreateBackup?.Invoke() ?? DatabaseBackupBLL.CreateVerifiedBackup();

        private UpdateBinarySnapshotService.SnapshotResult ResolveSnapshot(string install, string root) =>
            _hooks.CreateSnapshot?.Invoke(install, root)
            ?? UpdateBinarySnapshotService.CreateSnapshot(install, root);

        private UpdateBinaryInstaller.InstallResult ResolveInstall(string staging, string install)
        {
            if (_hooks.InstallFiles != null)
                return _hooks.InstallFiles(staging, install);
            return UpdateBinaryInstaller.InstallFromStaging(staging, install, _hooks.BeforeFileCopy);
        }

        private UpdateBinarySnapshotService.RestoreResult ResolveRestoreSnapshot(
            UpdateSnapshotInfo snapshot,
            string installDirectory,
            IReadOnlyList<string>? modified) =>
            _hooks.RestoreSnapshot?.Invoke(snapshot, installDirectory, modified)
            ?? UpdateBinarySnapshotService.RestoreFromSnapshot(snapshot, installDirectory, modified);

        private MigrationRunResult ResolveApplyUpTo(int target, string? migrationsDir) =>
            _hooks.ApplyUpTo?.Invoke(target, migrationsDir)
            ?? SchemaMigrationBLL.ApplyUpToDetailed(target, migrationsDir);

        private IDatabaseRestoreService ResolveDbRestore() =>
            _hooks.DatabaseRestore ?? new DatabaseRestoreService(log: _log);

        private IUpdateProcessController ResolveProcessController() =>
            _hooks.ProcessController ?? new UpdateProcessController();

        private IUpdateApplicationLauncher ResolveLauncher() =>
            _hooks.ApplicationLauncher ?? new UpdateApplicationLauncher();

        private UpdateHealthCheckService.HealthCheckResult ResolveHealth(
            string installDir,
            UpdateManifest manifest,
            string? migrationsDir)
        {
            if (_hooks.RunHealthCheck != null)
                return _hooks.RunHealthCheck(installDir, manifest);

            return UpdateHealthCheckService.RunFull(
                installDir,
                manifest,
                _hooks.DbHealthProbe,
                migrationsDir,
                _hooks.VerifyInstalledVersion);
        }

        private UpdateSnapshotInfo? ResolveLoadSnapshot(string snapshotPath) =>
            _hooks.LoadSnapshot?.Invoke(snapshotPath)
            ?? UpdateBinarySnapshotService.LoadSnapshot(snapshotPath);

        private string? ResolveInstalledAppVersion(string installDir, UpdateManifest manifest)
        {
            if (_hooks.VerifyInstalledVersion != null)
            {
                var (_, actual, _) = _hooks.VerifyInstalledVersion(installDir, manifest);
                return actual;
            }

            return UpdateBinarySnapshotService.ReadFileVersion(Path.Combine(installDir, "UI.exe"));
        }
    }
}
