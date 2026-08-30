namespace UI.DISEÑO.CHAT
{
    partial class FrmChat
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            panelNav = new Panel();
            btnNavClientes = new Button();
            btnNavReportes = new Button();
            btnNavInventario = new Button();
            btnNavHistorial = new Button();
            btnNavCaja = new Button();
            btnNavEstado = new Button();
            btnNavDeudas = new Button();
            btnNavPagar = new Button();
            btnBack = new Button();
            splitMain = new SplitContainer();
            panelLista = new Panel();
            lstConversaciones = new ListBox();
            txtBuscar = new TextBox();
            chkSoloNoLeidos = new CheckBox();
            lblNoLeidosTotal = new Label();
            lblListaTitulo = new Label();
            panelChat = new Panel();
            panelMensajes = new Panel();
            flowMensajes = new FlowLayoutPanel();
            panelComposer = new Panel();
            txtMensaje = new TextBox();
            lblAdjunto = new Label();
            btnUltimaFactura = new Button();
            btnPdf = new Button();
            btnEnviar = new Button();
            lblChatHeader = new Label();
            lblEstadoTwilio = new Label();
            timerRefresh = new System.Windows.Forms.Timer(components);
            panelNav.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            panelLista.SuspendLayout();
            panelChat.SuspendLayout();
            panelMensajes.SuspendLayout();
            panelComposer.SuspendLayout();
            SuspendLayout();
            // 
            // panelNav
            // 
            panelNav.BackColor = Color.White;
            panelNav.Controls.Add(btnNavClientes);
            panelNav.Controls.Add(btnNavReportes);
            panelNav.Controls.Add(btnNavInventario);
            panelNav.Controls.Add(btnNavHistorial);
            panelNav.Controls.Add(btnNavCaja);
            panelNav.Controls.Add(btnNavEstado);
            panelNav.Controls.Add(btnNavDeudas);
            panelNav.Controls.Add(btnNavPagar);
            panelNav.Controls.Add(btnBack);
            panelNav.Dock = DockStyle.Top;
            panelNav.Location = new Point(0, 0);
            panelNav.Name = "panelNav";
            panelNav.Size = new Size(1100, 52);
            panelNav.TabIndex = 0;
            panelNav.Tag = "classic";
            // 
            // btnNavClientes
            // 
            btnNavClientes.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavClientes.Location = new Point(940, 10);
            btnNavClientes.Name = "btnNavClientes";
            btnNavClientes.Size = new Size(120, 32);
            btnNavClientes.TabIndex = 8;
            btnNavClientes.Tag = "classic";
            btnNavClientes.Text = "CLIENTES";
            btnNavClientes.UseVisualStyleBackColor = true;
            // 
            // btnNavReportes
            // 
            btnNavReportes.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavReportes.Location = new Point(810, 10);
            btnNavReportes.Name = "btnNavReportes";
            btnNavReportes.Size = new Size(120, 32);
            btnNavReportes.TabIndex = 7;
            btnNavReportes.Tag = "classic";
            btnNavReportes.Text = "REPORTES";
            btnNavReportes.UseVisualStyleBackColor = true;
            // 
            // btnNavInventario
            // 
            btnNavInventario.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavInventario.Location = new Point(670, 10);
            btnNavInventario.Name = "btnNavInventario";
            btnNavInventario.Size = new Size(130, 32);
            btnNavInventario.TabIndex = 6;
            btnNavInventario.Tag = "classic";
            btnNavInventario.Text = "INVENTARIO";
            btnNavInventario.UseVisualStyleBackColor = true;
            // 
            // btnNavHistorial
            // 
            btnNavHistorial.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavHistorial.Location = new Point(540, 10);
            btnNavHistorial.Name = "btnNavHistorial";
            btnNavHistorial.Size = new Size(120, 32);
            btnNavHistorial.TabIndex = 5;
            btnNavHistorial.Tag = "classic";
            btnNavHistorial.Text = "HISTORIAL";
            btnNavHistorial.UseVisualStyleBackColor = true;
            // 
            // btnNavCaja
            // 
            btnNavCaja.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavCaja.Location = new Point(420, 10);
            btnNavCaja.Name = "btnNavCaja";
            btnNavCaja.Size = new Size(110, 32);
            btnNavCaja.TabIndex = 4;
            btnNavCaja.Tag = "classic";
            btnNavCaja.Text = "CAJA";
            btnNavCaja.UseVisualStyleBackColor = true;
            // 
            // btnNavEstado
            // 
            btnNavEstado.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavEstado.Location = new Point(300, 10);
            btnNavEstado.Name = "btnNavEstado";
            btnNavEstado.Size = new Size(110, 32);
            btnNavEstado.TabIndex = 3;
            btnNavEstado.Tag = "classic";
            btnNavEstado.Text = "ESTADO";
            btnNavEstado.UseVisualStyleBackColor = true;
            // 
            // btnNavDeudas
            // 
            btnNavDeudas.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavDeudas.Location = new Point(180, 10);
            btnNavDeudas.Name = "btnNavDeudas";
            btnNavDeudas.Size = new Size(110, 32);
            btnNavDeudas.TabIndex = 2;
            btnNavDeudas.Tag = "classic";
            btnNavDeudas.Text = "DEUDAS";
            btnNavDeudas.UseVisualStyleBackColor = true;
            // 
            // btnNavPagar
            // 
            btnNavPagar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavPagar.Location = new Point(60, 10);
            btnNavPagar.Name = "btnNavPagar";
            btnNavPagar.Size = new Size(110, 32);
            btnNavPagar.TabIndex = 1;
            btnNavPagar.Tag = "classic";
            btnNavPagar.Text = "COBRAR";
            btnNavPagar.UseVisualStyleBackColor = true;
            // 
            // btnBack
            // 
            btnBack.Font = new Font("Segoe UI", 11F);
            btnBack.Location = new Point(8, 8);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(43, 35);
            btnBack.TabIndex = 0;
            btnBack.Tag = "classic";
            btnBack.UseVisualStyleBackColor = true;
            // 
            // splitMain
            // 
            splitMain.Dock = DockStyle.Fill;
            splitMain.Location = new Point(0, 52);
            splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            splitMain.Panel1.Controls.Add(panelLista);
            splitMain.Panel1MinSize = 260;
            // 
            // splitMain.Panel2
            // 
            splitMain.Panel2.Controls.Add(panelChat);
            splitMain.Size = new Size(1100, 598);
            splitMain.SplitterDistance = 320;
            splitMain.TabIndex = 1;
            // 
            // panelLista
            // 
            panelLista.BackColor = Color.FromArgb(240, 242, 245);
            panelLista.Controls.Add(lstConversaciones);
            panelLista.Controls.Add(txtBuscar);
            panelLista.Controls.Add(chkSoloNoLeidos);
            panelLista.Controls.Add(lblNoLeidosTotal);
            panelLista.Controls.Add(lblListaTitulo);
            panelLista.Dock = DockStyle.Fill;
            panelLista.Location = new Point(0, 0);
            panelLista.Name = "panelLista";
            panelLista.Padding = new Padding(8);
            panelLista.Size = new Size(320, 598);
            panelLista.TabIndex = 0;
            // 
            // lstConversaciones
            // 
            lstConversaciones.BorderStyle = BorderStyle.None;
            lstConversaciones.Dock = DockStyle.Fill;
            lstConversaciones.DrawMode = DrawMode.OwnerDrawFixed;
            lstConversaciones.Font = new Font("Segoe UI", 10F);
            lstConversaciones.FormattingEnabled = true;
            lstConversaciones.IntegralHeight = false;
            lstConversaciones.ItemHeight = 23;
            lstConversaciones.Location = new Point(8, 122);
            lstConversaciones.Name = "lstConversaciones";
            lstConversaciones.Size = new Size(304, 468);
            lstConversaciones.TabIndex = 2;
            lstConversaciones.DrawItem += lstConversaciones_DrawItem;
            lstConversaciones.SelectedIndexChanged += lstConversaciones_SelectedIndexChanged;
            // 
            // txtBuscar
            // 
            txtBuscar.Dock = DockStyle.Top;
            txtBuscar.Font = new Font("Segoe UI", 10F);
            txtBuscar.Location = new Point(8, 92);
            txtBuscar.Name = "txtBuscar";
            txtBuscar.PlaceholderText = "Buscar miembro...";
            txtBuscar.Size = new Size(304, 30);
            txtBuscar.TabIndex = 1;
            txtBuscar.TextChanged += txtBuscar_TextChanged;
            // 
            // chkSoloNoLeidos
            // 
            chkSoloNoLeidos.AutoSize = true;
            chkSoloNoLeidos.Dock = DockStyle.Top;
            chkSoloNoLeidos.Font = new Font("Segoe UI", 9F);
            chkSoloNoLeidos.Location = new Point(8, 64);
            chkSoloNoLeidos.Name = "chkSoloNoLeidos";
            chkSoloNoLeidos.Padding = new Padding(0, 0, 0, 4);
            chkSoloNoLeidos.Size = new Size(304, 28);
            chkSoloNoLeidos.TabIndex = 4;
            chkSoloNoLeidos.Text = "Solo no leídos";
            chkSoloNoLeidos.UseVisualStyleBackColor = true;
            chkSoloNoLeidos.CheckedChanged += chkSoloNoLeidos_CheckedChanged;
            // 
            // lblNoLeidosTotal
            // 
            lblNoLeidosTotal.Dock = DockStyle.Top;
            lblNoLeidosTotal.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblNoLeidosTotal.ForeColor = Color.FromArgb(0, 128, 105);
            lblNoLeidosTotal.Location = new Point(8, 42);
            lblNoLeidosTotal.Name = "lblNoLeidosTotal";
            lblNoLeidosTotal.Padding = new Padding(0, 0, 0, 4);
            lblNoLeidosTotal.Size = new Size(304, 22);
            lblNoLeidosTotal.TabIndex = 3;
            lblNoLeidosTotal.Text = "Sin mensajes nuevos";
            // 
            // lblListaTitulo
            // 
            lblListaTitulo.Dock = DockStyle.Top;
            lblListaTitulo.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblListaTitulo.Location = new Point(8, 8);
            lblListaTitulo.Name = "lblListaTitulo";
            lblListaTitulo.Padding = new Padding(0, 0, 0, 6);
            lblListaTitulo.Size = new Size(304, 34);
            lblListaTitulo.TabIndex = 0;
            lblListaTitulo.Text = "Conversaciones";
            lblListaTitulo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panelChat
            // 
            panelChat.BackColor = Color.FromArgb(233, 237, 234);
            panelChat.Controls.Add(panelMensajes);
            panelChat.Controls.Add(panelComposer);
            panelChat.Controls.Add(lblChatHeader);
            panelChat.Controls.Add(lblEstadoTwilio);
            panelChat.Dock = DockStyle.Fill;
            panelChat.Location = new Point(0, 0);
            panelChat.Name = "panelChat";
            panelChat.Size = new Size(776, 598);
            panelChat.TabIndex = 0;
            // 
            // panelMensajes
            // 
            panelMensajes.AutoScroll = true;
            panelMensajes.Controls.Add(flowMensajes);
            panelMensajes.Dock = DockStyle.Fill;
            panelMensajes.Location = new Point(0, 70);
            panelMensajes.Name = "panelMensajes";
            panelMensajes.Padding = new Padding(8);
            panelMensajes.Size = new Size(776, 432);
            panelMensajes.TabIndex = 1;
            // 
            // flowMensajes
            // 
            flowMensajes.AutoSize = true;
            flowMensajes.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowMensajes.Dock = DockStyle.Top;
            flowMensajes.FlowDirection = FlowDirection.TopDown;
            flowMensajes.Location = new Point(8, 8);
            flowMensajes.Name = "flowMensajes";
            flowMensajes.Size = new Size(760, 0);
            flowMensajes.TabIndex = 0;
            flowMensajes.WrapContents = false;
            // 
            // panelComposer
            // 
            panelComposer.BackColor = Color.FromArgb(240, 242, 245);
            panelComposer.Controls.Add(txtMensaje);
            panelComposer.Controls.Add(lblAdjunto);
            panelComposer.Controls.Add(btnUltimaFactura);
            panelComposer.Controls.Add(btnPdf);
            panelComposer.Controls.Add(btnEnviar);
            panelComposer.Dock = DockStyle.Bottom;
            panelComposer.Location = new Point(0, 502);
            panelComposer.Name = "panelComposer";
            panelComposer.Padding = new Padding(8);
            panelComposer.Size = new Size(776, 96);
            panelComposer.TabIndex = 2;
            // 
            // txtMensaje
            // 
            txtMensaje.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtMensaje.Enabled = false;
            txtMensaje.Font = new Font("Segoe UI", 10F);
            txtMensaje.Location = new Point(197, 14);
            txtMensaje.Multiline = true;
            txtMensaje.Name = "txtMensaje";
            txtMensaje.PlaceholderText = "Escriba un mensaje o adjunte un PDF...";
            txtMensaje.ScrollBars = ScrollBars.Vertical;
            txtMensaje.Size = new Size(445, 52);
            txtMensaje.TabIndex = 0;
            txtMensaje.KeyDown += txtMensaje_KeyDown;
            // 
            // lblAdjunto
            // 
            lblAdjunto.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblAdjunto.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            lblAdjunto.ForeColor = Color.FromArgb(0, 100, 80);
            lblAdjunto.Location = new Point(197, 14);
            lblAdjunto.Name = "lblAdjunto";
            lblAdjunto.Size = new Size(445, 18);
            lblAdjunto.TabIndex = 3;
            lblAdjunto.Visible = false;
            // 
            // btnUltimaFactura
            // 
            btnUltimaFactura.BackColor = Color.FromArgb(240, 242, 245);
            btnUltimaFactura.Enabled = false;
            btnUltimaFactura.FlatStyle = FlatStyle.Flat;
            btnUltimaFactura.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            btnUltimaFactura.Location = new Point(89, 14);
            btnUltimaFactura.Name = "btnUltimaFactura";
            btnUltimaFactura.Size = new Size(98, 52);
            btnUltimaFactura.TabIndex = 4;
            btnUltimaFactura.Tag = "classic";
            btnUltimaFactura.Text = "ÚLT.\nFACT.";
            btnUltimaFactura.UseVisualStyleBackColor = true;
            btnUltimaFactura.Click += btnUltimaFactura_Click;
            // 
            // btnPdf
            // 
            btnPdf.BackColor = Color.FromArgb(240, 242, 245);
            btnPdf.Enabled = false;
            btnPdf.FlatStyle = FlatStyle.Flat;
            btnPdf.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnPdf.Location = new Point(11, 14);
            btnPdf.Name = "btnPdf";
            btnPdf.Size = new Size(72, 52);
            btnPdf.TabIndex = 2;
            btnPdf.Tag = "classic";
            btnPdf.Text = "PDF";
            btnPdf.UseVisualStyleBackColor = true;
            btnPdf.Click += btnPdf_Click;
            // 
            // btnEnviar
            // 
            btnEnviar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnEnviar.BackColor = Color.FromArgb(0, 128, 105);
            btnEnviar.Enabled = false;
            btnEnviar.FlatStyle = FlatStyle.Flat;
            btnEnviar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnEnviar.ForeColor = Color.White;
            btnEnviar.Location = new Point(648, 14);
            btnEnviar.Name = "btnEnviar";
            btnEnviar.Size = new Size(112, 52);
            btnEnviar.TabIndex = 1;
            btnEnviar.Tag = "classic";
            btnEnviar.Text = "Enviar";
            btnEnviar.UseVisualStyleBackColor = false;
            btnEnviar.Click += btnEnviar_Click;
            // 
            // lblChatHeader
            // 
            lblChatHeader.BackColor = Color.FromArgb(0, 128, 105);
            lblChatHeader.Dock = DockStyle.Top;
            lblChatHeader.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblChatHeader.ForeColor = Color.White;
            lblChatHeader.Location = new Point(0, 22);
            lblChatHeader.Name = "lblChatHeader";
            lblChatHeader.Padding = new Padding(12, 0, 0, 0);
            lblChatHeader.Size = new Size(776, 48);
            lblChatHeader.TabIndex = 0;
            lblChatHeader.Text = "Seleccione un miembro";
            lblChatHeader.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblEstadoTwilio
            // 
            lblEstadoTwilio.BackColor = Color.FromArgb(0, 128, 105);
            lblEstadoTwilio.Dock = DockStyle.Top;
            lblEstadoTwilio.Font = new Font("Segoe UI", 8F);
            lblEstadoTwilio.ForeColor = Color.White;
            lblEstadoTwilio.Location = new Point(0, 0);
            lblEstadoTwilio.Name = "lblEstadoTwilio";
            lblEstadoTwilio.Padding = new Padding(12, 0, 12, 0);
            lblEstadoTwilio.Size = new Size(776, 22);
            lblEstadoTwilio.TabIndex = 3;
            lblEstadoTwilio.Text = "Estado Twilio";
            lblEstadoTwilio.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // timerRefresh
            // 
            timerRefresh.Interval = 2500;
            timerRefresh.Tick += timerRefresh_Tick;
            // 
            // FrmChat
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1100, 650);
            Controls.Add(splitMain);
            Controls.Add(panelNav);
            MinimumSize = new Size(900, 550);
            Name = "FrmChat";
            StartPosition = FormStartPosition.CenterScreen;
            Tag = "classic";
            Text = "WhatsApp — Chat";
            Activated += FrmChat_Activated;
            FormClosed += FrmChat_FormClosed;
            Load += FrmChat_Load;
            panelNav.ResumeLayout(false);
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            panelLista.ResumeLayout(false);
            panelLista.PerformLayout();
            panelChat.ResumeLayout(false);
            panelMensajes.ResumeLayout(false);
            panelMensajes.PerformLayout();
            panelComposer.ResumeLayout(false);
            panelComposer.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelNav;
        private Button btnNavClientes;
        private Button btnNavReportes;
        private Button btnNavInventario;
        private Button btnNavHistorial;
        private Button btnNavCaja;
        private Button btnNavEstado;
        private Button btnNavDeudas;
        private Button btnNavPagar;
        private Button btnBack;
        private SplitContainer splitMain;
        private Panel panelLista;
        private Label lblListaTitulo;
        private Label lblNoLeidosTotal;
        private CheckBox chkSoloNoLeidos;
        private TextBox txtBuscar;
        private ListBox lstConversaciones;
        private Panel panelChat;
        private Label lblChatHeader;
        private Panel panelMensajes;
        private FlowLayoutPanel flowMensajes;
        private Panel panelComposer;
        private Label lblAdjunto;
        private Button btnPdf;
        private Button btnUltimaFactura;
        private Button btnEnviar;
        private TextBox txtMensaje;
        private Label lblEstadoTwilio;
        private System.Windows.Forms.Timer timerRefresh;
    }
}
