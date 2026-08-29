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
            txtBuscarProducto = new TextBox();
            lstProductosPos = new ListBox();
            picProductoPos = new PictureBox();
            panelToolbarFotoPos = new Panel();
            btnUndoFotoProductoPos = new Button();
            btnIaFotoProductoPos = new Button();
            btnRecortarFotoProductoPos = new Button();
            btnGirarFotoProductoPos = new Button();
            tabProductos = new TabControl();
            tabPago = new TabPage();
            chkSaldoAFavor = new CheckBox();
            pnlSaldoAFavor = new Panel();
            btnCobrarSaldo = new Button();
            btnCerrarSaldo = new Button();
            label7 = new Label();
            label5 = new Label();
            lblNombreSaldoAbono = new Label();
            btnAbonarSaldo = new Button();
            lblTotalSaldoAbono = new Label();
            dgvSaldoAbono = new DataGridView();
            cmbConSaldo = new ComboBox();
            cmbAsignarSaldo = new ComboBox();
            label4 = new Label();
            pnlPausarVentas = new Panel();
            btnCerrarPnlPausa = new Button();
            lblAsignarPausa = new Label();
            cmbClientePausarVenta = new ComboBox();
            lblMiembrosPausados = new Label();
            cmbMiembroPausados = new ComboBox();
            lblNombrePausaVenta = new Label();
            dgvPausaVentas = new DataGridView();
            lblTotalPausaTitulo = new Label();
            lblTotalPausaVenta = new Label();
            btnDespausar = new Button();
            panelFinanciamientoProducto = new Panel();
            lblConceptoProductoFin = new Label();
            txtProducto = new TextBox();
            lblPagoInicioProducto = new Label();
            txtPagoInicioProducto = new TextBox();
            lblSaldoProductoTitulo = new Label();
            lblSaldoRestanteProducto = new Label();
            lblFechaVenceProducto = new Label();
            dtpVenceDeudaProducto = new DateTimePicker();
            btnGuardarDeudaProducto = new Button();
            btnCerrarFinProducto = new Button();
            lblMiembroDebe = new Label();
            txtMiembroDebe = new TextBox();
            listMiembros = new ListBox();
            chkPausarVenta = new CheckBox();
            btnFinanciamiento = new Button();
            btnLimpiarCarrito = new Button();
            lblTotal = new Label();
            label6 = new Label();
            lblFotoProductoPos = new Label();
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
            txtCantidad = new TextBox();
            lblCantidad = new Label();
            panelNav = new Panel();
            btnNavClientes = new Button();
            btnNavReportes = new Button();
            btnNavInventario = new Button();
            btnNavHistorial = new Button();
            btnNavCaja = new Button();
            btnNavEstado = new Button();
            btnNavDeudas = new Button();
            ((System.ComponentModel.ISupportInitialize)picProductoPos).BeginInit();
            panelToolbarFotoPos.SuspendLayout();
            tabProductos.SuspendLayout();
            tabPago.SuspendLayout();
            pnlSaldoAFavor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvSaldoAbono).BeginInit();
            pnlPausarVentas.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPausaVentas).BeginInit();
            panelFinanciamientoProducto.SuspendLayout();
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
            cmbMembresia.Size = new Size(227, 43);
            cmbMembresia.TabIndex = 1;
            cmbMembresia.SelectedIndexChanged += cmbMembresia_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            label1.Location = new Point(746, 52);
            label1.Name = "label1";
            label1.Size = new Size(139, 46);
            label1.TabIndex = 2;
            label1.Text = "PRECIO";
            // 
            // txtMonto
            // 
            txtMonto.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            txtMonto.Location = new Point(884, 49);
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
            cmbCliente.Location = new Point(264, 98);
            cmbCliente.Name = "cmbCliente";
            cmbCliente.Size = new Size(298, 43);
            cmbCliente.TabIndex = 5;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            label3.Location = new Point(287, 60);
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
            // txtBuscarProducto
            // 
            txtBuscarProducto.Font = new Font("Segoe UI", 12F);
            txtBuscarProducto.Location = new Point(743, 426);
            txtBuscarProducto.Name = "txtBuscarProducto";
            txtBuscarProducto.PlaceholderText = "Buscar: nombre, Id, código/EAN…";
            txtBuscarProducto.Size = new Size(376, 34);
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
            lstProductosPos.Location = new Point(779, 478);
            lstProductosPos.Name = "lstProductosPos";
            lstProductosPos.Size = new Size(340, 355);
            lstProductosPos.TabIndex = 31;
            lstProductosPos.SelectedIndexChanged += lstProductosPos_SelectedIndexChanged;
            lstProductosPos.KeyDown += lstProductosPos_KeyDown;
            lstProductosPos.MouseDown += lstProductosPos_MouseDown;
            lstProductosPos.MouseLeave += lstProductosPos_MouseLeave;
            lstProductosPos.MouseMove += lstProductosPos_MouseMove;
            // 
            // picProductoPos
            // 
            picProductoPos.BackColor = Color.FromArgb(241, 245, 249);
            picProductoPos.BorderStyle = BorderStyle.FixedSingle;
            picProductoPos.Location = new Point(1554, 18);
            picProductoPos.Name = "picProductoPos";
            picProductoPos.Size = new Size(342, 355);
            picProductoPos.SizeMode = PictureBoxSizeMode.Zoom;
            picProductoPos.TabIndex = 42;
            picProductoPos.TabStop = false;
            // 
            // panelToolbarFotoPos
            // 
            panelToolbarFotoPos.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            panelToolbarFotoPos.BackColor = Color.FromArgb(40, 15, 23, 42);
            panelToolbarFotoPos.Controls.Add(btnUndoFotoProductoPos);
            panelToolbarFotoPos.Controls.Add(btnIaFotoProductoPos);
            panelToolbarFotoPos.Controls.Add(btnRecortarFotoProductoPos);
            panelToolbarFotoPos.Controls.Add(btnGirarFotoProductoPos);
            panelToolbarFotoPos.Location = new Point(1858, 231);
            panelToolbarFotoPos.Name = "panelToolbarFotoPos";
            panelToolbarFotoPos.Size = new Size(34, 136);
            panelToolbarFotoPos.TabIndex = 100;
            // 
            // btnUndoFotoProductoPos
            // 
            btnUndoFotoProductoPos.BackColor = Color.FromArgb(100, 116, 139);
            btnUndoFotoProductoPos.Cursor = Cursors.Hand;
            btnUndoFotoProductoPos.FlatAppearance.BorderSize = 0;
            btnUndoFotoProductoPos.FlatStyle = FlatStyle.Flat;
            btnUndoFotoProductoPos.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnUndoFotoProductoPos.ForeColor = Color.White;
            btnUndoFotoProductoPos.Location = new Point(1, 0);
            btnUndoFotoProductoPos.Name = "btnUndoFotoProductoPos";
            btnUndoFotoProductoPos.Size = new Size(32, 32);
            btnUndoFotoProductoPos.TabIndex = 0;
            btnUndoFotoProductoPos.TabStop = false;
            btnUndoFotoProductoPos.Text = "↶";
            btnUndoFotoProductoPos.UseVisualStyleBackColor = false;
            btnUndoFotoProductoPos.Click += btnUndoFotoProductoPos_Click;
            // 
            // btnIaFotoProductoPos
            // 
            btnIaFotoProductoPos.BackColor = Color.FromArgb(79, 70, 229);
            btnIaFotoProductoPos.Cursor = Cursors.Hand;
            btnIaFotoProductoPos.FlatAppearance.BorderSize = 0;
            btnIaFotoProductoPos.FlatStyle = FlatStyle.Flat;
            btnIaFotoProductoPos.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            btnIaFotoProductoPos.ForeColor = Color.White;
            btnIaFotoProductoPos.Location = new Point(1, 36);
            btnIaFotoProductoPos.Name = "btnIaFotoProductoPos";
            btnIaFotoProductoPos.Size = new Size(32, 32);
            btnIaFotoProductoPos.TabIndex = 1;
            btnIaFotoProductoPos.TabStop = false;
            btnIaFotoProductoPos.Text = "IA";
            btnIaFotoProductoPos.UseVisualStyleBackColor = false;
            btnIaFotoProductoPos.Click += btnIaFotoProductoPos_Click;
            // 
            // btnRecortarFotoProductoPos
            // 
            btnRecortarFotoProductoPos.BackColor = Color.FromArgb(14, 116, 144);
            btnRecortarFotoProductoPos.Cursor = Cursors.Hand;
            btnRecortarFotoProductoPos.FlatAppearance.BorderSize = 0;
            btnRecortarFotoProductoPos.FlatStyle = FlatStyle.Flat;
            btnRecortarFotoProductoPos.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnRecortarFotoProductoPos.ForeColor = Color.White;
            btnRecortarFotoProductoPos.Location = new Point(1, 72);
            btnRecortarFotoProductoPos.Name = "btnRecortarFotoProductoPos";
            btnRecortarFotoProductoPos.Size = new Size(32, 32);
            btnRecortarFotoProductoPos.TabIndex = 2;
            btnRecortarFotoProductoPos.TabStop = false;
            btnRecortarFotoProductoPos.Text = "✂";
            btnRecortarFotoProductoPos.UseVisualStyleBackColor = false;
            btnRecortarFotoProductoPos.Click += btnRecortarFotoProductoPos_Click;
            // 
            // btnGirarFotoProductoPos
            // 
            btnGirarFotoProductoPos.BackColor = Color.FromArgb(55, 65, 81);
            btnGirarFotoProductoPos.Cursor = Cursors.Hand;
            btnGirarFotoProductoPos.FlatAppearance.BorderSize = 0;
            btnGirarFotoProductoPos.FlatStyle = FlatStyle.Flat;
            btnGirarFotoProductoPos.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnGirarFotoProductoPos.ForeColor = Color.White;
            btnGirarFotoProductoPos.Location = new Point(1, 108);
            btnGirarFotoProductoPos.Name = "btnGirarFotoProductoPos";
            btnGirarFotoProductoPos.Size = new Size(32, 32);
            btnGirarFotoProductoPos.TabIndex = 3;
            btnGirarFotoProductoPos.TabStop = false;
            btnGirarFotoProductoPos.Text = "⟳";
            btnGirarFotoProductoPos.UseVisualStyleBackColor = false;
            btnGirarFotoProductoPos.Click += btnGirarFotoProductoPos_Click;
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
            tabProductos.Size = new Size(1924, 928);
            tabProductos.TabIndex = 33;
            tabProductos.SelectedIndexChanged += tabProductos_SelectedIndexChanged;
            // 
            // tabPago
            // 
            tabPago.Controls.Add(chkSaldoAFavor);
            tabPago.Controls.Add(pnlSaldoAFavor);
            tabPago.Controls.Add(label4);
            tabPago.Controls.Add(pnlPausarVentas);
            tabPago.Controls.Add(panelFinanciamientoProducto);
            tabPago.Controls.Add(chkPausarVenta);
            tabPago.Controls.Add(btnFinanciamiento);
            tabPago.Controls.Add(btnLimpiarCarrito);
            tabPago.Controls.Add(lblTotal);
            tabPago.Controls.Add(label6);
            tabPago.Controls.Add(panelToolbarFotoPos);
            tabPago.Controls.Add(picProductoPos);
            tabPago.Controls.Add(lblFotoProductoPos);
            tabPago.Controls.Add(lstProductosPos);
            tabPago.Controls.Add(dgvCarrito);
            tabPago.Controls.Add(btnPagarProductos);
            tabPago.Controls.Add(txtBuscarProducto);
            tabPago.Location = new Point(4, 37);
            tabPago.Name = "tabPago";
            tabPago.Padding = new Padding(3);
            tabPago.Size = new Size(1916, 887);
            tabPago.TabIndex = 0;
            tabPago.Text = "PRODUCTOS";
            tabPago.UseVisualStyleBackColor = true;
            // 
            // chkSaldoAFavor
            // 
            chkSaldoAFavor.AutoSize = true;
            chkSaldoAFavor.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            chkSaldoAFavor.Location = new Point(451, 433);
            chkSaldoAFavor.Name = "chkSaldoAFavor";
            chkSaldoAFavor.Size = new Size(113, 29);
            chkSaldoAFavor.TabIndex = 103;
            chkSaldoAFavor.Text = "ABONAR";
            chkSaldoAFavor.UseVisualStyleBackColor = true;
            chkSaldoAFavor.CheckedChanged += chkSaldoAFavor_CheckedChanged;
            // 
            // pnlSaldoAFavor
            // 
            pnlSaldoAFavor.Controls.Add(btnCobrarSaldo);
            pnlSaldoAFavor.Controls.Add(btnCerrarSaldo);
            pnlSaldoAFavor.Controls.Add(label7);
            pnlSaldoAFavor.Controls.Add(label5);
            pnlSaldoAFavor.Controls.Add(lblNombreSaldoAbono);
            pnlSaldoAFavor.Controls.Add(btnAbonarSaldo);
            pnlSaldoAFavor.Controls.Add(lblTotalSaldoAbono);
            pnlSaldoAFavor.Controls.Add(dgvSaldoAbono);
            pnlSaldoAFavor.Controls.Add(cmbConSaldo);
            pnlSaldoAFavor.Controls.Add(cmbAsignarSaldo);
            pnlSaldoAFavor.Location = new Point(888, 6);
            pnlSaldoAFavor.Name = "pnlSaldoAFavor";
            pnlSaldoAFavor.Size = new Size(660, 398);
            pnlSaldoAFavor.TabIndex = 102;
            pnlSaldoAFavor.Visible = false;
            // 
            // btnCobrarSaldo
            // 
            btnCobrarSaldo.BackColor = SystemColors.MenuHighlight;
            btnCobrarSaldo.ForeColor = Color.White;
            btnCobrarSaldo.Location = new Point(492, 188);
            btnCobrarSaldo.Name = "btnCobrarSaldo";
            btnCobrarSaldo.Size = new Size(118, 39);
            btnCobrarSaldo.TabIndex = 11;
            btnCobrarSaldo.Text = "COBRAR";
            btnCobrarSaldo.UseVisualStyleBackColor = false;
            btnCobrarSaldo.Click += btnCobrarSaldo_Click;
            // 
            // btnCerrarSaldo
            // 
            btnCerrarSaldo.FlatAppearance.BorderSize = 0;
            btnCerrarSaldo.FlatStyle = FlatStyle.Flat;
            btnCerrarSaldo.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnCerrarSaldo.ForeColor = Color.FromArgb(185, 28, 28);
            btnCerrarSaldo.Location = new Point(615, 5);
            btnCerrarSaldo.Name = "btnCerrarSaldo";
            btnCerrarSaldo.Size = new Size(42, 32);
            btnCerrarSaldo.TabIndex = 10;
            btnCerrarSaldo.Text = "X";
            btnCerrarSaldo.UseVisualStyleBackColor = true;
            btnCerrarSaldo.Click += btnCerrarSaldo_Click;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(61, 57);
            label7.Name = "label7";
            label7.Size = new Size(126, 28);
            label7.TabIndex = 7;
            label7.Text = "CON SALDO";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(90, 12);
            label5.Name = "label5";
            label5.Size = new Size(100, 28);
            label5.TabIndex = 6;
            label5.Text = "ASIGNAR";
            // 
            // lblNombreSaldoAbono
            // 
            lblNombreSaldoAbono.AutoSize = true;
            lblNombreSaldoAbono.Location = new Point(12, 116);
            lblNombreSaldoAbono.Name = "lblNombreSaldoAbono";
            lblNombreSaldoAbono.Size = new Size(99, 28);
            lblNombreSaldoAbono.TabIndex = 5;
            lblNombreSaldoAbono.Text = "NOMBRE";
            // 
            // btnAbonarSaldo
            // 
            btnAbonarSaldo.BackColor = SystemColors.MenuHighlight;
            btnAbonarSaldo.ForeColor = Color.White;
            btnAbonarSaldo.Location = new Point(492, 238);
            btnAbonarSaldo.Name = "btnAbonarSaldo";
            btnAbonarSaldo.Size = new Size(118, 39);
            btnAbonarSaldo.TabIndex = 4;
            btnAbonarSaldo.Text = "ABONAR";
            btnAbonarSaldo.UseVisualStyleBackColor = false;
            btnAbonarSaldo.Click += btnAbonarSaldo_Click;
            // 
            // lblTotalSaldoAbono
            // 
            lblTotalSaldoAbono.AutoSize = true;
            lblTotalSaldoAbono.Location = new Point(483, 294);
            lblTotalSaldoAbono.Name = "lblTotalSaldoAbono";
            lblTotalSaldoAbono.Size = new Size(72, 28);
            lblTotalSaldoAbono.TabIndex = 3;
            lblTotalSaldoAbono.Text = "TOTAL";
            // 
            // dgvSaldoAbono
            // 
            dgvSaldoAbono.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvSaldoAbono.Location = new Point(3, 148);
            dgvSaldoAbono.Name = "dgvSaldoAbono";
            dgvSaldoAbono.RowHeadersWidth = 51;
            dgvSaldoAbono.Size = new Size(477, 244);
            dgvSaldoAbono.TabIndex = 2;
            // 
            // cmbConSaldo
            // 
            cmbConSaldo.FormattingEnabled = true;
            cmbConSaldo.Location = new Point(195, 54);
            cmbConSaldo.Name = "cmbConSaldo";
            cmbConSaldo.Size = new Size(350, 36);
            cmbConSaldo.TabIndex = 1;
            cmbConSaldo.SelectedIndexChanged += cmbConSaldo_SelectedIndexChanged;
            // 
            // cmbAsignarSaldo
            // 
            cmbAsignarSaldo.FormattingEnabled = true;
            cmbAsignarSaldo.Location = new Point(195, 12);
            cmbAsignarSaldo.Name = "cmbAsignarSaldo";
            cmbAsignarSaldo.Size = new Size(347, 36);
            cmbAsignarSaldo.TabIndex = 0;
            cmbAsignarSaldo.SelectedIndexChanged += cmbAsignarSaldo_SelectedIndexChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label4.Location = new Point(652, 432);
            label4.Name = "label4";
            label4.Size = new Size(89, 28);
            label4.TabIndex = 10;
            label4.Text = "BUSCAR";
            // 
            // pnlPausarVentas
            // 
            pnlPausarVentas.BackColor = Color.FromArgb(255, 251, 235);
            pnlPausarVentas.BorderStyle = BorderStyle.FixedSingle;
            pnlPausarVentas.Controls.Add(btnCerrarPnlPausa);
            pnlPausarVentas.Controls.Add(lblAsignarPausa);
            pnlPausarVentas.Controls.Add(cmbClientePausarVenta);
            pnlPausarVentas.Controls.Add(lblMiembrosPausados);
            pnlPausarVentas.Controls.Add(cmbMiembroPausados);
            pnlPausarVentas.Controls.Add(lblNombrePausaVenta);
            pnlPausarVentas.Controls.Add(dgvPausaVentas);
            pnlPausarVentas.Controls.Add(lblTotalPausaTitulo);
            pnlPausarVentas.Controls.Add(lblTotalPausaVenta);
            pnlPausarVentas.Controls.Add(btnDespausar);
            pnlPausarVentas.Location = new Point(25, 6);
            pnlPausarVentas.Name = "pnlPausarVentas";
            pnlPausarVentas.Size = new Size(857, 398);
            pnlPausarVentas.TabIndex = 52;
            pnlPausarVentas.Visible = false;
            // 
            // btnCerrarPnlPausa
            // 
            btnCerrarPnlPausa.FlatAppearance.BorderSize = 0;
            btnCerrarPnlPausa.FlatStyle = FlatStyle.Flat;
            btnCerrarPnlPausa.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnCerrarPnlPausa.ForeColor = Color.FromArgb(185, 28, 28);
            btnCerrarPnlPausa.Location = new Point(810, 4);
            btnCerrarPnlPausa.Name = "btnCerrarPnlPausa";
            btnCerrarPnlPausa.Size = new Size(42, 32);
            btnCerrarPnlPausa.TabIndex = 0;
            btnCerrarPnlPausa.Text = "X";
            btnCerrarPnlPausa.UseVisualStyleBackColor = true;
            btnCerrarPnlPausa.Click += btnCerrarPnlPausa_Click;
            // 
            // lblAsignarPausa
            // 
            lblAsignarPausa.AutoSize = true;
            lblAsignarPausa.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblAsignarPausa.Location = new Point(12, 12);
            lblAsignarPausa.Name = "lblAsignarPausa";
            lblAsignarPausa.Size = new Size(159, 23);
            lblAsignarPausa.TabIndex = 1;
            lblAsignarPausa.Text = "Miembro (asignar)";
            // 
            // cmbClientePausarVenta
            // 
            cmbClientePausarVenta.Font = new Font("Segoe UI", 10F);
            cmbClientePausarVenta.FormattingEnabled = true;
            cmbClientePausarVenta.Location = new Point(12, 38);
            cmbClientePausarVenta.Name = "cmbClientePausarVenta";
            cmbClientePausarVenta.Size = new Size(450, 31);
            cmbClientePausarVenta.TabIndex = 2;
            cmbClientePausarVenta.Text = "Buscar miembro...";
            cmbClientePausarVenta.DropDown += cmbClientePausarVenta_DropDown;
            cmbClientePausarVenta.SelectedIndexChanged += cmbClientePausarVenta_SelectedIndexChanged;
            cmbClientePausarVenta.TextUpdate += cmbClientePausarVenta_TextUpdate;
            cmbClientePausarVenta.DropDownClosed += cmbClientePausarVenta_DropDownClosed;
            cmbClientePausarVenta.Enter += cmbClientePausarVenta_Enter;
            cmbClientePausarVenta.KeyDown += cmbClientePausarVenta_KeyDown;
            cmbClientePausarVenta.MouseDown += cmbClientePausarVenta_MouseDown;
            // 
            // lblMiembrosPausados
            // 
            lblMiembrosPausados.AutoSize = true;
            lblMiembrosPausados.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblMiembrosPausados.Location = new Point(12, 78);
            lblMiembrosPausados.Name = "lblMiembrosPausados";
            lblMiembrosPausados.Size = new Size(166, 23);
            lblMiembrosPausados.TabIndex = 3;
            lblMiembrosPausados.Text = "Miembros en pausa";
            // 
            // cmbMiembroPausados
            // 
            cmbMiembroPausados.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMiembroPausados.Font = new Font("Segoe UI", 10F);
            cmbMiembroPausados.FormattingEnabled = true;
            cmbMiembroPausados.Location = new Point(12, 104);
            cmbMiembroPausados.Name = "cmbMiembroPausados";
            cmbMiembroPausados.Size = new Size(450, 31);
            cmbMiembroPausados.TabIndex = 4;
            cmbMiembroPausados.SelectedIndexChanged += cmbMiembroPausados_SelectedIndexChanged;
            // 
            // lblNombrePausaVenta
            // 
            lblNombrePausaVenta.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblNombrePausaVenta.ForeColor = Color.FromArgb(30, 64, 175);
            lblNombrePausaVenta.Location = new Point(617, 154);
            lblNombrePausaVenta.Name = "lblNombrePausaVenta";
            lblNombrePausaVenta.Size = new Size(247, 28);
            lblNombrePausaVenta.TabIndex = 5;
            lblNombrePausaVenta.Text = "(sin selección)";
            // 
            // dgvPausaVentas
            // 
            dgvPausaVentas.AllowUserToAddRows = false;
            dgvPausaVentas.AllowUserToDeleteRows = false;
            dgvPausaVentas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvPausaVentas.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvPausaVentas.Location = new Point(12, 144);
            dgvPausaVentas.Name = "dgvPausaVentas";
            dgvPausaVentas.ReadOnly = true;
            dgvPausaVentas.RowHeadersWidth = 51;
            dgvPausaVentas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPausaVentas.Size = new Size(599, 240);
            dgvPausaVentas.TabIndex = 6;
            // 
            // lblTotalPausaTitulo
            // 
            lblTotalPausaTitulo.AutoSize = true;
            lblTotalPausaTitulo.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTotalPausaTitulo.Location = new Point(625, 352);
            lblTotalPausaTitulo.Name = "lblTotalPausaTitulo";
            lblTotalPausaTitulo.Size = new Size(72, 28);
            lblTotalPausaTitulo.TabIndex = 7;
            lblTotalPausaTitulo.Text = "TOTAL";
            // 
            // lblTotalPausaVenta
            // 
            lblTotalPausaVenta.AutoSize = true;
            lblTotalPausaVenta.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTotalPausaVenta.ForeColor = Color.FromArgb(180, 83, 9);
            lblTotalPausaVenta.Location = new Point(696, 348);
            lblTotalPausaVenta.Name = "lblTotalPausaVenta";
            lblTotalPausaVenta.Size = new Size(77, 32);
            lblTotalPausaVenta.TabIndex = 8;
            lblTotalPausaVenta.Text = "$0.00";
            // 
            // btnDespausar
            // 
            btnDespausar.BackColor = Color.FromArgb(22, 163, 74);
            btnDespausar.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnDespausar.ForeColor = Color.White;
            btnDespausar.Location = new Point(616, 186);
            btnDespausar.Name = "btnDespausar";
            btnDespausar.Size = new Size(220, 40);
            btnDespausar.TabIndex = 9;
            btnDespausar.Text = "DESPAUSAR";
            btnDespausar.UseVisualStyleBackColor = false;
            btnDespausar.Click += btnDespausar_Click;
            // 
            // panelFinanciamientoProducto
            // 
            panelFinanciamientoProducto.BackColor = Color.FromArgb(240, 248, 255);
            panelFinanciamientoProducto.BorderStyle = BorderStyle.FixedSingle;
            panelFinanciamientoProducto.Controls.Add(lblConceptoProductoFin);
            panelFinanciamientoProducto.Controls.Add(txtProducto);
            panelFinanciamientoProducto.Controls.Add(lblPagoInicioProducto);
            panelFinanciamientoProducto.Controls.Add(txtPagoInicioProducto);
            panelFinanciamientoProducto.Controls.Add(lblSaldoProductoTitulo);
            panelFinanciamientoProducto.Controls.Add(lblSaldoRestanteProducto);
            panelFinanciamientoProducto.Controls.Add(lblFechaVenceProducto);
            panelFinanciamientoProducto.Controls.Add(dtpVenceDeudaProducto);
            panelFinanciamientoProducto.Controls.Add(btnGuardarDeudaProducto);
            panelFinanciamientoProducto.Controls.Add(btnCerrarFinProducto);
            panelFinanciamientoProducto.Controls.Add(lblMiembroDebe);
            panelFinanciamientoProducto.Controls.Add(txtMiembroDebe);
            panelFinanciamientoProducto.Controls.Add(listMiembros);
            panelFinanciamientoProducto.Location = new Point(1129, 478);
            panelFinanciamientoProducto.Name = "panelFinanciamientoProducto";
            panelFinanciamientoProducto.Size = new Size(680, 355);
            panelFinanciamientoProducto.TabIndex = 42;
            panelFinanciamientoProducto.Visible = false;
            // 
            // lblConceptoProductoFin
            // 
            lblConceptoProductoFin.AutoSize = true;
            lblConceptoProductoFin.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblConceptoProductoFin.Location = new Point(12, 5);
            lblConceptoProductoFin.Name = "lblConceptoProductoFin";
            lblConceptoProductoFin.Size = new Size(86, 23);
            lblConceptoProductoFin.TabIndex = 0;
            lblConceptoProductoFin.Text = "Concepto";
            // 
            // txtProducto
            // 
            txtProducto.Font = new Font("Segoe UI", 10F);
            txtProducto.Location = new Point(12, 36);
            txtProducto.Multiline = true;
            txtProducto.Name = "txtProducto";
            txtProducto.Size = new Size(400, 100);
            txtProducto.TabIndex = 1;
            // 
            // lblPagoInicioProducto
            // 
            lblPagoInicioProducto.AutoSize = true;
            lblPagoInicioProducto.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblPagoInicioProducto.Location = new Point(12, 150);
            lblPagoInicioProducto.Name = "lblPagoInicioProducto";
            lblPagoInicioProducto.Size = new Size(102, 23);
            lblPagoInicioProducto.TabIndex = 5;
            lblPagoInicioProducto.Text = "Pago inicial";
            // 
            // txtPagoInicioProducto
            // 
            txtPagoInicioProducto.Font = new Font("Segoe UI", 12F);
            txtPagoInicioProducto.Location = new Point(160, 146);
            txtPagoInicioProducto.Name = "txtPagoInicioProducto";
            txtPagoInicioProducto.Size = new Size(140, 34);
            txtPagoInicioProducto.TabIndex = 6;
            txtPagoInicioProducto.Text = "0";
            txtPagoInicioProducto.TextAlign = HorizontalAlignment.Right;
            txtPagoInicioProducto.TextChanged += txtPagoInicioProducto_TextChanged;
            txtPagoInicioProducto.KeyPress += txtPagoInicioProducto_KeyPress;
            // 
            // lblSaldoProductoTitulo
            // 
            lblSaldoProductoTitulo.AutoSize = true;
            lblSaldoProductoTitulo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblSaldoProductoTitulo.Location = new Point(12, 190);
            lblSaldoProductoTitulo.Name = "lblSaldoProductoTitulo";
            lblSaldoProductoTitulo.Size = new Size(141, 23);
            lblSaldoProductoTitulo.TabIndex = 7;
            lblSaldoProductoTitulo.Text = "Saldo pendiente";
            // 
            // lblSaldoRestanteProducto
            // 
            lblSaldoRestanteProducto.AutoSize = true;
            lblSaldoRestanteProducto.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblSaldoRestanteProducto.ForeColor = Color.FromArgb(244, 67, 54);
            lblSaldoRestanteProducto.Location = new Point(160, 184);
            lblSaldoRestanteProducto.Name = "lblSaldoRestanteProducto";
            lblSaldoRestanteProducto.Size = new Size(77, 32);
            lblSaldoRestanteProducto.TabIndex = 8;
            lblSaldoRestanteProducto.Text = "$0.00";
            // 
            // lblFechaVenceProducto
            // 
            lblFechaVenceProducto.AutoSize = true;
            lblFechaVenceProducto.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblFechaVenceProducto.Location = new Point(12, 230);
            lblFechaVenceProducto.Name = "lblFechaVenceProducto";
            lblFechaVenceProducto.Size = new Size(153, 23);
            lblFechaVenceProducto.TabIndex = 9;
            lblFechaVenceProducto.Text = "Fecha límite pago";
            // 
            // dtpVenceDeudaProducto
            // 
            dtpVenceDeudaProducto.Font = new Font("Segoe UI", 10F);
            dtpVenceDeudaProducto.Format = DateTimePickerFormat.Short;
            dtpVenceDeudaProducto.Location = new Point(190, 226);
            dtpVenceDeudaProducto.Name = "dtpVenceDeudaProducto";
            dtpVenceDeudaProducto.Size = new Size(160, 30);
            dtpVenceDeudaProducto.TabIndex = 10;
            // 
            // btnGuardarDeudaProducto
            // 
            btnGuardarDeudaProducto.BackColor = Color.FromArgb(22, 163, 74);
            btnGuardarDeudaProducto.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnGuardarDeudaProducto.ForeColor = Color.White;
            btnGuardarDeudaProducto.Location = new Point(12, 280);
            btnGuardarDeudaProducto.Name = "btnGuardarDeudaProducto";
            btnGuardarDeudaProducto.Size = new Size(220, 40);
            btnGuardarDeudaProducto.TabIndex = 11;
            btnGuardarDeudaProducto.Text = "GUARDAR DEUDA";
            btnGuardarDeudaProducto.UseVisualStyleBackColor = false;
            btnGuardarDeudaProducto.Click += btnGuardarDeudaProducto_Click;
            // 
            // btnCerrarFinProducto
            // 
            btnCerrarFinProducto.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCerrarFinProducto.Location = new Point(250, 280);
            btnCerrarFinProducto.Name = "btnCerrarFinProducto";
            btnCerrarFinProducto.Size = new Size(152, 40);
            btnCerrarFinProducto.TabIndex = 12;
            btnCerrarFinProducto.Text = "CERRAR";
            btnCerrarFinProducto.UseVisualStyleBackColor = true;
            btnCerrarFinProducto.Click += btnCerrarFinProducto_Click;
            // 
            // lblMiembroDebe
            // 
            lblMiembroDebe.AutoSize = true;
            lblMiembroDebe.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblMiembroDebe.Location = new Point(430, 32);
            lblMiembroDebe.Name = "lblMiembroDebe";
            lblMiembroDebe.Size = new Size(216, 23);
            lblMiembroDebe.TabIndex = 2;
            lblMiembroDebe.Text = "Miembro deudor (buscar)";
            // 
            // txtMiembroDebe
            // 
            txtMiembroDebe.Font = new Font("Segoe UI", 11F);
            txtMiembroDebe.Location = new Point(430, 58);
            txtMiembroDebe.Name = "txtMiembroDebe";
            txtMiembroDebe.PlaceholderText = "Nombre, apellido o Id…";
            txtMiembroDebe.Size = new Size(230, 32);
            txtMiembroDebe.TabIndex = 3;
            txtMiembroDebe.TextChanged += txtMiembroDebe_TextChanged;
            txtMiembroDebe.KeyDown += txtMiembroDebe_KeyDown;
            // 
            // listMiembros
            // 
            listMiembros.Font = new Font("Segoe UI", 10F);
            listMiembros.IntegralHeight = false;
            listMiembros.ItemHeight = 23;
            listMiembros.Location = new Point(430, 96);
            listMiembros.Name = "listMiembros";
            listMiembros.Size = new Size(230, 200);
            listMiembros.TabIndex = 4;
            listMiembros.Click += listMiembros_Click;
            listMiembros.SelectedIndexChanged += listMiembros_SelectedIndexChanged;
            listMiembros.DoubleClick += listMiembros_DoubleClick;
            listMiembros.KeyDown += listMiembros_KeyDown;
            // 
            // chkPausarVenta
            // 
            chkPausarVenta.AutoSize = true;
            chkPausarVenta.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            chkPausarVenta.Location = new Point(251, 435);
            chkPausarVenta.Name = "chkPausarVenta";
            chkPausarVenta.Size = new Size(108, 29);
            chkPausarVenta.TabIndex = 51;
            chkPausarVenta.Text = "PAUSAR";
            chkPausarVenta.UseVisualStyleBackColor = true;
            chkPausarVenta.CheckedChanged += chkPausarVenta_CheckedChanged;
            // 
            // btnFinanciamiento
            // 
            btnFinanciamiento.BackColor = Color.FromArgb(33, 150, 243);
            btnFinanciamiento.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            btnFinanciamiento.ForeColor = Color.White;
            btnFinanciamiento.Location = new Point(1127, 412);
            btnFinanciamiento.Name = "btnFinanciamiento";
            btnFinanciamiento.Size = new Size(220, 60);
            btnFinanciamiento.TabIndex = 41;
            btnFinanciamiento.Text = "FINANCIAR";
            btnFinanciamiento.UseVisualStyleBackColor = false;
            btnFinanciamiento.Click += btnFinanciamiento_Click;
            // 
            // btnLimpiarCarrito
            // 
            btnLimpiarCarrito.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            btnLimpiarCarrito.Location = new Point(618, 835);
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
            lblTotal.Location = new Point(145, 836);
            lblTotal.Name = "lblTotal";
            lblTotal.Size = new Size(40, 46);
            lblTotal.TabIndex = 39;
            lblTotal.Text = "0";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            label6.Location = new Point(25, 836);
            label6.Name = "label6";
            label6.Size = new Size(123, 46);
            label6.TabIndex = 38;
            label6.Text = "TOTAL";
            // 
            // lblFotoProductoPos
            // 
            lblFotoProductoPos.AutoSize = true;
            lblFotoProductoPos.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblFotoProductoPos.ForeColor = Color.FromArgb(45, 55, 72);
            lblFotoProductoPos.Location = new Point(1746, 381);
            lblFotoProductoPos.Name = "lblFotoProductoPos";
            lblFotoProductoPos.Size = new Size(150, 23);
            lblFotoProductoPos.TabIndex = 41;
            lblFotoProductoPos.Text = "FOTO PRODUCTO";
            // 
            // dgvCarrito
            // 
            dgvCarrito.AllowUserToAddRows = false;
            dgvCarrito.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCarrito.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvCarrito.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCarrito.Location = new Point(17, 478);
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
            btnPagarProductos.Location = new Point(25, 409);
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
            tabMembresia.Controls.Add(txtCantidad);
            tabMembresia.Controls.Add(lblCantidad);
            tabMembresia.Controls.Add(label3);
            tabMembresia.Controls.Add(label1);
            tabMembresia.Controls.Add(cmbCliente);
            tabMembresia.Controls.Add(label2);
            tabMembresia.Controls.Add(txtMonto);
            tabMembresia.Controls.Add(cmbMembresia);
            tabMembresia.Location = new Point(4, 37);
            tabMembresia.Name = "tabMembresia";
            tabMembresia.Padding = new Padding(3);
            tabMembresia.Size = new Size(1916, 887);
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
            // txtCantidad
            // 
            txtCantidad.Enabled = false;
            txtCantidad.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            txtCantidad.Location = new Point(584, 90);
            txtCantidad.MaxLength = 2;
            txtCantidad.Name = "txtCantidad";
            txtCantidad.Size = new Size(90, 52);
            txtCantidad.TabIndex = 6;
            txtCantidad.Text = "1";
            txtCantidad.TextAlign = HorizontalAlignment.Center;
            txtCantidad.TextChanged += txtCantidad_TextChanged;
            txtCantidad.Leave += txtCantidad_Leave;
            // 
            // lblCantidad
            // 
            lblCantidad.AutoSize = true;
            lblCantidad.Enabled = false;
            lblCantidad.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblCantidad.ForeColor = Color.FromArgb(22, 101, 52);
            lblCantidad.Location = new Point(562, 36);
            lblCantidad.Name = "lblCantidad";
            lblCantidad.Size = new Size(146, 35);
            lblCantidad.TabIndex = 12;
            lblCantidad.Text = "CANTIDAD";
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
            panelNav.Size = new Size(1924, 52);
            panelNav.TabIndex = 0;
            // 
            // btnNavClientes
            // 
            btnNavClientes.BackColor = Color.Aqua;
            btnNavClientes.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavClientes.Location = new Point(820, 10);
            btnNavClientes.Name = "btnNavClientes";
            btnNavClientes.Size = new Size(120, 32);
            btnNavClientes.TabIndex = 7;
            btnNavClientes.Text = "CLIENTES";
            btnNavClientes.UseVisualStyleBackColor = false;
            // 
            // btnNavReportes
            // 
            btnNavReportes.BackColor = SystemColors.WindowText;
            btnNavReportes.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavReportes.ForeColor = SystemColors.ControlLightLight;
            btnNavReportes.Location = new Point(690, 10);
            btnNavReportes.Name = "btnNavReportes";
            btnNavReportes.Size = new Size(120, 32);
            btnNavReportes.TabIndex = 6;
            btnNavReportes.Text = "REPORTES";
            btnNavReportes.UseVisualStyleBackColor = false;
            // 
            // btnNavInventario
            // 
            btnNavInventario.BackColor = Color.SlateBlue;
            btnNavInventario.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavInventario.ForeColor = SystemColors.ControlLightLight;
            btnNavInventario.Location = new Point(550, 10);
            btnNavInventario.Name = "btnNavInventario";
            btnNavInventario.Size = new Size(130, 32);
            btnNavInventario.TabIndex = 5;
            btnNavInventario.Text = "INVENTARIO";
            btnNavInventario.UseVisualStyleBackColor = false;
            // 
            // btnNavHistorial
            // 
            btnNavHistorial.BackColor = Color.Yellow;
            btnNavHistorial.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavHistorial.Location = new Point(420, 10);
            btnNavHistorial.Name = "btnNavHistorial";
            btnNavHistorial.Size = new Size(120, 32);
            btnNavHistorial.TabIndex = 4;
            btnNavHistorial.Text = "HISTORIAL";
            btnNavHistorial.UseVisualStyleBackColor = false;
            // 
            // btnNavCaja
            // 
            btnNavCaja.BackColor = Color.Gray;
            btnNavCaja.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavCaja.ForeColor = SystemColors.ControlLightLight;
            btnNavCaja.Location = new Point(300, 10);
            btnNavCaja.Name = "btnNavCaja";
            btnNavCaja.Size = new Size(110, 32);
            btnNavCaja.TabIndex = 3;
            btnNavCaja.Text = "CAJA";
            btnNavCaja.UseVisualStyleBackColor = false;
            // 
            // btnNavEstado
            // 
            btnNavEstado.BackColor = SystemColors.HotTrack;
            btnNavEstado.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavEstado.ForeColor = SystemColors.ControlLight;
            btnNavEstado.Location = new Point(180, 10);
            btnNavEstado.Name = "btnNavEstado";
            btnNavEstado.Size = new Size(110, 32);
            btnNavEstado.TabIndex = 2;
            btnNavEstado.Text = "ESTADO";
            btnNavEstado.UseVisualStyleBackColor = false;
            // 
            // btnNavDeudas
            // 
            btnNavDeudas.BackColor = Color.Red;
            btnNavDeudas.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNavDeudas.ForeColor = SystemColors.ControlLightLight;
            btnNavDeudas.Location = new Point(60, 10);
            btnNavDeudas.Name = "btnNavDeudas";
            btnNavDeudas.Size = new Size(110, 32);
            btnNavDeudas.TabIndex = 1;
            btnNavDeudas.Text = "DEUDAS";
            btnNavDeudas.UseVisualStyleBackColor = false;
            // 
            // FrmPagos
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1924, 980);
            Controls.Add(tabProductos);
            Controls.Add(panelNav);
            Name = "FrmPagos";
            Text = "PAGO";
            WindowState = FormWindowState.Maximized;
            Load += FrmPagos_Load;
            ((System.ComponentModel.ISupportInitialize)picProductoPos).EndInit();
            panelToolbarFotoPos.ResumeLayout(false);
            tabProductos.ResumeLayout(false);
            tabPago.ResumeLayout(false);
            tabPago.PerformLayout();
            pnlSaldoAFavor.ResumeLayout(false);
            pnlSaldoAFavor.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvSaldoAbono).EndInit();
            pnlPausarVentas.ResumeLayout(false);
            pnlPausarVentas.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPausaVentas).EndInit();
            panelFinanciamientoProducto.ResumeLayout(false);
            panelFinanciamientoProducto.PerformLayout();
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
        private TextBox txtBuscarProducto;
        private ListBox lstProductosPos;
        private PictureBox picProductoPos;
        private Panel panelToolbarFotoPos;
        private Button btnUndoFotoProductoPos;
        private Button btnIaFotoProductoPos;
        private Button btnRecortarFotoProductoPos;
        private Button btnGirarFotoProductoPos;
        private TabControl tabProductos;
        private TabPage tabPago;
        private TabPage tabMembresia;
        private Button btnPagarProductos;
        private DataGridView dgvCarrito;
        private Label lblTotal;
        private Label label6;
        private Button btnLimpiarCarrito;
        private Button btnFinanciamiento;
        private CheckBox chkPausarVenta;
        private Panel pnlPausarVentas;
        private Button btnCerrarPnlPausa;
        private Label lblAsignarPausa;
        private ComboBox cmbClientePausarVenta;
        private Label lblMiembrosPausados;
        private ComboBox cmbMiembroPausados;
        private Label lblNombrePausaVenta;
        private DataGridView dgvPausaVentas;
        private Label lblTotalPausaTitulo;
        private Label lblTotalPausaVenta;
        private Button btnDespausar;
        private Panel panelFinanciamientoProducto;
        private Label lblConceptoProductoFin;
        private TextBox txtProducto;
        private Label lblMiembroDebe;
        private TextBox txtMiembroDebe;
        private ListBox listMiembros;
        private Label lblPagoInicioProducto;
        private TextBox txtPagoInicioProducto;
        private Label lblSaldoProductoTitulo;
        private Label lblSaldoRestanteProducto;
        private Label lblFechaVenceProducto;
        private DateTimePicker dtpVenceDeudaProducto;
        private Button btnGuardarDeudaProducto;
        private Button btnCerrarFinProducto;
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
        private Label lblCantidad;
        private TextBox txtCantidad;
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
        private Label label4;
        private Label lblFotoProductoPos;
        private Panel pnlSaldoAFavor;
        private CheckBox chkSaldoAFavor;
        private Label lblNombreSaldoAbono;
        private Button btnAbonarSaldo;
        private Label lblTotalSaldoAbono;
        private DataGridView dgvSaldoAbono;
        private ComboBox cmbConSaldo;
        private ComboBox cmbAsignarSaldo;
        private Label label7;
        private Label label5;
        private Button btnCerrarSaldo;
        private Button btnCobrarSaldo;
    }
}