"use strict";
// ═══════════════════════════════════════════════════════
// MÓDULO DE BÚSQUEDA — Node.js + TypeScript + MongoDB
// Proyecto: SIGD Empresarial
// Autor: Josue J.A.V.
// ═══════════════════════════════════════════════════════
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || (function () {
    var ownKeys = function(o) {
        ownKeys = Object.getOwnPropertyNames || function (o) {
            var ar = [];
            for (var k in o) if (Object.prototype.hasOwnProperty.call(o, k)) ar[ar.length] = k;
            return ar;
        };
        return ownKeys(o);
    };
    return function (mod) {
        if (mod && mod.__esModule) return mod;
        var result = {};
        if (mod != null) for (var k = ownKeys(mod), i = 0; i < k.length; i++) if (k[i] !== "default") __createBinding(result, mod, k[i]);
        __setModuleDefault(result, mod);
        return result;
    };
})();
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.Metadato = exports.app = exports.logger = void 0;
exports.escapeRegex = escapeRegex;
const express_1 = __importDefault(require("express"));
const mongoose_1 = __importStar(require("mongoose"));
const swagger_ui_express_1 = __importDefault(require("swagger-ui-express"));
const swagger_jsdoc_1 = __importDefault(require("swagger-jsdoc"));
const pino_1 = __importDefault(require("pino"));
// ── 1. LOGGER ────────────────────────────────────────
// Prod (NODE_ENV=production): JSON puro en stdout, 1 línea por entrada.
// Dev: pino-pretty con colores (instalado en devDependencies).
const loggerOptions = {
    level: process.env['LOG_LEVEL'] ?? 'info'
};
if (process.env['NODE_ENV'] !== 'production') {
    loggerOptions.transport = { target: 'pino-pretty', options: { colorize: true } };
}
exports.logger = (0, pino_1.default)(loggerOptions);
// ── 2. CONFIGURACIÓN EXPRESS ──────────────────────────
exports.app = (0, express_1.default)();
exports.app.use(express_1.default.json());
// Middleware para habilitar CORS (Cross-Origin Resource Sharing)
exports.app.use((req, res, next) => {
    res.setHeader('Access-Control-Allow-Origin', '*');
    res.setHeader('Access-Control-Allow-Methods', 'GET, POST, OPTIONS, PUT, PATCH, DELETE');
    res.setHeader('Access-Control-Allow-Headers', 'X-Requested-With,content-type,Authorization');
    if (req.method === 'OPTIONS') {
        res.sendStatus(200);
    }
    else {
        next();
    }
});
// Middleware de log HTTP: registra método, URL, status y latencia
// de cada petición al terminar la respuesta (evento 'finish').
exports.app.use((req, res, next) => {
    const start = Date.now();
    res.on('finish', () => {
        exports.logger.info({
            method: req.method,
            url: req.url,
            status: res.statusCode,
            duration_ms: Date.now() - start
        }, 'http_request');
    });
    next();
});
const MetadatoSchema = new mongoose_1.Schema({
    id_documento_sql: { type: Number, required: true, unique: true },
    id_empresa: { type: Number, required: true },
    codigo_interno: { type: String, required: true, unique: true },
    titulo: { type: String, required: true },
    tags: { type: [String], default: [] },
    version: { type: Number, min: 1 },
    contenido_extraido: { type: String },
    id_usuario_creacion: { type: Number, required: true },
    estatus: { type: Boolean, required: true, default: true },
    fecha_indexacion: { type: Date, default: Date.now },
    fecha_modificacion: { type: Date, default: null },
    id_usuario_modificacion: { type: Number, default: null },
    fecha_eliminacion: { type: Date, default: null },
    id_usuario_eliminacion: { type: Number, default: null }
}, { collection: 'DocumentosMetadata' });
// Índice de texto completo — coincide con IDX_BusquedaGlobal_Text del init script
MetadatoSchema.index({ titulo: 'text', tags: 'text', contenido_extraido: 'text' }, { weights: { titulo: 10, tags: 5, contenido_extraido: 1 }, default_language: 'spanish', name: 'IDX_BusquedaGlobal_Text' });
// Evita error "Cannot overwrite model" cuando Jest reimporta el módulo
exports.Metadato = mongoose_1.default.models['DocumentosMetadata'] ??
    mongoose_1.default.model('DocumentosMetadata', MetadatoSchema);
