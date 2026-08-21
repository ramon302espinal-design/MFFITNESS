using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DTO;

namespace UI.DISEÑO.Controles
{
    public partial class UcFichaResumenMiembro : UserControl
    {
        private static readonly Color ColorValorNormal = Color.FromArgb(15, 23, 42);
        private static readonly Color ColorAlertaMedica = Color.FromArgb(185, 28, 28);

        private int _clienteId;
        private bool _tieneMiembro;

        public event EventHandler? EditarInformacionClick;
        public event EventHandler? EliminarMiembroClick;

        public bool TieneMiembroCargado => _tieneMiembro;
        public int ClienteIdCargado => _clienteId;

        public UcFichaResumenMiembro()
        {
            InitializeComponent();

            btnEditarInformacion.Click += (_, _) => EditarInformacionClick?.Invoke(this, EventArgs.Empty);
            btnEliminarMiembro.Click += (_, _) => EliminarMiembroClick?.Invoke(this, EventArgs.Empty);

            Limpiar();
        }

        public void Limpiar()
        {
            _clienteId = 0;
            _tieneMiembro = false;
            btnEditarInformacion.Enabled = false;
            btnEliminarMiembro.Enabled = false;

            SetValor(lblIdValor, "—");
            SetValor(lblNombreValor, "—");
            SetValor(lblFechaNacValor, "—");
            SetValor(lblEdadValor, "—");
            SetValor(lblSexoValor, "—");
            SetValor(lblTelefonoValor, "—");
            SetValor(lblDireccionValor, "—");
            SetValor(lblFechaIngresoValor, "—");

            SetValor(lblEmNombreValor, "—");
            SetValor(lblEmParentescoValor, "—");
            SetValor(lblEmTelValor, "—");
            SetValor(lblEmTelAltValor, "—");

            SetValor(lblObjetivosValor, "—");
            SetValor(lblExperienciaValor, "—");
            SetValor(lblHorarioValor, "—");

            SetValorMedico(lblEnfermedadesValor, "—", alerta: false);
            SetValorMedico(lblLesionesValor, "—", alerta: false);
            SetValorMedico(lblMedicamentosValor, "—", alerta: false);
            SetValorMedico(lblAlergiasValor, "—", alerta: false);
            SetValorMedico(lblCirugiasValor, "—", alerta: false);
        }

        public void Mostrar(
            int id,
            string nombre,
            string telefono,
            string direccion,
            DateTime? fechaNacimiento,
            string? sexo,
            ClienteFichaSaludDTO? ficha)
        {
            _clienteId = id;
            _tieneMiembro = id > 0;
            btnEditarInformacion.Enabled = _tieneMiembro;
            btnEliminarMiembro.Enabled = _tieneMiembro;

            SetValor(lblIdValor, id > 0 ? id.ToString() : "—");
            SetValor(lblNombreValor, TextoOGuion(nombre));
            SetValor(lblTelefonoValor, TextoOGuion(telefono));
            SetValor(lblDireccionValor, TextoOGuion(direccion));

            if (fechaNacimiento.HasValue)
            {
                SetValor(lblFechaNacValor, fechaNacimiento.Value.ToString("dd/MM/yyyy"));
                SetValor(lblEdadValor, CalcularEdad(fechaNacimiento.Value).ToString());
            }
            else
            {
                SetValor(lblFechaNacValor, "—");
                SetValor(lblEdadValor, "—");
            }

            SetValor(lblSexoValor, string.IsNullOrWhiteSpace(sexo) ? "No registrado" : sexo.Trim());

            if (ficha?.FechaIngreso.HasValue == true)
                SetValor(lblFechaIngresoValor, ficha.FechaIngreso.Value.ToString("dd/MM/yyyy"));
            else
                SetValor(lblFechaIngresoValor, "—");

            if (ficha is null)
            {
                SetValor(lblEmNombreValor, "—");
                SetValor(lblEmParentescoValor, "—");
                SetValor(lblEmTelValor, "—");
                SetValor(lblEmTelAltValor, "—");
                SetValor(lblObjetivosValor, "Sin objetivos registrados");
                SetValor(lblExperienciaValor, "No registrada");
                SetValor(lblHorarioValor, "No registrado");
                SetValorMedico(lblEnfermedadesValor, "Ninguna", alerta: false);
                SetValorMedico(lblLesionesValor, "Sin observaciones médicas", alerta: false);
                SetValorMedico(lblMedicamentosValor, "No toma medicamentos", alerta: false);
                SetValorMedico(lblAlergiasValor, "Sin alergias", alerta: false);
                SetValorMedico(lblCirugiasValor, "Sin cirugías", alerta: false);
                return;
            }

            SetValor(lblEmNombreValor, TextoOGuion(ficha.EmergenciaNombre));
            SetValor(lblEmParentescoValor, TextoOGuion(ficha.EmergenciaParentesco));
            SetValor(lblEmTelValor, TextoOGuion(ficha.EmergenciaTelefono));
            SetValor(lblEmTelAltValor, TextoOGuion(ficha.EmergenciaTelefonoAlt));

            SetValor(lblObjetivosValor, FormatearObjetivos(ficha));
            SetValor(lblExperienciaValor, FormatearExperiencia(ficha.ExperienciaNivel));
            SetValor(lblHorarioValor, FormatearHorario(ficha));

            string enfermedades = FormatearEnfermedades(ficha);
            SetValorMedico(lblEnfermedadesValor, enfermedades, EsAlertaMedica(enfermedades));

            string lesiones = FormatearLesiones(ficha);
            SetValorMedico(lblLesionesValor, lesiones, EsAlertaMedica(lesiones));

            string medicamentos = FormatearMedicamentos(ficha);
            SetValorMedico(lblMedicamentosValor, medicamentos, EsAlertaMedica(medicamentos));

            string alergias = FormatearAlergias(ficha);
            SetValorMedico(lblAlergiasValor, alergias, EsAlertaMedica(alergias));

            string cirugias = FormatearCirugias(ficha);
            SetValorMedico(lblCirugiasValor, cirugias, EsAlertaMedica(cirugias));
        }

