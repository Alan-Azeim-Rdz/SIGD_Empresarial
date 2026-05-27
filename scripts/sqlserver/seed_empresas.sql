USE [SIGD_Central];
GO

-- ============================================================
-- SEED: 2 EMPRESAS DEMO + TRIGGERS DE AUDITORÍA DE LOGIN
-- ============================================================

PRINT '========================================'
PRINT '  INICIANDO SEED DE EMPRESAS Y TRIGGERS'
PRINT '========================================'

-- ============================================================
-- SECCIÓN 1: ROL ADMINISTRADOR (necesario para empresas demo)
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[Rol] WHERE Nombre = 'Administrador' AND Estatus = 1)
BEGIN
    INSERT INTO [dbo].[Rol] (Nombre, Descripcion, IdUsuarioCreacion, FechaCreacion, Estatus)
    VALUES (N'Administrador', N'Administrador de la empresa. Gestiona usuarios, roles, departamentos y documentos.', 1, GETDATE(), 1);
    PRINT '  ✓ Rol Administrador creado.';
END
ELSE
    PRINT '  → Rol Administrador ya existe.';
GO

-- ============================================================
-- SECCIÓN 2: EMPRESA 1 - TechCorp Solutions
-- ============================================================

-- 2.1 Crear Empresa 1
IF NOT EXISTS (SELECT 1 FROM [dbo].[Empresa] WHERE Slug = 'techcorp')
BEGIN
    INSERT INTO [dbo].[Empresa] (Nombre, Slug, RFC, CorreoContacto, FechaRegistro, Estatus)
    VALUES (N'TechCorp Solutions', N'techcorp', N'TCS123456789', N'contacto@techcorp.local', GETDATE(), 1);
    PRINT '  ✓ Empresa TechCorp Solutions creada.';
END
ELSE
    PRINT '  → Empresa TechCorp ya existe.';
GO

-- 2.2 Departamentos para TechCorp
DECLARE @EmpTech INT = (SELECT Id FROM [dbo].[Empresa] WHERE Slug = 'techcorp');

IF NOT EXISTS (SELECT 1 FROM [dbo].[Departamento] WHERE Nombre = 'Administración' AND IdEmpresa = @EmpTech)
BEGIN
    INSERT INTO [dbo].[Departamento] (Nombre, Abreviatura, Estatus, FechaCreacion, IdEmpresa, IdUsuarioCreacion)
    VALUES (N'Administración', N'ADM', 1, GETDATE(), @EmpTech, 1);
    PRINT '  ✓ Departamento Administración (TechCorp) creado.';
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Departamento] WHERE Nombre = 'Tecnología de Información' AND IdEmpresa = @EmpTech)
BEGIN
    INSERT INTO [dbo].[Departamento] (Nombre, Abreviatura, Estatus, FechaCreacion, IdEmpresa, IdUsuarioCreacion)
    VALUES (N'Tecnología de Información', N'TI', 1, GETDATE(), @EmpTech, 1);
    PRINT '  ✓ Departamento TI (TechCorp) creado.';
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Departamento] WHERE Nombre = 'Recursos Humanos' AND IdEmpresa = @EmpTech)
BEGIN
    INSERT INTO [dbo].[Departamento] (Nombre, Abreviatura, Estatus, FechaCreacion, IdEmpresa, IdUsuarioCreacion)
    VALUES (N'Recursos Humanos', N'RRHH', 1, GETDATE(), @EmpTech, 1);
    PRINT '  ✓ Departamento RRHH (TechCorp) creado.';
END
GO

-- 2.3 Tipo de documentos para TechCorp
DECLARE @EmpTech INT = (SELECT Id FROM [dbo].[Empresa] WHERE Slug = 'techcorp');
IF NOT EXISTS (SELECT 1 FROM [dbo].[TipoDocumento] WHERE Nombre = 'Manual Técnico' AND IdEmpresa = @EmpTech)
BEGIN
    INSERT INTO [dbo].[TipoDocumento] (Nombre, Abreviatura, TiempoRetencionMeses, Estatus, FechaCreacion, IdEmpresa, IdUsuarioCreacion)
    VALUES 
    (N'Manual Técnico', N'MT', 60, 1, GETDATE(), @EmpTech, 1),
    (N'Política Interna', N'PI', 36, 1, GETDATE(), @EmpTech, 1),
    (N'Contrato', N'CON', 84, 1, GETDATE(), @EmpTech, 1);
    PRINT '  ✓ Tipos de documento (TechCorp) creados.';
