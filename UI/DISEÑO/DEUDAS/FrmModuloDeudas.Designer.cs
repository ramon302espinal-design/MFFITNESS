namespace UI
{
    partial class FrmModuloDeudas
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                // Los hijos se liberan en OnFormClosing; aquí solo por seguridad.
                if (dashboardForm != null && !dashboardForm.IsDisposed)
                    dashboardForm.Dispose();
                if (gestionForm != null && !gestionForm.IsDisposed)
                    gestionForm.Dispose();
                if (crearForm != null && !crearForm.IsDisposed)
                    crearForm.Dispose();
                if (historialForm != null && !historialForm.IsDisposed)
                    historialForm.Dispose();
                dashboardForm = null;
                gestionForm = null;
                crearForm = null;
                historialForm = null;
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

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
            tabHistorial = new TabPage();
            layoutNavDeudas.SuspendLayout();
            panelNav.SuspendLayout();
            tabControl.SuspendLayout();
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
            btnNavClientes.Cursor = Cursors.Hand;
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
            btnNavReportes.Cursor = Cursors.Hand;
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
            btnNavInventario.Cursor = Cursors.Hand;
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
            btnNavHistorial.Cursor = Cursors.Hand;
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
            btnNavCaja.Cursor = Cursors.Hand;
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
            btnNavEstado.Cursor = Cursors.Hand;
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
            btnNavDeudas.Cursor = Cursors.Hand;
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
            btnNavPagar.Cursor = Cursors.Hand;
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
            btnBack.Cursor = Cursors.Hand;
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
            tabControl.ItemSize = new Size(220, 36);
            tabControl.Location = new Point(0, 52);
            tabControl.Margin = new Padding(0);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(1400, 748);
            tabControl.SizeMode = TabSizeMode.Fixed;
            tabControl.TabIndex = 1;
            tabControl.SelectedIndexChanged += tabControl_SelectedIndexChanged;
            // 
            // tabDashboard
            // 
            tabDashboard.BackColor = Color.White;
            tabDashboard.Location = new Point(4, 40);
            tabDashboard.Name = "tabDashboard";
            tabDashboard.Padding = new Padding(3);
            tabDashboard.Size = new Size(1392, 704);
            tabDashboard.TabIndex = 0;
            tabDashboard.Text = "Dashboard";
            // 
            // tabGestion
            // 
            tabGestion.BackColor = Color.White;
            tabGestion.Location = new Point(4, 40);
            tabGestion.Name = "tabGestion";
            tabGestion.Padding = new Padding(3);
            tabGestion.Size = new Size(1392, 704);
            tabGestion.TabIndex = 1;
            tabGestion.Text = "Gestión de Deudas";
            // 
            // tabCrear
            // 
            tabCrear.BackColor = Color.White;
            tabCrear.Location = new Point(4, 40);
            tabCrear.Name = "tabCrear";
            tabCrear.Padding = new Padding(3);
            tabCrear.Size = new Size(1392, 704);
            tabCrear.TabIndex = 2;
            tabCrear.Text = "Nueva Deuda";
            // 
            // tabHistorial
            // 
            tabHistorial.BackColor = Color.White;
            tabHistorial.Location = new Point(4, 40);
            tabHistorial.Name = "tabHistorial";
            tabHistorial.Padding = new Padding(3);
            tabHistorial.Size = new Size(1392, 704);
            tabHistorial.TabIndex = 3;
            tabHistorial.Text = "Historial";
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
    }
}
