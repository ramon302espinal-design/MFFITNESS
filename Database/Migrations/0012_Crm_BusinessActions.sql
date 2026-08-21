-- CRM Financiero FASE 11.4: acciones de negocio (ActionRecord).
-- Destino: SchemaVersion 12.
-- Append-oriented. NO muta ventas/costos/stock históricos.
-- DecisionEventId / DecisionHistoryId opcionales (vínculo FASE 10).

IF OBJECT_ID(N'dbo.CrmBusinessActions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CrmBusinessActions
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CrmBusinessActions PRIMARY KEY,
        ActionId UNIQUEIDENTIFIER NOT NULL,
        DecisionEventId UNIQUEIDENTIFIER NULL,
        DecisionHistoryId BIGINT NULL,
        -- BusinessActionType enum
        ActionType INT NOT NULL,
        Area TINYINT NOT NULL,
        EntityType TINYINT NOT NULL,
        EntityId NVARCHAR(64) NULL,
        EntityName NVARCHAR(200) NULL,
        Description NVARCHAR(1000) NOT NULL,
        Reason NVARCHAR(1000) NULL,
        Notes NVARCHAR(1000) NULL,
        QuantityInvolved DECIMAL(18,4) NULL,
        CapitalInvolved DECIMAL(18,2) NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_CrmBusinessActions_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedBy NVARCHAR(80) NULL,
        -- 1 Pending, 2 InProgress, 3 Completed, 4 Cancelled, 5 NoResult
        Status TINYINT NOT NULL CONSTRAINT DF_CrmBusinessActions_Status DEFAULT (1),
        StartedAt DATETIME2 NULL,
        EvaluationDays INT NULL,
        EvaluationDueAt DATETIME2 NULL,
        CompletedAt DATETIME2 NULL,
        CompletedBy NVARCHAR(80) NULL,
        -- ExpectedImpact
        ExpectedSummary NVARCHAR(500) NULL,
        ExpectedNotes NVARCHAR(500) NULL,
        -- Claves métrica separadas por | (ej. capital.frozen|sales.revenue)
        ExpectedMetricKeys NVARCHAR(500) NULL,
        -- ActualImpact (relleno 11.8+)
        Outcome TINYINT NULL,
        Confidence TINYINT NULL,
        ActualSummary NVARCHAR(1000) NULL,
        ActualNotes NVARCHAR(500) NULL,
        CONSTRAINT UQ_CrmBusinessActions_ActionId UNIQUE (ActionId),
        CONSTRAINT FK_CrmBusinessActions_History
            FOREIGN KEY (DecisionHistoryId) REFERENCES dbo.CrmDecisionEvents (Id)
    );

    CREATE INDEX IX_CrmBusinessActions_Status_Created
        ON dbo.CrmBusinessActions (Status, CreatedAt DESC);

    CREATE INDEX IX_CrmBusinessActions_DecisionEvent
        ON dbo.CrmBusinessActions (DecisionEventId);

    CREATE INDEX IX_CrmBusinessActions_Entity
        ON dbo.CrmBusinessActions (EntityType, EntityId);

    CREATE INDEX IX_CrmBusinessActions_ActionType
        ON dbo.CrmBusinessActions (ActionType, CreatedAt DESC);
END
GO
