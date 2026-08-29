using BLL.Models;

using BLL.Services;

using CORE;

using DL;

using DTO;

using Microsoft.Data.SqlClient;

using System;

using System.Collections.Generic;



namespace BLL

{

    public class ProgramacionBLL

    {

        private readonly MembresiaProgramadaDAL _programadaDAL = new MembresiaProgramadaDAL();

        private readonly MembresiaDAL _membresiaDAL = new MembresiaDAL();

        private readonly PlanDAL _planDAL = new PlanDAL();

        private readonly PagoDAL _pagoDAL = new PagoDAL();

        private readonly HistorialMembresiaDAL _historialDAL = new HistorialMembresiaDAL();

        private readonly DeudaBLL _deudaBLL = new DeudaBLL();

        private readonly CajaTransaccionService _txService = new CajaTransaccionService();

        private readonly MensajeAutomaticoBLL _mensajeBLL = new MensajeAutomaticoBLL();



        public static readonly string[] PlanesPermitidos =

        {

            "PREMIUM", "PRO", "MENSUALIDAD", "M-A", "3X", "3x",

            "GLUTEOS GRANDE", "ABDOMEN PLANO"

        };



        public ProgramacionOperacionResult ProgramarMembresia(

            int clienteId,

            int planId,

            decimal monto,

            DateTime fechaInicioProgramada,

            DateTime fechaFinProgramada,

            string usuario,

            string? nota = null)

        {

            _programadaDAL.EnsureSchema();

            _deudaBLL.ValidarSinDeudaPendienteParaMembresia(clienteId);



            if (!_membresiaDAL.TieneMembresiaActiva(clienteId))

                throw new Exception("Solo se puede programar un miembro ACTIVO.");



            if (_programadaDAL.TienePendiente(clienteId))

                throw new Exception("El miembro ya tiene una programación pendiente.");



            if (monto < 0)

                throw new Exception("El monto no puede ser negativo.");



            var plan = _planDAL.ObtenerPlan(planId)

                ?? throw new Exception("Plan no válido.");



            fechaInicioProgramada = fechaInicioProgramada.Date;

            fechaFinProgramada = MembresiaHelper.CalcularFechaFinProgramacion(fechaInicioProgramada);



            if (fechaFinProgramada < fechaInicioProgramada)

                throw new Exception("La fecha fin programada no puede ser anterior al inicio.");



            var membresiaActiva = _membresiaDAL.ObtenerMembresiaActiva(clienteId)

                ?? throw new Exception("No se encontró la membresía activa.");



            DateTime venceActual = Convert.ToDateTime(membresiaActiva["FechaFin"]).Date;

            if (fechaInicioProgramada < venceActual)

                throw new Exception(

                    $"El inicio programado no puede ser anterior al vencimiento actual ({venceActual:dd/MM/yyyy}).");



            int membresiaOrigenId = Convert.ToInt32(membresiaActiva["Id"]);

            bool esCortesiaCero = monto == 0;

            string conceptoPago = "Programación de membresía";

            string conceptoHistorial = string.IsNullOrWhiteSpace(nota)

                ? $"Programado {plan.Nombre}: {fechaInicioProgramada:dd/MM/yyyy} → {fechaFinProgramada:dd/MM/yyyy}"

                : nota.Trim();

            if (conceptoHistorial.Length > 200)

                conceptoHistorial = conceptoHistorial[..200];



            var result = new ProgramacionOperacionResult

            {

                PlanId = planId,

                Monto = monto,

                FechaInicioProgramada = fechaInicioProgramada,

                FechaFinProgramada = fechaFinProgramada,

                PlanNombre = plan.Nombre

            };



            DateTime ahora = TimeZoneHelper.NowDominicanRepublic();



            _txService.Ejecutar((conn, tx) =>

            {

                result.PagoId = _pagoDAL.RegistrarPagoConId(

                    conn, tx,

                    clienteId,

                    ahora,

                    fechaFinProgramada,

                    monto,

                    "EFECTIVO",

                    conceptoPago,

                    usuario);



                if (!esCortesiaCero)

                {

                    string? nombreCliente = new ClienteDAL().ObtenerClientePorId(clienteId)?["Nombre"]?.ToString();

                    result.CajaMovimientoId = _txService.RegistrarIngresoConId(

                        conn, tx,

                        monto,

                        CajaConceptoHelper.IngresoProgramacion(clienteId, nombreCliente, plan.Nombre),

                        usuario,

                        "EFECTIVO",

                        clienteId);

                }



                var dto = new MembresiaProgramadaDTO

                {

                    ClienteId = clienteId,

                    PlanId = planId,

                    Monto = monto,

                    FechaPago = ahora,

                    FechaInicioProgramada = fechaInicioProgramada,

                    FechaFinProgramada = fechaFinProgramada,

                    MembresiaOrigenId = membresiaOrigenId,

                    Estado = "PENDIENTE",

                    Usuario = usuario,

                    PagoId = result.PagoId,

                    CajaMovimientoId = result.CajaMovimientoId > 0 ? result.CajaMovimientoId : null,

                    Nota = conceptoHistorial

                };



                result.ProgramacionId = _programadaDAL.Insertar(conn, tx, dto);



                _historialDAL.Registrar(

                    conn, tx,

                    clienteId,

                    "PROGRAMACION",

                    planId,

                    monto,

                    usuario,

                    conceptoHistorial);

            });



            return result;

        }



