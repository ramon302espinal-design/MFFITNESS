-- ContentSid por plantilla (opcional; si vacio usa TwilioContentSidGenerico de App.config)
USE [MF CYBER DB];
GO

IF COL_LENGTH('dbo.MensajesAutomaticos', 'ContentSid') IS NULL
BEGIN
    ALTER TABLE dbo.MensajesAutomaticos ADD ContentSid NVARCHAR(64) NULL;
    PRINT 'Columna ContentSid agregada a MensajesAutomaticos';
END
GO

PRINT 'Configure TwilioContentSidGenerico en UI/App.config (plantilla Approved con {{1}}, ej. HX4b6bbb98799fc7a7fe02187bceb46ecb).';
