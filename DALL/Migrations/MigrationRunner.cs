using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace DL.Migrations
{
    /// <summary>
    /// Motor de migraciones SQL versionadas. Sin dependencia de WinForms.
    /// Reutilizable desde la UI y un futuro UpdateManager.
    /// </summary>
    public sealed class MigrationRunner
    {
        private const string AppLockResource = "MFFITNESS_SchemaMigration";
        private static readonly Regex FileNamePattern = new(
            @"^(?<version>\d{4})_(?<name>.+)\.sql$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly DBHelper _db;
        private readonly Action<string> _log;

        public MigrationRunner(DBHelper db, Action<string>? log = null)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _log = log ?? (_ => { });
        }

        public static string ResolveDefaultDirectory()
        {
            string fromBase = Path.Combine(AppContext.BaseDirectory, "Database", "Migrations");
            if (Directory.Exists(fromBase) && Directory.EnumerateFiles(fromBase, "*.sql").Any())
                return fromBase;

            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                string candidate = Path.Combine(dir.FullName, "Database", "Migrations");
                if (Directory.Exists(candidate))
                    return candidate;
                dir = dir.Parent;
            }

            return fromBase;
        }

        /// <summary>
        /// Aplica todas las migraciones pendientes (sin techo). Compatibilidad FASE 5/6.
        /// </summary>
        public MigrationRunResult Run(string? migrationsDirectory = null) =>
            RunInternal(targetVersion: null, migrationsDirectory);

        /// <summary>
        /// Aplica migraciones exclusivamente hasta targetVersion inclusive.
        /// Nunca ejecuta scripts con Version &gt; targetVersion.
        /// Si falta algún script requerido en (current+1..target), falla sin aplicar ninguno pendiente incompleto de secuencia.
        /// </summary>
        public MigrationRunResult RunUpTo(int targetVersion, string? migrationsDirectory = null)
        {
            if (targetVersion < 1)
                return MigrationRunResult.Fail(0, 0, "TargetVersion inválido (< 1).");

            return RunInternal(targetVersion, migrationsDirectory);
        }

        /// <summary>
        /// Lista versiones pendientes estrictamente hasta target (sin aplicar).
        /// </summary>
        public IReadOnlyList<int> ListPendingUntil(int currentVersion, int targetVersion, string migrationsDirectory)
        {
            if (!Directory.Exists(migrationsDirectory))
                return Array.Empty<int>();

            return Discover(migrationsDirectory)
                .Where(m => m.Version > currentVersion && m.Version <= targetVersion)
                .OrderBy(m => m.Version)
                .Select(m => m.Version)
                .ToList();
        }

        private MigrationRunResult RunInternal(int? targetVersion, string? migrationsDirectory)
        {
            string directory = string.IsNullOrWhiteSpace(migrationsDirectory)
                ? ResolveDefaultDirectory()
                : migrationsDirectory;

            int initialVersion = 0;
            try
            {
                SchemaVersionDAL.EnsureBaseline(_db);
                var current = SchemaVersionDAL.GetCurrent(_db);
                initialVersion = current.Version;
                _log(targetVersion.HasValue
                    ? $"versión inicial: {initialVersion}; target: {targetVersion.Value}"
                    : $"versión inicial: {initialVersion}");

                if (targetVersion.HasValue && targetVersion.Value < initialVersion)
                {
                    string msg = $"No se puede bajar SchemaVersion de {initialVersion} a {targetVersion.Value}.";
                    _log(msg);
                    return MigrationRunResult.Fail(initialVersion, initialVersion, msg);
                }

                if (targetVersion.HasValue && targetVersion.Value == initialVersion)
                {
                    string none = $"Sin migraciones pendientes hasta {targetVersion.Value}. Versión final: {initialVersion}";
                    _log(none);
                    return MigrationRunResult.Ok(initialVersion, initialVersion, Array.Empty<int>(), none);
                }

                if (!Directory.Exists(directory))
                {
                    string msg = $"Carpeta de migraciones no encontrada: {directory}";
                    _log(msg);
                    return MigrationRunResult.Fail(initialVersion, initialVersion, msg);
                }

                var discovered = Discover(directory);
                List<MigrationDefinition> pending;
                if (targetVersion.HasValue)
                {
                    int target = targetVersion.Value;
                    pending = discovered
                        .Where(m => m.Version > initialVersion && m.Version <= target)
                        .OrderBy(m => m.Version)
                        .ToList();

                    var missing = new List<int>();
                    for (int v = initialVersion + 1; v <= target; v++)
                    {
                        if (pending.All(m => m.Version != v))
                            missing.Add(v);
                    }

                    if (missing.Count > 0)
                    {
                        string msg = "Faltan migraciones requeridas hasta target "
                            + target + ": " + string.Join(", ", missing.Select(v => v.ToString("0000")));
                        _log(msg);
                        return MigrationRunResult.Fail(initialVersion, initialVersion, msg);
                    }
                }
                else
                {
                    pending = discovered.Where(m => m.Version > initialVersion).OrderBy(m => m.Version).ToList();
                }

                if (pending.Count == 0)
                {
                    string none = $"Sin migraciones pendientes. Versión final: {initialVersion}";
                    _log(none);
                    return MigrationRunResult.Ok(initialVersion, initialVersion, Array.Empty<int>(), none);
                }

                ValidateSequence(initialVersion, pending);

                using SqlConnection conn = _db.GetConnection();
                conn.Open();

                if (!TryAcquireAppLock(conn, out string lockError))
                {
                    _log(lockError);
                    return MigrationRunResult.Fail(initialVersion, initialVersion, lockError);
                }

                try
                {
                    var currentOnConn = SchemaVersionDAL.GetCurrent(conn);
                    if (currentOnConn.Version != initialVersion)
                    {
                        string msg = $"La versión cambió durante el lock ({initialVersion} → {currentOnConn.Version}). Reintentar.";
                        _log(msg);
                        return MigrationRunResult.Fail(initialVersion, currentOnConn.Version, msg);
                    }

                    var applied = new List<int>();
                    int runningVersion = initialVersion;

                    foreach (var migration in pending)
                    {
                        if (targetVersion.HasValue && migration.Version > targetVersion.Value)
                        {
                            _log($"omitida (sobre target): {migration.FileName}");
                            break;
                        }

                        _log($"migración iniciada: {migration.FileName} (destino SchemaVersion {migration.Version})");
                        try
                        {
                            ApplyMigration(conn, migration);
                            runningVersion = migration.Version;
                            applied.Add(migration.Version);
                            _log($"migración completada: {migration.FileName}");
                        }
                        catch (Exception ex)
                        {
                            _log($"migración fallida: {migration.FileName}");
                            _log($"error: {ex.Message}");
                            _log($"versión final: {runningVersion}");
                            return MigrationRunResult.Fail(
                                initialVersion,
                                runningVersion,
                                $"Falló {migration.FileName}: {ex.Message}",
                                migration.FileName);
                        }
                    }

                    _log($"versión final: {runningVersion}");
                    return MigrationRunResult.Ok(
                        initialVersion,
                        runningVersion,
                        applied,
                        applied.Count == 0
                            ? $"Sin cambios. Versión {runningVersion}."
                            : $"Aplicadas: {string.Join(", ", applied)}. Versión {initialVersion} → {runningVersion}.");
                }
                finally
                {
                    ReleaseAppLock(conn);
                }
            }
            catch (Exception ex)
            {
                _log($"error: {ex.Message}");
                _log($"versión final: {initialVersion}");
                return MigrationRunResult.Fail(initialVersion, initialVersion, ex.Message);
            }
        }

        public IReadOnlyList<MigrationDefinition> Discover(string directory)
        {
            var files = Directory.GetFiles(directory, "*.sql");
            var malformed = new List<string>();
            var byVersion = new Dictionary<int, MigrationDefinition>();

            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                var match = FileNamePattern.Match(name);
                if (!match.Success)
                {
                    malformed.Add(name);
                    continue;
                }

                int version = int.Parse(match.Groups["version"].Value);
                if (version < 1)
                {
                    malformed.Add(name);
                    continue;
                }

                var definition = new MigrationDefinition
                {
                    Version = version,
                    Name = match.Groups["name"].Value,
                    FilePath = file
                };

                if (!byVersion.TryAdd(version, definition))
                {
                    throw new InvalidOperationException(
                        $"Número de migración duplicado {version:0000}: '{byVersion[version].FileName}' y '{name}'.");
                }
            }

            if (malformed.Count > 0)
            {
                throw new InvalidOperationException(
                    "Archivos SQL con nombre inválido (se espera NNNN_Descripcion.sql): "
                    + string.Join(", ", malformed));
            }

            return byVersion.Values.OrderBy(m => m.Version).ToList();
        }

        private static void ValidateSequence(int currentVersion, List<MigrationDefinition> pending)
        {
            int expected = currentVersion + 1;
            foreach (var migration in pending)
            {
                if (migration.Version <= currentVersion)
                    throw new InvalidOperationException($"No se puede ejecutar {migration.FileName}: versión <= actual ({currentVersion}).");

                if (migration.Version != expected)
                {
                    throw new InvalidOperationException(
                        $"Hueco en migraciones: se esperaba {expected:0000} y apareció {migration.FileName}. No se saltan versiones.");
                }

                expected++;
            }
        }

        private static void ApplyMigration(SqlConnection conn, MigrationDefinition migration)
        {
            string sql = File.ReadAllText(migration.FilePath, Encoding.UTF8);
            var batches = SplitBatches(sql);
            if (batches.Count == 0)
                throw new InvalidOperationException($"{migration.FileName} está vacío.");

            using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                using (var abort = new SqlCommand("SET XACT_ABORT ON;", conn, tx))
                    abort.ExecuteNonQuery();

                foreach (string batch in batches)
                {
                    using var cmd = new SqlCommand(batch, conn, tx)
                    {
                        CommandTimeout = 120
                    };
                    cmd.ExecuteNonQuery();
                }

                string description = $"{migration.FileName}";
                if (description.Length > 300)
                    description = description[..300];

                SchemaVersionDAL.RegisterApplied(conn, tx, migration.Version, description);
                tx.Commit();
            }
            catch
            {
                try { tx.Rollback(); } catch { /* transacción ya abortada */ }
                throw;
            }
        }

        internal static IReadOnlyList<string> SplitBatches(string sql)
        {
            var batches = new List<string>();
            var sb = new StringBuilder();
            using var reader = new StringReader(sql);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (Regex.IsMatch(line.Trim(), @"^GO\s*$", RegexOptions.IgnoreCase))
                {
                    string batch = sb.ToString().Trim();
                    if (batch.Length > 0)
                        batches.Add(batch);
                    sb.Clear();
                }
                else
                {
                    sb.AppendLine(line);
                }
            }

            string last = sb.ToString().Trim();
            if (last.Length > 0)
                batches.Add(last);

            return batches;
        }

        private static bool TryAcquireAppLock(SqlConnection conn, out string error)
        {
            using var cmd = new SqlCommand(@"
DECLARE @result INT;
EXEC @result = sp_getapplock
    @Resource = @Resource,
    @LockMode = 'Exclusive',
    @LockOwner = 'Session',
    @LockTimeout = 0;
SELECT @result;", conn);
            cmd.Parameters.AddWithValue("@Resource", AppLockResource);
            int result = Convert.ToInt32(cmd.ExecuteScalar());
            if (result >= 0)
            {
                error = string.Empty;
                return true;
            }

            error = "No se pudo obtener el lock de migraciones (otra instancia POS puede estar actualizando el esquema). Código: " + result;
            return false;
        }

        private static void ReleaseAppLock(SqlConnection conn)
        {
            try
            {
                using var cmd = new SqlCommand(@"
EXEC sp_releaseapplock
    @Resource = @Resource,
    @LockOwner = 'Session';", conn);
                cmd.Parameters.AddWithValue("@Resource", AppLockResource);
                cmd.ExecuteNonQuery();
            }
            catch
            {
                // El lock de sesión se libera al cerrar la conexión.
            }
        }
    }
}
