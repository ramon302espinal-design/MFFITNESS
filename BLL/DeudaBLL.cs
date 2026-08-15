using DL;
using CORE;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLL
{
    public class DeudaBLL
    {
        private readonly DeudaDAL dal = new DeudaDAL();
        private readonly CajaDAL cajaDAL = new CajaDAL();
        private readonly MensajeAutomaticoBLL mensajeBLL = new MensajeAutomaticoBLL();
        private readonly ClienteDAL clienteDAL = new ClienteDAL();

        /// <summary>
        /// Revierte un pago de deuda previamente registrado
        /// </summary>
        /// <param name="pagoId">ID del pago a revertir</param>
        /// <param name="usuario">Usuario que realiza la reversión</param>
        /// <exception cref="Exception">Si no hay caja abierta o el pago no existe</exception>
        public void RevertirPago(int pagoId, string usuario)
        {
            var caja = cajaDAL.ObtenerCajaAbierta();

            if (caja == null)
                throw new Exception("No hay caja abierta");

            int cajaId = Convert.ToInt32(caja["Id"]);

            dal.RevertirPago(pagoId, usuario, cajaId);
        }

        // ===============================
        // CREAR DEUDA
        // ===============================
        /// <summary>
        /// Crea una nueva deuda para un cliente y envía notificación WhatsApp
        /// </summary>
        /// <param name="clienteId">ID del cliente deudor</param>
        /// <param name="concepto">Descripción de la deuda</param>
        /// <param name="monto">Monto total de la deuda</param>
        /// <param name="vencimiento">Fecha de vencimiento</param>
        /// <param name="usuario">Usuario que registra la deuda</param>
        /// <exception cref="Exception">Si el cliente o monto son inválidos</exception>
        public int CrearDeudaConId(int clienteId, string concepto, decimal monto, DateTime vencimiento, string usuario)
        {
            if (clienteId <= 0) throw new Exception("Cliente inválido");
            if (monto <= 0) throw new Exception("El monto de la deuda debe ser mayor a cero.");

            ValidarDeudaDuplicada(clienteId, concepto, monto);

            int deudaId = dal.InsertarDeuda(clienteId, concepto, monto, vencimiento, usuario);

            // WhatsApp fuera del hilo de UI/cobro (crear deuda o venta a crédito).
            int clienteIdBg = clienteId;
            string conceptoBg = concepto;
            decimal montoBg = monto;
            DateTime vencimientoBg = vencimiento;
            int deudaIdBg = deudaId;
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    EnviarNotificacionDeudaCreada(clienteIdBg, conceptoBg, montoBg, vencimientoBg, deudaIdBg);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error WhatsApp deuda creada (bg): {ex.Message}");
                }
            });

            AppEventos.DeudaModificada();
            return deudaId;
        }

        // ===============================
        // REGISTRAR PAGO
        // ===============================
        /// <summary>
        /// Registra un pago hacia una deuda existente y envía notificación WhatsApp
        /// </summary>
        /// <param name="deudaId">ID de la deuda</param>
        /// <param name="monto">Monto del pago</param>
        /// <param name="metodoPago">Método de pago (EFECTIVO, TARJETA, etc.)</param>
        /// <param name="usuario">Usuario que registra el pago</param>
        /// <exception cref="Exception">Si la deuda o monto son inválidos, o no hay caja abierta</exception>
        /// <remarks>
        /// Envía notificación de pago parcial si queda saldo, o felicitación si se paga completa
        /// </remarks>
        public int RegistrarPagoConId(int deudaId, decimal monto, string metodoPago, string usuario)
        {
            if (deudaId <= 0)
                throw new Exception("Deuda inválida");

            if (monto <= 0)
                throw new Exception("El monto del pago debe ser mayor a cero.");

            var caja = cajaDAL.ObtenerCajaAbierta();

            if (caja == null)
                throw new Exception("No hay caja abierta para registrar pagos.");

            int cajaId = Convert.ToInt32(caja["Id"]);

            decimal saldoAnterior = dal.ObtenerSaldoPorDeuda(deudaId);
            int clienteId = dal.ObtenerClientePorDeuda(deudaId);

            if (monto > saldoAnterior)
                throw new Exception($"El monto del pago (RD${monto:N2}) no puede ser mayor al saldo pendiente (RD${saldoAnterior:N2}).");

            int pagoId = dal.RegistrarPagoCompleto(deudaId, monto, metodoPago, usuario, cajaId);

            decimal saldoNuevo = saldoAnterior - monto;
            int clienteIdCapture = clienteId;
            int deudaIdCapture = deudaId;
            decimal montoCapture = monto;
            decimal saldoAnteriorCapture = saldoAnterior;

            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    if (saldoNuevo <= 0)
                        EnviarNotificacionDeudaPagadaCompleta(clienteIdCapture, saldoAnteriorCapture, deudaIdCapture);
                    else
                        EnviarNotificacionPagoRecibido(clienteIdCapture, montoCapture, saldoNuevo, deudaIdCapture);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error WhatsApp pago deuda (bg): {ex.Message}");
                }
            });

            AppEventos.DeudaModificada();
            return pagoId;
        }

        public void AnularDeuda(int deudaId, string usuario)
        {
            if (deudaId <= 0)
                throw new Exception("Deuda inválida");

            dal.AnularDeuda(deudaId, usuario);
            AppEventos.DeudaModificada();
        }

        /// <summary>
        /// Obtiene todas las deudas del sistema
        /// </summary>
        /// <returns>DataTable con todas las deudas y sus datos</returns>
        public DataTable ObtenerDeudas()
        {
            var dt = dal.ObtenerDeudas();
            EnriquecerAporteInicial(dt);
            EnriquecerFechasPlanMembresia(dt);
            return dt;
        }

        /// <summary>
        /// Datos listos para reporte PDF de deudas activas (una fila por deuda).
        /// </summary>
        public DataTable ObtenerDatosReporteDeudas()
        {
            DataTable origen = dal.ObtenerDeudasParaReporte();
            DateTime hoy = DateTime.Today;

            var porClienteActivas = new Dictionary<int, (int Cantidad, decimal Monto)>();
            var porClienteVencidas = new Dictionary<int, int>();

            foreach (DataRow row in origen.Rows)
            {
                int clienteId = Convert.ToInt32(row["ClienteId"]);
                decimal saldo = row["Saldo"] == DBNull.Value ? 0m : Convert.ToDecimal(row["Saldo"]);
                DateTime? venc = row["FechaVencimiento"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(row["FechaVencimiento"]).Date;

                if (!porClienteActivas.ContainsKey(clienteId))
                    porClienteActivas[clienteId] = (0, 0m);

                var act = porClienteActivas[clienteId];
                porClienteActivas[clienteId] = (act.Cantidad + 1, act.Monto + saldo);

                if (venc.HasValue && venc.Value < hoy)
                {
                    porClienteVencidas.TryGetValue(clienteId, out int vencidas);
                    porClienteVencidas[clienteId] = vencidas + 1;
                }
            }

            var reporte = new DataTable();
            reporte.Columns.Add("NombreDelDeudor", typeof(string));
            reporte.Columns.Add("Telefono", typeof(string));
            reporte.Columns.Add("Direccion", typeof(string));
            reporte.Columns.Add("DeudasActivas", typeof(int));
            reporte.Columns.Add("MontoDeudasActivas", typeof(decimal));
            reporte.Columns.Add("MontoPendiente", typeof(decimal));
            reporte.Columns.Add("DeudasVencidas", typeof(int));
            reporte.Columns.Add("FechaHoraDeuda", typeof(DateTime));
            reporte.Columns.Add("PagoInicial", typeof(decimal));
            reporte.Columns.Add("FechaAVencer", typeof(DateTime));
            reporte.Columns.Add("Concepto", typeof(string));

            foreach (DataRow row in origen.Rows)
            {
                int clienteId = Convert.ToInt32(row["ClienteId"]);
                porClienteActivas.TryGetValue(clienteId, out var resumen);
                porClienteVencidas.TryGetValue(clienteId, out int vencidas);

                decimal saldoPendiente = row["Saldo"] == DBNull.Value
                    ? 0m
                    : Convert.ToDecimal(row["Saldo"]);

                decimal pagoInicial = 0m;
                if (row.Table.Columns.Contains("PagoInicialFinanciamiento") &&
                    row["PagoInicialFinanciamiento"] != DBNull.Value)
                    pagoInicial = Convert.ToDecimal(row["PagoInicialFinanciamiento"]);

                DateTime fechaCreacion = row["FechaCreacion"] == DBNull.Value
                    ? DateTime.Now
                    : Convert.ToDateTime(row["FechaCreacion"]);

                DateTime fechaVence = row["FechaVencimiento"] == DBNull.Value
                    ? DateTime.Today
                    : Convert.ToDateTime(row["FechaVencimiento"]);

                reporte.Rows.Add(
                    row["Nombre"]?.ToString() ?? "Cliente",
                    row["Telefono"]?.ToString() ?? "",
                    row["Direccion"]?.ToString() ?? "",
                    resumen.Cantidad,
                    resumen.Monto,
                    saldoPendiente,
                    vencidas,
                    fechaCreacion,
                    pagoInicial,
                    fechaVence,
                    row["Concepto"]?.ToString() ?? "");
            }

            return reporte;
        }

        private static void EnriquecerAporteInicial(DataTable dt)
        {
            if (dt == null) return;

            if (!dt.Columns.Contains("AporteInicial"))
                dt.Columns.Add("AporteInicial", typeof(string));

            bool tienePagoInicial = dt.Columns.Contains("PagoInicialFinanciamiento");

            foreach (DataRow row in dt.Rows)
            {
                if (row["MembresiaId"] == DBNull.Value || row["MembresiaId"] == null)
                {
                    row["AporteInicial"] = "-";
                    continue;
                }

                decimal pagoInicial = 0;
                if (tienePagoInicial && row["PagoInicialFinanciamiento"] != DBNull.Value)
                    pagoInicial = Convert.ToDecimal(row["PagoInicialFinanciamiento"]);

                row["AporteInicial"] = pagoInicial > 0
                    ? $"Sí ({pagoInicial:N2})"
                    : "No ($0.00)";
            }
        }

        /// <summary>
        /// Corrige fechas de plan truncadas por desactivaciones (FechaFin = GETDATE()).
        /// </summary>
        private static void EnriquecerFechasPlanMembresia(DataTable dt)
        {
            if (dt == null) return;
            if (!dt.Columns.Contains("FechaInicioMembresia") || !dt.Columns.Contains("FechaFinMembresia"))
                return;

            foreach (DataRow row in dt.Rows)
            {
                if (row["MembresiaId"] == DBNull.Value || row["MembresiaId"] == null)
                    continue;

                if (row["FechaInicioMembresia"] == DBNull.Value || row["FechaInicioMembresia"] == null)
                    continue;

                DateTime inicio = Convert.ToDateTime(row["FechaInicioMembresia"]).Date;
                DateTime? finActual = row["FechaFinMembresia"] == DBNull.Value || row["FechaFinMembresia"] == null
                    ? null
                    : Convert.ToDateTime(row["FechaFinMembresia"]).Date;

                row["FechaFinMembresia"] = MembresiaHelper.NormalizarFechaFinPlan(inicio, finActual);
            }
        }

        // ===============================
        // DASHBOARD
        // ===============================

        /// <summary>
        /// Obtiene el número de deudas activas en el sistema
        /// </summary>
        /// <returns>Cantidad de deudas activas</returns>
        public int DeudasActivas()
        {
            return dal.DeudasActivas();
        }

        /// <summary>
        /// Obtiene el número de deudas vencidas en el sistema
        /// </summary>
        /// <returns>Cantidad de deudas activas que ya vencieron</returns>
        public int DeudasVencidas()
        {
            return dal.DeudasVencidas();
        }

        /// <summary>
        /// Calcula el ingreso total pendiente por deudas activas
        /// </summary>
        /// <returns>Suma de todos los saldos de deudas activas</returns>
        public decimal IngresoPendiente()
        {
            return dal.IngresoPendiente();
        }

        /// <summary>
        /// True si el cliente tiene deuda ACTIVA con saldo &gt; 0 (módulo Deudas).
        /// </summary>
        public bool TieneDeudasActivas(int clienteId) =>
            clienteId > 0 && dal.TieneDeudasActivas(clienteId);

        /// <summary>
        /// Bloquea compra/renovación de plan si el cliente tiene deuda de MEMBRESÍA con saldo pendiente.
        /// Las deudas de venta (producto a crédito) no bloquean: solo generan aviso
        /// (ver <see cref="TieneAvisoDeudaProducto"/>).
        /// </summary>
        public bool ClienteBloqueadoPorDeudaPendiente(int clienteId, out string motivo)
        {
            motivo = string.Empty;
            if (clienteId <= 0)
                return false;

            var deudas = ObtenerDeudasActivas(clienteId, deMembresia: true);
            if (deudas.Count == 0)
                return false;

            string nombre = ObtenerNombreCliente(clienteId);
            decimal total = 0m;
            var lineas = new System.Text.StringBuilder();

            foreach (DataRow row in deudas)
            {
                decimal saldo = LeerSaldo(row);
                total += saldo;

                string plan = row["Plan"]?.ToString()?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(plan) || plan.Equals("N/A", StringComparison.OrdinalIgnoreCase))
                    plan = row["Concepto"]?.ToString()?.Trim() ?? "plan financiado";

                string concepto = row["Concepto"]?.ToString()?.Trim() ?? string.Empty;
                if (deudas.Count == 1)
                {
                    motivo =
                        $"{nombre} ya tiene registrado el plan {plan} y tiene una deuda pendiente de RD$ {saldo:N2}.\n\n" +
                        (string.IsNullOrWhiteSpace(concepto) ? string.Empty : $"Concepto: {concepto}\n\n") +
                        "Debe saldar esa deuda en el módulo de Deudas antes de renovar o comprar un plan.";
                    return true;
                }

                lineas.AppendLine($"• Plan {plan}: RD$ {saldo:N2}");
            }

            motivo =
                $"{nombre} ya tiene planes registrados con deuda pendiente:\n\n" +
                $"{lineas}\n" +
                $"Deuda total: RD$ {total:N2}\n\n" +
                "Debe saldar esas deudas en el módulo de Deudas antes de renovar o comprar un plan.";
            return true;
        }

        /// <summary>
        /// Aviso informativo (NO bloquea) cuando el cliente financió una venta
        /// mediante "producto a crédito" y aún tiene saldo pendiente.
        /// </summary>
        public bool TieneAvisoDeudaProducto(int clienteId, out string aviso)
        {
            aviso = string.Empty;
            if (clienteId <= 0)
                return false;

            var deudas = ObtenerDeudasActivas(clienteId, deMembresia: false);
            if (deudas.Count == 0)
                return false;

            string nombre = ObtenerNombreCliente(clienteId);

            if (deudas.Count == 1)
            {
                DataRow row = deudas[0];
                decimal saldo = LeerSaldo(row);
                string concepto = row["Concepto"]?.ToString()?.Trim() ?? string.Empty;
                string detalle = string.IsNullOrWhiteSpace(concepto) ? "un producto a credito" : concepto;
                string abonado = DescribirAbono(row);

                aviso =
                    $"{nombre} ya tiene registrado {detalle} y tiene una deuda pendiente de RD$ {saldo:N2}.\n\n" +
                    (string.IsNullOrWhiteSpace(concepto) ? string.Empty : $"Concepto: {concepto}\n") +
                    (string.IsNullOrWhiteSpace(abonado) ? string.Empty : $"{abonado}\n") +
                    "\nDebe saldar esa deuda.";
                return true;
            }

            decimal total = 0m;
            var lineas = new System.Text.StringBuilder();
            foreach (DataRow row in deudas)
            {
                decimal saldo = LeerSaldo(row);
                total += saldo;

                string concepto = row["Concepto"]?.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(concepto))
                    concepto = "producto a credito";

                string abonado = DescribirAbono(row);
                lineas.Append($"• {concepto}: RD$ {saldo:N2}");
                lineas.AppendLine(string.IsNullOrWhiteSpace(abonado) ? string.Empty : $" ({abonado})");
            }

            aviso =
                $"{nombre} ya tiene productos a credito con deuda pendiente:\n\n" +
                $"{lineas}\n" +
                $"Deuda total: RD$ {total:N2}\n\n" +
                "Debe saldar esa deuda.";
            return true;
        }

        /// <summary>
        /// Detalle del abono para que el saldo del aviso sea verificable
        /// contra el módulo de Deudas (total original menos lo ya pagado).
        /// </summary>
        private static string DescribirAbono(DataRow row)
        {
            if (!row.Table.Columns.Contains("MontoPagado") || !row.Table.Columns.Contains("MontoTotal"))
                return string.Empty;

            decimal pagado = row["MontoPagado"] != DBNull.Value ? Convert.ToDecimal(row["MontoPagado"]) : 0m;
            if (pagado <= 0)
                return string.Empty;

            decimal montoTotal = row["MontoTotal"] != DBNull.Value ? Convert.ToDecimal(row["MontoTotal"]) : 0m;
            return $"total RD$ {montoTotal:N2} - abonado RD$ {pagado:N2}";
        }

        /// <summary>
        /// Deudas activas del cliente separadas por origen: membresía (financiamiento de plan)
        /// o venta (producto a crédito). Sin columnas de origen se asume membresía (fail-closed).
        /// </summary>
        private List<DataRow> ObtenerDeudasActivas(int clienteId, bool deMembresia)
        {
            var resultado = new List<DataRow>();
            var deudas = dal.ObtenerDeudasActivasCliente(clienteId);
            if (deudas == null || deudas.Rows.Count == 0)
                return resultado;

            foreach (DataRow row in deudas.Rows)
            {
                if (EsDeudaDeMembresia(row) == deMembresia)
                    resultado.Add(row);
            }

            return resultado;
        }

        private static bool EsDeudaDeMembresia(DataRow row)
        {
            bool tieneColumnasOrigen =
                row.Table.Columns.Contains("PlanId") && row.Table.Columns.Contains("MembresiaId");
            if (!tieneColumnasOrigen)
                return true;

            if (TieneReferencia(row, "PlanId") || TieneReferencia(row, "MembresiaId"))
                return true;

            // Deudas legacy de plan que se insertaron sin PlanId/MembresiaId: siguen bloqueando.
            string concepto = row["Concepto"]?.ToString() ?? string.Empty;
            return concepto.IndexOf("membres", StringComparison.OrdinalIgnoreCase) >= 0
                || concepto.IndexOf("financiamiento", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool TieneReferencia(DataRow row, string columna)
        {
            object valor = row[columna];
            return valor != DBNull.Value && Convert.ToInt32(valor) > 0;
        }

        private static decimal LeerSaldo(DataRow row) =>
            row["Saldo"] != DBNull.Value ? Convert.ToDecimal(row["Saldo"]) : 0m;

        private string ObtenerNombreCliente(int clienteId)
        {
            var cliente = clienteDAL.ObtenerClientePorId(clienteId);
            string nombre = cliente?["Nombre"]?.ToString()?.Trim() ?? string.Empty;
            return string.IsNullOrWhiteSpace(nombre) ? "El cliente" : nombre;
        }

        /// <summary>
        /// Lanza excepción si el cliente no puede comprar/renovar por deuda pendiente.
        /// </summary>
        public void ValidarSinDeudaPendienteParaMembresia(int clienteId)
        {
            if (ClienteBloqueadoPorDeudaPendiente(clienteId, out string motivo))
                throw new Exception(motivo);
        }

        /// <summary>
        /// Verifica si un cliente tiene deudas vencidas
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <returns>True si tiene deudas vencidas, false en caso contrario</returns>
        public bool TieneDeudasVencidas(int clienteId)
        {
            return dal.TieneDeudasVencidas(clienteId);
        }

        /// <summary>
        /// Obtiene el monto total de deudas vencidas de un cliente
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <returns>Monto total adeudado y vencido</returns>
        public decimal ObtenerMontoDeudasVencidas(int clienteId)
        {
            return dal.ObtenerMontoDeudasVencidas(clienteId);
        }

        /// <summary>
        /// Obtiene el total cobrado en pagos de deudas el día de hoy
        /// </summary>
        /// <returns>Monto total cobrado hoy</returns>
        public decimal TotalCobradoHoy()
        {
            return dal.TotalCobradoHoy();
        }

        /// <summary>
        /// Obtiene el total cobrado en pagos de deudas en el mes actual
        /// </summary>
        /// <returns>Monto total cobrado este mes</returns>
        public decimal TotalCobradoEsteMes()
        {
            return dal.TotalCobradoEsteMes();
        }

        /// <summary>
        /// Obtiene la cantidad de clientes únicos con deudas activas
        /// </summary>
        /// <returns>Cantidad de clientes con deuda</returns>
        public int ClientesConDeuda()
        {
            return dal.ClientesConDeuda();
        }

        // ===============================
        // 📱 NOTIFICACIONES WHATSAPP - DEUDAS
        // ===============================

        /// <summary>
        /// Envía notificación cuando se crea una deuda nueva
        /// </summary>
        private void EnviarNotificacionDeudaCreada(int clienteId, string concepto, decimal monto, DateTime fechaVencimiento, int? deudaId = null)
        {
            try
            {
                mensajeBLL.EnviarNotificacionDeudaCreada(clienteId, concepto, monto, fechaVencimiento, deudaId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error enviando WhatsApp deuda creada: {ex.Message}");
            }
        }

        /// <summary>
        /// Envía recordatorio de vencimiento próximo (5 días antes por defecto)
        /// </summary>
        public bool EnviarRecordatorioVencimiento(int deudaId)
        {
            try
            {
                return mensajeBLL.EnviarRecordatorioDeuda(deudaId, forzar: true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error recordatorio vencimiento: {ex.Message}");
                return false;
            }
        }

        public bool EnviarRecordatorioVenceHoy(int deudaId)
        {
            try
            {
                return mensajeBLL.EnviarRecordatorioDeudaVenceHoy(deudaId, forzar: true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error recordatorio vence hoy: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Envía notificación cuando una deuda ya venció
        /// </summary>
        public bool EnviarNotificacionDeudaVencida(int deudaId)
        {
            try
            {
                return mensajeBLL.EnviarNotificacionDeudaVencida(deudaId, forzar: true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error notificación deuda vencida: {ex.Message}");
                return false;
            }
        }

        private void EnviarNotificacionPagoRecibido(int clienteId, decimal montoPago, decimal saldoRestante, int deudaId)
        {
            try
            {
                mensajeBLL.EnviarNotificacionPagoDeudaRecibido(clienteId, montoPago, saldoRestante, deudaId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error confirmación pago: {ex.Message}");
            }
        }

        private void EnviarNotificacionDeudaPagadaCompleta(int clienteId, decimal montoTotal, int deudaId)
        {
            try
            {
                mensajeBLL.EnviarNotificacionDeudaPagadaCompleta(clienteId, montoTotal, deudaId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error felicitación deuda pagada: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifica deudas próximas a vencer y vencidas, envía notificaciones automáticas
        /// </summary>
        public int VerificarYEnviarNotificaciones()
        {
            try
            {
                int enviados = mensajeBLL.EjecutarAutomatizaciones();

                var membresiaBLL = new MembresiaBLL();
                membresiaBLL.ActualizarVencimientos();

                return enviados;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error verificando notificaciones: {ex.Message}");
                return 0;
            }
        }

        public string? ObtenerUltimoErrorWhatsApp(int clienteId) =>
            mensajeBLL.ObtenerUltimoErrorCliente(clienteId);

        // ===============================
        // 🔧 MÉTODOS AUXILIARES PRIVADOS
        // ===============================

        /// <summary>
        /// Obtiene una deuda específica por su ID
        /// </summary>
        /// <param name="deudaId">ID de la deuda</param>
        /// <returns>DataRow con los datos de la deuda o null si no existe</returns>
        private DataRow? ObtenerDeudaPorId(int deudaId)
        {
            var deudas = dal.ObtenerDeudas();
            foreach (DataRow row in deudas.Rows)
            {
                if (Convert.ToInt32(row["Id"]) == deudaId)
                {
                    return row;
                }
            }
            return null;
        }

        /// <summary>
        /// Valida que no exista una deuda duplicada (mismo cliente, concepto y monto en el mismo día)
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="concepto">Concepto de la deuda</param>
        /// <param name="monto">Monto de la deuda</param>
        /// <exception cref="Exception">Si se detecta una deuda duplicada</exception>
        private void ValidarDeudaDuplicada(int clienteId, string concepto, decimal monto)
        {
            var deudas = dal.ObtenerDeudas();
            DateTime hoy = DateTime.Now.Date;

            foreach (DataRow row in deudas.Rows)
            {
                int rowClienteId = Convert.ToInt32(row["ClienteId"]);
                string rowConcepto = row["Concepto"]?.ToString() ?? "";
                decimal rowMonto = Convert.ToDecimal(row["MontoTotal"]);
                DateTime rowFecha = Convert.ToDateTime(row["FechaCreacion"]).Date;

                if (rowClienteId == clienteId &&
                    rowConcepto.Equals(concepto, StringComparison.OrdinalIgnoreCase) &&
                    rowMonto == monto &&
                    rowFecha == hoy)
                {
                    throw new Exception($"Ya existe una deuda registrada para este cliente con el mismo concepto y monto el día de hoy. Evite duplicados.");
                }
            }
        }
    }
}