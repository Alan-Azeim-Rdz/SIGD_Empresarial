# Módulo de Búsqueda — SIGD Empresarial

> Microservicio de indexación y búsqueda full-text de documentos sobre MongoDB, construido con Node.js y TypeScript.

---

## 📋 Tabla de Contenidos

- [Descripción](#-descripción)
- [Tecnologías](#-tecnologías)
- [Estructura del Proyecto](#-estructura-del-proyecto)
- [Requisitos Previos](#-requisitos-previos)
- [Instalación y Ejecución](#-instalación-y-ejecución)
- [Variables de Entorno](#-variables-de-entorno)
- [API / Endpoints](#-api--endpoints)
- [Documentación Interactiva (Swagger)](#-documentación-interactiva-openApiswagger)
- [Base de Datos](#-base-de-datos)
- [Seguridad](#-seguridad)
- [Logs Estructurados](#-logs-estructurados)
- [Docker](#-docker)
- [Tests](#-tests)
- [Pruebas Rápidas](#-pruebas-rápidas)
- [Problemas Conocidos y Solución](#-problemas-conocidos-y-solución)
- [Integración con Otros Módulos](#-integración-con-otros-módulos)
- [Contribución](#-contribución)
- [Licencia](#-licencia)

---

## 🎯 Descripción

El Módulo de Búsqueda es el microservicio encargado de la **indexación y recuperación de metadatos de documentos** dentro del SIGD Empresarial. Cuando el Módulo Central (.NET) aprueba y publica un documento normativo, lo registra aquí vía `POST /indexar`. A partir de ese momento, cualquier cliente —el portal de operarios o el dashboard— puede localizar ese documento en tiempo real mediante `GET /buscar`.

A diferencia del Módulo Central (que gestiona el ciclo de vida documental en SQL Server) o del Módulo de Reportes (que genera estadísticas en PostgreSQL), este módulo se especializa exclusivamente en **velocidad de búsqueda**: almacena un subconjunto liviano de metadatos en MongoDB y responde consultas de texto libre sobre título, tags y contenido extraído.

Su diseño es deliberadamente simple: `index.ts` expone tres endpoints Express, un modelo Mongoose y la lógica de sanitización necesaria para operar de forma segura en producción. El punto de entrada real al servidor es `server.ts`, que centraliza la conexión a MongoDB y el `app.listen()`.

---

## 🛠️ Tecnologías

| Tecnología          | Versión    | Uso                                              |
|---------------------|------------|--------------------------------------------------|
| Node.js             | 20 LTS     | Runtime de JavaScript del servidor               |
| TypeScript          | ^6.0.3     | Tipado estático sobre Node.js                    |
| Express             | ^5.2.1     | Framework HTTP (versión 5, async por defecto)    |
| Mongoose            | ^9.6.2     | ODM para MongoDB                                 |
| pino                | ^9.0.0     | Logger estructurado JSON de alto rendimiento     |
| pino-pretty         | ^11.0.0    | Formato coloreado de logs en desarrollo          |
| swagger-ui-express  | ^5.0.0     | Interfaz Swagger UI interactiva en `/docs`       |
| swagger-jsdoc       | ^6.2.8     | Generación de spec OpenAPI desde anotaciones     |
| ts-node-dev         | ^2.0.0     | Recarga automática en desarrollo                 |
| ts-node             | ^10.9.2    | Ejecución directa de TypeScript                  |
| Jest                | ^29.7.0    | Framework de tests unitarios                     |
| ts-jest             | ^29.1.0    | Transformador TypeScript para Jest               |
| supertest           | ^7.0.0     | Tests de endpoints HTTP sin servidor real        |
| MongoDB             | 7.0        | Base de datos documental (imagen Docker oficial) |

---

## 📂 Estructura del Proyecto

```
src/ModuloBusqueda/
├── index.ts              # App Express, modelo Mongoose y los 3 endpoints (exporta app, logger, Metadato, escapeRegex)
├── server.ts             # Punto de entrada real: conecta a MongoDB y llama app.listen()
├── package.json          # Dependencias y scripts npm
├── tsconfig.json         # Configuración TypeScript (NodeNext, modo strict)
├── jest.config.ts        # Configuración Jest: ts-jest, cobertura 80%+, setupFiles
├── Dockerfile            # Imagen multi-stage: base → development → builder → production
├── __tests__/            # Suites de tests unitarios (no requieren MongoDB)
│   ├── setup.ts          #   Variables de entorno para silenciar pino en tests
│   ├── helpers.ts        #   Fixtures y payloads de prueba reutilizables
│   ├── escapeRegex.test.ts
│   ├── indexar.test.ts
│   ├── buscar.test.ts
│   └── documento.test.ts
├── coverage/             # Reporte de cobertura generado por Jest (no versionar)
├── dist/                 # Salida del compilador tsc (no versionar)
└── node_modules/         # Dependencias instaladas (no versionar)
```

---

## ⚙️ Requisitos Previos

- **Docker Desktop** 4.x o superior (para la opción recomendada)
- **Git** (para clonar el repositorio)
- **Editor recomendado:** VS Code con las extensiones ESLint y Prettier
- **Sin Docker:** Node.js 20 LTS y una instancia de MongoDB 7.0 accesible

---

## 🚀 Instalación y Ejecución

### Opción 1: Con Docker Compose (recomendado)

Desde la raíz del repositorio:

```bash
# Copiar y completar las variables de entorno
cp .env.example .env   # editar MONGO_USERNAME, MONGO_PASSWORD

# Levantar solo este módulo y su base de datos
docker compose up mongodb modulo_busqueda

# O levantar todo el stack
docker compose up
```

El servicio quedará disponible en `http://localhost:3000`.

> El modo activo en `docker-compose.yml` es **development**: monta el código local como volumen y usa `ts-node-dev` para recarga automática al guardar.

### Opción 2: Sin Docker (desarrollo local)

Requiere una instancia de MongoDB 7 corriendo localmente o en red.

```bash
cd src/ModuloBusqueda

# Instalar dependencias
npm install

# Configurar variables de entorno necesarias
export MONGO_URI="mongodb://localhost:27017/sigd_busqueda"

# Modo desarrollo con recarga automática
npm run dev

# O compilar y ejecutar como producción
npm run build
npm start
```

---

## 🔧 Variables de Entorno

| Variable      | Descripción                                    | Valor de Ejemplo                                                              | Requerida |
|---------------|------------------------------------------------|-------------------------------------------------------------------------------|-----------|
| `MONGO_URI`   | Cadena de conexión completa a MongoDB          | `mongodb://admin:pass@mongodb:27017/sigd_busqueda?authSource=admin`          | Sí        |
| `LOG_LEVEL`   | Nivel mínimo de log (debug/info/warn/error)    | `info`                                                                        | No        |
| `NODE_ENV`    | Entorno de ejecución                           | `production`                                                                  | No        |

La URI se construye en `docker-compose.yml` usando:

```
MONGO_URI=mongodb://${MONGO_USERNAME}:${MONGO_PASSWORD}@mongodb:27017/sigd_busqueda?authSource=admin
```

Las variables `MONGO_USERNAME` y `MONGO_PASSWORD` deben estar definidas en el archivo `.env` de la raíz del repositorio.

Si `MONGO_URI` no está definida, el módulo cae al fallback `mongodb://localhost:27017/sigd` (útil para pruebas locales rápidas, no para producción).

---

## 📡 API / Endpoints

Puerto base: **3000**

---

### POST /indexar

Registra los metadatos de un documento recién aprobado. Es invocado por el Módulo Central (.NET) tras aprobar un documento.

**Parámetros (body JSON):**

| Campo                | Tipo     | Requerido | Descripción                                          |
|----------------------|----------|-----------|------------------------------------------------------|
| `id_documento_sql`   | number   | Sí        | ID del documento en SQL Server (único)               |
| `codigo_interno`     | string   | Sí        | Código único interno, ej. `"CAL-MAN-001"` (único)   |
| `titulo`             | string   | Sí        | Título del documento                                 |
| `tags`               | string[] | No        | Palabras clave para búsqueda                         |
| `version`            | number   | No        | Versión del documento (default: `1`)                 |
| `contenido_extraido` | string   | No        | Texto extraído del PDF para búsqueda full-text       |
| `id_usuario_creacion`| number   | Sí        | ID del usuario que aprobó el documento               |

**Ejemplo de petición:**

```bash
curl -X POST http://localhost:3000/indexar \
  -H "Content-Type: application/json" \
  -d '{
    "id_documento_sql": 11,
    "codigo_interno": "CAL-MAN-001",
    "titulo": "Manual de Calidad ISO 9001:2015",
    "tags": ["calidad", "ISO 9001", "manual", "SGC"],
    "version": 4,
    "contenido_extraido": "Documento rector del Sistema de Gestión de Calidad.",
    "id_usuario_creacion": 1
  }'
```

**Respuesta exitosa (201):**

```json
{
  "success": true,
  "mensaje": "Documento indexado correctamente",
  "data": {
    "_id": "6650a1b2c3d4e5f6a7b8c9d0",
    "id_documento_sql": 11,
    "codigo_interno": "CAL-MAN-001",
    "titulo": "Manual de Calidad ISO 9001:2015",
    "tags": ["calidad", "ISO 9001", "manual", "SGC"],
    "version": 4,
    "estatus": true,
    "fecha_indexacion": "2026-05-22T10:00:00.000Z"
  }
}
```

**Códigos de estado:**

| Código | Situación                                              |
|--------|--------------------------------------------------------|
| 201    | Documento indexado correctamente                       |
| 400    | Faltan campos obligatorios                             |
| 409    | `id_documento_sql` o `codigo_interno` ya existe        |
| 500    | Error interno del servidor                             |

---

### GET /buscar

Busca documentos activos por título, tags o contenido extraído usando expresión regular insensible a mayúsculas. Protegido contra ReDoS.

**Parámetros (query string):**

| Parámetro | Tipo   | Requerido | Descripción                                              |
|-----------|--------|-----------|----------------------------------------------------------|
| `q`       | string | Sí        | Término de búsqueda (máximo 100 caracteres)              |

**Ejemplo de petición:**

```bash
curl "http://localhost:3000/buscar?q=calidad"
```

**Respuesta exitosa (200):**

```json
{
  "success": true,
  "total": 1,
  "data": [
    {
      "_id": "6650a1b2c3d4e5f6a7b8c9d0",
      "id_documento_sql": 11,
      "codigo_interno": "CAL-MAN-001",
      "titulo": "Manual de Calidad ISO 9001:2015",
      "tags": ["calidad", "ISO 9001", "manual", "SGC"],
      "version": 4,
      "estatus": true,
      "fecha_indexacion": "2026-05-22T10:00:00.000Z"
    }
  ]
}
```

Solo se devuelven documentos con `estatus: true`. El array puede estar vacío si no hay coincidencias.

**Códigos de estado:**

| Código | Situación                                         |
|--------|---------------------------------------------------|
| 200    | Búsqueda completada (puede retornar array vacío)  |
| 400    | Parámetro `q` ausente, solo espacios, o >100 chars|
| 500    | Error interno del servidor                        |

---

### GET /documento/:id

Devuelve los metadatos de un único documento activo. El parámetro `:id` puede ser numérico o alfanumérico.

**Parámetros (path):**

| Parámetro | Tipo   | Descripción                                                         |
|-----------|--------|---------------------------------------------------------------------|
| `id`      | string | Numérico → busca por `id_documento_sql`; alfanumérico → por `codigo_interno` |

Ambos casos filtran adicionalmente por `estatus: true`.

**Ejemplos de petición:**

```bash
# Por ID numérico (SQL Server)
curl http://localhost:3000/documento/11

# Por código interno
curl http://localhost:3000/documento/CAL-MAN-001
```

**Respuesta exitosa (200):**

```json
{
  "success": true,
  "data": {
    "_id": "6650a1b2c3d4e5f6a7b8c9d0",
    "id_documento_sql": 11,
    "codigo_interno": "CAL-MAN-001",
    "titulo": "Manual de Calidad ISO 9001:2015",
    "tags": ["calidad", "ISO 9001", "manual", "SGC"],
    "version": 4,
    "estatus": true,
    "fecha_indexacion": "2026-05-22T10:00:00.000Z"
  }
}
```

**Códigos de estado:**

| Código | Situación                                      |
|--------|------------------------------------------------|
| 200    | Documento encontrado                           |
| 404    | No existe ningún documento activo con ese ID   |
| 500    | Error interno del servidor                     |

---

## 📖 Documentación Interactiva (OpenAPI/Swagger)

El módulo expone una interfaz Swagger UI generada automáticamente desde anotaciones JSDoc en `index.ts`.

| URL                              | Descripción                                      |
|----------------------------------|--------------------------------------------------|
| `http://localhost:3000/docs`     | Swagger UI interactivo — prueba los 3 endpoints desde el navegador |
| `http://localhost:3000/docs.json`| Especificación OpenAPI 3.0 en formato JSON crudo |

**Librerías utilizadas:**
- `swagger-ui-express ^5.0.0` — renderiza la interfaz Swagger
- `swagger-jsdoc ^6.2.8` — genera la spec desde comentarios `@openapi` en el código

La spec se genera en tiempo de ejecución: no es necesario regenerarla ni mantener un archivo separado.

---

## 🗄️ Base de Datos

**Motor:** MongoDB 7.0  
**Base de datos:** `sigd_busqueda`  
**Colección activa (Mongoose):** `DocumentosMetadata`

### Schema Mongoose (`index.ts`)

| Campo                    | Tipo      | Requerido | Descripción                                        |
|--------------------------|-----------|-----------|----------------------------------------------------|
| `id_documento_sql`       | Number    | Sí        | ID del documento en SQL Server (índice único)      |
| `codigo_interno`         | String    | Sí        | Código único interno (índice único)                |
| `titulo`                 | String    | Sí        | Título del documento                               |
| `tags`                   | [String]  | No        | Palabras clave para búsqueda (default: `[]`)       |
| `version`                | Number    | No        | Versión del documento (mínimo 1; default: `1`)     |
| `contenido_extraido`     | String    | No        | Texto del documento para búsqueda full-text        |
| `id_usuario_creacion`    | Number    | Sí        | ID del usuario que aprobó e indexó el documento    |
| `estatus`                | Boolean   | Sí        | Borrado lógico: `true` = activo (default: `true`)  |
| `fecha_indexacion`       | Date      | No        | Fecha de inserción (default: `Date.now`)           |
| `fecha_modificacion`     | Date      | No        | Última modificación (default: `null`)              |
| `id_usuario_modificacion`| Number    | No        | Usuario que modificó (default: `null`)             |
| `fecha_eliminacion`      | Date      | No        | Fecha de borrado lógico (default: `null`)          |
| `id_usuario_eliminacion` | Number    | No        | Usuario que eliminó (default: `null`)              |

### Inicialización

El script `scripts/mongo/init_busqueda.js` se ejecuta automáticamente la primera vez que el contenedor de MongoDB se levanta (vía `docker-entrypoint-initdb.d`). Crea:

- La base de datos `sigd_busqueda`
- La colección `DocumentosMetadata` con validación de esquema JSON
- Índices de texto completo ponderados (título × 10, tags × 5, contenido × 1)
- La colección `Usuarios` con índice único por correo
- Un usuario administrador semilla (`admin@sigd.local`)

---

## 🔐 Seguridad

**Sanitización anti-ReDoS:** El endpoint `GET /buscar` aplica la función `escapeRegex()` antes de construir cualquier expresión regular contra MongoDB. Esta función escapa todos los metacaracteres regex (`.*+?^${}()|[\]`), convirtiendo la consulta en texto literal. Sin esta protección, un atacante podría enviar patrones como `(a+)+` que consumen CPU de forma exponencial.

**Límite de longitud de query:** Las consultas de búsqueda están limitadas a **100 caracteres**. Cualquier valor más largo es rechazado con `400 Bad Request` antes de llegar a la base de datos.

Estas dos medidas se aplican de forma independiente y en el orden correcto: primero se valida la longitud (operación O(1)), luego se sanitiza el contenido.

---

## 📊 Logs Estructurados

**Librería:** `pino ^9.0.0` (con `pino-pretty ^11.0.0` en desarrollo)

| Entorno     | Formato                                    | Destino  |
|-------------|--------------------------------------------|----------|
| Producción  | JSON puro (una línea por entrada)          | stdout   |
| Desarrollo  | Coloreado con pino-pretty                  | stdout   |

**Nivel configurable** con la variable de entorno `LOG_LEVEL` (valores: `debug`, `info`, `warn`, `error`; default: `info`).

**Eventos registrados:**

| Evento                      | Nivel | Contexto incluido                                      |
|-----------------------------|-------|--------------------------------------------------------|
| `http_request`              | info  | `method`, `url`, `status`, `duration_ms`               |
| `server_started`            | info  | `port`, `node_env`, `log_level`                        |
| `mongodb_connected`         | info  | `uri_masked` (contraseña enmascarada)                  |
| `mongodb_connection_failed` | error | `err`                                                  |
| `request_failed`            | error | `err`, `endpoint`, campo de contexto adicional         |

Las URIs de MongoDB se enmascaran automáticamente antes de loguearlas (la contraseña no aparece en los logs).

**Ejemplo de log en modo desarrollo:**

```
[21:49:42.834] INFO (25): http_request
    method: "GET"
    url: "/buscar?q=calidad"
    status: 200
    duration_ms: 3
```

---

## 🐳 Docker

El `Dockerfile` usa **4 stages**:

| Stage        | Base             | Descripción                                                    |
|--------------|------------------|----------------------------------------------------------------|
| `base`       | `node:20-alpine` | Workdir `/usr/src/app`, copia `package*.json`                 |
| `development`| `base`           | Instala todas las dependencias; el código llega por volumen    |
| `builder`    | `base`           | Copia fuentes y compila TypeScript → `dist/`                  |
| `production` | `base`           | Solo deps de producción + `dist/` copiado desde `builder`     |

**Stage activo en `docker-compose.yml`:** `development`

**Puerto expuesto:** `3000`

**Volúmenes (modo desarrollo):**
- `./src/ModuloBusqueda:/usr/src/app` — código local montado en el contenedor
- `/usr/src/app/node_modules` — volumen anónimo que protege `node_modules` interno contra sobreescritura

**Comando de inicio (development):** `npm run dev` (ts-node-dev con recarga automática)  
**Comando de inicio (production):** `node dist/server.js`

---

## 🧪 Tests

**Framework:** Jest 29 + ts-jest + supertest  
**Suites:** 4 archivos, **35 tests** en total  
**Cobertura:** 100% líneas, 98% statements, 85% funciones, 82% branches

| Suite                     | Tests | Qué cubre                                           |
|---------------------------|-------|-----------------------------------------------------|
| `escapeRegex.test.ts`     | 10    | Función de sanitización anti-ReDoS                  |
| `indexar.test.ts`         | 8     | `POST /indexar`: validaciones, duplicados, errores  |
| `buscar.test.ts`          | 8     | `GET /buscar`: query, límites, regex, errores       |
| `documento.test.ts`       | 9     | `GET /documento/:id`: numérico vs alfanumérico      |

**Sin infraestructura real:** los tests usan mocks puros de Mongoose — no requieren MongoDB ni Docker corriendo.

**Ejecutar tests (sin instalar nada localmente):**

```bash
# Desde src/ModuloBusqueda/
docker run --rm \
  -v "$(pwd):/app" \
  -w /app \
  node:20-alpine \
  sh -c "npm ci && npm test"
```

El reporte de cobertura se genera en `coverage/` (excluido de git).

---

## 🧪 Pruebas Rápidas

Tras ejecutar `docker compose up modulo_busqueda`:

**1. Verificar que el servidor responde:**

```bash
curl "http://localhost:3000/buscar?q=test"
# Esperar: {"success":true,"total":0,"data":[]}
```

**2. Indexar un documento de prueba:**

```bash
curl -X POST http://localhost:3000/indexar \
  -H "Content-Type: application/json" \
  -d '{
    "id_documento_sql": 1,
    "codigo_interno": "TEST-001",
    "titulo": "Procedimiento de Prueba",
    "tags": ["prueba", "demo"],
    "id_usuario_creacion": 1
  }'
# Esperar: {"success":true,"mensaje":"Documento indexado correctamente",...}
```

**3. Buscar el documento recién indexado:**

```bash
curl "http://localhost:3000/buscar?q=prueba"
# Esperar: {"success":true,"total":1,"data":[{...}]}
```

**4. Obtener el documento por código interno:**

```bash
curl http://localhost:3000/documento/TEST-001
# Esperar: {"success":true,"data":{...}}
```

**5. Obtener el documento por ID numérico:**

```bash
curl http://localhost:3000/documento/1
# Esperar: {"success":true,"data":{...}}
```

**6. Verificar rechazo de query demasiado largo:**

```bash
curl "http://localhost:3000/buscar?q=$(python3 -c 'print("a"*101)')"
# Esperar: {"success":false,"mensaje":"El término de búsqueda es demasiado largo..."}
```

**7. Explorar la API en Swagger:**

Abrir en el navegador: `http://localhost:3000/docs`

---

## 🐛 Problemas Conocidos y Solución

**Error: "❌ Error conectando a MongoDB"**

El contenedor `app_busqueda_node` no puede alcanzar MongoDB. Causas comunes:
- `MONGO_URI` incorrecta en el archivo `.env`
- El contenedor `mongodb` no ha terminado de inicializarse

Solución: verificar `docker logs sigd_mongodb` y que `MONGO_USERNAME` / `MONGO_PASSWORD` en `.env` coincidan con la URI.

---

**Error: `409 Conflict` al reintentar `/indexar`**

El `id_documento_sql` o `codigo_interno` enviado ya existe en la colección (índices únicos en MongoDB).

Solución: cambiar los campos únicos o eliminar el documento existente desde la consola de Mongo antes de reinsertar.

---

**Puerto 3000 ya ocupado**

```
Error: bind: address already in use :::3000
```

Solución: identificar el proceso con `netstat -ano | findstr :3000` (Windows) y detenerlo, o cambiar el mapeo en `docker-compose.yml` a `"3001:3000"`.

---

## 🤝 Integración con Otros Módulos

```
┌─────────────────────────┐       POST /indexar        ┌────────────────────────┐
│  ModuloCentral (.NET)   │ ─────────────────────────► │  ModuloBusqueda        │
│  Puerto 5000            │                             │  Node.js + MongoDB     │
│  SQL Server             │                             │  Puerto 3000           │
└─────────────────────────┘                             └────────────┬───────────┘
                                                                     │
                                                  GET /buscar?q=...  │
                                                  GET /documento/:id │
                                                                     ▼
                                                        ┌────────────────────────┐
                                                        │  Portal Operario       │
                                                        │  (ModuloReportes /     │
                                                        │   portal_operario.php) │
                                                        └────────────────────────┘
```

- **ModuloCentral → ModuloBusqueda:** El módulo .NET llama a `POST /indexar` cada vez que aprueba un documento nuevo o actualiza uno existente.
- **Portal de Operarios → ModuloBusqueda:** La vista `portal_operario.php` realiza búsquedas en tiempo real contra `GET /buscar?q=...` y extrae `response.data.data` antes de renderizar la tabla de resultados.
- **ModuloReportes:** No consume directamente este módulo; recibe documentos sincronizados desde ModuloCentral vía su propio endpoint `api/sync.php`.

Todos los servicios se comunican dentro de la red Docker `sigd_network` usando los nombres de servicio como hostnames.

---

## 👥 Contribución

1. Crear una rama a partir de `development`:
   ```bash
   git checkout -b feature/busqueda-mi-mejora
   ```
2. Realizar los cambios en `src/ModuloBusqueda/`.
3. Verificar que los tests siguen pasando: `npm test`
4. Verificar que el módulo sigue respondiendo correctamente (ver [Pruebas Rápidas](#-pruebas-rápidas)).
5. Confirmar los cambios con un mensaje descriptivo:
   ```bash
   git commit -m "feat(busqueda): descripción del cambio"
   ```
6. Abrir un Pull Request hacia la rama `development` en GitHub.

---

## 📄 Licencia

Proyecto académico — Ingeniería en Informática

---

> 🌍 English version available on request.
