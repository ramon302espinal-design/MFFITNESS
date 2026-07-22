using DTO;
using DL;
using BLL.Models;
using BLL.Services;
using System;
using CORE;

namespace BLL
{
    public class MembresiaBLL
    {
        private readonly MembresiaDAL dal = new MembresiaDAL();
        private readonly CajaDAL cajaDAL = new CajaDAL();
        private readonly PlanDAL planDAL = new PlanDAL();
        private readonly ClienteDAL clienteDAL = new ClienteDAL();
        private readonly MensajeAutomaticoBLL mensajeBLL = new MensajeAutomaticoBLL();
        private readonly DeudaBLL deudaBLL = new DeudaBLL();


        // ===============================
        // DESACTIVAR MIEMBRO (membresía + historial en una transacción)
        // ===============================
        public int DesactivarMiembro(
            int clienteId,
            string usuario,
            string motivo,
            ModoDesactivacionMiembro modo = ModoDesactivacionMiembro.SinMembresia)
        {
            if (clienteId <= 0) throw new Exception("Cliente no válido");

            if (string.IsNullOrWhiteSpace(usuario))
                usuario = "ADMIN";

            if (string.IsNullOrWhiteSpace(motivo))
                motivo = "Sin especificar";

            bool marcarComoVencido = modo == ModoDesactivacionMiembro.Vencido;
            return dal.DesactivarMiembro(clienteId, usuario, motivo.Trim(), marcarComoVencido);
        }


        // ===============================
        // CREAR MEMBRESÍA
        // ===============================
        public void CrearMembresia(MembresiaDTO dto)
        {
            var plan = planDAL.ObtenerPlan(dto.PlanId);

            if (dto.FechaFin <= dto.FechaInicio)
                throw new Exception("Fechas inválidas");

            if (plan == null)
                throw new Exception("Plan no encontrado");

            // 🔥 VALIDACIÓN CRÍTICA: No permitir si ya tiene membresía activa
            ValidarMembresiaActiva(dto.ClienteId);

            // 🔹 REGISTRAR NUEVA
            dal.RegistrarMembresia(
                dto.ClienteId,
                dto.PlanId,
                plan.Precio,
                dto.FechaInicio,
                dto.FechaFin,
                "ADMIN"
            );
        }


        // ===============================
        // ACTUALIZAR MEMBRESÍAS VENCIDAS
        // ===============================
        public void ActualizarVencimientos()
        {
            dal.ActualizarVencidas();
        } 
        // ===============================
        // 🔥 ALERTAS
        // ===============================
        public int ObtenerVencenHoy()
        {
            return dal.ClientesVencenHoy();
        }

        public int ObtenerVencidos()
        {
            return dal.ClientesVencidos();
        }

        // ===============================
        // VALIDACIÓN CENTRALIZADA DE MEMBRESÍA ACTIVA
        // ===============================
        public bool TieneMembresiaActiva(int clienteId) =>
            clienteId > 0 && dal.TieneMembresiaActiva(clienteId);

        /// <summary>
        /// True si hay historial de plan vencido y no hay membresía vigente (para renovación).
        /// Consulta ligera: no carga todo el grid de Estado Clientes.
        /// </summary>
        public bool TieneMembresiaVencidaSinActiva(int clienteId) =>
            clienteId > 0
            && !dal.TieneMembresiaActiva(clienteId)
            && dal.TieneMembresiaVencida(clienteId);

