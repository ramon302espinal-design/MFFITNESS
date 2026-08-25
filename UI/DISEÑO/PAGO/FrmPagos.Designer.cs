namespace UI.DISEÑO
{
    partial class FrmPagos
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
            btnPagar = new Button();
            cmbMembresia = new ComboBox();
            label1 = new Label();
            txtMonto = new TextBox();
            label2 = new Label();
            cmbCliente = new ComboBox();
            label3 = new Label();
            btnBack = new Button();
            lblBuscarProducto = new Label();
            txtBuscarProducto = new TextBox();
            lstProductosPos = new ListBox();
            lblFotoProductoPos = new Label();
            picProductoPos = new PictureBox();
            tabProductos = new TabControl();
            tabPago = new TabPage();
            btnLimpiarCarrito = new Button();
            lblTotal = new Label();
            label6 = new Label();
            dgvCarrito = new DataGridView();
            btnPagarProductos = new Button();
            tabMembresia = new TabPage();
            pnlOferta = new Panel();
            lblOfertaPct = new Label();
            txtDescuentoPorcental = new TextBox();
            lblOfertaMonto = new Label();
            txtDescuentoMonto = new TextBox();
            lblTotalPagarTitulo = new Label();
            lblTotalPagar = new Label();
            lblMotivoOferta = new Label();
            txtMotivo = new TextBox();
            pnlFinanciamiento = new Panel();
            lblFechaLimite = new Label();
            dtpFechaVencimiento = new DateTimePicker();
            lblPagoInicial = new Label();
            txtPagoInicial = new TextBox();
            lblSaldo = new Label();
            lblSaldoValor = new Label();
            chkFinanciamiento = new CheckBox();
            panelNav = new Panel();
            btnNavClientes = new Button();
            btnNavReportes = new Button();
            btnNavInventario = new Button();
            btnNavHistorial = new Button();
            btnNavCaja = new Button();
            btnNavEstado = new Button();
            btnNavDeudas = new Button();
            ((System.ComponentModel.ISupportInitialize)picProductoPos).BeginInit();
            tabProductos.SuspendLayout();
            tabPago.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCarrito).BeginInit();
            tabMembresia.SuspendLayout();
            pnlOferta.SuspendLayout();
            pnlFinanciamiento.SuspendLayout();
            panelNav.SuspendLayout();
            SuspendLayout();
            // 
            // btnPagar
            // 
            btnPagar.BackColor = Color.FromArgb(22, 163, 74);
            btnPagar.Font = new Font("Segoe UI", 30F, FontStyle.Bold);
            btnPagar.ForeColor = Color.White;
            btnPagar.Location = new Point(752, 119);
            btnPagar.Name = "btnPagar";
            btnPagar.Size = new Size(266, 80);
            btnPagar.TabIndex = 0;
            btnPagar.Text = "COBRAR";
            btnPagar.UseVisualStyleBackColor = false;
            btnPagar.Click += btnPagarMembresia_Click;
            // 
            // cmbMembresia
            // 
            cmbMembresia.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            cmbMembresia.FormattingEnabled = true;
            cmbMembresia.Location = new Point(31, 98);
            cmbMembresia.Name = "cmbMembresia";
            cmbMembresia.Size = new Size(151, 43);
            cmbMembresia.TabIndex = 1;
            cmbMembresia.SelectedIndexChanged += cmbMembresia_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            label1.Location = new Point(686, 31);
            label1.Name = "label1";
            label1.Size = new Size(139, 46);
            label1.TabIndex = 2;
            label1.Text = "PRECIO";
            // 
            // txtMonto
            // 
            txtMonto.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            txtMonto.Location = new Point(824, 28);
            txtMonto.Name = "txtMonto";
            txtMonto.ReadOnly = true;
            txtMonto.Size = new Size(218, 52);
            txtMonto.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            label2.Location = new Point(31, 60);
            label2.Name = "label2";
            label2.Size = new Size(161, 35);
            label2.TabIndex = 4;
            label2.Text = "MEMBRESIA";
            // 
            // cmbCliente
            // 
            cmbCliente.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            cmbCliente.FormattingEnabled = true;
            cmbCliente.Location = new Point(203, 98);
            cmbCliente.Name = "cmbCliente";
            cmbCliente.Size = new Size(298, 43);
            cmbCliente.TabIndex = 5;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            label3.Location = new Point(231, 60);
            label3.Name = "label3";
            label3.Size = new Size(251, 35);
            label3.TabIndex = 6;
            label3.Text = "MIEMBRO A PAGAR";
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
            // lblBuscarProducto
            // 
            lblBuscarProducto.AutoSize = true;
            lblBuscarProducto.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblBuscarProducto.Location = new Point(766, 289);
            lblBuscarProducto.Name = "lblBuscarProducto";
            lblBuscarProducto.Size = new Size(101, 28);
            lblBuscarProducto.TabIndex = 28;
            lblBuscarProducto.Text = "Escanear:";
            // 
            // txtBuscarProducto
            // 
            txtBuscarProducto.Font = new Font("Segoe UI", 12F);
            txtBuscarProducto.Location = new Point(846, 287);
            txtBuscarProducto.Name = "txtBuscarProducto";
            txtBuscarProducto.PlaceholderText = "Escanear EAN (lector siempre activo en PRODUCTOS)";
            txtBuscarProducto.Size = new Size(275, 34);
            txtBuscarProducto.TabIndex = 29;
            txtBuscarProducto.TextChanged += txtBuscarProducto_TextChanged;
            txtBuscarProducto.KeyDown += txtBuscarProducto_KeyDown;
            // 
            // lstProductosPos
            // 
            lstProductosPos.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lstProductosPos.FormattingEnabled = true;
            lstProductosPos.IntegralHeight = false;
            lstProductosPos.ItemHeight = 28;
            lstProductosPos.Location = new Point(781, 327);
            lstProductosPos.Name = "lstProductosPos";
            lstProductosPos.Size = new Size(340, 355);
            lstProductosPos.TabIndex = 31;
            lstProductosPos.SelectedIndexChanged += lstProductosPos_SelectedIndexChanged;
            lstProductosPos.KeyDown += lstProductosPos_KeyDown;
            lstProductosPos.MouseDown += lstProductosPos_MouseDown;
            lstProductosPos.MouseLeave += lstProductosPos_MouseLeave;
            lstProductosPos.MouseMove += lstProductosPos_MouseMove;
            // 
            // lblFotoProductoPos
            // 
            lblFotoProductoPos.AutoSize = true;
            lblFotoProductoPos.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblFotoProductoPos.ForeColor = Color.FromArgb(45, 55, 72);
            lblFotoProductoPos.Location = new Point(1401, 294);
            lblFotoProductoPos.Name = "lblFotoProductoPos";
            lblFotoProductoPos.Size = new Size(150, 23);
            lblFotoProductoPos.TabIndex = 41;
            lblFotoProductoPos.Text = "FOTO PRODUCTO";
            // 
            // picProductoPos
            // 
            picProductoPos.BackColor = Color.FromArgb(241, 245, 249);
            picProductoPos.BorderStyle = BorderStyle.FixedSingle;
            picProductoPos.Location = new Point(1240, 327);
            picProductoPos.Name = "picProductoPos";
            picProductoPos.Size = new Size(390, 355);
            picProductoPos.SizeMode = PictureBoxSizeMode.Zoom;
            picProductoPos.TabIndex = 42;
            picProductoPos.TabStop = false;
            // 
            // tabProductos
            // 
            tabProductos.Controls.Add(tabPago);
            tabProductos.Controls.Add(tabMembresia);
            tabProductos.Dock = DockStyle.Fill;
            tabProductos.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            tabProductos.Location = new Point(0, 52);
            tabProductos.Name = "tabProductos";
            tabProductos.SelectedIndex = 0;
            tabProductos.Size = new Size(1739, 807);
            tabProductos.TabIndex = 33;
            tabProductos.SelectedIndexChanged += tabProductos_SelectedIndexChanged;
            // 
            // tabPago
            // 
            tabPago.Controls.Add(btnLimpiarCarrito);
            tabPago.Controls.Add(lblTotal);
            tabPago.Controls.Add(label6);
            tabPago.Controls.Add(picProductoPos);
            tabPago.Controls.Add(lblFotoProductoPos);
            tabPago.Controls.Add(lstProductosPos);
            tabPago.Controls.Add(dgvCarrito);
            tabPago.Controls.Add(btnPagarProductos);
            tabPago.Controls.Add(txtBuscarProducto);
            tabPago.Controls.Add(lblBuscarProducto);
            tabPago.Location = new Point(4, 37);
            tabPago.Name = "tabPago";
            tabPago.Padding = new Padding(3);
            tabPago.Size = new Size(1731, 766);
            tabPago.TabIndex = 0;
            tabPago.Text = "PRODUCTOS";
            tabPago.UseVisualStyleBackColor = true;
            // 
            // btnLimpiarCarrito
            // 
            btnLimpiarCarrito.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            btnLimpiarCarrito.Location = new Point(610, 708);
            btnLimpiarCarrito.Name = "btnLimpiarCarrito";
            btnLimpiarCarrito.Size = new Size(150, 44);
            btnLimpiarCarrito.TabIndex = 40;
            btnLimpiarCarrito.Text = "LIMPIAR";
            btnLimpiarCarrito.UseVisualStyleBackColor = true;
            btnLimpiarCarrito.Click += btnLimpiarCarrito_Click;
            // 
            // lblTotal
            // 
            lblTotal.AutoSize = true;
            lblTotal.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblTotal.Location = new Point(137, 708);
            lblTotal.Name = "lblTotal";
            lblTotal.Size = new Size(40, 46);
            lblTotal.TabIndex = 39;
            lblTotal.Text = "0";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            label6.Location = new Point(17, 708);
            label6.Name = "label6";
            label6.Size = new Size(123, 46);
            label6.TabIndex = 38;
            label6.Text = "TOTAL";
            // 
            // dgvCarrito
            // 
            dgvCarrito.AllowUserToAddRows = false;
            dgvCarrito.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCarrito.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvCarrito.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCarrito.Location = new Point(17, 327);
            dgvCarrito.Name = "dgvCarrito";
            dgvCarrito.RowHeadersWidth = 51;
            dgvCarrito.Size = new Size(751, 355);
            dgvCarrito.TabIndex = 36;
            dgvCarrito.CellContentClick += dgvCarrito_CellContentClick;
            dgvCarrito.KeyDown += dgvCarrito_KeyDown;
            // 
            // btnPagarProductos
            // 
            btnPagarProductos.BackColor = Color.FromArgb(22, 163, 74);
            btnPagarProductos.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            btnPagarProductos.ForeColor = Color.White;
            btnPagarProductos.Location = new Point(296, 258);
            btnPagarProductos.Name = "btnPagarProductos";
            btnPagarProductos.Size = new Size(220, 59);
            btnPagarProductos.TabIndex = 35;
            btnPagarProductos.Text = "COBRAR";
            btnPagarProductos.UseVisualStyleBackColor = false;
            btnPagarProductos.Click += btnPagarProductos_Click;
            // 
            // tabMembresia
            // 
            tabMembresia.Controls.Add(pnlOferta);
            tabMembresia.Controls.Add(pnlFinanciamiento);
            tabMembresia.Controls.Add(chkFinanciamiento);
            tabMembresia.Controls.Add(btnPagar);
            tabMembresia.Controls.Add(label3);
            tabMembresia.Controls.Add(label1);
            tabMembresia.Controls.Add(cmbCliente);
            tabMembresia.Controls.Add(label2);
            tabMembresia.Controls.Add(txtMonto);
            tabMembresia.Controls.Add(cmbMembresia);
            tabMembresia.Location = new Point(4, 37);
            tabMembresia.Name = "tabMembresia";
            tabMembresia.Padding = new Padding(3);
            tabMembresia.Size = new Size(1374, 766);
            tabMembresia.TabIndex = 1;
            tabMembresia.Text = "MEMBRESIA";
            tabMembresia.UseVisualStyleBackColor = true;
            // 
            // pnlOferta
            // 
            pnlOferta.BackColor = Color.FromArgb(255, 247, 237);
            pnlOferta.BorderStyle = BorderStyle.FixedSingle;
            pnlOferta.Controls.Add(lblOfertaPct);
            pnlOferta.Controls.Add(txtDescuentoPorcental);
            pnlOferta.Controls.Add(lblOfertaMonto);
            pnlOferta.Controls.Add(txtDescuentoMonto);
            pnlOferta.Controls.Add(lblTotalPagarTitulo);
            pnlOferta.Controls.Add(lblTotalPagar);
            pnlOferta.Controls.Add(lblMotivoOferta);
            pnlOferta.Controls.Add(txtMotivo);
            pnlOferta.Location = new Point(640, 376);
            pnlOferta.Name = "pnlOferta";
            pnlOferta.Size = new Size(420, 225);
            pnlOferta.TabIndex = 11;
            pnlOferta.Visible = false;
            // 
            // lblOfertaPct
            // 
            lblOfertaPct.AutoSize = true;
            lblOfertaPct.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblOfertaPct.Location = new Point(14, 16);
            lblOfertaPct.Name = "lblOfertaPct";
            lblOfertaPct.Size = new Size(132, 25);
            lblOfertaPct.TabIndex = 0;
            lblOfertaPct.Text = "Descuento %:";
            // 
            // txtDescuentoPorcental
            // 
            txtDescuentoPorcental.Font = new Font("Segoe UI", 14F);
            txtDescuentoPorcental.Location = new Point(160, 12);
            txtDescuentoPorcental.Name = "txtDescuentoPorcental";
            txtDescuentoPorcental.Size = new Size(120, 39);
            txtDescuentoPorcental.TabIndex = 1;
            txtDescuentoPorcental.Text = "0";
            txtDescuentoPorcental.TextAlign = HorizontalAlignment.Right;
            txtDescuentoPorcental.TextChanged += txtDescuentoPorcental_TextChanged;
            // 
            // lblOfertaMonto
            // 
            lblOfertaMonto.AutoSize = true;
            lblOfertaMonto.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblOfertaMonto.Location = new Point(14, 68);
            lblOfertaMonto.Name = "lblOfertaMonto";
            lblOfertaMonto.Size = new Size(153, 25);
            lblOfertaMonto.TabIndex = 2;
            lblOfertaMonto.Text = "Descuento RD$:";
            // 
            // txtDescuentoMonto
            // 
            txtDescuentoMonto.Font = new Font("Segoe UI", 14F);
            txtDescuentoMonto.Location = new Point(160, 64);
            txtDescuentoMonto.Name = "txtDescuentoMonto";
            txtDescuentoMonto.Size = new Size(160, 39);
            txtDescuentoMonto.TabIndex = 3;
            txtDescuentoMonto.Text = "0.00";
            txtDescuentoMonto.TextAlign = HorizontalAlignment.Right;
            txtDescuentoMonto.TextChanged += txtDescuentoMonto_TextChanged;
            // 
            // lblTotalPagarTitulo
            // 
            lblTotalPagarTitulo.AutoSize = true;
            lblTotalPagarTitulo.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTotalPagarTitulo.Location = new Point(14, 118);
            lblTotalPagarTitulo.Name = "lblTotalPagarTitulo";
            lblTotalPagarTitulo.Size = new Size(133, 25);
            lblTotalPagarTitulo.TabIndex = 4;
            lblTotalPagarTitulo.Text = "Total a pagar:";
            // 
            // lblTotalPagar
            // 
            lblTotalPagar.AutoSize = true;
            lblTotalPagar.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTotalPagar.ForeColor = Color.FromArgb(22, 163, 74);
            lblTotalPagar.Location = new Point(160, 112);
            lblTotalPagar.Name = "lblTotalPagar";
            lblTotalPagar.Size = new Size(133, 37);
            lblTotalPagar.TabIndex = 5;
            lblTotalPagar.Text = "RD$ 0.00";
            // 
            // lblMotivoOferta
            // 
            lblMotivoOferta.AutoSize = true;
            lblMotivoOferta.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblMotivoOferta.Location = new Point(14, 156);
            lblMotivoOferta.Name = "lblMotivoOferta";
            lblMotivoOferta.Size = new Size(81, 25);
            lblMotivoOferta.TabIndex = 6;
            lblMotivoOferta.Text = "Motivo:";
            // 
            // txtMotivo
            // 
            txtMotivo.Font = new Font("Segoe UI", 11F);
            txtMotivo.Location = new Point(14, 180);
            txtMotivo.Multiline = true;
            txtMotivo.Name = "txtMotivo";
            txtMotivo.PlaceholderText = "Ej. promo temporada, referido...";
            txtMotivo.Size = new Size(386, 34);
            txtMotivo.TabIndex = 7;
            // 
            // pnlFinanciamiento
            // 
            pnlFinanciamiento.BackColor = Color.FromArgb(240, 248, 255);
            pnlFinanciamiento.BorderStyle = BorderStyle.FixedSingle;
            pnlFinanciamiento.Controls.Add(lblFechaLimite);
            pnlFinanciamiento.Controls.Add(dtpFechaVencimiento);
            pnlFinanciamiento.Controls.Add(lblPagoInicial);
            pnlFinanciamiento.Controls.Add(txtPagoInicial);
            pnlFinanciamiento.Controls.Add(lblSaldo);
            pnlFinanciamiento.Controls.Add(lblSaldoValor);
            pnlFinanciamiento.Location = new Point(31, 376);
            pnlFinanciamiento.Name = "pnlFinanciamiento";
            pnlFinanciamiento.Size = new Size(591, 225);
            pnlFinanciamiento.TabIndex = 9;
            pnlFinanciamiento.Visible = false;
            // 
            // lblFechaLimite
            // 
            lblFechaLimite.AutoSize = true;
            lblFechaLimite.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblFechaLimite.Location = new Point(15, 115);
            lblFechaLimite.Name = "lblFechaLimite";
            lblFechaLimite.Size = new Size(204, 25);
            lblFechaLimite.TabIndex = 4;
            lblFechaLimite.Text = "Fecha Limite de Pago:";
            // 
            // dtpFechaVencimiento
            // 
            dtpFechaVencimiento.Enabled = false;
            dtpFechaVencimiento.Font = new Font("Segoe UI", 11F);
            dtpFechaVencimiento.Format = DateTimePickerFormat.Short;
            dtpFechaVencimiento.Location = new Point(230, 110);
            dtpFechaVencimiento.Name = "dtpFechaVencimiento";
            dtpFechaVencimiento.Size = new Size(200, 32);
            dtpFechaVencimiento.TabIndex = 5;
            // 
            // lblPagoInicial
            // 
            lblPagoInicial.AutoSize = true;
            lblPagoInicial.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblPagoInicial.Location = new Point(15, 15);
            lblPagoInicial.Name = "lblPagoInicial";
            lblPagoInicial.Size = new Size(120, 25);
            lblPagoInicial.TabIndex = 0;
            lblPagoInicial.Text = "Pago Inicial:";
            // 
            // txtPagoInicial
            // 
            txtPagoInicial.Font = new Font("Segoe UI", 14F);
            txtPagoInicial.Location = new Point(150, 10);
            txtPagoInicial.Name = "txtPagoInicial";
            txtPagoInicial.Size = new Size(150, 39);
            txtPagoInicial.TabIndex = 1;
            txtPagoInicial.Text = "0";
            txtPagoInicial.TextAlign = HorizontalAlignment.Right;
            txtPagoInicial.TextChanged += txtPagoInicial_TextChanged;
            // 
            // lblSaldo
            // 
            lblSaldo.AutoSize = true;
            lblSaldo.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblSaldo.Location = new Point(15, 65);
            lblSaldo.Name = "lblSaldo";
            lblSaldo.Size = new Size(161, 25);
            lblSaldo.TabIndex = 2;
            lblSaldo.Text = "Saldo Pendiente:";
            // 
            // lblSaldoValor
            // 
            lblSaldoValor.AutoSize = true;
            lblSaldoValor.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblSaldoValor.ForeColor = Color.FromArgb(244, 67, 54);
            lblSaldoValor.Location = new Point(180, 60);
            lblSaldoValor.Name = "lblSaldoValor";
            lblSaldoValor.Size = new Size(88, 37);
            lblSaldoValor.TabIndex = 3;
            lblSaldoValor.Text = "$0.00";
            // 
            // chkFinanciamiento
            // 
            chkFinanciamiento.AutoSize = true;
            chkFinanciamiento.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            chkFinanciamiento.ForeColor = Color.FromArgb(33, 150, 243);
            chkFinanciamiento.Location = new Point(31, 338);
            chkFinanciamiento.Name = "chkFinanciamiento";
            chkFinanciamiento.Size = new Size(185, 32);
            chkFinanciamiento.TabIndex = 8;
            chkFinanciamiento.Text = " Financiamiento";
            chkFinanciamiento.UseVisualStyleBackColor = true;
            chkFinanciamiento.CheckedChanged += chkFinanciamiento_CheckedChanged;
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
            panelNav.Controls.Add(btnBack);
            panelNav.Dock = DockStyle.Top;
            panelNav.Location = new Point(0, 0);
            panelNav.Name = "panelNav";
            panelNav.Size = new Size(1739, 52);
            panelNav.TabIndex = 0;
            // 
            // btnNavClientes
            // 
            btnNavClientes.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavClientes.Location = new Point(820, 10);
            btnNavClientes.Name = "btnNavClientes";
            btnNavClientes.Size = new Size(120, 32);
            btnNavClientes.TabIndex = 7;
            btnNavClientes.Text = "CLIENTES";
            btnNavClientes.UseVisualStyleBackColor = true;
            // 
            // btnNavReportes
            // 
            btnNavReportes.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavReportes.Location = new Point(690, 10);
            btnNavReportes.Name = "btnNavReportes";
            btnNavReportes.Size = new Size(120, 32);
            btnNavReportes.TabIndex = 6;
            btnNavReportes.Text = "REPORTES";
            btnNavReportes.UseVisualStyleBackColor = true;
            // 
            // btnNavInventario
            // 
            btnNavInventario.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavInventario.Location = new Point(550, 10);
            btnNavInventario.Name = "btnNavInventario";
            btnNavInventario.Size = new Size(130, 32);
            btnNavInventario.TabIndex = 5;
            btnNavInventario.Text = "INVENTARIO";
            btnNavInventario.UseVisualStyleBackColor = true;
            // 
            // btnNavHistorial
            // 
            btnNavHistorial.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavHistorial.Location = new Point(420, 10);
            btnNavHistorial.Name = "btnNavHistorial";
            btnNavHistorial.Size = new Size(120, 32);
            btnNavHistorial.TabIndex = 4;
            btnNavHistorial.Text = "HISTORIAL";
            btnNavHistorial.UseVisualStyleBackColor = true;
            // 
            // btnNavCaja
            // 
            btnNavCaja.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavCaja.Location = new Point(300, 10);
            btnNavCaja.Name = "btnNavCaja";
            btnNavCaja.Size = new Size(110, 32);
            btnNavCaja.TabIndex = 3;
            btnNavCaja.Text = "CAJA";
            btnNavCaja.UseVisualStyleBackColor = true;
            // 
            // btnNavEstado
            // 
            btnNavEstado.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavEstado.Location = new Point(180, 10);
            btnNavEstado.Name = "btnNavEstado";
            btnNavEstado.Size = new Size(110, 32);
            btnNavEstado.TabIndex = 2;
            btnNavEstado.Text = "ESTADO";
            btnNavEstado.UseVisualStyleBackColor = true;
            // 
            // btnNavDeudas
            // 
            btnNavDeudas.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavDeudas.Location = new Point(60, 10);
            btnNavDeudas.Name = "btnNavDeudas";
            btnNavDeudas.Size = new Size(110, 32);
            btnNavDeudas.TabIndex = 1;
            btnNavDeudas.Text = "DEUDAS";
            btnNavDeudas.UseVisualStyleBackColor = true;
            // 
            // FrmPagos
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1739, 859);
            Controls.Add(tabProductos);
            Controls.Add(panelNav);
            Name = "FrmPagos";
            Text = "PAGO";
            WindowState = FormWindowState.Maximized;
            Load += FrmPagos_Load;
            ((System.ComponentModel.ISupportInitialize)picProductoPos).EndInit();
            tabProductos.ResumeLayout(false);
            tabPago.ResumeLayout(false);
            tabPago.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCarrito).EndInit();
            tabMembresia.ResumeLayout(false);
            tabMembresia.PerformLayout();
            pnlOferta.ResumeLayout(false);
            pnlOferta.PerformLayout();
            pnlFinanciamiento.ResumeLayout(false);
            pnlFinanciamiento.PerformLayout();
            panelNav.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Button btnPagar;
        private ComboBox cmbMembresia;
        private Label label1;
        private TextBox txtMonto;
        private Label label2;
        private ComboBox cmbCliente;
        private Label label3;
        private Button btnBack;
        private Label lblBuscarProducto;
        private TextBox txtBuscarProducto;
        private ListBox lstProductosPos;
        private Label lblFotoProductoPos;
        private PictureBox picProductoPos;
        private TabControl tabProductos;
        private TabPage tabPago;
        private TabPage tabMembresia;
        private Button btnPagarProductos;
        private DataGridView dgvCarrito;
        private Label lblTotal;
        private Label label6;
        private Button btnLimpiarCarrito;
        private Panel panelNav;
        private Button btnNavDeudas;
        private Button btnNavEstado;
        private Button btnNavCaja;
        private Button btnNavHistorial;
        private Button btnNavInventario;
        private Button btnNavReportes;
        private Button btnNavClientes;
        // ?? CONTROLES DE FINANCIAMIENTO
        private CheckBox chkFinanciamiento;
        private Panel pnlFinanciamiento;
        private Label lblPagoInicial;
        private TextBox txtPagoInicial;
        private Label lblSaldo;
        private Label lblSaldoValor;
        private Label lblFechaLimite;
        private DateTimePicker dtpFechaVencimiento;
        private Panel pnlOferta;
        private Label lblOfertaPct;
        private TextBox txtDescuentoPorcental;
        private Label lblOfertaMonto;
        private TextBox txtDescuentoMonto;
        private Label lblTotalPagarTitulo;
        private Label lblTotalPagar;
        private Label lblMotivoOferta;
        private TextBox txtMotivo;
    }
}