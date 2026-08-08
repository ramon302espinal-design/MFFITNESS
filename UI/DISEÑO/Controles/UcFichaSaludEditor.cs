using System;
using System.Windows.Forms;

namespace UI.DISEÑO.Controles
{
    public partial class UcFichaSaludEditor : UserControl
    {
        private bool _eventosWiriados;
        private bool _syncEnfermedades;
        private int _clienteId;
        private bool _soloLectura;

        public UcFichaSaludEditor()
        {
            InitializeComponent();
            if (!DesignMode)
                WireEventos();
        }

        public void WireEventos()
        {
            if (_eventosWiriados || DesignMode)
                return;

            void OnEnfermedadChanged(object? s, EventArgs e) => SincronizarNingunaEnfermedad(s as CheckBox);

            chkDiabetes.CheckedChanged += OnEnfermedadChanged;
            chkHipertension.CheckedChanged += OnEnfermedadChanged;
            chkAsma.CheckedChanged += OnEnfermedadChanged;
            chkProblemasCardiacos.CheckedChanged += OnEnfermedadChanged;
            chkColesterolAlto.CheckedChanged += OnEnfermedadChanged;
            chkArtritis.CheckedChanged += OnEnfermedadChanged;
            chkHernia.CheckedChanged += OnEnfermedadChanged;
            chkEpilepsia.CheckedChanged += OnEnfermedadChanged;
            chkEmbarazo.CheckedChanged += OnEnfermedadChanged;
            chkNingunaEnfermedad.CheckedChanged += OnEnfermedadChanged;

            rbMedicamentosSi.CheckedChanged += (_, _) => ActualizarUiMedicamentos();
            rbMedicamentosNo.CheckedChanged += (_, _) => ActualizarUiMedicamentos();
            rbAlergiasSi.CheckedChanged += (_, _) => ActualizarUiAlergias();
            rbAlergiasNo.CheckedChanged += (_, _) => ActualizarUiAlergias();
            rbCirugiasSi.CheckedChanged += (_, _) => ActualizarUiCirugias();
            rbCirugiasNo.CheckedChanged += (_, _) => ActualizarUiCirugias();
            dtpCirugiasFecha.ValueChanged += (_, _) => ActualizarAntiguedadCirugia();

            chkObjOtro.CheckedChanged += (_, _) =>
            {
                bool otro = chkObjOtro.Checked;
                txtObjOtroDescripcion.Enabled = otro;
                if (!otro)
                    txtObjOtroDescripcion.Clear();
            };

            rbHorVariado.CheckedChanged += (_, _) => ActualizarUiHorario();
            rbHorManana.CheckedChanged += (_, _) => ActualizarUiHorario();
            rbHorTarde.CheckedChanged += (_, _) => ActualizarUiHorario();
            rbHorNoche.CheckedChanged += (_, _) => ActualizarUiHorario();

            dtpCirugiasFecha.MaxDate = DateTime.Today;
            if (dtpCirugiasFecha.Value.Date > DateTime.Today)
                dtpCirugiasFecha.Value = DateTime.Today;

            ActualizarUiMedicamentos();
            ActualizarUiAlergias();
            ActualizarUiCirugias();
            ActualizarUiHorario();

            _eventosWiriados = true;
        }

