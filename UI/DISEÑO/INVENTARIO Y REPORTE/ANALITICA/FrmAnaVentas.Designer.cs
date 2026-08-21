namespace UI
{
    partial class FrmAnaVentas
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
            pnlVentas = new Panel();
            lblVentasTitle = new Label();
            lblVentasValue = new Label();
            lblVentasDesc = new Label();
            pnlUnidades = new Panel();
            lblUnidadesTitle = new Label();
            lblUnidadesValue = new Label();
            lblUnidadesDesc = new Label();
            pnlGanancia = new Panel();
            lblGananciaTitle = new Label();
            lblGananciaValue = new Label();
            lblGananciaDesc = new Label();
            pnlMargen = new Panel();
            lblMargenTitle = new Label();
            lblMargenValue = new Label();
            lblMargenDesc = new Label();
            pnlPeriodo = new Panel();
            lblPeriodoTitle = new Label();
            lblPeriodoValue = new Label();
            lblPeriodoDesc = new Label();
            pnlProductos = new Panel();
            lblProductosTitle = new Label();
            lblProductosValue = new Label();
            lblProductosDesc = new Label();
            pnlCajeros = new Panel();
            lblCajerosTitle = new Label();
            lblCajerosValue = new Label();
            lblCajerosDesc = new Label();
            pnlHorario = new Panel();
            lblHorarioTitle = new Label();
            lblHorarioValue = new Label();
            lblHorarioDesc = new Label();
            pnlGrafico = new Panel();
            lblGraficoTitle = new Label();
            lblGraficoValue = new Label();
            lblGraficoDesc = new Label();
            panelHeaderLocal.SuspendLayout();
            panelScroll.SuspendLayout();
            pnlVentas.SuspendLayout();
            pnlUnidades.SuspendLayout();
            pnlGanancia.SuspendLayout();
            pnlMargen.SuspendLayout();
            pnlPeriodo.SuspendLayout();
            pnlProductos.SuspendLayout();
            pnlCajeros.SuspendLayout();
            pnlHorario.SuspendLayout();
            pnlGrafico.SuspendLayout();
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
            lblHeaderLocal.Text = "Ventas";
            // 
            // panelScroll
            // 
            panelScroll.AutoScroll = true;
            panelScroll.BackColor = Color.FromArgb(247, 249, 252);
            panelScroll.Controls.Add(pnlVentas);
            panelScroll.Controls.Add(pnlUnidades);
            panelScroll.Controls.Add(pnlGanancia);
            panelScroll.Controls.Add(pnlMargen);
            panelScroll.Controls.Add(pnlPeriodo);
            panelScroll.Controls.Add(pnlProductos);
            panelScroll.Controls.Add(pnlCajeros);
            panelScroll.Controls.Add(pnlHorario);
            panelScroll.Controls.Add(pnlGrafico);
            panelScroll.Dock = DockStyle.Fill;
            panelScroll.Location = new Point(0, 64);
            panelScroll.Name = "panelScroll";
            panelScroll.Padding = new Padding(8);
            panelScroll.Size = new Size(940, 552);
            panelScroll.TabIndex = 1;
            // 
            // pnlVentas
            // 
            pnlVentas.BackColor = Color.White;
            pnlVentas.BorderStyle = BorderStyle.FixedSingle;
            pnlVentas.Controls.Add(lblVentasDesc);
            pnlVentas.Controls.Add(lblVentasValue);
            pnlVentas.Controls.Add(lblVentasTitle);
            pnlVentas.Location = new Point(16, 16);
            pnlVentas.Name = "pnlVentas";
            pnlVentas.Size = new Size(900, 110);
            pnlVentas.TabIndex = 0;
            pnlVentas.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblVentasTitle
            // 
            lblVentasTitle.AutoSize = true;
            lblVentasTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblVentasTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblVentasTitle.Location = new Point(14, 12);
            lblVentasTitle.Name = "lblVentasTitle";
            lblVentasTitle.Size = new Size(120, 23);
            lblVentasTitle.TabIndex = 0;
            lblVentasTitle.Text = "Ventas";
            // 
            // lblVentasValue
            // 
            lblVentasValue.AutoSize = true;
            lblVentasValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblVentasValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblVentasValue.Location = new Point(14, 42);
            lblVentasValue.Name = "lblVentasValue";
            lblVentasValue.Size = new Size(120, 41);
            lblVentasValue.TabIndex = 1;
            lblVentasValue.Text = "RD$ 0.00";
            // 
            // lblVentasDesc
            // 
            lblVentasDesc.AutoSize = true;
            lblVentasDesc.Font = new Font("Segoe UI", 8.5F);
            lblVentasDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblVentasDesc.Location = new Point(16, 84);
            lblVentasDesc.Name = "lblVentasDesc";
            lblVentasDesc.Size = new Size(180, 19);
            lblVentasDesc.TabIndex = 2;
            lblVentasDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlUnidades
            // 
            pnlUnidades.BackColor = Color.White;
            pnlUnidades.BorderStyle = BorderStyle.FixedSingle;
            pnlUnidades.Controls.Add(lblUnidadesDesc);
            pnlUnidades.Controls.Add(lblUnidadesValue);
            pnlUnidades.Controls.Add(lblUnidadesTitle);
            pnlUnidades.Location = new Point(16, 142);
            pnlUnidades.Name = "pnlUnidades";
            pnlUnidades.Size = new Size(900, 110);
            pnlUnidades.TabIndex = 1;
            pnlUnidades.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblUnidadesTitle
            // 
            lblUnidadesTitle.AutoSize = true;
            lblUnidadesTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblUnidadesTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblUnidadesTitle.Location = new Point(14, 12);
            lblUnidadesTitle.Name = "lblUnidadesTitle";
            lblUnidadesTitle.Size = new Size(120, 23);
            lblUnidadesTitle.TabIndex = 0;
            lblUnidadesTitle.Text = "Unidades";
            // 
            // lblUnidadesValue
            // 
            lblUnidadesValue.AutoSize = true;
            lblUnidadesValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblUnidadesValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblUnidadesValue.Location = new Point(14, 42);
            lblUnidadesValue.Name = "lblUnidadesValue";
            lblUnidadesValue.Size = new Size(120, 41);
            lblUnidadesValue.TabIndex = 1;
            lblUnidadesValue.Text = "—";
            // 
            // lblUnidadesDesc
            // 
            lblUnidadesDesc.AutoSize = true;
            lblUnidadesDesc.Font = new Font("Segoe UI", 8.5F);
            lblUnidadesDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblUnidadesDesc.Location = new Point(16, 84);
            lblUnidadesDesc.Name = "lblUnidadesDesc";
            lblUnidadesDesc.Size = new Size(180, 19);
            lblUnidadesDesc.TabIndex = 2;
            lblUnidadesDesc.Text = "Dato visual mock — sin logica";
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
            // pnlMargen
            // 
            pnlMargen.BackColor = Color.White;
            pnlMargen.BorderStyle = BorderStyle.FixedSingle;
            pnlMargen.Controls.Add(lblMargenDesc);
            pnlMargen.Controls.Add(lblMargenValue);
            pnlMargen.Controls.Add(lblMargenTitle);
            pnlMargen.Location = new Point(16, 394);
            pnlMargen.Name = "pnlMargen";
            pnlMargen.Size = new Size(900, 110);
            pnlMargen.TabIndex = 3;
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
            lblMargenValue.Text = "RD$ 0.00";
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
            // pnlPeriodo
            // 
            pnlPeriodo.BackColor = Color.White;
            pnlPeriodo.BorderStyle = BorderStyle.FixedSingle;
            pnlPeriodo.Controls.Add(lblPeriodoDesc);
            pnlPeriodo.Controls.Add(lblPeriodoValue);
            pnlPeriodo.Controls.Add(lblPeriodoTitle);
            pnlPeriodo.Location = new Point(16, 520);
            pnlPeriodo.Name = "pnlPeriodo";
            pnlPeriodo.Size = new Size(900, 110);
            pnlPeriodo.TabIndex = 4;
            pnlPeriodo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblPeriodoTitle
            // 
            lblPeriodoTitle.AutoSize = true;
            lblPeriodoTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblPeriodoTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblPeriodoTitle.Location = new Point(14, 12);
            lblPeriodoTitle.Name = "lblPeriodoTitle";
            lblPeriodoTitle.Size = new Size(120, 23);
            lblPeriodoTitle.TabIndex = 0;
            lblPeriodoTitle.Text = "Periodo";
            // 
            // lblPeriodoValue
            // 
            lblPeriodoValue.AutoSize = true;
            lblPeriodoValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblPeriodoValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblPeriodoValue.Location = new Point(14, 42);
            lblPeriodoValue.Name = "lblPeriodoValue";
            lblPeriodoValue.Size = new Size(120, 41);
            lblPeriodoValue.TabIndex = 1;
            lblPeriodoValue.Text = "—";
            // 
            // lblPeriodoDesc
            // 
            lblPeriodoDesc.AutoSize = true;
            lblPeriodoDesc.Font = new Font("Segoe UI", 8.5F);
            lblPeriodoDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblPeriodoDesc.Location = new Point(16, 84);
            lblPeriodoDesc.Name = "lblPeriodoDesc";
            lblPeriodoDesc.Size = new Size(180, 19);
            lblPeriodoDesc.TabIndex = 2;
            lblPeriodoDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlProductos
            // 
            pnlProductos.BackColor = Color.White;
            pnlProductos.BorderStyle = BorderStyle.FixedSingle;
            pnlProductos.Controls.Add(lblProductosDesc);
            pnlProductos.Controls.Add(lblProductosValue);
            pnlProductos.Controls.Add(lblProductosTitle);
            pnlProductos.Location = new Point(16, 646);
            pnlProductos.Name = "pnlProductos";
            pnlProductos.Size = new Size(900, 110);
            pnlProductos.TabIndex = 5;
            pnlProductos.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblProductosTitle
            // 
            lblProductosTitle.AutoSize = true;
            lblProductosTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblProductosTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblProductosTitle.Location = new Point(14, 12);
            lblProductosTitle.Name = "lblProductosTitle";
            lblProductosTitle.Size = new Size(120, 23);
            lblProductosTitle.TabIndex = 0;
            lblProductosTitle.Text = "Productos";
            // 
            // lblProductosValue
            // 
            lblProductosValue.AutoSize = true;
            lblProductosValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblProductosValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblProductosValue.Location = new Point(14, 42);
            lblProductosValue.Name = "lblProductosValue";
            lblProductosValue.Size = new Size(120, 41);
            lblProductosValue.TabIndex = 1;
            lblProductosValue.Text = "0 %";
            // 
            // lblProductosDesc
            // 
            lblProductosDesc.AutoSize = true;
            lblProductosDesc.Font = new Font("Segoe UI", 8.5F);
            lblProductosDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblProductosDesc.Location = new Point(16, 84);
            lblProductosDesc.Name = "lblProductosDesc";
            lblProductosDesc.Size = new Size(180, 19);
            lblProductosDesc.TabIndex = 2;
            lblProductosDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlCajeros
            // 
            pnlCajeros.BackColor = Color.White;
            pnlCajeros.BorderStyle = BorderStyle.FixedSingle;
            pnlCajeros.Controls.Add(lblCajerosDesc);
            pnlCajeros.Controls.Add(lblCajerosValue);
            pnlCajeros.Controls.Add(lblCajerosTitle);
            pnlCajeros.Location = new Point(16, 772);
            pnlCajeros.Name = "pnlCajeros";
            pnlCajeros.Size = new Size(900, 110);
            pnlCajeros.TabIndex = 6;
            pnlCajeros.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblCajerosTitle
            // 
            lblCajerosTitle.AutoSize = true;
            lblCajerosTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCajerosTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblCajerosTitle.Location = new Point(14, 12);
            lblCajerosTitle.Name = "lblCajerosTitle";
            lblCajerosTitle.Size = new Size(120, 23);
            lblCajerosTitle.TabIndex = 0;
            lblCajerosTitle.Text = "Cajeros";
            // 
            // lblCajerosValue
            // 
            lblCajerosValue.AutoSize = true;
            lblCajerosValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblCajerosValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblCajerosValue.Location = new Point(14, 42);
            lblCajerosValue.Name = "lblCajerosValue";
            lblCajerosValue.Size = new Size(120, 41);
            lblCajerosValue.TabIndex = 1;
            lblCajerosValue.Text = "RD$ 0.00";
            // 
            // lblCajerosDesc
            // 
            lblCajerosDesc.AutoSize = true;
            lblCajerosDesc.Font = new Font("Segoe UI", 8.5F);
            lblCajerosDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblCajerosDesc.Location = new Point(16, 84);
            lblCajerosDesc.Name = "lblCajerosDesc";
            lblCajerosDesc.Size = new Size(180, 19);
            lblCajerosDesc.TabIndex = 2;
            lblCajerosDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlHorario
            // 
            pnlHorario.BackColor = Color.White;
            pnlHorario.BorderStyle = BorderStyle.FixedSingle;
            pnlHorario.Controls.Add(lblHorarioDesc);
            pnlHorario.Controls.Add(lblHorarioValue);
            pnlHorario.Controls.Add(lblHorarioTitle);
            pnlHorario.Location = new Point(16, 898);
            pnlHorario.Name = "pnlHorario";
            pnlHorario.Size = new Size(900, 110);
            pnlHorario.TabIndex = 7;
            pnlHorario.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblHorarioTitle
            // 
            lblHorarioTitle.AutoSize = true;
            lblHorarioTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblHorarioTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblHorarioTitle.Location = new Point(14, 12);
            lblHorarioTitle.Name = "lblHorarioTitle";
            lblHorarioTitle.Size = new Size(120, 23);
            lblHorarioTitle.TabIndex = 0;
            lblHorarioTitle.Text = "Horario";
            // 
            // lblHorarioValue
            // 
            lblHorarioValue.AutoSize = true;
            lblHorarioValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblHorarioValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblHorarioValue.Location = new Point(14, 42);
            lblHorarioValue.Name = "lblHorarioValue";
            lblHorarioValue.Size = new Size(120, 41);
            lblHorarioValue.TabIndex = 1;
            lblHorarioValue.Text = "—";
            // 
            // lblHorarioDesc
            // 
            lblHorarioDesc.AutoSize = true;
            lblHorarioDesc.Font = new Font("Segoe UI", 8.5F);
            lblHorarioDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblHorarioDesc.Location = new Point(16, 84);
            lblHorarioDesc.Name = "lblHorarioDesc";
            lblHorarioDesc.Size = new Size(180, 19);
            lblHorarioDesc.TabIndex = 2;
            lblHorarioDesc.Text = "Dato visual mock — sin logica";
            // 
            // pnlGrafico
            // 
            pnlGrafico.BackColor = Color.White;
            pnlGrafico.BorderStyle = BorderStyle.FixedSingle;
            pnlGrafico.Controls.Add(lblGraficoDesc);
            pnlGrafico.Controls.Add(lblGraficoValue);
            pnlGrafico.Controls.Add(lblGraficoTitle);
            pnlGrafico.Location = new Point(16, 1024);
            pnlGrafico.Name = "pnlGrafico";
            pnlGrafico.Size = new Size(900, 110);
            pnlGrafico.TabIndex = 8;
            pnlGrafico.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // lblGraficoTitle
            // 
            lblGraficoTitle.AutoSize = true;
            lblGraficoTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblGraficoTitle.ForeColor = Color.FromArgb(45, 55, 72);
            lblGraficoTitle.Location = new Point(14, 12);
            lblGraficoTitle.Name = "lblGraficoTitle";
            lblGraficoTitle.Size = new Size(120, 23);
            lblGraficoTitle.TabIndex = 0;
            lblGraficoTitle.Text = "Grafico";
            // 
            // lblGraficoValue
            // 
            lblGraficoValue.AutoSize = true;
            lblGraficoValue.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblGraficoValue.ForeColor = Color.FromArgb(26, 32, 44);
            lblGraficoValue.Location = new Point(14, 42);
            lblGraficoValue.Name = "lblGraficoValue";
            lblGraficoValue.Size = new Size(120, 41);
            lblGraficoValue.TabIndex = 1;
            lblGraficoValue.Text = "0 %";
            // 
            // lblGraficoDesc
            // 
            lblGraficoDesc.AutoSize = true;
            lblGraficoDesc.Font = new Font("Segoe UI", 8.5F);
            lblGraficoDesc.ForeColor = Color.FromArgb(113, 128, 150);
            lblGraficoDesc.Location = new Point(16, 84);
            lblGraficoDesc.Name = "lblGraficoDesc";
            lblGraficoDesc.Size = new Size(180, 19);
            lblGraficoDesc.TabIndex = 2;
            lblGraficoDesc.Text = "Dato visual mock — sin logica";
            // 
            // FrmAnaVentas
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(940, 600);
            Controls.Add(panelScroll);
            Controls.Add(panelHeaderLocal);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FrmAnaVentas";
            Text = "Ventas";
            panelHeaderLocal.ResumeLayout(false);
            panelHeaderLocal.PerformLayout();
            panelScroll.ResumeLayout(false);
            pnlVentas.ResumeLayout(false);
            pnlVentas.PerformLayout();
            pnlUnidades.ResumeLayout(false);
            pnlUnidades.PerformLayout();
            pnlGanancia.ResumeLayout(false);
            pnlGanancia.PerformLayout();
            pnlMargen.ResumeLayout(false);
            pnlMargen.PerformLayout();
            pnlPeriodo.ResumeLayout(false);
            pnlPeriodo.PerformLayout();
            pnlProductos.ResumeLayout(false);
            pnlProductos.PerformLayout();
            pnlCajeros.ResumeLayout(false);
            pnlCajeros.PerformLayout();
            pnlHorario.ResumeLayout(false);
            pnlHorario.PerformLayout();
            pnlGrafico.ResumeLayout(false);
            pnlGrafico.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelHeaderLocal;
        private Label lblHeaderLocal;
        private Label lblCrmHint;
        private Panel panelScroll;
        private Panel pnlVentas;
        private Label lblVentasTitle;
        private Label lblVentasValue;
        private Label lblVentasDesc;
        private Panel pnlUnidades;
        private Label lblUnidadesTitle;
        private Label lblUnidadesValue;
        private Label lblUnidadesDesc;
        private Panel pnlGanancia;
        private Label lblGananciaTitle;
        private Label lblGananciaValue;
        private Label lblGananciaDesc;
        private Panel pnlMargen;
        private Label lblMargenTitle;
        private Label lblMargenValue;
        private Label lblMargenDesc;
        private Panel pnlPeriodo;
        private Label lblPeriodoTitle;
        private Label lblPeriodoValue;
        private Label lblPeriodoDesc;
        private Panel pnlProductos;
        private Label lblProductosTitle;
        private Label lblProductosValue;
        private Label lblProductosDesc;
        private Panel pnlCajeros;
        private Label lblCajerosTitle;
        private Label lblCajerosValue;
        private Label lblCajerosDesc;
        private Panel pnlHorario;
        private Label lblHorarioTitle;
        private Label lblHorarioValue;
        private Label lblHorarioDesc;
        private Panel pnlGrafico;
        private Label lblGraficoTitle;
        private Label lblGraficoValue;
        private Label lblGraficoDesc;
    }
}
