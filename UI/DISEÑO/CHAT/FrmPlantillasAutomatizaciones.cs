using BLL;
using BLL.Models;
using CORE;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace UI.DISEÑO.CHAT
{
    /// <summary>
    /// Edita el texto local y las opciones del aviso. No reenvía avisos ya registrados.
    /// El texto no sustituye la plantilla aprobada en Meta.
    /// </summary>
    public partial class FrmPlantillasAutomatizaciones : Form
    {
        private readonly MensajeAutomaticoBLL _mensajes = new();
        private readonly ChatBLL _chat = new();
        private readonly int? _clienteId;
        private IReadOnlyList<WhatsAppAutomatizacionDto> _items = Array.Empty<WhatsAppAutomatizacionDto>();
        private bool _cargando;

        public FrmPlantillasAutomatizaciones(int? clienteId)
        {
            _clienteId = clienteId;
            InitializeComponent();
            _btnGuardar.Click += (_, _) => Guardar();
            _btnRestaurar.Click += (_, _) => Restaurar();
            _btnPreview.Click += (_, _) => ActualizarPreview();
            _btnProbar.Click += (_, _) => ProbarSinEnviar();
            _lista.SelectedIndexChanged += (_, _) => MostrarSeleccion();
            Resize += (_, _) => Organizar();
            Load += (_, _) =>
            {
                Organizar();
                Cargar();
            };
        }

        private void Organizar()
        {
            int ancho = ClientSize.Width;
            int alto = ClientSize.Height;
            if (ancho < 200 || alto < 200)
                return;

            int listaAncho = 250;
            _lblTipos.SetBounds(10, 8, listaAncho, 24);
            _lista.SetBounds(10, 34, listaAncho, Math.Max(120, alto - 46));

            int x = 272;
            int w = Math.Max(320, ancho - x - 12);
            _aviso.SetBounds(x, 8, w, 42);
            _evento.SetBounds(x, 52, w, 40);
            _filaVars.SetBounds(x, 96, w, 64);

            _activa.SetBounds(x, 168, 120, 28);
            _lblHoras.SetBounds(x + 130, 168, 170, 28);
            _horas.SetBounds(x + 302, 168, 70, 28);
            _lblDias.SetBounds(x, 202, 150, 28);
            _dias.SetBounds(x + 152, 202, 70, 28);
            _lblUrgente.SetBounds(x + 236, 202, 170, 28);
            _diasUrgente.SetBounds(x + 408, 202, 70, 28);

            int botonY = alto - 44;
            _btnGuardar.SetBounds(x, botonY, 120, 34);
            _btnRestaurar.SetBounds(x + 128, botonY, 200, 34);
            _btnPreview.SetBounds(x + 336, botonY, 130, 34);
            _btnProbar.SetBounds(x + 474, botonY, 160, 34);

            _resultado.SetBounds(x, botonY - 40, w, 36);
            _vacios.SetBounds(x, 238, Math.Min(w, 420), 26);

            int editorTop = 268;
            int editorBottom = botonY - 46;
            int editorAlto = Math.Max(160, editorBottom - editorTop);
            int textoAlto = (int)(editorAlto * 0.52);
            _texto.SetBounds(x, editorTop, w, textoAlto);
            _preview.SetBounds(x, editorTop + textoAlto + 4, w, Math.Max(70, editorAlto - textoAlto - 4));
        }

        private void LlenarVariables()
        {
            _filaVars.Controls.Clear();
            var actual = Seleccion();
            var def = actual == null ? null : WhatsAppPlantillaCatalogo.Buscar(actual.Tipo);
            if (def == null)
                return;

            foreach (string variable in def.Variables)
            {
                string token = "{" + variable + "}";
                var btn = new Button
                {
                    Text = token,
                    AutoSize = true,
                    Height = 28,
                    Margin = new Padding(0, 2, 6, 2),
                    Enabled = true
                };
                btn.Click += (_, _) =>
                {
                    _texto.SelectedText = token;
                    _texto.Focus();
                };
                _filaVars.Controls.Add(btn);
            }
        }

        private void Cargar()
        {
            try
            {
                _items = _mensajes.ListarCentroConversaciones();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Plantillas", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _items = Array.Empty<WhatsAppAutomatizacionDto>();
            }

            _cargando = true;
            int anterior = _lista.SelectedIndex;
            _lista.Items.Clear();
            foreach (var item in _items)
                _lista.Items.Add(item.Titulo);
            _cargando = false;
            if (_lista.Items.Count == 0)
                return;

            _lista.SelectedIndex = anterior >= 0 && anterior < _lista.Items.Count ? anterior : 0;
        }

        private WhatsAppAutomatizacionDto? Seleccion()
        {
            int i = _lista.SelectedIndex;
            if (i < 0 || i >= _items.Count)
                return null;
            return _items[i];
        }

        private void MostrarSeleccion()
        {
            if (_cargando)
                return;

            var item = Seleccion();
            if (item == null)
                return;

            _evento.Text = item.Evento + " — " + item.Condicion;
            _activa.Enabled = true;
            _activa.Checked = item.Activa;
            bool membresiaVencida = string.Equals(item.Tipo, "MEMBRESIA_VENCIDA", StringComparison.OrdinalIgnoreCase);
            _lblHoras.Text = membresiaVencida ? "Cada 3 días (72 h)" : "Evitar duplicado (horas)";
            _horas.Enabled = !membresiaVencida;
            _horas.Value = membresiaVencida
                ? 72
                : Math.Min(_horas.Maximum, Math.Max(_horas.Minimum, item.EvitarDuplicadoHoras));

            bool deuda = string.Equals(item.Tipo, "RECORDATORIO_VENCIMIENTO_DEUDA", StringComparison.OrdinalIgnoreCase);
            int fallbackPrimero = deuda ? TwilioSettings.DiasRecordatorioDeuda : TwilioSettings.DiasRecordatorioMembresia;
            int fallbackUrgente = deuda ? TwilioSettings.DiasRecordatorioDeudaUrgente : TwilioSettings.DiasRecordatorioMembresiaUrgente;
            _dias.Enabled = true;
            _diasUrgente.Enabled = true;
            _dias.Value = EnRango(_dias, item.DiasAntes ?? fallbackPrimero);
            _diasUrgente.Value = EnRango(_diasUrgente, item.DiasUrgente ?? fallbackUrgente);

            _texto.ReadOnly = false;
            _texto.Text = item.Plantilla;
            _resultado.Text = string.IsNullOrWhiteSpace(item.UltimoResultado)
                ? "Sin resultado registrado."
                : $"Último resultado ({item.UltimaEjecucion:dd/MM HH:mm}): {item.UltimoResultado}";
            LlenarVariables();
            ActualizarPreview();
        }

        private static decimal EnRango(NumericUpDown control, int valor) =>
            Math.Min(control.Maximum, Math.Max(control.Minimum, valor));

        private void ActualizarPreview()
        {
            _preview.Text = _mensajes.PrevisualizarPlantilla(_texto.Text, _vacios.Checked);
            string mostrado = string.IsNullOrWhiteSpace(_preview.Text) ? _texto.Text : _preview.Text;
            var compat = WhatsAppPlantillaCompatibilidad.Evaluar(true, mostrado);
            if (!compat.PuedeEnviar)
                _resultado.Text = compat.Aviso;
        }

        private void Guardar()
        {
            var item = Seleccion();
            if (item == null)
                return;

            try
            {
                int? dias = _dias.Value > 0 ? (int)_dias.Value : null;
                int? urgente = _diasUrgente.Value > 0 ? (int)_diasUrgente.Value : null;
                _mensajes.GuardarCentroConversaciones(
                    item.Tipo,
                    _texto.Text,
                    _activa.Checked,
                    dias,
                    urgente,
                    (int)_horas.Value);
                MessageBox.Show(this,
                    "Guardado. Aplica a los próximos envíos. No se reenviaron notificaciones anteriores.",
                    "Plantillas",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                Cargar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Plantillas", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Restaurar()
        {
            var item = Seleccion();
            if (item == null)
                return;

            _texto.Text = _mensajes.RestaurarPlantillaPredeterminada(item.Tipo);
            ActualizarPreview();
        }

        private void ProbarSinEnviar()
        {
            ActualizarPreview();
            if (string.IsNullOrWhiteSpace(_preview.Text))
            {
                MessageBox.Show(this, "La vista previa quedó vacía. No hay nada que probar.", "Plantillas",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_clienteId is not int clienteId || clienteId <= 0)
            {
                MessageBox.Show(this,
                    "Vista previa lista. Abra un miembro en el chat para probar con sus datos, sin llamar a Twilio y sin guardar el mensaje.",
                    "Plantillas", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ChatEnvioResult resultado = _chat.EnviarMensajeManual(
                clienteId,
                _preview.Text,
                simular: true,
                usarPlantillaAprobada: true);

            MessageBox.Show(this, resultado.Detalle, "Prueba",
                MessageBoxButtons.OK,
                resultado.Exito ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }
    }
}