END
GO

-- 2.4 Administrador de TechCorp
-- Correo: admin.tech@techcorp.local | Contraseña: Admin@Tech2026!
DECLARE @EmpTech INT = (SELECT Id FROM [dbo].[Empresa] WHERE Slug = 'techcorp');
DECLARE @DptoTech INT = (SELECT TOP 1 Id FROM [dbo].[Departamento] WHERE Nombre = 'Administración' AND IdEmpresa = @EmpTech);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Usuario] WHERE Correo = 'admin.tech@techcorp.local')
BEGIN
    INSERT INTO [dbo].[Usuario] (IdDepartamento, IdEmpresa, Nombre, ApellidoP, ApellidoM, Correo, Contrasena, FechaCreacion, Estatus, IdUsuarioCreacion)
    VALUES (
        @DptoTech, @EmpTech,
        N'Ana', N'García', N'Martínez',
        N'admin.tech@techcorp.local',
        CONVERT(VARCHAR(255), HASHBYTES('SHA2_256', CONVERT(VARBINARY, N'Admin@Tech2026!')), 2),
        GETDATE(), 1, 1
    );

    DECLARE @AdminTechId INT = SCOPE_IDENTITY();
    DECLARE @RolAdminId INT = (SELECT TOP 1 Id FROM [dbo].[Rol] WHERE Nombre = 'Administrador' AND Estatus = 1);

    INSERT INTO [dbo].[Usuario_Rol] (IdUsuario, IdRol, FechaAsignacion, FechaCreacion, Estatus, IdUsuarioCreacion)
    VALUES (@AdminTechId, @RolAdminId, GETDATE(), GETDATE(), 1, 1);

    -- Actualizar IdUsuarioCreacion del departamento
    UPDATE [dbo].[Departamento] SET IdUsuarioCreacion = @AdminTechId 
    WHERE IdEmpresa = @EmpTech AND IdUsuarioCreacion = 1;

    PRINT '  ✓ Admin TechCorp (admin.tech@techcorp.local) creado y rol asignado.';
END
ELSE
    PRINT '  → Admin TechCorp ya existe.';
GO


-- ============================================================
-- SECCIÓN 3: EMPRESA 2 - Grupo Innovar
-- ============================================================

-- 3.1 Crear Empresa 2
IF NOT EXISTS (SELECT 1 FROM [dbo].[Empresa] WHERE Slug = 'grupoinnovar')
BEGIN
    INSERT INTO [dbo].[Empresa] (Nombre, Slug, RFC, CorreoContacto, FechaRegistro, Estatus)
    VALUES (N'Grupo Innovar', N'grupoinnovar', N'GIN654321XYZ', N'info@grupoinnovar.local', GETDATE(), 1);
    PRINT '  ✓ Empresa Grupo Innovar creada.';
END
ELSE
    PRINT '  → Empresa Grupo Innovar ya existe.';
GO

-- 3.2 Departamentos para Grupo Innovar
DECLARE @EmpInnovar INT = (SELECT Id FROM [dbo].[Empresa] WHERE Slug = 'grupoinnovar');

IF NOT EXISTS (SELECT 1 FROM [dbo].[Departamento] WHERE Nombre = 'Administración' AND IdEmpresa = @EmpInnovar)
BEGIN
    INSERT INTO [dbo].[Departamento] (Nombre, Abreviatura, Estatus, FechaCreacion, IdEmpresa, IdUsuarioCreacion)
    VALUES (N'Administración', N'ADM', 1, GETDATE(), @EmpInnovar, 1);
    PRINT '  ✓ Departamento Administración (Grupo Innovar) creado.';
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Departamento] WHERE Nombre = 'Finanzas' AND IdEmpresa = @EmpInnovar)
BEGIN
    INSERT INTO [dbo].[Departamento] (Nombre, Abreviatura, Estatus, FechaCreacion, IdEmpresa, IdUsuarioCreacion)
    VALUES (N'Finanzas', N'FIN', 1, GETDATE(), @EmpInnovar, 1);
    PRINT '  ✓ Departamento Finanzas (Grupo Innovar) creado.';
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Departamento] WHERE Nombre = 'Operaciones' AND IdEmpresa = @EmpInnovar)
BEGIN
    INSERT INTO [dbo].[Departamento] (Nombre, Abreviatura, Estatus, FechaCreacion, IdEmpresa, IdUsuarioCreacion)
    VALUES (N'Operaciones', N'OPS', 1, GETDATE(), @EmpInnovar, 1);
    PRINT '  ✓ Departamento Operaciones (Grupo Innovar) creado.';