// ── 4. OPENAPI / SWAGGER ──────────────────────────────
// apis: [__filename] funciona en ts-node-dev (.ts) y en producción
// compilada (.js) porque tsconfig no tiene removeComments: true.
const swaggerSpec = (0, swagger_jsdoc_1.default)({
    definition: {
        openapi: '3.0.0',
        info: {
            title: 'ModuloBusqueda API',
            version: '1.0.0',
            description: 'API REST del Módulo de Búsqueda del SIGD Empresarial. ' +
                'Permite indexar documentos aprobados y realizar búsquedas full-text ' +
                'sobre sus metadatos almacenados en MongoDB.'
        },
        servers: [
            { url: 'http://localhost:3000', description: 'Desarrollo local' },
            { url: 'http://modulo_busqueda:3000', description: 'Red Docker interna' }
        ],
        tags: [
            { name: 'Documentos', description: 'Indexación y búsqueda de documentos' }
        ],
        components: {
            schemas: {
                MetadatoInput: {
                    type: 'object',
                    required: ['id_documento_sql', 'id_empresa', 'codigo_interno', 'titulo', 'id_usuario_creacion'],
                    properties: {
                        id_documento_sql: { type: 'integer', example: 11, description: 'ID del documento en SQL Server' },
                        id_empresa: { type: 'integer', example: 1, description: 'ID de la empresa (tenant)' },
                        codigo_interno: { type: 'string', example: 'CAL-MAN-001', description: 'Código interno único del documento' },
                        titulo: { type: 'string', example: 'Manual de Calidad ISO 9001:2015' },
                        tags: { type: 'array', items: { type: 'string' }, example: ['calidad', 'ISO 9001', 'manual', 'SGC'] },
                        version: { type: 'integer', minimum: 1, example: 4, description: 'Versión del documento (default 1 si no se envía)' },
                        contenido_extraido: { type: 'string', example: 'Documento rector del Sistema de Gestión de Calidad de la empresa.' },
                        id_usuario_creacion: { type: 'integer', example: 1, description: 'ID del usuario que aprobó el documento' }
                    }
                },
                Metadato: {
                    allOf: [
                        { '$ref': '#/components/schemas/MetadatoInput' },
                        {
                            type: 'object',
                            properties: {
                                _id: { type: 'string', example: '6650a1b2c3d4e5f6a7b8c9d0', readOnly: true },
                                estatus: { type: 'boolean', example: true, description: 'Borrado lógico: true=activo' },
                                fecha_indexacion: { type: 'string', format: 'date-time', readOnly: true }
                            }
                        }
                    ]
                },
                RespuestaError: {
                    type: 'object',
                    properties: {
                        success: { type: 'boolean', example: false },
                        mensaje: { type: 'string', example: 'Descripción del error' },
                        detalle: { type: 'string', example: 'Mensaje técnico opcional' }
                    }
                }
            }
        }
    },
    apis: [__filename]
});
exports.app.use('/docs', swagger_ui_express_1.default.serve, swagger_ui_express_1.default.setup(swaggerSpec));
exports.app.get('/docs.json', (_req, res) => res.json(swaggerSpec));
// ── 5. ENDPOINTS ──────────────────────────────────────
/**
 * @openapi
 * /indexar:
 *   post:
 *     tags:
 *       - Documentos
 *     summary: Indexar un documento
 *     description: >
 *       Registra los metadatos de un documento aprobado en MongoDB.
 *       Lo llama el módulo .NET Central cuando el documento pasa a estado Aprobado.
 *       Si no se envía `version`, el valor por defecto es 1.
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             $ref: '#/components/schemas/MetadatoInput'
 *           example:
 *             id_documento_sql: 11
 *             codigo_interno: CAL-MAN-001
 *             titulo: Manual de Calidad ISO 9001:2015
 *             tags: [calidad, ISO 9001, manual, SGC, gestión]
 *             version: 4
 *             contenido_extraido: Documento rector del Sistema de Gestión de Calidad de la empresa.
 *             id_usuario_creacion: 1
 *     responses:
 *       '201':
 *         description: Documento indexado correctamente
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                   example: true
 *                 mensaje:
 *                   type: string
 *                   example: Documento indexado correctamente
 *                 data:
 *                   $ref: '#/components/schemas/Metadato'
 *       '400':
 *         description: Faltan campos obligatorios
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/RespuestaError'
 *             example:
 *               success: false
 *               mensaje: 'Faltan campos obligatorios: id_documento_sql, codigo_interno, titulo, id_usuario_creacion'
 *       '409':
 *         description: El documento ya está indexado (id o código duplicado)
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/RespuestaError'
 *             example:
 *               success: false
 *               mensaje: Este documento ya está indexado
 *       '500':
 *         description: Error interno del servidor
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/RespuestaError'
 */
