# ⚡ GUÍA RÁPIDA - PRIMEROS PASOS

## 🚀 Iniciar el Sistema por Primera Vez

### Paso 1: Ejecutar la Aplicación
```bash
dotnet run
```

Acceder a: `https://localhost:5001/Auth/Login`

---

## 📌 Crear Estructura Base de Datos

Ejecutar en SQL Server Management Studio:

### 1️⃣ Crear Rol Administrador

```sql
-- Si el rol no existe, crearlo
IF NOT EXISTS (SELECT 1 FROM Rol WHERE Nombre = 'Administrador')
BEGIN
    INSERT INTO Rol (Nombre, Descripcion, Estatus, FechaCreacion)
    VALUES ('Administrador', 'Rol con acceso total al sistema', 1, GETDATE())
END
GO
```

### 2️⃣ Asignar Rol al Usuario Actual

```sql
-- Obtener ID del usuario administrador
DECLARE @idUsuarioAdmin INT = (SELECT TOP 1 Id FROM Usuario WHERE Correo = 'tuusuario@empresa.com')
DECLARE @idRolAdmin INT = (SELECT Id FROM Rol WHERE Nombre = 'Administrador')

-- Si no existe la asignación, crear
IF NOT EXISTS (SELECT 1 FROM UsuarioRol WHERE IdUsuario = @idUsuarioAdmin AND IdRol = @idRolAdmin)
BEGIN
    INSERT INTO UsuarioRol (IdUsuario, IdRol, FechaAsignacion, FechaCreacion, Estatus)
    VALUES (@idUsuarioAdmin, @idRolAdmin, GETDATE(), GETDATE(), 1)
END
GO
```

### 3️⃣ Crear Departamento Inicial

```sql
-- Crear departamento si no existe
IF NOT EXISTS (SELECT 1 FROM Departamento WHERE Nombre = 'Administración')
BEGIN
    INSERT INTO Departamento (Nombre, Abreviatura, Estatus, FechaCreacion)
    VALUES ('Administración', 'ADM', 1, GETDATE())
END
GO
```

---

## 🔧 Iniciar Sesión

1. **Ir a:** `https://localhost:5001/Auth/Login`
2. **Usuario:** Tu correo electrónico
3. **Contraseña:** La que creaste en la BD
4. **Resultado:** Acceso al Panel Admin

---

## 📊 Crear Estructura de Organización

Una vez logueado como administrador:

### Paso 1: Departamentos
1. Panel Admin → **Departamentos**
2. Clic: **Nuevo Departamento**
3. Crear:
   - Ventas (VTA)
   - Sistemas (SIS)
   - Recursos Humanos (RH)
   - Finanzas (FIN)

### Paso 2: Roles
1. Panel Admin → **Roles**
2. Clic: **Nuevo Rol**
3. Crear:
   - **Gerente** - Acceso total dentro de su depto
   - **Supervisor** - Acceso moderado
   - **Empleado** - Acceso básico

### Paso 3: Permisos
1. Panel Admin → **Permisos**
2. Clic: **Nuevo Permiso**
3. Crear permisos por módulo:

**Módulo: Documentos**
```
VER_DOCUMENTOS - Ver todos los documentos
CREAR_DOCUMENTO - Crear nuevos documentos
EDITAR_DOCUMENTO - Editar documentos
ELIMINAR_DOCUMENTO - Eliminar documentos
```

**Módulo: Usuarios**
```
VER_USUARIOS - Ver lista de usuarios
GESTIONAR_USUARIOS - Crear y editar usuarios
```

**Módulo: Administración**
```
GESTIONAR_ROLES - Administrar roles y permisos
GESTIONAR_DEPARTAMENTOS - Crear departamentos
```

### Paso 4: Asignar Permisos a Roles
1. Panel Admin → **Roles**
2. Seleccionar un rol
3. Clic: **Permisos**
4. Seleccionar permisos necesarios

**Ejemplo - Rol Gerente:**
- ✅ VER_DOCUMENTOS
- ✅ CREAR_DOCUMENTO
- ✅ EDITAR_DOCUMENTO
- ✅ VER_USUARIOS

**Ejemplo - Rol Supervisor:**
- ✅ VER_DOCUMENTOS
- ✅ CREAR_DOCUMENTO
- ✅ VER_USUARIOS

**Ejemplo - Rol Empleado:**
- ✅ VER_DOCUMENTOS

### Paso 5: Crear Usuarios
1. Panel Admin → **Panel Principal** → **Nuevo Usuario**
   O
   **Auth** → **Registro**
2. Completar datos:
   - Nombre: [Nombre del usuario]
   - Apellido Paterno: [Apellido]
   - Apellido Materno: [Opcional]
   - Correo: usuario@empresa.com
   - Departamento: [Seleccionar]
   - Contraseña: [Contraseña temporal]
3. Guardar

### Paso 6: Asignar Roles a Usuarios
1. Auth → **Usuarios**
2. Seleccionar usuario
3. Clic: **Roles**
4. Marcar roles a asignar:
   - ☑ Gerente
   - ☑ Supervisor
   - ☑ Empleado
5. Guardar

