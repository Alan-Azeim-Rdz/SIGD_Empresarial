-- 1. Desvincular temporalmente las dependencias asignándolas a la empresa demo (1) para evitar errores de FK
UPDATE departamento SET id_empresa = 1;
UPDATE usuario SET id_empresa = 1;
UPDATE tipo_documento SET id_empresa = 1;
UPDATE documento_vigente SET id_empresa = 1;

-- 2. Eliminar las empresas obsoletas (3 y 4)
DELETE FROM empresa WHERE id_empresa IN (3, 4);

-- 3. Crear las empresas con los IDs correctos (2 y 3) alineados con SQL Server
INSERT INTO empresa (id_empresa, nombre, slug, estatus) 
VALUES 
    (2, 'TechCorp Solutions', 'techcorp', true), 
    (3, 'Grupo Innovar', 'grupoinnovar', true) 
ON CONFLICT (id_empresa) DO NOTHING; 

-- 4. Asignar departamentos a las empresas correspondientes
UPDATE departamento SET id_empresa = 2 WHERE id_departamento IN (1, 2, 6);
UPDATE departamento SET id_empresa = 3 WHERE id_departamento IN (3, 4, 5);

-- 5. Asignar usuarios a las empresas según su departamento
UPDATE usuario SET id_empresa = 2 WHERE id_departamento IN (1, 2, 6);
UPDATE usuario SET id_empresa = 3 WHERE id_departamento IN (3, 4, 5);

-- 6. Asignar tipos de documento a las empresas correspondientes
UPDATE tipo_documento SET id_empresa = 2 WHERE id_tipo IN (1, 2, 3, 5);
UPDATE tipo_documento SET id_empresa = 3 WHERE id_tipo IN (4, 6, 7);

-- 7. Asignar documentos vigentes a las empresas según su departamento
UPDATE documento_vigente SET id_empresa = 2 WHERE id_departamento IN (1, 2, 6);
UPDATE documento_vigente SET id_empresa = 3 WHERE id_departamento IN (3, 4, 5);
