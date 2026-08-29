using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

        public static void Wire(Form host)
        {
            if (host == null || ThemeHost.IsDesignTime())
                return;

            if (FormsCableadas.TryGetValue(host, out _))
                return;

            FormsCableadas.Add(host, CableMarker);

            void AlCargar(object? _, EventArgs __)
            {
                host.Load -= AlCargar;
                if (host.IsDisposed)
                    return;

                HashSet<TextBoxBase> campos = RecolectarCamposBusqueda(host);
                if (campos.Count == 0)
                    return;

                WireRecursivo(host, host, campos);
            }

            if (host.IsHandleCreated)
                AlCargar(null, EventArgs.Empty);
            else
                host.Load += AlCargar;
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

        private static bool EsCampoBusqueda(TextBoxBase tb)
        {
            if (tb.ReadOnly)
                return false;

            string name = tb.Name ?? string.Empty;
            if (name.Contains("buscar", StringComparison.OrdinalIgnoreCase))
                return true;

            if (tb is TextBox tbox && !string.IsNullOrWhiteSpace(tbox.PlaceholderText))
            {
                return tbox.PlaceholderText.Contains("buscar", StringComparison.OrdinalIgnoreCase);
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
