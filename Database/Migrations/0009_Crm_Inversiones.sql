-- CRM Financiero FASE 6.3: inversiones + vínculo a ENTRADAS de stock.
-- Destino: SchemaVersion 9.
-- Compra operativa = MovimientosStock ENTRADA (no se crea tabla Compras).

IF OBJECT_ID(N'dbo.CrmInversiones', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CrmInversiones
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CrmInversiones PRIMARY KEY,
        Nombre NVARCHAR(120) NOT NULL,
        Descripcion NVARCHAR(500) NULL,
        FechaInicio DATE NOT NULL,
        FechaCierre DATE NULL,
        Estado TINYINT NOT NULL CONSTRAINT DF_CrmInversiones_Estado DEFAULT (0),
        -- 0 Planificada, 1 Activa, 2 Recuperada, 3 Cerrada, 4 ConPerdida
        Observaciones NVARCHAR(500) NULL,
        UsuarioCreacion NVARCHAR(80) NULL,
        FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_CrmInversiones_FechaCreacion DEFAULT (SYSDATETIME())
    );
END
GO

IF OBJECT_ID(N'dbo.CrmInversionLineas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CrmInversionLineas
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CrmInversionLineas PRIMARY KEY,
        InversionId INT NOT NULL,
        MovimientoStockId INT NOT NULL,
        -- v1: una ENTRADA pertenece a una sola inversión (UNIQUE)
        FechaAsignacion DATETIME2 NOT NULL CONSTRAINT DF_CrmInversionLineas_FechaAsignacion DEFAULT (SYSDATETIME()),
        CONSTRAINT FK_CrmInversionLineas_Inversion
            FOREIGN KEY (InversionId) REFERENCES dbo.CrmInversiones (Id),
        CONSTRAINT FK_CrmInversionLineas_Movimiento
            FOREIGN KEY (MovimientoStockId) REFERENCES dbo.MovimientosStock (Id),
        CONSTRAINT UQ_CrmInversionLineas_Movimiento UNIQUE (MovimientoStockId)
    );

    CREATE INDEX IX_CrmInversionLineas_InversionId
        ON dbo.CrmInversionLineas (InversionId);
END
GO
