using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Media;
using System.Windows.Forms;
using BLL.Models;
using CORE;
using UI.Theme;

namespace UI.DISEÑO
{
    /// <summary>
    /// Modal calculadora POS premium. Solo captura el cobro; no persiste en BD.
    /// Compatible con TryCobrarConCalculadora en FrmPagos (firma sin cambios).
    /// </summary>
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmPago : Form
    {
        private readonly decimal _totalAPagar;
        private MetodoPagoPOS _metodoSeleccionado = MetodoPagoPOS.Efectivo;
        private string _entradaMonto = string.Empty;
        private bool _sincronizandoTexto;
        private bool _editandoMontoDirecto;
        private Button? _billeteSeleccionado;

        private static readonly Color ColorPrincipal = Color.FromArgb(0x1A, 0x8C, 0xFF);
        private static readonly Color ColorHover = Color.FromArgb(0x15, 0x77, 0xE6);
        private static readonly Color ColorPressed = Color.FromArgb(0x0F, 0x63, 0xC9);
        private static readonly Color ColorClaro = Color.FromArgb(0xEE, 0xF6, 0xFF);
        private static readonly Color ColorTexto = Color.FromArgb(0x1F, 0x29, 0x37);
        private static readonly Color ColorSecundario = Color.FromArgb(0x6B, 0x72, 0x80);
        private static readonly Color ColorBorde = Color.FromArgb(0xD6, 0xD6, 0xD6);
        private static readonly Color ColorFondo = Color.FromArgb(0xF8, 0xF9, 0xFB);
        private static readonly Color ColorError = Color.FromArgb(0xDC, 0x26, 0x26);

        public SolicitudPagoDTO? PagoResultado { get; private set; }

        /// <summary>Constructor para el diseñador de WinForms.</summary>
        public FrmPago()
        {
            _totalAPagar = 0;
            InitializeComponent();
        }

        public FrmPago(decimal totalAPagar)
        {
            if (totalAPagar <= 0)
                throw new ArgumentOutOfRangeException(nameof(totalAPagar), "El total debe ser mayor a cero.");

            _totalAPagar = totalAPagar;
            InitializeComponent();
            if (ThemeHost.IsDesignTime())
                return;

            ConfigurarTagsYEventos();
            AplicarEstiloPremium();
            InicializarEstado();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (ThemeHost.IsDesignTime() || DesignMode)
                return;

            ThemeApplier.ApplyRoundedRegion(this, 18);
            ThemeApplier.ApplyRoundedRegion(cardTotal, 18);
            ThemeApplier.ApplyRoundedRegion(cardCambio, 20);
            ThemeApplier.ApplyRoundedRegion(btnCerrar, 12);
            ThemeApplier.ApplyRoundedRegion(btnVistaPrevia, 12);
            ThemeApplier.ApplyRoundedRegion(pnlMontoInput, 12);

            AcceptButton = btnPagar;

            foreach (Button billete in ObtenerBilletes())
                ThemeApplier.ApplyRoundedRegion(billete, 12);

            txtMontoRecibido.Focus();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (IsHandleCreated && !IsDisposed)
                ThemeApplier.ApplyRoundedRegion(this, 18);
        }

        #region Cableado

        private void ConfigurarTagsYEventos()
        {
            btnBillete50.Tag = 50m;
            btnBillete100.Tag = 100m;
            btnBillete200.Tag = 200m;
            btnBillete500.Tag = 500m;
            btnBillete1000.Tag = 1000m;
            btnBillete2000.Tag = 2000m;

            foreach (Button billete in ObtenerBilletes())
            {
                billete.Click += BilleteRapido_Click;
                billete.MouseEnter += Billete_MouseEnter;
                billete.MouseLeave += Billete_MouseLeave;
            }

            cmbMetodo.SelectedIndexChanged += cmbMetodo_SelectedIndexChanged;
            btnPagar.Click += (_, _) => ProcesarPago();
            btnCerrar.Click += (_, _) => CerrarSinProcesar();
            btnCerrar.TabStop = false;
            btnVistaPrevia.Click += btnVistaPrevia_Click;

            btnPagar.MouseEnter += (_, _) => btnPagar.BackColor = CobrarButtonStyle.VerdeHover;
            btnPagar.MouseLeave += (_, _) => btnPagar.BackColor = CobrarButtonStyle.Verde;
            btnPagar.MouseDown += (_, _) => btnPagar.BackColor = Color.FromArgb(21, 128, 61);
            btnPagar.MouseUp += (_, _) => btnPagar.BackColor = CobrarButtonStyle.VerdeHover;

            btnCerrar.MouseEnter += (_, _) => btnCerrar.BackColor = Color.FromArgb(0xF1, 0xF5, 0xF9);
            btnCerrar.MouseLeave += (_, _) => btnCerrar.BackColor = Color.Transparent;

            btnVistaPrevia.MouseEnter += (_, _) => btnVistaPrevia.BackColor = ColorClaro;
            btnVistaPrevia.MouseLeave += (_, _) => btnVistaPrevia.BackColor = Color.White;

            txtMontoRecibido.Enter += txtMontoRecibido_Enter;
            txtMontoRecibido.Leave += txtMontoRecibido_Leave;
            txtMontoRecibido.TextChanged += txtMontoRecibido_TextChanged;
            txtMontoRecibido.KeyPress += txtMontoRecibido_KeyPress;

            chkImprimirRecibo.Visible = false;
            chkImprimirRecibo.Checked = false;

            cardTotal.Paint += CardBlanca_Paint;
            cardCambio.Paint += CardCambio_Paint;
            pnlMontoInput.Paint += InputBorder_Paint;
        }

        private Button[] ObtenerBilletes() =>
        [
            btnBillete50, btnBillete100, btnBillete200,
            btnBillete500, btnBillete1000, btnBillete2000
        ];

        private void cmbMetodo_SelectedIndexChanged(object? sender, EventArgs e)
        {
            var metodo = cmbMetodo.SelectedIndex switch
            {
                1 => MetodoPagoPOS.Tarjeta,
                2 => MetodoPagoPOS.Transferencia,
                _ => MetodoPagoPOS.Efectivo
            };
            AplicarMetodoPago(metodo);
        }

        private void BilleteRapido_Click(object? sender, EventArgs e)
        {
            if (_metodoSeleccionado != MetodoPagoPOS.Efectivo)
                return;

            if (sender is not Button btn)
                return;

            decimal valor = btn.Tag switch
            {
                decimal d => d,
                int i => i,
                _ => 0m
            };

            if (valor <= 0)
                return;

            ReproducirTecla();
            _billeteSeleccionado = btn;
            ActualizarEstiloBilletes();
            AplicarBilleteRapido(valor);
        }

        private void Billete_MouseEnter(object? sender, EventArgs e)
        {
            if (sender is not Button btn || btn == _billeteSeleccionado)
                return;
            if (!btn.Enabled)
                return;
            btn.BackColor = Color.FromArgb(0x3A, 0xA3, 0xFF);
            btn.ForeColor = Color.White;
            btn.FlatAppearance.BorderSize = 0;
        }

        private void Billete_MouseLeave(object? sender, EventArgs e)
        {
            if (sender is Button)
                ActualizarEstiloBilletes();
        }

        private void btnVistaPrevia_Click(object? sender, EventArgs e)
        {
            if (_editandoMontoDirecto)
            {
                _editandoMontoDirecto = false;
                NormalizarEntradaDesdeTexto(txtMontoRecibido.Text);
                SincronizarTextoMonto();
            }

            decimal recibido = ObtenerMontoRecibidoActual();
            decimal cambio = recibido - _totalAPagar;
            string preview =
                "========== MFFITNESS ==========\r\n" +
                "VISTA PREVIA DE PAGO\r\n" +
                $"Fecha: {DateTime.Now.ToString(FechaHoraFormats.FechaHora)}\r\n" +
                "--------------------------------\r\n" +
                $"TOTAL:      {FormatearMoneda(_totalAPagar)}\r\n" +
                $"RECIBIDO:   {FormatearMoneda(recibido)}\r\n" +
                $"CAMBIO:     {FormatearMoneda(cambio)}\r\n" +
                $"MÉTODO:     {_metodoSeleccionado.ToMetodoBd()}\r\n" +
                "================================";

            using var frm = new FrmVistaPrevia(preview);
            frm.ShowDialog(this);
        }

        #endregion

        #region Teclado físico

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode is Keys.Enter or Keys.Space)
            {
                if (cmbMetodo.DroppedDown)
                    return;

                e.Handled = true;
                e.SuppressKeyPress = true;
                BeginInvoke(new Action(ProcesarPago));
                return;
            }

