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
const MONGO_URL = process.env.MONGO_URL || 'mongodb://localhost:27017/sigd';

mongoose.connect(MONGO_URL)
  .then(() => console.log('✅ Conectado a MongoDB'))
  .catch((err) => console.error('❌ Error conectando a MongoDB:', err));

// ── 3. MODELO DE DATOS (cómo luce un documento en MongoDB)
// Esto define la "forma" de cada registro que guardaremos
interface IMetadato extends Document {
  id_documento: string;   // Ej: "DOC-001"
  titulo: string;         // Ej: "Manual de Calidad"
  etiquetas: string[];    // Ej: ["calidad", "ISO", "manual"]
  extension: string;      // Ej: "pdf"
  tamanio_kb: number;     // Ej: 245
  fecha_indexacion: Date; // Ej: 2024-01-15
}

const MetadatoSchema = new Schema<IMetadato>({
  id_documento:     { type: String, required: true, unique: true },
  titulo:           { type: String, required: true },
  etiquetas:        { type: [String], default: [] },
  extension:        { type: String, required: true },
  tamanio_kb:       { type: Number, required: true },
  fecha_indexacion: { type: Date, default: Date.now }
});

const Metadato = mongoose.model<IMetadato>('Metadato', MetadatoSchema);

// ── 4. ENDPOINTS ──────────────────────────────────────

// ┌─────────────────────────────────────────────────────┐
// │ ENDPOINT 1: POST /indexar                           │
// │ Lo llama el módulo .NET cuando aprueba un documento │
// └─────────────────────────────────────────────────────┘
app.post('/indexar', async (req: Request, res: Response) => {
  try {
    // Tomamos los datos que llegaron en el cuerpo del POST
    const { id_documento, titulo, etiquetas, extension, tamanio_kb } = req.body;

    // Validamos que los campos obligatorios existan
    if (!id_documento || !titulo || !extension || !tamanio_kb) {
      res.status(400).json({
        success: false,
        mensaje: 'Faltan campos obligatorios: id_documento, titulo, extension, tamanio_kb'
      });
      return;
    }

    // Guardamos en MongoDB
    const nuevoMetadato = new Metadato({
      id_documento,
      titulo,
      etiquetas: etiquetas || [],
      extension,
      tamanio_kb,
      fecha_indexacion: new Date()
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

// ┌─────────────────────────────────────────────────────┐
// │ ENDPOINT 2: GET /buscar?q=palabra                   │
// │ Busca documentos por título o etiquetas             │
// └─────────────────────────────────────────────────────┘
app.get('/buscar', async (req: Request, res: Response) => {
  try {
    // Tomamos el parámetro ?q= de la URL
    const query = req.query.q as string;

    if (!query || query.trim() === '') {
      res.status(400).json({
        success: false,
        mensaje: 'Debes enviar un término de búsqueda. Ejemplo: /buscar?q=calidad'
      });
      return;
    }

    // Buscamos en MongoDB — busca en título Y en etiquetas
    // $regex significa "contiene esta palabra"
    // $options: 'i' significa que no importa mayúsculas/minúsculas
    const resultados = await Metadato.find({
      $or: [
        { titulo:    { $regex: query, $options: 'i' } },
        { etiquetas: { $regex: query, $options: 'i' } }
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
    // Tomamos el :id de la URL. Ej: /documento/DOC-001
    const { id } = req.params;

    const documento = await Metadato.findOne({ id_documento: id } as any);

    if (!documento) {
      res.status(404).json({
        success: false,
        mensaje: `No se encontró ningún documento con id: ${id}`
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