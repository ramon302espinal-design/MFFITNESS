-- =========================================
-- PLANTILLAS DE NOTIFICACIONES - MÓDULO DE DEUDAS
-- =========================================
-- Ejecutar contra: (localdb)\MSSQLLocalDB - MF CYBER DB
-- Fecha: 30/06/2026
-- =========================================

USE [MF CYBER DB];
GO

-- =========================================
-- 1. PLANTILLA: DEUDA CREADA
-- =========================================
IF NOT EXISTS (SELECT 1 FROM MensajesAutomaticos WHERE Tipo = 'DEUDA_CREADA')
BEGIN
	INSERT INTO MensajesAutomaticos (Tipo, Plantilla, Activa)
	VALUES (
		'DEUDA_CREADA',
		N'Hola {CLIENTE}, se ha registrado una deuda a tu nombre en MFFITNESS:

📋 Concepto: {CONCEPTO}
💰 Monto: ${MONTO}
📅 Vence: {FECHA_VENCIMIENTO}

¡Gracias por tu confianza! 💪 Te esperamos.',
		1
	);
	PRINT '✅ Plantilla DEUDA_CREADA creada';
END
ELSE
BEGIN
	UPDATE MensajesAutomaticos
	SET Plantilla = N'Hola {CLIENTE}, se ha registrado una deuda a tu nombre en MFFITNESS:

📋 Concepto: {CONCEPTO}
💰 Monto: ${MONTO}
📅 Vence: {FECHA_VENCIMIENTO}

¡Gracias por tu confianza! 💪 Te esperamos.',
		Activa = 1
	WHERE Tipo = 'DEUDA_CREADA';
	PRINT '✅ Plantilla DEUDA_CREADA actualizada';
END
GO

-- =========================================
-- 2. PLANTILLA: RECORDATORIO DE VENCIMIENTO (3 días antes)
-- =========================================
IF NOT EXISTS (SELECT 1 FROM MensajesAutomaticos WHERE Tipo = 'RECORDATORIO_VENCIMIENTO_DEUDA')
BEGIN
	INSERT INTO MensajesAutomaticos (Tipo, Plantilla, Activa)
	VALUES (
		'RECORDATORIO_VENCIMIENTO_DEUDA',
		N'Hola {CLIENTE}, recordatorio amigable de MFFITNESS:

⏰ Tu deuda vence en {DIAS_RESTANTES} días
📋 Concepto: {CONCEPTO}
💰 Saldo pendiente: ${SALDO}

¡Te esperamos para saldarla! 💪',
		1
	);
	PRINT '✅ Plantilla RECORDATORIO_VENCIMIENTO_DEUDA creada';
END
ELSE
BEGIN
	UPDATE MensajesAutomaticos
	SET Plantilla = N'Hola {CLIENTE}, recordatorio amigable de MFFITNESS:

⏰ Tu deuda vence en {DIAS_RESTANTES} días
📋 Concepto: {CONCEPTO}
💰 Saldo pendiente: ${SALDO}

¡Te esperamos para saldarla! 💪',
		Activa = 1
	WHERE Tipo = 'RECORDATORIO_VENCIMIENTO_DEUDA';
	PRINT '✅ Plantilla RECORDATORIO_VENCIMIENTO_DEUDA actualizada';
END
GO

-- =========================================
-- 3. PLANTILLA: DEUDA VENCIDA
-- =========================================
IF NOT EXISTS (SELECT 1 FROM MensajesAutomaticos WHERE Tipo = 'DEUDA_VENCIDA')
BEGIN
	INSERT INTO MensajesAutomaticos (Tipo, Plantilla, Activa)
	VALUES (
		'DEUDA_VENCIDA',
		N'Hola {CLIENTE}, tu deuda en MFFITNESS ha vencido:

📋 Concepto: {CONCEPTO}
💰 Saldo: ${SALDO}
📅 Vencía el: {FECHA_VENCIMIENTO}

Por favor, acércate a saldarla. ¡Gracias por tu comprensión! 💪',
		1
	);
	PRINT '✅ Plantilla DEUDA_VENCIDA creada';
END
ELSE
BEGIN
	UPDATE MensajesAutomaticos
	SET Plantilla = N'Hola {CLIENTE}, tu deuda en MFFITNESS ha vencido:

