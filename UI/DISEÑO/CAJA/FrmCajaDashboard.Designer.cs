namespace UI.DISEÑO
{
    partial class FrmCajaDashboard
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
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
            panelTop = new Panel();
            lblEstadoCaja = new Label();
            lblTitulo = new Label();
            panelMontoInicial = new Panel();
            lblMontoInicial = new Label();
            label1 = new Label();
            panelIndicadores = new Panel();
            lblAyudaCaja = new Label();
            panel1 = new Panel();
            lblIngresosHoy = new Label();
            panelIngresos = new Label();
            panelGastos = new Panel();
            lblGastosHoy = new Label();
            label2 = new Label();
            panelBalance = new Panel();
            lblBalance = new Label();
            label3 = new Label();
            panelBotones = new Panel();
            btnCierresCaja = new Button();
            btnRegistrarGasto = new Button();
            btnRegistrarIngreso = new Button();
            btnMovimientos = new Button();
            btnCerrarCaja = new Button();
            btnAbrirCaja = new Button();
            panelNav.SuspendLayout();
            panelTop.SuspendLayout();
            panelMontoInicial.SuspendLayout();
            panelIndicadores.SuspendLayout();
            panel1.SuspendLayout();
            panelGastos.SuspendLayout();
            panelBalance.SuspendLayout();
            panelBotones.SuspendLayout();
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
            panelNav.Size = new Size(1062, 52);
            panelNav.TabIndex = 0;
            panelNav.Tag = "classic";
            // 
            // btnNavClientes
            // 
            btnNavClientes.FlatStyle = FlatStyle.Standard;
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
            btnNavReportes.FlatStyle = FlatStyle.Standard;
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
            btnNavInventario.FlatStyle = FlatStyle.Standard;
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
            btnNavHistorial.FlatStyle = FlatStyle.Standard;
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
            btnNavCaja.FlatStyle = FlatStyle.Standard;
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
            btnNavEstado.FlatStyle = FlatStyle.Standard;
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
            btnNavDeudas.FlatStyle = FlatStyle.Standard;
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
            btnNavPagar.FlatStyle = FlatStyle.Standard;
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
            btnBack.FlatStyle = FlatStyle.Standard;
            btnBack.Font = new Font("Segoe UI", 11F);
            btnBack.Location = new Point(8, 8);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(43, 35);
            btnBack.TabIndex = 0;
            btnBack.Tag = "classic";
            btnBack.UseVisualStyleBackColor = true;
            // 
            // panelTop
            // 
            panelTop.BackColor = Color.FromArgb(30, 30, 30, 30);
            panelTop.Controls.Add(lblEstadoCaja);
            panelTop.Controls.Add(lblTitulo);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(0, 52);
            panelTop.Name = "panelTop";
            panelTop.Size = new Size(1062, 65);
            panelTop.TabIndex = 1;
            // 
            // lblEstadoCaja
            // 
            lblEstadoCaja.AutoSize = true;
            lblEstadoCaja.BackColor = Color.Transparent;
            lblEstadoCaja.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblEstadoCaja.ForeColor = SystemColors.ActiveCaptionText;
            lblEstadoCaja.Location = new Point(750, 9);
            lblEstadoCaja.Name = "lblEstadoCaja";
            lblEstadoCaja.Size = new Size(0, 41);
            lblEstadoCaja.TabIndex = 1;
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.BackColor = Color.Transparent;
            lblTitulo.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitulo.ForeColor = SystemColors.ActiveCaptionText;
            lblTitulo.Location = new Point(652, 9);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(92, 41);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "CAJA";
            // 
            // panelMontoInicial
            // 
            panelMontoInicial.BackColor = Color.Silver;
            panelMontoInicial.Controls.Add(lblMontoInicial);
            panelMontoInicial.Controls.Add(label1);
            panelMontoInicial.Location = new Point(115, 17);
            panelMontoInicial.Name = "panelMontoInicial";
            panelMontoInicial.Size = new Size(298, 120);
            panelMontoInicial.TabIndex = 1;
            // 
            // lblMontoInicial
            // 
            lblMontoInicial.AutoSize = true;
            lblMontoInicial.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            lblMontoInicial.Location = new Point(16, 47);
            lblMontoInicial.Name = "lblMontoInicial";
            lblMontoInicial.Size = new Size(50, 60);
            lblMontoInicial.TabIndex = 1;
            lblMontoInicial.Text = "0";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label1.Location = new Point(75, 9);
            label1.Name = "label1";
            label1.Size = new Size(139, 23);
            label1.TabIndex = 0;
            label1.Text = "MONTO INICIAL";
            // 
            // panelIndicadores
            // 
            panelIndicadores.BackColor = Color.White;
            panelIndicadores.Controls.Add(lblAyudaCaja);
            panelIndicadores.Controls.Add(panel1);
            panelIndicadores.Controls.Add(panelGastos);
            panelIndicadores.Controls.Add(panelBalance);
            panelIndicadores.Controls.Add(panelMontoInicial);
            panelIndicadores.Dock = DockStyle.Top;
            panelIndicadores.Location = new Point(0, 117);
            panelIndicadores.Name = "panelIndicadores";
            panelIndicadores.Size = new Size(1062, 175);
            panelIndicadores.TabIndex = 1;
            // 
            // lblAyudaCaja
            // 
            lblAyudaCaja.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblAyudaCaja.Font = new Font("Segoe UI", 8F);
            lblAyudaCaja.ForeColor = Color.FromArgb(64, 64, 64);
            lblAyudaCaja.Location = new Point(12, 148);
            lblAyudaCaja.Name = "lblAyudaCaja";
            lblAyudaCaja.Size = new Size(1038, 20);
            lblAyudaCaja.TabIndex = 3;
            lblAyudaCaja.Text = "Ingresos hoy = cobros netos del día. Balance = turno actual.";
            // 
            // panel1
            // 
            panel1.BackColor = Color.Silver;
            panel1.Controls.Add(lblIngresosHoy);
            panel1.Controls.Add(panelIngresos);
            panel1.Location = new Point(446, 17);
            panel1.Name = "panel1";
            panel1.Size = new Size(298, 120);
            panel1.TabIndex = 2;
            // 
            // lblIngresosHoy
            // 
            lblIngresosHoy.AutoSize = true;
            lblIngresosHoy.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            lblIngresosHoy.Location = new Point(16, 47);
            lblIngresosHoy.Name = "lblIngresosHoy";
            lblIngresosHoy.Size = new Size(50, 60);
            lblIngresosHoy.TabIndex = 2;
            lblIngresosHoy.Text = "0";
            // 
            // panelIngresos
            // 
            panelIngresos.AutoSize = true;
            panelIngresos.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, 1, true);
            panelIngresos.Location = new Point(75, 9);
            panelIngresos.Name = "panelIngresos";
            panelIngresos.Size = new Size(134, 23);
            panelIngresos.TabIndex = 0;
            panelIngresos.Text = "INGRESOS HOY";
            // 
            // panelGastos
            // 
            panelGastos.BackColor = Color.Silver;
            panelGastos.Controls.Add(lblGastosHoy);
            panelGastos.Controls.Add(label2);
            panelGastos.Location = new Point(771, 17);
            panelGastos.Name = "panelGastos";
            panelGastos.Size = new Size(298, 120);
            panelGastos.TabIndex = 2;
            // 
            // lblGastosHoy
            // 
            lblGastosHoy.AutoSize = true;
            lblGastosHoy.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            lblGastosHoy.Location = new Point(16, 47);
            lblGastosHoy.Name = "lblGastosHoy";
            lblGastosHoy.Size = new Size(50, 60);
            lblGastosHoy.TabIndex = 3;
            lblGastosHoy.Text = "0";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, 1, true);
            label2.Location = new Point(86, 8);
            label2.Name = "label2";
            label2.Size = new Size(117, 23);
            label2.TabIndex = 3;
            label2.Text = "GASTOS TURNO";
            // 
            // panelBalance
            // 
            panelBalance.BackColor = Color.Silver;
            panelBalance.Controls.Add(lblBalance);
            panelBalance.Controls.Add(label3);
            panelBalance.Location = new Point(1095, 17);
            panelBalance.Name = "panelBalance";
            panelBalance.Size = new Size(298, 120);
            panelBalance.TabIndex = 2;
            // 
            // lblBalance
            // 
            lblBalance.AutoSize = true;
            lblBalance.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            lblBalance.Location = new Point(16, 47);
            lblBalance.Name = "lblBalance";
            lblBalance.Size = new Size(50, 60);
            lblBalance.TabIndex = 4;
            lblBalance.Text = "0";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, 1, true);
            label3.Location = new Point(108, 9);
            label3.Name = "label3";
            label3.Size = new Size(87, 23);
            label3.TabIndex = 4;
            label3.Text = "BALANCE TURNO";
            // 
            // panelBotones
            // 
            panelBotones.BackColor = SystemColors.Control;
            panelBotones.Controls.Add(btnCierresCaja);
            panelBotones.Controls.Add(btnRegistrarGasto);
            panelBotones.Controls.Add(btnRegistrarIngreso);
            panelBotones.Controls.Add(btnMovimientos);
            panelBotones.Controls.Add(btnCerrarCaja);
            panelBotones.Controls.Add(btnAbrirCaja);
            panelBotones.Dock = DockStyle.Fill;
            panelBotones.ForeColor = SystemColors.ControlText;
            panelBotones.Location = new Point(0, 267);
            panelBotones.Name = "panelBotones";
            panelBotones.Size = new Size(1062, 406);
            panelBotones.TabIndex = 2;
            panelBotones.Tag = "classic";
            // 
            // btnCierresCaja
            // 
            btnCierresCaja.FlatStyle = FlatStyle.Standard;
            btnCierresCaja.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnCierresCaja.Location = new Point(8, 164);
            btnCierresCaja.Name = "btnCierresCaja";
            btnCierresCaja.Size = new Size(200, 60);
            btnCierresCaja.TabIndex = 5;
            btnCierresCaja.Tag = "classic";
            btnCierresCaja.Text = "CUADRES DE CAJA";
            btnCierresCaja.UseVisualStyleBackColor = true;
            btnCierresCaja.Click += btnCierresCaja_Click;
            // 
            // btnRegistrarGasto
            // 
            btnRegistrarGasto.FlatStyle = FlatStyle.Standard;
            btnRegistrarGasto.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnRegistrarGasto.Location = new Point(430, 88);
            btnRegistrarGasto.Name = "btnRegistrarGasto";
            btnRegistrarGasto.Size = new Size(200, 60);
            btnRegistrarGasto.TabIndex = 3;
            btnRegistrarGasto.Tag = "classic";
            btnRegistrarGasto.Text = "REGISTRAR GASTO";
            btnRegistrarGasto.UseVisualStyleBackColor = true;
            btnRegistrarGasto.Click += btnRegistrarGasto_Click;
            // 
            // btnRegistrarIngreso
            // 
            btnRegistrarIngreso.FlatStyle = FlatStyle.Standard;
            btnRegistrarIngreso.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnRegistrarIngreso.Location = new Point(224, 88);
            btnRegistrarIngreso.Name = "btnRegistrarIngreso";
            btnRegistrarIngreso.Size = new Size(200, 60);
            btnRegistrarIngreso.TabIndex = 4;
            btnRegistrarIngreso.Tag = "classic";
            btnRegistrarIngreso.Text = "REGISTRAR INGRESO";
            btnRegistrarIngreso.UseVisualStyleBackColor = true;
            btnRegistrarIngreso.Click += btnRegistrarIngreso_Click;
            // 
            // btnMovimientos
            // 
            btnMovimientos.FlatStyle = FlatStyle.Standard;
            btnMovimientos.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnMovimientos.Location = new Point(636, 88);
            btnMovimientos.Name = "btnMovimientos";
            btnMovimientos.Size = new Size(200, 60);
            btnMovimientos.TabIndex = 2;
            btnMovimientos.Tag = "classic";
            btnMovimientos.Text = "VER MOVIMIENTOS";
            btnMovimientos.UseVisualStyleBackColor = true;
            btnMovimientos.Click += btnVerMovimientos_Click;
            // 
            // btnCerrarCaja
            // 
            btnCerrarCaja.FlatStyle = FlatStyle.Standard;
            btnCerrarCaja.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnCerrarCaja.Location = new Point(842, 88);
            btnCerrarCaja.Name = "btnCerrarCaja";
            btnCerrarCaja.Size = new Size(200, 60);
            btnCerrarCaja.TabIndex = 1;
            btnCerrarCaja.Tag = "classic";
            btnCerrarCaja.Text = "CERRAR CAJA";
            btnCerrarCaja.UseVisualStyleBackColor = true;
            btnCerrarCaja.Click += btnCerrarCaja_Click;
            // 
            // btnAbrirCaja
            // 
            btnAbrirCaja.FlatStyle = FlatStyle.Standard;
            btnAbrirCaja.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnAbrirCaja.Location = new Point(8, 88);
            btnAbrirCaja.Name = "btnAbrirCaja";
            btnAbrirCaja.Size = new Size(200, 60);
            btnAbrirCaja.TabIndex = 0;
            btnAbrirCaja.Tag = "classic";
            btnAbrirCaja.Text = "ABRIR CAJA";
            btnAbrirCaja.UseVisualStyleBackColor = true;
            btnAbrirCaja.Click += btnAbrirCaja_Click;
            // 
            // FrmCajaDashboard
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveBorder;
            ClientSize = new Size(1062, 673);
            Controls.Add(panelBotones);
            Controls.Add(panelIndicadores);
            Controls.Add(panelTop);
            Controls.Add(panelNav);
            Name = "FrmCajaDashboard";
            Text = "CAJA";
            WindowState = FormWindowState.Maximized;
            Load += FrmCajaDashboard_Load;
            panelNav.ResumeLayout(false);
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            panelMontoInicial.ResumeLayout(false);
            panelMontoInicial.PerformLayout();
            panelIndicadores.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panelGastos.ResumeLayout(false);
            panelGastos.PerformLayout();
            panelBalance.ResumeLayout(false);
            panelBalance.PerformLayout();
            panelBotones.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panelNav;
        private Button btnBack;
        private Button btnNavPagar;
        private Button btnNavDeudas;
        private Button btnNavEstado;
        private Button btnNavCaja;
        private Button btnNavHistorial;
        private Button btnNavInventario;
        private Button btnNavReportes;
        private Button btnNavClientes;
        private Panel panelTop;
        private Label lblTitulo;
        private Panel panelMontoInicial;
        private Panel panelIndicadores;
        private Panel panel1;
        private Panel panelGastos;
        private Panel panelBalance;
        private Label lblMontoInicial;
        private Label label1;
        private Label panelIngresos;
        private Label lblIngresosHoy;
        private Label lblGastosHoy;
        private Label label2;
        private Label lblBalance;
        private Label label3;
        private Panel panelBotones;
        private Button btnAbrirCaja;
        private Button btnRegistrarIngreso;
        private Button btnRegistrarGasto;
        private Button btnMovimientos;
        private Button btnCerrarCaja;
        private Button btnCierresCaja;
        private Label lblAyudaCaja;
        private Label lblEstadoCaja;
    }
}