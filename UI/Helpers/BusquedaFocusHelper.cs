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
            if (EsEntradaTextoActiva(desdeMensaje))
                return desdeMensaje;

            if (desdeMensaje != null)
                return desdeMensaje;

            if (desdeFocus != null)
                return desdeFocus;

            if (formulario != null)
            {
                Control? hojaActiva = ResolverHojaActiva(formulario);
                if (hojaActiva != null)
                    return hojaActiva;

                Control? recursivo = BuscarControlConFocoRecursivo(formulario);
                if (recursivo != null)
                    return recursivo;
            }

            return formulario?.ActiveControl;
        }

        /// <summary>Baja por ContainerControl.ActiveControl hasta la hoja (forms embebidos / tabs).</summary>
        private static Control? ResolverHojaActiva(Control root)
        {
            Control? actual = root is ContainerControl ccRoot ? ccRoot.ActiveControl : null;
            if (actual == null)
                return null;

            while (actual is ContainerControl cc && cc.ActiveControl != null)
                actual = cc.ActiveControl;

            return actual;
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

        /// <summary>
        /// True si el usuario está escribiendo o tipando en un control de entrada.
        /// Mientras sea true, los atajos de módulo (P/C/E/…) y navegación Back/Esc no deben dispararse;
        /// la tecla llega al control con normalidad.
        /// </summary>
        internal static bool EsEntradaTextoActiva(Control? activo)
        {
            if (activo == null)
                return false;

            for (Control? c = activo; c != null; c = c.Parent)
            {
                if (!c.Enabled || !c.Visible)
                    continue;

                // TextBox / RichTextBox / MaskedTextBox / editing control de grilla.
                if (c is TextBoxBase { ReadOnly: false })
                    return true;

                // DropDown (escritura) y DropDownList (type-ahead al buscar miembro/plan).
                if (c is ComboBox cb && (cb.Focused || cb.ContainsFocus || cb.DroppedDown))
                    return true;

                if (c is NumericUpDown nud && (nud.Focused || nud.ContainsFocus))
                    return true;

                if (c is DateTimePicker dtp && (dtp.Focused || dtp.ContainsFocus))
                    return true;

                if (c is DataGridView dgv && dgv.IsCurrentCellInEditMode)
                    return true;

                // Listas usadas como buscador (p. ej. listMiembros en financiamiento).
                if (c is ListBox { Focused: true })
                    return true;
            }

            return false;
        }

        internal static bool EsEntradaTextoActiva(IntPtr hwnd, Form? formulario) =>
            EsEntradaTextoActiva(ResolverControlConFoco(hwnd, formulario));

        /// <summary>Atajos de módulo/cobro/escáner: no interceptar si hay entrada de texto activa.</summary>
        internal static bool DebeAnularAtajosTeclado(Form? formulario, IntPtr hwndOrigen = default) =>
            EsEntradaTextoActiva(hwndOrigen, formulario);

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
