-- =========================================
-- WHATSAPP PRODUCCION - MFFITNESS
-- Ejecutar en: (localdb)\MSSQLLocalDB - MF CYBER DB
-- =========================================

USE [MF CYBER DB];
GO

-- Columna para evitar duplicados y rastrear deuda/membresia
IF COL_LENGTH('dbo.RegistroMensajes', 'ReferenciaId') IS NULL
BEGIN
    ALTER TABLE dbo.RegistroMensajes ADD ReferenciaId INT NULL;
    PRINT 'Columna ReferenciaId agregada a RegistroMensajes';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RegistroMensajes_ReferenciaId')
BEGIN
    CREATE INDEX IX_RegistroMensajes_ReferenciaId
    ON dbo.RegistroMensajes (ReferenciaId, Tipo, Estado);
END
GO

-- =========================================
-- PLANTILLA FACTURA MEMBRESIA
-- =========================================
IF NOT EXISTS (SELECT 1 FROM MensajesAutomaticos WHERE Tipo = 'FACTURA_MEMBRESIA')
BEGIN
    INSERT INTO MensajesAutomaticos (Tipo, Plantilla, Activa)
    VALUES (
        'FACTURA_MEMBRESIA',
        N'MFFITNESS - COMPROBANTE DE PAGO

Hola {CLIENTE}!

Plan: {PLAN}
Monto pagado: RD${MONTO}
Metodo: {METODO_PAGO}
Fecha de pago: {FECHA_PAGO}
Membresia valida hasta: {FECHA_VENCE}
No. Recibo: {NUMERO_RECIBO}

Gracias por tu confianza. MF Fitness',
        1
    );
    PRINT 'Plantilla FACTURA_MEMBRESIA creada';
END
ELSE
BEGIN
    UPDATE MensajesAutomaticos
    SET Plantilla = N'MFFITNESS - COMPROBANTE DE PAGO

Hola {CLIENTE}!

Plan: {PLAN}
Monto pagado: RD${MONTO}
Metodo: {METODO_PAGO}
Fecha de pago: {FECHA_PAGO}
Membresia valida hasta: {FECHA_VENCE}
No. Recibo: {NUMERO_RECIBO}

Gracias por tu confianza. MF Fitness',
        Activa = 1
    WHERE Tipo = 'FACTURA_MEMBRESIA';
    PRINT 'Plantilla FACTURA_MEMBRESIA actualizada';
END
GO

-- =========================================
-- PLANTILLA FINANCIAMIENTO
-- =========================================
IF NOT EXISTS (SELECT 1 FROM MensajesAutomaticos WHERE Tipo = 'FINANCIAMIENTO')
BEGIN
    INSERT INTO MensajesAutomaticos (Tipo, Plantilla, Activa)
    VALUES (
        'FINANCIAMIENTO',
        N'Hola {CLIENTE}! Tu membresia {PLAN} fue activada en MFFITNESS.

Precio total: RD${PRECIO_TOTAL}
Pago inicial: RD${PAGO_INICIAL}
Saldo pendiente: RD${SALDO}
Fecha limite de pago: {FECHA_VENCIMIENTO}

Ya puedes entrenar. Completa tu pago antes de la fecha limite.',
        1
    );
END
GO

-- =========================================
-- RECORDATORIO DEUDA - 5 DIAS
-- =========================================
UPDATE MensajesAutomaticos
SET Plantilla = N'Hola {CLIENTE}, recordatorio de MFFITNESS:

Tu deuda vence en {DIAS_RESTANTES} dias ({FECHA_VENCIMIENTO})
Concepto: {CONCEPTO}
Saldo pendiente: RD${SALDO}

Te esperamos para saldarla.',
    Activa = 1
WHERE Tipo = 'RECORDATORIO_VENCIMIENTO_DEUDA';
GO

-- =========================================
-- VENCIMIENTO MEMBRESIA - 5 DIAS
-- =========================================
UPDATE MensajesAutomaticos
SET Plantilla = N'Hola {CLIENTE}! Tu membresia en MFFITNESS vence el {FECHA_VENCE}. Renueva a tiempo para seguir entrenando.',
    Activa = 1
WHERE Tipo = 'VENCIMIENTO_PROXIMO';
GO

-- =========================================
-- CLIENTE PRODUCCION: RAMON ESPINAL
-- =========================================
IF EXISTS (SELECT 1 FROM Clientes WHERE Nombre LIKE '%RAMON%ESPINAL%')
BEGIN
    UPDATE Clientes
    SET Telefono = '+18098392136'
    WHERE Nombre LIKE '%RAMON%ESPINAL%';
    PRINT 'Telefono actualizado para Ramon Espinal';
END
ELSE IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Telefono LIKE '%8098392136%')
BEGIN
    INSERT INTO Clientes (Nombre, Telefono, Direccion, FechaNacimiento)
    VALUES ('RAMON ESPINAL', '+18098392136', 'Cliente produccion', GETDATE());
    PRINT 'Cliente Ramon Espinal creado';
END
GO

PRINT 'Script WhatsApp produccion completado.';
