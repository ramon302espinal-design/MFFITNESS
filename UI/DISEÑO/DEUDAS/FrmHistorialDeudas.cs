using BLL;
using CORE;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using UI.Helpers;
using UI.Theme;

namespace UI
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmHistorialDeudas : Form
    {    
        HistorialBLL historialBLL = new HistorialBLL();
        private DataTable dtHistorialCompleto = new();

        public FrmHistorialDeudas()
        {
            InitializeComponent();
            ThemeHost.Attach(this);
        }

        private void FrmHistorialDeudas_Load(object sender, EventArgs e)
        {
            // 🔐 Validación de permisos SOLO si el formulario NO está embebido
            // Cuando TopLevel = false, significa que está dentro del módulo unificado
            if (this.TopLevel)
            {
                // Lógica permisiva (OR): Si tiene alguno de estos permisos, puede acceder
                bool tieneAcceso = Sesion.TienePermiso("VER_HISTORIAL_DEUDAS") ||
                                   Sesion.TienePermiso("1003") ||
                                   Sesion.TienePermiso("VER_DEUDAS") ||
                                   Sesion.Rol?.ToUpper() == "ADMIN";

                if (!tieneAcceso)
                {
                    MessageBox.Show("No tienes acceso a este módulo", "Acceso Denegado", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.Close();
                    return;
                }
            }

            ConfigurarGrid();
            ConfigurarFiltros();
            CargarHistorial();

            AppEventos.OnDeudaModificada += CargarHistorial;
            AppEventos.OnPagoRegistrado += CargarHistorial;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            AppEventos.OnDeudaModificada -= CargarHistorial;
            AppEventos.OnPagoRegistrado -= CargarHistorial;
            base.OnFormClosed(e);
        }

        // ===============================
        // CONFIGURAR GRID PROFESIONAL
        // ===============================
        private void ConfigurarGrid()
        {
            dgvHistorial.BorderStyle = BorderStyle.None;
            dgvHistorial.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            dgvHistorial.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvHistorial.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 122, 204);
            dgvHistorial.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvHistorial.BackgroundColor = Color.White;
            dgvHistorial.EnableHeadersVisualStyles = false;
            dgvHistorial.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 37, 38);
            dgvHistorial.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvHistorial.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvHistorial.ColumnHeadersHeight = 35;
            dgvHistorial.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvHistorial.RowTemplate.Height = 30;
            dgvHistorial.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvHistorial.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvHistorial.MultiSelect = false;
            dgvHistorial.ReadOnly = true;
            dgvHistorial.AllowUserToAddRows = false;
            dgvHistorial.AllowUserToDeleteRows = false;
        }

        // ===============================
        // CONFIGURAR FILTROS
        // ===============================
        private void ConfigurarFiltros()
        {
            cmbTipo.Items.Clear();
            cmbTipo.Items.AddRange(new string[] { "Todos", "DEUDA", "PAGO", "PAGO_INICIAL", "ACTUALIZACION" });
            cmbTipo.SelectedIndex = 0;

            // Establecer rango de fechas por defecto (últimos 30 días)
            dtpDesde.Value = DateTime.Now.AddDays(-30);
            dtpHasta.Value = DateTime.Now;
        }

        // ===============================
        // CARGAR HISTORIAL
        // ===============================
        private void CargarHistorial()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(CargarHistorial));
                return;
            }

            try
            {
                dtHistorialCompleto = historialBLL.ObtenerHistorial(null, null, null, null);
                EnriquecerHistorialFinanciamiento(dtHistorialCompleto);
                AplicarFiltros();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar historial: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===============================
        // MÉTODO PÚBLICO PARA REFRESCAR DESDE MÓDULO PRINCIPAL
        // ===============================
        public void ActualizarDatos()
        {
            CargarHistorial();
        }

        private void EnriquecerHistorialFinanciamiento(DataTable dt)
        {
            if (dt == null) return;

            if (!dt.Columns.Contains("AporteInicial"))
                dt.Columns.Add("AporteInicial", typeof(string));

            var pagosInicialesPorDeuda = new Dictionary<int, decimal>();
            if (dt.Columns.Contains("DeudaId"))
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (row["Tipo"]?.ToString() != "PAGO_INICIAL" || row["DeudaId"] == DBNull.Value)
                        continue;

                    int deudaId = Convert.ToInt32(row["DeudaId"]);
                    pagosInicialesPorDeuda[deudaId] = Convert.ToDecimal(row["Monto"]);
                }
            }

            foreach (DataRow row in dt.Rows)
            {
                string tipo = row["Tipo"]?.ToString() ?? string.Empty;

                if (tipo == "PAGO_INICIAL")
                {
                    row["AporteInicial"] = $"Sí ({Convert.ToDecimal(row["Monto"]):N2})";
                    continue;
                }

                if (tipo != "DEUDA")
                {
                    row["AporteInicial"] = string.Empty;
                    continue;
                }

                string descripcion = row["Descripcion"]?.ToString() ?? string.Empty;
                bool esFinanciamiento =
                    descripcion.Contains("Financiamiento", StringComparison.OrdinalIgnoreCase) ||
                    descripcion.Contains("Saldo plan", StringComparison.OrdinalIgnoreCase) ||
                    descripcion.Contains("Pago inicial:", StringComparison.OrdinalIgnoreCase);

                if (!esFinanciamiento)
                {
                    row["AporteInicial"] = "-";
                    continue;
                }

                if (dt.Columns.Contains("DeudaId") &&
                    row["DeudaId"] != DBNull.Value &&
                    pagosInicialesPorDeuda.TryGetValue(Convert.ToInt32(row["DeudaId"]), out decimal montoInicial))
                {
                    row["AporteInicial"] = $"Sí ({montoInicial:N2})";
                    continue;
                }

                row["AporteInicial"] = "No ($0.00)";
            }
        }

        // ===============================
        // APLICAR FILTROS 
        // ===============================
        private void AplicarFiltros()
        {
            if (dtHistorialCompleto == null || dtHistorialCompleto.Rows.Count == 0)
            {
                dgvHistorial.DataSource = null;
                lblTotalDeudas.Text = "Total Deudas: $0.00";
                lblTotalPagos.Text = "Total Pagos: $0.00";
                lblBalance.Text = "Balance: $0.00";
                return;
            }

            try
            {
                DataView dv = dtHistorialCompleto.DefaultView;
                string filtro = "1=1"; // Filtro base siempre verdadero

                // Filtro por tipo
                if (cmbTipo.SelectedIndex > 0) // No es "Todos"
                {
                    string tipo = cmbTipo.Text;
                    filtro += $" AND Tipo = '{tipo}'";
                }

                // Filtro por fecha
                filtro += $" AND Fecha >= #{dtpDesde.Value:MM/dd/yyyy}# AND Fecha <= #{dtpHasta.Value:MM/dd/yyyy 23:59:59}#";

                // Filtro por búsqueda de cliente
                string textoCliente = txtCliente.Text.Trim();
                if (!string.IsNullOrEmpty(textoCliente))
                {
                    filtro += $" AND Nombre LIKE '%{textoCliente.Replace("'", "''")}%'";
                }

                dv.RowFilter = filtro;
                dgvHistorial.DataSource = dv;

                FormatearColumnas();
                CalcularTotales();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al aplicar filtros: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===============================
        // FORMATEAR COLUMNAS
        // ===============================
        private void FormatearColumnas()
        {
            if (dgvHistorial.Columns.Count == 0) return;

            DataGridViewHelper.HideColumn(dgvHistorial, "Id");
            DataGridViewHelper.HideColumn(dgvHistorial, "DeudaId");

            DataGridViewHelper.ConfigureColumn(dgvHistorial, "Nombre", col =>
            {
                col.HeaderText = "Cliente";
                col.Width = 200;
            });

            DataGridViewHelper.ConfigureColumn(dgvHistorial, "Tipo", col =>
                col.HeaderText = "Tipo");

            DataGridViewHelper.ConfigureColumn(dgvHistorial, "Descripcion", col =>
            {
                col.HeaderText = "Descripción";
                col.Width = 250;
            });

            DataGridViewHelper.ConfigureColumn(dgvHistorial, "FechaLimitePago", col =>
            {
                col.HeaderText = "Fecha Límite Pago";
                col.DefaultCellStyle.Format = "dd/MM/yyyy";
                col.Width = 130;
                col.DisplayIndex = 3;
            });

            DataGridViewHelper.ConfigureColumn(dgvHistorial, "AporteInicial", col =>
            {
                col.HeaderText = "Pago Inicial";
                col.Width = 120;
                col.DisplayIndex = 3;
            });

            DataGridViewHelper.ConfigureColumn(dgvHistorial, "Monto", col =>
            {
                col.HeaderText = "Monto";
                col.DefaultCellStyle.Format = "C2";
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                col.Width = 120;
            });

            DataGridViewHelper.ConfigureColumn(dgvHistorial, "Fecha", col =>
            {
                col.HeaderText = "Fecha";
                col.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                col.Width = 150;
            });

            DataGridViewHelper.ConfigureColumn(dgvHistorial, "Usuario", col =>
            {
                col.HeaderText = "Usuario";
                col.Width = 100;
            });
        }

        // ===============================
        // CALCULAR TOTALES
        // ===============================
        private void CalcularTotales()
        {
            if (dgvHistorial.DataSource == null)
            {
                lblTotalDeudas.Text = "Total Deudas: $0.00";
                lblTotalPagos.Text = "Total Pagos: $0.00";
                lblBalance.Text = "Balance: $0.00";
                return;
            }

            try
            {
                DataView dv = (DataView)dgvHistorial.DataSource;
                DataTable dt = dv.ToTable();

                decimal totalDeudas = 0;
                decimal totalPagos = 0;

                foreach (DataRow row in dt.Rows)
                {
                    string tipo = row["Tipo"].ToString();
                    decimal monto = Convert.ToDecimal(row["Monto"]);

                    if (tipo == "DEUDA")
                        totalDeudas += monto;
                    else if (tipo == "PAGO" || tipo == "PAGO_INICIAL")
                        totalPagos += monto;
                }

                decimal balance = totalDeudas - totalPagos;

                lblTotalDeudas.Text = $"Total Deudas: {totalDeudas:C2}";
                lblTotalPagos.Text = $"Total Pagos: {totalPagos:C2}";
                lblBalance.Text = $"Balance: {balance:C2}";

                // Color del balance
                if (balance > 0)
                    lblBalance.ForeColor = Color.Red;
                else if (balance < 0)
                    lblBalance.ForeColor = Color.Green;
                else
                    lblBalance.ForeColor = Color.Black;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al calcular totales: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===============================
        // EVENTOS DE FILTROS
        // ===============================
        private void txtCliente_TextChanged(object sender, EventArgs e)
        {
            AplicarFiltros();
        }

        private void cmbTipo_SelectedIndexChanged(object sender, EventArgs e)
        {
            AplicarFiltros();
        }

        private void dtpDesde_ValueChanged(object sender, EventArgs e)
        {
            AplicarFiltros();
        }

        private void dtpHasta_ValueChanged(object sender, EventArgs e)
        {
            AplicarFiltros();
        }

        private void dgvHistorial_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvHistorial.Columns[e.ColumnIndex].Name == "Tipo" && e.Value != null)
            {
                string tipo = e.Value.ToString();

                if (tipo == "PAGO")
                    e.CellStyle.ForeColor = Color.Green;
                else if (tipo == "PAGO_INICIAL")
                    e.CellStyle.ForeColor = Color.FromArgb(0, 150, 136);
                else if (tipo == "DEUDA")
                    e.CellStyle.ForeColor = Color.Red;
                else if (tipo == "ACTUALIZACION")
                    e.CellStyle.ForeColor = Color.Orange;
            }
        }

        // ===============================
        // BOTÓN ACTUALIZAR
        // ===============================
        private void btnActualizar_Click(object sender, EventArgs e)
        {
            CargarHistorial();
            MessageBox.Show("Historial actualizado correctamente", "Éxito", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ===============================
        // BOTÓN EXPORTAR
        // ===============================
        private void btnExportar_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvHistorial.Rows.Count == 0)
                {
                    MessageBox.Show("No hay datos para exportar", "Advertencia", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "Archivo CSV|*.csv|Archivo de texto|*.txt",
                    Title = "Exportar Historial",
                    FileName = $"Historial_Deudas_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(sfd.FileName, false, System.Text.Encoding.UTF8))
                    {
                        // Encabezados
                        string[] headers = new string[dgvHistorial.Columns.Count];
                        for (int i = 0; i < dgvHistorial.Columns.Count; i++)
                        {
                            if (dgvHistorial.Columns[i].Visible)
                                headers[i] = dgvHistorial.Columns[i].HeaderText;
                        }
                        sw.WriteLine(string.Join(",", headers.Where(h => !string.IsNullOrEmpty(h))));

                        // Datos
                        foreach (DataGridViewRow row in dgvHistorial.Rows)
                        {
                            if (row.IsNewRow) continue;

                            string[] cells = new string[dgvHistorial.Columns.Count];
                            for (int i = 0; i < dgvHistorial.Columns.Count; i++)
                            {
                                if (dgvHistorial.Columns[i].Visible)
                                {
                                    object value = row.Cells[i].Value;
                                    cells[i] = value?.ToString()?.Replace(",", ";") ?? string.Empty;
                                }
                            }
                            sw.WriteLine(string.Join(",", cells.Where(c => c != null)));
                        }
                    }

                    MessageBox.Show("Historial exportado exitosamente", "Éxito", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Preguntar si desea abrir el archivo
                    if (MessageBox.Show("¿Desea abrir el archivo?", "Exportación Completa", 
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = sfd.FileName,
                            UseShellExecute = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===============================
        // BOTÓN IMPRIMIR
        // ===============================
        private void btnImprimir_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvHistorial.Rows.Count == 0)
                {
                    MessageBox.Show("No hay datos para imprimir", "Advertencia", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Vista previa simple
                string reporte = GenerarReporteTexto();

                FrmVistaPrevia frmPrevia = new FrmVistaPrevia(reporte);
                frmPrevia.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar reporte: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===============================
        // GENERAR REPORTE DE TEXTO
        // ===============================
        private string GenerarReporteTexto()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine("═══════════════════════════════════════════════════════════");
            sb.AppendLine("        HISTORIAL DE DEUDAS Y PAGOS - MF FITNESS");
            sb.AppendLine("═══════════════════════════════════════════════════════════");
            sb.AppendLine($"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            sb.AppendLine($"Usuario: {Sesion.Usuario}");
            sb.AppendLine($"Período: {dtpDesde.Value:dd/MM/yyyy} - {dtpHasta.Value:dd/MM/yyyy}");
            sb.AppendLine("═══════════════════════════════════════════════════════════");
            sb.AppendLine();

            foreach (DataGridViewRow row in dgvHistorial.Rows)
            {
                if (row.IsNewRow) continue;

                string cliente = row.Cells["Nombre"].Value?.ToString() ?? "";
                string tipo = row.Cells["Tipo"].Value?.ToString() ?? "";
                string descripcion = row.Cells["Descripcion"].Value?.ToString() ?? "";
                string aporteInicial = dgvHistorial.Columns.Contains("AporteInicial")
                    ? row.Cells["AporteInicial"].Value?.ToString() ?? ""
                    : "";
                string monto = row.Cells["Monto"].Value != null ? 
                    Convert.ToDecimal(row.Cells["Monto"].Value).ToString("C2") : "$0.00";
                string fecha = row.Cells["Fecha"].Value != null ? 
                    Convert.ToDateTime(row.Cells["Fecha"].Value).ToString("dd/MM/yyyy HH:mm") : "";
                string usuario = row.Cells["Usuario"].Value?.ToString() ?? "";

                sb.AppendLine($"Cliente: {cliente}");
                sb.AppendLine($"Tipo: {tipo}");
                sb.AppendLine($"Descripción: {descripcion}");
                if (!string.IsNullOrWhiteSpace(aporteInicial))
                    sb.AppendLine($"Pago inicial: {aporteInicial}");
                sb.AppendLine($"Monto: {monto}");
                sb.AppendLine($"Fecha: {fecha}");
                sb.AppendLine($"Usuario: {usuario}");
                sb.AppendLine("───────────────────────────────────────────────────────────");
            }

            sb.AppendLine();
            sb.AppendLine("═══════════════════════════════════════════════════════════");
            sb.AppendLine("                    RESUMEN FINANCIERO");
            sb.AppendLine("═══════════════════════════════════════════════════════════");
            sb.AppendLine(lblTotalDeudas.Text);
            sb.AppendLine(lblTotalPagos.Text);
            sb.AppendLine(lblBalance.Text);
            sb.AppendLine("═══════════════════════════════════════════════════════════");

            return sb.ToString();
        }
    }
}
