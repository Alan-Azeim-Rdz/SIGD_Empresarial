// Fixtures y utilidades reutilizables para los tests de ModuloBusqueda.

/** Retorna un objeto plano que simula un documento de Metadato de MongoDB. */
export function mockDoc(overrides: Record<string, unknown> = {}): Record<string, unknown> {
  return {
    _id:               'mock-id-abc123',
    id_documento_sql:  1,
    id_empresa:        1,
    codigo_interno:    'CAL-MAN-001',
    titulo:            'Manual de Calidad ISO 9001:2015',
    tags:              ['calidad', 'ISO'],
    version:           '1.0',
    ip_subida:         '127.0.0.1',
    contenido_extraido:'Documento de prueba.',
    id_usuario_creacion: 1,
    estatus:           true,
    fecha_indexacion:  new Date('2024-01-15').toISOString(),
    fecha_modificacion: null,
    id_usuario_modificacion: null,
    ...overrides,
  };
}

/** Payload mínimo válido para POST /indexar */
export const payloadIndexarValido = {
  id_documento_sql:    42,
  id_empresa:          1,
  codigo_interno:      'TEST-DOC-001',
  titulo:              'Documento de Prueba',
  tags:                ['prueba', 'test'],
  version:             '2.0',
  ip_subida:           '192.168.1.100',
  contenido_extraido:  'Contenido de prueba para indexación.',
  id_usuario_creacion: 7,
};
