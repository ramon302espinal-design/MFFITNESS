using BLL.Services;
using CORE;
using CORE.Ollama;
using System.Collections.Concurrent;
using System.Text;
using UI.Helpers;

namespace UI.Services
{
    /// <summary>
    /// Vigila la carpeta de facturas (Dev/Prod) mientras el dashboard de caja está abierto.
    /// FileSystemWatcher + polling; solo procesa con caja ABIERTA; feedback visible.
    /// </summary>
    public sealed class FacturaGastosWatcherHost : IDisposable
    {
        private static readonly HashSet<string> Extensiones = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".bmp", ".webp", ".pdf"
        };

        private readonly Form _owner;
        private readonly Func<bool> _isCajaAbierta;
        private readonly Action _onGastoRegistrado;
        private readonly Action _onVerMovimientos;
        private readonly Action<string>? _onStatusChanged;

        private FileSystemWatcher? _watcher;
        private System.Windows.Forms.Timer? _pollTimer;
        private CancellationTokenSource? _cts;
        private readonly ConcurrentQueue<string> _cola = new();
        private readonly ConcurrentDictionary<string, byte> _enColaOProceso = new(StringComparer.OrdinalIgnoreCase);
        private readonly object _loopSync = new();
        private readonly object _logSync = new();
        private Task? _loop;
        private string? _root;
        private bool _disposed;
        private bool _procesando;
        private string _status = "Inactivo";

        public string? WatchedRoot => _root;
        public string StatusText => _status;

