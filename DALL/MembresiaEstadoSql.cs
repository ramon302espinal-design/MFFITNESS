namespace DL
{
    /// <summary>
    /// Criterio único de estado de membresía (grid Estado + dashboard + validaciones).
    /// Vigencia inclusive hasta el día de FechaFin (p. ej. vence el 15 → activo todo el día 15).
    /// Si FechaFin es futura, NUNCA es VENCIDO (salvo salida → DESACTIVADO).
    /// </summary>
    internal static class MembresiaEstadoSql
    {
        public const string OuterApplyUltimaMembresia = @"
                OUTER APPLY (
                    SELECT TOP 1 *
                    FROM Membresias mx
                    WHERE mx.ClienteId = c.ID
                    ORDER BY mx.FechaFin DESC, mx.Id DESC
                ) m";

        public const string ExpresionUltimaSalida = @"
                        EXISTS (
                            SELECT 1
                            FROM HistorialMembresias h
                            INNER JOIN (
                                SELECT ClienteId, MAX(Fecha) AS UltimaFecha
                                FROM HistorialMembresias
                                GROUP BY ClienteId
                            ) ult ON ult.ClienteId = h.ClienteId AND ult.UltimaFecha = h.Fecha
                            WHERE h.ClienteId = c.ID
                              AND h.TipoMovimiento = 'SALIDA'
                        )";

        public const string ExpresionUltimaBajaVencido = @"
                        EXISTS (
                            SELECT 1
                            FROM HistorialMembresias h
                            INNER JOIN (
                                SELECT ClienteId, MAX(Fecha) AS UltimaFecha
                                FROM HistorialMembresias
                                GROUP BY ClienteId
                            ) ult ON ult.ClienteId = h.ClienteId AND ult.UltimaFecha = h.Fecha
                            WHERE h.ClienteId = c.ID
                              AND h.TipoMovimiento = 'BAJA_VENCIDO'
                        )";

        /// <summary>
        /// CASE Estado (alias c = Clientes, m = última membresía).
        /// La fecha de vencimiento manda sobre historial BAJA_VENCIDO residual.
        /// </summary>
        public const string CasoEstado = @"
                    CASE
                        WHEN m.Id IS NULL THEN 'SIN MEMBRESIA'
                        WHEN " + ExpresionUltimaSalida + @" THEN 'DESACTIVADO'
                        WHEN m.FechaFin IS NULL THEN 'SIN MEMBRESIA'
                        WHEN CAST(m.FechaFin AS DATE) > CAST(GETDATE() AS DATE) THEN 'ACTIVO'
                        WHEN CAST(m.FechaFin AS DATE) = CAST(GETDATE() AS DATE)
                             AND NOT (" + ExpresionUltimaBajaVencido + @")
                            THEN 'ACTIVO'
                        WHEN CAST(m.FechaFin AS DATE) = CAST(GETDATE() AS DATE)
                             AND (" + ExpresionUltimaBajaVencido + @")
                            THEN 'VENCIDO'
                        WHEN CAST(m.FechaFin AS DATE) < CAST(GETDATE() AS DATE) THEN 'VENCIDO'
                        ELSE 'SIN MEMBRESIA'
                    END";

        public const string PredicadoActivo = @"
                m.Id IS NOT NULL
                AND m.FechaFin IS NOT NULL
                AND NOT (" + ExpresionUltimaSalida + @")
                AND (
                    CAST(m.FechaFin AS DATE) > CAST(GETDATE() AS DATE)
                    OR (
                        CAST(m.FechaFin AS DATE) = CAST(GETDATE() AS DATE)
                        AND NOT (" + ExpresionUltimaBajaVencido + @")
                    )
                )";

        public const string PredicadoVencido = @"
                (" + CasoEstado + @") = 'VENCIDO'";
    }
}
