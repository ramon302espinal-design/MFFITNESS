-- Préstamos con plazos, interés y mora (cabecera + detalle).
-- Mapeo UI FrmModuloDeudas.tabCrear → pnlIntereses / dgvPlazos.
-- Destino: SchemaVersion 39.
-- Idempotente. MF CYBER DB (PROD) y MF_CYBER_DB_DEV.
-- No altera dbo.Deudas ni flujos de financiamiento existente.

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* -------------------------------------------------------------------------
   PrestamosCuotas — encabezado (frecuencia, %, plazos, resumen, mora)
   ------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.PrestamosCuotas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PrestamosCuotas
    (
        Id                  INT IDENTITY(1,1) NOT NULL,
        DeudaId             INT NOT NULL,
        ClienteId           INT NOT NULL,
        -- rdSemanal / rdQuincenal / rdMensual
        Frecuencia          NVARCHAR(20) NOT NULL,
        -- txtInteres
        InteresPorcentaje   DECIMAL(9,4) NOT NULL
            CONSTRAINT DF_PrestamosCuotas_InteresPorcentaje DEFAULT (0),
        -- numPlazos
        NumeroPlazos        INT NOT NULL,
        -- txtResumenFinanciamiento
        InteresTotal        DECIMAL(18,2) NOT NULL
            CONSTRAINT DF_PrestamosCuotas_InteresTotal DEFAULT (0),
        TotalConInteres     DECIMAL(18,2) NOT NULL
            CONSTRAINT DF_PrestamosCuotas_TotalConInteres DEFAULT (0),
        CuotaBase           DECIMAL(18,2) NOT NULL
            CONSTRAINT DF_PrestamosCuotas_CuotaBase DEFAULT (0),
        -- chkMora
        ActivarMora         BIT NOT NULL
            CONSTRAINT DF_PrestamosCuotas_ActivarMora DEFAULT (0),
        FechaCreacion       DATETIME2(0) NOT NULL
            CONSTRAINT DF_PrestamosCuotas_FechaCreacion DEFAULT (SYSDATETIME()),
        Usuario             NVARCHAR(100) NULL,
        Estado              NVARCHAR(20) NOT NULL
            CONSTRAINT DF_PrestamosCuotas_Estado DEFAULT (N'ACTIVO'),

        CONSTRAINT PK_PrestamosCuotas PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_PrestamosCuotas_DeudaId UNIQUE (DeudaId),
        CONSTRAINT FK_PrestamosCuotas_Deudas
            FOREIGN KEY (DeudaId) REFERENCES dbo.Deudas (Id) ON DELETE CASCADE,
        CONSTRAINT FK_PrestamosCuotas_Clientes
            FOREIGN KEY (ClienteId) REFERENCES dbo.Clientes (ID),
        CONSTRAINT CK_PrestamosCuotas_Frecuencia
            CHECK (Frecuencia IN (N'SEMANAL', N'QUINCENAL', N'MENSUAL')),
        CONSTRAINT CK_PrestamosCuotas_NumeroPlazos
            CHECK (NumeroPlazos > 0),
        CONSTRAINT CK_PrestamosCuotas_Montos
            CHECK (InteresPorcentaje >= 0
               AND InteresTotal >= 0
               AND TotalConInteres >= 0
               AND CuotaBase >= 0),
        CONSTRAINT CK_PrestamosCuotas_Estado
            CHECK (Estado IN (N'ACTIVO', N'CANCELADO', N'LIQUIDADO'))
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_PrestamosCuotas_ClienteId'
      AND object_id = OBJECT_ID(N'dbo.PrestamosCuotas'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_PrestamosCuotas_ClienteId
        ON dbo.PrestamosCuotas (ClienteId);
END
GO

/* -------------------------------------------------------------------------
   PrestamoDetalleCuotas — filas de dgvPlazos
   Columnas: CUOTA # | FECHA VENCIMIENTO | CAPITAL | INTERES | MORA POTENCIAL | TOTAL
   ------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.PrestamoDetalleCuotas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PrestamoDetalleCuotas
    (
        Id                  INT IDENTITY(1,1) NOT NULL,
        PrestamoId          INT NOT NULL,
        NumeroCuota         INT NOT NULL,
        FechaVencimiento    DATE NOT NULL,
        Capital             DECIMAL(18,2) NOT NULL
            CONSTRAINT DF_PrestamoDetalleCuotas_Capital DEFAULT (0),
        Interes             DECIMAL(18,2) NOT NULL
            CONSTRAINT DF_PrestamoDetalleCuotas_Interes DEFAULT (0),
        MoraPotencial       DECIMAL(18,2) NOT NULL
            CONSTRAINT DF_PrestamoDetalleCuotas_MoraPotencial DEFAULT (0),
        Total               DECIMAL(18,2) NOT NULL
            CONSTRAINT DF_PrestamoDetalleCuotas_Total DEFAULT (0),

        CONSTRAINT PK_PrestamoDetalleCuotas PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_PrestamoDetalleCuotas_Prestamo_Cuota
            UNIQUE (PrestamoId, NumeroCuota),
        CONSTRAINT FK_PrestamoDetalleCuotas_PrestamosCuotas
            FOREIGN KEY (PrestamoId) REFERENCES dbo.PrestamosCuotas (Id) ON DELETE CASCADE,
        CONSTRAINT CK_PrestamoDetalleCuotas_NumeroCuota
            CHECK (NumeroCuota > 0),
        CONSTRAINT CK_PrestamoDetalleCuotas_Montos
            CHECK (Capital >= 0
               AND Interes >= 0
               AND MoraPotencial >= 0
               AND Total >= 0)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_PrestamoDetalleCuotas_FechaVencimiento'
      AND object_id = OBJECT_ID(N'dbo.PrestamoDetalleCuotas'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_PrestamoDetalleCuotas_FechaVencimiento
        ON dbo.PrestamoDetalleCuotas (FechaVencimiento);
END
GO
