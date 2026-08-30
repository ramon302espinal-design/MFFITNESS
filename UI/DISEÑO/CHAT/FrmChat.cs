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
        private string? _rutaPdfPendiente;
        private bool _suprimirSeleccion;
        private int _cargaMensajesToken;
        private CancellationTokenSource? _cargaMensajesCts;
        private System.Windows.Forms.Timer? _timerBuscar;
        private System.Windows.Forms.Timer? _timerEstadoWebhook;
        private ToolTip? _toolTipEstado;
        private Label? _lblEstadoMensajes;

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
            ConfigurarModoSoloRecepcion();
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
                if (!TwilioSettings.WebhookInboundConfigurado || string.IsNullOrWhiteSpace(WhatsAppStackBootstrapper.TryGetNgrokPublicUrl()))
                    _ = SincronizarNgrokSilenciosoAsync();
            };
            _timerEstadoWebhook.Start();

            ActualizarContadorNoLeidos();

            if (_clienteInicialId is > 0)
                SeleccionarCliente(_clienteInicialId.Value);

            _ultimoNoLeidosConocido = _chatBll.ContarNoLeidosTotal();
            timerRefresh.Start();
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

            RefrescarVista();
        }

        private void RefrescarVista()
        {
            _ultimoNoLeidosConocido = _chatBll.ContarNoLeidosTotal();

            ActualizarContadorNoLeidos();
            CargarConversaciones(mantenerSeleccion: true);

            if (_clienteSeleccionadoId is int cid && cid > 0)
            {
                int maxId = _chatBll.ObtenerMaxIdMensajeChat(cid);
                if (maxId != _ultimoMaxIdMensajes)
                    _ = CargarMensajesAsync(cid);
            }
        }

        private void OnMensajeEntranteGlobal(int clienteId, int mensajeId)
        {
            if (IsDisposed || !IsHandleCreated)
                return;

            void aplicar()
            {
                ActualizarContadorNoLeidos();
                CargarConversaciones(mantenerSeleccion: true);

                if (_clienteSeleccionadoId == clienteId)
                    _ = CargarMensajesAsync(clienteId);
            }

            if (InvokeRequired)
                BeginInvoke(aplicar);
            else
                aplicar();
        }

        private void ConfigurarModoSoloRecepcion()
        {
            if (TwilioSettings.ChatEnvioManualHabilitado)
                return;

            txtMensaje.Visible = false;
            btnEnviar.Visible = false;
            btnPdf.Visible = false;
            btnUltimaFactura.Visible = false;
            lblAdjunto.Visible = false;

            var lblSoloRecepcion = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(100, 100, 100),
                Text = "Solo recepción de WhatsApp — envío deshabilitado temporalmente",
                TextAlign = ContentAlignment.MiddleCenter
            };
            panelComposer.Controls.Add(lblSoloRecepcion);
            lblSoloRecepcion.BringToFront();
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

            bool puedeEnviar = conv.TelefonoValido && TwilioSettings.ChatEnvioManualHabilitado;
            btnEnviar.Enabled = puedeEnviar && !_enviando;
            txtMensaje.Enabled = puedeEnviar;
            btnPdf.Enabled = puedeEnviar && !_enviando;
            btnUltimaFactura.Enabled = puedeEnviar && !_enviando;

            if (!puedeEnviar)
                lblEstadoTwilio.Text = "Teléfono inválido — actualice en Clientes";
            else
                ActualizarEstadoTwilio();

            _chatBll.MarcarConversacionLeida(conv.ClienteId);
            MarcarConversacionLeidaEnLista(conv.ClienteId);
            ActualizarContadorNoLeidos();
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
        }

        private void CargarMensajes(int clienteId) => _ = CargarMensajesAsync(clienteId);

        private Panel CrearBurbuja(ChatMensajeDto msg, bool esSalida, int anchoMax)
        {
            int maxAnchoBurbuja = (int)(anchoMax * 0.72);
            int innerW = maxAnchoBurbuja - 24;

            Color fondo = esSalida ? Color.FromArgb(220, 248, 198) : Color.White;
            if (msg.EsAutomatico)
                fondo = Color.FromArgb(255, 249, 220);

            string cuerpo = FormatearCuerpoBurbuja(msg);
            string meta = msg.Fecha.ToString("dd/MM HH:mm");
            if (esSalida && !string.IsNullOrWhiteSpace(msg.Usuario))
                meta += $" · {msg.Usuario}";
            if (!string.Equals(msg.Estado, "ENVIADO", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(msg.Estado, "RECIBIDO", StringComparison.OrdinalIgnoreCase))
                meta += $" · {msg.Estado}";

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
            if (msg.EsAutomatico)
            {
                string tipo = string.IsNullOrWhiteSpace(msg.TipoAutomatico) ? "" : ": " + msg.TipoAutomatico;
                sb.AppendLine("[Auto" + tipo + "]");
            }

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
            if (!TwilioSettings.ChatEnvioManualHabilitado)
                return;
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

        private void btnUltimaFactura_Click(object sender, EventArgs e) => EnviarUltimaFacturaMiembro();

        private void EnviarUltimaFacturaMiembro()
        {
            if (!TwilioSettings.ChatEnvioManualHabilitado)
                return;
            if (!_clienteSeleccionadoId.HasValue || _enviando)
                return;

            _enviando = true;
            btnEnviar.Enabled = false;
            btnPdf.Enabled = false;
            btnUltimaFactura.Enabled = false;
            txtMensaje.Enabled = false;
            Cursor = Cursors.WaitCursor;

            try
            {
                string caption = txtMensaje.Text.Trim();
                ChatEnvioResult resultado = _chatBll.EnviarUltimaFacturaMiembro(
                    _clienteSeleccionadoId.Value,
                    string.IsNullOrWhiteSpace(caption) ? null : caption);

                if (resultado.Exito)
                {
                    txtMensaje.Clear();
                    LimpiarAdjuntoPdf();
                    CargarMensajes(_clienteSeleccionadoId.Value);
                    CargarConversaciones(mantenerSeleccion: true);
                }
                else
                {
                    MessageBox.Show(this, resultado.Detalle, "WhatsApp",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    CargarMensajes(_clienteSeleccionadoId.Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _enviando = false;
                Cursor = Cursors.Default;
                txtMensaje.Enabled = true;
                btnPdf.Enabled = true;
                btnUltimaFactura.Enabled = true;
                btnEnviar.Enabled = true;
                txtMensaje.Focus();
            }
        }

        private void btnEnviar_Click(object sender, EventArgs e) => EnviarMensajeActual();

        private void txtMensaje_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                EnviarMensajeActual();
            }
        }

        private void EnviarMensajeActual()
        {
            if (!TwilioSettings.ChatEnvioManualHabilitado)
                return;
            if (!_clienteSeleccionadoId.HasValue || _enviando)
                return;

            string texto = txtMensaje.Text.Trim();
            bool tienePdf = !string.IsNullOrWhiteSpace(_rutaPdfPendiente);

            if (!tienePdf && string.IsNullOrWhiteSpace(texto))
                return;

            if (tienePdf && string.IsNullOrWhiteSpace(texto) && !ConfirmarEnvioPdfSinTexto())
                return;

            _enviando = true;
            btnEnviar.Enabled = false;
            btnPdf.Enabled = false;
            btnUltimaFactura.Enabled = false;
            txtMensaje.Enabled = false;
            Cursor = Cursors.WaitCursor;

            try
            {
                ChatEnvioResult resultado = tienePdf
                    ? _chatBll.EnviarPdfManual(_clienteSeleccionadoId.Value, _rutaPdfPendiente!, texto)
                    : _chatBll.EnviarMensajeManual(_clienteSeleccionadoId.Value, texto);

                if (resultado.Exito)
                {
                    txtMensaje.Clear();
                    LimpiarAdjuntoPdf();
                    CargarMensajes(_clienteSeleccionadoId.Value);
                    CargarConversaciones(mantenerSeleccion: true);
                }
                else
                {
                    MessageBox.Show(this, resultado.Detalle, "WhatsApp",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    CargarMensajes(_clienteSeleccionadoId.Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _enviando = false;
                Cursor = Cursors.Default;
                txtMensaje.Enabled = true;
                btnPdf.Enabled = true;
                btnUltimaFactura.Enabled = true;
                btnEnviar.Enabled = true;
                txtMensaje.Focus();
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
            RefrescarVista();
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
