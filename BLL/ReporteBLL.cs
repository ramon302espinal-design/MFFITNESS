using DL;
using System.Data;
using System;
using ClosedXML.Excel;

// 👇 IMPORTANTE: usar alias para evitar conflicto
using IOPath = System.IO.Path;
using System.IO;

// iText
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.IO.Font.Constants;
using iText.Kernel.Font;

namespace BLL
{
    public class ReporteBLL
    {
        private ReporteDAL reporteDAL = new ReporteDAL();

        // ===============================
        // 🔥 OBTENER DATOS (CLAVE)
        // ===============================
        public DataTable ObtenerReporte(string tipo, DateTime desde, DateTime hasta)
        {
            if (string.IsNullOrEmpty(tipo))
                throw new Exception("Debe seleccionar un tipo de reporte.");

            switch (tipo)
            {
                case "CAJA":
                    return reporteDAL.ObtenerCajaPorFecha(desde, hasta);

                case "VENTAS":
                    return reporteDAL.ObtenerVentasPorFecha(desde, hasta);

                case "PAGOS":
                    return reporteDAL.ObtenerPagosPorFecha(desde, hasta);

                default:
                    throw new Exception("Tipo de reporte inválido");
            }
        }

        // ===============================
        // 🔥 EXPORTAR DESDE GRID (PRO)
        // ===============================
        public void GenerarReporteDesdeDataTable(DataTable datos, string ruta, string formato)
        {
            // 1. AUDITORÍA DE ENTRADA: Si esto falla aquí, ni siquiera intentamos abrir archivos
            if (datos == null || datos.Rows.Count == 0) throw new Exception("No hay datos para exportar.");
            if (string.IsNullOrEmpty(ruta)) throw new Exception("La ruta de destino es nula o inválida.");

            formato = formato.ToLower();

            // 2. GESTIÓN DE EXCEL
            if (formato == ".xlsx")
            {
                using (var wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add("Reporte");
                    ws.Cell(1, 1).Value = "REPORTE GENERAL - MFFITNESS";
                    ws.Cell(1, 1).Style.Font.Bold = true;
                    ws.Cell(3, 1).InsertTable(datos);
                    ws.Columns().AdjustToContents();
                    wb.SaveAs(ruta);
                }
            }
            // 3. GESTIÓN DE PDF (PROTECCIÓN TOTAL)
            else if (formato == ".pdf")
            {
                // Usamos un MemoryStream para que el archivo NO se bloquee en el disco si algo falla a mitad del proceso
                using (MemoryStream ms = new MemoryStream())
                {
                    try
                    {
                        // Inicialización de iText 7 con gestión de recursos automática
                        using (PdfWriter writer = new PdfWriter(ms))
                        {
                            using (PdfDocument pdf = new PdfDocument(writer))
                            {
                                using (Document doc = new Document(pdf, iText.Kernel.Geom.PageSize.A4))
                                {
                                    // Fuentes estándar (Sin riesgos de registro)
                                    var fontBold = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
                                    var fontNormal = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

                                    doc.Add(new Paragraph("REPORTE GENERAL - MFFITNESS")
                                        .SetFont(fontBold).SetFontSize(16));
                                    doc.Add(new Paragraph("Generado el: " + DateTime.Now.ToString("G"))
                                        .SetFont(fontNormal).SetFontSize(10));

                                    Table tabla = new Table(datos.Columns.Count).UseAllAvailableWidth();

                                    // CABECERAS (Con validación de nulos)
                                    foreach (DataColumn col in datos.Columns)
                                    {
                                        tabla.AddHeaderCell(new Cell()
                                            .Add(new Paragraph(col.ColumnName ?? "Columna"))
                                            .SetFont(fontBold)
                                            .SetBackgroundColor(iText.Kernel.Colors.ColorConstants.BLACK)
                                            .SetFontColor(iText.Kernel.Colors.ColorConstants.WHITE));
                                    }

                                    // FILAS (Usamos Convert.ToString para neutralizar los NULL de SQL)
                                    foreach (DataRow row in datos.Rows)
                                    {
                                        foreach (var cellValue in row.ItemArray)
                                        {
                                            // Convert.ToString es mágico: si es DBNull devuelve "", no rompe el programa
                                            string texto = Convert.ToString(cellValue);

                                            // Formateo rápido si es decimal (dinero)
                                            if (cellValue is decimal d) texto = d.ToString("N2");

                                            tabla.AddCell(new Cell().Add(new Paragraph(texto ?? "").SetFont(fontNormal).SetFontSize(9)));
                                        }
                                    }

                                    doc.Add(tabla);
                                    // IMPORTANTE: Dejamos que los 'using' cierren todo
                                }
                            }
                        }

                        // 4. ESCRITURA FINAL: Solo tocamos el disco cuando el PDF ya está listo en RAM
                        // Esto elimina el error "File path cannot be null" y los bloqueos de archivo
                        File.WriteAllBytes(ruta, ms.ToArray());
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error generando PDF: " + ex.ToString());
                    }
                }
            }
        }
        // ===============================
        // 🔥 AUTOMÁTICO DIARIO (PRO)
        // ===============================
        public void GenerarReporteAutomaticoDiario()
        {
            DateTime hoy = DateTime.Today;

            string carpetaBase = IOPath.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "MFITNESS_REPORTES"
            );

