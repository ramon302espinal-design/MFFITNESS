using BLL;
using BLL.Models;
using CORE;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UI.Helpers;
using UI.Theme;

namespace UI.DISEÑO.CHAT
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmChat : Form
    {
        private readonly ChatBLL _chatBll = new();
        private List<ChatConversacionDto> _conversaciones = new();
        private int? _clienteSeleccionadoId;
        private int? _clienteInicialId;
        private bool _enviando;
        private int _ultimoConteoMensajes;
        private long _ultimoMaxIdMensajes;
        private int _ultimoNoLeidosConocido = -1;
        private int _ultimoMaxIdEntradaConocido = -1;
        private string? _rutaPdfPendiente;
        private bool _suprimirSeleccion;
        private int _cargaMensajesToken;
        private CancellationTokenSource? _cargaMensajesCts;
        private System.Windows.Forms.Timer? _timerBuscar;
        private System.Windows.Forms.Timer? _timerEstadoWebhook;
        private ToolTip? _toolTipEstado;
        private Label? _lblEstadoMensajes;
        private CheckBox? _chkModoPrueba;
        private Label? _lblVentana;
        private bool _ajustandoModoPrueba;
        private int _ultimaHuellaEstados = int.MinValue;
        private int _cargaListaToken;
        private int _tickEstadoOcupado;
        private int _burbujasClienteId;
        private readonly List<BurbujaSlot> _burbujas = new();
        private readonly Font _fuenteListaNombre = new("Segoe UI", 10F, FontStyle.Bold);
        private readonly Font _fuenteListaDetalle = new("Segoe UI", 9F);
        private readonly Font _fuenteBurbuja = new("Segoe UI", 10F);
        private readonly Font _fuenteBurbujaMeta = new("Segoe UI", 7.5F);

        public FrmChat() : this(null)
        {
        }

        public FrmChat(int? clienteInicialId)
        {
            _clienteInicialId = clienteInicialId;
            InitializeComponent();
            ThemeHost.Attach(this);
            if (ThemeHost.IsDesignTime())
                return;

            ModuloNavBar.Wire(panelNav, this, ModuloNavBar.ModuloChat);
            lstConversaciones.ItemHeight = 56;
        }

        /// <summary>True si el chat está abierto y el miembro indicado está seleccionado.</summary>
        public static bool EstaViendoCliente(int clienteId)
        {
            if (clienteId <= 0)
                return false;

            foreach (Form form in Application.OpenForms)
            {
                if (form is FrmChat chat && !chat.IsDisposed && chat.Visible && chat.EsClienteActivo(clienteId))
                    return true;
            }

            return false;
        }

        public bool EsClienteActivo(int clienteId) =>
            _clienteSeleccionadoId == clienteId && Visible;

        private void FrmChat_Load(object sender, EventArgs e)
        {
            if (ThemeHost.IsDesignTime())
                return;

            WhatsAppStackSecrets.InvalidateCache();
            ConfigurarPanelMensajes();
            ConfigurarCentroConversaciones();
            txtBuscar.PlaceholderText = "Buscar nombre o teléfono";
            ChatNotificationHost.MensajeEntranteRecibido += OnMensajeEntranteGlobal;

            ActualizarEstadoTwilio();
            _ = CargarConversacionesAsync(mantenerSeleccion: false);
            _ = BootstrapWebhookSilenciosoAsync();

            _timerBuscar = new System.Windows.Forms.Timer { Interval = 350 };
            _timerBuscar.Tick += (_, _) =>
            {
                _timerBuscar!.Stop();
                _ = CargarConversacionesAsync(mantenerSeleccion: true);
            };

            _toolTipEstado = new ToolTip { AutoPopDelay = 30_000, InitialDelay = 400 };

            _lblEstadoMensajes = new Label
            {
                AutoSize = false,
                Anchor = AnchorStyles.None,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.Gray,
                Text = "Seleccione un miembro para ver la conversación",
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            panelMensajes.Controls.Add(_lblEstadoMensajes);
            CentrarEstadoMensajes();
            panelMensajes.Resize += (_, _) => CentrarEstadoMensajes();

            lblEstadoTwilio.Cursor = Cursors.Hand;
            lblEstadoTwilio.Click += (_, _) => _ = ActivarWebhookInboundAsync();

            _timerEstadoWebhook = new System.Windows.Forms.Timer { Interval = 8000 };
            _timerEstadoWebhook.Tick += async (_, _) =>
            {
                if (Interlocked.CompareExchange(ref _tickEstadoOcupado, 1, 0) != 0)
                    return;

                try
                {
                    ActualizarEstadoTwilio();
                    await RefrescarEstadosEntregaAsync();
                    if (!TwilioSettings.WebhookInboundConfigurado || string.IsNullOrWhiteSpace(WhatsAppStackBootstrapper.TryGetNgrokPublicUrl()))
                        _ = SincronizarNgrokSilenciosoAsync();
                }
                finally
                {
                    Interlocked.Exchange(ref _tickEstadoOcupado, 0);
                }
            };
            _timerEstadoWebhook.Start();

            ActualizarContadorNoLeidos();

            if (_clienteInicialId is > 0)
                SeleccionarCliente(_clienteInicialId.Value);

            SincronizarHuellaChat();
            // Sin polling de UI: ChatNotificationHost avisa mensajes entrantes;
            // envío/búsqueda/foco refrescan solo cuando hay acción o cambio real.
            timerRefresh.Stop();
        }

        private void FrmChat_FormClosed(object sender, FormClosedEventArgs e)
        {
            ChatNotificationHost.MensajeEntranteRecibido -= OnMensajeEntranteGlobal;
            timerRefresh.Stop();
            _timerBuscar?.Stop();
            _timerBuscar?.Dispose();
            _timerEstadoWebhook?.Stop();
            _timerEstadoWebhook?.Dispose();
            _cargaMensajesCts?.Cancel();
            _cargaMensajesCts?.Dispose();
            LiberarBurbujas();
            _fuenteListaNombre.Dispose();
            _fuenteListaDetalle.Dispose();
            _fuenteBurbuja.Dispose();
            _fuenteBurbujaMeta.Dispose();
            _toolTipEstado?.Dispose();
        }

        /// <summary>Navega a un miembro (desde Estado Clientes u otro módulo).</summary>
        public void NavegarACliente(int clienteId)
        {
            if (IsDisposed || clienteId <= 0)
                return;

            _clienteInicialId = clienteId;
            _ = NavegarAClienteAsync(clienteId);
        }

        private async Task NavegarAClienteAsync(int clienteId)
        {
            await CargarConversacionesAsync(mantenerSeleccion: false);
            if (IsDisposed)
                return;

            SeleccionarCliente(clienteId);

            if (lstConversaciones.SelectedIndex < 0)
            {
                MessageBox.Show(this,
                    "Ese miembro no aparece en chat. Verifique que tenga teléfono registrado en Clientes.",
                    "WhatsApp", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void FrmChat_Activated(object? sender, EventArgs e)
        {
            if (ThemeHost.IsDesignTime())
                return;

            // Al volver al form: UI solo si hay cambio real (no rebuild cada focus).
            _ = RefrescarVistaAsync(forzar: false);
        }

        /// <summary>
        /// Recarga lista/mensajes. Con forzar=false solo si cambió huella (no leídos / max entrada / hilo abierto).
        /// La lectura va fuera de la pantalla para que el chat no se congele.
        /// </summary>
        private async Task RefrescarVistaAsync(bool forzar = false)
        {
            int noLeidos;
            int maxEntrada;
            try
            {
                (noLeidos, maxEntrada) = await Task.Run(() =>
                    (_chatBll.ContarNoLeidosTotal(), _chatBll.ObtenerMaxIdMensajeEntrada())).ConfigureAwait(true);
            }
            catch
            {
                return;
            }

            if (IsDisposed)
                return;

            bool hayCambio = forzar
                || noLeidos != _ultimoNoLeidosConocido
                || maxEntrada != _ultimoMaxIdEntradaConocido;

            if (_clienteSeleccionadoId is int cid && cid > 0)
            {
                try
                {
                    int maxId = await Task.Run(() => _chatBll.ObtenerMaxIdMensajeChat(cid)).ConfigureAwait(true);
                    if (IsDisposed)
                        return;

                    if (maxId != _ultimoMaxIdMensajes)
                    {
                        hayCambio = true;
                        _ = CargarMensajesAsync(cid);
                    }
                }
                catch
                {
                    /* ignore lectura puntual */
                }
            }

            if (!hayCambio)
                return;

            _ultimoNoLeidosConocido = noLeidos;
            _ultimoMaxIdEntradaConocido = maxEntrada;
            MostrarContadorNoLeidos(noLeidos);
            await CargarConversacionesAsync(mantenerSeleccion: true);
        }

        private void SincronizarHuellaChat()
        {
            try
            {
                _ultimoNoLeidosConocido = _chatBll.ContarNoLeidosTotal();
                _ultimoMaxIdEntradaConocido = _chatBll.ObtenerMaxIdMensajeEntrada();
            }
            catch
            {
                _ultimoNoLeidosConocido = -1;
                _ultimoMaxIdEntradaConocido = -1;
            }
        }

        private void OnMensajeEntranteGlobal(int clienteId, int mensajeId)
        {
            if (IsDisposed || !IsHandleCreated)
                return;

            void aplicar()
            {
                // Tiempo real por evento (mensaje entrante), no por timer de UI.
                _ = RefrescarVistaAsync(forzar: true);

                if (_clienteSeleccionadoId == clienteId)
                    _ = CargarMensajesAsync(clienteId);
            }

            if (InvokeRequired)
                BeginInvoke(aplicar);
            else
                aplicar();
        }

        private void ConfigurarCentroConversaciones()
        {
            panelComposer.Height = 188;

            btnPdf.Location = new Point(11, 112);
            btnPdf.Size = new Size(72, 56);
            btnUltimaFactura.Location = new Point(89, 112);
            btnUltimaFactura.Size = new Size(98, 56);
            txtMensaje.Location = new Point(197, 112);
            txtMensaje.Height = 56;
            btnEnviar.Location = new Point(panelComposer.Width - 124, 112);
            btnEnviar.Size = new Size(112, 56);
            btnEnviar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblAdjunto.Location = new Point(197, 92);

            _lblVentana = new Label
            {
                AutoSize = false,
                Location = new Point(11, 8),
                Size = new Size(430, 36),
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(60, 60, 60),
                Text = "Seleccione un miembro. El modo prueba no llama a Twilio ni guarda el mensaje."
            };

            _chkModoPrueba = new CheckBox
            {
                AutoSize = true,
                Location = new Point(480, 10),
                Text = "Modo prueba (no envía a WhatsApp)",
                Checked = true,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
            };
            _chkModoPrueba.CheckedChanged += (_, _) =>
            {
                if (_ajustandoModoPrueba)
                    return;

                if (_chkModoPrueba.Checked)
                {
                    btnEnviar.Text = "Probar";
                    return;
                }

                DialogResult ok = MessageBox.Show(this,
                    "El modo real enviará WhatsApp por Twilio al número del miembro.\r\n" +
                    "Con la ventana de 24 horas cerrada solo se ofrece la plantilla aprobada.\r\n\r\n¿Activar el envío real?",
                    "WhatsApp",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (ok != DialogResult.Yes)
                {
                    _ajustandoModoPrueba = true;
                    _chkModoPrueba.Checked = true;
                    _ajustandoModoPrueba = false;
                    return;
                }

                btnEnviar.Text = "Enviar";
            };

            var btnFicha = new Button
            {
                Text = "Ficha",
                Location = new Point(11, 72),
                Size = new Size(72, 28),
                FlatStyle = FlatStyle.Flat
            };
            btnFicha.Click += (_, _) => AbrirFichaMiembro();

            var btnPlantillas = new Button
            {
                Text = "Plantillas y automatizaciones",
                Location = new Point(89, 72),
                Size = new Size(220, 28),
                FlatStyle = FlatStyle.Flat
            };
            btnPlantillas.Click += (_, _) => AbrirPlantillas();

            panelComposer.Controls.Add(_lblVentana);
            panelComposer.Controls.Add(_chkModoPrueba);
            panelComposer.Controls.Add(btnFicha);
            panelComposer.Controls.Add(btnPlantillas);
            btnEnviar.Text = "Probar";

            panelComposer.Resize += (_, _) =>
            {
                if (_chkModoPrueba == null || _lblVentana == null)
                    return;

                _lblVentana.Width = Math.Max(200, _chkModoPrueba.Left - 20);
                txtMensaje.Width = Math.Max(120, panelComposer.ClientSize.Width - txtMensaje.Left - btnEnviar.Width - 24);
                btnEnviar.Left = panelComposer.ClientSize.Width - btnEnviar.Width - 12;
            };
        }

        private void AbrirPlantillas()
        {
            using var frm = new FrmPlantillasAutomatizaciones(_clienteSeleccionadoId);
            frm.ShowDialog(this);
            if (_clienteSeleccionadoId is int id)
                CargarMensajes(id);
        }

        private void AbrirFichaMiembro()
        {
            if (_clienteSeleccionadoId is not int clienteId)
            {
                MessageBox.Show(this, "Seleccione un miembro.", "Ficha", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string resumen;
            try
            {
                resumen = _chatBll.ObtenerResumenMiembro(clienteId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Ficha del miembro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            UI.DISEÑO.FrmPresentacion? presentacion = null;
            foreach (Form form in Application.OpenForms)
            {
                if (form is UI.DISEÑO.FrmPresentacion abierta)
                {
                    presentacion = abierta;
                    break;
                }
            }

            if (presentacion == null)
            {
                MessageBox.Show(this, resumen, "Ficha del miembro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var frm = new UI.DISEÑO.FrmClientes(presentacion, clienteId);
            frm.ShowDialog(this);
        }

        private async Task RefrescarEstadosEntregaAsync()
        {
            if (_clienteSeleccionadoId is not int clienteId || clienteId <= 0 || _enviando)
                return;

            int huella;
            try
            {
                huella = await Task.Run(() => _chatBll.ObtenerHuellaEstados(clienteId)).ConfigureAwait(true);
            }
            catch
            {
                return;
            }

            if (IsDisposed || _clienteSeleccionadoId != clienteId || huella == _ultimaHuellaEstados)
                return;

            _ultimaHuellaEstados = huella;
            await CargarMensajesAsync(clienteId);
        }

        private void ConfigurarPanelMensajes()
        {
            panelMensajes.BackColor = Color.FromArgb(233, 237, 234);
            ActivarDobleBuffer(panelMensajes);
            ActivarDobleBuffer(lstConversaciones);
            flowMensajes.Dock = DockStyle.None;
            flowMensajes.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            flowMensajes.WrapContents = false;
            flowMensajes.FlowDirection = FlowDirection.TopDown;
            flowMensajes.AutoSize = false;
            flowMensajes.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            AjustarAnchoFlowMensajes();
            panelMensajes.Resize += (_, _) => AjustarAnchoFlowMensajes();
        }

        private static void ActivarDobleBuffer(Control control)
        {
            typeof(Control).InvokeMember(
                "DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetProperty,
                null,
                control,
                new object[] { true });
        }

        private void CentrarEstadoMensajes()
        {
            if (_lblEstadoMensajes == null)
                return;

            int w = Math.Max(200, panelMensajes.ClientSize.Width - 32);
            int h = 80;
            _lblEstadoMensajes.Size = new Size(w, h);
            _lblEstadoMensajes.Location = new Point(
                (panelMensajes.ClientSize.Width - w) / 2,
                Math.Max(8, (panelMensajes.ClientSize.Height - h) / 2));
        }

        private void AjustarAnchoFlowMensajes()
        {
            int w = Math.Max(200, panelMensajes.ClientSize.Width - panelMensajes.Padding.Horizontal);
            if (flowMensajes.Width == w)
                return;

            flowMensajes.Width = w;
            foreach (BurbujaSlot slot in _burbujas)
            {
                slot.Contenedor.Width = w;
                slot.Caja.Location = slot.EsSalida
                    ? new Point(Math.Max(0, w - slot.Caja.Width - 8), 0)
                    : new Point(8, 0);
            }
        }

        private void AjustarAltoFlowMensajes(bool conservarScroll)
        {
            Point scroll = panelMensajes.AutoScrollPosition;
            int y = conservarScroll ? -scroll.Y : 0;
            int h = 0;
            foreach (Control c in flowMensajes.Controls)
                h += c.Height + c.Margin.Vertical;

            flowMensajes.Height = Math.Max(h, 1);
            panelMensajes.AutoScrollMinSize = new Size(0, flowMensajes.Height + panelMensajes.Padding.Vertical + 8);
            if (conservarScroll)
                panelMensajes.AutoScrollPosition = new Point(0, y);
        }

        private void ActualizarContadorNoLeidos()
        {
            try
            {
                MostrarContadorNoLeidos(_chatBll.ContarNoLeidosTotal());
            }
            catch
            {
                // el contador se reintenta en el próximo refresco
            }
        }

        private void MostrarContadorNoLeidos(int total)
        {
            lblNoLeidosTotal.Text = total > 0
                ? $"{total} mensaje(s) nuevo(s)"
                : "Sin mensajes nuevos";
            lblNoLeidosTotal.ForeColor = total > 0
                ? Color.FromArgb(220, 53, 69)
                : Color.FromArgb(0, 128, 105);
        }

        private void chkSoloNoLeidos_CheckedChanged(object sender, EventArgs e)
        {
            _ = CargarConversacionesAsync(mantenerSeleccion: true);
        }

        private void ActualizarEstadoTwilio()
        {
            string estado = _chatBll.ObtenerEstadoConexion();
            bool conectado = estado.StartsWith("Conectado", StringComparison.OrdinalIgnoreCase);
            bool webhookPendiente = estado.Contains("Webhook inbound", StringComparison.OrdinalIgnoreCase);

            if (conectado)
            {
                lblEstadoTwilio.Visible = false;
                lblEstadoTwilio.Text = estado;
                _toolTipEstado?.SetToolTip(lblEstadoTwilio, string.Empty);
                return;
            }

            lblEstadoTwilio.Visible = true;
            if (webhookPendiente)
            {
                lblEstadoTwilio.Text = estado + "  ·  clic aquí para activar";
                lblEstadoTwilio.BackColor = Color.FromArgb(180, 110, 0);
                lblEstadoTwilio.ForeColor = Color.White;
                _toolTipEstado?.SetToolTip(lblEstadoTwilio, TwilioSettings.ObtenerAyudaWebhookInbound());
            }
            else
            {
                lblEstadoTwilio.Text = estado;
                lblEstadoTwilio.BackColor = Color.FromArgb(160, 50, 50);
                lblEstadoTwilio.ForeColor = Color.White;
                _toolTipEstado?.SetToolTip(lblEstadoTwilio, estado);
            }
        }

        private async Task BootstrapWebhookSilenciosoAsync()
        {
            try
            {
                await Task.Run(() => WhatsAppStackBootstrapper.EnsureReady(tryLaunchNgrok: true));
                if (InvokeRequired)
                    BeginInvoke(ActualizarEstadoTwilio);
                else
                    ActualizarEstadoTwilio();
            }
            catch
            {
                // ignore
            }
        }

        private async Task ActivarWebhookInboundAsync()
        {
            lblEstadoTwilio.Text = "Activando recepción WhatsApp...";
            lblEstadoTwilio.Enabled = false;
            Cursor = Cursors.WaitCursor;
            try
            {
                WhatsAppStackBootstrapper.BootstrapResult r = await Task.Run(() =>
                    WhatsAppStackBootstrapper.EnsureReady(tryLaunchNgrok: true));

                ActualizarEstadoTwilio();

                if (r.WebhookConfigurado && !string.IsNullOrWhiteSpace(r.PublicUrl))
                {
                    string webhook = r.PublicUrl + TwilioSettings.WebhookInboundPath;
                    try
                    {
                        Clipboard.SetText(webhook);
                    }
                    catch
                    {
                        // ignore
                    }

                    MessageBox.Show(this,
                        $"Recepción activa.\r\n\r\n" +
                        $"URL pública:\r\n{r.PublicUrl}\r\n\r\n" +
                        $"Twilio webhook (POST) — copiada al portapapeles:\r\n{webhook}\r\n\r\n" +
                        "Twilio Console → Messaging → WhatsApp Sender →\r\n" +
                        "When a message comes in: pegue esa URL y guarde.\r\n\r\n" +
                        "El teléfono del miembro debe coincidir con el de Clientes.",
                        "WhatsApp", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(this,
                        r.Mensaje + "\r\n\r\n" + TwilioSettings.ObtenerAyudaWebhookInbound(),
                        "WhatsApp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "WhatsApp", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                lblEstadoTwilio.Enabled = true;
                Cursor = Cursors.Default;
                ActualizarEstadoTwilio();
            }
        }

        private async Task SincronizarNgrokSilenciosoAsync()
        {
            try
            {
                await Task.Run(() => WhatsAppStackBootstrapper.EnsureReady(tryLaunchNgrok: true));
                WhatsAppStackSecrets.InvalidateCache();
                if (InvokeRequired)
                    BeginInvoke(ActualizarEstadoTwilio);
                else
                    ActualizarEstadoTwilio();
            }
            catch
            {
                // ignore
            }
        }

        private async Task CargarConversacionesAsync(bool mantenerSeleccion)
        {
            int token = Interlocked.Increment(ref _cargaListaToken);
            string filtro = txtBuscar.Text.Trim();
            bool soloNoLeidos = chkSoloNoLeidos.Checked;

            List<ChatConversacionDto> lista;
            try
            {
                lista = await Task.Run(() => _chatBll.ListarConversaciones(
                    string.IsNullOrWhiteSpace(filtro) ? null : filtro,
                    soloNoLeidos).ToList()).ConfigureAwait(true);
            }
            catch
            {
                return;
            }

            if (token != _cargaListaToken || IsDisposed)
                return;

            int? previo = mantenerSeleccion ? _clienteSeleccionadoId : null;
            _conversaciones = lista;
            _suprimirSeleccion = true;
            try
            {
                lstConversaciones.BeginUpdate();
                lstConversaciones.Items.Clear();
                foreach (var conv in _conversaciones)
                    lstConversaciones.Items.Add(conv);
                lstConversaciones.EndUpdate();

                if (previo.HasValue)
                    SeleccionarClienteSilencioso(previo.Value);
            }
            finally
            {
                _suprimirSeleccion = false;
            }
        }

        private void SeleccionarCliente(int clienteId)
        {
            SeleccionarClienteSilencioso(clienteId);
        }

        private void SeleccionarClienteSilencioso(int clienteId)
        {
            for (int i = 0; i < _conversaciones.Count; i++)
            {
                if (_conversaciones[i].ClienteId != clienteId)
                    continue;

                lstConversaciones.SelectedIndex = i;
                return;
            }
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            _timerBuscar?.Stop();
            _timerBuscar?.Start();
        }

        private void lstConversaciones_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_suprimirSeleccion)
                return;

            if (lstConversaciones.SelectedItem is not ChatConversacionDto conv)
            {
                _clienteSeleccionadoId = null;
                _cargaMensajesCts?.Cancel();
                lblChatHeader.Text = "Seleccione un miembro";
                btnEnviar.Enabled = false;
                txtMensaje.Enabled = false;
                btnPdf.Enabled = false;
                btnUltimaFactura.Enabled = false;
                LimpiarAdjuntoPdf();
                LiberarBurbujas();
                if (_lblEstadoMensajes != null)
                    _lblEstadoMensajes.Visible = true;
                return;
            }

            AplicarSeleccionConversacion(conv);
        }

        private void AplicarSeleccionConversacion(ChatConversacionDto conv)
        {
            _clienteSeleccionadoId = conv.ClienteId;
            string telefono = MensajeAutomaticoBLL.NormalizarTelefono(conv.Telefono);
            lblChatHeader.Text = conv.TelefonoValido
                ? $"{conv.Nombre}  ·  {telefono}"
                : $"{conv.Nombre}  ·  (sin teléfono válido)";

            bool puedeEnviar = conv.TelefonoValido;
            btnEnviar.Enabled = puedeEnviar && !_enviando;
            txtMensaje.Enabled = puedeEnviar;
            btnPdf.Enabled = puedeEnviar && !_enviando;
            btnUltimaFactura.Enabled = puedeEnviar && !_enviando;

            if (!puedeEnviar && _lblVentana != null)
                _lblVentana.Text = "Teléfono inválido — actualice el número en Clientes.";
            else if (puedeEnviar)
                ActualizarEstadoTwilio();

            _ = CargarMensajesAsync(conv.ClienteId);
            _ = CompletarSeleccionAsync(conv.ClienteId, puedeEnviar);
        }

        private async Task CompletarSeleccionAsync(int clienteId, bool consultarVentana)
        {
            try
            {
                var info = await Task.Run(() =>
                {
                    _chatBll.MarcarConversacionLeida(clienteId);
                    WhatsAppVentanaInfo? ventana = consultarVentana
                        ? _chatBll.EvaluarVentana(clienteId)
                        : null;
                    int noLeidos = _chatBll.ContarNoLeidosTotal();
                    int maxEntrada = _chatBll.ObtenerMaxIdMensajeEntrada();
                    return (ventana, noLeidos, maxEntrada);
                }).ConfigureAwait(true);

                if (IsDisposed || _clienteSeleccionadoId != clienteId)
                    return;

                if (info.ventana != null && _lblVentana != null)
                {
                    _lblVentana.Text = info.ventana.Resumen;
                    _lblVentana.ForeColor = info.ventana.Abierta
                        ? Color.FromArgb(0, 110, 80)
                        : Color.FromArgb(150, 70, 0);
                }

                MostrarContadorNoLeidos(info.noLeidos);
                _ultimoNoLeidosConocido = info.noLeidos;
                _ultimoMaxIdEntradaConocido = info.maxEntrada;
                MarcarConversacionLeidaEnLista(clienteId);
            }
            catch
            {
                if (!IsDisposed && _lblVentana != null && consultarVentana)
                    _lblVentana.Text = "No se pudo consultar la ventana de atención.";
            }
        }

        private void MarcarConversacionLeidaEnLista(int clienteId)
        {
            for (int i = 0; i < _conversaciones.Count; i++)
            {
                ChatConversacionDto c = _conversaciones[i];
                if (c.ClienteId != clienteId || c.NoLeidos <= 0)
                    continue;

                var actualizada = ClonarConversacion(c, noLeidos: 0);
                _conversaciones[i] = actualizada;
                if (i < lstConversaciones.Items.Count)
                    lstConversaciones.Items[i] = actualizada;
                lstConversaciones.Invalidate();
                break;
            }
        }

        private static ChatConversacionDto ClonarConversacion(ChatConversacionDto c, int? noLeidos = null) =>
            new()
            {
                ClienteId = c.ClienteId,
                Nombre = c.Nombre,
                Telefono = c.Telefono,
                UltimoMensaje = c.UltimoMensaje,
                UltimaFecha = c.UltimaFecha,
                NoLeidos = noLeidos ?? c.NoLeidos,
                TelefonoValido = c.TelefonoValido
            };

        private async Task CargarMensajesAsync(int clienteId)
        {
            _cargaMensajesCts?.Cancel();
            _cargaMensajesCts?.Dispose();
            _cargaMensajesCts = new CancellationTokenSource();
            CancellationToken ct = _cargaMensajesCts.Token;
            int token = Interlocked.Increment(ref _cargaMensajesToken);

            List<ChatMensajeDto> mensajes;
            try
            {
                mensajes = await Task.Run(
                    () => _chatBll.ListarMensajes(clienteId).ToList(),
                    ct).ConfigureAwait(true);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception)
            {
                if (token != _cargaMensajesToken || IsDisposed || _clienteSeleccionadoId != clienteId)
                    return;

                if (_burbujas.Count == 0 && _lblEstadoMensajes != null)
                {
                    _lblEstadoMensajes.Visible = true;
                    _lblEstadoMensajes.Text = "La conversación tardó demasiado.\r\nVuelva a seleccionar al miembro.";
                }

                return;
            }

            if (token != _cargaMensajesToken || IsDisposed || _clienteSeleccionadoId != clienteId)
                return;

            _ultimoConteoMensajes = mensajes.Count;
            _ultimoMaxIdMensajes = mensajes.Count > 0 ? mensajes.Max(m => m.Id) : 0;
            int anchoMax = Math.Max(280, panelMensajes.ClientSize.Width - panelMensajes.Padding.Horizontal);

            if (_lblEstadoMensajes != null)
            {
                _lblEstadoMensajes.Visible = mensajes.Count == 0;
                _lblEstadoMensajes.Text = mensajes.Count == 0
                    ? (TwilioSettings.WebhookInboundConfigurado
                        ? "Sin mensajes aún con este miembro.\r\nEscriba abajo o espere mensajes entrantes."
                        : "Sin mensajes. Active el webhook (barra naranja) para recibir WhatsApp.")
                    : string.Empty;
            }

            bool irAlFinal = PintarMensajes(clienteId, mensajes, anchoMax);
            if (_lblEstadoMensajes != null)
                _lblEstadoMensajes.Visible = mensajes.Count == 0;

            if (irAlFinal)
                BeginInvoke(ScrollAlFinal);

            try
            {
                _ultimaHuellaEstados = await Task.Run(() => _chatBll.ObtenerHuellaEstados(clienteId)).ConfigureAwait(true);
            }
            catch
            {
                // la huella se reintenta en el timer
            }
        }

        /// <summary>
        /// True si hay mensajes nuevos y conviene bajar al final.
        /// Si solo cambió el estado de entrega, actualiza esas burbujas y no mueve el scroll.
        /// </summary>
        private bool PintarMensajes(int clienteId, IReadOnlyList<ChatMensajeDto> mensajes, int anchoMax)
        {
            bool mismoHilo = _burbujasClienteId == clienteId && _burbujas.Count > 0 && _burbujas.Count <= mensajes.Count;
            if (mismoHilo)
            {
                for (int i = 0; i < _burbujas.Count; i++)
                {
                    if (_burbujas[i].Id != mensajes[i].Id)
                    {
                        mismoHilo = false;
                        break;
                    }
                }
            }

            if (mismoHilo)
            {
                bool cambioEstado = false;
                bool agrego = _burbujas.Count < mensajes.Count;
                flowMensajes.SuspendLayout();
                try
                {
                    for (int i = 0; i < _burbujas.Count; i++)
                    {
                        string firma = FirmaMensaje(mensajes[i]);
                        if (_burbujas[i].Firma == firma)
                            continue;

                        AplicarBurbuja(_burbujas[i], mensajes[i], anchoMax, firma);
                        cambioEstado = true;
                    }

                    for (int i = _burbujas.Count; i < mensajes.Count; i++)
                        AgregarBurbuja(mensajes[i], anchoMax);
                }
                finally
                {
                    flowMensajes.ResumeLayout(false);
                }

                if (!cambioEstado && !agrego)
                    return false;

                AjustarAltoFlowMensajes(conservarScroll: !agrego);
                return agrego;
            }

            flowMensajes.SuspendLayout();
            panelMensajes.SuspendLayout();
            try
            {
                LiberarBurbujas();
                _burbujasClienteId = clienteId;
                foreach (ChatMensajeDto msg in mensajes)
                    AgregarBurbuja(msg, anchoMax);
            }
            finally
            {
                flowMensajes.ResumeLayout(false);
                panelMensajes.ResumeLayout(false);
            }

            AjustarAnchoFlowMensajes();
            AjustarAltoFlowMensajes(conservarScroll: false);
            return mensajes.Count > 0;
        }

        private void AgregarBurbuja(ChatMensajeDto msg, int anchoMax)
        {
            var slot = new BurbujaSlot();
            slot.Caja = new Panel();
            slot.Cuerpo = new Label
            {
                AutoSize = false,
                Font = _fuenteBurbuja,
                ForeColor = Color.Black
            };
            slot.Meta = new Label
            {
                AutoSize = false,
                Font = _fuenteBurbujaMeta,
                ForeColor = Color.Gray
            };
            slot.Caja.Controls.Add(slot.Cuerpo);
            slot.Caja.Controls.Add(slot.Meta);
            slot.Contenedor = new Panel
            {
                Margin = new Padding(0, 2, 0, 2)
            };
            slot.Contenedor.Controls.Add(slot.Caja);
            AplicarBurbuja(slot, msg, anchoMax, FirmaMensaje(msg));
            flowMensajes.Controls.Add(slot.Contenedor);
            _burbujas.Add(slot);
        }

        private void AplicarBurbuja(BurbujaSlot slot, ChatMensajeDto msg, int anchoMax, string firma)
        {
            slot.Id = msg.Id;
            slot.Firma = firma;
            bool esSalida = !string.Equals(msg.Direccion, "ENTRADA", StringComparison.OrdinalIgnoreCase);
            slot.EsSalida = esSalida;

            int maxAnchoBurbuja = (int)(anchoMax * 0.72);
            int innerW = Math.Max(40, maxAnchoBurbuja - 24);
            Color fondo = esSalida ? Color.FromArgb(220, 248, 198) : Color.White;
            if (msg.EsAutomatico || string.Equals(msg.Origen, "AUTO", StringComparison.OrdinalIgnoreCase))
                fondo = Color.FromArgb(255, 249, 220);
            else if (string.Equals(msg.Origen, "PLANTILLA", StringComparison.OrdinalIgnoreCase))
                fondo = Color.FromArgb(220, 235, 255);
            else if (string.Equals(msg.Estado, "SIMULADO", StringComparison.OrdinalIgnoreCase))
                fondo = Color.FromArgb(230, 230, 230);

            bool fallido = string.Equals(msg.Estado, "FALLIDO", StringComparison.OrdinalIgnoreCase)
                           || string.Equals(msg.Estado, "ERROR", StringComparison.OrdinalIgnoreCase);
            if (fallido)
                fondo = Color.FromArgb(255, 214, 214);

            string cuerpo = FormatearCuerpoBurbuja(msg);
            string meta = msg.Fecha.ToString("dd/MM HH:mm");
            if (!string.IsNullOrWhiteSpace(msg.Estado))
                meta += " · " + WhatsAppEstadoChat.Etiqueta(msg.Estado);
            if (esSalida && !string.IsNullOrWhiteSpace(msg.Usuario))
                meta += $" · {msg.Usuario}";
            if (fallido && !string.IsNullOrWhiteSpace(msg.DetalleError))
            {
                string error = msg.DetalleError.Trim();
                if (error.Length > 140)
                    error = error[..140];
                meta += " · " + error;
            }

            Size szCuerpo = TextRenderer.MeasureText(cuerpo, _fuenteBurbuja, new Size(innerW, int.MaxValue),
                TextFormatFlags.WordBreak);
            Size szMeta = TextRenderer.MeasureText(meta, _fuenteBurbujaMeta, new Size(innerW, int.MaxValue),
                TextFormatFlags.WordBreak);
            int burbujaH = 16 + szCuerpo.Height + 4 + szMeta.Height + 10;

            slot.Caja.BackColor = fondo;
            slot.Caja.Width = maxAnchoBurbuja;
            slot.Caja.Height = burbujaH;
            slot.Cuerpo.Location = new Point(10, 8);
            slot.Cuerpo.Size = szCuerpo;
            slot.Cuerpo.Text = cuerpo;
            slot.Meta.Location = new Point(10, 8 + szCuerpo.Height + 4);
            slot.Meta.Size = szMeta;
            slot.Meta.Text = meta;
            slot.Contenedor.Width = anchoMax;
            slot.Contenedor.Height = burbujaH + 8;
            slot.Caja.Location = esSalida
                ? new Point(Math.Max(0, anchoMax - slot.Caja.Width - 8), 0)
                : new Point(8, 0);
        }

        private static string FirmaMensaje(ChatMensajeDto msg) =>
            string.Concat(
                msg.Direccion, "\u001f",
                msg.Estado, "\u001f",
                msg.DetalleError, "\u001f",
                msg.Cuerpo, "\u001f",
                msg.Origen, "\u001f",
                msg.Usuario, "\u001f",
                msg.EsAutomatico ? "1" : "0", "\u001f",
                msg.TipoAutomatico);

        private void LiberarBurbujas()
        {
            foreach (BurbujaSlot slot in _burbujas)
                slot.Contenedor.Dispose();

            _burbujas.Clear();
            _burbujasClienteId = 0;
            if (flowMensajes.Controls.Count > 0)
                flowMensajes.Controls.Clear();
        }

        private sealed class BurbujaSlot
        {
            public long Id;
            public string Firma = string.Empty;
            public bool EsSalida;
            public Panel Contenedor = null!;
            public Panel Caja = null!;
            public Label Cuerpo = null!;
            public Label Meta = null!;
        }

        private void CargarMensajes(int clienteId) => _ = CargarMensajesAsync(clienteId);

        private static string FormatearCuerpoBurbuja(ChatMensajeDto msg)
        {
            var sb = new StringBuilder();
            if (msg.EsAutomatico || string.Equals(msg.Origen, "AUTO", StringComparison.OrdinalIgnoreCase))
            {
                string tipo = string.IsNullOrWhiteSpace(msg.TipoAutomatico) ? "" : ": " + msg.TipoAutomatico;
                sb.AppendLine("[Automático" + tipo + "]");
            }
            else if (string.Equals(msg.Origen, "PLANTILLA", StringComparison.OrdinalIgnoreCase))
                sb.AppendLine("[Plantilla aprobada]");
            else if (string.Equals(msg.Origen, "SESION", StringComparison.OrdinalIgnoreCase)
                     || string.Equals(msg.Origen, "MANUAL", StringComparison.OrdinalIgnoreCase))
                sb.AppendLine("[Manual]");
            else if (string.Equals(msg.Estado, "SIMULADO", StringComparison.OrdinalIgnoreCase))
                sb.AppendLine("[Prueba]");

            if (string.IsNullOrWhiteSpace(msg.Cuerpo))
                return sb.Length > 0 ? sb.ToString().TrimEnd() : "(sin texto)";

            foreach (string linea in msg.Cuerpo.Split('\n'))
            {
                string texto = linea.Trim();
                if (texto.Length == 0)
                    continue;

                if (Uri.TryCreate(texto, UriKind.Absolute, out Uri? uri)
                    && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                {
                    sb.AppendLine(texto.StartsWith("[Adjunto", StringComparison.OrdinalIgnoreCase)
                        || texto.StartsWith("[PDF", StringComparison.OrdinalIgnoreCase)
                        ? "📎 Adjunto"
                        : "🔗 " + texto);
                }
                else
                {
                    sb.AppendLine(texto);
                }
            }

            return sb.ToString().TrimEnd();
        }

        private void ScrollAlFinal()
        {
            try
            {
                AjustarAltoFlowMensajes(conservarScroll: false);
                int max = panelMensajes.VerticalScroll.Maximum;
                panelMensajes.AutoScrollPosition = new Point(0, max);
            }
            catch
            {
                // scroll opcional
            }
        }

        private void btnPdf_Click(object sender, EventArgs e)
        {
            if (_chkModoPrueba?.Checked != false)
            {
                MessageBox.Show(this,
                    "El modo prueba no adjunta PDF. Desactive el modo prueba para enviar un archivo real.",
                    "WhatsApp", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!_clienteSeleccionadoId.HasValue)
                return;

            using var dialog = new OpenFileDialog
            {
                Title = "Seleccionar PDF para WhatsApp",
                Filter = "PDF (*.pdf)|*.pdf",
                Multiselect = false
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            _rutaPdfPendiente = dialog.FileName;
            ActualizarAdjuntoUi();
        }

        private void ActualizarAdjuntoUi()
        {
            if (string.IsNullOrWhiteSpace(_rutaPdfPendiente))
            {
                lblAdjunto.Visible = false;
                lblAdjunto.Text = string.Empty;
                btnPdf.Text = "PDF";
                return;
            }

            lblAdjunto.Visible = true;
            lblAdjunto.Text = "📎 " + Path.GetFileName(_rutaPdfPendiente) + "  (clic PDF para cambiar)";
            btnPdf.Text = "PDF ✓";
        }

        private void LimpiarAdjuntoPdf()
        {
            _rutaPdfPendiente = null;
            ActualizarAdjuntoUi();
        }

        private void btnUltimaFactura_Click(object sender, EventArgs e) => _ = EnviarUltimaFacturaMiembroAsync();

        private async Task EnviarUltimaFacturaMiembroAsync()
        {
            if (_chkModoPrueba?.Checked != false)
            {
                MessageBox.Show(this,
                    "El modo prueba no envía la factura. Desactive el modo prueba para el envío real.",
                    "WhatsApp", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!_clienteSeleccionadoId.HasValue || _enviando)
                return;

            int clienteId = _clienteSeleccionadoId.Value;
            string caption = txtMensaje.Text.Trim();
            MarcarEnvioEnCurso(true, "Buscando la última factura…");

            try
            {
                ChatEnvioResult resultado = await Task.Run(() =>
                    _chatBll.EnviarUltimaFacturaMiembro(
                        clienteId,
                        string.IsNullOrWhiteSpace(caption) ? null : caption)).ConfigureAwait(true);

                if (IsDisposed)
                    return;

                if (resultado.Exito)
                {
                    if (_lblVentana != null)
                        _lblVentana.Text = resultado.Detalle;
                    txtMensaje.Clear();
                    LimpiarAdjuntoPdf();
                    await CargarMensajesAsync(clienteId);
                    _ = CargarConversacionesAsync(mantenerSeleccion: true);
                    SincronizarHuellaChat();
                }
                else
                {
                    MessageBox.Show(this, resultado.Detalle, "WhatsApp",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    CargarMensajes(clienteId);
                }
            }
            catch (Exception ex)
            {
                if (!IsDisposed)
                    MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (!IsDisposed)
                    MarcarEnvioEnCurso(false, null);
            }
        }

        private void MarcarEnvioEnCurso(bool enCurso, string? aviso)
        {
            _enviando = enCurso;
            UseWaitCursor = enCurso;
            btnEnviar.Enabled = !enCurso && _clienteSeleccionadoId.HasValue;
            btnPdf.Enabled = !enCurso && _clienteSeleccionadoId.HasValue;
            btnUltimaFactura.Enabled = !enCurso && _clienteSeleccionadoId.HasValue;
            txtMensaje.Enabled = !enCurso && _clienteSeleccionadoId.HasValue;
            if (_lblVentana != null && !string.IsNullOrWhiteSpace(aviso))
                _lblVentana.Text = aviso;
            if (!enCurso)
                txtMensaje.Focus();
        }

        private void btnEnviar_Click(object sender, EventArgs e) => _ = EnviarMensajeActualAsync();

        private void txtMensaje_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                _ = EnviarMensajeActualAsync();
            }
        }

        private async Task EnviarMensajeActualAsync()
        {
            if (!_clienteSeleccionadoId.HasValue || _enviando)
                return;

            int clienteId = _clienteSeleccionadoId.Value;
            string texto = txtMensaje.Text.Trim();
            string? rutaPdf = _rutaPdfPendiente;
            bool tienePdf = !string.IsNullOrWhiteSpace(rutaPdf);
            bool simular = _chkModoPrueba?.Checked != false;

            if (!tienePdf && string.IsNullOrWhiteSpace(texto))
                return;

            if (tienePdf && simular)
            {
                MessageBox.Show(this,
                    "El modo prueba no envía PDF. Quite el adjunto o desactive el modo prueba.",
                    "WhatsApp", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (tienePdf && string.IsNullOrWhiteSpace(texto) && !ConfirmarEnvioPdfSinTexto())
                return;

            MarcarEnvioEnCurso(true, tienePdf ? "Enviando PDF…" : "Enviando…");

            try
            {
                ChatEnvioResult resultado = await Task.Run(() => tienePdf
                    ? _chatBll.EnviarPdfManual(clienteId, rutaPdf!, texto)
                    : _chatBll.EnviarMensajeManual(clienteId, texto, simular, usarPlantillaAprobada: false))
                    .ConfigureAwait(true);

                if (IsDisposed)
                    return;

                if (!tienePdf && resultado.RequierePlantilla)
                {
                    DialogResult usar = MessageBox.Show(this,
                        resultado.Detalle + "\r\n\r\n¿Enviar este texto dentro de la plantilla aprobada de Twilio?",
                        "Ventana de WhatsApp cerrada",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);
                    if (usar == DialogResult.Yes)
                    {
                        MarcarEnvioEnCurso(true, "Enviando con plantilla…");
                        resultado = await Task.Run(() => _chatBll.EnviarMensajeManual(
                            clienteId,
                            texto,
                            simular,
                            usarPlantillaAprobada: true)).ConfigureAwait(true);
                    }
                }

                if (IsDisposed)
                    return;

                if (resultado.Exito)
                {
                    if (resultado.Simulado)
                    {
                        if (_lblVentana != null)
                            _lblVentana.Text = "Prueba hecha. No se llamó a Twilio y no se guardó en el historial.";
                        MessageBox.Show(this, resultado.Detalle, "Modo prueba",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        if (_lblVentana != null)
                            _lblVentana.Text = resultado.Detalle;
                        txtMensaje.Clear();
                        LimpiarAdjuntoPdf();
                        await CargarMensajesAsync(clienteId);
                        _ = CargarConversacionesAsync(mantenerSeleccion: true);
                        SincronizarHuellaChat();
                    }
                }
                else if (!resultado.RequierePlantilla)
                {
                    MessageBox.Show(this, resultado.Detalle, "WhatsApp",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    CargarMensajes(clienteId);
                }
            }
            catch (Exception ex)
            {
                if (!IsDisposed)
                    MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (!IsDisposed)
                    MarcarEnvioEnCurso(false, null);
            }
        }

        private bool ConfirmarEnvioPdfSinTexto()
        {
            return MessageBox.Show(this,
                "¿Enviar el PDF sin mensaje de texto?",
                "WhatsApp", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        private void timerRefresh_Tick(object sender, EventArgs e)
        {
            // Desactivado a propósito: la UI se actualiza por ChatNotificationHost,
            // envío local, búsqueda y Activated (solo si hay cambio).
        }

        private void lstConversaciones_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index < 0 || e.Index >= lstConversaciones.Items.Count)
                return;

            if (lstConversaciones.Items[e.Index] is not ChatConversacionDto conv)
                return;

            bool seleccionado = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            bool sinLeer = conv.NoLeidos > 0;
            Color fondo = seleccionado
                ? Color.FromArgb(0, 128, 105)
                : sinLeer ? Color.FromArgb(232, 245, 233) : Color.White;
            Color texto = seleccionado ? Color.White : Color.Black;
            Color secundario = seleccionado ? Color.FromArgb(220, 255, 245) : Color.Gray;

            using var brushFondo = new SolidBrush(fondo);
            e.Graphics.FillRectangle(brushFondo, e.Bounds);

            var bounds = e.Bounds;
            bounds.Inflate(-8, -4);

            using var brushTexto = new SolidBrush(texto);
            using var brushSec = new SolidBrush(secundario);

            string nombre = conv.Nombre;
            if (!conv.TelefonoValido)
                nombre += " ⚠";

            e.Graphics.DrawString(nombre, _fuenteListaNombre, brushTexto, bounds.Left, bounds.Top);

            string preview = conv.UltimoMensaje ?? conv.Telefono;
            if (preview.Length > 42)
                preview = preview[..39] + "...";

            e.Graphics.DrawString(preview, _fuenteListaDetalle, brushSec, bounds.Left, bounds.Top + 22);

            if (conv.NoLeidos > 0)
            {
                string badge = conv.NoLeidos > 9 ? "9+" : conv.NoLeidos.ToString();
                var badgeRect = new Rectangle(bounds.Right - 28, bounds.Top + 4, 24, 20);
                using var brushBadge = new SolidBrush(Color.FromArgb(37, 211, 102));
                e.Graphics.FillEllipse(brushBadge, badgeRect);
                using var brushBadgeText = new SolidBrush(Color.White);
                e.Graphics.DrawString(badge, _fuenteListaDetalle, brushBadgeText, badgeRect.X + 4, badgeRect.Y + 1);
            }
            else if (conv.UltimaFecha.HasValue)
            {
                string hora = conv.UltimaFecha.Value.ToString("dd/MM HH:mm");
                var size = e.Graphics.MeasureString(hora, _fuenteListaDetalle);
                e.Graphics.DrawString(hora, _fuenteListaDetalle, brushSec,
                    bounds.Right - size.Width, bounds.Top + 2);
            }
        }
    }
}
