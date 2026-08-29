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

        public const string ExpresionProgramacionPendiente = @"
                        EXISTS (
                            SELECT 1
                            FROM MembresiasProgramadas mp
                            WHERE mp.ClienteId = c.ID
                              AND mp.Estado = N'PENDIENTE'
                        )";

        /// <summary>Para consultas sobre Membresias (alias m): excluir quien ya pagó por anticipado.</summary>
        public const string FiltroMembresiaSinProgramacionPendiente = @"
                AND NOT EXISTS (
                    SELECT 1
                    FROM MembresiasProgramadas mp
                    WHERE mp.ClienteId = m.ClienteId
                      AND mp.Estado = N'PENDIENTE'
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
                        WHEN CAST(m.FechaFin AS DATE) >= CAST(GETDATE() AS DATE)
                             AND " + ExpresionProgramacionPendiente + @" THEN 'ACTIVO Y PROGRAMADO'
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

        /// <summary>
        /// Vigentes para dashboard / KPI: ACTIVO + ACTIVO Y PROGRAMADO (misma etiqueta que el grid).
        /// </summary>
        public const string PredicadoActivoVigente = @"
                (" + CasoEstado + @") IN (N'ACTIVO', N'ACTIVO Y PROGRAMADO')";

        /// <summary>Excluye el cliente técnico del POS (no es miembro real).</summary>
        public const string FiltroSinVisitanteSistema = @"
                c.Nombre <> N'VISITANTE (SISTEMA)'";

        /// <summary>Tipos de HistorialMembresias que representan cobro/alta (SSOT con KPI mes).</summary>
        public const string TiposMovimientoCobroMembresiaIn = @"
                N'PAGO', N'RENOVACION', N'ALTA_EXISTENTE', N'ALTA',
                N'PROGRAMACION', N'ATLETA', N'VISITA', N'PARCIAL'";

        /// <summary>
        /// Monto cobrado en el ciclo de la última membresía (alias c, m).
        /// HistorialMembresias del periodo + abonos Deudas ligadas a MembresiaId.
        /// CAST a DATETIME2: FechaInicio puede ser DATE y DATEADD(minute) no aplica a DATE.
        /// </summary>
        public const string ExpresionMontoPagadoMembresiaVigente = @"
                    ISNULL((
                        SELECT SUM(ISNULL(h.Monto, 0))
                        FROM HistorialMembresias h
                        WHERE h.ClienteId = c.ID
                          AND m.Id IS NOT NULL
                          AND m.FechaInicio IS NOT NULL
                          AND h.Fecha >= DATEADD(MINUTE, -30, CAST(m.FechaInicio AS DATETIME2))
                          AND (m.FechaFin IS NULL OR h.Fecha <= DATEADD(DAY, 1, CAST(m.FechaFin AS DATE)))
                          AND UPPER(LTRIM(RTRIM(h.TipoMovimiento))) IN (" + TiposMovimientoCobroMembresiaIn + @")
                    ), 0)
                    + ISNULL((
                        SELECT SUM(ISNULL(de.MontoPagado, 0))
                        FROM Deudas de
                        WHERE de.ClienteId = c.ID
                          AND de.MembresiaId = m.Id
                    ), 0)";
    }
}
