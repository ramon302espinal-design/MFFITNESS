using BLL;
using CORE;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using UI.Helpers;

namespace UI
{
    [System.ComponentModel.DesignerCategory("Form")]
    public partial class FrmHistorialDeudas : Form
    {    
        HistorialBLL historialBLL = new HistorialBLL();
        private DataTable dtHistorialCompleto = new();

        /// <summary>Fuente del tipo de movimiento; se crea una vez para no asignar en cada celda.</summary>
        private Font? fuenteTipo;

        public FrmHistorialDeudas()
        {
            InitializeComponent();
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

            ConfigurarFiltros();
            CargarHistorial();

            AppEventos.OnDeudaModificada += CargarHistorial;
            AppEventos.OnPagoRegistrado += CargarHistorial;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            AppEventos.OnDeudaModificada -= CargarHistorial;
            AppEventos.OnPagoRegistrado -= CargarHistorial;
            fuenteTipo?.Dispose();
            fuenteTipo = null;
            base.OnFormClosed(e);
        }

        // ===============================
        // CONFIGURAR FILTROS
        // ===============================
        private void ConfigurarFiltros()
        {
            // Tipos reales que registra HistorialDeudas (deuda, financiamiento, cobros y reversas).
            cmbTipo.Items.Clear();
            cmbTipo.Items.AddRange(new string[]
            {
                "Todos", "DEUDA", "PAGO_INICIAL", "PAGO", "REVERSO_PAGO", "ANULACION"
            });
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
            if (IsDisposed || Disposing)
                return;

            if (InvokeRequired)
            {
                try
                {
                    if (IsHandleCreated)
                        BeginInvoke(new Action(CargarHistorial));
                }
                catch (ObjectDisposedException)
                {
                    // Formulario cerrado mientras el evento global aún notificaba.
                }
                return;
            }

            if (!PuedeUsarGrid())
                return;

            try
            {
                dtHistorialCompleto = historialBLL.ObtenerHistorial(null, null, null, null);
                EnriquecerHistorialFinanciamiento(dtHistorialCompleto);
                AplicarFiltros();
            }
            catch (ObjectDisposedException)
            {
                // Ignorar: el grid ya no está vivo (módulo cerrado / tab reciclado).
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar historial: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool PuedeUsarGrid() =>
            !IsDisposed
            && !Disposing
            && IsHandleCreated
            && dgvHistorial != null
            && !dgvHistorial.IsDisposed;

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
            if (!PuedeUsarGrid())
                return;

            if (dtHistorialCompleto == null || dtHistorialCompleto.Rows.Count == 0)
            {
                dgvHistorial.DataSource = null;
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
            }
            catch (ObjectDisposedException)
            {
                // Ignorar: UI ya no disponible.
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
            if (!PuedeUsarGrid() || dgvHistorial.Columns.Count == 0) return;

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
        // TOTALES (export / impresión)
        // ===============================
        private void ObtenerTotalesVisibles(out decimal totalDeudas, out decimal totalPagos, out decimal balance)
        {
            totalDeudas = 0m;
            totalPagos = 0m;
            balance = 0m;

            if (dgvHistorial.DataSource is not DataView dv)
                return;

            foreach (DataRowView row in dv)
            {
                string tipo = row["Tipo"]?.ToString() ?? string.Empty;
                decimal monto = row["Monto"] == DBNull.Value ? 0m : Convert.ToDecimal(row["Monto"]);

                if (tipo == "DEUDA")
                    totalDeudas += monto;
                else if (tipo == "PAGO" || tipo == "PAGO_INICIAL")
                    totalPagos += monto;
                else if (tipo == "REVERSO_PAGO")
                    totalPagos -= monto; // pago devuelto: deja de contar como cobrado
            }

            balance = totalDeudas - totalPagos;
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

        // ===============================
        // COLOR POR TIPO DE MOVIMIENTO
        // ===============================
        /// <summary>
        /// Cada movimiento del financiamiento se lee de un golpe: la deuda en rojo,
        /// el pago de inicio en azul y los cobros posteriores en verde.
        /// </summary>
        private void dgvHistorial_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string columna = dgvHistorial.Columns[e.ColumnIndex].Name;
            if (columna != "Tipo" && columna != "Monto")
                return;

            if (!TryColorMovimiento(ObtenerTipoFila(e.RowIndex), out Color color))
                return;

            e.CellStyle.ForeColor = color;
            e.CellStyle.SelectionForeColor = Color.White;

            if (columna == "Tipo")
            {
                fuenteTipo ??= new Font(dgvHistorial.Font, FontStyle.Bold);
                e.CellStyle.Font = fuenteTipo;
            }
        }

        private string ObtenerTipoFila(int rowIndex)
        {
            if (dgvHistorial.Rows[rowIndex].DataBoundItem is not DataRowView fila)
                return string.Empty;

            if (!fila.Row.Table.Columns.Contains("Tipo"))
                return string.Empty;

            return fila["Tipo"]?.ToString()?.Trim().ToUpperInvariant() ?? string.Empty;
        }

        private static bool TryColorMovimiento(string tipo, out Color color)
        {
            switch (tipo)
            {
                case "DEUDA":
                    color = Color.Firebrick;
                    return true;
                case "PAGO_INICIAL":
                    color = Color.RoyalBlue;
                    return true;
                case "PAGO":
                    color = Color.ForestGreen;
                    return true;
                case "REVERSO_PAGO":
                    color = Color.DarkOrange;
                    return true;
                case "ANULACION":
                    color = Color.DimGray;
                    return true;
                default:
                    color = Color.Empty;
                    return false;
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
        // BOTÓN EXPORTAR (CSV/TXT + PDF)
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

                using var sfd = new SaveFileDialog
                {
                    Filter = "PDF (*.pdf)|*.pdf|Archivo CSV (*.csv)|*.csv|Archivo de texto (*.txt)|*.txt",
                    FilterIndex = 1,
                    DefaultExt = "pdf",
                    AddExtension = true,
                    Title = "Exportar Historial",
                    FileName = $"Historial_Deudas_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (sfd.ShowDialog(this) != DialogResult.OK)
                    return;

                Cursor = Cursors.WaitCursor;

                string rutaElegida = sfd.FileName;
                string extension = System.IO.Path.GetExtension(rutaElegida)?.ToLowerInvariant() ?? "";
                string carpeta = System.IO.Path.GetDirectoryName(rutaElegida)
                    ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string baseName = System.IO.Path.GetFileNameWithoutExtension(rutaElegida);
                string rutaPdf = System.IO.Path.Combine(carpeta, baseName + ".pdf");

                DataTable datos = ConstruirTablaHistorialVisible();
                ObtenerTotalesVisibles(out decimal totalDeudas, out decimal totalPagos, out decimal balance);

                string filtroTipo = cmbTipo.SelectedIndex > 0 ? cmbTipo.Text : "Todos";
                string filtroCliente = txtCliente.Text?.Trim() ?? string.Empty;

                // PDF siempre: vista organizada de todos los resultados filtrados.
                new ReporteBLL().GenerarPdfHistorialDeudas(
                    datos,
                    rutaPdf,
                    Sesion.Usuario ?? "ADMIN",
                    dtpDesde.Value.Date,
                    dtpHasta.Value.Date,
                    filtroTipo,
                    filtroCliente,
                    totalDeudas,
                    totalPagos,
                    balance);

                // CSV / TXT si el usuario eligió esos formatos (además del PDF).
                if (extension == ".csv" || extension == ".txt")
                    ExportarHistorialTexto(rutaElegida);

                string mensaje = extension == ".pdf"
                    ? $"PDF generado:\n{rutaPdf}"
                    : $"Exportado:\n{rutaElegida}\n\nPDF generado:\n{rutaPdf}";

                DialogResult abrir = MessageBox.Show(
                    mensaje + "\n\n¿Desea abrir el PDF?",
                    "Exportación completa",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (abrir == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = rutaPdf,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Copia de las filas visibles del grid (mismo filtro que ve el usuario).
        /// </summary>
        private DataTable ConstruirTablaHistorialVisible()
        {
            var tabla = new DataTable();
            tabla.Columns.Add("Nombre", typeof(string));
            tabla.Columns.Add("Tipo", typeof(string));
            tabla.Columns.Add("Descripcion", typeof(string));
            tabla.Columns.Add("AporteInicial", typeof(string));
            tabla.Columns.Add("FechaLimitePago", typeof(DateTime));
            tabla.Columns.Add("Monto", typeof(decimal));
            tabla.Columns.Add("Fecha", typeof(DateTime));
            tabla.Columns.Add("Usuario", typeof(string));

            bool tieneAporte = dgvHistorial.Columns.Contains("AporteInicial");
            bool tieneLimite = dgvHistorial.Columns.Contains("FechaLimitePago");

            foreach (DataGridViewRow row in dgvHistorial.Rows)
            {
                if (row.IsNewRow) continue;

                DataRow nr = tabla.NewRow();
                nr["Nombre"] = row.Cells["Nombre"].Value?.ToString() ?? "";
                nr["Tipo"] = row.Cells["Tipo"].Value?.ToString() ?? "";
                nr["Descripcion"] = row.Cells["Descripcion"].Value?.ToString() ?? "";
                nr["AporteInicial"] = tieneAporte
                    ? row.Cells["AporteInicial"].Value?.ToString() ?? ""
                    : "";
                nr["FechaLimitePago"] = tieneLimite && row.Cells["FechaLimitePago"].Value != null
                    && row.Cells["FechaLimitePago"].Value != DBNull.Value
                    ? Convert.ToDateTime(row.Cells["FechaLimitePago"].Value)
                    : (object)DBNull.Value;
                nr["Monto"] = row.Cells["Monto"].Value != null && row.Cells["Monto"].Value != DBNull.Value
                    ? Convert.ToDecimal(row.Cells["Monto"].Value)
                    : 0m;
                nr["Fecha"] = row.Cells["Fecha"].Value != null && row.Cells["Fecha"].Value != DBNull.Value
                    ? Convert.ToDateTime(row.Cells["Fecha"].Value)
                    : (object)DBNull.Value;
                nr["Usuario"] = row.Cells["Usuario"].Value?.ToString() ?? "";
                tabla.Rows.Add(nr);
            }

            return tabla;
        }

        private void ExportarHistorialTexto(string ruta)
        {
            using var sw = new System.IO.StreamWriter(ruta, false, System.Text.Encoding.UTF8);

            string[] headers = new string[dgvHistorial.Columns.Count];
            for (int i = 0; i < dgvHistorial.Columns.Count; i++)
            {
                if (dgvHistorial.Columns[i].Visible)
                    headers[i] = dgvHistorial.Columns[i].HeaderText;
            }
            sw.WriteLine(string.Join(",", headers.Where(h => !string.IsNullOrEmpty(h))));

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

            ObtenerTotalesVisibles(out decimal totalDeudas, out decimal totalPagos, out decimal balance);

            sb.AppendLine();
            sb.AppendLine("═══════════════════════════════════════════════════════════");
            sb.AppendLine("                    RESUMEN FINANCIERO");
            sb.AppendLine("═══════════════════════════════════════════════════════════");
            sb.AppendLine($"Total Deudas: {totalDeudas:C2}");
            sb.AppendLine($"Total Pagos: {totalPagos:C2}");
            sb.AppendLine($"Balance: {balance:C2}");
            sb.AppendLine("═══════════════════════════════════════════════════════════");

            return sb.ToString();
        }
    }
}
