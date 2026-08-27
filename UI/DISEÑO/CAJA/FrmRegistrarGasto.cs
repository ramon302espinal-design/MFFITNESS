using BLL.Commands;
using BLL.Services;
using CORE;
using CORE.Commands;
using System;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UI.Helpers;
using UI.Theme;

namespace UI.DISEÑO
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmRegistrarGasto : Form
    {
        private readonly bool _esIngreso;
        private CancellationTokenSource? _ctsIa;
        private bool _leyendoFactura;

        public FrmRegistrarGasto() : this(esIngreso: false)
        {
        }

        public FrmRegistrarGasto(bool esIngreso)
        {
            _esIngreso = esIngreso;
            InitializeComponent();
            ThemeHost.Attach(this);

            Text = _esIngreso ? "Registrar Ingreso" : "Registrar Gasto";
            if (lblTitulo != null)
                lblTitulo.Text = _esIngreso ? "Registrar Ingreso" : "Registrar Gasto";

            if (cmbTipoMovimiento != null)
            {
                cmbTipoMovimiento.Items.Clear();
                cmbTipoMovimiento.Items.Add(_esIngreso ? "Ingreso" : "Egreso");
                cmbTipoMovimiento.SelectedIndex = 0;
                cmbTipoMovimiento.Enabled = false;
            }

            if (lblTipoMovimiento != null)
                lblTipoMovimiento.Text = "Tipo de movimiento";

            // IA solo en gastos: sugerencia editable; nunca registra sola.
            if (btnLeerFactura != null)
                btnLeerFactura.Visible = !_esIngreso;
            if (btnTomarFotoFactura != null)
                btnTomarFotoFactura.Visible = !_esIngreso;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _ctsIa?.Cancel();
            _ctsIa?.Dispose();
            _ctsIa = null;
            base.OnFormClosed(e);
        }

        private async void btnTomarFotoFactura_Click(object? sender, EventArgs e)
        {
            if (_esIngreso || _leyendoFactura)
                return;

            using var cam = new FrmCapturaProductoCamara(
                soloIriun: true,
                tituloVentana: "Factura — Iriun",
                instruccionCaptura: "Iriun activa · enfoca la factura · clic para capturar · Esc cancela");

            if (cam.ShowDialog(this) != DialogResult.OK || cam.FotoCapturada == null)
                return;

            using Image foto = cam.FotoCapturada;
            await ProcesarImagenFacturaAsync(foto).ConfigureAwait(true);
        }

        private async void btnLeerFactura_Click(object? sender, EventArgs e)
        {
            if (_esIngreso || _leyendoFactura)
                return;

            using var dlg = new OpenFileDialog
            {
                Title = "Foto o escaneo de la factura",
                Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.bmp;*.webp|Todos|*.*",
                CheckFileExists = true,
                Multiselect = false
            };

            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                using var img = Image.FromFile(dlg.FileName);
                await ProcesarImagenFacturaAsync(img).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "No se pudo abrir la imagen.\n" + ex.Message,
                    "Factura IA",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private async Task ProcesarImagenFacturaAsync(Image foto)
        {
            if (_leyendoFactura)
                return;

            _leyendoFactura = true;
            _ctsIa?.Cancel();
            _ctsIa?.Dispose();
            _ctsIa = new CancellationTokenSource(TimeSpan.FromSeconds(90));
            SetUiLeyendo(true);

            try
            {
                byte[] jpeg = await Task.Run(() =>
                    ProductoImagenHelper.ToJpegBytes(foto, maxSide: 1600, quality: 85),
                    _ctsIa.Token).ConfigureAwait(true);

                var svc = new ProductoVisionService();
                FacturaVisionSuggestion suggestion = await svc
                    .AnalizarFacturaGastoAsync(jpeg, _ctsIa.Token)
                    .ConfigureAwait(true);

                AplicarSugerenciaFactura(suggestion);
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show(
                    this,
                    "La lectura de la factura se canceló o tardó demasiado.",
                    "Factura IA",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "No se pudo leer la factura.\n" + ex.Message,
                    "Factura IA",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            finally
            {
                _leyendoFactura = false;
                SetUiLeyendo(false);
            }
        }

        private void AplicarSugerenciaFactura(FacturaVisionSuggestion suggestion)
        {
            string? concepto = FacturaVisionSuggestion.SanitizeConcepto(suggestion.Concepto);
            decimal? monto = FacturaVisionSuggestion.SanitizeMonto(suggestion.Monto);

            if (string.IsNullOrWhiteSpace(concepto) && monto is null)
            {
                MessageBox.Show(
                    this,
                    "No se pudo extraer concepto ni monto. Escribe los datos manualmente.",
                    "Factura IA",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (!string.IsNullOrWhiteSpace(concepto) && txtConcepto != null)
                txtConcepto.Text = concepto;

            if (monto is > 0 && txtMonto != null)
                txtMonto.Text = monto.Value.ToString("0.##", CultureInfo.CurrentCulture);

            if (concepto != null && monto is > 0)
            {
                MessageBox.Show(
                    this,
                    "Se rellenaron concepto (detalle) y monto = TOTAL a pagar.\nRevisa y confirma antes de registrar.",
                    "Factura IA",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                txtConcepto?.Focus();
            }
            else
            {
                MessageBox.Show(
                    this,
                    "Solo se pudo leer parte de la factura. Completa lo que falte.",
                    "Factura IA",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private void SetUiLeyendo(bool leyendo)
        {
            if (btnTomarFotoFactura != null)
            {
                btnTomarFotoFactura.Enabled = !leyendo;
                btnTomarFotoFactura.Text = leyendo ? "Leyendo…" : "Tomar foto (Iriun)";
            }

            if (btnLeerFactura != null)
            {
                btnLeerFactura.Enabled = !leyendo;
                btnLeerFactura.Text = leyendo ? "Leyendo…" : "Desde archivo";
            }

            if (btnRegistrar != null)
                btnRegistrar.Enabled = !leyendo;
            if (btnCancelar != null)
                btnCancelar.Enabled = !leyendo;

            Cursor = leyendo ? Cursors.WaitCursor : Cursors.Default;
        }

        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            try
            {
                if (_leyendoFactura)
                    return;

                string concepto = txtConcepto?.Text?.Trim() ?? string.Empty;
                string montoTexto = txtMonto?.Text?.Trim() ?? string.Empty;

                if (string.IsNullOrEmpty(concepto))
                {
                    MessageBox.Show("Ingrese un concepto válido.");
                    txtConcepto?.Focus();
                    return;
                }

                if (!decimal.TryParse(montoTexto, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal monto)
                    && !decimal.TryParse(montoTexto.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out monto))
                {
                    MessageBox.Show("Ingrese un monto válido.");
                    txtMonto?.Focus();
                    return;
                }

                if (monto <= 0)
                {
                    MessageBox.Show("Ingrese un monto válido.");
                    txtMonto?.Focus();
                    return;
                }

                CommandResult result = _esIngreso
                    ? CajaCommandService.RegistrarIngreso(concepto, monto, Sesion.Usuario)
                    : CajaCommandService.RegistrarGasto(concepto, monto, Sesion.Usuario);

                if (!result.Success)
                {
                    MessageBox.Show(result.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                AppEventos.CajaCambiada();
                MessageBox.Show(result.Message, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            if (_leyendoFactura)
            {
                _ctsIa?.Cancel();
                return;
            }

            Close();
        }
    }
}
