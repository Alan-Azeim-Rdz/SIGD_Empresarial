import request from 'supertest';
import { app, Metadato } from '../index';
import { payloadIndexarValido } from './helpers';

describe('POST /indexar', () => {
  // Mock del método save() en la instancia de Metadato
  let saveSpy: jest.SpyInstance;

  beforeEach(() => {
    // Por defecto, save() resuelve correctamente (sin errores de DB)
    saveSpy = jest.spyOn(Metadato.prototype, 'save').mockResolvedValue(undefined as never);
  });

  it('indexa correctamente con todos los campos válidos → 201', async () => {
    const res = await request(app)
      .post('/indexar')
      .send(payloadIndexarValido);

    expect(res.status).toBe(201);
    expect(res.body.success).toBe(true);
    expect(res.body.mensaje).toMatch(/indexado/i);
    expect(res.body.data).toBeDefined();
  });

  it('rechaza si falta id_documento_sql → 400', async () => {
    const { id_documento_sql: _, ...sinId } = payloadIndexarValido;
    const res = await request(app).post('/indexar').send(sinId);

    expect(res.status).toBe(400);
    expect(res.body.success).toBe(false);
    expect(res.body.mensaje).toMatch(/obligatorios/i);
  });

  it('rechaza si falta id_empresa → 400', async () => {
    const { id_empresa: _, ...sinEmpresa } = payloadIndexarValido;
    const res = await request(app).post('/indexar').send(sinEmpresa);

    expect(res.status).toBe(400);
    expect(res.body.success).toBe(false);
    expect(res.body.mensaje).toMatch(/obligatorios/i);
  });

  it('rechaza si falta codigo_interno → 400', async () => {
    const { codigo_interno: _, ...sinCodigo } = payloadIndexarValido;
    const res = await request(app).post('/indexar').send(sinCodigo);

    expect(res.status).toBe(400);
    expect(res.body.success).toBe(false);
  });

  it('rechaza si falta titulo → 400', async () => {
    const { titulo: _, ...sinTitulo } = payloadIndexarValido;
    const res = await request(app).post('/indexar').send(sinTitulo);

    expect(res.status).toBe(400);
    expect(res.body.success).toBe(false);
  });

  it('rechaza si falta id_usuario_creacion → 400', async () => {
    const { id_usuario_creacion: _, ...sinUsuario } = payloadIndexarValido;
    const res = await request(app).post('/indexar').send(sinUsuario);

    expect(res.status).toBe(400);
    expect(res.body.success).toBe(false);
  });

  it('cuando no se envía version el documento se guarda con version = "0.1"', async () => {
    const { version: _, ...sinVersion } = payloadIndexarValido;
    const res = await request(app).post('/indexar').send(sinVersion);

    expect(res.status).toBe(201);
    // El default ?? '0.1' se aplica en el constructor antes de llamar save()
    expect(res.body.data.version).toBe('0.1');
  });

  it('si el documento ya existe (código 11000 de MongoDB) → 409', async () => {
    saveSpy.mockRejectedValue({ code: 11000, message: 'duplicate key error' });

    const res = await request(app).post('/indexar').send(payloadIndexarValido);

    expect(res.status).toBe(409);
    expect(res.body.success).toBe(false);
    expect(res.body.mensaje).toMatch(/ya está indexado/i);
  });

  it('si mongoose lanza un error genérico → 500', async () => {
    saveSpy.mockRejectedValue(new Error('Conexión perdida con MongoDB'));

    const res = await request(app).post('/indexar').send(payloadIndexarValido);

    expect(res.status).toBe(500);
    expect(res.body.success).toBe(false);
    expect(res.body.mensaje).toMatch(/error interno/i);
  });

  it('body vacío devuelve 400 (todos los campos faltan)', async () => {
    const res = await request(app).post('/indexar').send({});

    expect(res.status).toBe(400);
    expect(res.body.success).toBe(false);
  });
});
