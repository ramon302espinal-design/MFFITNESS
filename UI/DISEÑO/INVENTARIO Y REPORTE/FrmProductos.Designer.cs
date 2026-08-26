namespace UI.DISEÑO
{
    partial class FrmProductos
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
            tabProductos = new TabPage();
            panel3 = new Panel();
            lblKpiGanVal = new Label();
            label12 = new Label();
            panel1 = new Panel();
            lblKpiInvVal = new Label();
            label11 = new Label();
            panel2 = new Panel();
            lblStockActual = new Label();
            labelStock = new Label();
            label9 = new Label();
            txtMotivoExtra = new TextBox();
            cmbMotivo = new ComboBox();
            label8 = new Label();
            label6 = new Label();
            cmbProducto = new ComboBox();
            numSalida = new NumericUpDown();
            btnSalida = new Button();
            numCantidad = new NumericUpDown();
            btnEntrada = new Button();
            dgvMovimientos = new DataGridView();
            lblBuscarProductos = new Label();
            txtBuscarProductos = new TextBox();
            dgvProductos = new DataGridView();
            btnEditar = new Button();
            btnEliminar = new Button();
            cmbCategoria = new ComboBox();
            label1 = new Label();
            label7 = new Label();
            btnGuardar = new Button();
            btnAgregarProductos = new Button();
            label5 = new Label();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            txtCompra = new TextBox();
            txtVenta = new TextBox();
            txtStock = new TextBox();
            txtStockMinimo = new TextBox();
            txtNombre = new TextBox();
            txtCodigo = new TextBox();
            labelCodigo = new Label();
            lblFotoaqui = new Label();
            picFotoProducto = new PictureBox();
            btnAñadirFoto = new Button();
            tabControl1 = new TabControl();
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
            tabProductos.SuspendLayout();
            panel3.SuspendLayout();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numSalida).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numCantidad).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvMovimientos).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvProductos).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picFotoProducto).BeginInit();
            tabControl1.SuspendLayout();
            panelNav.SuspendLayout();
            SuspendLayout();
            // 
            // tabProductos
            // 
            tabProductos.Controls.Add(panel3);
            tabProductos.Controls.Add(panel1);
            tabProductos.Controls.Add(panel2);
            tabProductos.Controls.Add(label9);
            tabProductos.Controls.Add(txtMotivoExtra);
            tabProductos.Controls.Add(cmbMotivo);
            tabProductos.Controls.Add(label8);
            tabProductos.Controls.Add(label6);
            tabProductos.Controls.Add(cmbProducto);
            tabProductos.Controls.Add(numSalida);
            tabProductos.Controls.Add(btnSalida);
            tabProductos.Controls.Add(numCantidad);
            tabProductos.Controls.Add(btnEntrada);
            tabProductos.Controls.Add(dgvMovimientos);
            tabProductos.Controls.Add(lblBuscarProductos);
            tabProductos.Controls.Add(txtBuscarProductos);
            tabProductos.Controls.Add(dgvProductos);
            tabProductos.Controls.Add(btnEditar);
            tabProductos.Controls.Add(btnEliminar);
            tabProductos.Controls.Add(cmbCategoria);
            tabProductos.Controls.Add(label1);
            tabProductos.Controls.Add(label7);
            tabProductos.Controls.Add(btnGuardar);
            tabProductos.Controls.Add(btnAgregarProductos);
            tabProductos.Controls.Add(label5);
            tabProductos.Controls.Add(label4);
            tabProductos.Controls.Add(label3);
            tabProductos.Controls.Add(label2);
            tabProductos.Controls.Add(txtCompra);
            tabProductos.Controls.Add(txtVenta);
            tabProductos.Controls.Add(txtStock);
            tabProductos.Controls.Add(txtStockMinimo);
            tabProductos.Controls.Add(txtNombre);
            tabProductos.Controls.Add(txtCodigo);
            tabProductos.Controls.Add(labelCodigo);
            tabProductos.Controls.Add(lblFotoaqui);
            tabProductos.Controls.Add(picFotoProducto);
            tabProductos.Controls.Add(btnAñadirFoto);
            tabProductos.Location = new Point(4, 29);
            tabProductos.Name = "tabProductos";
            tabProductos.Padding = new Padding(3);
            tabProductos.Size = new Size(1916, 940);
            tabProductos.TabIndex = 0;
            tabProductos.Text = "REGISTRO DE PRODUCTOS";
            tabProductos.UseVisualStyleBackColor = true;
            tabProductos.Click += tabProductos_Click;
            // 
            // panel3
            // 
            panel3.BackColor = Color.Gold;
            panel3.Controls.Add(lblKpiGanVal);
            panel3.Controls.Add(label12);
            panel3.ForeColor = Color.Crimson;
            panel3.ImeMode = ImeMode.On;
            panel3.Location = new Point(1474, 574);
            panel3.Name = "panel3";
            panel3.Size = new Size(331, 85);
            panel3.TabIndex = 65;
            // 
            // lblKpiGanVal
            // 
            lblKpiGanVal.AutoSize = true;
            lblKpiGanVal.BackColor = Color.Transparent;
            lblKpiGanVal.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
            lblKpiGanVal.ForeColor = SystemColors.HotTrack;
            lblKpiGanVal.Location = new Point(19, 35);
            lblKpiGanVal.Name = "lblKpiGanVal";
            lblKpiGanVal.Size = new Size(34, 40);
            lblKpiGanVal.TabIndex = 61;
            lblKpiGanVal.Text = "0";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            label12.ForeColor = Color.Black;
            label12.Location = new Point(19, 0);
            label12.Name = "label12";
            label12.Size = new Size(294, 35);
            label12.TabIndex = 60;
            label12.Text = "GANANCIA POTENCIAL";
            // 
            // panel1
            // 
            panel1.BackColor = Color.Gold;
            panel1.Controls.Add(lblKpiInvVal);
            panel1.Controls.Add(label11);
            panel1.ForeColor = Color.Crimson;
            panel1.ImeMode = ImeMode.On;
            panel1.Location = new Point(1474, 659);
            panel1.Name = "panel1";
            panel1.Size = new Size(302, 72);
            panel1.TabIndex = 64;
            // 
            // lblKpiInvVal
            // 
            lblKpiInvVal.AutoSize = true;
            lblKpiInvVal.BackColor = Color.Transparent;
            lblKpiInvVal.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
            lblKpiInvVal.ForeColor = SystemColors.HotTrack;
            lblKpiInvVal.Location = new Point(11, 32);
            lblKpiInvVal.Name = "lblKpiInvVal";
            lblKpiInvVal.Size = new Size(34, 40);
            lblKpiInvVal.TabIndex = 61;
            lblKpiInvVal.Text = "0";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            label11.ForeColor = Color.Black;
            label11.Location = new Point(19, 0);
            label11.Name = "label11";
            label11.Size = new Size(167, 35);
            label11.TabIndex = 60;
            label11.Text = "INVENTARIO";
            // 
            // panel2
            // 
            panel2.BackColor = Color.Gold;
            panel2.Controls.Add(lblStockActual);
            panel2.Controls.Add(labelStock);
            panel2.ForeColor = Color.Crimson;
            panel2.ImeMode = ImeMode.On;
            panel2.Location = new Point(1474, 731);
            panel2.Name = "panel2";
            panel2.Size = new Size(249, 101);
            panel2.TabIndex = 63;
            // 
            // lblStockActual
            // 
            lblStockActual.AutoSize = true;
            lblStockActual.BackColor = Color.Transparent;
            lblStockActual.Font = new Font("Segoe UI", 30F, FontStyle.Bold);
            lblStockActual.ForeColor = SystemColors.HotTrack;
            lblStockActual.Location = new Point(96, 32);
            lblStockActual.Name = "lblStockActual";
            lblStockActual.Size = new Size(58, 67);
            lblStockActual.TabIndex = 61;
            lblStockActual.Text = "0";
            // 
            // labelStock
            // 
            labelStock.AutoSize = true;
            labelStock.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            labelStock.ForeColor = Color.Black;
            labelStock.Location = new Point(25, 0);
            labelStock.Name = "labelStock";
            labelStock.Size = new Size(198, 35);
            labelStock.TabIndex = 60;
            labelStock.Text = "STOCK ACTUAL";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            label9.Location = new Point(47, 502);
            label9.Name = "label9";
            label9.Size = new Size(251, 35);
            label9.TabIndex = 51;
            label9.Text = "ENTRADA Y SALIDA";
            // 
            // txtMotivoExtra
            // 
            txtMotivoExtra.Enabled = false;
            txtMotivoExtra.Location = new Point(56, 806);
            txtMotivoExtra.Name = "txtMotivoExtra";
            txtMotivoExtra.Size = new Size(273, 27);
            txtMotivoExtra.TabIndex = 50;
            // 
            // cmbMotivo
            // 
            cmbMotivo.FormattingEnabled = true;
            cmbMotivo.Location = new Point(56, 645);
            cmbMotivo.Name = "cmbMotivo";
            cmbMotivo.Size = new Size(216, 28);
            cmbMotivo.TabIndex = 49;
            cmbMotivo.Text = "MOTIVO";
            cmbMotivo.SelectedIndexChanged += cmbMotivo_SelectedIndexChanged;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            label8.Location = new Point(427, 30);
            label8.Name = "label8";
            label8.Size = new Size(340, 35);
            label8.TabIndex = 48;
            label8.Text = "PRODUCTOS REGISTRADOS";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            label6.Location = new Point(783, 442);
            label6.Name = "label6";
            label6.Size = new Size(369, 35);
            label6.TabIndex = 47;
            label6.Text = "HISTORIAL DE MOVIMIENTOS";
            // 
            // cmbProducto
            // 
            cmbProducto.FormattingEnabled = true;
            cmbProducto.Location = new Point(56, 562);
            cmbProducto.Name = "cmbProducto";
            cmbProducto.Size = new Size(242, 28);
            cmbProducto.TabIndex = 44;
            cmbProducto.Text = "SELECCION DE PRODUCTOS";
            // 
            // numSalida
            // 
            numSalida.Location = new Point(197, 750);
            numSalida.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            numSalida.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numSalida.Name = "numSalida";
            numSalida.Size = new Size(150, 27);
            numSalida.TabIndex = 43;
            numSalida.TextAlign = HorizontalAlignment.Center;
            numSalida.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // btnSalida
            // 
            btnSalida.Location = new Point(53, 749);
            btnSalida.Name = "btnSalida";
            btnSalida.Size = new Size(138, 28);
            btnSalida.TabIndex = 42;
            btnSalida.Text = "SALIDA";
            btnSalida.UseVisualStyleBackColor = true;
            btnSalida.Click += btnSalida_Click;
            // 
            // numCantidad
            // 
            numCantidad.Location = new Point(197, 694);
            numCantidad.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            numCantidad.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numCantidad.Name = "numCantidad";
            numCantidad.Size = new Size(150, 27);
            numCantidad.TabIndex = 41;
            numCantidad.TextAlign = HorizontalAlignment.Center;
            numCantidad.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // btnEntrada
            // 
            btnEntrada.Location = new Point(53, 692);
            btnEntrada.Name = "btnEntrada";
            btnEntrada.Size = new Size(138, 28);
            btnEntrada.TabIndex = 40;
            btnEntrada.Text = "ENTRADA";
            btnEntrada.UseVisualStyleBackColor = true;
            btnEntrada.Click += btnEntrada_Click;
            // 
            // dgvMovimientos
            // 
            dgvMovimientos.AllowUserToAddRows = false;
            dgvMovimientos.AllowUserToDeleteRows = false;
            dgvMovimientos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvMovimientos.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvMovimientos.ColumnHeadersHeight = 29;
            dgvMovimientos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvMovimientos.Location = new Point(427, 493);
            dgvMovimientos.MultiSelect = false;
            dgvMovimientos.Name = "dgvMovimientos";
            dgvMovimientos.ReadOnly = true;
            dgvMovimientos.RowHeadersWidth = 51;
            dgvMovimientos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMovimientos.Size = new Size(1041, 400);
            dgvMovimientos.TabIndex = 37;
            dgvMovimientos.CellFormatting += dgvMovimientos_CellFormatting;
            dgvMovimientos.RowPrePaint += dgvMovimientos_RowPrePaint;
            // 
            // lblBuscarProductos
            // 
            lblBuscarProductos.AutoSize = true;
            lblBuscarProductos.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblBuscarProductos.Location = new Point(773, 30);
            lblBuscarProductos.Name = "lblBuscarProductos";
            lblBuscarProductos.Size = new Size(92, 35);
            lblBuscarProductos.TabIndex = 53;
            lblBuscarProductos.Text = "Buscar";
            // 
            // txtBuscarProductos
            // 
            txtBuscarProductos.Font = new Font("Segoe UI", 11F);
            txtBuscarProductos.Location = new Point(873, 33);
            txtBuscarProductos.Name = "txtBuscarProductos";
            txtBuscarProductos.PlaceholderText = "Nombre, categoría o código…";
            txtBuscarProductos.Size = new Size(494, 32);
            txtBuscarProductos.TabIndex = 54;
            txtBuscarProductos.TextChanged += txtBuscarProductos_TextChanged;
            txtBuscarProductos.KeyDown += txtBuscarProductos_KeyDown;
            // 
            // dgvProductos
            // 
            dgvProductos.AllowUserToAddRows = false;
            dgvProductos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvProductos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvProductos.Location = new Point(427, 75);
            dgvProductos.Name = "dgvProductos";
            dgvProductos.ReadOnly = true;
            dgvProductos.RowHeadersWidth = 51;
            dgvProductos.Size = new Size(925, 283);
            dgvProductos.TabIndex = 27;
            dgvProductos.CellClick += dgvProductos_CellClick;
            dgvProductos.CellDoubleClick += dgvProductos_CellDoubleClick;
            dgvProductos.RowPrePaint += dgvProductos_RowPrePaint;
            dgvProductos.SelectionChanged += dgvProductos_SelectionChanged;
            // 
            // btnEditar
            // 
            btnEditar.Location = new Point(168, 398);
            btnEditar.Name = "btnEditar";
            btnEditar.Size = new Size(138, 28);
            btnEditar.TabIndex = 26;
            btnEditar.Text = "EDITAR";
            btnEditar.UseVisualStyleBackColor = true;
            btnEditar.Click += btnEditar_Click;
            // 
            // btnEliminar
            // 
            btnEliminar.Location = new Point(304, 398);
            btnEliminar.Name = "btnEliminar";
            btnEliminar.Size = new Size(138, 28);
            btnEliminar.TabIndex = 25;
            btnEliminar.Text = "ELIMINAR";
            btnEliminar.UseVisualStyleBackColor = true;
            btnEliminar.Click += btnEliminar_Click;
            // 
            // cmbCategoria
            // 
            cmbCategoria.FormattingEnabled = true;
            cmbCategoria.Location = new Point(227, 120);
            cmbCategoria.Name = "cmbCategoria";
            cmbCategoria.Size = new Size(151, 28);
            cmbCategoria.TabIndex = 24;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(116, 123);
            label1.Name = "label1";
            label1.Size = new Size(87, 20);
            label1.TabIndex = 23;
            label1.Text = "CATEGORIA";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(116, 77);
            label7.Name = "label7";
            label7.Size = new Size(70, 20);
            label7.TabIndex = 22;
            label7.Text = "NOMBRE";
            // 
            // btnGuardar
            // 
            btnGuardar.Location = new Point(47, 398);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(138, 28);
            btnGuardar.TabIndex = 21;
            btnGuardar.Text = "GUARDAR";
            btnGuardar.UseVisualStyleBackColor = true;
            btnGuardar.Click += btnGuardar_Click;
            // 
            // btnAgregarProductos
            // 
            btnAgregarProductos.BackColor = Color.FromArgb(37, 99, 235);
            btnAgregarProductos.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnAgregarProductos.ForeColor = Color.White;
            btnAgregarProductos.Location = new Point(148, 27);
            btnAgregarProductos.Name = "btnAgregarProductos";
            btnAgregarProductos.Size = new Size(230, 28);
            btnAgregarProductos.TabIndex = 22;
            btnAgregarProductos.Text = "AGREGAR CON FOTO / ARCHIVO";
            btnAgregarProductos.UseVisualStyleBackColor = false;
            btnAgregarProductos.Click += btnAgregarProductos_Click;
            // 
            // label5
            // 
            label5.Location = new Point(116, 170);
            label5.Name = "label5";
            label5.Size = new Size(107, 20);
            label5.TabIndex = 20;
            label5.Text = "Precio Compra";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(116, 214);
            label4.Name = "label4";
            label4.Size = new Size(91, 20);
            label4.TabIndex = 19;
            label4.Text = "Precio Venta";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(116, 268);
            label3.Name = "label3";
            label3.Size = new Size(88, 20);
            label3.TabIndex = 18;
            label3.Text = "Stock Inicial";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(116, 321);
            label2.Name = "label2";
            label2.Size = new Size(100, 20);
            label2.TabIndex = 17;
            label2.Text = "Stock Minimo";
            // 
            // txtCompra
            // 
            txtCompra.Location = new Point(227, 167);
            txtCompra.Name = "txtCompra";
            txtCompra.Size = new Size(194, 27);
            txtCompra.TabIndex = 15;
            // 
            // txtVenta
            // 
            txtVenta.Location = new Point(227, 214);
            txtVenta.Name = "txtVenta";
            txtVenta.Size = new Size(194, 27);
            txtVenta.TabIndex = 14;
            // 
            // txtStock
            // 
            txtStock.Enabled = false;
            txtStock.Location = new Point(227, 265);
            txtStock.Name = "txtStock";
            txtStock.ReadOnly = true;
            txtStock.Size = new Size(194, 27);
            txtStock.TabIndex = 13;
            // 
            // txtStockMinimo
            // 
            txtStockMinimo.Location = new Point(227, 318);
            txtStockMinimo.Name = "txtStockMinimo";
            txtStockMinimo.Size = new Size(194, 27);
            txtStockMinimo.TabIndex = 12;
            // 
            // txtNombre
            // 
            txtNombre.Location = new Point(227, 75);
            txtNombre.Name = "txtNombre";
            txtNombre.Size = new Size(194, 27);
            txtNombre.TabIndex = 11;
            // 
            // txtCodigo
            // 
            txtCodigo.Font = new Font("Segoe UI", 11F);
            txtCodigo.Location = new Point(227, 351);
            txtCodigo.Name = "txtCodigo";
            txtCodigo.PlaceholderText = "Escanear EAN para buscar o registrar…";
            txtCodigo.Size = new Size(194, 32);
            txtCodigo.TabIndex = 16;
            txtCodigo.KeyDown += txtCodigo_KeyDown;
            // 
            // labelCodigo
            // 
            labelCodigo.AutoSize = true;
            labelCodigo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelCodigo.ForeColor = Color.FromArgb(45, 55, 72);
            labelCodigo.Location = new Point(116, 357);
            labelCodigo.Name = "labelCodigo";
            labelCodigo.Size = new Size(107, 20);
            labelCodigo.TabIndex = 66;
            labelCodigo.Text = "COD. BARRAS";
            // 
            // lblFotoaqui
            // 
            lblFotoaqui.AutoSize = true;
            lblFotoaqui.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblFotoaqui.ForeColor = Color.FromArgb(45, 55, 72);
            lblFotoaqui.Location = new Point(1550, 54);
            lblFotoaqui.Name = "lblFotoaqui";
            lblFotoaqui.Size = new Size(161, 20);
            lblFotoaqui.TabIndex = 67;
            lblFotoaqui.Text = "FOTO DEL PRODUCTO";
            // 
            // picFotoProducto
            // 
            picFotoProducto.BackColor = Color.FromArgb(241, 245, 249);
            picFotoProducto.BorderStyle = BorderStyle.FixedSingle;
            picFotoProducto.Location = new Point(1358, 77);
            picFotoProducto.Name = "picFotoProducto";
            picFotoProducto.Size = new Size(501, 283);
            picFotoProducto.SizeMode = PictureBoxSizeMode.Zoom;
            picFotoProducto.TabIndex = 68;
            picFotoProducto.TabStop = false;
            // 
            // btnAñadirFoto
            // 
            btnAñadirFoto.BackColor = Color.FromArgb(55, 65, 81);
            btnAñadirFoto.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnAñadirFoto.ForeColor = Color.White;
            btnAñadirFoto.Location = new Point(1358, 361);
            btnAñadirFoto.Name = "btnAñadirFoto";
            btnAñadirFoto.Size = new Size(501, 32);
            btnAñadirFoto.TabIndex = 69;
            btnAñadirFoto.Text = "AÑADIR FOTO";
            btnAñadirFoto.UseVisualStyleBackColor = false;
            btnAñadirFoto.Click += btnAñadirFoto_Click;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabProductos);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 52);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1924, 973);
            tabControl1.SizeMode = TabSizeMode.FillToRight;
            tabControl1.TabIndex = 18;
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
            // FrmProductos
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1924, 1025);
            Controls.Add(tabControl1);
            Controls.Add(panelNav);
            Name = "FrmProductos";
            RightToLeft = RightToLeft.No;
            Text = "INVENTARIO";
            WindowState = FormWindowState.Maximized;
            Load += FrmProductos_Load;
            tabProductos.ResumeLayout(false);
            tabProductos.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numSalida).EndInit();
            ((System.ComponentModel.ISupportInitialize)numCantidad).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvMovimientos).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvProductos).EndInit();
            ((System.ComponentModel.ISupportInitialize)picFotoProducto).EndInit();
            tabControl1.ResumeLayout(false);
            panelNav.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TabPage tabProductos;
        private Label label6;
        private Label labelStock;
        private ComboBox cmbProducto;
        private NumericUpDown numSalida;
        private Button btnSalida;
        private NumericUpDown numCantidad;
        private Button btnEntrada;
        private DataGridView dgvMovimientos;
        private Label lblBuscarProductos;
        private TextBox txtBuscarProductos;
        private DataGridView dgvProductos;
        private Button btnEditar;
        private Button btnEliminar;
        private ComboBox cmbCategoria;
        private Label label1;
        private Label label7;
        private Button btnGuardar;
        private Button btnAgregarProductos;
        private Label label5;
        private Label label4;
        private Label label3;
        private Label label2;
        private TextBox txtCompra;
        private TextBox txtVenta;
        private TextBox txtStock;
        private TextBox txtStockMinimo;
        private TextBox txtNombre;
        private Label labelCodigo;
        private TextBox txtCodigo;
        private Label lblFotoaqui;
        private PictureBox picFotoProducto;
        private Button btnAñadirFoto;
        private TabControl tabControl1;
        private Label label8;
        private ComboBox cmbMotivo;
        private TextBox txtMotivoExtra;
        private Label label9;
        private Panel panel2;
        private Label lblStockActual;
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
        private Panel panel1;
        private Label label10;
        private Label label11;
        private Panel panel3;
        private Label label12;
        private Label lblKpiInvVal;
        private Label lblKpiGanVal;
    }
}