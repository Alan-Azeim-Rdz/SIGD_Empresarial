USE SIGD_Central;
GO

-- Departamento
SET IDENTITY_INSERT Departamento ON;
INSERT INTO Departamento (Id, Nombre, Abreviatura, Estatus, FechaCreacion)
VALUES (1, 'Sistemas', 'SIS', 1, GETDATE());
SET IDENTITY_INSERT Departamento OFF;
GO

-- Roles
SET IDENTITY_INSERT Rol ON;
INSERT INTO Rol (Id, Nombre, Descripcion, Estatus, FechaCreacion) VALUES
(1, 'Admin',    'Administrador del sistema', 1, GETDATE()),
(2, 'Revisor',  'Revisa y aprueba documentos', 1, GETDATE()),
(3, 'Operario', 'Consulta documentos vigentes', 1, GETDATE());
SET IDENTITY_INSERT Rol OFF;
GO

-- Usuario admin (contraseña en texto plano, como espera AuthController)
SET IDENTITY_INSERT Usuario ON;
INSERT INTO Usuario (Id, IdDepartamento, Nombre, ApellidoP, ApellidoM, Correo, Contrasena, Estatus, FechaCreacion)
VALUES (1, 1, 'Administrador', 'del', 'Sistema', 'admin@sigd.local', 'Admin@SIGD2026!', 1, GETDATE());
SET IDENTITY_INSERT Usuario OFF;
GO

-- Vincular admin con rol Admin
INSERT INTO Usuario_Rol (IdUsuario, IdRol, Estatus, FechaCreacion)
VALUES (1, 1, 1, GETDATE());
GO

PRINT 'Datos semilla insertados correctamente.';