-- Saldo a favor (prepago reserva productos POS): cabecera + detalle.
-- Cobro: caja + saldo ACTIVO, sin Ventas/Stock.
-- Despacho: Ventas + Stock, sin nuevo ingreso de caja.
-- Destino: SchemaVersion 30.
-- Idempotente.

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.SaldoClientes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SaldoClientes
    (
        Id                INT IDENTITY(1,1) NOT NULL,
        ClienteId         INT NOT NULL,
        ClienteNombre     NVARCHAR(200) NOT NULL,
        TotalReserva      DECIMAL(18,2) NOT NULL,
        MontoCobrado      DECIMAL(18,2) NOT NULL,
        CajaMovimientoId  INT NULL,
        VentaId           INT NULL,
        Usuario           NVARCHAR(100) NULL,
        FechaCobro        DATETIME2(0) NOT NULL
            CONSTRAINT DF_SaldoClientes_FechaCobro DEFAULT (SYSDATETIME()),
        FechaDespacho     DATETIME2(0) NULL,
        Estado            NVARCHAR(20) NOT NULL
            CONSTRAINT DF_SaldoClientes_Estado DEFAULT (N'ACTIVO'),
        CONSTRAINT PK_SaldoClientes PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_SaldoClientes_Clientes
            FOREIGN KEY (ClienteId) REFERENCES dbo.Clientes (ID),
        CONSTRAINT FK_SaldoClientes_Ventas
            FOREIGN KEY (VentaId) REFERENCES dbo.Ventas (Id),
        CONSTRAINT CK_SaldoClientes_Estado
            CHECK (Estado IN (N'ACTIVO', N'DESPACHADO', N'CANCELADO')),
        CONSTRAINT CK_SaldoClientes_TotalReserva
            CHECK (TotalReserva >= 0),
        CONSTRAINT CK_SaldoClientes_MontoCobrado
            CHECK (MontoCobrado >= 0)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_SaldoClientes_ClienteActivo'
      AND object_id = OBJECT_ID(N'dbo.SaldoClientes'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UX_SaldoClientes_ClienteActivo
        ON dbo.SaldoClientes (ClienteId)
        WHERE Estado = N'ACTIVO';
END
GO

IF OBJECT_ID(N'dbo.SaldoClientesDetalle', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SaldoClientesDetalle
    (
        Id              INT IDENTITY(1,1) NOT NULL,
        SaldoClienteId  INT NOT NULL,
        ProductoId      INT NOT NULL,
        Producto        NVARCHAR(200) NOT NULL,
        Precio          DECIMAL(18,2) NOT NULL,
        Cantidad        INT NOT NULL,
        Total           DECIMAL(18,2) NOT NULL,
        CONSTRAINT PK_SaldoClientesDetalle PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_SaldoClientesDetalle_Cabecera
            FOREIGN KEY (SaldoClienteId) REFERENCES dbo.SaldoClientes (Id),
        CONSTRAINT FK_SaldoClientesDetalle_Productos
            FOREIGN KEY (ProductoId) REFERENCES dbo.Productos (Id),
        CONSTRAINT CK_SaldoClientesDetalle_Cantidad
            CHECK (Cantidad > 0),
        CONSTRAINT CK_SaldoClientesDetalle_Total
            CHECK (Total >= 0)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_SaldoClientesDetalle_SaldoClienteId'
      AND object_id = OBJECT_ID(N'dbo.SaldoClientesDetalle'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_SaldoClientesDetalle_SaldoClienteId
        ON dbo.SaldoClientesDetalle (SaldoClienteId);
END
GO
