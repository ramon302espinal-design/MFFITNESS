using DTO;
using BLL;
using BLL.Models;
using System;
using System.Drawing;
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
        private readonly FrmPresentacion _presentacion;
        private readonly MensajeAutomaticoBLL mensajeBLL = new MensajeAutomaticoBLL();
        private readonly BindingSource _bsEstado = new BindingSource();

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

            CargarEstado();

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
            timerActualizacion.Stop();
            base.OnFormClosed(e);
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
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en selección del grid: {ex.Message}");
                btnRenovar.Enabled = false;
                btnCongelar.Enabled = false;
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