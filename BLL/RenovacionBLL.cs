using Microsoft.Data.SqlClient;
using System;
using CORE;
using DL;
using BLL.Services;
using BLL.Models;
using DTO;

namespace BLL
{
    public class RenovacionBLL
    {
        private readonly CajaTransaccionService txService = new CajaTransaccionService();
        private readonly MembresiaDAL membresiaDAL = new MembresiaDAL();
        private readonly PagoDAL pagoDAL = new PagoDAL();
        private readonly HistorialMembresiaDAL HistorialMembresiaDAL = new HistorialMembresiaDAL();
        private readonly PlanDAL planDAL = new PlanDAL();
        private readonly DeudaBLL deudaBLL = new DeudaBLL();

        public RenovacionOperacionResult RenovarClienteConResultado(
            int clienteId,
            int planId,
            decimal precio,
            string usuario,
            string? conceptoRenovacion = null,
            string? metodoPago = null)
        {
            deudaBLL.ValidarSinDeudaPendienteParaMembresia(clienteId);

            if (precio < 0)
                throw new Exception("El monto no puede ser negativo.");

            bool esCortesiaCero = precio == 0;
            string conceptoPago = string.IsNullOrWhiteSpace(conceptoRenovacion)
                ? "Renovación de membresía"
                : conceptoRenovacion.Trim();
            string conceptoHistorial = conceptoPago.Length > 200
                ? conceptoPago.Substring(0, 200)
                : conceptoPago;
            // Misma convención que membresía/POS: "Efectivo" / "Transferencia".
            string metodo = string.IsNullOrWhiteSpace(metodoPago) ? "Efectivo" : metodoPago.Trim();

            var result = new RenovacionOperacionResult();

            txService.Ejecutar((conn, tx) =>
            {
                var plan = planDAL.ObtenerPlan(conn, tx, planId);

                if (plan == null)
                    throw new Exception("Plan no válido.");

                result.MembresiasCerradas = membresiaDAL.CerrarMembresiasActivas(conn, tx, clienteId);

                DateTime inicio = CORE.TimeZoneHelper.NowDominicanRepublic();
                DateTime fin = MembresiaHelper.CalcularFechaVencimiento(inicio);

                decimal precioMembresia = plan.Precio > 0 ? plan.Precio : precio;

                result.MembresiaId = membresiaDAL.CrearMembresiaConId(conn, tx, new MembresiaDTO
                {
                    ClienteId = clienteId,
                    PlanId = planId,
                    FechaInicio = inicio,
                    FechaFin = fin
                }, precioMembresia, usuario);

                result.PagoId = pagoDAL.RegistrarPagoConId(conn, tx,
                    clienteId,
                    CORE.TimeZoneHelper.NowDominicanRepublic(),
                    fin,
                    precio,
                    metodo,
                    conceptoPago,
                    usuario);

                if (!esCortesiaCero)
                {
                    string? nombreCliente = new ClienteDAL().ObtenerClientePorId(clienteId)?["Nombre"]?.ToString();
                    result.CajaMovimientoId = txService.RegistrarIngresoConId(conn, tx,
                        precio,
                        CajaConceptoHelper.IngresoRenovacion(clienteId, nombreCliente),
                        usuario,
                        metodo,
                        clienteId);
                }

                result.FechaFinMembresia = fin;

                HistorialMembresiaDAL.Registrar(conn, tx,
                    clienteId,
                    "RENOVACION",
                    planId,
                    precio,
                    usuario,
                    conceptoHistorial);

                new CongelacionDAL().CerrarActiva(conn, tx, clienteId, DateTime.Today);
            });

            MovimientoFinancieroNotifier.PagoConCaja();

            return result;
        }

        /// <summary>Compat: renovación estándar sin concepto personalizado.</summary>
        public RenovacionOperacionResult RenovarClienteConResultado(int clienteId, int planId, decimal precio, string usuario)
            => RenovarClienteConResultado(clienteId, planId, precio, usuario, conceptoRenovacion: null);