        /// <summary>
        /// True si el cliente ya tiene membresía ACTIVA (pagada o financiada vigente).
        /// El financiamiento (activación a crédito) aplica a VENCIDO / DESACTIVADO / sin plan.
        /// </summary>
        public bool ClienteNoElegibleParaFinanciamiento(int clienteId, out string motivo)
        {
            motivo = string.Empty;
            if (clienteId <= 0)
            {
                motivo = "Cliente inválido.";
                return true;
            }

            if (!dal.TieneMembresiaActiva(clienteId))
                return false;

            var membresia = dal.ObtenerMembresiaActiva(clienteId);
            var cliente = clienteDAL.ObtenerClientePorId(clienteId);
            string nombre = cliente?["Nombre"]?.ToString() ?? "El cliente";
            string plan = membresia?["Plan"]?.ToString() ?? "su plan actual";
            string vence = membresia != null
                ? Convert.ToDateTime(membresia["FechaFin"]).ToString("dd/MM/yyyy")
                : "-";

            motivo =
                $"{nombre} ya tiene una membresía activa/paga ({plan}).\n\n" +
                $"Vence: {vence}\n\n" +
                "El financiamiento solo aplica a clientes sin membresía vigente\n" +
                "(vencidos, desactivados o sin plan).\n" +
                "Use renovación o espere a que el plan actual venza.";
            return true;
        }

        private void ValidarMembresiaActiva(int clienteId)
        {
            if (!dal.TieneMembresiaActiva(clienteId))
                return;

            var membresiaActiva = dal.ObtenerMembresiaActiva(clienteId);
            var cliente = clienteDAL.ObtenerClientePorId(clienteId);
            string nombreCliente = cliente != null ? cliente["Nombre"]?.ToString() ?? "Cliente" : "Cliente";

            if (membresiaActiva != null)
            {
                string nombrePlan = membresiaActiva["Plan"]?.ToString() ?? "Desconocido";
                DateTime fechaFin = Convert.ToDateTime(membresiaActiva["FechaFin"]);

                throw new Exception(
                    $"{nombreCliente} (#{clienteId}) ya está activo con el plan {nombrePlan}.\n\n" +
                    $"Fecha de vencimiento: {fechaFin:dd/MM/yyyy}\n\n" +
                    "Si este no es el cliente correcto, vuelva a seleccionarlo en la lista.");
            }

            throw new Exception($"{nombreCliente} ya tiene una membresía activa.");
        }

        public MembresiaOperacionResult PagarMembresiaCompleta(
            int clienteId,
            int planId,
            decimal? montoOverride,
            DateTime? fechaVencimientoOverride,
            string metodoPago,
            string? conceptoMembresia,
            string usuario)
        {
            deudaBLL.ValidarSinDeudaPendienteParaMembresia(clienteId);
            ValidarMembresiaActiva(clienteId);
            // Si quedó Activa=1 en membresía vencida, liberar índice UX_Cliente_Activa.
            dal.LiberarMarcadasActivas(clienteId);

            var plan = planDAL.ObtenerPlan(planId);
            if (plan == null) throw new Exception("Plan no encontrado.");

            DateTime inicio = DateTime.Now;
            DateTime fin = MembresiaHelper.CalcularFechaVencimiento(inicio);
            decimal monto = montoOverride ?? plan.Precio;
            string concepto = conceptoMembresia ?? $"Membresía {plan.Nombre}";
            DateTime fechaVencimientoPago = fechaVencimientoOverride ?? fin;

            int membresiaId = dal.RegistrarMembresiaConId(
                clienteId, planId, plan.Precio, inicio, fin, usuario, "Inscripción");

            try
            {
                var pagoBLL = new PagoBLL();
                var (pagoId, cajaMovId) = pagoBLL.RegistrarPagoConResultado(
                    clienteId,
                    inicio,
                    fechaVencimientoPago,
                    monto,
                    metodoPago,
                    concepto,
                    usuario);

                var result = new MembresiaOperacionResult
                {
                    MembresiaId = membresiaId,
                    PagoId = pagoId,
                    CajaMovimientoId = cajaMovId,
                    FechaFinMembresia = fin
                };

                // WhatsApp/PDF se envian despues del cobro (UI en segundo plano)
                // para no congelar la pantalla de pagos.
                return result;
            }
            catch
            {
                dal.DesactivarMembresiaPorId(membresiaId);
                throw;
            }
        }

