# 🗂️ ESTRUCTURA DE DATOS Y RELACIONES

## Diagrama de Tablas Gestionadas

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          GESTIÓN DEL SISTEMA                            │
└─────────────────────────────────────────────────────────────────────────┘

                              USUARIO
                    ┌─────────────────────────┐
                    │ • Id (PK)               │
                    │ • Nombre                │
                    │ • ApellidoP             │
                    │ • ApellidoM             │
                    │ • Correo                │
                    │ • Contrasena            │
                    │ • IdDepartamento (FK)   │◄──────────┐
                    │ • Estatus               │           │
                    │ • Campos Auditoria      │           │
                    └────────────┬────────────┘           │
                                 │                        │
                    ┌────────────┴──────────┐            │
                    │                       │            │
                    ▼ (N)            (1) ◄─┘            │
            ┌────────────────┐                      ┌───────────────┐
            │  UsuarioRol    │                      │  Departamento │
            ├────────────────┤          ┌──────────►├───────────────┤
            │ IdUsuario (FK) │          │           │ • Id (PK)     │
            │ IdRol (FK)     │─────┐    │           │ • Nombre      │
            │ FechaAsignacio │     │    │           │ • Abreviatura │
            │ Estatus        │     │    │           │ • Estatus     │
            └────────────────┘     │    │           └───────────────┘
                    (N)            │ (1)│
                                   │    │
            ┌──────────────────┐   │    │
            │      ROL         │   │    │
            ├──────────────────┤   │    │
            │ • Id (PK)        │◄──┘    │
            │ • Nombre         │        │
            │ • Descripcion    │        │
            │ • Estatus        │        │
            │ • Campos Auditoria       │
            └────────┬─────────┘        │
                     │ (1)              │
                     │                  │
            ┌────────┴──────┐           │
            │                │           │
        (N) ▼            (1) ▼           │
    ┌──────────────┐  ┌─────────────┐  │
    │  RolPermiso  │  │  PERMISO    │  │
    ├──────────────┤  ├─────────────┤  │
    │ IdRol (FK)   │  │ • Id (PK)   │  │
    │ IdPermiso(FK)│  │ • Codigo    │  │
    │ Estatus      │  │ • Descripción  │
    └──────────────┘  │ • Modulo    │  │
                      │ • Estatus   │  │
                      │ • Campos    │  │
                      │   Auditoria │  │
                      └─────────────┘  │
                                       │
                      ┌────────────────┘
                      │
                      ▼ (N)
            ┌──────────────────┐
            │ TIPODOCUMENTO    │
            ├──────────────────┤
            │ • Id (PK)        │
            │ • Nombre         │
            │ • Abreviatura    │
            │ • TiempoRetención│
            │ • Estatus        │
            │ • Campos Auditoria
            └──────────────────┘
```

---

## 📊 Relaciones Detalladas

### 1️⃣ **USUARIO ↔ DEPARTAMENTO** (N:1)
```
Muchos usuarios pertenecen a 1 departamento
├─ Usuario.IdDepartamento → Departamento.Id
└─ Obligatoria al crear usuario
```

### 2️⃣ **USUARIO ↔ ROL** (N:N través de UsuarioRol)
```
Un usuario puede tener múltiples roles
Un rol puede ser asignado a múltiples usuarios
├─ UsuarioRol.IdUsuario → Usuario.Id
├─ UsuarioRol.IdRol → Rol.Id
├─ UsuarioRol.FechaAsignacion (automática)
└─ UsuarioRol.Estatus (permite desactivar sin eliminar)
```

### 3️⃣ **ROL ↔ PERMISO** (N:N través de RolPermiso)
```
Un rol puede tener múltiples permisos
Un permiso puede ser asignado a múltiples roles
├─ RolPermiso.IdRol → Rol.Id
├─ RolPermiso.IdPermiso → Permiso.Id
└─ RolPermiso.Estatus (permite desactivar sin eliminar)
```

### 4️⃣ **PERMISO** (Estructura Modular)
```
Módulos disponibles:
├─ Documentos
│  ├─ VER_DOCUMENTOS
│  ├─ CREAR_DOCUMENTO
│  ├─ EDITAR_DOCUMENTO
│  └─ ELIMINAR_DOCUMENTO
├─ Usuarios
│  ├─ VER_USUARIOS
│  ├─ CREAR_USUARIO
│  ├─ EDITAR_USUARIO
│  └─ GESTIONAR_ROLES
├─ Administración
│  └─ GESTIONAR_ROLES
├─ Reportes
│  └─ GENERAR_REPORTES
└─ Configuración
   └─ CONFIGURAR_SISTEMA
```

---

## 🔄 Flujo de Datos en Autenticación

```
┌─────────────────┐
│  Usuario Login  │
└────────┬────────┘
         │
         ▼
┌──────────────────────────────┐
│ Buscar Usuario por Correo    │
│ + Estatus = true             │
└────────┬─────────────────────┘
         │
         ▼
