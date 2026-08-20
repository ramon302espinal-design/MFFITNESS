namespace UI
{
    partial class FrmAnaliticaInventario
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
            pnlValordelinventario = new Panel();
            lblValordelinventarioTitle = new Label();
            lblValordelinventarioValue = new Label();
            lblValordelinventarioDesc = new Label();
            pnlGananciapotencial = new Panel();
            lblGananciapotencialTitle = new Label();
            lblGananciapotencialValue = new Label();
            lblGananciapotencialDesc = new Label();
            pnlMargen = new Panel();
            lblMargenTitle = new Label();
            lblMargenValue = new Label();
            lblMargenDesc = new Label();
            pnlRotacion = new Panel();
            lblRotacionTitle = new Label();
            lblRotacionValue = new Label();
            lblRotacionDesc = new Label();
            pnlCapitalinmovilizado = new Panel();
            lblCapitalinmovilizadoTitle = new Label();
            lblCapitalinmovilizadoValue = new Label();
            lblCapitalinmovilizadoDesc = new Label();
            pnlTabladeproductos = new Panel();
            lblTabladeproductosTitle = new Label();
            lblTabladeproductosValue = new Label();
            lblTabladeproductosDesc = new Label();
            panelHeaderLocal.SuspendLayout();
            panelScroll.SuspendLayout();
            pnlValordelinventario.SuspendLayout();
            pnlGananciapotencial.SuspendLayout();
            pnlMargen.SuspendLayout();
            pnlRotacion.SuspendLayout();
            pnlCapitalinmovilizado.SuspendLayout();
            pnlTabladeproductos.SuspendLayout();
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
            lblHeaderLocal.Text = "Rentabilidad / Inventario";
            // 
            // panelScroll
            // 
            panelScroll.AutoScroll = true;
            panelScroll.BackColor = Color.FromArgb(247, 249, 252);
            panelScroll.Controls.Add(pnlValordelinventario);
            panelScroll.Controls.Add(pnlGananciapotencial);
            panelScroll.Controls.Add(pnlMargen);
            panelScroll.Controls.Add(pnlRotacion);
            panelScroll.Controls.Add(pnlCapitalinmovilizado);
            panelScroll.Controls.Add(pnlTabladeproductos);
            panelScroll.Dock = DockStyle.Fill;
            panelScroll.Location = new Point(0, 48);
            panelScroll.Name = "panelScroll";
            panelScroll.Padding = new Padding(8);
            panelScroll.Size = new Size(940, 552);
            panelScroll.TabIndex = 1;
            // 
            // pnlValordelinventario
            // 
            pnlValordelinventario.BackColor = Color.White;
            pnlValordelinventario.BorderStyle = BorderStyle.FixedSingle;
            pnlValordelinventario.Controls.Add(lblValordelinventarioDesc);
            pnlValordelinventario.Controls.Add(lblValordelinventarioValue);
            pnlValordelinventario.Controls.Add(lblValordelinventarioTitle);
            pnlValordelinventario.Location = new Point(16, 16);
            pnlValordelinventario.Name = "pnlValordelinventario";
            pnlValordelinventario.Size = new Size(900, 110);
            pnlValordelinventario.TabIndex = 0;
            pnlValordelinventario.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblValordelinventarioTitle
            // 
            lblValordelinventarioTitle.AutoSize = true;
            lblValordelinventarioTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblValordelinventarioTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblValordelinventarioTitle.Location = new Point(14, 12);
            lblValordelinventarioTitle.Name = "lblValordelinventarioTitle";
            lblValordelinventarioTitle.Size = new Size(120, 23);
            lblValordelinventarioTitle.TabIndex = 0;
            lblValordelinventarioTitle.Text = "Valor del inventario";
            // 
            // lblValordelinventarioValue
            // 
            lblValordelinventarioValue.AutoSize = true;
            lblValordelinventarioValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblValordelinventarioValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblValordelinventarioValue.Location = new Point(14, 42);
            lblValordelinventarioValue.Name = "lblValordelinventarioValue";
            lblValordelinventarioValue.Size = new Size(120, 41);
            lblValordelinventarioValue.TabIndex = 1;
            lblValordelinventarioValue.Text = "RD$ 0.00";
            // 
            // lblValordelinventarioDesc
            // 
            lblValordelinventarioDesc.AutoSize = true;
            lblValordelinventarioDesc.Font = new Font("Segoe UI", 8.5F);
            lblValordelinventarioDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblValordelinventarioDesc.Location = new Point(16, 84);
            lblValordelinventarioDesc.Name = "lblValordelinventarioDesc";
            lblValordelinventarioDesc.Size = new Size(180, 19);
            lblValordelinventarioDesc.TabIndex = 2;
            lblValordelinventarioDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlGananciapotencial
            // 
            pnlGananciapotencial.BackColor = Color.White;
            pnlGananciapotencial.BorderStyle = BorderStyle.FixedSingle;
            pnlGananciapotencial.Controls.Add(lblGananciapotencialDesc);
            pnlGananciapotencial.Controls.Add(lblGananciapotencialValue);
            pnlGananciapotencial.Controls.Add(lblGananciapotencialTitle);
            pnlGananciapotencial.Location = new Point(16, 142);
            pnlGananciapotencial.Name = "pnlGananciapotencial";
            pnlGananciapotencial.Size = new Size(900, 110);
            pnlGananciapotencial.TabIndex = 1;
            pnlGananciapotencial.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblGananciapotencialTitle
            // 
            lblGananciapotencialTitle.AutoSize = true;
            lblGananciapotencialTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblGananciapotencialTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblGananciapotencialTitle.Location = new Point(14, 12);
            lblGananciapotencialTitle.Name = "lblGananciapotencialTitle";
            lblGananciapotencialTitle.Size = new Size(120, 23);
            lblGananciapotencialTitle.TabIndex = 0;
            lblGananciapotencialTitle.Text = "Ganancia potencial";
            // 
            // lblGananciapotencialValue
            // 
            lblGananciapotencialValue.AutoSize = true;
            lblGananciapotencialValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblGananciapotencialValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblGananciapotencialValue.Location = new Point(14, 42);
            lblGananciapotencialValue.Name = "lblGananciapotencialValue";
            lblGananciapotencialValue.Size = new Size(120, 41);
            lblGananciapotencialValue.TabIndex = 1;
            lblGananciapotencialValue.Text = "—";
            // 
            // lblGananciapotencialDesc
            // 
            lblGananciapotencialDesc.AutoSize = true;
            lblGananciapotencialDesc.Font = new Font("Segoe UI", 8.5F);
            lblGananciapotencialDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblGananciapotencialDesc.Location = new Point(16, 84);
            lblGananciapotencialDesc.Name = "lblGananciapotencialDesc";
            lblGananciapotencialDesc.Size = new Size(180, 19);
            lblGananciapotencialDesc.TabIndex = 2;
            lblGananciapotencialDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlMargen
            // 
            pnlMargen.BackColor = Color.White;
            pnlMargen.BorderStyle = BorderStyle.FixedSingle;
            pnlMargen.Controls.Add(lblMargenDesc);
            pnlMargen.Controls.Add(lblMargenValue);
            pnlMargen.Controls.Add(lblMargenTitle);
            pnlMargen.Location = new Point(16, 268);
            pnlMargen.Name = "pnlMargen";
            pnlMargen.Size = new Size(900, 110);
            pnlMargen.TabIndex = 2;
            pnlMargen.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblMargenTitle
            // 
            lblMargenTitle.AutoSize = true;
            lblMargenTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblMargenTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblMargenTitle.Location = new Point(14, 12);
            lblMargenTitle.Name = "lblMargenTitle";
            lblMargenTitle.Size = new Size(120, 23);
            lblMargenTitle.TabIndex = 0;
            lblMargenTitle.Text = "Margen";
            // 
            // lblMargenValue
            // 
            lblMargenValue.AutoSize = true;
            lblMargenValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblMargenValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblMargenValue.Location = new Point(14, 42);
            lblMargenValue.Name = "lblMargenValue";
            lblMargenValue.Size = new Size(120, 41);
            lblMargenValue.TabIndex = 1;
            lblMargenValue.Text = "0 %";
            // 
            // lblMargenDesc
            // 
            lblMargenDesc.AutoSize = true;
            lblMargenDesc.Font = new Font("Segoe UI", 8.5F);
            lblMargenDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblMargenDesc.Location = new Point(16, 84);
            lblMargenDesc.Name = "lblMargenDesc";
            lblMargenDesc.Size = new Size(180, 19);
            lblMargenDesc.TabIndex = 2;
            lblMargenDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlRotacion
            // 
            pnlRotacion.BackColor = Color.White;
            pnlRotacion.BorderStyle = BorderStyle.FixedSingle;
            pnlRotacion.Controls.Add(lblRotacionDesc);
            pnlRotacion.Controls.Add(lblRotacionValue);
            pnlRotacion.Controls.Add(lblRotacionTitle);
            pnlRotacion.Location = new Point(16, 394);
            pnlRotacion.Name = "pnlRotacion";
            pnlRotacion.Size = new Size(900, 110);
            pnlRotacion.TabIndex = 3;
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
            lblRotacionValue.Text = "RD$ 0.00";
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
            // pnlCapitalinmovilizado
            // 
            pnlCapitalinmovilizado.BackColor = Color.White;
            pnlCapitalinmovilizado.BorderStyle = BorderStyle.FixedSingle;
            pnlCapitalinmovilizado.Controls.Add(lblCapitalinmovilizadoDesc);
            pnlCapitalinmovilizado.Controls.Add(lblCapitalinmovilizadoValue);
            pnlCapitalinmovilizado.Controls.Add(lblCapitalinmovilizadoTitle);
            pnlCapitalinmovilizado.Location = new Point(16, 520);
            pnlCapitalinmovilizado.Name = "pnlCapitalinmovilizado";
            pnlCapitalinmovilizado.Size = new Size(900, 110);
            pnlCapitalinmovilizado.TabIndex = 4;
            pnlCapitalinmovilizado.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblCapitalinmovilizadoTitle
            // 
            lblCapitalinmovilizadoTitle.AutoSize = true;
            lblCapitalinmovilizadoTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCapitalinmovilizadoTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblCapitalinmovilizadoTitle.Location = new Point(14, 12);
            lblCapitalinmovilizadoTitle.Name = "lblCapitalinmovilizadoTitle";
            lblCapitalinmovilizadoTitle.Size = new Size(120, 23);
            lblCapitalinmovilizadoTitle.TabIndex = 0;
            lblCapitalinmovilizadoTitle.Text = "Capital inmovilizado";
            // 
            // lblCapitalinmovilizadoValue
            // 
            lblCapitalinmovilizadoValue.AutoSize = true;
            lblCapitalinmovilizadoValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblCapitalinmovilizadoValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblCapitalinmovilizadoValue.Location = new Point(14, 42);
            lblCapitalinmovilizadoValue.Name = "lblCapitalinmovilizadoValue";
            lblCapitalinmovilizadoValue.Size = new Size(120, 41);
            lblCapitalinmovilizadoValue.TabIndex = 1;
            lblCapitalinmovilizadoValue.Text = "—";
            // 
            // lblCapitalinmovilizadoDesc
            // 
            lblCapitalinmovilizadoDesc.AutoSize = true;
            lblCapitalinmovilizadoDesc.Font = new Font("Segoe UI", 8.5F);
            lblCapitalinmovilizadoDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblCapitalinmovilizadoDesc.Location = new Point(16, 84);
            lblCapitalinmovilizadoDesc.Name = "lblCapitalinmovilizadoDesc";
            lblCapitalinmovilizadoDesc.Size = new Size(180, 19);
            lblCapitalinmovilizadoDesc.TabIndex = 2;
            lblCapitalinmovilizadoDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlTabladeproductos
            // 
            pnlTabladeproductos.BackColor = Color.White;
            pnlTabladeproductos.BorderStyle = BorderStyle.FixedSingle;
            pnlTabladeproductos.Controls.Add(lblTabladeproductosDesc);
            pnlTabladeproductos.Controls.Add(lblTabladeproductosValue);
            pnlTabladeproductos.Controls.Add(lblTabladeproductosTitle);
            pnlTabladeproductos.Location = new Point(16, 646);
            pnlTabladeproductos.Name = "pnlTabladeproductos";
            pnlTabladeproductos.Size = new Size(900, 110);
            pnlTabladeproductos.TabIndex = 5;
            pnlTabladeproductos.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblTabladeproductosTitle
            // 
            lblTabladeproductosTitle.AutoSize = true;
            lblTabladeproductosTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblTabladeproductosTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblTabladeproductosTitle.Location = new Point(14, 12);
            lblTabladeproductosTitle.Name = "lblTabladeproductosTitle";
            lblTabladeproductosTitle.Size = new Size(120, 23);
            lblTabladeproductosTitle.TabIndex = 0;
            lblTabladeproductosTitle.Text = "Tabla de productos";
            // 
            // lblTabladeproductosValue
            // 
            lblTabladeproductosValue.AutoSize = true;
            lblTabladeproductosValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTabladeproductosValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblTabladeproductosValue.Location = new Point(14, 42);
            lblTabladeproductosValue.Name = "lblTabladeproductosValue";
            lblTabladeproductosValue.Size = new Size(120, 41);
            lblTabladeproductosValue.TabIndex = 1;
            lblTabladeproductosValue.Text = "0 %";
            // 
            // lblTabladeproductosDesc
            // 
            lblTabladeproductosDesc.AutoSize = true;
            lblTabladeproductosDesc.Font = new Font("Segoe UI", 8.5F);
            lblTabladeproductosDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblTabladeproductosDesc.Location = new Point(16, 84);
            lblTabladeproductosDesc.Name = "lblTabladeproductosDesc";
            lblTabladeproductosDesc.Size = new Size(180, 19);
            lblTabladeproductosDesc.TabIndex = 2;
            lblTabladeproductosDesc.Text = "Dato visual mock — sin logica";
            // 
            // FrmAnaliticaInventario
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(940, 600);
            Controls.Add(panelScroll);
            Controls.Add(panelHeaderLocal);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FrmAnaliticaInventario";
            Text = "Rentabilidad / Inventario";
            panelHeaderLocal.ResumeLayout(false);
            panelHeaderLocal.PerformLayout();
            panelScroll.ResumeLayout(false);
            pnlValordelinventario.ResumeLayout(false);
            pnlValordelinventario.PerformLayout();
            pnlGananciapotencial.ResumeLayout(false);
            pnlGananciapotencial.PerformLayout();
            pnlMargen.ResumeLayout(false);
            pnlMargen.PerformLayout();
            pnlRotacion.ResumeLayout(false);
            pnlRotacion.PerformLayout();
            pnlCapitalinmovilizado.ResumeLayout(false);
            pnlCapitalinmovilizado.PerformLayout();
            pnlTabladeproductos.ResumeLayout(false);
            pnlTabladeproductos.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelHeaderLocal;
        private Label lblHeaderLocal;
        private Panel panelScroll;
        private Panel pnlValordelinventario;
        private Label lblValordelinventarioTitle;
        private Label lblValordelinventarioValue;
        private Label lblValordelinventarioDesc;
        private Panel pnlGananciapotencial;
        private Label lblGananciapotencialTitle;
        private Label lblGananciapotencialValue;
        private Label lblGananciapotencialDesc;
        private Panel pnlMargen;
        private Label lblMargenTitle;
        private Label lblMargenValue;
        private Label lblMargenDesc;
        private Panel pnlRotacion;
        private Label lblRotacionTitle;
        private Label lblRotacionValue;
        private Label lblRotacionDesc;
        private Panel pnlCapitalinmovilizado;
        private Label lblCapitalinmovilizadoTitle;
        private Label lblCapitalinmovilizadoValue;
        private Label lblCapitalinmovilizadoDesc;
        private Panel pnlTabladeproductos;
        private Label lblTabladeproductosTitle;
        private Label lblTabladeproductosValue;
        private Label lblTabladeproductosDesc;
    }
}
