namespace UI
{
    partial class FrmDeudaDashboard
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
            panel1 = new Panel();
            panelIngresoPendiente = new Panel();
            lblIngresoPendiente = new Label();
            label3 = new Label();
            panelDeudasVencidas = new Panel();
            lblDeudasVencidas = new Label();
            label2 = new Label();
            panelDeudasActivas = new Panel();
            lblDeudasActivas = new Label();
            label1 = new Label();
            panel1.SuspendLayout();
            panelIngresoPendiente.SuspendLayout();
            panelDeudasVencidas.SuspendLayout();
            panelDeudasActivas.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = Color.White;
            panel1.Controls.Add(panelIngresoPendiente);
            panel1.Controls.Add(panelDeudasVencidas);
            panel1.Controls.Add(panelDeudasActivas);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(1563, 200);
            panel1.TabIndex = 0;
            // 
            // panelIngresoPendiente
            // 
            panelIngresoPendiente.BackColor = Color.FromArgb(45, 45, 45, 50);
            panelIngresoPendiente.Controls.Add(lblIngresoPendiente);
            panelIngresoPendiente.Controls.Add(label3);
            panelIngresoPendiente.Location = new Point(748, 36);
            panelIngresoPendiente.Name = "panelIngresoPendiente";
            panelIngresoPendiente.Size = new Size(250, 125);
            panelIngresoPendiente.TabIndex = 1;
            // 
            // lblIngresoPendiente
            // 
            lblIngresoPendiente.AutoSize = true;
            lblIngresoPendiente.BackColor = Color.Transparent;
            lblIngresoPendiente.Font = new Font("Segoe UI", 25F, FontStyle.Bold);
            lblIngresoPendiente.ForeColor = Color.ForestGreen;
            lblIngresoPendiente.Location = new Point(4, 53);
            lblIngresoPendiente.Name = "lblIngresoPendiente";
            lblIngresoPendiente.Size = new Size(49, 57);
            lblIngresoPendiente.TabIndex = 3;
            lblIngresoPendiente.Text = "0";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.BackColor = Color.ForestGreen;
            label3.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label3.ForeColor = Color.White;
            label3.Location = new Point(17, 7);
            label3.Name = "label3";
            label3.Size = new Size(214, 28);
            label3.TabIndex = 2;
            label3.Text = "INGRESO PENDIENTE";
            // 
            // panelDeudasVencidas
            // 
            panelDeudasVencidas.BackColor = Color.FromArgb(45, 45, 45, 50);
            panelDeudasVencidas.Controls.Add(lblDeudasVencidas);
            panelDeudasVencidas.Controls.Add(label2);
            panelDeudasVencidas.Location = new Point(442, 36);
            panelDeudasVencidas.Name = "panelDeudasVencidas";
            panelDeudasVencidas.Size = new Size(250, 125);
            panelDeudasVencidas.TabIndex = 1;
            // 
            // lblDeudasVencidas
            // 
            lblDeudasVencidas.AutoSize = true;
            lblDeudasVencidas.BackColor = Color.Transparent;
            lblDeudasVencidas.Font = new Font("Segoe UI", 25F, FontStyle.Bold);
            lblDeudasVencidas.ForeColor = Color.Red;
            lblDeudasVencidas.Location = new Point(4, 53);
            lblDeudasVencidas.Name = "lblDeudasVencidas";
            lblDeudasVencidas.Size = new Size(49, 57);
            lblDeudasVencidas.TabIndex = 2;
            lblDeudasVencidas.Text = "0";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.Red;
            label2.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label2.ForeColor = Color.White;
            label2.Location = new Point(27, 8);
            label2.Name = "label2";
            label2.Size = new Size(196, 28);
            label2.TabIndex = 1;
            label2.Text = "DEUDAS VENCIDAS";
            // 
            // panelDeudasActivas
            // 
            panelDeudasActivas.BackColor = Color.FromArgb(45, 45, 45, 50);
            panelDeudasActivas.Controls.Add(lblDeudasActivas);
            panelDeudasActivas.Controls.Add(label1);
            panelDeudasActivas.Location = new Point(137, 36);
            panelDeudasActivas.Name = "panelDeudasActivas";
            panelDeudasActivas.Size = new Size(250, 125);
            panelDeudasActivas.TabIndex = 0;
            // 
            // lblDeudasActivas
            // 
            lblDeudasActivas.AutoSize = true;
            lblDeudasActivas.BackColor = Color.Transparent;
            lblDeudasActivas.Font = new Font("Segoe UI", 25F, FontStyle.Bold);
            lblDeudasActivas.ForeColor = Color.DarkViolet;
            lblDeudasActivas.Location = new Point(4, 53);
            lblDeudasActivas.Name = "lblDeudasActivas";
            lblDeudasActivas.Size = new Size(49, 57);
            lblDeudasActivas.TabIndex = 1;
            lblDeudasActivas.Text = "0";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.DarkViolet;
            label1.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label1.ForeColor = SystemColors.ControlLightLight;
            label1.Location = new Point(32, 10);
            label1.Name = "label1";
            label1.Size = new Size(179, 28);
            label1.TabIndex = 0;
            label1.Text = "DEUDAS ACTIVAS";
            // 
            // FrmDeudaDashboard
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ControlDark;
            ClientSize = new Size(1563, 673);
            Controls.Add(panel1);
            Name = "FrmDeudaDashboard";
            Text = "DEUDAS ";
            WindowState = FormWindowState.Maximized;
            Load += FrmDeudaDashboard_Load;
            panel1.ResumeLayout(false);
            panelIngresoPendiente.ResumeLayout(false);
            panelIngresoPendiente.PerformLayout();
            panelDeudasVencidas.ResumeLayout(false);
            panelDeudasVencidas.PerformLayout();
            panelDeudasActivas.ResumeLayout(false);
            panelDeudasActivas.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Panel panelIngresoPendiente;
        private Panel panelDeudasVencidas;
        private Panel panelDeudasActivas;
        private Label label2;
        private Label label1;
        private Label label3;
        private Label lblDeudasActivas;
        private Label lblIngresoPendiente;
        private Label lblDeudasVencidas;
        private Label label4;
    }
}
