# 📊 SCRIPTS SQL - DATOS INICIALES

## Precaución
⚠️ **Ejecutar SOLO si las tablas están vacías**
⚠️ **Hacer backup antes de ejecutar**

---

## Script 1: Departamentos Básicos

```sql
-- LIMPIAR (si es necesario)
-- DELETE FROM Departamento WHERE Nombre IN ('Administración', 'Ventas', 'IT', 'RRHH', 'Finanzas')

-- INSERTAR DEPARTAMENTOS
INSERT INTO Departamento (Nombre, Abreviatura, Estatus, FechaCreacion)
VALUES 
('Administración', 'ADM', 1, GETDATE()),
('Ventas', 'VTA', 1, GETDATE()),
('Sistemas/IT', 'SIS', 1, GETDATE()),
('Recursos Humanos', 'RH', 1, GETDATE()),
('Finanzas', 'FIN', 1, GETDATE()),
('Operaciones', 'OPR', 1, GETDATE()),
('Marketing', 'MKT', 1, GETDATE())
GO
```

---

## Script 2: Roles por Nivel

```sql
-- INSERTAR ROLES
INSERT INTO Rol (Nombre, Descripcion, Estatus, FechaCreacion)
VALUES 
('Administrador', 'Acceso total al sistema y panel de administración', 1, GETDATE()),
('Gerente', 'Control sobre su departamento y supervisión de usuarios', 1, GETDATE()),
('Supervisor', 'Supervisión de proyectos y documentos', 1, GETDATE()),
('Empleado', 'Acceso estándar a documentos y funciones básicas', 1, GETDATE()),
('Consultor Externo', 'Acceso limitado a documentos específicos', 1, GETDATE()),
('Auditor', 'Acceso de lectura a toda la información', 1, GETDATE())
GO
```

---

## Script 3: Permisos por Módulo

```sql
-- MÓDULO: DOCUMENTOS
INSERT INTO Permiso (Codigo, Descripcion, Modulo, Estatus, FechaCreacion)
VALUES 
('VER_DOCUMENTOS', 'Ver lista de documentos', 'Documentos', 1, GETDATE()),
('VER_DOCUMENTO_DETALLE', 'Ver detalle de un documento', 'Documentos', 1, GETDATE()),
('DESCARGAR_DOCUMENTO', 'Descargar documentos', 'Documentos', 1, GETDATE()),
('CREAR_DOCUMENTO', 'Crear nuevos documentos', 'Documentos', 1, GETDATE()),
('EDITAR_DOCUMENTO', 'Editar documentos existentes', 'Documentos', 1, GETDATE()),
('ELIMINAR_DOCUMENTO', 'Eliminar documentos', 'Documentos', 1, GETDATE()),
('COMPARTIR_DOCUMENTO', 'Compartir documentos con otros usuarios', 'Documentos', 1, GETDATE()),
('SOLICITAR_FIRMA', 'Solicitar firma electrónica', 'Documentos', 1, GETDATE())

-- MÓDULO: USUARIOS
INSERT INTO Permiso (Codigo, Descripcion, Modulo, Estatus, FechaCreacion)
VALUES 
('VER_USUARIOS', 'Ver lista de usuarios', 'Usuarios', 1, GETDATE()),
('VER_USUARIO_DETALLE', 'Ver detalle de un usuario', 'Usuarios', 1, GETDATE()),
('CREAR_USUARIO', 'Registrar nuevos usuarios', 'Usuarios', 1, GETDATE()),
('EDITAR_USUARIO', 'Editar datos de usuarios', 'Usuarios', 1, GETDATE()),
('CAMBIAR_CONTRASENA_USUARIO', 'Cambiar contraseña de otros usuarios', 'Usuarios', 1, GETDATE()),
('ELIMINAR_USUARIO', 'Desactivar usuarios', 'Usuarios', 1, GETDATE()),
('ASIGNAR_ROLES', 'Asignar roles a usuarios', 'Usuarios', 1, GETDATE()),
('VER_BITACORA_USUARIO', 'Ver accesos y acciones de usuarios', 'Usuarios', 1, GETDATE())

-- MÓDULO: ADMINISTRACIÓN
INSERT INTO Permiso (Codigo, Descripcion, Modulo, Estatus, FechaCreacion)
VALUES 
('GESTIONAR_DEPARTAMENTOS', 'Crear y editar departamentos', 'Administración', 1, GETDATE()),
('GESTIONAR_ROLES', 'Crear, editar y asignar roles', 'Administración', 1, GETDATE()),
('GESTIONAR_PERMISOS', 'Crear y gestionar permisos', 'Administración', 1, GETDATE()),
('GESTIONAR_TIPOS_DOCUMENTO', 'Crear tipos de documento', 'Administración', 1, GETDATE()),
('VER_PANEL_ADMIN', 'Acceder al panel administrativo', 'Administración', 1, GETDATE()),
('CONFIGURAR_SISTEMA', 'Modificar configuración general', 'Administración', 1, GETDATE())

-- MÓDULO: REPORTES
INSERT INTO Permiso (Codigo, Descripcion, Modulo, Estatus, FechaCreacion)
VALUES 
('VER_REPORTES', 'Ver reportes disponibles', 'Reportes', 1, GETDATE()),
('GENERAR_REPORTES', 'Generar nuevos reportes', 'Reportes', 1, GETDATE()),
('EXPORTAR_REPORTES', 'Exportar reportes a Excel/PDF', 'Reportes', 1, GETDATE()),
('VER_BITACORA_DOCUMENTOS', 'Ver historial de documentos', 'Reportes', 1, GETDATE()),
('VER_BITACORA_ACCESOS', 'Ver registro de accesos', 'Reportes', 1, GETDATE())

-- MÓDULO: CONFIGURACIÓN
INSERT INTO Permiso (Codigo, Descripcion, Modulo, Estatus, FechaCreacion)
VALUES 
('CONFIGURAR_PERFILES', 'Configurar perfiles de usuario', 'Configuración', 1, GETDATE()),
('CONFIGURAR_INTEGRACIONES', 'Configurar integraciones externas', 'Configuración', 1, GETDATE()),
('GESTIONAR_RESPALDOS', 'Realizar respaldos del sistema', 'Configuración', 1, GETDATE()),
('VER_LOGS_SISTEMA', 'Ver logs del sistema', 'Configuración', 1, GETDATE())

GO
```

