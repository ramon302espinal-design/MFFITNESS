using CORE;
using CORE.Update;
using DL.Backup;
using DL.Migrations;

namespace BLL.Update
{
    /// <summary>
    /// Hooks inyectables para pruebas (caja abierta / backup fallido) sin tocar datos reales.
    /// Null = implementación productiva.
    /// </summary>
    public sealed class UpdateOrchestratorHooks
    {
        public Func<string>? GetCurrentAppVersion { get; init; }
        public Func<int>? GetCurrentDbVersion { get; init; }
        public Func<bool>? IsCajaAbierta { get; init; }
        public Func<DatabaseBackupResult>? CreateBackup { get; init; }
        public Func<string?, MigrationRunResult>? ApplyMigrations { get; init; }
    }

    /// <summary>
    /// Orquestador V0: versión app/DB, caja, backup, migraciones, verificación.
    /// Sin descargas ni reemplazo de binarios. Sin WinForms.
    /// </summary>
    public sealed class UpdateOrchestrator
    {
        private readonly UpdateOrchestratorHooks _hooks;
        private readonly Action<string> _log;

        public UpdateOrchestrator(UpdateOrchestratorHooks? hooks = null, Action<string>? log = null)
        {
            _hooks = hooks ?? new UpdateOrchestratorHooks();
            _log = log ?? MigrationLog.Write;
        }

        public UpdateResult Run(UpdateTarget target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            string currentApp = string.Empty;
            string targetApp = target.TargetAppVersion?.Trim() ?? string.Empty;
            int currentDb = 0;
            int targetDb = target.TargetDbVersion;
            bool? cajaAbierta = null;
            bool backupCreated = false;
            bool backupVerified = false;
            string? backupPath = null;
            bool migrationsApplied = false;
            IReadOnlyList<int> applied = Array.Empty<int>();
            int finalDb = 0;

            try
            {
                _log("update inicio");
                _log($"update target app={targetApp} db={targetDb}");

                currentApp = ResolveAppVersion();
                _log($"update versión app actual: {currentApp}");

                currentDb = ResolveDbVersion();
                finalDb = currentDb;
                _log($"update versión DB actual: {currentDb}");

                if (targetDb < 1)
                {
                    return Fail(currentApp, targetApp, currentDb, targetDb, finalDb, cajaAbierta,
                        backupCreated, backupVerified, backupPath, migrationsApplied, applied,
                        "TargetDbVersion inválido.", UpdateStage.Failed);
                }

                if (string.IsNullOrWhiteSpace(targetApp))
                {
                    return Fail(currentApp, targetApp, currentDb, targetDb, finalDb, cajaAbierta,
                        backupCreated, backupVerified, backupPath, migrationsApplied, applied,
                        "TargetAppVersion vacío.", UpdateStage.Failed);
                }

                if (targetDb < currentDb)
                {
                    return Fail(currentApp, targetApp, currentDb, targetDb, finalDb, cajaAbierta,
                        backupCreated, backupVerified, backupPath, migrationsApplied, applied,
                        $"No se puede bajar SchemaVersion de {currentDb} a {targetDb}.", UpdateStage.Failed);
                }

                // --- Caja (fail closed) ---
                try
                {
                    cajaAbierta = ResolveCajaAbierta();
                    _log($"update estado caja: {(cajaAbierta == true ? "ABIERTA" : "CERRADA")}");
                }
                catch (Exception ex)
                {
                    _log($"update error consultando caja: {ex.Message}");
                    return Fail(currentApp, targetApp, currentDb, targetDb, finalDb, null,
                        backupCreated, backupVerified, backupPath, migrationsApplied, applied,
                        "No se pudo determinar el estado de caja. Actualización cancelada (fail closed).",
                        UpdateStage.Failed);
                }

                if (cajaAbierta == true)
                {
                    _log("update bloqueada: caja abierta");
                    return UpdateResult.CreateBlocked(
                        currentApp,
                        targetApp,
                        currentDb,
                        targetDb,
                        true,
                        "Caja abierta. La actualización requiere caja cerrada.");
                }

                // --- Backup ---
                _log("update backup iniciado");
                DatabaseBackupResult backup = ResolveBackup();
                backupCreated = backup.Success && !string.IsNullOrEmpty(backup.BackupPath);
                backupVerified = backup.Success && backup.Verified;
                backupPath = backup.BackupPath;

                if (!backup.Success || !backup.Verified || !backupCreated)
                {
                    string err = string.IsNullOrWhiteSpace(backup.ErrorMessage)
                        ? "Backup fallido o no verificado."
                        : backup.ErrorMessage;
                    _log($"update backup fallido: {err}");
                    return Fail(currentApp, targetApp, currentDb, targetDb, finalDb, cajaAbierta,
                        backupCreated, backupVerified, backupPath, migrationsApplied, applied,
                        "Backup fallido. No se ejecutan migraciones. " + err,
                        UpdateStage.Failed);
                }

                _log($"update backup completado: {backupPath} size={backup.SizeBytes} verified={backup.Verified}");

                // --- Migraciones ---
                if (currentDb >= targetDb)
                {
                    _log($"update migraciones: no necesarias (DB {currentDb} >= target {targetDb})");
                    finalDb = ResolveDbVersion();
                }
                else
                {
                    _log($"update migraciones iniciadas (hacia {targetDb})");
                    MigrationRunResult mig = ResolveMigrations(target.MigrationsDirectory);
                    applied = mig.AppliedVersions;
                    migrationsApplied = mig.AppliedVersions.Count > 0;
                    finalDb = mig.FinalVersion;

                    if (!mig.Success)
                    {
                        _log($"update migraciones fallidas: {mig.Message}");
                        // Releer por si el runner hizo rollback parcial correcto
                        try { finalDb = ResolveDbVersion(); } catch { /* keep mig.FinalVersion */ }
                        return Fail(currentApp, targetApp, currentDb, targetDb, finalDb, cajaAbierta,
                            backupCreated, backupVerified, backupPath, migrationsApplied, applied,
                            "Migración fallida. " + mig.Message,
                            UpdateStage.Failed);
                    }

                    _log($"update migraciones OK: aplicadas=[{string.Join(",", applied)}] versión={finalDb}");
                }

                // --- Verificación ---
                _log("update verificando SchemaVersion");
                finalDb = ResolveDbVersion();
                _log($"update versión DB final: {finalDb}");

                if (finalDb != targetDb)
                {
                    return Fail(currentApp, targetApp, currentDb, targetDb, finalDb, cajaAbierta,
                        backupCreated, backupVerified, backupPath, migrationsApplied, applied,
                        $"SchemaVersion final ({finalDb}) no coincide con target ({targetDb}).",
                        UpdateStage.Failed);
                }

                _log("update resultado: SUCCESS");
                return UpdateResult.CreateSuccess(
                    currentApp,
                    targetApp,
                    currentDb,
                    targetDb,
                    finalDb,
                    cajaAbierta == true,
                    backupCreated,
                    backupVerified,
                    backupPath,
                    migrationsApplied,
                    applied);
            }
            catch (Exception ex)
            {
                _log($"update error: {ex.Message}");
                return Fail(currentApp, targetApp, currentDb, targetDb, finalDb, cajaAbierta,
                    backupCreated, backupVerified, backupPath, migrationsApplied, applied,
                    ex.Message,
                    UpdateStage.Failed);
            }
        }

