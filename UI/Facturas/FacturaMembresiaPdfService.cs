using System.Diagnostics;
using BLL.Facturas;
using BLL.Models;
using CORE;

namespace UI.Facturas
{
    /// <summary>
    /// Wrapper UI: reutiliza PDF ya generado (WhatsApp) o lo crea si falta.
    /// </summary>
    public static class FacturaMembresiaPdfService
    {
        public static void ConfigurarLicencia() =>
            FacturaMembresiaPdfGenerator.ConfigurarLicencia();

        /// <summary>Compat: genera y opcionalmente abre. Por defecto ya no abre.</summary>
        public static string? GenerarAbrirDesdeOperacion(
            IWin32Window? owner,
            int clienteId,
            string nombrePlan,
            decimal montoPagado,
            DateTime fechaVencimiento,
            string metodoPago,
            MembresiaOperacionResult operacion,
            string? notaExtra = null)
            => GenerarDesdeOperacion(
                owner, clienteId, nombrePlan, montoPagado, fechaVencimiento,
                metodoPago, operacion, notaExtra, abrirPdf: false);

        public static string? GenerarDesdeOperacion(
            IWin32Window? owner,
            int clienteId,
            string nombrePlan,
            decimal montoPagado,
            DateTime fechaVencimiento,
            string metodoPago,
            MembresiaOperacionResult operacion,
            string? notaExtra = null,
            bool abrirPdf = false)
        {
            try
            {
                int pagoId = operacion.PagoId > 0
                    ? operacion.PagoId
                    : (operacion.MembresiaId > 0 ? operacion.MembresiaId : clienteId);

                string? path = FacturaStorage.ResolverRutaFacturaExistente(pagoId);
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                {
                    path = FacturaMembresiaPdfGenerator.GenerarDesdePago(
                        clienteId,
                        nombrePlan,
                        montoPagado,
                        fechaVencimiento,
                        metodoPago,
                        pagoId,
                        notaExtra);
                }

                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                    throw new Exception("No se pudo guardar el PDF de la factura.");

                if (abrirPdf)
                    AbrirArchivo(path);

                return path;
            }
            catch (Exception ex)
            {
                // Solo avisar si hay owner UI y se pidió abrir; post-pago silencioso solo loguea.
                if (owner != null && abrirPdf)
                {
                    MessageBox.Show(
                        owner,
                        "El pago se registró, pero no se pudo generar la factura PDF.\n\n" + ex.Message,
                        "Factura",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                else
                {
                    Debug.WriteLine($"[Factura PDF] {ex.Message}");
                }

                return null;
            }
        }

        private static void AbrirArchivo(string path)
        {
            string temp = Path.Combine(
                Path.GetTempPath(),
                $"MFFITNESS_{Path.GetFileNameWithoutExtension(path)}_{DateTime.Now:HHmmss}.pdf");
            File.Copy(path, temp, overwrite: true);

            Process.Start(new ProcessStartInfo
            {
                FileName = temp,
                UseShellExecute = true
            });
        }
    }
}
