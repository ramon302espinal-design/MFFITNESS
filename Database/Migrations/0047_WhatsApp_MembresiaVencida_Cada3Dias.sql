-- Aviso de membresía vencida: deja de ser diario.
-- Cada 3 días, a partir de las 9:00. No reenvía historial ni toca otras automatizaciones.
-- Destino: SchemaVersion 47. Idempotente.

IF OBJECT_ID(N'dbo.WhatsAppAutomatizacionConfig', N'U') IS NOT NULL
BEGIN
    UPDATE dbo.WhatsAppAutomatizacionConfig
    SET Condicion = N'Cada 3 días, a partir de las 9:00. No cambia el acceso.',
        EvitarDuplicadoHoras = 72,
        FechaModificacion = SYSDATETIME()
    WHERE Tipo = N'MEMBRESIA_VENCIDA'
      AND (
          EvitarDuplicadoHoras <= 24
          OR Condicion LIKE N'Aviso diario%'
      );
END
GO
