# Módulo de Reportes — SIGD Empresarial

> Microservicio de consulta pública, generación de reportes PDF y sincronización de documentos, construido con PHP 8.2 y PostgreSQL.

---

## 📋 Tabla de Contenidos

- [Descripción](#-descripción)
- [Tecnologías](#-tecnologías)
- [Estructura del Proyecto](#-estructura-del-proyecto)
- [Requisitos Previos](#-requisitos-previos)
- [Instalación y Ejecución](#-instalación-y-ejecución)
- [Variables de Entorno](#-variables-de-entorno)
- [Interfaz Web](#-interfaz-web)
- [Base de Datos](#-base-de-datos)
- [Seguridad](#-seguridad)
- [Docker](#-docker)
- [Pruebas Rápidas](#-pruebas-rápidas)
- [Problemas Conocidos y Solución](#-problemas-conocidos-y-solución)
- [Integración con Otros Módulos](#-integración-con-otros-módulos)
- [Contribución](#-contribución)
- [Licencia](#-licencia)

---

## 🎯 Descripción

El Módulo de Reportes es el microservicio de **consulta pública y trazabilidad documental** del SIGD Empresarial. Cumple tres funciones diferenciadas dentro del sistema:

**1. Portal de consulta pública (`index.php`):** Expone una interfaz web donde cualquier usuario puede buscar documentos vigentes, visualizar su estado y descargar una ficha individual en PDF. No requiere autenticación y está pensado para consultas rápidas desde planta o administración.

**2. Sincronización con el Módulo Central (`api/sync.php`):** Recibe los metadatos de documentos aprobados/actualizados desde el módulo .NET y los persiste en PostgreSQL mediante operaciones UPSERT idempotentes. Esta copia local permite que los reportes y estadísticas funcionen de forma autónoma, sin depender de SQL Server.

**3. Dashboard y reportes de cumplimiento:** Un conjunto de controladores y vistas proporcionan KPIs en tiempo real (documentos activos, departamentos, últimas publicaciones) y funciones de auditoría como el registro de acuses de lectura, necesarios para demostrar conformidad ante auditorías ISO.

---

## 🛠️ Tecnologías

| Tecnología       | Versión    | Uso                                              |
|------------------|------------|--------------------------------------------------|
| PHP              | 8.2        | Lenguaje del servidor                            |
| Apache           | 2.4        | Servidor web (imagen `php:8.2-apache`)           |
| PostgreSQL       | 16-alpine  | Base de datos relacional                         |
| PDO / pdo_pgsql  | nativa     | Capa de acceso a datos con prepared statements   |
| dompdf/dompdf    | ^3.1       | Generación de reportes y fichas en PDF           |
| Composer         | latest     | Gestor de dependencias PHP                       |
| Bootstrap        | 5.3.2      | Estilos del portal de operarios (CDN)            |

---

## 📂 Estructura del Proyecto

```
src/ModuloReportes/
├── index.php                    # Portal público (búsqueda, descarga y reporte PDF)
├── api/
│   └── sync.php                 # Endpoint de sincronización con ModuloCentral
├── config/
│   └── Database.php             # Clase de conexión PDO a PostgreSQL
├── controllers/
│   ├── DashboardController.php  # KPIs, gráficas y documentos recientes
│   ├── ReporteController.php    # Registro de acuses y reporte de cumplimiento
│   └── SyncController.php       # Lógica UPSERT de sincronización de documentos
├── models/
│   └── Acuse.php                # Modelo para registro e historial de acuses de lectura
├── views/
│   ├── dashboard.php            # Vista de estadísticas con gráficas (Chart.js)
│   └── portal_operario.php      # Portal dark-mode para operarios (Bootstrap 5)
├── vendor/                      # Dependencias instaladas por Composer
├── composer.json                # Declaración de dependencias PHP
└── Dockerfile                   # PHP 8.2 + Apache + extensión pdo_pgsql
```

---

## ⚙️ Requisitos Previos

- **Docker Desktop** 4.x o superior (para la opción recomendada)
- **Git** (para clonar el repositorio)
- **Editor recomendado:** VS Code con las extensiones PHP Intelephense y Docker
- **Sin Docker:** PHP 8.2+ con extensiones `pdo`, `pdo_pgsql`, Composer, y una instancia de PostgreSQL 16 accesible

---

## 🚀 Instalación y Ejecución

### Opción 1: Con Docker Compose (recomendado)

Desde la raíz del repositorio:

```bash
# Copiar y completar las variables de entorno
cp .env.example .env   # editar PG_USER, PG_PASSWORD, PG_DATABASE, SYNC_API_KEY

# Levantar solo este módulo y su base de datos
docker compose up postgres modulo_reportes

# O levantar todo el stack
docker compose up
```

El servicio quedará disponible en `http://localhost:8000`.

> En modo desarrollo (`docker-compose.yml` activo), el directorio `./src/ModuloReportes` se monta como volumen en `/var/www/html`, por lo que los cambios en el código PHP se reflejan de inmediato sin reconstruir la imagen.

### Opción 2: Sin Docker (desarrollo local)

Requiere PHP 8.2+ con las extensiones `pdo` y `pdo_pgsql` habilitadas, y una instancia de PostgreSQL 16.

```bash
cd src/ModuloReportes

# Instalar dependencias PHP
composer install

# Configurar variables de entorno necesarias
export DB_HOST=localhost
export DB_PORT=5432
export DB_NAME=sigd_reportes
export DB_USER=sigd_user
export DB_PASS=tu_contraseña
export SYNC_API_KEY=sigd_sync_secret_2026

# Levantar el servidor de desarrollo de PHP
php -S localhost:8000
```

Antes de arrancar, ejecutar el script de inicialización de la base de datos:

```bash
psql -U sigd_user -d sigd_reportes -f ../../scripts/postgres/init_Reportes.sql
```

---

## 🔧 Variables de Entorno

| Variable       | Descripción                                           | Valor de Ejemplo          | Requerida |
|----------------|-------------------------------------------------------|---------------------------|-----------|
| `DB_HOST`      | Hostname del servidor PostgreSQL                      | `postgres`                | Sí        |
| `DB_PORT`      | Puerto de PostgreSQL                                  | `5432`                    | Sí        |
| `DB_USER`      | Usuario de la base de datos                           | `sigd_user`               | Sí        |
| `DB_PASS`      | Contraseña del usuario de la BD                       | `mi_contraseña`           | Sí        |
| `DB_NAME`      | Nombre de la base de datos                            | `sigd_reportes`           | Sí        |
| `SYNC_API_KEY` | Clave API compartida con el Módulo Central            | `sigd_sync_secret_2026`   | Sí        |

Las variables `DB_*` provienen de las variables `PG_*` definidas en el `.env` raíz del proyecto. `SYNC_API_KEY` debe coincidir exactamente con `ReportesModule__SyncApiKey` del Módulo Central.

---

## 🌐 Interfaz Web

### Portal de Consulta Pública — `index.php`

**Ruta:** `http://localhost:8000/`  
**Parámetro de acción:** `?accion=` (valores: `buscar` [defecto], `reporte`, `descargar`)

Pantalla principal de acceso libre. Permite a cualquier usuario buscar en los documentos vigentes registrados en PostgreSQL.

| Ruta URL                            | Funcionalidad                                                    |
|-------------------------------------|------------------------------------------------------------------|
| `/?accion=buscar` (o simplemente `/`) | Listado de todos los documentos vigentes con buscador por título |
| `/?accion=buscar&q=calidad`           | Filtrar documentos cuyo título contenga "calidad" (ILIKE)        |
| `/?accion=reporte`                    | Genera y descarga un PDF con todos los documentos vigentes        |
| `/?accion=descargar&id=1`             | Genera y descarga la ficha PDF del documento con `id_documento=1` |

**Ejemplo visual (listado):**
```
┌─────────────────────────────────────────────────────────────┐
│ SIGD Empresarial                         [Generar Reporte PDF] │
├─────────────────────────────────────────────────────────────┤
│ Buscar Documentos Vigentes                                    │
│ [ Escribe el nombre del documento... ] [Buscar]               │
├──────┬──────────────────────────┬──────────┬───────┬─────────┤
│ ID   │ Título                   │ Código   │ Versión │ Estado │
├──────┼──────────────────────────┼──────────┼─────────┼────────┤
│  1   │ Manual de Calidad        │ MAN-001  │  v2     │ Aprobado │
│  2   │ Procedimiento de Pruebas │ PROC-005 │  v1     │ Aprobado │
└──────┴──────────────────────────┴──────────┴─────────┴────────┘
```

### Portal de Operarios — `views/portal_operario.php`

Interfaz dark-mode construida con Bootstrap 5. Diseñada para operarios en planta. Permite buscar normativas usando el Módulo de Búsqueda (Node.js/Puerto 3000) y registrar acuses de lectura.

| Ruta URL                  | Funcionalidad                                              |
|---------------------------|------------------------------------------------------------|
| `?action=portal`          | Tabla de documentos con búsqueda en tiempo real            |
| `?action=dashboard`       | Dashboard de estadísticas con gráficas (Chart.js)          |

Las URLs de los servicios (Módulo Central: 5000, Node.js: 3000) son configurables desde un modal de "Configuración de Conexiones", con persistencia en `localStorage` del navegador.

### Dashboard de Estadísticas — `views/dashboard.php`

Vista que consume los endpoints JSON del `DashboardController`:

| Endpoint (action=)         | Descripción                                             |
|----------------------------|---------------------------------------------------------|
| `api_kpis`                 | Totales: documentos activos, departamentos, last update |
| `api_docs_por_depto`       | Top 10 departamentos por cantidad de documentos         |
| `api_evolucion`            | Publicaciones mensuales en los últimos 12 meses         |
| `api_recientes`            | Últimos 10 documentos publicados                        |

### Endpoint de Registro de Acuse — `ReporteController`

| Acción                          | Método | Descripción                                          |
|---------------------------------|--------|------------------------------------------------------|
| `?action=registrar_acuse`        | POST   | Registra la firma de lectura de un operario          |
| `?action=clima_cumplimiento&id_depto=X` | GET | % de cumplimiento por departamento (llama a `sp_reporte_cumplimiento_depto`) |

---

## 🗄️ Base de Datos

**Motor:** PostgreSQL 16-alpine  
**Nombre de la BD:** definido por la variable `PG_DATABASE` (ej. `sigd_reportes`)

### Tablas principales

| Tabla                  | Descripción                                                              |
|------------------------|--------------------------------------------------------------------------|
| `departamento`         | Catálogo de departamentos de la empresa                                  |
| `usuario`              | Usuarios del sistema (espejo del Módulo Central)                         |
| `tipo_documento`       | Catálogo de tipos: Procedimiento, Manual, Formato, Instructivo           |
| `documento_vigente`    | Documentos publicados sincronizados desde el Módulo Central              |
| `acuse_lectura`        | Registro de firmas de lectura de operarios (trazabilidad ISO)            |
| `reporte_descarga`     | Registro de descargas de documentos                                      |
| `estadistica_documento`| Contador de vistas por documento (actualizado por trigger automático)    |
| `bitacora_sync`        | Log de cada intento de sincronización (exitoso o fallido)                |

### Campos clave de `documento_vigente`

| Columna                  | Tipo          | Descripción                                      |
|--------------------------|---------------|--------------------------------------------------|
| `id_documento`           | INT (PK)      | ID del documento (mismo que en SQL Server)       |
| `codigo_interno`         | VARCHAR(50)   | Código único interno                             |
| `titulo`                 | VARCHAR(255)  | Título del documento                             |
| `id_tipo`                | INT (FK)      | Tipo de documento                                |
| `id_departamento`        | INT (FK)      | Departamento dueño del documento                 |
| `version_actual`         | INT           | Versión en curso                                 |
| `fecha_publicacion`      | TIMESTAMP     | Fecha de publicación                             |
| `ruta_archivo_descarga`  | VARCHAR(500)  | Ruta del archivo en el servidor de origen        |
| `hash_verificacion`      | VARCHAR(255)  | Hash del archivo para verificar integridad       |
| `estatus`                | BOOLEAN       | Borrado lógico: `true` = vigente                 |

### Inicialización

El script `scripts/postgres/init_Reportes.sql` se ejecuta automáticamente la primera vez que el contenedor de PostgreSQL arranca (vía `docker-entrypoint-initdb.d`). Crea:

- Todas las tablas con campos de auditoría estándar
- Índices de búsqueda y restricciones de llaves foráneas
- Triggers para actualizar `fecha_modificacion` automáticamente en UPDATE
- Trigger para actualizar `estadistica_documento` en cada nuevo acuse de lectura
- Funciones almacenadas: `sp_reporte_cumplimiento_depto`, `fn_crear_usuario`, `fn_validar_login` (hasheo SHA-256 vía `pgcrypto`)
- Datos semilla: departamento de Administración, usuario Super Admin (`admin@sigd.local`), 4 tipos de documento base

---

## 🔐 Seguridad

**CORS restringido:** El endpoint `api/sync.php` solo acepta el header `Origin` de tres orígenes explícitos: `http://modulo_central` (nombre del servicio Docker), `http://localhost:5000` y `http://127.0.0.1:5000`. Cualquier otro origen no recibe el header `Access-Control-Allow-Origin`.

**Validación de API Key:** Cada petición a `api/sync.php` debe incluir el header `X-Api-Key` con el valor de la variable de entorno `SYNC_API_KEY`. La comparación usa `hash_equals()` para evitar timing attacks. Las peticiones sin clave válida reciben `401 Unauthorized` y el procesamiento se detiene inmediatamente.

**Prepared statements en PDO:** Todas las consultas SQL que incorporan datos externos usan `bindValue()` con tipado explícito (`PDO::PARAM_INT`, `PDO::PARAM_STR`). La capa de acceso a datos tiene `PDO::ATTR_EMULATE_PREPARES => false`, lo que fuerza el uso de preparaciones nativas de PostgreSQL y elimina el riesgo de inyección SQL.

---

## 🐳 Docker

El `Dockerfile` usa una imagen base **`php:8.2-apache`** (single-stage):

```
FROM php:8.2-apache
  → Instala libpq-dev + extensiones pdo, pdo_pgsql, pgsql
  → Habilita mod_rewrite de Apache
  → Copia Composer desde su imagen oficial
  → Copia el código del proyecto a /var/www/html
  → Ejecuta composer install --no-dev --optimize-autoloader (si existe composer.json)
  → Ajusta permisos: chown www-data /var/www/html
```

**Puerto expuesto internamente:** 80 (Apache)  
**Puerto mapeado en docker-compose:** `8000:80` (acceso: `http://localhost:8000`)

**Volúmenes (modo desarrollo):**
- `./src/ModuloReportes:/var/www/html` — código local montado; los cambios en PHP se reflejan sin rebuild

**Servidor web:** Apache 2.4 con `mod_rewrite` habilitado (necesario si se implementa enrutamiento URL limpio en el futuro).

---

## 🧪 Pruebas Rápidas

Tras ejecutar `docker compose up postgres modulo_reportes`:

**1. Verificar que el portal responde:**

```bash
curl -s -o /dev/null -w "%{http_code}" http://localhost:8000/
# Esperar: 200
```

**2. Verificar el health-check del endpoint de sincronización:**

```bash
curl -X POST http://localhost:8000/api/sync.php?action=ping \
  -H "X-Api-Key: sigd_sync_secret_2026"
# Esperar: {"status":"ok","modulo":"ModuloReportes","timestamp":"..."}
```

**3. Verificar que la API Key es obligatoria:**

```bash
curl -X POST http://localhost:8000/api/sync.php?action=ping
# Esperar: {"status":"error","message":"Acceso denegado: clave de API inválida o ausente."}
```

**4. Sincronizar un documento de prueba:**

```bash
curl -X POST "http://localhost:8000/api/sync.php?action=sincronizar" \
  -H "X-Api-Key: sigd_sync_secret_2026" \
  -H "Content-Type: application/json" \
  -d '{
    "id_documento": 1,
    "codigo_interno": "MAN-001",
    "titulo": "Manual de Calidad",
    "id_tipo": 2,
    "id_departamento": 1,
    "version_actual": 1,
    "fecha_publicacion": "2026-05-22T10:00:00",
    "ruta_archivo_descarga": "/archivos/MAN-001.pdf",
    "id_usuario_creacion": 1
  }'
# Esperar: {"status":"success","message":"Documento sincronizado correctamente.","id":1}
```

**5. Verificar que el documento aparece en el portal:**

Abrir en el navegador: `http://localhost:8000/` — debe aparecer "Manual de Calidad" en la tabla.

---

## 🐛 Problemas Conocidos y Solución

**Error de conexión a la base de datos al abrir el portal:**

```
Fallo crítico en la conexión a PostgreSQL: ...
```

El contenedor de PostgreSQL puede no estar listo aún (arranca más lento que PHP/Apache).

Solución: esperar 10–15 segundos y recargar la página, o revisar `docker logs sigd_postgres`. Verificar que `PG_USER`, `PG_PASSWORD` y `PG_DATABASE` en el `.env` sean correctas y coincidan con las variables `DB_*` inyectadas al contenedor de PHP.

---

**Error 401 al sincronizar desde el Módulo Central:**

```json
{"status":"error","message":"Acceso denegado: clave de API inválida o ausente."}
```

La variable `SYNC_API_KEY` en el contenedor de PHP y la variable `ReportesModule__SyncApiKey` en el contenedor de .NET no coinciden.

Solución: asegurarse de que ambas apuntan al mismo valor en el archivo `.env` de la raíz. El valor por defecto en ambos módulos es `sigd_sync_secret_2026` (solo para desarrollo).

---

**Puerto 8000 ya ocupado:**

```
Error starting userland proxy: listen tcp4 0.0.0.0:8000: bind: address already in use
```

Solución: identificar el proceso con `netstat -ano | findstr :8000` (Windows) y detenerlo, o cambiar el mapeo en `docker-compose.yml` a `"8080:80"`.

---

## 🤝 Integración con Otros Módulos

```
┌─────────────────────────┐
│  ModuloCentral (.NET)   │
│  Puerto 5000            │
│  SQL Server             │
└────────────┬────────────┘
             │  POST /api/sync.php?action=sincronizar
             │  Header: X-Api-Key: ${SYNC_API_KEY}
             ▼
┌─────────────────────────────────────────────────────────────┐
│                    ModuloReportes                           │
│  PHP 8.2 + Apache + PostgreSQL                              │
│  Puerto 8000                                                │
│                                                             │
│  index.php       → Consulta pública, descarga PDF           │
│  api/sync.php    → Recibe documentos del Módulo Central      │
│  DashboardCtrl   → KPIs y gráficas para vistas internas      │
│  ReporteCtrl     → Acuses de lectura, cumplimiento ISO       │
└─────────────────────────────────────────────────────────────┘
             ▲
             │  Registrar acuse de lectura
             │  POST /index.php?action=registrar_acuse
             │
┌────────────┴────────────┐         GET /buscar?q=...
│  Portal Operario        │ ──────────────────────────►  ModuloBusqueda
│  (portal_operario.php)  │                              Node.js / Puerto 3000
└─────────────────────────┘
```

- **ModuloCentral → ModuloReportes:** Cada vez que se aprueba o actualiza un documento, ModuloCentral llama a `POST /api/sync.php` con los metadatos completos. La comunicación es server-to-server dentro de `sigd_network`.
- **Portal Operario → ModuloBusqueda:** La vista `portal_operario.php` consulta el microservicio Node.js para búsquedas full-text en tiempo real.
- **Portal Operario → ModuloReportes:** Tras encontrar un documento, el operario firma su lectura enviando `POST /index.php?action=registrar_acuse` a este módulo.
- **ModuloBusqueda:** No tiene dependencia directa de este módulo.

---

## 👥 Contribución

1. Crear una rama a partir de `development`:
   ```bash
   git checkout -b feature/reportes-mi-mejora
   ```
2. Realizar los cambios en `src/ModuloReportes/`.
3. Verificar que el portal responde y que la sincronización funciona (ver [Pruebas Rápidas](#-pruebas-rápidas)).
4. Confirmar los cambios con un mensaje descriptivo:
   ```bash
   git commit -m "feat(reportes): descripción del cambio"
   ```
5. Abrir un Pull Request hacia la rama `development` en GitHub.

---

## 📄 Licencia

Proyecto académico — Ingeniería en Informática

---

> 🌍 English version available on request.
