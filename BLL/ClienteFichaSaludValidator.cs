using System.Text.RegularExpressions;

namespace BLL
{
    /// <summary>
    /// Validaciones de ficha de alta (emergencia / médica / lesiones / medicamentos).
    /// </summary>
    public static class ClienteFichaSaludValidator
    {
        private static readonly Regex TelefonoSimple = new(@"^[\d\s\-\+\(\)]{7,20}$", RegexOptions.Compiled);

        public static bool Validar(DTO.ClienteFichaSaludDTO ficha, out string mensaje)
        {
            mensaje = "";

            if (string.IsNullOrWhiteSpace(ficha.EmergenciaNombre))
            {
                mensaje = "Contacto de emergencia: el nombre es obligatorio.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(ficha.EmergenciaParentesco))
            {
                mensaje = "Contacto de emergencia: el parentesco es obligatorio.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(ficha.EmergenciaTelefono))
            {
                mensaje = "Contacto de emergencia: el teléfono es obligatorio.";
                return false;
            }

            if (!TelefonoSimple.IsMatch(ficha.EmergenciaTelefono.Trim()))
            {
                mensaje = "Contacto de emergencia: teléfono inválido.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(ficha.EmergenciaTelefonoAlt)
                && !TelefonoSimple.IsMatch(ficha.EmergenciaTelefonoAlt.Trim()))
            {
                mensaje = "Contacto de emergencia: teléfono alternativo inválido.";
                return false;
            }

            bool algunaEnfermedad =
                ficha.Diabetes || ficha.Hipertension || ficha.Asma || ficha.ProblemasCardiacos
                || ficha.ColesterolAlto || ficha.Artritis || ficha.Hernia || ficha.Epilepsia
                || ficha.Embarazo || !string.IsNullOrWhiteSpace(ficha.EnfermedadOtra);

            if (!ficha.NingunaEnfermedad && !algunaEnfermedad)
            {
                mensaje = "Información médica: marque al menos una enfermedad o 'Ninguna'.";
                return false;
            }

            if (ficha.NingunaEnfermedad && algunaEnfermedad)
            {
                mensaje = "Información médica: si marca 'Ninguna', no puede marcar otras enfermedades.";
                return false;
            }

            if (ficha.NingunaEnfermedad)
                ficha.EnfermedadOtra = null;

            bool algunaLesion =
                ficha.LesionHombro || ficha.LesionRodilla || ficha.LesionEspalda
                || ficha.LesionCuello || ficha.LesionTobillo || ficha.LesionCadera;

            if (algunaLesion && string.IsNullOrWhiteSpace(ficha.LesionDescripcion))
            {
                mensaje = "Lesiones: indique una descripción breve de la lesión.";
                return false;
            }

            if (!algunaLesion)
                ficha.LesionDescripcion = null;

            if (ficha.TomaMedicamentos && string.IsNullOrWhiteSpace(ficha.ListaMedicamentos))
            {
                mensaje = "Medicamentos: si toma medicamentos, indique la lista.";
                return false;
            }

            if (!ficha.TomaMedicamentos)
                ficha.ListaMedicamentos = null;

            if (ficha.TieneAlergias && string.IsNullOrWhiteSpace(ficha.AlergiasDescripcion))
            {
                mensaje = "Alergias: si responde Sí, indique la descripción.";
                return false;
            }

            if (!ficha.TieneAlergias)
                ficha.AlergiasDescripcion = null;

            if (ficha.TieneCirugias)
            {
                if (string.IsNullOrWhiteSpace(ficha.CirugiasDescripcion))
                {
                    mensaje = "Cirugías: si responde Sí, indique la descripción.";
                    return false;
                }

                if (!ficha.CirugiasFecha.HasValue)
                {
                    mensaje = "Cirugías: indique la fecha.";
                    return false;
                }

                if (ficha.CirugiasFecha.Value.Date > DateTime.Today)
                {
                    mensaje = "Cirugías: la fecha no puede ser futura.";
                    return false;
                }
            }
            else
            {
                ficha.CirugiasDescripcion = null;
                ficha.CirugiasFecha = null;
            }

            bool algunObjetivo =
                ficha.ObjPerderGrasa || ficha.ObjGanarMasa || ficha.ObjTonificar
                || ficha.ObjMejorarCondicion || ficha.ObjRehabilitacion || ficha.ObjSalud
                || ficha.ObjCompetencia || ficha.ObjOtro;

            if (!algunObjetivo)
            {
                mensaje = "Objetivo fitness: seleccione al menos una opción.";
                return false;
            }

            if (ficha.ObjOtro && string.IsNullOrWhiteSpace(ficha.ObjOtroDescripcion))
            {
                mensaje = "Objetivo fitness: si marca 'Otro', indique el detalle.";
                return false;
            }

            if (!ficha.ObjOtro)
                ficha.ObjOtroDescripcion = null;

            if (string.IsNullOrWhiteSpace(ficha.ExperienciaNivel))
            {
                mensaje = "Experiencia: seleccione una opción.";
                return false;
            }

            string exp = ficha.ExperienciaNivel.Trim();
            if (exp is not ("Nunca" or "Menos6Meses" or "1Ano" or "2Anos" or "Mas5Anos"))
            {
                mensaje = "Experiencia: opción inválida.";
                return false;
            }

            ficha.ExperienciaNivel = exp;

            if (string.IsNullOrWhiteSpace(ficha.HorarioPreferido))
            {
                mensaje = "Horario preferido: seleccione una opción.";
                return false;
            }

            string horario = ficha.HorarioPreferido.Trim();
            if (horario is not ("Manana" or "Tarde" or "Noche" or "Variado"))
            {
                mensaje = "Horario preferido: opción inválida.";
                return false;
            }

            if (horario == "Variado" && string.IsNullOrWhiteSpace(ficha.HorarioVariadoDetalle))
            {
                mensaje = "Horario preferido: si elige Variado, explique.";
                return false;
            }

            if (horario != "Variado")
                ficha.HorarioVariadoDetalle = null;

            ficha.HorarioPreferido = horario;
            return true;
        }
    }
}
