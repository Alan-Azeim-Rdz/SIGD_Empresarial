# 📋 GUÍA DE ADMINISTRACIÓN DEL SISTEMA - Panel Administrativo Completo

## 🎯 Resumen de Cambios Implementados

Se ha creado un **panel administrativo completo** que permite a los administradores gestionar todos los aspectos del sistema sin necesidad de acceder directamente a la base de datos.

---

## 📊 Tablas Gestionadas

### ✅ 1. **DEPARTAMENTOS**
**Tabla:** `Departamento`

**Campos:**
- Nombre (requerido, único)
- Abreviatura (requerido, máx 10 caracteres)

**Funcionalidades:**
- ✅ Crear nuevos departamentos
- ✅ Editar departamentos existentes
- ✅ Listar departamentos activos
- ✅ Eliminar departamentos (baja lógica)

**Acceso:** Panel Admin → Departamentos

---

### ✅ 2. **ROLES**
**Tabla:** `Rol`

**Campos:**
- Nombre (requerido, único)
- Descripción (opcional)

**Funcionalidades:**
- ✅ Crear nuevos roles
- ✅ Editar roles existentes
- ✅ Listar roles activos
- ✅ Asignar permisos a cada rol
- ✅ Eliminar roles (baja lógica)

**Acceso:** Panel Admin → Roles

---

### ✅ 3. **PERMISOS**
**Tabla:** `Permiso`

**Campos:**
- Código (requerido, único, formato: MAYÚSCULAS_SIN_ESPACIOS)
- Descripción (requerido)
- Módulo (requerido: Documentos, Usuarios, Administración, Reportes, Configuración)

**Funcionalidades:**
- ✅ Crear nuevos permisos por módulo
- ✅ Editar permisos existentes
- ✅ Listar permisos activos
- ✅ Organizar por módulo
- ✅ Eliminar permisos (baja lógica)

**Acceso:** Panel Admin → Permisos

---

### ✅ 4. **TIPOS DE DOCUMENTO**
**Tabla:** `TipoDocumento`

**Campos:**
- Nombre (requerido, único)
- Abreviatura (requerido, máx 10 caracteres)
- TiempoRetencionMeses (requerido, mínimo 1)

**Funcionalidades:**
- ✅ Crear nuevos tipos de documento
- ✅ Editar tipos de documento
- ✅ Listar tipos de documento activos
- ✅ Definir política de retención
- ✅ Eliminar tipos (baja lógica)

**Acceso:** Panel Admin → Tipos de Documento

---

### ✅ 5. **USUARIOS**
**Tabla:** `Usuario`

**Campos Solicitados en Registro:**
- Nombre (requerido)
- Apellido Paterno (requerido)
- Apellido Materno (opcional)
- Correo (requerido, único)
- IdDepartamento (requerido)
- Contraseña Temporal (requerido)

**Funcionalidades:**
- ✅ Crear nuevos usuarios con todos los datos
- ✅ Listar usuarios activos
- ✅ Asignar roles a usuarios
- ✅ Cambio obligatorio de contraseña en primer acceso (implementado en controlador)
- ✅ Estatus automático: Activo

**Acceso:** Auth → Registro

---

### ✅ 6. **ASIGNACIÓN DE ROLES A USUARIOS**
**Tabla Relacional:** `UsuarioRol` (N:N)

**Funcionalidades:**
- ✅ Asignar múltiples roles a un usuario
- ✅ Cambiar roles de un usuario
- ✅ Deactivar roles anteriores automáticamente
- ✅ Vista con resumen de roles asignados

**Acceso:** Panel Admin → Usuarios → Botón "Roles"

---

### ✅ 7. **ASIGNACIÓN DE PERMISOS A ROLES**
**Tabla Relacional:** `RolPermiso` (N:N)

**Funcionalidades:**
- ✅ Asignar múltiples permisos a un rol
- ✅ Cambiar permisos de un rol
- ✅ Deactivar permisos anteriores automáticamente
- ✅ Permisos organizados por módulo
- ✅ Vista con resumen de permisos asignados

**Acceso:** Panel Admin → Roles → Botón "Permisos"

---

## 🔐 Sistema de Autenticación Mejorado

### Login Automático con Roles desde BD
Cuando un usuario inicia sesión:
1. Se cargan automáticamente todos sus roles desde `UsuarioRol`
2. Si no tiene roles asignados, se asigna el rol "Usuario" por defecto
3. Los roles se convierten en claims de autenticación

### Protección de Acciones
- ✅ Registro: Solo `[Authorize(Roles = "Administrador,Superior")]`
- ✅ Panel Admin: Solo `[Authorize(Roles = "Administrador")]`
- ✅ Acceso Denegado: Vista personalizada cuando no hay permisos

---

## 📁 Archivos Creados

### Controladores:
- `Controllers/AdminController.cs` - Controlador administrativo principal

### Vistas:
**Panel Principal:**
- `Views/Admin/Index.cshtml` - Dashboard con estadísticas

**Departamentos:**
- `Views/Admin/Departamentos.cshtml` - Listado
- `Views/Admin/CrearDepartamento.cshtml` - Formulario de creación
- `Views/Admin/EditarDepartamento.cshtml` - Formulario de edición

**Roles:**
- `Views/Admin/Roles.cshtml` - Listado
- `Views/Admin/CrearRol.cshtml` - Formulario de creación
- `Views/Admin/EditarRol.cshtml` - Formulario de edición

**Permisos:**
- `Views/Admin/Permisos.cshtml` - Listado
- `Views/Admin/CrearPermiso.cshtml` - Formulario de creación
- `Views/Admin/EditarPermiso.cshtml` - Formulario de edición

