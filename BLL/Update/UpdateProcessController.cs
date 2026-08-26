using System.Diagnostics;
using CORE.Update;

namespace BLL.Update
{
    /// <summary>
    /// Control de procesos del POS. Cierre graceful sin Kill() automático (FASE 9).
    /// Matching por ruta completa del UI.exe instalado (no cualquier UI.exe del sistema).
    /// </summary>
    public interface IUpdateProcessController
    {
        bool IsProcessRunning(string executablePath);
        bool RequestGracefulClose(string executablePath);
        bool WaitForExit(string executablePath, TimeSpan timeout);
    }

    public sealed class UpdateProcessController : IUpdateProcessController
    {
        public bool IsProcessRunning(string executablePath)
        {
            foreach (Process process in EnumerateMatchingProcesses(executablePath))
            {
                process.Dispose();
                return true;
            }

            return false;
        }

        public bool RequestGracefulClose(string executablePath)
        {
            bool requested = false;

            // Señal OTA: la UI escucha y hace Application.Exit / Environment.Exit.
            try
            {
                if (EventWaitHandle.TryOpenExisting(UpdateExitSignal.EventName, out EventWaitHandle? exitEvent))
                {
                    using (exitEvent)
                    {
                        exitEvent.Set();
                        requested = true;
                    }
                }
            }
            catch
            {
                // Fail soft: continuar con CloseMainWindow.
            }

            foreach (Process process in EnumerateMatchingProcesses(executablePath))
            {
                try
                {
                    if (process.CloseMainWindow())
                        requested = true;
                }
                catch
                {
                    // Fail closed: no Kill automático.
                }
                finally
                {
                    process.Dispose();
                }
            }

            return requested || !IsProcessRunning(executablePath);
        }

        public bool WaitForExit(string executablePath, TimeSpan timeout)
        {
            DateTime deadline = DateTime.UtcNow + timeout;
            while (DateTime.UtcNow < deadline)
            {
                if (!IsProcessRunning(executablePath))
                    return true;

                Thread.Sleep(250);
            }

            return !IsProcessRunning(executablePath);
        }

        private static IEnumerable<Process> EnumerateMatchingProcesses(string executablePath)
        {
            string processName = Path.GetFileNameWithoutExtension(executablePath);
            string targetFull;
            try
            {
                targetFull = Path.GetFullPath(executablePath);
            }
            catch
            {
                yield break;
            }

            foreach (Process process in Process.GetProcessesByName(processName))
            {
                bool match = false;
                try
                {
                    string? modulePath = process.MainModule?.FileName;
                    if (!string.IsNullOrWhiteSpace(modulePath))
                    {
                        match = string.Equals(
                            Path.GetFullPath(modulePath),
                            targetFull,
                            StringComparison.OrdinalIgnoreCase);
                    }
                }
                catch
                {
                    // Sin acceso a MainModule: no asumir match (evita cerrar otro UI.exe).
                    match = false;
                }

                if (match)
                    yield return process;
                else
                    process.Dispose();
            }
        }
    }

    /// <summary>
    /// Fake para smoke tests offline.
    /// </summary>
    public sealed class FakeUpdateProcessController : IUpdateProcessController
    {
        private readonly Dictionary<string, bool> _running = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, bool> _canClose = new(StringComparer.OrdinalIgnoreCase);

        public void SetRunning(string executablePath, bool running) =>
            _running[executablePath] = running;

        public void SetCanClose(string executablePath, bool canClose) =>
            _canClose[executablePath] = canClose;

        public bool IsProcessRunning(string executablePath) =>
            _running.TryGetValue(executablePath, out bool r) && r;

        public bool RequestGracefulClose(string executablePath)
        {
            if (_canClose.TryGetValue(executablePath, out bool can) && !can)
                return false;

            _running[executablePath] = false;
            return true;
        }

        public bool WaitForExit(string executablePath, TimeSpan timeout)
        {
            if (_canClose.TryGetValue(executablePath, out bool can) && !can)
                return false;

            _running[executablePath] = false;
            return true;
        }
    }
}
