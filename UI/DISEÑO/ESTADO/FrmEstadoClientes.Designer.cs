namespace UI.DISEÑO
{
    partial class FrmEstadoClientes
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
            panelBusqueda = new Panel();
            lblBuscar = new Label();
            txtBuscar = new TextBox();
            panelAcciones = new Panel();
            btnRenovar = new Button();
            btnDesactivar = new Button();
            btnCongelar = new Button();
            dgvEstado = new DataGridView();
            btnAtras = new Button();
            panelNav.SuspendLayout();
            panelBusqueda.SuspendLayout();
            panelAcciones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvEstado).BeginInit();
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
            // panelBusqueda
            // 
            panelBusqueda.Controls.Add(lblBuscar);
            panelBusqueda.Controls.Add(txtBuscar);
            panelBusqueda.Dock = DockStyle.Top;
            panelBusqueda.Location = new Point(0, 52);
            panelBusqueda.Name = "panelBusqueda";
            panelBusqueda.Padding = new Padding(12, 8, 12, 8);
            panelBusqueda.Size = new Size(1062, 56);
            panelBusqueda.TabIndex = 1;
            // 
            // lblBuscar
            // 
            lblBuscar.AutoSize = true;
            lblBuscar.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblBuscar.Location = new Point(16, 12);
            lblBuscar.Name = "lblBuscar";
            lblBuscar.Size = new Size(100, 35);
            lblBuscar.TabIndex = 0;
            lblBuscar.Text = "Buscar:";
            // 
            // txtBuscar
            // 
            txtBuscar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtBuscar.Font = new Font("Segoe UI", 15F);
            txtBuscar.Location = new Point(122, 10);
            txtBuscar.Name = "txtBuscar";
            txtBuscar.PlaceholderText = "Nombre, plan, estado...";
            txtBuscar.Size = new Size(920, 41);
            txtBuscar.TabIndex = 1;
            txtBuscar.TabStop = false;
            txtBuscar.TextChanged += txtBuscar_TextChanged;
            // 
            // panelAcciones
            // 
            panelAcciones.Controls.Add(btnRenovar);
            panelAcciones.Controls.Add(btnDesactivar);
            panelAcciones.Controls.Add(btnCongelar);
            panelAcciones.Dock = DockStyle.Bottom;
            panelAcciones.Location = new Point(0, 609);
            panelAcciones.Name = "panelAcciones";
            panelAcciones.Padding = new Padding(12, 8, 12, 8);
            panelAcciones.Size = new Size(1062, 64);
            panelAcciones.TabIndex = 3;
            panelAcciones.Tag = "classic";
            // 
            // btnRenovar
            // 
            btnRenovar.Anchor = AnchorStyles.None;
            btnRenovar.Enabled = false;
            btnRenovar.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnRenovar.Location = new Point(300, 11);
            btnRenovar.Name = "btnRenovar";
            btnRenovar.Size = new Size(142, 42);
            btnRenovar.TabIndex = 0;
            btnRenovar.Text = "RENOVAR";
            btnRenovar.UseVisualStyleBackColor = true;
            btnRenovar.Click += btnRenovar_Click;
            // 
            // btnDesactivar
            // 
            btnDesactivar.Anchor = AnchorStyles.None;
            btnDesactivar.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnDesactivar.Location = new Point(462, 11);
            btnDesactivar.Name = "btnDesactivar";
            btnDesactivar.Size = new Size(126, 42);
            btnDesactivar.TabIndex = 1;
            btnDesactivar.Text = "DESACTIVAR";
            btnDesactivar.UseVisualStyleBackColor = true;
            btnDesactivar.Click += btnDesactivar_Click;
            // 
            // btnCongelar
            // 
            btnCongelar.Anchor = AnchorStyles.None;
            btnCongelar.Enabled = false;
            btnCongelar.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnCongelar.Location = new Point(608, 11);
            btnCongelar.Name = "btnCongelar";
            btnCongelar.Size = new Size(126, 42);
            btnCongelar.TabIndex = 2;
            btnCongelar.Text = "CONGELAR";
            btnCongelar.UseVisualStyleBackColor = true;
            btnCongelar.Click += btnCongelar_Click;
            // 
            // dgvEstado
            // 
            dgvEstado.AllowUserToAddRows = false;
            dgvEstado.AllowUserToDeleteRows = false;
            dgvEstado.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvEstado.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvEstado.Location = new Point(0, 108);
            dgvEstado.MultiSelect = false;
            dgvEstado.Name = "dgvEstado";
            dgvEstado.ReadOnly = true;
            dgvEstado.RowHeadersWidth = 51;
            dgvEstado.RowTemplate.Height = 32;
            dgvEstado.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvEstado.Size = new Size(1062, 342);
            dgvEstado.TabIndex = 2;
            dgvEstado.CellDoubleClick += dgvEstado_CellDoubleClick;
            dgvEstado.CellFormatting += dgvEstado_CellFormatting;
            dgvEstado.SelectionChanged += dgvEstado_SelectionChanged;
            // 
            // btnAtras
            // 
            btnAtras.BackColor = Color.White;
            btnAtras.Cursor = Cursors.Hand;
            btnAtras.FlatAppearance.BorderColor = Color.FromArgb(27, 146, 255);
            btnAtras.FlatAppearance.MouseOverBackColor = Color.FromArgb(235, 245, 255);
            btnAtras.FlatStyle = FlatStyle.Flat;
            btnAtras.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnAtras.ForeColor = Color.FromArgb(27, 146, 255);
            btnAtras.Location = new Point(8, 1);
            btnAtras.Name = "btnAtras";
            btnAtras.Size = new Size(44, 45);
            btnAtras.TabIndex = 20;
            btnAtras.Text = "◀️";
            btnAtras.UseVisualStyleBackColor = false;
            btnAtras.Visible = false;
            // 
            // FrmEstadoClientes
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1062, 673);
            Controls.Add(dgvEstado);
            Controls.Add(panelAcciones);
            Controls.Add(panelBusqueda);
            Controls.Add(panelNav);
            Controls.Add(btnAtras);
            Name = "FrmEstadoClientes";
            Text = "ESTADO Y RENOVACION";
            WindowState = FormWindowState.Maximized;
            Load += FrmEstadoClientes_Load;
            panelNav.ResumeLayout(false);
            panelBusqueda.ResumeLayout(false);
            panelBusqueda.PerformLayout();
            panelAcciones.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvEstado).EndInit();
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
        private Panel panelBusqueda;
        private Panel panelAcciones;
        private DataGridView dgvEstado;
        private Button btnAtras;
        private Button btnRenovar;
        private Button btnDesactivar;
        private Button btnCongelar;
        private Label lblBuscar;
        private TextBox txtBuscar;
    }
}