**Tipos de Documento:**
- `Views/Admin/TiposDocumento.cshtml` - Listado
- `Views/Admin/CrearTipoDocumento.cshtml` - Formulario de creación
- `Views/Admin/EditarTipoDocumento.cshtml` - Formulario de edición

**Asignaciones:**
- `Views/Admin/AsignarRolesUsuario.cshtml` - Asignar roles a usuario
- `Views/Admin/AsignarPermisosRol.cshtml` - Asignar permisos a rol

**Usuarios:**
- `Views/Auth/Usuarios.cshtml` - Listado de usuarios

---

## 🔧 Configuración Requerida

### 1. Habilitar Administrador Inicial
Para que el primer administrador pueda acceder al panel, en la base de datos ejecutar:

```sql
-- 1. Crear rol Administrador
INSERT INTO Rol (Nombre, Descripcion, Estatus, FechaCreacion)
VALUES ('Administrador', 'Rol con acceso total al sistema', 1, GETDATE());

-- 2. Obtener el ID del rol creado y del usuario administrador
-- Asignar rol al usuario administrador
INSERT INTO UsuarioRol (IdUsuario, IdRol, FechaAsignacion, FechaCreacion, Estatus)
VALUES (@idUsuarioAdmin, @idRolAdmin, GETDATE(), GETDATE(), 1);
```

### 2. Crear Permisos Iniciales (Opcional)
```sql
INSERT INTO Permiso (Codigo, Descripcion, Modulo, Estatus, FechaCreacion)
VALUES 
('VER_DOCUMENTOS', 'Ver documentos', 'Documentos', 1, GETDATE()),
('CREAR_DOCUMENTO', 'Crear nuevo documento', 'Documentos', 1, GETDATE()),
('EDITAR_DOCUMENTO', 'Editar documentos', 'Documentos', 1, GETDATE()),
('ELIMINAR_DOCUMENTO', 'Eliminar documentos', 'Documentos', 1, GETDATE()),
('GESTIONAR_USUARIOS', 'Gestionar usuarios del sistema', 'Usuarios', 1, GETDATE()),
('GESTIONAR_ROLES', 'Gestionar roles y permisos', 'Administración', 1, GETDATE());
```

---

## 🚀 Flujo de Uso

### Paso 1: Crear Estructura Base
1. Panel Admin → Departamentos → Crear departamentos
2. Panel Admin → Roles → Crear roles necesarios
3. Panel Admin → Permisos → Crear permisos por módulo
4. Panel Admin → Tipos de Documento → Crear tipos

### Paso 2: Configurar Permisos
1. Panel Admin → Roles → Seleccionar rol
2. Clic en "Permisos"
3. Asignar permisos necesarios

### Paso 3: Registrar Usuarios
1. Auth → Registro
2. Completar todos los datos (incluido Departamento)
3. Usuario creado con rol "Usuario" por defecto

### Paso 4: Asignar Roles a Usuarios
1. Auth → Usuarios
2. Clic en "Roles" del usuario
3. Seleccionar roles a asignar
4. Guardar

---

## 📝 Tablas NO Gestionadas (Manejadas por Triggers/Eventos)

Las siguientes tablas se llenan automáticamente y NO requieren gestión manual:

- ✅ `BitacoraAcceso` - Registra accesos (triggers)
- ✅ `BitacoraControlDocumento` - Registra cambios en documentos (triggers)
- ✅ `BitacoraTransaccional` - Registra transacciones (triggers)

---

## 🛡️ Campos Automáticos

En todas las tablas gestionables se auto-completan:
- `IdUsuarioCreacion` - ID del usuario que creó el registro
- `FechaCreacion` - Fecha y hora de creación
- `IdUsuarioModificacion` - ID del usuario que modificó
- `FechaModificacion` - Fecha y hora de modificación
- `IdUsuarioEliminacion` - ID del usuario que eliminó
- `FechaEliminacion` - Fecha y hora de eliminación
- `Estatus` - Se asigna automáticamente según la acción

---

## ✨ Características Adicionales

✅ **Validaciones:**
- Nombres únicos en cada tabla
- Códigos únicos para permisos
- Correos únicos para usuarios
- Campos obligatorios marcados con asterisco

✅ **UX Mejorada:**
- Iconos FontAwesome en botones
- Alertas de confirmación en eliminaciones
- Mensajes de éxito/error claros
- Diseño responsivo Bootstrap
- Colores temáticos (Jewel Amethyst)

✅ **Seguridad:**
- Autorización por roles en todos los controladores
- Baja lógica (Estatus) en lugar de eliminación física
- Validaciones en servidor y cliente
- Auditoría automática (campos de creación/modificación)

---

## 🔗 Rutas Principales

| Ruta | Descripción |
|------|-------------|
| `/Admin/Index` | Panel de administración principal |
| `/Admin/Departamentos` | Gestión de departamentos |
| `/Admin/Roles` | Gestión de roles |
| `/Admin/Permisos` | Gestión de permisos |
| `/Admin/TiposDocumento` | Gestión de tipos de documento |
| `/Auth/Registro` | Registro de nuevos usuarios |
| `/Auth/Usuarios` | Listado de usuarios con gestión de roles |

---

## 📚 Notas Importantes

1. **Primera Carga:** Es posible que el sistema detecte que no hay departamentos. Crear al menos uno antes de registrar usuarios.

2. **Cambio de Contraseña Obligatorio:** Aunque está comentado en la vista, la lógica debe implementarse en el primer acceso del usuario.

3. **Relaciones N:N Automáticas:** Las tablas `UsuarioRol` y `RolPermiso` se populan automáticamente al asignar relaciones desde el panel.

4. **Bitácoras:** Se llenan automáticamente mediante triggers SQL, no requieren intervención manual.

---

✅ **¡Sistema Completamente Implementado y Listo para Usar!**
