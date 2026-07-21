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
            dgvProductos = new DataGridView();
            btnEditar = new Button();
            btnEliminar = new Button();
            cmbCategoria = new ComboBox();
            label1 = new Label();
            label7 = new Label();
            btnGuardar = new Button();
            label5 = new Label();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            txtCompra = new TextBox();
            txtVenta = new TextBox();
            txtStock = new TextBox();
            txtStockMinimo = new TextBox();
            txtNombre = new TextBox();
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
            panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numSalida).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numCantidad).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvMovimientos).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvProductos).BeginInit();
            tabControl1.SuspendLayout();
            panelNav.SuspendLayout();
            SuspendLayout();
            // 
            // tabProductos
            // 
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
            tabProductos.Controls.Add(dgvProductos);
            tabProductos.Controls.Add(btnEditar);
            tabProductos.Controls.Add(btnEliminar);
            tabProductos.Controls.Add(cmbCategoria);
            tabProductos.Controls.Add(label1);
            tabProductos.Controls.Add(label7);
            tabProductos.Controls.Add(btnGuardar);
            tabProductos.Controls.Add(label5);
            tabProductos.Controls.Add(label4);
            tabProductos.Controls.Add(label3);
            tabProductos.Controls.Add(label2);
            tabProductos.Controls.Add(txtCompra);
            tabProductos.Controls.Add(txtVenta);
            tabProductos.Controls.Add(txtStock);
            tabProductos.Controls.Add(txtStockMinimo);
            tabProductos.Controls.Add(txtNombre);
            tabProductos.Location = new Point(4, 29);
            tabProductos.Name = "tabProductos";
            tabProductos.Padding = new Padding(3);
            tabProductos.Size = new Size(1658, 846);
            tabProductos.TabIndex = 0;
            tabProductos.Text = "REGISTRO DE PRODUCTOS";
            tabProductos.UseVisualStyleBackColor = true;
            tabProductos.Click += tabProductos_Click;
            // 
            // panel2
            // 
            panel2.BackColor = Color.Gold;
            panel2.Controls.Add(lblStockActual);
            panel2.Controls.Add(labelStock);
            panel2.ForeColor = Color.Crimson;
            panel2.ImeMode = ImeMode.On;
            panel2.Location = new Point(1372, 123);
            panel2.Name = "panel2";
            panel2.Size = new Size(250, 194);
            panel2.TabIndex = 63;
            // 
            // lblStockActual
            // 
            lblStockActual.AutoSize = true;
            lblStockActual.BackColor = Color.Transparent;
            lblStockActual.Font = new Font("Segoe UI", 30F, FontStyle.Bold);
            lblStockActual.ForeColor = SystemColors.HotTrack;
            lblStockActual.Location = new Point(94, 81);
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
            labelStock.Location = new Point(25, 31);
            labelStock.Name = "labelStock";
            labelStock.Size = new Size(198, 35);
            labelStock.TabIndex = 60;
            labelStock.Text = "STOCK ACTUAL";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            label9.Location = new Point(47, 447);
            label9.Name = "label9";
            label9.Size = new Size(251, 35);
            label9.TabIndex = 51;
            label9.Text = "ENTRADA Y SALIDA";
            // 
            // txtMotivoExtra
            // 
            txtMotivoExtra.Enabled = false;
            txtMotivoExtra.Location = new Point(56, 751);
            txtMotivoExtra.Name = "txtMotivoExtra";
            txtMotivoExtra.Size = new Size(273, 27);
            txtMotivoExtra.TabIndex = 50;
            // 
            // cmbMotivo
            // 
            cmbMotivo.FormattingEnabled = true;
            cmbMotivo.Location = new Point(56, 590);
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
            label8.Location = new Point(682, 27);
            label8.Name = "label8";
            label8.Size = new Size(340, 35);
            label8.TabIndex = 48;
            label8.Text = "PRODUCTOS REGISTRADOS";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            label6.Location = new Point(783, 390);
            label6.Name = "label6";
            label6.Size = new Size(369, 35);
            label6.TabIndex = 47;
            label6.Text = "HISTORIAL DE MOVIMIENTOS";
            // 
            // cmbProducto
            // 
            cmbProducto.FormattingEnabled = true;
            cmbProducto.Location = new Point(56, 507);
            cmbProducto.Name = "cmbProducto";
            cmbProducto.Size = new Size(242, 28);
            cmbProducto.TabIndex = 44;
            cmbProducto.Text = "SELECCION DE PRODUCTOS";
            // 
            // numSalida
            // 
            numSalida.Location = new Point(197, 695);
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
            btnSalida.Location = new Point(53, 694);
            btnSalida.Name = "btnSalida";
            btnSalida.Size = new Size(138, 28);
            btnSalida.TabIndex = 42;
            btnSalida.Text = "SALIDA";
            btnSalida.UseVisualStyleBackColor = true;
            btnSalida.Click += btnSalida_Click;
            // 
            // numCantidad
            // 
            numCantidad.Location = new Point(197, 639);
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
            btnEntrada.Location = new Point(53, 637);
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
            dgvMovimientos.Location = new Point(427, 438);
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
            // 
            // btnEditar
            // 
            btnEditar.Location = new Point(215, 364);
            btnEditar.Name = "btnEditar";
            btnEditar.Size = new Size(138, 28);
            btnEditar.TabIndex = 26;
            btnEditar.Text = "EDITAR";
            btnEditar.UseVisualStyleBackColor = true;
            btnEditar.Click += btnEditar_Click;
            // 
            // btnEliminar
            // 
            btnEliminar.Location = new Point(369, 364);
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
            btnGuardar.Location = new Point(47, 364);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(138, 28);
            btnGuardar.TabIndex = 21;
            btnGuardar.Text = "GUARDAR";
            btnGuardar.UseVisualStyleBackColor = true;
            btnGuardar.Click += btnGuardar_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
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
            // tabControl1
            // 
            tabControl1.Controls.Add(tabProductos);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 52);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1666, 879);
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
            panelNav.Size = new Size(1666, 52);
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
            btnBack.Text = "";
            btnBack.UseVisualStyleBackColor = true;
            // 
            // FrmProductos
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1666, 931);
            Controls.Add(tabControl1);
            Controls.Add(panelNav);
            Name = "FrmProductos";
            Text = "Inventario";
            WindowState = FormWindowState.Maximized;
            Load += FrmProductos_Load;
            tabProductos.ResumeLayout(false);
            tabProductos.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numSalida).EndInit();
            ((System.ComponentModel.ISupportInitialize)numCantidad).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvMovimientos).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvProductos).EndInit();
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
        private DataGridView dgvProductos;
        private Button btnEditar;
        private Button btnEliminar;
        private ComboBox cmbCategoria;
        private Label label1;
        private Label label7;
        private Button btnGuardar;
        private Label label5;
        private Label label4;
        private Label label3;
        private Label label2;
        private TextBox txtCompra;
        private TextBox txtVenta;
        private TextBox txtStock;
        private TextBox txtStockMinimo;
        private TextBox txtNombre;
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
    }
}