using System;
using System.Collections.Generic;

namespace CORE
{
    public sealed class ProgramacionActivadaEventArgs : EventArgs
    {
        public int ProgramacionId { get; init; }
        public int ClienteId { get; init; }
        public int MembresiaId { get; init; }
        public string ClienteNombre { get; init; } = string.Empty;
        public string PlanNombre { get; init; } = string.Empty;
        public DateTime FechaInicio { get; init; }
        public DateTime FechaFin { get; init; }
    }

    public static class AppEventos
    {
        // ===============================
        // PRUEBAS Y CONFIGURACIONES DE CAJA
        // ===============================
        

        public static event Action? OnCajaCambiada;

        public static void CajaCambiada()
        {
            OnCajaCambiada?.Invoke();
        }
        public static event Action? OnPagoRegistrado;

        public static void PagoRegistrado()
        {
            OnPagoRegistrado?.Invoke();
        }

        public static event Action? OnDeudaModificada;

        public static void DeudaModificada()
        {
            OnDeudaModificada?.Invoke();
        }

        /// <summary>
        /// Stock de un producto quedó en 0 o en/bajo StockMinimo (tras salida/venta).
        /// </summary>
        public static event Action<int>? OnStockCritico;

        public static void StockCritico(int productoId)
        {
            if (productoId <= 0)
                return;
            OnStockCritico?.Invoke(productoId);
        }

        /// <summary>Congelar, desactivar, programación aplicada, ajuste de fecha, etc.</summary>
        public static event Action? OnEstadoMembresiaCambiada;

        public static void EstadoMembresiaCambiada()
        {
            OnEstadoMembresiaCambiada?.Invoke();
        }

        public static event Action<ProgramacionActivadaEventArgs>? OnProgramacionActivada;

        private static readonly HashSet<int> _programacionEventoEmitido = new();

        public static void ProgramacionActivada(ProgramacionActivadaEventArgs info)
        {
            if (info == null || info.ProgramacionId <= 0)
                return;

            lock (_programacionEventoEmitido)
            {
                if (!_programacionEventoEmitido.Add(info.ProgramacionId))
                    return;
            }

            OnProgramacionActivada?.Invoke(info);
        }
    }
}