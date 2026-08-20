namespace UI
{
    partial class FrmAnaInversiones
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
            pnlResumen = new Panel();
            lblResumenTitle = new Label();
            lblResumenValue = new Label();
            lblResumenDesc = new Label();
            pnlInversionesactivas = new Panel();
            lblInversionesactivasTitle = new Label();
            lblInversionesactivasValue = new Label();
            lblInversionesactivasDesc = new Label();
            pnlInversionescerradas = new Panel();
            lblInversionescerradasTitle = new Label();
            lblInversionescerradasValue = new Label();
            lblInversionescerradasDesc = new Label();
            pnlHistorial = new Panel();
            lblHistorialTitle = new Label();
            lblHistorialValue = new Label();
            lblHistorialDesc = new Label();
            pnlDetalle = new Panel();
            lblDetalleTitle = new Label();
            lblDetalleValue = new Label();
            lblDetalleDesc = new Label();
            panelHeaderLocal.SuspendLayout();
            panelScroll.SuspendLayout();
            pnlResumen.SuspendLayout();
            pnlInversionesactivas.SuspendLayout();
            pnlInversionescerradas.SuspendLayout();
            pnlHistorial.SuspendLayout();
            pnlDetalle.SuspendLayout();
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
            lblHeaderLocal.Text = "Inversiones";
            // 
            // panelScroll
            // 
            panelScroll.AutoScroll = true;
            panelScroll.BackColor = Color.FromArgb(247, 249, 252);
            panelScroll.Controls.Add(pnlResumen);
            panelScroll.Controls.Add(pnlInversionesactivas);
            panelScroll.Controls.Add(pnlInversionescerradas);
            panelScroll.Controls.Add(pnlHistorial);
            panelScroll.Controls.Add(pnlDetalle);
            panelScroll.Dock = DockStyle.Fill;
            panelScroll.Location = new Point(0, 48);
            panelScroll.Name = "panelScroll";
            panelScroll.Padding = new Padding(8);
            panelScroll.Size = new Size(940, 552);
            panelScroll.TabIndex = 1;
            // 
            // pnlResumen
            // 
            pnlResumen.BackColor = Color.White;
            pnlResumen.BorderStyle = BorderStyle.FixedSingle;
            pnlResumen.Controls.Add(lblResumenDesc);
            pnlResumen.Controls.Add(lblResumenValue);
            pnlResumen.Controls.Add(lblResumenTitle);
            pnlResumen.Location = new Point(16, 16);
            pnlResumen.Name = "pnlResumen";
            pnlResumen.Size = new Size(900, 110);
            pnlResumen.TabIndex = 0;
            pnlResumen.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblResumenTitle
            // 
            lblResumenTitle.AutoSize = true;
            lblResumenTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblResumenTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblResumenTitle.Location = new Point(14, 12);
            lblResumenTitle.Name = "lblResumenTitle";
            lblResumenTitle.Size = new Size(120, 23);
            lblResumenTitle.TabIndex = 0;
            lblResumenTitle.Text = "Resumen";
            // 
            // lblResumenValue
            // 
            lblResumenValue.AutoSize = true;
            lblResumenValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblResumenValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblResumenValue.Location = new Point(14, 42);
            lblResumenValue.Name = "lblResumenValue";
            lblResumenValue.Size = new Size(120, 41);
            lblResumenValue.TabIndex = 1;
            lblResumenValue.Text = "RD$ 0.00";
            // 
            // lblResumenDesc
            // 
            lblResumenDesc.AutoSize = true;
            lblResumenDesc.Font = new Font("Segoe UI", 8.5F);
            lblResumenDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblResumenDesc.Location = new Point(16, 84);
            lblResumenDesc.Name = "lblResumenDesc";
            lblResumenDesc.Size = new Size(180, 19);
            lblResumenDesc.TabIndex = 2;
            lblResumenDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlInversionesactivas
            // 
            pnlInversionesactivas.BackColor = Color.White;
            pnlInversionesactivas.BorderStyle = BorderStyle.FixedSingle;
            pnlInversionesactivas.Controls.Add(lblInversionesactivasDesc);
            pnlInversionesactivas.Controls.Add(lblInversionesactivasValue);
            pnlInversionesactivas.Controls.Add(lblInversionesactivasTitle);
            pnlInversionesactivas.Location = new Point(16, 142);
            pnlInversionesactivas.Name = "pnlInversionesactivas";
            pnlInversionesactivas.Size = new Size(900, 110);
            pnlInversionesactivas.TabIndex = 1;
            pnlInversionesactivas.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblInversionesactivasTitle
            // 
            lblInversionesactivasTitle.AutoSize = true;
            lblInversionesactivasTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblInversionesactivasTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblInversionesactivasTitle.Location = new Point(14, 12);
            lblInversionesactivasTitle.Name = "lblInversionesactivasTitle";
            lblInversionesactivasTitle.Size = new Size(120, 23);
            lblInversionesactivasTitle.TabIndex = 0;
            lblInversionesactivasTitle.Text = "Inversiones activas";
            // 
            // lblInversionesactivasValue
            // 
            lblInversionesactivasValue.AutoSize = true;
            lblInversionesactivasValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblInversionesactivasValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblInversionesactivasValue.Location = new Point(14, 42);
            lblInversionesactivasValue.Name = "lblInversionesactivasValue";
            lblInversionesactivasValue.Size = new Size(120, 41);
            lblInversionesactivasValue.TabIndex = 1;
            lblInversionesactivasValue.Text = "—";
            // 
            // lblInversionesactivasDesc
            // 
            lblInversionesactivasDesc.AutoSize = true;
            lblInversionesactivasDesc.Font = new Font("Segoe UI", 8.5F);
            lblInversionesactivasDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblInversionesactivasDesc.Location = new Point(16, 84);
            lblInversionesactivasDesc.Name = "lblInversionesactivasDesc";
            lblInversionesactivasDesc.Size = new Size(180, 19);
            lblInversionesactivasDesc.TabIndex = 2;
            lblInversionesactivasDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlInversionescerradas
            // 
            pnlInversionescerradas.BackColor = Color.White;
            pnlInversionescerradas.BorderStyle = BorderStyle.FixedSingle;
            pnlInversionescerradas.Controls.Add(lblInversionescerradasDesc);
            pnlInversionescerradas.Controls.Add(lblInversionescerradasValue);
            pnlInversionescerradas.Controls.Add(lblInversionescerradasTitle);
            pnlInversionescerradas.Location = new Point(16, 268);
            pnlInversionescerradas.Name = "pnlInversionescerradas";
            pnlInversionescerradas.Size = new Size(900, 110);
            pnlInversionescerradas.TabIndex = 2;
            pnlInversionescerradas.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblInversionescerradasTitle
            // 
            lblInversionescerradasTitle.AutoSize = true;
            lblInversionescerradasTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblInversionescerradasTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblInversionescerradasTitle.Location = new Point(14, 12);
            lblInversionescerradasTitle.Name = "lblInversionescerradasTitle";
            lblInversionescerradasTitle.Size = new Size(120, 23);
            lblInversionescerradasTitle.TabIndex = 0;
            lblInversionescerradasTitle.Text = "Inversiones cerradas";
            // 
            // lblInversionescerradasValue
            // 
            lblInversionescerradasValue.AutoSize = true;
            lblInversionescerradasValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblInversionescerradasValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblInversionescerradasValue.Location = new Point(14, 42);
            lblInversionescerradasValue.Name = "lblInversionescerradasValue";
            lblInversionescerradasValue.Size = new Size(120, 41);
            lblInversionescerradasValue.TabIndex = 1;
            lblInversionescerradasValue.Text = "0 %";
            // 
            // lblInversionescerradasDesc
            // 
            lblInversionescerradasDesc.AutoSize = true;
            lblInversionescerradasDesc.Font = new Font("Segoe UI", 8.5F);
            lblInversionescerradasDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblInversionescerradasDesc.Location = new Point(16, 84);
            lblInversionescerradasDesc.Name = "lblInversionescerradasDesc";
            lblInversionescerradasDesc.Size = new Size(180, 19);
            lblInversionescerradasDesc.TabIndex = 2;
            lblInversionescerradasDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlHistorial
            // 
            pnlHistorial.BackColor = Color.White;
            pnlHistorial.BorderStyle = BorderStyle.FixedSingle;
            pnlHistorial.Controls.Add(lblHistorialDesc);
            pnlHistorial.Controls.Add(lblHistorialValue);
            pnlHistorial.Controls.Add(lblHistorialTitle);
            pnlHistorial.Location = new Point(16, 394);
            pnlHistorial.Name = "pnlHistorial";
            pnlHistorial.Size = new Size(900, 110);
            pnlHistorial.TabIndex = 3;
            pnlHistorial.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblHistorialTitle
            // 
            lblHistorialTitle.AutoSize = true;
            lblHistorialTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblHistorialTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblHistorialTitle.Location = new Point(14, 12);
            lblHistorialTitle.Name = "lblHistorialTitle";
            lblHistorialTitle.Size = new Size(120, 23);
            lblHistorialTitle.TabIndex = 0;
            lblHistorialTitle.Text = "Historial";
            // 
            // lblHistorialValue
            // 
            lblHistorialValue.AutoSize = true;
            lblHistorialValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblHistorialValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblHistorialValue.Location = new Point(14, 42);
            lblHistorialValue.Name = "lblHistorialValue";
            lblHistorialValue.Size = new Size(120, 41);
            lblHistorialValue.TabIndex = 1;
            lblHistorialValue.Text = "RD$ 0.00";
            // 
            // lblHistorialDesc
            // 
            lblHistorialDesc.AutoSize = true;
            lblHistorialDesc.Font = new Font("Segoe UI", 8.5F);
            lblHistorialDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblHistorialDesc.Location = new Point(16, 84);
            lblHistorialDesc.Name = "lblHistorialDesc";
            lblHistorialDesc.Size = new Size(180, 19);
            lblHistorialDesc.TabIndex = 2;
            lblHistorialDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlDetalle
            // 
            pnlDetalle.BackColor = Color.White;
            pnlDetalle.BorderStyle = BorderStyle.FixedSingle;
            pnlDetalle.Controls.Add(lblDetalleDesc);
            pnlDetalle.Controls.Add(lblDetalleValue);
            pnlDetalle.Controls.Add(lblDetalleTitle);
            pnlDetalle.Location = new Point(16, 520);
            pnlDetalle.Name = "pnlDetalle";
            pnlDetalle.Size = new Size(900, 110);
            pnlDetalle.TabIndex = 4;
            pnlDetalle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblDetalleTitle
            // 
            lblDetalleTitle.AutoSize = true;
            lblDetalleTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblDetalleTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblDetalleTitle.Location = new Point(14, 12);
            lblDetalleTitle.Name = "lblDetalleTitle";
            lblDetalleTitle.Size = new Size(120, 23);
            lblDetalleTitle.TabIndex = 0;
            lblDetalleTitle.Text = "Detalle";
            // 
            // lblDetalleValue
            // 
            lblDetalleValue.AutoSize = true;
            lblDetalleValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblDetalleValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblDetalleValue.Location = new Point(14, 42);
            lblDetalleValue.Name = "lblDetalleValue";
            lblDetalleValue.Size = new Size(120, 41);
            lblDetalleValue.TabIndex = 1;
            lblDetalleValue.Text = "—";
            // 
            // lblDetalleDesc
            // 
            lblDetalleDesc.AutoSize = true;
            lblDetalleDesc.Font = new Font("Segoe UI", 8.5F);
            lblDetalleDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblDetalleDesc.Location = new Point(16, 84);
            lblDetalleDesc.Name = "lblDetalleDesc";
            lblDetalleDesc.Size = new Size(180, 19);
            lblDetalleDesc.TabIndex = 2;
            lblDetalleDesc.Text = "Dato visual mock — sin logica";
            // 
            // FrmAnaInversiones
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(940, 600);
            Controls.Add(panelScroll);
            Controls.Add(panelHeaderLocal);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FrmAnaInversiones";
            Text = "Inversiones";
            panelHeaderLocal.ResumeLayout(false);
            panelHeaderLocal.PerformLayout();
            panelScroll.ResumeLayout(false);
            pnlResumen.ResumeLayout(false);
            pnlResumen.PerformLayout();
            pnlInversionesactivas.ResumeLayout(false);
            pnlInversionesactivas.PerformLayout();
            pnlInversionescerradas.ResumeLayout(false);
            pnlInversionescerradas.PerformLayout();
            pnlHistorial.ResumeLayout(false);
            pnlHistorial.PerformLayout();
            pnlDetalle.ResumeLayout(false);
            pnlDetalle.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelHeaderLocal;
        private Label lblHeaderLocal;
        private Panel panelScroll;
        private Panel pnlResumen;
        private Label lblResumenTitle;
        private Label lblResumenValue;
        private Label lblResumenDesc;
        private Panel pnlInversionesactivas;
        private Label lblInversionesactivasTitle;
        private Label lblInversionesactivasValue;
        private Label lblInversionesactivasDesc;
        private Panel pnlInversionescerradas;
        private Label lblInversionescerradasTitle;
        private Label lblInversionescerradasValue;
        private Label lblInversionescerradasDesc;
        private Panel pnlHistorial;
        private Label lblHistorialTitle;
        private Label lblHistorialValue;
        private Label lblHistorialDesc;
        private Panel pnlDetalle;
        private Label lblDetalleTitle;
        private Label lblDetalleValue;
        private Label lblDetalleDesc;
    }
}
