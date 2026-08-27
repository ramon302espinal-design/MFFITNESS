-- Ampliar Concepto en DetalleCaja para detalle de factura (IA).
-- Destino: SchemaVersion 29.
-- Idempotente. El runner registra la versión.

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF COL_LENGTH(N'dbo.DetalleCaja', N'Concepto') IS NOT NULL
BEGIN
    DECLARE @len INT =
        (SELECT CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS
         WHERE TABLE_SCHEMA = N'dbo' AND TABLE_NAME = N'DetalleCaja' AND COLUMN_NAME = N'Concepto');

    IF @len IS NOT NULL AND @len > 0 AND @len < 1000
        ALTER TABLE dbo.DetalleCaja ALTER COLUMN Concepto NVARCHAR(1000) NOT NULL;
END
GO
