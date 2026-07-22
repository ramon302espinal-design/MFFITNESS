using BLL;
using BLL.Commands;
using CORE; // 🔥 IMPORTANTE
using CORE.Commands;
using System;
using System.Windows.Forms;
using UI.Helpers;
using UI.Theme;

namespace UI
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmDeudaDashboard : Form
    {
        private DeudaBLL deudaBLL = new DeudaBLL();

        public FrmDeudaDashboard()
        {
            InitializeComponent();
            ThemeHost.Attach(this);
        }
        private decimal SafeDecimal(object? valor)
        {
            if (valor == null || valor == DBNull.Value)
                return 0;

            return Convert.ToDecimal(valor);
        }
        private void FrmDeudaDashboard_Load(object sender, EventArgs e)
        {
            AplicarPermisosSoloLecturaConsulta();

            CargarTodo();

            // 🔥 ESCUCHAR EVENTO GLOBAL
            AppEventos.OnPagoRegistrado += ActualizarDashboard;
            AppEventos.OnDeudaModificada += ActualizarDashboard;
        }

        /// <summary>
        /// CONSULTA / solo historial: puede ver deudas y DESCARGAR REPORTE,
        /// pero no crear ni cobrar.
        /// </summary>
        private void AplicarPermisosSoloLecturaConsulta()
        {
            bool esAdmin = string.Equals(Sesion.Rol?.Trim(), "ADMIN", StringComparison.OrdinalIgnoreCase);
            bool puedeCrear = esAdmin
                || Sesion.TienePermiso("CREAR_DEUDA")
                || Sesion.TienePermiso("VER_DEUDAS");
            bool puedePagar = esAdmin
                || Sesion.TienePermiso("PAGAR_DEUDA")
                || Sesion.TienePermiso("VER_DEUDAS");

            btnCrearDeuda.Visible = puedeCrear;
            btnRegistrarPago.Visible = puedePagar;
            btnIrAPagarDeuda.Visible = puedePagar;
            txtMontoPago.Visible = puedePagar;

            // Reporte: disponible para quien ya puede entrar al dashboard
            // (incluye CONSULTA con VER_HISTORIAL_DEUDAS).
            btnDescargarReporte.Visible = true;
            btnDescargarReporte.Enabled = true;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            AppEventos.OnPagoRegistrado -= ActualizarDashboard;
            AppEventos.OnDeudaModificada -= ActualizarDashboard;
            base.OnFormClosed(e);
        }

        // 🔹 MÉTODO PRINCIPAL
        private void CargarTodo()
        {
            if (IsDisposed || Disposing || !IsHandleCreated)
                return;

            if (dgvDeudas == null || dgvDeudas.IsDisposed)
                return;

            try
            {
                dgvDeudas.DataSource = deudaBLL.ObtenerDeudas();
                FormatearGridDeudas();

                lblDeudasActivas.Text = deudaBLL.DeudasActivas().ToString();
                lblDeudasVencidas.Text = deudaBLL.DeudasVencidas().ToString();
                lblIngresoPendiente.Text = deudaBLL.IngresoPendiente().ToString("N2");
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando dashboard: " + ex.Message);
            }
        }

        // 🔥 ACTUALIZACIÓN SEGURA DESDE EVENTOS
        private void ActualizarDashboard()
        {
            if (IsDisposed || Disposing)
                return;

            if (InvokeRequired)
            {
                try
                {
                    if (IsHandleCreated)
                        BeginInvoke(new Action(ActualizarDashboard));
                }
                catch (ObjectDisposedException)
                {
                }
                return;
            }

            CargarTodo();
        }

        // ===============================
        // MÉTODO PÚBLICO PARA REFRESCAR DESDE MÓDULO PRINCIPAL
        // ===============================
        public void ActualizarDatos()
        {
            CargarTodo();
        }

        // ===============================
        // BOTONES
        // ===============================
        private void btnCrearDeuda_Click(object sender, EventArgs e)
        {
            try
            {
                FrmCrearDeuda frm = new FrmCrearDeuda();

                var resultado = frm.ShowDialog();

                if (resultado == DialogResult.OK)
                {
                    CargarTodo(); // 🔥 esto lo dejamos (doble seguridad)
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir formulario: " + ex.Message);
            }
        }

        private void btnRegistrarPago_Click(object sender, EventArgs e)
        {
            try
            {
                if (IsDisposed || dgvDeudas == null || dgvDeudas.IsDisposed)
                    return;

                if (dgvDeudas.CurrentRow == null)
                {
                    MessageBox.Show("Selecciona una deuda");
                    return;
                }

                int deudaId = Convert.ToInt32(dgvDeudas.CurrentRow.Cells["Id"].Value);

                // 🔥 TOMAR MONTO DESDE INPUT (ej: txtMontoPago)
                if (!decimal.TryParse(txtMontoPago.Text, out decimal monto))
                {
                    MessageBox.Show("Ingrese un monto válido");
                    return;
                }

                if (monto <= 0)
                {
                    MessageBox.Show("El monto debe ser mayor a 0");
                    return;
                }

                // 🔥 OPCIONAL: VALIDACIÓN RÁPIDA DESDE GRID (UX)
                decimal saldo = Convert.ToDecimal(dgvDeudas.CurrentRow.Cells["Saldo"].Value);

                if (monto > saldo)
                {
                    MessageBox.Show($"El monto excede el saldo pendiente: {saldo:N2}");
                    return;
                }

                var result = DeudaCommandService.RegistrarPago(
                    deudaId, monto, "EFECTIVO", Sesion.Usuario);

                if (!result.Success)
                {
                    MessageBox.Show(result.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                MessageBox.Show(result.Message, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Un solo refresco local; AppEventos notifica a otros módulos vivos.
                AppEventos.PagoRegistrado();
                CargarTodo();
                txtMontoPago.Clear();
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btnIrAPagarDeuda_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvDeudas.CurrentRow == null)
                {
                    MessageBox.Show("Selecciona una deuda para pagar.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (!dgvDeudas.Columns.Contains("Id") ||
                    !dgvDeudas.Columns.Contains("Nombre") ||
                    !dgvDeudas.Columns.Contains("Saldo") ||
                    !dgvDeudas.Columns.Contains("Estado"))
                {
                    MessageBox.Show("Error de configuración del grid. Recargue el dashboard.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var row = dgvDeudas.CurrentRow;
                int deudaId = Convert.ToInt32(row.Cells["Id"].Value);
                string nombre = row.Cells["Nombre"].Value?.ToString() ?? "Cliente";
                decimal saldo = SafeDecimal(row.Cells["Saldo"].Value);
                string estado = row.Cells["Estado"].Value?.ToString() ?? "ACTIVA";

                if (estado == "PAGADA")
                {
                    MessageBox.Show("Esta deuda ya está pagada.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (saldo <= 0)
                {
                    MessageBox.Show("Esta deuda no tiene saldo pendiente.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (FrmPagarDeudas frm = new FrmPagarDeudas(nombre, saldo, estado, null))
                {
                    if (frm.ShowDialog() != DialogResult.OK)
                        return;

                    var result = DeudaCommandService.RegistrarPago(
                        deudaId, frm.Monto, frm.Metodo, Sesion.Usuario);

                    if (!result.Success)
                    {
                        MessageBox.Show(result.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    AppEventos.PagoRegistrado();
                    CargarTodo();

                    MessageBox.Show(result.Message, "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al procesar pago: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDescargarReporte_Click(object sender, EventArgs e)
        {
            try
            {
                using var frm = new FrmReporteDeudas();
                frm.ShowDialog(FindForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir reporte: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvDeudas_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvDeudas.CurrentRow == null) return;

            decimal saldo = SafeDecimal(dgvDeudas.CurrentRow.Cells["Saldo"].Value);

            txtMontoPago.Text = saldo.ToString("N2");
        }

        private void FormatearGridDeudas()
        {
            if (dgvDeudas.Columns.Count == 0) return;

            DataGridViewHelper.HideColumn(dgvDeudas, "Id");
            DataGridViewHelper.HideColumn(dgvDeudas, "ClienteId");
            DataGridViewHelper.HideColumn(dgvDeudas, "MembresiaId");
            DataGridViewHelper.HideColumn(dgvDeudas, "PlanId");
            DataGridViewHelper.HideColumn(dgvDeudas, "PagoInicialFinanciamiento");

            DataGridViewHelper.ConfigureColumn(dgvDeudas, "FechaVencimiento", col =>
            {
                col.HeaderText = "Fecha Límite Pago";
                col.DefaultCellStyle.Format = "dd/MM/yyyy";
            });

            DataGridViewHelper.ConfigureColumn(dgvDeudas, "AporteInicial", col =>
                col.HeaderText = "Pago Inicial");

            DataGridViewHelper.ConfigureColumn(dgvDeudas, "FechaFinMembresia", col =>
                col.HeaderText = "Vence Plan");
        }
    }

}