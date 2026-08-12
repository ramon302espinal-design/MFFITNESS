using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CORE;

namespace CORE.Update
{
    /// <summary>
    /// Persistencia de UpdateSession en %LocalAppData%\MFFITNESS\updates\sessions\{UpdateId}.json.
    /// Escritura atómica: temporal → flush → replace.
    /// </summary>
    public sealed class UpdateSessionStorage
    {
        private readonly string _sessionsDirectory;

        private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

        public UpdateSessionStorage(string? sessionsDirectory = null)
        {
            _sessionsDirectory = string.IsNullOrWhiteSpace(sessionsDirectory)
                ? UpdateDownloadStorage.CarpetaSessions
                : sessionsDirectory;

            Directory.CreateDirectory(_sessionsDirectory);
        }

        public string SessionsDirectory => _sessionsDirectory;

        public static JsonSerializerOptions SharedJsonOptions => JsonOptions;

        public UpdateSession Create(
            UpdateManifest? manifest = null,
            string? packagePath = null,
            string? packageSha256 = null,
            bool packageVerified = false,
            string? installDirectory = null,
            string? uiExecutableName = null)
        {
            DateTime now = DateTime.UtcNow;
            var session = new UpdateSession
            {
                SchemaVersion = UpdateSessionContract.CurrentSchemaVersion,
                UpdateId = Guid.NewGuid().ToString("N"),
                Status = UpdateSessionStatus.Active,
                CurrentStage = UpdateEndToEndStage.Checking,
                StartedAtUtc = now,
                LastHeartbeatUtc = now,
                Manifest = manifest,
                AppVersionTarget = manifest?.AppVersion,
                DbVersionTarget = manifest?.TargetDbVersion,
                PackagePath = packagePath,
                PackageSha256 = packageSha256,
                PackageVerified = packageVerified,
                InstallDirectory = installDirectory,
                UiExecutableName = string.IsNullOrWhiteSpace(uiExecutableName) ? "UI.exe" : uiExecutableName
            };

            Save(session);
            return session;
        }

        public UpdateSession? Load(string updateId)
        {
            if (string.IsNullOrWhiteSpace(updateId))
                return null;

            string path = GetSessionPath(updateId);
            if (!File.Exists(path))
                return null;

            try
            {
                string json = File.ReadAllText(path, Encoding.UTF8);
                var session = JsonSerializer.Deserialize<UpdateSession>(json, JsonOptions);
                if (session == null || string.IsNullOrWhiteSpace(session.UpdateId))
                    return null;

                return session;
            }
            catch (JsonException)
            {
                return null;
            }
            catch (IOException)
            {
                return null;
            }
        }

        /// <summary>
        /// Guarda la sesión de forma segura: escribe .tmp, flush, luego File.Replace / Move.
        /// Nunca escribe directamente sobre el JSON oficial.
        /// </summary>
        public void Save(UpdateSession session)
        {
            ArgumentNullException.ThrowIfNull(session);
            if (string.IsNullOrWhiteSpace(session.UpdateId))
                throw new ArgumentException("UpdateId vacío.", nameof(session));

            Directory.CreateDirectory(_sessionsDirectory);

            string finalPath = GetSessionPath(session.UpdateId);
            string tempPath = finalPath + ".tmp";
            string backupPath = finalPath + ".bak";

            session.LastHeartbeatUtc = DateTime.UtcNow;

            byte[] payload = JsonSerializer.SerializeToUtf8Bytes(session, JsonOptions);

            using (var fs = new FileStream(
                       tempPath,
                       FileMode.Create,
                       FileAccess.Write,
                       FileShare.None,
                       bufferSize: 4096,
                       FileOptions.WriteThrough))
            {
                fs.Write(payload, 0, payload.Length);
                fs.Flush(flushToDisk: true);
            }

            if (File.Exists(finalPath))
            {
                // Replace atómico relativo: final → bak, temp → final.
                File.Replace(tempPath, finalPath, backupPath, ignoreMetadataErrors: true);
                try { File.Delete(backupPath); } catch { /* best-effort cleanup */ }
            }
            else
            {
                File.Move(tempPath, finalPath);
            }
        }

