// ==========================================================
// SIGD EMPRESARIAL — SEED DATA DE DEMOSTRACIÓN
// Módulo de Búsqueda (MongoDB)
// Inserta 18 documentos de muestra en DocumentosMetadata
// para que el Buscador Global devuelva resultados reales.
// ==========================================================

db = db.getSiblingDB('sigd_busqueda');

print('============================================');
print('  Insertando datos de muestra en MongoDB...');
print('============================================');

const ahora = new Date();
const hace  = (meses) => new Date(ahora - meses * 30 * 24 * 60 * 60 * 1000);

const docs = [
  {
    id_documento_sql: 1,
    codigo_interno: 'ADM-POL-001',
    titulo: 'Política de Seguridad de la Información',
    tags: ['seguridad', 'política', 'administración', 'ISO 27001'],
    version: 2,
    contenido_extraido: 'Esta política establece los lineamientos para la protección de la información confidencial de la empresa, incluyendo controles de acceso, cifrado y gestión de incidentes de seguridad.',
    atributos_especificos: { norma: 'ISO 27001', revision: 'anual' },
    estatus: true, fecha_creacion: hace(11), id_usuario_creacion: 1,
    fecha_modificacion: null, id_usuario_modificacion: null,
    fecha_eliminacion: null, id_usuario_eliminacion: null
  },
  {
    id_documento_sql: 2,
    codigo_interno: 'ADM-MAN-001',
    titulo: 'Manual de Organización y Funciones',
    tags: ['manual', 'organización', 'funciones', 'administración'],
    version: 1,
    contenido_extraido: 'Describe la estructura organizacional de la empresa, los puestos de trabajo, funciones, responsabilidades y líneas de autoridad de cada área.',
    atributos_especificos: { version: '1.0' },
    estatus: true, fecha_creacion: hace(9), id_usuario_creacion: 1,
    fecha_modificacion: null, id_usuario_modificacion: null,
    fecha_eliminacion: null, id_usuario_eliminacion: null
  },
  {
    id_documento_sql: 3,
    codigo_interno: 'ADM-PROC-001',
    titulo: 'Procedimiento de Auditorías Internas',
    tags: ['auditoría', 'procedimiento', 'ISO 9001', 'administración'],
    version: 3,
    contenido_extraido: 'Define el proceso para planificar, ejecutar y dar seguimiento a las auditorías internas del sistema de gestión de calidad, conforme a la norma ISO 9001:2015.',
    atributos_especificos: { norma: 'ISO 9001', frecuencia: 'trimestral' },
    estatus: true, fecha_creacion: hace(6), id_usuario_creacion: 1,
    fecha_modificacion: null, id_usuario_modificacion: null,
    fecha_eliminacion: null, id_usuario_eliminacion: null
  },
  {
    id_documento_sql: 4,
    codigo_interno: 'RH-PROC-001',
    titulo: 'Procedimiento de Reclutamiento y Selección',
    tags: ['reclutamiento', 'selección', 'recursos humanos', 'personal'],
    version: 1,
    contenido_extraido: 'Establece el proceso para la atracción, evaluación y contratación de nuevo personal, incluyendo entrevistas, pruebas psicométricas y verificación de referencias.',
    atributos_especificos: {},
    estatus: true, fecha_creacion: hace(10), id_usuario_creacion: 1,
    fecha_modificacion: null, id_usuario_modificacion: null,
    fecha_eliminacion: null, id_usuario_eliminacion: null
  },
  {
    id_documento_sql: 5,
    codigo_interno: 'RH-PROC-002',
    titulo: 'Procedimiento de Evaluación de Desempeño',
    tags: ['evaluación', 'desempeño', 'recursos humanos', 'KPI'],
    version: 2,
    contenido_extraido: 'Describe el proceso anual de evaluación del desempeño de los colaboradores, indicadores clave y plan de desarrollo individual.',
    atributos_especificos: { frecuencia: 'semestral' },
    estatus: true, fecha_creacion: hace(5), id_usuario_creacion: 1,
    fecha_modificacion: null, id_usuario_modificacion: null,
    fecha_eliminacion: null, id_usuario_eliminacion: null
  },
  {
    id_documento_sql: 6,
    codigo_interno: 'RH-FMT-001',
    titulo: 'Formato de Solicitud de Vacaciones',
    tags: ['vacaciones', 'formato', 'recursos humanos', 'solicitud'],
    version: 1,
    contenido_extraido: 'Formato estándar para que los colaboradores soliciten formalmente sus períodos vacacionales, incluyendo datos del empleado, fechas solicitadas y aprobación del jefe directo.',
    atributos_especificos: {},
    estatus: true, fecha_creacion: hace(8), id_usuario_creacion: 1,
    fecha_modificacion: null, id_usuario_modificacion: null,
    fecha_eliminacion: null, id_usuario_eliminacion: null
  },
  {
    id_documento_sql: 7,
    codigo_interno: 'PRD-INS-001',
    titulo: 'Instructivo de Operación de Línea A',
    tags: ['producción', 'instructivo', 'línea A', 'operación', 'maquinaria'],
    version: 1,
    contenido_extraido: 'Instrucciones paso a paso para la operación segura y eficiente de la Línea A de producción, incluyendo arranque, ajustes de parámetros y paro de emergencia.',
    atributos_especificos: { maquina: 'Línea A', turno: 'todos' },
    estatus: true, fecha_creacion: hace(7), id_usuario_creacion: 1,
    fecha_modificacion: null, id_usuario_modificacion: null,
    fecha_eliminacion: null, id_usuario_eliminacion: null
  },
  {
    id_documento_sql: 8,
    codigo_interno: 'PRD-INS-002',
    titulo: 'Instructivo de Operación de Línea B',
    tags: ['producción', 'instructivo', 'línea B', 'operación', 'maquinaria'],
    version: 2,
    contenido_extraido: 'Instrucciones para la operación de la Línea B de producción, parámetros de proceso, tiempos de ciclo y controles de calidad en línea.',
    atributos_especificos: { maquina: 'Línea B', turno: 'todos' },
    estatus: true, fecha_creacion: hace(4), id_usuario_creacion: 1,
    fecha_modificacion: null, id_usuario_modificacion: null,
    fecha_eliminacion: null, id_usuario_eliminacion: null
  },
  {
    id_documento_sql: 9,
    codigo_interno: 'PRD-PROC-001',
    titulo: 'Procedimiento de Control de Producción',
    tags: ['producción', 'control', 'procedimiento', 'planificación'],
    version: 1,
    contenido_extraido: 'Establece los controles para garantizar que la producción cumpla con los estándares de calidad, tiempo y costo, incluyendo seguimiento de órdenes de producción.',
    atributos_especificos: {},
    estatus: true, fecha_creacion: hace(3), id_usuario_creacion: 1,
    fecha_modificacion: null, id_usuario_modificacion: null,
    fecha_eliminacion: null, id_usuario_eliminacion: null
  },
  {
    id_documento_sql: 10,
    codigo_interno: 'PRD-ESP-001',
    titulo: 'Especificación Técnica de Materiales',
    tags: ['materiales', 'especificación', 'producción', 'técnico'],
    version: 1,
    contenido_extraido: 'Especificaciones técnicas de los materiales e insumos utilizados en el proceso productivo, incluyendo dimensiones, tolerancias y proveedores aprobados.',
    atributos_especificos: {},
    estatus: true, fecha_creacion: hace(2), id_usuario_creacion: 1,
    fecha_modificacion: null, id_usuario_modificacion: null,
    fecha_eliminacion: null, id_usuario_eliminacion: null
  },
  {
    id_documento_sql: 11,
    codigo_interno: 'CAL-MAN-001',
    titulo: 'Manual de Calidad ISO 9001:2015',
    tags: ['calidad', 'ISO 9001', 'manual', 'SGC', 'gestión'],
    version: 4,
    contenido_extraido: 'Documento rector del Sistema de Gestión de Calidad de la empresa. Describe el alcance del SGC, la política de calidad, objetivos y la interacción entre procesos.',
    atributos_especificos: { norma: 'ISO 9001:2015', alcance: 'empresa completa' },
    estatus: true, fecha_creacion: hace(12), id_usuario_creacion: 1,
    fecha_modificacion: null, id_usuario_modificacion: null,
    fecha_eliminacion: null, id_usuario_eliminacion: null
  },
  {
    id_documento_sql: 12,
    codigo_interno: 'CAL-PROC-001',
    titulo: 'Procedimiento de Control de No Conformidades',
    tags: ['no conformidad', 'calidad', 'procedimiento', 'acciones correctivas'],
    version: 2,
    contenido_extraido: 'Define el proceso para identificar, registrar, analizar y corregir no conformidades detectadas durante auditorías, inspecciones o quejas de clientes.',
    atributos_especificos: { norma: 'ISO 9001:2015' },
    estatus: true, fecha_creacion: hace(6), id_usuario_creacion: 1,
    fecha_modificacion: null, id_usuario_modificacion: null,
    fecha_eliminacion: null, id_usuario_eliminacion: null
  },
  {
    id_documento_sql: 13,
    codigo_interno: 'CAL-FMT-001',
    titulo: 'Formato de Reporte de No Conformidad',
    tags: ['formato', 'no conformidad', 'calidad', 'reporte'],
    version: 1,
    contenido_extraido: 'Formato para documentar no conformidades detectadas, descripción del problema, causa raíz, acciones correctivas y evidencia de cierre.',
    atributos_especificos: {},
    estatus: true, fecha_creacion: hace(5), id_usuario_creacion: 1,
    fecha_modificacion: null, id_usuario_modificacion: null,
    fecha_eliminacion: null, id_usuario_eliminacion: null
  },
  {
    id_documento_sql: 14,
    codigo_interno: 'MNT-PROC-001',
    titulo: 'Procedimiento de Mantenimiento Preventivo',
    tags: ['mantenimiento', 'preventivo', 'procedimiento', 'equipos', 'maquinaria'],
    version: 1,
    contenido_extraido: 'Establece el programa y los pasos para realizar el mantenimiento preventivo de los equipos e instalaciones, incluyendo frecuencias, listas de verificación y responsables.',
    atributos_especificos: { tipo: 'preventivo' },
    estatus: true, fecha_creacion: hace(9), id_usuario_creacion: 1,
    fecha_modificacion: null, id_usuario_modificacion: null,
    fecha_eliminacion: null, id_usuario_eliminacion: null
  },
  {
    id_documento_sql: 15,
    codigo_interno: 'MNT-INS-001',
    titulo: 'Instructivo de Lubricación de Equipos',
    tags: ['mantenimiento', 'lubricación', 'instructivo', 'equipos'],
    version: 1,
    contenido_extraido: 'Instrucciones para la lubricación periódica de equipos y maquinaria, especificando tipo de lubricante, puntos de aplicación, cantidades y frecuencias recomendadas.',
    atributos_especificos: {},
    estatus: true, fecha_creacion: hace(1), id_usuario_creacion: 1,
    fecha_modificacion: null, id_usuario_modificacion: null,
    fecha_eliminacion: null, id_usuario_eliminacion: null
  },
  {
    id_documento_sql: 16,
    codigo_interno: 'TI-POL-001',
    titulo: 'Política de Uso Aceptable de Recursos Informáticos',
    tags: ['TI', 'política', 'informática', 'seguridad', 'sistemas'],
    version: 1,
    contenido_extraido: 'Define el uso permitido de los recursos tecnológicos de la empresa, incluyendo internet, correo electrónico, dispositivos móviles y software autorizado.',
    atributos_especificos: { revision: 'anual' },
    estatus: true, fecha_creacion: hace(11), id_usuario_creacion: 1,
    fecha_modificacion: null, id_usuario_modificacion: null,
    fecha_eliminacion: null, id_usuario_eliminacion: null
  },
  {
    id_documento_sql: 17,
    codigo_interno: 'TI-PROC-001',
    titulo: 'Procedimiento de Respaldo y Recuperación de Datos',
    tags: ['TI', 'respaldo', 'backup', 'recuperación', 'sistemas'],
    version: 2,
    contenido_extraido: 'Establece el proceso para realizar copias de respaldo de datos críticos, la periodicidad, medios de almacenamiento y el procedimiento para recuperación ante desastres.',
    atributos_especificos: { frecuencia: 'diaria' },
    estatus: true, fecha_creacion: hace(3), id_usuario_creacion: 1,
    fecha_modificacion: null, id_usuario_modificacion: null,
    fecha_eliminacion: null, id_usuario_eliminacion: null
  },
  {
    id_documento_sql: 18,
    codigo_interno: 'TI-MAN-001',
    titulo: 'Manual de Usuarios del Sistema SIGD',
    tags: ['SIGD', 'manual', 'usuario', 'sistema', 'TI', 'documentos'],
    version: 1,
    contenido_extraido: 'Guía completa para los usuarios del Sistema Integral de Gestión Documental (SIGD). Incluye registro, creación de documentos, flujo de aprobación y búsqueda de normativas.',
    atributos_especificos: { sistema: 'SIGD Empresarial' },
    estatus: true, fecha_creacion: hace(2), id_usuario_creacion: 1,
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
