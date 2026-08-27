-- Auditoría de sugerencias IA (Ollama visión). No crea productos ni toca stock/ventas.
-- Destino: SchemaVersion 21.
-- Idempotente.

IF OBJECT_ID(N'dbo.ProductoVisionAuditoria', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductoVisionAuditoria
    (
        Id                   INT            IDENTITY(1,1) NOT NULL,
        FechaUtc             DATETIME2(0)   NOT NULL
            CONSTRAINT DF_ProductoVisionAuditoria_FechaUtc DEFAULT (SYSUTCDATETIME()),
        Usuario              NVARCHAR(100)  NULL,
        Modelo               NVARCHAR(80)   NULL,
        NombreSugerido       NVARCHAR(200)  NULL,
        CategoriaSugerida    NVARCHAR(120)  NULL,
        PrecioCompraEstimado DECIMAL(18, 2) NULL,
        PrecioVentaEstimado  DECIMAL(18, 2) NULL,
        RespuestaRaw         NVARCHAR(MAX)  NULL,
        CONSTRAINT PK_ProductoVisionAuditoria PRIMARY KEY CLUSTERED (Id)
    );
END
GO

IF COL_LENGTH(N'dbo.Productos', N'RutaImagen') IS NULL
BEGIN
    ALTER TABLE dbo.Productos
        ADD RutaImagen NVARCHAR(500) NULL;
END
GO
