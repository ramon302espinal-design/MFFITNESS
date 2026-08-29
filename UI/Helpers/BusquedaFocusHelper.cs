using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UI.Theme;

namespace UI.Helpers
{
    /// <summary>
    /// Quita el foco (caret) de cajas de búsqueda al hacer clic en zona vacía del formulario.
    /// </summary>
    internal static class BusquedaFocusHelper
    {
        private static readonly ConditionalWeakTable<Form, object> FormsCableadas = new();
        private static readonly object CableMarker = new();

        [DllImport("user32.dll")]
        private static extern IntPtr GetFocus();

        public static void Wire(Form host)
        {
            if (host == null || ThemeHost.IsDesignTime())
                return;

            if (FormsCableadas.TryGetValue(host, out _))
            {
                WireFormulariosEmbebidos(host);
                return;
            }

            FormsCableadas.Add(host, CableMarker);

            void AlCargar(object? _, EventArgs __)
            {
                host.Load -= AlCargar;
                if (host.IsDisposed)
                    return;

                CablearBusquedaEnControl(host, host);
                WireFormulariosEmbebidos(host);
            }

            if (host.IsHandleCreated)
                AlCargar(null, EventArgs.Empty);
            else
                host.Load += AlCargar;
        }

        /// <summary>Formularios TopLevel=false dentro de tabs/paneles (Deudas, CRM).</summary>
        public static void WireFormulariosEmbebidos(Control root)
        {
            if (root == null || root.IsDisposed)
                return;

            foreach (Control hijo in root.Controls)
            {
                if (hijo is Form embebido && !embebido.TopLevel)
                    Wire(embebido);

                WireFormulariosEmbebidos(hijo);
            }
        }

        private static void CablearBusquedaEnControl(Form host, Control nodo)
        {
            HashSet<TextBoxBase> campos = RecolectarCamposBusqueda(nodo);
            if (campos.Count == 0)
                return;

            WireRecursivo(nodo, host, campos);
        }

        private static void WireRecursivo(Control nodo, Form host, HashSet<TextBoxBase> campos)
        {
            if (!EsControlQueConservaFoco(nodo))
            {
                nodo.MouseDown += (_, e) =>
                {
                    if (e.Button == MouseButtons.Left)
                        QuitarFocoBusqueda(host, campos, nodo);
                };
            }

            foreach (Control hijo in nodo.Controls)
                WireRecursivo(hijo, host, campos);
        }

        private static void QuitarFocoBusqueda(Form host, HashSet<TextBoxBase> campos, Control clicado)
        {
            if (host.ActiveControl is not TextBoxBase activo || !campos.Contains(activo))
                return;

            if (ReferenciaEsMismaArea(activo, clicado))
                return;

            Control? destino = clicado;
            while (destino != null && !destino.CanFocus)
                destino = destino.Parent;

            if (destino != null && destino.CanFocus)
                destino.Focus();
        }

        private static bool ReferenciaEsMismaArea(Control busqueda, Control clicado)
        {
            for (Control? c = clicado; c != null; c = c.Parent)
            {
                if (ReferenceEquals(c, busqueda))
                    return true;
            }

            return false;
        }

        private static HashSet<TextBoxBase> RecolectarCamposBusqueda(Control root)
        {
            var set = new HashSet<TextBoxBase>();
            RecolectarRecursivo(root, set);
            return set;
        }

        private static void RecolectarRecursivo(Control root, HashSet<TextBoxBase> set)
        {
            if (root is TextBoxBase tb && EsCampoBusqueda(tb))
                set.Add(tb);

            foreach (Control c in root.Controls)
                RecolectarRecursivo(c, set);
        }

        /// <summary>Resuelve el control destino del mensaje WM_KEY* (más fiable que Form.ActiveControl).</summary>
        internal static Control? ResolverControlConFoco(IntPtr hwnd, Form? formulario)
        {
            Control? desdeFocus = ObtenerControlDesdeHandle(GetFocus());

            // GetFocus gana cuando hay un TextBox real (forms embebidos + KeyPreview del host).
            if (EsEntradaTextoActiva(desdeFocus))
                return desdeFocus;

            Control? desdeMensaje = ObtenerControlDesdeHandle(hwnd);
            if (desdeMensaje != null)
                return desdeMensaje;

            if (desdeFocus != null)
                return desdeFocus;

            if (formulario != null)
            {
                Control? recursivo = BuscarControlConFocoRecursivo(formulario);
                if (recursivo != null)
                    return recursivo;
            }

            return formulario?.ActiveControl;
        }

        private static Control? ObtenerControlDesdeHandle(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
                return null;

            try
            {
                return Control.FromHandle(hwnd);
            }
            catch
            {
                return null;
            }
        }

        private static Control? BuscarControlConFocoRecursivo(Control root)
        {
            if (root.Focused)
                return root;

            foreach (Control hijo in root.Controls)
            {
                Control? encontrado = BuscarControlConFocoRecursivo(hijo);
                if (encontrado != null)
                    return encontrado;
            }

            return null;
        }

        /// <summary>True si el foco está en un campo editable (TextBox, Combo editable, ListBox).</summary>
        internal static bool EsEntradaTextoActiva(Control? activo)
        {
            if (activo == null)
                return false;

            for (Control? c = activo; c != null; c = c.Parent)
            {
                if (c is TextBoxBase { ReadOnly: false })
                    return true;

                if (c is ComboBox cb && (cb.DropDownStyle == ComboBoxStyle.DropDown || cb.DroppedDown))
                    return true;

                if (c is ListBox lb && lb.Focused)
                    return true;
            }

            return false;
        }

        internal static bool EsEntradaTextoActiva(IntPtr hwnd, Form? formulario) =>
            EsEntradaTextoActiva(ResolverControlConFoco(hwnd, formulario));

        private static bool EsCampoBusqueda(TextBoxBase tb)
        {
            if (tb.ReadOnly)
                return false;

            string name = tb.Name ?? string.Empty;
            if (name.Contains("busca", StringComparison.OrdinalIgnoreCase))
                return true;

            if (string.Equals(tb.Tag as string, "busqueda", StringComparison.OrdinalIgnoreCase))
                return true;

            if (tb is TextBox tbox && !string.IsNullOrWhiteSpace(tbox.PlaceholderText))
            {
                string ph = tbox.PlaceholderText;
                return ph.Contains("buscar", StringComparison.OrdinalIgnoreCase)
                    || ph.Contains("filtrar", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private static bool EsControlQueConservaFoco(Control c) =>
            c is TextBoxBase
                or ComboBox
                or ListBox
                or NumericUpDown
                or DateTimePicker
                or CheckBox
                or RadioButton
                or Button
                or DataGridView
                or MenuStrip
                or StatusStrip
                or ToolStrip
                or TabControl;
    }
}
