-- Script: Ajustar restricción CHECK en DetalleCaja para permitir Monto = 0
-- Recomendación: ejecutar en entorno de staging/backup antes de producción.

SET NOCOUNT ON;

BEGIN TRY
	BEGIN TRANSACTION;

	DECLARE @constraintName sysname;

	SELECT TOP 1 @constraintName = cc.name
	FROM sys.check_constraints cc
	WHERE cc.parent_object_id = OBJECT_ID(N'dbo.DetalleCaja')
	  AND cc.definition LIKE N'%Monto%>%';

	IF @constraintName IS NOT NULL
	BEGIN
		PRINT 'Encontrado constraint: ' + @constraintName;
		EXEC('ALTER TABLE dbo.DetalleCaja DROP CONSTRAINT [' + @constraintName + ']');
		PRINT 'Constraint eliminado: ' + @constraintName;
	END
	ELSE
	BEGIN
		PRINT 'No se encontró constraint tipo CHECK con "Monto > ..." en DetalleCaja.';
	END

	IF NOT EXISTS (
		SELECT 1 FROM sys.check_constraints cc
		WHERE cc.parent_object_id = OBJECT_ID(N'dbo.DetalleCaja')
		  AND cc.name = N'CK_DetalleCaja_Monto_NonNegative'
	)
	BEGIN
		ALTER TABLE dbo.DetalleCaja
		ADD CONSTRAINT CK_DetalleCaja_Monto_NonNegative CHECK (Monto >= 0);

		PRINT 'Constraint CK_DetalleCaja_Monto_NonNegative creada.';
	END
	ELSE
	BEGIN
		PRINT 'Constraint CK_DetalleCaja_Monto_NonNegative ya existe.';
	END

	-- Mostrar definición de la columna Monto
	SELECT COLUMN_NAME, IS_NULLABLE, DATA_TYPE, NUMERIC_PRECISION, NUMERIC_SCALE
	FROM INFORMATION_SCHEMA.COLUMNS
	WHERE TABLE_NAME = 'DetalleCaja' AND COLUMN_NAME = 'Monto';

	COMMIT TRANSACTION;
END TRY
BEGIN CATCH
	IF XACT_STATE() <> 0
		ROLLBACK TRANSACTION;

	DECLARE @ErrMsg nvarchar(4000) = ERROR_MESSAGE();
	RAISERROR('Error al aplicar cambios en DetalleCaja: %s', 16, 1, @ErrMsg);
END CATCH
