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
                crearForm?.Dispose();
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
            btnBack = new Button();
            btnNavPagar = new Button();
            btnNavDeudas = new Button();
            btnNavEstado = new Button();
            btnNavCaja = new Button();
            btnNavHistorial = new Button();
            btnNavInventario = new Button();
            btnNavReportes = new Button();
            btnNavClientes = new Button();
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
            layoutNavDeudas.Location = new System.Drawing.Point(0, 0);
            layoutNavDeudas.Margin = new Padding(0);
            layoutNavDeudas.Name = "layoutNavDeudas";
            layoutNavDeudas.Padding = new Padding(0);
            layoutNavDeudas.RowCount = 2;
            layoutNavDeudas.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
            layoutNavDeudas.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutNavDeudas.Size = new System.Drawing.Size(1400, 800);
            layoutNavDeudas.TabIndex = 0;
            // 
            // panelNav
            // 
            panelNav.BackColor = System.Drawing.Color.White;
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
            panelNav.Location = new System.Drawing.Point(0, 0);
            panelNav.Margin = new Padding(0);
            panelNav.Name = "panelNav";
            panelNav.Size = new System.Drawing.Size(1400, 52);
            panelNav.TabIndex = 0;
            // 
            // btnBack
            // 
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.FlatStyle = FlatStyle.Flat;
            btnBack.Font = new System.Drawing.Font("Segoe UI", 11F);
            btnBack.Location = new System.Drawing.Point(8, 8);
            btnBack.Name = "btnBack";
            btnBack.Size = new System.Drawing.Size(43, 35);
            btnBack.TabIndex = 0;
            btnBack.Text = "";
            btnBack.UseVisualStyleBackColor = true;
            // 
            // btnNavPagar
            // 
            btnNavPagar.BackColor = Color.FromArgb(22, 163, 74);
            btnNavPagar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnNavPagar.ForeColor = Color.White;
            btnNavPagar.Location = new System.Drawing.Point(60, 10);
            btnNavPagar.Name = "btnNavPagar";
            btnNavPagar.Size = new System.Drawing.Size(110, 32);
            btnNavPagar.TabIndex = 1;
            btnNavPagar.Text = "COBRAR";
            btnNavPagar.UseVisualStyleBackColor = false;
            // 
            // btnNavDeudas
            // 
            btnNavDeudas.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnNavDeudas.Location = new System.Drawing.Point(180, 10);
            btnNavDeudas.Name = "btnNavDeudas";
            btnNavDeudas.Size = new System.Drawing.Size(110, 32);
            btnNavDeudas.TabIndex = 2;
            btnNavDeudas.Text = "DEUDAS";
            btnNavDeudas.UseVisualStyleBackColor = true;
            // 
            // btnNavEstado
            // 
            btnNavEstado.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnNavEstado.Location = new System.Drawing.Point(300, 10);
            btnNavEstado.Name = "btnNavEstado";
            btnNavEstado.Size = new System.Drawing.Size(110, 32);
            btnNavEstado.TabIndex = 3;
            btnNavEstado.Text = "ESTADO";
            btnNavEstado.UseVisualStyleBackColor = true;
            // 
            // btnNavCaja
            // 
            btnNavCaja.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnNavCaja.Location = new System.Drawing.Point(420, 10);
            btnNavCaja.Name = "btnNavCaja";
            btnNavCaja.Size = new System.Drawing.Size(110, 32);
            btnNavCaja.TabIndex = 4;
            btnNavCaja.Text = "CAJA";
            btnNavCaja.UseVisualStyleBackColor = true;
            // 
            // btnNavHistorial
            // 
            btnNavHistorial.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnNavHistorial.Location = new System.Drawing.Point(540, 10);
            btnNavHistorial.Name = "btnNavHistorial";
            btnNavHistorial.Size = new System.Drawing.Size(120, 32);
            btnNavHistorial.TabIndex = 5;
            btnNavHistorial.Text = "HISTORIAL";
            btnNavHistorial.UseVisualStyleBackColor = true;
            // 
            // btnNavInventario
            // 
            btnNavInventario.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnNavInventario.Location = new System.Drawing.Point(670, 10);
            btnNavInventario.Name = "btnNavInventario";
            btnNavInventario.Size = new System.Drawing.Size(130, 32);
            btnNavInventario.TabIndex = 6;
            btnNavInventario.Text = "INVENTARIO";
            btnNavInventario.UseVisualStyleBackColor = true;
            // 
            // btnNavReportes
            // 
            btnNavReportes.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnNavReportes.Location = new System.Drawing.Point(810, 10);
            btnNavReportes.Name = "btnNavReportes";
            btnNavReportes.Size = new System.Drawing.Size(120, 32);
            btnNavReportes.TabIndex = 7;
            btnNavReportes.Text = "REPORTES";
            btnNavReportes.UseVisualStyleBackColor = true;
            // 
            // btnNavClientes
            // 
            btnNavClientes.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnNavClientes.Location = new System.Drawing.Point(940, 10);
            btnNavClientes.Name = "btnNavClientes";
            btnNavClientes.Size = new System.Drawing.Size(120, 32);
            btnNavClientes.TabIndex = 8;
            btnNavClientes.Text = "CLIENTES";
            btnNavClientes.UseVisualStyleBackColor = true;
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabDashboard);
            tabControl.Controls.Add(tabGestion);
            tabControl.Controls.Add(tabCrear);
            tabControl.Controls.Add(tabHistorial);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            tabControl.Location = new System.Drawing.Point(0, 52);
            tabControl.Margin = new Padding(0);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new System.Drawing.Size(1400, 748);
            tabControl.TabIndex = 1;
            tabControl.SelectedIndexChanged += tabControl_SelectedIndexChanged;
            // 
            // tabDashboard
            // 
            tabDashboard.BackColor = System.Drawing.Color.White;
            tabDashboard.Location = new System.Drawing.Point(4, 44);
            tabDashboard.Name = "tabDashboard";
            tabDashboard.Padding = new Padding(3);
            tabDashboard.Size = new System.Drawing.Size(1392, 700);
            tabDashboard.TabIndex = 0;
            tabDashboard.Text = "📊 Dashboard";
            // 
            // tabGestion
            // 
            tabGestion.BackColor = System.Drawing.Color.White;
            tabGestion.Location = new System.Drawing.Point(4, 44);
            tabGestion.Name = "tabGestion";
            tabGestion.Padding = new Padding(3);
            tabGestion.Size = new System.Drawing.Size(1392, 700);
            tabGestion.TabIndex = 1;
            tabGestion.Text = "📋 Gestión de Deudas";
            // 
            // tabCrear
            // 
            tabCrear.BackColor = System.Drawing.Color.White;
            tabCrear.Location = new System.Drawing.Point(4, 44);
            tabCrear.Name = "tabCrear";
            tabCrear.Padding = new Padding(3);
            tabCrear.Size = new System.Drawing.Size(1392, 700);
            tabCrear.TabIndex = 2;
            tabCrear.Text = "➕ Nueva Deuda";
            // 
            // tabHistorial
            // 
            tabHistorial.BackColor = System.Drawing.Color.White;
            tabHistorial.Location = new System.Drawing.Point(4, 44);
            tabHistorial.Name = "tabHistorial";
            tabHistorial.Padding = new Padding(3);
            tabHistorial.Size = new System.Drawing.Size(1392, 700);
            tabHistorial.TabIndex = 3;
            tabHistorial.Text = "📜 Historial";
            // 
            // FrmModuloDeudas
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = System.Drawing.Color.White;
            ClientSize = new System.Drawing.Size(1400, 800);
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
