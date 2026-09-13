using System;
using System.Collections.Generic;
using DL;

namespace BLL
{
    public sealed class PrestamoCronogramaDto
    {
        public string Frecuencia { get; init; } = "SEMANAL";
        public decimal InteresPorcentaje { get; init; }
        public int NumeroPlazos { get; init; }
        public decimal InteresTotal { get; init; }
        public decimal TotalConInteres { get; init; }
        public decimal CuotaBase { get; init; }
        public bool ActivarMora { get; init; }
        public List<PrestamoCuotaDetalleRow> Cuotas { get; init; } = new();
    }

    public class PrestamoCuotasBLL
    {
        private readonly PrestamoCuotasDAL dal = new();

        public int RegistrarTrasDeudaProductoCredito(
            int deudaId,
            int clienteId,
            PrestamoCronogramaDto cronograma,
            string? usuario,
            string? conceptoAviso = null)
        {
            if (deudaId <= 0)
                throw new Exception("Deuda inválida para el préstamo.");
            if (clienteId <= 0)
                throw new Exception("Cliente inválido.");
            if (cronograma == null)
                throw new ArgumentNullException(nameof(cronograma));
            if (cronograma.NumeroPlazos <= 0 || cronograma.Cuotas.Count == 0)
                throw new Exception("Configure plazos e interés antes de guardar el producto a crédito.");

            string freq = (cronograma.Frecuencia ?? string.Empty).Trim().ToUpperInvariant();
            if (freq is not ("SEMANAL" or "QUINCENAL" or "MENSUAL"))
                throw new Exception("Frecuencia de plazos inválida.");

            int prestamoId = dal.InsertarPrestamoConDetalle(
                deudaId,
                clienteId,
                freq,
                cronograma.InteresPorcentaje,
                cronograma.NumeroPlazos,
                cronograma.InteresTotal,
                cronograma.TotalConInteres,
                cronograma.CuotaBase,
                cronograma.ActivarMora,
                usuario,
                cronograma.Cuotas);

            // WhatsApp enriquecido: la venta omitió el aviso genérico a propósito.
            NotificarWhatsAppPrestamoEnBackground(clienteId, deudaId, conceptoAviso, cronograma);

            return prestamoId;
        }

        private static void NotificarWhatsAppPrestamoEnBackground(
            int clienteId,
            int deudaId,
            string? conceptoAviso,
            PrestamoCronogramaDto cronograma)
        {
            int clienteIdBg = clienteId;
            int deudaIdBg = deudaId;
            string conceptoBg = string.IsNullOrWhiteSpace(conceptoAviso)
                ? "Producto a credito"
                : conceptoAviso.Trim();
            PrestamoCronogramaDto cronoBg = cronograma;

            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    new MensajeAutomaticoBLL().EnviarNotificacionDeudaCreadaConPrestamo(
                        clienteIdBg,
                        conceptoBg,
                        cronoBg,
                        deudaIdBg);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Error WhatsApp préstamo plazos (bg): {ex.Message}");
                }
            });
        }
    }
}
