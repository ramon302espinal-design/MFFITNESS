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
        private readonly MensajeAutomaticoBLL mensajeBLL = new MensajeAutomaticoBLL();
        private readonly DeudaBLL deudaBLL = new DeudaBLL();

        public RenovacionOperacionResult RenovarClienteConResultado(int clienteId, int planId, decimal precio, string usuario)
        {
            deudaBLL.ValidarSinDeudaPendienteParaMembresia(clienteId);

            var result = new RenovacionOperacionResult();

            txService.Ejecutar((conn, tx) =>
            {
                var plan = planDAL.ObtenerPlan(conn, tx, planId);

                if (plan == null)
                    throw new Exception("Plan no válido.");

                // Evitar dos membresías ACTIVA: cerrar las previas en la misma TX.
                result.MembresiasCerradas = membresiaDAL.CerrarMembresiasActivas(conn, tx, clienteId);

                DateTime inicio = CORE.TimeZoneHelper.NowDominicanRepublic();
                DateTime fin = MembresiaHelper.CalcularFechaVencimiento(inicio);

                result.MembresiaId = membresiaDAL.CrearMembresiaConId(conn, tx, new MembresiaDTO
                {
                    ClienteId = clienteId,
                    PlanId = planId,
                    FechaInicio = inicio,
                    FechaFin = fin
                }, precio, usuario);

                result.PagoId = pagoDAL.RegistrarPagoConId(conn, tx,
                    clienteId,
                    CORE.TimeZoneHelper.NowDominicanRepublic(),
                    fin,
                    precio,
                    "EFECTIVO",
                    "Renovación de membresía",
                    usuario);

                result.CajaMovimientoId = txService.RegistrarIngresoConId(conn, tx,
                    precio,
                    $"Renovación cliente {clienteId}",
                    usuario);

                result.FechaFinMembresia = fin;

                HistorialMembresiaDAL.Registrar(conn, tx,
                    clienteId,
                    "RENOVACION",
                    planId,
                    precio,
                    usuario,
                    "Renovación de membresía");
            });

            // WhatsApp fuera de la transacción SQL (no bloquear el cobro 4-12s).
            try
            {
                var plan = planDAL.ObtenerPlan(planId);
                DateTime inicio = CORE.TimeZoneHelper.NowDominicanRepublic();
                DateTime fin = MembresiaHelper.CalcularFechaVencimiento(inicio);
                string numeroRecibo = result.PagoId > 0
                    ? $"MF-{result.PagoId}"
                    : $"MF-{clienteId}-{inicio:yyyyMMddHHmm}";

                System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        mensajeBLL.EnviarFacturaMembresia(
                            clienteId,
                            plan?.Nombre ?? "Membresia",
                            precio,
                            inicio,
                            fin,
                            numeroRecibo,
                            "EFECTIVO",
                            result.PagoId);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error WhatsApp renovación (bg): {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error preparando WhatsApp renovación: {ex.Message}");
            }

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
        }
    }
}