        private string ResolveAppVersion() =>
            _hooks.GetCurrentAppVersion?.Invoke() ?? AppVersion.SemanticVersion;

        private int ResolveDbVersion() =>
            _hooks.GetCurrentDbVersion?.Invoke() ?? SchemaMigrationBLL.GetCurrentDbVersion();

        private bool ResolveCajaAbierta() =>
            _hooks.IsCajaAbierta?.Invoke() ?? new CajaBLL().ObtenerEstadoCaja();

        private DatabaseBackupResult ResolveBackup() =>
            _hooks.CreateBackup?.Invoke() ?? DatabaseBackupBLL.CreateVerifiedBackup();

        private MigrationRunResult ResolveMigrations(string? directory) =>
            _hooks.ApplyMigrations?.Invoke(directory) ?? SchemaMigrationBLL.ApplyPendingDetailed(directory);

        private static UpdateResult Fail(
            string currentApp,
            string targetApp,
            int currentDb,
            int targetDb,
            int finalDb,
            bool? cajaAbierta,
            bool backupCreated,
            bool backupVerified,
            string? backupPath,
            bool migrationsApplied,
            IReadOnlyList<int> applied,
            string error,
            UpdateStage stage)
        {
            MigrationLog.Write($"update resultado: FAILED stage={stage} {error}");
            return UpdateResult.CreateFailed(
                currentApp, targetApp, currentDb, targetDb, finalDb, cajaAbierta,
                backupCreated, backupVerified, backupPath, migrationsApplied, applied, error, stage);
        }
    }
}
