# 🚀 REFERENCIA RÁPIDA - URLs Y COMANDOS

## 🌐 URLs Principales

### Autenticación
```
GET  /Auth/Login              → Página de login
POST /Auth/Login              → Procesar login
GET  /Auth/Registro           → Formulario registro
POST /Auth/Registro           → Procesar registro
GET  /Auth/Logout             → Cerrar sesión
GET  /Auth/AccesoDenegado     → Página acceso denegado
GET  /Auth/Usuarios           → Listado de usuarios (ADMIN)
```

### Panel Administrativo
```
GET  /Admin/Index             → Dashboard principal
```

### Gestión de Departamentos
```
GET  /Admin/Departamentos     → Listado
GET  /Admin/CrearDepartamento → Formulario crear
POST /Admin/CrearDepartamento → Procesar crear
GET  /Admin/EditarDepartamento/{id}     → Formulario editar
POST /Admin/EditarDepartamento          → Procesar editar
POST /Admin/EliminarDepartamento/{id}   → Eliminar (baja lógica)
```

### Gestión de Roles
```
GET  /Admin/Roles             → Listado
GET  /Admin/CrearRol          → Formulario crear
POST /Admin/CrearRol          → Procesar crear
GET  /Admin/EditarRol/{id}    → Formulario editar
POST /Admin/EditarRol         → Procesar editar
POST /Admin/EliminarRol/{id}  → Eliminar (baja lógica)
GET  /Admin/AsignarPermisosRol/{id}    → Asignar permisos
POST /Admin/AsignarPermisosRol         → Procesar asignación
```

### Gestión de Permisos
```
GET  /Admin/Permisos          → Listado
GET  /Admin/CrearPermiso      → Formulario crear
POST /Admin/CrearPermiso      → Procesar crear
GET  /Admin/EditarPermiso/{id}        → Formulario editar
POST /Admin/EditarPermiso             → Procesar editar
POST /Admin/EliminarPermiso/{id}      → Eliminar (baja lógica)
```

### Gestión de Tipos de Documento
```
GET  /Admin/TiposDocumento    → Listado
GET  /Admin/CrearTipoDocumento        → Formulario crear
POST /Admin/CrearTipoDocumento        → Procesar crear
GET  /Admin/EditarTipoDocumento/{id}  → Formulario editar
POST /Admin/EditarTipoDocumento       → Procesar editar
POST /Admin/EliminarTipoDocumento/{id} → Eliminar (baja lógica)
```

### Asignación de Roles a Usuarios
```
GET  /Admin/AsignarRolesUsuario/{id}   → Interfaz asignación
POST /Admin/AsignarRolesUsuario        → Procesar asignación
```

---

## 💻 Comandos de Desarrollo

### Iniciar la Aplicación
```bash
dotnet run
# Acceder a: https://localhost:5001/Auth/Login
```

### Compilar el Proyecto
```bash
dotnet build
```

### Restaurar Paquetes
```bash
dotnet restore
```

### Limpiar Construcción
```bash
dotnet clean
```

### Ver Información del Proyecto
```bash
dotnet --version
```

---

## 🗄️ Comandos SQL Útiles

### Ver Estructura de Usuarios
```sql
SELECT * FROM Usuario WHERE Estatus = 1
```

### Ver Roles de Usuario
```sql
SELECT u.Correo, r.Nombre
FROM Usuario u
LEFT JOIN UsuarioRol ur ON u.Id = ur.IdUsuario
LEFT JOIN Rol r ON ur.IdRol = r.Id
WHERE u.Estatus = 1 AND ur.Estatus = 1
```

### Ver Permisos de Rol
```sql
SELECT r.Nombre, p.Codigo, p.Descripcion
FROM Rol r
LEFT JOIN RolPermiso rp ON r.Id = rp.IdRol
LEFT JOIN Permiso p ON rp.IdPermiso = p.Id
WHERE r.Estatus = 1 AND rp.Estatus = 1
```

### Departamentos Activos
```sql
SELECT * FROM Departamento WHERE Estatus = 1
```

