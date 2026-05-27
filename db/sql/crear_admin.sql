-- Insertar usuario administrador inicial
-- Contraseña: Admin2026* (hash SHA256 Unicode)
INSERT INTO Usuario (
    IdDepartamento, Nombre, ApellidoP, Correo,
    Contrasena, Estatus, FechaCreacion, IdUsuarioCreacion
)
VALUES (
    1,
    'Administrador',
    'Sistema',
    'admin@sigd.com',
    (SELECT HASHBYTES('SHA2_256', N'Admin2026*')),
    1,
    GETDATE(),
    1
);

-- Asignar rol Administrador al usuario creado
INSERT INTO UsuarioRol (IdUsuario, IdRol, Estatus, FechaCreacion, IdUsuarioCreacion)
VALUES (
    (SELECT TOP 1 Id FROM Usuario WHERE Correo = 'admin@sigd.com'),
    (SELECT TOP 1 Id FROM Rol WHERE Nombre = 'Administrador'),
    1,
    GETDATE(),
    1
);
