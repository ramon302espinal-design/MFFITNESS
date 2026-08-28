using BLL;

using BLL.Services;

using CORE;

using CORE.Ollama;

using System;

using System.Collections.Generic;

using System.Data;

using System.Drawing;

using System.Globalization;

using System.IO;

using System.Threading;

using System.Threading.Tasks;

using System.Windows.Forms;

using UI.Helpers;



namespace UI.DISEÑO

{

    public partial class FrmProductos

    {

        private ContextMenuStrip? _menuAñadirFotoManual;

        private string? _rutaImagenPendiente;

        private byte[]? _jpegPendiente;

        private CancellationTokenSource? _visionCts;



        /// <summary>Precalienta Ollama al abrir inventario (menos espera al capturar).</summary>

        internal static void PrecalentarVisionIaEnSegundoPlano()

        {

            _ = Task.Run(async () => await ProductoVisionService.WarmUpAsync().ConfigureAwait(false));

        }



        private void btnAgregarProductos_Click(object? sender, EventArgs e)

        {

            _ = CapturarDosFotosYAnalizarAsync();

        }



        private void btnAñadirFoto_Click(object? sender, EventArgs e)

        {

            AsegurarMenuAñadirFotoManual();

            Control anchor = sender as Control ?? btnAñadirFoto;

            _menuAñadirFotoManual!.Show(anchor, new Point(0, anchor.Height));

        }



        private void AsegurarMenuAñadirFotoManual()

        {

            if (_menuAñadirFotoManual != null)

                return;



            _menuAñadirFotoManual = new ContextMenuStrip();

            _menuAñadirFotoManual.Items.Add("Tomar foto", null, (_, _) => _ = TomarFotoProductoManualAsync());

            _menuAñadirFotoManual.Items.Add("Escoger archivo", null, (_, _) => _ = EscogerArchivoProductoManualAsync());

        }