        /// <summary>
        /// Renovación con financiamiento (mismo efecto que Pagos financiado):
        /// cierra plan vigente, crea membresía, registra pago inicial en caja/historial
        /// y deja el saldo como deuda ACTIVA vinculada al plan.
        /// </summary>
        public RenovacionOperacionResult RenovarClienteFinanciadoConResultado(
            int clienteId,
            int planId,
            decimal pagoInicial,
            string metodoPago,
            string usuario,
            DateTime fechaVencimientoDeuda)
        {
            deudaBLL.ValidarSinDeudaPendienteParaMembresia(clienteId);

            var plan = planDAL.ObtenerPlan(planId);
            if (plan == null)
                throw new Exception("Plan no válido.");

            if (PlanNombres.EsOferta(plan.Nombre))
                throw new Exception("El plan OFERTA no admite financiamiento.");

            if (PlanNombres.EsParcial(plan.Nombre))
                throw new Exception("ATLETA y VISITA no admiten financiamiento; cobre el monto completo.");

            if (pagoInicial < 0 || pagoInicial > plan.Precio)
                throw new Exception("Pago inicial inválido.");

            decimal saldo = Math.Round(plan.Precio - pagoInicial, 2, MidpointRounding.AwayFromZero);
            if (saldo <= 0)
                throw new Exception("Para financiamiento el pago inicial debe ser menor al precio del plan.");

            if (fechaVencimientoDeuda.Date < DateTime.Today)
                throw new Exception("La fecha límite de pago no puede ser anterior a hoy.");

            if (pagoInicial > 0 && string.IsNullOrWhiteSpace(metodoPago))
                throw new Exception("Método de pago requerido.");

            string metodo = string.IsNullOrWhiteSpace(metodoPago) ? "Efectivo" : metodoPago.Trim();
            string conceptoPago = $"Pago inicial - Renovación {plan.Nombre}";
            string notaHistorial =
                $"Renovación financiada - Inicial: ${pagoInicial:N2}, Saldo: ${saldo:N2}";

            var result = new RenovacionOperacionResult
            {
                PlanId = planId,
                PlanNombre = plan.Nombre,
                PagoInicial = pagoInicial,
                SaldoPendiente = saldo,
                FechaLimiteDeuda = fechaVencimientoDeuda.Date,
                EsFinanciada = true
            };

            var deudaDAL = new DeudaDAL();
            var clienteDAL = new ClienteDAL();

            txService.Ejecutar((conn, tx) =>
            {
                result.MembresiasCerradas = membresiaDAL.CerrarMembresiasActivas(conn, tx, clienteId);

                DateTime inicio = TimeZoneHelper.NowDominicanRepublic();
                DateTime fin = MembresiaHelper.CalcularFechaVencimiento(inicio);
                result.FechaPago = inicio;
                result.FechaFinMembresia = fin;

                result.MembresiaId = membresiaDAL.CrearMembresiaConId(
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

                result.DeudaId = deudaDAL.InsertarDeudaMembresia(
                    conn,
                    tx,
                    clienteId,
                    result.MembresiaId,
                    planId,
                    $"Saldo plan {plan.Nombre}",
                    saldo,
                    fechaVencimientoDeuda.Date,
                    usuario,
                    plan.Precio,
                    pagoInicial);

                if (pagoInicial > 0)
                {
                    result.PagoId = pagoDAL.RegistrarPagoConId(
                        conn,
                        tx,
                        clienteId,
                        inicio,
                        fin,
                        pagoInicial,
                        metodo,
                        conceptoPago,
                        usuario);

                    string? nombreCliente = clienteDAL.ObtenerClientePorId(clienteId)?["Nombre"]?.ToString();
                    result.CajaMovimientoId = txService.RegistrarIngresoConId(
                        conn,
                        tx,
                        pagoInicial,
                        CajaConceptoHelper.IngresoPagoInicialFinanciado(clienteId, nombreCliente, conceptoPago),
                        usuario,
                        metodo,
                        clienteId);
                }

                HistorialMembresiaDAL.Registrar(
                    conn,
                    tx,
                    clienteId,
                    "RENOVACION",
                    planId,
                    pagoInicial,
                    usuario,
                    notaHistorial);

                new CongelacionDAL().CerrarActiva(conn, tx, clienteId, DateTime.Today);
            });

            MovimientoFinancieroNotifier.MembresiaFinanciada(result.CajaMovimientoId, result.DeudaId);

            new MembresiaBLL().NotificarWhatsAppFinanciamiento(
                clienteId,
                planId,
                pagoInicial,
                fechaVencimientoDeuda.Date,
                metodo,
                new MembresiaOperacionResult
                {
                    MembresiaId = result.MembresiaId,
                    PagoId = result.PagoId,
                    CajaMovimientoId = result.CajaMovimientoId,
                    DeudaId = result.DeudaId,
                    PlanId = planId,
                    PlanNombre = plan.Nombre,
                    FechaPago = result.FechaPago,
                    FechaFinMembresia = result.FechaFinMembresia
                });

            return result;
        }

        public void RevertirRenovacion(RenovacionOperacionResult operacion, string usuario)
        {
            if (operacion.PagoId > 0)
            {
                var pagoBLL = new PagoBLL();
                pagoBLL.RevertirPagoMembresia(operacion.PagoId, operacion.CajaMovimientoId, usuario);
            }

            if (operacion.MembresiaId > 0)
                membresiaDAL.DesactivarMembresiaPorId(operacion.MembresiaId);

            // Restaurar membresías que se cerraron al renovar.
            if (operacion.MembresiasCerradas != null)
            {
                foreach (var previa in operacion.MembresiasCerradas)
                {
                    if (previa.Id > 0)
                        membresiaDAL.ReactivarMembresiaPorId(previa.Id, previa.FechaFin);
                }
            }

            MovimientoFinancieroNotifier.EstadoMembresia();
        }
    }
}