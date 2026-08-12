using DL;
using DL.Migrations;

namespace BLL.Update
{
    /// <summary>
    /// Probe de salud DB: conexión, SchemaVersion, pending hasta target, query mínima.
    /// No inserta datos de negocio.
    /// </summary>
    public sealed class UpdateDbHealthProbe : IUpdateDbHealthProbe
    {
        private readonly DBHelper _db;
        private readonly Action<string> _log;

        public UpdateDbHealthProbe(DBHelper? db = null, Action<string>? log = null)
        {
            _db = db ?? new DBHelper();
            _log = log ?? (_ => { });
        }

        public UpdateDbHealthProbeResult Probe(int targetDbVersion, string? migrationsDirectory)
        {
            try
            {
                SchemaVersionDAL.EnsureBaseline(_db);
                var current = SchemaVersionDAL.GetCurrent(_db);
                _log($"health-db SchemaVersion={current.Version} target={targetDbVersion}");

                object? integrity = _db.ExecuteScalar(
                    "SELECT TOP (1) Version FROM dbo.SchemaVersion WHERE EsActual = 1;");
                if (integrity == null || integrity == DBNull.Value)
                {
                    return new UpdateDbHealthProbeResult
                    {
                        Success = false,
                        SqlConnected = true,
                        SchemaVersionExists = false,
                        Message = "Query de integridad SchemaVersion falló.",
                        SchemaVersion = current.Version
                    };
                }

                bool matches = current.Version == targetDbVersion;
                bool noPending = true;
                string dir = migrationsDirectory ?? MigrationRunner.ResolveDefaultDirectory();
                if (Directory.Exists(dir))
                {
                    var runner = new MigrationRunner(_db, _log);
                    var pending = runner.ListPendingUntil(current.Version, targetDbVersion, dir);
                    noPending = pending.Count == 0;
                }
                else if (current.Version < targetDbVersion)
                {
                    noPending = false;
                }

                bool ok = matches && noPending;
                return new UpdateDbHealthProbeResult
                {
                    Success = ok,
                    SqlConnected = true,
                    SchemaVersionExists = true,
                    SchemaVersion = current.Version,
                    MatchesTarget = matches,
                    NoPendingUntilTarget = noPending,
                    IntegrityQueryOk = true,
                    Message = ok
                        ? "DB health OK."
                        : $"DB health FAIL. Schema={current.Version} Target={targetDbVersion} PendingUntilTarget={!noPending}"
                };
            }
            catch (Exception ex)
            {
                return new UpdateDbHealthProbeResult
                {
                    Success = false,
                    SqlConnected = false,
                    Message = "DB health error: " + ex.Message
                };
            }
        }
    }
}
