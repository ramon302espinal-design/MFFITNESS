using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace UI.DISEÑO
{
    partial class FrmPeticionIaFoto
    {
        private IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        private void InitializeComponent()
        {
            lblTitulo = new Label();
            lblEjemplos = new Label();
            txtPeticion = new TextBox();
            btnMicrofono = new Button();
            lblEstadoVoz = new Label();
            panelNivelVoz = new Panel();
            panelNivelVozFill = new Panel();
            btnAplicar = new Button();
            btnCancelar = new Button();
            panelNivelVoz.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitulo
            // 
            lblTitulo.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(30, 41, 59);
            lblTitulo.Location = new Point(16, 14);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(488, 28);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "¿Qué quieres que haga la IA con esta foto?";
            // 
            // lblEjemplos
            // 
            lblEjemplos.ForeColor = Color.FromArgb(100, 116, 139);
            lblEjemplos.Location = new Point(16, 46);
            lblEjemplos.Name = "lblEjemplos";
            lblEjemplos.Size = new Size(488, 40);
            lblEjemplos.TabIndex = 1;
            lblEjemplos.Text = "Escribe o pulsa 🎤 y dicta: ponla nítida · quítale el fondo · mejora la calidad";
            // 
            // txtPeticion
            // 
            txtPeticion.Font = new Font("Segoe UI", 11F);
            txtPeticion.Location = new Point(16, 94);
            txtPeticion.Multiline = true;
            txtPeticion.Name = "txtPeticion";
            txtPeticion.ScrollBars = ScrollBars.Vertical;
            txtPeticion.Size = new Size(488, 100);
            txtPeticion.TabIndex = 2;
            txtPeticion.Text = "Ponla nítida y mejora la calidad sin cambiar el producto";
            // 
            // btnMicrofono
            // 
            btnMicrofono.BackColor = Color.FromArgb(79, 70, 229);
            btnMicrofono.Cursor = Cursors.Hand;
            btnMicrofono.FlatAppearance.BorderSize = 0;
            btnMicrofono.FlatStyle = FlatStyle.Flat;
            btnMicrofono.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnMicrofono.ForeColor = Color.White;
            btnMicrofono.Location = new Point(16, 204);
            btnMicrofono.Name = "btnMicrofono";
            btnMicrofono.Size = new Size(44, 36);
            btnMicrofono.TabIndex = 3;
            btnMicrofono.Text = "🎤";
            btnMicrofono.UseVisualStyleBackColor = false;
            btnMicrofono.Click += btnMicrofono_Click;
            // 
            // lblEstadoVoz
            // 
            lblEstadoVoz.ForeColor = Color.FromArgb(22, 101, 52);
            lblEstadoVoz.Location = new Point(66, 204);
            lblEstadoVoz.Name = "lblEstadoVoz";
            lblEstadoVoz.Size = new Size(438, 36);
            lblEstadoVoz.TabIndex = 4;
            lblEstadoVoz.Text = "Pulsa 🎤 para activar el micrófono";
            lblEstadoVoz.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panelNivelVoz
            // 
            panelNivelVoz.BackColor = Color.FromArgb(226, 232, 240);
            panelNivelVoz.Controls.Add(panelNivelVozFill);
            panelNivelVoz.Location = new Point(16, 246);
            panelNivelVoz.Name = "panelNivelVoz";
            panelNivelVoz.Size = new Size(488, 8);
            panelNivelVoz.TabIndex = 5;
            // 
            // panelNivelVozFill
            // 
            panelNivelVozFill.BackColor = Color.FromArgb(34, 197, 94);
            panelNivelVozFill.Dock = DockStyle.Left;
            panelNivelVozFill.Location = new Point(0, 0);
            panelNivelVozFill.Name = "panelNivelVozFill";
            panelNivelVozFill.Size = new Size(4, 8);
            panelNivelVozFill.TabIndex = 0;
            // 
            // btnAplicar
            // 
            btnAplicar.BackColor = Color.FromArgb(79, 70, 229);
            btnAplicar.FlatAppearance.BorderSize = 0;
            btnAplicar.FlatStyle = FlatStyle.Flat;
            btnAplicar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnAplicar.ForeColor = Color.White;
            btnAplicar.Location = new Point(252, 268);
            btnAplicar.Name = "btnAplicar";
            btnAplicar.Size = new Size(120, 34);
            btnAplicar.TabIndex = 6;
            btnAplicar.Text = "Aplicar IA";
            btnAplicar.UseVisualStyleBackColor = false;
            btnAplicar.Click += btnAplicar_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.DialogResult = DialogResult.Cancel;
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.Location = new Point(386, 268);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(110, 34);
            btnCancelar.TabIndex = 7;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = true;
            btnCancelar.Click += btnCancelar_Click;
            // 
            // FrmPeticionIaFoto
            // 
            AcceptButton = btnAplicar;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(248, 250, 252);
            CancelButton = btnCancelar;
            ClientSize = new Size(520, 318);
            Controls.Add(btnCancelar);
            Controls.Add(btnAplicar);
            Controls.Add(panelNivelVoz);
            Controls.Add(lblEstadoVoz);
            Controls.Add(btnMicrofono);
            Controls.Add(txtPeticion);
            Controls.Add(lblEjemplos);
            Controls.Add(lblTitulo);
            Font = new Font("Segoe UI", 9.5F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmPeticionIaFoto";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Arreglar foto con IA · voz";
            Shown += FrmPeticionIaFoto_Shown;
            panelNivelVoz.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTitulo;
        private Label lblEjemplos;
        private TextBox txtPeticion;
        private Button btnMicrofono;
        private Label lblEstadoVoz;
        private Panel panelNivelVoz;
        private Panel panelNivelVozFill;
        private Button btnAplicar;
        private Button btnCancelar;
    }
}