---

## Script 4: Asignar Permisos a Roles

```sql
-- ADMINISTRADOR - Todos los permisos
INSERT INTO RolPermiso (IdRol, IdPermiso, FechaCreacion, Estatus)
SELECT r.Id, p.Id, GETDATE(), 1
FROM Rol r, Permiso p
WHERE r.Nombre = 'Administrador'
GO

-- GERENTE - Permisos para gestionar su departamento
INSERT INTO RolPermiso (IdRol, IdPermiso, FechaCreacion, Estatus)
SELECT r.Id, p.Id, GETDATE(), 1
FROM Rol r, Permiso p
WHERE r.Nombre = 'Gerente'
AND p.Codigo IN (
    'VER_DOCUMENTOS', 'VER_DOCUMENTO_DETALLE', 'DESCARGAR_DOCUMENTO',
    'CREAR_DOCUMENTO', 'EDITAR_DOCUMENTO', 'COMPARTIR_DOCUMENTO',
    'VER_USUARIOS', 'VER_USUARIO_DETALLE', 'CREAR_USUARIO',
    'EDITAR_USUARIO', 'ASIGNAR_ROLES',
    'VER_REPORTES', 'GENERAR_REPORTES', 'EXPORTAR_REPORTES',
    'VER_BITACORA_DOCUMENTOS', 'VER_BITACORA_ACCESOS'
)
GO

-- SUPERVISOR - Permisos de supervisión
INSERT INTO RolPermiso (IdRol, IdPermiso, FechaCreacion, Estatus)
SELECT r.Id, p.Id, GETDATE(), 1
FROM Rol r, Permiso p
WHERE r.Nombre = 'Supervisor'
AND p.Codigo IN (
    'VER_DOCUMENTOS', 'VER_DOCUMENTO_DETALLE', 'DESCARGAR_DOCUMENTO',
    'CREAR_DOCUMENTO', 'EDITAR_DOCUMENTO',
    'VER_USUARIOS', 'VER_USUARIO_DETALLE',
    'VER_REPORTES', 'GENERAR_REPORTES'
)
GO

-- EMPLEADO - Permisos básicos
INSERT INTO RolPermiso (IdRol, IdPermiso, FechaCreacion, Estatus)
SELECT r.Id, p.Id, GETDATE(), 1
FROM Rol r, Permiso p
WHERE r.Nombre = 'Empleado'
AND p.Codigo IN (
    'VER_DOCUMENTOS', 'VER_DOCUMENTO_DETALLE', 'DESCARGAR_DOCUMENTO',
    'CREAR_DOCUMENTO',
    'VER_REPORTES'
)
GO

-- CONSULTOR EXTERNO - Acceso muy limitado
INSERT INTO RolPermiso (IdRol, IdPermiso, FechaCreacion, Estatus)
SELECT r.Id, p.Id, GETDATE(), 1
FROM Rol r, Permiso p
WHERE r.Nombre = 'Consultor Externo'
AND p.Codigo IN (
    'VER_DOCUMENTOS', 'VER_DOCUMENTO_DETALLE', 'DESCARGAR_DOCUMENTO'
)
GO

-- AUDITOR - Acceso de lectura
INSERT INTO RolPermiso (IdRol, IdPermiso, FechaCreacion, Estatus)
SELECT r.Id, p.Id, GETDATE(), 1
FROM Rol r, Permiso p
WHERE r.Nombre = 'Auditor'
AND p.Codigo IN (
    'VER_DOCUMENTOS', 'VER_DOCUMENTO_DETALLE',
    'VER_USUARIOS', 'VER_USUARIO_DETALLE',
    'VER_REPORTES', 'VER_BITACORA_DOCUMENTOS', 
    'VER_BITACORA_ACCESOS', 'VER_LOGS_SISTEMA'
)
GO
```

