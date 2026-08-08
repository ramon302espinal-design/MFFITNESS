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
            string user = ResolveUsuario(usuario);
            var caja = cajaDAL.ObtenerCajaAbierta(user);

            if (caja != null)
                throw new Exception("Ya tienes una caja abierta.");

            cajaDAL.AbrirCaja(montoInicial, user);
        }

        public bool ObtenerEstadoCaja()
            => ObtenerCajaAbiertaHoy() != null;

        // ===============================
        // REGISTRAR INGRESO / EGRESO (UI manual via CajaCommandService)
        // ===============================
        public int RegistrarIngresoConId(string concepto, decimal monto, string? usuario = null)
        {
            if (string.IsNullOrWhiteSpace(concepto))
                throw new Exception("Concepto requerido");

            if (monto <= 0)
                throw new Exception("Monto inválido");

            string user = ResolveUsuario(usuario);
            DataRow? caja = cajaDAL.ObtenerCajaAbierta(user);
            if (caja == null)
                throw new Exception("No hay caja abierta para registrar ingresos.");

            int cajaId = Convert.ToInt32(caja["Id"]);
            return cajaDAL.InsertarMovimiento(cajaId, "INGRESO", concepto, monto, user);
        }

        public int RegistrarEgresoConId(string concepto, decimal monto, string? usuario = null)
        {
            if (string.IsNullOrWhiteSpace(concepto))
                throw new Exception("Concepto requerido");

            if (monto <= 0)
                throw new Exception("Monto inválido");

            DataRow? caja = ObtenerCajaAbiertaHoy(usuario);
            if (caja == null)
                throw new CajaNoAbiertaException();

            int cajaId = Convert.ToInt32(caja["Id"]);
            string user = ResolveUsuario(usuario);

            return cajaDAL.InsertarMovimiento(cajaId, "EGRESO", concepto, monto, user);
        }

        private static string ResolveUsuario(string? usuario)
        {
            if (!string.IsNullOrWhiteSpace(usuario))
                return usuario.Trim();

            if (!string.IsNullOrWhiteSpace(Sesion.Usuario))
                return Sesion.Usuario.Trim();

            return "ADMIN";
        }

        // ===============================
        // OBTENER CAJA ABIERTA (del usuario en sesión)
        // ===============================
        public DataRow? ObtenerCajaAbiertaHoy(string? usuario = null)
            => cajaDAL.ObtenerCajaAbierta(ResolveUsuario(usuario));

        // ===============================
        // MOVIMIENTOS DEL DÍA
        // ===============================
        public DataTable MovimientosHoy()
        {
            DataRow? caja = ObtenerCajaAbiertaHoy();
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
            DataRow? caja = ObtenerCajaAbiertaHoy();
            return caja != null ? Convert.ToDecimal(caja["MontoInicial"]) : 0;
        }

        public decimal IngresosHoy()
        {
            return SumarMovimientosCajaAbierta("INGRESO");
        }

        public decimal EgresosHoy()
        {
            return SumarMovimientosCajaAbierta("EGRESO");
        }

        public decimal BalanceActual()
        {
            return ObtenerMontoInicial() + IngresosHoy() - EgresosHoy();
        }

        private decimal SumarMovimientosCajaAbierta(string tipo)
        {
            DataTable dt = MovimientosHoy();
            decimal total = 0;

            foreach (DataRow row in dt.Rows)
            {
                if (string.Equals(row["TipoMovimiento"]?.ToString(), tipo, StringComparison.OrdinalIgnoreCase))
                    total += Convert.ToDecimal(row["Monto"]);
            }

            return total;
        }
    }
}
