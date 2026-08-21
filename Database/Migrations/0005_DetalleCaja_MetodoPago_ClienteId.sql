-- Auditoría operativa de movimientos de caja:
-- Método de pago + ClienteId (miembro) para reportes y trazabilidad.
-- Destino: SchemaVersion 5.
-- Requiere esquema POS base (dbo.DetalleCaja). No crea la tabla: solo la altera.

IF OBJECT_ID(N'dbo.DetalleCaja', N'U') IS NULL
BEGIN
    RAISERROR(
        N'Migración 0005: no existe dbo.DetalleCaja. Esta BD no tiene el esquema POS base. Use MF_CYBER_DB_DEV o [MF CYBER DB] con esquema completo (no una BD vacía).',
        16,
        1);
    RETURN;
END
GO

IF COL_LENGTH(N'dbo.DetalleCaja', N'MetodoPago') IS NULL
    ALTER TABLE dbo.DetalleCaja ADD MetodoPago NVARCHAR(50) NULL;
GO

IF COL_LENGTH(N'dbo.DetalleCaja', N'ClienteId') IS NULL
    ALTER TABLE dbo.DetalleCaja ADD ClienteId INT NULL;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_DetalleCaja_ClienteId'
      AND object_id = OBJECT_ID(N'dbo.DetalleCaja'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_DetalleCaja_ClienteId
        ON dbo.DetalleCaja (ClienteId)
        WHERE ClienteId IS NOT NULL;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_DetalleCaja_Clientes'
      AND parent_object_id = OBJECT_ID(N'dbo.DetalleCaja'))
BEGIN
    ALTER TABLE dbo.DetalleCaja WITH NOCHECK
    ADD CONSTRAINT FK_DetalleCaja_Clientes
        FOREIGN KEY (ClienteId) REFERENCES dbo.Clientes (ID);
END
GO

-- Backfill ClienteId desde el patrón histórico del Concepto: "Cliente {id}" / "(Cliente {id})"
;WITH Parsed AS
(
    SELECT
        dc.Id,
        TRY_CAST(
            LEFT(
                SUBSTRING(
                    dc.Concepto,
                    NULLIF(PATINDEX(N'%[Cc]liente [0-9]%', dc.Concepto), 0) + 8,
                    20),
                PATINDEX(
                    N'%[^0-9]%',
                    SUBSTRING(
                        dc.Concepto,
                        NULLIF(PATINDEX(N'%[Cc]liente [0-9]%', dc.Concepto), 0) + 8,
                        20) + N'x') - 1
            ) AS INT
        ) AS ClienteParsed
    FROM dbo.DetalleCaja dc
    WHERE dc.ClienteId IS NULL
      AND dc.Concepto IS NOT NULL
      AND PATINDEX(N'%[Cc]liente [0-9]%', dc.Concepto) > 0
)
UPDATE dc
SET ClienteId = p.ClienteParsed
FROM dbo.DetalleCaja dc
INNER JOIN Parsed p ON p.Id = dc.Id
INNER JOIN dbo.Clientes c ON c.ID = p.ClienteParsed
WHERE dc.ClienteId IS NULL
  AND p.ClienteParsed IS NOT NULL;
GO

-- Backfill ClienteId / MetodoPago desde Ventas (concepto: Venta de productos (Id N))
UPDATE dc
SET
    ClienteId = COALESCE(dc.ClienteId, v.ClienteId),
    MetodoPago = COALESCE(NULLIF(LTRIM(RTRIM(dc.MetodoPago)), N''), v.MetodoPago)
FROM dbo.DetalleCaja dc
INNER JOIN dbo.Ventas v
    ON dc.Concepto LIKE N'%Venta de productos (Id ' + CAST(v.Id AS varchar(20)) + N')%'
WHERE (dc.ClienteId IS NULL AND v.ClienteId IS NOT NULL)
   OR (NULLIF(LTRIM(RTRIM(dc.MetodoPago)), N'') IS NULL AND v.MetodoPago IS NOT NULL);
GO

-- Backfill MetodoPago desde Pagos (mismo día + cliente + monto)
UPDATE dc
SET MetodoPago = p.MetodoPago
FROM dbo.DetalleCaja dc
INNER JOIN dbo.Pagos p
    ON dc.ClienteId = p.ClienteId
   AND CAST(dc.Fecha AS date) = CAST(p.FechaPago AS date)
   AND ABS(dc.Monto - p.Monto) < 0.01
WHERE NULLIF(LTRIM(RTRIM(dc.MetodoPago)), N'') IS NULL
  AND dc.TipoMovimiento = N'INGRESO'
  AND p.MetodoPago IS NOT NULL;
GO

-- Backfill MetodoPago desde PagosDeuda
UPDATE dc
SET MetodoPago = pd.MetodoPago
FROM dbo.DetalleCaja dc
INNER JOIN dbo.PagosDeuda pd
    ON CAST(dc.Fecha AS date) = CAST(pd.Fecha AS date)
   AND ABS(dc.Monto - pd.Monto) < 0.01
   AND ABS(DATEDIFF(SECOND, pd.Fecha, dc.Fecha)) <= 120
WHERE NULLIF(LTRIM(RTRIM(dc.MetodoPago)), N'') IS NULL
  AND dc.TipoMovimiento = N'INGRESO'
  AND dc.Concepto LIKE N'%deuda%'
  AND pd.MetodoPago IS NOT NULL;
GO
