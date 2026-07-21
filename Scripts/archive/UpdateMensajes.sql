UPDATE [dbo].[MensajesAutomaticos] 
SET [Plantilla] = 'Hola {CLIENTE}! Gracias por tu pago. Tu membresía ha sido activada. Fecha de vencimiento: {FECHA_VENCE}. Saludos, MF Fitness' 
WHERE [Tipo] = 'PAGO_MEMBRESIA';

UPDATE [dbo].[MensajesAutomaticos] 
SET [Plantilla] = 'Hola {CLIENTE}! Tu membresía vence en 3 días. Fecha final: {FECHA_VENCE}. Renueva ahora. Saludos, MF Fitness' 
WHERE [Tipo] = 'VENCIMIENTO_PROXIMO';