        public void RevertirPagoMembresiaCompleta(MembresiaOperacionResult operacion, string usuario)
        {
            if (operacion.PagoId > 0)
            {
                var pagoBLL = new PagoBLL();
                pagoBLL.RevertirPagoMembresia(operacion.PagoId, operacion.CajaMovimientoId, usuario);
            }

            if (operacion.MembresiaId > 0)
                dal.DesactivarMembresiaPorId(operacion.MembresiaId);
        }

        public MembresiaOperacionResult VenderMembresiaFinanciadaConResultado(
            int clienteId,
            int planId,
            decimal pagoInicial,
            string metodoPago,
            string conceptoPago,
            string usuario,
            DateTime? fechaVencimientoDeuda = null)
        {
            if (ClienteNoElegibleParaFinanciamiento(clienteId, out string motivoFinanciamiento))
                throw new Exception(motivoFinanciamiento);

            deudaBLL.ValidarSinDeudaPendienteParaMembresia(clienteId);

            var plan = planDAL.ObtenerPlan(planId);
            if (plan == null) throw new Exception("Plan no encontrado.");

            if (pagoInicial < 0 || pagoInicial > plan.Precio)
                throw new Exception("Pago inicial inválido.");

            DateTime inicio = DateTime.Now;
            DateTime fin = MembresiaHelper.CalcularFechaVencimiento(inicio);
            decimal saldo = plan.Precio - pagoInicial;

            if (saldo > 0)
            {
                if (!fechaVencimientoDeuda.HasValue)
                    throw new Exception("La fecha límite de pago es obligatoria cuando hay saldo pendiente.");

                if (fechaVencimientoDeuda.Value.Date < DateTime.Today)
                    throw new Exception("La fecha límite de pago no puede ser anterior a hoy.");
            }

            if (pagoInicial > 0)
            {
                if (string.IsNullOrWhiteSpace(metodoPago))
                    throw new Exception("Método de pago requerido.");
                if (string.IsNullOrWhiteSpace(conceptoPago))
                    throw new Exception("Concepto de pago requerido.");
            }

            var result = new MembresiaOperacionResult
            {
                FechaFinMembresia = fin
            };

            var txService = new CajaTransaccionService();
            var deudaDAL = new DeudaDAL();
            var pagoDAL = new PagoDAL();
            var historialDAL = new HistorialMembresiaDAL();
            string conceptoCaja = $"Pago Cliente {clienteId} - {conceptoPago}";
            string notaHistorial =
                $"Financiamiento - Inicial: ${pagoInicial:N2}, Saldo: ${saldo:N2}";

            // Membresía + deuda (+ pago inicial/caja) + historial en una sola TX.
            // El historial PAGO anula SALIDA (DESACTIVADO → ACTIVO en Estado Clientes).
            txService.Ejecutar((conn, tx) =>
            {
                result.MembresiaId = dal.CrearMembresiaConId(
                    conn,
                    tx,
                    new MembresiaDTO
                    {
                        ClienteId = clienteId,
                        PlanId = planId,
                        FechaInicio = inicio,
                        FechaFin = fin,
                        Estado = "Financiado"
                    },
                    plan.Precio,
                    usuario);

                if (saldo > 0)
                {
                    result.DeudaId = deudaDAL.InsertarDeudaMembresia(
                        conn,
                        tx,
                        clienteId,
                        result.MembresiaId,
                        planId,
                        $"Saldo plan {plan.Nombre}",
                        saldo,
                        fechaVencimientoDeuda!.Value.Date,
                        usuario,
                        plan.Precio,
                        pagoInicial);
                }

                if (pagoInicial > 0)
                {
                    result.PagoId = pagoDAL.RegistrarPagoConId(
                        conn,
                        tx,
                        clienteId,
                        inicio,
                        fin,
                        pagoInicial,
                        metodoPago,
                        conceptoPago,
                        usuario);

                    result.CajaMovimientoId = txService.RegistrarIngresoConId(
                        conn,
                        tx,
                        pagoInicial,
                        conceptoCaja,
                        usuario);
                }

                historialDAL.Registrar(
                    conn,
                    tx,
                    clienteId,
                    "PAGO",
                    planId,
                    pagoInicial,
                    usuario,
                    notaHistorial);
            });

            if (result.DeudaId > 0)
                AppEventos.DeudaModificada();

            EnviarWhatsAppEnBackground(() =>
                EnviarWhatsAppFinanciamiento(clienteId, planId, pagoInicial, fechaVencimientoDeuda, result));

            return result;
        }

