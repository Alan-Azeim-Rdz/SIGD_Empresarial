-- ── TABLA: DocumentosVigentes ─────────────────────
CREATE TABLE DocumentosVigentes (
    id               SERIAL PRIMARY KEY,
    id_documento_central INT NOT NULL,
    titulo           VARCHAR(255) NOT NULL,
    version          VARCHAR(20)  NOT NULL,
    estado           VARCHAR(50)  DEFAULT 'Aprobado',
    extension        VARCHAR(10)  NOT NULL,
    ruta_archivo     VARCHAR(500),
    fecha_aprobacion DATE         NOT NULL,
    fecha_publicacion TIMESTAMP   DEFAULT NOW(),
    id_tipo_documento INT,
    descripcion      TEXT
);

CREATE TABLE Reportes (
    id               SERIAL PRIMARY KEY,
    tipo             VARCHAR(100) NOT NULL,
    fecha_generacion TIMESTAMP    DEFAULT NOW(),
    total_documentos INT          DEFAULT 0,
    generado_por     VARCHAR(100),
    contenido        TEXT
);

CREATE INDEX idx_documentos_titulo  ON DocumentosVigentes(titulo);
CREATE INDEX idx_documentos_estado  ON DocumentosVigentes(estado);
CREATE INDEX idx_reportes_fecha     ON Reportes(fecha_generacion);

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
    SELECT d.id, d.titulo, d.version, d.estado, d.extension, d.fecha_aprobacion
    FROM DocumentosVigentes d
    WHERE d.titulo ILIKE '%' || termino || '%'
       OR d.descripcion ILIKE '%' || termino || '%'
    ORDER BY d.titulo;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION actualizar_total_reporte()
RETURNS TRIGGER AS $$
BEGIN
    NEW.total_documentos := (SELECT COUNT(*) FROM DocumentosVigentes WHERE estado = 'Aprobado');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_actualizar_total
    BEFORE INSERT ON Reportes
    FOR EACH ROW
    EXECUTE FUNCTION actualizar_total_reporte();

CREATE VIEW vista_documentos_vigentes AS
SELECT id, titulo, version, extension, fecha_aprobacion, estado
FROM DocumentosVigentes
WHERE estado = 'Aprobado'
ORDER BY titulo;