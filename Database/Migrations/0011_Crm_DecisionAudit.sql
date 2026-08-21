-- CRM Financiero FASE 10.23: auditoría de decisiones.
-- Destino: SchemaVersion 11.
-- Append-only: quién / qué / cuándo sobre CrmDecisionEvents.

IF OBJECT_ID(N'dbo.CrmDecisionAudit', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CrmDecisionAudit
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CrmDecisionAudit PRIMARY KEY,
        HistoryId BIGINT NULL,
        EventId UNIQUEIDENTIFIER NULL,
        Fingerprint NVARCHAR(220) NULL,
        EventType NVARCHAR(80) NULL,
        -- 1 Detected, 2 StartReview, 3 Resolve, 4 Ignore, 5 Reopen, 6 DuplicateSuppressed
        Action TINYINT NOT NULL,
        PreviousStatus TINYINT NULL,
        NewStatus TINYINT NULL,
        Actor NVARCHAR(80) NULL,
        Note NVARCHAR(500) NULL,
        Detail NVARCHAR(500) NULL,
        AtUtc DATETIME2 NOT NULL CONSTRAINT DF_CrmDecisionAudit_AtUtc DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_CrmDecisionAudit_History
            FOREIGN KEY (HistoryId) REFERENCES dbo.CrmDecisionEvents (Id)
    );

    CREATE INDEX IX_CrmDecisionAudit_EventId_At
        ON dbo.CrmDecisionAudit (EventId, AtUtc DESC);

    CREATE INDEX IX_CrmDecisionAudit_HistoryId_At
        ON dbo.CrmDecisionAudit (HistoryId, AtUtc DESC);

    CREATE INDEX IX_CrmDecisionAudit_AtUtc
        ON dbo.CrmDecisionAudit (AtUtc DESC);
END
GO
