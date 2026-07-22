namespace DTO
{
    /// <summary>
    /// Ficha de salud y emergencia capturada al agregar un cliente.
    /// </summary>
    public sealed class ClienteFichaSaludDTO
    {
        public int ClienteId { get; set; }

        public string? EmergenciaNombre { get; set; }
        public string? EmergenciaParentesco { get; set; }
        public string? EmergenciaTelefono { get; set; }
        public string? EmergenciaTelefonoAlt { get; set; }

        public bool Diabetes { get; set; }
        public bool Hipertension { get; set; }
        public bool Asma { get; set; }
        public bool ProblemasCardiacos { get; set; }
        public bool ColesterolAlto { get; set; }
        public bool Artritis { get; set; }
        public bool Hernia { get; set; }
        public bool Epilepsia { get; set; }
        public bool Embarazo { get; set; }
        public bool NingunaEnfermedad { get; set; }
        public string? EnfermedadOtra { get; set; }

        public bool LesionHombro { get; set; }
        public bool LesionRodilla { get; set; }
        public bool LesionEspalda { get; set; }
        public bool LesionCuello { get; set; }
        public bool LesionTobillo { get; set; }
        public bool LesionCadera { get; set; }
        public bool LesionOtro { get; set; }
        public string? LesionDescripcion { get; set; }

        public bool TomaMedicamentos { get; set; }
        public string? ListaMedicamentos { get; set; }

        public bool TieneAlergias { get; set; }
        public string? AlergiasDescripcion { get; set; }

        public bool TieneCirugias { get; set; }
        public string? CirugiasDescripcion { get; set; }
        public DateTime? CirugiasFecha { get; set; }

        public bool ObjPerderGrasa { get; set; }
        public bool ObjGanarMasa { get; set; }
        public bool ObjTonificar { get; set; }
        public bool ObjMejorarCondicion { get; set; }
        public bool ObjRehabilitacion { get; set; }
        public bool ObjSalud { get; set; }
        public bool ObjCompetencia { get; set; }
        public bool ObjOtro { get; set; }
        public string? ObjOtroDescripcion { get; set; }

        /// <summary>Nunca | Menos6Meses | 1Ano | 2Anos | Mas5Anos</summary>
        public string? ExperienciaNivel { get; set; }

        /// <summary>Manana | Tarde | Noche | Variado</summary>
        public string? HorarioPreferido { get; set; }
        public string? HorarioVariadoDetalle { get; set; }

        public DateTime? FechaIngreso { get; set; }
    }
}
