using DTO;
using BLL;
using BLL.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Data;
using CORE;
using UI.Helpers;
using UI.Theme;
using UI;

namespace UI.DISEÑO
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmEstadoClientes : Form
    {
        // ===============================
        // INSTANCIAS
        // ===============================
        private readonly EstadoBLL estadoBLL = new EstadoBLL();
        private readonly MembresiaBLL membresiaBLL = new MembresiaBLL();
        private readonly PlanBLL planBLL = new PlanBLL();
        private readonly FrmPresentacion _presentacion;
        private readonly MensajeAutomaticoBLL mensajeBLL = new MensajeAutomaticoBLL();
        private readonly ReporteBLL reporteBLL = new ReporteBLL();
        private readonly BindingSource _bsEstado = new BindingSource();
        private static readonly CultureInfo CulturaDo = CultureInfo.GetCultureInfo("es-DO");
        private ContextMenuStrip? _menuEstado;
        private ToolStripMenuItem? _mnuAjustarFechaFin;

        // ===============================
        // CONTROL
        // ===============================
        private System.Windows.Forms.Timer timerActualizacion = new System.Windows.Forms.Timer();
        private bool cargando = false;
        private bool _estadoUiInicializado;

        /// <summary>Constructor para el diseñador de WinForms.</summary>
        public FrmEstadoClientes()
        {
            InitializeComponent();
            ThemeHost.Attach(this);
            _presentacion = null!;
        }

        public FrmEstadoClientes(FrmPresentacion presentacion)
        {
            InitializeComponent();
            ThemeHost.Attach(this);
            _presentacion = presentacion;
            if (ThemeHost.IsDesignTime())
                return;

            ModuloNavBar.Wire(panelNav, this, ModuloNavBar.ModuloEstado);
            // Columnas vienen del DataSource; AutoGenerate solo en runtime.
            dgvEstado.AutoGenerateColumns = true;
        }

        // ===============================
        // OBTENER CLIENTE
        // ===============================
        private int ObtenerClienteSeleccionado()
        {
            if (!TryObtenerClienteDeFila(dgvEstado.CurrentRow, out int id))
                return 0;
            return id;
        }

        private static DataGridViewCell? ObtenerCelda(DataGridViewRow row, string nombreColumna)
        {
            if (row.DataGridView == null)
                return null;

            if (row.DataGridView.Columns.Contains(nombreColumna))
                return row.Cells[nombreColumna];

            foreach (DataGridViewColumn col in row.DataGridView.Columns)
            {
                if (string.Equals(col.Name, nombreColumna, StringComparison.OrdinalIgnoreCase))
                    return row.Cells[col.Index];
            }

            return null;
        }

        private static string ObtenerValorCelda(DataGridViewRow row, string nombreColumna)
        {
            return ObtenerCelda(row, nombreColumna)?.Value?.ToString() ?? "";
        }

        // ===============================
        // LOAD
        // ===============================
        private void FrmEstadoClientes_Load(object? sender, EventArgs e)
        {
            if (_estadoUiInicializado || ThemeHost.IsDesignTime())
                return;

            _estadoUiInicializado = true;

            InicializarComboMesesPanel();

            AppEventos.OnPagoRegistrado -= OnDatosEstadoCambiaron;
            AppEventos.OnDeudaModificada -= OnDatosEstadoCambiaron;
            AppEventos.OnPagoRegistrado += OnDatosEstadoCambiaron;
            AppEventos.OnDeudaModificada += OnDatosEstadoCambiaron;

            CargarEstado();

            ConfigurarMenuContextualEstado();

            timerActualizacion.Interval = 30000;
            timerActualizacion.Tick -= TimerActualizacion_Tick;
            timerActualizacion.Tick += TimerActualizacion_Tick;
            timerActualizacion.Start();

            // Rojo fijo; no enganchar BackColorChanged→Load (congelaba con el tema/hover).
            btnDesactivar.BackColor = Color.Red;
            btnDesactivar.ForeColor = Color.White;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            AppEventos.OnPagoRegistrado -= OnDatosEstadoCambiaron;
            AppEventos.OnDeudaModificada -= OnDatosEstadoCambiaron;
            timerActualizacion.Stop();
            base.OnFormClosed(e);
        }

        private void OnDatosEstadoCambiaron()
        {
            CargarEstado();
        }

        private void TimerActualizacion_Tick(object? sender, EventArgs e) 
        {
            CargarEstado();
        }

        // ===============================
        // CARGAR DATA
        // ===============================
        private void CargarEstado()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(CargarEstado));
                return;
            }

            if (cargando) return;

            try
            {
                cargando = true;
                dgvEstado.SuspendLayout();
                dgvEstado.CellFormatting -= dgvEstado_CellFormatting;

                membresiaBLL.ActualizarVencimientos();

                DataTable tabla = estadoBLL.ObtenerEstadoClientes()
                    ?? throw new InvalidOperationException("No se pudo obtener el estado de clientes.");

                AsegurarColumnasEstado(tabla);

                _bsEstado.DataSource = tabla;
                dgvEstado.DataSource = _bsEstado;
                AplicarFiltroBusqueda();
                FormatearGrid();
                ActualizarKpisSegunPeriodo(tabla);
                ActualizarEtiquetaTiempo();
                dgvEstado.ClearSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar estado: " + ex.Message);
            }
            finally
            {
                dgvEstado.CellFormatting -= dgvEstado_CellFormatting;
                dgvEstado.CellFormatting += dgvEstado_CellFormatting;
                dgvEstado.ResumeLayout();
                cargando = false;
            }
        }

        /// <summary>M-A y MENSUALIDAD → bucket MENSUALIDAD; planes especiales con bucket propio.</summary>
        private static string ClasificarPlanKpi(string nombrePlan)
        {
            string n = (nombrePlan ?? string.Empty).Trim();
            if (string.Equals(n, "M-A", StringComparison.OrdinalIgnoreCase)
                || string.Equals(n, "MENSUALIDAD", StringComparison.OrdinalIgnoreCase))
                return "MENSUALIDAD";
            if (string.Equals(n, "PREMIUM", StringComparison.OrdinalIgnoreCase))
                return "PREMIUM";
            if (string.Equals(n, "PRO", StringComparison.OrdinalIgnoreCase))
                return "PRO";
            if (string.Equals(n, "3x", StringComparison.OrdinalIgnoreCase)
                || string.Equals(n, "3X", StringComparison.OrdinalIgnoreCase))
                return "3X";
            if (string.Equals(n, "ABDOMEN PLANO", StringComparison.OrdinalIgnoreCase))
                return "ABDOMEN PLANO";
            if (string.Equals(n, "GLUTEOS GRANDE", StringComparison.OrdinalIgnoreCase))
                return "GLUTEOS GRANDE";
            return string.Empty;
        }

        private Dictionary<string, decimal> ObtenerPreciosPlanes()
        {
            var map = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            try
            {
                DataTable planes = planBLL.ObtenerPlanes();
                if (planes == null || !planes.Columns.Contains("Nombre") || !planes.Columns.Contains("Precio"))
                    return map;

                foreach (DataRow row in planes.Rows)
                {
                    string nombre = Convert.ToString(row["Nombre"])?.Trim() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(nombre))
                        continue;
                    decimal precio = row["Precio"] == DBNull.Value ? 0m : Convert.ToDecimal(row["Precio"]);
                    map[nombre] = precio;
                }
            }
            catch
            {
                // KPI degradado a 0 si no hay catálogo.
            }

            return map;
        }

        private static void SetKpi(Label? label, string text)
        {
            if (label == null || label.IsDisposed)
                return;
            label.Text = text;
        }

        private static void AsegurarColumnasEstado(DataTable tabla)
        {
            string[] requeridas =
            {
                "ID", "Nombre", "Membresia", "FechaInicio", "FechaFin", "Estado",
                "EstadoDeuda", "SaldoPendiente", "MontoFinanciado", "VencimientoDeuda"
            };

            foreach (string col in requeridas)
            {
                if (!tabla.Columns.Contains(col))
                    throw new InvalidOperationException(
                        $"La consulta de estado no devolvió la columna '{col}'.");
            }
        }

        // ===============================
        // 🆕 FORMATEAR GRID CON COLUMNAS DE FINANCIAMIENTO
        // ===============================
        private void FormatearGrid()
        {
            try
            {
                DataGridViewHelper.HideColumn(dgvEstado, "ID");

                DataGridViewHelper.ConfigureColumn(dgvEstado, "Nombre", col =>
                {
                    col.HeaderText = "Cliente";
                    col.Width = 200;
                });

                DataGridViewHelper.ConfigureColumn(dgvEstado, "FechaInicio", col =>
                {
                    col.HeaderText = "Inicio";
                    col.DefaultCellStyle.Format = "dd/MM/yyyy";
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    col.Width = 110;
                    col.MinimumWidth = 110;
                });

                DataGridViewHelper.ConfigureColumn(dgvEstado, "FechaFin", col =>
                {
                    col.HeaderText = "Vencimiento";
                    col.DefaultCellStyle.Format = "dd/MM/yyyy";
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    col.Width = 110;
                    col.MinimumWidth = 110;
                });

                DataGridViewHelper.ConfigureColumn(dgvEstado, "Membresia", col =>
                {
                    col.HeaderText = "Plan";
                    col.Width = 130;
                    col.MinimumWidth = 100;
                });

                DataGridViewHelper.ConfigureColumn(dgvEstado, "Estado", col =>
                {
                    col.HeaderText = "Estado";
                    col.Width = 160;
                });

                DataGridViewHelper.ConfigureColumn(dgvEstado, "EstadoDeuda", col =>
                {
                    col.HeaderText = "Estado Deuda";
                    col.Width = 110;
                    col.DefaultCellStyle.Font = new Font(dgvEstado.Font, FontStyle.Bold);
                });

                DataGridViewHelper.ConfigureColumn(dgvEstado, "SaldoPendiente", col =>
                {
                    col.HeaderText = "Saldo Pendiente";
                    col.DefaultCellStyle.Format = "C2";
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    col.Width = 120;
                    col.DefaultCellStyle.ForeColor = Color.Red;
                    col.DefaultCellStyle.Font = new Font(dgvEstado.Font, FontStyle.Bold);
                });

                DataGridViewHelper.ConfigureColumn(dgvEstado, "MontoFinanciado", col =>
                {
                    col.HeaderText = "Monto Financiado";
                    col.DefaultCellStyle.Format = "C2";
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    col.Width = 130;
                });

                DataGridViewHelper.ConfigureColumn(dgvEstado, "VencimientoDeuda", col =>
                {
                    col.HeaderText = "Vence Deuda";
                    col.DefaultCellStyle.Format = "dd/MM/yyyy";
                    col.Width = 100;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al formatear grid: {ex.Message}");
            }
        }

        private void AplicarFiltroBusqueda()
        {
            var termino = txtBuscar.Text.Trim();
            _bsEstado.Filter = string.IsNullOrEmpty(termino)
                ? null
                : BusquedaGridHelper.ConstruirFiltroEstadoClientes(termino);
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            AplicarFiltroBusqueda();
        }

        private void dgvEstado_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (dgvEstado.CurrentRow == null)
                {
                    btnRenovar.Enabled = false;
                    btnCongelar.Enabled = false;
                    btnProgramar.Enabled = false;
                    return;
                }

                string estado = ObtenerValorCelda(dgvEstado.CurrentRow, "Estado");
                btnRenovar.Enabled =
                    estado.Equals("VENCIDO", StringComparison.OrdinalIgnoreCase) ||
                    estado.Equals("DESACTIVADO", StringComparison.OrdinalIgnoreCase) ||
                    estado.Equals("SIN MEMBRESIA", StringComparison.OrdinalIgnoreCase);
                btnCongelar.Enabled =
                    estado.Equals("ACTIVO", StringComparison.OrdinalIgnoreCase) ||
                    estado.Equals("CONGELADO", StringComparison.OrdinalIgnoreCase);
                btnProgramar.Enabled =
                    estado.Equals("ACTIVO", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en selección del grid: {ex.Message}");
                btnRenovar.Enabled = false;
                btnCongelar.Enabled = false;
                btnProgramar.Enabled = false;
            }
        }

        private void dgvEstado_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            var fila = dgvEstado.Rows[e.RowIndex];
            string estadoDeuda = ObtenerValorCelda(fila, "EstadoDeuda");

            if (!estadoDeuda.Equals("ACTIVA", StringComparison.OrdinalIgnoreCase))
                return;

            if (!TryObtenerClienteDeFila(fila, out int clienteId))
                return;

            AbrirGestionDeudasCliente(clienteId);
        }

        private void ConfigurarMenuContextualEstado()
        {
            if (_menuEstado != null)
                return;

            _menuEstado = new ContextMenuStrip();
            _mnuAjustarFechaFin = new ToolStripMenuItem("Modificar fecha de vencimiento…");
            _mnuAjustarFechaFin.Click += mnuAjustarFechaFin_Click;
            _menuEstado.Items.Add(_mnuAjustarFechaFin);
            dgvEstado.ContextMenuStrip = _menuEstado;
        }

        private void dgvEstado_CellMouseDown(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right || e.RowIndex < 0 || dgvEstado.Columns.Count == 0)
                return;

            dgvEstado.ClearSelection();
            dgvEstado.Rows[e.RowIndex].Selected = true;
            int col = e.ColumnIndex >= 0 ? e.ColumnIndex : 0;
            if (col >= dgvEstado.Columns.Count)
                col = 0;
            if (dgvEstado.Rows[e.RowIndex].Cells[col].Visible)
                dgvEstado.CurrentCell = dgvEstado.Rows[e.RowIndex].Cells[col];
            else
            {
                foreach (DataGridViewCell cell in dgvEstado.Rows[e.RowIndex].Cells)
                {
                    if (cell.Visible && cell.OwningColumn.Visible)
                    {
                        dgvEstado.CurrentCell = cell;
                        break;
                    }
                }
            }
        }

        private void mnuAjustarFechaFin_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!TryObtenerClienteDeFila(dgvEstado.CurrentRow, out int clienteId))
                {
                    MessageBox.Show(this, "Selecciona un cliente.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string estado = ObtenerValorCelda(dgvEstado.CurrentRow!, "Estado");
                if (!estado.Equals("ACTIVO", StringComparison.OrdinalIgnoreCase)
                    && !estado.Equals("VENCIDO", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show(this,
                        "Solo se puede ajustar la fecha de vencimiento en miembros ACTIVO o VENCIDO.\n" +
                        "Estado actual: " + estado + ".",
                        "Ajuste de fecha",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                string nombre = ObtenerValorCelda(dgvEstado.CurrentRow!, "Nombre");
                DateTime fechaActual = DateTime.Today;
                var celdaFin = ObtenerCelda(dgvEstado.CurrentRow!, "FechaFin");
                if (celdaFin?.Value != null && celdaFin.Value != DBNull.Value)
                    fechaActual = Convert.ToDateTime(celdaFin.Value).Date;

                using var dlg = new FrmAjustarFechaVencimiento(nombre, fechaActual);
                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;

                var (anterior, nueva) = membresiaBLL.AjustarFechaFinMembresia(
                    clienteId, dlg.FechaNueva, Sesion.Usuario);

                MessageBox.Show(this,
                    $"Vencimiento actualizado: {anterior:dd/MM/yyyy} → {nueva:dd/MM/yyyy}.",
                    "Éxito",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                CargarEstado();
                _presentacion?.CargarDashboard();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "No se pudo modificar la fecha: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static bool TryObtenerClienteDeFila(DataGridViewRow? fila, out int clienteId)
        {
            clienteId = 0;
            if (fila == null || fila.IsNewRow)
                return false;

            if (fila.DataBoundItem is DataRowView drv)
            {
                if (drv.Row.Table.Columns.Contains("ID")
                    && drv["ID"] != null
                    && drv["ID"] != DBNull.Value
                    && int.TryParse(drv["ID"].ToString(), out clienteId)
                    && clienteId > 0)
                {
                    return true;
                }
            }

            var cell = ObtenerCelda(fila, "ID");
            if (cell?.Value == null || cell.Value == DBNull.Value)
                return false;

            return int.TryParse(cell.Value.ToString(), out clienteId) && clienteId > 0;
        }

        private void AbrirGestionDeudasCliente(int clienteId)
        {
            try
            {
                using var frm = new FrmModuloDeudas(clienteId);
                frm.ShowDialog();
                CargarEstado();
                _presentacion?.CargarDashboard();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al abrir gestión de deudas: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ===============================
        // COLORES
        // ===============================
        private void dgvEstado_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (cargando || e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if (dgvEstado.Columns.Count == 0 || e.RowIndex >= dgvEstado.Rows.Count)
                return;

            DataGridViewRow fila = dgvEstado.Rows[e.RowIndex];
            if (fila.IsNewRow)
                return;

            string estado = ObtenerValorCelda(fila, "Estado").ToUpperInvariant();

            if (estado == "VENCIDO")
                fila.DefaultCellStyle.BackColor = Color.LightCoral;
            else if (estado == "ACTIVO Y PROGRAMADO")
                fila.DefaultCellStyle.BackColor = Color.PaleGreen;
            else if (estado == "ACTIVO")
                fila.DefaultCellStyle.BackColor = Color.LightGreen;
            else if (estado == "CONGELADO")
                fila.DefaultCellStyle.BackColor = Color.LightSkyBlue;
            else if (estado == "DESACTIVADO" || estado == "SIN MEMBRESIA")
                fila.DefaultCellStyle.BackColor = Color.LightGray;

            string colName = dgvEstado.Columns[e.ColumnIndex].Name;

            // Etiquetas legibles en la columna Estado (datos internos: VENCIDO / DESACTIVADO)
            if (string.Equals(colName, "Estado", StringComparison.OrdinalIgnoreCase))
            {
                if (estado == "VENCIDO")
                {
                    e.Value = "CLIENTE VENCIDO";
                    e.FormattingApplied = true;
                }
                else if (estado == "DESACTIVADO")
                {
                    e.Value = "CLIENTE DESACTIVADO";
                    e.FormattingApplied = true;
                }
                else if (estado == "CONGELADO")
                {
                    e.Value = "CLIENTE CONGELADO";
                    e.FormattingApplied = true;
                }
                else if (estado == "ACTIVO Y PROGRAMADO")
                {
                    e.Value = "ACTIVO Y PROGRAMADO";
                    e.FormattingApplied = true;
                }
            }

            // Fechas solo dd/MM/yyyy (evita desalineación por hora/offset)
            if ((string.Equals(colName, "FechaInicio", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(colName, "FechaFin", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(colName, "VencimientoDeuda", StringComparison.OrdinalIgnoreCase))
                && e.Value != null && e.Value != DBNull.Value
                && e.Value is DateTime dt)
            {
                e.Value = dt.ToString("dd/MM/yyyy");
                e.FormattingApplied = true;
                e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            if (string.Equals(colName, "EstadoDeuda", StringComparison.OrdinalIgnoreCase))
            {
                string estadoDeuda = e.Value?.ToString() ?? "";
                if (estadoDeuda == "ACTIVA")
                {
                    e.CellStyle.BackColor = Color.LightYellow;
                    e.CellStyle.ForeColor = Color.DarkOrange;
                    e.CellStyle.Font = new Font(dgvEstado.Font, FontStyle.Bold);
                }
            }

            if (string.Equals(colName, "SaldoPendiente", StringComparison.OrdinalIgnoreCase))
            {
                if (e.Value != null && decimal.TryParse(e.Value.ToString(), out decimal saldo) && saldo > 0)
                {
                    e.CellStyle.BackColor = Color.MistyRose;
                    e.CellStyle.ForeColor = Color.DarkRed;
                    e.CellStyle.Font = new Font(dgvEstado.Font, FontStyle.Bold);
                }
            }
        }

        // ===============================
        // RENOVAR CLIENTE
        // ===============================
        private void btnAñadirMiembro_Click(object sender, EventArgs e)
        {
            try
            {
                using var frm = new UI.DISEÑO.ESTADO.FrmAñadirMiembro();
                frm.ShowDialog(this);
                if (frm.CambioRealizado)
                {
                    CargarEstado();
                    _presentacion?.CargarDashboard();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Error al abrir Añadir miembro: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnRenovar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!TryObtenerClienteDeFila(dgvEstado.CurrentRow, out int clienteId))
                {
                    MessageBox.Show("Selecciona un cliente.");
                    return;
                }

                string nombre = ObtenerValorCelda(dgvEstado.CurrentRow!, "Nombre");

                RenovacionMembresiaDialog.Mostrar(this, clienteId, nombre, () =>
                {
                    CargarEstado();
                    _presentacion?.CargarDashboard();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al renovar: " + ex.Message);
            }
        }

        // ===============================
        // CONGELAR / ACTIVAR
        // ===============================
        private void btnCongelar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!TryObtenerClienteDeFila(dgvEstado.CurrentRow, out int clienteId))
                {
                    MessageBox.Show("Selecciona un cliente.");
                    return;
                }

                string nombreCliente = ObtenerValorCelda(dgvEstado.CurrentRow!, "Nombre");
                using var frm = new FrmCongelarMiembro(clienteId, nombreCliente);
                frm.ShowDialog(this);
                if (frm.CambioRealizado)
                {
                    CargarEstado();
                    _presentacion?.CargarDashboard();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al congelar miembro: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnProgramar_Click(object sender, EventArgs e)
        {
            try
            {
                var miembros = ObtenerMiembrosActivosDesdeGrid();
                if (miembros.Count == 0)
                {
                    MessageBox.Show(this,
                        "No hay miembros ACTIVOS en el grid para programar.",
                        "Programación",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                TryObtenerClienteDeFila(dgvEstado.CurrentRow, out int seleccionado);

                using var frm = new UI.DISEÑO.ESTADO.FrmProgramacion(miembros, seleccionado);
                frm.ShowDialog(this);
                if (frm.ProgramacionCompletada)
                {
                    CargarEstado();
                    _presentacion?.CargarDashboard();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Error al abrir programación: " + ex.Message,
                    "Programación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private List<ProgramacionMiembroItemDTO> ObtenerMiembrosActivosDesdeGrid()
        {
            var lista = new List<ProgramacionMiembroItemDTO>();
            foreach (DataGridViewRow fila in dgvEstado.Rows)
            {
                if (fila.IsNewRow)
                    continue;

                string estado = ObtenerValorCelda(fila, "Estado");
                if (!estado.Equals("ACTIVO", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!TryObtenerClienteDeFila(fila, out int clienteId))
                    continue;

                DateTime vence = DateTime.Today;
                var celdaFin = ObtenerCelda(fila, "FechaFin");
                if (celdaFin?.Value != null && celdaFin.Value != DBNull.Value)
                    vence = Convert.ToDateTime(celdaFin.Value).Date;

                lista.Add(new ProgramacionMiembroItemDTO
                {
                    ClienteId = clienteId,
                    Nombre = ObtenerValorCelda(fila, "Nombre"),
                    Membresia = ObtenerValorCelda(fila, "Membresia"),
                    FechaVencimiento = vence
                });
            }

            return lista
                .OrderBy(m => m.Nombre, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        // ===============================
        // DESACTIVAR CLIENTE
        // ===============================
        private void btnDesactivar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!TryObtenerClienteDeFila(dgvEstado.CurrentRow, out int clienteId))
                {
                    MessageBox.Show("Selecciona un cliente.");
                    return;
                }

                string nombreCliente = ObtenerValorCelda(dgvEstado.CurrentRow!, "Nombre");
                string estadoActual = ObtenerValorCelda(dgvEstado.CurrentRow!, "Estado");

                ModoDesactivacionMiembro? modo = DesactivacionMiembroDialog.Mostrar(this, nombreCliente);
                if (modo == null)
                    return;

                if (estadoActual.Equals("DESACTIVADO", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show(
                        "El cliente ya está DESACTIVADO.",
                        "Desactivar",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                string usuario = Sesion.Usuario ?? "ADMIN";

                string motivo = Microsoft.VisualBasic.Interaction.InputBox(
                    "¿Por qué se va el cliente?",
                    "Motivo de salida",
                    "Sin especificar"
                );

                // InputBox: Cancel → ""; Aceptar con default → "Sin especificar".
                if (motivo.Length == 0)
                    return;

                if (string.IsNullOrWhiteSpace(motivo))
                    motivo = "Sin especificar";

                Cursor = Cursors.WaitCursor;
                int resultado;
                try
                {
                    resultado = membresiaBLL.DesactivarMiembro(
                        clienteId,
                        usuario,
                        motivo,
                        ModoDesactivacionMiembro.SinMembresia);
                }
                finally
                {
                    Cursor = Cursors.Default;
                }

                if (resultado <= 0)
                {
                    MessageBox.Show(
                        "No se pudo registrar la baja del cliente.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Refrescar primero: el usuario ve el cambio aunque WhatsApp tarde.
                CargarEstado();
                _presentacion?.CargarDashboard();

                MessageBox.Show("Miembro desactivado correctamente.\nEstado: CLIENTE DESACTIVADO.");

                int clienteWhatsApp = clienteId;
                string motivoWhatsApp = motivo;
                System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        mensajeBLL.EnviarMensajeDesactivacion(clienteWhatsApp, motivoWhatsApp);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("WhatsApp desactivación: " + ex.Message);
                    }
                });
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                MessageBox.Show(
                    "Error al desactivar miembro: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ===============================
        // PDF ESTADO CLIENTES
        // ===============================
        private void btnDescargarPDF_Click(object? sender, EventArgs e)
        {
            var periodo = SeleccionPeriodoEstadoDialog.Mostrar(this);
            if (periodo == null)
                return;

            try
            {
                DataTable detalle;
                DataTable resumen;
                string etiqueta = periodo.Etiqueta;

                if (periodo.EsHoy)
                {
                    detalle = estadoBLL.ObtenerDetalleActivosReporte();
                    resumen = estadoBLL.ObtenerResumenDesdeDetalle(detalle);
                    etiqueta = "HOY";
                }
                else
                {
                    detalle = estadoBLL.ObtenerDetalleMembresiasPorMes(periodo.Anio, periodo.Mes);
                    resumen = estadoBLL.ObtenerResumenPlanesPorMes(periodo.Anio, periodo.Mes);
                }

                if (detalle.Rows.Count == 0)
                {
                    MessageBox.Show(this,
                        "No hay datos para el período seleccionado.",
                        "Reporte PDF",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                string slug = periodo.EsHoy
                    ? "Hoy"
                    : $"{periodo.Anio}{periodo.Mes:00}";

                using var sfd = new SaveFileDialog
                {
                    Filter = "PDF (*.pdf)|*.pdf",
                    DefaultExt = "pdf",
                    AddExtension = true,
                    FileName = $"EstadoClientes_{slug}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
                };

                if (sfd.ShowDialog(this) != DialogResult.OK)
                    return;

                Cursor = Cursors.WaitCursor;
                DateTime generado = DateTime.Now;
                reporteBLL.GenerarPdfEstadoClientes(
                    resumen,
                    detalle,
                    etiqueta,
                    periodo.EsHoy,
                    generado,
                    sfd.FileName);

                DialogResult abrir = MessageBox.Show(this,
                    $"PDF generado correctamente.\n\n{sfd.FileName}\n\n¿Desea abrirlo?",
                    "Reporte PDF",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (abrir == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = sfd.FileName,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "No se pudo generar el PDF: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        // ===============================
        // KPIs POR MES (cbmMesesPanel + lblTiempo)
        // ===============================
        private sealed class OpcionMesPanel
        {
            public bool EsHoy { get; init; }
            public int Mes { get; init; }
            public int Anio { get; init; }
            public string Etiqueta { get; init; } = string.Empty;

            public override string ToString() => Etiqueta;
        }

        private bool _cmbMesesInicializado;

        private void InicializarComboMesesPanel()
        {
            if (_cmbMesesInicializado || cbmMesesPanel == null || cbmMesesPanel.IsDisposed)
                return;

            _cmbMesesInicializado = true;
            cbmMesesPanel.DropDownStyle = ComboBoxStyle.DropDownList;

            int anio = DateTime.Today.Year;
            var items = new object[13];
            items[0] = new OpcionMesPanel { EsHoy = true, Anio = anio, Etiqueta = "HOY" };

            for (int mes = 1; mes <= 12; mes++)
            {
                string nombre = CulturaDo.DateTimeFormat.GetMonthName(mes);
                if (!string.IsNullOrEmpty(nombre))
                    nombre = char.ToUpper(nombre[0], CulturaDo) + nombre[1..];

                items[mes] = new OpcionMesPanel
                {
                    EsHoy = false,
                    Mes = mes,
                    Anio = anio,
                    Etiqueta = $"{nombre} {anio}"
                };
            }

            cbmMesesPanel.Items.AddRange(items);
            cbmMesesPanel.SelectedIndex = 0;
            cbmMesesPanel.SelectedIndexChanged += cbmMesesPanel_SelectedIndexChanged;
            ActualizarEtiquetaTiempo();
        }

        private void cbmMesesPanel_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cargando || !_cmbMesesInicializado)
                return;

            ActualizarEtiquetaTiempo();

            if (_bsEstado.DataSource is DataTable tablaActual)
                ActualizarKpisSegunPeriodo(tablaActual);
            else
                ActualizarKpisSegunPeriodo(null);
        }

        private void ActualizarEtiquetaTiempo()
        {
            if (lblTiempo == null || lblTiempo.IsDisposed)
                return;

            if (cbmMesesPanel?.SelectedItem is not OpcionMesPanel opcion)
            {
                lblTiempo.Text = "HOY";
                return;
            }

            lblTiempo.Text = opcion.EsHoy ? "HOY" : opcion.Etiqueta.ToUpper(CulturaDo);
        }

        private void ActualizarKpisSegunPeriodo(DataTable? tablaEstado)
        {
            if (cbmMesesPanel?.SelectedItem is not OpcionMesPanel opcion || opcion.EsHoy)
            {
                if (tablaEstado != null)
                    ActualizarKpisPlanesActivos(tablaEstado);
                else
                    AplicarKpisAControles(0, 0m, 0, 0m, 0, 0m, 0, 0m, 0, 0m, 0, 0m);
                return;
            }

            try
            {
                DataTable kpis = estadoBLL.ObtenerKpisPlanesPorMes(opcion.Anio, opcion.Mes);
                ActualizarKpisPlanesHistorico(kpis);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"KPI mes estado: {ex.Message}");
                AplicarKpisAControles(0, 0m, 0, 0m, 0, 0m, 0, 0m, 0, 0m, 0, 0m);
            }
        }

        private void ActualizarKpisPlanesActivos(DataTable tabla)
        {
            int cMensualidad = 0, cPremium = 0, cPro = 0, c3x = 0, cAbdomen = 0, cGluteos = 0;
            decimal mMensualidad = 0m, mPremium = 0m, mPro = 0m, m3x = 0m, mAbdomen = 0m, mGluteos = 0m;

            Dictionary<string, decimal> precios = ObtenerPreciosPlanes();

            if (tabla.Columns.Contains("Membresia") && tabla.Columns.Contains("Estado"))
            {
                foreach (DataRow row in tabla.Rows)
                {
                    string estado = Convert.ToString(row["Estado"])?.Trim() ?? string.Empty;
                    if (!string.Equals(estado, "ACTIVO", StringComparison.OrdinalIgnoreCase))
                        continue;

                    string plan = Convert.ToString(row["Membresia"])?.Trim() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(plan)
                        || string.Equals(plan, "SIN MEMBRESIA", StringComparison.OrdinalIgnoreCase))
                        continue;

                    decimal precio = precios.TryGetValue(plan, out decimal p) ? p : 0m;
                    AcumularKpiPlan(plan, 1, precio,
                        ref cMensualidad, ref mMensualidad,
                        ref cPremium, ref mPremium,
                        ref cPro, ref mPro,
                        ref c3x, ref m3x,
                        ref cAbdomen, ref mAbdomen,
                        ref cGluteos, ref mGluteos);
                }
            }

            AplicarKpisAControles(
                cMensualidad, mMensualidad, cPremium, mPremium, cPro, mPro,
                c3x, m3x, cAbdomen, mAbdomen, cGluteos, mGluteos);
        }

        private void ActualizarKpisPlanesHistorico(DataTable kpis)
        {
            int cMensualidad = 0, cPremium = 0, cPro = 0, c3x = 0, cAbdomen = 0, cGluteos = 0;
            decimal mMensualidad = 0m, mPremium = 0m, mPro = 0m, m3x = 0m, mAbdomen = 0m, mGluteos = 0m;

            foreach (DataRow row in kpis.Rows)
            {
                string plan = Convert.ToString(row["PlanNombre"])?.Trim() ?? string.Empty;
                int cantidad = row["Cantidad"] == DBNull.Value ? 0 : Convert.ToInt32(row["Cantidad"]);
                decimal monto = row["MontoTotal"] == DBNull.Value ? 0m : Convert.ToDecimal(row["MontoTotal"]);

                if (cantidad <= 0 && monto <= 0)
                    continue;

                AcumularKpiPlan(plan, cantidad, monto,
                    ref cMensualidad, ref mMensualidad,
                    ref cPremium, ref mPremium,
                    ref cPro, ref mPro,
                    ref c3x, ref m3x,
                    ref cAbdomen, ref mAbdomen,
                    ref cGluteos, ref mGluteos);
            }

            AplicarKpisAControles(
                cMensualidad, mMensualidad, cPremium, mPremium, cPro, mPro,
                c3x, m3x, cAbdomen, mAbdomen, cGluteos, mGluteos);
        }

        private void AcumularKpiPlan(
            string plan, int cantidad, decimal monto,
            ref int cMensualidad, ref decimal mMensualidad,
            ref int cPremium, ref decimal mPremium,
            ref int cPro, ref decimal mPro,
            ref int c3x, ref decimal m3x,
            ref int cAbdomen, ref decimal mAbdomen,
            ref int cGluteos, ref decimal mGluteos)
        {
            switch (ClasificarPlanKpi(plan))
            {
                case "MENSUALIDAD":
                    cMensualidad += cantidad;
                    mMensualidad += monto;
                    break;
                case "PREMIUM":
                    cPremium += cantidad;
                    mPremium += monto;
                    break;
                case "PRO":
                    cPro += cantidad;
                    mPro += monto;
                    break;
                case "3X":
                    c3x += cantidad;
                    m3x += monto;
                    break;
                case "ABDOMEN PLANO":
                    cAbdomen += cantidad;
                    mAbdomen += monto;
                    break;
                case "GLUTEOS GRANDE":
                    cGluteos += cantidad;
                    mGluteos += monto;
                    break;
            }
        }

        private void AplicarKpisAControles(
            int cMensualidad, decimal mMensualidad,
            int cPremium, decimal mPremium,
            int cPro, decimal mPro,
            int c3x, decimal m3x,
            int cAbdomen, decimal mAbdomen,
            int cGluteos, decimal mGluteos)
        {
            SetKpi(lblCMensualidad, cMensualidad.ToString("N0", CulturaDo));
            SetKpi(lblMMensualidad, "RD$ " + mMensualidad.ToString("N2", CulturaDo));
            SetKpi(lblCPremium, cPremium.ToString("N0", CulturaDo));
            SetKpi(lblMPremium, "RD$ " + mPremium.ToString("N2", CulturaDo));
            SetKpi(lblCPro, cPro.ToString("N0", CulturaDo));
            SetKpi(lblMPro, "RD$ " + mPro.ToString("N2", CulturaDo));
            SetKpi(lblC3x, c3x.ToString("N0", CulturaDo));
            SetKpi(lblM3x, "RD$ " + m3x.ToString("N2", CulturaDo));
            SetKpi(lblCAbdomenPlano, cAbdomen.ToString("N0", CulturaDo));
            SetKpi(lblMAbdomenPlano, "RD$ " + mAbdomen.ToString("N2", CulturaDo));
            SetKpi(lblCGluteosGrande, cGluteos.ToString("N0", CulturaDo));
            SetKpi(lblMGluteosGrande, "RD$ " + mGluteos.ToString("N2", CulturaDo));

            int cTotal = cMensualidad + cPremium + cPro + c3x + cAbdomen + cGluteos;
            decimal mTotal = mMensualidad + mPremium + mPro + m3x + mAbdomen + mGluteos;
            SetKpi(lblCTotal, cTotal.ToString("N0", CulturaDo));
            SetKpi(lblMTotal, "RD$ " + mTotal.ToString("N2", CulturaDo));
        }

        // ===============================
        // CERRAR
        // ===============================
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            timerActualizacion.Stop();
            timerActualizacion.Dispose();
            base.OnFormClosing(e);
        }
    }
}