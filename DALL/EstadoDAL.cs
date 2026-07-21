using System;
using System.Data;

namespace DL
{
    public class EstadoDAL
    {
        private readonly DBHelper db = new DBHelper();

        public DataTable ObtenerEstadoClientes()
        {
            // Activos, vencidos y desactivados (misma regla que el dashboard).
            // Clientes recién agregados (sin compra) no aparecen hasta pagar un plan.
            string query = $@"
            SELECT *
            FROM (
                SELECT 
                    c.ID,
                    c.Nombre,
                    ISNULL(p.Nombre, 'SIN MEMBRESIA') AS Membresia,
                    m.FechaInicio,
                    m.FechaFin,
                    {MembresiaEstadoSql.CasoEstado} AS Estado,
                    ISNULL(d.Estado, 'N/A') AS EstadoDeuda,
                    ISNULL(d.Saldo, 0) AS SaldoPendiente,
                    ISNULL(d.MontoTotal, 0) AS MontoFinanciado,
                    d.FechaVencimiento AS VencimientoDeuda
                FROM Clientes c
                {MembresiaEstadoSql.OuterApplyUltimaMembresia}
                LEFT JOIN Planes p ON p.Id = m.PlanId
                LEFT JOIN (
                    SELECT 
                        ClienteId,
                        Estado,
                        SUM(Saldo) AS Saldo,
                        SUM(MontoTotal) AS MontoTotal,
                        MAX(FechaVencimiento) AS FechaVencimiento
                    FROM Deudas
                    WHERE Estado = 'ACTIVA' AND MembresiaId IS NOT NULL
                    GROUP BY ClienteId, Estado
                ) d ON d.ClienteId = c.ID
            ) estado
            WHERE estado.Estado IN ('ACTIVO', 'VENCIDO', 'DESACTIVADO')
            ORDER BY estado.Nombre";

            return db.ExecuteQuery(query);
        }
    }
}