        /// <summary>
        /// Collage dual: izquierda nombre + derecha código. IA al cerrar OK (desde FrmProductos).
        /// </summary>
        private async Task CapturarDosFotosYAnalizarAsync()
        {
            try
            {
                using var dual = new FrmCapturaProductoDualCamara();
                if (dual.ShowDialog(this) != DialogResult.OK || dual.FotoNombre == null)
                    return;

                using Image fotoNombre = dual.FotoNombre;
                await ProcesarFotosProductoIaAsync(fotoNombre, dual.JpegCodigo);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Agregar con foto", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }



        private async Task TomarFotoProductoManualAsync()

        {

            try

            {

                using var cam = new FrmCapturaProductoCamara();

                if (cam.ShowDialog(this) != DialogResult.OK || cam.FotoCapturada == null)

                    return;



                using Image foto = cam.FotoCapturada;

                await ProcesarImagenManualAsync(foto);

            }

            catch (Exception ex)

            {

                MessageBox.Show(this, ex.Message, "Tomar foto", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }

        }



        private async Task EscogerArchivoProductoManualAsync()

        {

            try

            {

                using var dlg = new OpenFileDialog

                {

                    Title = "Escoger foto del producto",

                    Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.bmp;*.webp|Todos|*.*",

                    CheckFileExists = true,

                    Multiselect = false

                };



                if (dlg.ShowDialog(this) != DialogResult.OK)

                    return;



                using var img = Image.FromFile(dlg.FileName);

                await ProcesarImagenManualAsync(img);

            }

            catch (Exception ex)

            {

                MessageBox.Show(this, ex.Message, "Escoger archivo", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }

        }



        private Task ProcesarImagenManualAsync(Image foto)

        {

            byte[] jpegDisco = ProductoImagenHelper.ToJpegBytes(foto, maxSide: 1280, quality: 88);



            try

            {

                _jpegPendiente = jpegDisco;

                _rutaImagenPendiente = ProductoImagenStorage.GuardarPendiente(jpegDisco);

                MostrarFotoEnPreview(_rutaImagenPendiente);



                int? productoId = ObtenerProductoIdSeleccionado();

                if (productoId is > 0)

                {

                    string path = ProductoImagenStorage.GuardarParaProducto(productoId.Value, jpegDisco);

                    ProductoImagenStorage.FinalizarPendiente(productoId.Value, _rutaImagenPendiente);

                    productoBLL.ActualizarRutaImagen(productoId.Value, path);

                    _rutaImagenPendiente = path;

                    MostrarFotoEnPreview(path);

                    if (lblFotoaqui != null)

                        lblFotoaqui.Text = "FOTO DEL PRODUCTO (guardada)";

                }

                else if (lblFotoaqui != null)

                {

                    lblFotoaqui.Text = "FOTO PENDIENTE — pulsa GUARDAR";

                }

            }

            catch (Exception ex)

            {

                MessageBox.Show(this, "No se pudo guardar la foto: " + ex.Message,

                    "Añadir foto", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }



            return Task.CompletedTask;

        }



        private int? ObtenerProductoIdSeleccionado()

        {

            if (dgvProductos.CurrentRow == null)

                return null;



            object? val = dgvProductos.CurrentRow.Cells["Id"]?.Value;

            if (val == null || val == DBNull.Value)

                return null;



            return Convert.ToInt32(val);

        }



        private async Task ProcesarFotosProductoIaAsync(Image fotoNombre, byte[]? jpegCodigoOpcional)

        {

            byte[] jpegDisco = ProductoImagenHelper.ToJpegBytes(fotoNombre, maxSide: 1280, quality: 88);

            byte[] jpegIaNombre = ProductoImagenHelper.ToJpegBytes(

                fotoNombre,

                maxSide: Math.Min(OllamaOptions.VisionMaxSide, 896),

                quality: 82);



            try

            {

                _rutaImagenPendiente = ProductoImagenStorage.GuardarPendiente(jpegDisco);

                _jpegPendiente = jpegDisco;

                MostrarFotoEnPreview(_rutaImagenPendiente);

                ActualizarEstadoVision("Analizando nombre con IA…");

            }

            catch (Exception ex)

            {

                MessageBox.Show(this, "No se pudo guardar la foto en disco: " + ex.Message,

                    "Foto", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;

            }



            await AnalizarFotosYRellenarAsync(jpegIaNombre, jpegCodigoOpcional);

        }



        private async Task AnalizarFotosYRellenarAsync(byte[] jpegNombre, byte[]? jpegCodigo)

        {

            _visionCts?.Cancel();

            _visionCts?.Dispose();

            _visionCts = new CancellationTokenSource();

            CancellationToken ct = _visionCts.Token;



            btnAgregarProductos.Enabled = false;

            UseWaitCursor = true;

            try

            {

                AppConfig.LoadOllamaOptions();

                var cats = ObtenerNombresCategoriasCatalogo();

                var vision = new ProductoVisionService();



                Task<ProductoVisionSuggestion> taskNombre = Task.Run(async () =>

                    await vision.AnalizarNombreEmpaqueAsync(jpegNombre, cats, ct).ConfigureAwait(false), ct);



                Task<string?> taskCodigoLocal = jpegCodigo != null

                    ? Task.Run(() => ProductoBarcodeImageReader.TryReadFromJpegBytes(jpegCodigo), ct)

                    : Task.FromResult<string?>(null);



                ActualizarEstadoVision("Leyendo nombre (IA) y código (local)…");



                await Task.WhenAll(taskNombre, taskCodigoLocal).ConfigureAwait(true);



                ProductoVisionSuggestion suggestion = await taskNombre.ConfigureAwait(true);

                string? codigo = await taskCodigoLocal.ConfigureAwait(true);



                if (codigo == null && jpegCodigo != null)

                {

                    ActualizarEstadoVision("Código: reintento con IA…");

                    codigo = await Task.Run(async () =>

                        await vision.LeerCodigoBarraImagenAsync(jpegCodigo, ct).ConfigureAwait(false), ct)

                        .ConfigureAwait(true);



                    if (!string.IsNullOrWhiteSpace(codigo)

                        && !ProductoBarcodeNormalizer.TryNormalizeBarcode(codigo, out codigo))

                        codigo = null;

                }



                AplicarSugerenciaVision(suggestion, codigo);

                ActualizarEstadoVision("FOTO DEL PRODUCTO (guardada en PC)");



                string msgCodigo = codigo != null

                    ? $"Código: {codigo}\n"

                    : (jpegCodigo != null ? "Código: no detectado (completa a mano).\n" : "");



                MessageBox.Show(

                    this,

                    "Sugerencia de IA aplicada.\n" +

                    msgCodigo +

                    "La foto del nombre quedó en disco (no en SQL).\n\n" +

                    "Revisa los campos y pulsa GUARDAR.\n" +

                    "El producto NO se guardó automáticamente.",

                    "Producto por foto",

                    MessageBoxButtons.OK,

                    MessageBoxIcon.Information);

            }

            catch (OperationCanceledException)

            {

                ActualizarEstadoVision("FOTO DEL PRODUCTO (análisis cancelado)");

            }

            catch (Exception ex)

            {

                ActualizarEstadoVision("FOTO DEL PRODUCTO (sin IA)");

                MessageBox.Show(

                    this,

                    "La foto sí se guardó en el PC, pero la IA falló:\n" + ex.Message +

                    "\n\nPuedes completar los datos a mano y GUARDAR.",

                    "Visión IA",

                    MessageBoxButtons.OK,

                    MessageBoxIcon.Warning);

            }

            finally

            {

                btnAgregarProductos.Enabled = true;

                UseWaitCursor = false;

            }

        }



        private void ActualizarEstadoVision(string texto)

        {

            if (lblFotoaqui == null || lblFotoaqui.IsDisposed)

                return;



            if (InvokeRequired)

            {

                BeginInvoke(new Action(() => lblFotoaqui.Text = texto));

                return;

            }



            lblFotoaqui.Text = texto;

        }



        internal string? ObtenerRutaImagenParaGuardar(int? productoIdExistente)

        {

            if (_jpegPendiente == null || _jpegPendiente.Length == 0)

                return null;



            try

            {

                if (productoIdExistente is > 0)

                {

                    string path = ProductoImagenStorage.GuardarParaProducto(productoIdExistente.Value, _jpegPendiente);

                    ProductoImagenStorage.FinalizarPendiente(productoIdExistente.Value, _rutaImagenPendiente);

                    return path;

                }



                return _rutaImagenPendiente;

            }

            catch

            {

                return _rutaImagenPendiente;

            }

        }



        internal void ConfirmarImagenTrasAlta(int productoId)

        {

            if (productoId <= 0 || _jpegPendiente == null)

                return;



            try

            {

                string path = ProductoImagenStorage.GuardarParaProducto(productoId, _jpegPendiente);

                ProductoImagenStorage.FinalizarPendiente(productoId, _rutaImagenPendiente);

                _rutaImagenPendiente = path;

                MostrarFotoEnPreview(path);

            }

            catch

            {

                // best-effort

            }

        }



        private void MostrarFotoEnPreview(string? ruta)

        {

            string? real = ProductoImagenStorage.ResolverRutaExistente(ruta);

            Image? old = picFotoProducto.Image;

            picFotoProducto.Image = null;

            old?.Dispose();



            if (real == null)

                return;



            using var fs = new FileStream(real, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            using var tmp = Image.FromStream(fs);

            picFotoProducto.Image = new Bitmap(tmp);

        }



        private void LimpiarFotoPendiente()

        {

            _visionCts?.Cancel();

            _rutaImagenPendiente = null;

            _jpegPendiente = null;

            _ultimoPreviewProductoId = -1;

            Image? old = picFotoProducto.Image;

            picFotoProducto.Image = null;

            old?.Dispose();

            if (lblFotoaqui != null)

                lblFotoaqui.Text = "FOTO DEL PRODUCTO";

        }



        private IReadOnlyList<string> ObtenerNombresCategoriasCatalogo()

        {

            var list = new List<string>();

            if (cmbCategoria.DataSource == null)

                return list;



            for (int i = 0; i < cmbCategoria.Items.Count; i++)

            {

                if (cmbCategoria.Items[i] is not DataRowView row)

                    continue;

                string nombre = row["Nombre"]?.ToString()?.Trim() ?? string.Empty;

                if (nombre.Length > 0)

                    list.Add(nombre);

            }



            return list;

        }



        private void AplicarSugerenciaVision(ProductoVisionSuggestion s, string? codigoBarra = null)

        {

            if (!string.IsNullOrWhiteSpace(s.Nombre))

                txtNombre.Text = s.Nombre.Trim();



            if (!string.IsNullOrWhiteSpace(codigoBarra))

                txtCodigo.Text = codigoBarra;



            if (s.PrecioCompraEstimado.HasValue && s.PrecioCompraEstimado.Value > 0)

                txtCompra.Text = s.PrecioCompraEstimado.Value.ToString("0.##", CultureInfo.CurrentCulture);



            if (s.PrecioVentaEstimado.HasValue && s.PrecioVentaEstimado.Value > 0)

                txtVenta.Text = s.PrecioVentaEstimado.Value.ToString("0.##", CultureInfo.CurrentCulture);



            if (string.IsNullOrWhiteSpace(txtStockMinimo.Text))

                txtStockMinimo.Text = "1";



            SeleccionarCategoriaPorNombre(s.Categoria);



            if (!string.IsNullOrWhiteSpace(codigoBarra))

            {

                txtCodigo.Focus();

                txtCodigo.SelectAll();

            }

            else

            {

                txtNombre.Focus();

                txtNombre.SelectAll();

            }

        }



        private void SeleccionarCategoriaPorNombre(string? categoria)

        {

            if (string.IsNullOrWhiteSpace(categoria) || cmbCategoria.DataSource == null)

                return;



            string wanted = NormalizarTexto(categoria);

            int bestIndex = -1;

            int bestScore = 0;



            for (int i = 0; i < cmbCategoria.Items.Count; i++)

            {

                if (cmbCategoria.Items[i] is not DataRowView row)

                    continue;



                string nombre = row["Nombre"]?.ToString()?.Trim() ?? string.Empty;

                string n = NormalizarTexto(nombre);

                if (n.Length == 0)

                    continue;



                int score = 0;

                if (n == wanted)

                    score = 100;

                else if (n.Contains(wanted) || wanted.Contains(n))

                    score = 70;

                else if (n.StartsWith(wanted) || wanted.StartsWith(n))

                    score = 50;



                if (score > bestScore)

                {

                    bestScore = score;

                    bestIndex = i;

                }

            }



            if (bestIndex >= 0)

                cmbCategoria.SelectedIndex = bestIndex;

        }



        private static string NormalizarTexto(string value)

        {

            string form = value.Trim().ToLowerInvariant().Normalize(System.Text.NormalizationForm.FormD);

            var sb = new System.Text.StringBuilder(form.Length);

            foreach (char c in form)

            {

                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)

                    != System.Globalization.UnicodeCategory.NonSpacingMark)

                    sb.Append(c);

            }

            return sb.ToString().Normalize(System.Text.NormalizationForm.FormC);

        }

    }

}


