using System.Globalization;
using CORE;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BLL.Facturas
{
    public static class FacturaMembresiaPdfGenerator
    {
        private static bool _licenseConfigured;

        public static void ConfigurarLicencia()
        {
            if (_licenseConfigured)
                return;
            QuestPDF.Settings.License = LicenseType.Community;
            _licenseConfigured = true;
        }

        /// <summary>
        /// Genera PDF nítido y lo guarda como factura_{pagoId}.pdf (LocalAppData + wwwroot espejo).
        /// </summary>
        public static string GenerarYGuardar(FacturaMembresiaData data)
        {
            ConfigurarLicencia();

            int pagoId = Math.Max(1, data.NumeroFactura);
            using var stream = new MemoryStream();
            new FacturaMembresiaDocument(data).GeneratePdf(stream);
            byte[] bytes = stream.ToArray();

            FacturaStorage.GuardarFactura(pagoId, bytes);

            // Publicar en Supabase Storage (bucket FACTURAS) para Twilio sin Ngrok.
            try
            {
                FacturaSupabaseUploader.TryUploadAndGetPublicUrl(pagoId, bytes);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Supabase upload: {ex.Message}");
            }

            // Copia legible adicional con nombre FAC
            try
            {
                string fac = $"FAC-{pagoId:D2}";
                string legible = Path.Combine(
                    FacturaStorage.CarpetaFacturas,
                    $"{fac}_{Sanitize(data.ClienteNombre)}_{data.FechaEmision:yyyyMMdd_HHmmss}.pdf");
                File.WriteAllBytes(legible, bytes);
            }
            catch
            {
                // opcional
            }

            return FacturaStorage.RutaFacturaPago(pagoId);
        }

        public static string? GenerarDesdePago(
            int clienteId,
            string nombrePlan,
            decimal montoPagado,
            DateTime fechaVencimiento,
            string metodoPago,
            int pagoId,
            string? notaExtra = null)
        {
            try
            {
                var clienteBll = new ClienteBLL();
                var row = clienteBll.ObtenerPorId(clienteId);
                string nombre = row?["Nombre"]?.ToString()?.Trim() ?? $"Cliente #{clienteId}";
                string telefono = row?["Telefono"]?.ToString()?.Trim() ?? string.Empty;

                int numero = pagoId > 0 ? pagoId : clienteId;
                var data = new FacturaMembresiaData
                {
                    ClienteId = clienteId,
                    ClienteNombre = nombre,
                    ClienteTelefono = telefono,
                    NombrePlan = nombrePlan,
                    MontoPagado = montoPagado,
                    PrecioUnitario = montoPagado,
                    FechaEmision = DateTime.Now,
                    FechaVencimientoMembresia = fechaVencimiento,
                    MetodoPago = string.IsNullOrWhiteSpace(metodoPago) ? "Efectivo" : metodoPago,
                    NumeroFactura = numero,
                    NotaImportanteExtra = notaExtra
                };

                return GenerarYGuardar(data);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Factura PDF: {ex.Message}");
                return null;
            }
        }

        private static string Sanitize(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Cliente";
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name.Trim();
        }
    }

    internal sealed class FacturaMembresiaDocument : IDocument
    {
        private static readonly string Blue = "#0D6EFD";
        private static readonly string Green = "#1DB954";
        private static readonly string PinRed = "#E53935";

        private readonly FacturaMembresiaData _data;
        private readonly string? _logoPath;

        public FacturaMembresiaDocument(FacturaMembresiaData data)
        {
            _data = data;
            _logoPath = FacturaStorage.ResolverLogoPath();
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            var es = CultureInfo.GetCultureInfo("es-DO");
            string monto = FormatearMonto(_data.MontoPagado);
            string precio = FormatearMonto(_data.PrecioUnitario > 0 ? _data.PrecioUnitario : _data.MontoPagado);
            string fac = $"FAC-{Math.Max(1, _data.NumeroFactura):D2}";
            string miembro = $"#{Math.Max(1, _data.ClienteId):D2}";
            string plan = NormalizarPlan(_data.NombrePlan);
            string venceLargo = CapitalizarMes(
                _data.FechaVencimientoMembresia.ToString("dd 'de' MMMM 'de' yyyy", es), es);
            string nota = string.IsNullOrWhiteSpace(_data.NotaImportanteExtra)
                ? $"Tu membresía vence el próximo {venceLargo}. Recuerda registrar tu pago."
                : _data.NotaImportanteExtra.Trim();
            string telefonoCliente = string.IsNullOrWhiteSpace(_data.ClienteTelefono)
                ? "-"
                : _data.ClienteTelefono;

            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(40);
                page.MarginVertical(36);
                page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(11).FontColor(Colors.Black));

                page.Content().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem(2).Column(brand =>
                        {
                            brand.Item().Text("MFFITNESS").FontSize(24).Bold();
                            brand.Item().PaddingTop(2).Text("Tu mejor versión empieza aquí").FontSize(10);
                            brand.Item().PaddingTop(8).Text("Tel: 809-839-2136").FontSize(10);
                            brand.Item().Row(loc =>
                            {
                                loc.AutoItem().Text("📍").FontSize(10).FontColor(PinRed);
                                loc.AutoItem().PaddingLeft(3).Text("La Reforma del yuna").FontSize(10);
                            });
                        });

                        row.ConstantItem(88).AlignCenter().AlignMiddle().Element(logo =>
                        {
                            if (_logoPath != null)
                                logo.Width(82).Height(82).Image(_logoPath).FitArea();
                            else
                                logo.Width(82).Height(82).Background(Colors.Black).AlignCenter().AlignMiddle()
                                    .Text("MF").FontColor(Colors.White).Bold().FontSize(22);
                        });

                        row.RelativeItem(2).AlignRight().Column(doc =>
                        {
                            doc.Item().AlignRight().Text("FACTURA").FontSize(24).Bold();
                            doc.Item().PaddingTop(4).AlignRight().Text($"N #{fac}").FontSize(12).SemiBold();
                            doc.Item().PaddingTop(2).AlignRight().Text("PAGADO").FontSize(14).Bold().FontColor(Green);
                        });
                    });

                    col.Item().PaddingTop(12).LineHorizontal(3).LineColor(Colors.Black);

                    col.Item().PaddingTop(16).Row(row =>
                    {
                        row.RelativeItem().Column(left =>
                        {
                            left.Item().Text("FACTURADO A:").Bold().FontSize(11);
                            left.Item().PaddingTop(6).Text(_data.ClienteNombre);
                            left.Item().Text($"ID Miembro {miembro}");
                            left.Item().Text($"Tel: {telefonoCliente}");
                        });

                        row.RelativeItem().Column(right =>
                        {
                            right.Item().Text("DETALLES DE LA FACTURA").Bold().FontSize(11);
                            right.Item().PaddingTop(6).Text($"Fecha de emisión: {_data.FechaEmision:dd/MM/yyyy}");
                            right.Item().Text($"Fecha de vencimiento: {_data.FechaVencimientoMembresia:dd/MM/yyyy}");
                            right.Item().Text($"Metodo de pago: {_data.MetodoPago}");
                        });
                    });

                    col.Item().PaddingTop(22).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3.2f);
                            columns.RelativeColumn(1.2f);
                            columns.ConstantColumn(55);
                            columns.RelativeColumn(1.2f);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCell).Text("DESCRIPCIÓN DEL SERVICIO");
                            header.Cell().Element(HeaderCell).AlignRight().Text("PRECIO UNIT.");
                            header.Cell().Element(HeaderCell).AlignCenter().Text("CANT.");
                            header.Cell().Element(HeaderCell).AlignRight().Text("TOTAL");
                        });

                        table.Cell().PaddingVertical(12).PaddingHorizontal(10).Text(plan).Bold();
                        table.Cell().PaddingVertical(12).PaddingHorizontal(10).AlignRight().Text(precio);
                        table.Cell().PaddingVertical(12).PaddingHorizontal(10).AlignCenter().Text("01");
                        table.Cell().PaddingVertical(12).PaddingHorizontal(10).AlignRight().Text(monto).Bold();
                    });

                    col.Item().Height(160);
                    col.Item().LineHorizontal(3).LineColor(Colors.Black);

                    col.Item().PaddingTop(14).Row(row =>
                    {
                        row.RelativeItem(1.5f).Column(info =>
                        {
                            info.Item().Text("INFORMACIÓN IMPORTANTE:").Bold().FontSize(11);
                            info.Item().PaddingTop(6).Width(280).Text(nota).FontSize(11).LineHeight(1.35f);
                        });

                        row.RelativeItem().AlignRight().Column(total =>
                        {
                            total.Item().AlignRight().Text("TOTAL PAGADO").Bold().FontSize(16);
                            total.Item().PaddingTop(6).AlignRight().Text(monto).Bold().FontSize(18);
                        });
                    });

                    col.Item().PaddingTop(42).AlignCenter()
                        .Text(text =>
                        {
                            text.Span("GRACIAS POR SER PARTE DE LA FAMILIA ").Bold().FontSize(13);
                            text.Span("⚡").FontSize(13).FontColor("#F5C518");
                        });
                });
            });

            IContainer HeaderCell(IContainer container) =>
                container
                    .Background(Blue)
                    .PaddingVertical(8)
                    .PaddingHorizontal(10)
                    .DefaultTextStyle(x => x.FontColor(Colors.White).Bold().FontSize(10));
        }

        private static string NormalizarPlan(string plan)
        {
            string p = (plan ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(p))
                return "PLAN";
            if (!p.StartsWith("PLAN", StringComparison.OrdinalIgnoreCase))
                return $"PLAN {p}".ToUpperInvariant();
            return p.ToUpperInvariant();
        }

        private static string FormatearMonto(decimal monto) =>
            "RD$" + monto.ToString("#,0", CultureInfo.GetCultureInfo("es-DO"));

        private static string CapitalizarMes(string texto, CultureInfo culture)
        {
            var partes = texto.Split(' ');
            for (int i = 0; i < partes.Length; i++)
            {
                if (i > 0 && partes[i - 1].Equals("de", StringComparison.OrdinalIgnoreCase)
                    && partes[i].Length > 2
                    && !partes[i].All(char.IsDigit))
                {
                    partes[i] = culture.TextInfo.ToTitleCase(partes[i]);
                    break;
                }
            }
            return string.Join(' ', partes);
        }
    }
}
