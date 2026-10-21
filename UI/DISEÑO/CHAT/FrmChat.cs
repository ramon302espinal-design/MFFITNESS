using BLL;
using BLL.Models;
using CORE;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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
            CargarConversaciones(mantenerSeleccion: false);
            _ = BootstrapWebhookSilenciosoAsync();

            _timerBuscar = new System.Windows.Forms.Timer { Interval = 350 };
            _timerBuscar.Tick += (_, _) =>
            {
                _timerBuscar!.Stop();
                CargarConversaciones(mantenerSeleccion: true);
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
            _timerEstadoWebhook.Tick += (_, _) =>
            {
                ActualizarEstadoTwilio();
                RefrescarEstadosEntrega();
                if (!TwilioSettings.WebhookInboundConfigurado || string.IsNullOrWhiteSpace(WhatsAppStackBootstrapper.TryGetNgrokPublicUrl()))
                    _ = SincronizarNgrokSilenciosoAsync();
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
            _toolTipEstado?.Dispose();
        }

        /// <summary>Navega a un miembro (desde Estado Clientes u otro módulo).</summary>
        public void NavegarACliente(int clienteId)
        {
            if (IsDisposed || clienteId <= 0)
                return;

            _clienteInicialId = clienteId;
            CargarConversaciones(mantenerSeleccion: false);
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
            RefrescarVista(forzar: false);
        }

        /// <summary>
        /// Recarga lista/mensajes. Con forzar=false solo si cambió huella (no leídos / max entrada / hilo abierto).
        /// </summary>
        private void RefrescarVista(bool forzar = false)
        {
            int noLeidos;
            int maxEntrada;
            try
            {
                noLeidos = _chatBll.ContarNoLeidosTotal();
                maxEntrada = _chatBll.ObtenerMaxIdMensajeEntrada();
            }
            catch
            {
                return;
            }

            bool hayCambio = forzar
                || noLeidos != _ultimoNoLeidosConocido
                || maxEntrada != _ultimoMaxIdEntradaConocido;

            if (_clienteSeleccionadoId is int cid && cid > 0)
            {
                try
                {
                    int maxId = _chatBll.ObtenerMaxIdMensajeChat(cid);
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
            ActualizarContadorNoLeidos();
            CargarConversaciones(mantenerSeleccion: true);
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
                RefrescarVista(forzar: true);

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

        private void RefrescarEstadosEntrega()
        {
            if (_clienteSeleccionadoId is not int clienteId || clienteId <= 0 || _enviando)
                return;

            int huella;
            try
            {
                huella = _chatBll.ObtenerHuellaEstados(clienteId);
            }
            catch
            {
                return;
            }

            if (huella == _ultimaHuellaEstados)
                return;

            _ultimaHuellaEstados = huella;
            CargarMensajes(clienteId);
        }

        private void ActualizarVentana(int clienteId)
        {
            try
            {
                var info = _chatBll.EvaluarVentana(clienteId);
                if (_lblVentana != null)
                {
                    _lblVentana.Text = info.Resumen;
                    _lblVentana.ForeColor = info.Abierta
                        ? Color.FromArgb(0, 110, 80)
                        : Color.FromArgb(150, 70, 0);
                }
            }
            catch
            {
                if (_lblVentana != null)
                    _lblVentana.Text = "No se pudo consultar la ventana de atención.";
            }
        }

        private void ConfigurarPanelMensajes()
        {
            panelMensajes.BackColor = Color.FromArgb(233, 237, 234);
            flowMensajes.Dock = DockStyle.None;
            flowMensajes.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            flowMensajes.WrapContents = false;
            flowMensajes.FlowDirection = FlowDirection.TopDown;
            flowMensajes.AutoSize = true;
            flowMensajes.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            AjustarAnchoFlowMensajes();
            panelMensajes.Resize += (_, _) => AjustarAnchoFlowMensajes();
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
            int w = panelMensajes.ClientSize.Width - panelMensajes.Padding.Horizontal;
            flowMensajes.Width = Math.Max(200, w);
            foreach (Control c in flowMensajes.Controls)
                c.Width = flowMensajes.Width;
        }

        private void AjustarAltoFlowMensajes()
        {
            int h = 0;
            foreach (Control c in flowMensajes.Controls)
                h += c.Height + c.Margin.Vertical;

            flowMensajes.Height = Math.Max(h, 1);
            panelMensajes.AutoScrollMinSize = new Size(0, flowMensajes.Height + panelMensajes.Padding.Vertical + 8);
        }

        private void ActualizarContadorNoLeidos()
        {
            int total = _chatBll.ContarNoLeidosTotal();
            lblNoLeidosTotal.Text = total > 0
                ? $"{total} mensaje(s) nuevo(s)"
                : "Sin mensajes nuevos";
            lblNoLeidosTotal.ForeColor = total > 0
                ? Color.FromArgb(220, 53, 69)
                : Color.FromArgb(0, 128, 105);
        }

        private void chkSoloNoLeidos_CheckedChanged(object sender, EventArgs e)
        {
            CargarConversaciones(mantenerSeleccion: true);
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

        private void CargarConversaciones(bool mantenerSeleccion)
        {
            int? previo = mantenerSeleccion ? _clienteSeleccionadoId : null;
            string filtro = txtBuscar.Text.Trim();
            _conversaciones = _chatBll.ListarConversaciones(
                string.IsNullOrWhiteSpace(filtro) ? null : filtro,
                chkSoloNoLeidos.Checked).ToList();

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
                flowMensajes.Controls.Clear();
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
            ActualizarVentana(conv.ClienteId);

            if (!puedeEnviar && _lblVentana != null)
                _lblVentana.Text = "Teléfono inválido — actualice el número en Clientes.";
            else if (puedeEnviar)
                ActualizarEstadoTwilio();

            _chatBll.MarcarConversacionLeida(conv.ClienteId);
            MarcarConversacionLeidaEnLista(conv.ClienteId);
            ActualizarContadorNoLeidos();
            SincronizarHuellaChat();
            _ = CargarMensajesAsync(conv.ClienteId);
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

            flowMensajes.SuspendLayout();
            panelMensajes.SuspendLayout();
            try
            {
                flowMensajes.Controls.Clear();
                foreach (ChatMensajeDto msg in mensajes)
                {
                    if (ct.IsCancellationRequested)
                        return;

                    bool esSalida = !string.Equals(msg.Direccion, "ENTRADA", StringComparison.OrdinalIgnoreCase);
                    flowMensajes.Controls.Add(CrearBurbuja(msg, esSalida, anchoMax));
                }
            }
            finally
            {
                flowMensajes.ResumeLayout(true);
                panelMensajes.ResumeLayout(true);
            }

            AjustarAnchoFlowMensajes();
            AjustarAltoFlowMensajes();
            if (_lblEstadoMensajes != null)
                _lblEstadoMensajes.Visible = mensajes.Count == 0;

            BeginInvoke(ScrollAlFinal);
            try
            {
                _ultimaHuellaEstados = _chatBll.ObtenerHuellaEstados(clienteId);
            }
            catch
            {
                // la huella se reintenta en el timer
            }
        }

        private void CargarMensajes(int clienteId) => _ = CargarMensajesAsync(clienteId);

        private Panel CrearBurbuja(ChatMensajeDto msg, bool esSalida, int anchoMax)
        {
            int maxAnchoBurbuja = (int)(anchoMax * 0.72);
            int innerW = maxAnchoBurbuja - 24;

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

            var fontCuerpo = new Font("Segoe UI", 10F);
            var fontMeta = new Font("Segoe UI", 7.5F);
            Size szCuerpo = TextRenderer.MeasureText(cuerpo, fontCuerpo, new Size(innerW, int.MaxValue),
                TextFormatFlags.WordBreak);
            Size szMeta = TextRenderer.MeasureText(meta, fontMeta, new Size(innerW, int.MaxValue),
                TextFormatFlags.WordBreak);

            int burbujaH = 16 + szCuerpo.Height + 4 + szMeta.Height + 10;

            var burbuja = new Panel
            {
                Width = maxAnchoBurbuja,
                Height = burbujaH,
                BackColor = fondo,
                Padding = new Padding(10, 8, 10, 8)
            };

            var lblCuerpo = new Label
            {
                AutoSize = false,
                Font = fontCuerpo,
                ForeColor = Color.Black,
                Location = new Point(10, 8),
                Size = szCuerpo,
                Text = cuerpo
            };
            var lblMeta = new Label
            {
                AutoSize = false,
                Font = fontMeta,
                ForeColor = Color.Gray,
                Location = new Point(10, 8 + szCuerpo.Height + 4),
                Size = szMeta,
                Text = meta
            };

            burbuja.Controls.Add(lblCuerpo);
            burbuja.Controls.Add(lblMeta);

            var contenedor = new Panel
            {
                Width = anchoMax,
                Height = burbujaH + 8,
                Padding = new Padding(0, 4, 0, 4),
                Margin = new Padding(0, 2, 0, 2)
            };

            burbuja.Location = esSalida
                ? new Point(anchoMax - burbuja.Width - 8, 0)
                : new Point(8, 0);

            contenedor.Controls.Add(burbuja);
            return contenedor;
        }

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
                AjustarAltoFlowMensajes();
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
                    CargarMensajes(clienteId);
                    CargarConversaciones(mantenerSeleccion: true);
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
                        CargarMensajes(clienteId);
                        CargarConversaciones(mantenerSeleccion: true);
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

            using var fontNombre = new Font("Segoe UI", 10F, FontStyle.Bold);
            using var fontPreview = new Font("Segoe UI", 9F);
            using var brushTexto = new SolidBrush(texto);
            using var brushSec = new SolidBrush(secundario);

            string nombre = conv.Nombre;
            if (!conv.TelefonoValido)
                nombre += " ⚠";

            e.Graphics.DrawString(nombre, fontNombre, brushTexto, bounds.Left, bounds.Top);

            string preview = conv.UltimoMensaje ?? conv.Telefono;
            if (preview.Length > 42)
                preview = preview[..39] + "...";

            e.Graphics.DrawString(preview, fontPreview, brushSec, bounds.Left, bounds.Top + 22);

            if (conv.NoLeidos > 0)
            {
                string badge = conv.NoLeidos > 9 ? "9+" : conv.NoLeidos.ToString();
                var badgeRect = new Rectangle(bounds.Right - 28, bounds.Top + 4, 24, 20);
                using var brushBadge = new SolidBrush(Color.FromArgb(37, 211, 102));
                e.Graphics.FillEllipse(brushBadge, badgeRect);
                using var brushBadgeText = new SolidBrush(Color.White);
                e.Graphics.DrawString(badge, fontPreview, brushBadgeText, badgeRect.X + 4, badgeRect.Y + 1);
            }
            else if (conv.UltimaFecha.HasValue)
            {
                string hora = conv.UltimaFecha.Value.ToString("dd/MM HH:mm");
                var size = e.Graphics.MeasureString(hora, fontPreview);
                e.Graphics.DrawString(hora, fontPreview, brushSec,
                    bounds.Right - size.Width, bounds.Top + 2);
            }
        }
    }
}