END
GO

-- 3.3 Tipos de documento para Grupo Innovar
DECLARE @EmpInnovar INT = (SELECT Id FROM [dbo].[Empresa] WHERE Slug = 'grupoinnovar');
IF NOT EXISTS (SELECT 1 FROM [dbo].[TipoDocumento] WHERE Nombre = 'Reporte Financiero' AND IdEmpresa = @EmpInnovar)
BEGIN
    INSERT INTO [dbo].[TipoDocumento] (Nombre, Abreviatura, TiempoRetencionMeses, Estatus, FechaCreacion, IdEmpresa, IdUsuarioCreacion)
    VALUES 
    (N'Reporte Financiero', N'RF', 60, 1, GETDATE(), @EmpInnovar, 1),
    (N'Acta de Reunión', N'AR', 24, 1, GETDATE(), @EmpInnovar, 1),
    (N'Procedimiento Operativo', N'PO', 48, 1, GETDATE(), @EmpInnovar, 1);
    PRINT '  ✓ Tipos de documento (Grupo Innovar) creados.';
END
GO

-- 3.4 Administrador de Grupo Innovar
-- Correo: admin@grupoinnovar.local | Contraseña: Admin@Innov2026!
DECLARE @EmpInnovar INT = (SELECT Id FROM [dbo].[Empresa] WHERE Slug = 'grupoinnovar');
DECLARE @DptoInnovar INT = (SELECT TOP 1 Id FROM [dbo].[Departamento] WHERE Nombre = 'Administración' AND IdEmpresa = @EmpInnovar);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Usuario] WHERE Correo = 'admin@grupoinnovar.local')
BEGIN
    INSERT INTO [dbo].[Usuario] (IdDepartamento, IdEmpresa, Nombre, ApellidoP, ApellidoM, Correo, Contrasena, FechaCreacion, Estatus, IdUsuarioCreacion)
    VALUES (
        @DptoInnovar, @EmpInnovar,
        N'Carlos', N'López', N'Hernández',
        N'admin@grupoinnovar.local',
        CONVERT(VARCHAR(255), HASHBYTES('SHA2_256', CONVERT(VARBINARY, N'Admin@Innov2026!')), 2),
        GETDATE(), 1, 1
    );

    DECLARE @AdminInnovarId INT = SCOPE_IDENTITY();
    DECLARE @RolAdminId2 INT = (SELECT TOP 1 Id FROM [dbo].[Rol] WHERE Nombre = 'Administrador' AND Estatus = 1);

    INSERT INTO [dbo].[Usuario_Rol] (IdUsuario, IdRol, FechaAsignacion, FechaCreacion, Estatus, IdUsuarioCreacion)
    VALUES (@AdminInnovarId, @RolAdminId2, GETDATE(), GETDATE(), 1, 1);

    UPDATE [dbo].[Departamento] SET IdUsuarioCreacion = @AdminInnovarId 
    WHERE IdEmpresa = @EmpInnovar AND IdUsuarioCreacion = 1;

    PRINT '  ✓ Admin Grupo Innovar (admin@grupoinnovar.local) creado y rol asignado.';
END
ELSE
    PRINT '  → Admin Grupo Innovar ya existe.';
GO


-- ============================================================
-- SECCIÓN 4: TRIGGERS ADICIONALES DE AUDITORÍA
-- ============================================================

PRINT '  → Verificando trigger de auditoría de subida de versión...';
GO

