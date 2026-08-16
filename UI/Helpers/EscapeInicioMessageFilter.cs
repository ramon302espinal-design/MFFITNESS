using CORE;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using UI.DISEÑO;

namespace UI.Helpers
{
    /// <summary>
    /// Atajos de teclado de sesión:
    /// ESC → cierra módulos y vuelve a FrmPresentacion.
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

            Keys tecla = (Keys)(int)m.WParam;
            if (tecla != Keys.Escape && tecla != Keys.End)
                return false;

            Form? activa = Form.ActiveForm;
            if (activa == null || activa is FrmLogin)
                return false;

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
