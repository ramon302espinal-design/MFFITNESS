-- CRM Financiero FASE 11.8: deltas post-acción en ActionRecord.
-- Destino: SchemaVersion 14.
-- Columna DeltasPayload: codec texto (BusinessActionDeltaCodec). No muta POS.

IF COL_LENGTH(N'dbo.CrmBusinessActions', N'DeltasPayload') IS NULL
BEGIN
    ALTER TABLE dbo.CrmBusinessActions
        ADD DeltasPayload NVARCHAR(MAX) NULL;
END
GO