        public void RevertirMembresiaFinanciada(MembresiaOperacionResult operacion, string usuario)
        {
            if (operacion.PagoId > 0)
            {
                var pagoBLL = new PagoBLL();
                pagoBLL.RevertirPagoMembresia(operacion.PagoId, operacion.CajaMovimientoId, usuario);
            }

            if (operacion.DeudaId > 0)
            {
                var deudaBLLLocal = new DeudaBLL();
                deudaBLLLocal.AnularDeuda(operacion.DeudaId, usuario);
            }

            if (operacion.MembresiaId > 0)
                dal.DesactivarMembresiaPorId(operacion.MembresiaId);
        }

        // ===============================
        // VENDER MEMBRESÍA FINANCIADA
        // ===============================
        private static void EnviarWhatsAppEnBackground(Action envio)
        {
            // Enviar en segundo plano pero registrar fallos en disco (la UI solo abría el PDF).
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    envio();
                }
                catch (Exception ex)
                {
                    try
                    {
                        string log = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            "MFFITNESS",
                            "whatsapp-last-error.txt");
                        Directory.CreateDirectory(Path.GetDirectoryName(log)!);
                        File.WriteAllText(log, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\n{ex}");
                    }
                    catch
                    {
                        // ignore
                    }

                    System.Diagnostics.Debug.WriteLine($"Error WhatsApp (background): {ex.Message}");
                }
            });
        }

        public string EnviarWhatsAppTrasPagoMembresia(
            int clienteId,
            int planId,
            decimal monto,
            DateTime fechaPago,
            DateTime fechaVencimiento,
            string metodoPago,
            int pagoId)
        {
            var plan = planDAL.ObtenerPlan(planId);
            if (plan == null)
                return "Plan no encontrado para WhatsApp.";

            try
            {
                return EnviarWhatsAppPagoMembresia(
                    clienteId, plan, monto, fechaPago, fechaVencimiento, metodoPago, pagoId);
            }
            catch (Exception ex)
            {
                return "Error WhatsApp: " + ex.Message;
            }
        }

        private string EnviarWhatsAppPagoMembresia(
            int clienteId,
            PlanDTO plan,
            decimal monto,
            DateTime fechaPago,
            DateTime fechaVencimiento,
            string metodoPago,
            int pagoId)
        {
            string numeroRecibo = pagoId > 0
                ? $"MF-{pagoId}"
                : $"MF-{clienteId}-{fechaPago:yyyyMMddHHmm}";

            return mensajeBLL.EnviarFacturaMembresia(
                clienteId,
                plan.Nombre ?? "Membresia",
                monto,
                fechaPago,
                fechaVencimiento,
                numeroRecibo,
                metodoPago,
                pagoId);
        }

        private void EnviarWhatsAppFinanciamiento(
            int clienteId,
            int planId,
            decimal pagoInicial,
            DateTime? fechaVencimientoDeuda,
            MembresiaOperacionResult result)
        {
            var plan = planDAL.ObtenerPlan(planId);
            if (plan == null)
                return;

            decimal saldo = plan.Precio - pagoInicial;
            DateTime fin = result.FechaFinMembresia;

            if (saldo > 0)
            {
                mensajeBLL.EnviarMensajeFinanciamiento(
                    clienteId,
                    plan.Nombre ?? string.Empty,
                    plan.Precio,
                    pagoInicial,
                    saldo,
                    fechaVencimientoDeuda ?? fin);
                return;
            }

            if (pagoInicial > 0 && result.PagoId > 0)
            {
                EnviarWhatsAppPagoMembresia(
                    clienteId,
                    plan,
                    pagoInicial,
                    DateTime.Now,
                    fin,
                    "Efectivo",
                    result.PagoId);
            }
        }
    }
}