exports.app.post('/indexar', async (req, res) => {
    try {
        const { id_documento_sql, id_empresa, codigo_interno, titulo, tags, version, contenido_extraido, id_usuario_creacion } = req.body;
        if (!id_documento_sql || !id_empresa || !codigo_interno || !titulo || !id_usuario_creacion) {
            res.status(400).json({
                success: false,
                mensaje: 'Faltan campos obligatorios: id_documento_sql, id_empresa, codigo_interno, titulo, id_usuario_creacion'
            });
            return;
        }
        const nuevoMetadato = new exports.Metadato({
            id_documento_sql,
            id_empresa,
            codigo_interno,
            titulo,
            tags: tags || [],
            version: version ?? 1,
            contenido_extraido: contenido_extraido ?? undefined,
            id_usuario_creacion,
            estatus: true,
            fecha_indexacion: new Date()
        });
        await nuevoMetadato.save();
        res.status(201).json({
            success: true,
            mensaje: 'Documento indexado correctamente',
            data: nuevoMetadato
        });
    }
    catch (error) {
        const err = error;
        if (err.code === 11000) {
            res.status(409).json({ success: false, mensaje: 'Este documento ya está indexado' });
            return;
        }
        exports.logger.error({ err, endpoint: 'POST /indexar' }, 'request_failed');
        res.status(500).json({ success: false, mensaje: 'Error interno del servidor', detalle: err.message });
    }
});
// ── Helper: escapa metacaracteres regex para evitar ReDoS ──
// Sin esto, un usuario podría enviar patrones como "(a+)+" que consumen
// recursos exponencialmente (Denegación de Servicio por regex catastrófico).
function escapeRegex(text) {
    return text.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
}
/**
 * @openapi
 * /buscar:
 *   get:
 *     tags:
 *       - Documentos
 *     summary: Buscar documentos por término
 *     description: >
 *       Busca documentos activos por término en título, tags y contenido extraído.
 *       La búsqueda es insensible a mayúsculas y está protegida contra ataques
 *       ReDoS mediante escape de metacaracteres regex.
 *     parameters:
 *       - in: query
 *         name: q
 *         required: true
 *         schema:
 *           type: string
 *           maxLength: 100
 *         description: Término de búsqueda (máximo 100 caracteres)
 *         example: calidad
 *     responses:
 *       '200':
 *         description: Lista de documentos que coinciden con el término
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                   example: true
 *                 total:
 *                   type: integer
 *                   example: 3
 *                 data:
 *                   type: array
 *                   items:
 *                     $ref: '#/components/schemas/Metadato'
 *       '400':
 *         description: Parámetro `q` ausente o mayor a 100 caracteres
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/RespuestaError'
 *             example:
 *               success: false
 *               mensaje: Debes enviar un término de búsqueda. Ejemplo /buscar?q=calidad
 *       '500':
 *         description: Error interno del servidor
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/RespuestaError'
 */
