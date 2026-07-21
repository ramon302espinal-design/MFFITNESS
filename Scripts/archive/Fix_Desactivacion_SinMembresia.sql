-- Ya no se usa FechaFin = NULL (columna NOT NULL en BD).
-- La baja SIN MEMBRESIA se identifica por historial SALIDA + Activa = 0.
-- Este script solo asegura membresias con ultimo movimiento SALIDA queden inactivas.

UPDATE m
SET m.Activa = 0
FROM Membresias m
INNER JOIN (
    SELECT h.ClienteId
    FROM HistorialMembresias h
    INNER JOIN (
        SELECT ClienteId, MAX(Fecha) AS UltimaFecha
        FROM HistorialMembresias
        GROUP BY ClienteId
    ) u ON u.ClienteId = h.ClienteId AND u.UltimaFecha = h.Fecha
    WHERE h.TipoMovimiento = 'SALIDA'
) sal ON sal.ClienteId = m.ClienteId
WHERE m.Activa = 1;

PRINT 'Membresias con baja SALIDA marcadas como inactivas (SIN MEMBRESIA en grid).';
