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
            string? conceptoRenovacion = null)
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
                    "EFECTIVO",
                    conceptoPago,
                    usuario);

                if (!esCortesiaCero)
                {
                    string? nombreCliente = new ClienteDAL().ObtenerClientePorId(clienteId)?["Nombre"]?.ToString();
                    result.CajaMovimientoId = txService.RegistrarIngresoConId(conn, tx,
                        precio,
                        CajaConceptoHelper.IngresoRenovacion(clienteId, nombreCliente),
                        usuario,
                        "EFECTIVO",
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