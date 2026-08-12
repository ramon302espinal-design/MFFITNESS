using System.Diagnostics;

namespace BLL.Update
{
    /// <summary>
    /// Control de procesos del POS. Cierre graceful sin Kill() automático (FASE 9).
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
            string processName = Path.GetFileNameWithoutExtension(executablePath);
            return Process.GetProcessesByName(processName).Any(p =>
            {
                try
                {
                    return string.Equals(
                        Path.GetFileName(p.MainModule?.FileName ?? string.Empty),
                        Path.GetFileName(executablePath),
                        StringComparison.OrdinalIgnoreCase);
                }
                catch
                {
                    return true;
                }
                finally
                {
                    p.Dispose();
                }
            });
        }

        public bool RequestGracefulClose(string executablePath)
        {
            string processName = Path.GetFileNameWithoutExtension(executablePath);
            bool requested = false;

            foreach (Process process in Process.GetProcessesByName(processName))
            {
                try
                {
                    if (!string.Equals(
                            Path.GetFileName(process.MainModule?.FileName ?? string.Empty),
                            Path.GetFileName(executablePath),
                            StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (process.MainWindowHandle != IntPtr.Zero)
                    {
                        process.CloseMainWindow();
                        requested = true;
                    }
                    else
                    {
                        // Sin ventana principal: intentar cierre estándar (no Kill).
                        requested = process.CloseMainWindow() || requested;
                    }
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