### Roles Activos
```sql
SELECT * FROM Rol WHERE Estatus = 1
```

### Permisos Activos
```sql
SELECT * FROM Permiso WHERE Estatus = 1 ORDER BY Modulo
```

### Tipos de Documento Activos
```sql
SELECT * FROM TipoDocumento WHERE Estatus = 1
```

---

## 🔑 Credenciales de Prueba

### Administrador
```
Correo: admin@empresa.com
Contraseña: Admin123!
Rol: Administrador
```

### Gerente Ventas
```
Correo: carlos.garcia@empresa.com
Contraseña: CarlosVta123!
Rol: Gerente
Departamento: Ventas
```

### Empleado Ventas
```
Correo: maria.rodriguez@empresa.com
Contraseña: MariaVta123!
Rol: Empleado
Departamento: Ventas
```

### Gerente IT
```
Correo: juan.martinez@empresa.com
Contraseña: JuanIT123!
Rol: Gerente
Departamento: Sistemas/IT
```

### Empleado IT
```
Correo: ana.gonzalez@empresa.com
Contraseña: AnaIT123!
Rol: Empleado
Departamento: Sistemas/IT
```

---

## 🎨 Colores del Tema

```css
--jewel-amethyst: [Color primario]
```

Se usa en:
- Headers de secciones
- Botones principales
- Bordes de tarjetas
- Iconos destacados

---

## 📋 Validaciones Comunes

### Campo Requerido
```
Marca: * (asterisco rojo)
Error: "Este campo es requerido"
```

### Código de Permiso
```
Formato: MAYÚSCULAS_SIN_ESPACIOS
Ejemplo: VER_DOCUMENTOS, CREAR_USUARIO
Error: "Código inválido"
```

### Correo Electrónico
```
Formato: usuario@empresa.com
Validación: Único en la tabla Usuario
Error: "Este correo ya está registrado"
```

### Departamento
```
Obligatorio: Sí
Validación: Debe estar activo
Error: "El departamento seleccionado no es válido"
```

### Nombre
```
Validación: Único en su tabla
Error: "Este nombre ya existe"
```

---

## 🔐 Claims de Autenticación

En `User.FindFirst()`:

```csharp
// Obtener ID del usuario
ClaimTypes.NameIdentifier

// Obtener correo del usuario
ClaimTypes.Name

// Obtener roles (puede ser múltiple)
ClaimTypes.Role
```

Ejemplo:
```csharp
var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
var email = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value)
```

---

## 📁 Estructura de Carpetas

```
Gestion de Documentos/
├── Controllers/
│   ├── AdminController.cs (NUEVO)
│   ├── AuthController.cs (MODIFICADO)
│   └── HomeController.cs
├── Models/
│   ├── Usuario.cs
│   ├── Rol.cs
│   ├── Permiso.cs
│   ├── Departamento.cs
│   ├── TipoDocumento.cs
│   ├── UsuarioRol.cs
│   ├── RolPermiso.cs
│   ├── DirContext.cs
│   └── ...
├── Views/
│   ├── Admin/ (NUEVA CARPETA)
│   │   ├── Index.cshtml
│   │   ├── Departamentos.cshtml
│   │   ├── CrearDepartamento.cshtml
│   │   ├── EditarDepartamento.cshtml
│   │   ├── Roles.cshtml
│   │   ├── CrearRol.cshtml
│   │   ├── EditarRol.cshtml
│   │   ├── Permisos.cshtml
│   │   ├── CrearPermiso.cshtml
│   │   ├── EditarPermiso.cshtml
│   │   ├── TiposDocumento.cshtml
│   │   ├── CrearTipoDocumento.cshtml
│   │   ├── EditarTipoDocumento.cshtml
│   │   ├── AsignarRolesUsuario.cshtml
│   │   └── AsignarPermisosRol.cshtml
│   ├── Auth/
│   │   ├── Login.cshtml
│   │   ├── Registro.cshtml (MODIFICADO)
│   │   ├── Usuarios.cshtml (NUEVO)
│   │   ├── AccesoDenegado.cshtml
│   │   └── ...
│   ├── Home/
│   ├── Shared/
│   ├── _ViewImports.cshtml (MODIFICADO)
│   ├── _ViewStart.cshtml
│   └── ...
├── wwwroot/
├── appsettings.json
├── Program.cs
└── ...
```

