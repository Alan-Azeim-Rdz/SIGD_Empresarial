-- ==========================================================
-- 1. CREACIÓN DE TABLAS BASE CON CAMPOS DE AUDITORÍA
-- ==========================================================

CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE TABLE departamento (
    id_departamento INT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    abreviatura VARCHAR(20),
    
    -- Auditoría y Borrado Lógico
    estatus BOOLEAN DEFAULT TRUE,
    fecha_creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    id_usuario_creacion INT,
    fecha_modificacion TIMESTAMP,
    id_usuario_modificacion INT,
    fecha_eliminacion TIMESTAMP,
    id_usuario_eliminacion INT
);

CREATE TABLE usuario (
    id_usuario INT PRIMARY KEY,
    id_departamento INT REFERENCES departamento(id_departamento),
    nombre VARCHAR(100) NOT NULL,
    apellido_p VARCHAR(100) NOT NULL,
    correo VARCHAR(150) UNIQUE NOT NULL,
    contrasena VARCHAR(255) NOT NULL,
    
    -- Auditoría y Borrado Lógico
    estatus BOOLEAN DEFAULT TRUE,
    fecha_creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    id_usuario_creacion INT,
    fecha_modificacion TIMESTAMP,
    id_usuario_modificacion INT,
    fecha_eliminacion TIMESTAMP,
    id_usuario_eliminacion INT
);

CREATE TABLE tipo_documento (
    id_tipo INT PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL,
    abreviatura VARCHAR(10),
    
    -- Auditoría y Borrado Lógico
    estatus BOOLEAN DEFAULT TRUE,
    fecha_creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    id_usuario_creacion INT,
    fecha_modificacion TIMESTAMP,
    id_usuario_modificacion INT,
    fecha_eliminacion TIMESTAMP,
    id_usuario_eliminacion INT
);

CREATE TABLE documento_vigente (
    id_documento INT PRIMARY KEY,
    codigo_interno VARCHAR(50) UNIQUE NOT NULL,
    titulo VARCHAR(255) NOT NULL,
    id_tipo INT REFERENCES tipo_documento(id_tipo),
    id_departamento INT REFERENCES departamento(id_departamento),
    version_actual INT NOT NULL,
    fecha_publicacion TIMESTAMP NOT NULL,
    ruta_archivo_descarga VARCHAR(500) NOT NULL,
    hash_verificacion VARCHAR(255),
    
    -- Auditoría y Borrado Lógico
    estatus BOOLEAN DEFAULT TRUE,
    fecha_creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    id_usuario_creacion INT,
    fecha_modificacion TIMESTAMP,
    id_usuario_modificacion INT,
    fecha_eliminacion TIMESTAMP,
    id_usuario_eliminacion INT
);

-- Índice para búsqueda rápida
CREATE INDEX idx_busqueda_documento ON documento_vigente (codigo_interno, id_departamento);

CREATE TABLE acuse_lectura (
    id_acuse SERIAL PRIMARY KEY,
    id_documento INT REFERENCES documento_vigente(id_documento),
    id_usuario INT REFERENCES usuario(id_usuario),
    fecha_lectura TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- Momento exacto del acuse (auditoría ISO)
    direccion_ip VARCHAR(50),
    dispositivo_info TEXT,
    
    -- Auditoría (Al ser un registro transaccional, rara vez se edita, pero mantenemos el estándar)
    estatus BOOLEAN DEFAULT TRUE,
    fecha_creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    id_usuario_creacion INT,
    fecha_modificacion TIMESTAMP,
    id_usuario_modificacion INT,
    fecha_eliminacion TIMESTAMP,
    id_usuario_eliminacion INT
);

CREATE TABLE reporte_descarga (
    id_descarga SERIAL PRIMARY KEY,
    id_documento INT REFERENCES documento_vigente(id_documento),
    id_usuario INT REFERENCES usuario(id_usuario),
    
    -- Auditoría
    estatus BOOLEAN DEFAULT TRUE,
    fecha_creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    id_usuario_creacion INT,
    fecha_modificacion TIMESTAMP,
    id_usuario_modificacion INT,
    fecha_eliminacion TIMESTAMP,
    id_usuario_eliminacion INT
);

-- Se corrigió la relación: ahora hace referencia estricta a documento_vigente
CREATE TABLE estadistica_documento (
    id_documento INT PRIMARY KEY REFERENCES documento_vigente(id_documento),
    total_vistas INT DEFAULT 0,
    ultima_consulta TIMESTAMP,
    
    -- Auditoría
    estatus BOOLEAN DEFAULT TRUE,
    fecha_creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    id_usuario_creacion INT,
    fecha_modificacion TIMESTAMP,
    id_usuario_modificacion INT,
    fecha_eliminacion TIMESTAMP,
    id_usuario_eliminacion INT
);


-- ==========================================================
-- 2. RESTRICCIONES DE LLAVES FORÁNEAS (AUDITORÍA)
-- ==========================================================
-- Aplicamos las relaciones de los usuarios de auditoría al final para evitar errores circulares.