        public FacturaGastosWatcherHost(
            Form owner,
            Func<bool> isCajaAbierta,
            Action onGastoRegistrado,
            Action onVerMovimientos,
            Action<string>? onStatusChanged = null)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _isCajaAbierta = isCajaAbierta;
            _onGastoRegistrado = onGastoRegistrado;
            _onVerMovimientos = onVerMovimientos;
            _onStatusChanged = onStatusChanged;
        }

        public void Start()
        {
            if (_disposed)
                return;

            AppConfig.LoadOllamaOptions();
            if (!OllamaOptions.FacturaGastosAutoEnabled)
            {
                SetStatus("Auto-factura deshabilitada en config");
                return;
            }

            _root = FacturaGastosFolder.ResolveRoot(createIfMissing: true);
            string env = SafeEnv();
            string folderLogical = FacturaGastosFolder.ResolveFolderNameForEnvironment();
            Log($"START root={_root} env={env} folder={folderLogical} enabled=true");
            if (string.Equals(env, "Production", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(folderLogical, "FacturaGastos", StringComparison.OrdinalIgnoreCase)
                && string.IsNullOrWhiteSpace(OllamaOptions.FacturaGastosFolderPath))
            {
                Log("WARN Production debería usar FacturaGastos — revisar config");
            }

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            _watcher?.Dispose();
            try
            {
                _watcher = new FileSystemWatcher(_root)
                {
                    NotifyFilter = NotifyFilters.FileName
                                   | NotifyFilters.LastWrite
                                   | NotifyFilters.CreationTime
                                   | NotifyFilters.Size,
                    IncludeSubdirectories = false,
                    EnableRaisingEvents = true
                };
                _watcher.Created += OnFsEvent;
                _watcher.Changed += OnFsEvent;
                _watcher.Renamed += OnFsRenamed;
                _watcher.Error += (_, e) =>
                    Log("WATCHER_ERROR " + e.GetException().Message);
            }
            catch (Exception ex)
            {
                Log("WATCHER_INIT_FAIL " + ex.Message);
                // Polling seguirá cubriendo.
            }

            _pollTimer?.Stop();
            _pollTimer?.Dispose();
            _pollTimer = new System.Windows.Forms.Timer { Interval = 3000 };
            _pollTimer.Tick += (_, _) =>
            {
                try
                {
                    if (_disposed || string.IsNullOrWhiteSpace(_root))
                        return;
                    EncolarPendientes(_root);
                    AsegurarLoop();
                }
                catch (Exception ex)
                {
                    Log("POLL_ERR " + ex.Message);
                }
            };
            _pollTimer.Start();

            EncolarPendientes(_root);
            AsegurarLoop();

            DISEÑO.FrmFacturaGastoHud.ShowVigilando(_root);
            SetStatus($"Vigilando {Path.GetFileName(_root)}");
        }

        public void Stop()
        {
            try
            {
                _pollTimer?.Stop();
                _pollTimer?.Dispose();
                _pollTimer = null;
            }
            catch { /* ignore */ }

            try
            {
                if (_watcher != null)
                {
                    _watcher.EnableRaisingEvents = false;
                    _watcher.Created -= OnFsEvent;
                    _watcher.Changed -= OnFsEvent;
                    _watcher.Renamed -= OnFsRenamed;
                    _watcher.Dispose();
                    _watcher = null;
                }
            }
            catch { /* ignore */ }

            try { _cts?.Cancel(); } catch { /* ignore */ }
            SetStatus("Detenido");
            Log("STOP");
        }

        public void SyncCajaState(bool cajaAbierta)
        {
            if (_disposed || string.IsNullOrWhiteSpace(_root))
                return;

            Log(cajaAbierta ? "CAJA_ABIERTA" : "CAJA_CERRADA");
            if (cajaAbierta)
            {
                EncolarPendientes(_root);
                AsegurarLoop();
                SetStatus($"Vigilando {Path.GetFileName(_root)} · caja ABIERTA");
            }
            else
            {
                SetStatus($"Vigilando {Path.GetFileName(_root)} · caja CERRADA (en espera)");
            }
        }

        private void OnFsRenamed(object sender, RenamedEventArgs e) =>
            EncolarSiAplica(e.FullPath);

        private void OnFsEvent(object sender, FileSystemEventArgs e) =>
            EncolarSiAplica(e.FullPath);

        private void EncolarSiAplica(string? path)
        {
            if (string.IsNullOrWhiteSpace(path) || _disposed || string.IsNullOrWhiteSpace(_root))
                return;

            string ext = Path.GetExtension(path);
            if (!Extensiones.Contains(ext))
                return;

            string full;
            try { full = Path.GetFullPath(path); }
            catch { return; }

            string procesadas = FacturaGastosFolder.Procesadas(_root);
            string errores = FacturaGastosFolder.Errores(_root);
            if (full.StartsWith(procesadas, StringComparison.OrdinalIgnoreCase)
                || full.StartsWith(errores, StringComparison.OrdinalIgnoreCase))
                return;

            // Solo raíz (no subcarpetas).
            string? parent = Path.GetDirectoryName(full);
            if (parent == null
                || !string.Equals(
                    Path.GetFullPath(parent).TrimEnd('\\'),
                    Path.GetFullPath(_root).TrimEnd('\\'),
                    StringComparison.OrdinalIgnoreCase))
                return;

            if (!_enColaOProceso.TryAdd(full, 0))
                return;

            _cola.Enqueue(full);
            Log("ENCOLADO " + Path.GetFileName(full));
            AsegurarLoop();
        }

        private void EncolarPendientes(string root)
        {
            try
            {
                foreach (string file in Directory.EnumerateFiles(root))
                    EncolarSiAplica(file);
            }
            catch (Exception ex)
            {
                Log("SCAN_ERR " + ex.Message);
            }
        }

        private void AsegurarLoop()
        {
            lock (_loopSync)
            {
                if (_loop is { IsCompleted: false })
                    return;

                CancellationToken ct = _cts?.Token ?? CancellationToken.None;
                _loop = Task.Run(() => LoopAsync(ct), ct);
            }
        }

        private async Task LoopAsync(CancellationToken ct)
        {
            // Vive mientras el host esté activo: no salir en idle (polling reencola).
            while (!ct.IsCancellationRequested && !_disposed)
            {
                if (!_cola.TryDequeue(out string? path))
                {
                    await Task.Delay(500, ct).ConfigureAwait(false);
                    continue;
                }

                if (!_isCajaAbierta())
                {
                    // Reencolar: quitar marca para que el poll lo vuelva a tomar.
                    _enColaOProceso.TryRemove(path, out _);
                    Log("ESPERA_CAJA " + Path.GetFileName(path));
                    SetStatus("Factura en espera · abre la caja");
                    await Task.Delay(1200, ct).ConfigureAwait(false);
                    continue;
                }

                await ProcesarArchivoAsync(path, ct).ConfigureAwait(false);
            }
        }

        private async Task ProcesarArchivoAsync(string path, CancellationToken ct)
        {
            string fileName = Path.GetFileName(path);
            try
            {
                if (_procesando)
                {
                    // Serializar: devolver a cola.
                    _enColaOProceso.TryRemove(path, out _);
                    await Task.Delay(300, ct).ConfigureAwait(false);
                    EncolarSiAplica(path);
                    return;
                }

                _procesando = true;
                SetStatus($"Procesando {fileName}…");
                Log("PROCESS_BEGIN " + fileName);
                DISEÑO.FrmFacturaGastoHud.ShowLeyendo(fileName);

                if (!File.Exists(path))
                {
                    Log("GONE " + fileName);
                    DISEÑO.FrmFacturaGastoHud.ShowError(
                        "La factura desapareció antes de procesarse.",
                        fileName);
                    return;
                }

                if (!await EsperarArchivoEstableAsync(path, ct).ConfigureAwait(false))
                {
                    Log("NOT_STABLE " + fileName);
                    _enColaOProceso.TryRemove(path, out _);
                    DISEÑO.FrmFacturaGastoHud.ShowError(
                        "El archivo aún se estaba copiando. Se reintentará.",
                        fileName);
                    return;
                }

                string ext = Path.GetExtension(path).ToLowerInvariant();
                byte[]? jpeg = null;
                if (ext != ".pdf")
                {
                    var load = await Task.Run(() =>
                    {
                        bool ok = ProductoImagenHelper.TryLoadAsJpegBytes(
                            path,
                            maxSide: Math.Max(OllamaOptions.VisionMaxSide, 1280),
                            out byte[]? bytes,
                            out string? err);
                        return (ok, bytes, err);
                    }, ct).ConfigureAwait(false);

                    jpeg = load.bytes;
                    if (!load.ok || jpeg == null || jpeg.Length == 0)
                    {
                        string detail =
                            "Detalle: " + (load.err ?? "Formato no legible o archivo bloqueado.") +
                            "\nSoportados: JPG, PNG, BMP, WEBP, PDF.";
                        Log("LOAD_FAIL " + fileName + " " + load.err);
                        await FinalizarErrorAsync(
                                path,
                                "No se pudo abrir la imagen de la factura.",
                                detail)
                            .ConfigureAwait(false);
                        return;
                    }

                    Log("LOAD_OK " + fileName + " jpegBytes=" + jpeg.Length);
                }

                // Token propio del proceso: NO se cancela al navegar módulos.
                // Solo se cancela si se detiene el host de la app o timeout duro.
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct);
                int timeoutSec = Math.Max(OllamaOptions.TimeoutSeconds, 180) * 3 + 120;
                linked.CancelAfter(TimeSpan.FromSeconds(timeoutSec));
                Log($"ORCH_TIMEOUT_SEC={timeoutSec}");

                var orch = new FacturaGastoOrchestrator();
                FacturaGastoAutoResult result = await orch.ProcesarYRegistrarAsync(
                        path,
                        jpeg,
                        Sesion.Usuario,
                        _isCajaAbierta,
                        linked.Token)
                    .ConfigureAwait(false);

                Log("ORCH " + (result.Success ? "OK" : "FAIL") +
                    " msg=" + result.Message +
                    " trace=" + result.PipelineTrace +
                    " detail=" + (result.ErrorDetail ?? ""));

                if (result.Success)
                {
                    MoverArchivo(path, FacturaGastosFolder.Procesadas(_root!), result.SourceFileName);
                    SetStatus($"OK · {fileName}");

                    // Cableado sistema: refresca caja / egresos en cualquier módulo abierto.
                    NotificarUi(() =>
                    {
                        try { _onGastoRegistrado(); } catch { /* ignore */ }
                    });

                    DISEÑO.FrmFacturaGastoHud.ShowExito(
                        result.Message + (result.Monto is > 0 ? $"\nMonto: {result.Monto:C}" : string.Empty),
                        () =>
                        {
                            try { _onVerMovimientos(); } catch { /* ignore */ }
                        });
                }
                else if (result.Message.Contains("caja está cerrada", StringComparison.OrdinalIgnoreCase))
                {
                    _enColaOProceso.TryRemove(path, out _);
                    SetStatus("Caja cerrada · factura en espera");
                    DISEÑO.FrmFacturaGastoHud.ShowError(
                        "Caja cerrada. Abre la caja para registrar el egreso.",
                        fileName);
                }
                else
                {
                    await FinalizarErrorAsync(path, result.Message, BuildErrorDetail(result))
                        .ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                Log("CANCEL " + fileName);
                _enColaOProceso.TryRemove(path, out _);
                DISEÑO.FrmFacturaGastoHud.ShowError(
                    "Proceso cancelado o tiempo agotado leyendo la factura.",
                    fileName + "\nDeja la app abierta; la IA puede tardar 1–3 min.");
            }
            catch (Exception ex)
            {
                Log("EX " + fileName + " " + ex);
                await FinalizarErrorAsync(path, "Error inesperado al procesar factura.", ex.Message)
                    .ConfigureAwait(false);
            }
            finally
            {
                _procesando = false;
                if (!string.IsNullOrWhiteSpace(_root))
                    SetStatus($"Vigilando {Path.GetFileName(_root)}");
            }
        }

        private async Task FinalizarErrorAsync(string path, string message, string? detail)
        {
            string name = Path.GetFileName(path);
            MoverArchivo(path, FacturaGastosFolder.Errores(_root!), name);
            SetStatus($"Error · {name}");
            string fullDetail = detail ?? string.Empty;
            DISEÑO.FrmFacturaGastoHud.ShowError(message, fullDetail);
            await Task.CompletedTask;
        }

        private static string BuildErrorDetail(FacturaGastoAutoResult result)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(result.SourceFileName))
                parts.Add("Archivo: " + result.SourceFileName);
            if (!string.IsNullOrWhiteSpace(result.PipelineTrace))
                parts.Add("Pipeline: " + result.PipelineTrace);
            if (!string.IsNullOrWhiteSpace(result.ErrorDetail))
                parts.Add(result.ErrorDetail);
            return string.Join("\n", parts);
        }

        private static void MoverArchivo(string path, string destDir, string? preferredName)
        {
            try
            {
                if (!File.Exists(path))
                    return;

                Directory.CreateDirectory(destDir);
                string name = preferredName ?? Path.GetFileName(path);
                string dest = Path.Combine(destDir, name);
                if (File.Exists(dest))
                {
                    string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string stem = Path.GetFileNameWithoutExtension(name);
                    string ext = Path.GetExtension(name);
                    dest = Path.Combine(destDir, $"{stem}_{stamp}{ext}");
                }

                File.Move(path, dest, overwrite: false);
            }
            catch
            {
                // best-effort
            }
        }

        private static async Task<bool> EsperarArchivoEstableAsync(string path, CancellationToken ct)
        {
            long last = -1;
            int estableCount = 0;
            for (int i = 0; i < 25; i++)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    if (!File.Exists(path))
                        return false;

                    var info = new FileInfo(path);
                    info.Refresh();
                    long size = info.Length;
                    if (size > 0 && size == last)
                    {
                        estableCount++;
                        if (estableCount >= 2)
                        {
                            using var fs = new FileStream(
                                path,
                                FileMode.Open,
                                FileAccess.Read,
                                FileShare.ReadWrite | FileShare.Delete);
                            return fs.Length > 0;
                        }
                    }
                    else
                    {
                        estableCount = 0;
                    }

                    last = size;
                }
                catch
                {
                    estableCount = 0;
                }

                await Task.Delay(400, ct).ConfigureAwait(false);
            }

            try
            {
                return File.Exists(path) && new FileInfo(path).Length > 0;
            }
            catch
            {
                return false;
            }
        }

        private void SetStatus(string text)
        {
            _status = text;
            try { _onStatusChanged?.Invoke(text); } catch { /* ignore */ }
        }

        private void Log(string line)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_root))
                    return;

                string path = Path.Combine(_root, "_auto.log");
                string row = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + line + Environment.NewLine;
                lock (_logSync)
                    File.AppendAllText(path, row, Encoding.UTF8);
            }
            catch
            {
                // ignore
            }
        }

        private static string SafeEnv()
        {
            try { return AppConfig.PeekEnvironment(); }
            catch
            {
                try { return AppConfig.EnvironmentName; }
                catch { return "?"; }
            }
        }

        private void NotificarUi(Action action)
        {
            try
            {
                if (_owner.IsDisposed)
                    return;

                if (_owner.InvokeRequired)
                    _owner.BeginInvoke(action);
                else
                    action();
            }
            catch
            {
                // ignore
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            Stop();
            _cts?.Dispose();
            _cts = null;
        }
    }
}
