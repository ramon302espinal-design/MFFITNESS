using DL;
using CORE;
using System;
using System.Data;

namespace BLL
{
    public class CajaBLL
    {
        private CajaDAL cajaDAL = new CajaDAL();

        public void AbrirCaja(decimal montoInicial, string usuario)
            => AbrirCajaSeguro(montoInicial, usuario);

        public void AbrirCajaSeguro(decimal montoInicial, string usuario)
        {
            var caja = cajaDAL.ObtenerCajaAbierta();

            if (caja != null)
                throw new Exception("Ya hay una caja abierta.");

            cajaDAL.AbrirCaja(montoInicial, ResolveUsuario(usuario));
        }

        public bool ObtenerEstadoCaja()
            => cajaDAL.ObtenerCajaAbierta() != null;

        // ===============================
        // REGISTRAR INGRESO / EGRESO (UI manual via CajaCommandService)
        // ===============================
        public int RegistrarIngresoConId(string concepto, decimal monto, string? usuario = null)
        {
            if (string.IsNullOrWhiteSpace(concepto))
                throw new Exception("Concepto requerido");

            if (monto <= 0)
                throw new Exception("Monto inválido");

            DataRow caja = cajaDAL.ObtenerCajaAbierta();
            if (caja == null)
                throw new Exception("No hay caja abierta para registrar ingresos.");

            int cajaId = Convert.ToInt32(caja["Id"]);
            string user = ResolveUsuario(usuario);

            return cajaDAL.InsertarMovimiento(cajaId, "INGRESO", concepto, monto, user);
        }

        public int RegistrarEgresoConId(string concepto, decimal monto, string? usuario = null)
        {
            if (string.IsNullOrWhiteSpace(concepto))
                throw new Exception("Concepto requerido");

            if (monto <= 0)
                throw new Exception("Monto inválido");

            DataRow? caja = ObtenerCajaAbiertaHoy();
            if (caja == null)
                throw new CajaNoAbiertaException();

            int cajaId = Convert.ToInt32(caja["Id"]);
            string user = ResolveUsuario(usuario);

            return cajaDAL.InsertarMovimiento(cajaId, "EGRESO", concepto, monto, user);
        }

        private static string ResolveUsuario(string? usuario)
        {
            if (!string.IsNullOrWhiteSpace(usuario))
                return usuario;

            if (!string.IsNullOrWhiteSpace(Sesion.Usuario))
                return Sesion.Usuario;

            return "ADMIN";
        }

        // ===============================
        // OBTENER CAJA ABIERTA
        // ===============================
        public DataRow? ObtenerCajaAbiertaHoy()
            => cajaDAL.ObtenerCajaAbierta();

        // ===============================
        // MOVIMIENTOS DEL DÍA
        // ===============================
        public DataTable MovimientosHoy()
        {
            DataRow caja = ObtenerCajaAbiertaHoy();
            if (caja == null) return new DataTable();

            int cajaId = Convert.ToInt32(caja["Id"]);
            return cajaDAL.ObtenerMovimientos(cajaId);
        }

        // Cierre con cuadre: CajaServiceBLL.CerrarCajaConCuadre (no usar CerrarCaja aquí).

        // ===============================
        // BALANCE
        // ===============================
        public decimal ObtenerMontoInicial()
        {
            DataRow caja = ObtenerCajaAbiertaHoy();
            return caja != null ? Convert.ToDecimal(caja["MontoInicial"]) : 0;
        }

        /// <summary>Ingresos netos del calendario (todas las cajas del día). Mismo número que home.</summary>
        public decimal IngresosNetosDelDia(DateTime? fecha = null)
            => IngresosCajaSSOT.IngresosNetosDelDia(fecha);

        /// <summary>Ingresos netos del mes calendario (todas las cajas).</summary>
        public decimal IngresosNetosMes()
            => IngresosCajaSSOT.IngresosNetosMesActual();

        /// <summary>Ingresos netos de la caja abierta (turno actual).</summary>
        public decimal IngresosNetosSesion()
        {
            DataRow? caja = ObtenerCajaAbiertaHoy();
            if (caja == null)
                return 0m;

            return IngresosCajaSSOT.IngresosNetosSesion(Convert.ToInt32(caja["Id"]));
        }

        /// <summary>Gastos operativos del turno (sin reversos de corrección).</summary>
        public decimal EgresosOperativosSesion()
        {
            DataRow? caja = ObtenerCajaAbiertaHoy();
            if (caja == null)
                return 0m;

            return IngresosCajaSSOT.EgresosOperativosSesion(Convert.ToInt32(caja["Id"]));
        }

        /// <inheritdoc cref="IngresosNetosDelDia"/>
        public decimal IngresosHoy() => IngresosNetosDelDia();

        /// <inheritdoc cref="EgresosOperativosSesion"/>
        public decimal EgresosHoy() => EgresosOperativosSesion();

        public decimal BalanceActual()
        {
            return ObtenerMontoInicial() + IngresosNetosSesion() - EgresosOperativosSesion();
        }
    }
}