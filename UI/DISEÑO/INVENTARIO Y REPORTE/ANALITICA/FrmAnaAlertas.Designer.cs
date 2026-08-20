namespace UI
{
    partial class FrmAnaAlertas
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
            pnlCriticas = new Panel();
            lblCriticasTitle = new Label();
            lblCriticasValue = new Label();
            lblCriticasDesc = new Label();
            pnlImportantes = new Panel();
            lblImportantesTitle = new Label();
            lblImportantesValue = new Label();
            lblImportantesDesc = new Label();
            pnlAdvertencias = new Panel();
            lblAdvertenciasTitle = new Label();
            lblAdvertenciasValue = new Label();
            lblAdvertenciasDesc = new Label();
            pnlInformacion = new Panel();
            lblInformacionTitle = new Label();
            lblInformacionValue = new Label();
            lblInformacionDesc = new Label();
            pnlAlertasdeinventario = new Panel();
            lblAlertasdeinventarioTitle = new Label();
            lblAlertasdeinventarioValue = new Label();
            lblAlertasdeinventarioDesc = new Label();
            pnlAlertasfinancieras = new Panel();
            lblAlertasfinancierasTitle = new Label();
            lblAlertasfinancierasValue = new Label();
            lblAlertasfinancierasDesc = new Label();
            pnlAlertasderentabilidad = new Panel();
            lblAlertasderentabilidadTitle = new Label();
            lblAlertasderentabilidadValue = new Label();
            lblAlertasderentabilidadDesc = new Label();
            pnlHistorial = new Panel();
            lblHistorialTitle = new Label();
            lblHistorialValue = new Label();
            lblHistorialDesc = new Label();
            panelHeaderLocal.SuspendLayout();
            panelScroll.SuspendLayout();
            pnlCriticas.SuspendLayout();
            pnlImportantes.SuspendLayout();
            pnlAdvertencias.SuspendLayout();
            pnlInformacion.SuspendLayout();
            pnlAlertasdeinventario.SuspendLayout();
            pnlAlertasfinancieras.SuspendLayout();
            pnlAlertasderentabilidad.SuspendLayout();
            pnlHistorial.SuspendLayout();
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
            lblHeaderLocal.Text = "Alertas";
            // 
            // panelScroll
            // 
            panelScroll.AutoScroll = true;
            panelScroll.BackColor = Color.FromArgb(247, 249, 252);
            panelScroll.Controls.Add(pnlCriticas);
            panelScroll.Controls.Add(pnlImportantes);
            panelScroll.Controls.Add(pnlAdvertencias);
            panelScroll.Controls.Add(pnlInformacion);
            panelScroll.Controls.Add(pnlAlertasdeinventario);
            panelScroll.Controls.Add(pnlAlertasfinancieras);
            panelScroll.Controls.Add(pnlAlertasderentabilidad);
            panelScroll.Controls.Add(pnlHistorial);
            panelScroll.Dock = DockStyle.Fill;
            panelScroll.Location = new Point(0, 48);
            panelScroll.Name = "panelScroll";
            panelScroll.Padding = new Padding(8);
            panelScroll.Size = new Size(940, 552);
            panelScroll.TabIndex = 1;
            // 
            // pnlCriticas
            // 
            pnlCriticas.BackColor = Color.White;
            pnlCriticas.BorderStyle = BorderStyle.FixedSingle;
            pnlCriticas.Controls.Add(lblCriticasDesc);
            pnlCriticas.Controls.Add(lblCriticasValue);
            pnlCriticas.Controls.Add(lblCriticasTitle);
            pnlCriticas.Location = new Point(16, 16);
            pnlCriticas.Name = "pnlCriticas";
            pnlCriticas.Size = new Size(900, 110);
            pnlCriticas.TabIndex = 0;
            pnlCriticas.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblCriticasTitle
            // 
            lblCriticasTitle.AutoSize = true;
            lblCriticasTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCriticasTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblCriticasTitle.Location = new Point(14, 12);
            lblCriticasTitle.Name = "lblCriticasTitle";
            lblCriticasTitle.Size = new Size(120, 23);
            lblCriticasTitle.TabIndex = 0;
            lblCriticasTitle.Text = "Criticas";
            // 
            // lblCriticasValue
            // 
            lblCriticasValue.AutoSize = true;
            lblCriticasValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblCriticasValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblCriticasValue.Location = new Point(14, 42);
            lblCriticasValue.Name = "lblCriticasValue";
            lblCriticasValue.Size = new Size(120, 41);
            lblCriticasValue.TabIndex = 1;
            lblCriticasValue.Text = "RD$ 0.00";
            // 
            // lblCriticasDesc
            // 
            lblCriticasDesc.AutoSize = true;
            lblCriticasDesc.Font = new Font("Segoe UI", 8.5F);
            lblCriticasDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblCriticasDesc.Location = new Point(16, 84);
            lblCriticasDesc.Name = "lblCriticasDesc";
            lblCriticasDesc.Size = new Size(180, 19);
            lblCriticasDesc.TabIndex = 2;
            lblCriticasDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlImportantes
            // 
            pnlImportantes.BackColor = Color.White;
            pnlImportantes.BorderStyle = BorderStyle.FixedSingle;
            pnlImportantes.Controls.Add(lblImportantesDesc);
            pnlImportantes.Controls.Add(lblImportantesValue);
            pnlImportantes.Controls.Add(lblImportantesTitle);
            pnlImportantes.Location = new Point(16, 142);
            pnlImportantes.Name = "pnlImportantes";
            pnlImportantes.Size = new Size(900, 110);
            pnlImportantes.TabIndex = 1;
            pnlImportantes.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblImportantesTitle
            // 
            lblImportantesTitle.AutoSize = true;
            lblImportantesTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblImportantesTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblImportantesTitle.Location = new Point(14, 12);
            lblImportantesTitle.Name = "lblImportantesTitle";
            lblImportantesTitle.Size = new Size(120, 23);
            lblImportantesTitle.TabIndex = 0;
            lblImportantesTitle.Text = "Importantes";
            // 
            // lblImportantesValue
            // 
            lblImportantesValue.AutoSize = true;
            lblImportantesValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblImportantesValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblImportantesValue.Location = new Point(14, 42);
            lblImportantesValue.Name = "lblImportantesValue";
            lblImportantesValue.Size = new Size(120, 41);
            lblImportantesValue.TabIndex = 1;
            lblImportantesValue.Text = "—";
            // 
            // lblImportantesDesc
            // 
            lblImportantesDesc.AutoSize = true;
            lblImportantesDesc.Font = new Font("Segoe UI", 8.5F);
            lblImportantesDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblImportantesDesc.Location = new Point(16, 84);
            lblImportantesDesc.Name = "lblImportantesDesc";
            lblImportantesDesc.Size = new Size(180, 19);
            lblImportantesDesc.TabIndex = 2;
            lblImportantesDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlAdvertencias
            // 
            pnlAdvertencias.BackColor = Color.White;
            pnlAdvertencias.BorderStyle = BorderStyle.FixedSingle;
            pnlAdvertencias.Controls.Add(lblAdvertenciasDesc);
            pnlAdvertencias.Controls.Add(lblAdvertenciasValue);
            pnlAdvertencias.Controls.Add(lblAdvertenciasTitle);
            pnlAdvertencias.Location = new Point(16, 268);
            pnlAdvertencias.Name = "pnlAdvertencias";
            pnlAdvertencias.Size = new Size(900, 110);
            pnlAdvertencias.TabIndex = 2;
            pnlAdvertencias.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblAdvertenciasTitle
            // 
            lblAdvertenciasTitle.AutoSize = true;
            lblAdvertenciasTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblAdvertenciasTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblAdvertenciasTitle.Location = new Point(14, 12);
            lblAdvertenciasTitle.Name = "lblAdvertenciasTitle";
            lblAdvertenciasTitle.Size = new Size(120, 23);
            lblAdvertenciasTitle.TabIndex = 0;
            lblAdvertenciasTitle.Text = "Advertencias";
            // 
            // lblAdvertenciasValue
            // 
            lblAdvertenciasValue.AutoSize = true;
            lblAdvertenciasValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblAdvertenciasValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblAdvertenciasValue.Location = new Point(14, 42);
            lblAdvertenciasValue.Name = "lblAdvertenciasValue";
            lblAdvertenciasValue.Size = new Size(120, 41);
            lblAdvertenciasValue.TabIndex = 1;
            lblAdvertenciasValue.Text = "0 %";
            // 
            // lblAdvertenciasDesc
            // 
            lblAdvertenciasDesc.AutoSize = true;
            lblAdvertenciasDesc.Font = new Font("Segoe UI", 8.5F);
            lblAdvertenciasDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblAdvertenciasDesc.Location = new Point(16, 84);
            lblAdvertenciasDesc.Name = "lblAdvertenciasDesc";
            lblAdvertenciasDesc.Size = new Size(180, 19);
            lblAdvertenciasDesc.TabIndex = 2;
            lblAdvertenciasDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlInformacion
            // 
            pnlInformacion.BackColor = Color.White;
            pnlInformacion.BorderStyle = BorderStyle.FixedSingle;
            pnlInformacion.Controls.Add(lblInformacionDesc);
            pnlInformacion.Controls.Add(lblInformacionValue);
            pnlInformacion.Controls.Add(lblInformacionTitle);
            pnlInformacion.Location = new Point(16, 394);
            pnlInformacion.Name = "pnlInformacion";
            pnlInformacion.Size = new Size(900, 110);
            pnlInformacion.TabIndex = 3;
            pnlInformacion.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblInformacionTitle
            // 
            lblInformacionTitle.AutoSize = true;
            lblInformacionTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblInformacionTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblInformacionTitle.Location = new Point(14, 12);
            lblInformacionTitle.Name = "lblInformacionTitle";
            lblInformacionTitle.Size = new Size(120, 23);
            lblInformacionTitle.TabIndex = 0;
            lblInformacionTitle.Text = "Informacion";
            // 
            // lblInformacionValue
            // 
            lblInformacionValue.AutoSize = true;
            lblInformacionValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblInformacionValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblInformacionValue.Location = new Point(14, 42);
            lblInformacionValue.Name = "lblInformacionValue";
            lblInformacionValue.Size = new Size(120, 41);
            lblInformacionValue.TabIndex = 1;
            lblInformacionValue.Text = "RD$ 0.00";
            // 
            // lblInformacionDesc
            // 
            lblInformacionDesc.AutoSize = true;
            lblInformacionDesc.Font = new Font("Segoe UI", 8.5F);
            lblInformacionDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblInformacionDesc.Location = new Point(16, 84);
            lblInformacionDesc.Name = "lblInformacionDesc";
            lblInformacionDesc.Size = new Size(180, 19);
            lblInformacionDesc.TabIndex = 2;
            lblInformacionDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlAlertasdeinventario
            // 
            pnlAlertasdeinventario.BackColor = Color.White;
            pnlAlertasdeinventario.BorderStyle = BorderStyle.FixedSingle;
            pnlAlertasdeinventario.Controls.Add(lblAlertasdeinventarioDesc);
            pnlAlertasdeinventario.Controls.Add(lblAlertasdeinventarioValue);
            pnlAlertasdeinventario.Controls.Add(lblAlertasdeinventarioTitle);
            pnlAlertasdeinventario.Location = new Point(16, 520);
            pnlAlertasdeinventario.Name = "pnlAlertasdeinventario";
            pnlAlertasdeinventario.Size = new Size(900, 110);
            pnlAlertasdeinventario.TabIndex = 4;
            pnlAlertasdeinventario.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblAlertasdeinventarioTitle
            // 
            lblAlertasdeinventarioTitle.AutoSize = true;
            lblAlertasdeinventarioTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblAlertasdeinventarioTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblAlertasdeinventarioTitle.Location = new Point(14, 12);
            lblAlertasdeinventarioTitle.Name = "lblAlertasdeinventarioTitle";
            lblAlertasdeinventarioTitle.Size = new Size(120, 23);
            lblAlertasdeinventarioTitle.TabIndex = 0;
            lblAlertasdeinventarioTitle.Text = "Alertas de inventario";
            // 
            // lblAlertasdeinventarioValue
            // 
            lblAlertasdeinventarioValue.AutoSize = true;
            lblAlertasdeinventarioValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblAlertasdeinventarioValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblAlertasdeinventarioValue.Location = new Point(14, 42);
            lblAlertasdeinventarioValue.Name = "lblAlertasdeinventarioValue";
            lblAlertasdeinventarioValue.Size = new Size(120, 41);
            lblAlertasdeinventarioValue.TabIndex = 1;
            lblAlertasdeinventarioValue.Text = "—";
            // 
            // lblAlertasdeinventarioDesc
            // 
            lblAlertasdeinventarioDesc.AutoSize = true;
            lblAlertasdeinventarioDesc.Font = new Font("Segoe UI", 8.5F);
            lblAlertasdeinventarioDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblAlertasdeinventarioDesc.Location = new Point(16, 84);
            lblAlertasdeinventarioDesc.Name = "lblAlertasdeinventarioDesc";
            lblAlertasdeinventarioDesc.Size = new Size(180, 19);
            lblAlertasdeinventarioDesc.TabIndex = 2;
            lblAlertasdeinventarioDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlAlertasfinancieras
            // 
            pnlAlertasfinancieras.BackColor = Color.White;
            pnlAlertasfinancieras.BorderStyle = BorderStyle.FixedSingle;
            pnlAlertasfinancieras.Controls.Add(lblAlertasfinancierasDesc);
            pnlAlertasfinancieras.Controls.Add(lblAlertasfinancierasValue);
            pnlAlertasfinancieras.Controls.Add(lblAlertasfinancierasTitle);
            pnlAlertasfinancieras.Location = new Point(16, 646);
            pnlAlertasfinancieras.Name = "pnlAlertasfinancieras";
            pnlAlertasfinancieras.Size = new Size(900, 110);
            pnlAlertasfinancieras.TabIndex = 5;
            pnlAlertasfinancieras.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblAlertasfinancierasTitle
            // 
            lblAlertasfinancierasTitle.AutoSize = true;
            lblAlertasfinancierasTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblAlertasfinancierasTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblAlertasfinancierasTitle.Location = new Point(14, 12);
            lblAlertasfinancierasTitle.Name = "lblAlertasfinancierasTitle";
            lblAlertasfinancierasTitle.Size = new Size(120, 23);
            lblAlertasfinancierasTitle.TabIndex = 0;
            lblAlertasfinancierasTitle.Text = "Alertas financieras";
            // 
            // lblAlertasfinancierasValue
            // 
            lblAlertasfinancierasValue.AutoSize = true;
            lblAlertasfinancierasValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblAlertasfinancierasValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblAlertasfinancierasValue.Location = new Point(14, 42);
            lblAlertasfinancierasValue.Name = "lblAlertasfinancierasValue";
            lblAlertasfinancierasValue.Size = new Size(120, 41);
            lblAlertasfinancierasValue.TabIndex = 1;
            lblAlertasfinancierasValue.Text = "0 %";
            // 
            // lblAlertasfinancierasDesc
            // 
            lblAlertasfinancierasDesc.AutoSize = true;
            lblAlertasfinancierasDesc.Font = new Font("Segoe UI", 8.5F);
            lblAlertasfinancierasDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblAlertasfinancierasDesc.Location = new Point(16, 84);
            lblAlertasfinancierasDesc.Name = "lblAlertasfinancierasDesc";
            lblAlertasfinancierasDesc.Size = new Size(180, 19);
            lblAlertasfinancierasDesc.TabIndex = 2;
            lblAlertasfinancierasDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlAlertasderentabilidad
            // 
            pnlAlertasderentabilidad.BackColor = Color.White;
            pnlAlertasderentabilidad.BorderStyle = BorderStyle.FixedSingle;
            pnlAlertasderentabilidad.Controls.Add(lblAlertasderentabilidadDesc);
            pnlAlertasderentabilidad.Controls.Add(lblAlertasderentabilidadValue);
            pnlAlertasderentabilidad.Controls.Add(lblAlertasderentabilidadTitle);
            pnlAlertasderentabilidad.Location = new Point(16, 772);
            pnlAlertasderentabilidad.Name = "pnlAlertasderentabilidad";
            pnlAlertasderentabilidad.Size = new Size(900, 110);
            pnlAlertasderentabilidad.TabIndex = 6;
            pnlAlertasderentabilidad.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblAlertasderentabilidadTitle
            // 
            lblAlertasderentabilidadTitle.AutoSize = true;
            lblAlertasderentabilidadTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblAlertasderentabilidadTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblAlertasderentabilidadTitle.Location = new Point(14, 12);
            lblAlertasderentabilidadTitle.Name = "lblAlertasderentabilidadTitle";
            lblAlertasderentabilidadTitle.Size = new Size(120, 23);
            lblAlertasderentabilidadTitle.TabIndex = 0;
            lblAlertasderentabilidadTitle.Text = "Alertas de rentabilidad";
            // 
            // lblAlertasderentabilidadValue
            // 
            lblAlertasderentabilidadValue.AutoSize = true;
            lblAlertasderentabilidadValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblAlertasderentabilidadValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblAlertasderentabilidadValue.Location = new Point(14, 42);
            lblAlertasderentabilidadValue.Name = "lblAlertasderentabilidadValue";
            lblAlertasderentabilidadValue.Size = new Size(120, 41);
            lblAlertasderentabilidadValue.TabIndex = 1;
            lblAlertasderentabilidadValue.Text = "RD$ 0.00";
            // 
            // lblAlertasderentabilidadDesc
            // 
            lblAlertasderentabilidadDesc.AutoSize = true;
            lblAlertasderentabilidadDesc.Font = new Font("Segoe UI", 8.5F);
            lblAlertasderentabilidadDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblAlertasderentabilidadDesc.Location = new Point(16, 84);
            lblAlertasderentabilidadDesc.Name = "lblAlertasderentabilidadDesc";
            lblAlertasderentabilidadDesc.Size = new Size(180, 19);
            lblAlertasderentabilidadDesc.TabIndex = 2;
            lblAlertasderentabilidadDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlHistorial
            // 
            pnlHistorial.BackColor = Color.White;
            pnlHistorial.BorderStyle = BorderStyle.FixedSingle;
            pnlHistorial.Controls.Add(lblHistorialDesc);
            pnlHistorial.Controls.Add(lblHistorialValue);
            pnlHistorial.Controls.Add(lblHistorialTitle);
            pnlHistorial.Location = new Point(16, 898);
            pnlHistorial.Name = "pnlHistorial";
            pnlHistorial.Size = new Size(900, 110);
            pnlHistorial.TabIndex = 7;
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
            lblHistorialValue.Text = "—";
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
            // FrmAnaAlertas
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(940, 600);
            Controls.Add(panelScroll);
            Controls.Add(panelHeaderLocal);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FrmAnaAlertas";
            Text = "Alertas";
            panelHeaderLocal.ResumeLayout(false);
            panelHeaderLocal.PerformLayout();
            panelScroll.ResumeLayout(false);
            pnlCriticas.ResumeLayout(false);
            pnlCriticas.PerformLayout();
            pnlImportantes.ResumeLayout(false);
            pnlImportantes.PerformLayout();
            pnlAdvertencias.ResumeLayout(false);
            pnlAdvertencias.PerformLayout();
            pnlInformacion.ResumeLayout(false);
            pnlInformacion.PerformLayout();
            pnlAlertasdeinventario.ResumeLayout(false);
            pnlAlertasdeinventario.PerformLayout();
            pnlAlertasfinancieras.ResumeLayout(false);
            pnlAlertasfinancieras.PerformLayout();
            pnlAlertasderentabilidad.ResumeLayout(false);
            pnlAlertasderentabilidad.PerformLayout();
            pnlHistorial.ResumeLayout(false);
            pnlHistorial.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelHeaderLocal;
        private Label lblHeaderLocal;
        private Panel panelScroll;
        private Panel pnlCriticas;
        private Label lblCriticasTitle;
        private Label lblCriticasValue;
        private Label lblCriticasDesc;
        private Panel pnlImportantes;
        private Label lblImportantesTitle;
        private Label lblImportantesValue;
        private Label lblImportantesDesc;
        private Panel pnlAdvertencias;
        private Label lblAdvertenciasTitle;
        private Label lblAdvertenciasValue;
        private Label lblAdvertenciasDesc;
        private Panel pnlInformacion;
        private Label lblInformacionTitle;
        private Label lblInformacionValue;
        private Label lblInformacionDesc;
        private Panel pnlAlertasdeinventario;
        private Label lblAlertasdeinventarioTitle;
        private Label lblAlertasdeinventarioValue;
        private Label lblAlertasdeinventarioDesc;
        private Panel pnlAlertasfinancieras;
        private Label lblAlertasfinancierasTitle;
        private Label lblAlertasfinancierasValue;
        private Label lblAlertasfinancierasDesc;
        private Panel pnlAlertasderentabilidad;
        private Label lblAlertasderentabilidadTitle;
        private Label lblAlertasderentabilidadValue;
        private Label lblAlertasderentabilidadDesc;
        private Panel pnlHistorial;
        private Label lblHistorialTitle;
        private Label lblHistorialValue;
        private Label lblHistorialDesc;
    }
}
