using Microsoft.Data.SqlClient;
using System.Data;
using System;




namespace DL
{
    public class DeudaDAL
    {
        private DBHelper db = new DBHelper();
        public void RevertirPago(int pagoId, string usuario, int cajaId)
        {
            using (SqlConnection conn = new SqlConnection(db.ConnectionString))
            {
                conn.Open();

                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        // 🔒 OBTENER PAGO ORIGINAL
                        SqlCommand cmdPago = new SqlCommand(@"
                SELECT DeudaId, Monto 
                FROM PagosDeuda WITH (UPDLOCK)
                WHERE Id = @PagoId AND Estado = 'ACTIVO'", conn, tx);

                        cmdPago.Parameters.Add("@PagoId", SqlDbType.Int).Value = pagoId;

                        int deudaId;
                        decimal monto;

                        using (var reader = cmdPago.ExecuteReader())
                        {
                            if (!reader.Read())
                                throw new Exception("Pago no válido o ya anulado");

                            deudaId = Convert.ToInt32(reader["DeudaId"]);
                            monto = Convert.ToDecimal(reader["Monto"]);
                        }

                        // 🔹 MARCAR COMO ANULADO
                        SqlCommand cmdAnular = new SqlCommand(@"
                UPDATE PagosDeuda 
                SET Estado = 'ANULADO'
                WHERE Id = @PagoId", conn, tx);

                        cmdAnular.Parameters.Add("@PagoId", SqlDbType.Int).Value = pagoId;
                        cmdAnular.ExecuteNonQuery();

                        // 🔹 CREAR PAGO INVERSO (AUDITORÍA)
                        SqlCommand cmdInverso = new SqlCommand(@"
                INSERT INTO PagosDeuda (DeudaId, Monto, MetodoPago, Fecha, Usuario, Estado, PagoRelacionadoId)
                VALUES (@DeudaId, @Monto, 'REVERSO', GETDATE(), @Usuario, 'ACTIVO', @PagoId)", conn, tx);

                        cmdInverso.Parameters.Add("@DeudaId", SqlDbType.Int).Value = deudaId;
                        cmdInverso.Parameters.Add("@Monto", SqlDbType.Decimal).Value = monto;
                        cmdInverso.Parameters.Add("@Usuario", SqlDbType.VarChar, 100).Value = usuario;
                        cmdInverso.Parameters.Add("@PagoId", SqlDbType.Int).Value = pagoId;
                        cmdInverso.ExecuteNonQuery();

                        // 🔹 DEVOLVER SALDO A LA DEUDA
                        SqlCommand cmdDeuda = new SqlCommand(@"
                UPDATE Deudas
                SET 
                    MontoPagado = MontoPagado - @Monto,
                    Saldo = Saldo + @Monto,
                    Estado = 'ACTIVA'
                WHERE Id = @DeudaId", conn, tx);

                        cmdDeuda.Parameters.Add("@Monto", SqlDbType.Decimal).Value = monto;
                        cmdDeuda.Parameters.Add("@DeudaId", SqlDbType.Int).Value = deudaId;
                        cmdDeuda.ExecuteNonQuery();

                        // 🔹 HISTORIAL
                        SqlCommand cmdHistorial = new SqlCommand(@"
                INSERT INTO HistorialDeudas
                (DeudaId, ClienteId, TipoMovimiento, Monto, Descripcion, Fecha, Usuario)
                VALUES
                (@DeudaId,
                 (SELECT ClienteId FROM Deudas WHERE Id = @DeudaId),
                 'REVERSO_PAGO',
                 @Monto,
                 'Reversión de pago',
                 GETDATE(),
                 @Usuario)", conn, tx);

                        cmdHistorial.Parameters.Add("@DeudaId", SqlDbType.Int).Value = deudaId;
                        cmdHistorial.Parameters.Add("@Monto", SqlDbType.Decimal).Value = monto;
                        cmdHistorial.Parameters.Add("@Usuario", SqlDbType.VarChar, 100).Value = usuario;
                        cmdHistorial.ExecuteNonQuery();

                        // 🔹 CAJA (EGRESO)
                        SqlCommand cmdCaja = new SqlCommand(@"
                INSERT INTO DetalleCaja
                (CajaId, TipoMovimiento, Concepto, Monto, Fecha, Usuario, MetodoPago, ClienteId)
                VALUES
                (@CajaId, 'EGRESO', 'Reverso pago deuda', @Monto, GETDATE(), @Usuario, 'REVERSO',
                 (SELECT ClienteId FROM Deudas WHERE Id = @DeudaId))", conn, tx);

                        cmdCaja.Parameters.Add("@CajaId", SqlDbType.Int).Value = cajaId;
                        cmdCaja.Parameters.Add("@Monto", SqlDbType.Decimal).Value = monto;
                        cmdCaja.Parameters.Add("@Usuario", SqlDbType.VarChar, 100).Value = usuario;
                        cmdCaja.Parameters.Add("@DeudaId", SqlDbType.Int).Value = deudaId;
                        cmdCaja.ExecuteNonQuery();

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public int RegistrarPagoCompleto(
                                            int deudaId,
                                            decimal monto,
                                            string metodo,
                                            string usuario,
                                            int cajaId)
        {
            using (SqlConnection conn = new SqlConnection(db.ConnectionString))
            {
                conn.Open();

                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        // 🔒 BLOQUEAR DEUDA
                        SqlCommand cmd = new SqlCommand(@"
                SELECT Saldo, ClienteId 
                FROM Deudas WITH (UPDLOCK, ROWLOCK)
                WHERE Id = @Id", conn, tx);

                        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = deudaId;

                        int clienteId;
                        decimal saldo;

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                                throw new Exception("La deuda no existe");

                            saldo = Convert.ToDecimal(reader["Saldo"]);
                            clienteId = Convert.ToInt32(reader["ClienteId"]);

                            if (monto <= 0)
                                throw new Exception("Monto inválido");

                            if (monto > saldo)
                                throw new Exception($"Excede saldo: {saldo:N2}");
                        }

                        // 🔹 INSERTAR PAGO
                        SqlCommand cmdPago = new SqlCommand(@"
                INSERT INTO PagosDeuda (DeudaId, Monto, MetodoPago, Fecha, Usuario)
                OUTPUT INSERTED.Id
                VALUES (@DeudaId, @Monto, @Metodo, GETDATE(), @Usuario)", conn, tx);

                        cmdPago.Parameters.Add("@DeudaId", SqlDbType.Int).Value = deudaId;
                        cmdPago.Parameters.Add("@Monto", SqlDbType.Decimal).Value = monto;
                        cmdPago.Parameters.Add("@Metodo", SqlDbType.VarChar, 50).Value = metodo;
                        cmdPago.Parameters.Add("@Usuario", SqlDbType.VarChar, 100).Value = usuario;
                        int pagoId = Convert.ToInt32(cmdPago.ExecuteScalar());

                        // 🔹 UPDATE DEUDA
                        SqlCommand cmdDeuda = new SqlCommand(@"
                UPDATE Deudas
                SET 
                    MontoPagado = MontoPagado + @Monto,
                    Saldo = Saldo - @Monto,
                    Estado = CASE 
                                WHEN Saldo - @Monto <= 0 THEN 'PAGADA'
                                ELSE 'ACTIVA'
                             END
                WHERE Id = @DeudaId", conn, tx);

                        cmdDeuda.Parameters.Add("@Monto", SqlDbType.Decimal).Value = monto;
                        cmdDeuda.Parameters.Add("@DeudaId", SqlDbType.Int).Value = deudaId;
                        cmdDeuda.ExecuteNonQuery();

                        // 🔹 HISTORIAL
                        SqlCommand cmdHistorial = new SqlCommand(@"
                INSERT INTO HistorialDeudas
                (DeudaId, ClienteId, TipoMovimiento, Monto, Descripcion, Fecha, Usuario)
                VALUES
                (@DeudaId, @ClienteId, 'PAGO', @Monto, 'Pago de deuda', GETDATE(), @Usuario)", conn, tx);

                        cmdHistorial.Parameters.Add("@DeudaId", SqlDbType.Int).Value = deudaId;
                        cmdHistorial.Parameters.Add("@ClienteId", SqlDbType.Int).Value = clienteId;
                        cmdHistorial.Parameters.Add("@Monto", SqlDbType.Decimal).Value = monto;
                        cmdHistorial.Parameters.Add("@Usuario", SqlDbType.VarChar, 100).Value = usuario;
                        cmdHistorial.ExecuteNonQuery();

                        // 🔥 CAJA (MISMA TRANSACCIÓN)
                        SqlCommand cmdCaja = new SqlCommand(@"
                INSERT INTO DetalleCaja
                (CajaId, TipoMovimiento, Concepto, Monto, Fecha, Usuario, MetodoPago, ClienteId)
                VALUES
                (@CajaId, 'INGRESO', @Concepto, @Monto, GETDATE(), @Usuario, @MetodoPago, @ClienteId)", conn, tx);

                        cmdCaja.Parameters.Add("@CajaId", SqlDbType.Int).Value = cajaId;
                        cmdCaja.Parameters.Add("@Concepto", SqlDbType.VarChar, 200).Value = $"Pago deuda Cliente {clienteId}";
                        cmdCaja.Parameters.Add("@Monto", SqlDbType.Decimal).Value = monto;
                        cmdCaja.Parameters.Add("@Usuario", SqlDbType.VarChar, 100).Value = usuario;
                        cmdCaja.Parameters.Add("@MetodoPago", SqlDbType.NVarChar, 50).Value =
                            string.IsNullOrWhiteSpace(metodo) ? DBNull.Value : metodo.Trim();
                        cmdCaja.Parameters.Add("@ClienteId", SqlDbType.Int).Value = clienteId;
                        cmdCaja.ExecuteNonQuery();

                        tx.Commit();
                        return pagoId;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public decimal ObtenerSaldoPorDeuda(int deudaId)
        {
            string query = "SELECT Saldo FROM Deudas WHERE Id = @Id";

            SqlParameter[] p =
            {
        new SqlParameter("@Id", deudaId)
    };

            object result = db.ExecuteScalar(query, p);

            if (result == null || result == DBNull.Value)
                throw new Exception("La deuda no existe");

            return Convert.ToDecimal(result);
        }

        // ===============================
        // INSERTAR DEUDA
        // ===============================
        public int InsertarDeuda(int clienteId, string concepto, decimal monto, DateTime vencimiento, string usuario)
        {
            string query = @"
    INSERT INTO Deudas
    (ClienteId, Concepto, MontoTotal, MontoPagado, Saldo, FechaVencimiento, Estado, Usuario)
    OUTPUT INSERTED.Id
    VALUES
    (@ClienteId, @Concepto, @Monto, 0, @Monto, @Vencimiento, 'ACTIVA', @Usuario)";

            SqlParameter[] p =
            {
        new SqlParameter("@ClienteId", clienteId),
        new SqlParameter("@Concepto", concepto),
        new SqlParameter("@Monto", monto),
        new SqlParameter("@Vencimiento", vencimiento),
        new SqlParameter("@Usuario", usuario)
    };

            int deudaId = Convert.ToInt32(db.ExecuteScalar(query, p));

            // 🔥 REGISTRAR HISTORIAL
            string historialQuery = @"
    INSERT INTO HistorialDeudas
    (DeudaId, ClienteId, TipoMovimiento, Monto, Descripcion, Fecha, Usuario)
    VALUES
    (@DeudaId, @ClienteId, 'DEUDA', @Monto, @Concepto, GETDATE(), @Usuario)";

            SqlParameter[] h =
            {
        new SqlParameter("@DeudaId", deudaId),
        new SqlParameter("@ClienteId", clienteId),
        new SqlParameter("@Monto", monto),
        new SqlParameter("@Concepto", concepto),
        new SqlParameter("@Usuario", usuario)
    };

            db.ExecuteNonQuery(historialQuery, h);
            return deudaId;
        }

        public void AnularDeuda(int deudaId, string usuario)
        {
            string query = @"
                UPDATE Deudas
                SET Estado = 'ANULADA'
                WHERE Id = @DeudaId
                  AND Estado = 'ACTIVA'
                  AND MontoPagado = 0
                  AND Saldo = MontoTotal";

            SqlParameter[] p =
            {
                new SqlParameter("@DeudaId", deudaId)
            };

            int rows = db.ExecuteNonQuery(query, p);
            if (rows == 0)
                throw new Exception("La deuda no puede anularse (tiene pagos o ya fue modificada).");

            string historialQuery = @"
                INSERT INTO HistorialDeudas
                (DeudaId, ClienteId, TipoMovimiento, Monto, Descripcion, Fecha, Usuario)
                VALUES
                (@DeudaId,
                 (SELECT ClienteId FROM Deudas WHERE Id = @DeudaId),
                 'ANULACION',
                 0,
                 'Deuda anulada (deshacer)',
                 GETDATE(),
                 @Usuario)";

            SqlParameter[] h =
            {
                new SqlParameter("@DeudaId", deudaId),
                new SqlParameter("@Usuario", usuario)
            };

            db.ExecuteNonQuery(historialQuery, h);
        }

        // ===============================
        // INSERTAR DEUDA DE MEMBRESÍA FINANCIADA
        // ===============================
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="membresiaId">ID de la membresía financiada</param>
        /// <param name="planId">ID del plan</param>
        /// <param name="concepto">Descripción de la deuda</param>
        /// <param name="monto">Saldo pendiente (Total - Pago Inicial)</param>
        /// <param name="vencimiento">Fecha de vencimiento</param>
        /// <param name="usuario">Usuario que registra</param>
        /// <param name="montoTotalPlan">Precio total del plan financiado</param>
        /// <param name="pagoInicial">Monto aportado al momento del financiamiento</param>
        /// <returns>ID de la deuda creada</returns>
        public int InsertarDeudaMembresia(
            int clienteId,
            int membresiaId,
            int planId,
            string concepto,
            decimal monto,
            DateTime vencimiento,
            string usuario,
            decimal montoTotalPlan = 0,
            decimal pagoInicial = 0)
        {
            using SqlConnection conn = new SqlConnection(db.ConnectionString);
            conn.Open();
            using SqlTransaction tx = conn.BeginTransaction();
            try
            {
                int deudaId = InsertarDeudaMembresia(
                    conn, tx, clienteId, membresiaId, planId, concepto, monto,
                    vencimiento, usuario, montoTotalPlan, pagoInicial);
                tx.Commit();
                return deudaId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Inserta deuda de membresía financiada dentro de una transacción existente.
        /// </summary>
        public int InsertarDeudaMembresia(
            SqlConnection conn,
            SqlTransaction tx,
            int clienteId,
            int membresiaId,
            int planId,
            string concepto,
            decimal monto,
            DateTime vencimiento,
            string usuario,
            decimal montoTotalPlan = 0,
            decimal pagoInicial = 0)
        {
            decimal totalPlan = montoTotalPlan > 0 ? montoTotalPlan : monto + pagoInicial;
            string nombrePlan = concepto.StartsWith("Saldo plan ", StringComparison.OrdinalIgnoreCase)
                ? concepto.Substring("Saldo plan ".Length)
                : concepto;

            string descripcionDeuda =
                $"Financiamiento {nombrePlan} | Total plan: {totalPlan:N2} | " +
                $"Pago inicial: {pagoInicial:N2} | Saldo pendiente: {monto:N2} | " +
                $"Fecha límite: {vencimiento:dd/MM/yyyy}";

            if (pagoInicial <= 0)
                descripcionDeuda += " (sin aporte inicial)";

            string query = @"
    INSERT INTO Deudas
    (ClienteId, MembresiaId, PlanId, Concepto, MontoTotal, MontoPagado, Saldo, FechaVencimiento, Estado, Usuario)
    OUTPUT INSERTED.Id
    VALUES
    (@ClienteId, @MembresiaId, @PlanId, @Concepto, @Monto, 0, @Monto, @Vencimiento, 'ACTIVA', @Usuario)";

            int deudaId;
            using (SqlCommand cmd = new SqlCommand(query, conn, tx))
            {
                cmd.Parameters.AddWithValue("@ClienteId", clienteId);
                cmd.Parameters.AddWithValue("@MembresiaId", membresiaId);
                cmd.Parameters.AddWithValue("@PlanId", planId);
                cmd.Parameters.AddWithValue("@Concepto", concepto);
                cmd.Parameters.AddWithValue("@Monto", monto);
                cmd.Parameters.AddWithValue("@Vencimiento", vencimiento);
                cmd.Parameters.AddWithValue("@Usuario", usuario);
                deudaId = Convert.ToInt32(cmd.ExecuteScalar());
            }

            string historialDeudaQuery = @"
    INSERT INTO HistorialDeudas
    (DeudaId, ClienteId, TipoMovimiento, Monto, Descripcion, Fecha, Usuario)
    VALUES
    (@DeudaId, @ClienteId, 'DEUDA', @Monto, @Descripcion, GETDATE(), @Usuario)";

            using (SqlCommand cmdHist = new SqlCommand(historialDeudaQuery, conn, tx))
            {
                cmdHist.Parameters.AddWithValue("@DeudaId", deudaId);
                cmdHist.Parameters.AddWithValue("@ClienteId", clienteId);
                cmdHist.Parameters.AddWithValue("@Monto", monto);
                cmdHist.Parameters.AddWithValue("@Descripcion", descripcionDeuda);
                cmdHist.Parameters.AddWithValue("@Usuario", usuario);
                cmdHist.ExecuteNonQuery();
            }

            if (pagoInicial > 0)
            {
                string historialPagoInicialQuery = @"
    INSERT INTO HistorialDeudas
    (DeudaId, ClienteId, TipoMovimiento, Monto, Descripcion, Fecha, Usuario)
    VALUES
    (@DeudaId, @ClienteId, 'PAGO_INICIAL', @Monto, @Descripcion, GETDATE(), @Usuario)";

                using (SqlCommand cmdPago = new SqlCommand(historialPagoInicialQuery, conn, tx))
                {
                    cmdPago.Parameters.AddWithValue("@DeudaId", deudaId);
                    cmdPago.Parameters.AddWithValue("@ClienteId", clienteId);
                    cmdPago.Parameters.AddWithValue("@Monto", pagoInicial);
                    cmdPago.Parameters.AddWithValue("@Descripcion", $"Pago inicial al financiar - {nombrePlan}");
                    cmdPago.Parameters.AddWithValue("@Usuario", usuario);
                    cmdPago.ExecuteNonQuery();
                }
            }

            return deudaId;
        }

        // ===============================
        // INSERTAR PAGO
        // ===============================
        public void InsertarPago(int deudaId, decimal monto, string metodo, string usuario)
        {
            string query = @"
INSERT INTO PagosDeuda (DeudaId, Monto, MetodoPago, Usuario)
VALUES (@DeudaId, @Monto, @Metodo, @Usuario);

UPDATE Deudas
SET 
    MontoPagado = MontoPagado + @Monto,
    Saldo = Saldo - @Monto,
    Estado = CASE 
                WHEN Saldo - @Monto <= 0 THEN 'PAGADA'
                ELSE 'ACTIVA'
             END
WHERE Id = @DeudaId";

            SqlParameter[] p =
            {
        new SqlParameter("@DeudaId", deudaId),
        new SqlParameter("@Monto", monto),
        new SqlParameter("@Metodo", metodo),
        new SqlParameter("@Usuario", usuario)
    };

            db.ExecuteNonQuery(query, p);

            // 🔥 HISTORIAL (CORREGIDO)
            string historialQuery = @"
INSERT INTO HistorialDeudas
(DeudaId, ClienteId, TipoMovimiento, Monto, Descripcion, Fecha, Usuario)
VALUES
(@DeudaId,
 (SELECT ClienteId FROM Deudas WHERE Id = @DeudaId),
 'PAGO',
 @Monto,
 'Pago de deuda',
 GETDATE(),
 @Usuario)";

            SqlParameter[] h =
            {
        new SqlParameter("@DeudaId", deudaId),
        new SqlParameter("@Monto", monto),
        new SqlParameter("@Usuario", usuario)
    };

            db.ExecuteNonQuery(historialQuery, h);
        }
        // ===============================
        // OBTENER DEUDAS (CON INFO DE PLANES)
        // ===============================
        public DataTable ObtenerDeudas(bool soloActivas = true)
        {
            string condicionEstado = soloActivas ? "WHERE d.Estado = 'ACTIVA'" : string.Empty;
            string query = $@"
            SELECT 
                d.Id, 
                d.ClienteId, 
                c.Nombre, 
                d.Concepto, 
                d.MontoTotal, 
                d.MontoPagado, 
                d.Saldo,
                d.Estado, 
                d.FechaCreacion, 
                d.FechaVencimiento, 
                d.Usuario,
                d.MembresiaId,
                d.PlanId,
                ISNULL(p.Nombre, 'N/A') AS [Plan],
                m.FechaInicio AS FechaInicioMembresia,
                m.FechaFin AS FechaFinMembresia,
                ISNULL(pi.PagoInicial, 0) AS PagoInicialFinanciamiento
            FROM Deudas d
            INNER JOIN Clientes c ON c.ID = d.ClienteId
            LEFT JOIN Membresias m ON m.Id = d.MembresiaId
            LEFT JOIN Planes p ON p.Id = d.PlanId
            OUTER APPLY (
                SELECT TOP 1 h.Monto AS PagoInicial
                FROM HistorialDeudas h
                WHERE h.DeudaId = d.Id
                  AND h.TipoMovimiento = 'PAGO_INICIAL'
                ORDER BY h.Fecha ASC
            ) pi
            {condicionEstado}
            ORDER BY d.FechaVencimiento ASC";

            return db.ExecuteQuery(query);
        }

        /// <summary>
        /// Deudas activas con teléfono y dirección del cliente (para reporte PDF).
        /// </summary>
        public DataTable ObtenerDeudasParaReporte()
        {
            string query = @"
            SELECT 
                d.Id, 
                d.ClienteId, 
                c.Nombre,
                ISNULL(c.Telefono, '') AS Telefono,
                ISNULL(c.Direccion, '') AS Direccion,
                d.Concepto, 
                d.MontoTotal, 
                d.MontoPagado, 
                d.Saldo,
                d.Estado, 
                d.FechaCreacion, 
                d.FechaVencimiento, 
                d.Usuario,
                d.MembresiaId,
                d.PlanId,
                ISNULL(p.Nombre, 'N/A') AS [Plan],
                m.FechaInicio AS FechaInicioMembresia,
                m.FechaFin AS FechaFinMembresia,
                ISNULL(pi.PagoInicial, 0) AS PagoInicialFinanciamiento
            FROM Deudas d
            INNER JOIN Clientes c ON c.ID = d.ClienteId
            LEFT JOIN Membresias m ON m.Id = d.MembresiaId
            LEFT JOIN Planes p ON p.Id = d.PlanId
            OUTER APPLY (
                SELECT TOP 1 h.Monto AS PagoInicial
                FROM HistorialDeudas h
                WHERE h.DeudaId = d.Id
                  AND h.TipoMovimiento = 'PAGO_INICIAL'
                ORDER BY h.Fecha ASC
            ) pi
            WHERE d.Estado = 'ACTIVA'
            ORDER BY d.FechaVencimiento ASC";

            return db.ExecuteQuery(query);
        }

        public int ObtenerClientePorDeuda(int deudaId)
        {
            string query = "SELECT ClienteId FROM Deudas WHERE Id = @Id";

            SqlParameter[] p =
            {
                new SqlParameter("@Id", deudaId)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, p));
        }

        // ===============================
        // DASHBOARD
        // ===============================
        public int DeudasActivas()
        {
            return Convert.ToInt32(db.ExecuteScalar("SELECT COUNT(*) FROM Deudas WHERE Estado = 'ACTIVA'"));
        }

        public int DeudasVencidas()
        {
            return Convert.ToInt32(db.ExecuteScalar("SELECT COUNT(*) FROM Deudas WHERE Estado = 'ACTIVA' AND FechaVencimiento < GETDATE()"));
        }

        public decimal IngresoPendiente()
        {
            return Convert.ToDecimal(db.ExecuteScalar("SELECT ISNULL(SUM(Saldo),0) FROM Deudas WHERE Estado = 'ACTIVA'"));
        }

        /// <summary>
        /// Obtiene el total cobrado en pagos de deudas el día de hoy
        /// </summary>
        /// <returns>Monto total cobrado hoy</returns>
        public decimal TotalCobradoHoy()
        {
            string query = @"
                SELECT ISNULL(SUM(Monto), 0)
                FROM PagosDeuda
                WHERE CAST(Fecha AS DATE) = CAST(GETDATE() AS DATE)
                  AND Estado <> 'ANULADO'";

            return Convert.ToDecimal(db.ExecuteScalar(query));
        }

        /// <summary>
        /// Obtiene el total cobrado en pagos de deudas en el mes actual
        /// </summary>
        /// <returns>Monto total cobrado este mes</returns>
        public decimal TotalCobradoEsteMes()
        {
            string query = @"
                SELECT ISNULL(SUM(Monto), 0)
                FROM PagosDeuda
                WHERE YEAR(Fecha) = YEAR(GETDATE())
                  AND MONTH(Fecha) = MONTH(GETDATE())
                  AND Estado <> 'ANULADO'";

            return Convert.ToDecimal(db.ExecuteScalar(query));
        }

        /// <summary>
        /// Obtiene la cantidad de clientes únicos con deudas activas
        /// </summary>
        /// <returns>Cantidad de clientes con deuda</returns>
        public int ClientesConDeuda()
        {
            string query = @"
                SELECT COUNT(DISTINCT ClienteId)
                FROM Deudas
                WHERE Estado = 'ACTIVA' AND Saldo > 0";

            return Convert.ToInt32(db.ExecuteScalar(query));
        }

        /// <summary>
        /// True si el cliente tiene al menos una deuda ACTIVA con saldo pendiente.
        /// </summary>
        public bool TieneDeudasActivas(int clienteId)
        {
            string query = @"
                SELECT COUNT(*)
                FROM Deudas
                WHERE ClienteId = @ClienteId
                  AND Estado = 'ACTIVA'
                  AND Saldo > 0";

            SqlParameter[] p = { new SqlParameter("@ClienteId", SqlDbType.Int) { Value = clienteId } };
            return Convert.ToInt32(db.ExecuteScalar(query, p)) > 0;
        }

        /// <summary>
        /// Deudas activas del cliente (con plan y saldo) para avisos de bloqueo.
        /// </summary>
        public DataTable ObtenerDeudasActivasCliente(int clienteId)
        {
            string query = @"
                SELECT
                    d.Id,
                    d.Concepto,
                    d.MontoTotal,
                    d.MontoPagado,
                    d.Saldo,
                    d.FechaVencimiento,
                    d.MembresiaId,
                    d.PlanId,
                    ISNULL(p.Nombre, NULL) AS [Plan]
                FROM Deudas d
                LEFT JOIN Planes p ON p.Id = d.PlanId
                WHERE d.ClienteId = @ClienteId
                  AND d.Estado = 'ACTIVA'
                  AND d.Saldo > 0
                ORDER BY d.FechaVencimiento ASC";

            SqlParameter[] p = { new SqlParameter("@ClienteId", SqlDbType.Int) { Value = clienteId } };
            return db.ExecuteQuery(query, p);
        }

        /// <summary>
        /// Verifica si un cliente tiene deudas vencidas
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <returns>True si tiene deudas vencidas, false en caso contrario</returns>
        public bool TieneDeudasVencidas(int clienteId)
        {
            string query = @"
                SELECT COUNT(*) 
                FROM Deudas 
                WHERE ClienteId = @ClienteId 
                  AND Estado = 'ACTIVA' 
                  AND FechaVencimiento < GETDATE()";

            SqlParameter[] p = { new SqlParameter("@ClienteId", SqlDbType.Int) { Value = clienteId } };

            int count = Convert.ToInt32(db.ExecuteScalar(query, p));
            return count > 0;
        }

        /// <summary>
        /// Obtiene el monto total de deudas vencidas de un cliente
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <returns>Monto total de deudas vencidas</returns>
        public decimal ObtenerMontoDeudasVencidas(int clienteId)
        {
            string query = @"
                SELECT ISNULL(SUM(Saldo), 0)
                FROM Deudas 
                WHERE ClienteId = @ClienteId 
                  AND Estado = 'ACTIVA' 
                  AND FechaVencimiento < GETDATE()";

            SqlParameter[] p = { new SqlParameter("@ClienteId", SqlDbType.Int) { Value = clienteId } };

            return Convert.ToDecimal(db.ExecuteScalar(query, p));
        }
    }
}