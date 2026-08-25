-- Código de barras por producto (lector NetumScan / teclado HID).
-- Destino: SchemaVersion 20.
-- Idempotente.

IF COL_LENGTH('dbo.Productos', 'CodigoBarra') IS NULL
BEGIN
    ALTER TABLE dbo.Productos
        ADD CodigoBarra NVARCHAR(32) NULL;
END
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_Productos_CodigoBarra'
      AND object_id = OBJECT_ID(N'dbo.Productos'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UX_Productos_CodigoBarra
        ON dbo.Productos (CodigoBarra)
        WHERE CodigoBarra IS NOT NULL AND Activo = 1;
END
GO
