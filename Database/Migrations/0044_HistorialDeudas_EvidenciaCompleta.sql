-- Evidencia completa HistorialDeudas: columnas de auditoría + SP enriquecido.
-- Destino: SchemaVersion 44. Idempotente PROD (MF CYBER DB) y DEV.

-- Columnas de evidencia (nullable: filas antiguas sin foto).
IF COL_LENGTH(N'dbo.HistorialDeudas', N'SaldoResultante') IS NULL
    ALTER TABLE dbo.HistorialDeudas ADD SaldoResultante DECIMAL(18,2) NULL;
GO

IF COL_LENGTH(N'dbo.HistorialDeudas', N'MetodoPago') IS NULL
    ALTER TABLE dbo.HistorialDeudas ADD MetodoPago NVARCHAR(50) NULL;
GO

IF COL_LENGTH(N'dbo.HistorialDeudas', N'CuotaNumero') IS NULL
    ALTER TABLE dbo.HistorialDeudas ADD CuotaNumero INT NULL;
GO

IF COL_LENGTH(N'dbo.HistorialDeudas', N'FaltaCuota') IS NULL
    ALTER TABLE dbo.HistorialDeudas ADD FaltaCuota DECIMAL(18,2) NULL;
GO

-- Descripción más larga para evidencia de abono/cuota/mora.
IF EXISTS (
    SELECT 1
    FROM sys.columns c
    INNER JOIN sys.types t ON t.user_type_id = c.user_type_id
    WHERE c.object_id = OBJECT_ID(N'dbo.HistorialDeudas')
      AND c.name = N'Descripcion'
      AND t.name IN (N'varchar', N'nvarchar')
      AND c.max_length > 0
      AND c.max_length < 1000)
BEGIN
    ALTER TABLE dbo.HistorialDeudas ALTER COLUMN Descripcion NVARCHAR(500) NULL;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_ObtenerHistorial
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
        ISNULL(d.Concepto, N'') AS ConceptoDeuda,
        d.FechaVencimiento AS FechaLimitePago,
        h.Monto,
        h.SaldoResultante,
        h.MetodoPago,
        h.CuotaNumero,
        h.FaltaCuota,
        h.Fecha,
        h.Usuario,
        (
            SELECT COUNT(1)
            FROM dbo.RegistroMensajes rm
            WHERE rm.ReferenciaId = h.DeudaId
              AND ISNULL(rm.Estado, N'') = N'ENVIADO'
        ) AS AvisosWhatsApp
    FROM dbo.HistorialDeudas h
    INNER JOIN dbo.Clientes c ON c.ID = h.ClienteId
    LEFT JOIN dbo.Deudas d ON d.Id = h.DeudaId
    WHERE (@ClienteId IS NULL OR h.ClienteId = @ClienteId)
      AND (@Tipo IS NULL OR h.TipoMovimiento = @Tipo)
      AND (@Desde IS NULL OR h.Fecha >= @Desde)
      AND (@Hasta IS NULL OR h.Fecha <= @Hasta)
    ORDER BY h.Fecha DESC, h.Id DESC;
END
GO
