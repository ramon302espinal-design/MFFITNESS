using System.Diagnostics;
using System.Text;

namespace UI.Helpers
{
    /// <summary>
    /// Transcripción local con faster-whisper (mejor acento dominicano/latino que SAPI de Windows).
    /// Venv: %LocalAppData%\MFFITNESS\whisper-venv
    /// </summary>
    internal static class PeticionIaWhisperHelper
    {
        private static readonly object Sync = new();
        private static bool? _availableCached;
        private static string? _pythonExeCached;
        private static string? _scriptPathCached;

        public const string ModelSize = "small";

        public static bool IsAvailable()
        {
            if (_availableCached.HasValue)
                return _availableCached.Value;

            lock (Sync)
            {
                if (_availableCached.HasValue)
                    return _availableCached.Value;

                string? python = ResolverPython();
                string? script = ResolverScript();
                _pythonExeCached = python;
                _scriptPathCached = script;

                if (string.IsNullOrWhiteSpace(python) || string.IsNullOrWhiteSpace(script))
                {
                    _availableCached = false;
                    return false;
                }

                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = python,
                        Arguments = "-c \"import faster_whisper; print('ok')\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8
                    };

                    using Process? p = Process.Start(psi);
                    if (p == null)
                    {
                        _availableCached = false;
                        return false;
                    }

                    p.WaitForExit(15000);
                    _availableCached = p.ExitCode == 0;
                    return _availableCached.Value;
                }
                catch
                {
                    _availableCached = false;
                    return false;
                }
            }
        }

        public static async Task<string?> TranscribirAsync(string wavPath, CancellationToken ct = default)
        {
            if (!IsAvailable() || string.IsNullOrWhiteSpace(wavPath) || !File.Exists(wavPath))
                return null;

            string python = _pythonExeCached!;
            string script = _scriptPathCached!;

            var psi = new ProcessStartInfo
            {
                FileName = python,
                Arguments = $"\"{script}\" \"{wavPath}\" {ModelSize}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using Process? process = Process.Start(psi);
            if (process == null)
                return null;

            var stdoutTask = process.StandardOutput.ReadToEndAsync(ct);
            var stderrTask = process.StandardError.ReadToEndAsync(ct);

            await process.WaitForExitAsync(ct).ConfigureAwait(false);
            string stdout = await stdoutTask.ConfigureAwait(false);
            _ = await stderrTask.ConfigureAwait(false);

            if (process.ExitCode != 0)
                return null;

            return stdout.Trim();
        }

        private static string? ResolverPython()
        {
            string local = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MFFITNESS",
                "whisper-venv",
                "Scripts",
                "python.exe");

            return File.Exists(local) ? local : null;
        }

        private static string? ResolverScript()
        {
            string local = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MFFITNESS",
                "Whisper",
                "transcribe_es.py");

            if (File.Exists(local))
                return local;

            try
            {
                string fromRepo = Path.GetFullPath(Path.Combine(
                    AppContext.BaseDirectory,
                    "..", "..", "..", "..",
                    "Tools", "Whisper", "transcribe_es.py"));

                if (File.Exists(fromRepo))
                    return fromRepo;
            }
            catch { /* ignore */ }

            string deployed = Path.Combine(
                AppContext.BaseDirectory,
                "Tools", "Whisper", "transcribe_es.py");

            return File.Exists(deployed) ? deployed : null;
        }
    }
}
