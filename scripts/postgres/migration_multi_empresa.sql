-- 1. Crear tabla empresa
CREATE TABLE IF NOT EXISTS empresa (
    id_empresa INT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    slug VARCHAR(50) UNIQUE NOT NULL,
    estatus BOOLEAN DEFAULT TRUE,
    fecha_creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Insertar Empresa Demo y Empresas Base
INSERT INTO empresa (id_empresa, nombre, slug, estatus)
VALUES 
    (1, 'Empresa Demo', 'demo', TRUE),
    (2, 'TechCorp Solutions', 'techcorp', TRUE),
    (3, 'Grupo Innovar', 'grupoinnovar', TRUE)
ON CONFLICT (id_empresa) DO NOTHING;

-- 2. Modificar departamento
ALTER TABLE departamento ADD COLUMN IF NOT EXISTS id_empresa INT REFERENCES empresa(id_empresa);
UPDATE departamento SET id_empresa = 1 WHERE id_departamento <> 1;

-- 3. Modificar usuario
ALTER TABLE usuario ADD COLUMN IF NOT EXISTS id_empresa INT REFERENCES empresa(id_empresa);
UPDATE usuario SET id_empresa = 1 WHERE id_usuario <> 1;

-- 4. Modificar tipo_documento
ALTER TABLE tipo_documento ADD COLUMN IF NOT EXISTS id_empresa INT REFERENCES empresa(id_empresa);
UPDATE tipo_documento SET id_empresa = 1;

-- 5. Modificar documento_vigente
ALTER TABLE documento_vigente ADD COLUMN IF NOT EXISTS id_empresa INT REFERENCES empresa(id_empresa);
UPDATE documento_vigente SET id_empresa = 1;
