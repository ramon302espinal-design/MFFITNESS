-- CRM Financiero FASE 11.6: baseline snapshot en ActionRecord.
-- Destino: SchemaVersion 13.
-- Columna BaselinePayload: codec texto v1 (BusinessActionBaselineCodec). No muta POS.

IF COL_LENGTH(N'dbo.CrmBusinessActions', N'BaselinePayload') IS NULL
BEGIN
    ALTER TABLE dbo.CrmBusinessActions
        ADD BaselinePayload NVARCHAR(MAX) NULL;
END
GO
