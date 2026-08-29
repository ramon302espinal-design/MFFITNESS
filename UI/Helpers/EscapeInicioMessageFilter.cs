using CORE;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using UI.DISEÑO;

namespace UI.Helpers
{
    /// <summary>
    /// Atajos de teclado de sesión:
    /// Backspace → retrocede un nivel (◀ o cerrar ventana/modal activa), repetible.
    /// ESC → cierra todos los módulos y vuelve a FrmPresentacion.
    /// END → cierra módulos y ejecuta la misma salida de sesión que el botón SALIR.
    /// </summary>
    internal sealed class EscapeInicioMessageFilter : IMessageFilter
    {
        private const int WmKeyDown = 0x0100;
        private const int WmSysKeyDown = 0x0104;
        private bool _navegando;

        public bool PreFilterMessage(ref Message m)
        {
            if ((m.Msg != WmKeyDown && m.Msg != WmSysKeyDown) || _navegando || !Sesion.Activa)
                return false;

            Keys tecla = (Keys)(int)m.WParam & Keys.KeyCode;
            if (tecla != Keys.Escape && tecla != Keys.End && tecla != Keys.Back)
                return false;

            if (Control.ModifierKeys != Keys.None)
                return false;

            Form? activa = Form.ActiveForm;
            if (activa == null || activa is FrmLogin)
                return false;

            if (tecla == Keys.Back)
            {
                if (activa is FrmPresentacion)
                    return false;

                if (BusquedaFocusHelper.EsEntradaTextoActiva(m.HWnd, activa))
                    return false;

                // Calculadora POS: Backspace borra dígitos del monto.
                if (activa is FrmPago)
                    return false;

                _navegando = true;
                Form destino = activa;
                destino.BeginInvoke(new Action(() => RetrocederUnNivel(destino)));
                return true;
            }

            if (tecla == Keys.Escape)
            {
                if (activa is FrmPresentacion)
                    return false;

                _navegando = true;
                activa.BeginInvoke(new Action(VolverAPresentacion));
                return true;
            }

            // END: salir de sesión (misma lógica que btnCerrarSesion).
            _navegando = true;
            activa.BeginInvoke(new Action(CerrarSesionConEnd));
            return true;
        }

        private void RetrocederUnNivel(Form activa)
        {
            try
            {
                if (activa.IsDisposed)
                    return;

                if (IntentarRetrocederConBotonBack(activa))
                    return;

                activa.DialogResult = DialogResult.Cancel;
                activa.Close();
            }
            catch (ObjectDisposedException)
            {
                // La ventana ya fue cerrada por su dueño modal.
            }
            finally
            {
                _navegando = false;
            }
        }

        private static bool IntentarRetrocederConBotonBack(Form form)
        {
            Button? back = BuscarBotonBack(form);
            if (back == null || back.IsDisposed || !back.Enabled)
                return false;

            back.PerformClick();
            return true;
        }

        private static Button? BuscarBotonBack(Control root)
        {
            foreach (Control c in root.Controls)
            {
                if (c is Button btn && EsBotonBack(btn))
                    return btn;

                Button? anidado = BuscarBotonBack(c);
                if (anidado != null)
                    return anidado;
            }

            return null;
        }

        private static bool EsBotonBack(Button btn) =>
            btn.Name is "btnBack" or "btnNavBack";

        private void VolverAPresentacion()
        {
            try
            {
                FrmPresentacion? presentacion = CerrarModulosAbiertos();

                if (presentacion == null || presentacion.IsDisposed)
                    presentacion = new FrmPresentacion();

                if (!presentacion.Visible)
                    presentacion.Show();

                if (presentacion.WindowState == FormWindowState.Minimized)
                    presentacion.WindowState = FormWindowState.Maximized;

                presentacion.BringToFront();
                presentacion.Activate();
                presentacion.CargarDashboard();
            }
            finally
            {
                _navegando = false;
            }
        }

        private void CerrarSesionConEnd()
        {
            try
            {
                FrmPresentacion? presentacion = CerrarModulosAbiertos();
                if (presentacion == null || presentacion.IsDisposed)
                    return;

                presentacion.IntentarCerrarSesion();
            }
            finally
            {
                _navegando = false;
            }
        }

        private static FrmPresentacion? CerrarModulosAbiertos()
        {
            FrmPresentacion? presentacion = null;
            var abiertas = new List<Form>();

            foreach (Form form in Application.OpenForms)
            {
                abiertas.Add(form);
                if (form is FrmPresentacion principal)
                    presentacion = principal;
            }

            for (int i = abiertas.Count - 1; i >= 0; i--)
            {
                Form form = abiertas[i];
                if (form.IsDisposed || form is FrmPresentacion || form is FrmLogin)
                    continue;

                try
                {
                    form.DialogResult = DialogResult.Cancel;
                    form.Close();
                }
                catch (ObjectDisposedException)
                {
                    // Ya la cerró su propietaria.
                }
            }

            return presentacion;
        }
    }
}
