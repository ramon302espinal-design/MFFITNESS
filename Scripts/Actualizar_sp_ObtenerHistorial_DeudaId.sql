-- Historial de deudas: DeudaId + fecha límite vinculada al módulo de deudas.
USE [MF CYBER DB];
GO

IF OBJECT_ID('dbo.sp_ObtenerHistorial', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ObtenerHistorial;
GO

CREATE PROCEDURE dbo.sp_ObtenerHistorial
    @ClienteId INT = NULL,
    @Tipo VARCHAR(50) = NULL,
    @Desde DATETIME = NULL,
    @Hasta DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        h.Id,
        h.DeudaId,
        c.Nombre,
        h.TipoMovimiento AS Tipo,
        h.Descripcion,
        d.FechaVencimiento AS FechaLimitePago,
        h.Monto,
        h.Fecha,
        h.Usuario
    FROM HistorialDeudas h
    INNER JOIN Clientes c ON c.ID = h.ClienteId
    LEFT JOIN Deudas d ON d.Id = h.DeudaId
    WHERE (@ClienteId IS NULL OR h.ClienteId = @ClienteId)
      AND (@Tipo IS NULL OR h.TipoMovimiento = @Tipo)
      AND (@Desde IS NULL OR h.Fecha >= @Desde)
      AND (@Hasta IS NULL OR h.Fecha <= @Hasta)
    ORDER BY h.Fecha DESC;
END
GO
