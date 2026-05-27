// ==========================================================
// SIGD EMPRESARIAL — SEED DATA DE DEMOSTRACIÓN
// Módulo de Búsqueda (MongoDB)
// Inserta 16 documentos de muestra alineados con SQL Server
// ==========================================================

db = db.getSiblingDB('sigd_busqueda');

print('============================================');
print('  Re-creando datos de muestra en MongoDB...');
print('============================================');

db.DocumentosMetadata.deleteMany({});

const ahora = new Date();
const hace  = (dias) => new Date(ahora - dias * 24 * 60 * 60 * 1000);

const docs = [
  // ── TECHCORP SOLUTIONS (id_empresa = 2) ──────────────────────────
  {
    id_documento_sql: 1,
    id_empresa: 2,
    codigo_interno: 'TC-MT-001',
    titulo: 'Manual de Configuración de Servidores Linux',
    tags: ['manual', 'configuracion', 'servidores', 'linux'],
    version: 2,
    contenido_extraido: 'Lineamientos completos para la instalacion, configuracion y aseguramiento de servidores corporativos Linux Debian y RedHat.',
    estatus: true, fecha_creacion: hace(85), id_usuario_creacion: 2,
    fecha_modificacion: null, id_usuario_modificacion: null,
    fecha_eliminacion: null, id_usuario_eliminacion: null
  },
  {
    id_documento_sql: 2,
    id_empresa: 2,
    codigo_interno: 'TC-PI-001',
    titulo: 'Política de Seguridad de la Información',
    tags: ['politica', 'seguridad', 'informacion', 'ISO 27001'],
    version: 1,
    contenido_extraido: 'Establece las politicas y controles de seguridad para salvaguardar los activos de informacion y cumplir con ISO 27001.',
    estatus: true, fecha_creacion: hace(70), id_usuario_creacion: 2,
    fecha_modificacion: null, id_usuario_modificacion: null,
    fecha_eliminacion: null, id_usuario_eliminacion: null
  },
  {
    id_documento_sql: 3,
    id_empresa: 2,
    codigo_interno: 'TC-PI-002',
    titulo: 'Política de Vacaciones y Permisos',
    tags: ['politica', 'vacaciones', 'permisos', 'recursos humanos'],
    version: 1,
    contenido_extraido: 'Normativa interna para la solicitud y aprobacion de periodos vacacionales, permisos con goce y sin goce de sueldo.',
    estatus: true, fecha_creacion: hace(30), id_usuario_creacion: 4,
    fecha_modificacion: null, id_usuario_modificacion: null,
    fecha_eliminacion: null, id_usuario_eliminacion: null
  },
  {
    id_documento_sql: 4,
    id_empresa: 2,
    codigo_interno: 'TC-CON-001',
    titulo: 'Contrato de Servicios Cloud – Proveedor AWS',
    tags: ['contrato', 'servicios', 'cloud', 'aws', 'proveedor'],
    version: 1,
    contenido_extraido: 'Acuerdo de nivel de servicio (SLA) y contrato comercial para el aprovisionamiento de infraestructura en la nube de Amazon Web Services.',
    estatus: true, fecha_creacion: hace(60), id_usuario_creacion: 6,
    fecha_modificacion: null, id_usuario_modificacion: null,
    fecha_eliminacion: null, id_usuario_eliminacion: null
  },
  {
    id_documento_sql: 5,
    id_empresa: 2,
    codigo_interno: 'TC-MT-002',
    titulo: 'Guía de Implementación de DevOps con GitLab CI/CD',
    tags: ['manual', 'devops', 'gitlab', 'cicd', 'tecnologia'],
    version: 1,
    contenido_extraido: 'Manual tecnico que detalla el pipeline estandarizado para despliegues continuos utilizando Docker y Gitlab CI/CD.',
    estatus: true, fecha_creacion: hace(10), id_usuario_creacion: 2,
    fecha_modificacion: null, id_usuario_modificacion: null,
    fecha_eliminacion: null, id_usuario_eliminacion: null
  },
  {
    id_documento_sql: 6,
    id_empresa: 2,
    codigo_interno: 'TC-LEG-001',
    titulo: 'Política de Protección de Datos Personales (LGPDP)',
    tags: ['politica', 'legal', 'datos personales', 'privacidad'],
    version: 1,
    contenido_extraido: 'Establece los lineamientos y politicas obligatorias para el tratamiento y resguardo de datos personales de clientes y empleados.',
    estatus: true, fecha_creacion: hace(20), id_usuario_creacion: 6,
    fecha_modificacion: null, id_usuario_modificacion: null,
    fecha_eliminacion: null, id_usuario_eliminacion: null
  },
  {
    id_documento_sql: 7,
    id_empresa: 2,
    codigo_interno: 'TC-CON-002',
    titulo: 'Contrato Colectivo de Trabajo 2026',
    tags: ['contrato', 'laboral', 'trabajo', 'recursos humanos'],
    version: 1,
    contenido_extraido: 'Acuerdo oficial entre el sindicato y la empresa que establece las condiciones de trabajo, salarios y prestaciones para el periodo 2026.',
    estatus: true, fecha_creacion: hace(45), id_usuario_creacion: 4,
    fecha_modificacion: null, id_usuario_modificacion: null,
    fecha_eliminacion: null, id_usuario_eliminacion: null
  },
  {
    id_documento_sql: 8,
    id_empresa: 2,
    codigo_interno: 'TC-ADM-001',
    titulo: 'Manual de Procesos Administrativos v1.0',
    tags: ['manual', 'procesos', 'administracion', 'gestion'],
    version: 1,
    contenido_extraido: 'Describe el flujo de compras, aprobacion de viaticos, reembolsos y demas procesos administrativos internos.',
    estatus: true, fecha_creacion: hace(50), id_usuario_creacion: 2,
    fecha_modificacion: null, id_usuario_modificacion: null,
    fecha_eliminacion: null, id_usuario_eliminacion: null
  },

  // ── GRUPO INNOVAR (id_empresa = 3) ──────────────────────────
  {
    id_documento_sql: 9,
    id_empresa: 3,
    codigo_interno: 'GI-RF-001',
    titulo: 'Reporte Financiero Q1 2026',
    tags: ['reporte', 'financiero', 'presupuesto', 'q1'],
    version: 1,
    contenido_extraido: 'Estados financieros consolidados correspondientes al primer trimestre de 2026, balance general y estado de resultados.',
    estatus: true, fecha_creacion: hace(80), id_usuario_creacion: 8,
    fecha_modificacion: null, id_usuario_modificacion: null,
    fecha_eliminacion: null, id_usuario_eliminacion: null
  },
  {
    id_documento_sql: 10,
    id_empresa: 3,
    codigo_interno: 'GI-RF-002',
    titulo: 'Reporte Financiero Q2 2026',
    tags: ['reporte', 'financiero', 'presupuesto', 'q2'],
    version: 1,
    contenido_extraido: 'Borrador del reporte financiero del segundo trimestre de 2026, analisis de desviaciones presupuestarias.',
    estatus: true, fecha_creacion: hace(15), id_usuario_creacion: 8,
    fecha_modificacion: null, id_usuario_modificacion: null,
    fecha_eliminacion: null, id_usuario_eliminacion: null
  },
  {
    id_documento_sql: 11,
    id_empresa: 3,
    codigo_interno: 'GI-AR-001',
    titulo: 'Acta Reunión Consejo Directivo – Enero 2026',
    tags: ['acta', 'reunion', 'consejo', 'administracion'],
    version: 1,
    contenido_extraido: 'Acta oficial de la primera sesion ordinaria del Consejo Directivo, acuerdos sobre inversion de capital y metas corporativas.',
    estatus: true, fecha_creacion: hace(75), id_usuario_creacion: 3,
    fecha_modificacion: null, id_usuario_modificacion: null,
    fecha_eliminacion: null, id_usuario_eliminacion: null
  },
  {
    id_documento_sql: 12,
    id_empresa: 3,
    codigo_interno: 'GI-PO-001',
    titulo: 'Procedimiento de Control de Calidad en Línea de Producción',
    tags: ['procedimiento', 'calidad', 'produccion', 'operaciones'],
    version: 2,
    contenido_extraido: 'Establece los puntos de inspeccion, tolerancias dimensionales y de peso para el producto final en las lineas de ensamble.',
    estatus: true, fecha_creacion: hace(65), id_usuario_creacion: 10,
    fecha_modificacion: null, id_usuario_modificacion: null,
    fecha_eliminacion: null, id_usuario_eliminacion: null
  },
  {
    id_documento_sql: 13,
    id_empresa: 3,
    codigo_interno: 'GI-PO-002',
    titulo: 'Procedimiento de Gestión de Proveedores',
    tags: ['procedimiento', 'proveedores', 'compras', 'operaciones'],
    version: 1,
    contenido_extraido: 'Metodologia para la evaluacion, seleccion y control de proveedores estrategicos de materia prima e insumos industriales.',
    estatus: true, fecha_creacion: hace(8), id_usuario_creacion: 10,
    fecha_modificacion: null, id_usuario_modificacion: null,
    fecha_eliminacion: null, id_usuario_eliminacion: null
  },
  {
    id_documento_sql: 14,
    id_empresa: 3,
    codigo_interno: 'GI-AR-002',
    titulo: 'Acta Reunión Comercial – Plan de Ventas 2026',
    tags: ['acta', 'reunion', 'comercial', 'ventas'],
    version: 1,
    contenido_extraido: 'Minuta de acuerdos de la junta de planeacion comercial, metas por territorio y lanzamiento de nuevos productos.',
    estatus: true, fecha_creacion: hace(25), id_usuario_creacion: 12,
    fecha_modificacion: null, id_usuario_modificacion: null,
    fecha_eliminacion: null, id_usuario_eliminacion: null
  },
  {
    id_documento_sql: 15,
    id_empresa: 3,
    codigo_interno: 'GI-RF-003',
    titulo: 'Presupuesto Anual 2026 – Proyección vs Real',
    tags: ['reporte', 'presupuesto', 'anual', 'finanzas'],
    version: 1,
    contenido_extraido: 'Planilla de control presupuestario general para el año 2026, comparativa mensual de ingresos, costos y gastos operativos.',
    estatus: true, fecha_creacion: hace(50), id_usuario_creacion: 8,
    fecha_modificacion: null, id_usuario_modificacion: null,
    fecha_eliminacion: null, id_usuario_eliminacion: null
  },
  {
    id_documento_sql: 16,
    id_empresa: 3,
    codigo_interno: 'GI-PO-003',
    titulo: 'Procedimiento de Atención a Quejas y Reclamaciones',
    tags: ['procedimiento', 'quejas', 'atencion', 'comercial', 'clientes'],
    version: 1,
    contenido_extraido: 'Establece los canales de recepcion, tiempos maximos de respuesta y acciones correctivas para quejas de clientes.',
    estatus: true, fecha_creacion: hace(40), id_usuario_creacion: 12,
    fecha_modificacion: null, id_usuario_modificacion: null,
    fecha_eliminacion: null, id_usuario_eliminacion: null
  }
];

let insertados = 0;
let omitidos   = 0;

docs.forEach(doc => {
  try {
    db.DocumentosMetadata.insertOne(doc);
    insertados++;
  } catch (e) {
    omitidos++;
  }
});

print('============================================');
print('  SEED DATA MONGODB COMPLETADO');
print('  Documentos insertados : ' + insertados);
print('  Omitidos (ya existían): ' + omitidos);
print('  Total en colección    : ' + db.DocumentosMetadata.countDocuments({ estatus: true }));
print('============================================');
