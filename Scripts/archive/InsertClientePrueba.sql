-- Crear cliente de prueba para WhatsApp
INSERT INTO [dbo].[Clientes] ([Nombre], [Telefono], [Direccion], [FechaNacimiento])
VALUES ('Juan Prueba WhatsApp', '+18098392136', 'Dirección Prueba', GETDATE());

-- Obtener el ID del cliente
SELECT TOP 1 [Id], [Nombre], [Telefono] FROM [dbo].[Clientes] ORDER BY [Id] DESC;
