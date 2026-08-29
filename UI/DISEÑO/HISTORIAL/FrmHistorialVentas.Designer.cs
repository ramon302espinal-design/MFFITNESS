namespace UI.DISEÑO
{
    partial class FrmHistorialVentas
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
            tabControl1 = new TabControl();
            tabMembresia = new TabPage();
            dgvHistorialMembresia = new DataGridView();
            txtBuscarHistMembresia = new TextBox();
            lblBuscarHistMembresia = new Label();
            label4 = new Label();
            dgvHistorial = new DataGridView();
            label1 = new Label();
            tabProductos = new TabPage();
            txtBuscarProductos = new TextBox();
            label5 = new Label();
            dgvDetalleProductos = new DataGridView();
            label3 = new Label();
            dgvVentasProductos = new DataGridView();
            label2 = new Label();
            panelNav.SuspendLayout();
            tabControl1.SuspendLayout();
            tabMembresia.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvHistorialMembresia).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvHistorial).BeginInit();
            tabProductos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvDetalleProductos).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvVentasProductos).BeginInit();
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
            panelNav.Size = new Size(1447, 52);
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
            btnBack.Text = "◀️";
            btnBack.UseVisualStyleBackColor = true;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabMembresia);
            tabControl1.Controls.Add(tabProductos);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            tabControl1.Location = new Point(0, 52);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1447, 621);
            tabControl1.TabIndex = 1;
            // 
            // tabMembresia
            // 
            tabMembresia.BackColor = Color.White;
            tabMembresia.Controls.Add(dgvHistorialMembresia);
            tabMembresia.Controls.Add(txtBuscarHistMembresia);
            tabMembresia.Controls.Add(lblBuscarHistMembresia);
            tabMembresia.Controls.Add(label4);
            tabMembresia.Controls.Add(dgvHistorial);
            tabMembresia.Controls.Add(label1);
            tabMembresia.Location = new Point(4, 34);
            tabMembresia.Name = "tabMembresia";
            tabMembresia.Padding = new Padding(12);
            tabMembresia.Size = new Size(1439, 583);
            tabMembresia.TabIndex = 0;
            tabMembresia.Text = "MEMBRESÍA";
            // 
            // dgvHistorialMembresia
            // 
            dgvHistorialMembresia.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvHistorialMembresia.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvHistorialMembresia.Location = new Point(900, 96);
            dgvHistorialMembresia.Name = "dgvHistorialMembresia";
            dgvHistorialMembresia.RowHeadersWidth = 51;
            dgvHistorialMembresia.Size = new Size(516, 454);
            dgvHistorialMembresia.TabIndex = 5;
            // 
            // txtBuscarHistMembresia
            // 
            txtBuscarHistMembresia.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtBuscarHistMembresia.Font = new Font("Segoe UI", 11F);
            txtBuscarHistMembresia.Location = new Point(980, 52);
            txtBuscarHistMembresia.Name = "txtBuscarHistMembresia";
            txtBuscarHistMembresia.PlaceholderText = "Nombre, teléfono, lugar, plan...";
            txtBuscarHistMembresia.Size = new Size(436, 32);
            txtBuscarHistMembresia.TabIndex = 4;
            txtBuscarHistMembresia.TextChanged += txtBuscarHistMembresia_TextChanged;
            // 
            // lblBuscarHistMembresia
            // 
            lblBuscarHistMembresia.AutoSize = true;
            lblBuscarHistMembresia.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblBuscarHistMembresia.Location = new Point(900, 56);
            lblBuscarHistMembresia.Name = "lblBuscarHistMembresia";
            lblBuscarHistMembresia.Size = new Size(77, 25);
            lblBuscarHistMembresia.TabIndex = 3;
            lblBuscarHistMembresia.Text = "Buscar:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            label4.Location = new Point(900, 16);
            label4.Name = "label4";
            label4.Size = new Size(285, 32);
            label4.TabIndex = 2;
            label4.Text = "HISTORIAL MEMBRESÍA";
            // 
            // dgvHistorial
            // 
            dgvHistorial.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            dgvHistorial.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvHistorial.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvHistorial.Location = new Point(20, 56);
            dgvHistorial.Name = "dgvHistorial";
            dgvHistorial.RowHeadersWidth = 51;
            dgvHistorial.Size = new Size(860, 494);
            dgvHistorial.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            label1.Location = new Point(20, 16);
            label1.Name = "label1";
            label1.Size = new Size(168, 32);
            label1.TabIndex = 0;
            label1.Text = "MEMBRESÍAS";
            // 
            // tabProductos
            // 
            tabProductos.BackColor = Color.White;
            tabProductos.Controls.Add(txtBuscarProductos);
            tabProductos.Controls.Add(label5);
            tabProductos.Controls.Add(dgvDetalleProductos);
            tabProductos.Controls.Add(label3);
            tabProductos.Controls.Add(dgvVentasProductos);
            tabProductos.Controls.Add(label2);
            tabProductos.Location = new Point(4, 34);
            tabProductos.Name = "tabProductos";
            tabProductos.Padding = new Padding(12);
            tabProductos.Size = new Size(1439, 583);
            tabProductos.TabIndex = 1;
            tabProductos.Text = "PRODUCTOS";
            // 
            // txtBuscarProductos
            // 
            txtBuscarProductos.Font = new Font("Segoe UI", 11F);
            txtBuscarProductos.Location = new Point(100, 52);
            txtBuscarProductos.Name = "txtBuscarProductos";
            txtBuscarProductos.PlaceholderText = "Producto, cliente, teléfono, fecha, total, método...";
            txtBuscarProductos.Size = new Size(780, 32);
            txtBuscarProductos.TabIndex = 5;
            txtBuscarProductos.TextChanged += txtBuscarProductos_TextChanged;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            label5.Location = new Point(20, 56);
            label5.Name = "label5";
            label5.Size = new Size(77, 25);
            label5.TabIndex = 4;
            label5.Text = "Buscar:";
            // 
            // dgvDetalleProductos
            // 
            dgvDetalleProductos.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvDetalleProductos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDetalleProductos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvDetalleProductos.Location = new Point(900, 96);
            dgvDetalleProductos.Name = "dgvDetalleProductos";
            dgvDetalleProductos.RowHeadersWidth = 51;
            dgvDetalleProductos.Size = new Size(516, 454);
            dgvDetalleProductos.TabIndex = 3;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            label3.Location = new Point(900, 16);
            label3.Name = "label3";
            label3.Size = new Size(299, 32);
            label3.TabIndex = 2;
            label3.Text = "DETALLE DE PRODUCTOS";
            // 
            // dgvVentasProductos
            // 
            dgvVentasProductos.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            dgvVentasProductos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvVentasProductos.Location = new Point(20, 96);
            dgvVentasProductos.Name = "dgvVentasProductos";
            dgvVentasProductos.RowHeadersWidth = 51;
            dgvVentasProductos.Size = new Size(860, 454);
            dgvVentasProductos.TabIndex = 1;
            dgvVentasProductos.SelectionChanged += dgvVentasProductos_SelectionChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            label2.Location = new Point(20, 16);
            label2.Name = "label2";
            label2.Size = new Size(157, 32);
            label2.TabIndex = 0;
            label2.Text = "PRODUCTOS";
            // 
            // FrmHistorialVentas
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1447, 673);
            Controls.Add(tabControl1);
            Controls.Add(panelNav);
            Name = "FrmHistorialVentas";
            Text = "Historial de ventas";
            WindowState = FormWindowState.Maximized;
            FormClosed += FrmHistorialVentas_FormClosed;
            Load += FrmHistorialVentas_Load;
            panelNav.ResumeLayout(false);
            tabControl1.ResumeLayout(false);
            tabMembresia.ResumeLayout(false);
            tabMembresia.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvHistorialMembresia).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvHistorial).EndInit();
            tabProductos.ResumeLayout(false);
            tabProductos.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvDetalleProductos).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvVentasProductos).EndInit();
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
        private TabControl tabControl1;
        private TabPage tabMembresia;
        private TabPage tabProductos;
        private DataGridView dgvHistorial;
        private DataGridView dgvDetalleProductos;
        private DataGridView dgvVentasProductos;
        private Label label3;
        private Label label2;
        private Label label1;
        private DataGridView dgvHistorialMembresia;
        private Label label4;
        private Label lblBuscarHistMembresia;
        private TextBox txtBuscarHistMembresia;
        private Label label5;
        private TextBox txtBuscarProductos;
    }
}
