namespace BLL.Models.Crm
{
    /// <summary>Área del DecisionEvent (FASE 10.4 / brief §9).</summary>
    public enum DecisionEventArea
    {
        Sales = 1,
        Profit = 2,
        Margin = 3,
        Roi = 4,
        Inventory = 5,
        Capital = 6,
        Product = 7,
        Trend = 8,
        Forecast = 9,
        Investment = 10,
        Liquidity = 11,
        Operations = 12
    }

    /// <summary>Entidad afectada por el evento.</summary>
    public enum DecisionEntityType
    {
        /// <summary>Portafolio / negocio completo (sin entidad).</summary>
        Portfolio = 0,
        Product = 1,
        Category = 2,
        Investment = 3,
        Period = 4
    }

    /// <summary>
    /// Estado de ciclo de vida (FASE 10.4 / brief §107 · resolución 10.22).
    /// </summary>
    public enum DecisionEventStatus
    {
        /// <summary>NUEVA — pendiente de atención.</summary>
        Active = 1,
        /// <summary>RESUELTA — el usuario cerró el caso.</summary>
        Resolved = 2,
        /// <summary>IGNORADA — descartada conscientemente.</summary>
        Ignored = 3,
        /// <summary>Condición no evaluable (TEST 7 / producto nuevo).</summary>
        InsufficientData = 4,
        /// <summary>EN REVISIÓN — el usuario está atendiendo (FASE 10.22).</summary>
        InReview = 5
    }

    /// <summary>
    /// Severidad del evento (FASE 10.5 / brief §11).
    /// Depende del impacto real — distinta de Prioridad (10.6).
    /// </summary>
    public enum DecisionSeverity
    {
        Unspecified = 0,
        Info = 1,
        Low = 2,
        Medium = 3,
        High = 4,
        Critical = 5
    }

    /// <summary>Nivel de impacto por dimensión (FASE 10.5 / brief §14).</summary>
    public enum DecisionImpactLevel
    {
        None = 0,
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    /// <summary>
    /// Evaluación de impacto multi-dimensión (FASE 10.5).
    /// Prioridad/urgencia = FASE 10.6 · umbrales monetarios = 10.7+.
    /// </summary>
    public sealed class DecisionImpactAssessment
    {
        public DecisionImpactLevel Financial { get; init; }
        public DecisionImpactLevel Sales { get; init; }
        public DecisionImpactLevel Inventory { get; init; }
        public DecisionImpactLevel Liquidity { get; init; }
        public DecisionImpactLevel Capital { get; init; }
        public DecisionImpactLevel Operational { get; init; }

        /// <summary>TEST 11: capital alto pero aún vende → no forzar Critical.</summary>
        public bool ProductStillSelling { get; init; }

        /// <summary>Contexto estacional (TEST 12) — amortigua severidad de tendencia.</summary>
        public bool SeasonalContext { get; init; }

        /// <summary>TEST 7/13: no alerta avanzada.</summary>
        public bool InsufficientData { get; init; }
    }

    /// <summary>
    /// Prioridad de revisión (FASE 10.6 / brief §12–§13).
    /// Independiente de Severidad. Ordena qué revisar primero.
    /// </summary>
    public enum DecisionPriority
    {
        Unspecified = 0,
        /// <summary>INFORMATIVA — no exige cola de acción.</summary>
        Info = 1,
        Low = 2,
        Medium = 3,
        High = 4,
        Critical = 5
    }

    /// <summary>Urgencia temporal (brief §15). Distinta del impacto/severidad.</summary>
    public enum DecisionUrgencyLevel
    {
        None = 0,
        Low = 1,
        Medium = 2,
        High = 3,
        /// <summary>Revisión inmediata recomendada (quiebre, ventana corta).</summary>
        Immediate = 4
    }

    /// <summary>
    /// Entrada para resolver prioridad. Umbrales monetarios = 10.7.
    /// </summary>
    public sealed class DecisionPriorityAssessment
    {
        public DecisionSeverity Severity { get; init; }
        public DecisionUrgencyLevel Urgency { get; init; }

        /// <summary>El usuario debería mirarlo hoy / ahora.</summary>
        public bool RequiresImmediateReview { get; init; }

        /// <summary>Riesgo de quiebre con demanda activa.</summary>
        public bool TimeSensitiveStockout { get; init; }

        /// <summary>Oportunidad con ventana (crecimiento + stock OK).</summary>
        public bool OpportunityWindow { get; init; }

        /// <summary>TEST 11: sigue vendiendo → no subir urgencia de capital solo.</summary>
        public bool ProductStillSelling { get; init; }

        public bool InsufficientData { get; init; }
    }

    /// <summary>Hecho de evidencia (snapshot mínimo para explicación/auditoría).</summary>
    public sealed class DecisionEvidenceFact
    {
        public string Key { get; init; } = string.Empty;
        public string Label { get; init; } = string.Empty;
        public string ValueText { get; init; } = string.Empty;
        /// <summary>Clave del catálogo 10.2 si aplica.</summary>
        public string? MetricKey { get; init; }
    }

    /// <summary>
    /// Evento de decisión: "algo relevante ocurrió" (FASE 10.4).
    /// No ejecuta acciones. No persiste aún (10.21+).
    /// </summary>
    public sealed class DecisionEvent
    {
        public Guid EventId { get; init; }

        /// <summary>Código de tipo (catálogo), ej. sales.strong_decline.</summary>
        public string EventType { get; init; } = string.Empty;

        public DecisionEventArea Area { get; init; }
        public DecisionEntityType EntityType { get; init; }
        public string? EntityId { get; init; }
        public string EntityName { get; init; } = string.Empty;

        /// <summary>Clave de período (ej. ThisMonth|2026-08) para fingerprint.</summary>
        public string? PeriodKey { get; init; }

        public DateTime DetectedAt { get; init; }
        public DateTime CreatedAt { get; init; }

        /// <summary>Asignar con DecisionSeverityResolver (FASE 10.5).</summary>
        public DecisionSeverity Severity { get; init; } = DecisionSeverity.Unspecified;

        /// <summary>Asignación = FASE 10.6. Independiente de Severity.</summary>
        public DecisionPriority Priority { get; init; } = DecisionPriority.Unspecified;

        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Reason { get; init; } = string.Empty;
        public string Impact { get; init; } = string.Empty;

        /// <summary>Lenguaje suave. Motor de recomendaciones = 10.19.</summary>
        public string Recommendation { get; init; } = string.Empty;

        public DecisionEventStatus Status { get; init; } = DecisionEventStatus.Active;

        /// <summary>Servicio/regla productora (SSOT 10.3).</summary>
        public string Source { get; init; } = string.Empty;

        /// <summary>
        /// Huella estable para deduplicar (TEST 8).
        /// Area|EventType|EntityType|EntityId|PeriodKey
        /// </summary>
        public string Fingerprint { get; init; } = string.Empty;

        public IReadOnlyList<DecisionEvidenceFact> Evidence { get; init; }
            = Array.Empty<DecisionEvidenceFact>();

        public IReadOnlyList<string> MetricKeys { get; init; }
            = Array.Empty<string>();
    }

    /// <summary>Descriptor de tipo de evento (catálogo 10.4). Sin evaluación.</summary>
    public sealed class DecisionEventTypeDescriptor
    {
        public string Code { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public DecisionEventArea Area { get; init; }
        public string Notes { get; init; } = string.Empty;
        /// <summary>Señal legacy mapeable (si existe).</summary>
        public string? LegacySignal { get; init; }
    }
}