        public void Limpiar()
        {
            _clienteId = 0;

            txtEmergenciaNombre.Clear();
            txtEmergenciaParentesco.Clear();
            txtEmergenciaTelefono.Clear();
            txtEmergenciaTelefonoAlt.Clear();

            _syncEnfermedades = true;
            try
            {
                chkDiabetes.Checked = false;
                chkHipertension.Checked = false;
                chkAsma.Checked = false;
                chkProblemasCardiacos.Checked = false;
                chkColesterolAlto.Checked = false;
                chkArtritis.Checked = false;
                chkHernia.Checked = false;
                chkEpilepsia.Checked = false;
                chkEmbarazo.Checked = false;
                chkNingunaEnfermedad.Checked = false;
            }
            finally
            {
                _syncEnfermedades = false;
            }

            txtEnfermedadOtra.Clear();

            chkLesionHombro.Checked = false;
            chkLesionRodilla.Checked = false;
            chkLesionEspalda.Checked = false;
            chkLesionCuello.Checked = false;
            chkLesionTobillo.Checked = false;
            chkLesionCadera.Checked = false;
            txtLesionDescripcion.Clear();

            rbMedicamentosNo.Checked = true;
            ActualizarUiMedicamentos();

            rbAlergiasNo.Checked = true;
            ActualizarUiAlergias();

            rbCirugiasNo.Checked = true;
            dtpCirugiasFecha.Value = DateTime.Today;
            lblAntiguedadCirugia.Text = "";
            ActualizarUiCirugias();

            chkObjPerderGrasa.Checked = false;
            chkObjGanarMasa.Checked = false;
            chkObjTonificar.Checked = false;
            chkObjMejorarCondicion.Checked = false;
            chkObjRehabilitacion.Checked = false;
            chkObjSalud.Checked = false;
            chkObjCompetencia.Checked = false;
            chkObjOtro.Checked = false;
            txtObjOtroDescripcion.Clear();
            txtObjOtroDescripcion.Enabled = false;

            rbExpNunca.Checked = false;
            rbExpMenos6.Checked = false;
            rbExp1Ano.Checked = false;
            rbExp2Anos.Checked = false;
            rbExpMas5.Checked = false;

            rbHorManana.Checked = false;
            rbHorTarde.Checked = false;
            rbHorNoche.Checked = false;
            rbHorVariado.Checked = false;
            ActualizarUiHorario();

            dtpFechaIngresoFicha.Value = DateTime.Today;
            AplicarSoloLecturaSiCorresponde();
        }

        public void Cargar(DTO.ClienteFichaSaludDTO? ficha, int clienteId = 0)
        {
            if (ficha is null)
            {
                Limpiar();
                _clienteId = clienteId;
                return;
            }

            _clienteId = ficha.ClienteId > 0 ? ficha.ClienteId : clienteId;

            txtEmergenciaNombre.Text = ficha.EmergenciaNombre ?? "";
            txtEmergenciaParentesco.Text = ficha.EmergenciaParentesco ?? "";
            txtEmergenciaTelefono.Text = ficha.EmergenciaTelefono ?? "";
            txtEmergenciaTelefonoAlt.Text = ficha.EmergenciaTelefonoAlt ?? "";

            _syncEnfermedades = true;
            try
            {
                chkDiabetes.Checked = ficha.Diabetes;
                chkHipertension.Checked = ficha.Hipertension;
                chkAsma.Checked = ficha.Asma;
                chkProblemasCardiacos.Checked = ficha.ProblemasCardiacos;
                chkColesterolAlto.Checked = ficha.ColesterolAlto;
                chkArtritis.Checked = ficha.Artritis;
                chkHernia.Checked = ficha.Hernia;
                chkEpilepsia.Checked = ficha.Epilepsia;
                chkEmbarazo.Checked = ficha.Embarazo;
                chkNingunaEnfermedad.Checked = ficha.NingunaEnfermedad;
            }
            finally
            {
                _syncEnfermedades = false;
            }

            txtEnfermedadOtra.Text = ficha.EnfermedadOtra ?? "";

            chkLesionHombro.Checked = ficha.LesionHombro;
            chkLesionRodilla.Checked = ficha.LesionRodilla;
            chkLesionEspalda.Checked = ficha.LesionEspalda;
            chkLesionCuello.Checked = ficha.LesionCuello;
            chkLesionTobillo.Checked = ficha.LesionTobillo;
            chkLesionCadera.Checked = ficha.LesionCadera;
            txtLesionDescripcion.Text = ficha.LesionDescripcion ?? "";

            if (ficha.TomaMedicamentos)
                rbMedicamentosSi.Checked = true;
            else
                rbMedicamentosNo.Checked = true;
            txtListaMedicamentos.Text = ficha.ListaMedicamentos ?? "";
            ActualizarUiMedicamentos();
            if (ficha.TomaMedicamentos)
                txtListaMedicamentos.Text = ficha.ListaMedicamentos ?? "";

            if (ficha.TieneAlergias)
                rbAlergiasSi.Checked = true;
            else
                rbAlergiasNo.Checked = true;
            txtAlergiasDescripcion.Text = ficha.AlergiasDescripcion ?? "";
            ActualizarUiAlergias();
            if (ficha.TieneAlergias)
                txtAlergiasDescripcion.Text = ficha.AlergiasDescripcion ?? "";

            if (ficha.TieneCirugias)
                rbCirugiasSi.Checked = true;
            else
                rbCirugiasNo.Checked = true;
            txtCirugiasDescripcion.Text = ficha.CirugiasDescripcion ?? "";
            if (ficha.CirugiasFecha.HasValue)
            {
                DateTime fecha = ficha.CirugiasFecha.Value.Date;
                if (fecha > dtpCirugiasFecha.MaxDate)
                    fecha = dtpCirugiasFecha.MaxDate;
                if (fecha < dtpCirugiasFecha.MinDate)
                    fecha = dtpCirugiasFecha.MinDate;
                dtpCirugiasFecha.Value = fecha;
            }
            else
            {
                dtpCirugiasFecha.Value = DateTime.Today;
            }
            ActualizarUiCirugias();
            if (ficha.TieneCirugias)
                txtCirugiasDescripcion.Text = ficha.CirugiasDescripcion ?? "";

            chkObjPerderGrasa.Checked = ficha.ObjPerderGrasa;
            chkObjGanarMasa.Checked = ficha.ObjGanarMasa;
            chkObjTonificar.Checked = ficha.ObjTonificar;
            chkObjMejorarCondicion.Checked = ficha.ObjMejorarCondicion;
            chkObjRehabilitacion.Checked = ficha.ObjRehabilitacion;
            chkObjSalud.Checked = ficha.ObjSalud;
            chkObjCompetencia.Checked = ficha.ObjCompetencia;
            chkObjOtro.Checked = ficha.ObjOtro;
            txtObjOtroDescripcion.Text = ficha.ObjOtroDescripcion ?? "";
            txtObjOtroDescripcion.Enabled = ficha.ObjOtro;

            SeleccionarExperiencia(ficha.ExperienciaNivel);
            SeleccionarHorario(ficha.HorarioPreferido);
            txtHorarioVariadoDetalle.Text = ficha.HorarioVariadoDetalle ?? "";
            ActualizarUiHorario();
            if (rbHorVariado.Checked)
                txtHorarioVariadoDetalle.Text = ficha.HorarioVariadoDetalle ?? "";

            dtpFechaIngresoFicha.Value = ficha.FechaIngreso?.Date ?? DateTime.Today;
            AplicarSoloLecturaSiCorresponde();
        }

