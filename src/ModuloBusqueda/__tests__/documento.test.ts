import request from 'supertest';
import { app, Metadato } from '../index';
import { mockDoc } from './helpers';

describe('GET /documento/:id', () => {
  let findOneSpy: jest.SpyInstance;

  beforeEach(() => {
    findOneSpy = jest.spyOn(Metadato, 'findOne').mockResolvedValue(null as never);
  });

  it('id numérico → busca por id_documento_sql con estatus:true', async () => {
    findOneSpy.mockResolvedValue(mockDoc({ id_documento_sql: 42 }) as never);

    const res = await request(app).get('/documento/42');

    expect(res.status).toBe(200);
    expect(findOneSpy).toHaveBeenCalledWith({ id_documento_sql: 42, estatus: true });
  });

  it('id alfanumérico → busca por codigo_interno con estatus:true', async () => {
    findOneSpy.mockResolvedValue(mockDoc({ codigo_interno: 'CAL-MAN-001' }) as never);

    const res = await request(app).get('/documento/CAL-MAN-001');

    expect(res.status).toBe(200);
    expect(findOneSpy).toHaveBeenCalledWith({ codigo_interno: 'CAL-MAN-001', estatus: true });
  });

  it('documento encontrado → 200 con { success: true, data }', async () => {
    findOneSpy.mockResolvedValue(mockDoc() as never);

    const res = await request(app).get('/documento/1');

    expect(res.status).toBe(200);
    expect(res.body.success).toBe(true);
    expect(res.body.data).toBeDefined();
  });

  it('documento no encontrado (findOne retorna null) → 404', async () => {
    findOneSpy.mockResolvedValue(null as never);

    const res = await request(app).get('/documento/999');

    expect(res.status).toBe(404);
    expect(res.body.success).toBe(false);
    expect(res.body.mensaje).toMatch(/no se encontró/i);
  });

  it('documento con estatus:false invisible (filtro de borrado lógico) → 404', async () => {
    // El filtro incluye estatus:true en la query, así que un doc con estatus:false
    // no lo devuelve MongoDB → findOne retorna null → 404
    findOneSpy.mockResolvedValue(null as never);

    const res = await request(app).get('/documento/CAL-MAN-BORRADO');

    expect(res.status).toBe(404);
    // Verificar que el filtro de búsqueda incluyó estatus:true
    const filtro = findOneSpy.mock.calls[0]?.[0] as Record<string, unknown>;
    expect(filtro['estatus']).toBe(true);
  });

  it('error de base de datos → 500', async () => {
    findOneSpy.mockRejectedValue(new Error('MongoDB no disponible'));

    const res = await request(app).get('/documento/1');

    expect(res.status).toBe(500);
    expect(res.body.success).toBe(false);
    expect(res.body.mensaje).toMatch(/error/i);
  });

  it('id con letras y números (alfanumérico) no se trata como numérico', async () => {
    findOneSpy.mockResolvedValue(mockDoc() as never);

    await request(app).get('/documento/DOC-123');

    const filtro = findOneSpy.mock.calls[0]?.[0] as Record<string, unknown>;
    // Debe buscar por codigo_interno, NO por id_documento_sql
    expect(filtro).toHaveProperty('codigo_interno', 'DOC-123');
    expect(filtro).not.toHaveProperty('id_documento_sql');
  });
});
