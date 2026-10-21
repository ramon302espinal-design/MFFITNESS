using System.Drawing;
using System.Windows.Forms;

namespace UI.DISEÑO.CHAT
{
    partial class FrmPlantillasAutomatizaciones
    {
        private Label _aviso = null!;
        private Label _lblTipos = null!;
        private ListBox _lista = null!;
        private Label _evento = null!;
        private FlowLayoutPanel _filaVars = null!;
        private CheckBox _activa = null!;
        private Label _lblHoras = null!;
        private NumericUpDown _horas = null!;
        private Label _lblDias = null!;
        private NumericUpDown _dias = null!;
        private Label _lblUrgente = null!;
        private NumericUpDown _diasUrgente = null!;
        private TextBox _texto = null!;
        private CheckBox _vacios = null!;
        private TextBox _preview = null!;
        private Label _resultado = null!;
        private Button _btnGuardar = null!;
        private Button _btnRestaurar = null!;
        private Button _btnPreview = null!;
        private Button _btnProbar = null!;

        private void InitializeComponent()
        {
            _aviso = new Label();
            _lblTipos = new Label();
            _lista = new ListBox();
            _evento = new Label();
            _filaVars = new FlowLayoutPanel();
            _activa = new CheckBox();
            _lblHoras = new Label();
            _horas = new NumericUpDown();
            _lblDias = new Label();
            _dias = new NumericUpDown();
            _lblUrgente = new Label();
            _diasUrgente = new NumericUpDown();
            _texto = new TextBox();
            _vacios = new CheckBox();
            _preview = new TextBox();
            _resultado = new Label();
            _btnGuardar = new Button();
            _btnRestaurar = new Button();
            _btnPreview = new Button();
            _btnProbar = new Button();
            ((System.ComponentModel.ISupportInitialize)_horas).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_dias).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_diasUrgente).BeginInit();
            SuspendLayout();
            // 
            // _aviso
            // 
            _aviso.Location = new Point(272, 8);
            _aviso.Name = "_aviso";
            _aviso.Size = new Size(816, 42);
            _aviso.TabIndex = 0;
            _aviso.Text = "El texto y las opciones se guardan para los próximos envíos. No reenvían historial y no modifican la plantilla aprobada en WhatsApp. Quitar Activada detiene solo el WhatsApp de ese tipo.";
            // 
            // _lblTipos
            // 
            _lblTipos.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _lblTipos.Location = new Point(10, 8);
            _lblTipos.Name = "_lblTipos";
            _lblTipos.Size = new Size(250, 24);
            _lblTipos.TabIndex = 1;
            _lblTipos.Text = "Tipo de evento";
            _lblTipos.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _lista
            // 
            _lista.BorderStyle = BorderStyle.FixedSingle;
            _lista.HorizontalScrollbar = true;
            _lista.IntegralHeight = false;
            _lista.Location = new Point(10, 34);
            _lista.Name = "_lista";
            _lista.Size = new Size(250, 694);
            _lista.TabIndex = 2;
            // 
            // _evento
            // 
            _evento.Location = new Point(272, 52);
            _evento.Name = "_evento";
            _evento.Size = new Size(816, 40);
            _evento.TabIndex = 3;
            _evento.Text = "Evento";
            // 
            // _filaVars
            // 
            _filaVars.AutoScroll = true;
            _filaVars.Location = new Point(272, 84);
            _filaVars.Name = "_filaVars";
            _filaVars.Size = new Size(816, 64);
            _filaVars.TabIndex = 4;
            // 
            // _activa
            // 
            _activa.Checked = true;
            _activa.CheckState = CheckState.Checked;
            _activa.Location = new Point(272, 168);
            _activa.Name = "_activa";
            _activa.Size = new Size(120, 28);
            _activa.TabIndex = 5;
            _activa.Text = "Activada";
            // 
            // _lblHoras
            // 
            _lblHoras.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _lblHoras.Location = new Point(402, 168);
            _lblHoras.Name = "_lblHoras";
            _lblHoras.Size = new Size(170, 28);
            _lblHoras.TabIndex = 6;
            _lblHoras.Text = "Evitar duplicado (horas)";
            _lblHoras.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _horas
            // 
            _horas.Location = new Point(574, 168);
            _horas.Maximum = new decimal(new int[] { 720, 0, 0, 0 });
            _horas.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            _horas.Name = "_horas";
            _horas.Size = new Size(70, 27);
            _horas.TabIndex = 7;
            _horas.TextAlign = HorizontalAlignment.Center;
            _horas.Value = new decimal(new int[] { 24, 0, 0, 0 });
            // 
            // _lblDias
            // 
            _lblDias.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _lblDias.Location = new Point(272, 202);
            _lblDias.Name = "_lblDias";
            _lblDias.Size = new Size(150, 28);
            _lblDias.TabIndex = 8;
            _lblDias.Text = "Primer aviso (días)";
            _lblDias.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _dias
            // 
            _dias.Location = new Point(424, 202);
            _dias.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
            _dias.Name = "_dias";
            _dias.Size = new Size(70, 27);
            _dias.TabIndex = 9;
            _dias.TextAlign = HorizontalAlignment.Center;
            // 
            // _lblUrgente
            // 
            _lblUrgente.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _lblUrgente.Location = new Point(508, 202);
            _lblUrgente.Name = "_lblUrgente";
            _lblUrgente.Size = new Size(170, 28);
            _lblUrgente.TabIndex = 10;
            _lblUrgente.Text = "Aviso urgente (días)";
            _lblUrgente.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _diasUrgente
            // 
            _diasUrgente.Location = new Point(680, 202);
            _diasUrgente.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
            _diasUrgente.Name = "_diasUrgente";
            _diasUrgente.Size = new Size(70, 27);
            _diasUrgente.TabIndex = 11;
            _diasUrgente.TextAlign = HorizontalAlignment.Center;
            // 
            // _texto
            // 
            _texto.BorderStyle = BorderStyle.FixedSingle;
            _texto.Font = new Font("Segoe UI", 10F);
            _texto.Location = new Point(272, 268);
            _texto.Multiline = true;
            _texto.Name = "_texto";
            _texto.ScrollBars = ScrollBars.Vertical;
            _texto.Size = new Size(816, 180);
            _texto.TabIndex = 13;
            // 
            // _vacios
            // 
            _vacios.Location = new Point(272, 238);
            _vacios.Name = "_vacios";
            _vacios.Size = new Size(420, 26);
            _vacios.TabIndex = 12;
            _vacios.Text = "Vista previa con datos opcionales vacíos";
            // 
            // _preview
            // 
            _preview.BorderStyle = BorderStyle.FixedSingle;
            _preview.Font = new Font("Segoe UI", 10F);
            _preview.Location = new Point(272, 452);
            _preview.Multiline = true;
            _preview.Name = "_preview";
            _preview.ReadOnly = true;
            _preview.ScrollBars = ScrollBars.Vertical;
            _preview.Size = new Size(816, 200);
            _preview.TabIndex = 14;
            // 
            // _resultado
            // 
            _resultado.Location = new Point(272, 656);
            _resultado.Name = "_resultado";
            _resultado.Size = new Size(816, 36);
            _resultado.TabIndex = 15;
            _resultado.Text = "Sin resultado registrado.";
            // 
            // _btnGuardar
            // 
            _btnGuardar.Location = new Point(272, 696);
            _btnGuardar.Name = "_btnGuardar";
            _btnGuardar.Size = new Size(120, 34);
            _btnGuardar.TabIndex = 16;
            _btnGuardar.Text = "Guardar";
            // 
            // _btnRestaurar
            // 
            _btnRestaurar.Location = new Point(400, 696);
            _btnRestaurar.Name = "_btnRestaurar";
            _btnRestaurar.Size = new Size(200, 34);
            _btnRestaurar.TabIndex = 17;
            _btnRestaurar.Text = "Restaurar predeterminado";
            // 
            // _btnPreview
            // 
            _btnPreview.Location = new Point(608, 696);
            _btnPreview.Name = "_btnPreview";
            _btnPreview.Size = new Size(130, 34);
            _btnPreview.TabIndex = 18;
            _btnPreview.Text = "Vista previa";
            // 
            // _btnProbar
            // 
            _btnProbar.Location = new Point(746, 696);
            _btnProbar.Name = "_btnProbar";
            _btnProbar.Size = new Size(160, 34);
            _btnProbar.TabIndex = 19;
            _btnProbar.Text = "Probar sin enviar";
            // 
            // FrmPlantillasAutomatizaciones
            // 
            BackColor = SystemColors.Control;
            ClientSize = new Size(1100, 740);
            Controls.Add(_aviso);
            Controls.Add(_lblTipos);
            Controls.Add(_lista);
            Controls.Add(_evento);
            Controls.Add(_filaVars);
            Controls.Add(_activa);
            Controls.Add(_lblHoras);
            Controls.Add(_horas);
            Controls.Add(_lblDias);
            Controls.Add(_dias);
            Controls.Add(_lblUrgente);
            Controls.Add(_diasUrgente);
            Controls.Add(_vacios);
            Controls.Add(_texto);
            Controls.Add(_preview);
            Controls.Add(_resultado);
            Controls.Add(_btnGuardar);
            Controls.Add(_btnRestaurar);
            Controls.Add(_btnPreview);
            Controls.Add(_btnProbar);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(980, 680);
            Name = "FrmPlantillasAutomatizaciones";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Plantillas y automatizaciones";
            ((System.ComponentModel.ISupportInitialize)_horas).EndInit();
            ((System.ComponentModel.ISupportInitialize)_dias).EndInit();
            ((System.ComponentModel.ISupportInitialize)_diasUrgente).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
