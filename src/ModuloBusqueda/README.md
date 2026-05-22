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

El Módulo de Búsqueda es el microservicio encargado de la **indexación y recuperación de metadatos de documentos** dentro del SIGD Empresarial. Cuando el Módulo Central (.NET) aprueba y publica un documento normativo, lo registra aquí vía `POST /indexar`. A partir de ese momento, cualquier cliente —el portal de operarios o el dashboard— puede localizar ese documento en tiempo real mediante `GET /buscar`.

A diferencia del Módulo Central (que gestiona el ciclo de vida documental en SQL Server) o del Módulo de Reportes (que genera estadísticas en PostgreSQL), este módulo se especializa exclusivamente en **velocidad de búsqueda**: almacena un subconjunto liviano de metadatos en MongoDB y responde consultas de texto libre sobre título y etiquetas.

Su diseño es deliberadamente simple: un único archivo `index.ts` expone tres endpoints Express, un modelo Mongoose y la lógica de sanitización necesaria para operar de forma segura en producción.

---

## 🛠️ Tecnologías

| Tecnología      | Versión   | Uso                                              |
|-----------------|-----------|--------------------------------------------------|
| Node.js         | 20 LTS    | Runtime de JavaScript del servidor               |
| TypeScript      | ^6.0.3    | Tipado estático sobre Node.js                    |
| Express         | ^5.2.1    | Framework HTTP (versión 5, async por defecto)    |
| Mongoose        | ^9.6.2    | ODM para MongoDB                                 |
| ts-node-dev     | ^2.0.0    | Recarga automática en desarrollo                 |
| ts-node         | ^10.9.2   | Ejecución directa de TypeScript                  |
| MongoDB         | 7.0        | Base de datos documental (imagen Docker oficial) |

---

## 📂 Estructura del Proyecto

```
src/ModuloBusqueda/
├── index.ts          # Punto de entrada: app Express, modelo Mongoose y los 3 endpoints
├── package.json      # Dependencias y scripts npm
├── tsconfig.json     # Configuración TypeScript (NodeNext, modo strict)
├── Dockerfile        # Imagen multi-stage: base → development → builder → production
├── dist/             # Salida del compilador tsc (generado; no versionar)
└── node_modules/     # Dependencias instaladas (no versionar)
```

El proyecto vive en un único archivo fuente (`index.ts`) que contiene la definición del servidor, el schema Mongoose y todos los handlers HTTP.

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

| Variable    | Descripción                                    | Valor de Ejemplo                                                              | Requerida |
|-------------|------------------------------------------------|-------------------------------------------------------------------------------|-----------|
| `MONGO_URI` | Cadena de conexión completa a MongoDB          | `mongodb://admin:pass@mongodb:27017/sigd_busqueda?authSource=admin`          | Sí        |

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

| Campo          | Tipo     | Requerido | Descripción                           |
|----------------|----------|-----------|---------------------------------------|
| `id_documento` | string   | Sí        | Identificador único, ej. `"DOC-001"` |
| `titulo`       | string   | Sí        | Título del documento                  |
| `extension`    | string   | Sí        | Formato del archivo, ej. `"pdf"`      |
| `tamanio_kb`   | number   | Sí        | Tamaño en kilobytes                   |
| `etiquetas`    | string[] | No        | Palabras clave para búsqueda          |

**Ejemplo de petición:**

```bash
curl -X POST http://localhost:3000/indexar \
  -H "Content-Type: application/json" \
  -d '{
    "id_documento": "DOC-001",
    "titulo": "Manual de Calidad ISO 9001",
    "extension": "pdf",
    "tamanio_kb": 245,
    "etiquetas": ["calidad", "ISO", "manual"]
  }'
```

**Respuesta exitosa (201):**

```json
{
  "success": true,
  "mensaje": "Documento indexado correctamente",
  "data": {
    "id_documento": "DOC-001",
    "titulo": "Manual de Calidad ISO 9001",
    "extension": "pdf",
    "tamanio_kb": 245,
    "etiquetas": ["calidad", "ISO", "manual"],
    "fecha_indexacion": "2026-05-22T10:00:00.000Z"
  }
}
```

**Códigos de estado:**

| Código | Situación                                      |
|--------|------------------------------------------------|
| 201    | Documento indexado correctamente               |
| 400    | Faltan campos obligatorios                     |
| 409    | El `id_documento` ya existe en la colección    |
| 500    | Error interno del servidor                     |

---

### GET /buscar