📋 Concepto: {CONCEPTO}
💰 Saldo: ${SALDO}
📅 Vencía el: {FECHA_VENCIMIENTO}

Por favor, acércate a saldarla. ¡Gracias por tu comprensión! 💪',
		Activa = 1
	WHERE Tipo = 'DEUDA_VENCIDA';
	PRINT '✅ Plantilla DEUDA_VENCIDA actualizada';
END
GO

-- =========================================
-- 4. PLANTILLA: PAGO RECIBIDO (Deuda parcial)
-- =========================================
IF NOT EXISTS (SELECT 1 FROM MensajesAutomaticos WHERE Tipo = 'PAGO_DEUDA_RECIBIDO')
BEGIN
	INSERT INTO MensajesAutomaticos (Tipo, Plantilla, Activa)
	VALUES (
		'PAGO_DEUDA_RECIBIDO',
		N'¡Gracias {CLIENTE}! Pago recibido en MFFITNESS:

💰 Monto pagado: ${MONTO_PAGO}
📊 Saldo restante: ${SALDO}

¡Sigue así! 💪',
		1
	);
	PRINT '✅ Plantilla PAGO_DEUDA_RECIBIDO creada';
END
ELSE
BEGIN
	UPDATE MensajesAutomaticos
	SET Plantilla = N'¡Gracias {CLIENTE}! Pago recibido en MFFITNESS:

💰 Monto pagado: ${MONTO_PAGO}
📊 Saldo restante: ${SALDO}

¡Sigue así! 💪',
		Activa = 1
	WHERE Tipo = 'PAGO_DEUDA_RECIBIDO';
	PRINT '✅ Plantilla PAGO_DEUDA_RECIBIDO actualizada';
END
GO

-- =========================================
-- 5. PLANTILLA: DEUDA PAGADA COMPLETA
-- =========================================
IF NOT EXISTS (SELECT 1 FROM MensajesAutomaticos WHERE Tipo = 'DEUDA_PAGADA_COMPLETA')
BEGIN
	INSERT INTO MensajesAutomaticos (Tipo, Plantilla, Activa)
	VALUES (
		'DEUDA_PAGADA_COMPLETA',
		N'¡FELICIDADES {CLIENTE}! 🎉

Tu deuda ha sido saldada completamente en MFFITNESS.
💰 Monto total: ${MONTO_TOTAL}

¡Eres un campeón! Gracias por tu compromiso. 💪
No naciste pa'' semilla! ⚡',
		1
	);
	PRINT '✅ Plantilla DEUDA_PAGADA_COMPLETA creada';
END
ELSE
BEGIN
	UPDATE MensajesAutomaticos
	SET Plantilla = N'¡FELICIDADES {CLIENTE}! 🎉

Tu deuda ha sido saldada completamente en MFFITNESS.
💰 Monto total: ${MONTO_TOTAL}

¡Eres un campeón! Gracias por tu compromiso. 💪
No naciste pa'' semilla! ⚡',
		Activa = 1
	WHERE Tipo = 'DEUDA_PAGADA_COMPLETA';
	PRINT '✅ Plantilla DEUDA_PAGADA_COMPLETA actualizada';
END
GO

-- =========================================
-- VERIFICACIÓN FINAL
-- =========================================
SELECT 
	Tipo,
	LEFT(Plantilla, 50) + '...' AS Vista_Previa,
	Activa,
	FechaCreacion
FROM MensajesAutomaticos
WHERE Tipo LIKE '%DEUDA%' OR Tipo LIKE '%RECORDATORIO_VENCIMIENTO_DEUDA%'
ORDER BY Tipo;

PRINT '';
PRINT '=========================================';
PRINT '✅ TODAS LAS PLANTILLAS DE DEUDAS CREADAS';
PRINT '=========================================';
PRINT '1. DEUDA_CREADA';
PRINT '2. RECORDATORIO_VENCIMIENTO_DEUDA';
PRINT '3. DEUDA_VENCIDA';
PRINT '4. PAGO_DEUDA_RECIBIDO';
PRINT '5. DEUDA_PAGADA_COMPLETA';
PRINT '';
PRINT '📱 Listo para enviar notificaciones WhatsApp';
PRINT '=========================================';
GO
