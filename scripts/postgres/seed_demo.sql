-- ==========================================================
-- SIGD EMPRESARIAL — SEED DATA DE DEMOSTRACIÓN
-- Módulo de Reportes (PostgreSQL)
-- Inserta datos de muestra realistas para que el Dashboard
-- y el Portal Operario muestren información visual.
-- ==========================================================

-- ── Departamentos adicionales ──────────────────────────────
INSERT INTO departamento (id_departamento, nombre, abreviatura, estatus, id_usuario_creacion)
VALUES
    (2, 'Recursos Humanos',       'RH',  TRUE, 1),
    (3, 'Producción',             'PRD', TRUE, 1),
    (4, 'Calidad',                'CAL', TRUE, 1),
    (5, 'Mantenimiento',          'MNT', TRUE, 1),
    (6, 'Sistemas e Informática', 'TI',  TRUE, 1)
ON CONFLICT (id_departamento) DO NOTHING;

-- ── Tipos de documento adicionales ────────────────────────
INSERT INTO tipo_documento (id_tipo, nombre, abreviatura, estatus, id_usuario_creacion)
VALUES
    (5, 'Política',       'POL', TRUE, 1),
    (6, 'Especificación', 'ESP', TRUE, 1),
    (7, 'Registro',       'REG', TRUE, 1)
ON CONFLICT (id_tipo) DO NOTHING;

-- ── Usuarios de muestra ────────────────────────────────────
INSERT INTO usuario (id_usuario, id_departamento, nombre, apellido_p, correo, contrasena, estatus, id_usuario_creacion)
VALUES
    (2, 2, 'María',  'García',    'maria.garcia@sigd.local',  UPPER(encode(digest('Admin@SIGD2026!','sha256'),'hex')), TRUE, 1),
    (3, 3, 'Carlos', 'Ramírez',   'carlos.ramirez@sigd.local', UPPER(encode(digest('Admin@SIGD2026!','sha256'),'hex')), TRUE, 1),
    (4, 4, 'Ana',    'Martínez',  'ana.martinez@sigd.local',   UPPER(encode(digest('Admin@SIGD2026!','sha256'),'hex')), TRUE, 1),
    (5, 5, 'Jorge',  'López',     'jorge.lopez@sigd.local',    UPPER(encode(digest('Admin@SIGD2026!','sha256'),'hex')), TRUE, 1),
    (6, 6, 'Laura',  'Hernández', 'laura.hernandez@sigd.local',UPPER(encode(digest('Admin@SIGD2026!','sha256'),'hex')), TRUE, 1)
ON CONFLICT (id_usuario) DO NOTHING;

-- ── Documentos Vigentes (los que aparecen en el Dashboard) ─
INSERT INTO documento_vigente
    (id_documento, codigo_interno, titulo, id_tipo, id_departamento, version_actual,
     fecha_publicacion, ruta_archivo_descarga, hash_verificacion, estatus, id_usuario_creacion)
