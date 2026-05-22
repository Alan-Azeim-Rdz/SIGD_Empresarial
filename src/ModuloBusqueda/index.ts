// ═══════════════════════════════════════════════════════
// MÓDULO DE BÚSQUEDA — Node.js + TypeScript + MongoDB
// Proyecto: SIGD Empresarial
// Autor: Josue J.A.V.
// ═══════════════════════════════════════════════════════

import express, { Request, Response } from 'express';
import mongoose, { Schema, Document } from 'mongoose';

// ── 1. CONFIGURACIÓN INICIAL ──────────────────────────
const app = express();
const PORT = 3000;

// Le decimos a Express que entienda JSON
// (para recibir datos en el POST /indexar)
app.use(express.json());

// ── 2. CONEXIÓN A MONGODB ─────────────────────────────
// La URL viene de una variable de entorno (más seguro)
// Si no hay variable, usa localhost por defecto
const MONGO_URI = process.env.MONGO_URI || 'mongodb://localhost:27017/sigd';

mongoose.connect(MONGO_URI)
  .then(() => console.log('✅ Conectado a MongoDB'))
  .catch((err) => console.error('❌ Error conectando a MongoDB:', err));

// ── 3. MODELO DE DATOS (cómo luce un documento en MongoDB)
// Esquema sincronizado con scripts/mongo/init_busqueda.js
// Cualquier cambio aquí debe replicarse en el init script.
interface IMetadato extends Document {
  id_documento_sql:       number;   // ID del documento en SQL Server (módulo central)
  codigo_interno:         string;   // Código interno único del documento
  titulo:                 string;   // Título del documento
  tags:                   string[]; // Etiquetas para clasificación
  version?:               number;   // Número de versión sincronizado con SQL Server
  contenido_extraido?:    string;   // Texto extraído para búsqueda full-text (opcional)
  id_usuario_creacion:    number;   // ID del usuario que creó el registro
  estatus:                boolean;  // Borrado lógico: true=activo, false=eliminado
  fecha_indexacion:       Date;
  // Campos de auditoría opcionales
  fecha_modificacion?:    Date | null;
  id_usuario_modificacion?: number | null;
  fecha_eliminacion?:     Date | null;
  id_usuario_eliminacion?: number | null;
}

const MetadatoSchema = new Schema<IMetadato>(
  {
    id_documento_sql:        { type: Number, required: true, unique: true },
    codigo_interno:          { type: String, required: true, unique: true },
    titulo:                  { type: String, required: true },
    tags:                    { type: [String], default: [] },
    version:                 { type: Number, min: 1 },
    contenido_extraido:      { type: String },
    id_usuario_creacion:     { type: Number, required: true },
    estatus:                 { type: Boolean, required: true, default: true },
    fecha_indexacion:        { type: Date, default: Date.now },
    fecha_modificacion:      { type: Date, default: null },
    id_usuario_modificacion: { type: Number, default: null },
    fecha_eliminacion:       { type: Date, default: null },
    id_usuario_eliminacion:  { type: Number, default: null }
  },
  { collection: 'DocumentosMetadata' }
);

// Índice de texto completo — coincide con IDX_BusquedaGlobal_Text del init script
MetadatoSchema.index(
  { titulo: 'text', tags: 'text', contenido_extraido: 'text' },
  { weights: { titulo: 10, tags: 5, contenido_extraido: 1 }, default_language: 'spanish', name: 'IDX_BusquedaGlobal_Text' }
);

const Metadato = mongoose.model<IMetadato>('DocumentosMetadata', MetadatoSchema);

// ── 4. ENDPOINTS ──────────────────────────────────────