---

## Script 5: Tipos de Documento

```sql
-- INSERTAR TIPOS DE DOCUMENTO
INSERT INTO TipoDocumento (Nombre, Abreviatura, TiempoRetencionMeses, Estatus, FechaCreacion)
VALUES 
('Contrato', 'CT', 60, 1, GETDATE()),        -- 5 años
('Factura', 'FAC', 72, 1, GETDATE()),        -- 6 años
('Solicitud', 'SOL', 12, 1, GETDATE()),      -- 1 año
('Reporte', 'REP', 24, 1, GETDATE()),        -- 2 años
('Certificado', 'CERT', 120, 1, GETDATE()),  -- 10 años
('Acta', 'ACT', 60, 1, GETDATE()),           -- 5 años
('Memorando', 'MEM', 6, 1, GETDATE()),       -- 6 meses
('Propuesta', 'PROP', 12, 1, GETDATE()),     -- 1 año
('Presupuesto', 'PRES', 36, 1, GETDATE()),   -- 3 años
('Nota Interna', 'NOTE', 3, 1, GETDATE())    -- 3 meses
GO
```

---

## Script 6: Usuario Administrador de Prueba

```sql
-- CREAR USUARIO ADMINISTRADOR
DECLARE @idDeptAdmin INT = (SELECT TOP 1 Id FROM Departamento WHERE Nombre = 'Administración')

INSERT INTO Usuario (IdDepartamento, Nombre, ApellidoP, ApellidoM, Correo, Contrasena, Estatus, FechaCreacion)
VALUES 
(@idDeptAdmin, 'Admin', 'Sistema', NULL, 'admin@empresa.com', 'Admin123!', 1, GETDATE())

-- ASIGNAR ROL ADMINISTRADOR
DECLARE @idUsuarioAdmin INT = (SELECT Id FROM Usuario WHERE Correo = 'admin@empresa.com')
DECLARE @idRolAdmin INT = (SELECT Id FROM Rol WHERE Nombre = 'Administrador')

INSERT INTO UsuarioRol (IdUsuario, IdRol, FechaAsignacion, FechaCreacion, Estatus)
VALUES (@idUsuarioAdmin, @idRolAdmin, GETDATE(), GETDATE(), 1)

-- Mensajes confirmación
SELECT 'Usuario Admin creado:' as Resultado
SELECT * FROM Usuario WHERE Correo = 'admin@empresa.com'

SELECT 'Rol asignado:' as Resultado
SELECT ur.*, r.Nombre FROM UsuarioRol ur 
JOIN Rol r ON ur.IdRol = r.Id 
WHERE ur.IdUsuario = @idUsuarioAdmin

GO
```

---

## Script 7: Usuarios de Prueba por Departamento

