using System;
using System.Data;
using System.Windows.Forms;
using BLL;
using BLL.Commands;
using CORE;
using UI.Helpers;
using UI.Theme;

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
        private int? _clienteHistorialId;
        private string _clienteHistorialNombre = string.Empty;

        public FrmDeudas() : this(null)
        {
        }

        public FrmDeudas(int? clienteIdPreseleccionado)
        {
            InitializeComponent();
            ThemeHost.Attach(this);

            if (clienteIdPreseleccionado.HasValue)
            {
                _clienteIdPreseleccionado = clienteIdPreseleccionado;
                _seleccionClientePendiente = true;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            EmbeddedFormHelper.CorregirSiEmbebido(this);
            base.OnLoad(e);
        }
        private void dgvDeudas_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.ColumnIndex >= dgvDeudas.Columns.Count)
                return;

            if (dgvDeudas.Rows.Count == 0)
                return;

            if (e.CellStyle == null)
                e.CellStyle = new DataGridViewCellStyle(dgvDeudas.DefaultCellStyle);

            string nombreColumna = dgvDeudas.Columns[e.ColumnIndex].Name;

            // 🔥 FORMATO COLUMNA ESTADO
            if (nombreColumna == "Estado" && e.Value != null)
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
            if (nombreColumna == "DiasRestantes" && e.Value != null)
            {
                if (e.Value is not int dias)
                {
                    if (!int.TryParse(e.Value.ToString(), out dias))
                        return;
                }

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
            if (nombreColumna == "Plan" && e.Value != null)
            {
                string plan = e.Value.ToString();
                if (plan != "N/A")
                {
                    e.CellStyle.BackColor = Color.AliceBlue;
                    e.CellStyle.ForeColor = Color.DarkBlue;
                    e.CellStyle.Font = new Font(dgvDeudas.Font, FontStyle.Bold);
                }
            }

            if (nombreColumna == "AporteInicial" && e.Value != null)
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

            // 🔥 MONTOS: EL SALDO MANDA. EN DEUDAS PAGADAS LOS IMPORTES SON HISTÓRICOS
            if (nombreColumna == "Saldo" && e.Value != null && e.Value != DBNull.Value)
            {
                decimal saldoFila = Convert.ToDecimal(e.Value);
                e.CellStyle.Font = new Font(dgvDeudas.Font, FontStyle.Bold);
                e.CellStyle.ForeColor = saldoFila > 0 ? Color.Firebrick : Color.Green;
            }

            if ((nombreColumna == "MontoTotal" || nombreColumna == "MontoPagado")
                && EsFilaSaldada(e.RowIndex))
            {
                e.CellStyle.ForeColor = Color.Gray;
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

        /// <summary>
        /// Una fila saldada ya no representa dinero por cobrar: sus importes se
        /// muestran atenuados para que no se lean como deuda vigente.
        /// </summary>
        private bool EsFilaSaldada(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dgvDeudas.Rows.Count)
                return false;

            if (!dgvDeudas.Columns.Contains("Saldo"))
                return false;

            var valor = dgvDeudas.Rows[rowIndex].Cells["Saldo"].Value;
            if (valor == null || valor == DBNull.Value)
                return false;

            return Convert.ToDecimal(valor) <= 0;
        }

        // ===============================
        // LOAD
        // ===============================
        private void FrmDeudas_Load(object sender, EventArgs e)
        {
            BusquedaFocusHelper.Wire(this);

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
            cmbFiltro.Items.Add("Vencidas");
            // "Todas" = activas + vencidas pendientes. Nunca se listan PAGADAS.
            cmbFiltro.SelectedIndex = 0;
        }

        // ===============================
        // CONFIGURAR GRID (PRO)
        // ===============================
        private void ConfigurarGrid()
        {
            dgvDeudas.AutoGenerateColumns = true;
            dgvDeudas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDeudas.MultiSelect = false;
            dgvDeudas.ReadOnly = true;
            dgvDeudas.AllowUserToAddRows = false;
            dgvDeudas.AllowUserToDeleteRows = false;
            dgvDeudas.RowHeadersVisible = false;
            dgvDeudas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDeudas.BorderStyle = BorderStyle.None;
            dgvDeudas.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            dgvDeudas.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;

            dgvDeudas.DefaultCellStyle.SelectionBackColor = ColorSeleccionGrid;
            dgvDeudas.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvDeudas.RowsDefaultCellStyle.SelectionBackColor = ColorSeleccionGrid;
            dgvDeudas.RowsDefaultCellStyle.SelectionForeColor = Color.White;
            dgvDeudas.AlternatingRowsDefaultCellStyle.SelectionBackColor = ColorSeleccionGrid;
            dgvDeudas.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.White;
            dgvDeudas.BackgroundColor = Color.White;
            dgvDeudas.EnableHeadersVisualStyles = false;
            dgvDeudas.ColumnHeadersDefaultCellStyle.BackColor = Color.Black;
            dgvDeudas.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;

            dgvDeudas.DataSource = _bsDeudas;
            dgvDeudas.DataBindingComplete += DgvDeudas_DespuesDeEnlazar;
        }

        private void DgvDeudas_DespuesDeEnlazar(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (IsDisposed || Disposing || dgvDeudas.IsDisposed)
                return;

            FormatearGrid();
        }

        private void DgvDeudas_HandleCreated(object? sender, EventArgs e)
        {
            dgvDeudas.HandleCreated -= DgvDeudas_HandleCreated;
            if (_bsDeudas.DataSource != null && dgvDeudas.Columns.Count > 0)
                FormatearGrid();
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
                    BeginInvoke(new Action(CargarDeudas));
                }
                catch (ObjectDisposedException)
                {
                }
                return;
            }

            if (dgvDeudas == null || dgvDeudas.IsDisposed)
                return;

            try
            {
                // Gestión de deudas: solo pendientes (ACTIVA / vencidas).
                // Las PAGADAS viven en Historial; aquí no deben aparecer.
                DataTable? dt = deudaBLL.ObtenerDeudas(incluirHistorial: false);
                if (dt == null)
                {
                    MessageBox.Show("No se pudieron obtener las deudas.", "Error");
                    return;
                }

                ExcluirDeudasPagadas(dt);

                if (!dt.Columns.Contains("DiasRestantes"))
                    dt.Columns.Add("DiasRestantes", typeof(int));

                foreach (DataRow row in dt.Rows)
                {
                    if (row["FechaVencimiento"] == DBNull.Value || row["FechaVencimiento"] == null)
                    {
                        row["DiasRestantes"] = 0;
                        continue;
                    }

                    DateTime fechaVencimiento = Convert.ToDateTime(row["FechaVencimiento"]);
                    row["DiasRestantes"] = (fechaVencimiento.Date - DateTime.Now.Date).Days;
                }

                _bsDeudas.DataSource = dt;
                AplicarFiltros();
                ActualizarResumenCliente(dt);

                if (!dgvDeudas.IsHandleCreated)
                {
                    dgvDeudas.HandleCreated -= DgvDeudas_HandleCreated;
                    dgvDeudas.HandleCreated += DgvDeudas_HandleCreated;
                }
            }
            catch (ObjectDisposedException)
            {
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

        /// <summary>
        /// Red de seguridad: aunque la consulta ya sea solo activas, elimina
        /// cualquier fila PAGADA / saldo 0 que pudiera colarse.
        /// </summary>
        private static void ExcluirDeudasPagadas(DataTable dt)
        {
            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                DataRow row = dt.Rows[i];
                string estado = row.Table.Columns.Contains("Estado")
                    ? (row["Estado"]?.ToString() ?? string.Empty).Trim().ToUpperInvariant()
                    : string.Empty;
                decimal saldo = row.Table.Columns.Contains("Saldo") && row["Saldo"] != DBNull.Value
                    ? Convert.ToDecimal(row["Saldo"])
                    : 0m;

                if (estado == "PAGADA" || estado == "ANULADA" || saldo <= 0m)
                    row.Delete();
            }

            dt.AcceptChanges();
        }

        // ===============================
        // APLICAR FILTROS (COMBO + BUSCADOR)
        // ===============================
        private void AplicarFiltros()
        {
            if (IsDisposed || Disposing || _bsDeudas == null)
                return;

            var filtros = new System.Collections.Generic.List<string>
            {
                // Base dura: gestión = solo pendientes.
                "Estado = 'ACTIVA' AND Saldo > 0"
            };

            // Doble clic en Nombre: pendientes del cliente (sin pagadas).
            if (_clienteHistorialId.HasValue)
            {
                filtros.Add($"ClienteId = {_clienteHistorialId.Value}");
                try
                {
                    _bsDeudas.Filter = string.Join(" AND ", filtros);
                }
                catch (ObjectDisposedException)
                {
                }
                return;
            }

            string filtroCombo = cmbFiltro?.SelectedItem?.ToString() ?? "Todas";
            switch (filtroCombo)
            {
                case "Activas":
                    filtros.Add("DiasRestantes >= 0");
                    break;
                case "Vencidas":
                    filtros.Add("DiasRestantes < 0");
                    break;
            }

            var termino = txtBuscar?.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(termino))
                filtros.Add("(" + BusquedaGridHelper.ConstruirFiltroDeudas(termino) + ")");

            try
            {
                _bsDeudas.Filter = string.Join(" AND ", filtros);
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            if (_clienteHistorialId.HasValue)
                return;

            AplicarFiltros();

            if (_seleccionClientePendiente)
                BeginInvoke(new Action(AplicarSeleccionClientePendiente));
        }

        // ===============================
        // EVENTO: CAMBIO DE FILTRO
        // ===============================
        private void cmbFiltro_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_clienteHistorialId.HasValue)
                return;

            CargarDeudas();
        }

        /// <summary>
        /// Enfoca todo el historial de deudas del cliente únicamente cuando el
        /// doble clic se realiza sobre la columna Nombre.
        /// </summary>
        private void dgvDeudas_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if (!string.Equals(
                    dgvDeudas.Columns[e.ColumnIndex].Name,
                    "Nombre",
                    StringComparison.OrdinalIgnoreCase))
                return;

            DataGridViewRow row = dgvDeudas.Rows[e.RowIndex];
            object? clienteIdValue = row.Cells["ClienteId"].Value;
            if (clienteIdValue == null || clienteIdValue == DBNull.Value)
                return;

            _clienteHistorialId = Convert.ToInt32(clienteIdValue);
            _clienteHistorialNombre = row.Cells["Nombre"].Value?.ToString()?.Trim() ?? "Cliente";

            // Solo pendientes del cliente (activas / vencidas). Sin PAGADAS.
            CargarDeudas();
        }

        // ===============================
        // MENÚ CONTEXTUAL (CLIC DERECHO)
        // ===============================
        /// <summary>
        /// El clic derecho enfoca la fila del miembro antes de abrir el menú, para que
        /// la acción siempre opere sobre la deuda que el usuario está señalando.
        /// </summary>
        private void dgvDeudas_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right || e.RowIndex < 0)
                return;

            EnfocarFila(e.RowIndex, e.ColumnIndex);
            cmsDeudas.Show(dgvDeudas, dgvDeudas.PointToClient(Cursor.Position));
        }

        private void EnfocarFila(int rowIndex, int columnIndex)
        {
            if (rowIndex < 0 || rowIndex >= dgvDeudas.Rows.Count)
                return;

            DataGridViewRow row = dgvDeudas.Rows[rowIndex];
            if (row.IsNewRow)
                return;

            dgvDeudas.ClearSelection();
            row.Selected = true;

            DataGridViewCell? celda =
                columnIndex >= 0 && dgvDeudas.Columns[columnIndex].Visible
                    ? row.Cells[columnIndex]
                    : PrimeraCeldaVisible(row);

            if (celda != null)
                dgvDeudas.CurrentCell = celda;
        }

        private static DataGridViewCell? PrimeraCeldaVisible(DataGridViewRow row)
        {
            foreach (DataGridViewCell celda in row.Cells)
            {
                if (celda.OwningColumn != null && celda.OwningColumn.Visible)
                    return celda;
            }

            return null;
        }

        // ===============================
        // EDITAR DEUDA
        // ===============================
        private void miEditarDeuda_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvDeudas.CurrentRow == null)
                {
                    MessageBox.Show("Seleccione una deuda para editar.", "Aviso");
                    return;
                }

                if (!dgvDeudas.Columns.Contains("Id") || !dgvDeudas.Columns.Contains("Estado"))
                {
                    MessageBox.Show("Error de configuración del grid. Recargue el formulario.", "Error");
                    return;
                }

                string estado = dgvDeudas.CurrentRow.Cells["Estado"].Value?.ToString()?.Trim() ?? "ACTIVA";
                if (!estado.Equals("ACTIVA", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show(
                        $"Solo se pueden editar deudas activas. Esta deuda está {estado.ToUpperInvariant()}.",
                        "Editar deuda",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                int deudaId = ObtenerDeudaId();

                using FrmEditarDeuda frm = new FrmEditarDeuda(deudaId);
                if (frm.ShowDialog(this) != DialogResult.OK)
                    return;

                CargarDeudas();
                SeleccionarDeuda(deudaId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al editar la deuda: {ex.Message}", "Error");
            }
        }

        /// <summary>
        /// Devuelve el foco a la deuda editada; si quedó saldada ya no está en el grid.
        /// </summary>
        private void SeleccionarDeuda(int deudaId)
        {
            if (!dgvDeudas.Columns.Contains("Id"))
                return;

            foreach (DataGridViewRow row in dgvDeudas.Rows)
            {
                if (row.IsNewRow)
                    continue;

                var valor = row.Cells["Id"].Value;
                if (valor == null || valor == DBNull.Value)
                    continue;

                if (Convert.ToInt32(valor) != deudaId)
                    continue;

                row.Selected = true;

                DataGridViewCell? celda = PrimeraCeldaVisible(row);
                if (celda != null)
                    dgvDeudas.CurrentCell = celda;

                dgvDeudas.FirstDisplayedScrollingRowIndex = row.Index;
                return;
            }
        }

        /// <summary>
        /// Resumen del cliente enfocado: solo saldos aún pendientes.
        /// </summary>
        private void ActualizarResumenCliente(DataTable? datos)
        {
            if (!_clienteHistorialId.HasValue || datos == null)
            {
                lblDebe.Visible = false;
                lblDebe.Text = string.Empty;
                return;
            }

            decimal saldoPendiente = 0m;

            foreach (DataRow row in datos.Rows)
            {
                if (row.RowState == DataRowState.Deleted)
                    continue;

                if (row["ClienteId"] == DBNull.Value ||
                    Convert.ToInt32(row["ClienteId"]) != _clienteHistorialId.Value)
                    continue;

                string estado = row.Table.Columns.Contains("Estado")
                    ? (row["Estado"]?.ToString() ?? string.Empty).Trim().ToUpperInvariant()
                    : "ACTIVA";
                decimal saldo = LeerMonto(row, "Saldo");
                if (estado != "ACTIVA" || saldo <= 0m)
                    continue;

                saldoPendiente += saldo;
            }

            lblDebe.Text = $"{_clienteHistorialNombre} debe RD$ {saldoPendiente:N2}";
            lblDebe.Visible = true;
            lblDebe.BringToFront();
        }

        private static decimal LeerMonto(DataRow row, string columna)
        {
            if (!row.Table.Columns.Contains(columna) || row[columna] == DBNull.Value)
                return 0m;

            return Convert.ToDecimal(row[columna]);
        }

        // ===============================
        // FORMATO VISUAL
        // ===============================
        private void FormatearGrid()
        {
            if (dgvDeudas.Columns.Count == 0) return;

            DataGridViewHelper.RunColumnLayout(dgvDeudas, () =>
            {
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
                DataGridViewHelper.SetColumnWidth(col, 120);
            });

            DataGridViewHelper.ConfigureColumn(dgvDeudas, "OrigenPrecio", col =>
            {
                col.HeaderText = "Origen";
                DataGridViewHelper.SetColumnWidth(col, 90);
            });

            DataGridViewHelper.ConfigureColumn(dgvDeudas, "PrecioTotal", col =>
            {
                col.DefaultCellStyle.Format = "N2";
                col.HeaderText = "Precio Total";
            });

            DataGridViewHelper.ConfigureColumn(dgvDeudas, "MontoTotal", col =>
            {
                col.DefaultCellStyle.Format = "N2";
                col.HeaderText = "Capital Deuda";
            });

            DataGridViewHelper.ConfigureColumn(dgvDeudas, "MontoPagado", col =>
            {
                col.DefaultCellStyle.Format = "N2";
                col.HeaderText = "Pagado";
            });

            DataGridViewHelper.ConfigureColumn(dgvDeudas, "Saldo", col =>
            {
                col.DefaultCellStyle.Format = "N2";
                col.HeaderText = "Saldo Pendiente";
                col.DefaultCellStyle.Font = new Font(dgvDeudas.Font, FontStyle.Bold);
            });

            DataGridViewHelper.ConfigureColumn(dgvDeudas, "FechaVencimiento", col =>
            {
                col.DefaultCellStyle.Format = "dd/MM/yyyy";
                col.HeaderText = "Fecha Límite Pago";
                DataGridViewHelper.SetColumnWidth(col, 130);
            });

            DataGridViewHelper.ConfigureColumn(dgvDeudas, "FechaInicioMembresia", col =>
            {
                col.DefaultCellStyle.Format = "dd/MM/yyyy";
                col.HeaderText = "Inicio Plan";
                DataGridViewHelper.SetColumnWidth(col, 100);
            });

            DataGridViewHelper.ConfigureColumn(dgvDeudas, "FechaFinMembresia", col =>
            {
                col.DefaultCellStyle.Format = "dd/MM/yyyy";
                col.HeaderText = "Vence Plan";
                DataGridViewHelper.SetColumnWidth(col, 100);
            });

            DataGridViewHelper.ConfigureColumn(dgvDeudas, "Plan", col =>
            {
                col.HeaderText = "Plan Financiado";
                DataGridViewHelper.SetColumnWidth(col, 120);
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
            DataGridViewHelper.SetDisplayIndex(dgvDeudas, "OrigenPrecio", 3);
            DataGridViewHelper.SetDisplayIndex(dgvDeudas, "AporteInicial", 4);
            DataGridViewHelper.SetDisplayIndex(dgvDeudas, "PrecioTotal", 5);
            DataGridViewHelper.SetDisplayIndex(dgvDeudas, "MontoTotal", 6);
            DataGridViewHelper.SetDisplayIndex(dgvDeudas, "MontoPagado", 7);
            DataGridViewHelper.SetDisplayIndex(dgvDeudas, "Saldo", 8);
            DataGridViewHelper.SetDisplayIndex(dgvDeudas, "FechaVencimiento", 9);
            DataGridViewHelper.SetDisplayIndex(dgvDeudas, "DiasRestantes", 10);
            DataGridViewHelper.SetDisplayIndex(dgvDeudas, "Estado", 11);
            DataGridViewHelper.SetDisplayIndex(dgvDeudas, "FechaInicioMembresia", 12);
            DataGridViewHelper.SetDisplayIndex(dgvDeudas, "FechaFinMembresia", 13);
            });

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
            if (ModuloDeudasHost.AbrirCrearDeuda(this))
                return;

            MessageBox.Show(
                "Abra el módulo de Deudas y use la pestaña \"Nueva Deuda\".",
                "Nueva deuda",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
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
                decimal saldo = Convert.ToDecimal(row.Cells["Saldo"].Value);
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

                FrmPagarDeudas frm = new FrmPagarDeudas(nombre, saldo, estado, ultimoPago);

                if (frm.ShowDialog() == DialogResult.OK)
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

                    CargarDeudas();
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
            _clienteHistorialId = null;
            _clienteHistorialNombre = string.Empty;
            lblDebe.Visible = false;
            lblDebe.Text = string.Empty;
            CargarDeudas();
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
                    !dgvDeudas.Columns.Contains("Nombre"))
                {
                    MessageBox.Show("Error de configuración del grid. Recargue el formulario.", "Error");
                    return;
                }

                var row = dgvDeudas.CurrentRow;

                if (row.Cells["ClienteId"].Value == null || row.Cells["ClienteId"].Value == DBNull.Value)
                {
                    MessageBox.Show("Seleccione una deuda válida.", "Aviso");
                    return;
                }

                int clienteId = Convert.ToInt32(row.Cells["ClienteId"].Value);
                string nombre = row.Cells["Nombre"].Value?.ToString() ?? "Cliente";

                // Un solo mensaje con TODO lo que debe el miembro (membresía y
                // producto a crédito), con la fecha de cada financiamiento.
                bool enviado;
                Cursor = Cursors.WaitCursor;
                try
                {
                    enviado = deudaBLL.EnviarResumenDeudasCliente(clienteId);
                }
                finally
                {
                    Cursor = Cursors.Default;
                }

                if (enviado)
                {
                    MessageBox.Show(
                        $"Estado de cuenta enviado a {nombre} por WhatsApp.",
                        "Exito",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    string? error = deudaBLL.UltimoDetalleWhatsApp
                                    ?? deudaBLL.ObtenerUltimoErrorWhatsApp(clienteId);
                    MessageBox.Show(
                        $"No se pudo enviar WhatsApp a {nombre}.\n\n" +
                        (error ?? "Revise la plantilla de Twilio y el teléfono del miembro."),
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
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            AppEventos.OnPagoRegistrado -= CargarDeudas;
            AppEventos.OnDeudaModificada -= CargarDeudas;
            if (dgvDeudas != null)
            {
                dgvDeudas.DataBindingComplete -= DgvDeudas_DespuesDeEnlazar;
                dgvDeudas.HandleCreated -= DgvDeudas_HandleCreated;
            }
            base.OnFormClosed(e);
        }
    }
}