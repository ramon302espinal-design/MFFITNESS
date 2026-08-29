using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace DL
{
    public class EstadoDAL
    {
        private readonly DBHelper db = new DBHelper();

        public DataTable ObtenerEstadoClientes()
        {
            new CongelacionDAL().EnsureSchema();
            new MembresiaProgramadaDAL().EnsureSchema();

            // Activos, vencidos, desactivados y congelados (misma regla que el dashboard).
            // Clientes recién agregados (sin compra) no aparecen hasta pagar un plan.
            string query = $@"
            SELECT *
            FROM (
                SELECT 
                    c.ID,
                    c.Nombre,
                    ISNULL(
                        CASE
                            -- Desactivado: no reflejar plan aunque quede membresía histórica en BD.
                            WHEN m.Id IS NULL THEN NULL
                            WHEN {MembresiaEstadoSql.ExpresionUltimaSalida} THEN NULL
                            -- Cualquier plan registrado en Planes (no se listan nombres a mano:
                            -- un plan nuevo como M-A debe reflejarse sin tocar código).
                            ELSE NULLIF(LTRIM(RTRIM(ISNULL(p.Nombre, ''))), '')
                        END,
                        'SIN MEMBRESIA') AS Membresia,
                    m.FechaInicio,
                    m.FechaFin,
                    {MembresiaEstadoSql.CasoEstado} AS Estado,
                    CASE WHEN d.ClienteId IS NULL THEN 'N/A' ELSE 'ACTIVA' END AS EstadoDeuda,
                    ISNULL(d.Saldo, 0) AS SaldoPendiente,
                    ISNULL(d.MontoFinanciado, 0) AS MontoFinanciado,
                    d.FechaVencimiento AS VencimientoDeuda
                FROM Clientes c
                {MembresiaEstadoSql.OuterApplyUltimaMembresia}
                LEFT JOIN Planes p ON p.Id = m.PlanId
                LEFT JOIN (
                    -- Saldo real del cliente: incluye plan financiado y producto a crédito.
                    -- MontoFinanciado sigue siendo solo del plan (no se mezcla con ventas).
                    SELECT 
                        ClienteId,
                        SUM(Saldo) AS Saldo,
                        SUM(CASE WHEN MembresiaId IS NOT NULL THEN MontoTotal ELSE 0 END) AS MontoFinanciado,
                        MAX(FechaVencimiento) AS FechaVencimiento
                    FROM Deudas
                    WHERE Estado = 'ACTIVA' AND Saldo > 0
                    GROUP BY ClienteId
                ) d ON d.ClienteId = c.ID
            ) estado
            WHERE estado.Estado IN ('ACTIVO', 'ACTIVO Y PROGRAMADO', 'VENCIDO', 'DESACTIVADO', 'CONGELADO')
            ORDER BY estado.Nombre";

            return db.ExecuteQuery(query);
        }

        /// <summary>
        /// Ingresos por plan en un mes calendario (HistorialMembresias).
        /// Cantidad = movimientos de cobro/alta; MontoTotal = Σ Monto registrado.
        /// </summary>
        public DataTable ObtenerKpisPlanesPorMes(int anio, int mes)
        {
            DateTime desde = new DateTime(anio, mes, 1);
            DateTime hasta = desde.AddMonths(1);

            string query = @"
                SELECT
                    ISNULL(NULLIF(LTRIM(RTRIM(p.Nombre)), ''), N'SIN PLAN') AS PlanNombre,
                    COUNT(*) AS Cantidad,
                    SUM(ISNULL(h.Monto, 0)) AS MontoTotal
                FROM dbo.HistorialMembresias h
                INNER JOIN dbo.Clientes c ON c.ID = h.ClienteId
                LEFT JOIN dbo.Planes p ON p.Id = h.PlanId
                WHERE h.Fecha >= @Desde
                  AND h.Fecha < @Hasta
                  AND c.Nombre <> N'VISITANTE (SISTEMA)'
                  AND UPPER(LTRIM(RTRIM(h.TipoMovimiento))) IN (
                        N'PAGO', N'RENOVACION', N'ALTA_EXISTENTE', N'ALTA',
                        N'ATLETA', N'VISITA', N'PARCIAL', N'PROGRAMACION')
                GROUP BY p.Nombre";

            SqlParameter[] parametros =
            {
                new SqlParameter("@Desde", desde),
                new SqlParameter("@Hasta", hasta)
            };

            return db.ExecuteQuery(query, parametros);
        }

        /// <summary>Detalle de cobros/altas del mes para reporte PDF.</summary>
        public DataTable ObtenerDetalleMembresiasPorMes(int anio, int mes)
        {
            DateTime desde = new DateTime(anio, mes, 1);
            DateTime hasta = desde.AddMonths(1);

            string query = @"
                SELECT
                    h.Fecha,
                    c.Nombre AS Cliente,
                    ISNULL(NULLIF(LTRIM(RTRIM(p.Nombre)), ''), N'Sin plan') AS Plan,
                    h.TipoMovimiento AS Movimiento,
                    ISNULL(h.Monto, 0) AS Monto
                FROM dbo.HistorialMembresias h
                INNER JOIN dbo.Clientes c ON c.ID = h.ClienteId
                LEFT JOIN dbo.Planes p ON p.Id = h.PlanId
                WHERE h.Fecha >= @Desde
                  AND h.Fecha < @Hasta
                  AND c.Nombre <> N'VISITANTE (SISTEMA)'
                  AND UPPER(LTRIM(RTRIM(h.TipoMovimiento))) IN (
                        N'PAGO', N'RENOVACION', N'ALTA_EXISTENTE', N'ALTA',
                        N'ATLETA', N'VISITA', N'PARCIAL', N'PROGRAMACION')
                ORDER BY h.Fecha DESC, c.Nombre";

            SqlParameter[] parametros =
            {
                new SqlParameter("@Desde", desde),
                new SqlParameter("@Hasta", hasta)
            };

            return db.ExecuteQuery(query, parametros);
        }
    }
}
