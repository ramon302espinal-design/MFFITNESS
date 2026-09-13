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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
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
            pnlIntereses = new Panel();
            dtInicioDepago = new DateTimePicker();
            label3 = new Label();
            chkMora = new CheckBox();
            dgvPlazos = new DataGridView();
            Column1 = new DataGridViewTextBoxColumn();
            Column2 = new DataGridViewTextBoxColumn();
            Column3 = new DataGridViewTextBoxColumn();
            Column4 = new DataGridViewTextBoxColumn();
            Column5 = new DataGridViewTextBoxColumn();
            Column6 = new DataGridViewTextBoxColumn();
            label14 = new Label();
            txtResumenFinanciamiento = new TextBox();
            txtInteres = new TextBox();
            label13 = new Label();
            numPlazos = new NumericUpDown();
            lblPlazos = new Label();
            rdMensual = new RadioButton();
            rdQuincenal = new RadioButton();
            rdSemanal = new RadioButton();
            label12 = new Label();
            btnCancelar = new Button();
            btnGuardar = new Button();
            pnlOperacion = new Panel();
            cmbbuscarproductos = new ComboBox();
            btnlimpiar = new Button();
            btnagregar = new Button();
            pnlCrearDeuda = new Panel();
            lblPagoDeInicio = new Label();
            txtPagodeinicio = new TextBox();
            lblSaldoPendienteTitulo = new Label();
            lblSaldorestante = new Label();
            lblMontoRd = new Label();
            txtMonto = new TextBox();
            txtConcepto = new TextBox();
            label2 = new Label();
            numCantidad = new NumericUpDown();
            lblCantidad = new Label();
            lblBuscarProducto = new Label();
            cmbTipoPlan = new ComboBox();
            labelTipoPlan = new Label();
            lstSugerenciasProductos = new ListBox();
            pnlDatos = new Panel();
            txtNombre = new Label();
            txtDirTrabaja = new TextBox();
            label10 = new Label();
            label9 = new Label();
            txtTrabaja = new TextBox();
            txtCedula = new TextBox();
            label15 = new Label();
            label6 = new Label();
            txtNombreEmergencia = new TextBox();
            label7 = new Label();
            txtParentesco = new TextBox();
            txtDireccion = new TextBox();
            label8 = new Label();
            label5 = new Label();
            txtTelEmergencia = new TextBox();
            txtTelefono = new TextBox();
            label11 = new Label();
            label4 = new Label();
            label1 = new Label();
            cbClientes = new ComboBox();
            tabHistorial = new TabPage();
            layoutNavDeudas.SuspendLayout();
            panelNav.SuspendLayout();
            tabControl.SuspendLayout();
            tabCrear.SuspendLayout();
            pnlIntereses.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPlazos).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numPlazos).BeginInit();
            pnlOperacion.SuspendLayout();
            pnlCrearDeuda.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCantidad).BeginInit();
            pnlDatos.SuspendLayout();
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
            layoutNavDeudas.Size = new Size(1924, 1055);
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
            panelNav.Size = new Size(1924, 52);
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
            tabControl.Size = new Size(1924, 1003);
            tabControl.TabIndex = 1;
            tabControl.SelectedIndexChanged += tabControl_SelectedIndexChanged;
            // 
            // tabDashboard
            // 
            tabDashboard.BackColor = Color.White;
            tabDashboard.Location = new Point(4, 32);
            tabDashboard.Name = "tabDashboard";
            tabDashboard.Padding = new Padding(3);
            tabDashboard.Size = new Size(1916, 967);
            tabDashboard.TabIndex = 0;
            tabDashboard.Text = "📊 Dashboard";
            // 
            // tabGestion
            // 
            tabGestion.BackColor = Color.White;
            tabGestion.Location = new Point(4, 32);
            tabGestion.Name = "tabGestion";
            tabGestion.Padding = new Padding(3);
            tabGestion.Size = new Size(1916, 967);
            tabGestion.TabIndex = 1;
            tabGestion.Text = "📋 Gestión de Deudas";
            // 
            // tabCrear
            // 
            tabCrear.BackColor = SystemColors.Control;
            tabCrear.Controls.Add(pnlIntereses);
            tabCrear.Controls.Add(pnlOperacion);
            tabCrear.Controls.Add(pnlDatos);
            tabCrear.Font = new Font("Segoe UI", 9F);
            tabCrear.ForeColor = SystemColors.ControlText;
            tabCrear.Location = new Point(4, 32);
            tabCrear.Name = "tabCrear";
            tabCrear.Padding = new Padding(3);
            tabCrear.Size = new Size(1916, 967);
            tabCrear.TabIndex = 2;
            tabCrear.Tag = "classic";
            tabCrear.Text = "➕ Nueva Deuda";
            tabCrear.Click += tabCrear_Click;
            // 
            // pnlIntereses
            // 
            pnlIntereses.BackColor = SystemColors.ControlDark;
            pnlIntereses.Controls.Add(dtInicioDepago);
            pnlIntereses.Controls.Add(label3);
            pnlIntereses.Controls.Add(chkMora);
            pnlIntereses.Controls.Add(dgvPlazos);
            pnlIntereses.Controls.Add(label14);
            pnlIntereses.Controls.Add(txtResumenFinanciamiento);
            pnlIntereses.Controls.Add(txtInteres);
            pnlIntereses.Controls.Add(label13);
            pnlIntereses.Controls.Add(numPlazos);
            pnlIntereses.Controls.Add(lblPlazos);
            pnlIntereses.Controls.Add(rdMensual);
            pnlIntereses.Controls.Add(rdQuincenal);
            pnlIntereses.Controls.Add(rdSemanal);
            pnlIntereses.Controls.Add(label12);
            pnlIntereses.Controls.Add(btnCancelar);
            pnlIntereses.Controls.Add(btnGuardar);
            pnlIntereses.Location = new Point(905, 11);
            pnlIntereses.Name = "pnlIntereses";
            pnlIntereses.Size = new Size(827, 539);
            pnlIntereses.TabIndex = 88;
            // 
            // dtInicioDepago
            // 
            dtInicioDepago.Font = new Font("Segoe UI", 9F);
            dtInicioDepago.Format = DateTimePickerFormat.Short;
            dtInicioDepago.Location = new Point(179, 150);
            dtInicioDepago.Name = "dtInicioDepago";
            dtInicioDepago.Size = new Size(140, 27);
            dtInicioDepago.TabIndex = 103;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label3.Location = new Point(31, 153);
            label3.Name = "label3";
            label3.Size = new Size(140, 23);
            label3.TabIndex = 102;
            label3.Text = "INICIO DE PAGO";
            // 
            // chkMora
            // 
            chkMora.AutoSize = true;
            chkMora.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            chkMora.Location = new Point(23, 476);
            chkMora.Name = "chkMora";
            chkMora.Size = new Size(145, 24);
            chkMora.TabIndex = 101;
            chkMora.Text = "ACTIVAR MORA";
            chkMora.UseVisualStyleBackColor = true;
            // 
            // dgvPlazos
            // 
            dgvPlazos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Control;
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            dgvPlazos.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvPlazos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvPlazos.Columns.AddRange(new DataGridViewColumn[] { Column1, Column2, Column3, Column4, Column5, Column6 });
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = SystemColors.Window;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            dgvPlazos.DefaultCellStyle = dataGridViewCellStyle2;
            dgvPlazos.Location = new Point(23, 189);
            dgvPlazos.Name = "dgvPlazos";
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = SystemColors.Control;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.True;
            dgvPlazos.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            dgvPlazos.RowHeadersWidth = 51;
            dgvPlazos.Size = new Size(780, 265);
            dgvPlazos.TabIndex = 100;
            // 
            // Column1
            // 
            Column1.HeaderText = "CUOTA #";
            Column1.MinimumWidth = 6;
            Column1.Name = "Column1";
            // 
            // Column2
            // 
            Column2.HeaderText = "FECHA VENCIMIENTO";
            Column2.MinimumWidth = 6;
            Column2.Name = "Column2";
            // 
            // Column3
            // 
            Column3.HeaderText = "CAPITAL";
            Column3.MinimumWidth = 6;
            Column3.Name = "Column3";
            // 
            // Column4
            // 
            Column4.HeaderText = "INTERES";
            Column4.MinimumWidth = 6;
            Column4.Name = "Column4";
            // 
            // Column5
            // 
            Column5.HeaderText = "MORA POTENCIAL";
            Column5.MinimumWidth = 6;
            Column5.Name = "Column5";
            // 
            // Column6
            // 
            Column6.HeaderText = "TOTAL";
            Column6.MinimumWidth = 6;
            Column6.Name = "Column6";
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label14.Location = new Point(464, 12);
            label14.Name = "label14";
            label14.Size = new Size(276, 23);
            label14.TabIndex = 99;
            label14.Text = "RESUMEN DEL FINANCIAMIENTO";
            // 
            // txtResumenFinanciamiento
            // 
            txtResumenFinanciamiento.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            txtResumenFinanciamiento.Location = new Point(464, 38);
            txtResumenFinanciamiento.Multiline = true;
            txtResumenFinanciamiento.Name = "txtResumenFinanciamiento";
            txtResumenFinanciamiento.Size = new Size(316, 143);
            txtResumenFinanciamiento.TabIndex = 98;
            txtResumenFinanciamiento.Text = "INTERES TOTAL:\r\nTOTAL CON INTERES:\r\nCUOTA BASE:";
            // 
            // txtInteres
            // 
            txtInteres.Anchor = AnchorStyles.None;
            txtInteres.Location = new Point(134, 115);
            txtInteres.Name = "txtInteres";
            txtInteres.Size = new Size(74, 27);
            txtInteres.TabIndex = 97;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label13.Location = new Point(30, 118);
            label13.Name = "label13";
            label13.Size = new Size(102, 23);
            label13.TabIndex = 96;
            label13.Text = "INTERES  %";
            // 
            // numPlazos
            // 
            numPlazos.Anchor = AnchorStyles.None;
            numPlazos.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            numPlazos.Location = new Point(213, 92);
            numPlazos.Name = "numPlazos";
            numPlazos.Size = new Size(50, 27);
            numPlazos.TabIndex = 95;
            // 
            // lblPlazos
            // 
            lblPlazos.AutoSize = true;
            lblPlazos.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblPlazos.Location = new Point(31, 82);
            lblPlazos.Name = "lblPlazos";
            lblPlazos.Size = new Size(179, 23);
            lblPlazos.TabIndex = 94;
            lblPlazos.Text = "PLAZOS SEMANALES";
            // 
            // rdMensual
            // 
            rdMensual.AutoSize = true;
            rdMensual.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            rdMensual.Location = new Point(294, 44);
            rdMensual.Name = "rdMensual";
            rdMensual.Size = new Size(102, 24);
            rdMensual.TabIndex = 93;
            rdMensual.TabStop = true;
            rdMensual.Text = "MENSUAL";
            rdMensual.UseVisualStyleBackColor = true;
            // 
            // rdQuincenal
            // 
            rdQuincenal.AutoSize = true;
            rdQuincenal.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            rdQuincenal.Location = new Point(153, 44);
            rdQuincenal.Name = "rdQuincenal";
            rdQuincenal.Size = new Size(117, 24);
            rdQuincenal.TabIndex = 92;
            rdQuincenal.TabStop = true;
            rdQuincenal.Text = "QUINCENAL";
            rdQuincenal.UseVisualStyleBackColor = true;
            // 
            // rdSemanal
            // 
            rdSemanal.AutoSize = true;
            rdSemanal.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            rdSemanal.Location = new Point(30, 44);
            rdSemanal.Name = "rdSemanal";
            rdSemanal.Size = new Size(102, 24);
            rdSemanal.TabIndex = 91;
            rdSemanal.TabStop = true;
            rdSemanal.Text = "SEMANAL";
            rdSemanal.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label12.Location = new Point(111, 7);
            label12.Name = "label12";
            label12.Size = new Size(212, 28);
            label12.TabIndex = 90;
            label12.Text = "INTERESES Y PLAZOS";
            // 
            // btnCancelar
            // 
            btnCancelar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnCancelar.Location = new Point(369, 476);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(110, 36);
            btnCancelar.TabIndex = 89;
            btnCancelar.Text = "CANCELAR";
            btnCancelar.UseVisualStyleBackColor = true;
            // 
            // btnGuardar
            // 
            btnGuardar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnGuardar.Location = new Point(213, 476);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(110, 36);
            btnGuardar.TabIndex = 88;
            btnGuardar.Text = "GUARDAR";
            btnGuardar.UseVisualStyleBackColor = true;
            // 
            // pnlOperacion
            // 
            pnlOperacion.BackColor = SystemColors.ControlDark;
            pnlOperacion.Controls.Add(cmbbuscarproductos);
            pnlOperacion.Controls.Add(btnlimpiar);
            pnlOperacion.Controls.Add(btnagregar);
            pnlOperacion.Controls.Add(pnlCrearDeuda);
            pnlOperacion.Controls.Add(lblMontoRd);
            pnlOperacion.Controls.Add(txtMonto);
            pnlOperacion.Controls.Add(txtConcepto);
            pnlOperacion.Controls.Add(label2);
            pnlOperacion.Controls.Add(numCantidad);
            pnlOperacion.Controls.Add(lblCantidad);
            pnlOperacion.Controls.Add(lblBuscarProducto);
            pnlOperacion.Controls.Add(cmbTipoPlan);
            pnlOperacion.Controls.Add(labelTipoPlan);
            pnlOperacion.Controls.Add(lstSugerenciasProductos);
            pnlOperacion.Location = new Point(460, 11);
            pnlOperacion.Name = "pnlOperacion";
            pnlOperacion.Size = new Size(443, 539);
            pnlOperacion.TabIndex = 76;
            // 
            // cmbbuscarproductos
            // 
            cmbbuscarproductos.Enabled = false;
            cmbbuscarproductos.FormattingEnabled = true;
            cmbbuscarproductos.Location = new Point(109, 75);
            cmbbuscarproductos.Name = "cmbbuscarproductos";
            cmbbuscarproductos.Size = new Size(324, 28);
            cmbbuscarproductos.TabIndex = 89;
            // 
            // btnlimpiar
            // 
            btnlimpiar.Enabled = false;
            btnlimpiar.FlatStyle = FlatStyle.System;
            btnlimpiar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnlimpiar.Location = new Point(27, 182);
            btnlimpiar.Name = "btnlimpiar";
            btnlimpiar.Size = new Size(86, 33);
            btnlimpiar.TabIndex = 88;
            btnlimpiar.Text = "LIMPIAR";
            btnlimpiar.UseVisualStyleBackColor = true;
            // 
            // btnagregar
            // 
            btnagregar.Enabled = false;
            btnagregar.FlatStyle = FlatStyle.System;
            btnagregar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnagregar.Location = new Point(27, 143);
            btnagregar.Name = "btnagregar";
            btnagregar.Size = new Size(86, 33);
            btnagregar.TabIndex = 87;
            btnagregar.Text = "AGREGAR";
            btnagregar.UseVisualStyleBackColor = true;
            // 
            // pnlCrearDeuda
            // 
            pnlCrearDeuda.BackColor = Color.FromArgb(240, 248, 255);
            pnlCrearDeuda.BorderStyle = BorderStyle.FixedSingle;
            pnlCrearDeuda.Controls.Add(lblPagoDeInicio);
            pnlCrearDeuda.Controls.Add(txtPagodeinicio);
            pnlCrearDeuda.Controls.Add(lblSaldoPendienteTitulo);
            pnlCrearDeuda.Controls.Add(lblSaldorestante);
            pnlCrearDeuda.Location = new Point(9, 325);
            pnlCrearDeuda.Name = "pnlCrearDeuda";
            pnlCrearDeuda.Size = new Size(400, 180);
            pnlCrearDeuda.TabIndex = 86;
            // 
            // lblPagoDeInicio
            // 
            lblPagoDeInicio.AutoSize = true;
            lblPagoDeInicio.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblPagoDeInicio.Location = new Point(12, 52);
            lblPagoDeInicio.Name = "lblPagoDeInicio";
            lblPagoDeInicio.Size = new Size(143, 25);
            lblPagoDeInicio.TabIndex = 0;
            lblPagoDeInicio.Text = "Pago de inicio:";
            // 
            // txtPagodeinicio
            // 
            txtPagodeinicio.Font = new Font("Segoe UI", 12F);
            txtPagodeinicio.Location = new Point(170, 51);
            txtPagodeinicio.Name = "txtPagodeinicio";
            txtPagodeinicio.Size = new Size(150, 34);
            txtPagodeinicio.TabIndex = 1;
            txtPagodeinicio.Text = "0";
            txtPagodeinicio.TextAlign = HorizontalAlignment.Right;
            // 
            // lblSaldoPendienteTitulo
            // 
            lblSaldoPendienteTitulo.AutoSize = true;
            lblSaldoPendienteTitulo.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblSaldoPendienteTitulo.Location = new Point(12, 100);
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
            lblSaldorestante.Location = new Point(170, 96);
            lblSaldorestante.Name = "lblSaldorestante";
            lblSaldorestante.Size = new Size(77, 32);
            lblSaldorestante.TabIndex = 3;
            lblSaldorestante.Text = "$0.00";
            // 
            // lblMontoRd
            // 
            lblMontoRd.AutoSize = true;
            lblMontoRd.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblMontoRd.Location = new Point(9, 290);
            lblMontoRd.Name = "lblMontoRd";
            lblMontoRd.Size = new Size(52, 28);
            lblMontoRd.TabIndex = 84;
            lblMontoRd.Text = "RD$";
            // 
            // txtMonto
            // 
            txtMonto.Font = new Font("Segoe UI", 11F);
            txtMonto.Location = new Point(67, 290);
            txtMonto.Name = "txtMonto";
            txtMonto.ReadOnly = true;
            txtMonto.Size = new Size(120, 32);
            txtMonto.TabIndex = 85;
            txtMonto.Text = "0.00";
            txtMonto.TextAlign = HorizontalAlignment.Right;
            // 
            // txtConcepto
            // 
            txtConcepto.Location = new Point(134, 226);
            txtConcepto.Multiline = true;
            txtConcepto.Name = "txtConcepto";
            txtConcepto.Size = new Size(296, 63);
            txtConcepto.TabIndex = 83;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label2.Location = new Point(12, 229);
            label2.Name = "label2";
            label2.Size = new Size(116, 28);
            label2.TabIndex = 82;
            label2.Text = "CONCEPTO";
            // 
            // numCantidad
            // 
            numCantidad.Enabled = false;
            numCantidad.Location = new Point(53, 110);
            numCantidad.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            numCantidad.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numCantidad.Name = "numCantidad";
            numCantidad.Size = new Size(60, 27);
            numCantidad.TabIndex = 80;
            numCantidad.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // lblCantidad
            // 
            lblCantidad.AutoSize = true;
            lblCantidad.Enabled = false;
            lblCantidad.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblCantidad.Location = new Point(5, 114);
            lblCantidad.Name = "lblCantidad";
            lblCantidad.Size = new Size(47, 19);
            lblCantidad.TabIndex = 79;
            lblCantidad.Text = "CANT";
            // 
            // lblBuscarProducto
            // 
            lblBuscarProducto.AutoSize = true;
            lblBuscarProducto.Enabled = false;
            lblBuscarProducto.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblBuscarProducto.Location = new Point(7, 78);
            lblBuscarProducto.Name = "lblBuscarProducto";
            lblBuscarProducto.Size = new Size(102, 23);
            lblBuscarProducto.TabIndex = 77;
            lblBuscarProducto.Text = "PRODUCTO";
            // 
            // cmbTipoPlan
            // 
            cmbTipoPlan.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTipoPlan.FormattingEnabled = true;
            cmbTipoPlan.Location = new Point(134, 33);
            cmbTipoPlan.Name = "cmbTipoPlan";
            cmbTipoPlan.Size = new Size(296, 28);
            cmbTipoPlan.TabIndex = 76;
            // 
            // labelTipoPlan
            // 
            labelTipoPlan.AutoSize = true;
            labelTipoPlan.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            labelTipoPlan.Location = new Point(7, 33);
            labelTipoPlan.Name = "labelTipoPlan";
            labelTipoPlan.Size = new Size(126, 28);
            labelTipoPlan.TabIndex = 75;
            labelTipoPlan.Text = "OPERACIÓN";
            // 
            // lstSugerenciasProductos
            // 
            lstSugerenciasProductos.Enabled = false;
            lstSugerenciasProductos.FormattingEnabled = true;
            lstSugerenciasProductos.IntegralHeight = false;
            lstSugerenciasProductos.Location = new Point(115, 110);
            lstSugerenciasProductos.Name = "lstSugerenciasProductos";
            lstSugerenciasProductos.Size = new Size(315, 109);
            lstSugerenciasProductos.TabIndex = 81;
            lstSugerenciasProductos.Visible = false;
            // 
            // pnlDatos
            // 
            pnlDatos.BackColor = SystemColors.ControlDark;
            pnlDatos.Controls.Add(txtNombre);
            pnlDatos.Controls.Add(txtDirTrabaja);
            pnlDatos.Controls.Add(label10);
            pnlDatos.Controls.Add(label9);
            pnlDatos.Controls.Add(txtTrabaja);
            pnlDatos.Controls.Add(txtCedula);
            pnlDatos.Controls.Add(label15);
            pnlDatos.Controls.Add(label6);
            pnlDatos.Controls.Add(txtNombreEmergencia);
            pnlDatos.Controls.Add(label7);
            pnlDatos.Controls.Add(txtParentesco);
            pnlDatos.Controls.Add(txtDireccion);
            pnlDatos.Controls.Add(label8);
            pnlDatos.Controls.Add(label5);
            pnlDatos.Controls.Add(txtTelEmergencia);
            pnlDatos.Controls.Add(txtTelefono);
            pnlDatos.Controls.Add(label11);
            pnlDatos.Controls.Add(label4);
            pnlDatos.Controls.Add(label1);
            pnlDatos.Controls.Add(cbClientes);
            pnlDatos.Location = new Point(8, 11);
            pnlDatos.Name = "pnlDatos";
            pnlDatos.Size = new Size(450, 539);
            pnlDatos.TabIndex = 43;
            // 
            // txtNombre
            // 
            txtNombre.AutoSize = true;
            txtNombre.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            txtNombre.ForeColor = Color.Black;
            txtNombre.Location = new Point(16, 60);
            txtNombre.Name = "txtNombre";
            txtNombre.Size = new Size(99, 28);
            txtNombre.TabIndex = 49;
            txtNombre.Text = "NOMBRE";
            // 
            // txtDirTrabaja
            // 
            txtDirTrabaja.Location = new Point(204, 299);
            txtDirTrabaja.Name = "txtDirTrabaja";
            txtDirTrabaja.Size = new Size(232, 27);
            txtDirTrabaja.TabIndex = 48;
            txtDirTrabaja.TextAlign = HorizontalAlignment.Right;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label10.Location = new Point(15, 297);
            label10.Name = "label10";
            label10.Size = new Size(190, 28);
            label10.TabIndex = 47;
            label10.Text = "DIR. DEL TRABAJO";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label9.Location = new Point(11, 247);
            label9.Name = "label9";
            label9.Size = new Size(179, 28);
            label9.TabIndex = 46;
            label9.Text = "DONDE TRABAJA";
            // 
            // txtTrabaja
            // 
            txtTrabaja.Location = new Point(190, 247);
            txtTrabaja.Name = "txtTrabaja";
            txtTrabaja.Size = new Size(256, 27);
            txtTrabaja.TabIndex = 45;
            txtTrabaja.TextAlign = HorizontalAlignment.Right;
            // 
            // txtCedula
            // 
            txtCedula.Location = new Point(107, 196);
            txtCedula.Name = "txtCedula";
            txtCedula.Size = new Size(232, 27);
            txtCedula.TabIndex = 44;
            txtCedula.TextAlign = HorizontalAlignment.Right;
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label15.Location = new Point(18, 194);
            label15.Name = "label15";
            label15.Size = new Size(88, 28);
            label15.TabIndex = 43;
            label15.Text = "CEDULA";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label6.Location = new Point(8, 441);
            label6.Name = "label6";
            label6.Size = new Size(183, 28);
            label6.TabIndex = 22;
            label6.Text = "TEL. EMERGENCIA";
            // 
            // txtNombreEmergencia
            // 
            txtNombreEmergencia.Location = new Point(109, 390);
            txtNombreEmergencia.Name = "txtNombreEmergencia";
            txtNombreEmergencia.ReadOnly = true;
            txtNombreEmergencia.Size = new Size(297, 27);
            txtNombreEmergencia.TabIndex = 23;
            txtNombreEmergencia.TextAlign = HorizontalAlignment.Right;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label7.Location = new Point(8, 387);
            label7.Name = "label7";
            label7.Size = new Size(99, 28);
            label7.TabIndex = 24;
            label7.Text = "NOMBRE";
            // 
            // txtParentesco
            // 
            txtParentesco.Location = new Point(146, 497);
            txtParentesco.Name = "txtParentesco";
            txtParentesco.ReadOnly = true;
            txtParentesco.Size = new Size(172, 27);
            txtParentesco.TabIndex = 26;
            txtParentesco.TextAlign = HorizontalAlignment.Right;
            // 
            // txtDireccion
            // 
            txtDireccion.Location = new Point(133, 102);
            txtDireccion.Name = "txtDireccion";
            txtDireccion.ReadOnly = true;
            txtDireccion.Size = new Size(172, 27);
            txtDireccion.TabIndex = 38;
            txtDireccion.TextAlign = HorizontalAlignment.Right;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label8.Location = new Point(8, 493);
            label8.Name = "label8";
            label8.Size = new Size(138, 28);
            label8.TabIndex = 27;
            label8.Text = "PARENTESCO";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label5.Location = new Point(15, 100);
            label5.Name = "label5";
            label5.Size = new Size(117, 28);
            label5.TabIndex = 37;
            label5.Text = "DIRECCION";
            // 
            // txtTelEmergencia
            // 
            txtTelEmergencia.Location = new Point(192, 446);
            txtTelEmergencia.Name = "txtTelEmergencia";
            txtTelEmergencia.ReadOnly = true;
            txtTelEmergencia.Size = new Size(172, 27);
            txtTelEmergencia.TabIndex = 28;
            txtTelEmergencia.TextAlign = HorizontalAlignment.Right;
            // 
            // txtTelefono
            // 
            txtTelefono.Location = new Point(64, 144);
            txtTelefono.Name = "txtTelefono";
            txtTelefono.ReadOnly = true;
            txtTelefono.Size = new Size(172, 27);
            txtTelefono.TabIndex = 36;
            txtTelefono.TextAlign = HorizontalAlignment.Right;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label11.ForeColor = Color.Blue;
            label11.Location = new Point(127, 355);
            label11.Name = "label11";
            label11.Size = new Size(204, 23);
            label11.TabIndex = 32;
            label11.Text = "CONTACTO EMERGENTE";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label4.Location = new Point(21, 139);
            label4.Name = "label4";
            label4.Size = new Size(45, 28);
            label4.TabIndex = 35;
            label4.Text = "TEL";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label1.Location = new Point(15, 15);
            label1.Name = "label1";
            label1.Size = new Size(90, 28);
            label1.TabIndex = 33;
            label1.Text = "CLIENTE";
            // 
            // cbClientes
            // 
            cbClientes.DropDownStyle = ComboBoxStyle.DropDownList;
            cbClientes.FormattingEnabled = true;
            cbClientes.Location = new Point(107, 16);
            cbClientes.Name = "cbClientes";
            cbClientes.Size = new Size(280, 28);
            cbClientes.TabIndex = 34;
            // 
            // tabHistorial
            // 
            tabHistorial.BackColor = Color.White;
            tabHistorial.Location = new Point(4, 32);
            tabHistorial.Name = "tabHistorial";
            tabHistorial.Padding = new Padding(3);
            tabHistorial.Size = new Size(1916, 967);
            tabHistorial.TabIndex = 3;
            tabHistorial.Text = "📜 Historial";
            // 
            // FrmModuloDeudas
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1924, 1055);
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
            pnlIntereses.ResumeLayout(false);
            pnlIntereses.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPlazos).EndInit();
            ((System.ComponentModel.ISupportInitialize)numPlazos).EndInit();
            pnlOperacion.ResumeLayout(false);
            pnlOperacion.PerformLayout();
            pnlCrearDeuda.ResumeLayout(false);
            pnlCrearDeuda.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCantidad).EndInit();
            pnlDatos.ResumeLayout(false);
            pnlDatos.PerformLayout();
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
        private TextBox txtParentesco;
        private Label label7;
        private TextBox txtNombreEmergencia;
        private Label label6;
        private TextBox txtTelEmergencia;
        private Label label8;
        private Label label11;
        private Panel pnlDatos;
        private TextBox txtDireccion;
        private Label label5;
        private TextBox txtTelefono;
        private Label label4;
        private Label label1;
        private ComboBox cbClientes;
        private Panel pnlOperacion;
        private Button btnlimpiar;
        private Button btnagregar;
        private Panel pnlCrearDeuda;
        private Label lblPagoDeInicio;
        private TextBox txtPagodeinicio;
        private Label lblSaldoPendienteTitulo;
        private Label lblSaldorestante;
        private Label lblMontoRd;
        private TextBox txtMonto;
        private TextBox txtConcepto;
        private Label label2;
        private NumericUpDown numCantidad;
        private Label lblCantidad;
        private Label lblBuscarProducto;
        private ComboBox cmbTipoPlan;
        private Label labelTipoPlan;
        private ListBox lstSugerenciasProductos;
        private Panel pnlIntereses;
        private CheckBox chkMora;
        private DataGridView dgvPlazos;
        private Label label14;
        private TextBox txtResumenFinanciamiento;
        private TextBox txtInteres;
        private Label label13;
        private NumericUpDown numPlazos;
        private Label lblPlazos;
        private RadioButton rdMensual;
        private RadioButton rdQuincenal;
        private RadioButton rdSemanal;
        private Label label12;
        private Button btnCancelar;
        private Button btnGuardar;
        private Label label9;
        private TextBox txtTrabaja;
        private TextBox txtCedula;
        private Label label15;
        private TextBox txtDirTrabaja;
        private Label label10;
        private Label txtNombre;
        private DataGridViewTextBoxColumn Column1;
        private DataGridViewTextBoxColumn Column2;
        private DataGridViewTextBoxColumn Column3;
        private DataGridViewTextBoxColumn Column4;
        private DataGridViewTextBoxColumn Column5;
        private DataGridViewTextBoxColumn Column6;
        private ComboBox cmbbuscarproductos;
        private Label label3;
        private DateTimePicker dtInicioDepago;
    }
}
