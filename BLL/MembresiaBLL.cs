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

        public CongelacionDTO? ObtenerCongelacionActiva(int clienteId) =>
            new CongelacionDAL().ObtenerActiva(clienteId);

        public CongelacionDTO CongelarMiembro(int clienteId, string motivo, string usuario)
        {
            if (clienteId <= 0)
                throw new Exception("Cliente no válido.");
            if (string.IsNullOrWhiteSpace(motivo))
                throw new Exception("Indique el motivo de congelamiento.");
            if (string.IsNullOrWhiteSpace(usuario))
                usuario = "ADMIN";

            var congelacionDAL = new CongelacionDAL();
            congelacionDAL.EnsureSchema();

            if (congelacionDAL.ObtenerActiva(clienteId) != null)
                throw new Exception("El cliente ya está congelado.");

            if (!dal.TieneMembresiaActiva(clienteId))
                throw new Exception("Solo se puede congelar un miembro ACTIVO.");

            var membresia = dal.ObtenerMembresiaActiva(clienteId)
                ?? throw new Exception("No se encontró la membresía activa.");

            int membresiaId = Convert.ToInt32(membresia["Id"]);
            DateTime fechaFin = Convert.ToDateTime(membresia["FechaFin"]).Date;
            DateTime hoy = CongelacionHelper.HoyPc();
            if (CongelacionHelper.EsFinDeSemana(hoy))
                throw new InvalidOperationException(
                    "El congelamiento solo se registra de lunes a viernes (no sábado ni domingo).");

            int diaAncla = CongelacionHelper.CalcularDiaAncla(hoy);
            int diasRestantes = CongelacionHelper.CalcularDiasRestantes(fechaFin, hoy);

            var dto = new CongelacionDTO
            {
                ClienteId = clienteId,
                MembresiaId = membresiaId,
                FechaCongelacion = hoy,
                DiaAncla = diaAncla,
                DiasRestantes = diasRestantes,
                FechaFinOriginal = fechaFin,
                Motivo = motivo.Trim(),
                Usuario = usuario,
                Activa = true
            };

            dto.Id = congelacionDAL.Insertar(dto);
            dal.DesactivarMembresiaPorId(membresiaId);
            new HistorialMembresiaDAL().Insertar(
                clienteId,
                "CONGELACION",
                null,
                null,
                usuario,
                motivo.Trim());

            return dto;
        }

        public DateTime ActivarMiembroCongelado(int clienteId, string usuario)
        {
            if (clienteId <= 0)
                throw new Exception("Cliente no válido.");
            if (string.IsNullOrWhiteSpace(usuario))
                usuario = "ADMIN";

            var congelacionDAL = new CongelacionDAL();
            var cong = congelacionDAL.ObtenerActiva(clienteId)
                ?? throw new Exception("El cliente no tiene un congelamiento activo.");

            DateTime hoy = CongelacionHelper.HoyPc();
            int diaAncla = cong.FechaCongelacion.Day > 0
                ? cong.FechaCongelacion.Day
                : cong.DiaAncla;

            if (!CongelacionHelper.PuedeActivarHoy(diaAncla, hoy))
            {
                var cliente = clienteDAL.ObtenerClientePorId(clienteId);
                string nombre = cliente?["Nombre"]?.ToString() ?? "El cliente";
                throw new InvalidOperationException(
                    CongelacionHelper.MensajeActivacionBloqueada(nombre, diaAncla, hoy));
            }

            DateTime nuevaFechaFin = CongelacionHelper.CalcularFechaFinAlActivar(
                hoy,
                cong.FechaFinOriginal,
                cong.DiasRestantes);
            int membresiaId = cong.MembresiaId ?? 0;
            if (membresiaId <= 0)
                throw new Exception("No se encontró la membresía congelada.");

            dal.ReactivarMembresiaPorId(membresiaId, nuevaFechaFin);
            congelacionDAL.CerrarActiva(clienteId, hoy);
            new HistorialMembresiaDAL().Insertar(
                clienteId,
                "DESCONGELACION",
                null,
                null,
                usuario,
                $"Reactivación. Días restantes: {cong.DiasRestantes}. Vence {nuevaFechaFin:dd/MM/yyyy}.");

            return nuevaFechaFin;
        }

        /// <summary>
        /// Ajuste administrativo de FechaFin (sin cobro). Actualiza Activa e inserta
        /// historial AJUSTE_FECHA para que Estado/avisos/dashboard sigan el SSOT.
        /// Solo ACTIVO/VENCIDO (no CONGELADO ni DESACTIVADO).
        /// </summary>
        public (DateTime FechaAnterior, DateTime FechaNueva) AjustarFechaFinMembresia(
            int clienteId,
            DateTime nuevaFechaFin,
            string? usuario = null)
        {
            if (clienteId <= 0)
                throw new Exception("Cliente no válido.");

            if (string.IsNullOrWhiteSpace(usuario))
                usuario = string.IsNullOrWhiteSpace(Sesion.Usuario) ? "ADMIN" : Sesion.Usuario;

            var congelacion = new CongelacionDAL().ObtenerActiva(clienteId);
            if (congelacion != null)
                throw new Exception("No se puede ajustar la fecha mientras el miembro está CONGELADO. Actívelo primero.");

            var historialDAL = new HistorialMembresiaDAL();
            string? ultimoTipo = historialDAL.ObtenerUltimoTipoMovimiento(clienteId);
            if (string.Equals(ultimoTipo, "SALIDA", StringComparison.OrdinalIgnoreCase))
                throw new Exception("No se puede ajustar la fecha de un miembro DESACTIVADO. Renueve o reactive el plan.");

            System.Data.DataRow? membresia = dal.ObtenerUltimaMembresia(clienteId)
                ?? throw new Exception("El cliente no tiene membresía registrada.");

            if (membresia["FechaInicio"] == null || membresia["FechaInicio"] == DBNull.Value)
                throw new Exception("La membresía no tiene fecha de inicio.");

            DateTime fechaInicio = Convert.ToDateTime(membresia["FechaInicio"]).Date;
            DateTime fechaAnterior = membresia["FechaFin"] == null || membresia["FechaFin"] == DBNull.Value
                ? fechaInicio
                : Convert.ToDateTime(membresia["FechaFin"]).Date;

            DateTime fechaNueva = nuevaFechaFin.Date;
            if (fechaNueva < fechaInicio)
                throw new Exception($"La fecha de vencimiento no puede ser anterior al inicio ({fechaInicio:dd/MM/yyyy}).");

            if (fechaNueva == fechaAnterior)
                throw new Exception("La fecha de vencimiento no cambió.");

            int membresiaId = Convert.ToInt32(membresia["Id"]);
            int? planId = membresia["PlanId"] == null || membresia["PlanId"] == DBNull.Value
                ? null
                : Convert.ToInt32(membresia["PlanId"]);

            bool activa = fechaNueva >= DateTime.Today;
            dal.ActualizarFechaFinMembresia(membresiaId, fechaNueva, activa);

            historialDAL.Insertar(
                clienteId,
                "AJUSTE_FECHA",
                planId,
                null,
                usuario,
                $"Vencimiento: {fechaAnterior:dd/MM/yyyy} -> {fechaNueva:dd/MM/yyyy}");

            return (fechaAnterior, fechaNueva);
        }

        /// <summary>
        /// Integra un miembro que ya pagó fuera de la app: crea membresía + historial
        /// sin movimiento de caja ni pago. FechaFin según regla de negocio desde fechaInicio.
        /// </summary>
        public MembresiaOperacionResult RegistrarMiembroYaPagado(
            int clienteId,
            int planId,
            DateTime fechaInicio,
            string? usuario = null,
            DateTime? fechaFin = null)
        {
            if (clienteId <= 0)
                throw new Exception("Seleccione un miembro válido.");
            if (planId <= 0)
                throw new Exception("Seleccione un plan válido.");

            if (string.IsNullOrWhiteSpace(usuario))
                usuario = string.IsNullOrWhiteSpace(Sesion.Usuario) ? "ADMIN" : Sesion.Usuario;

            string estado = clienteDAL.ObtenerEstadoMembresia(clienteId);
            if (string.Equals(estado, "ACTIVO", StringComparison.OrdinalIgnoreCase))
                throw new Exception("El miembro ya está ACTIVO. No se puede volver a integrar.");
            if (string.Equals(estado, "CONGELADO", StringComparison.OrdinalIgnoreCase))
                throw new Exception("El miembro está CONGELADO. Actívelo primero o use el flujo de congelación.");

            var plan = planDAL.ObtenerPlan(planId)
                ?? throw new Exception("Plan no encontrado.");

            DateTime inicio = fechaInicio.Date;
            DateTime fin = fechaFin?.Date ?? MembresiaHelper.CalcularFechaVencimiento(inicio);
            if (fin < inicio)
                throw new Exception("La fecha de vencimiento calculada no es válida.");

            // Precio de catálogo solo para KPI/historial de plan; NO se registra en caja.
            int membresiaId = dal.RegistrarMembresiaConId(
                clienteId,
                planId,
                plan.Precio,
                inicio,
                fin,
                usuario,
                "Integración");

            new CongelacionDAL().CerrarActiva(clienteId, DateTime.Today);

            new HistorialMembresiaDAL().Insertar(
                clienteId,
                "ALTA_EXISTENTE",
                planId,
                null,
                usuario,
                $"Integración sin cobro. Plan {plan.Nombre}. Vence {fin:dd/MM/yyyy}.");

            return new MembresiaOperacionResult
            {
                MembresiaId = membresiaId,
                PagoId = 0,
                CajaMovimientoId = 0,
                FechaFinMembresia = fin
            };
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
            ClienteElegibleParaRenovacion(clienteId);

        /// <summary>
        /// Misma elegibilidad que RENOVAR en Estado: VENCIDO, DESACTIVADO (baja) o plan vencido.
        /// </summary>
        public bool ClienteElegibleParaRenovacion(int clienteId) =>
            clienteId > 0
            && !dal.TieneMembresiaActiva(clienteId)
            && (dal.TieneMembresiaVencida(clienteId) || dal.TieneUltimaSalidaOBaja(clienteId));

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
            if (monto < 0)
                throw new Exception("El monto no puede ser negativo.");

            bool esOferta = PlanNombres.EsOferta(plan.Nombre);
            if (monto == 0 && !esOferta)
                throw new Exception("El monto debe ser mayor a 0.");

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
                    usuario,
                    permitirMontoCero: esOferta);

                // Historial PAGO anula SALIDA/BAJA (DESACTIVADO/VENCIDO → ACTIVO en Estado).
                new HistorialMembresiaDAL().Insertar(
                    clienteId,
                    "PAGO",
                    planId,
                    monto,
                    usuario,
                    string.IsNullOrWhiteSpace(conceptoMembresia)
                        ? "Pago de membresía"
                        : (conceptoMembresia!.Length > 200
                            ? conceptoMembresia.Substring(0, 200)
                            : conceptoMembresia));

                new CongelacionDAL().CerrarActiva(clienteId, DateTime.Today);

                var result = new MembresiaOperacionResult
                {
                    MembresiaId = membresiaId,
                    PagoId = pagoId,
                    CajaMovimientoId = cajaMovId,
                    FechaFinMembresia = fin
                };

                // WhatsApp/PDF se envían después del cobro (UI en segundo plano).
                return result;
            }
            catch
            {
                dal.DesactivarMembresiaPorId(membresiaId);
                throw;
            }
        }

        /// <summary>
        /// ATLETA / VISITA: ingreso a caja + Pagos + historial, sin fila Membresias
        /// y sin cambiar reglas de Estado (no usa TipoMovimiento PAGO).
        /// </summary>
        public MembresiaOperacionResult RegistrarPagoPlanParcial(
            int clienteId,
            int planId,
            decimal? montoOverride,
            string metodoPago,
            string? concepto,
            string usuario,
            int cantidad = 1)
        {
            if (cantidad < 1)
                throw new Exception("La cantidad debe ser al menos 1.");
            if (cantidad > 99)
                throw new Exception("La cantidad máxima por cobro es 99.");

            var plan = planDAL.ObtenerPlan(planId);
            if (plan == null)
                throw new Exception("Plan no encontrado.");

            if (!PlanNombres.EsParcial(plan.Nombre))
                throw new Exception("El plan seleccionado no es un acceso parcial (ATLETA/VISITA).");

            // Sin cliente en combo: usa VISITANTE (SISTEMA) para Pagos/Historial/Caja (FK).
            int clienteEfectivo = clienteId > 0
                ? clienteId
                : new ClienteBLL().ObtenerOCrearVisitanteSistema();

            decimal montoUnitario = montoOverride ?? plan.Precio;
            if (montoUnitario <= 0)
                throw new Exception("El monto debe ser mayor a 0.");

            if (montoOverride.HasValue && Math.Abs(montoOverride.Value - plan.Precio) > 0.009m)
                throw new Exception(
                    $"ATLETA y VISITA se cobran al precio fijo del plan (RD$ {plan.Precio:N2} c/u).");

            DateTime ahora = DateTime.Now;
            // Vencimiento del recibo = mismo día (no es vigencia de membresía).
            DateTime finDia = ahora.Date.AddDays(1).AddTicks(-1);
            string nombrePlan = plan.Nombre?.Trim() ?? PlanNombres.TipoHistorialParcial(plan.Nombre);
            string conceptoBase = string.IsNullOrWhiteSpace(concepto)
                ? $"Plan {nombrePlan}"
                : concepto.Trim();

            string tipoHistorial = PlanNombres.TipoHistorialParcial(plan.Nombre);
            string? nombreCliente = clienteDAL.ObtenerClientePorId(clienteEfectivo)?["Nombre"]?.ToString();

            var pagoDal = new PagoDAL();
            var historialDal = new HistorialMembresiaDAL();
            var txService = new CajaTransaccionService();

            int pagoId = 0;
            int cajaMovId = 0;

            txService.Ejecutar((conn, tx) =>
            {
                for (int i = 0; i < cantidad; i++)
                {
                    string conceptoPago = cantidad > 1
                        ? $"{conceptoBase} ({i + 1}/{cantidad})"
                        : conceptoBase;
                    string nota = conceptoPago.Length > 200
                        ? conceptoPago.Substring(0, 200)
                        : conceptoPago;
                    string conceptoCaja = CajaConceptoHelper.IngresoPagoMembresia(
                        clienteEfectivo, nombreCliente, conceptoPago);

                    pagoId = pagoDal.RegistrarPagoConId(
                        conn, tx,
                        clienteEfectivo,
                        ahora,
                        finDia,
                        montoUnitario,
                        metodoPago,
                        conceptoPago,
                        usuario);

                    cajaMovId = txService.RegistrarIngresoConId(
                        conn, tx,
                        montoUnitario,
                        conceptoCaja,
                        usuario,
                        metodoPago,
                        clienteEfectivo);

                    historialDal.Registrar(
                        conn, tx,
                        clienteEfectivo,
                        tipoHistorial,
                        planId,
                        montoUnitario,
                        usuario,
                        nota);
                }
            });

            return new MembresiaOperacionResult
            {
                MembresiaId = 0,
                PagoId = pagoId,
                CajaMovimientoId = cajaMovId,
                FechaFinMembresia = finDia
            };
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

            if (PlanNombres.EsParcial(plan.Nombre))
                throw new Exception("ATLETA y VISITA no admiten financiamiento; cobre el monto completo.");

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
            string? nombreCliente = clienteDAL.ObtenerClientePorId(clienteId)?["Nombre"]?.ToString();
            string conceptoCaja = CajaConceptoHelper.IngresoPagoInicialFinanciado(
                clienteId, nombreCliente, conceptoPago);
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
                        usuario,
                        metodoPago,
                        clienteId);
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

                new CongelacionDAL().CerrarActiva(conn, tx, clienteId, DateTime.Today);
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