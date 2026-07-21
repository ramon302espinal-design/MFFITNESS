-- =========================================
-- AUTOMATIZACION WHATSAPP PRODUCCION - MFFITNESS
-- Ejecutar en: (localdb)\MSSQLLocalDB - MF CYBER DB
-- =========================================

USE [MF CYBER DB];
GO

IF NOT EXISTS (SELECT 1 FROM MensajesAutomaticos WHERE Tipo = 'VENCIMIENTO_HOY')
BEGIN
    INSERT INTO MensajesAutomaticos (Tipo, Plantilla, Activa)
    VALUES (
        'VENCIMIENTO_HOY',
        N'MFFITNESS - AVISO URGENTE

Hola {CLIENTE}!

Tu plan {PLAN} vence HOY ({FECHA_VENCE}).
Renueva hoy para mantener tu acceso activo.

MF Fitness',
        1
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM MensajesAutomaticos WHERE Tipo = 'MEMBRESIA_VENCIDA')
BEGIN
    INSERT INTO MensajesAutomaticos (Tipo, Plantilla, Activa)
    VALUES (
        'MEMBRESIA_VENCIDA',
        N'MFFITNESS - MEMBRESIA VENCIDA

Hola {CLIENTE}!

Tu plan {PLAN} vencio el {FECHA_VENCE}.
Motivo: {MOTIVO}

Acercate al gimnasio para renovar tu membresia.

MF Fitness',
        1
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM MensajesAutomaticos WHERE Tipo = 'DEUDA_VENCE_HOY')
BEGIN
    INSERT INTO MensajesAutomaticos (Tipo, Plantilla, Activa)
    VALUES (
        'DEUDA_VENCE_HOY',
        N'MFFITNESS - PAGO VENCE HOY

Hola {CLIENTE}!

Tu deuda "{CONCEPTO}" vence HOY ({FECHA_VENCIMIENTO}).
Saldo pendiente: {SALDO}

Realiza tu pago hoy para evitar suspension.

MF Fitness',
        1
    );
END
GO

PRINT 'Plantillas de automatizacion WhatsApp listas.';
