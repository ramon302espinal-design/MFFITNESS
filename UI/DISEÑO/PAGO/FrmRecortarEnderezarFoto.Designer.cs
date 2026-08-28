using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace UI.DISEÑO
{
    partial class FrmRecortarEnderezarFoto
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                LiberarImagenesRuntime();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            lblAyuda = new Label();
            picFoto = new PictureBox();
            lblAngulo = new Label();
            trkAngulo = new TrackBar();
            btnLimpiarRecorte = new Button();
            btnAplicar = new Button();
            btnCancelar = new Button();
            ((ISupportInitialize)picFoto).BeginInit();
            ((ISupportInitialize)trkAngulo).BeginInit();
            SuspendLayout();
            // 
            // lblAyuda
            // 
            lblAyuda.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblAyuda.ForeColor = Color.FromArgb(71, 85, 105);
            lblAyuda.Location = new Point(16, 12);
            lblAyuda.Name = "lblAyuda";
            lblAyuda.Size = new Size(688, 22);
            lblAyuda.TabIndex = 0;
            lblAyuda.Text = "Arrastra sobre la foto para recortar. Usa el control para enderezar.";
            // 
            // picFoto
            // 
            picFoto.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            picFoto.BackColor = Color.FromArgb(226, 232, 240);
            picFoto.BorderStyle = BorderStyle.FixedSingle;
            picFoto.Cursor = Cursors.Cross;
            picFoto.Location = new Point(16, 40);
            picFoto.Name = "picFoto";
            picFoto.Size = new Size(688, 420);
            picFoto.SizeMode = PictureBoxSizeMode.Zoom;
            picFoto.TabIndex = 1;
            picFoto.TabStop = false;
            picFoto.Paint += picFoto_Paint;
            picFoto.MouseDown += picFoto_MouseDown;
            picFoto.MouseMove += picFoto_MouseMove;
            picFoto.MouseUp += picFoto_MouseUp;
            // 
            // lblAngulo
            // 
            lblAngulo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblAngulo.AutoSize = true;
            lblAngulo.Location = new Point(16, 474);
            lblAngulo.Name = "lblAngulo";
            lblAngulo.Size = new Size(99, 20);
            lblAngulo.TabIndex = 2;
            lblAngulo.Text = "Enderezar: 0°";
            // 
            // trkAngulo
            // 
            trkAngulo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            trkAngulo.LargeChange = 5;
            trkAngulo.Location = new Point(140, 468);
            trkAngulo.Maximum = 20;
            trkAngulo.Minimum = -20;
            trkAngulo.Name = "trkAngulo";
            trkAngulo.Size = new Size(360, 53);
            trkAngulo.TabIndex = 3;
            trkAngulo.TickFrequency = 5;
            trkAngulo.ValueChanged += trkAngulo_ValueChanged;
            // 
            // btnLimpiarRecorte
            // 
            btnLimpiarRecorte.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnLimpiarRecorte.FlatStyle = FlatStyle.Flat;
            btnLimpiarRecorte.Location = new Point(520, 470);
            btnLimpiarRecorte.Name = "btnLimpiarRecorte";
            btnLimpiarRecorte.Size = new Size(120, 28);
            btnLimpiarRecorte.TabIndex = 4;
            btnLimpiarRecorte.Text = "Limpiar recorte";
            btnLimpiarRecorte.UseVisualStyleBackColor = true;
            btnLimpiarRecorte.Click += btnLimpiarRecorte_Click;
            // 
            // btnAplicar
            // 
            btnAplicar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnAplicar.BackColor = Color.FromArgb(22, 163, 74);
            btnAplicar.FlatAppearance.BorderSize = 0;
            btnAplicar.FlatStyle = FlatStyle.Flat;
            btnAplicar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnAplicar.ForeColor = Color.White;
            btnAplicar.Location = new Point(470, 512);
            btnAplicar.Name = "btnAplicar";
            btnAplicar.Size = new Size(110, 32);
            btnAplicar.TabIndex = 5;
            btnAplicar.Text = "Aplicar";
            btnAplicar.UseVisualStyleBackColor = false;
            btnAplicar.Click += btnAplicar_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancelar.DialogResult = DialogResult.Cancel;
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.Location = new Point(594, 512);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(110, 32);
            btnCancelar.TabIndex = 6;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = true;
            btnCancelar.Click += btnCancelar_Click;
            // 
            // FrmRecortarEnderezarFoto
            // 
            AcceptButton = btnAplicar;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(248, 250, 252);
            CancelButton = btnCancelar;
            ClientSize = new Size(720, 560);
            Controls.Add(btnCancelar);
            Controls.Add(btnAplicar);
            Controls.Add(btnLimpiarRecorte);
            Controls.Add(trkAngulo);
            Controls.Add(lblAngulo);
            Controls.Add(picFoto);
            Controls.Add(lblAyuda);
            Font = new Font("Segoe UI", 9.5F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmRecortarEnderezarFoto";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Recortar y enderezar foto";
            ((ISupportInitialize)picFoto).EndInit();
            ((ISupportInitialize)trkAngulo).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblAyuda;
        private PictureBox picFoto;
        private Label lblAngulo;
        private TrackBar trkAngulo;
        private Button btnLimpiarRecorte;
        private Button btnAplicar;
        private Button btnCancelar;
    }
}
