-- ===============================================================
-- Columna Intentos en RegistroMensajes (reintentos controlados)
-- Ejecutar una vez en produccion
-- ===============================================================

USE [MF CYBER DB];
GO

IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'RegistroMensajes'
      AND COLUMN_NAME = 'Intentos'
)
BEGIN
    ALTER TABLE [dbo].[RegistroMensajes]
    ADD [Intentos] INT NOT NULL CONSTRAINT DF_RegistroMensajes_Intentos DEFAULT 0;

    PRINT 'Columna Intentos agregada a RegistroMensajes.';
END
ELSE
BEGIN
    PRINT 'Columna Intentos ya existe.';
END
GO
