-- Idempotencia webhook Twilio inbound (MessageSid unico).
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = N'ChatMensajes')
AND NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_ChatMensajes_TwilioSid'
      AND object_id = OBJECT_ID(N'dbo.ChatMensajes'))
BEGIN
    CREATE UNIQUE INDEX UX_ChatMensajes_TwilioSid
        ON dbo.ChatMensajes (TwilioMessageSid)
        WHERE TwilioMessageSid IS NOT NULL;
END