Busca documentos por título o etiquetas usando expresión regular insensible a mayúsculas. Protegido contra ReDoS.

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
      "_id": "...",
      "id_documento": "DOC-001",
      "titulo": "Manual de Calidad ISO 9001",
      "etiquetas": ["calidad", "ISO", "manual"],
      "extension": "pdf",
      "tamanio_kb": 245,
      "fecha_indexacion": "2026-05-22T10:00:00.000Z"
    }
  ]
}
```

**Códigos de estado:**

| Código | Situación                                         |
|--------|---------------------------------------------------|
| 200    | Búsqueda completada (puede retornar array vacío)  |
| 400    | Parámetro `q` ausente o supera 100 caracteres     |
| 500    | Error interno del servidor                        |

---

### GET /documento/:id

Devuelve los metadatos de un único documento a partir de su `id_documento`.

**Parámetros (path):**

| Parámetro | Tipo   | Descripción                                  |
|-----------|--------|----------------------------------------------|
| `id`      | string | Valor de `id_documento`, ej. `"DOC-001"`     |

**Ejemplo de petición:**

```bash
curl http://localhost:3000/documento/DOC-001
```

**Respuesta exitosa (200):**

```json
{
  "success": true,
  "data": {
    "_id": "...",
    "id_documento": "DOC-001",
    "titulo": "Manual de Calidad ISO 9001",
    "etiquetas": ["calidad", "ISO", "manual"],
    "extension": "pdf",
    "tamanio_kb": 245,
    "fecha_indexacion": "2026-05-22T10:00:00.000Z"
  }
}
```

**Códigos de estado:**

| Código | Situación                                      |
|--------|------------------------------------------------|
| 200    | Documento encontrado                           |
| 404    | No existe ningún documento con ese ID          |
| 500    | Error interno del servidor                     |

---

## 🗄️ Base de Datos

**Motor:** MongoDB 7.0  
**Base de datos:** `sigd_busqueda`  
**Colección activa (Mongoose):** `metadatos`

### Schema Mongoose (`index.ts`)

| Campo              | Tipo     | Requerido | Descripción                              |
|--------------------|----------|-----------|------------------------------------------|
| `id_documento`     | String   | Sí        | Identificador único (índice único)       |
| `titulo`           | String   | Sí        | Título del documento                     |
| `etiquetas`        | [String] | No        | Palabras clave para búsqueda             |
| `extension`        | String   | Sí        | Formato del archivo (`pdf`, `docx`, ...) |
| `tamanio_kb`       | Number   | Sí        | Tamaño en KB                             |
| `fecha_indexacion` | Date     | No        | Fecha de inserción (default: `Date.now`) |

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
  -d '{"id_documento":"TEST-001","titulo":"Procedimiento de Prueba","extension":"pdf","tamanio_kb":10,"etiquetas":["prueba","demo"]}'
# Esperar: {"success":true,"mensaje":"Documento indexado correctamente",...}
```

**3. Buscar el documento recién indexado:**

```bash
curl "http://localhost:3000/buscar?q=prueba"
# Esperar: {"success":true,"total":1,"data":[{...}]}
```

**4. Obtener el documento por ID:**

```bash
curl http://localhost:3000/documento/TEST-001
# Esperar: {"success":true,"data":{...}}
```

**5. Verificar rechazo de query demasiado largo:**

```bash
curl "http://localhost:3000/buscar?q=$(python3 -c 'print("a"*101)')"
# Esperar: {"success":false,"mensaje":"El término de búsqueda es demasiado largo..."}
```

---

## 🐛 Problemas Conocidos y Solución

**Error: "❌ Error conectando a MongoDB"**

El contenedor `app_busqueda_node` no puede alcanzar MongoDB. Causas comunes:
- `MONGO_URI` incorrecta en el archivo `.env`
- El contenedor `mongodb` no ha terminado de inicializarse

Solución: verificar `docker logs sigd_mongodb` y que `MONGO_USERNAME` / `MONGO_PASSWORD` en `.env` coincidan con la URI.

---

**Error: `409 Conflict` al reintentar `/indexar`**

El `id_documento` enviado ya existe en la colección (índice único en MongoDB).

Solución: cambiar el `id_documento` o eliminar el documento existente con la consola de Mongo antes de reinsertar.

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
- **Portal de Operarios → ModuloBusqueda:** La vista `portal_operario.php` realiza búsquedas en tiempo real contra `GET /buscar` para que los operarios encuentren normativas vigentes.
- **ModuloReportes:** No consume directamente este módulo; recibe documentos sincronizados desde ModuloCentral vía su propio endpoint `api/sync.php`.

Todos los servicios se comunican dentro de la red Docker `sigd_network` usando los nombres de servicio como hostnames.

---

## 👥 Contribución

1. Crear una rama a partir de `development`:
   ```bash
   git checkout -b feature/busqueda-mi-mejora
   ```
2. Realizar los cambios en `src/ModuloBusqueda/`.
3. Verificar que el módulo sigue respondiendo correctamente (ver [Pruebas Rápidas](#-pruebas-rápidas)).
4. Confirmar los cambios con un mensaje descriptivo:
   ```bash
   git commit -m "feat(busqueda): descripción del cambio"
   ```
5. Abrir un Pull Request hacia la rama `development` en GitHub.

---

## 📄 Licencia

Proyecto académico — Ingeniería en Informática

---

> 🌍 English version available on request.
