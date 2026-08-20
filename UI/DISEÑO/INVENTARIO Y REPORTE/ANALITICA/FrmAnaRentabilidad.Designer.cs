namespace UI
{
    partial class FrmAnaRentabilidad
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            panelHeaderLocal = new Panel();
            lblHeaderLocal = new Label();
            panelScroll = new Panel();
            pnlReservavisual = new Panel();
            lblReservavisualTitle = new Label();
            lblReservavisualValue = new Label();
            lblReservavisualDesc = new Label();
            pnlSinusoensidebar = new Panel();
            lblSinusoensidebarTitle = new Label();
            lblSinusoensidebarValue = new Label();
            lblSinusoensidebarDesc = new Label();
            panelHeaderLocal.SuspendLayout();
            panelScroll.SuspendLayout();
            pnlReservavisual.SuspendLayout();
            pnlSinusoensidebar.SuspendLayout();
            SuspendLayout();
            // 
            // panelHeaderLocal
            // 
            panelHeaderLocal.BackColor = Color.White;
            panelHeaderLocal.BorderStyle = BorderStyle.FixedSingle;
            panelHeaderLocal.Controls.Add(lblHeaderLocal);
            panelHeaderLocal.Dock = DockStyle.Top;
            panelHeaderLocal.Location = new Point(0, 0);
            panelHeaderLocal.Name = "panelHeaderLocal";
            panelHeaderLocal.Size = new Size(940, 48);
            panelHeaderLocal.TabIndex = 0;
            // 
            // lblHeaderLocal
            // 
            lblHeaderLocal.AutoSize = true;
            lblHeaderLocal.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblHeaderLocal.ForeColor = Color.FromArgb(26, 32, 44);
            lblHeaderLocal.Location = new Point(14, 12);
            lblHeaderLocal.Name = "lblHeaderLocal";
            lblHeaderLocal.Size = new Size(100, 28);
            lblHeaderLocal.TabIndex = 0;
            lblHeaderLocal.Text = "Rentabilidad (reserva)";
            // 
            // panelScroll
            // 
            panelScroll.AutoScroll = true;
            panelScroll.BackColor = Color.FromArgb(247, 249, 252);
            panelScroll.Controls.Add(pnlReservavisual);
            panelScroll.Controls.Add(pnlSinusoensidebar);
            panelScroll.Dock = DockStyle.Fill;
            panelScroll.Location = new Point(0, 48);
            panelScroll.Name = "panelScroll";
            panelScroll.Padding = new Padding(8);
            panelScroll.Size = new Size(940, 552);
            panelScroll.TabIndex = 1;
            // 
            // pnlReservavisual
            // 
            pnlReservavisual.BackColor = Color.White;
            pnlReservavisual.BorderStyle = BorderStyle.FixedSingle;
            pnlReservavisual.Controls.Add(lblReservavisualDesc);
            pnlReservavisual.Controls.Add(lblReservavisualValue);
            pnlReservavisual.Controls.Add(lblReservavisualTitle);
            pnlReservavisual.Location = new Point(16, 16);
            pnlReservavisual.Name = "pnlReservavisual";
            pnlReservavisual.Size = new Size(900, 110);
            pnlReservavisual.TabIndex = 0;
            pnlReservavisual.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblReservavisualTitle
            // 
            lblReservavisualTitle.AutoSize = true;
            lblReservavisualTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblReservavisualTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblReservavisualTitle.Location = new Point(14, 12);
            lblReservavisualTitle.Name = "lblReservavisualTitle";
            lblReservavisualTitle.Size = new Size(120, 23);
            lblReservavisualTitle.TabIndex = 0;
            lblReservavisualTitle.Text = "Reserva visual";
            // 
            // lblReservavisualValue
            // 
            lblReservavisualValue.AutoSize = true;
            lblReservavisualValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblReservavisualValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblReservavisualValue.Location = new Point(14, 42);
            lblReservavisualValue.Name = "lblReservavisualValue";
            lblReservavisualValue.Size = new Size(120, 41);
            lblReservavisualValue.TabIndex = 1;
            lblReservavisualValue.Text = "RD$ 0.00";
            // 
            // lblReservavisualDesc
            // 
            lblReservavisualDesc.AutoSize = true;
            lblReservavisualDesc.Font = new Font("Segoe UI", 8.5F);
            lblReservavisualDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblReservavisualDesc.Location = new Point(16, 84);
            lblReservavisualDesc.Name = "lblReservavisualDesc";
            lblReservavisualDesc.Size = new Size(180, 19);
            lblReservavisualDesc.TabIndex = 2;
            lblReservavisualDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlSinusoensidebar
            // 
            pnlSinusoensidebar.BackColor = Color.White;
            pnlSinusoensidebar.BorderStyle = BorderStyle.FixedSingle;
            pnlSinusoensidebar.Controls.Add(lblSinusoensidebarDesc);
            pnlSinusoensidebar.Controls.Add(lblSinusoensidebarValue);
            pnlSinusoensidebar.Controls.Add(lblSinusoensidebarTitle);
            pnlSinusoensidebar.Location = new Point(16, 142);
            pnlSinusoensidebar.Name = "pnlSinusoensidebar";
            pnlSinusoensidebar.Size = new Size(900, 110);
            pnlSinusoensidebar.TabIndex = 1;
            pnlSinusoensidebar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblSinusoensidebarTitle
            // 
            lblSinusoensidebarTitle.AutoSize = true;
            lblSinusoensidebarTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblSinusoensidebarTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblSinusoensidebarTitle.Location = new Point(14, 12);
            lblSinusoensidebarTitle.Name = "lblSinusoensidebarTitle";
            lblSinusoensidebarTitle.Size = new Size(120, 23);
            lblSinusoensidebarTitle.TabIndex = 0;
            lblSinusoensidebarTitle.Text = "Sin uso en sidebar";
            // 
            // lblSinusoensidebarValue
            // 
            lblSinusoensidebarValue.AutoSize = true;
            lblSinusoensidebarValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblSinusoensidebarValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblSinusoensidebarValue.Location = new Point(14, 42);
            lblSinusoensidebarValue.Name = "lblSinusoensidebarValue";
            lblSinusoensidebarValue.Size = new Size(120, 41);
            lblSinusoensidebarValue.TabIndex = 1;
            lblSinusoensidebarValue.Text = "—";
            // 
            // lblSinusoensidebarDesc
            // 
            lblSinusoensidebarDesc.AutoSize = true;
            lblSinusoensidebarDesc.Font = new Font("Segoe UI", 8.5F);
            lblSinusoensidebarDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblSinusoensidebarDesc.Location = new Point(16, 84);
            lblSinusoensidebarDesc.Name = "lblSinusoensidebarDesc";
            lblSinusoensidebarDesc.Size = new Size(180, 19);
            lblSinusoensidebarDesc.TabIndex = 2;
            lblSinusoensidebarDesc.Text = "Dato visual mock — sin logica";
            // 
            // FrmAnaRentabilidad
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(940, 600);
            Controls.Add(panelScroll);
            Controls.Add(panelHeaderLocal);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FrmAnaRentabilidad";
            Text = "Rentabilidad (reserva)";
            panelHeaderLocal.ResumeLayout(false);
            panelHeaderLocal.PerformLayout();
            panelScroll.ResumeLayout(false);
            pnlReservavisual.ResumeLayout(false);
            pnlReservavisual.PerformLayout();
            pnlSinusoensidebar.ResumeLayout(false);
            pnlSinusoensidebar.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelHeaderLocal;
        private Label lblHeaderLocal;
        private Panel panelScroll;
        private Panel pnlReservavisual;
        private Label lblReservavisualTitle;
        private Label lblReservavisualValue;
        private Label lblReservavisualDesc;
        private Panel pnlSinusoensidebar;
        private Label lblSinusoensidebarTitle;
        private Label lblSinusoensidebarValue;
        private Label lblSinusoensidebarDesc;
    }
}
