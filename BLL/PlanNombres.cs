namespace BLL
{
    /// <summary>Nombres de plan con comportamiento especial en POS.</summary>
    public static class PlanNombres
    {
        public const string Oferta = "OFERTA";
        public const string Atleta = "ATLETA";
        public const string Visita = "VISITA";

        /// <summary>
        /// Cliente técnico para cobros ATLETA/VISITA sin miembro permanente.
        /// No debe aparecer en cmbCliente ni en Estado como ACTIVO.
        /// </summary>
        public const string ClienteVisitanteSistema = "VISITANTE (SISTEMA)";

        public static bool EsOferta(string? nombre) =>
            Igual(nombre, Oferta);

        public static bool EsClienteVisitanteSistema(string? nombre) =>
            Igual(nombre, ClienteVisitanteSistema);

        /// <summary>
        /// Acceso parcial (día/visita): cobra caja + historial, sin Membresias ni Estado ACTIVO.
        /// </summary>
        public static bool EsParcial(string? nombre) =>
            EsAtleta(nombre) || EsVisita(nombre);

        public static bool EsAtleta(string? nombre) =>
            Igual(nombre, Atleta);

        public static bool EsVisita(string? nombre) =>
            Igual(nombre, Visita);

        /// <summary>TipoMovimiento en HistorialMembresias (no afecta SALIDA/PAGO de Estado).</summary>
        public static string TipoHistorialParcial(string? nombrePlan)
        {
            if (EsAtleta(nombrePlan))
                return Atleta;
            if (EsVisita(nombrePlan))
                return Visita;
            return "PARCIAL";
        }

        private static bool Igual(string? nombre, string esperado) =>
            string.Equals((nombre ?? string.Empty).Trim(), esperado, StringComparison.OrdinalIgnoreCase);
    }
}