VALUES
    -- Administración
    (1, 'ADM-POL-001', 'Política de Seguridad de la Información',            5, 1, 2,
     NOW() - INTERVAL '11 months', 'gridfs://mock-id-001', 'HASH001', TRUE, 1),
    (2, 'ADM-MAN-001', 'Manual de Organización y Funciones',                  2, 1, 1,
     NOW() - INTERVAL '9 months',  'gridfs://mock-id-002', 'HASH002', TRUE, 1),
    (3, 'ADM-PROC-001','Procedimiento de Auditorías Internas',                1, 1, 3,
     NOW() - INTERVAL '6 months',  'gridfs://mock-id-003', 'HASH003', TRUE, 1),
    -- Recursos Humanos
    (4, 'RH-PROC-001', 'Procedimiento de Reclutamiento y Selección',          1, 2, 1,
     NOW() - INTERVAL '10 months', 'gridfs://mock-id-004', 'HASH004', TRUE, 1),
    (5, 'RH-PROC-002', 'Procedimiento de Evaluación de Desempeño',            1, 2, 2,
     NOW() - INTERVAL '5 months',  'gridfs://mock-id-005', 'HASH005', TRUE, 1),
    (6, 'RH-FMT-001',  'Formato de Solicitud de Vacaciones',                  3, 2, 1,
     NOW() - INTERVAL '8 months',  'gridfs://mock-id-006', 'HASH006', TRUE, 1),
    -- Producción
    (7, 'PRD-INS-001', 'Instructivo de Operación de Línea A',                 4, 3, 1,
     NOW() - INTERVAL '7 months',  'gridfs://mock-id-007', 'HASH007', TRUE, 1),
    (8, 'PRD-INS-002', 'Instructivo de Operación de Línea B',                 4, 3, 2,
     NOW() - INTERVAL '4 months',  'gridfs://mock-id-008', 'HASH008', TRUE, 1),
    (9, 'PRD-PROC-001','Procedimiento de Control de Producción',              1, 3, 1,
     NOW() - INTERVAL '3 months',  'gridfs://mock-id-009', 'HASH009', TRUE, 1),
    (10,'PRD-ESP-001', 'Especificación Técnica de Materiales',                6, 3, 1,
     NOW() - INTERVAL '2 months',  'gridfs://mock-id-010', 'HASH010', TRUE, 1),
    -- Calidad
    (11,'CAL-MAN-001', 'Manual de Calidad ISO 9001:2015',                     2, 4, 4,
     NOW() - INTERVAL '12 months', 'gridfs://mock-id-011', 'HASH011', TRUE, 1),
    (12,'CAL-PROC-001','Procedimiento de Control de No Conformidades',        1, 4, 2,
     NOW() - INTERVAL '6 months',  'gridfs://mock-id-012', 'HASH012', TRUE, 1),
    (13,'CAL-FMT-001', 'Formato de Reporte de No Conformidad',                3, 4, 1,
     NOW() - INTERVAL '5 months',  'gridfs://mock-id-013', 'HASH013', TRUE, 1),
    -- Mantenimiento
    (14,'MNT-PROC-001','Procedimiento de Mantenimiento Preventivo',           1, 5, 1,
     NOW() - INTERVAL '9 months',  'gridfs://mock-id-014', 'HASH014', TRUE, 1),
    (15,'MNT-INS-001', 'Instructivo de Lubricación de Equipos',               4, 5, 1,
     NOW() - INTERVAL '1 month',   'gridfs://mock-id-015', 'HASH015', TRUE, 1),
    -- Sistemas
    (16,'TI-POL-001',  'Política de Uso Aceptable de Recursos Informáticos',  5, 6, 1,
     NOW() - INTERVAL '11 months', 'gridfs://mock-id-016', 'HASH016', TRUE, 1),
    (17,'TI-PROC-001', 'Procedimiento de Respaldo y Recuperación de Datos',   1, 6, 2,
     NOW() - INTERVAL '3 months',  'gridfs://mock-id-017', 'HASH017', TRUE, 1),
    (18,'TI-MAN-001',  'Manual de Usuarios del Sistema SIGD',                 2, 6, 1,
     NOW() - INTERVAL '2 months',  'gridfs://mock-id-018', 'HASH018', TRUE, 1)
ON CONFLICT (id_documento) DO NOTHING;

-- ── Acuses de lectura de muestra ───────────────────────────
-- Esto pobla el KPI de acuses y las estadísticas
INSERT INTO acuse_lectura
    (id_documento, id_usuario, direccion_ip, dispositivo_info, estatus, id_usuario_creacion)
VALUES
    (11, 1, '192.168.1.10', 'Chrome/Windows',  TRUE, 1),
    (11, 2, '192.168.1.11', 'Firefox/Windows', TRUE, 1),
    (11, 3, '192.168.1.12', 'Chrome/macOS',    TRUE, 1),
    (1,  4, '192.168.1.13', 'Edge/Windows',    TRUE, 1),
    (1,  5, '192.168.1.14', 'Chrome/Android',  TRUE, 1),
    (7,  3, '192.168.1.12', 'Chrome/macOS',    TRUE, 1),
    (7,  6, '192.168.1.15', 'Safari/iOS',      TRUE, 1),
    (4,  2, '192.168.1.11', 'Firefox/Windows', TRUE, 1),
    (14, 5, '192.168.1.14', 'Chrome/Android',  TRUE, 1),
    (16, 6, '192.168.1.15', 'Safari/iOS',      TRUE, 1),
    (16, 1, '192.168.1.10', 'Chrome/Windows',  TRUE, 1),
    (3,  4, '192.168.1.13', 'Edge/Windows',    TRUE, 1)
ON CONFLICT DO NOTHING;

-- ── Confirmación ───────────────────────────────────────────
DO $$
DECLARE
    total_docs    INT;
    total_deptos  INT;
    total_acuses  INT;
BEGIN
    SELECT COUNT(*) INTO total_docs   FROM documento_vigente WHERE estatus = true;
    SELECT COUNT(*) INTO total_deptos FROM departamento WHERE estatus = true;
    SELECT COUNT(*) INTO total_acuses FROM acuse_lectura WHERE estatus = true;
    RAISE NOTICE '============================================';
    RAISE NOTICE '  SEED DATA INSERTADO EXITOSAMENTE';
    RAISE NOTICE '  Documentos vigentes : %', total_docs;
    RAISE NOTICE '  Departamentos       : %', total_deptos;
    RAISE NOTICE '  Acuses de lectura   : %', total_acuses;
    RAISE NOTICE '============================================';
END $$;
