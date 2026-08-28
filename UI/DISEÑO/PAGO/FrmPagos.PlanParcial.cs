using System;
using System.Data;
using System.Windows.Forms;
using BLL;

namespace UI.DISEÑO
{
    public partial class FrmPagos
    {
        private const int CantidadPlanParcialMax = 99;
        private bool _sincronizandoCantidadParcial;

        /// <summary>ATLETA/VISITA: activa cantidad y recalcula precio total.</summary>
        private void ActualizarUiPlanParcialCantidad()
        {
            bool esParcial = EsPlanParcialSeleccionado();

            lblCantidad.Enabled = esParcial;
            txtCantidad.Enabled = esParcial;

            if (!esParcial)
            {
                _sincronizandoCantidadParcial = true;
                try
                {
                    txtCantidad.Text = "1";
                }
                finally
                {
                    _sincronizandoCantidadParcial = false;
                }

                return;
            }

            if (!int.TryParse(txtCantidad.Text.Trim(), out int cant) || cant < 1)
            {
                _sincronizandoCantidadParcial = true;
                try
                {
                    txtCantidad.Text = "1";
                }
                finally
                {
                    _sincronizandoCantidadParcial = false;
                }
            }

            ActualizarMontoPlanParcial();
        }

        private void ActualizarMontoPlanParcial()
        {
            if (!EsPlanParcialSeleccionado())
                return;

            if (cmbMembresia.SelectedItem is not DataRowView row)
                return;

            decimal precioUnitario = Convert.ToDecimal(row["Precio"]);
            int cantidad = ObtenerCantidadPlanParcial();
            decimal total = Math.Round(precioUnitario * cantidad, 2, MidpointRounding.AwayFromZero);
            txtMonto.Text = total.ToString("0.00");
        }

        private int ObtenerCantidadPlanParcial()
        {
            if (!int.TryParse(txtCantidad.Text.Trim(), out int cantidad) || cantidad < 1)
                return 1;
            return Math.Min(cantidad, CantidadPlanParcialMax);
        }

        private bool TryObtenerCantidadPlanParcial(out int cantidad, out string? error)
        {
            cantidad = 0;
            error = null;

            if (!EsPlanParcialSeleccionado())
            {
                cantidad = 1;
                return true;
            }

            if (!int.TryParse(txtCantidad.Text.Trim(), out cantidad) || cantidad < 1)
            {
                error = "Indique una cantidad válida (mínimo 1).";
                return false;
            }

            if (cantidad > CantidadPlanParcialMax)
            {
                error = $"La cantidad máxima por cobro es {CantidadPlanParcialMax}.";
                return false;
            }

            return true;
        }

        private void txtCantidad_TextChanged(object? sender, EventArgs e)
        {
            if (_sincronizandoCantidadParcial || !txtCantidad.Enabled)
                return;

            ActualizarMontoPlanParcial();
        }

        private void txtCantidad_Leave(object? sender, EventArgs e)
        {
            if (!txtCantidad.Enabled)
                return;

            _sincronizandoCantidadParcial = true;
            try
            {
                if (!int.TryParse(txtCantidad.Text.Trim(), out int cant) || cant < 1)
                    txtCantidad.Text = "1";
                else if (cant > CantidadPlanParcialMax)
                    txtCantidad.Text = CantidadPlanParcialMax.ToString();
            }
            finally
            {
                _sincronizandoCantidadParcial = false;
            }

            ActualizarMontoPlanParcial();
        }
    }
}