---

## 🔗 Rutas de Navegación Recomendadas

### Para Administrador (Primer Login)
```
1. /Auth/Login
2. /Admin/Index (Dashboard)
3. /Admin/Departamentos (Crear estructura)
4. /Admin/Roles (Crear roles)
5. /Admin/Permisos (Crear permisos)
6. /Auth/Registro (Registrar usuarios)
7. /Auth/Usuarios (Asignar roles)
```

### Para Usuario Normal
```
1. /Auth/Login
2. / (Dashboard)
3. [Opciones según roles]
```

---

## 💡 Tips Útiles

### Crear Departamento Rápido
1. Panel Admin → Departamentos → Nuevo Departamento
2. Llenar: Nombre, Abreviatura
3. Guardar

### Asignar Rol a Usuario Rápido
1. Auth → Usuarios
2. Buscar usuario
3. Clic en "Roles"
4. Marcar roles
5. Guardar

### Cambiar Permisos de Todo un Rol
1. Admin → Roles
2. Seleccionar rol
3. Clic en "Permisos"
4. Modificar
5. Guardar
6. ¡Todos los usuarios con ese rol se actualizan!

### Ver Estructura Completa
```sql
SELECT u.Correo, d.Nombre as Departamento, r.Nombre as Rol, p.Codigo as Permiso
FROM Usuario u
LEFT JOIN Departamento d ON u.IdDepartamento = d.Id
LEFT JOIN UsuarioRol ur ON u.Id = ur.IdUsuario AND ur.Estatus = 1
LEFT JOIN Rol r ON ur.IdRol = r.Id
LEFT JOIN RolPermiso rp ON r.Id = rp.IdRol AND rp.Estatus = 1
LEFT JOIN Permiso p ON rp.IdPermiso = p.Id
WHERE u.Estatus = 1
ORDER BY u.Correo
```

---

## 🆘 Troubleshooting Rápido

| Problema | Solución |
|----------|----------|
| "No autorizado" | Asignar rol Administrador |
| "Departamento no válido" | Crear departamento activo |
| "Correo ya registrado" | Usar correo diferente |
| "Roles no se actualizan" | Logout y login nuevamente |
| "Acceso Denegado" | Verificar roles del usuario |

---

## 📞 Contactos y Recursos

### Documentación Incluida
- `INDICE_DOCUMENTACION.md` - Índice general
- `GUIA_RAPIDA.md` - Primeros pasos
- `DOCUMENTACION_PANEL_ADMIN.md` - Guía completa
- `DIAGRAMA_RELACIONES.md` - Arquitectura
- `SCRIPTS_SQL_INICIALES.md` - Datos de prueba
- `RESUMEN_EJECUTIVO.md` - Visión general

### Archivos de Código
- `AdminController.cs` - Lógica principal
- `AuthController.cs` - Autenticación
- `Vistas Admin` - Interfaz de usuario

---

## ✨ Resumen de Capacidades

✅ Crear usuarios con todos los datos  
✅ Gestionar departamentos  
✅ Gestionar roles  
✅ Gestionar permisos por módulo  
✅ Gestionar tipos de documento  
✅ Asignar roles a usuarios  
✅ Asignar permisos a roles  
✅ Autenticación dinámica con roles desde BD  
✅ Auditoría automática  
✅ Baja lógica sin eliminación física  

---

🎉 **¡TODO LISTO PARA COMENZAR!**

Elige tu documento según tu necesidad:
- Empezar rápido: **GUIA_RAPIDA.md**
- Entender todo: **DOCUMENTACION_PANEL_ADMIN.md**
- Ver estructura: **DIAGRAMA_RELACIONES.md**
- Datos de prueba: **SCRIPTS_SQL_INICIALES.md**