```sql
-- USUARIOS DE PRUEBA
DECLARE @idDeptVentas INT = (SELECT Id FROM Departamento WHERE Nombre = 'Ventas')
DECLARE @idDeptIT INT = (SELECT Id FROM Departamento WHERE Nombre = 'Sistemas/IT')
DECLARE @idDeptRH INT = (SELECT Id FROM Departamento WHERE Nombre = 'Recursos Humanos')

INSERT INTO Usuario (IdDepartamento, Nombre, ApellidoP, ApellidoM, Correo, Contrasena, Estatus, FechaCreacion)
VALUES 
(@idDeptVentas, 'Carlos', 'García', 'López', 'carlos.garcia@empresa.com', 'CarlosVta123!', 1, GETDATE()),
(@idDeptVentas, 'María', 'Rodríguez', 'Martín', 'maria.rodriguez@empresa.com', 'MariaVta123!', 1, GETDATE()),
(@idDeptIT, 'Juan', 'Martínez', 'Sánchez', 'juan.martinez@empresa.com', 'JuanIT123!', 1, GETDATE()),
(@idDeptIT, 'Ana', 'González', 'Fernández', 'ana.gonzalez@empresa.com', 'AnaIT123!', 1, GETDATE()),
(@idDeptRH, 'Pedro', 'López', 'Jiménez', 'pedro.lopez@empresa.com', 'PedroRH123!', 1, GETDATE())

-- ASIGNAR ROLES A USUARIOS
DECLARE @idRolGerente INT = (SELECT Id FROM Rol WHERE Nombre = 'Gerente')
DECLARE @idRolEmpleado INT = (SELECT Id FROM Rol WHERE Nombre = 'Empleado')
DECLARE @idCarlos INT = (SELECT Id FROM Usuario WHERE Correo = 'carlos.garcia@empresa.com')
DECLARE @idMaria INT = (SELECT Id FROM Usuario WHERE Correo = 'maria.rodriguez@empresa.com')
DECLARE @idJuan INT = (SELECT Id FROM Usuario WHERE Correo = 'juan.martinez@empresa.com')
DECLARE @idAna INT = (SELECT Id FROM Usuario WHERE Correo = 'ana.gonzalez@empresa.com')
DECLARE @idPedro INT = (SELECT Id FROM Usuario WHERE Correo = 'pedro.lopez@empresa.com')

INSERT INTO UsuarioRol (IdUsuario, IdRol, FechaAsignacion, FechaCreacion, Estatus)
VALUES 
(@idCarlos, @idRolGerente, GETDATE(), GETDATE(), 1),  -- Gerente de Ventas
(@idMaria, @idRolEmpleado, GETDATE(), GETDATE(), 1),  -- Empleado Ventas
(@idJuan, @idRolGerente, GETDATE(), GETDATE(), 1),    -- Gerente IT
(@idAna, @idRolEmpleado, GETDATE(), GETDATE(), 1),    -- Empleado IT
(@idPedro, @idRolGerente, GETDATE(), GETDATE(), 1)    -- Gerente RH

-- CONFIRMACIÓN
SELECT 'Total Usuarios Creados:' as Resultado, COUNT(*) as Total FROM Usuario WHERE Estatus = 1
SELECT 'Total Usuarios con Rol:' as Resultado, COUNT(*) as Total FROM UsuarioRol WHERE Estatus = 1

GO
```

---

## Script 8: Verificación Completa

```sql
-- REPORTE FINAL
SELECT '=== ESTRUCTURA DEL SISTEMA ===' as Reporte

-- Departamentos
SELECT 'Departamentos Activos:' as Item, COUNT(*) as Total FROM Departamento WHERE Estatus = 1

-- Roles
SELECT 'Roles Disponibles:' as Item, COUNT(*) as Total FROM Rol WHERE Estatus = 1

-- Permisos
SELECT 'Permisos por Módulo:' as Item, Modulo, COUNT(*) as Total FROM Permiso WHERE Estatus = 1 GROUP BY Modulo

-- Usuarios
SELECT 'Usuarios Activos:' as Item, COUNT(*) as Total FROM Usuario WHERE Estatus = 1

-- Asignaciones
SELECT 'Usuarios con Rol:' as Item, COUNT(*) as Total FROM UsuarioRol WHERE Estatus = 1

-- Matriz Rol-Permiso
SELECT 'Permisos por Rol:' as Item
SELECT r.Nombre as Rol, COUNT(rp.IdPermiso) as TotalPermisos 
FROM Rol r 
LEFT JOIN RolPermiso rp ON r.Id = rp.IdRol AND rp.Estatus = 1
WHERE r.Estatus = 1
GROUP BY r.Nombre

-- Usuarios con sus roles
SELECT 'Usuarios y Roles:' as Item
SELECT u.Correo as Usuario, STRING_AGG(r.Nombre, ', ') as Roles
FROM Usuario u
LEFT JOIN UsuarioRol ur ON u.Id = ur.IdUsuario AND ur.Estatus = 1
LEFT JOIN Rol r ON ur.IdRol = r.Id
WHERE u.Estatus = 1
GROUP BY u.Correo

GO
```

---

## 🔄 Ejecutar Todo en Orden

1. **Script 1:** Departamentos
2. **Script 2:** Roles
3. **Script 3:** Permisos
4. **Script 4:** Asignación Permisos-Roles
5. **Script 5:** Tipos Documento
6. **Script 6:** Usuario Admin
7. **Script 7:** Usuarios Prueba
8. **Script 8:** Verificación

---

## 🔐 Credenciales de Prueba

```
Usuario: admin@empresa.com
Contraseña: Admin123!
Rol: Administrador
```

```
Usuario: carlos.garcia@empresa.com
Contraseña: CarlosVta123!
Rol: Gerente
Departamento: Ventas
```

```
Usuario: maria.rodriguez@empresa.com
Contraseña: MariaVta123!
Rol: Empleado
Departamento: Ventas
```

---

✅ **¡Base de datos lista con datos de prueba!**