exports.app.get('/buscar', async (req, res) => {
    try {
        const query = req.query['q']?.trim();
        const id_empresa = parseInt(req.query['id_empresa'], 10);
        if (!id_empresa) {
            res.status(400).json({ success: false, mensaje: 'Debes enviar el parámetro id_empresa.' });
            return;
        }
        if (!query) {
            res.status(400).json({ success: false, mensaje: 'Debes enviar un término de búsqueda. Ejemplo: /buscar?q=calidad' });
            return;
        }
        if (query.length > 100) {
            res.status(400).json({ success: false, mensaje: 'El término de búsqueda es demasiado largo (máximo 100 caracteres).' });
            return;
        }
        const safeQuery = escapeRegex(query);
        const regex = { $regex: safeQuery, $options: 'i' };
        const resultados = await exports.Metadato.find({
            estatus: true,
            id_empresa,
            $or: [
                { titulo: regex },
                { tags: regex },
                { contenido_extraido: regex }
            ]
        });
        res.status(200).json({ success: true, total: resultados.length, data: resultados });
    }
    catch (error) {
        exports.logger.error({ err: error, endpoint: 'GET /buscar', q: req.query['q'] }, 'request_failed');
        res.status(500).json({ success: false, mensaje: 'Error al buscar documentos', detalle: error.message });
    }
});
/**
 * @openapi
 * /documento/{id}:
 *   get:
 *     tags:
 *       - Documentos
 *     summary: Obtener un documento por ID o código interno
 *     description: >
 *       Devuelve los metadatos completos de un documento activo.
 *       Si el parámetro `id` es numérico se busca por `id_documento_sql`;
 *       si es alfanumérico (ej. CAL-MAN-001) se busca por `codigo_interno`.
 *     parameters:
 *       - in: path
 *         name: id
 *         required: true
 *         schema:
 *           type: string
 *         description: ID numérico de SQL Server o código interno del documento
 *         example: CAL-MAN-001
 *     responses:
 *       '200':
 *         description: Documento encontrado
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                   example: true
 *                 data:
 *                   $ref: '#/components/schemas/Metadato'
 *       '404':
 *         description: No existe ningún documento activo con ese ID o código
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/RespuestaError'
 *             example:
 *               success: false
 *               mensaje: 'No se encontró ningún documento activo con id: CAL-MAN-999'
 *       '500':
 *         description: Error interno del servidor
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/RespuestaError'
 */
exports.app.get('/documento/:id', async (req, res) => {
    try {
        const id = req.params['id'];
        const id_empresa = parseInt(req.query['id_empresa'], 10);
        if (!id_empresa) {
            res.status(400).json({ success: false, mensaje: 'Debes enviar el parámetro id_empresa.' });
            return;
        }
        const esNumerico = /^\d+$/.test(id);
        const filtro = esNumerico
            ? { id_documento_sql: parseInt(id, 10), id_empresa, estatus: true }
            : { codigo_interno: id, id_empresa, estatus: true };
        const documento = await exports.Metadato.findOne(filtro);
        if (!documento) {
            res.status(404).json({ success: false, mensaje: `No se encontró ningún documento activo con id: ${id}` });
            return;
        }
        res.status(200).json({ success: true, data: documento });
    }
    catch (error) {
        exports.logger.error({ err: error, endpoint: 'GET /documento/:id', id: req.params['id'] }, 'request_failed');
        res.status(500).json({ success: false, mensaje: 'Error al obtener el documento', detalle: error.message });
    }
});
//# sourceMappingURL=index.js.map