            if (!Directory.Exists(carpetaBase))
                Directory.CreateDirectory(carpetaBase);

            string carpetaMes = IOPath.Combine(carpetaBase, hoy.ToString("yyyy-MM"));

            if (!Directory.Exists(carpetaMes))
                Directory.CreateDirectory(carpetaMes);

            string nombre = "Reporte_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

            string rutaExcel = IOPath.Combine(carpetaMes, nombre + ".xlsx");
            string rutaPDF = IOPath.Combine(carpetaMes, nombre + ".pdf");

            bool existeExcel = reporteDAL.ExisteReporte(hoy, "EXCEL");
            bool existePDF = reporteDAL.ExisteReporte(hoy, "PDF");

            // 🔥 AQUÍ YA USAMOS DATOS REALES
            DataTable datos = reporteDAL.ObtenerCajaPorFecha(hoy, hoy);

            // 🔥 SI NO HAY DATOS, NO HACER NADA (CLAVE)
            if (datos == null || datos.Rows.Count == 0)
            {
                return; // NO ROMPER EL SISTEMA
            }
            if (!existeExcel)
            {
                GenerarReporteDesdeDataTable(datos, rutaExcel, ".xlsx");
                reporteDAL.InsertarReporte(hoy, "EXCEL", rutaExcel);
            }

           
        }

