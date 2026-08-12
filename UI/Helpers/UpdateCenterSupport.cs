using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BLL;
using CORE;
using CORE.Update;
using UI.DISEÑO;
using UI.Theme;

namespace UI.Helpers
{
    /// <summary>
    /// Soporte de presentación del Centro de Actualizaciones (sin lógica de install).
    /// </summary>
    public static class UpdateCenterSupport
    {
        public sealed class LocalSnapshot
        {
            public string InstalledAppVersion { get; init; } = CORE.AppVersion.SemanticVersion;
            public string Build { get; init; } = CORE.AppVersion.Build;
            public string Informational { get; init; } = CORE.AppVersion.Informational;
            public int? DbVersion { get; init; }
            public string? DbError { get; init; }
            public bool CajaCheckOk { get; init; }
            public bool CajaAbierta { get; init; }
            public string? CajaError { get; init; }
            public bool UpdateManagerPresent { get; init; }
            public string? UpdateManagerPath { get; init; }
            public UpdateSession? LastSession { get; init; }
            public string InstallDirectory { get; init; } = string.Empty;
        }

        public sealed class StatusView
        {
            public string Title { get; init; } = string.Empty;
            public string Hint { get; init; } = string.Empty;
            public Color Accent { get; init; } = AppTheme.Info;
            public bool CanInstall { get; init; }
        }

        public static void Open(IWin32Window? owner)
        {
            using var frm = new FrmActualizacion();
            if (owner != null)
                frm.ShowDialog(owner);
            else
                frm.ShowDialog();
        }

        public static LocalSnapshot CaptureLocal()
        {
            string installDir = AppContext.BaseDirectory.TrimEnd(
                Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            int? dbVersion = null;
            string? dbError = null;
            try
            {
                dbVersion = SchemaMigrationBLL.GetCurrentDbVersion();
            }
            catch (Exception ex)
            {
                dbError = ex.Message;
            }

            bool cajaCheckOk = false;
            bool cajaAbierta = true;
            string? cajaError = null;
            try
            {
                cajaAbierta = new CajaBLL().ObtenerEstadoCaja();
                cajaCheckOk = true;
            }
            catch (Exception ex)
            {
                cajaError = ex.Message;
            }

            string? umPath = UpdateLaunchBLL.ResolveUpdateManagerPath(installDir);

            return new LocalSnapshot
            {
                InstalledAppVersion = CORE.AppVersion.SemanticVersion,
                Build = CORE.AppVersion.Build,
                Informational = CORE.AppVersion.Informational,
                DbVersion = dbVersion,
                DbError = dbError,
                CajaCheckOk = cajaCheckOk,
                CajaAbierta = cajaAbierta,
                CajaError = cajaError,
                UpdateManagerPresent = umPath != null,
                UpdateManagerPath = umPath,
                LastSession = TryLoadLatestSession(),
                InstallDirectory = installDir
            };
        }

        public static StatusView MapLaunchStatus(UpdateLaunchStatus status, string message)
        {
            return status switch
            {
                UpdateLaunchStatus.Available => new StatusView
                {
                    Title = "Actualización disponible",
                    Hint = message,
                    Accent = AppTheme.Success,
                    CanInstall = true
                },
                UpdateLaunchStatus.NotAvailable => new StatusView
                {
                    Title = "Sistema al día",
                    Hint = message,
                    Accent = AppTheme.Primary,
                    CanInstall = false
                },
                UpdateLaunchStatus.Incompatible => new StatusView
                {
                    Title = "Incompatible",
                    Hint = message,
                    Accent = AppTheme.Warning,
                    CanInstall = false
                },
                UpdateLaunchStatus.Blocked => new StatusView
                {
                    Title = "Bloqueado",
                    Hint = message,
                    Accent = AppTheme.Warning,
                    CanInstall = false
                },
                UpdateLaunchStatus.DiscoveryFailed or UpdateLaunchStatus.DownloadFailed or UpdateLaunchStatus.Failed
                    => new StatusView
                    {
                        Title = "No se pudo completar",
                        Hint = message,
                        Accent = AppTheme.Error,
                        CanInstall = false
                    },
                UpdateLaunchStatus.Prepared => new StatusView
                {
                    Title = "Paquete listo",
                    Hint = message,
                    Accent = AppTheme.Info,
                    CanInstall = true
                },
                UpdateLaunchStatus.Launched => new StatusView
                {
                    Title = "Instalación en curso",
                    Hint = message,
                    Accent = AppTheme.Primary,
                    CanInstall = false
                },
                _ => new StatusView
                {
                    Title = "Estado desconocido",
                    Hint = message,
                    Accent = AppTheme.TextSecondary,
                    CanInstall = false
                }
            };
        }

        public static string FormatSessionSummary(UpdateSession? session)
        {
            if (session == null)
                return "Sin sesiones locales recientes.";

            string when = session.CompletedAtUtc?.ToLocalTime().ToString("dd/MM/yyyy HH:mm")
                          ?? session.StartedAtUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm");

            return $"Última sesión: {session.Status} · {when}"
                   + (string.IsNullOrWhiteSpace(session.AppVersionTarget)
                       ? string.Empty
                       : $" · destino {session.AppVersionTarget}");
        }

        private static UpdateSession? TryLoadLatestSession()
        {
            try
            {
                var store = new UpdateSessionStorage();
                if (!Directory.Exists(store.SessionsDirectory))
                    return null;

                var files = Directory.EnumerateFiles(store.SessionsDirectory, "*.json")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.LastWriteTimeUtc)
                    .Take(8);

                UpdateSession? best = null;
                DateTime bestStamp = DateTime.MinValue;

                foreach (var file in files)
                {
                    var session = store.Load(Path.GetFileNameWithoutExtension(file.Name));
                    if (session == null)
                        continue;

                    DateTime stamp = session.CompletedAtUtc ?? session.LastHeartbeatUtc;
                    if (stamp >= bestStamp)
                    {
                        bestStamp = stamp;
                        best = session;
                    }
                }

                return best;
            }
            catch
            {
                return null;
            }
        }
    }
}
