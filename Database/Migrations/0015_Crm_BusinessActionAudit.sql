-- CRM Financiero FASE 11.12: auditoría de acciones de negocio.
-- Destino: SchemaVersion 15.
-- Append-only: quién (Sesion) / qué / cuándo. No muta POS.

IF OBJECT_ID(N'dbo.CrmBusinessActionAudit', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CrmBusinessActionAudit
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CrmBusinessActionAudit PRIMARY KEY,
        ActionId UNIQUEIDENTIFIER NOT NULL,
        DecisionEventId UNIQUEIDENTIFIER NULL,
        ActionType INT NULL,
        -- 1 Register … 9 SetEvaluationWindow
        AuditAction TINYINT NOT NULL,
        PreviousStatus TINYINT NULL,
        NewStatus TINYINT NULL,
        Outcome TINYINT NULL,
        Actor NVARCHAR(80) NULL,
        ActorUserId INT NULL,
        Note NVARCHAR(500) NULL,
        Detail NVARCHAR(500) NULL,
        AtUtc DATETIME2 NOT NULL CONSTRAINT DF_CrmBusinessActionAudit_AtUtc DEFAULT (SYSUTCDATETIME())
    );

    CREATE INDEX IX_CrmBusinessActionAudit_ActionId_At
        ON dbo.CrmBusinessActionAudit (ActionId, AtUtc DESC);

    CREATE INDEX IX_CrmBusinessActionAudit_DecisionEventId_At
        ON dbo.CrmBusinessActionAudit (DecisionEventId, AtUtc DESC);

    CREATE INDEX IX_CrmBusinessActionAudit_AtUtc
        ON dbo.CrmBusinessActionAudit (AtUtc DESC);

    CREATE INDEX IX_CrmBusinessActionAudit_Actor_At
        ON dbo.CrmBusinessActionAudit (Actor, AtUtc DESC);
END
GO