// ┌─────────────────────────────────────────────────────┐
// │ ENDPOINT 1: POST /indexar                           │
// │ Lo llama el módulo .NET cuando aprueba un documento │
// └─────────────────────────────────────────────────────┘
app.post('/indexar', async (req: Request, res: Response) => {
  try {
    // Tomamos los datos que llegaron en el cuerpo del POST
    const { id_documento_sql, codigo_interno, titulo, tags, version, contenido_extraido, id_usuario_creacion } = req.body;

    // Validamos campos requeridos según el $jsonSchema del init script
    if (!id_documento_sql || !codigo_interno || !titulo || !id_usuario_creacion) {
      res.status(400).json({
        success: false,
        mensaje: 'Faltan campos obligatorios: id_documento_sql, codigo_interno, titulo, id_usuario_creacion'
      });
      return;
    }

    // Guardamos en MongoDB
    const nuevoMetadato = new Metadato({
      id_documento_sql,
      codigo_interno,
      titulo,
      tags:                tags || [],
      version:             version ?? 1,
      contenido_extraido:  contenido_extraido ?? undefined,
      id_usuario_creacion,
      estatus:             true,
      fecha_indexacion:    new Date()
    });

    await nuevoMetadato.save();

    res.status(201).json({
      success: true,
      mensaje: 'Documento indexado correctamente',
      data: nuevoMetadato
    });

  } catch (error: any) {
    // Si el documento ya existe (id duplicado)
    if (error.code === 11000) {
      res.status(409).json({
        success: false,
        mensaje: 'Este documento ya está indexado'
      });
      return;
    }
    res.status(500).json({
      success: false,
      mensaje: 'Error interno del servidor',
      detalle: error.message
    });
  }
});
// ── Helper: escapa caracteres especiales de regex para evitar ReDoS ──
// Convierte cualquier metacarácter regex en texto literal seguro.
// Sin esto, un usuario podría enviar patrones como "(a+)+" que consumen
// recursos exponencialmente (ataque de Denegación de Servicio).
function escapeRegex(text: string): string {
  return text.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
}
// ┌─────────────────────────────────────────────────────┐
// │ ENDPOINT 2: GET /buscar?q=palabra                   │
// │ Busca documentos por título o etiquetas             │
// └─────────────────────────────────────────────────────┘

app.get('/buscar', async (req: Request, res: Response) => {
  try {
    // Tomamos el parámetro ?q= de la URL
    const query = (req.query.q as string)?.trim();

    // Validación 1: no puede estar vacío
    if (!query) {
      res.status(400).json({
        success: false,
        mensaje: 'Debes enviar un término de búsqueda. Ejemplo: /buscar?q=calidad'
      });
      return;
    }

    // Validación 2: límite de longitud (previene queries abusivos)
    if (query.length > 100) {
      res.status(400).json({
        success: false,
        mensaje: 'El término de búsqueda es demasiado largo (máximo 100 caracteres).'
      });
      return;
    }

    // Sanitización: escapamos caracteres especiales de regex
    // El usuario busca texto literal, no patrones regex
    const safeQuery = escapeRegex(query);

    // Buscamos en MongoDB — solo documentos activos (borrado lógico)
    // Busca en titulo, tags y contenido_extraido
    const regex = { $regex: safeQuery, $options: 'i' };
    const resultados = await Metadato.find({
      estatus: true,
      $or: [
        { titulo:             regex },
        { tags:               regex },
        { contenido_extraido: regex }
      ]
    });

    res.status(200).json({
      success: true,
      total: resultados.length,
      data: resultados
    });

  } catch (error: any) {
    res.status(500).json({
      success: false,
      mensaje: 'Error al buscar documentos',
      detalle: error.message
    });
  }
});

// ┌─────────────────────────────────────────────────────┐
// │ ENDPOINT 3: GET /documento/:id                      │
// │ Devuelve los metadatos de UN documento específico   │
// └─────────────────────────────────────────────────────┘
app.get('/documento/:id', async (req: Request, res: Response) => {
  try {
    // Tomamos el :id de la URL.
    // Si es numérico → busca por id_documento_sql; si no → por codigo_interno
    const { id } = req.params;
    const esNumerico = /^\d+$/.test(id);

    const filtro = esNumerico
      ? { id_documento_sql: parseInt(id, 10), estatus: true }
      : { codigo_interno: id,                 estatus: true };

    const documento = await Metadato.findOne(filtro);

    if (!documento) {
      res.status(404).json({
        success: false,
        mensaje: `No se encontró ningún documento activo con id: ${id}`
      });
      return;
    }

    res.status(200).json({
      success: true,
      data: documento
    });

  } catch (error: any) {
    res.status(500).json({
      success: false,
      mensaje: 'Error al obtener el documento',
      detalle: error.message
    });
  }
});

// ── 5. INICIAR EL SERVIDOR ────────────────────────────
app.listen(PORT, () => {
  console.log(`🚀 Módulo de Búsqueda corriendo en http://localhost:${PORT}`);
  console.log(`   POST /indexar`);
  console.log(`   GET  /buscar?q=...`);
  console.log(`   GET  /documento/:id`);
});