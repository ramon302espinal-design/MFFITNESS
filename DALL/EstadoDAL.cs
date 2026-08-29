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
                    d.FechaVencimiento AS VencimientoDeuda,
                    {MembresiaEstadoSql.ExpresionMontoPagadoMembresiaVigente} AS MontoPagado
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
                WHERE {MembresiaEstadoSql.FiltroSinVisitanteSistema}
            ) estado
            WHERE estado.Estado IN ('ACTIVO', 'ACTIVO Y PROGRAMADO', 'VENCIDO', 'DESACTIVADO', 'CONGELADO')
            ORDER BY estado.Nombre";

            return db.ExecuteQuery(query);
        }

        /// <summary>
        /// Conteos SSOT para dashboard: mismos estados que <see cref="ObtenerEstadoClientes"/>.
        /// Activos = ACTIVO + ACTIVO Y PROGRAMADO.
        /// </summary>
        public (int Activos, int Vencidos, int Congelados, int Desactivados) ObtenerConteosDashboard()
        {
            new CongelacionDAL().EnsureSchema();
            new MembresiaProgramadaDAL().EnsureSchema();

            string query = $@"
                SELECT
                    SUM(CASE WHEN e.Estado IN (N'ACTIVO', N'ACTIVO Y PROGRAMADO') THEN 1 ELSE 0 END) AS Activos,
                    SUM(CASE WHEN e.Estado = N'VENCIDO' THEN 1 ELSE 0 END) AS Vencidos,
                    SUM(CASE WHEN e.Estado = N'CONGELADO' THEN 1 ELSE 0 END) AS Congelados,
                    SUM(CASE WHEN e.Estado = N'DESACTIVADO' THEN 1 ELSE 0 END) AS Desactivados
                FROM (
                    SELECT {MembresiaEstadoSql.CasoEstado} AS Estado
                    FROM Clientes c
                    {MembresiaEstadoSql.OuterApplyUltimaMembresia}
                    WHERE {MembresiaEstadoSql.FiltroSinVisitanteSistema}
                ) e";

            DataTable dt = db.ExecuteQuery(query);
            if (dt.Rows.Count == 0)
                return (0, 0, 0, 0);

            DataRow row = dt.Rows[0];
            return (
                row["Activos"] == DBNull.Value ? 0 : Convert.ToInt32(row["Activos"]),
                row["Vencidos"] == DBNull.Value ? 0 : Convert.ToInt32(row["Vencidos"]),
                row["Congelados"] == DBNull.Value ? 0 : Convert.ToInt32(row["Congelados"]),
                row["Desactivados"] == DBNull.Value ? 0 : Convert.ToInt32(row["Desactivados"]));
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
                  AND UPPER(LTRIM(RTRIM(h.TipoMovimiento))) IN (" + MembresiaEstadoSql.TiposMovimientoCobroMembresiaIn + @")
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
                  AND UPPER(LTRIM(RTRIM(h.TipoMovimiento))) IN (" + MembresiaEstadoSql.TiposMovimientoCobroMembresiaIn + @")
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
