namespace BLL.Models.Crm
{
    /// <summary>
    /// Rol de un servicio en la cadena de decisiones (FASE 10.3).
    /// </summary>
    public enum DecisionSourceRole
    {
        /// <summary>Calcula y es dueño de la métrica.</summary>
        CanonicalOwner = 1,

        /// <summary>Compone a partir de owners · no redefine fórmulas base.</summary>
        Composer = 2,

        /// <summary>Solo presenta · nunca recalcula.</summary>
        UiConsumer = 3,

        /// <summary>Orquesta reglas (FASE 10.8+) · consume owners/composers.</summary>
        DecisionEngine = 4
    }

    /// <summary>Entrada del mapa SSOT (FASE 10.3).</summary>
    public sealed class DecisionSourceDescriptor
    {
        public string ServiceName { get; init; } = string.Empty;
        public DecisionSourceRole Role { get; init; }
        public string Owns { get; init; } = string.Empty;
        public string MustNot { get; init; } = string.Empty;
        public string ConsumedBy { get; init; } = string.Empty;
        public string Phase { get; init; } = string.Empty;
    }
}
