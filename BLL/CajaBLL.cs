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

        public decimal IngresosHoy()
        {
            // Ingresos vigentes: el ingreso original de un reverso deja de contar
            // (marca REVERSO (Ref #id)), igual que los paneles y el cuadre.
            return SumarIngresosNetosCajaAbierta();
        }

        public decimal EgresosHoy()
        {
            // Solo gastos operativos. Los egresos de reverso (corrección de pago
            // inicial / deshacer) no inflan el panel de Gastos.
            return SumarGastosOperativosCajaAbierta();
        }

        public decimal BalanceActual()
        {
            return ObtenerMontoInicial() + IngresosHoy() - EgresosHoy();
        }

        private decimal SumarIngresosNetosCajaAbierta()
        {
            DataTable dt = MovimientosHoy();
            if (dt.Rows.Count == 0)
                return 0m;

            var idsRevertidos = new System.Collections.Generic.HashSet<int>();
            foreach (DataRow row in dt.Rows)
            {
                string concepto = row["Concepto"]?.ToString() ?? string.Empty;
                string metodo = dt.Columns.Contains("MetodoPago")
                    ? row["MetodoPago"]?.ToString() ?? string.Empty
                    : string.Empty;

                if (!CajaConceptoHelper.EsReverso(concepto, metodo))
                    continue;

                if (TryExtraerIdReverso(concepto, out int refId))
                    idsRevertidos.Add(refId);
            }

            decimal total = 0m;
            foreach (DataRow row in dt.Rows)
            {
                if (!string.Equals(row["TipoMovimiento"]?.ToString(), "INGRESO", StringComparison.OrdinalIgnoreCase))
                    continue;

                string concepto = row["Concepto"]?.ToString() ?? string.Empty;
                if (concepto.StartsWith("REVERSO (Ref #", StringComparison.OrdinalIgnoreCase))
                    continue;

                int id = Convert.ToInt32(row["Id"]);
                if (idsRevertidos.Contains(id))
                    continue;

                total += Convert.ToDecimal(row["Monto"]);
            }

            return total;
        }

        private decimal SumarGastosOperativosCajaAbierta()
        {
            DataTable dt = MovimientosHoy();
            decimal total = 0m;

            foreach (DataRow row in dt.Rows)
            {
                if (!string.Equals(row["TipoMovimiento"]?.ToString(), "EGRESO", StringComparison.OrdinalIgnoreCase))
                    continue;

                string concepto = row["Concepto"]?.ToString() ?? string.Empty;
                string metodo = dt.Columns.Contains("MetodoPago")
                    ? row["MetodoPago"]?.ToString() ?? string.Empty
                    : string.Empty;

                if (CajaConceptoHelper.EsReverso(concepto, metodo))
                    continue;

                total += Convert.ToDecimal(row["Monto"]);
            }

            return total;
        }

        private static bool TryExtraerIdReverso(string concepto, out int movimientoId)
        {
            movimientoId = 0;
            const string marca = "REVERSO (Ref #";
            if (!concepto.StartsWith(marca, StringComparison.OrdinalIgnoreCase))
                return false;

            int start = marca.Length;
            int end = concepto.IndexOf(')', start);
            if (end <= start)
                return false;

            return int.TryParse(concepto.AsSpan(start, end - start), out movimientoId) && movimientoId > 0;
        }
    }
}