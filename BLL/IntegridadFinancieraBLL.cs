using CORE;
using DL;
using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace BLL
{
    /// <summary>
    /// Fase 11.5 — auditoría de ventas financiadas huérfanas y desalineaciones.
    /// </summary>
    public class IntegridadFinancieraBLL
    {
        private readonly IntegridadFinancieraDAL dal = new();

        public IntegridadFinancieraReporte EjecutarAuditoria()
        {
            var reporte = new IntegridadFinancieraReporte
            {
                VentasFinanciadasHuerfanas = dal.ContarVentasFinanciadasHuerfanas(),
                DeudasVentaSinVentaId = dal.ContarDeudasProductoSinVentaId(),
                DeudasSaldoDesalineado = dal.ContarDeudasSaldoDesalineadoConVenta(),
                PagosInicialSinHistorial = dal.ContarFinanciamientosConCobroSinPagoInicial(),
                IngresosCajaSinDetalle = dal.ContarIngresosVentaSinVenta()
            };

            reporte.TotalAlertas =
                reporte.VentasFinanciadasHuerfanas
                + reporte.DeudasVentaSinVentaId
                + reporte.DeudasSaldoDesalineado
                + reporte.PagosInicialSinHistorial
                + reporte.IngresosCajaSinDetalle;

            return reporte;
        }

        public void RegistrarAuditoriaEnLog()
        {
            IntegridadFinancieraReporte r = EjecutarAuditoria();
            if (r.TotalAlertas == 0)
            {
                System.Diagnostics.Debug.WriteLine("[Integridad F11] OK — 0 alertas.");
                return;
            }

            System.Diagnostics.Debug.WriteLine(
                $"[Integridad F11] ALERTAS={r.TotalAlertas} · " +
                $"Huérfanas={r.VentasFinanciadasHuerfanas} · " +
                $"Sin Venta Id={r.DeudasVentaSinVentaId} · " +
                $"Saldo≠={r.DeudasSaldoDesalineado} · " +
                $"Sin PAGO_INICIAL={r.PagosInicialSinHistorial} · " +
                $"Caja sin venta={r.IngresosCajaSinDetalle}");
        }
    }

    public sealed class IntegridadFinancieraReporte
    {
        public int VentasFinanciadasHuerfanas { get; set; }
        public int DeudasVentaSinVentaId { get; set; }
        public int DeudasSaldoDesalineado { get; set; }
        public int PagosInicialSinHistorial { get; set; }
        public int IngresosCajaSinDetalle { get; set; }
        public int TotalAlertas { get; set; }
    }
}