ALTER TABLE departamento 
    ADD CONSTRAINT fk_depto_usu_crea FOREIGN KEY (id_usuario_creacion) REFERENCES usuario(id_usuario),
    ADD CONSTRAINT fk_depto_usu_mod FOREIGN KEY (id_usuario_modificacion) REFERENCES usuario(id_usuario),
    ADD CONSTRAINT fk_depto_usu_elim FOREIGN KEY (id_usuario_eliminacion) REFERENCES usuario(id_usuario);

ALTER TABLE usuario 
    ADD CONSTRAINT fk_usu_usu_crea FOREIGN KEY (id_usuario_creacion) REFERENCES usuario(id_usuario),
    ADD CONSTRAINT fk_usu_usu_mod FOREIGN KEY (id_usuario_modificacion) REFERENCES usuario(id_usuario),
    ADD CONSTRAINT fk_usu_usu_elim FOREIGN KEY (id_usuario_eliminacion) REFERENCES usuario(id_usuario);

ALTER TABLE tipo_documento 
    ADD CONSTRAINT fk_tipo_usu_crea FOREIGN KEY (id_usuario_creacion) REFERENCES usuario(id_usuario),
    ADD CONSTRAINT fk_tipo_usu_mod FOREIGN KEY (id_usuario_modificacion) REFERENCES usuario(id_usuario),
    ADD CONSTRAINT fk_tipo_usu_elim FOREIGN KEY (id_usuario_eliminacion) REFERENCES usuario(id_usuario);

ALTER TABLE documento_vigente 
    ADD CONSTRAINT fk_doc_usu_crea FOREIGN KEY (id_usuario_creacion) REFERENCES usuario(id_usuario),
    ADD CONSTRAINT fk_doc_usu_mod FOREIGN KEY (id_usuario_modificacion) REFERENCES usuario(id_usuario),
    ADD CONSTRAINT fk_doc_usu_elim FOREIGN KEY (id_usuario_eliminacion) REFERENCES usuario(id_usuario);

ALTER TABLE acuse_lectura 
    ADD CONSTRAINT fk_acuse_usu_crea FOREIGN KEY (id_usuario_creacion) REFERENCES usuario(id_usuario),
    ADD CONSTRAINT fk_acuse_usu_mod FOREIGN KEY (id_usuario_modificacion) REFERENCES usuario(id_usuario),
    ADD CONSTRAINT fk_acuse_usu_elim FOREIGN KEY (id_usuario_eliminacion) REFERENCES usuario(id_usuario);

ALTER TABLE reporte_descarga 
    ADD CONSTRAINT fk_descarga_usu_crea FOREIGN KEY (id_usuario_creacion) REFERENCES usuario(id_usuario),
    ADD CONSTRAINT fk_descarga_usu_mod FOREIGN KEY (id_usuario_modificacion) REFERENCES usuario(id_usuario),
    ADD CONSTRAINT fk_descarga_usu_elim FOREIGN KEY (id_usuario_eliminacion) REFERENCES usuario(id_usuario);

ALTER TABLE estadistica_documento 
    ADD CONSTRAINT fk_est_usu_crea FOREIGN KEY (id_usuario_creacion) REFERENCES usuario(id_usuario),
    ADD CONSTRAINT fk_est_usu_mod FOREIGN KEY (id_usuario_modificacion) REFERENCES usuario(id_usuario),
    ADD CONSTRAINT fk_est_usu_elim FOREIGN KEY (id_usuario_eliminacion) REFERENCES usuario(id_usuario);


-- ==========================================================
-- 3. FUNCIONES Y TRIGGERS AUTOMÁTICOS
-- ==========================================================

-- A) Trigger Genérico para actualizar automáticamente "fecha_modificacion" al hacer un UPDATE
CREATE OR REPLACE FUNCTION trg_set_fecha_modificacion()
RETURNS TRIGGER AS $$
BEGIN
    NEW.fecha_modificacion = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_departamento_mod BEFORE UPDATE ON departamento FOR EACH ROW EXECUTE FUNCTION trg_set_fecha_modificacion();
CREATE TRIGGER trg_usuario_mod BEFORE UPDATE ON usuario FOR EACH ROW EXECUTE FUNCTION trg_set_fecha_modificacion();
CREATE TRIGGER trg_tipo_documento_mod BEFORE UPDATE ON tipo_documento FOR EACH ROW EXECUTE FUNCTION trg_set_fecha_modificacion();
CREATE TRIGGER trg_documento_vigente_mod BEFORE UPDATE ON documento_vigente FOR EACH ROW EXECUTE FUNCTION trg_set_fecha_modificacion();
CREATE TRIGGER trg_estadistica_mod BEFORE UPDATE ON estadistica_documento FOR EACH ROW EXECUTE FUNCTION trg_set_fecha_modificacion();

