-- Ventas pausadas (hold cart POS): cabecera + detalle.
-- No afecta Ventas/Caja/Stock/Deudas: solo snapshot del carrito.
-- Destino: SchemaVersion 26.
-- Idempotente.

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.VentasPausadas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.VentasPausadas
    (
        Id              INT IDENTITY(1,1) NOT NULL,
        ClienteId       INT NOT NULL,
        ClienteNombre   NVARCHAR(200) NOT NULL,
        Total           DECIMAL(18,2) NOT NULL,
        Usuario         NVARCHAR(100) NULL,
        FechaPausa      DATETIME2(0) NOT NULL
            CONSTRAINT DF_VentasPausadas_FechaPausa DEFAULT (SYSDATETIME()),
        Estado          NVARCHAR(20) NOT NULL
            CONSTRAINT DF_VentasPausadas_Estado DEFAULT (N'PAUSADA'),
        CONSTRAINT PK_VentasPausadas PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_VentasPausadas_Clientes
            FOREIGN KEY (ClienteId) REFERENCES dbo.Clientes (ID),
        CONSTRAINT CK_VentasPausadas_Estado
            CHECK (Estado IN (N'PAUSADA', N'DESPAUSADA', N'CANCELADA')),
        CONSTRAINT CK_VentasPausadas_Total
            CHECK (Total >= 0)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_VentasPausadas_ClienteActiva'
      AND object_id = OBJECT_ID(N'dbo.VentasPausadas'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UX_VentasPausadas_ClienteActiva
        ON dbo.VentasPausadas (ClienteId)
        WHERE Estado = N'PAUSADA';
END
GO

IF OBJECT_ID(N'dbo.VentasPausadasDetalle', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.VentasPausadasDetalle
    (
        Id               INT IDENTITY(1,1) NOT NULL,
        VentaPausadaId   INT NOT NULL,
        ProductoId       INT NOT NULL,
        Producto         NVARCHAR(200) NOT NULL,
        Precio           DECIMAL(18,2) NOT NULL,
        Cantidad         INT NOT NULL,
        Total            DECIMAL(18,2) NOT NULL,
        CONSTRAINT PK_VentasPausadasDetalle PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_VentasPausadasDetalle_Cabecera
            FOREIGN KEY (VentaPausadaId) REFERENCES dbo.VentasPausadas (Id),
        CONSTRAINT FK_VentasPausadasDetalle_Productos
            FOREIGN KEY (ProductoId) REFERENCES dbo.Productos (Id),
        CONSTRAINT CK_VentasPausadasDetalle_Cantidad
            CHECK (Cantidad > 0),
        CONSTRAINT CK_VentasPausadasDetalle_Total
            CHECK (Total >= 0)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_VentasPausadasDetalle_VentaPausadaId'
      AND object_id = OBJECT_ID(N'dbo.VentasPausadasDetalle'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_VentasPausadasDetalle_VentaPausadaId
        ON dbo.VentasPausadasDetalle (VentaPausadaId);
END
GO