        /// <summary>Aplica programaciones cuya fecha de inicio ya llegó (al cargar Estado / vencimientos).</summary>

        public IReadOnlyList<ProgramacionActivadaEventArgs> AplicarProgramacionesPendientes()

        {

            _programadaDAL.EnsureSchema();

            List<MembresiaProgramadaDTO> pendientes =

                _programadaDAL.ListarPendientesParaAplicar(DateTime.Today);



            var activadas = new List<ProgramacionActivadaEventArgs>();

            foreach (var prog in pendientes)

            {

                try

                {

                    ProgramacionActivadaEventArgs? info = AplicarUna(prog);

                    if (info != null)

                        activadas.Add(info);

                }

                catch (Exception ex)

                {

                    System.Diagnostics.Debug.WriteLine(

                        $"[ProgramacionBLL] No se aplicó #{prog.Id} cliente {prog.ClienteId}: {ex.Message}");

                }

            }



            return activadas;

        }



        private ProgramacionActivadaEventArgs? AplicarUna(MembresiaProgramadaDTO prog)

        {

            int membresiaId = 0;



            _txService.Ejecutar((conn, tx) =>

            {

                _membresiaDAL.CerrarMembresiasActivas(conn, tx, prog.ClienteId);



                membresiaId = _membresiaDAL.CrearMembresiaConId(conn, tx, new MembresiaDTO

                {

                    ClienteId = prog.ClienteId,

                    PlanId = prog.PlanId,

                    FechaInicio = prog.FechaInicioProgramada,

                    FechaFin = prog.FechaFinProgramada

                }, prog.Monto, prog.Usuario ?? "ADMIN");



                string nota = $"Aplicación programación #{prog.Id}: {prog.FechaInicioProgramada:dd/MM/yyyy} → {prog.FechaFinProgramada:dd/MM/yyyy}";

                _historialDAL.Registrar(

                    conn, tx,

                    prog.ClienteId,

                    "RENOVACION",

                    prog.PlanId,

                    null,

                    prog.Usuario ?? "ADMIN",

                    nota);



                _programadaDAL.MarcarAplicada(conn, tx, prog.Id, membresiaId);

                new CongelacionDAL().CerrarActiva(conn, tx, prog.ClienteId, DateTime.Today);

            });



            if (membresiaId <= 0)

                return null;



            string planNombre = prog.PlanNombre ?? string.Empty;

            if (string.IsNullOrWhiteSpace(planNombre))

            {

                var plan = _planDAL.ObtenerPlan(prog.PlanId);

                planNombre = plan?.Nombre ?? "PLAN";

            }



            string clienteNombre = prog.ClienteNombre ?? string.Empty;

            if (string.IsNullOrWhiteSpace(clienteNombre))

            {

                clienteNombre = new ClienteDAL().ObtenerClientePorId(prog.ClienteId)?["Nombre"]?.ToString() ?? "Miembro";

            }



            EnviarWhatsAppActivacion(prog, planNombre);



            return new ProgramacionActivadaEventArgs

            {

                ProgramacionId = prog.Id,

                ClienteId = prog.ClienteId,

                MembresiaId = membresiaId,

                ClienteNombre = clienteNombre,

                PlanNombre = planNombre,

                FechaInicio = prog.FechaInicioProgramada,

                FechaFin = prog.FechaFinProgramada

            };

        }



        private void EnviarWhatsAppActivacion(MembresiaProgramadaDTO prog, string planNombre)

        {

            if (_programadaDAL.WhatsAppActivacionYaEnviada(prog.Id))

                return;



            try

            {

                bool enviado = _mensajeBLL.EnviarMensajeProgramacionActivada(

                    prog.ClienteId,

                    planNombre,

                    prog.FechaInicioProgramada,

                    prog.FechaFinProgramada,

                    prog.Id);



                if (enviado)

                    _programadaDAL.MarcarWhatsAppActivacionEnviada(prog.Id);

            }

            catch (Exception ex)

            {

                System.Diagnostics.Debug.WriteLine(

                    $"[ProgramacionBLL] WhatsApp activación #{prog.Id}: {ex.Message}");

            }

        }

    }

}


