using DTO;
using System;
using System.Windows.Forms;

namespace UI.DISEÑO
{
    /// <summary>
    /// Lógica de la ficha de salud. Los controles viven en el Designer.
    /// </summary>
    partial class FrmClientes
    {
        private bool _fichaUiLista;
        private bool _syncEnfermedades;

        private void InicializarFichaSaludUi()
        {
            if (_fichaUiLista || DesignMode)
                return;

            WireFichaEventos();
            dtpCirugiasFecha.MaxDate = DateTime.Today;
            if (dtpCirugiasFecha.Value.Date > DateTime.Today)
                dtpCirugiasFecha.Value = DateTime.Today;

            ActualizarEdad();
            _fichaUiLista = true;
        }

        private void WireFichaEventos()
        {
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

            txtFecha.ValueChanged -= TxtFecha_ValueChanged;
            txtFecha.ValueChanged += TxtFecha_ValueChanged;
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
            lblListaMedicamentos.Enabled = toma;
            txtListaMedicamentos.Enabled = toma;
            if (!toma)
                txtListaMedicamentos.Clear();
        }

        private void ActualizarUiAlergias()
        {
            bool tiene = rbAlergiasSi.Checked;
            lblAlergiasDescripcion.Enabled = tiene;
            txtAlergiasDescripcion.Enabled = tiene;
            if (!tiene)
                txtAlergiasDescripcion.Clear();
        }

        private void ActualizarUiCirugias()
        {
            bool tiene = rbCirugiasSi.Checked;
            lblCirugiasDescripcion.Enabled = tiene;
            txtCirugiasDescripcion.Enabled = tiene;
            lblCirugiasFecha.Enabled = tiene;
            dtpCirugiasFecha.Enabled = tiene;
            lblCirugiaAntiguedad.Enabled = tiene;
            if (!tiene)
            {
                txtCirugiasDescripcion.Clear();
                lblCirugiaAntiguedad.Text = "";
            }
            else
            {
                ActualizarAntiguedadCirugia();
            }
        }

        private void ActualizarAntiguedadCirugia()
        {
            if (!rbCirugiasSi.Checked)
            {
                lblCirugiaAntiguedad.Text = "";
                return;
            }

            lblCirugiaAntiguedad.Text = FormatearTiempoDesde(dtpCirugiasFecha.Value.Date, DateTime.Today);
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
            txtHorarioVariadoDetalle.Enabled = variado;
            if (!variado)
                txtHorarioVariadoDetalle.Clear();
        }

        private void TxtFecha_ValueChanged(object? sender, EventArgs e) => ActualizarEdad();

        private void ActualizarEdad()
        {
            DateTime nac = txtFecha.Value.Date;
            int edad = DateTime.Today.Year - nac.Year;
            if (nac > DateTime.Today.AddYears(-edad))
                edad--;
            if (edad < 0) edad = 0;
            lblEdad.Text = edad.ToString();
        }

        private void LimpiarFichaSaludUi()
        {
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
            lblCirugiaAntiguedad.Text = "";
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

        private ClienteFichaSaludDTO ConstruirFichaDesdeUi()
        {
            return new ClienteFichaSaludDTO
            {
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
                FechaIngreso = dtpFechaIngreso.Value.Date
            };
        }

        private void PoblarFichaEnUi(ClienteFichaSaludDTO? ficha)
        {
            if (ficha is null)
            {
                LimpiarFichaSaludUi();
                return;
            }

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

            if (ficha.FechaIngreso.HasValue)
            {
                DateTime ingreso = ficha.FechaIngreso.Value.Date;
                if (ingreso < dtpFechaIngreso.MinDate)
                    ingreso = dtpFechaIngreso.MinDate;
                if (ingreso > dtpFechaIngreso.MaxDate)
                    ingreso = dtpFechaIngreso.MaxDate;
                dtpFechaIngreso.Value = ingreso;
            }
            else
            {
                dtpFechaIngreso.Value = DateTime.Today;
            }
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
