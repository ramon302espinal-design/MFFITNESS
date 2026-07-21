namespace UI.DISEÑO
{
    partial class FrmPresentacion
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
            btnAgregarCliente = new Button();
            btnPagar = new Button();
            btnEstado = new Button();
            btnHistorial = new Button();
            btnProductoStock = new Button();
            panelDashboard = new Panel();
            panelClientesActivos = new Panel();
            lblClientesActivos = new Label();
            label1 = new Label();
            panelClientesVencidos = new Panel();
            lblClientesVencidos = new Label();
            label2 = new Label();
            panelIngresosHoy = new Panel();
            lblIngresosHoy = new Label();
            label3 = new Label();
            panelIngresosMes = new Panel();
            lblIngresosMes = new Label();
            label4 = new Label();
            panel1 = new Panel();
            btnDeudas = new Button();
            btnReportes = new Button();
            btnCerrarSesion = new Button();
            btnCaja = new Button();
            panelDashboard.SuspendLayout();
            panelClientesActivos.SuspendLayout();
            panelClientesVencidos.SuspendLayout();
            panelIngresosHoy.SuspendLayout();
            panelIngresosMes.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // btnAgregarCliente
            // 
            btnAgregarCliente.BackColor = SystemColors.Highlight;
            btnAgregarCliente.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            btnAgregarCliente.Location = new Point(8, 160);
            btnAgregarCliente.Name = "btnAgregarCliente";
            btnAgregarCliente.Size = new Size(250, 48);
            btnAgregarCliente.TabIndex = 0;
            btnAgregarCliente.Text = "CLIENTES";
            btnAgregarCliente.UseVisualStyleBackColor = false;
            btnAgregarCliente.Click += btnAgregarCliente_Click;
            // 
            // btnPagar
            // 
            btnPagar.BackColor = Color.FromArgb(22, 163, 74);
            btnPagar.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            btnPagar.ForeColor = Color.White;
            btnPagar.Location = new Point(8, 104);
            btnPagar.Name = "btnPagar";
            btnPagar.Size = new Size(250, 48);
            btnPagar.TabIndex = 1;
            btnPagar.Text = "COBRAR";
            btnPagar.UseVisualStyleBackColor = false;
            btnPagar.Click += btnPagar_Click;
            // 
            // btnEstado
            // 
            btnEstado.BackColor = SystemColors.AppWorkspace;
            btnEstado.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            btnEstado.Location = new Point(8, 328);
            btnEstado.Name = "btnEstado";
            btnEstado.Size = new Size(250, 48);
            btnEstado.TabIndex = 2;
            btnEstado.Text = "ESTADO";
            btnEstado.UseVisualStyleBackColor = false;
            btnEstado.Click += btnEstado_Click;
            // 
            // btnHistorial
            // 
            btnHistorial.BackColor = SystemColors.AppWorkspace;
            btnHistorial.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnHistorial.Location = new Point(8, 440);
            btnHistorial.Name = "btnHistorial";
            btnHistorial.Size = new Size(250, 48);
            btnHistorial.TabIndex = 3;
            btnHistorial.Text = "HISTORIAL DE PAGOS";
            btnHistorial.UseVisualStyleBackColor = false;
            btnHistorial.Click += btnHistorial_Click;
            // 
            // btnProductoStock
            // 
            btnProductoStock.BackColor = SystemColors.AppWorkspace;
            btnProductoStock.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            btnProductoStock.Location = new Point(8, 272);
            btnProductoStock.Name = "btnProductoStock";
            btnProductoStock.Size = new Size(250, 48);
            btnProductoStock.TabIndex = 4;
            btnProductoStock.Text = "INVENTARIO";
            btnProductoStock.UseVisualStyleBackColor = false;
            btnProductoStock.Click += btnProductoStock_Click;
            // 
            // panelDashboard
            // 
            panelDashboard.BackColor = Color.FromArgb(241, 245, 249);
            panelDashboard.Controls.Add(panelClientesActivos);
            panelDashboard.Controls.Add(panelClientesVencidos);
            panelDashboard.Controls.Add(panelIngresosHoy);
            panelDashboard.Controls.Add(panelIngresosMes);
            panelDashboard.Dock = DockStyle.Fill;
            panelDashboard.Location = new Point(268, 28);
            panelDashboard.Name = "panelDashboard";
            panelDashboard.Size = new Size(1557, 701);
            panelDashboard.TabIndex = 5;
            // 
            // panelClientesActivos
            // 
            panelClientesActivos.BackColor = Color.FromArgb(45, 45, 45, 50);
            panelClientesActivos.Controls.Add(lblClientesActivos);
            panelClientesActivos.Controls.Add(label1);
            panelClientesActivos.Location = new Point(40, 40);
            panelClientesActivos.Name = "panelClientesActivos";
            panelClientesActivos.Size = new Size(332, 205);
            panelClientesActivos.TabIndex = 2;
            // 
            // lblClientesActivos
            // 
            lblClientesActivos.AutoSize = true;
            lblClientesActivos.BackColor = Color.Transparent;
            lblClientesActivos.Font = new Font("Segoe UI", 28F, FontStyle.Bold);
            lblClientesActivos.ForeColor = Color.Lime;
            lblClientesActivos.Location = new Point(3, 42);
            lblClientesActivos.Name = "lblClientesActivos";
            lblClientesActivos.Size = new Size(54, 62);
            lblClientesActivos.TabIndex = 1;
            lblClientesActivos.Text = "0";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Lime;
            label1.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label1.ForeColor = Color.White;
            label1.Location = new Point(83, 10);
            label1.Name = "label1";
            label1.Size = new Size(163, 23);
            label1.TabIndex = 0;
            label1.Text = "CLIENTES ACTIVOS";
            // 
            // panelClientesVencidos
            // 
            panelClientesVencidos.BackColor = Color.FromArgb(45, 45, 45, 50);
            panelClientesVencidos.Controls.Add(lblClientesVencidos);
            panelClientesVencidos.Controls.Add(label2);
            panelClientesVencidos.Location = new Point(391, 40);
            panelClientesVencidos.Name = "panelClientesVencidos";
            panelClientesVencidos.Size = new Size(338, 205);
            panelClientesVencidos.TabIndex = 3;
            // 
            // lblClientesVencidos
            // 
            lblClientesVencidos.AutoSize = true;
            lblClientesVencidos.BackColor = Color.Transparent;
            lblClientesVencidos.Font = new Font("Segoe UI", 28F, FontStyle.Bold);
            lblClientesVencidos.ForeColor = Color.Red;
            lblClientesVencidos.Location = new Point(3, 42);
            lblClientesVencidos.Name = "lblClientesVencidos";
            lblClientesVencidos.Size = new Size(54, 62);
            lblClientesVencidos.TabIndex = 2;
            lblClientesVencidos.Text = "0";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.Red;
            label2.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label2.ForeColor = Color.White;
            label2.Location = new Point(78, 10);
            label2.Name = "label2";
            label2.Size = new Size(176, 23);
            label2.TabIndex = 2;
            label2.Text = "CLIENTES VENCIDOS";
            // 
            // panelIngresosHoy
            // 
            panelIngresosHoy.BackColor = Color.FromArgb(45, 45, 45, 50);
            panelIngresosHoy.Controls.Add(lblIngresosHoy);
            panelIngresosHoy.Controls.Add(label3);
            panelIngresosHoy.Location = new Point(747, 40);
            panelIngresosHoy.Name = "panelIngresosHoy";
            panelIngresosHoy.Size = new Size(344, 205);
            panelIngresosHoy.TabIndex = 4;
            panelIngresosHoy.Tag = "VER_INGRESOS";
            // 
            // lblIngresosHoy
            // 
            lblIngresosHoy.AutoSize = true;
            lblIngresosHoy.BackColor = Color.Transparent;
            lblIngresosHoy.Font = new Font("Segoe UI", 28F, FontStyle.Bold);
            lblIngresosHoy.ForeColor = Color.Blue;
            lblIngresosHoy.Location = new Point(4, 42);
            lblIngresosHoy.Name = "lblIngresosHoy";
            lblIngresosHoy.Size = new Size(54, 62);
            lblIngresosHoy.TabIndex = 4;
            lblIngresosHoy.Text = "0";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.BackColor = Color.Blue;
            label3.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label3.ForeColor = Color.White;
            label3.Location = new Point(108, 10);
            label3.Name = "label3";
            label3.Size = new Size(134, 23);
            label3.TabIndex = 3;
            label3.Text = "INGRESOS HOY";
            // 
            // panelIngresosMes
            // 
            panelIngresosMes.BackColor = Color.FromArgb(45, 45, 45, 50);
            panelIngresosMes.Controls.Add(lblIngresosMes);
            panelIngresosMes.Controls.Add(label4);
            panelIngresosMes.Location = new Point(1110, 40);
            panelIngresosMes.Name = "panelIngresosMes";
            panelIngresosMes.Size = new Size(400, 205);
            panelIngresosMes.TabIndex = 5;
            panelIngresosMes.Tag = "VER_INGRESOS";
            // 
            // lblIngresosMes
            // 
            lblIngresosMes.AutoSize = true;
            lblIngresosMes.BackColor = Color.Transparent;
            lblIngresosMes.Font = new Font("Segoe UI", 28F, FontStyle.Bold);
            lblIngresosMes.ForeColor = Color.Fuchsia;
            lblIngresosMes.Location = new Point(3, 42);
            lblIngresosMes.Name = "lblIngresosMes";
            lblIngresosMes.Size = new Size(54, 62);
            lblIngresosMes.TabIndex = 5;
            lblIngresosMes.Text = "0";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.BackColor = Color.Fuchsia;
            label4.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label4.ForeColor = Color.White;
            label4.Location = new Point(101, 10);
            label4.Name = "label4";
            label4.Size = new Size(179, 23);
            label4.TabIndex = 4;
            label4.Text = "INGRESOS MENSUAL";
            // 
            // panel1
            // 
            panel1.BackColor = Color.FromArgb(12, 12, 14);
            panel1.Controls.Add(btnDeudas);
            panel1.Controls.Add(btnReportes);
            panel1.Controls.Add(btnCerrarSesion);
            panel1.Controls.Add(btnCaja);
            panel1.Controls.Add(btnAgregarCliente);
            panel1.Controls.Add(btnEstado);
            panel1.Controls.Add(btnProductoStock);
            panel1.Controls.Add(btnPagar);
            panel1.Controls.Add(btnHistorial);
            panel1.Dock = DockStyle.Left;
            panel1.Location = new Point(0, 28);
            panel1.Name = "panel1";
            panel1.Size = new Size(268, 701);
            panel1.TabIndex = 6;
            // 
            // btnDeudas
            // 
            btnDeudas.BackColor = Color.Red;
            btnDeudas.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            btnDeudas.Location = new Point(8, 384);
            btnDeudas.Name = "btnDeudas";
            btnDeudas.Size = new Size(250, 48);
            btnDeudas.TabIndex = 7;
            btnDeudas.Text = "DEUDAS";
            btnDeudas.UseVisualStyleBackColor = false;
            btnDeudas.Click += btnDeudas_Click;
            // 
            // btnReportes
            // 
            btnReportes.BackColor = SystemColors.AppWorkspace;
            btnReportes.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            btnReportes.Location = new Point(8, 496);
            btnReportes.Name = "btnReportes";
            btnReportes.Size = new Size(250, 48);
            btnReportes.TabIndex = 6;
            btnReportes.Text = "REPORTES";
            btnReportes.UseVisualStyleBackColor = false;
            btnReportes.Click += btnReportes_Click;
            // 
            // btnCerrarSesion
            // 
            btnCerrarSesion.BackColor = SystemColors.AppWorkspace;
            btnCerrarSesion.Dock = DockStyle.Bottom;
            btnCerrarSesion.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            btnCerrarSesion.Location = new Point(0, 653);
            btnCerrarSesion.Name = "btnCerrarSesion";
            btnCerrarSesion.Size = new Size(268, 48);
            btnCerrarSesion.TabIndex = 5;
            btnCerrarSesion.Text = "SALIR";
            btnCerrarSesion.UseVisualStyleBackColor = false;
            btnCerrarSesion.Click += btnCerrarSesion_Click;
            // 
            // btnCaja
            // 
            btnCaja.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCaja.Location = new Point(8, 216);
            btnCaja.Name = "btnCaja";
            btnCaja.Size = new Size(250, 48);
            btnCaja.TabIndex = 0;
            btnCaja.Text = "CAJA";
            btnCaja.UseVisualStyleBackColor = true;
            btnCaja.Click += btnCaja_Click;
            // 
            // FrmPresentacion
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1280, 720);
            Controls.Add(panelDashboard);
            Controls.Add(panel1);
            Name = "FrmPresentacion";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "MFFITNESS";
            WindowState = FormWindowState.Normal;
            Load += FrmPresentacion_Load;
            panelDashboard.ResumeLayout(false);
            panelClientesActivos.ResumeLayout(false);
            panelClientesActivos.PerformLayout();
            panelClientesVencidos.ResumeLayout(false);
            panelClientesVencidos.PerformLayout();
            panelIngresosHoy.ResumeLayout(false);
            panelIngresosHoy.PerformLayout();
            panelIngresosMes.ResumeLayout(false);
            panelIngresosMes.PerformLayout();
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Button btnAgregarCliente;
        private Button btnPagar;
        private Button btnEstado;
        private Button btnHistorial;
        private Button btnProductoStock;
        private Panel panelDashboard;
        private Panel panelIngresosMes;
        private Panel panelIngresosHoy;
        private Panel panelClientesVencidos;
        private Panel panelClientesActivos;
        private Label label1;
        private Label lblClientesActivos;
        private Label label4;
        private Label label3;
        private Label lblClientesVencidos;
        private Label label2;
        private Label lblIngresosHoy;
        private Label lblIngresosMes;
        private Panel panel1;
        private Button btnCaja;
        private Button btnCerrarSesion;
        private Button btnReportes;
        private Button btnDeudas;
    }
}
