-- CRM Financiero FASE 10.21: historial de DecisionEvent.
-- Destino: SchemaVersion 10.
-- Append-only de detecciones. Resolución/ignorar = FASE 10.22 (columnas preparadas).

IF OBJECT_ID(N'dbo.CrmDecisionEvents', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CrmDecisionEvents
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CrmDecisionEvents PRIMARY KEY,
        EventId UNIQUEIDENTIFIER NOT NULL,
        Fingerprint NVARCHAR(220) NOT NULL,
        EventType NVARCHAR(80) NOT NULL,
        Area TINYINT NOT NULL,
        EntityType TINYINT NOT NULL,
        EntityId NVARCHAR(64) NULL,
        EntityName NVARCHAR(200) NULL,
        PeriodKey NVARCHAR(80) NULL,
        Severity TINYINT NOT NULL CONSTRAINT DF_CrmDecisionEvents_Severity DEFAULT (0),
        Priority TINYINT NOT NULL CONSTRAINT DF_CrmDecisionEvents_Priority DEFAULT (0),
        -- 1 Active, 2 Resolved, 3 Ignored, 4 InsufficientData, 5 InReview
        Status TINYINT NOT NULL CONSTRAINT DF_CrmDecisionEvents_Status DEFAULT (1),
        Title NVARCHAR(200) NOT NULL,
        Description NVARCHAR(1000) NULL,
        Reason NVARCHAR(1000) NULL,
        Impact NVARCHAR(500) NULL,
        Recommendation NVARCHAR(1000) NULL,
        Source NVARCHAR(120) NULL,
        GroupKey NVARCHAR(120) NULL,
        DetectedAt DATETIME2 NOT NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_CrmDecisionEvents_CreatedAt DEFAULT (SYSDATETIME()),
        -- Placeholders FASE 10.22 (resolución / ignorado)
        ResolvedAt DATETIME2 NULL,
        ResolvedBy NVARCHAR(80) NULL,
        ResolutionNote NVARCHAR(500) NULL,
        CONSTRAINT UQ_CrmDecisionEvents_EventId UNIQUE (EventId)
    );

    CREATE INDEX IX_CrmDecisionEvents_Fingerprint_Status
        ON dbo.CrmDecisionEvents (Fingerprint, Status);

    CREATE INDEX IX_CrmDecisionEvents_DetectedAt
        ON dbo.CrmDecisionEvents (DetectedAt DESC);

    CREATE INDEX IX_CrmDecisionEvents_EventType_DetectedAt
        ON dbo.CrmDecisionEvents (EventType, DetectedAt DESC);

    CREATE INDEX IX_CrmDecisionEvents_Entity
        ON dbo.CrmDecisionEvents (EntityType, EntityId);
END
GO