        /// <summary>
        /// PDF de deudas activas con columnas de deudor, montos, vencidas, fechas y pago inicial.
        /// </summary>
        public void GenerarPdfReporteDeudas(DataTable datos, string ruta)
        {
            if (datos == null || datos.Rows.Count == 0)
                throw new Exception("No hay deudas activas para reportar.");
            if (string.IsNullOrWhiteSpace(ruta))
                throw new Exception("Ruta de PDF inválida.");

            using MemoryStream ms = new MemoryStream();
            using (PdfWriter writer = new PdfWriter(ms))
            using (PdfDocument pdf = new PdfDocument(writer))
            using (Document doc = new Document(pdf, iText.Kernel.Geom.PageSize.A4.Rotate()))
            {
                var fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                var fontNormal = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                doc.SetMargins(28, 28, 28, 28);
                doc.Add(new Paragraph("REPORTE DE DEUDAS - MF FITNESS")
                    .SetFont(fontBold).SetFontSize(16));
                doc.Add(new Paragraph($"Generado: {DateTime.Now:dd/MM/yyyy hh:mm tt}")
                    .SetFont(fontNormal).SetFontSize(10));

                decimal totalPendiente = 0m;
                foreach (DataRow row in datos.Rows)
                {
                    if (row.Table.Columns.Contains("MontoPendiente") && row["MontoPendiente"] != DBNull.Value)
                        totalPendiente += Convert.ToDecimal(row["MontoPendiente"]);
                    else if (row.Table.Columns.Contains("MontoDeudasActivas") && row["MontoDeudasActivas"] != DBNull.Value)
                        totalPendiente += Convert.ToDecimal(row["MontoDeudasActivas"]);
                }

                // Si MontoPendiente es por deuda, la suma es correcta.
                // Si se usó MontoDeudasActivas (por cliente), evitar doble conteo no aplica
                // porque ahora siempre enviamos MontoPendiente = Saldo de la fila.
                doc.Add(new Paragraph($"TOTAL MONTO PENDIENTE: RD$ {totalPendiente:N2}")
                    .SetFont(fontBold).SetFontSize(12).SetMarginBottom(12));

                string[] headers =
                {
                    "Nombre del que debe",
                    "Teléfono",
                    "Dirección",
                    "Deudas activas",
                    "Monto deudas activas",
                    "Monto pendiente",
                    "Deudas vencidas",
                    "Fecha y hora de deuda",
                    "Pago inicial",
                    "Fecha a vencer",
                    "Concepto"
                };

                Table tabla = new Table(UnitValue.CreatePercentArray(new float[]
                {
                    12, 9, 12, 6, 9, 9, 6, 10, 8, 8, 11
                })).UseAllAvailableWidth();

                foreach (string h in headers)
                {
                    tabla.AddHeaderCell(new Cell()
                        .Add(new Paragraph(h).SetFont(fontBold).SetFontSize(7))
                        .SetBackgroundColor(ColorConstants.DARK_GRAY)
                        .SetFontColor(ColorConstants.WHITE)
                        .SetPadding(3));
                }

                foreach (DataRow row in datos.Rows)
                {
                    string nombre = Convert.ToString(row["NombreDelDeudor"]) ?? "";
                    string telefono = row.Table.Columns.Contains("Telefono")
                        ? (Convert.ToString(row["Telefono"]) ?? "")
                        : "";
                    string direccion = row.Table.Columns.Contains("Direccion")
                        ? (Convert.ToString(row["Direccion"]) ?? "")
                        : "";
                    string activas = Convert.ToString(row["DeudasActivas"]) ?? "0";
                    decimal montoActivas = row["MontoDeudasActivas"] == DBNull.Value
                        ? 0m : Convert.ToDecimal(row["MontoDeudasActivas"]);
                    decimal montoPendiente = row.Table.Columns.Contains("MontoPendiente") && row["MontoPendiente"] != DBNull.Value
                        ? Convert.ToDecimal(row["MontoPendiente"])
                        : montoActivas;
                    string vencidas = Convert.ToString(row["DeudasVencidas"]) ?? "0";
                    string fechaHora = row["FechaHoraDeuda"] == DBNull.Value
                        ? "-"
                        : Convert.ToDateTime(row["FechaHoraDeuda"]).ToString("dd/MM/yyyy hh:mm tt");
                    decimal pagoInicial = row["PagoInicial"] == DBNull.Value
                        ? 0m : Convert.ToDecimal(row["PagoInicial"]);
                    string fechaVence = row["FechaAVencer"] == DBNull.Value
                        ? "-"
                        : Convert.ToDateTime(row["FechaAVencer"]).ToString("dd/MM/yyyy");
                    string concepto = Convert.ToString(row["Concepto"]) ?? "";

                    void Celda(string texto) =>
                        tabla.AddCell(new Cell()
                            .Add(new Paragraph(texto).SetFont(fontNormal).SetFontSize(7))
                            .SetPadding(3));

                    Celda(nombre);
                    Celda(telefono);
                    Celda(direccion);
                    Celda(activas);
                    Celda($"RD$ {montoActivas:N2}");
                    Celda($"RD$ {montoPendiente:N2}");
                    Celda(vencidas);
                    Celda(fechaHora);
                    Celda($"RD$ {pagoInicial:N2}");
                    Celda(fechaVence);
                    Celda(concepto);
                }

                doc.Add(tabla);
                doc.Add(new Paragraph($"TOTAL MONTO PENDIENTE: RD$ {totalPendiente:N2}")
                    .SetFont(fontBold).SetFontSize(11).SetMarginTop(10));
            }

            File.WriteAllBytes(ruta, ms.ToArray());
        }
    }
}