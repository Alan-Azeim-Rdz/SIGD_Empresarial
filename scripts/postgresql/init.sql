-- ═══════════════════════════════════════════════════════
-- SCRIPT PostgreSQL — Módulo Consulta Pública
-- Proyecto: SIGD Empresarial
-- Autor: Josue J.A.V.
-- ═══════════════════════════════════════════════════════

-- ── 1. CREAR BASE DE DATOS ────────────────────────────
CREATE DATABASE sigd_consulta;

-- Conectarse a la base de datos
\c sigd_consulta;

-- ── 2. TABLA: DocumentosVigentes ─────────────────────
-- Solo documentos que ya fueron aprobados en el módulo .NET
CREATE TABLE DocumentosVigentes (
    id               SERIAL PRIMARY KEY,
    id_documento_central INT NOT NULL,        -- ID del documento en SQL Server
    titulo           VARCHAR(255) NOT NULL,
    version          VARCHAR(20)  NOT NULL,
    estado           VARCHAR(50)  DEFAULT 'Aprobado',
    extension        VARCHAR(10)  NOT NULL,   -- pdf, docx, etc.
    ruta_archivo     VARCHAR(500),            -- ruta donde está guardado
    fecha_aprobacion DATE         NOT NULL,
    fecha_publicacion TIMESTAMP   DEFAULT NOW(),
    id_tipo_documento INT,
    descripcion      TEXT
);

-- ── 3. TABLA: Reportes ────────────────────────────────
-- Guarda un registro de cada reporte generado
CREATE TABLE Reportes (
    id               SERIAL PRIMARY KEY,
    tipo             VARCHAR(100) NOT NULL,   -- 'cumplimiento', 'auditoria', etc.
    fecha_generacion TIMESTAMP    DEFAULT NOW(),
    total_documentos INT          DEFAULT 0,
    generado_por     VARCHAR(100),            -- nombre del usuario que lo generó
    contenido        TEXT                     -- resumen del reporte en texto
);

-- ── 4. ÍNDICES (para búsquedas más rápidas) ───────────
CREATE INDEX idx_documentos_titulo  ON DocumentosVigentes(titulo);
CREATE INDEX idx_documentos_estado  ON DocumentosVigentes(estado);
CREATE INDEX idx_reportes_fecha     ON Reportes(fecha_generacion);

-- ── 5. DATOS DE EJEMPLO ───────────────────────────────
INSERT INTO DocumentosVigentes 
    (id_documento_central, titulo, version, estado, extension, fecha_aprobacion, descripcion)
VALUES
    (1, 'Manual de Calidad ISO 9001',      '2.0', 'Aprobado', 'pdf', '2024-01-15', 'Manual principal del sistema de calidad'),
    (2, 'Procedimiento de Auditoría',      '1.5', 'Aprobado', 'pdf', '2024-02-20', 'Procedimiento para auditorías internas'),
    (3, 'Política de Calidad',             '3.0', 'Aprobado', 'pdf', '2024-03-10', 'Política general de calidad empresarial'),
    (4, 'Manual de Procedimientos',        '1.0', 'Aprobado', 'docx','2024-04-05', 'Manual de procedimientos operativos'),
    (5, 'Reglamento Interno de Trabajo',   '2.1', 'Aprobado', 'pdf', '2024-05-12', 'Reglamento interno para empleados');

INSERT INTO Reportes
    (tipo, total_documentos, generado_por, contenido)
VALUES
    ('cumplimiento', 5, 'Admin', 'Reporte inicial del sistema con 5 documentos vigentes');

-- ── 6. STORED PROCEDURE: Buscar Documentos ────────────
CREATE OR REPLACE FUNCTION buscar_documentos(termino VARCHAR)
RETURNS TABLE (
    id               INT,
    titulo           VARCHAR,
    version          VARCHAR,
    estado           VARCHAR,
    extension        VARCHAR,
    fecha_aprobacion DATE
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        d.id,
        d.titulo,
        d.version,
        d.estado,
        d.extension,
        d.fecha_aprobacion
    FROM DocumentosVigentes d
    WHERE d.titulo ILIKE '%' || termino || '%'
       OR d.descripcion ILIKE '%' || termino || '%'
    ORDER BY d.titulo;
END;
$$ LANGUAGE plpgsql;

-- ── 7. TRIGGER: Registrar reporte automáticamente ─────
-- Cada vez que se inserta un reporte, actualiza el total
CREATE OR REPLACE FUNCTION actualizar_total_reporte()
RETURNS TRIGGER AS $$
BEGIN
    -- Contamos cuántos documentos vigentes hay actualmente
    NEW.total_documentos := (SELECT COUNT(*) FROM DocumentosVigentes WHERE estado = 'Aprobado');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_actualizar_total
    BEFORE INSERT ON Reportes
    FOR EACH ROW
    EXECUTE FUNCTION actualizar_total_reporte();

-- ── 8. VISTA: Documentos vigentes resumida ────────────
CREATE VIEW vista_documentos_vigentes AS
SELECT
    id,
    titulo,
    version,
    extension,
    fecha_aprobacion,
    estado
FROM DocumentosVigentes
WHERE estado = 'Aprobado'
ORDER BY titulo;