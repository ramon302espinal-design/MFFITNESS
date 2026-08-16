using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace DL
{
    public class CierreCajaDAL
    {
        private readonly DBHelper db = new DBHelper();

        /// <summary>
        /// Totales de la caja abierta alineados con los paneles:
        /// INGRESO = ingresos vigentes (sin los ya reversados);
        /// EGRESO = solo gastos operativos (sin movimientos de reverso).
        /// </summary>
        public decimal ObtenerTotalPorCaja(int cajaId, string tipo)
        {
            string tipoNorm = (tipo ?? string.Empty).Trim().ToUpperInvariant();

            string query = tipoNorm == "INGRESO"
                ? @"
            SELECT ISNULL(SUM(dc.Monto), 0)
            FROM DetalleCaja dc
            WHERE dc.CajaId = @CajaId
              AND dc.TipoMovimiento = 'INGRESO'
              AND dc.Concepto NOT LIKE 'REVERSO (Ref #%'
              AND NOT EXISTS (
                  SELECT 1
                  FROM DetalleCaja rev
                  WHERE rev.CajaId = dc.CajaId
                    AND rev.TipoMovimiento = 'EGRESO'
                    AND rev.Concepto LIKE CONCAT('REVERSO (Ref #', dc.Id, '):%')
              )"
                : @"
            SELECT ISNULL(SUM(dc.Monto), 0)
            FROM DetalleCaja dc
            WHERE dc.CajaId = @CajaId
              AND dc.TipoMovimiento = 'EGRESO'
              AND ISNULL(dc.MetodoPago, '') <> 'REVERSO'
              AND dc.Concepto NOT LIKE 'REVERSO%'
              AND dc.Concepto NOT LIKE 'Reverso%'";

            SqlParameter[] p =
            {
                new SqlParameter("@CajaId", cajaId)
            };

            return Convert.ToDecimal(db.ExecuteScalar(query, p));
        }

        public void RegistrarCierreCompleto(
            int cajaId,
            string turno,
            decimal ingresos,
            decimal gastos,
            decimal totalSistema,
            decimal totalContado,
            decimal diferencia,
            string usuario)
        {
            string query = @"INSERT INTO CierreCaja
                (Fecha, Turno, TotalIngresos, TotalGastos, TotalSistema, TotalContado, Diferencia, FechaCierre, CajaId, Usuario)
                VALUES
                (CAST(GETDATE() AS DATE), @Turno, @Ingresos, @Gastos, @TotalSistema, @TotalContado, @Diferencia, GETDATE(), @CajaId, @Usuario)";

            SqlParameter[] parametros = {
                new SqlParameter("@Turno", turno),
                new SqlParameter("@Ingresos", ingresos),
                new SqlParameter("@Gastos", gastos),
                new SqlParameter("@TotalSistema", totalSistema),
                new SqlParameter("@TotalContado", totalContado),
                new SqlParameter("@Diferencia", diferencia),
                new SqlParameter("@CajaId", cajaId),
                new SqlParameter("@Usuario", usuario)
            };

            db.ExecuteNonQuery(query, parametros);
        }

        public void EliminarCierre(int id)
        {
            string query = "DELETE FROM CierreCaja WHERE Id = @Id";
            SqlParameter[] p = { new SqlParameter("@Id", id) };
            db.ExecuteNonQuery(query, p);
        }

        public DataTable ObtenerHistorialCierres(bool verTodos, string usuario)
        {
            string query = @"
                SELECT
                    cc.Id,
                    cc.Fecha,
                    cc.Turno,
                    ISNULL(c.MontoInicial, 0) AS MontoInicial,
                    cc.TotalIngresos,
                    cc.TotalGastos,
                    cc.TotalSistema,
                    cc.TotalContado,
                    cc.Diferencia,
                    cc.FechaCierre,
                    cc.CajaId,
                    cc.Usuario
                FROM CierreCaja cc
                LEFT JOIN Caja c ON c.Id = cc.CajaId
                WHERE @VerTodos = 1
                   OR UPPER(LTRIM(RTRIM(cc.Usuario))) = UPPER(LTRIM(RTRIM(@Usuario)))
                ORDER BY cc.FechaCierre DESC, cc.Id DESC";

            SqlParameter[] parametros =
            {
                new SqlParameter("@VerTodos", SqlDbType.Bit) { Value = verTodos },
                new SqlParameter("@Usuario", SqlDbType.NVarChar, 100)
                {
                    Value = usuario?.Trim() ?? string.Empty
                }
            };

            return db.ExecuteQuery(query, parametros);
        }

        public bool YaExisteCierreHoy(string turno)
        {
            string query = @"SELECT COUNT(*) FROM CierreCaja 
                             WHERE Fecha = CAST(GETDATE() AS DATE) AND Turno = @Turno";
            SqlParameter[] p = { new SqlParameter("@Turno", turno) };
            return Convert.ToInt32(db.ExecuteScalar(query, p)) > 0;
        }

        public decimal ObtenerTotalPorTipoMovimientoHoy(string tipoMovimiento)
        {
            string tipoNorm = (tipoMovimiento ?? string.Empty).Trim().ToUpperInvariant();

            string query = tipoNorm == "INGRESO"
                ? @"
            SELECT ISNULL(SUM(dc.Monto), 0)
            FROM DetalleCaja dc
            WHERE dc.TipoMovimiento = 'INGRESO'
              AND CAST(dc.Fecha AS DATE) = CAST(GETDATE() AS DATE)
              AND dc.Concepto NOT LIKE 'REVERSO (Ref #%'
              AND NOT EXISTS (
                  SELECT 1
                  FROM DetalleCaja rev
                  WHERE rev.CajaId = dc.CajaId
                    AND rev.TipoMovimiento = 'EGRESO'
                    AND rev.Concepto LIKE CONCAT('REVERSO (Ref #', dc.Id, '):%')
              )"
                : @"
            SELECT ISNULL(SUM(dc.Monto), 0)
            FROM DetalleCaja dc
            WHERE dc.TipoMovimiento = 'EGRESO'
              AND CAST(dc.Fecha AS DATE) = CAST(GETDATE() AS DATE)
              AND ISNULL(dc.MetodoPago, '') <> 'REVERSO'
              AND dc.Concepto NOT LIKE 'REVERSO%'
              AND dc.Concepto NOT LIKE 'Reverso%'";

            return Convert.ToDecimal(db.ExecuteScalar(query));
        }
    }
}
