UPDATE Usuarios
SET FechaAlta = GETDATE()
WHERE FechaAlta IS NULL;