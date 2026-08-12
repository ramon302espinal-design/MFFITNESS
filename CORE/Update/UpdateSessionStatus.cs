namespace CORE.Update
{
    /// <summary>
    /// Estado terminal/no-terminal de una sesión de actualización.
    /// </summary>
    public enum UpdateSessionStatus
    {
        /// <summary>Sesión en curso (no terminal).</summary>
        Active = 0,

        /// <summary>Actualización completada con App+DB+Health verificados.</summary>
        Completed = 1,

        /// <summary>Bloqueada por pre-gates (caja, package, etc.). Sin side effects destructivos.</summary>
        Blocked = 2,

        /// <summary>Falló sin recovery (o antes de side effects).</summary>
        Failed = 3,

        /// <summary>Falló; recovery automático restauró App vieja + DB vieja.</summary>
        FailedRecovered = 4,

        /// <summary>Falló; recovery automático incompleto. Requiere intervención.</summary>
        FailedRecoveryRequired = 5,

        /// <summary>Estado ambiguo o start UI falló post-health; no Completed hasta confirmar.</summary>
        RecoveryRequired = 6
    }
}
