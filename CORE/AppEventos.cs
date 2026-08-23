using System;

namespace CORE
{
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
    }
}