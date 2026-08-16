namespace UI.DISEÑO
{
    partial class FrmPago
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            tlpRoot = new TableLayoutPanel();
            pnlHeader = new Panel();
            lblIconoHeader = new Label();
            lblTitulo = new Label();
            btnCerrar = new Button();
            tlpContent = new TableLayoutPanel();
            pnlIzquierda = new Panel();
            cardTotal = new Panel();
            lblIconoTotal = new Label();
            lblTotalEtiqueta = new Label();
            lblTotalMonto = new Label();
            lblMetodoTitulo = new Label();
            cmbMetodo = new ComboBox();
            lblMontoTitulo = new Label();
            lblMontoSubtexto = new Label();
            tlpBilletes = new TableLayoutPanel();
            btnBillete50 = new Button();
            btnBillete100 = new Button();
            btnBillete200 = new Button();
            btnBillete500 = new Button();
            btnBillete1000 = new Button();
            btnBillete2000 = new Button();
            pnlMontoInput = new Panel();
            lblIconoMonto = new Label();
            txtMontoRecibido = new TextBox();
            pnlDerecha = new Panel();
            cardCambio = new Panel();
            tlpCambio = new TableLayoutPanel();
            lblCambioTitulo = new Label();
            lblIconoCambio = new Label();
            lblCambioMonto = new Label();
            pnlFooter = new Panel();
            tlpFooterTop = new TableLayoutPanel();
            chkImprimirRecibo = new CheckBox();
            btnVistaPrevia = new Button();
            btnPagar = new Button();
            tlpRoot.SuspendLayout();
            pnlHeader.SuspendLayout();
            tlpContent.SuspendLayout();
            pnlIzquierda.SuspendLayout();
            cardTotal.SuspendLayout();
            tlpBilletes.SuspendLayout();
            pnlMontoInput.SuspendLayout();
            pnlDerecha.SuspendLayout();
            cardCambio.SuspendLayout();
            tlpCambio.SuspendLayout();
            pnlFooter.SuspendLayout();
            tlpFooterTop.SuspendLayout();
            SuspendLayout();
            // 
            // tlpRoot
            // 
            tlpRoot.BackColor = Color.FromArgb(248, 249, 251);
            tlpRoot.ColumnCount = 1;
            tlpRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpRoot.Controls.Add(pnlHeader, 0, 0);
            tlpRoot.Controls.Add(tlpContent, 0, 1);
            tlpRoot.Controls.Add(pnlFooter, 0, 2);
            tlpRoot.Dock = DockStyle.Fill;
            tlpRoot.Location = new Point(0, 0);
            tlpRoot.Name = "tlpRoot";
            tlpRoot.Padding = new Padding(30);
            tlpRoot.RowCount = 3;
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 180F));
            tlpRoot.Size = new Size(1150, 780);
            tlpRoot.TabIndex = 0;
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.Transparent;
            pnlHeader.Controls.Add(lblIconoHeader);
            pnlHeader.Controls.Add(lblTitulo);
            pnlHeader.Controls.Add(btnCerrar);
            pnlHeader.Dock = DockStyle.Fill;
            pnlHeader.Location = new Point(33, 33);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(1084, 74);
            pnlHeader.TabIndex = 0;
            // 
            // lblIconoHeader
            // 
            lblIconoHeader.Font = new Font("Segoe MDL2 Assets", 28F);
            lblIconoHeader.ForeColor = Color.FromArgb(26, 140, 255);
            lblIconoHeader.Location = new Point(0, 12);
            lblIconoHeader.Name = "lblIconoHeader";
            lblIconoHeader.Size = new Size(48, 48);
            lblIconoHeader.TabIndex = 0;
            lblIconoHeader.Text = "\uE8C7";
            lblIconoHeader.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 32F, FontStyle.Bold, GraphicsUnit.Pixel);
            lblTitulo.ForeColor = Color.FromArgb(31, 41, 55);
            lblTitulo.Location = new Point(56, 16);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(220, 42);
            lblTitulo.TabIndex = 1;
            lblTitulo.Text = "PAGAR VENTA";
            // 
            // btnCerrar
            // 
            btnCerrar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCerrar.BackColor = Color.Transparent;
            btnCerrar.Cursor = Cursors.Hand;
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.FlatAppearance.MouseOverBackColor = Color.FromArgb(241, 245, 249);
            btnCerrar.FlatStyle = FlatStyle.Flat;
            btnCerrar.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            btnCerrar.ForeColor = Color.FromArgb(31, 41, 55);
            btnCerrar.Location = new Point(1036, 12);
            btnCerrar.Name = "btnCerrar";
            btnCerrar.Size = new Size(48, 48);
            btnCerrar.TabIndex = 2;
            btnCerrar.Text = "X";
            btnCerrar.UseVisualStyleBackColor = false;
            // 
            // tlpContent
            // 
            tlpContent.ColumnCount = 2;
            tlpContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            tlpContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            tlpContent.Controls.Add(pnlIzquierda, 0, 0);
            tlpContent.Controls.Add(pnlDerecha, 1, 0);
            tlpContent.Dock = DockStyle.Fill;
            tlpContent.Location = new Point(33, 113);
            tlpContent.Name = "tlpContent";
            tlpContent.RowCount = 1;
            tlpContent.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpContent.Size = new Size(1084, 434);
            tlpContent.TabIndex = 1;
            // 
            // pnlIzquierda
            // 
            pnlIzquierda.AutoScroll = true;
            pnlIzquierda.BackColor = Color.Transparent;
            pnlIzquierda.Controls.Add(cardTotal);
            pnlIzquierda.Controls.Add(lblMetodoTitulo);
            pnlIzquierda.Controls.Add(cmbMetodo);
            pnlIzquierda.Controls.Add(lblMontoTitulo);
            pnlIzquierda.Controls.Add(lblMontoSubtexto);
            pnlIzquierda.Controls.Add(tlpBilletes);
            pnlIzquierda.Controls.Add(pnlMontoInput);
            pnlIzquierda.Dock = DockStyle.Fill;
            pnlIzquierda.Location = new Point(3, 3);
            pnlIzquierda.Name = "pnlIzquierda";
            pnlIzquierda.Padding = new Padding(0, 0, 16, 0);
            pnlIzquierda.Size = new Size(698, 428);
            pnlIzquierda.TabIndex = 0;
            // 
            // cardTotal
            // 
            cardTotal.BackColor = Color.White;
            cardTotal.Controls.Add(lblIconoTotal);
            cardTotal.Controls.Add(lblTotalEtiqueta);
            cardTotal.Controls.Add(lblTotalMonto);
            cardTotal.Location = new Point(0, 0);
            cardTotal.Name = "cardTotal";
            cardTotal.Size = new Size(620, 110);
            cardTotal.TabIndex = 0;
            // 
            // lblIconoTotal
            // 
            lblIconoTotal.Font = new Font("Segoe MDL2 Assets", 36F);
            lblIconoTotal.ForeColor = Color.FromArgb(26, 140, 255);
            lblIconoTotal.Location = new Point(20, 16);
            lblIconoTotal.Name = "lblIconoTotal";
            lblIconoTotal.Size = new Size(64, 78);
            lblIconoTotal.TabIndex = 0;
            lblIconoTotal.Text = "\uE8C8";
            lblIconoTotal.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblTotalEtiqueta
            // 
            lblTotalEtiqueta.AutoSize = true;
            lblTotalEtiqueta.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Pixel);
            lblTotalEtiqueta.ForeColor = Color.FromArgb(107, 114, 128);
            lblTotalEtiqueta.Location = new Point(96, 18);
            lblTotalEtiqueta.Name = "lblTotalEtiqueta";
            lblTotalEtiqueta.Size = new Size(160, 24);
            lblTotalEtiqueta.TabIndex = 1;
            lblTotalEtiqueta.Text = "TOTAL A PAGAR";
            // 
            // lblTotalMonto
            // 
            lblTotalMonto.Font = new Font("Segoe UI", 40F, FontStyle.Bold, GraphicsUnit.Pixel);
            lblTotalMonto.ForeColor = Color.FromArgb(26, 140, 255);
            lblTotalMonto.Location = new Point(96, 46);
            lblTotalMonto.Name = "lblTotalMonto";
            lblTotalMonto.Size = new Size(500, 52);
            lblTotalMonto.TabIndex = 2;
            lblTotalMonto.Text = "RD$0.00";
            lblTotalMonto.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblMetodoTitulo
            // 
            lblMetodoTitulo.AutoSize = true;
            lblMetodoTitulo.Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Pixel);
            lblMetodoTitulo.ForeColor = Color.FromArgb(31, 41, 55);
            lblMetodoTitulo.Location = new Point(0, 126);
            lblMetodoTitulo.Name = "lblMetodoTitulo";
            lblMetodoTitulo.Size = new Size(180, 27);
            lblMetodoTitulo.TabIndex = 1;
            lblMetodoTitulo.Text = "MÉTODO DE PAGO";
            // 
            // cmbMetodo
            // 
            cmbMetodo.BackColor = Color.White;
            cmbMetodo.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMetodo.FlatStyle = FlatStyle.Flat;
            cmbMetodo.Font = new Font("Segoe UI", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            cmbMetodo.FormattingEnabled = true;
            cmbMetodo.Items.AddRange(new object[] { "Efectivo", "Tarjeta", "Transferencia" });
            cmbMetodo.Location = new Point(0, 158);
            cmbMetodo.Name = "cmbMetodo";
            cmbMetodo.Size = new Size(620, 28);
            cmbMetodo.TabIndex = 2;
            // 
            // lblMontoTitulo
            // 
            lblMontoTitulo.AutoSize = true;
            lblMontoTitulo.Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Pixel);
            lblMontoTitulo.ForeColor = Color.FromArgb(31, 41, 55);
            lblMontoTitulo.Location = new Point(0, 206);
            lblMontoTitulo.Name = "lblMontoTitulo";
            lblMontoTitulo.Size = new Size(190, 27);
            lblMontoTitulo.TabIndex = 3;
            lblMontoTitulo.Text = "MONTO RECIBIDO";
            // 
            // lblMontoSubtexto
            // 
            lblMontoSubtexto.AutoSize = true;
            lblMontoSubtexto.Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblMontoSubtexto.ForeColor = Color.FromArgb(107, 114, 128);
            lblMontoSubtexto.Location = new Point(0, 236);
            lblMontoSubtexto.Name = "lblMontoSubtexto";
            lblMontoSubtexto.Size = new Size(480, 19);
            lblMontoSubtexto.TabIndex = 4;
            lblMontoSubtexto.Text = "Selecciona un monto rápido o escribe el monto recibido.";
            // 
            // tlpBilletes
            // 
            tlpBilletes.ColumnCount = 3;
            tlpBilletes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tlpBilletes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tlpBilletes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
            tlpBilletes.Controls.Add(btnBillete50, 0, 0);
            tlpBilletes.Controls.Add(btnBillete100, 1, 0);
            tlpBilletes.Controls.Add(btnBillete200, 2, 0);
            tlpBilletes.Controls.Add(btnBillete500, 0, 1);
            tlpBilletes.Controls.Add(btnBillete1000, 1, 1);
            tlpBilletes.Controls.Add(btnBillete2000, 2, 1);
            tlpBilletes.Location = new Point(0, 262);
            tlpBilletes.Name = "tlpBilletes";
            tlpBilletes.RowCount = 2;
            tlpBilletes.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tlpBilletes.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tlpBilletes.Size = new Size(620, 128);
            tlpBilletes.TabIndex = 5;
            ConfigurarBillete(btnBillete50, "RD$50");
            ConfigurarBillete(btnBillete100, "RD$100");
            ConfigurarBillete(btnBillete200, "RD$200");
            ConfigurarBillete(btnBillete500, "RD$500");
            ConfigurarBillete(btnBillete1000, "RD$1000");
            ConfigurarBillete(btnBillete2000, "RD$2000");
            // 
            // pnlMontoInput
            // 
            pnlMontoInput.BackColor = Color.White;
            pnlMontoInput.Controls.Add(lblIconoMonto);
            pnlMontoInput.Controls.Add(txtMontoRecibido);
            pnlMontoInput.Location = new Point(0, 400);
            pnlMontoInput.Name = "pnlMontoInput";
            pnlMontoInput.Size = new Size(620, 58);
            pnlMontoInput.TabIndex = 6;
            // 
            // lblIconoMonto
            // 
            lblIconoMonto.Font = new Font("Segoe MDL2 Assets", 18F);
            lblIconoMonto.ForeColor = Color.FromArgb(107, 114, 128);
            lblIconoMonto.Location = new Point(12, 12);
            lblIconoMonto.Name = "lblIconoMonto";
            lblIconoMonto.Size = new Size(36, 36);
            lblIconoMonto.TabIndex = 0;
            lblIconoMonto.Text = "\uE8C8";
            lblIconoMonto.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtMontoRecibido
            // 
            txtMontoRecibido.BorderStyle = BorderStyle.None;
            txtMontoRecibido.Font = new Font("Segoe UI", 22F, FontStyle.Bold, GraphicsUnit.Pixel);
            txtMontoRecibido.ForeColor = Color.FromArgb(31, 41, 55);
            txtMontoRecibido.Location = new Point(54, 16);
            txtMontoRecibido.Name = "txtMontoRecibido";
            txtMontoRecibido.PlaceholderText = "Ingrese el monto recibido";
            txtMontoRecibido.Size = new Size(540, 29);
            txtMontoRecibido.TabIndex = 1;
            // 
            // pnlDerecha
            // 
            pnlDerecha.BackColor = Color.Transparent;
            pnlDerecha.Controls.Add(cardCambio);
            pnlDerecha.Dock = DockStyle.Fill;
            pnlDerecha.Location = new Point(707, 3);
            pnlDerecha.Name = "pnlDerecha";
            pnlDerecha.Padding = new Padding(8, 0, 0, 0);
            pnlDerecha.Size = new Size(374, 428);
            pnlDerecha.TabIndex = 1;
            // 
            // cardCambio
            // 
            cardCambio.BackColor = Color.FromArgb(26, 140, 255);
            cardCambio.Controls.Add(tlpCambio);
            cardCambio.Dock = DockStyle.Fill;
            cardCambio.Location = new Point(8, 0);
            cardCambio.Name = "cardCambio";
            cardCambio.Padding = new Padding(28);
            cardCambio.Size = new Size(366, 428);
            cardCambio.TabIndex = 0;
            // 
            // tlpCambio
            // 
            tlpCambio.BackColor = Color.Transparent;
            tlpCambio.ColumnCount = 1;
            tlpCambio.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpCambio.Controls.Add(lblCambioTitulo, 0, 0);
            tlpCambio.Controls.Add(lblIconoCambio, 0, 1);
            tlpCambio.Controls.Add(lblCambioMonto, 0, 2);
            tlpCambio.Dock = DockStyle.Fill;
            tlpCambio.Location = new Point(28, 28);
            tlpCambio.Name = "tlpCambio";
            tlpCambio.RowCount = 3;
            tlpCambio.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            tlpCambio.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));
            tlpCambio.RowStyles.Add(new RowStyle(SizeType.Percent, 55F));
            tlpCambio.Size = new Size(310, 372);
            tlpCambio.TabIndex = 0;
            // 
            // lblCambioTitulo
            // 
            lblCambioTitulo.Dock = DockStyle.Fill;
            lblCambioTitulo.Font = new Font("Segoe UI", 28F, FontStyle.Bold, GraphicsUnit.Pixel);
            lblCambioTitulo.ForeColor = Color.White;
            lblCambioTitulo.Location = new Point(0, 0);
            lblCambioTitulo.Margin = new Padding(0, 0, 0, 8);
            lblCambioTitulo.Name = "lblCambioTitulo";
            lblCambioTitulo.Size = new Size(310, 40);
            lblCambioTitulo.TabIndex = 0;
            lblCambioTitulo.Text = "CAMBIO";
            lblCambioTitulo.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblIconoCambio
            // 
            lblIconoCambio.Dock = DockStyle.Fill;
            lblIconoCambio.Font = new Font("Segoe MDL2 Assets", 56F);
            lblIconoCambio.ForeColor = Color.FromArgb(230, 255, 255, 255);
            lblIconoCambio.Location = new Point(0, 48);
            lblIconoCambio.Margin = new Padding(0);
            lblIconoCambio.Name = "lblIconoCambio";
            lblIconoCambio.Size = new Size(310, 146);
            lblIconoCambio.TabIndex = 1;
            lblIconoCambio.Text = "\uE8C7";
            lblIconoCambio.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblCambioMonto
            // 
            lblCambioMonto.Dock = DockStyle.Fill;
            lblCambioMonto.Font = new Font("Segoe UI", 40F, FontStyle.Bold, GraphicsUnit.Pixel);
            lblCambioMonto.ForeColor = Color.White;
            lblCambioMonto.Location = new Point(0, 194);
            lblCambioMonto.Margin = new Padding(0, 12, 0, 0);
            lblCambioMonto.Name = "lblCambioMonto";
            lblCambioMonto.Size = new Size(310, 178);
            lblCambioMonto.TabIndex = 2;
            lblCambioMonto.Text = "RD$0.00";
            lblCambioMonto.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pnlFooter
            // 
            pnlFooter.BackColor = Color.Transparent;
            pnlFooter.Controls.Add(tlpFooterTop);
            pnlFooter.Controls.Add(btnPagar);
            pnlFooter.Dock = DockStyle.Fill;
            pnlFooter.Location = new Point(33, 553);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Size = new Size(1084, 194);
            pnlFooter.TabIndex = 2;
            // 
            // tlpFooterTop
            // 
            tlpFooterTop.ColumnCount = 2;
            tlpFooterTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpFooterTop.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F));
            tlpFooterTop.Controls.Add(chkImprimirRecibo, 0, 0);
            tlpFooterTop.Controls.Add(btnVistaPrevia, 1, 0);
            tlpFooterTop.Dock = DockStyle.Top;
            tlpFooterTop.Location = new Point(0, 0);
            tlpFooterTop.Name = "tlpFooterTop";
            tlpFooterTop.RowCount = 1;
            tlpFooterTop.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpFooterTop.Size = new Size(1084, 70);
            tlpFooterTop.TabIndex = 0;
            // 
            // chkImprimirRecibo
            // 
            chkImprimirRecibo.Anchor = AnchorStyles.Left;
            chkImprimirRecibo.AutoSize = true;
            chkImprimirRecibo.Checked = true;
            chkImprimirRecibo.CheckState = CheckState.Checked;
            chkImprimirRecibo.Font = new Font("Segoe UI", 18F, FontStyle.Regular, GraphicsUnit.Pixel);
            chkImprimirRecibo.ForeColor = Color.FromArgb(31, 41, 55);
            chkImprimirRecibo.Location = new Point(3, 22);
            chkImprimirRecibo.Name = "chkImprimirRecibo";
            chkImprimirRecibo.Size = new Size(170, 26);
            chkImprimirRecibo.TabIndex = 0;
            chkImprimirRecibo.Text = "Imprimir recibo";
            chkImprimirRecibo.UseVisualStyleBackColor = true;
            // 
            // btnVistaPrevia
            // 
            btnVistaPrevia.Anchor = AnchorStyles.Right;
            btnVistaPrevia.BackColor = Color.White;
            btnVistaPrevia.Cursor = Cursors.Hand;
            btnVistaPrevia.FlatAppearance.BorderColor = Color.FromArgb(214, 214, 214);
            btnVistaPrevia.FlatAppearance.BorderSize = 1;
            btnVistaPrevia.FlatAppearance.MouseOverBackColor = Color.FromArgb(238, 246, 255);
            btnVistaPrevia.FlatStyle = FlatStyle.Flat;
            btnVistaPrevia.Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Pixel);
            btnVistaPrevia.ForeColor = Color.FromArgb(31, 41, 55);
            btnVistaPrevia.Location = new Point(914, 5);
            btnVistaPrevia.Name = "btnVistaPrevia";
            btnVistaPrevia.Size = new Size(170, 60);
            btnVistaPrevia.TabIndex = 1;
            btnVistaPrevia.Text = "Vista previa";
            btnVistaPrevia.UseVisualStyleBackColor = false;
            // 
            // btnPagar
            // 
            btnPagar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnPagar.BackColor = Color.FromArgb(22, 163, 74);
            btnPagar.Cursor = Cursors.Hand;
            btnPagar.FlatAppearance.BorderSize = 0;
            btnPagar.FlatAppearance.MouseDownBackColor = Color.FromArgb(21, 128, 61);
            btnPagar.FlatAppearance.MouseOverBackColor = Color.FromArgb(34, 197, 94);
            btnPagar.FlatStyle = FlatStyle.Flat;
            btnPagar.Font = new Font("Segoe UI", 28F, FontStyle.Bold, GraphicsUnit.Pixel);
            btnPagar.ForeColor = Color.White;
            btnPagar.Location = new Point(0, 90);
            btnPagar.Name = "btnPagar";
            btnPagar.Size = new Size(1084, 72);
            btnPagar.TabIndex = 1;
            btnPagar.Text = "COBRAR";
            btnPagar.TextAlign = ContentAlignment.MiddleCenter;
            btnPagar.UseVisualStyleBackColor = false;
            // 
            // FrmPago
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(248, 249, 251);
            ClientSize = new Size(1150, 780);
            Controls.Add(tlpRoot);
            FormBorderStyle = FormBorderStyle.None;
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmPago";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Pagar Venta";
            tlpRoot.ResumeLayout(false);
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            tlpContent.ResumeLayout(false);
            pnlIzquierda.ResumeLayout(false);
            pnlIzquierda.PerformLayout();
            cardTotal.ResumeLayout(false);
            cardTotal.PerformLayout();
            tlpBilletes.ResumeLayout(false);
            pnlMontoInput.ResumeLayout(false);
            pnlMontoInput.PerformLayout();
            pnlDerecha.ResumeLayout(false);
            cardCambio.ResumeLayout(false);
            tlpCambio.ResumeLayout(false);
            pnlFooter.ResumeLayout(false);
            tlpFooterTop.ResumeLayout(false);
            tlpFooterTop.PerformLayout();
            ResumeLayout(false);
        }

        private static void ConfigurarBillete(Button btn, string texto)
        {
            btn.BackColor = Color.White;
            btn.Cursor = Cursors.Hand;
            btn.Dock = DockStyle.Fill;
            btn.FlatAppearance.BorderColor = Color.FromArgb(214, 214, 214);
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(58, 163, 255);
            btn.FlatStyle = FlatStyle.Flat;
            btn.Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Pixel);
            btn.ForeColor = Color.FromArgb(31, 41, 55);
            btn.Margin = new Padding(8);
            btn.Text = texto;
            btn.UseVisualStyleBackColor = false;
        }

        #endregion

        private TableLayoutPanel tlpRoot;
        private Panel pnlHeader;
        private Label lblIconoHeader;
        private Label lblTitulo;
        private Button btnCerrar;
        private TableLayoutPanel tlpContent;
        private Panel pnlIzquierda;
        private Panel cardTotal;
        private Label lblIconoTotal;
        private Label lblTotalEtiqueta;
        private Label lblTotalMonto;
        private Label lblMetodoTitulo;
        private ComboBox cmbMetodo;
        private Label lblMontoTitulo;
        private Label lblMontoSubtexto;
        private TableLayoutPanel tlpBilletes;
        private Button btnBillete50;
        private Button btnBillete100;
        private Button btnBillete200;
        private Button btnBillete500;
        private Button btnBillete1000;
        private Button btnBillete2000;
        private Panel pnlMontoInput;
        private Label lblIconoMonto;
        private TextBox txtMontoRecibido;
        private Panel pnlDerecha;
        private Panel cardCambio;
        private TableLayoutPanel tlpCambio;
        private Label lblCambioTitulo;
        private Label lblIconoCambio;
        private Label lblCambioMonto;
        private Panel pnlFooter;
        private TableLayoutPanel tlpFooterTop;
        private CheckBox chkImprimirRecibo;
        private Button btnVistaPrevia;
        private Button btnPagar;
    }
}
