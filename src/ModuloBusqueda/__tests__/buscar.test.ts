import request from 'supertest';
import { app, Metadato } from '../index';
import { mockDoc } from './helpers';

describe('GET /buscar', () => {
  let findSpy: jest.SpyInstance;

  beforeEach(() => {
    findSpy = jest.spyOn(Metadato, 'find').mockResolvedValue([] as never);
  });

  it('sin parámetro id_empresa → 400', async () => {
    const res = await request(app).get('/buscar').query({ q: 'test' });

    expect(res.status).toBe(400);
    expect(res.body.success).toBe(false);
    expect(res.body.mensaje).toMatch(/id_empresa/i);
  });

  it('sin parámetro q → 400', async () => {
    const res = await request(app).get('/buscar').query({ id_empresa: 1 });

    expect(res.status).toBe(400);
    expect(res.body.success).toBe(false);
    expect(res.body.mensaje).toMatch(/término de búsqueda/i);
  });

  it('q con solo espacios (trim → vacío) → 400', async () => {
    const res = await request(app).get('/buscar').query({ q: '   ', id_empresa: 1 });

    expect(res.status).toBe(400);
    expect(res.body.success).toBe(false);
  });

  it('q con más de 100 caracteres → 400 con mensaje específico', async () => {
    const q101 = 'a'.repeat(101);
    const res  = await request(app).get('/buscar').query({ q: q101, id_empresa: 1 });

    expect(res.status).toBe(400);
    expect(res.body.mensaje).toMatch(/demasiado largo/i);
  });

  it('respuesta exitosa tiene la forma { success, total, data[] }', async () => {
    findSpy.mockResolvedValue([mockDoc()] as never);

    const res = await request(app).get('/buscar').query({ q: 'calidad', id_empresa: 1 });

    expect(res.status).toBe(200);
    expect(res.body).toMatchObject({
      success: true,
      total:   expect.any(Number),
      data:    expect.any(Array),
    });
  });

  it('total coincide con la longitud de data', async () => {
    const docs = [mockDoc(), mockDoc({ id_documento_sql: 2, codigo_interno: 'CAL-MAN-002' })];
    findSpy.mockResolvedValue(docs as never);

    const res = await request(app).get('/buscar').query({ q: 'calidad', id_empresa: 1 });

    expect(res.body.total).toBe(2);
    expect(res.body.data).toHaveLength(2);
    expect(res.body.total).toBe(res.body.data.length);
  });

  it('q con caracteres regex especiales aplica escapeRegex antes de buscar', async () => {
    const res = await request(app).get('/buscar').query({ q: 'calidad.ISO', id_empresa: 1 });

    expect(res.status).toBe(200);
    // El primer argumento de find() debe contener el regex escapado
    const filtro = findSpy.mock.calls[0]?.[0] as Record<string, unknown>;
    const orClause = filtro['$or'] as Array<Record<string, unknown>>;
    const tituloFiltro = orClause[0]?.['titulo'] as Record<string, string>;
    expect(tituloFiltro?.['$regex']).toBe('calidad\\.ISO');
  });

  it('el filtro de búsqueda incluye siempre estatus: true y id_empresa', async () => {
    await request(app).get('/buscar').query({ q: 'test', id_empresa: 1 });

    const filtro = findSpy.mock.calls[0]?.[0] as Record<string, unknown>;
    expect(filtro['estatus']).toBe(true);
    expect(filtro['id_empresa']).toBe(1);
  });

  it('el filtro de búsqueda excluye versiones de borrador/inestables', async () => {
    await request(app).get('/buscar').query({ q: 'test', id_empresa: 1 });

    const filtro = findSpy.mock.calls[0]?.[0] as Record<string, any>;
    expect(filtro).toHaveProperty('version');
    expect(filtro.version).toEqual({ $not: /\.([1-9]\d*)$/ });
  });

  it('si Metadato.find lanza un error → 500', async () => {
    findSpy.mockRejectedValue(new Error('timeout de MongoDB'));

    const res = await request(app).get('/buscar').query({ q: 'calidad', id_empresa: 1 });

    expect(res.status).toBe(500);
    expect(res.body.success).toBe(false);
    expect(res.body.mensaje).toMatch(/buscar/i);
  });

  it('q con exactamente 100 caracteres es aceptado → 200', async () => {
    const q100 = 'a'.repeat(100);
    const res  = await request(app).get('/buscar').query({ q: q100, id_empresa: 1 });

    expect(res.status).toBe(200);
  });
});
