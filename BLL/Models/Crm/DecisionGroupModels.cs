namespace BLL.Models.Crm
{
    /// <summary>Grupo de DecisionEvents relacionados (FASE 10.18 / TEST 10).</summary>
    public sealed class DecisionGroup
    {
        public string GroupId { get; init; } = string.Empty;

        /// <summary>Clave estable: Entity|Id o Theme|Period.</summary>
        public string GroupKey { get; init; } = string.Empty;

        public string Title { get; init; } = string.Empty;
        public string Summary { get; init; } = string.Empty;

        public DecisionEntityType EntityType { get; init; }
        public string? EntityId { get; init; }
        public string EntityName { get; init; } = string.Empty;

        public DecisionSeverity Severity { get; init; }
        public DecisionPriority Priority { get; init; }

        public IReadOnlyList<DecisionEvent> Events { get; init; }
            = Array.Empty<DecisionEvent>();

        /// <summary>Evento líder del grupo (mayor prioridad/severidad).</summary>
        public DecisionEvent? Primary { get; init; }

        /// <summary>Narrativa suave del grupo (FASE 10.19).</summary>
        public string Recommendation { get; init; } = string.Empty;

        public int EventCount => Events.Count;
    }
}
