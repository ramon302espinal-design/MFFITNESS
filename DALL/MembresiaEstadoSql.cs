namespace DL
{
    /// <summary>
    /// Criterio único de estado de membresía (grid Estado + dashboard + validaciones).
    /// Vigencia inclusive hasta el día de FechaFin (p. ej. vence el 15 → activo todo el día 15).
    /// Último historial SALIDA → DESACTIVADO; BAJA_VENCIDO → VENCIDO; si no, rige FechaFin.
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

        /// <summary>Último movimiento por Id (evita empates de Fecha en el mismo segundo).</summary>
        public const string ExpresionUltimaSalida = @"
                        EXISTS (
                            SELECT 1
                            FROM HistorialMembresias h
                            INNER JOIN (
                                SELECT ClienteId, MAX(Id) AS UltimoId
                                FROM HistorialMembresias
                                GROUP BY ClienteId
                            ) ult ON ult.ClienteId = h.ClienteId AND ult.UltimoId = h.Id
                            WHERE h.ClienteId = c.ID
                              AND h.TipoMovimiento = 'SALIDA'
                        )";

        public const string ExpresionUltimaBajaVencido = @"
                        EXISTS (
                            SELECT 1
                            FROM HistorialMembresias h
                            INNER JOIN (
                                SELECT ClienteId, MAX(Id) AS UltimoId
                                FROM HistorialMembresias
                                GROUP BY ClienteId
                            ) ult ON ult.ClienteId = h.ClienteId AND ult.UltimoId = h.Id
                            WHERE h.ClienteId = c.ID
                              AND h.TipoMovimiento = 'BAJA_VENCIDO'
                        )";

        public const string ExpresionCongelado = @"
                        EXISTS (
                            SELECT 1
                            FROM CongelacionesMembresia g
                            WHERE g.ClienteId = c.ID
                              AND g.Activa = 1
                        )";

        /// <summary>
        /// CASE Estado (alias c = Clientes, m = última membresía).
        /// SALIDA → DESACTIVADO; congelación activa → CONGELADO; si no, rige FechaFin.
        /// </summary>
        public const string CasoEstado = @"
                    CASE
                        WHEN m.Id IS NULL THEN 'SIN MEMBRESIA'
                        WHEN " + ExpresionUltimaSalida + @" THEN 'DESACTIVADO'
                        WHEN " + ExpresionCongelado + @" THEN 'CONGELADO'
                        WHEN " + ExpresionUltimaBajaVencido + @" THEN 'VENCIDO'
                        WHEN m.FechaFin IS NULL THEN 'SIN MEMBRESIA'
                        WHEN CAST(m.FechaFin AS DATE) >= CAST(GETDATE() AS DATE) THEN 'ACTIVO'
                        WHEN CAST(m.FechaFin AS DATE) < CAST(GETDATE() AS DATE) THEN 'VENCIDO'
                        ELSE 'SIN MEMBRESIA'
                    END";

        public const string PredicadoActivo = @"
                m.Id IS NOT NULL
                AND m.FechaFin IS NOT NULL
                AND NOT (" + ExpresionUltimaSalida + @")
                AND NOT (" + ExpresionCongelado + @")
                AND NOT (" + ExpresionUltimaBajaVencido + @")
                AND CAST(m.FechaFin AS DATE) >= CAST(GETDATE() AS DATE)";

        public const string PredicadoVencido = @"
                (" + CasoEstado + @") = 'VENCIDO'";
    }
}