        /// <summary>
        /// En MIEMBROS: solo consulta. En AGREGAR se deja editable.
        /// </summary>
        public void SetSoloLectura(bool soloLectura)
        {
            _soloLectura = soloLectura;
            AplicarSoloLecturaSiCorresponde();
        }

        private void AplicarSoloLecturaSiCorresponde()
        {
            if (!_soloLectura)
                return;

            void Fijar(Control c)
            {
                switch (c)
                {
                    case TextBox tb:
                        tb.Enabled = true;
                        tb.ReadOnly = true;
                        tb.TabStop = false;
                        break;
                    case CheckBox chk:
                        chk.Enabled = true;
                        chk.AutoCheck = false;
                        break;
                    case RadioButton rb:
                        rb.Enabled = true;
                        rb.AutoCheck = false;
                        break;
                    case DateTimePicker dtp:
                        dtp.Enabled = false;
                        break;
                    case Label lbl:
                        lbl.Enabled = true;
                        break;
                }
            }

            foreach (Control c in Controls)
                AplicarSoloLecturaRecursivo(c, Fijar);
        }

        private static void AplicarSoloLecturaRecursivo(Control root, Action<Control> fijar)
        {
            fijar(root);
            foreach (Control child in root.Controls)
                AplicarSoloLecturaRecursivo(child, fijar);
        }

