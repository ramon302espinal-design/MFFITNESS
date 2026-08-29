using Microsoft.Data.SqlClient;
using System.Data;
using System;
using DTO;




namespace DL
{
    public class DeudaDAL
    {
        private DBHelper db = new DBHelper();
        private readonly VentasDAL ventasDAL = new VentasDAL();
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

                        ventasDAL.SincronizarMontoPagadoDesdeDeuda(conn, tx, deudaId);

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

                        ventasDAL.SincronizarMontoPagadoDesdeDeuda(conn, tx, deudaId);

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
        /// <param name="pagoInicial">
        /// Aporte al financiar (venta a crédito / productos). Solo historial PAGO_INICIAL;
        /// la caja la registra quien cobra (p. ej. VentasBLL).
        /// </param>
        /// <param name="montoTotalFinanciado">Total del financiamiento (saldo + pago inicial).</param>
        public int InsertarDeuda(
            int clienteId,
            string concepto,
            decimal monto,
            DateTime vencimiento,
            string usuario,
            decimal pagoInicial = 0,
            decimal montoTotalFinanciado = 0)
        {
            using SqlConnection conn = new SqlConnection(db.ConnectionString);
            conn.Open();
            using SqlTransaction tx = conn.BeginTransaction();
            try
            {
                int id = InsertarDeuda(
                    conn, tx, clienteId, concepto, monto, vencimiento, usuario,
                    pagoInicial, montoTotalFinanciado);
                tx.Commit();
                return id;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        /// <summary>Inserta deuda + historial DEUDA (+ PAGO_INICIAL si aplica) dentro de TX existente.</summary>
        public int InsertarDeuda(
            SqlConnection conn,
            SqlTransaction tx,
            int clienteId,
            string concepto,
            decimal monto,
            DateTime vencimiento,
            string usuario,
            decimal pagoInicial = 0,
            decimal montoTotalFinanciado = 0)
        {
            pagoInicial = decimal.Round(Math.Max(0m, pagoInicial), 2);
            decimal totalFinanciado = montoTotalFinanciado > 0
                ? decimal.Round(montoTotalFinanciado, 2)
                : decimal.Round(monto + pagoInicial, 2);

            string query = @"
    INSERT INTO Deudas
    (ClienteId, Concepto, MontoTotal, MontoPagado, Saldo, FechaVencimiento, Estado, Usuario)
    OUTPUT INSERTED.Id
    VALUES
    (@ClienteId, @Concepto, @Monto, 0, @Monto, @Vencimiento, 'ACTIVA', @Usuario)";

            int deudaId;
            using (SqlCommand cmd = new SqlCommand(query, conn, tx))
            {
                cmd.Parameters.AddWithValue("@ClienteId", clienteId);
                cmd.Parameters.AddWithValue("@Concepto", concepto);
                cmd.Parameters.AddWithValue("@Monto", monto);
                cmd.Parameters.AddWithValue("@Vencimiento", vencimiento);
                cmd.Parameters.AddWithValue("@Usuario", usuario);
                deudaId = Convert.ToInt32(cmd.ExecuteScalar());
            }

            string descripcionDeuda = concepto;
            if (pagoInicial > 0)
            {
                descripcionDeuda =
                    $"{concepto} | Total: {totalFinanciado:N2} | Pago inicial: {pagoInicial:N2} | " +
                    $"Saldo pendiente: {monto:N2} | Fecha límite: {vencimiento:dd/MM/yyyy}";
            }

            using (SqlCommand cmdHist = new SqlCommand(@"
    INSERT INTO HistorialDeudas
    (DeudaId, ClienteId, TipoMovimiento, Monto, Descripcion, Fecha, Usuario)
    VALUES
    (@DeudaId, @ClienteId, 'DEUDA', @Monto, @Concepto, GETDATE(), @Usuario)", conn, tx))
            {
                cmdHist.Parameters.AddWithValue("@DeudaId", deudaId);
                cmdHist.Parameters.AddWithValue("@ClienteId", clienteId);
                cmdHist.Parameters.AddWithValue("@Monto", monto);
                cmdHist.Parameters.AddWithValue("@Concepto", descripcionDeuda);
                cmdHist.Parameters.AddWithValue("@Usuario", usuario);
                cmdHist.ExecuteNonQuery();
            }

            if (pagoInicial > 0)
            {
                using SqlCommand cmdPi = new SqlCommand(@"
    INSERT INTO HistorialDeudas
    (DeudaId, ClienteId, TipoMovimiento, Monto, Descripcion, Fecha, Usuario)
    VALUES
    (@DeudaId, @ClienteId, 'PAGO_INICIAL', @Monto, @Descripcion, GETDATE(), @Usuario)", conn, tx);
                cmdPi.Parameters.AddWithValue("@DeudaId", deudaId);
                cmdPi.Parameters.AddWithValue("@ClienteId", clienteId);
                cmdPi.Parameters.AddWithValue("@Monto", pagoInicial);
                cmdPi.Parameters.AddWithValue("@Descripcion", $"Pago inicial al financiar - {concepto}");
                cmdPi.Parameters.AddWithValue("@Usuario", usuario);
                cmdPi.ExecuteNonQuery();
            }

            return deudaId;
        }

        public static bool ExistePagoInicialVigente(SqlConnection conn, SqlTransaction tx, int deudaId)
        {
            using SqlCommand cmd = new SqlCommand(@"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM HistorialDeudas
                    WHERE DeudaId = @DeudaId AND TipoMovimiento = 'PAGO_INICIAL'
                ) THEN 1 ELSE 0 END", conn, tx);
            cmd.Parameters.AddWithValue("@DeudaId", deudaId);
            return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
        }

        public void AnularDeuda(int deudaId, string usuario)
        {
            using SqlConnection conn = new SqlConnection(db.ConnectionString);
            conn.Open();
            using SqlTransaction tx = conn.BeginTransaction();
            try
            {
                AnularDeuda(conn, tx, deudaId, usuario);
                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public void AnularDeuda(SqlConnection conn, SqlTransaction tx, int deudaId, string usuario)
        {
            using SqlCommand cmd = new SqlCommand(@"
                UPDATE Deudas
                SET Estado = 'ANULADA'
                WHERE Id = @DeudaId
                  AND Estado = 'ACTIVA'
                  AND MontoPagado = 0
                  AND Saldo = MontoTotal", conn, tx);
            cmd.Parameters.AddWithValue("@DeudaId", deudaId);

            if (cmd.ExecuteNonQuery() == 0)
                throw new Exception("La deuda no puede anularse (tiene pagos o ya fue modificada).");

            using SqlCommand cmdHist = new SqlCommand(@"
                INSERT INTO HistorialDeudas
                (DeudaId, ClienteId, TipoMovimiento, Monto, Descripcion, Fecha, Usuario)
                VALUES
                (@DeudaId,
                 (SELECT ClienteId FROM Deudas WHERE Id = @DeudaId),
                 'ANULACION',
                 0,
                 'Deuda anulada (deshacer)',
                 GETDATE(),
                 @Usuario)", conn, tx);
            cmdHist.Parameters.AddWithValue("@DeudaId", deudaId);
            cmdHist.Parameters.AddWithValue("@Usuario", usuario);
            cmdHist.ExecuteNonQuery();
        }

        // ===============================
        // EDICIÓN DE DEUDA
        // ===============================
        /// <summary>
        /// Detalle completo de una deuda (para la pantalla de edición).
        /// </summary>
        /// <param name="deudaId">ID de la deuda</param>
        /// <returns>Fila con los datos de la deuda o null si no existe</returns>
        public DataRow? ObtenerDeudaDetalle(int deudaId)
        {
            string query = @"
            SELECT
                d.Id,
                d.ClienteId,
                c.Nombre AS Cliente,
                d.Concepto,
                d.MontoTotal,
                d.MontoPagado,
                d.Saldo,
                d.Estado,
                d.FechaCreacion,
                d.FechaVencimiento,
                d.MembresiaId,
                d.PlanId,
                ISNULL(p.Nombre, '') AS [Plan],
                ISNULL(pi.PagoInicial, 0) AS PagoInicial
            FROM Deudas d
            INNER JOIN Clientes c ON c.ID = d.ClienteId
            LEFT JOIN Planes p ON p.Id = d.PlanId
            OUTER APPLY (
                SELECT SUM(CASE WHEN h.TipoMovimiento = 'PAGO_INICIAL'
                                THEN h.Monto ELSE -h.Monto END) AS PagoInicial
                FROM HistorialDeudas h
                WHERE h.DeudaId = d.Id
                  AND h.TipoMovimiento IN ('PAGO_INICIAL', 'REVERSO_PAGO_INICIAL')
            ) pi
            WHERE d.Id = @Id";

            SqlParameter[] p =
            {
                new SqlParameter("@Id", SqlDbType.Int) { Value = deudaId }
            };

            DataTable dt = db.ExecuteQuery(query, p);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        /// <summary>
        /// Edita el financiamiento de una deuda activa: concepto, plan, fecha límite y
        /// pago inicial. El monto financiado se deriva del total menos el pago inicial.
        /// Si el pago inicial cambia, el anterior se reversa (egreso en caja + movimiento
        /// REVERSO_PAGO_INICIAL) y el nuevo se registra como ingreso, de modo que ningún
        /// panel ni cuadre sume dos veces. No altera la vigencia de la membresía.
        /// </summary>
        /// <param name="deudaId">ID de la deuda a editar</param>
        /// <param name="concepto">Nuevo concepto / descripción</param>
        /// <param name="totalFinanciado">Total del plan o producto financiado</param>
        /// <param name="pagoInicialNuevo">Pago inicial corregido (0 = sin inicial)</param>
        /// <param name="vencimiento">Nueva fecha límite de pago</param>
        /// <param name="planId">Plan financiado, o null para producto a crédito / otro concepto</param>
        /// <param name="metodoPago">Método del nuevo ingreso en caja</param>
        /// <param name="cajaId">Caja abierta donde se asientan reverso e ingreso (0 si no hay)</param>
        /// <param name="usuario">Usuario que edita</param>
        /// <returns>Resultado con saldo, estado y detalle del reverso aplicado</returns>
        public EdicionDeudaDTO ActualizarDeudaFinanciamiento(
            int deudaId,
            string concepto,
            decimal totalFinanciado,
            decimal pagoInicialNuevo,
            DateTime vencimiento,
            int? planId,
            string metodoPago,
            int cajaId,
            string usuario)
        {
            using SqlConnection conn = new SqlConnection(db.ConnectionString);
            conn.Open();
            using SqlTransaction tx = conn.BeginTransaction();
            try
            {
                int clienteId;
                string conceptoActual;
                string estadoActual;
                decimal montoTotalActual;
                decimal montoPagado;
                DateTime fechaCreacion;
                DateTime? vencimientoActual;
                int? planIdActual;
                int? membresiaId;

                string queryActual = @"
            SELECT ClienteId, Concepto, MontoTotal, MontoPagado, FechaCreacion,
                   FechaVencimiento, Estado, PlanId, MembresiaId
            FROM Deudas WITH (UPDLOCK, ROWLOCK)
            WHERE Id = @Id";

                using (SqlCommand cmdActual = new SqlCommand(queryActual, conn, tx))
                {
                    cmdActual.Parameters.Add("@Id", SqlDbType.Int).Value = deudaId;

                    using SqlDataReader reader = cmdActual.ExecuteReader();
                    if (!reader.Read())
                        throw new Exception("La deuda no existe o fue eliminada.");

                    clienteId = Convert.ToInt32(reader["ClienteId"]);
                    conceptoActual = reader["Concepto"] == DBNull.Value
                        ? string.Empty
                        : Convert.ToString(reader["Concepto"]) ?? string.Empty;
                    montoTotalActual = reader["MontoTotal"] == DBNull.Value
                        ? 0m
                        : Convert.ToDecimal(reader["MontoTotal"]);
                    montoPagado = Convert.ToDecimal(reader["MontoPagado"]);
                    fechaCreacion = reader["FechaCreacion"] == DBNull.Value
                        ? DateTime.Now
                        : Convert.ToDateTime(reader["FechaCreacion"]);
                    vencimientoActual = reader["FechaVencimiento"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(reader["FechaVencimiento"]);
                    estadoActual = (Convert.ToString(reader["Estado"]) ?? string.Empty).Trim();
                    planIdActual = reader["PlanId"] == DBNull.Value
                        ? null
                        : Convert.ToInt32(reader["PlanId"]);
                    membresiaId = reader["MembresiaId"] == DBNull.Value
                        ? null
                        : Convert.ToInt32(reader["MembresiaId"]);
                }

                if (!string.Equals(estadoActual, "ACTIVA", StringComparison.OrdinalIgnoreCase))
                    throw new Exception(
                        $"Solo se pueden editar deudas activas. Estado actual: {estadoActual}.");

                if (membresiaId.HasValue && !planId.HasValue)
                    throw new Exception(
                        "Esta deuda financia una membresía: debe mantener un plan seleccionado.");

                decimal pagoInicialAnterior = ObtenerPagoInicialVigente(conn, tx, deudaId);
                decimal nuevoMontoTotal = decimal.Round(totalFinanciado - pagoInicialNuevo, 2);

                if (nuevoMontoTotal < 0m)
                    throw new Exception("El pago inicial no puede superar el total financiado.");

                if (nuevoMontoTotal < montoPagado)
                    throw new Exception(
                        "Con ese pago inicial el financiamiento queda por debajo de los abonos " +
                        $"ya cobrados (RD$ {montoPagado:N2}). Reduzca el pago inicial.");

                bool cambiaPagoInicial = pagoInicialAnterior != pagoInicialNuevo;
                if (cambiaPagoInicial && cajaId <= 0)
                    throw new Exception(
                        "Debe haber una caja abierta para reversar el pago inicial anterior " +
                        "y registrar el nuevo.");

                decimal nuevoSaldo = nuevoMontoTotal - montoPagado;
                string nuevoEstado = nuevoSaldo <= 0m ? "PAGADA" : "ACTIVA";

                string queryUpdate = @"
            UPDATE Deudas
            SET Concepto = @Concepto,
                MontoTotal = @MontoTotal,
                Saldo = @Saldo,
                FechaVencimiento = @Vencimiento,
                PlanId = @PlanId,
                Estado = @Estado
            WHERE Id = @Id
              AND Estado = 'ACTIVA'";

                using (SqlCommand cmdUpdate = new SqlCommand(queryUpdate, conn, tx))
                {
                    cmdUpdate.Parameters.Add("@Concepto", SqlDbType.NVarChar, 200).Value = concepto;
                    cmdUpdate.Parameters.Add("@MontoTotal", SqlDbType.Decimal).Value = nuevoMontoTotal;
                    cmdUpdate.Parameters.Add("@Saldo", SqlDbType.Decimal).Value = nuevoSaldo;
                    cmdUpdate.Parameters.Add("@Vencimiento", SqlDbType.DateTime).Value = vencimiento;
                    cmdUpdate.Parameters.Add("@PlanId", SqlDbType.Int).Value =
                        planId.HasValue ? planId.Value : (object)DBNull.Value;
                    cmdUpdate.Parameters.Add("@Estado", SqlDbType.NVarChar, 20).Value = nuevoEstado;
                    cmdUpdate.Parameters.Add("@Id", SqlDbType.Int).Value = deudaId;

                    if (cmdUpdate.ExecuteNonQuery() == 0)
                        throw new Exception("La deuda cambió de estado durante la edición. Actualice e intente de nuevo.");
                }

                var resultado = new EdicionDeudaDTO
                {
                    DeudaId = deudaId,
                    TotalFinanciado = decimal.Round(totalFinanciado, 2),
                    PagoInicialAnterior = pagoInicialAnterior,
                    PagoInicialNuevo = pagoInicialNuevo,
                    Abonos = montoPagado,
                    Saldo = nuevoSaldo,
                    Estado = nuevoEstado
                };

                if (cambiaPagoInicial)
                {
                    AplicarReversoPagoInicial(
                        conn,
                        tx,
                        resultado,
                        clienteId,
                        cajaId,
                        metodoPago,
                        fechaCreacion,
                        planIdActual,
                        usuario);
                }

                string descripcion = ConstruirDescripcionEdicion(
                    conn,
                    tx,
                    conceptoActual,
                    concepto,
                    montoTotalActual,
                    nuevoMontoTotal,
                    vencimientoActual,
                    vencimiento,
                    planIdActual,
                    planId,
                    resultado);

                string queryHistorial = @"
            INSERT INTO HistorialDeudas
            (DeudaId, ClienteId, TipoMovimiento, Monto, Descripcion, Fecha, Usuario)
            VALUES
            (@DeudaId, @ClienteId, 'EDICION', @Monto, @Descripcion, GETDATE(), @Usuario)";

                using (SqlCommand cmdHistorial = new SqlCommand(queryHistorial, conn, tx))
                {
                    cmdHistorial.Parameters.Add("@DeudaId", SqlDbType.Int).Value = deudaId;
                    cmdHistorial.Parameters.Add("@ClienteId", SqlDbType.Int).Value = clienteId;
                    cmdHistorial.Parameters.Add("@Monto", SqlDbType.Decimal).Value = nuevoMontoTotal;
                    cmdHistorial.Parameters.Add("@Descripcion", SqlDbType.VarChar, 255).Value = descripcion;
                    cmdHistorial.Parameters.Add("@Usuario", SqlDbType.VarChar, 100).Value = usuario;
                    cmdHistorial.ExecuteNonQuery();
                }

                tx.Commit();
                return resultado;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Pago inicial vigente de la deuda: lo aportado al financiar menos los reversos.
        /// </summary>
        private static decimal ObtenerPagoInicialVigente(
            SqlConnection conn,
            SqlTransaction tx,
            int deudaId)
        {
            using SqlCommand cmd = new SqlCommand(@"
            SELECT ISNULL(SUM(CASE WHEN TipoMovimiento = 'PAGO_INICIAL'
                                   THEN Monto ELSE -Monto END), 0)
            FROM HistorialDeudas
            WHERE DeudaId = @DeudaId
              AND TipoMovimiento IN ('PAGO_INICIAL', 'REVERSO_PAGO_INICIAL')", conn, tx);

            cmd.Parameters.Add("@DeudaId", SqlDbType.Int).Value = deudaId;
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }

        /// <summary>
        /// Deja el pago inicial anterior en reverso y asienta el nuevo: historial de la deuda,
        /// caja (egreso + ingreso), pago de membresía e historial de membresía. Así los paneles
        /// restan lo viejo en lugar de acumularlo.
        /// </summary>
        private static void AplicarReversoPagoInicial(
            SqlConnection conn,
            SqlTransaction tx,
            EdicionDeudaDTO resultado,
            int clienteId,
            int cajaId,
            string metodoPago,
            DateTime fechaCreacionDeuda,
            int? planIdOriginal,
            string usuario)
        {
            var cajaDAL = new CajaDAL();
            string nombreCliente = ObtenerNombreCliente(conn, tx, clienteId);
            decimal anterior = resultado.PagoInicialAnterior;
            decimal nuevo = resultado.PagoInicialNuevo;

            if (anterior > 0m)
            {
                RegistrarMovimientoHistorial(
                    conn,
                    tx,
                    resultado.DeudaId,
                    clienteId,
                    "REVERSO_PAGO_INICIAL",
                    anterior,
                    $"Reverso pago inicial de RD$ {anterior:N2} por edición de la deuda",
                    usuario);

                // Se marca con la referencia al ingreso original: así los paneles de
                // ingresos lo descuentan igual que un "deshacer" de caja.
                string conceptoReverso = ConstruirConceptoReverso(
                    conn, tx, cajaId, clienteId, anterior, nombreCliente, resultado.DeudaId);

                cajaDAL.InsertarMovimientoConId(
                    conn,
                    tx,
                    cajaId,
                    "EGRESO",
                    conceptoReverso,
                    anterior,
                    usuario,
                    "REVERSO",
                    clienteId);

                resultado.ReversoCaja = true;
            }

            if (nuevo > 0m)
            {
                RegistrarMovimientoHistorial(
                    conn,
                    tx,
                    resultado.DeudaId,
                    clienteId,
                    "PAGO_INICIAL",
                    nuevo,
                    $"Pago inicial corregido a RD$ {nuevo:N2} (antes RD$ {anterior:N2})",
                    usuario);

                cajaDAL.InsertarMovimientoConId(
                    conn,
                    tx,
                    cajaId,
                    "INGRESO",
                    $"Pago membresía - {nombreCliente} (Cliente {clienteId}) - Pago inicial corregido (Deuda #{resultado.DeudaId})",
                    nuevo,
                    usuario,
                    string.IsNullOrWhiteSpace(metodoPago) ? "Efectivo" : metodoPago,
                    clienteId);

                resultado.IngresoCaja = true;
            }

            AjustarPagoMembresia(
                conn, tx, clienteId, anterior, nuevo, fechaCreacionDeuda, metodoPago, usuario);

            AjustarHistorialMembresia(
                conn, tx, clienteId, planIdOriginal, anterior, nuevo, fechaCreacionDeuda, resultado.Saldo);
        }

        /// <summary>
        /// Concepto del egreso de reverso. Si el ingreso original sigue vivo en la misma caja
        /// se usa la marca "REVERSO (Ref #id)" que los paneles de ingresos ya descuentan;
        /// si no se puede ubicar (caja distinta o ya cerrada) queda un egreso descriptivo.
        /// </summary>
        private static string ConstruirConceptoReverso(
            SqlConnection conn,
            SqlTransaction tx,
            int cajaId,
            int clienteId,
            decimal monto,
            string nombreCliente,
            int deudaId)
        {
            string generico =
                $"REVERSO pago inicial - {nombreCliente} (Cliente {clienteId}) - Deuda #{deudaId}";

            using SqlCommand cmd = new SqlCommand(@"
            SELECT TOP 1 dc.Id, dc.Concepto
            FROM DetalleCaja dc
            WHERE dc.CajaId = @CajaId
              AND dc.TipoMovimiento = 'INGRESO'
              AND dc.ClienteId = @ClienteId
              AND ABS(dc.Monto - @Monto) < 0.01
              AND dc.Concepto NOT LIKE 'REVERSO (Ref #%'
              AND NOT EXISTS (
                  SELECT 1
                  FROM DetalleCaja rev
                  WHERE rev.CajaId = dc.CajaId
                    AND rev.TipoMovimiento = 'EGRESO'
                    AND rev.Concepto LIKE CONCAT('REVERSO (Ref #', dc.Id, '):%')
              )
            ORDER BY dc.Id DESC", conn, tx);

            cmd.Parameters.Add("@CajaId", SqlDbType.Int).Value = cajaId;
            cmd.Parameters.Add("@ClienteId", SqlDbType.Int).Value = clienteId;
            cmd.Parameters.Add("@Monto", SqlDbType.Decimal).Value = monto;

            int movimientoId;
            string conceptoOriginal;

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                if (!reader.Read())
                    return generico;

                movimientoId = Convert.ToInt32(reader["Id"]);
                conceptoOriginal = reader["Concepto"] == DBNull.Value
                    ? string.Empty
                    : Convert.ToString(reader["Concepto"]) ?? string.Empty;
            }

            string concepto = $"REVERSO (Ref #{movimientoId}): {conceptoOriginal}".TrimEnd();
            return concepto.Length > 200 ? concepto.Substring(0, 200) : concepto;
        }

        private static void RegistrarMovimientoHistorial(
            SqlConnection conn,
            SqlTransaction tx,
            int deudaId,
            int clienteId,
            string tipoMovimiento,
            decimal monto,
            string descripcion,
            string usuario)
        {
            using SqlCommand cmd = new SqlCommand(@"
            INSERT INTO HistorialDeudas
            (DeudaId, ClienteId, TipoMovimiento, Monto, Descripcion, Fecha, Usuario)
            VALUES
            (@DeudaId, @ClienteId, @Tipo, @Monto, @Descripcion, GETDATE(), @Usuario)", conn, tx);

            cmd.Parameters.Add("@DeudaId", SqlDbType.Int).Value = deudaId;
            cmd.Parameters.Add("@ClienteId", SqlDbType.Int).Value = clienteId;
            cmd.Parameters.Add("@Tipo", SqlDbType.VarChar, 50).Value = tipoMovimiento;
            cmd.Parameters.Add("@Monto", SqlDbType.Decimal).Value = monto;
            cmd.Parameters.Add("@Descripcion", SqlDbType.VarChar, 255).Value =
                descripcion.Length > 255 ? descripcion.Substring(0, 255) : descripcion;
            cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 100).Value = usuario;
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Reemplaza el monto del pago de membresía que originó el financiamiento para que
        /// el historial de pagos y los reportes no sigan sumando el importe viejo. Si no
        /// existe (deuda sin pago inicial), crea el registro del nuevo cobro.
        /// </summary>
        private static void AjustarPagoMembresia(
            SqlConnection conn,
            SqlTransaction tx,
            int clienteId,
            decimal anterior,
            decimal nuevo,
            DateTime fechaCreacionDeuda,
            string metodoPago,
            string usuario)
        {
            int pagoId = 0;

            if (anterior > 0m)
            {
                using SqlCommand cmdBuscar = new SqlCommand(@"
            SELECT TOP 1 Id
            FROM Pagos
            WHERE ClienteId = @ClienteId
              AND ABS(Monto - @Monto) < 0.01
              AND ABS(DATEDIFF(MINUTE, FechaPago, @Fecha)) <= 10
            ORDER BY ABS(DATEDIFF(SECOND, FechaPago, @Fecha)), Id DESC", conn, tx);

                cmdBuscar.Parameters.Add("@ClienteId", SqlDbType.Int).Value = clienteId;
                cmdBuscar.Parameters.Add("@Monto", SqlDbType.Decimal).Value = anterior;
                cmdBuscar.Parameters.Add("@Fecha", SqlDbType.DateTime).Value = fechaCreacionDeuda;

                object? encontrado = cmdBuscar.ExecuteScalar();
                if (encontrado != null && encontrado != DBNull.Value)
                    pagoId = Convert.ToInt32(encontrado);
            }

            if (pagoId > 0)
            {
                if (nuevo <= 0m)
                {
                    using SqlCommand cmdBorrar = new SqlCommand(
                        "DELETE FROM Pagos WHERE Id = @Id", conn, tx);
                    cmdBorrar.Parameters.Add("@Id", SqlDbType.Int).Value = pagoId;
                    cmdBorrar.ExecuteNonQuery();
                    return;
                }

                using SqlCommand cmdActualizar = new SqlCommand(@"
            UPDATE Pagos
            SET Monto = @Monto,
                Usuario = @Usuario
            WHERE Id = @Id", conn, tx);

                cmdActualizar.Parameters.Add("@Monto", SqlDbType.Decimal).Value = nuevo;
                cmdActualizar.Parameters.Add("@Usuario", SqlDbType.VarChar, 100).Value = usuario;
                cmdActualizar.Parameters.Add("@Id", SqlDbType.Int).Value = pagoId;
                cmdActualizar.ExecuteNonQuery();
                return;
            }

            if (nuevo <= 0m)
                return;

            using SqlCommand cmdInsertar = new SqlCommand(@"
            INSERT INTO Pagos (ClienteId, FechaPago, Monto, MetodoPago, Concepto, Usuario)
            VALUES (@ClienteId, GETDATE(), @Monto, @Metodo, @Concepto, @Usuario)", conn, tx);

            cmdInsertar.Parameters.Add("@ClienteId", SqlDbType.Int).Value = clienteId;
            cmdInsertar.Parameters.Add("@Monto", SqlDbType.Decimal).Value = nuevo;
            cmdInsertar.Parameters.Add("@Metodo", SqlDbType.VarChar, 50).Value =
                string.IsNullOrWhiteSpace(metodoPago) ? "Efectivo" : metodoPago;
            cmdInsertar.Parameters.Add("@Concepto", SqlDbType.VarChar, 200).Value =
                "Pago inicial corregido (edición de deuda)";
            cmdInsertar.Parameters.Add("@Usuario", SqlDbType.VarChar, 100).Value = usuario;
            cmdInsertar.ExecuteNonQuery();
        }

        /// <summary>
        /// Alinea el movimiento PAGO del historial de membresía con el pago inicial corregido,
        /// sin crear un movimiento nuevo (evita que parezca otra renovación).
        /// </summary>
        private static void AjustarHistorialMembresia(
            SqlConnection conn,
            SqlTransaction tx,
            int clienteId,
            int? planId,
            decimal anterior,
            decimal nuevo,
            DateTime fechaCreacionDeuda,
            decimal saldoResultante)
        {
            using SqlCommand cmd = new SqlCommand(@"
            UPDATE HistorialMembresias
            SET Monto = @MontoNuevo,
                Nota = @Nota
            WHERE Id = (
                SELECT TOP 1 Id
                FROM HistorialMembresias
                WHERE ClienteId = @ClienteId
                  AND TipoMovimiento = 'PAGO'
                  AND ABS(Monto - @MontoAnterior) < 0.01
                  AND ABS(DATEDIFF(MINUTE, Fecha, @Fecha)) <= 10
                  AND (@PlanId IS NULL OR PlanId = @PlanId)
                ORDER BY ABS(DATEDIFF(SECOND, Fecha, @Fecha)), Id DESC
            )", conn, tx);

            cmd.Parameters.Add("@MontoNuevo", SqlDbType.Decimal).Value = nuevo;
            cmd.Parameters.Add("@Nota", SqlDbType.VarChar, 255).Value =
                $"Financiamiento - Inicial: ${nuevo:N2}, Saldo: ${saldoResultante:N2} (corregido, antes ${anterior:N2})";
            cmd.Parameters.Add("@ClienteId", SqlDbType.Int).Value = clienteId;
            cmd.Parameters.Add("@MontoAnterior", SqlDbType.Decimal).Value = anterior;
            cmd.Parameters.Add("@Fecha", SqlDbType.DateTime).Value = fechaCreacionDeuda;
            cmd.Parameters.Add("@PlanId", SqlDbType.Int).Value =
                planId.HasValue ? planId.Value : (object)DBNull.Value;

            cmd.ExecuteNonQuery();
        }

        private static string ObtenerNombreCliente(SqlConnection conn, SqlTransaction tx, int clienteId)
        {
            using SqlCommand cmd = new SqlCommand(
                "SELECT Nombre FROM Clientes WHERE ID = @Id", conn, tx);
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = clienteId;

            object? nombre = cmd.ExecuteScalar();
            string texto = nombre == null || nombre == DBNull.Value
                ? string.Empty
                : Convert.ToString(nombre)?.Trim() ?? string.Empty;

            return texto.Length > 0 ? texto : $"#{clienteId}";
        }

        /// <summary>
        /// Resume en una sola línea (máx. 255) qué cambió en la deuda, para el historial.
        /// </summary>
        private static string ConstruirDescripcionEdicion(
            SqlConnection conn,
            SqlTransaction tx,
            string conceptoAnterior,
            string conceptoNuevo,
            decimal montoAnterior,
            decimal montoNuevo,
            DateTime? vencimientoAnterior,
            DateTime vencimientoNuevo,
            int? planIdAnterior,
            int? planIdNuevo,
            EdicionDeudaDTO resultado)
        {
            var cambios = new System.Collections.Generic.List<string>();

            if (resultado.PagoInicialAnterior != resultado.PagoInicialNuevo)
                cambios.Add(
                    $"Pago inicial: {resultado.PagoInicialAnterior:N2} -> {resultado.PagoInicialNuevo:N2}" +
                    (resultado.ReversoCaja ? " (reverso en caja)" : string.Empty));

            if (montoAnterior != montoNuevo)
                cambios.Add($"Financiado: {montoAnterior:N2} -> {montoNuevo:N2}");

            if (vencimientoAnterior?.Date != vencimientoNuevo.Date)
                cambios.Add(
                    $"Fecha límite: {(vencimientoAnterior.HasValue ? vencimientoAnterior.Value.ToString("dd/MM/yyyy") : "sin fecha")}" +
                    $" -> {vencimientoNuevo:dd/MM/yyyy}");

            if (planIdAnterior != planIdNuevo)
                cambios.Add(
                    $"Plan: {NombrePlan(conn, tx, planIdAnterior)} -> {NombrePlan(conn, tx, planIdNuevo)}");

            if (!string.Equals(conceptoAnterior, conceptoNuevo, StringComparison.Ordinal))
                cambios.Add($"Concepto: {conceptoAnterior} -> {conceptoNuevo}");

            string detalle = cambios.Count > 0
                ? string.Join(" | ", cambios)
                : "Sin cambios en los importes";

            string descripcion = "Edición de deuda: " + detalle;

            if (string.Equals(resultado.Estado, "PAGADA", StringComparison.OrdinalIgnoreCase))
                descripcion += " | Queda SALDADA";

            return descripcion.Length > 255 ? descripcion.Substring(0, 255) : descripcion;
        }

        private static string NombrePlan(SqlConnection conn, SqlTransaction tx, int? planId)
        {
            if (!planId.HasValue)
                return "Producto a crédito / otro";

            using SqlCommand cmd = new SqlCommand(
                "SELECT Nombre FROM Planes WHERE Id = @Id", conn, tx);
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = planId.Value;

            object? nombre = cmd.ExecuteScalar();
            return nombre == null || nombre == DBNull.Value
                ? $"Plan #{planId.Value}"
                : Convert.ToString(nombre)?.Trim() ?? $"Plan #{planId.Value}";
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
            // Pendientes reales: ACTIVA con saldo > 0 (mismo criterio que Estado Clientes y WhatsApp).
            string condicionEstado = soloActivas ? "WHERE d.Estado = 'ACTIVA' AND d.Saldo > 0" : string.Empty;
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
                SELECT SUM(CASE WHEN h.TipoMovimiento = 'PAGO_INICIAL'
                                THEN h.Monto ELSE -h.Monto END) AS PagoInicial
                FROM HistorialDeudas h
                WHERE h.DeudaId = d.Id
                  AND h.TipoMovimiento IN ('PAGO_INICIAL', 'REVERSO_PAGO_INICIAL')
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
                SELECT SUM(CASE WHEN h.TipoMovimiento = 'PAGO_INICIAL'
                                THEN h.Monto ELSE -h.Monto END) AS PagoInicial
                FROM HistorialDeudas h
                WHERE h.DeudaId = d.Id
                  AND h.TipoMovimiento IN ('PAGO_INICIAL', 'REVERSO_PAGO_INICIAL')
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
        /// Estado de cuenta del miembro: todos los financiamientos pendientes
        /// (membresia y producto a credito) con la fecha en que se pactó cada uno.
        /// </summary>
        public DataTable ObtenerResumenDeudasCliente(int clienteId)
        {
            string query = @"
                SELECT
                    d.Id,
                    d.Concepto,
                    ISNULL(p.Nombre, '') AS [Plan],
                    d.MontoTotal,
                    d.MontoPagado,
                    d.Saldo,
                    d.FechaCreacion,
                    d.FechaVencimiento,
                    ISNULL(pi.PagoInicial, 0) AS PagoInicial
                FROM Deudas d
                LEFT JOIN Planes p ON p.Id = d.PlanId
                OUTER APPLY (
                    SELECT SUM(CASE WHEN h.TipoMovimiento = 'PAGO_INICIAL'
                                    THEN h.Monto ELSE -h.Monto END) AS PagoInicial
                    FROM HistorialDeudas h
                    WHERE h.DeudaId = d.Id
                      AND h.TipoMovimiento IN ('PAGO_INICIAL', 'REVERSO_PAGO_INICIAL')
                ) pi
                WHERE d.ClienteId = @ClienteId
                  AND d.Estado = 'ACTIVA'
                  AND d.Saldo > 0
                ORDER BY d.FechaCreacion ASC";

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

        /// <summary>
        /// Contexto de precio legítimo por deuda (membresía / producto) para enriquecer historial.
        /// </summary>
        public DataTable ObtenerContextoFinanciamientoPorDeudas(IReadOnlyCollection<int> deudaIds)
        {
            var dt = new DataTable();
            if (deudaIds == null || deudaIds.Count == 0)
                return dt;

            string ids = string.Join(",", deudaIds);
            string query = $@"
                SELECT d.Id AS DeudaId,
                       d.MembresiaId,
                       d.Concepto,
                       d.MontoTotal AS SaldoDeuda,
                       ISNULL((
                           SELECT SUM(CASE
                               WHEN h.TipoMovimiento = 'PAGO_INICIAL' THEN h.Monto
                               WHEN h.TipoMovimiento = 'REVERSO_PAGO_INICIAL' THEN -h.Monto
                               ELSE 0 END)
                           FROM HistorialDeudas h
                           WHERE h.DeudaId = d.Id
                       ), 0) AS PagoInicialFinanciamiento
                FROM Deudas d
                WHERE d.Id IN ({ids})";

            return db.ExecuteQuery(query);
        }
    }
}