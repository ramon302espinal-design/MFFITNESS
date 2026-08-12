using CORE.Update;

namespace CORE.Update
{
    /// <summary>
    /// Guardas de sesión para UI startup (FASE 10B.7).
    /// </summary>
    public static class UpdateSessionGuard
    {
        public sealed class StartupDecision
        {
            public bool AllowNormalStartup { get; init; }
            public bool SkipAutoMigrations { get; init; }
            public bool BlockStartup { get; init; }
            public string Message { get; init; } = string.Empty;
            public UpdateSession? BlockingSession { get; init; }
        }

        public static StartupDecision Evaluate(UpdateSessionStorage? storage = null, TimeSpan? activeHeartbeatWindow = null)
        {
            var store = storage ?? new UpdateSessionStorage();
            TimeSpan window = activeHeartbeatWindow ?? TimeSpan.FromMinutes(30);

            // Recovery crítico: no iniciar normalmente
            foreach (string file in Directory.Exists(store.SessionsDirectory)
                         ? Directory.EnumerateFiles(store.SessionsDirectory, "*.json")
                         : Array.Empty<string>())
            {
                string id = Path.GetFileNameWithoutExtension(file);
                var session = store.Load(id);
                if (session == null)
                {
                    return new StartupDecision
                    {
                        AllowNormalStartup = false,
                        BlockStartup = true,
                        SkipAutoMigrations = true,
                        Message = "Sesión de actualización corrupta o ilegible. RecoveryRequired."
                    };
                }

                if (session.Status is UpdateSessionStatus.FailedRecoveryRequired
                    or UpdateSessionStatus.RecoveryRequired)
                {
                    return new StartupDecision
                    {
                        AllowNormalStartup = false,
                        BlockStartup = true,
                        SkipAutoMigrations = true,
                        Message = $"Actualización en estado {session.Status}. No se inicia UI normalmente. "
                                  + (session.ErrorMessage ?? string.Empty),
                        BlockingSession = session
                    };
                }
            }

            var pending = store.FindPendingSessions();
            DateTime cutoff = DateTime.UtcNow - window;
            var active = pending.Where(s => s.LastHeartbeatUtc >= cutoff).ToList();
            if (active.Count > 0)
            {
                return new StartupDecision
                {
                    AllowNormalStartup = true,
                    SkipAutoMigrations = true,
                    BlockStartup = false,
                    Message = "UpdateSession activa: UI no ejecuta migraciones (UpdateManager responsable).",
                    BlockingSession = active[0]
                };
            }

            return new StartupDecision
            {
                AllowNormalStartup = true,
                SkipAutoMigrations = false,
                BlockStartup = false,
                Message = "Sin sesión de actualización activa."
            };
        }
    }
}