        public DTO.ClienteFichaSaludDTO Obtener()
        {
            return new DTO.ClienteFichaSaludDTO
            {
                ClienteId = _clienteId,
                EmergenciaNombre = txtEmergenciaNombre.Text,
                EmergenciaParentesco = txtEmergenciaParentesco.Text,
                EmergenciaTelefono = txtEmergenciaTelefono.Text,
                EmergenciaTelefonoAlt = txtEmergenciaTelefonoAlt.Text,
                Diabetes = chkDiabetes.Checked,
                Hipertension = chkHipertension.Checked,
                Asma = chkAsma.Checked,
                ProblemasCardiacos = chkProblemasCardiacos.Checked,
                ColesterolAlto = chkColesterolAlto.Checked,
                Artritis = chkArtritis.Checked,
                Hernia = chkHernia.Checked,
                Epilepsia = chkEpilepsia.Checked,
                Embarazo = chkEmbarazo.Checked,
                NingunaEnfermedad = chkNingunaEnfermedad.Checked,
                EnfermedadOtra = txtEnfermedadOtra.Text,
                LesionHombro = chkLesionHombro.Checked,
                LesionRodilla = chkLesionRodilla.Checked,
                LesionEspalda = chkLesionEspalda.Checked,
                LesionCuello = chkLesionCuello.Checked,
                LesionTobillo = chkLesionTobillo.Checked,
                LesionCadera = chkLesionCadera.Checked,
                LesionDescripcion = txtLesionDescripcion.Text,
                TomaMedicamentos = rbMedicamentosSi.Checked,
                ListaMedicamentos = txtListaMedicamentos.Text,
                TieneAlergias = rbAlergiasSi.Checked,
                AlergiasDescripcion = txtAlergiasDescripcion.Text,
                TieneCirugias = rbCirugiasSi.Checked,
                CirugiasDescripcion = txtCirugiasDescripcion.Text,
                CirugiasFecha = rbCirugiasSi.Checked ? dtpCirugiasFecha.Value.Date : null,
                ObjPerderGrasa = chkObjPerderGrasa.Checked,
                ObjGanarMasa = chkObjGanarMasa.Checked,
                ObjTonificar = chkObjTonificar.Checked,
                ObjMejorarCondicion = chkObjMejorarCondicion.Checked,
                ObjRehabilitacion = chkObjRehabilitacion.Checked,
                ObjSalud = chkObjSalud.Checked,
                ObjCompetencia = chkObjCompetencia.Checked,
                ObjOtro = chkObjOtro.Checked,
                ObjOtroDescripcion = txtObjOtroDescripcion.Text,
                ExperienciaNivel = ObtenerExperienciaSeleccionada(),
                HorarioPreferido = ObtenerHorarioSeleccionado(),
                HorarioVariadoDetalle = txtHorarioVariadoDetalle.Text,
                FechaIngreso = dtpFechaIngresoFicha.Value.Date
            };
        }

        private void SincronizarNingunaEnfermedad(CheckBox? origen)
        {
            if (_syncEnfermedades)
                return;

            try
            {
                _syncEnfermedades = true;
                if (origen == chkNingunaEnfermedad && chkNingunaEnfermedad.Checked)
                {
                    chkDiabetes.Checked = false;
                    chkHipertension.Checked = false;
                    chkAsma.Checked = false;
                    chkProblemasCardiacos.Checked = false;
                    chkColesterolAlto.Checked = false;
                    chkArtritis.Checked = false;
                    chkHernia.Checked = false;
                    chkEpilepsia.Checked = false;
                    chkEmbarazo.Checked = false;
                    txtEnfermedadOtra.Clear();
                }
                else if (origen != chkNingunaEnfermedad && origen != null && origen.Checked)
                {
                    chkNingunaEnfermedad.Checked = false;
                }
            }
            finally
            {
                _syncEnfermedades = false;
            }
        }

        private void ActualizarUiMedicamentos()
        {
            bool toma = rbMedicamentosSi.Checked;
            lblListaMedicamentos.Enabled = toma || _soloLectura;
            txtListaMedicamentos.Enabled = toma || _soloLectura;
            if (!toma && !_soloLectura)
                txtListaMedicamentos.Clear();
            AplicarSoloLecturaSiCorresponde();
        }

        private void ActualizarUiAlergias()
        {
            bool tiene = rbAlergiasSi.Checked;
            lblAlergiasDescripcion.Enabled = tiene || _soloLectura;
            txtAlergiasDescripcion.Enabled = tiene || _soloLectura;
            if (!tiene && !_soloLectura)
                txtAlergiasDescripcion.Clear();
            AplicarSoloLecturaSiCorresponde();
        }