-- B) Trigger para actualizar la estadística cuando hay un acuse de lectura
CREATE OR REPLACE FUNCTION trg_actualizar_estadistica()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO estadistica_documento (id_documento, total_vistas, ultima_consulta, id_usuario_creacion)
    VALUES (NEW.id_documento, 1, CURRENT_TIMESTAMP, NEW.id_usuario_creacion)
    ON CONFLICT (id_documento) DO UPDATE
    SET total_vistas = estadistica_documento.total_vistas + 1,
        ultima_consulta = EXCLUDED.ultima_consulta,
        id_usuario_modificacion = NEW.id_usuario_creacion; -- Quién causó el cambio en la estadística
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_acuse_lectura
AFTER INSERT ON acuse_lectura
FOR EACH ROW EXECUTE FUNCTION trg_actualizar_estadistica();

-- C) Función para Reporte
CREATE OR REPLACE FUNCTION sp_reporte_cumplimiento_depto(depto_id INT)
RETURNS TABLE (
    documento VARCHAR,
    total_lecturas BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT d.titulo, COUNT(a.id_acuse)
    FROM documento_vigente d
    LEFT JOIN acuse_lectura a ON d.id_documento = a.id_documento
    WHERE d.id_departamento = depto_id AND d.estatus = TRUE -- Solo documentos no eliminados
    GROUP BY d.titulo;
END;
$$ LANGUAGE plpgsql;

-- D) Funciones de Autenticación (Hasheo en BD)
CREATE OR REPLACE FUNCTION fn_crear_usuario(
    p_id_usuario INT,
    p_id_departamento INT,
    p_nombre VARCHAR,
    p_apellido_p VARCHAR,
    p_correo VARCHAR,
    p_contrasena_plana VARCHAR,
    p_id_usuario_creacion INT
) RETURNS VOID AS $$
BEGIN
    INSERT INTO usuario (id_usuario, id_departamento, nombre, apellido_p, correo, contrasena, id_usuario_creacion)
    VALUES (
        p_id_usuario, 
        p_id_departamento, 
        p_nombre, 
        p_apellido_p, 
        p_correo, 
        UPPER(encode(digest(p_contrasena_plana, 'sha256'), 'hex')), 
        p_id_usuario_creacion
    );
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_validar_login(
    p_correo VARCHAR,
    p_contrasena_plana VARCHAR
) RETURNS TABLE (
    id_usuario INT,
    id_departamento INT,
    nombre VARCHAR,
    apellido_p VARCHAR,
    correo VARCHAR
) AS $$
BEGIN
    RETURN QUERY
    SELECT u.id_usuario, u.id_departamento, u.nombre, u.apellido_p, u.correo
    FROM usuario u
    WHERE u.correo = p_correo
      AND u.contrasena = UPPER(encode(digest(p_contrasena_plana, 'sha256'), 'hex'))
      AND u.estatus = TRUE;
END;
$$ LANGUAGE plpgsql;


-- ==========================================================
-- 4. DATOS SEMILLA (SEED DATA)
-- Espejo del usuario Super Admin del módulo central para
-- mantener consistencia entre las bases de datos.
-- ==========================================================

-- 4.1 Departamento de Administración
INSERT INTO departamento (id_departamento, nombre, abreviatura, estatus)
VALUES (1, 'Administración General', 'ADM', TRUE)
ON CONFLICT (id_departamento) DO NOTHING;

-- 4.2 Usuario Super Admin (espejo del módulo central)
INSERT INTO usuario (id_usuario, id_departamento, nombre, apellido_p, correo, contrasena, estatus)
VALUES (1, 1, 'Super', 'Administrador', 'admin@sigd.local', UPPER(encode(digest('Admin@SIGD2026!', 'sha256'), 'hex')), TRUE)
ON CONFLICT (id_usuario) DO NOTHING;

-- 4.3 Actualizar auditoría del departamento
UPDATE departamento SET id_usuario_creacion = 1 WHERE id_departamento = 1 AND id_usuario_creacion IS NULL;

-- 4.4 Tipo de documento base
INSERT INTO tipo_documento (id_tipo, nombre, abreviatura, estatus, id_usuario_creacion)
VALUES (1, 'Procedimiento', 'PROC', TRUE, 1)
ON CONFLICT (id_tipo) DO NOTHING;

INSERT INTO tipo_documento (id_tipo, nombre, abreviatura, estatus, id_usuario_creacion)
VALUES (2, 'Manual', 'MAN', TRUE, 1)
ON CONFLICT (id_tipo) DO NOTHING;

INSERT INTO tipo_documento (id_tipo, nombre, abreviatura, estatus, id_usuario_creacion)
VALUES (3, 'Formato', 'FMT', TRUE, 1)
ON CONFLICT (id_tipo) DO NOTHING;

INSERT INTO tipo_documento (id_tipo, nombre, abreviatura, estatus, id_usuario_creacion)
VALUES (4, 'Instructivo', 'INS', TRUE, 1)
ON CONFLICT (id_tipo) DO NOTHING;

-- Actualizar auditoría del usuario (se creó a sí mismo)
UPDATE usuario SET id_usuario_creacion = 1 WHERE id_usuario = 1 AND id_usuario_creacion IS NULL;