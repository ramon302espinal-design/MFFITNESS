namespace UI
{
    partial class FrmModuloDeudas
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
            if (disposing)
            {
                // Limpiar componentes del diseñador
                components?.Dispose();

                // Limpiar formularios hijos embebidos
                dashboardForm?.Dispose();
                gestionForm?.Dispose();
                historialForm?.Dispose();
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
            layoutNavDeudas = new TableLayoutPanel();
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
            tabControl = new TabControl();
            tabDashboard = new TabPage();
            tabGestion = new TabPage();
            tabCrear = new TabPage();
            btnlimpiar = new Button();
            btnagregar = new Button();
            btnCancelar = new Button();
            btnGuardar = new Button();
            pnlCrearDeuda = new Panel();
            lblPagoDeInicio = new Label();
            txtPagodeinicio = new TextBox();
            lblSaldoPendienteTitulo = new Label();
            lblSaldorestante = new Label();
            lblFechaLimiteDeuda = new Label();
            dtpFechaVencimientodeuda = new DateTimePicker();
            txtMonto = new TextBox();
            label3 = new Label();
            txtConcepto = new TextBox();
            label2 = new Label();
            numCantidad = new NumericUpDown();
            lblCantidad = new Label();
            txtbuscarproductos = new TextBox();
            lblBuscarProducto = new Label();
            cmbTipoPlan = new ComboBox();
            labelTipoPlan = new Label();
            cbClientes = new ComboBox();
            label1 = new Label();
            lstSugerenciasProductos = new ListBox();
            tabHistorial = new TabPage();
            layoutNavDeudas.SuspendLayout();
            panelNav.SuspendLayout();
            tabControl.SuspendLayout();
            tabCrear.SuspendLayout();
            pnlCrearDeuda.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCantidad).BeginInit();
            SuspendLayout();
            // 
            // layoutNavDeudas
            // 
            layoutNavDeudas.ColumnCount = 1;
            layoutNavDeudas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutNavDeudas.Controls.Add(panelNav, 0, 0);
            layoutNavDeudas.Controls.Add(tabControl, 0, 1);
            layoutNavDeudas.Dock = DockStyle.Fill;
            layoutNavDeudas.Location = new Point(0, 0);
            layoutNavDeudas.Margin = new Padding(0);
            layoutNavDeudas.Name = "layoutNavDeudas";
            layoutNavDeudas.RowCount = 2;
            layoutNavDeudas.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
            layoutNavDeudas.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutNavDeudas.Size = new Size(1400, 800);
            layoutNavDeudas.TabIndex = 0;
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
            panelNav.Dock = DockStyle.Fill;
            panelNav.Location = new Point(0, 0);
            panelNav.Margin = new Padding(0);
            panelNav.Name = "panelNav";
            panelNav.Size = new Size(1400, 52);
            panelNav.TabIndex = 0;
            // 
            // btnNavClientes
            // 
            btnNavClientes.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavClientes.Location = new Point(940, 10);
            btnNavClientes.Name = "btnNavClientes";
            btnNavClientes.Size = new Size(120, 32);
            btnNavClientes.TabIndex = 8;
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
            btnNavDeudas.Text = "DEUDAS";
            btnNavDeudas.UseVisualStyleBackColor = true;
            // 
            // btnNavPagar
            // 
            btnNavPagar.BackColor = Color.FromArgb(22, 163, 74);
            btnNavPagar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavPagar.ForeColor = Color.White;
            btnNavPagar.Location = new Point(60, 10);
            btnNavPagar.Name = "btnNavPagar";
            btnNavPagar.Size = new Size(110, 32);
            btnNavPagar.TabIndex = 1;
            btnNavPagar.Text = "COBRAR";
            btnNavPagar.UseVisualStyleBackColor = false;
            // 
            // btnBack
            // 
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.FlatStyle = FlatStyle.Flat;
            btnBack.Font = new Font("Segoe UI", 11F);
            btnBack.Location = new Point(8, 8);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(43, 35);
            btnBack.TabIndex = 0;
            btnBack.UseVisualStyleBackColor = true;
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabDashboard);
            tabControl.Controls.Add(tabGestion);
            tabControl.Controls.Add(tabCrear);
            tabControl.Controls.Add(tabHistorial);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            tabControl.Location = new Point(0, 52);
            tabControl.Margin = new Padding(0);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(1400, 748);
            tabControl.TabIndex = 1;
            tabControl.SelectedIndexChanged += tabControl_SelectedIndexChanged;
            // 
            // tabDashboard
            // 
            tabDashboard.BackColor = Color.White;
            tabDashboard.Location = new Point(4, 32);
            tabDashboard.Name = "tabDashboard";
            tabDashboard.Padding = new Padding(3);
            tabDashboard.Size = new Size(1392, 712);
            tabDashboard.TabIndex = 0;
            tabDashboard.Text = "📊 Dashboard";
            // 
            // tabGestion
            // 
            tabGestion.BackColor = Color.White;
            tabGestion.Location = new Point(4, 32);
            tabGestion.Name = "tabGestion";
            tabGestion.Padding = new Padding(3);
            tabGestion.Size = new Size(1392, 712);
            tabGestion.TabIndex = 1;
            tabGestion.Text = "📋 Gestión de Deudas";
            // 
            // tabCrear
            // 
            tabCrear.BackColor = SystemColors.Control;
            tabCrear.Controls.Add(btnlimpiar);
            tabCrear.Controls.Add(btnagregar);
            tabCrear.Controls.Add(btnCancelar);
            tabCrear.Controls.Add(btnGuardar);
            tabCrear.Controls.Add(pnlCrearDeuda);
            tabCrear.Controls.Add(txtMonto);
            tabCrear.Controls.Add(label3);
            tabCrear.Controls.Add(txtConcepto);
            tabCrear.Controls.Add(label2);
            tabCrear.Controls.Add(numCantidad);
            tabCrear.Controls.Add(lblCantidad);
            tabCrear.Controls.Add(txtbuscarproductos);
            tabCrear.Controls.Add(lblBuscarProducto);
            tabCrear.Controls.Add(cmbTipoPlan);
            tabCrear.Controls.Add(labelTipoPlan);
            tabCrear.Controls.Add(cbClientes);
            tabCrear.Controls.Add(label1);
            tabCrear.Controls.Add(lstSugerenciasProductos);
            tabCrear.Font = new Font("Segoe UI", 9F);
            tabCrear.ForeColor = SystemColors.ControlText;
            tabCrear.Location = new Point(4, 32);
            tabCrear.Name = "tabCrear";
            tabCrear.Padding = new Padding(3);
            tabCrear.Size = new Size(1392, 712);
            tabCrear.TabIndex = 2;
            tabCrear.Tag = "classic";
            tabCrear.Text = "➕ Nueva Deuda";
            // 
            // btnlimpiar
            // 
            btnlimpiar.Enabled = false;
            btnlimpiar.FlatStyle = FlatStyle.System;
            btnlimpiar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnlimpiar.Location = new Point(449, 56);
            btnlimpiar.Name = "btnlimpiar";
            btnlimpiar.Size = new Size(112, 33);
            btnlimpiar.TabIndex = 17;
            btnlimpiar.Text = "LIMPIAR";
            btnlimpiar.UseVisualStyleBackColor = true;
            btnlimpiar.Click += btnlimpiar_Click;
            // 
            // btnagregar
            // 
            btnagregar.Enabled = false;
            btnagregar.FlatStyle = FlatStyle.System;
            btnagregar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnagregar.Location = new Point(449, 93);
            btnagregar.Name = "btnagregar";
            btnagregar.Size = new Size(112, 33);
            btnagregar.TabIndex = 16;
            btnagregar.Text = "AGREGAR";
            btnagregar.UseVisualStyleBackColor = true;
            btnagregar.Click += btnagregar_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnCancelar.Location = new Point(220, 502);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(110, 36);
            btnCancelar.TabIndex = 15;
            btnCancelar.Text = "CANCELAR";
            btnCancelar.UseVisualStyleBackColor = true;
            btnCancelar.Click += btnCancelar_Click;
            // 
            // btnGuardar
            // 
            btnGuardar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnGuardar.Location = new Point(70, 502);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(110, 36);
            btnGuardar.TabIndex = 14;
            btnGuardar.Text = "GUARDAR";
            btnGuardar.UseVisualStyleBackColor = true;
            btnGuardar.Click += btnGuardar_Click;
            // 
            // pnlCrearDeuda
            // 
            pnlCrearDeuda.BackColor = Color.FromArgb(240, 248, 255);
            pnlCrearDeuda.BorderStyle = BorderStyle.FixedSingle;
            pnlCrearDeuda.Controls.Add(lblPagoDeInicio);
            pnlCrearDeuda.Controls.Add(txtPagodeinicio);
            pnlCrearDeuda.Controls.Add(lblSaldoPendienteTitulo);
            pnlCrearDeuda.Controls.Add(lblSaldorestante);
            pnlCrearDeuda.Controls.Add(lblFechaLimiteDeuda);
            pnlCrearDeuda.Controls.Add(dtpFechaVencimientodeuda);
            pnlCrearDeuda.Location = new Point(12, 302);
            pnlCrearDeuda.Name = "pnlCrearDeuda";
            pnlCrearDeuda.Size = new Size(400, 180);
            pnlCrearDeuda.TabIndex = 13;
            // 
            // lblPagoDeInicio
            // 
            lblPagoDeInicio.AutoSize = true;
            lblPagoDeInicio.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblPagoDeInicio.Location = new Point(12, 16);
            lblPagoDeInicio.Name = "lblPagoDeInicio";
            lblPagoDeInicio.Size = new Size(143, 25);
            lblPagoDeInicio.TabIndex = 0;
            lblPagoDeInicio.Text = "Pago de inicio:";
            // 
            // txtPagodeinicio
            // 
            txtPagodeinicio.Font = new Font("Segoe UI", 12F);
            txtPagodeinicio.Location = new Point(170, 15);
            txtPagodeinicio.Name = "txtPagodeinicio";
            txtPagodeinicio.Size = new Size(150, 34);
            txtPagodeinicio.TabIndex = 1;
            txtPagodeinicio.Text = "0";
            txtPagodeinicio.TextAlign = HorizontalAlignment.Right;
            txtPagodeinicio.TextChanged += txtPagodeinicio_TextChanged;
            txtPagodeinicio.KeyPress += txtPagodeinicio_KeyPress;
            // 
            // lblSaldoPendienteTitulo
            // 
            lblSaldoPendienteTitulo.AutoSize = true;
            lblSaldoPendienteTitulo.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblSaldoPendienteTitulo.Location = new Point(12, 64);
            lblSaldoPendienteTitulo.Name = "lblSaldoPendienteTitulo";
            lblSaldoPendienteTitulo.Size = new Size(162, 25);
            lblSaldoPendienteTitulo.TabIndex = 2;
            lblSaldoPendienteTitulo.Text = "Saldo pendiente:";
            // 
            // lblSaldorestante
            // 
            lblSaldorestante.AutoSize = true;
            lblSaldorestante.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblSaldorestante.ForeColor = Color.FromArgb(244, 67, 54);
            lblSaldorestante.Location = new Point(170, 60);
            lblSaldorestante.Name = "lblSaldorestante";
            lblSaldorestante.Size = new Size(77, 32);
            lblSaldorestante.TabIndex = 3;
            lblSaldorestante.Text = "$0.00";
            // 
            // lblFechaLimiteDeuda
            // 
            lblFechaLimiteDeuda.AutoSize = true;
            lblFechaLimiteDeuda.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblFechaLimiteDeuda.Location = new Point(12, 118);
            lblFechaLimiteDeuda.Name = "lblFechaLimiteDeuda";
            lblFechaLimiteDeuda.Size = new Size(199, 25);
            lblFechaLimiteDeuda.TabIndex = 4;
            lblFechaLimiteDeuda.Text = "Fecha límite de pago:";
            // 
            // dtpFechaVencimientodeuda
            // 
            dtpFechaVencimientodeuda.Font = new Font("Segoe UI", 11F);
            dtpFechaVencimientodeuda.Format = DateTimePickerFormat.Short;
            dtpFechaVencimientodeuda.Location = new Point(220, 114);
            dtpFechaVencimientodeuda.Name = "dtpFechaVencimientodeuda";
            dtpFechaVencimientodeuda.Size = new Size(160, 32);
            dtpFechaVencimientodeuda.TabIndex = 5;
            // 
            // txtMonto
            // 
            txtMonto.Location = new Point(166, 258);
            txtMonto.Name = "txtMonto";
            txtMonto.ReadOnly = true;
            txtMonto.Size = new Size(150, 27);
            txtMonto.TabIndex = 12;
            txtMonto.TextAlign = HorizontalAlignment.Right;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label3.Location = new Point(12, 257);
            label3.Name = "label3";
            label3.Size = new Size(146, 28);
            label3.TabIndex = 11;
            label3.Text = "MONTO PLAN";
            // 
            // txtConcepto
            // 
            txtConcepto.Location = new Point(134, 215);
            txtConcepto.Name = "txtConcepto";
            txtConcepto.Size = new Size(266, 27);
            txtConcepto.TabIndex = 10;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label2.Location = new Point(12, 214);
            label2.Name = "label2";
            label2.Size = new Size(116, 28);
            label2.TabIndex = 9;
            label2.Text = "CONCEPTO";
            // 
            // numCantidad
            // 
            numCantidad.Enabled = false;
            numCantidad.Location = new Point(400, 110);
            numCantidad.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            numCantidad.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numCantidad.Name = "numCantidad";
            numCantidad.Size = new Size(60, 27);
            numCantidad.TabIndex = 7;
            numCantidad.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numCantidad.ValueChanged += numCantidad_ValueChanged;
            // 
            // lblCantidad
            // 
            lblCantidad.AutoSize = true;
            lblCantidad.Enabled = false;
            lblCantidad.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCantidad.Location = new Point(300, 98);
            lblCantidad.Name = "lblCantidad";
            lblCantidad.Size = new Size(56, 23);
            lblCantidad.TabIndex = 6;
            lblCantidad.Text = "CANT";
            // 
            // txtbuscarproductos
            // 
            txtbuscarproductos.Enabled = false;
            txtbuscarproductos.Location = new Point(120, 98);
            txtbuscarproductos.Name = "txtbuscarproductos";
            txtbuscarproductos.PlaceholderText = "Buscar por nombre, categoría, id, precio…";
            txtbuscarproductos.Size = new Size(180, 27);
            txtbuscarproductos.TabIndex = 5;
            txtbuscarproductos.TextChanged += txtbuscarproductos_TextChanged;
            txtbuscarproductos.KeyDown += txtbuscarproductos_KeyDown;
            // 
            // lblBuscarProducto
            // 
            lblBuscarProducto.AutoSize = true;
            lblBuscarProducto.Enabled = false;
            lblBuscarProducto.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblBuscarProducto.Location = new Point(12, 100);
            lblBuscarProducto.Name = "lblBuscarProducto";
            lblBuscarProducto.Size = new Size(102, 23);
            lblBuscarProducto.TabIndex = 4;
            lblBuscarProducto.Text = "PRODUCTO";
            // 
            // cmbTipoPlan
            // 
            cmbTipoPlan.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTipoPlan.FormattingEnabled = true;
            cmbTipoPlan.Location = new Point(139, 58);
            cmbTipoPlan.Name = "cmbTipoPlan";
            cmbTipoPlan.Size = new Size(280, 28);
            cmbTipoPlan.TabIndex = 3;
            cmbTipoPlan.SelectedIndexChanged += cmbTipoPlan_SelectedIndexChanged;
            // 
            // labelTipoPlan
            // 
            labelTipoPlan.AutoSize = true;
            labelTipoPlan.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            labelTipoPlan.Location = new Point(12, 58);
            labelTipoPlan.Name = "labelTipoPlan";
            labelTipoPlan.Size = new Size(126, 28);
            labelTipoPlan.TabIndex = 2;
            labelTipoPlan.Text = "OPERACIÓN";
            // 
            // cbClientes
            // 
            cbClientes.DropDownStyle = ComboBoxStyle.DropDownList;
            cbClientes.FormattingEnabled = true;
            cbClientes.Location = new Point(120, 15);
            cbClientes.Name = "cbClientes";
            cbClientes.Size = new Size(280, 28);
            cbClientes.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label1.Location = new Point(12, 15);
            label1.Name = "label1";
            label1.Size = new Size(90, 28);
            label1.TabIndex = 0;
            label1.Text = "CLIENTE";
            // 
            // lstSugerenciasProductos
            // 
            lstSugerenciasProductos.FormattingEnabled = true;
            lstSugerenciasProductos.IntegralHeight = false;
            lstSugerenciasProductos.Location = new Point(100, 126);
            lstSugerenciasProductos.Name = "lstSugerenciasProductos";
            lstSugerenciasProductos.Size = new Size(292, 85);
            lstSugerenciasProductos.TabIndex = 8;
            lstSugerenciasProductos.Click += lstSugerenciasProductos_Click;
            lstSugerenciasProductos.DoubleClick += lstSugerenciasProductos_DoubleClick;
            lstSugerenciasProductos.KeyDown += lstSugerenciasProductos_KeyDown;
            // 
            // tabHistorial
            // 
            tabHistorial.BackColor = Color.White;
            tabHistorial.Location = new Point(4, 32);
            tabHistorial.Name = "tabHistorial";
            tabHistorial.Padding = new Padding(3);
            tabHistorial.Size = new Size(1392, 712);
            tabHistorial.TabIndex = 3;
            tabHistorial.Text = "📜 Historial";
            // 
            // FrmModuloDeudas
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1400, 800);
            Controls.Add(layoutNavDeudas);
            Name = "FrmModuloDeudas";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Módulo de Gestión de Deudas - MF FITNESS";
            WindowState = FormWindowState.Maximized;
            Load += FrmModuloDeudas_Load;
            layoutNavDeudas.ResumeLayout(false);
            panelNav.ResumeLayout(false);
            tabControl.ResumeLayout(false);
            tabCrear.ResumeLayout(false);
            tabCrear.PerformLayout();
            pnlCrearDeuda.ResumeLayout(false);
            pnlCrearDeuda.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCantidad).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel layoutNavDeudas;
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
        private TabControl tabControl;
        private TabPage tabDashboard;
        private TabPage tabGestion;
        private TabPage tabCrear;
        private TabPage tabHistorial;

        // Pantalla "Nueva Deuda": controles del diseñador dentro de tabCrear.
        private Label label1;
        private ComboBox cbClientes;
        private Label labelTipoPlan;
        private ComboBox cmbTipoPlan;
        private Label lblBuscarProducto;
        private TextBox txtbuscarproductos;
        private Label lblCantidad;
        private NumericUpDown numCantidad;
        private ListBox lstSugerenciasProductos;
        private Label label2;
        private TextBox txtConcepto;
        private Label label3;
        private TextBox txtMonto;
        private Panel pnlCrearDeuda;
        private Label lblPagoDeInicio;
        private TextBox txtPagodeinicio;
        private Label lblSaldoPendienteTitulo;
        private Label lblSaldorestante;
        private Label lblFechaLimiteDeuda;
        private DateTimePicker dtpFechaVencimientodeuda;
        private Button btnGuardar;
        private Button btnCancelar;
        private Button btnagregar;
        private Button btnlimpiar;
    }
}