        private void ActualizarUiCirugias()
        {
            bool tiene = rbCirugiasSi.Checked;
            lblCirugiasDescripcion.Enabled = tiene || _soloLectura;
            txtCirugiasDescripcion.Enabled = tiene || _soloLectura;
            lblCirugiasFecha.Enabled = tiene || _soloLectura;
            dtpCirugiasFecha.Enabled = tiene && !_soloLectura;
            lblAntiguedadCirugia.Enabled = tiene || _soloLectura;
            if (!tiene)
            {
                if (!_soloLectura)
                    txtCirugiasDescripcion.Clear();
                lblAntiguedadCirugia.Text = "";
            }
            else
            {
                ActualizarAntiguedadCirugia();
            }
            AplicarSoloLecturaSiCorresponde();
        }

        private void ActualizarAntiguedadCirugia()
        {
            if (!rbCirugiasSi.Checked)
            {
                lblAntiguedadCirugia.Text = "";
                return;
            }

            lblAntiguedadCirugia.Text = FormatearTiempoDesde(dtpCirugiasFecha.Value.Date, DateTime.Today);
        }

        /// <summary>Ej.: "Hoy", "20 días", "4 meses", "2 años".</summary>
        private static string FormatearTiempoDesde(DateTime desde, DateTime hasta)
        {
            desde = desde.Date;
            hasta = hasta.Date;
            if (desde > hasta)
                return "";

            int anios = hasta.Year - desde.Year;
            if (desde.AddYears(anios) > hasta)
                anios--;
            if (anios >= 1)
                return anios == 1 ? "1 año" : $"{anios} años";

            int meses = (hasta.Year - desde.Year) * 12 + (hasta.Month - desde.Month);
            if (desde.AddMonths(meses) > hasta)
                meses--;
            if (meses >= 1)
                return meses == 1 ? "1 mes" : $"{meses} meses";

            int dias = (hasta - desde).Days;
            if (dias == 0)
                return "Hoy";
            return dias == 1 ? "1 día" : $"{dias} días";
        }

        private void ActualizarUiHorario()
        {
            bool variado = rbHorVariado.Checked;
            txtHorarioVariadoDetalle.Enabled = variado || _soloLectura;
            if (!variado && !_soloLectura)
                txtHorarioVariadoDetalle.Clear();
            AplicarSoloLecturaSiCorresponde();
        }

        private string? ObtenerExperienciaSeleccionada()
        {
            if (rbExpNunca.Checked) return "Nunca";
            if (rbExpMenos6.Checked) return "Menos6Meses";
            if (rbExp1Ano.Checked) return "1Ano";
            if (rbExp2Anos.Checked) return "2Anos";
            if (rbExpMas5.Checked) return "Mas5Anos";
            return null;
        }

        private string? ObtenerHorarioSeleccionado()
        {
            if (rbHorManana.Checked) return "Manana";
            if (rbHorTarde.Checked) return "Tarde";
            if (rbHorNoche.Checked) return "Noche";
            if (rbHorVariado.Checked) return "Variado";
            return null;
        }

        private void SeleccionarExperiencia(string? nivel)
        {
            rbExpNunca.Checked = false;
            rbExpMenos6.Checked = false;
            rbExp1Ano.Checked = false;
            rbExp2Anos.Checked = false;
            rbExpMas5.Checked = false;

            switch (nivel)
            {
                case "Nunca": rbExpNunca.Checked = true; break;
                case "Menos6Meses": rbExpMenos6.Checked = true; break;
                case "1Ano": rbExp1Ano.Checked = true; break;
                case "2Anos": rbExp2Anos.Checked = true; break;
                case "Mas5Anos": rbExpMas5.Checked = true; break;
            }
        }

        private void SeleccionarHorario(string? horario)
        {
            rbHorManana.Checked = false;
            rbHorTarde.Checked = false;
            rbHorNoche.Checked = false;
            rbHorVariado.Checked = false;

            switch (horario)
            {
                case "Manana": rbHorManana.Checked = true; break;
                case "Tarde": rbHorTarde.Checked = true; break;
                case "Noche": rbHorNoche.Checked = true; break;
                case "Variado": rbHorVariado.Checked = true; break;
            }
        }
    }
}
