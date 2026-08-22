namespace UI.DISEÑO
{
    partial class FrmRenovarMembresia
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
            lblCliente = new Label();
            lblPlan = new Label();
            cmbPlan = new ComboBox();
            lblPrecio = new Label();
            pnlOferta = new Panel();
            lblOfertaPct = new Label();
            txtDescuentoPorcental = new TextBox();
            lblOfertaMonto = new Label();
            txtDescuentoMonto = new TextBox();
            lblTotalPagarTitulo = new Label();
            lblTotalPagar = new Label();
            lblMotivoOferta = new Label();
            txtMotivo = new TextBox();
            panelAcciones = new Panel();
            btnConfirmar = new Button();
            btnCancelar = new Button();
            pnlOferta.SuspendLayout();
            panelAcciones.SuspendLayout();
            SuspendLayout();
            // 
            // lblCliente
            // 
            lblCliente.AutoSize = true;
            lblCliente.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCliente.Location = new Point(16, 16);
            lblCliente.Name = "lblCliente";
            lblCliente.Size = new Size(58, 23);
            lblCliente.TabIndex = 0;
            lblCliente.Text = "Cliente";
            // 
            // lblPlan
            // 
            lblPlan.AutoSize = true;
            lblPlan.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            lblPlan.Location = new Point(16, 52);
            lblPlan.Name = "lblPlan";
            lblPlan.Size = new Size(43, 23);
            lblPlan.TabIndex = 1;
            lblPlan.Text = "Plan:";
            // 
            // cmbPlan
            // 
            cmbPlan.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbPlan.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPlan.Font = new Font("Segoe UI", 10F);
            cmbPlan.FormattingEnabled = true;
            cmbPlan.Location = new Point(16, 78);
            cmbPlan.Name = "cmbPlan";
            cmbPlan.Size = new Size(332, 31);
            cmbPlan.TabIndex = 2;
            cmbPlan.SelectedIndexChanged += cmbPlan_SelectedIndexChanged;
            // 
            // lblPrecio
            // 
            lblPrecio.AutoSize = true;
            lblPrecio.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblPrecio.Location = new Point(16, 120);
            lblPrecio.Name = "lblPrecio";
            lblPrecio.Size = new Size(143, 23);
            lblPrecio.TabIndex = 3;
            lblPrecio.Text = "Precio: RD$ 0.00";
            // 
            // pnlOferta
            // 
            pnlOferta.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlOferta.BorderStyle = BorderStyle.FixedSingle;
            pnlOferta.Controls.Add(lblOfertaPct);
            pnlOferta.Controls.Add(txtDescuentoPorcental);
            pnlOferta.Controls.Add(lblOfertaMonto);
            pnlOferta.Controls.Add(txtDescuentoMonto);
            pnlOferta.Controls.Add(lblTotalPagarTitulo);
            pnlOferta.Controls.Add(lblTotalPagar);
            pnlOferta.Controls.Add(lblMotivoOferta);
            pnlOferta.Controls.Add(txtMotivo);
            pnlOferta.Location = new Point(16, 152);
            pnlOferta.Name = "pnlOferta";
            pnlOferta.Size = new Size(332, 220);
            pnlOferta.TabIndex = 5;
            pnlOferta.Tag = "classic";
            pnlOferta.Visible = false;
            // 
            // lblOfertaPct
            // 
            lblOfertaPct.AutoSize = true;
            lblOfertaPct.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            lblOfertaPct.Location = new Point(10, 12);
            lblOfertaPct.Name = "lblOfertaPct";
            lblOfertaPct.Size = new Size(92, 23);
            lblOfertaPct.TabIndex = 0;
            lblOfertaPct.Text = "Cortesía %:";
            // 
            // txtDescuentoPorcental
            // 
            txtDescuentoPorcental.Font = new Font("Segoe UI", 10F);
            txtDescuentoPorcental.Location = new Point(130, 8);
            txtDescuentoPorcental.Name = "txtDescuentoPorcental";
            txtDescuentoPorcental.Size = new Size(80, 30);
            txtDescuentoPorcental.TabIndex = 1;
            txtDescuentoPorcental.Text = "100";
            txtDescuentoPorcental.TextAlign = HorizontalAlignment.Right;
            txtDescuentoPorcental.TextChanged += txtDescuentoPorcental_TextChanged;
            // 
            // lblOfertaMonto
            // 
            lblOfertaMonto.AutoSize = true;
            lblOfertaMonto.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            lblOfertaMonto.Location = new Point(10, 52);
            lblOfertaMonto.Name = "lblOfertaMonto";
            lblOfertaMonto.Size = new Size(114, 23);
            lblOfertaMonto.TabIndex = 2;
            lblOfertaMonto.Text = "Valor ref. RD$:";
            // 
            // txtDescuentoMonto
            // 
            txtDescuentoMonto.Font = new Font("Segoe UI", 10F);
            txtDescuentoMonto.Location = new Point(130, 48);
            txtDescuentoMonto.Name = "txtDescuentoMonto";
            txtDescuentoMonto.Size = new Size(120, 30);
            txtDescuentoMonto.TabIndex = 3;
            txtDescuentoMonto.Text = "0.00";
            txtDescuentoMonto.TextAlign = HorizontalAlignment.Right;
            txtDescuentoMonto.TextChanged += txtDescuentoMonto_TextChanged;
            // 
            // lblTotalPagarTitulo
            // 
            lblTotalPagarTitulo.AutoSize = true;
            lblTotalPagarTitulo.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            lblTotalPagarTitulo.Location = new Point(10, 92);
            lblTotalPagarTitulo.Name = "lblTotalPagarTitulo";
            lblTotalPagarTitulo.Size = new Size(109, 23);
            lblTotalPagarTitulo.TabIndex = 4;
            lblTotalPagarTitulo.Text = "Total a pagar:";
            // 
            // lblTotalPagar
            // 
            lblTotalPagar.AutoSize = true;
            lblTotalPagar.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTotalPagar.ForeColor = Color.FromArgb(22, 163, 74);
            lblTotalPagar.Location = new Point(130, 90);
            lblTotalPagar.Name = "lblTotalPagar";
            lblTotalPagar.Size = new Size(99, 25);
            lblTotalPagar.TabIndex = 5;
            lblTotalPagar.Text = "RD$ 0.00";
            // 
            // lblMotivoOferta
            // 
            lblMotivoOferta.AutoSize = true;
            lblMotivoOferta.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            lblMotivoOferta.Location = new Point(10, 124);
            lblMotivoOferta.Name = "lblMotivoOferta";
            lblMotivoOferta.Size = new Size(67, 23);
            lblMotivoOferta.TabIndex = 6;
            lblMotivoOferta.Text = "Motivo:";
            // 
            // txtMotivo
            // 
            txtMotivo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtMotivo.Font = new Font("Segoe UI", 10F);
            txtMotivo.Location = new Point(10, 150);
            txtMotivo.Multiline = true;
            txtMotivo.Name = "txtMotivo";
            txtMotivo.PlaceholderText = "Ej. promo temporada, referido...";
            txtMotivo.Size = new Size(308, 56);
            txtMotivo.TabIndex = 7;
            // 
            // panelAcciones
            // 
            panelAcciones.Controls.Add(btnConfirmar);
            panelAcciones.Controls.Add(btnCancelar);
            panelAcciones.Dock = DockStyle.Bottom;
            panelAcciones.Location = new Point(0, 388);
            panelAcciones.Name = "panelAcciones";
            panelAcciones.Padding = new Padding(12, 8, 12, 8);
            panelAcciones.Size = new Size(364, 52);
            panelAcciones.TabIndex = 4;
            panelAcciones.Tag = "classic";
            // 
            // btnConfirmar
            // 
            btnConfirmar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnConfirmar.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnConfirmar.Location = new Point(144, 8);
            btnConfirmar.Name = "btnConfirmar";
            btnConfirmar.Size = new Size(120, 34);
            btnConfirmar.TabIndex = 0;
            btnConfirmar.Text = "CONFIRMAR";
            btnConfirmar.UseVisualStyleBackColor = true;
            btnConfirmar.Click += btnConfirmar_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancelar.DialogResult = DialogResult.Cancel;
            btnCancelar.Font = new Font("Segoe UI", 9.75F);
            btnCancelar.Location = new Point(270, 8);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(82, 34);
            btnCancelar.TabIndex = 1;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = true;
            // 
            // FrmRenovarMembresia
            // 
            AcceptButton = btnConfirmar;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancelar;
            ClientSize = new Size(364, 440);
            Controls.Add(pnlOferta);
            Controls.Add(panelAcciones);
            Controls.Add(lblPrecio);
            Controls.Add(cmbPlan);
            Controls.Add(lblPlan);
            Controls.Add(lblCliente);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(380, 420);
            Name = "FrmRenovarMembresia";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Tag = "classic";
            Text = "Renovar membresía";
            Load += FrmRenovarMembresia_Load;
            pnlOferta.ResumeLayout(false);
            pnlOferta.PerformLayout();
            panelAcciones.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblCliente;
        private Label lblPlan;
        private ComboBox cmbPlan;
        private Label lblPrecio;
        private Panel pnlOferta;
        private Label lblOfertaPct;
        private TextBox txtDescuentoPorcental;
        private Label lblOfertaMonto;
        private TextBox txtDescuentoMonto;
        private Label lblTotalPagarTitulo;
        private Label lblTotalPagar;
        private Label lblMotivoOferta;
        private TextBox txtMotivo;
        private Panel panelAcciones;
        private Button btnConfirmar;
        private Button btnCancelar;
    }
}