-- Trigger: registrar en BitacoraTransaccional cuando se sube una nueva versión de documento
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'TRG_Auditoria_NuevaVersion')
BEGIN
    EXEC('
    CREATE TRIGGER [dbo].[TRG_Auditoria_NuevaVersion]
    ON [dbo].[Documento_Version]
    AFTER INSERT
    AS
    BEGIN
        SET NOCOUNT ON;
        INSERT INTO [dbo].[BitacoraTransaccional] (IdUsuario, IdDocumento, IdVersion, Accion, FechaHora, Detalle, IdUsuarioCreacion, Estatus)
        SELECT 
            i.IdUsuarioSube,
            i.IdDocumento,
            i.Id,
            ''SUBIR_VERSION'',
            GETDATE(),
            ''Nueva versión v'' + CAST(i.NumeroVersion AS VARCHAR) + '' subida para documento Id='' + CAST(i.IdDocumento AS VARCHAR),
            i.IdUsuarioSube,
            1
        FROM inserted i;
    END
    ');
    PRINT '  ✓ Trigger TRG_Auditoria_NuevaVersion creado.';
END
ELSE
    PRINT '  → Trigger TRG_Auditoria_NuevaVersion ya existe.';
GO

-- Trigger: registrar en BitacoraTransaccional cuando se edita un documento
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'TRG_Auditoria_EditarDocumento')
BEGIN
    EXEC('
    CREATE TRIGGER [dbo].[TRG_Auditoria_EditarDocumento]
    ON [dbo].[Documento]
    AFTER UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        -- Auditar cuando cambia el Titulo (edición de metadatos)
        IF UPDATE(Titulo)
        BEGIN
            INSERT INTO [dbo].[BitacoraTransaccional] (IdUsuario, IdDocumento, Accion, FechaHora, Detalle, IdUsuarioCreacion, Estatus)
            SELECT 
                ISNULL(i.IdUsuarioModificacion, i.IdUsuarioPropietario),
                i.Id,
                ''EDITAR_DOCUMENTO'',
                GETDATE(),
                ''Documento Id='' + CAST(i.Id AS VARCHAR) + '' editado: "'' + i.Titulo + ''"'',
                ISNULL(i.IdUsuarioModificacion, i.IdUsuarioPropietario),
                1
            FROM inserted i;
        END
    END
    ');
    PRINT '  ✓ Trigger TRG_Auditoria_EditarDocumento creado.';
END
ELSE
    PRINT '  → Trigger TRG_Auditoria_EditarDocumento ya existe.';
GO

-- Trigger: registrar en BitacoraTransaccional cuando se elimina (soft delete) un documento
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'TRG_Auditoria_EliminarDocumento')
BEGIN
    EXEC('
    CREATE TRIGGER [dbo].[TRG_Auditoria_EliminarDocumento]
    ON [dbo].[Documento]
    AFTER UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        -- Solo cuando Estatus cambia de 1 a 0 (soft delete)
        IF UPDATE(Estatus)
        BEGIN
            INSERT INTO [dbo].[BitacoraTransaccional] (IdUsuario, IdDocumento, Accion, FechaHora, Detalle, IdUsuarioCreacion, Estatus)
            SELECT 
                ISNULL(i.IdUsuarioEliminacion, i.IdUsuarioPropietario),
                i.Id,
                ''ELIMINAR_DOCUMENTO'',
                GETDATE(),
                ''Documento Id='' + CAST(i.Id AS VARCHAR) + '' "'' + i.Titulo + ''" marcado como eliminado'',
                ISNULL(i.IdUsuarioEliminacion, i.IdUsuarioPropietario),
                1
            FROM inserted i
            INNER JOIN deleted d ON i.Id = d.Id
            WHERE i.Estatus = 0 AND d.Estatus = 1;
        END
    END
    ');
    PRINT '  ✓ Trigger TRG_Auditoria_EliminarDocumento creado.';
END
ELSE
    PRINT '  → Trigger TRG_Auditoria_EliminarDocumento ya existe.';
GO

PRINT '========================================'
PRINT '  SEED COMPLETADO'
PRINT '----------------------------------------'
PRINT '  SUPERADMIN: admin@sigd.local'
PRINT '  Contraseña: Admin@SIGD2026!'
PRINT '----------------------------------------'
PRINT '  EMPRESA 1: TechCorp Solutions'
PRINT '  Admin: admin.tech@techcorp.local'
PRINT '  Contraseña: Admin@Tech2026!'
PRINT '----------------------------------------'
PRINT '  EMPRESA 2: Grupo Innovar'
PRINT '  Admin: admin@grupoinnovar.local'
PRINT '  Contraseña: Admin@Innov2026!'
PRINT '========================================'
