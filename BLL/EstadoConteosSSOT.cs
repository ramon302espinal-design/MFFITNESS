using System;
using System.Data;
using DL;

namespace BLL
{
    /// <summary>
    /// Fase 9 — conteos de estado de clientes (home dashboard = grid Estado Clientes).
    /// Fuente: <see cref="EstadoDAL.ObtenerConteosDashboard"/> / <see cref="MembresiaEstadoSql"/>.
    /// </summary>
    public static class EstadoConteosSSOT
    {
        public static (int Activos, int Vencidos, int Congelados, int Desactivados) Obtener()
            => new EstadoDAL().ObtenerConteosDashboard();

        /// <summary>Recuenta el DataTable completo del grid (sin filtro de búsqueda).</summary>
        public static (int Activos, int Vencidos, int Congelados, int Desactivados) ContarDesdeTabla(DataTable? tabla)
        {
            if (tabla == null || !tabla.Columns.Contains("Estado"))
                return (0, 0, 0, 0);

            int activos = 0, vencidos = 0, congelados = 0, desactivados = 0;
            foreach (DataRow row in tabla.Rows)
            {
                string est = Convert.ToString(row["Estado"])?.Trim() ?? string.Empty;
                if (EstadoBLL.EsEstadoActivoVigente(est))
                    activos++;
                else if (string.Equals(est, "VENCIDO", StringComparison.OrdinalIgnoreCase))
                    vencidos++;
                else if (string.Equals(est, "CONGELADO", StringComparison.OrdinalIgnoreCase))
                    congelados++;
                else if (string.Equals(est, "DESACTIVADO", StringComparison.OrdinalIgnoreCase))
                    desactivados++;
            }

            return (activos, vencidos, congelados, desactivados);
        }

        public static bool CoincidenConTabla(DataTable tabla, out string resumen)
        {
            var ssot = Obtener();
            var grid = ContarDesdeTabla(tabla);
            resumen =
                $"Activos {grid.Activos}/{ssot.Activos} · Vencidos {grid.Vencidos}/{ssot.Vencidos} · " +
                $"Congelados {grid.Congelados}/{ssot.Congelados} · Desactivados {grid.Desactivados}/{ssot.Desactivados}";
            return grid.Activos == ssot.Activos
                   && grid.Vencidos == ssot.Vencidos
                   && grid.Congelados == ssot.Congelados
                   && grid.Desactivados == ssot.Desactivados;
        }
    }
}
