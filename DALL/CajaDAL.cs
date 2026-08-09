using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace DL
{
    public class CajaDAL
    {
        private DBHelper db = new DBHelper();

        public DataRow? ObtenerCajaAbierta()
        {
            string query = @"
            SELECT TOP 1 * 
            FROM Caja 
            WHERE Estado = 'ABIERTA'
            ORDER BY Id DESC";

            DataTable dt = db.ExecuteQuery(query);

            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }
        public DataRow? ObtenerCajaAbierta(SqlConnection conn, SqlTransaction tx)
        {
                    string query = @"
            SELECT TOP 1 * 
            FROM Caja 
            WHERE Estado = 'ABIERTA'
            ORDER BY Id DESC";

            using (SqlCommand cmd = new SqlCommand(query, conn, tx))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt.Rows.Count > 0 ? dt.Rows[0] : null;
            }
        }
        // ===============================
        // ABRIR CAJA
        // ===============================
        public void AbrirCaja(decimal montoInicial, string usuario)
        {
            string query = @"INSERT INTO Caja (FechaApertura, MontoInicial, Usuario, Estado)
                             VALUES (GETDATE(), @MontoInicial, @Usuario, 'ABIERTA')";

            SqlParameter[] parametros =
            {
                new SqlParameter("@MontoInicial", montoInicial),
                new SqlParameter("@Usuario", usuario)
            };

            db.ExecuteNonQuery(query, parametros);
        }

        // ===============================
        // INSERTAR MOVIMIENTO (CORRECTO)
        // ===============================
        public int InsertarMovimiento(int cajaId, string tipoMovimiento, string concepto, decimal monto, string usuario)
        {
            if (cajaId <= 0)
                throw new Exception("Caja inválida");

            if (string.IsNullOrWhiteSpace(tipoMovimiento))
                throw new Exception("Tipo de movimiento requerido");

            if (string.IsNullOrWhiteSpace(concepto))
                throw new Exception("Concepto requerido");

            if (monto <= 0)
                throw new Exception("Monto inválido");

            if (string.IsNullOrWhiteSpace(usuario))
                throw new Exception("Usuario requerido");

            string query = @"
            INSERT INTO DetalleCaja 
            (CajaId, TipoMovimiento, Concepto, Monto, Fecha, Usuario)
            OUTPUT INSERTED.Id
            VALUES 
            (@CajaId, @TipoMovimiento, @Concepto, @Monto, GETDATE(), @Usuario)";

            SqlParameter[] parametros =
            {
                new SqlParameter("@CajaId", cajaId),
                new SqlParameter("@TipoMovimiento", tipoMovimiento.ToUpper().Trim()),
                new SqlParameter("@Concepto", concepto.Trim()),
                new SqlParameter("@Monto", monto),
                new SqlParameter("@Usuario", usuario.Trim())
            };

            object? result = db.ExecuteScalar(query, parametros);
            return Convert.ToInt32(result);
        }

        public int InsertarMovimientoConId(SqlConnection conn, SqlTransaction tx,
             int cajaId, string tipoMovimiento, string concepto, decimal monto, string usuario)
        {
            if (cajaId <= 0)
                throw new Exception("Caja inválida");

            if (string.IsNullOrWhiteSpace(tipoMovimiento))
                throw new Exception("Tipo de movimiento requerido");

            if (string.IsNullOrWhiteSpace(concepto))
                throw new Exception("Concepto requerido");

            if (monto < 0)
                throw new Exception("Monto inválido");

            if (string.IsNullOrWhiteSpace(usuario))
                throw new Exception("Usuario requerido");

            string query = @"
                INSERT INTO DetalleCaja 
                (CajaId, TipoMovimiento, Concepto, Monto, Fecha, Usuario)
                OUTPUT INSERTED.Id
                VALUES 
                (@CajaId, @TipoMovimiento, @Concepto, @Monto, GETDATE(), @Usuario)";

            using (SqlCommand cmd = new SqlCommand(query, conn, tx))
            {
                cmd.Parameters.AddWithValue("@CajaId", cajaId);
                cmd.Parameters.AddWithValue("@TipoMovimiento", tipoMovimiento.ToUpper().Trim());
                cmd.Parameters.AddWithValue("@Concepto", concepto.Trim());
                cmd.Parameters.AddWithValue("@Monto", monto);
                cmd.Parameters.AddWithValue("@Usuario", usuario.Trim());

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public int ObtenerUltimoMovimientoIdPorConcepto(int cajaId, string concepto)
        {
            string query = @"
                SELECT TOP 1 Id
                FROM DetalleCaja
                WHERE CajaId = @CajaId
                  AND Concepto = @Concepto
                ORDER BY Id DESC";

            SqlParameter[] p =
            {
                new SqlParameter("@CajaId", cajaId),
                new SqlParameter("@Concepto", concepto)
            };

            object? result = db.ExecuteScalar(query, p);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        // ===============================
        // OBTENER MOVIMIENTOS
        // ===============================
        public DataTable ObtenerMovimientos(int cajaId)
        {
            string query = @"SELECT 
                                dc.Id, 
                                dc.TipoMovimiento, 
                                dc.Concepto, 
                                dc.Monto, 
                                dc.Fecha, 
                                dc.Usuario,
                                COALESCE(p.ClienteId, v.ClienteId, d.ClienteId) AS ClienteId,
                                COALESCE(c1.Nombre, c2.Nombre, c3.Nombre) AS NombreCliente
                             FROM DetalleCaja dc
                             LEFT JOIN Pagos p ON (dc.Concepto LIKE '%Pago membresía%' OR dc.Concepto LIKE '%Renovación%')
                                    AND CAST(dc.Fecha AS DATE) = CAST(p.FechaPago AS DATE)
                                    AND ABS(dc.Monto - p.Monto) < 0.01
                             LEFT JOIN Clientes c1 ON c1.Id = p.ClienteId
                             LEFT JOIN Ventas v ON dc.Concepto LIKE '%Venta%'
                                    AND CAST(dc.Fecha AS DATE) = CAST(v.Fecha AS DATE)
                                    AND ABS(dc.Monto - v.Total) < 0.01
                             LEFT JOIN Clientes c2 ON c2.Id = v.ClienteId
                             LEFT JOIN Deudas d ON dc.Concepto LIKE '%Abono deuda%'
                             LEFT JOIN Clientes c3 ON c3.Id = d.ClienteId
                             WHERE dc.CajaId = @CajaId
                             ORDER BY dc.Fecha DESC";

            SqlParameter[] parametros =
            {
                new SqlParameter("@CajaId", cajaId)
            };

            return db.ExecuteQuery(query, parametros);
        }

        // ===============================
        // REVERTIR MOVIMIENTO MANUAL
        // ===============================
        public void RevertirMovimiento(int movimientoId, string usuario)
        {
            using SqlConnection conn = new SqlConnection(db.ConnectionString);
            conn.Open();

            using SqlTransaction tx = conn.BeginTransaction();
            try
            {
                RevertirMovimientoEnTransaccion(conn, tx, movimientoId, usuario);
                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public void RevertirMovimientoEnTransaccion(SqlConnection conn, SqlTransaction tx, int movimientoId, string usuario)
        {
            SqlCommand cmdGet = new SqlCommand(@"
                    SELECT dc.CajaId, dc.TipoMovimiento, dc.Concepto, dc.Monto, c.Estado AS EstadoCaja
                    FROM DetalleCaja dc
                    INNER JOIN Caja c ON c.Id = dc.CajaId
                    WHERE dc.Id = @MovimientoId", conn, tx);

            cmdGet.Parameters.Add("@MovimientoId", SqlDbType.Int).Value = movimientoId;

            int cajaId;
            string tipoOriginal;
            string conceptoOriginal;
            decimal monto;
            string estadoCaja;

            using (var reader = cmdGet.ExecuteReader())
            {
                if (!reader.Read())
                    throw new Exception("Movimiento no encontrado.");

                cajaId = Convert.ToInt32(reader["CajaId"]);
                tipoOriginal = reader["TipoMovimiento"]?.ToString()?.ToUpperInvariant() ?? "";
                conceptoOriginal = reader["Concepto"]?.ToString() ?? "";
                monto = Convert.ToDecimal(reader["Monto"]);
                estadoCaja = reader["EstadoCaja"]?.ToString()?.ToUpperInvariant() ?? "";
            }

            if (estadoCaja != "ABIERTA")
                throw new Exception("No se puede deshacer: la caja está cerrada.");

            if (conceptoOriginal.StartsWith("REVERSO (Ref #", StringComparison.OrdinalIgnoreCase))
                throw new Exception("No se puede deshacer un movimiento de reversión.");

            if (tipoOriginal != "INGRESO" && tipoOriginal != "EGRESO")
                throw new Exception("Solo se pueden deshacer ingresos o egresos manuales.");

            string marcaReverso = $"REVERSO (Ref #{movimientoId}):";
            SqlCommand cmdExists = new SqlCommand(@"
                    SELECT COUNT(*)
                    FROM DetalleCaja
                    WHERE CajaId = @CajaId
                      AND Concepto LIKE @MarcaReverso", conn, tx);

            cmdExists.Parameters.Add("@CajaId", SqlDbType.Int).Value = cajaId;
            cmdExists.Parameters.Add("@MarcaReverso", SqlDbType.NVarChar, 200).Value = marcaReverso + "%";

            int yaRevertido = Convert.ToInt32(cmdExists.ExecuteScalar());
            if (yaRevertido > 0)
                throw new Exception("Este movimiento ya fue deshecho.");

            string tipoInverso = tipoOriginal == "INGRESO" ? "EGRESO" : "INGRESO";
            string conceptoReverso = $"{marcaReverso} {conceptoOriginal}";

            SqlCommand cmdInsert = new SqlCommand(@"
                    INSERT INTO DetalleCaja
                    (CajaId, TipoMovimiento, Concepto, Monto, Fecha, Usuario)
                    VALUES
                    (@CajaId, @TipoMovimiento, @Concepto, @Monto, GETDATE(), @Usuario)", conn, tx);

            cmdInsert.Parameters.Add("@CajaId", SqlDbType.Int).Value = cajaId;
            cmdInsert.Parameters.Add("@TipoMovimiento", SqlDbType.NVarChar, 50).Value = tipoInverso;
            cmdInsert.Parameters.Add("@Concepto", SqlDbType.NVarChar, 200).Value = conceptoReverso;
            cmdInsert.Parameters.Add("@Monto", SqlDbType.Decimal).Value = monto;
            cmdInsert.Parameters.Add("@Usuario", SqlDbType.NVarChar, 100).Value = usuario;
            cmdInsert.ExecuteNonQuery();
        }

        // ===============================
        // CERRAR CAJA
        // ===============================
        public void CerrarCaja(int cajaId)
        {
            string query = @"UPDATE Caja
                             SET Estado = 'CERRADA'
                             WHERE Id = @CajaId
                               AND Estado = 'ABIERTA'";

            SqlParameter[] parametros =
            {
                new SqlParameter("@CajaId", cajaId)
            };

            int afectados = db.ExecuteNonQuery(query, parametros);
            if (afectados <= 0)
                throw new Exception("No se pudo cerrar la caja (ya estaba cerrada o no existe).");
        }

        /// <summary>
        /// Inserta el cuadre en CierreCaja y marca la caja como CERRADA en la misma transacción.
        /// </summary>
        public void RegistrarCierreYCerrarCaja(
            int cajaId,
            string turno,
            decimal ingresos,
            decimal gastos,
            decimal totalSistema,
            decimal totalContado,
            decimal diferencia,
            string usuario)
        {
            using SqlConnection conn = new SqlConnection(db.ConnectionString);
            conn.Open();
            using SqlTransaction tx = conn.BeginTransaction();

            try
            {
                using (SqlCommand cmdInsert = new SqlCommand(@"
                    INSERT INTO CierreCaja
                    (Fecha, Turno, TotalIngresos, TotalGastos, TotalSistema, TotalContado, Diferencia, FechaCierre, CajaId, Usuario)
                    VALUES
                    (CAST(GETDATE() AS DATE), @Turno, @Ingresos, @Gastos, @TotalSistema, @TotalContado, @Diferencia, GETDATE(), @CajaId, @Usuario)",
                    conn, tx))
                {
                    cmdInsert.Parameters.AddWithValue("@Turno", turno);
                    cmdInsert.Parameters.AddWithValue("@Ingresos", ingresos);
                    cmdInsert.Parameters.AddWithValue("@Gastos", gastos);
                    cmdInsert.Parameters.AddWithValue("@TotalSistema", totalSistema);
                    cmdInsert.Parameters.AddWithValue("@TotalContado", totalContado);
                    cmdInsert.Parameters.AddWithValue("@Diferencia", diferencia);
                    cmdInsert.Parameters.AddWithValue("@CajaId", cajaId);
                    cmdInsert.Parameters.AddWithValue("@Usuario", usuario);
                    cmdInsert.ExecuteNonQuery();
                }

                using (SqlCommand cmdClose = new SqlCommand(@"
                    UPDATE Caja
                    SET Estado = 'CERRADA'
                    WHERE Id = @CajaId
                      AND Estado = 'ABIERTA'", conn, tx))
                {
                    cmdClose.Parameters.AddWithValue("@CajaId", cajaId);
                    int afectados = cmdClose.ExecuteNonQuery();
                    if (afectados <= 0)
                        throw new Exception("No se pudo cerrar la caja (ya estaba cerrada o no existe).");
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        // ===============================
        // OBTENER TOTAL INGRESOS POR FECHA (CORREGIDO)
        // ===============================
        public decimal ObtenerTotalIngresosPorFecha(DateTime fecha)
        {
            string query = IngresosNetosSql + @"
                             AND CAST(dc.Fecha AS DATE) = CAST(@fecha AS DATE)";

            SqlParameter[] parametros =
            {
                new SqlParameter("@fecha", fecha)
            };

            object? result = db.ExecuteScalar(query, parametros);
            return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0;
        }

        // ===============================
        // OBTENER TOTAL INGRESOS MES ACTUAL (CORREGIDO)
        // ===============================
        public decimal ObtenerTotalIngresosMesActual()
        {
            string query = IngresosNetosSql + @"
                             AND MONTH(dc.Fecha) = MONTH(GETDATE())
                             AND YEAR(dc.Fecha) = YEAR(GETDATE())";

            object? result = db.ExecuteScalar(query);
            return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0;
        }

        /// <summary>
        /// Suma ingresos vigentes en caja, excluyendo movimientos deshechos por Undo/Redo.
        /// </summary>
        private const string IngresosNetosSql = @"
            SELECT ISNULL(SUM(dc.Monto), 0)
            FROM DetalleCaja dc
            WHERE dc.TipoMovimiento = 'INGRESO'
              AND dc.Concepto NOT LIKE 'REVERSO (Ref #%'
              AND NOT EXISTS (
                  SELECT 1
                  FROM DetalleCaja rev
                  WHERE rev.CajaId = dc.CajaId
                    AND rev.TipoMovimiento = 'EGRESO'
                    AND rev.Concepto LIKE CONCAT('REVERSO (Ref #', dc.Id, '):%')
              )";
    }
}