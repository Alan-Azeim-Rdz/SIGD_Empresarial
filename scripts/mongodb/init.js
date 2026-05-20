// ═══════════════════════════════════════════════════════
// SCRIPT MongoDB — Módulo de Búsqueda
// Proyecto: SIGD Empresarial
// Autor: Josue J.A.V.
// ═══════════════════════════════════════════════════════

// ── 1. SELECCIONAR BASE DE DATOS ──────────────────────
db = db.getSiblingDB('sigd');

// ── 2. CREAR COLECCIÓN DE METADATOS ───────────────────
db.createCollection('metadatos', {
    validator: {
        $jsonSchema: {
            bsonType: 'object',
            required: ['id_documento', 'titulo', 'extension', 'tamanio_kb'],
            properties: {
                id_documento: {
                    bsonType: 'string',
                    description: 'ID único del documento — obligatorio'
                },
                titulo: {
                    bsonType: 'string',
                    description: 'Título del documento — obligatorio'
                },
                etiquetas: {
                    bsonType: 'array',
                    description: 'Lista de etiquetas para búsqueda'
                },
                extension: {
                    bsonType: 'string',
                    description: 'Extensión del archivo: pdf, docx, etc.'
                },
                tamanio_kb: {
                    bsonType: 'number',
                    description: 'Tamaño del archivo en kilobytes'
                },
                fecha_indexacion: {
                    bsonType: 'date',
                    description: 'Fecha en que se indexó el documento'
                }
            }
        }
    }
});

// ── 3. ÍNDICES (para búsquedas más rápidas) ───────────
// Índice de texto para buscar por título y etiquetas
db.metadatos.createIndex(
    { titulo: 'text', etiquetas: 'text' },
    { name: 'idx_busqueda_texto' }
);

// Índice único para evitar documentos duplicados
db.metadatos.createIndex(
    { id_documento: 1 },
    { unique: true, name: 'idx_id_documento_unico' }
);

// Índice por fecha de indexación
db.metadatos.createIndex(
    { fecha_indexacion: -1 },
    { name: 'idx_fecha_indexacion' }
);

// ── 4. DATOS DE EJEMPLO ───────────────────────────────
db.metadatos.insertMany([
    {
        id_documento:     'DOC-001',
        titulo:           'Manual de Calidad ISO 9001',
        etiquetas:        ['calidad', 'ISO', 'manual', 'normativa'],
        extension:        'pdf',
        tamanio_kb:       245,
        fecha_indexacion: new Date('2024-01-15')
    },
    {
        id_documento:     'DOC-002',
        titulo:           'Procedimiento de Auditoría',
        etiquetas:        ['auditoria', 'procedimiento', 'revision'],
        extension:        'pdf',
        tamanio_kb:       180,
        fecha_indexacion: new Date('2024-02-20')
    },
    {
        id_documento:     'DOC-003',
        titulo:           'Política de Calidad',
        etiquetas:        ['calidad', 'politica', 'empresa'],
        extension:        'pdf',
        tamanio_kb:       95,
        fecha_indexacion: new Date('2024-03-10')
    },
    {
        id_documento:     'DOC-004',
        titulo:           'Manual de Procedimientos',
        etiquetas:        ['manual', 'procedimientos', 'operativo'],
        extension:        'docx',
        tamanio_kb:       320,
        fecha_indexacion: new Date('2024-04-05')
    },
    {
        id_documento:     'DOC-005',
        titulo:           'Reglamento Interno de Trabajo',
        etiquetas:        ['reglamento', 'trabajo', 'empleados', 'normativa'],
        extension:        'pdf',
        tamanio_kb:       150,
        fecha_indexacion: new Date('2024-05-12')
    }
]);

// ── 5. VERIFICAR QUE TODO SE CREÓ BIEN ────────────────
print('✅ Base de datos: sigd');
print('✅ Colección metadatos creada con validación');
print('✅ Índices creados: texto, único, fecha');
print('✅ Documentos de ejemplo insertados: ' + db.metadatos.countDocuments());
print('🚀 MongoDB listo para el Módulo de Búsqueda');