        private static void SetValor(Label lbl, string texto)
        {
            lbl.Text = texto;
            lbl.ForeColor = ColorValorNormal;
        }

        private static void SetValorMedico(Label lbl, string texto, bool alerta)
        {
            lbl.Text = texto;
            lbl.ForeColor = alerta ? ColorAlertaMedica : ColorValorNormal;
        }

        private static string TextoOGuion(string? valor) =>
            string.IsNullOrWhiteSpace(valor) ? "—" : valor.Trim();

        private static int CalcularEdad(DateTime fechaNacimiento)
        {
            DateTime nac = fechaNacimiento.Date;
            int edad = DateTime.Today.Year - nac.Year;
            if (nac > DateTime.Today.AddYears(-edad))
                edad--;
            return edad < 0 ? 0 : edad;
        }

        private static bool EsAlertaMedica(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return false;

            string t = texto.Trim();
            return !string.Equals(t, "Ninguna", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(t, "Sin observaciones médicas", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(t, "Sin observaciones", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(t, "No toma medicamentos", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(t, "Sin alergias", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(t, "Sin cirugías", StringComparison.OrdinalIgnoreCase)
                && t != "—";
        }

        private static string FormatearObjetivos(ClienteFichaSaludDTO ficha)
        {
            var items = new List<string>();
            if (ficha.ObjPerderGrasa) items.Add("Perder grasa");
            if (ficha.ObjGanarMasa) items.Add("Ganar masa muscular");
            if (ficha.ObjTonificar) items.Add("Tonificar");
            if (ficha.ObjMejorarCondicion) items.Add("Mejorar condición");
            if (ficha.ObjRehabilitacion) items.Add("Rehabilitación");
            if (ficha.ObjSalud) items.Add("Salud general");
            if (ficha.ObjCompetencia) items.Add("Competencia");
            if (ficha.ObjOtro)
            {
                string otro = string.IsNullOrWhiteSpace(ficha.ObjOtroDescripcion)
                    ? "Otro"
                    : $"Otro ({ficha.ObjOtroDescripcion.Trim()})";
                items.Add(otro);
            }

            return items.Count == 0 ? "Sin objetivos registrados" : string.Join(", ", items);
        }

        private static string FormatearExperiencia(string? codigo) => codigo switch
        {
            "Nunca" => "Nunca entrenó",
            "Menos6Meses" => "Menos de 6 meses",
            "1Ano" => "Alrededor de 1 año",
            "2Anos" => "Alrededor de 2 años",
            "Mas5Anos" => "Más de 5 años",
            _ => string.IsNullOrWhiteSpace(codigo) ? "No registrada" : codigo
        };

        private static string FormatearHorario(ClienteFichaSaludDTO ficha)
        {
            string baseHorario = ficha.HorarioPreferido switch
            {
                "Manana" => "Mañana",
                "Tarde" => "Tarde",
                "Noche" => "Noche",
                "Variado" => "Variado",
                _ => string.IsNullOrWhiteSpace(ficha.HorarioPreferido) ? "No registrado" : ficha.HorarioPreferido
            };

            if (string.Equals(ficha.HorarioPreferido, "Variado", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(ficha.HorarioVariadoDetalle))
            {
                return $"{baseHorario}: {ficha.HorarioVariadoDetalle.Trim()}";
            }

            return baseHorario;
        }

        private static string FormatearEnfermedades(ClienteFichaSaludDTO ficha)
        {
            if (ficha.NingunaEnfermedad)
                return "Ninguna";

            var items = new List<string>();
            if (ficha.Diabetes) items.Add("Diabetes");
            if (ficha.Hipertension) items.Add("Hipertensión");
            if (ficha.Asma) items.Add("Asma");
            if (ficha.ProblemasCardiacos) items.Add("Problemas cardíacos");
            if (ficha.ColesterolAlto) items.Add("Colesterol alto");
            if (ficha.Artritis) items.Add("Artritis");
            if (ficha.Hernia) items.Add("Hernia");
            if (ficha.Epilepsia) items.Add("Epilepsia");
            if (ficha.Embarazo) items.Add("Embarazo");
            if (!string.IsNullOrWhiteSpace(ficha.EnfermedadOtra))
                items.Add(ficha.EnfermedadOtra.Trim());

            return items.Count == 0 ? "Ninguna" : string.Join(", ", items);
        }

        private static string FormatearLesiones(ClienteFichaSaludDTO ficha)
        {
            var zonas = new List<string>();
            if (ficha.LesionHombro) zonas.Add("Hombro");
            if (ficha.LesionRodilla) zonas.Add("Rodilla");
            if (ficha.LesionEspalda) zonas.Add("Espalda");
            if (ficha.LesionCuello) zonas.Add("Cuello");
            if (ficha.LesionTobillo) zonas.Add("Tobillo");
            if (ficha.LesionCadera) zonas.Add("Cadera");
            if (ficha.LesionOtro) zonas.Add("Otro");

            bool tieneDesc = !string.IsNullOrWhiteSpace(ficha.LesionDescripcion);
            if (zonas.Count == 0 && !tieneDesc)
                return "Sin observaciones médicas";

            if (zonas.Count == 0)
                return ficha.LesionDescripcion!.Trim();

            string texto = string.Join(", ", zonas);
            if (tieneDesc)
                texto += $" — {ficha.LesionDescripcion!.Trim()}";
            return texto;
        }

        private static string FormatearMedicamentos(ClienteFichaSaludDTO ficha)
        {
            if (!ficha.TomaMedicamentos)
                return "No toma medicamentos";
            return string.IsNullOrWhiteSpace(ficha.ListaMedicamentos)
                ? "Toma medicamentos (sin detalle)"
                : ficha.ListaMedicamentos.Trim();
        }

        private static string FormatearAlergias(ClienteFichaSaludDTO ficha)
        {
            if (!ficha.TieneAlergias)
                return "Sin alergias";
            return string.IsNullOrWhiteSpace(ficha.AlergiasDescripcion)
                ? "Tiene alergias (sin detalle)"
                : ficha.AlergiasDescripcion.Trim();
        }

        private static string FormatearCirugias(ClienteFichaSaludDTO ficha)
        {
            if (!ficha.TieneCirugias)
                return "Sin cirugías";

            var partes = new List<string>();
            if (!string.IsNullOrWhiteSpace(ficha.CirugiasDescripcion))
                partes.Add(ficha.CirugiasDescripcion.Trim());
            if (ficha.CirugiasFecha.HasValue)
                partes.Add($"Fecha: {ficha.CirugiasFecha.Value:dd/MM/yyyy}");

            return partes.Count == 0 ? "Tiene cirugías (sin detalle)" : string.Join(" · ", partes);
        }
    }
}
