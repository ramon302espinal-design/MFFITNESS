using System;
using System.Data;
using System.Windows.Forms;
using BLL;
using BLL.Commands;
using CORE;
using UI.Helpers;

namespace UI
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmDeudas : Form
    {
        private static readonly Color ColorSeleccionGrid = Color.FromArgb(0x1B, 0x92, 0xFF);

        private readonly DeudaBLL deudaBLL = new DeudaBLL();
        private readonly BindingSource _bsDeudas = new BindingSource();
        private int? _clienteIdPreseleccionado;
        private bool _seleccionClientePendiente;

        public FrmDeudas() : this(null)
        {
        }

        public FrmDeudas(int? clienteIdPreseleccionado)
        {
            InitializeComponent();

            if (clienteIdPreseleccionado.HasValue)
            {
                _clienteIdPreseleccionado = clienteIdPreseleccionado;
                _seleccionClientePendiente = true;
            }
        }
        private void dgvDeudas_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (IsDisposed || dgvDeudas.Rows.Count == 0 || e.RowIndex < 0)
                return;

            if (e.ColumnIndex < 0 || e.ColumnIndex >= dgvDeudas.Columns.Count)
                return;

            if (e.CellStyle == null)
                e.CellStyle = new DataGridViewCellStyle(dgvDeudas.DefaultCellStyle);

            // 🔥 FORMATO COLUMNA ESTADO
            if (e.ColumnIndex >= 0
                && dgvDeudas.Columns[e.ColumnIndex] is DataGridViewColumn estadoColumn
                && estadoColumn.Name == "Estado"
                && e.Value != null)
            {
                string estado = (e.Value?.ToString() ?? string.Empty).ToUpper();

                // 🔥 NEGRITA SIEMPRE
                e.CellStyle.Font = new Font(dgvDeudas.Font, FontStyle.Bold);

                if (estado == "ACTIVA")
                {
                    e.CellStyle.ForeColor = Color.Red;
                }
                else if (estado == "PAGADA")
                {
                    e.CellStyle.ForeColor = Color.Green;
                }
                else
                {
                    e.CellStyle.ForeColor = Color.Black;
                }

                // 🔥 FONDO NORMAL
                e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            // 🔥 FORMATO COLUMNA DÍAS RESTANTES
            if (dgvDeudas.Columns[e.ColumnIndex].Name == "DiasRestantes" && e.Value != null)
            {
                int dias = Convert.ToInt32(e.Value);

                e.CellStyle.Font = new Font(dgvDeudas.Font, FontStyle.Bold);
                e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                if (dias < 0)
                {
                    // VENCIDA
                    e.CellStyle.BackColor = Color.FromArgb(255, 200, 200); // Rojo claro
                    e.CellStyle.ForeColor = Color.DarkRed;
                    e.Value = $"⚠️ VENCIDA ({Math.Abs(dias)}d)";
                }
                else if (dias <= 3)
                {
                    // PRÓXIMA A VENCER
                    e.CellStyle.BackColor = Color.FromArgb(255, 255, 200); // Amarillo claro
                    e.CellStyle.ForeColor = Color.DarkOrange;
                    e.Value = $"⏰ {dias} día(s)";
                }
                else
                {
                    // NORMAL
                    e.CellStyle.ForeColor = Color.Green;
                    e.Value = $"✓ {dias} día(s)";
                }
            }

            // 🆕 RESALTAR COLUMNA PLAN CUANDO NO ES N/A
            if (dgvDeudas.Columns[e.ColumnIndex].Name == "Plan" && e.Value != null)
            {
                string plan = e.Value.ToString();
                if (plan != "N/A")
                {
                    e.CellStyle.BackColor = Color.AliceBlue;
                    e.CellStyle.ForeColor = Color.DarkBlue;
                    e.CellStyle.Font = new Font(dgvDeudas.Font, FontStyle.Bold);
                }
            }

            if (dgvDeudas.Columns[e.ColumnIndex].Name == "AporteInicial" && e.Value != null)
            {
                string aporte = e.Value.ToString() ?? string.Empty;
                e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                if (aporte.StartsWith("Sí", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.ForeColor = Color.FromArgb(0, 150, 136);
                    e.CellStyle.Font = new Font(dgvDeudas.Font, FontStyle.Bold);
                }
                else if (aporte.StartsWith("No", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.ForeColor = Color.DarkOrange;
                }
            }

            // 🔥 RESALTAR FILA COMPLETA SI ESTÁ VENCIDA
            try
            {
                if (dgvDeudas.Columns.Contains("DiasRestantes") && 
                    dgvDeudas.Columns.Contains("Estado"))
                {
                    var diasCell = dgvDeudas.Rows[e.RowIndex].Cells["DiasRestantes"];
                    var estadoCell = dgvDeudas.Rows[e.RowIndex].Cells["Estado"];

                    if (diasCell.Value != null && estadoCell.Value != null)
                    {
                        int dias = Convert.ToInt32(diasCell.Value);
                        string estado = estadoCell.Value.ToString();

                        if (dias < 0 && estado == "ACTIVA")
                        {
                            // Toda la fila en rojo suave si está vencida
                            e.CellStyle.BackColor = Color.FromArgb(255, 240, 240);
                        }
                    }
                }
            }
            catch { }

            if (dgvDeudas.Rows[e.RowIndex].Selected)
            {
                e.CellStyle.BackColor = ColorSeleccionGrid;
                e.CellStyle.ForeColor = Color.White;
            }
        }

        // ===============================
        // LOAD
        // ===============================
        private void FrmDeudas_Load(object sender, EventArgs e)
        {

            ConfigurarGrid();
            ConfigurarFiltros();

            if (_seleccionClientePendiente)
                cmbFiltro.SelectedItem = "Activas";

            CargarDeudas();

            // 🔥 Evento global (auto refresh)
            AppEventos.OnPagoRegistrado += CargarDeudas;
            AppEventos.OnDeudaModificada += CargarDeudas;
        }

        // ===============================
        // CONFIGURAR FILTROS
        // ===============================
        private void ConfigurarFiltros()
        {
            cmbFiltro.Items.Clear();
            cmbFiltro.Items.Add("Todas");
            cmbFiltro.Items.Add("Activas");
            cmbFiltro.Items.Add("Pagadas");
            cmbFiltro.Items.Add("Vencidas");
            cmbFiltro.SelectedIndex = 0; // "Todas" por defecto
        }

        // ===============================
        // CONFIGURAR GRID (PRO)
        // ===============================
        private void ConfigurarGrid()
        {
            // Estilos visuales del grid viven en el Designer.
            dgvDeudas.AutoGenerateColumns = true;
            dgvDeudas.DataSource = _bsDeudas;
        }

        // ===============================
        // CARGAR DEUDAS
        // ===============================
        private void CargarDeudas()
        {
            if (IsDisposed || Disposing)
                return;

            if (InvokeRequired)
            {
                try
                {
                    if (IsHandleCreated)
                        BeginInvoke(new Action(CargarDeudas));
                }
                catch (ObjectDisposedException) { }
                return;
            }

            if (!IsHandleCreated)
                return;

            try
            {
                DataTable? dt = deudaBLL.ObtenerDeudas();
                if (dt == null)
                {
                    MessageBox.Show("No se pudieron obtener las deudas.", "Error");
                    return;
                }

                // 🔥 AGREGAR COLUMNA DE DÍAS RESTANTES
                if (!dt.Columns.Contains("DiasRestantes"))
                {
                    dt.Columns.Add("DiasRestantes", typeof(int));
                }

                // 🔥 CALCULAR DÍAS RESTANTES
                foreach (DataRow row in dt.Rows)
                {
                    if (row["FechaVencimiento"] == DBNull.Value || row["FechaVencimiento"] == null)
                    {
                        row["DiasRestantes"] = 0;
                        continue;
                    }

                    DateTime fechaVencimiento = Convert.ToDateTime(row["FechaVencimiento"]);
                    int dias = (fechaVencimiento.Date - DateTime.Now.Date).Days;
                    row["DiasRestantes"] = dias;
                }

                // 🔥 APLICAR FILTRO
                _bsDeudas.DataSource = dt;
                AplicarFiltros();

                FormatearGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando deudas: " + ex.Message);
            }
        }

        // ===============================
        // MÉTODO PÚBLICO PARA REFRESCAR DESDE MÓDULO PRINCIPAL
        // ===============================
        public void ActualizarDatos()
        {
            CargarDeudas();
        }

        public void SeleccionarCliente(int clienteId)
        {
            _clienteIdPreseleccionado = clienteId;
            _seleccionClientePendiente = true;
            cmbFiltro.SelectedItem = "Activas";
            CargarDeudas();
        }

        // ===============================
        // APLICAR FILTROS (COMBO + BUSCADOR)
        // ===============================
        private void AplicarFiltros()
        {
            var filtros = new System.Collections.Generic.List<string>();

            string filtroCombo = cmbFiltro?.SelectedItem?.ToString() ?? "Todas";
            switch (filtroCombo)
            {
                case "Activas":
                    filtros.Add("Estado = 'ACTIVA'");
                    break;
                case "Pagadas":
                    filtros.Add("Estado = 'PAGADA'");
                    break;
                case "Vencidas":
                    filtros.Add("Estado = 'ACTIVA' AND DiasRestantes < 0");
                    break;
            }

            var termino = txtBuscar.Text.Trim();
            if (!string.IsNullOrEmpty(termino))
                filtros.Add("(" + BusquedaGridHelper.ConstruirFiltroDeudas(termino) + ")");

            _bsDeudas.Filter = filtros.Count > 0 ? string.Join(" AND ", filtros) : null;
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            AplicarFiltros();

            if (_seleccionClientePendiente)
                BeginInvoke(new Action(AplicarSeleccionClientePendiente));
        }

        // ===============================
        // EVENTO: CAMBIO DE FILTRO
        // ===============================
        private void cmbFiltro_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarDeudas();
        }

        // ===============================
        // FORMATO VISUAL
        // ===============================
        private void FormatearGrid()
        {
            if (dgvDeudas.Columns.Count == 0) return;

            DataGridViewHelper.HideColumn(dgvDeudas, "Id");
            DataGridViewHelper.HideColumn(dgvDeudas, "ClienteId");
            DataGridViewHelper.HideColumn(dgvDeudas, "FechaCreacion");
            DataGridViewHelper.HideColumn(dgvDeudas, "Usuario");
            DataGridViewHelper.HideColumn(dgvDeudas, "MembresiaId");
            DataGridViewHelper.HideColumn(dgvDeudas, "PlanId");
            DataGridViewHelper.HideColumn(dgvDeudas, "PagoInicialFinanciamiento");

            DataGridViewHelper.ConfigureColumn(dgvDeudas, "AporteInicial", col =>
            {
                col.HeaderText = "Pago Inicial";
                col.Width = 120;
            });

            DataGridViewHelper.ConfigureColumn(dgvDeudas, "MontoTotal", col =>
            {
                col.DefaultCellStyle.Format = "N2";
                col.HeaderText = "Monto Total";
            });

            DataGridViewHelper.ConfigureColumn(dgvDeudas, "MontoPagado", col =>
            {
                col.DefaultCellStyle.Format = "N2";
                col.HeaderText = "Pagado";
            });

            DataGridViewHelper.ConfigureColumn(dgvDeudas, "Saldo", col =>
            {
                col.DefaultCellStyle.Format = "N2";
                col.DefaultCellStyle.Font = new Font(dgvDeudas.Font, FontStyle.Bold);
            });

            DataGridViewHelper.ConfigureColumn(dgvDeudas, "FechaVencimiento", col =>
            {
                col.DefaultCellStyle.Format = "dd/MM/yyyy";
                col.HeaderText = "Fecha Límite Pago";
                col.Width = 130;
            });

            DataGridViewHelper.ConfigureColumn(dgvDeudas, "FechaInicioMembresia", col =>
            {
                col.DefaultCellStyle.Format = "dd/MM/yyyy";
                col.HeaderText = "Inicio Plan";
                col.Width = 100;
            });

            DataGridViewHelper.ConfigureColumn(dgvDeudas, "FechaFinMembresia", col =>
            {
                col.DefaultCellStyle.Format = "dd/MM/yyyy";
                col.HeaderText = "Vence Plan";
                col.Width = 100;
            });

            DataGridViewHelper.ConfigureColumn(dgvDeudas, "Plan", col =>
            {
                col.HeaderText = "Plan Financiado";
                col.Width = 120;
                col.DefaultCellStyle.Font = new Font(dgvDeudas.Font, FontStyle.Bold);
                col.DefaultCellStyle.ForeColor = Color.DarkBlue;
            });

            DataGridViewHelper.ConfigureColumn(dgvDeudas, "DiasRestantes", col =>
            {
                col.HeaderText = "Estado Vencimiento";
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            });

            DataGridViewHelper.SetDisplayIndex(dgvDeudas, "Nombre", 0);
            DataGridViewHelper.SetDisplayIndex(dgvDeudas, "Concepto", 1);
            DataGridViewHelper.SetDisplayIndex(dgvDeudas, "Plan", 2);
            DataGridViewHelper.SetDisplayIndex(dgvDeudas, "AporteInicial", 3);
            DataGridViewHelper.SetDisplayIndex(dgvDeudas, "MontoTotal", 4);
            DataGridViewHelper.SetDisplayIndex(dgvDeudas, "Saldo", 5);
            DataGridViewHelper.SetDisplayIndex(dgvDeudas, "FechaVencimiento", 6);
            DataGridViewHelper.SetDisplayIndex(dgvDeudas, "DiasRestantes", 7);
            DataGridViewHelper.SetDisplayIndex(dgvDeudas, "Estado", 8);
            DataGridViewHelper.SetDisplayIndex(dgvDeudas, "FechaInicioMembresia", 9);
            DataGridViewHelper.SetDisplayIndex(dgvDeudas, "FechaFinMembresia", 10);

            if (_seleccionClientePendiente)
                AplicarSeleccionClientePendiente();
            else
                dgvDeudas.ClearSelection();
        }

        private void AplicarSeleccionClientePendiente()
        {
            if (!_seleccionClientePendiente || !_clienteIdPreseleccionado.HasValue)
            {
                dgvDeudas.ClearSelection();
                return;
            }

            if (!dgvDeudas.Columns.Contains("ClienteId"))
            {
                dgvDeudas.ClearSelection();
                return;
            }

            int clienteId = _clienteIdPreseleccionado.Value;

            foreach (DataGridViewRow row in dgvDeudas.Rows)
            {
                if (row.IsNewRow)
                    continue;

                var valor = row.Cells["ClienteId"].Value;
                if (valor == null || valor == DBNull.Value)
                    continue;

                if (Convert.ToInt32(valor) != clienteId)
                    continue;

                row.Selected = true;

                if (dgvDeudas.Columns.Contains("Nombre"))
                    dgvDeudas.CurrentCell = row.Cells["Nombre"];
                else if (row.Cells.Count > 0)
                    dgvDeudas.CurrentCell = row.Cells[0];

                dgvDeudas.FirstDisplayedScrollingRowIndex = row.Index;
                _seleccionClientePendiente = false;
                return;
            }

            dgvDeudas.ClearSelection();
            _seleccionClientePendiente = false;
        }

        // ===============================
        // OBTENER ID SELECCIONADO
        // ===============================
        private int ObtenerDeudaId()
        {
            if (dgvDeudas.CurrentRow == null)
                throw new Exception("Seleccione una deuda.");

            // 🔥 VERIFICAR QUE LA COLUMNA ID EXISTA
            if (!dgvDeudas.Columns.Contains("Id"))
                throw new Exception("Error de configuración: Columna Id no encontrada.");

            var cellValue = dgvDeudas.CurrentRow.Cells["Id"].Value;

            if (cellValue == null || cellValue == DBNull.Value)
                throw new Exception("ID de deuda no válido.");

            return Convert.ToInt32(cellValue);
        }

        // ===============================
        // NUEVA DEUDA
        // ===============================
        private void btnNuevaDeuda_Click(object sender, EventArgs e)
        {
            using var frm = new FrmCrearDeuda();
            var owner = FindForm() ?? this;
            if (frm.ShowDialog(owner) == DialogResult.OK)
                CargarDeudas();
        }

        // ===============================
        // PAGAR DEUDA (SIMPLE PERO PRO)
        // ===============================
        private void btnPagar_Click(object sender, EventArgs e)
        {
            try
            {
                // 🔥 VALIDAR SELECCIÓN
                if (dgvDeudas.CurrentRow == null)
                {
                    MessageBox.Show("Por favor seleccione una deuda.", "Aviso");
                    return;
                }

                // 🔥 VALIDAR COLUMNAS NECESARIAS
                if (!dgvDeudas.Columns.Contains("Id") || 
                    !dgvDeudas.Columns.Contains("Nombre") || 
                    !dgvDeudas.Columns.Contains("Saldo") ||
                    !dgvDeudas.Columns.Contains("Estado"))
                {
                    MessageBox.Show("Error de configuración del grid. Recargue el formulario.", "Error");
                    return;
                }

                int deudaId = ObtenerDeudaId();
                var row = dgvDeudas.CurrentRow;

                string nombre = row.Cells["Nombre"].Value?.ToString() ?? "Cliente";
                decimal saldo = Convert.ToDecimal(row.Cells["Saldo"].Value ?? 0);
                string estado = row.Cells["Estado"].Value?.ToString() ?? "ACTIVA";

                // 🔥 VALIDAR SI YA ESTÁ PAGADA
                if (estado == "PAGADA")
                {
                    MessageBox.Show("Esta deuda ya está pagada.", "Aviso");
                    return;
                }

                if (saldo <= 0)
                {
                    MessageBox.Show("Esta deuda no tiene saldo pendiente.", "Aviso");
                    return;
                }

                // 🔥 Por ahora sin último pago (lo puedes mejorar luego)
                DateTime? ultimoPago = null;

                using var frm = new FrmPagarDeudas(nombre, saldo, estado, ultimoPago);
                var owner = FindForm() ?? this;

                if (frm.ShowDialog(owner) == DialogResult.OK)
                {
                    var result = DeudaCommandService.RegistrarPago(
                        deudaId, frm.Monto, frm.Metodo, Sesion.Usuario);

                    if (!result.Success)
                    {
                        MessageBox.Show(result.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    MessageBox.Show(result.Message, "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    AppEventos.PagoRegistrado();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al procesar pago: {ex.Message}", "Error");
            }
        }

        // ===============================
        // REFRESCAR MANUAL
        // ===============================
        private void btnActualizar_Click(object sender, EventArgs e)
        {
            CargarDeudas();
        }

        // ===============================
        // VER HISTORIAL
        // ===============================
        private void btnVerHistorial_Click(object sender, EventArgs e)
        {
            using var frm = new FrmHistorialDeudas();
            var owner = FindForm() ?? this;
            frm.ShowDialog(owner);
        }

        // ===============================
        // ENVIAR WHATSAPP (RECORDATORIO MANUAL)
        // ===============================
        private void btnEnviarWhatsApp_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvDeudas.CurrentRow == null)
                {
                    MessageBox.Show("Seleccione una deuda para enviar recordatorio.", "Aviso");
                    return;
                }

                // 🔥 VALIDAR COLUMNAS NECESARIAS
                if (!dgvDeudas.Columns.Contains("ClienteId") || 
                    !dgvDeudas.Columns.Contains("Estado") ||
                    !dgvDeudas.Columns.Contains("Nombre") ||
                    !dgvDeudas.Columns.Contains("Saldo") ||
                    !dgvDeudas.Columns.Contains("FechaVencimiento") ||
                    !dgvDeudas.Columns.Contains("DiasRestantes"))
                {
                    MessageBox.Show("Error de configuración del grid. Recargue el formulario.", "Error");
                    return;
                }

                int deudaId = ObtenerDeudaId();
                var row = dgvDeudas.CurrentRow;

                string estado = row.Cells["Estado"].Value?.ToString() ?? "";

                if (estado == "PAGADA")
                {
                    MessageBox.Show("Esta deuda ya está pagada. No se requiere recordatorio.", "Aviso");
                    return;
                }

                int clienteId = Convert.ToInt32(row.Cells["ClienteId"].Value);
                string nombre = row.Cells["Nombre"].Value?.ToString() ?? "Cliente";
                decimal saldo = Convert.ToDecimal(row.Cells["Saldo"].Value);
                DateTime vencimiento = Convert.ToDateTime(row.Cells["FechaVencimiento"].Value);
                int diasRestantes = Convert.ToInt32(row.Cells["DiasRestantes"].Value);

                bool enviado;
                if (diasRestantes < 0)
                    enviado = deudaBLL.EnviarNotificacionDeudaVencida(deudaId);
                else if (diasRestantes == 0)
                    enviado = deudaBLL.EnviarRecordatorioVenceHoy(deudaId);
                else
                    enviado = deudaBLL.EnviarRecordatorioVencimiento(deudaId);

                if (enviado)
                    MessageBox.Show($"Recordatorio enviado a {nombre} por WhatsApp.", "Exito");
                else
                {
                    string? error = deudaBLL.ObtenerUltimoErrorWhatsApp(clienteId);
                    MessageBox.Show(
                        $"No se pudo enviar WhatsApp a {nombre}.\n\n" +
                        (error ?? "Configure TwilioContentSidGenerico con plantilla aprobada en Twilio Console."),
                        "Aviso",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error enviando WhatsApp: {ex.Message}", "Error");
            }
        }


        // ===============================
        // LIMPIAR EVENTO (CRÍTICO)
        // ===============================
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            AppEventos.OnPagoRegistrado -= CargarDeudas;
            AppEventos.OnDeudaModificada -= CargarDeudas;
            base.OnFormClosing(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            AppEventos.OnPagoRegistrado -= CargarDeudas;
            AppEventos.OnDeudaModificada -= CargarDeudas;
            base.OnFormClosed(e);
        }
    }
}