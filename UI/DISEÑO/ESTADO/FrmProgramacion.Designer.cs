namespace UI.DISEÑO.ESTADO
{
    partial class FrmProgramacion
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
            lblTitulo = new Label();
            lblMiembroTitulo = new Label();
            cmbPMiembro = new ComboBox();
            lblPNombreTitulo = new Label();
            lblPNombre = new Label();
            lblPMembresiaTitulo = new Label();
            lblPMembresia = new Label();
            lblPVenceTitulo = new Label();
            lblPVence = new Label();
            lblPlanTitulo = new Label();
            cmbPMembresia = new ComboBox();
            lblPmontoPlan = new Label();
            lblInicioTitulo = new Label();
            dtPProgramar = new DateTimePicker();
            lblFinTitulo = new Label();
            dtPProgramado = new DateTimePicker();
            panelAcciones = new Panel();
            btnPProgramar = new Button();
            btnCancelar = new Button();
            panelAcciones.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitulo.Location = new Point(16, 12);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(292, 28);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Programar próxima membresía";
            // 
            // lblMiembroTitulo
            // 
            lblMiembroTitulo.AutoSize = true;
            lblMiembroTitulo.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblMiembroTitulo.Location = new Point(16, 52);
            lblMiembroTitulo.Name = "lblMiembroTitulo";
            lblMiembroTitulo.Size = new Size(138, 21);
            lblMiembroTitulo.TabIndex = 1;
            lblMiembroTitulo.Text = "Miembro activo:";
            // 
            // cmbPMiembro
            // 
            cmbPMiembro.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPMiembro.Font = new Font("Segoe UI", 10F);
            cmbPMiembro.FormattingEnabled = true;
            cmbPMiembro.Location = new Point(16, 76);
            cmbPMiembro.Name = "cmbPMiembro";
            cmbPMiembro.Size = new Size(448, 31);
            cmbPMiembro.TabIndex = 2;
            cmbPMiembro.SelectedIndexChanged += cmbPMiembro_SelectedIndexChanged;
            // 
            // lblPNombreTitulo
            // 
            lblPNombreTitulo.AutoSize = true;
            lblPNombreTitulo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPNombreTitulo.Location = new Point(16, 118);
            lblPNombreTitulo.Name = "lblPNombreTitulo";
            lblPNombreTitulo.Size = new Size(73, 20);
            lblPNombreTitulo.TabIndex = 3;
            lblPNombreTitulo.Text = "Nombre:";
            // 
            // lblPNombre
            // 
            lblPNombre.AutoSize = true;
            lblPNombre.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblPNombre.ForeColor = Color.FromArgb(27, 146, 255);
            lblPNombre.Location = new Point(110, 116);
            lblPNombre.Name = "lblPNombre";
            lblPNombre.Size = new Size(17, 23);
            lblPNombre.TabIndex = 4;
            lblPNombre.Text = "—";
            // 
            // lblPMembresiaTitulo
            // 
            lblPMembresiaTitulo.AutoSize = true;
            lblPMembresiaTitulo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPMembresiaTitulo.Location = new Point(16, 148);
            lblPMembresiaTitulo.Name = "lblPMembresiaTitulo";
            lblPMembresiaTitulo.Size = new Size(88, 20);
            lblPMembresiaTitulo.TabIndex = 5;
            lblPMembresiaTitulo.Text = "Membresía:";
            // 
            // lblPMembresia
            // 
            lblPMembresia.AutoSize = true;
            lblPMembresia.Font = new Font("Segoe UI", 10F);
            lblPMembresia.Location = new Point(110, 146);
            lblPMembresia.Name = "lblPMembresia";
            lblPMembresia.Size = new Size(17, 23);
            lblPMembresia.TabIndex = 6;
            lblPMembresia.Text = "—";
            // 
            // lblPVenceTitulo
            // 
            lblPVenceTitulo.AutoSize = true;
            lblPVenceTitulo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPVenceTitulo.Location = new Point(16, 178);
            lblPVenceTitulo.Name = "lblPVenceTitulo";
            lblPVenceTitulo.Size = new Size(58, 20);
            lblPVenceTitulo.TabIndex = 7;
            lblPVenceTitulo.Text = "Vence:";
            // 
            // lblPVence
            // 
            lblPVence.AutoSize = true;
            lblPVence.Font = new Font("Segoe UI", 10F);
            lblPVence.ForeColor = Color.DimGray;
            lblPVence.Location = new Point(110, 176);
            lblPVence.Name = "lblPVence";
            lblPVence.Size = new Size(17, 23);
            lblPVence.TabIndex = 8;
            lblPVence.Text = "—";
            // 
            // lblPlanTitulo
            // 
            lblPlanTitulo.AutoSize = true;
            lblPlanTitulo.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblPlanTitulo.Location = new Point(16, 214);
            lblPlanTitulo.Name = "lblPlanTitulo";
            lblPlanTitulo.Size = new Size(118, 21);
            lblPlanTitulo.TabIndex = 9;
            lblPlanTitulo.Text = "Plan a programar";
            // 
            // cmbPMembresia
            // 
            cmbPMembresia.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPMembresia.Font = new Font("Segoe UI", 10F);
            cmbPMembresia.FormattingEnabled = true;
            cmbPMembresia.Location = new Point(16, 238);
            cmbPMembresia.Name = "cmbPMembresia";
            cmbPMembresia.Size = new Size(448, 31);
            cmbPMembresia.TabIndex = 10;
            cmbPMembresia.SelectedIndexChanged += cmbPMembresia_SelectedIndexChanged;
            // 
            // lblPmontoPlan
            // 
            lblPmontoPlan.AutoSize = true;
            lblPmontoPlan.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblPmontoPlan.Location = new Point(16, 278);
            lblPmontoPlan.Name = "lblPmontoPlan";
            lblPmontoPlan.Size = new Size(143, 23);
            lblPmontoPlan.TabIndex = 11;
            lblPmontoPlan.Text = "Precio: RD$ 0.00";
            // 
            // lblInicioTitulo
            // 
            lblInicioTitulo.AutoSize = true;
            lblInicioTitulo.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblInicioTitulo.Location = new Point(16, 314);
            lblInicioTitulo.Name = "lblInicioTitulo";
            lblInicioTitulo.Size = new Size(198, 21);
            lblInicioTitulo.TabIndex = 12;
            lblInicioTitulo.Text = "Inicio periodo programado:";
            // 
            // dtPProgramar
            // 
            dtPProgramar.Font = new Font("Segoe UI", 10F);
            dtPProgramar.Format = DateTimePickerFormat.Short;
            dtPProgramar.Location = new Point(16, 338);
            dtPProgramar.Name = "dtPProgramar";
            dtPProgramar.Size = new Size(200, 30);
            dtPProgramar.TabIndex = 13;
            dtPProgramar.ValueChanged += dtPProgramar_ValueChanged;
            // 
            // lblFinTitulo
            // 
            lblFinTitulo.AutoSize = true;
            lblFinTitulo.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblFinTitulo.Location = new Point(240, 314);
            lblFinTitulo.Name = "lblFinTitulo";
            lblFinTitulo.Size = new Size(181, 21);
            lblFinTitulo.TabIndex = 14;
            lblFinTitulo.Text = "Vence periodo programado";
            // 
            // dtPProgramado
            // 
            dtPProgramado.Enabled = false;
            dtPProgramado.Font = new Font("Segoe UI", 10F);
            dtPProgramado.Format = DateTimePickerFormat.Short;
            dtPProgramado.Location = new Point(240, 338);
            dtPProgramado.Name = "dtPProgramado";
            dtPProgramado.Size = new Size(200, 30);
            dtPProgramado.TabIndex = 15;
            // 
            // panelAcciones
            // 
            panelAcciones.Controls.Add(btnPProgramar);
            panelAcciones.Controls.Add(btnCancelar);
            panelAcciones.Dock = DockStyle.Bottom;
            panelAcciones.Location = new Point(0, 392);
            panelAcciones.Name = "panelAcciones";
            panelAcciones.Padding = new Padding(12, 8, 12, 8);
            panelAcciones.Size = new Size(484, 56);
            panelAcciones.TabIndex = 16;
            // 
            // btnPProgramar
            // 
            btnPProgramar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnPProgramar.BackColor = Color.FromArgb(22, 163, 74);
            btnPProgramar.Cursor = Cursors.Hand;
            btnPProgramar.FlatAppearance.BorderSize = 0;
            btnPProgramar.FlatStyle = FlatStyle.Flat;
            btnPProgramar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnPProgramar.ForeColor = Color.White;
            btnPProgramar.Location = new Point(228, 10);
            btnPProgramar.Name = "btnPProgramar";
            btnPProgramar.Size = new Size(130, 36);
            btnPProgramar.TabIndex = 0;
            btnPProgramar.Text = "PROGRAMAR";
            btnPProgramar.UseVisualStyleBackColor = false;
            btnPProgramar.Click += btnPProgramar_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancelar.DialogResult = DialogResult.Cancel;
            btnCancelar.Font = new Font("Segoe UI", 9.5F);
            btnCancelar.Location = new Point(364, 10);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(100, 36);
            btnCancelar.TabIndex = 1;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = true;
            // 
            // FrmProgramacion
            // 
            AcceptButton = btnPProgramar;
            AutoScaleDimensions = new SizeF(9F, 23F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            CancelButton = btnCancelar;
            ClientSize = new Size(484, 448);
            Controls.Add(panelAcciones);
            Controls.Add(dtPProgramado);
            Controls.Add(lblFinTitulo);
            Controls.Add(dtPProgramar);
            Controls.Add(lblInicioTitulo);
            Controls.Add(lblPmontoPlan);
            Controls.Add(cmbPMembresia);
            Controls.Add(lblPlanTitulo);
            Controls.Add(lblPVence);
            Controls.Add(lblPVenceTitulo);
            Controls.Add(lblPMembresia);
            Controls.Add(lblPMembresiaTitulo);
            Controls.Add(lblPNombre);
            Controls.Add(lblPNombreTitulo);
            Controls.Add(cmbPMiembro);
            Controls.Add(lblMiembroTitulo);
            Controls.Add(lblTitulo);
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmProgramacion";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Programar membresía";
            Load += FrmProgramacion_Load;
            panelAcciones.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTitulo;
        private Label lblMiembroTitulo;
        private ComboBox cmbPMiembro;
        private Label lblPNombreTitulo;
        private Label lblPNombre;
        private Label lblPMembresiaTitulo;
        private Label lblPMembresia;
        private Label lblPVenceTitulo;
        private Label lblPVence;
        private Label lblPlanTitulo;
        private ComboBox cmbPMembresia;
        private Label lblPmontoPlan;
        private Label lblInicioTitulo;
        private DateTimePicker dtPProgramar;
        private Label lblFinTitulo;
        private DateTimePicker dtPProgramado;
        private Panel panelAcciones;
        private Button btnPProgramar;
        private Button btnCancelar;
    }
}
