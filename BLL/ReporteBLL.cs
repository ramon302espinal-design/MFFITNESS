using DL;
using System.Data;
using System;
using ClosedXML.Excel;
using CORE;

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
                doc.Add(new Paragraph($"Generado: {DateTime.Now.ToString(FechaHoraFormats.FechaHora)}")
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
                        : Convert.ToDateTime(row["FechaHoraDeuda"]).ToString(FechaHoraFormats.FechaHora);
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

        /// <summary>
        /// PDF del historial de deudas/pagos filtrado: tabla ordenada + resumen financiero.
        /// </summary>
        public void GenerarPdfHistorialDeudas(
            DataTable datos,
            string ruta,
            string usuario,
            DateTime desde,
            DateTime hasta,
            string filtroTipo,
            string filtroCliente,
            decimal totalDeudas,
            decimal totalPagos,
            decimal balance)
        {
            if (datos == null || datos.Rows.Count == 0)
                throw new Exception("No hay movimientos para exportar.");
            if (string.IsNullOrWhiteSpace(ruta))
                throw new Exception("Ruta de PDF inválida.");

            using MemoryStream ms = new MemoryStream();
            using (PdfWriter writer = new PdfWriter(ms))
            using (PdfDocument pdf = new PdfDocument(writer))
            using (Document doc = new Document(pdf, iText.Kernel.Geom.PageSize.A4.Rotate()))
            {
                var fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                var fontNormal = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                doc.SetMargins(24, 24, 24, 24);

                doc.Add(new Paragraph("HISTORIAL DE DEUDAS Y PAGOS - MF FITNESS")
                    .SetFont(fontBold).SetFontSize(16));
                doc.Add(new Paragraph($"Generado: {DateTime.Now.ToString(FechaHoraFormats.FechaHoraSegundos)}    ·    Usuario: {usuario ?? "-"}")
                    .SetFont(fontNormal).SetFontSize(9));
                doc.Add(new Paragraph(
                    $"Período: {desde:dd/MM/yyyy} — {hasta:dd/MM/yyyy}" +
                    $"    ·    Tipo: {(string.IsNullOrWhiteSpace(filtroTipo) ? "Todos" : filtroTipo)}" +
                    $"    ·    Cliente: {(string.IsNullOrWhiteSpace(filtroCliente) ? "Todos" : filtroCliente)}" +
                    $"    ·    Registros: {datos.Rows.Count}")
                    .SetFont(fontNormal).SetFontSize(9).SetMarginBottom(10));

                Table resumen = new Table(UnitValue.CreatePercentArray(new float[] { 33, 33, 34 }))
                    .UseAllAvailableWidth()
                    .SetMarginBottom(12);

                void CeldaResumen(string titulo, string valor, DeviceRgb fondo)
                {
                    resumen.AddCell(new Cell()
                        .Add(new Paragraph(titulo).SetFont(fontBold).SetFontSize(8).SetFontColor(ColorConstants.WHITE))
                        .Add(new Paragraph(valor).SetFont(fontBold).SetFontSize(11).SetFontColor(ColorConstants.WHITE))
                        .SetBackgroundColor(fondo)
                        .SetPadding(6)
                        .SetTextAlignment(TextAlignment.CENTER));
                }

                CeldaResumen("TOTAL DEUDAS", $"RD$ {totalDeudas:N2}", new DeviceRgb(178, 34, 34));
                CeldaResumen("TOTAL PAGOS", $"RD$ {totalPagos:N2}", new DeviceRgb(34, 139, 34));
                CeldaResumen("BALANCE", $"RD$ {balance:N2}",
                    balance > 0 ? new DeviceRgb(178, 34, 34)
                    : balance < 0 ? new DeviceRgb(34, 139, 34)
                    : new DeviceRgb(70, 70, 70));

                doc.Add(resumen);

                string[] headers =
                {
                    "Cliente", "Tipo", "Descripción", "Pago inicial",
                    "Fecha límite", "Monto", "Fecha", "Usuario"
                };

                Table tabla = new Table(UnitValue.CreatePercentArray(new float[]
                {
                    16, 10, 22, 10, 10, 10, 12, 10
                })).UseAllAvailableWidth();

                foreach (string h in headers)
                {
                    tabla.AddHeaderCell(new Cell()
                        .Add(new Paragraph(h).SetFont(fontBold).SetFontSize(8))
                        .SetBackgroundColor(ColorConstants.DARK_GRAY)
                        .SetFontColor(ColorConstants.WHITE)
                        .SetPadding(4));
                }

                bool tieneAporte = datos.Columns.Contains("AporteInicial");
                bool tieneLimite = datos.Columns.Contains("FechaLimitePago");

                foreach (DataRow row in datos.Rows)
                {
                    string tipo = Convert.ToString(row["Tipo"])?.Trim().ToUpperInvariant() ?? "";
                    DeviceRgb? colorTipo = ColorTipoMovimiento(tipo);

                    string cliente = Convert.ToString(row["Nombre"]) ?? "";
                    string descripcion = Convert.ToString(row["Descripcion"]) ?? "";
                    string aporte = tieneAporte ? (Convert.ToString(row["AporteInicial"]) ?? "") : "";
                    string limite = !tieneLimite || row["FechaLimitePago"] == DBNull.Value
                        ? "-"
                        : Convert.ToDateTime(row["FechaLimitePago"]).ToString("dd/MM/yyyy");
                    decimal monto = row["Monto"] == DBNull.Value ? 0m : Convert.ToDecimal(row["Monto"]);
                    string fecha = row["Fecha"] == DBNull.Value
                        ? "-"
                        : Convert.ToDateTime(row["Fecha"]).ToString(FechaHoraFormats.FechaHora);
                    string user = Convert.ToString(row["Usuario"]) ?? "";

                    void Celda(string texto, bool resaltarTipo = false)
                    {
                        var p = new Paragraph(texto).SetFont(fontNormal).SetFontSize(7);
                        if (resaltarTipo && colorTipo != null)
                            p.SetFontColor(colorTipo).SetFont(fontBold);
                        else if (colorTipo != null && texto.StartsWith("RD$", StringComparison.Ordinal))
                            p.SetFontColor(colorTipo).SetFont(fontBold);

                        tabla.AddCell(new Cell().Add(p).SetPadding(3));
                    }

                    Celda(cliente);
                    Celda(tipo, resaltarTipo: true);
                    Celda(descripcion);
                    Celda(string.IsNullOrWhiteSpace(aporte) ? "-" : aporte);
                    Celda(limite);
                    Celda($"RD$ {monto:N2}");
                    Celda(fecha);
                    Celda(user);
                }

                doc.Add(tabla);

                doc.Add(new Paragraph(
                    $"Resumen · Deudas RD$ {totalDeudas:N2}  ·  Pagos RD$ {totalPagos:N2}  ·  Balance RD$ {balance:N2}")
                    .SetFont(fontBold).SetFontSize(10).SetMarginTop(12));
            }

            File.WriteAllBytes(ruta, ms.ToArray());
        }

        private static DeviceRgb? ColorTipoMovimiento(string tipo) => tipo switch
        {
            "DEUDA" => new DeviceRgb(178, 34, 34),
            "PAGO_INICIAL" => new DeviceRgb(65, 105, 225),
            "PAGO" => new DeviceRgb(34, 139, 34),
            "REVERSO_PAGO" => new DeviceRgb(255, 140, 0),
            "ANULACION" => new DeviceRgb(105, 105, 105),
            _ => null
        };
    }
}