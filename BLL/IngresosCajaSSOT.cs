using DL;
using System;

namespace BLL
{
    /// <summary>
    /// SSOT de ingresos y egresos de caja (Fase 6).
    /// Ingresos netos del día/mes: suma INGRESO vigente en DetalleCaja (sin movimientos reversados).
    /// Balance del turno: MontoInicial + ingresos sesión − gastos operativos sesión.
    /// No entra a ingresos: ventas financiadas sin abono; saldo de deuda pendiente.
    /// </summary>
    public static class IngresosCajaSSOT
    {
        public static decimal IngresosNetosDelDia(DateTime? fecha = null)
        {
            var dal = new CajaDAL();
            return dal.ObtenerTotalIngresosPorFecha(fecha ?? DateTime.Today);
        }

        public static decimal IngresosNetosMesActual()
        {
            return new CajaDAL().ObtenerTotalIngresosMesActual();
        }

        public static decimal IngresosNetosSesion(int cajaId)
        {
            return new CierreCajaDAL().ObtenerTotalPorCaja(cajaId, "INGRESO");
        }

        public static decimal EgresosOperativosSesion(int cajaId)
        {
            return new CierreCajaDAL().ObtenerTotalPorCaja(cajaId, "EGRESO");
        }

        /// <summary>Ingresos netos vigentes en un rango inclusive (SSOT reportes CRM/CAJA).</summary>
        public static decimal IngresosNetosPorRango(DateTime desde, DateTime hasta)
        {
            return new CajaDAL().ObtenerTotalIngresosPorRango(desde, hasta);
        }

        /// <summary>Gastos operativos (sin reversos) en un rango inclusive.</summary>
        public static decimal EgresosOperativosPorRango(DateTime desde, DateTime hasta)
        {
            return new CierreCajaDAL().ObtenerEgresosOperativosPorRango(desde, hasta);
        }
    }
}
