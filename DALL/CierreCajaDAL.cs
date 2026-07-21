using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace DL
{
    public class CierreCajaDAL
    {
        private readonly DBHelper db = new DBHelper();

        public decimal ObtenerTotalPorCaja(int cajaId, string tipo)
        {
            string query = @"SELECT ISNULL(SUM(Monto),0) FROM DetalleCaja 
                             WHERE CajaId = @CajaId AND TipoMovimiento = @Tipo";

            SqlParameter[] p = {
                new SqlParameter("@CajaId", cajaId),
                new SqlParameter("@Tipo", tipo)
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

        public DataTable ObtenerHistorialCierres()
        {
            string query = @"
                SELECT
                    Id,
                    Fecha,
                    Turno,
                    TotalIngresos,
                    TotalGastos,
                    TotalSistema,
                    TotalContado,
                    Diferencia,
                    FechaCierre,
                    CajaId,
                    Usuario
                FROM CierreCaja
                ORDER BY FechaCierre DESC, Id DESC";
            return db.ExecuteQuery(query);
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
            string query = @"SELECT ISNULL(SUM(Monto),0) FROM DetalleCaja
                             WHERE TipoMovimiento = @TipoMovimiento
                             AND CAST(Fecha AS DATE) = CAST(GETDATE() AS DATE)";
            SqlParameter[] p = { new SqlParameter("@TipoMovimiento", tipoMovimiento) };
            return Convert.ToDecimal(db.ExecuteScalar(query, p));
        }
    }
}
