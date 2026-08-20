namespace UI
{
    partial class FrmAnaProductosEstrella
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
            pnlProductosestrella = new Panel();
            lblProductosestrellaTitle = new Label();
            lblProductosestrellaValue = new Label();
            lblProductosestrellaDesc = new Label();
            pnlCrecimiento = new Panel();
            lblCrecimientoTitle = new Label();
            lblCrecimientoValue = new Label();
            lblCrecimientoDesc = new Label();
            pnlGanancia = new Panel();
            lblGananciaTitle = new Label();
            lblGananciaValue = new Label();
            lblGananciaDesc = new Label();
            pnlROI = new Panel();
            lblROITitle = new Label();
            lblROIValue = new Label();
            lblROIDesc = new Label();
            pnlRotacion = new Panel();
            lblRotacionTitle = new Label();
            lblRotacionValue = new Label();
            lblRotacionDesc = new Label();
            pnlScore = new Panel();
            lblScoreTitle = new Label();
            lblScoreValue = new Label();
            lblScoreDesc = new Label();
            pnlTabla = new Panel();
            lblTablaTitle = new Label();
            lblTablaValue = new Label();
            lblTablaDesc = new Label();
            panelHeaderLocal.SuspendLayout();
            panelScroll.SuspendLayout();
            pnlProductosestrella.SuspendLayout();
            pnlCrecimiento.SuspendLayout();
            pnlGanancia.SuspendLayout();
            pnlROI.SuspendLayout();
            pnlRotacion.SuspendLayout();
            pnlScore.SuspendLayout();
            pnlTabla.SuspendLayout();
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
            lblHeaderLocal.Text = "Productos estrella";
            // 
            // panelScroll
            // 
            panelScroll.AutoScroll = true;
            panelScroll.BackColor = Color.FromArgb(247, 249, 252);
            panelScroll.Controls.Add(pnlProductosestrella);
            panelScroll.Controls.Add(pnlCrecimiento);
            panelScroll.Controls.Add(pnlGanancia);
            panelScroll.Controls.Add(pnlROI);
            panelScroll.Controls.Add(pnlRotacion);
            panelScroll.Controls.Add(pnlScore);
            panelScroll.Controls.Add(pnlTabla);
            panelScroll.Dock = DockStyle.Fill;
            panelScroll.Location = new Point(0, 48);
            panelScroll.Name = "panelScroll";
            panelScroll.Padding = new Padding(8);
            panelScroll.Size = new Size(940, 552);
            panelScroll.TabIndex = 1;
            // 
            // pnlProductosestrella
            // 
            pnlProductosestrella.BackColor = Color.White;
            pnlProductosestrella.BorderStyle = BorderStyle.FixedSingle;
            pnlProductosestrella.Controls.Add(lblProductosestrellaDesc);
            pnlProductosestrella.Controls.Add(lblProductosestrellaValue);
            pnlProductosestrella.Controls.Add(lblProductosestrellaTitle);
            pnlProductosestrella.Location = new Point(16, 16);
            pnlProductosestrella.Name = "pnlProductosestrella";
            pnlProductosestrella.Size = new Size(900, 110);
            pnlProductosestrella.TabIndex = 0;
            pnlProductosestrella.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblProductosestrellaTitle
            // 
            lblProductosestrellaTitle.AutoSize = true;
            lblProductosestrellaTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblProductosestrellaTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblProductosestrellaTitle.Location = new Point(14, 12);
            lblProductosestrellaTitle.Name = "lblProductosestrellaTitle";
            lblProductosestrellaTitle.Size = new Size(120, 23);
            lblProductosestrellaTitle.TabIndex = 0;
            lblProductosestrellaTitle.Text = "Productos estrella";
            // 
            // lblProductosestrellaValue
            // 
            lblProductosestrellaValue.AutoSize = true;
            lblProductosestrellaValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblProductosestrellaValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblProductosestrellaValue.Location = new Point(14, 42);
            lblProductosestrellaValue.Name = "lblProductosestrellaValue";
            lblProductosestrellaValue.Size = new Size(120, 41);
            lblProductosestrellaValue.TabIndex = 1;
            lblProductosestrellaValue.Text = "RD$ 0.00";
            // 
            // lblProductosestrellaDesc
            // 
            lblProductosestrellaDesc.AutoSize = true;
            lblProductosestrellaDesc.Font = new Font("Segoe UI", 8.5F);
            lblProductosestrellaDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblProductosestrellaDesc.Location = new Point(16, 84);
            lblProductosestrellaDesc.Name = "lblProductosestrellaDesc";
            lblProductosestrellaDesc.Size = new Size(180, 19);
            lblProductosestrellaDesc.TabIndex = 2;
            lblProductosestrellaDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlCrecimiento
            // 
            pnlCrecimiento.BackColor = Color.White;
            pnlCrecimiento.BorderStyle = BorderStyle.FixedSingle;
            pnlCrecimiento.Controls.Add(lblCrecimientoDesc);
            pnlCrecimiento.Controls.Add(lblCrecimientoValue);
            pnlCrecimiento.Controls.Add(lblCrecimientoTitle);
            pnlCrecimiento.Location = new Point(16, 142);
            pnlCrecimiento.Name = "pnlCrecimiento";
            pnlCrecimiento.Size = new Size(900, 110);
            pnlCrecimiento.TabIndex = 1;
            pnlCrecimiento.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblCrecimientoTitle
            // 
            lblCrecimientoTitle.AutoSize = true;
            lblCrecimientoTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCrecimientoTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblCrecimientoTitle.Location = new Point(14, 12);
            lblCrecimientoTitle.Name = "lblCrecimientoTitle";
            lblCrecimientoTitle.Size = new Size(120, 23);
            lblCrecimientoTitle.TabIndex = 0;
            lblCrecimientoTitle.Text = "Crecimiento";
            // 
            // lblCrecimientoValue
            // 
            lblCrecimientoValue.AutoSize = true;
            lblCrecimientoValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblCrecimientoValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblCrecimientoValue.Location = new Point(14, 42);
            lblCrecimientoValue.Name = "lblCrecimientoValue";
            lblCrecimientoValue.Size = new Size(120, 41);
            lblCrecimientoValue.TabIndex = 1;
            lblCrecimientoValue.Text = "—";
            // 
            // lblCrecimientoDesc
            // 
            lblCrecimientoDesc.AutoSize = true;
            lblCrecimientoDesc.Font = new Font("Segoe UI", 8.5F);
            lblCrecimientoDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblCrecimientoDesc.Location = new Point(16, 84);
            lblCrecimientoDesc.Name = "lblCrecimientoDesc";
            lblCrecimientoDesc.Size = new Size(180, 19);
            lblCrecimientoDesc.TabIndex = 2;
            lblCrecimientoDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlGanancia
            // 
            pnlGanancia.BackColor = Color.White;
            pnlGanancia.BorderStyle = BorderStyle.FixedSingle;
            pnlGanancia.Controls.Add(lblGananciaDesc);
            pnlGanancia.Controls.Add(lblGananciaValue);
            pnlGanancia.Controls.Add(lblGananciaTitle);
            pnlGanancia.Location = new Point(16, 268);
            pnlGanancia.Name = "pnlGanancia";
            pnlGanancia.Size = new Size(900, 110);
            pnlGanancia.TabIndex = 2;
            pnlGanancia.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblGananciaTitle
            // 
            lblGananciaTitle.AutoSize = true;
            lblGananciaTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblGananciaTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblGananciaTitle.Location = new Point(14, 12);
            lblGananciaTitle.Name = "lblGananciaTitle";
            lblGananciaTitle.Size = new Size(120, 23);
            lblGananciaTitle.TabIndex = 0;
            lblGananciaTitle.Text = "Ganancia";
            // 
            // lblGananciaValue
            // 
            lblGananciaValue.AutoSize = true;
            lblGananciaValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblGananciaValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblGananciaValue.Location = new Point(14, 42);
            lblGananciaValue.Name = "lblGananciaValue";
            lblGananciaValue.Size = new Size(120, 41);
            lblGananciaValue.TabIndex = 1;
            lblGananciaValue.Text = "0 %";
            // 
            // lblGananciaDesc
            // 
            lblGananciaDesc.AutoSize = true;
            lblGananciaDesc.Font = new Font("Segoe UI", 8.5F);
            lblGananciaDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblGananciaDesc.Location = new Point(16, 84);
            lblGananciaDesc.Name = "lblGananciaDesc";
            lblGananciaDesc.Size = new Size(180, 19);
            lblGananciaDesc.TabIndex = 2;
            lblGananciaDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlROI
            // 
            pnlROI.BackColor = Color.White;
            pnlROI.BorderStyle = BorderStyle.FixedSingle;
            pnlROI.Controls.Add(lblROIDesc);
            pnlROI.Controls.Add(lblROIValue);
            pnlROI.Controls.Add(lblROITitle);
            pnlROI.Location = new Point(16, 394);
            pnlROI.Name = "pnlROI";
            pnlROI.Size = new Size(900, 110);
            pnlROI.TabIndex = 3;
            pnlROI.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblROITitle
            // 
            lblROITitle.AutoSize = true;
            lblROITitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblROITitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblROITitle.Location = new Point(14, 12);
            lblROITitle.Name = "lblROITitle";
            lblROITitle.Size = new Size(120, 23);
            lblROITitle.TabIndex = 0;
            lblROITitle.Text = "ROI";
            // 
            // lblROIValue
            // 
            lblROIValue.AutoSize = true;
            lblROIValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblROIValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblROIValue.Location = new Point(14, 42);
            lblROIValue.Name = "lblROIValue";
            lblROIValue.Size = new Size(120, 41);
            lblROIValue.TabIndex = 1;
            lblROIValue.Text = "RD$ 0.00";
            // 
            // lblROIDesc
            // 
            lblROIDesc.AutoSize = true;
            lblROIDesc.Font = new Font("Segoe UI", 8.5F);
            lblROIDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblROIDesc.Location = new Point(16, 84);
            lblROIDesc.Name = "lblROIDesc";
            lblROIDesc.Size = new Size(180, 19);
            lblROIDesc.TabIndex = 2;
            lblROIDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlRotacion
            // 
            pnlRotacion.BackColor = Color.White;
            pnlRotacion.BorderStyle = BorderStyle.FixedSingle;
            pnlRotacion.Controls.Add(lblRotacionDesc);
            pnlRotacion.Controls.Add(lblRotacionValue);
            pnlRotacion.Controls.Add(lblRotacionTitle);
            pnlRotacion.Location = new Point(16, 520);
            pnlRotacion.Name = "pnlRotacion";
            pnlRotacion.Size = new Size(900, 110);
            pnlRotacion.TabIndex = 4;
            pnlRotacion.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblRotacionTitle
            // 
            lblRotacionTitle.AutoSize = true;
            lblRotacionTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblRotacionTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblRotacionTitle.Location = new Point(14, 12);
            lblRotacionTitle.Name = "lblRotacionTitle";
            lblRotacionTitle.Size = new Size(120, 23);
            lblRotacionTitle.TabIndex = 0;
            lblRotacionTitle.Text = "Rotacion";
            // 
            // lblRotacionValue
            // 
            lblRotacionValue.AutoSize = true;
            lblRotacionValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblRotacionValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblRotacionValue.Location = new Point(14, 42);
            lblRotacionValue.Name = "lblRotacionValue";
            lblRotacionValue.Size = new Size(120, 41);
            lblRotacionValue.TabIndex = 1;
            lblRotacionValue.Text = "—";
            // 
            // lblRotacionDesc
            // 
            lblRotacionDesc.AutoSize = true;
            lblRotacionDesc.Font = new Font("Segoe UI", 8.5F);
            lblRotacionDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblRotacionDesc.Location = new Point(16, 84);
            lblRotacionDesc.Name = "lblRotacionDesc";
            lblRotacionDesc.Size = new Size(180, 19);
            lblRotacionDesc.TabIndex = 2;
            lblRotacionDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlScore
            // 
            pnlScore.BackColor = Color.White;
            pnlScore.BorderStyle = BorderStyle.FixedSingle;
            pnlScore.Controls.Add(lblScoreDesc);
            pnlScore.Controls.Add(lblScoreValue);
            pnlScore.Controls.Add(lblScoreTitle);
            pnlScore.Location = new Point(16, 646);
            pnlScore.Name = "pnlScore";
            pnlScore.Size = new Size(900, 110);
            pnlScore.TabIndex = 5;
            pnlScore.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblScoreTitle
            // 
            lblScoreTitle.AutoSize = true;
            lblScoreTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblScoreTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblScoreTitle.Location = new Point(14, 12);
            lblScoreTitle.Name = "lblScoreTitle";
            lblScoreTitle.Size = new Size(120, 23);
            lblScoreTitle.TabIndex = 0;
            lblScoreTitle.Text = "Score";
            // 
            // lblScoreValue
            // 
            lblScoreValue.AutoSize = true;
            lblScoreValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblScoreValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblScoreValue.Location = new Point(14, 42);
            lblScoreValue.Name = "lblScoreValue";
            lblScoreValue.Size = new Size(120, 41);
            lblScoreValue.TabIndex = 1;
            lblScoreValue.Text = "0 %";
            // 
            // lblScoreDesc
            // 
            lblScoreDesc.AutoSize = true;
            lblScoreDesc.Font = new Font("Segoe UI", 8.5F);
            lblScoreDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblScoreDesc.Location = new Point(16, 84);
            lblScoreDesc.Name = "lblScoreDesc";
            lblScoreDesc.Size = new Size(180, 19);
            lblScoreDesc.TabIndex = 2;
            lblScoreDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlTabla
            // 
            pnlTabla.BackColor = Color.White;
            pnlTabla.BorderStyle = BorderStyle.FixedSingle;
            pnlTabla.Controls.Add(lblTablaDesc);
            pnlTabla.Controls.Add(lblTablaValue);
            pnlTabla.Controls.Add(lblTablaTitle);
            pnlTabla.Location = new Point(16, 772);
            pnlTabla.Name = "pnlTabla";
            pnlTabla.Size = new Size(900, 110);
            pnlTabla.TabIndex = 6;
            pnlTabla.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblTablaTitle
            // 
            lblTablaTitle.AutoSize = true;
            lblTablaTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblTablaTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblTablaTitle.Location = new Point(14, 12);
            lblTablaTitle.Name = "lblTablaTitle";
            lblTablaTitle.Size = new Size(120, 23);
            lblTablaTitle.TabIndex = 0;
            lblTablaTitle.Text = "Tabla";
            // 
            // lblTablaValue
            // 
            lblTablaValue.AutoSize = true;
            lblTablaValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTablaValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblTablaValue.Location = new Point(14, 42);
            lblTablaValue.Name = "lblTablaValue";
            lblTablaValue.Size = new Size(120, 41);
            lblTablaValue.TabIndex = 1;
            lblTablaValue.Text = "RD$ 0.00";
            // 
            // lblTablaDesc
            // 
            lblTablaDesc.AutoSize = true;
            lblTablaDesc.Font = new Font("Segoe UI", 8.5F);
            lblTablaDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblTablaDesc.Location = new Point(16, 84);
            lblTablaDesc.Name = "lblTablaDesc";
            lblTablaDesc.Size = new Size(180, 19);
            lblTablaDesc.TabIndex = 2;
            lblTablaDesc.Text = "Dato visual mock — sin logica";
            // 
            // FrmAnaProductosEstrella
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(940, 600);
            Controls.Add(panelScroll);
            Controls.Add(panelHeaderLocal);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FrmAnaProductosEstrella";
            Text = "Productos estrella";
            panelHeaderLocal.ResumeLayout(false);
            panelHeaderLocal.PerformLayout();
            panelScroll.ResumeLayout(false);
            pnlProductosestrella.ResumeLayout(false);
            pnlProductosestrella.PerformLayout();
            pnlCrecimiento.ResumeLayout(false);
            pnlCrecimiento.PerformLayout();
            pnlGanancia.ResumeLayout(false);
            pnlGanancia.PerformLayout();
            pnlROI.ResumeLayout(false);
            pnlROI.PerformLayout();
            pnlRotacion.ResumeLayout(false);
            pnlRotacion.PerformLayout();
            pnlScore.ResumeLayout(false);
            pnlScore.PerformLayout();
            pnlTabla.ResumeLayout(false);
            pnlTabla.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelHeaderLocal;
        private Label lblHeaderLocal;
        private Panel panelScroll;
        private Panel pnlProductosestrella;
        private Label lblProductosestrellaTitle;
        private Label lblProductosestrellaValue;
        private Label lblProductosestrellaDesc;
        private Panel pnlCrecimiento;
        private Label lblCrecimientoTitle;
        private Label lblCrecimientoValue;
        private Label lblCrecimientoDesc;
        private Panel pnlGanancia;
        private Label lblGananciaTitle;
        private Label lblGananciaValue;
        private Label lblGananciaDesc;
        private Panel pnlROI;
        private Label lblROITitle;
        private Label lblROIValue;
        private Label lblROIDesc;
        private Panel pnlRotacion;
        private Label lblRotacionTitle;
        private Label lblRotacionValue;
        private Label lblRotacionDesc;
        private Panel pnlScore;
        private Label lblScoreTitle;
        private Label lblScoreValue;
        private Label lblScoreDesc;
        private Panel pnlTabla;
        private Label lblTablaTitle;
        private Label lblTablaValue;
        private Label lblTablaDesc;
    }
}
