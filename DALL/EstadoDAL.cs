using System;
using System.Data;

namespace DL
{
    public class EstadoDAL
    {
        private readonly DBHelper db = new DBHelper();

        public DataTable ObtenerEstadoClientes()
        {
            new CongelacionDAL().EnsureSchema();

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
            WHERE estado.Estado IN ('ACTIVO', 'VENCIDO', 'DESACTIVADO', 'CONGELADO')
            ORDER BY estado.Nombre";

            return db.ExecuteQuery(query);
        }
    }
}