            if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                CerrarSinProcesar();
                return;
            }

            if (_metodoSeleccionado != MetodoPagoPOS.Efectivo || _editandoMontoDirecto)
                return;

            if (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                AgregarDigito(((int)e.KeyCode - (int)Keys.D0).ToString());
                return;
            }

            if (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                AgregarDigito(((int)e.KeyCode - (int)Keys.NumPad0).ToString());
                return;
            }

            if (e.KeyCode is Keys.Decimal or Keys.OemPeriod)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                AgregarDigito(".");
                return;
            }

            if (e.KeyCode == Keys.Back)
            {
                e.Handled = true;
                BorrarUltimoCaracter();
            }
        }

        private void txtMontoRecibido_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (_metodoSeleccionado != MetodoPagoPOS.Efectivo)
            {
                e.Handled = true;
                return;
            }

            if (char.IsDigit(e.KeyChar))
                return;
            if (e.KeyChar == '.' && !txtMontoRecibido.Text.Contains('.'))
                return;
            if (e.KeyChar == (char)Keys.Back)
                return;

            e.Handled = true;
        }

        private void txtMontoRecibido_Enter(object? sender, EventArgs e)
        {
            _editandoMontoDirecto = true;
            _sincronizandoTexto = true;
            txtMontoRecibido.Text = string.IsNullOrEmpty(_entradaMonto) ? string.Empty : _entradaMonto;
            txtMontoRecibido.SelectionStart = txtMontoRecibido.Text.Length;
            _sincronizandoTexto = false;
            pnlMontoInput.Invalidate();
        }

        private void txtMontoRecibido_Leave(object? sender, EventArgs e)
        {
            _editandoMontoDirecto = false;
            NormalizarEntradaDesdeTexto(txtMontoRecibido.Text);
            SincronizarTextoMonto();
            ActualizarCambio();
            pnlMontoInput.Invalidate();
        }

        private void txtMontoRecibido_TextChanged(object? sender, EventArgs e)
        {
            if (_sincronizandoTexto || !_editandoMontoDirecto)
                return;

            NormalizarEntradaDesdeTexto(txtMontoRecibido.Text);
            ActualizarCambio();
        }

        #endregion

        #region Lógica de monto

        private void InicializarEstado()
        {
            lblTotalMonto.Text = FormatearMoneda(_totalAPagar);
            cmbMetodo.SelectedIndex = 0;
            AplicarMetodoPago(MetodoPagoPOS.Efectivo);
            // Prefill exacto: Enter/PAGAR funciona de inmediato; billetes reemplazan si hay cambio.
            EstablecerMonto(_totalAPagar);
        }

        private void AplicarBilleteRapido(decimal valor)
        {
            // Reemplaza el monto (no suma): $50 luego $100 = $100
            EstablecerMonto(valor);
        }

        private void AplicarMetodoPago(MetodoPagoPOS metodo)
        {
            _metodoSeleccionado = metodo;
            bool esEfectivo = metodo == MetodoPagoPOS.Efectivo;

            tlpBilletes.Enabled = esEfectivo;
            txtMontoRecibido.ReadOnly = !esEfectivo;
            lblCambioMonto.Visible = true;
            lblCambioTitulo.Visible = true;

            if (esEfectivo)
            {
                SincronizarTextoMonto();
                ActualizarCambio();
            }
            else
            {
                _billeteSeleccionado = null;
                ActualizarEstiloBilletes();
                EstablecerMonto(_totalAPagar);
            }
        }

        private void AgregarDigito(string digito)
        {
            if (digito == ".")
            {
                if (_entradaMonto.Contains('.'))
                    return;
                if (string.IsNullOrEmpty(_entradaMonto))
                    _entradaMonto = "0";
                _entradaMonto += ".";
            }
            else
            {
                if (_entradaMonto == "0")
                    _entradaMonto = digito;
                else
                    _entradaMonto += digito;
            }

            _billeteSeleccionado = null;
            ActualizarEstiloBilletes();
            SincronizarTextoMonto();
            ActualizarCambio();
        }

        private void BorrarUltimoCaracter()
        {
            if (string.IsNullOrEmpty(_entradaMonto))
                return;

            _entradaMonto = _entradaMonto[..^1];
            _billeteSeleccionado = null;
            ActualizarEstiloBilletes();
            SincronizarTextoMonto();
            ActualizarCambio();
        }

        private void EstablecerMonto(decimal monto)
        {
            _entradaMonto = monto.ToString("0.##", CultureInfo.InvariantCulture);
            SincronizarTextoMonto();
            ActualizarCambio();
        }

        private void NormalizarEntradaDesdeTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                _entradaMonto = string.Empty;
                return;
            }

            string limpio = texto
                .Replace("RD$", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("$", string.Empty)
                .Trim();

            if (decimal.TryParse(limpio, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal valor) ||
                decimal.TryParse(limpio, NumberStyles.Number, CultureInfo.InvariantCulture, out valor))
            {
                _entradaMonto = valor.ToString("0.##", CultureInfo.InvariantCulture);
                return;
            }

            var filtrado = new System.Text.StringBuilder();
            bool puntoUsado = false;
            foreach (char c in limpio)
            {
                if (char.IsDigit(c))
                    filtrado.Append(c);
                else if ((c == '.' || c == ',') && !puntoUsado)
                {
                    filtrado.Append('.');
                    puntoUsado = true;
                }
            }

            _entradaMonto = filtrado.ToString();
        }

        private void SincronizarTextoMonto()
        {
            _sincronizandoTexto = true;

            if (_editandoMontoDirecto)
                txtMontoRecibido.Text = string.IsNullOrEmpty(_entradaMonto) ? string.Empty : _entradaMonto;
            else if (string.IsNullOrEmpty(_entradaMonto))
                txtMontoRecibido.Text = string.Empty;
            else
                txtMontoRecibido.Text = ObtenerMontoRecibidoActual().ToString("N2", CultureInfo.CurrentCulture);

            _sincronizandoTexto = false;
        }

        private void ActualizarCambio()
        {
            decimal recibido = ObtenerMontoRecibidoActual();
            decimal cambio = recibido - _totalAPagar;
            lblCambioMonto.Text = FormatearMoneda(Math.Max(0, cambio));
            lblCambioMonto.ForeColor = Color.White;

            if (_metodoSeleccionado == MetodoPagoPOS.Efectivo && recibido > 0 && cambio < 0)
                lblCambioMonto.Text = FormatearMoneda(cambio);
        }

        private decimal ObtenerMontoRecibidoActual()
        {
            if (_metodoSeleccionado != MetodoPagoPOS.Efectivo)
                return _totalAPagar;

            if (string.IsNullOrWhiteSpace(_entradaMonto))
                return 0m;

            return decimal.TryParse(_entradaMonto, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal valor)
                ? valor
                : 0m;
        }

        #endregion

        #region Procesar

        private bool ValidarAntesDeProcesar(out string mensaje)
        {
            decimal recibido = Math.Round(ObtenerMontoRecibidoActual(), 2, MidpointRounding.AwayFromZero);
            decimal total = Math.Round(_totalAPagar, 2, MidpointRounding.AwayFromZero);

            if (_metodoSeleccionado == MetodoPagoPOS.Efectivo && recibido < total)
            {
                mensaje = $"El monto recibido ({FormatearMoneda(recibido)}) es menor al total a pagar ({FormatearMoneda(total)}).";
                return false;
            }

            mensaje = string.Empty;
            return true;
        }

        private void ProcesarPago()
        {
            // Siempre sincroniza el texto visible antes de validar (evita monto 0 por foco).
            NormalizarEntradaDesdeTexto(txtMontoRecibido.Text);
            if (string.IsNullOrWhiteSpace(_entradaMonto) && _metodoSeleccionado == MetodoPagoPOS.Efectivo)
                EstablecerMonto(_totalAPagar);
            else
                SincronizarTextoMonto();

            _editandoMontoDirecto = false;

            if (!ValidarAntesDeProcesar(out string mensaje))
            {
                SystemSounds.Exclamation.Play();
                MessageBox.Show(mensaje, "Monto insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal recibido = Math.Round(ObtenerMontoRecibidoActual(), 2, MidpointRounding.AwayFromZero);
            if (_metodoSeleccionado != MetodoPagoPOS.Efectivo)
                recibido = Math.Round(_totalAPagar, 2, MidpointRounding.AwayFromZero);

            PagoResultado = new SolicitudPagoDTO
            {
                TotalAPagar = Math.Round(_totalAPagar, 2, MidpointRounding.AwayFromZero),
                MontoRecibido = recibido,
                MetodoSeleccionado = _metodoSeleccionado,
                DebeImprimirRecibo = false
            };

            SystemSounds.Asterisk.Play();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void CerrarSinProcesar()
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        #endregion

        #region Estilo

        private void AplicarEstiloPremium()
        {
            BackColor = ColorFondo;
            Font = CrearFuente(10f, FontStyle.Regular);

            lblTitulo.Font = CrearFuente(32f, FontStyle.Bold);
            lblTitulo.ForeColor = ColorTexto;

            lblTotalEtiqueta.Font = CrearFuente(20f, FontStyle.Bold);
            lblTotalEtiqueta.ForeColor = ColorSecundario;
            lblTotalMonto.Font = CrearFuente(60f, FontStyle.Bold);
            lblTotalMonto.ForeColor = ColorPrincipal;

            lblMetodoTitulo.Font = CrearFuente(22f, FontStyle.Bold);
            lblMetodoTitulo.ForeColor = ColorTexto;

            lblMontoTitulo.Font = CrearFuente(22f, FontStyle.Bold);
            lblMontoTitulo.ForeColor = ColorTexto;
            lblMontoSubtexto.Font = CrearFuente(18f, FontStyle.Regular);
            lblMontoSubtexto.ForeColor = ColorSecundario;

            lblCambioTitulo.Font = CrearFuente(28f, FontStyle.Bold);
            lblCambioTitulo.ForeColor = Color.White;
            lblCambioMonto.Font = CrearFuente(44f, FontStyle.Bold);
            lblCambioMonto.ForeColor = Color.White;

            chkImprimirRecibo.Font = CrearFuente(18f, FontStyle.Regular);
            chkImprimirRecibo.ForeColor = ColorTexto;

            btnPagar.Font = CrearFuente(28f, FontStyle.Bold);
            btnVistaPrevia.Font = CrearFuente(16f, FontStyle.Bold);
            txtMontoRecibido.Font = CrearFuente(22f, FontStyle.Bold);
            cmbMetodo.Font = CrearFuente(18f, FontStyle.Regular);

            ActualizarEstiloBilletes();
        }

        private void ActualizarEstiloBilletes()
        {
            foreach (Button btn in ObtenerBilletes())
            {
                bool seleccionado = btn == _billeteSeleccionado;
                btn.Font = CrearFuente(16f, FontStyle.Bold);

                if (seleccionado)
                {
                    btn.BackColor = ColorPrincipal;
                    btn.ForeColor = Color.White;
                    btn.FlatAppearance.BorderSize = 0;
                }
                else
                {
                    btn.BackColor = Color.White;
                    btn.ForeColor = ColorTexto;
                    btn.FlatAppearance.BorderSize = 1;
                    btn.FlatAppearance.BorderColor = ColorBorde;
                }

                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(0x3A, 0xA3, 0xFF);
                btn.FlatAppearance.MouseDownBackColor = ColorHover;
            }
        }

        private static Font CrearFuente(float size, FontStyle style)
        {
            try
            {
                return new Font("Segoe UI Variable", size, style, GraphicsUnit.Pixel);
            }
            catch
            {
                return new Font("Segoe UI", size, style, GraphicsUnit.Pixel);
            }
        }

        private static void ReproducirTecla() => SystemSounds.Beep.Play();

        private static string FormatearMoneda(decimal valor) => $"RD${valor:N2}";

        private void CardBlanca_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel panel)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var shadow = new SolidBrush(Color.FromArgb(25, 0, 0, 0));
            e.Graphics.FillRectangle(shadow, new Rectangle(4, 6, panel.Width - 8, panel.Height - 6));
        }

        private void CardCambio_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel panel)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var brush = new SolidBrush(ColorPrincipal);
            using var path = ThemeApplier.CreateRoundedPath(new Rectangle(0, 0, panel.Width - 1, panel.Height - 1), 20);
            e.Graphics.FillPath(brush, path);
        }

        private void InputBorder_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel panel)
                return;

            Color borde = txtMontoRecibido.Focused ? ColorPrincipal : ColorBorde;
            using var pen = new Pen(borde, txtMontoRecibido.Focused ? 2f : 1f);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var path = ThemeApplier.CreateRoundedPath(new Rectangle(0, 0, panel.Width - 1, panel.Height - 1), 12);
            e.Graphics.DrawPath(pen, path);
        }

        #endregion
    }
}
