import express, { Request, Response } from 'express';
import mongoose from 'mongoose';
import cors from 'cors';
import dotenv from 'dotenv';

dotenv.config();

const app = express();
const PORT = process.env.PORT || 3000;

// Middleware para procesar payloads JSON gigantes (común en normativas de calidad)
app.use(cors());
app.use(express.json({ limit: '50mb' }));

// URI de conexión inyectada desde el docker-compose
const MONGO_URI = process.env.MONGO_URI || 'mongodb://admin:admin@mongodb:27017/sistema_calidad_db?authSource=admin';

// ==========================================
// DEFINICIÓN DEL ESQUEMA CON AUDITORÍA (Mongoose)
// ==========================================
const DocumentoMetadataSchema = new mongoose.Schema({
    id_documento_sql: { type: Number, required: true, unique: true },
    codigo_interno: { type: String, required: true, unique: true },
    titulo: { type: String, required: true },
    tags: { type: [String], default: [] },
    contenido_extraido: { type: String, default: "" },
    atributos_especificos: { type: Object, default: {} },
    version: { type: Number, default: 1 },

    // Espejo de Auditoría y Borrado Lógico de PostgreSQL
    estatus: { type: Boolean, default: true },
    fecha_creacion: { type: Date, default: Date.now },
    id_usuario_creacion: { type: Number, required: true },
    fecha_modificacion: { type: Date, default: null },
    id_usuario_modificacion: { type: Number, default: null },
    fecha_eliminacion: { type: Date, default: null },
    id_usuario_eliminacion: { type: Number, default: null }
}, { collection: 'DocumentosMetadata' });

// --- AQUÍ CONSTRUIMOS EL ÍNDICE DE TEXTO DIRECTAMENTE EN EL ESQUEMA ---
// Esto le dice a MongoDB de forma automática qué campos indexar y con qué prioridad
DocumentoMetadataSchema.index(
    { titulo: 'text', tags: 'text', contenido_extraido: 'text' },
    {
        weights: { titulo: 10, tags: 5, contenido_extraido: 1 },
        name: 'IDX_BusquedaGlobal_Text',
        default_language: 'spanish'
    }
);

const DocumentoMetadata = mongoose.model('DocumentoMetadata', DocumentoMetadataSchema);

// ==========================================
// RUTA RAÍZ INFORMATIVA (Soluciona el "Cannot GET /")
// ==========================================
app.get('/', (req: Request, res: Response) => {
    res.status(200).json({
        modulo: "Módulo de Indexación y Búsqueda NoSQL",
        estado: "Conectado y Operacional",
        puerto: PORT
    });
});

// ==========================================
// ENDPOINT 1: RECEPCIÓN DEL WEBHOOK (.NET -> NODE)
// ==========================================
app.post('/api/busqueda/sincronizar', async (req: Request, res: Response): Promise<void> => {
    try {
        const {
            id_documento_sql,
            codigo_interno,
            titulo,
            tags,
            contenido_extraido,
            atributos_especificos,
            id_usuario_creacion,
            version
        } = req.body;

        // Validación estructural básica
        if (!id_documento_sql || !codigo_interno || !titulo) {
            res.status(400).json({ error: "Datos obligatorios faltantes (ID, Código o Título)." });
            return;
        }

        // Simulación de Upsert (Guardar o actualizar si ya existe)
        const documentoActualizado = await DocumentoMetadata.findOneAndUpdate(
            { id_documento_sql },
            {
                codigo_interno,
                titulo,
                tags,
                contenido_extraido,
                atributos_especificos,
                id_usuario_creacion,
                version,
                estatus: true,
                fecha_modificacion: new Date()
            },
            { upsert: true, new: true, setDefaultsOnInsert: true }
        );

        res.status(200).json({
            message: "Documento indexado con éxito en MongoDB",
            id: documentoActualizado._id
        });
    } catch (error: any) {
        res.status(500).json({ error: "Fallo interno al indexar: " + error.message });
    }
});

// ==========================================
// ENDPOINT 2: BUSCADOR GLOBAL (Full-Text Search)
// ==========================================
app.get('/api/busqueda/buscar', async (req: Request, res: Response): Promise<void> => {
    try {
        const queryTexto = req.query.q as string;

        if (!queryTexto) {
            res.status(400).json({ error: "Falta el parámetro de búsqueda '?q='" });
            return;
        }

        // Ejecuta la consulta usando el índice de texto '$text' configurado en Mongo
        const resultados = await DocumentoMetadata.find(
            {
                $text: { $search: queryTexto },
                estatus: true // Filtro de borrado lógico
            },
            { score: { $meta: "textScore" } }
        ).sort({ score: { $meta: "textScore" } }); // Ordena por relevancia automática

        res.status(200).json(resultados);
    } catch (error: any) {
        res.status(500).json({ error: "Error en la consulta NoSQL: " + error.message });
    }
});

// Conexión asíncrona a la Base de Datos
mongoose.connect(MONGO_URI)
    .then(() => {
        console.log('🌱 Conexión exitosa a MongoDB (SIGD_Búsquedas).');
        app.listen(PORT, () => {
            console.log(`Servidor de indexación corriendo en http://localhost:${PORT}`);
        });
    })
    .catch(err => console.error('Fallo crítico al conectar a MongoDB:', err));