using System.Diagnostics;
using CORE.Update;

namespace BLL.Update
{
    public static class UpdateHealthCheckService
    {
        public sealed class HealthCheckResult
        {
            public bool Success { get; init; }
            public string Message { get; init; } = string.Empty;
            public string? InstalledAppVersion { get; init; }
        }

        public static HealthCheckResult Run(string installDirectory, UpdateManifest manifest)
        {
            if (!UpdateBinaryInstaller.VerifyRequiredFiles(installDirectory, out string? missingError))
                return new HealthCheckResult { Success = false, Message = missingError ?? "Archivos faltantes." };

            if (!UpdateBinaryInstaller.CanLoadMainAssembly(installDirectory, out string? loadError))
                return new HealthCheckResult { Success = false, Message = loadError ?? "UI.exe no cargable." };

            if (!UpdateBinaryInstaller.VerifyAppVersion(
                    installDirectory,
                    manifest.AppVersion,
                    out string? actual,
                    out string? versionError))
            {
                return new HealthCheckResult
                {
                    Success = false,
                    Message = versionError ?? "Versión incorrecta.",
                    InstalledAppVersion = actual
                };
            }

            return new HealthCheckResult
            {
                Success = true,
                Message = "Health-check OK.",
                InstalledAppVersion = actual
            };
        }

        /// <summary>
        /// Health check completo FASE 10B: binarios + DB + par App/DB.
        /// </summary>
        public static HealthCheckResult RunFull(
            string installDirectory,
            UpdateManifest manifest,
            IUpdateDbHealthProbe? dbProbe = null,
            string? migrationsDirectory = null,
            Func<string, UpdateManifest, (bool Ok, string? Actual, string? Error)>? verifyAppVersion = null)
        {
            string? actualApp;

            if (verifyAppVersion != null)
            {
                var (ok, actual, error) = verifyAppVersion(installDirectory, manifest);
                actualApp = actual;
                if (!ok)
                {
                    return new HealthCheckResult
                    {
                        Success = false,
                        Message = error ?? "AppVersion incorrecta.",
                        InstalledAppVersion = actual
                    };
                }
            }
            else
            {
                var binaries = Run(installDirectory, manifest);
                if (!binaries.Success)
                    return binaries;
                actualApp = binaries.InstalledAppVersion;
            }

            if (!UpdateBinaryInstaller.VerifyRequiredFiles(installDirectory, out string? missing))
                return new HealthCheckResult { Success = false, Message = missing ?? "Archivos faltantes.", InstalledAppVersion = actualApp };

            var probe = dbProbe ?? new UpdateDbHealthProbe();
            var db = probe.Probe(manifest.TargetDbVersion, migrationsDirectory);
            if (!db.Success)
            {
                return new HealthCheckResult
                {
                    Success = false,
                    Message = db.Message,
                    InstalledAppVersion = actualApp
                };
            }

            bool pairOk = string.Equals(actualApp, manifest.AppVersion, StringComparison.OrdinalIgnoreCase)
                && db.SchemaVersion == manifest.TargetDbVersion;

            if (!pairOk)
            {
                return new HealthCheckResult
                {
                    Success = false,
                    Message = $"Par App/DB incompatible. App={actualApp} DB={db.SchemaVersion}; esperado App={manifest.AppVersion} DB={manifest.TargetDbVersion}.",
                    InstalledAppVersion = actualApp
                };
            }

            return new HealthCheckResult
            {
                Success = true,
                Message = "Health-check completo OK (binarios+DB).",
                InstalledAppVersion = actualApp
            };
        }
    }

    public interface IUpdateApplicationLauncher
    {
        bool Start(string executablePath, out string? error);
        bool WaitForStartup(string executablePath, TimeSpan timeout);
    }

    public sealed class UpdateApplicationLauncher : IUpdateApplicationLauncher
    {
        public bool Start(string executablePath, out string? error)
        {
            try
            {
                if (!File.Exists(executablePath))
                {
                    error = "Ejecutable no encontrado.";
                    return false;
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = executablePath,
                    WorkingDirectory = Path.GetDirectoryName(executablePath) ?? Environment.CurrentDirectory,
                    UseShellExecute = true
                });

                error = null;
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public bool WaitForStartup(string executablePath, TimeSpan timeout)
        {
            DateTime deadline = DateTime.UtcNow + timeout;
            string processName = Path.GetFileNameWithoutExtension(executablePath);

            while (DateTime.UtcNow < deadline)
            {
                if (Process.GetProcessesByName(processName).Any())
                    return true;

                Thread.Sleep(250);
            }

            return Process.GetProcessesByName(processName).Any();
        }
    }

    public sealed class FakeUpdateApplicationLauncher : IUpdateApplicationLauncher
    {
        public bool StartShouldSucceed { get; set; } = true;
        public bool StartupShouldSucceed { get; set; } = true;
        public bool StartCalled { get; private set; }

        public bool Start(string executablePath, out string? error)
        {
            StartCalled = true;
            if (!StartShouldSucceed)
            {
                error = "Arranque simulado fallido.";
                return false;
            }

            error = null;
            return true;
        }

        public bool WaitForStartup(string executablePath, TimeSpan timeout) => StartupShouldSucceed;
    }
}