        public void MarkHeartbeat(string updateId)
        {
            var session = Load(updateId)
                ?? throw new FileNotFoundException("Sesión no encontrada.", updateId);

            session.LastHeartbeatUtc = DateTime.UtcNow;
            Save(session);
        }

        public IReadOnlyList<UpdateSession> FindPendingSessions()
        {
            var result = new List<UpdateSession>();
            foreach (var session in EnumerateSessions())
            {
                if (!session.IsTerminal)
                    result.Add(session);
            }

            return result.OrderBy(s => s.StartedAtUtc).ToList();
        }

        public IReadOnlyList<UpdateSession> FindStaleSessions(TimeSpan staleThreshold)
        {
            if (staleThreshold <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(staleThreshold));

            DateTime cutoff = DateTime.UtcNow - staleThreshold;
            var result = new List<UpdateSession>();

            foreach (var session in EnumerateSessions())
            {
                if (session.IsTerminal)
                    continue;

                if (session.LastHeartbeatUtc < cutoff)
                    result.Add(session);
            }

            return result.OrderBy(s => s.LastHeartbeatUtc).ToList();
        }

        public UpdateSession MarkCompleted(string updateId, string? appVersionAfter = null, int? dbVersionAfter = null)
        {
            var session = Load(updateId)
                ?? throw new FileNotFoundException("Sesión no encontrada.", updateId);

            session.Status = UpdateSessionStatus.Completed;
            session.CurrentStage = UpdateEndToEndStage.Completed;
            session.CompletedAtUtc = DateTime.UtcNow;
            session.RecoveryStatus = UpdateRecoveryStatus.None;
            if (appVersionAfter != null)
                session.AppVersionAfter = appVersionAfter;
            if (dbVersionAfter != null)
                session.DbVersionAfter = dbVersionAfter;

            Save(session);
            return session;
        }

        public UpdateSession MarkFailed(
            string updateId,
            UpdateSessionStatus status,
            UpdateEndToEndStage stage,
            string errorMessage,
            UpdateRecoveryStatus recoveryStatus = UpdateRecoveryStatus.None)
        {
            if (status is UpdateSessionStatus.Completed or UpdateSessionStatus.Active)
                throw new ArgumentException("MarkFailed requiere un status de fallo/bloqueo/recovery.", nameof(status));

            var session = Load(updateId)
                ?? throw new FileNotFoundException("Sesión no encontrada.", updateId);

            session.Status = status;
            session.CurrentStage = stage;
            session.ErrorMessage = errorMessage;
            session.RecoveryStatus = recoveryStatus;
            session.CompletedAtUtc = DateTime.UtcNow;

            Save(session);
            return session;
        }

        /// <summary>
        /// Elimina solo sesiones terminales seguras.
        /// Nunca elimina FailedRecoveryRequired ni RecoveryRequired.
        /// </summary>
        public bool DeleteSafe(string updateId)
        {
            var session = Load(updateId);
            if (session == null)
                return false;

            // Nunca borrar recovery crítico.
            if (session.Status is UpdateSessionStatus.FailedRecoveryRequired
                or UpdateSessionStatus.RecoveryRequired)
            {
                return false;
            }

            // Solo terminales seguros.
            if (session.Status is not (
                    UpdateSessionStatus.Completed
                    or UpdateSessionStatus.Blocked
                    or UpdateSessionStatus.Failed
                    or UpdateSessionStatus.FailedRecovered))
            {
                return false;
            }

            string path = GetSessionPath(updateId);
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
                TryDelete(path + ".tmp");
                TryDelete(path + ".bak");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string GetSessionPath(string updateId)
        {
            string safeId = Path.GetFileName(updateId.Trim());
            if (string.IsNullOrWhiteSpace(safeId))
                throw new ArgumentException("UpdateId inválido.", nameof(updateId));

            return Path.Combine(_sessionsDirectory, safeId + ".json");
        }

        private IEnumerable<UpdateSession> EnumerateSessions()
        {
            if (!Directory.Exists(_sessionsDirectory))
                yield break;

            foreach (string file in Directory.EnumerateFiles(_sessionsDirectory, "*.json"))
            {
                string name = Path.GetFileNameWithoutExtension(file);
                var session = Load(name);
                if (session != null)
                    yield return session;
            }
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch
            {
                // ignore
            }
        }

        private static JsonSerializerOptions CreateJsonOptions() => new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };
    }
}
