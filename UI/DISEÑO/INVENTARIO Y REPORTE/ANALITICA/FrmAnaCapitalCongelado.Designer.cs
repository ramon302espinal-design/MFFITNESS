namespace UI
{
    partial class FrmAnaCapitalCongelado
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
            lblCrmHint = new Label();
            panelScroll = new Panel();
            pnlCapitalcongelado = new Panel();
            lblCapitalcongeladoTitle = new Label();
            lblCapitalcongeladoValue = new Label();
            lblCapitalcongeladoDesc = new Label();
            pnlPorcentaje = new Panel();
            lblPorcentajeTitle = new Label();
            lblPorcentajeValue = new Label();
            lblPorcentajeDesc = new Label();
            pnlProductosafectados = new Panel();
            lblProductosafectadosTitle = new Label();
            lblProductosafectadosValue = new Label();
            lblProductosafectadosDesc = new Label();
            pnlDiasinmovilizado = new Panel();
            lblDiasinmovilizadoTitle = new Label();
            lblDiasinmovilizadoValue = new Label();
            lblDiasinmovilizadoDesc = new Label();
            pnlProductoscriticos = new Panel();
            lblProductoscriticosTitle = new Label();
            lblProductoscriticosValue = new Label();
            lblProductoscriticosDesc = new Label();
            pnlProductoslentos = new Panel();
            lblProductoslentosTitle = new Label();
            lblProductoslentosValue = new Label();
            lblProductoslentosDesc = new Label();
            pnlTabla = new Panel();
            lblTablaTitle = new Label();
            lblTablaValue = new Label();
            lblTablaDesc = new Label();
            panelHeaderLocal.SuspendLayout();
            panelScroll.SuspendLayout();
            pnlCapitalcongelado.SuspendLayout();
            pnlPorcentaje.SuspendLayout();
            pnlProductosafectados.SuspendLayout();
            pnlDiasinmovilizado.SuspendLayout();
            pnlProductoscriticos.SuspendLayout();
            pnlProductoslentos.SuspendLayout();
            pnlTabla.SuspendLayout();
            SuspendLayout();
            // 
            // panelHeaderLocal
            // 
            panelHeaderLocal.BackColor = Color.White;
            panelHeaderLocal.BorderStyle = BorderStyle.FixedSingle;
            panelHeaderLocal.Controls.Add(lblCrmHint);
            panelHeaderLocal.Controls.Add(lblHeaderLocal);
            panelHeaderLocal.Dock = DockStyle.Top;
            panelHeaderLocal.Location = new Point(0, 0);
            panelHeaderLocal.Name = "panelHeaderLocal";
            panelHeaderLocal.Size = new Size(940, 64);
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

            // 
            // lblCrmHint
            // 
            lblCrmHint.AutoSize = true;
            lblCrmHint.Font = new Font("Segoe UI", 8F);
            lblCrmHint.ForeColor = Color.FromArgb(113, 128, 150);
            lblCrmHint.Location = new Point(14, 40);
            lblCrmHint.Name = "lblCrmHint";
            lblCrmHint.Size = new Size(800, 19);
            lblCrmHint.TabIndex = 1;
            lblCrmHint.Text = "";
            lblHeaderLocal.Text = "Capital congelado";
            // 
            // panelScroll
            // 
            panelScroll.AutoScroll = true;
            panelScroll.BackColor = Color.FromArgb(247, 249, 252);
            panelScroll.Controls.Add(pnlCapitalcongelado);
            panelScroll.Controls.Add(pnlPorcentaje);
            panelScroll.Controls.Add(pnlProductosafectados);
            panelScroll.Controls.Add(pnlDiasinmovilizado);
            panelScroll.Controls.Add(pnlProductoscriticos);
            panelScroll.Controls.Add(pnlProductoslentos);
            panelScroll.Controls.Add(pnlTabla);
            panelScroll.Dock = DockStyle.Fill;
            panelScroll.Location = new Point(0, 64);
            panelScroll.Name = "panelScroll";
            panelScroll.Padding = new Padding(8);
            panelScroll.Size = new Size(940, 552);
            panelScroll.TabIndex = 1;
            // 
            // pnlCapitalcongelado
            // 
            pnlCapitalcongelado.BackColor = Color.White;
            pnlCapitalcongelado.BorderStyle = BorderStyle.FixedSingle;
            pnlCapitalcongelado.Controls.Add(lblCapitalcongeladoDesc);
            pnlCapitalcongelado.Controls.Add(lblCapitalcongeladoValue);
            pnlCapitalcongelado.Controls.Add(lblCapitalcongeladoTitle);
            pnlCapitalcongelado.Location = new Point(16, 16);
            pnlCapitalcongelado.Name = "pnlCapitalcongelado";
            pnlCapitalcongelado.Size = new Size(900, 110);
            pnlCapitalcongelado.TabIndex = 0;
            pnlCapitalcongelado.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblCapitalcongeladoTitle
            // 
            lblCapitalcongeladoTitle.AutoSize = true;
            lblCapitalcongeladoTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCapitalcongeladoTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblCapitalcongeladoTitle.Location = new Point(14, 12);
            lblCapitalcongeladoTitle.Name = "lblCapitalcongeladoTitle";
            lblCapitalcongeladoTitle.Size = new Size(120, 23);
            lblCapitalcongeladoTitle.TabIndex = 0;
            lblCapitalcongeladoTitle.Text = "Capital congelado";
            // 
            // lblCapitalcongeladoValue
            // 
            lblCapitalcongeladoValue.AutoSize = true;
            lblCapitalcongeladoValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblCapitalcongeladoValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblCapitalcongeladoValue.Location = new Point(14, 42);
            lblCapitalcongeladoValue.Name = "lblCapitalcongeladoValue";
            lblCapitalcongeladoValue.Size = new Size(120, 41);
            lblCapitalcongeladoValue.TabIndex = 1;
            lblCapitalcongeladoValue.Text = "RD$ 0.00";
            // 
            // lblCapitalcongeladoDesc
            // 
            lblCapitalcongeladoDesc.AutoSize = true;
            lblCapitalcongeladoDesc.Font = new Font("Segoe UI", 8.5F);
            lblCapitalcongeladoDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblCapitalcongeladoDesc.Location = new Point(16, 84);
            lblCapitalcongeladoDesc.Name = "lblCapitalcongeladoDesc";
            lblCapitalcongeladoDesc.Size = new Size(180, 19);
            lblCapitalcongeladoDesc.TabIndex = 2;
            lblCapitalcongeladoDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlPorcentaje
            // 
            pnlPorcentaje.BackColor = Color.White;
            pnlPorcentaje.BorderStyle = BorderStyle.FixedSingle;
            pnlPorcentaje.Controls.Add(lblPorcentajeDesc);
            pnlPorcentaje.Controls.Add(lblPorcentajeValue);
            pnlPorcentaje.Controls.Add(lblPorcentajeTitle);
            pnlPorcentaje.Location = new Point(16, 142);
            pnlPorcentaje.Name = "pnlPorcentaje";
            pnlPorcentaje.Size = new Size(900, 110);
            pnlPorcentaje.TabIndex = 1;
            pnlPorcentaje.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblPorcentajeTitle
            // 
            lblPorcentajeTitle.AutoSize = true;
            lblPorcentajeTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblPorcentajeTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblPorcentajeTitle.Location = new Point(14, 12);
            lblPorcentajeTitle.Name = "lblPorcentajeTitle";
            lblPorcentajeTitle.Size = new Size(120, 23);
            lblPorcentajeTitle.TabIndex = 0;
            lblPorcentajeTitle.Text = "Porcentaje";
            // 
            // lblPorcentajeValue
            // 
            lblPorcentajeValue.AutoSize = true;
            lblPorcentajeValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblPorcentajeValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblPorcentajeValue.Location = new Point(14, 42);
            lblPorcentajeValue.Name = "lblPorcentajeValue";
            lblPorcentajeValue.Size = new Size(120, 41);
            lblPorcentajeValue.TabIndex = 1;
            lblPorcentajeValue.Text = "—";
            // 
            // lblPorcentajeDesc
            // 
            lblPorcentajeDesc.AutoSize = true;
            lblPorcentajeDesc.Font = new Font("Segoe UI", 8.5F);
            lblPorcentajeDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblPorcentajeDesc.Location = new Point(16, 84);
            lblPorcentajeDesc.Name = "lblPorcentajeDesc";
            lblPorcentajeDesc.Size = new Size(180, 19);
            lblPorcentajeDesc.TabIndex = 2;
            lblPorcentajeDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlProductosafectados
            // 
            pnlProductosafectados.BackColor = Color.White;
            pnlProductosafectados.BorderStyle = BorderStyle.FixedSingle;
            pnlProductosafectados.Controls.Add(lblProductosafectadosDesc);
            pnlProductosafectados.Controls.Add(lblProductosafectadosValue);
            pnlProductosafectados.Controls.Add(lblProductosafectadosTitle);
            pnlProductosafectados.Location = new Point(16, 268);
            pnlProductosafectados.Name = "pnlProductosafectados";
            pnlProductosafectados.Size = new Size(900, 110);
            pnlProductosafectados.TabIndex = 2;
            pnlProductosafectados.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblProductosafectadosTitle
            // 
            lblProductosafectadosTitle.AutoSize = true;
            lblProductosafectadosTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblProductosafectadosTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblProductosafectadosTitle.Location = new Point(14, 12);
            lblProductosafectadosTitle.Name = "lblProductosafectadosTitle";
            lblProductosafectadosTitle.Size = new Size(120, 23);
            lblProductosafectadosTitle.TabIndex = 0;
            lblProductosafectadosTitle.Text = "Productos afectados";
            // 
            // lblProductosafectadosValue
            // 
            lblProductosafectadosValue.AutoSize = true;
            lblProductosafectadosValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblProductosafectadosValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblProductosafectadosValue.Location = new Point(14, 42);
            lblProductosafectadosValue.Name = "lblProductosafectadosValue";
            lblProductosafectadosValue.Size = new Size(120, 41);
            lblProductosafectadosValue.TabIndex = 1;
            lblProductosafectadosValue.Text = "0 %";
            // 
            // lblProductosafectadosDesc
            // 
            lblProductosafectadosDesc.AutoSize = true;
            lblProductosafectadosDesc.Font = new Font("Segoe UI", 8.5F);
            lblProductosafectadosDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblProductosafectadosDesc.Location = new Point(16, 84);
            lblProductosafectadosDesc.Name = "lblProductosafectadosDesc";
            lblProductosafectadosDesc.Size = new Size(180, 19);
            lblProductosafectadosDesc.TabIndex = 2;
            lblProductosafectadosDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlDiasinmovilizado
            // 
            pnlDiasinmovilizado.BackColor = Color.White;
            pnlDiasinmovilizado.BorderStyle = BorderStyle.FixedSingle;
            pnlDiasinmovilizado.Controls.Add(lblDiasinmovilizadoDesc);
            pnlDiasinmovilizado.Controls.Add(lblDiasinmovilizadoValue);
            pnlDiasinmovilizado.Controls.Add(lblDiasinmovilizadoTitle);
            pnlDiasinmovilizado.Location = new Point(16, 394);
            pnlDiasinmovilizado.Name = "pnlDiasinmovilizado";
            pnlDiasinmovilizado.Size = new Size(900, 110);
            pnlDiasinmovilizado.TabIndex = 3;
            pnlDiasinmovilizado.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblDiasinmovilizadoTitle
            // 
            lblDiasinmovilizadoTitle.AutoSize = true;
            lblDiasinmovilizadoTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblDiasinmovilizadoTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblDiasinmovilizadoTitle.Location = new Point(14, 12);
            lblDiasinmovilizadoTitle.Name = "lblDiasinmovilizadoTitle";
            lblDiasinmovilizadoTitle.Size = new Size(120, 23);
            lblDiasinmovilizadoTitle.TabIndex = 0;
            lblDiasinmovilizadoTitle.Text = "Dias inmovilizado";
            // 
            // lblDiasinmovilizadoValue
            // 
            lblDiasinmovilizadoValue.AutoSize = true;
            lblDiasinmovilizadoValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblDiasinmovilizadoValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblDiasinmovilizadoValue.Location = new Point(14, 42);
            lblDiasinmovilizadoValue.Name = "lblDiasinmovilizadoValue";
            lblDiasinmovilizadoValue.Size = new Size(120, 41);
            lblDiasinmovilizadoValue.TabIndex = 1;
            lblDiasinmovilizadoValue.Text = "RD$ 0.00";
            // 
            // lblDiasinmovilizadoDesc
            // 
            lblDiasinmovilizadoDesc.AutoSize = true;
            lblDiasinmovilizadoDesc.Font = new Font("Segoe UI", 8.5F);
            lblDiasinmovilizadoDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblDiasinmovilizadoDesc.Location = new Point(16, 84);
            lblDiasinmovilizadoDesc.Name = "lblDiasinmovilizadoDesc";
            lblDiasinmovilizadoDesc.Size = new Size(180, 19);
            lblDiasinmovilizadoDesc.TabIndex = 2;
            lblDiasinmovilizadoDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlProductoscriticos
            // 
            pnlProductoscriticos.BackColor = Color.White;
            pnlProductoscriticos.BorderStyle = BorderStyle.FixedSingle;
            pnlProductoscriticos.Controls.Add(lblProductoscriticosDesc);
            pnlProductoscriticos.Controls.Add(lblProductoscriticosValue);
            pnlProductoscriticos.Controls.Add(lblProductoscriticosTitle);
            pnlProductoscriticos.Location = new Point(16, 520);
            pnlProductoscriticos.Name = "pnlProductoscriticos";
            pnlProductoscriticos.Size = new Size(900, 110);
            pnlProductoscriticos.TabIndex = 4;
            pnlProductoscriticos.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblProductoscriticosTitle
            // 
            lblProductoscriticosTitle.AutoSize = true;
            lblProductoscriticosTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblProductoscriticosTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblProductoscriticosTitle.Location = new Point(14, 12);
            lblProductoscriticosTitle.Name = "lblProductoscriticosTitle";
            lblProductoscriticosTitle.Size = new Size(120, 23);
            lblProductoscriticosTitle.TabIndex = 0;
            lblProductoscriticosTitle.Text = "Productos criticos";
            // 
            // lblProductoscriticosValue
            // 
            lblProductoscriticosValue.AutoSize = true;
            lblProductoscriticosValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblProductoscriticosValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblProductoscriticosValue.Location = new Point(14, 42);
            lblProductoscriticosValue.Name = "lblProductoscriticosValue";
            lblProductoscriticosValue.Size = new Size(120, 41);
            lblProductoscriticosValue.TabIndex = 1;
            lblProductoscriticosValue.Text = "—";
            // 
            // lblProductoscriticosDesc
            // 
            lblProductoscriticosDesc.AutoSize = true;
            lblProductoscriticosDesc.Font = new Font("Segoe UI", 8.5F);
            lblProductoscriticosDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblProductoscriticosDesc.Location = new Point(16, 84);
            lblProductoscriticosDesc.Name = "lblProductoscriticosDesc";
            lblProductoscriticosDesc.Size = new Size(180, 19);
            lblProductoscriticosDesc.TabIndex = 2;
            lblProductoscriticosDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlProductoslentos
            // 
            pnlProductoslentos.BackColor = Color.White;
            pnlProductoslentos.BorderStyle = BorderStyle.FixedSingle;
            pnlProductoslentos.Controls.Add(lblProductoslentosDesc);
            pnlProductoslentos.Controls.Add(lblProductoslentosValue);
            pnlProductoslentos.Controls.Add(lblProductoslentosTitle);
            pnlProductoslentos.Location = new Point(16, 646);
            pnlProductoslentos.Name = "pnlProductoslentos";
            pnlProductoslentos.Size = new Size(900, 110);
            pnlProductoslentos.TabIndex = 5;
            pnlProductoslentos.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblProductoslentosTitle
            // 
            lblProductoslentosTitle.AutoSize = true;
            lblProductoslentosTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblProductoslentosTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblProductoslentosTitle.Location = new Point(14, 12);
            lblProductoslentosTitle.Name = "lblProductoslentosTitle";
            lblProductoslentosTitle.Size = new Size(120, 23);
            lblProductoslentosTitle.TabIndex = 0;
            lblProductoslentosTitle.Text = "Productos lentos";
            // 
            // lblProductoslentosValue
            // 
            lblProductoslentosValue.AutoSize = true;
            lblProductoslentosValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblProductoslentosValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblProductoslentosValue.Location = new Point(14, 42);
            lblProductoslentosValue.Name = "lblProductoslentosValue";
            lblProductoslentosValue.Size = new Size(120, 41);
            lblProductoslentosValue.TabIndex = 1;
            lblProductoslentosValue.Text = "0 %";
            // 
            // lblProductoslentosDesc
            // 
            lblProductoslentosDesc.AutoSize = true;
            lblProductoslentosDesc.Font = new Font("Segoe UI", 8.5F);
            lblProductoslentosDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblProductoslentosDesc.Location = new Point(16, 84);
            lblProductoslentosDesc.Name = "lblProductoslentosDesc";
            lblProductoslentosDesc.Size = new Size(180, 19);
            lblProductoslentosDesc.TabIndex = 2;
            lblProductoslentosDesc.Text = "Dato visual mock — sin logica";
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
            // FrmAnaCapitalCongelado
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(940, 600);
            Controls.Add(panelScroll);
            Controls.Add(panelHeaderLocal);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FrmAnaCapitalCongelado";
            Text = "Capital congelado";
            panelHeaderLocal.ResumeLayout(false);
            panelHeaderLocal.PerformLayout();
            panelScroll.ResumeLayout(false);
            pnlCapitalcongelado.ResumeLayout(false);
            pnlCapitalcongelado.PerformLayout();
            pnlPorcentaje.ResumeLayout(false);
            pnlPorcentaje.PerformLayout();
            pnlProductosafectados.ResumeLayout(false);
            pnlProductosafectados.PerformLayout();
            pnlDiasinmovilizado.ResumeLayout(false);
            pnlDiasinmovilizado.PerformLayout();
            pnlProductoscriticos.ResumeLayout(false);
            pnlProductoscriticos.PerformLayout();
            pnlProductoslentos.ResumeLayout(false);
            pnlProductoslentos.PerformLayout();
            pnlTabla.ResumeLayout(false);
            pnlTabla.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelHeaderLocal;
        private Label lblHeaderLocal;
        private Label lblCrmHint;
        private Panel panelScroll;
        private Panel pnlCapitalcongelado;
        private Label lblCapitalcongeladoTitle;
        private Label lblCapitalcongeladoValue;
        private Label lblCapitalcongeladoDesc;
        private Panel pnlPorcentaje;
        private Label lblPorcentajeTitle;
        private Label lblPorcentajeValue;
        private Label lblPorcentajeDesc;
        private Panel pnlProductosafectados;
        private Label lblProductosafectadosTitle;
        private Label lblProductosafectadosValue;
        private Label lblProductosafectadosDesc;
        private Panel pnlDiasinmovilizado;
        private Label lblDiasinmovilizadoTitle;
        private Label lblDiasinmovilizadoValue;
        private Label lblDiasinmovilizadoDesc;
        private Panel pnlProductoscriticos;
        private Label lblProductoscriticosTitle;
        private Label lblProductoscriticosValue;
        private Label lblProductoscriticosDesc;
        private Panel pnlProductoslentos;
        private Label lblProductoslentosTitle;
        private Label lblProductoslentosValue;
        private Label lblProductoslentosDesc;
        private Panel pnlTabla;
        private Label lblTablaTitle;
        private Label lblTablaValue;
        private Label lblTablaDesc;
    }
}
