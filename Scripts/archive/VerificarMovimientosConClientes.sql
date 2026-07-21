-- ===============================================
-- Script: Verificar Movimientos de Caja con ClienteId
-- Propósito: Validar que la consulta de movimientos incluya correctamente el ClienteId
-- ===============================================

USE [MF CYBER DB];
GO

-- Verificar últimos movimientos con ClienteId
SELECT 
	dc.Id AS MovimientoId,
	dc.TipoMovimiento, 
	dc.Concepto, 
	dc.Monto, 
	dc.Fecha, 
	dc.Usuario,
	CASE 
		WHEN dc.Concepto LIKE '%Pago membresía%' OR dc.Concepto LIKE '%Renovación%' THEN p.ClienteId
		WHEN dc.Concepto LIKE '%Venta%' THEN v.ClienteId
		WHEN dc.Concepto LIKE '%Abono deuda%' THEN d.ClienteId
		ELSE NULL
	END AS ClienteId,
	CASE 
		WHEN dc.Concepto LIKE '%Pago membresía%' OR dc.Concepto LIKE '%Renovación%' THEN c1.Nombre
		WHEN dc.Concepto LIKE '%Venta%' THEN c2.Nombre
		WHEN dc.Concepto LIKE '%Abono deuda%' THEN c3.Nombre
		ELSE NULL
	END AS NombreCliente
FROM DetalleCaja dc
LEFT JOIN Pagos p ON (dc.Concepto LIKE '%Pago membresía%' OR dc.Concepto LIKE '%Renovación%')
					 AND CAST(dc.Fecha AS DATE) = CAST(p.FechaPago AS DATE)
					 AND dc.Monto = p.Monto
LEFT JOIN Clientes c1 ON c1.Id = p.ClienteId
LEFT JOIN Ventas v ON dc.Concepto LIKE '%Venta%'
					  AND CAST(dc.Fecha AS DATE) = CAST(v.FechaVenta AS DATE)
					  AND dc.Monto = v.Total
LEFT JOIN Clientes c2 ON c2.Id = v.ClienteId
LEFT JOIN Deudas d ON dc.Concepto LIKE '%Abono deuda%'
LEFT JOIN Clientes c3 ON c3.Id = d.ClienteId
WHERE CAST(dc.Fecha AS DATE) = CAST(GETDATE() AS DATE)
ORDER BY dc.Fecha DESC;

PRINT '✅ Consulta completada. Revisa los resultados para verificar que ClienteId se vincule correctamente.';