┌──────────────────────────────┐
│ Cargar UsuarioRol            │
│ Incluir Rol relacionado      │
│ Filtrar Estatus = true       │
└────────┬─────────────────────┘
         │
         ▼
┌──────────────────────────────┐
│ Crear Claims:                │
│ • NameIdentifier = Id        │
│ • Name = Correo              │
│ • Role = Nombres de Roles    │
│   (si existen, sino "Usuario")│
└────────┬─────────────────────┘
         │
         ▼
┌──────────────────────────────┐
│ Sign In usuario              │
│ Crear Cookie de sesión       │
└────────┬─────────────────────┘
         │
         ▼
┌──────────────────────────────┐
│ Redirigir a Inicio           │
│ Usuario autenticado con roles│
└──────────────────────────────┘
```

---

## 🔐 Flujo de Control de Acceso

```
┌──────────────────────┐
│ Usuario Accede a URL │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────────────┐
│ ¿Existe [Authorize]?         │
└──────────┬──────────────┬────┘
           │ Sí           │ No
           ▼               ▼
      ¿Usuario          Permitir acceso
      logueado?         sin restricción
           │
       Sí  │  No
    ┌──────┴──────┐
    ▼             ▼
  Verificar    Redirigir a
  Roles        Login
    │          (/Auth/Login)
    ▼
 ¿Tiene roles
 requeridos?
    │
Sí  │  No
 ┌──┴──┐
 ▼     ▼
Permitir Redirigir a
acceso   Acceso Denegado
         (/Auth/AccesoDenegado)
```

---

## 📋 Campos Auditoria (Automáticos)

```
Todas las tablas principales incluyen:

┌─────────────────────────┬──────────────────────────────┐
│ Campo                   │ Propósito                    │
├─────────────────────────┼──────────────────────────────┤
│ IdUsuarioCreacion       │ Quién creó el registro       │
│ FechaCreacion           │ Cuándo se creó              │
│ IdUsuarioModificacion   │ Quién modificó por última vez│
│ FechaModificacion       │ Cuándo se modificó          │
│ IdUsuarioEliminacion    │ Quién lo "eliminó"          │
│ FechaEliminacion        │ Cuándo se "eliminó"         │
│ Estatus                 │ true = activo, false = inactivo
└─────────────────────────┴──────────────────────────────┘

Nota: No se eliminan físicamente, solo se marca como inactivo
```

---

## 🎯 Casos de Uso

### Caso 1: Crear un Usuario Administrativo
```
1. Crear Departamento (ej: "Sistemas")
2. Crear Rol (ej: "Administrador")
3. Crear Permisos necesarios para ese Rol
4. Asignar Permisos al Rol
5. Registrar Usuario:
   - Nombre, Apellidos, Correo
   - Seleccionar Departamento "Sistemas"
   - Asignar contrase ña temporal
6. Asignar Rol "Administrador" al Usuario
7. Usuario puede acceder al Panel Admin
```

### Caso 2: Crear un Usuario Normal
```
1. Registrar Usuario:
   - Nombre, Apellidos, Correo
   - Seleccionar Departamento (ej: "Ventas")
   - Asignar contraseña temporal
2. NO asignar roles específicos
3. Usuario tendrá rol "Usuario" por defecto
4. Puede ver documentos pero no modificar admin
```

### Caso 3: Cambiar Permisos de un Rol
```
1. Panel Admin → Roles
2. Ver rol actual
3. Clic en "Permisos"
4. Seleccionar nuevos permisos
5. Los permisos anteriores se desactivan
6. Los nuevos se activan
7. Afecta a TODOS los usuarios con ese rol
```

---

## 🗝️ Claves Primarias y Foráneas

```
┌─────────────────┬────────────┬──────────────────────┐
│ Tabla           │ PK         │ FK                   │
├─────────────────┼────────────┼──────────────────────┤
│ Departamento    │ Id         │ (Auditoria)          │
│ Usuario         │ Id         │ IdDepartamento       │
│ Rol             │ Id         │ (Auditoria)          │
│ Permiso         │ Id         │ (Auditoria)          │
│ TipoDocumento   │ Id         │ (Auditoria)          │
│ UsuarioRol      │ Id/PK comp │ IdUsuario, IdRol     │
│ RolPermiso      │ PK comp    │ IdRol, IdPermiso     │
└─────────────────┴────────────┴──────────────────────┘
```

---

## ✨ Ventajas del Diseño

✅ **Escalable:** Fácil agregar nuevos roles y permisos sin modificar código

✅ **Flexible:** Los permisos son específicos por módulo

✅ **Auditado:** Todos los cambios quedan registrados

✅ **Seguro:** Autorización basada en claims (RBAC)

✅ **Mantenible:** Baja lógica permite recuperar datos si es necesario

✅ **Modular:** Cada tabla puede ser gestionada independientemente
