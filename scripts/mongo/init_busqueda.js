// ===========================================================
// Script de Inicialización de MongoDB para el Módulo de Búsqueda
// Se ejecuta automáticamente la primera vez que el contenedor
// se levanta via docker-entrypoint-initdb.d
//
// Conexión: Se autentica con las credenciales root definidas
// en MONGO_INITDB_ROOT_USERNAME / MONGO_INITDB_ROOT_PASSWORD
// ===========================================================

// Usar/crear la base de datos del módulo de búsqueda
// (Mongoose se conectará a esta misma DB via MONGO_URI)
db = db.getSiblingDB('sigd_busqueda');

print('========================================');
print('  Inicializando MongoDB - SIGD Búsqueda');
print('========================================');

// ----------------------------------------------------------
// 1. Colección principal: DocumentosMetadata
//    Debe coincidir con el esquema Mongoose en index.ts
// ----------------------------------------------------------
db.createCollection('DocumentosMetadata', {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["id_documento_sql", "codigo_interno", "titulo", "id_usuario_creacion"],
            properties: {
                id_documento_sql: {
                    bsonType: "number",
                    description: "ID del documento en SQL Server (módulo central)"
                },
                codigo_interno: {
                    bsonType: "string",
                    description: "Código interno único del documento"
                },
                titulo: {
                    bsonType: "string",
                    description: "Título del documento"
                },
                tags: {
                    bsonType: "array",
                    items: { bsonType: "string" },
                    description: "Etiquetas para clasificación"
                },
                contenido_extraido: {
                    bsonType: "string",
                    description: "Texto extraído del documento para búsqueda full-text"
                },
                atributos_especificos: {
                    bsonType: "object",
                    description: "Atributos dinámicos específicos del tipo de documento"
                },
                version: {
                    bsonType: "number",
                    minimum: 1,
                    description: "Número de versión del documento (sincronizado con SQL Server)"
                },
                // Campos de auditoría (espejo del estándar del proyecto)
                estatus: {
                    bsonType: "bool",
                    description: "Borrado lógico: true=activo, false=eliminado"
                },
                fecha_creacion: { bsonType: "date" },
                id_usuario_creacion: { bsonType: "number" },
                fecha_modificacion: { bsonType: ["date", "null"] },
                id_usuario_modificacion: { bsonType: ["number", "null"] },
                fecha_eliminacion: { bsonType: ["date", "null"] },
                id_usuario_eliminacion: { bsonType: ["number", "null"] }
            }
        }
    },
    validationLevel: "moderate",
    validationAction: "warn"
});

print('  ✓ Colección "DocumentosMetadata" creada con validación de esquema.');

// ----------------------------------------------------------
// 2. Índices
// ----------------------------------------------------------

// Índice de texto completo (coincide con IDX_BusquedaGlobal_Text de Mongoose)
db.DocumentosMetadata.createIndex(
    { titulo: "text", tags: "text", contenido_extraido: "text" },
    {
        weights: { titulo: 10, tags: 5, contenido_extraido: 1 },
        name: "IDX_BusquedaGlobal_Text",
        default_language: "spanish"
    }
);
print('  ✓ Índice de texto completo creado (español, ponderado).');

// Índice único por ID del documento en SQL Server
db.DocumentosMetadata.createIndex(
    { id_documento_sql: 1 },
    { unique: true, name: "IDX_IdDocumentoSQL_Unique" }
);
print('  ✓ Índice único por id_documento_sql creado.');

// Índice único por código interno
db.DocumentosMetadata.createIndex(
    { codigo_interno: 1 },
    { unique: true, name: "IDX_CodigoInterno_Unique" }
);
print('  ✓ Índice único por codigo_interno creado.');

// Índice para filtro de borrado lógico
db.DocumentosMetadata.createIndex(
    { estatus: 1 },
    { name: "IDX_Estatus" }
);
print('  ✓ Índice de estatus creado.');

// ----------------------------------------------------------
// 3. Colección Espejo de Usuarios (Para Autenticación)
// ----------------------------------------------------------
db.createCollection('Usuarios', {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["correo", "contrasena", "estatus"],
            properties: {
                correo: { bsonType: "string" },
                contrasena: { bsonType: "string" },
                estatus: { bsonType: "bool" }
            }
        }
    }
});
db.Usuarios.createIndex({ correo: 1 }, { unique: true, name: "IDX_Correo_Unique" });
print('  ✓ Colección "Usuarios" creada.');

// Insertar Seed Data de Admin con la contraseña YA HASHEADA (simulando lo que hace SQL Server/Postgres)
// El Hash SHA-256 de 'Admin@SIGD2026!' es:
db.Usuarios.insert({
    correo: 'admin@sigd.local',
    contrasena: '9F86D081884C7D659A2FEAA0C55AD015A3BF4F1B2B0B822CD15D6C15B0F00A08', // Placeholder del Hash real
    estatus: true
});
print('  ✓ Usuario Admin insertado en MongoDB.');

// ----------------------------------------------------------
// 4. Función de validación de login (JavaScript local)
// db.system.js fue eliminada en MongoDB 4.4+; se declara aquí
// como función JS estándar para usar durante la inicialización.
// En producción, la lógica de login y hashing vive en Node.js.
// ----------------------------------------------------------
function fn_validar_login(correo, hashContrasena) {
    // Recibe el hash SHA-256 ya calculado por la capa de aplicación (Node.js).
    // MongoDB no expone funciones criptográficas nativas en el shell.
    return db.Usuarios.findOne({
        correo: correo,
        contrasena: hashContrasena,
        estatus: true
    });
}
print('  ✓ Función fn_validar_login declarada como JS local (db.system.js eliminado en MongoDB 4.4+).');

print('========================================');
print('  MongoDB inicializado exitosamente');
print('  Base de datos: sigd_busqueda');
print('========================================');
