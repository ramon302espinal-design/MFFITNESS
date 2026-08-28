using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace UI.Helpers
{
    /// <summary>
    /// Invoca rembg local (%LocalAppData%\MFFITNESS\rembg-venv) para quitar fondo.
    /// Compone el resultado sobre blanco preservando el tamaño del original.
    /// </summary>
    internal static class ProductoRembgHelper
    {
        private static readonly object Sync = new();
        private static string? _rembgExeCached;
        private static bool? _availableCached;

        public static bool IsAvailable()
        {
            if (_availableCached.HasValue)
                return _availableCached.Value;

            lock (Sync)
            {
                if (_availableCached.HasValue)
                    return _availableCached.Value;

                string? exe = ResolverRembgExe();
                _rembgExeCached = exe;
                _availableCached = !string.IsNullOrWhiteSpace(exe) && File.Exists(exe);
                return _availableCached.Value;
            }
        }

        public static string? ResolverRembgExe()
        {
            if (!string.IsNullOrWhiteSpace(_rembgExeCached) && File.Exists(_rembgExeCached))
                return _rembgExeCached;

            string local = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MFFITNESS",
                "rembg-venv",
                "Scripts",
                "rembg.exe");

            if (File.Exists(local))
                return local;

            // Fallback: script del repo
            try
            {
                string? baseDir = AppContext.BaseDirectory;
                string candidate = Path.GetFullPath(Path.Combine(
                    baseDir,
                    "..", "..", "..", "..",
                    "Tools", "Rembg"));
                // No exe ahí; solo documentado.
            }
            catch
            {
                // ignore
            }

            return File.Exists(local) ? local : null;
        }

        /// <summary>
        /// Quita fondo con rembg y pega sobre blanco (mismo tamaño que <paramref name="source"/>).
        /// </summary>
        public static bool TryQuitarFondoSobreBlanco(Image source, out Bitmap? resultado, out string? error)
        {
            resultado = null;
            error = null;

            if (!IsAvailable())
            {
                error = "rembg no está instalado.";
                return false;
            }

            string? rembg = ResolverRembgExe();
            if (string.IsNullOrWhiteSpace(rembg))
            {
                error = "No se encontró rembg.exe.";
                return false;
            }

            string tempDir = Path.Combine(
                Path.GetTempPath(),
                "MFFITNESS",
                "rembg");
            Directory.CreateDirectory(tempDir);

            string id = Guid.NewGuid().ToString("N");
            string inputPath = Path.Combine(tempDir, id + "_in.jpg");
            string outputPath = Path.Combine(tempDir, id + "_out.png");

            try
            {
                byte[] jpeg = ProductoImagenHelper.ToJpegBytes(source, maxSide: 2048, quality: 92);
                File.WriteAllBytes(inputPath, jpeg);

                var psi = new ProcessStartInfo
                {
                    FileName = rembg,
                    // bria-rmbg ya descargado en warm-up; alta calidad para productos.
                    Arguments = $"i -m bria-rmbg \"{inputPath}\" \"{outputPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = Path.GetDirectoryName(rembg) ?? tempDir
                };

                using var proc = Process.Start(psi);
                if (proc == null)
                {
                    error = "No se pudo iniciar rembg.";
                    return false;
                }

                var stdoutTask = proc.StandardOutput.ReadToEndAsync();
                var stderrTask = proc.StandardError.ReadToEndAsync();
                bool exited = proc.WaitForExit(180_000);
                if (!exited)
                {
                    try { proc.Kill(entireProcessTree: true); } catch { /* ignore */ }
                    error = "rembg agotó el tiempo de espera.";
                    return false;
                }

                _ = stdoutTask.GetAwaiter().GetResult();
                string stderr = stderrTask.GetAwaiter().GetResult();

                if (proc.ExitCode != 0 || !File.Exists(outputPath))
                {
                    // Reintento con modelo por defecto si bria falla.
                    if (!TryRembgModel(rembg, inputPath, outputPath, model: null, out error))
                        return false;
                }

                using var cutout = LoadBitmapIndependent(outputPath);
                resultado = ComponerSobreBlancoMismoTamano(source, cutout);
                return resultado != null;
            }
            catch (Exception ex)
            {
                error = ex.GetBaseException().Message;
                resultado?.Dispose();
                resultado = null;
                return false;
            }
            finally
            {
                TryDelete(inputPath);
                TryDelete(outputPath);
            }
        }

        private static bool TryRembgModel(
            string rembgExe,
            string inputPath,
            string outputPath,
            string? model,
            out string? error)
        {
            error = null;
            try
            {
                TryDelete(outputPath);
                string args = string.IsNullOrWhiteSpace(model)
                    ? $"i \"{inputPath}\" \"{outputPath}\""
                    : $"i -m {model} \"{inputPath}\" \"{outputPath}\"";

                var psi = new ProcessStartInfo
                {
                    FileName = rembgExe,
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = Path.GetDirectoryName(rembgExe) ?? Path.GetTempPath()
                };

                using var proc = Process.Start(psi);
                if (proc == null)
                {
                    error = "No se pudo iniciar rembg.";
                    return false;
                }

                var stdoutTask = proc.StandardOutput.ReadToEndAsync();
                var stderrTask = proc.StandardError.ReadToEndAsync();
                if (!proc.WaitForExit(180_000))
                {
                    try { proc.Kill(entireProcessTree: true); } catch { /* ignore */ }
                    error = "rembg timeout.";
                    return false;
                }

                _ = stdoutTask.GetAwaiter().GetResult();
                string stderr = stderrTask.GetAwaiter().GetResult();

                if (proc.ExitCode != 0 || !File.Exists(outputPath))
                {
                    error = string.IsNullOrWhiteSpace(stderr)
                        ? $"rembg falló (código {proc.ExitCode})."
                        : stderr.Trim();
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static Bitmap LoadBitmapIndependent(string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var tmp = Image.FromStream(fs);
            return new Bitmap(tmp);
        }

        /// <summary>
        /// Dibuja el cutout (RGBA) centrado/escalado sobre lienzo blanco del tamaño original.
        /// No recorta el marco: el canvas = tamaño fuente.
        /// </summary>
        private static Bitmap ComponerSobreBlancoMismoTamano(Image original, Bitmap cutout)
        {
            int w = original.Width;
            int h = original.Height;
            var canvas = new Bitmap(w, h, PixelFormat.Format24bppRgb);
            using var g = Graphics.FromImage(canvas);
            g.Clear(Color.White);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingMode = CompositingMode.SourceOver;
            g.CompositingQuality = CompositingQuality.HighQuality;

            // Si rembg devolvió mismo tamaño, dibujar 1:1; si no, contain centrado.
            if (cutout.Width == w && cutout.Height == h)
            {
                g.DrawImage(cutout, 0, 0, w, h);
            }
            else
            {
                float scale = Math.Min((float)w / cutout.Width, (float)h / cutout.Height);
                int dw = Math.Max(1, (int)Math.Round(cutout.Width * scale));
                int dh = Math.Max(1, (int)Math.Round(cutout.Height * scale));
                int x = (w - dw) / 2;
                int y = (h - dh) / 2;
                g.DrawImage(cutout, new Rectangle(x, y, dw, dh));
            }

            return canvas;
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
    }
}
