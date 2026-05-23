import { escapeRegex } from '../index';

describe('escapeRegex', () => {
  it('devuelve string vacío sin cambios', () => {
    expect(escapeRegex('')).toBe('');
  });

  it('texto plano sin metacaracteres pasa intacto', () => {
    expect(escapeRegex('calidad')).toBe('calidad');
  });

  it('escapa el punto (.)', () => {
    expect(escapeRegex('calidad.ISO')).toBe('calidad\\.ISO');
  });

  it('escapa el asterisco (*)', () => {
    expect(escapeRegex('doc*')).toBe('doc\\*');
  });

  it('escapa paréntesis y el operador + (patrón ReDoS clásico)', () => {
    expect(escapeRegex('(a+)+')).toBe('\\(a\\+\\)\\+');
  });

  it('escapa todos los metacaracteres definidos: .*+?^${}()|[]\\', () => {
    const input    = '.*+?^${}()|[]\\';
    const expected = '\\.\\*\\+\\?\\^\\$\\{\\}\\(\\)\\|\\[\\]\\\\';
    expect(escapeRegex(input)).toBe(expected);
  });

  it('texto con acento (á, é, ó) pasa intacto', () => {
    expect(escapeRegex('revisión')).toBe('revisión');
  });

  it('texto con eñe (ñ) pasa intacto', () => {
    expect(escapeRegex('gestión_año')).toBe('gestión_año');
  });

  it('combinación de texto normal y especiales', () => {
    expect(escapeRegex('ISO 9001:2015 (calidad)')).toBe('ISO 9001:2015 \\(calidad\\)');
  });

  it('signo de dólar y corchetes se escapan', () => {
    expect(escapeRegex('precio[$100]')).toBe('precio\\[\\$100\\]');
  });
});