### Paso 7: Tipos de Documento (Opcional)
1. Panel Admin → **Tipos de Documento**
2. Clic: **Nuevo Tipo**
3. Crear:
   - Contrato (CT, 24 meses)
   - Factura (FC, 6 meses)
   - Solicitud (SOL, 3 meses)
   - Reporte (REP, 12 meses)

---

## ✅ Verificar Configuración

1. **Acceder al Panel Admin:**
   ```
   https://localhost:5001/Admin/Index
   ```

2. **Debe mostrar:**
   - ✅ Usuarios Activos: 1
   - ✅ Roles Disponibles: 3+
   - ✅ Departamentos: 4+
   - ✅ Tipos de Documento: 4+

3. **Crear nuevo usuario de prueba:**
   - Auth → Registro
   - Llenar todos los campos
   - Asignar departamento
   - Guardar

4. **Verificar en listado:**
   - Auth → Usuarios
   - Debe aparecer el nuevo usuario
   - Clic en "Roles" para asignarle

---

## 🔄 Ciclo Diario

### Crear Nuevo Empleado
```
1. Panel Admin → Crear Departamento (si no existe)
2. Panel Admin → Roles (asegurarse que exista el rol)
3. Auth → Registro → Crear usuario
4. Auth → Usuarios → Asignar roles
5. Listo: Empleado puede loguearse
```

### Cambiar Permisos de un Rol
```
1. Panel Admin → Roles
2. Seleccionar rol
3. Clic: Permisos
4. Modificar permisos
5. Guardar
6. Todos los usuarios con ese rol se actualizar automáticamente
```

### Desactivar Usuario
```
1. Auth → Usuarios
2. Seleccionar usuario (Click en Eliminar)
3. Confirmación
4. Usuario con Estatus = false
5. No puede loguearse
```

---

## 🆘 Solución de Problemas

### ❌ "No autorizado para acceder a Panel Admin"
**Solución:** El usuario no tiene rol "Administrador"
```sql
-- Asignar rol
SELECT * FROM Usuario WHERE Correo = 'tu@correo.com'  -- Obtener Id
SELECT * FROM Rol WHERE Nombre = 'Administrador'  -- Obtener Id Rol

INSERT INTO UsuarioRol (IdUsuario, IdRol, FechaCreacion, Estatus)
VALUES (@idUsuario, @idRol, GETDATE(), 1)
```

### ❌ "El departamento seleccionado no es válido"
**Solución:** No existe departamento activo
```sql
-- Ver departamentos activos
SELECT * FROM Departamento WHERE Estatus = 1

-- Crear nuevo
INSERT INTO Departamento (Nombre, Abreviatura, Estatus, FechaCreacion)
VALUES ('Nuevo Depto', 'ND', 1, GETDATE())
```

### ❌ "Este correo ya está registrado"
**Solución:** El correo ya existe en la BD
- Usar diferente correo electrónico
- O verificar si el usuario ya existe

### ❌ "No hay roles registrados"
**Solución:** Crear roles primero
1. Panel Admin → Roles → Nuevo Rol
2. Crear al menos un rol
3. Reintentar crear usuario

---

## 📱 Rutas de Acceso Rápido

| Acción | URL |
|--------|-----|
| Login | `/Auth/Login` |
| Registro Usuario | `/Auth/Registro` |
| Listado Usuarios | `/Auth/Usuarios` |
| Panel Admin | `/Admin/Index` |
| Gestionar Departamentos | `/Admin/Departamentos` |
| Gestionar Roles | `/Admin/Roles` |
| Gestionar Permisos | `/Admin/Permisos` |
| Gestionar Tipos Doc | `/Admin/TiposDocumento` |

---

## 🎯 Caso de Uso Completo

### Escenario: Empresa con 3 departamentos

**Paso 1: Crear Estructura**
```sql
-- Departamentos
INSERT INTO Departamento VALUES ('Ventas', 'VTA', 1, GETDATE(), null, null, null, null, null)
INSERT INTO Departamento VALUES ('IT', 'IT', 1, GETDATE(), null, null, null, null, null)
INSERT INTO Departamento VALUES ('HR', 'HR', 1, GETDATE(), null, null, null, null, null)

-- Roles
INSERT INTO Rol VALUES ('Gerente', 'Gestor de depto', 1, GETDATE(), null, null, null, null, null)
INSERT INTO Rol VALUES ('Empleado', 'Usuario normal', 1, GETDATE(), null, null, null, null, null)
```

**Paso 2: Por UI (Panel Admin)**
1. Crear Permisos para cada módulo
2. Asignar Permisos a cada Rol
3. Registrar usuarios por departamento
4. Asignar Roles a usuarios

**Paso 3: Verificación**
```sql
-- Ver estructura completa
SELECT u.Correo, u.Nombre, d.Nombre as Departamento, r.Nombre as Rol
FROM Usuario u
LEFT JOIN Departamento d ON u.IdDepartamento = d.Id
LEFT JOIN UsuarioRol ur ON u.Id = ur.IdUsuario
LEFT JOIN Rol r ON ur.IdRol = r.Id
WHERE u.Estatus = 1 AND (ur.Estatus = 1 OR ur.Estatus IS NULL)
```

---

✅ **¡Sistema Listo para Usar!**
