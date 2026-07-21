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
    }
}