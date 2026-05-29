<div align="center">

# 📄 SIGD Empresarial

### Sistema Integral de Gestión Documental

[![.NET](https://img.shields.io/badge/.NET_10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![PHP](https://img.shields.io/badge/PHP_8.2-777BB4?style=for-the-badge&logo=php&logoColor=white)](https://www.php.net/)
[![Node.js](https://img.shields.io/badge/Node.js_20-339933?style=for-the-badge&logo=nodedotjs&logoColor=white)](https://nodejs.org/)
[![Docker](https://img.shields.io/badge/Docker_Compose-2496ED?style=for-the-badge&logo=docker&logoColor=white)](https://docs.docker.com/compose/)
[![TypeScript](https://img.shields.io/badge/TypeScript-3178C6?style=for-the-badge&logo=typescript&logoColor=white)](https://www.typescriptlang.org/)

**Proyecto universitario · Ingeniería en Informática**

[🔗 Repositorio](https://github.com/Alan-Azeim-Rdz/SIGD_Empresarial) · [📖 Módulo Central](src/ModuloCentral/Gestion%20de%20Documentos/README.md) · [🔍 Módulo Búsqueda](src/ModuloBusqueda/README.md)

</div>

---

## 📋 Descripción

**SIGD Empresarial** es una plataforma de gestión documental empresarial desarrollada con arquitectura de microservicios. Permite a organizaciones controlar el ciclo de vida completo de sus documentos: desde su creación como borrador hasta su publicación como normativa vigente, con flujos de revisión y aprobación, búsqueda de texto completo y generación de reportes.

El sistema está compuesto por **tres módulos independientes** que se comunican entre sí a través de APIs REST y comparten estado mediante sus propias bases de datos especializadas, todos orquestados con Docker Compose.

---

## 🏗️ Diagrama de Arquitectura

```
┌─────────────────────────────────────────────────────────────────────┐
│                    DOCKER COMPOSE · sigd_network                     │
│                                                                       │
│   ┌─────────────────────┐        ┌──────────────────────┐           │
│   │   🗄️ SQL Server 2022  │        │   🐘 PostgreSQL 16    │           │
│   │   Puerto 1434         │        │   Puerto 5432         │           │
│   │   BD: SIGD_Central    │        │   BD: sigd_reportes   │           │
│   └──────────┬────────────┘        └──────────┬───────────┘           │
│              │ EF Core                         │ PDO                   │
│   ┌──────────▼────────────┐        ┌──────────▼───────────┐           │
│   │  🏢 MÓDULO CENTRAL     │        │  📊 MÓDULO REPORTES   │           │
│   │  ASP.NET Core 10 C#   │◄──────►│  PHP 8.2 + Apache    │           │
│   │  Puerto 5000           │  API   │  Puerto 8000          │           │
│   └──────────┬────────────┘  REST  └──────────────────────┘           │
│              │                                                         │
│              │ POST /indexar                                           │
│              │ GET  /buscar                                            │
│   ┌──────────▼────────────┐        ┌──────────────────────┐           │
│   │  🔍 MÓDULO BÚSQUEDA    │        │   🍃 MongoDB 7.0       │           │
│   │  Node.js + TypeScript │◄──────►│   Puerto 27017        │           │
│   │  Express 5 · Puerto 3000│       │   BD: sigd_busqueda   │           │
│   └───────────────────────┘        └──────────────────────┘           │
│                                                                       │
└─────────────────────────────────────────────────────────────────────┘

         ┌──────────────────────────────────────────┐
         │            🌐 USUARIO FINAL               │
         │  Central :5000 · Reportes :8000 · API :3000 │
         └──────────────────────────────────────────┘
```

### Flujo de integración entre módulos

| Origen | Destino | Endpoint | Propósito |
|--------|---------|----------|-----------|
| Módulo Central | Módulo Búsqueda | `POST /indexar` | Indexar documento al publicarlo |
| Módulo Central | Módulo Búsqueda | `GET /buscar?q=` | Búsqueda full-text desde la UI |
| Módulo Central | Módulo Reportes | `POST /api/sync` | Sincronizar metadatos de documentos |
| Módulo Reportes | Módulo Búsqueda | `GET /buscar?q=` | Portal de operarios con búsqueda |

---

## 🛠️ Tecnologías Utilizadas

### Backend

| Módulo | Lenguaje | Framework | Base de Datos | ORM / Driver |
|--------|----------|-----------|---------------|--------------|
| Central | C# | ASP.NET Core 10.0 MVC | SQL Server 2022 | Entity Framework Core 10 |
| Reportes | PHP 8.2 | Apache + PDO nativo | PostgreSQL 16 | PDO (pdo_pgsql) |
| Búsqueda | TypeScript | Express 5.2 | MongoDB 7.0 | Mongoose 9 |

### Infraestructura y herramientas

| Categoría | Tecnología | Versión |
|-----------|-----------|---------|
| Contenedores | Docker + Docker Compose | Compose 3.8 |
| Logging (Reportes) | Monolog | ^3.0 |
| Logging (Búsqueda) | Pino + Pino-Pretty | ^9.0 |
| Generación PDF | dompdf | ^3.1 |
| Documentación API | Swagger UI / OpenAPI 3 | — |
| Testing (Búsqueda) | Jest + ts-jest + Supertest | Jest 29 |
| Testing (Reportes) | PHPUnit | ^11.0 |
| Hot-reload (.NET) | `dotnet watch` | — |
| Hot-reload (Node) | ts-node-dev | ^2.0 |

---

## ✅ Requisitos Previos

Antes de levantar el proyecto asegúrate de tener instalado:

| Herramienta | Versión mínima | Verificar con |
|-------------|----------------|---------------|
| **Docker Desktop** | 24.x | `docker --version` |
| **Docker Compose** | 2.x (plugin) | `docker compose version` |
| **Git** | 2.x | `git --version` |

> **Nota:** No necesitas instalar .NET, PHP ni Node.js localmente. Todo corre dentro de contenedores Docker.

### Recursos mínimos de sistema

- 🐏 **RAM:** 4 GB disponibles (recomendado 8 GB)
- 💾 **Disco:** ~5 GB libres (imágenes Docker + datos)
- 🖥️ **OS:** Windows 10/11, macOS 12+, Ubuntu 20.04+

---

## 🚀 Instalación y Puesta en Marcha

### Paso 1 — Clonar el repositorio

```bash
git clone https://github.com/Alan-Azeim-Rdz/SIGD_Empresarial.git
cd SIGD_Empresarial
git checkout development
```

### Paso 2 — Configurar variables de entorno

```bash
# Copiar la plantilla de configuración
cp .env.example .env
```

Edita el archivo `.env` con tus credenciales:

```dotenv
# ── SQL Server (Módulo Central) ──────────────────────────────
SQL_DATABASE=SIGD_Central
SQL_SA_PASSWORD=TuPassword123!
APP_DB_USER=sa
APP_DB_PASSWORD=TuPassword123!

# ── PostgreSQL (Módulo Reportes) ─────────────────────────────
PG_USER=sigd_user
PG_PASSWORD=TuPasswordPostgres!
PG_DATABASE=sigd_reportes

# ── MongoDB (Módulo Búsqueda) ────────────────────────────────
MONGO_USERNAME=sigd_mongo
MONGO_PASSWORD=TuPasswordMongo!

# ── API Key compartida entre módulos ─────────────────────────
SYNC_API_KEY=una_clave_secreta_larga_y_aleatoria
```

> ⚠️ **Nunca** subas el archivo `.env` real al repositorio. Ya está en `.gitignore`.

### Paso 3 — Levantar el sistema

```bash
# Construir imágenes y levantar todos los servicios
docker compose up --build
```

La primera vez puede tardar **5–10 minutos** mientras Docker descarga las imágenes base y compila los módulos. Las ejecuciones posteriores son mucho más rápidas.

Para ejecutar en segundo plano:

```bash
docker compose up --build -d
```

### Paso 4 — Verificar que todos los servicios estén activos

```bash
docker compose ps
```

Deberías ver los 6 servicios con estado `running` / `healthy`:

```
NAME                   STATUS          PORTS
sigd_sqlserver         running         0.0.0.0:1434->1433/tcp
sigd_postgres          running         0.0.0.0:5432->5432/tcp
sigd_mongodb           running         0.0.0.0:27017->27017/tcp
sigd_central           running         0.0.0.0:5000->5000/tcp
sigd_reportes          running         0.0.0.0:8000->80/tcp
sigd_busqueda          running         0.0.0.0:3000->3000/tcp
```

### Paso 5 — Acceder al sistema

Una vez levantado, abre tu navegador:

| Módulo | URL | Descripción |
|--------|-----|-------------|
| 🏢 Central (principal) | http://localhost:5000 | Panel de administración y gestión documental |
| 📊 Reportes / Portal | http://localhost:8000 | Dashboard de reportes y portal de operarios |
| 🔍 Búsqueda (API) | http://localhost:3000 | API REST de búsqueda full-text |
| 📖 Swagger UI | http://localhost:3000/docs | Documentación interactiva de la API de búsqueda |

---

## 🔑 Credenciales Iniciales

> Estas credenciales son para el entorno de desarrollo y demos. **Cámbialas en producción.**

| Campo | Valor |
|-------|-------|
| **URL de acceso** | http://localhost:5000/login |
| **Correo** | `admin@sigd.local` |
| **Contraseña** | `Admin2026*` |
| **Rol** | Administrador del sistema |

El administrador tiene acceso completo para:
- ✅ Crear y gestionar usuarios, roles y permisos
- ✅ Configurar departamentos y empresas
- ✅ Gestionar el flujo de aprobación de documentos
- ✅ Ver bitácoras de acceso y auditoría

---

## 📁 Estructura del Repositorio

```
SIGD_Empresarial/
│
├── 📄 docker-compose.yml          # Orquestación principal (6 servicios)
├── 📄 docker-compose.debug.yml    # Configuración para debugging
├── 📄 .env.example                # Plantilla de variables de entorno
├── 📄 .gitignore
│
├── 📂 src/
│   │
│   ├── 📂 ModuloCentral/          # ASP.NET Core 10 · C# MVC
│   │   └── Gestion de Documentos/
│   │       ├── Controllers/       # Auth, Admin, Documento, Flujo, Búsqueda
│   │       ├── Models/            # EF Entities + DbContext (DirContext)
│   │       ├── Views/             # Razor Views (.cshtml)
│   │       ├── Services/          # ReportesIntegration, BusquedaIntegration, MongoGridFs
│   │       ├── Program.cs         # DI, autenticación, HttpClients
│   │       ├── appsettings.json
│   │       └── Dockerfile         # Multi-stage: dev (watch) → publish → final
│   │
│   ├── 📂 ModuloReportes/         # PHP 8.2 + Apache
│   │   ├── api/                   # sync.php · v1/dashboard.php · v1/portal.php
│   │   ├── config/                # Database.php · Logger.php
│   │   ├── controllers/           # Dashboard, Reporte, Sync
│   │   ├── models/                # Acuse.php
│   │   ├── views/                 # dashboard.php · portal_operario.php
│   │   ├── tests/                 # PHPUnit tests
│   │   ├── composer.json          # dompdf · monolog · phpunit
│   │   ├── index.php              # Punto de entrada
│   │   └── Dockerfile             # php:8.2-apache + pdo_pgsql + composer
│   │
│   └── 📂 ModuloBusqueda/         # Node.js 20 · TypeScript · Express 5
│       ├── __tests__/             # 35 tests · 100% cobertura de líneas
│       ├── index.ts               # App Express + modelo Mongoose + endpoints
│       ├── server.ts              # Punto de entrada
│       ├── package.json           # express · mongoose · pino · swagger
│       ├── tsconfig.json
│       ├── jest.config.ts
│       └── Dockerfile             # Multi-stage: dev (ts-node-dev) → builder → prod
│
├── 📂 scripts/
│   ├── sqlserver/                 # init_Central.sql · seed.sql · migrations
│   ├── postgres/                  # init_Reportes.sql · seed_demo.sql · migrations
│   ├── mongo/                     # init_busqueda.js · seed_demo.js
│   └── seeder/                    # seed_databases.py (seeder Python)
│
└── 📂 db/                         # Scripts SQL complementarios
```

---

## 🐳 Comandos Docker Útiles

### Gestión del sistema completo

```bash
# Levantar todos los servicios (reconstruyendo imágenes)
docker compose up --build

# Levantar en background
docker compose up -d

# Detener todos los servicios (conserva datos)
docker compose stop

# Detener y eliminar contenedores (conserva volúmenes/datos)
docker compose down

# Detener y eliminar TODO (incluyendo datos de BD) ⚠️
docker compose down -v
```

### Monitoreo y logs

```bash
# Ver estado de todos los servicios
docker compose ps

# Ver logs de todos los servicios en tiempo real
docker compose logs -f

# Ver logs de un servicio específico
docker compose logs -f sigd_central
docker compose logs -f sigd_reportes
docker compose logs -f sigd_busqueda

# Ver últimas 100 líneas de un servicio
docker compose logs --tail=100 sigd_central
```

### Acceder a contenedores

```bash
# Shell en el módulo central (.NET)
docker compose exec sigd_central bash

# Shell en el módulo de reportes (PHP)
docker compose exec sigd_reportes bash

# Shell en el módulo de búsqueda (Node.js)
docker compose exec sigd_busqueda sh

# Consola SQL Server
docker compose exec sigd_sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$SQL_SA_PASSWORD" -No

# Consola PostgreSQL
docker compose exec sigd_postgres psql -U $PG_USER -d sigd_reportes

# Consola MongoDB
docker compose exec sigd_mongodb mongosh -u $MONGO_USERNAME -p $MONGO_PASSWORD
```

### Mantenimiento

```bash
# Reconstruir solo un servicio sin reiniciar los demás
docker compose up --build sigd_central

# Reiniciar un servicio
docker compose restart sigd_busqueda

# Ver uso de recursos
docker stats

# Limpiar imágenes y caché de build no utilizadas
docker system prune --volumes
```

### Ejecutar tests

```bash
# Tests del Módulo Búsqueda (Jest · 35 tests)
docker compose exec sigd_busqueda npm test

# Tests con modo watch
docker compose exec sigd_busqueda npm run test:watch

# Tests del Módulo Reportes (PHPUnit)
docker compose exec sigd_reportes ./vendor/bin/phpunit tests/
```

---

## 🔄 Flujo de un Documento

El ciclo de vida completo de un documento en SIGD sigue estos estados:

```
  ┌──────────┐     Enviar a      ┌──────────────┐     Aprobar     ┌──────────┐
  │          │    revisión       │              │  ─────────────► │          │
  │ BORRADOR │ ──────────────► │   REVISIÓN   │                  │ APROBADO │
  │  (Draft) │                  │  (In Review) │  ◄───────────── │          │
  └──────────┘                  └──────────────┘   Solicitar      └────┬─────┘
       ▲                               │             cambios           │
       │                               │ Rechazar                      │ Publicar
       │                               ▼                               │
       │                         ┌──────────┐                         ▼
       └─────────────────────────│ RECHAZADO│              ┌──────────────────┐
              Editar y           └──────────┘              │    VIGENTE       │
              reenviar                                      │  (Normativa      │
                                                           │   publicada)     │
                                                           └────────┬─────────┘
                                                                    │
                                                           Nueva versión
                                                                    │
                                                                    ▼
                                                           ┌──────────────────┐
                                                           │    OBSOLETO      │
                                                           │  (Reemplazado    │
                                                           │   por v. nueva)  │
                                                           └──────────────────┘
```

### Descripción de estados

| Estado | Descripción | Quién puede actuar |
|--------|-------------|-------------------|
| 📝 **Borrador** | Documento en creación/edición | Autor del documento |
| 🔎 **Revisión** | Enviado para revisión técnica | Revisor asignado |
| ✅ **Aprobado** | Revisado y listo para publicar | Aprobador / Admin |
| 🚫 **Rechazado** | Devuelto con observaciones | — (vuelve al autor) |
| 📢 **Vigente** | Publicado como normativa activa | Admin (para publicar) |
| 📦 **Obsoleto** | Reemplazado por una versión más nueva | Sistema automático |

> Cuando un documento pasa a **Vigente**, el Módulo Central notifica automáticamente al Módulo de Búsqueda (`POST /indexar`) para indexarlo y hacerlo buscable, y al Módulo de Reportes (`POST /api/sync`) para actualizar estadísticas.

---

## 🌐 URLs de Acceso por Módulo

### 🏢 Módulo Central — `http://localhost:5000`

| Ruta | Descripción |
|------|-------------|
| `/login` | Inicio de sesión |
| `/home` | Dashboard principal |
| `/admin` | Panel de administración (usuarios, roles, permisos) |
| `/documento` | Gestión de documentos |
| `/flujo` | Flujos de aprobación |
| `/busqueda` | Búsqueda de documentos (integrada con Módulo Búsqueda) |

### 📊 Módulo Reportes — `http://localhost:8000`

| Ruta | Descripción |
|------|-------------|
| `/` | Dashboard de reportes y estadísticas |
| `/portal` | Portal público de operarios |
| `/api/v1/dashboard` | API JSON de métricas del dashboard |
| `/api/v1/portal` | API JSON del portal de operarios |
| `/api/sync` | Endpoint de sincronización (llamado por .NET) |

### 🔍 Módulo Búsqueda — `http://localhost:3000`

| Ruta | Método | Descripción |
|------|--------|-------------|
| `/buscar?q={texto}` | `GET` | Búsqueda full-text (máx. 100 caracteres) |
| `/indexar` | `POST` | Indexar un nuevo documento |
| `/documento/:id` | `GET` | Obtener metadatos por ID o código |
| `/docs` | `GET` | Swagger UI interactivo |
| `/docs.json` | `GET` | Especificación OpenAPI 3.0 |

---

## 🎓 Información Académica

| Campo | Detalle |
|-------|---------|
| **Materia** | Proyecto Final de Ingeniería en Informática |
| **Tipo** | Sistema web empresarial con arquitectura de microservicios |
| **Repositorio** | [github.com/Alan-Azeim-Rdz/SIGD_Empresarial](https://github.com/Alan-Azeim-Rdz/SIGD_Empresarial) |
| **Rama principal** | `development` |

### Objetivos del proyecto

- ✅ Implementar arquitectura de microservicios con tres módulos independientes
- ✅ Integrar tres motores de base de datos distintos (SQL Server, PostgreSQL, MongoDB)
- ✅ Implementar flujo de gestión documental completo con roles y permisos
- ✅ Lograr comunicación REST entre servicios con API Keys
- ✅ Containerizar la aplicación completa con Docker Compose
- ✅ Implementar búsqueda full-text sobre documentos indexados
- ✅ Generar reportes en PDF con dompdf
- ✅ Cobertura de tests unitarios ≥ 80% en módulo de búsqueda

---

<div align="center">



</div>
