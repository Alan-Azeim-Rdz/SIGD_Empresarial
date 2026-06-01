USE SIGD_Central;
SET NOCOUNT ON;

-- ================================================================
-- SEED COMPLETO: DATOS FICTICIOS PARA AMBAS EMPRESAS
-- Empresa 3: TechCorp Solutions (Id=3, Slug='techcorp')
-- Empresa 4: Grupo Innovar     (Id=4, Slug='grupoinnovar')
-- ================================================================
PRINT '=== INICIANDO SEED COMPLETO DE DATOS FICTICIOS ===';

-- Asegurar que los roles necesarios existen para evitar IDs nulos en Usuario_Rol
IF NOT EXISTS (SELECT 1 FROM Rol WHERE Nombre='Auditor')
    INSERT INTO Rol (Nombre, Descripcion, Estatus, FechaCreacion, IdUsuarioCreacion) VALUES (N'Auditor', N'Permite ver reportes e historiales', 1, GETDATE(), 1);
IF NOT EXISTS (SELECT 1 FROM Rol WHERE Nombre='Superior')
    INSERT INTO Rol (Nombre, Descripcion, Estatus, FechaCreacion, IdUsuarioCreacion) VALUES (N'Superior', N'Permite autorizar flujos', 1, GETDATE(), 1);

-- IDs de referencia
DECLARE @EmpTech    INT = (SELECT Id FROM Empresa WHERE Slug='techcorp');
DECLARE @EmpInnov   INT = (SELECT Id FROM Empresa WHERE Slug='grupoinnovar');
DECLARE @RolAdmin   INT = (SELECT Id FROM Rol WHERE Nombre='Administrador');
DECLARE @RolUsuario INT = (SELECT Id FROM Rol WHERE Nombre='Usuario');
DECLARE @RolAuditor INT = (SELECT Id FROM Rol WHERE Nombre='Auditor');
DECLARE @RolSuper   INT = (SELECT Id FROM Rol WHERE Nombre='Superior');

-- Contraseña estándar de prueba: Test@2026! (hash SHA2_256 UTF-16)
DECLARE @PwdTest VARCHAR(255) = CONVERT(VARCHAR(255), HASHBYTES('SHA2_256', CONVERT(VARBINARY, N'Test@2026!')), 2);
-- Admin TechCorp existente
DECLARE @AdminTech   INT = (SELECT Id FROM Usuario WHERE Correo='admin.tech@techcorp.local');
-- Admin Innovar existente
DECLARE @AdminInnov  INT = (SELECT Id FROM Usuario WHERE Correo='admin@grupoinnovar.local');

PRINT 'Refs: EmpTech=' + CAST(@EmpTech AS VARCHAR) + ' EmpInnov=' + CAST(@EmpInnov AS VARCHAR);

-- ================================================================
-- SECCIÓN A: DEPARTAMENTOS ADICIONALES (TechCorp)
-- ================================================================
DECLARE @DAdmTech  INT; DECLARE @DTITech   INT;
DECLARE @DRRHHTech INT; DECLARE @DLegalTech INT;

SET @DAdmTech  = (SELECT Id FROM Departamento WHERE Nombre='Administración'          AND IdEmpresa=@EmpTech);
SET @DTITech   = (SELECT Id FROM Departamento WHERE Nombre='Tecnología de Información' AND IdEmpresa=@EmpTech);
SET @DRRHHTech = (SELECT Id FROM Departamento WHERE Nombre='Recursos Humanos'          AND IdEmpresa=@EmpTech);

IF NOT EXISTS (SELECT 1 FROM Departamento WHERE Nombre='Legal y Cumplimiento' AND IdEmpresa=@EmpTech)
BEGIN
    INSERT INTO Departamento (Nombre, Abreviatura, Estatus, FechaCreacion, IdEmpresa, IdUsuarioCreacion)
    VALUES (N'Legal y Cumplimiento', N'LEG', 1, GETDATE(), @EmpTech, @AdminTech);
END
SET @DLegalTech = (SELECT Id FROM Departamento WHERE Nombre='Legal y Cumplimiento' AND IdEmpresa=@EmpTech);

-- ================================================================
-- SECCIÓN B: DEPARTAMENTOS ADICIONALES (Grupo Innovar)
-- ================================================================
DECLARE @DAdmInnov  INT; DECLARE @DFinInnov  INT;
DECLARE @DOpsInnov  INT; DECLARE @DCompInnov INT;

SET @DAdmInnov  = (SELECT Id FROM Departamento WHERE Nombre='Administración' AND IdEmpresa=@EmpInnov);
SET @DFinInnov  = (SELECT Id FROM Departamento WHERE Nombre='Finanzas'       AND IdEmpresa=@EmpInnov);
SET @DOpsInnov  = (SELECT Id FROM Departamento WHERE Nombre='Operaciones'    AND IdEmpresa=@EmpInnov);

IF NOT EXISTS (SELECT 1 FROM Departamento WHERE Nombre='Comercial' AND IdEmpresa=@EmpInnov)
BEGIN
    INSERT INTO Departamento (Nombre, Abreviatura, Estatus, FechaCreacion, IdEmpresa, IdUsuarioCreacion)
    VALUES (N'Comercial', N'COM', 1, GETDATE(), @EmpInnov, @AdminInnov);
END
SET @DCompInnov = (SELECT Id FROM Departamento WHERE Nombre='Comercial' AND IdEmpresa=@EmpInnov);

PRINT 'Departamentos listos.';

-- ================================================================
-- SECCIÓN C: USUARIOS TECHCORP (6 usuarios + admin = 7)
-- ================================================================

-- u1: Editor TI
IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Correo='marco.torres@techcorp.local')
BEGIN
    INSERT INTO Usuario (IdDepartamento, IdEmpresa, Nombre, ApellidoP, ApellidoM, Correo, Contrasena, FechaCreacion, Estatus, IdUsuarioCreacion)
    VALUES (@DTITech, @EmpTech, N'Marco', N'Torres', N'Ríos', N'marco.torres@techcorp.local', @PwdTest, DATEADD(DAY,-90,GETDATE()), 1, @AdminTech);
    DECLARE @u1Tech INT = SCOPE_IDENTITY();
    INSERT INTO Usuario_Rol (IdUsuario, IdRol, FechaAsignacion, FechaCreacion, Estatus, IdUsuarioCreacion)
    VALUES (@u1Tech, @RolUsuario, DATEADD(DAY,-90,GETDATE()), DATEADD(DAY,-90,GETDATE()), 1, @AdminTech);
END

-- u2: Aprobador TI
IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Correo='sofia.ramos@techcorp.local')
BEGIN
    INSERT INTO Usuario (IdDepartamento, IdEmpresa, Nombre, ApellidoP, ApellidoM, Correo, Contrasena, FechaCreacion, Estatus, IdUsuarioCreacion)
    VALUES (@DTITech, @EmpTech, N'Sofía', N'Ramos', N'Gutiérrez', N'sofia.ramos@techcorp.local', @PwdTest, DATEADD(DAY,-80,GETDATE()), 1, @AdminTech);
    DECLARE @u2Tech INT = SCOPE_IDENTITY();
    INSERT INTO Usuario_Rol (IdUsuario, IdRol, FechaAsignacion, FechaCreacion, Estatus, IdUsuarioCreacion)
    VALUES (@u2Tech, @RolSuper, DATEADD(DAY,-80,GETDATE()), DATEADD(DAY,-80,GETDATE()), 1, @AdminTech);
END

-- u3: Editor RRHH
IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Correo='lucia.mendoza@techcorp.local')
BEGIN
    INSERT INTO Usuario (IdDepartamento, IdEmpresa, Nombre, ApellidoP, ApellidoM, Correo, Contrasena, FechaCreacion, Estatus, IdUsuarioCreacion)
    VALUES (@DRRHHTech, @EmpTech, N'Lucía', N'Mendoza', N'Salinas', N'lucia.mendoza@techcorp.local', @PwdTest, DATEADD(DAY,-75,GETDATE()), 1, @AdminTech);
    DECLARE @u3Tech INT = SCOPE_IDENTITY();
    INSERT INTO Usuario_Rol (IdUsuario, IdRol, FechaAsignacion, FechaCreacion, Estatus, IdUsuarioCreacion)
    VALUES (@u3Tech, @RolUsuario, DATEADD(DAY,-75,GETDATE()), DATEADD(DAY,-75,GETDATE()), 1, @AdminTech);
END

-- u4: Auditor
IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Correo='jorge.vargas@techcorp.local')
BEGIN
    INSERT INTO Usuario (IdDepartamento, IdEmpresa, Nombre, ApellidoP, ApellidoM, Correo, Contrasena, FechaCreacion, Estatus, IdUsuarioCreacion)
    VALUES (@DLegalTech, @EmpTech, N'Jorge', N'Vargas', N'Peña', N'jorge.vargas@techcorp.local', @PwdTest, DATEADD(DAY,-60,GETDATE()), 1, @AdminTech);
    DECLARE @u4Tech INT = SCOPE_IDENTITY();
    INSERT INTO Usuario_Rol (IdUsuario, IdRol, FechaAsignacion, FechaCreacion, Estatus, IdUsuarioCreacion)
    VALUES (@u4Tech, @RolAuditor, DATEADD(DAY,-60,GETDATE()), DATEADD(DAY,-60,GETDATE()), 1, @AdminTech);
END

-- u5: Editor Legal
IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Correo='diana.flores@techcorp.local')
BEGIN
    INSERT INTO Usuario (IdDepartamento, IdEmpresa, Nombre, ApellidoP, ApellidoM, Correo, Contrasena, FechaCreacion, Estatus, IdUsuarioCreacion)
    VALUES (@DLegalTech, @EmpTech, N'Diana', N'Flores', N'Castillo', N'diana.flores@techcorp.local', @PwdTest, DATEADD(DAY,-55,GETDATE()), 1, @AdminTech);
    DECLARE @u5Tech INT = SCOPE_IDENTITY();
    INSERT INTO Usuario_Rol (IdUsuario, IdRol, FechaAsignacion, FechaCreacion, Estatus, IdUsuarioCreacion)
    VALUES (@u5Tech, @RolUsuario, DATEADD(DAY,-55,GETDATE()), DATEADD(DAY,-55,GETDATE()), 1, @AdminTech);
END

-- u6: Aprobador/Superior RRHH
IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Correo='raul.suarez@techcorp.local')
BEGIN
    INSERT INTO Usuario (IdDepartamento, IdEmpresa, Nombre, ApellidoP, ApellidoM, Correo, Contrasena, FechaCreacion, Estatus, IdUsuarioCreacion)
    VALUES (@DRRHHTech, @EmpTech, N'Raúl', N'Suárez', N'López', N'raul.suarez@techcorp.local', @PwdTest, DATEADD(DAY,-50,GETDATE()), 1, @AdminTech);
    DECLARE @u6Tech INT = SCOPE_IDENTITY();
    INSERT INTO Usuario_Rol (IdUsuario, IdRol, FechaAsignacion, FechaCreacion, Estatus, IdUsuarioCreacion)
    VALUES (@u6Tech, @RolSuper, DATEADD(DAY,-50,GETDATE()), DATEADD(DAY,-50,GETDATE()), 1, @AdminTech);
END

PRINT 'Usuarios TechCorp listos.';

-- ================================================================
-- SECCIÓN D: USUARIOS GRUPO INNOVAR (6 usuarios + admin = 7)
-- ================================================================

-- v1: Editor Finanzas
IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Correo='patricia.luna@grupoinnovar.local')
BEGIN
    INSERT INTO Usuario (IdDepartamento, IdEmpresa, Nombre, ApellidoP, ApellidoM, Correo, Contrasena, FechaCreacion, Estatus, IdUsuarioCreacion)
    VALUES (@DFinInnov, @EmpInnov, N'Patricia', N'Luna', N'Ortega', N'patricia.luna@grupoinnovar.local', @PwdTest, DATEADD(DAY,-85,GETDATE()), 1, @AdminInnov);
    DECLARE @v1Innov INT = SCOPE_IDENTITY();
    INSERT INTO Usuario_Rol (IdUsuario, IdRol, FechaAsignacion, FechaCreacion, Estatus, IdUsuarioCreacion)
    VALUES (@v1Innov, @RolUsuario, DATEADD(DAY,-85,GETDATE()), DATEADD(DAY,-85,GETDATE()), 1, @AdminInnov);
END

-- v2: Aprobador Finanzas
IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Correo='ernesto.medina@grupoinnovar.local')
BEGIN
    INSERT INTO Usuario (IdDepartamento, IdEmpresa, Nombre, ApellidoP, ApellidoM, Correo, Contrasena, FechaCreacion, Estatus, IdUsuarioCreacion)
    VALUES (@DFinInnov, @EmpInnov, N'Ernesto', N'Medina', N'Reyes', N'ernesto.medina@grupoinnovar.local', @PwdTest, DATEADD(DAY,-70,GETDATE()), 1, @AdminInnov);
    DECLARE @v2Innov INT = SCOPE_IDENTITY();
    INSERT INTO Usuario_Rol (IdUsuario, IdRol, FechaAsignacion, FechaCreacion, Estatus, IdUsuarioCreacion)
    VALUES (@v2Innov, @RolSuper, DATEADD(DAY,-70,GETDATE()), DATEADD(DAY,-70,GETDATE()), 1, @AdminInnov);
END

-- v3: Editor Operaciones
IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Correo='isabel.cano@grupoinnovar.local')
BEGIN
    INSERT INTO Usuario (IdDepartamento, IdEmpresa, Nombre, ApellidoP, ApellidoM, Correo, Contrasena, FechaCreacion, Estatus, IdUsuarioCreacion)
    VALUES (@DOpsInnov, @EmpInnov, N'Isabel', N'Cano', N'Jiménez', N'isabel.cano@grupoinnovar.local', @PwdTest, DATEADD(DAY,-65,GETDATE()), 1, @AdminInnov);
    DECLARE @v3Innov INT = SCOPE_IDENTITY();
    INSERT INTO Usuario_Rol (IdUsuario, IdRol, FechaAsignacion, FechaCreacion, Estatus, IdUsuarioCreacion)
    VALUES (@v3Innov, @RolUsuario, DATEADD(DAY,-65,GETDATE()), DATEADD(DAY,-65,GETDATE()), 1, @AdminInnov);
END

-- v4: Auditor
IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Correo='roberto.solis@grupoinnovar.local')
BEGIN
    INSERT INTO Usuario (IdDepartamento, IdEmpresa, Nombre, ApellidoP, ApellidoM, Correo, Contrasena, FechaCreacion, Estatus, IdUsuarioCreacion)
    VALUES (@DAdmInnov, @EmpInnov, N'Roberto', N'Solís', N'Vega', N'roberto.solis@grupoinnovar.local', @PwdTest, DATEADD(DAY,-60,GETDATE()), 1, @AdminInnov);
    DECLARE @v4Innov INT = SCOPE_IDENTITY();
    INSERT INTO Usuario_Rol (IdUsuario, IdRol, FechaAsignacion, FechaCreacion, Estatus, IdUsuarioCreacion)
    VALUES (@v4Innov, @RolAuditor, DATEADD(DAY,-60,GETDATE()), DATEADD(DAY,-60,GETDATE()), 1, @AdminInnov);
END

-- v5: Editor Comercial
IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Correo='elena.ruiz@grupoinnovar.local')
BEGIN
    INSERT INTO Usuario (IdDepartamento, IdEmpresa, Nombre, ApellidoP, ApellidoM, Correo, Contrasena, FechaCreacion, Estatus, IdUsuarioCreacion)
    VALUES (@DCompInnov, @EmpInnov, N'Elena', N'Ruiz', N'Navarro', N'elena.ruiz@grupoinnovar.local', @PwdTest, DATEADD(DAY,-45,GETDATE()), 1, @AdminInnov);
    DECLARE @v5Innov INT = SCOPE_IDENTITY();
    INSERT INTO Usuario_Rol (IdUsuario, IdRol, FechaAsignacion, FechaCreacion, Estatus, IdUsuarioCreacion)
    VALUES (@v5Innov, @RolUsuario, DATEADD(DAY,-45,GETDATE()), DATEADD(DAY,-45,GETDATE()), 1, @AdminInnov);
END

-- v6: Aprobador Operaciones
IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Correo='miguel.rojas@grupoinnovar.local')
BEGIN
    INSERT INTO Usuario (IdDepartamento, IdEmpresa, Nombre, ApellidoP, ApellidoM, Correo, Contrasena, FechaCreacion, Estatus, IdUsuarioCreacion)
    VALUES (@DOpsInnov, @EmpInnov, N'Miguel', N'Rojas', N'Paredes', N'miguel.rojas@grupoinnovar.local', @PwdTest, DATEADD(DAY,-40,GETDATE()), 1, @AdminInnov);
    DECLARE @v6Innov INT = SCOPE_IDENTITY();
    INSERT INTO Usuario_Rol (IdUsuario, IdRol, FechaAsignacion, FechaCreacion, Estatus, IdUsuarioCreacion)
    VALUES (@v6Innov, @RolSuper, DATEADD(DAY,-40,GETDATE()), DATEADD(DAY,-40,GETDATE()), 1, @AdminInnov);
END

PRINT 'Usuarios Grupo Innovar listos.';

-- Variables de usuario para usarlas en documentos
DECLARE @uMarco   INT = (SELECT Id FROM Usuario WHERE Correo='marco.torres@techcorp.local');
DECLARE @uSofia   INT = (SELECT Id FROM Usuario WHERE Correo='sofia.ramos@techcorp.local');
DECLARE @uLucia   INT = (SELECT Id FROM Usuario WHERE Correo='lucia.mendoza@techcorp.local');
DECLARE @uJorge   INT = (SELECT Id FROM Usuario WHERE Correo='jorge.vargas@techcorp.local');
DECLARE @uDiana   INT = (SELECT Id FROM Usuario WHERE Correo='diana.flores@techcorp.local');
DECLARE @uRaul    INT = (SELECT Id FROM Usuario WHERE Correo='raul.suarez@techcorp.local');
DECLARE @vPatricia INT = (SELECT Id FROM Usuario WHERE Correo='patricia.luna@grupoinnovar.local');
DECLARE @vErnesto  INT = (SELECT Id FROM Usuario WHERE Correo='ernesto.medina@grupoinnovar.local');
DECLARE @vIsabel   INT = (SELECT Id FROM Usuario WHERE Correo='isabel.cano@grupoinnovar.local');
DECLARE @vRoberto  INT = (SELECT Id FROM Usuario WHERE Correo='roberto.solis@grupoinnovar.local');
DECLARE @vElena    INT = (SELECT Id FROM Usuario WHERE Correo='elena.ruiz@grupoinnovar.local');
DECLARE @vMiguel   INT = (SELECT Id FROM Usuario WHERE Correo='miguel.rojas@grupoinnovar.local');

-- Tipos de documento TechCorp
DECLARE @TMTech  INT = (SELECT Id FROM TipoDocumento WHERE Nombre='Manual Técnico'   AND IdEmpresa=@EmpTech);
DECLARE @TPITech INT = (SELECT Id FROM TipoDocumento WHERE Nombre='Política Interna'  AND IdEmpresa=@EmpTech);
DECLARE @TConTech INT = (SELECT Id FROM TipoDocumento WHERE Nombre='Contrato'         AND IdEmpresa=@EmpTech);

-- Tipos de documento Innovar
DECLARE @TRFInnov  INT = (SELECT Id FROM TipoDocumento WHERE Nombre='Reporte Financiero'     AND IdEmpresa=@EmpInnov);
DECLARE @TARInnov  INT = (SELECT Id FROM TipoDocumento WHERE Nombre='Acta de Reunión'         AND IdEmpresa=@EmpInnov);
DECLARE @TPOInnov  INT = (SELECT Id FROM TipoDocumento WHERE Nombre='Procedimiento Operativo' AND IdEmpresa=@EmpInnov);

-- ================================================================
-- SECCIÓN E: PERMISOS DEL SISTEMA
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM Permiso WHERE Codigo='DOC.CREAR')
BEGIN
    INSERT INTO Permiso (Codigo, Descripcion, Modulo, Estatus, FechaCreacion, IdUsuarioCreacion) VALUES
    ('DOC.CREAR',      'Crear nuevos documentos',               'Documentos', 1, GETDATE(), 1),
    ('DOC.EDITAR',     'Editar documentos existentes',           'Documentos', 1, GETDATE(), 1),
    ('DOC.ELIMINAR',   'Eliminar documentos',                    'Documentos', 1, GETDATE(), 1),
    ('DOC.VER',        'Ver y descargar documentos',             'Documentos', 1, GETDATE(), 1),
    ('DOC.APROBAR',    'Aprobar o rechazar documentos en flujo', 'Documentos', 1, GETDATE(), 1),
    ('DOC.FIRMAR',     'Firmar digitalmente documentos',         'Documentos', 1, GETDATE(), 1),
    ('USR.CREAR',      'Crear nuevos usuarios',                  'Usuarios',   1, GETDATE(), 1),
    ('USR.EDITAR',     'Editar usuarios existentes',             'Usuarios',   1, GETDATE(), 1),
    ('USR.ELIMINAR',   'Desactivar usuarios',                    'Usuarios',   1, GETDATE(), 1),
    ('RPT.VER',        'Ver reportes y dashboards',              'Reportes',   1, GETDATE(), 1),
    ('RPT.EXPORTAR',   'Exportar reportes a PDF/Excel',          'Reportes',   1, GETDATE(), 1),
    ('AUD.BITACORA',   'Ver bitácoras de auditoría',             'Auditoria',  1, GETDATE(), 1),
    ('CONF.ROLES',     'Gestionar roles y permisos',             'Configuracion', 1, GETDATE(), 1),
    ('CONF.DEPTOS',    'Gestionar departamentos',                'Configuracion', 1, GETDATE(), 1);
    PRINT 'Permisos del sistema creados.';
END

-- Asignar permisos a roles
DECLARE @pDocCrear   INT = (SELECT Id FROM Permiso WHERE Codigo='DOC.CREAR');
DECLARE @pDocEditar  INT = (SELECT Id FROM Permiso WHERE Codigo='DOC.EDITAR');
DECLARE @pDocElim    INT = (SELECT Id FROM Permiso WHERE Codigo='DOC.ELIMINAR');
DECLARE @pDocVer     INT = (SELECT Id FROM Permiso WHERE Codigo='DOC.VER');
DECLARE @pDocAprob   INT = (SELECT Id FROM Permiso WHERE Codigo='DOC.APROBAR');
DECLARE @pDocFirmar  INT = (SELECT Id FROM Permiso WHERE Codigo='DOC.FIRMAR');
DECLARE @pUsrCrear   INT = (SELECT Id FROM Permiso WHERE Codigo='USR.CREAR');
DECLARE @pUsrEditar  INT = (SELECT Id FROM Permiso WHERE Codigo='USR.EDITAR');
DECLARE @pUsrElim    INT = (SELECT Id FROM Permiso WHERE Codigo='USR.ELIMINAR');
DECLARE @pRptVer     INT = (SELECT Id FROM Permiso WHERE Codigo='RPT.VER');
DECLARE @pRptExp     INT = (SELECT Id FROM Permiso WHERE Codigo='RPT.EXPORTAR');
DECLARE @pAudBit     INT = (SELECT Id FROM Permiso WHERE Codigo='AUD.BITACORA');
DECLARE @pConfRoles  INT = (SELECT Id FROM Permiso WHERE Codigo='CONF.ROLES');
DECLARE @pConfDeptos INT = (SELECT Id FROM Permiso WHERE Codigo='CONF.DEPTOS');

-- Rol Admin → todos los permisos
IF NOT EXISTS (SELECT 1 FROM Rol_Permiso WHERE IdRol=@RolAdmin AND IdPermiso=@pDocCrear)
BEGIN
    INSERT INTO Rol_Permiso (IdRol, IdPermiso, Estatus, FechaCreacion, IdUsuarioCreacion) VALUES
    (@RolAdmin, @pDocCrear,   1, GETDATE(), 1), (@RolAdmin, @pDocEditar,  1, GETDATE(), 1),
    (@RolAdmin, @pDocElim,    1, GETDATE(), 1), (@RolAdmin, @pDocVer,     1, GETDATE(), 1),
    (@RolAdmin, @pDocAprob,   1, GETDATE(), 1), (@RolAdmin, @pDocFirmar,  1, GETDATE(), 1),
    (@RolAdmin, @pUsrCrear,   1, GETDATE(), 1), (@RolAdmin, @pUsrEditar,  1, GETDATE(), 1),
    (@RolAdmin, @pUsrElim,    1, GETDATE(), 1), (@RolAdmin, @pRptVer,     1, GETDATE(), 1),
    (@RolAdmin, @pRptExp,     1, GETDATE(), 1), (@RolAdmin, @pAudBit,     1, GETDATE(), 1),
    (@RolAdmin, @pConfRoles,  1, GETDATE(), 1), (@RolAdmin, @pConfDeptos, 1, GETDATE(), 1);
END


-- Rol Superior → aprobar, ver
IF NOT EXISTS (SELECT 1 FROM Rol_Permiso WHERE IdRol=@RolSuper AND IdPermiso=@pDocAprob)
BEGIN
    INSERT INTO Rol_Permiso (IdRol, IdPermiso, Estatus, FechaCreacion, IdUsuarioCreacion) VALUES
    (@RolSuper, @pDocAprob, 1, GETDATE(), 1), (@RolSuper, @pDocVer, 1, GETDATE(), 1);
END

-- Rol Auditor → ver reportes, bitácoras, documentos
IF NOT EXISTS (SELECT 1 FROM Rol_Permiso WHERE IdRol=@RolAuditor AND IdPermiso=@pDocVer)
BEGIN
    INSERT INTO Rol_Permiso (IdRol, IdPermiso, Estatus, FechaCreacion, IdUsuarioCreacion) VALUES
    (@RolAuditor, @pDocVer,   1, GETDATE(), 1), (@RolAuditor, @pRptVer,   1, GETDATE(), 1),
    (@RolAuditor, @pRptExp,   1, GETDATE(), 1), (@RolAuditor, @pAudBit,   1, GETDATE(), 1);
END

PRINT 'Permisos por rol configurados.';

-- ================================================================
-- SECCIÓN F: DOCUMENTOS TECHCORP (8 documentos variados)
-- ================================================================
PRINT '--- Documentos TechCorp ---';

-- Doc T1: Manual Técnico - Vigente (aprobado y firmado)
DECLARE @dT1 INT;
IF NOT EXISTS (SELECT 1 FROM Documento WHERE CodigoInterno='TC-MT-001')
BEGIN
    INSERT INTO Documento (CodigoInterno, Titulo, IdDepartamento, EstadoActual, IdUsuarioPropietario,
        FechaCreacion, Estatus, IdUsuarioCreacion, IdTipoDocumento, IdEmpresa)
    VALUES ('TC-MT-001', N'Manual de Configuración de Servidores Linux', @DTITech, 'Vigente',
        @uMarco, DATEADD(DAY,-85,GETDATE()), 1, @uMarco, @TMTech, @EmpTech);
END
SET @dT1 = (SELECT Id FROM Documento WHERE CodigoInterno='TC-MT-001');

-- Doc T2: Política Interna - Vigente
DECLARE @dT2 INT;
IF NOT EXISTS (SELECT 1 FROM Documento WHERE CodigoInterno='TC-PI-001')
BEGIN
    INSERT INTO Documento (CodigoInterno, Titulo, IdDepartamento, EstadoActual, IdUsuarioPropietario,
        FechaCreacion, Estatus, IdUsuarioCreacion, IdTipoDocumento, IdEmpresa)
    VALUES ('TC-PI-001', N'Política de Seguridad de la Información', @DTITech, 'Vigente',
        @uMarco, DATEADD(DAY,-70,GETDATE()), 1, @uMarco, @TPITech, @EmpTech);
END
SET @dT2 = (SELECT Id FROM Documento WHERE CodigoInterno='TC-PI-001');

-- Doc T3: Política RRHH - En Revisión
DECLARE @dT3 INT;
IF NOT EXISTS (SELECT 1 FROM Documento WHERE CodigoInterno='TC-PI-002')
BEGIN
    INSERT INTO Documento (CodigoInterno, Titulo, IdDepartamento, EstadoActual, IdUsuarioPropietario,
        FechaCreacion, Estatus, IdUsuarioCreacion, IdTipoDocumento, IdEmpresa)
    VALUES ('TC-PI-002', N'Política de Vacaciones y Permisos', @DRRHHTech, 'En Revisión',
        @uLucia, DATEADD(DAY,-30,GETDATE()), 1, @uLucia, @TPITech, @EmpTech);
END
SET @dT3 = (SELECT Id FROM Documento WHERE CodigoInterno='TC-PI-002');

-- Doc T4: Contrato - Vigente
DECLARE @dT4 INT;
IF NOT EXISTS (SELECT 1 FROM Documento WHERE CodigoInterno='TC-CON-001')
BEGIN
    INSERT INTO Documento (CodigoInterno, Titulo, IdDepartamento, EstadoActual, IdUsuarioPropietario,
        FechaCreacion, Estatus, IdUsuarioCreacion, IdTipoDocumento, IdEmpresa)
    VALUES ('TC-CON-001', N'Contrato de Servicios Cloud – Proveedor AWS', @DLegalTech, 'Vigente',
        @uDiana, DATEADD(DAY,-60,GETDATE()), 1, @uDiana, @TConTech, @EmpTech);
END
SET @dT4 = (SELECT Id FROM Documento WHERE CodigoInterno='TC-CON-001');

-- Doc T5: Manual TI - Borrador
DECLARE @dT5 INT;
IF NOT EXISTS (SELECT 1 FROM Documento WHERE CodigoInterno='TC-MT-002')
BEGIN
    INSERT INTO Documento (CodigoInterno, Titulo, IdDepartamento, EstadoActual, IdUsuarioPropietario,
        FechaCreacion, Estatus, IdUsuarioCreacion, IdTipoDocumento, IdEmpresa)
    VALUES ('TC-MT-002', N'Guía de Implementación de DevOps con GitLab CI/CD', @DTITech, 'Borrador',
        @uMarco, DATEADD(DAY,-10,GETDATE()), 1, @uMarco, @TMTech, @EmpTech);
END
SET @dT5 = (SELECT Id FROM Documento WHERE CodigoInterno='TC-MT-002');

-- Doc T6: Política Legal - Pendiente Firma
DECLARE @dT6 INT;
IF NOT EXISTS (SELECT 1 FROM Documento WHERE CodigoInterno='TC-LEG-001')
BEGIN
    INSERT INTO Documento (CodigoInterno, Titulo, IdDepartamento, EstadoActual, IdUsuarioPropietario,
        FechaCreacion, Estatus, IdUsuarioCreacion, IdTipoDocumento, IdEmpresa)
    VALUES ('TC-LEG-001', N'Política de Protección de Datos Personales (LGPDP)', @DLegalTech, 'Pendiente Firma',
        @uDiana, DATEADD(DAY,-20,GETDATE()), 1, @uDiana, @TPITech, @EmpTech);
END
SET @dT6 = (SELECT Id FROM Documento WHERE CodigoInterno='TC-LEG-001');

-- Doc T7: Contrato RRHH - Vigente
DECLARE @dT7 INT;
IF NOT EXISTS (SELECT 1 FROM Documento WHERE CodigoInterno='TC-CON-002')
BEGIN
    INSERT INTO Documento (CodigoInterno, Titulo, IdDepartamento, EstadoActual, IdUsuarioPropietario,
        FechaCreacion, Estatus, IdUsuarioCreacion, IdTipoDocumento, IdEmpresa)
    VALUES ('TC-CON-002', N'Contrato Colectivo de Trabajo 2026', @DRRHHTech, 'Vigente',
        @uLucia, DATEADD(DAY,-45,GETDATE()), 1, @uLucia, @TConTech, @EmpTech);
END
SET @dT7 = (SELECT Id FROM Documento WHERE CodigoInterno='TC-CON-002');

-- Doc T8: Manual ADM - Rechazado (workflow completo de ejemplo)
DECLARE @dT8 INT;
IF NOT EXISTS (SELECT 1 FROM Documento WHERE CodigoInterno='TC-ADM-001')
BEGIN
    INSERT INTO Documento (CodigoInterno, Titulo, IdDepartamento, EstadoActual, IdUsuarioPropietario,
        FechaCreacion, Estatus, IdUsuarioCreacion, IdTipoDocumento, IdEmpresa)
    VALUES ('TC-ADM-001', N'Manual de Procesos Administrativos v1.0', @DAdmTech, 'Rechazado',
        @AdminTech, DATEADD(DAY,-50,GETDATE()), 1, @AdminTech, @TMTech, @EmpTech);
END
SET @dT8 = (SELECT Id FROM Documento WHERE CodigoInterno='TC-ADM-001');

PRINT '  ✓ 8 documentos TechCorp creados.';

-- ================================================================
-- SECCIÓN G: VERSIONES DE DOCUMENTOS TECHCORP
-- ================================================================

IF NOT EXISTS (SELECT 1 FROM Documento_Version WHERE IdDocumento=@dT1 AND NumeroVersion=1)
BEGIN
    INSERT INTO Documento_Version (IdDocumento, NumeroVersion, RutaArchivoFisico, HashDocumento, MotivoCambio, IdUsuarioSube, FechaSubida, Estatus, IdUsuarioCreacion, ExtensionArchivo, MimeType, TamanoBytes)
    VALUES (@dT1, 1, '/archivos/techcorp/TC-MT-001_v1.pdf', 'a1b2c3d4e5f6789012345678901234567890123456789012345678901234abcd', N'Versión inicial', @uMarco, DATEADD(DAY,-85,GETDATE()), 1, @uMarco, '.pdf', 'application/pdf', 1024000);

    INSERT INTO Documento_Version (IdDocumento, NumeroVersion, RutaArchivoFisico, HashDocumento, MotivoCambio, IdUsuarioSube, FechaSubida, Estatus, IdUsuarioCreacion, ExtensionArchivo, MimeType, TamanoBytes)
    VALUES (@dT1, 2, '/archivos/techcorp/TC-MT-001_v2.pdf', 'b2c3d4e5f6789012345678901234567890123456789012345678901234abcde', N'Corrección de comandos de red en sección 4', @uMarco, DATEADD(DAY,-60,GETDATE()), 1, @uMarco, '.pdf', 'application/pdf', 1048576);
END

IF NOT EXISTS (SELECT 1 FROM Documento_Version WHERE IdDocumento=@dT2 AND NumeroVersion=1)
    INSERT INTO Documento_Version (IdDocumento, NumeroVersion, RutaArchivoFisico, HashDocumento, MotivoCambio, IdUsuarioSube, FechaSubida, Estatus, IdUsuarioCreacion, ExtensionArchivo, MimeType, TamanoBytes)
    VALUES (@dT2, 1, '/archivos/techcorp/TC-PI-001_v1.pdf', 'c3d4e5f6789012345678901234567890123456789012345678901234abcdef01', N'Publicación inicial', @uMarco, DATEADD(DAY,-70,GETDATE()), 1, @uMarco, '.pdf', 'application/pdf', 512000);

IF NOT EXISTS (SELECT 1 FROM Documento_Version WHERE IdDocumento=@dT3 AND NumeroVersion=1)
    INSERT INTO Documento_Version (IdDocumento, NumeroVersion, RutaArchivoFisico, HashDocumento, MotivoCambio, IdUsuarioSube, FechaSubida, Estatus, IdUsuarioCreacion, ExtensionArchivo, MimeType, TamanoBytes)
    VALUES (@dT3, 1, '/archivos/techcorp/TC-PI-002_v1.docx', 'd4e5f6789012345678901234567890123456789012345678901234abcdef0102', N'Borrador para revisión legal', @uLucia, DATEADD(DAY,-30,GETDATE()), 1, @uLucia, '.docx', 'application/vnd.openxmlformats-officedocument.wordprocessingml.document', 245760);

IF NOT EXISTS (SELECT 1 FROM Documento_Version WHERE IdDocumento=@dT4 AND NumeroVersion=1)
    INSERT INTO Documento_Version (IdDocumento, NumeroVersion, RutaArchivoFisico, HashDocumento, MotivoCambio, IdUsuarioSube, FechaSubida, Estatus, IdUsuarioCreacion, ExtensionArchivo, MimeType, TamanoBytes)
    VALUES (@dT4, 1, '/archivos/techcorp/TC-CON-001_v1.pdf', 'e5f6789012345678901234567890123456789012345678901234abcdef010203', N'Contrato firmado y escaneado', @uDiana, DATEADD(DAY,-60,GETDATE()), 1, @uDiana, '.pdf', 'application/pdf', 2097152);

IF NOT EXISTS (SELECT 1 FROM Documento_Version WHERE IdDocumento=@dT5 AND NumeroVersion=1)
    INSERT INTO Documento_Version (IdDocumento, NumeroVersion, RutaArchivoFisico, HashDocumento, MotivoCambio, IdUsuarioSube, FechaSubida, Estatus, IdUsuarioCreacion, ExtensionArchivo, MimeType, TamanoBytes)
    VALUES (@dT5, 1, '/archivos/techcorp/TC-MT-002_v1.md', 'f6789012345678901234567890123456789012345678901234abcdef01020304', N'Primer borrador', @uMarco, DATEADD(DAY,-10,GETDATE()), 1, @uMarco, '.md', 'text/markdown', 51200);

IF NOT EXISTS (SELECT 1 FROM Documento_Version WHERE IdDocumento=@dT6 AND NumeroVersion=1)
    INSERT INTO Documento_Version (IdDocumento, NumeroVersion, RutaArchivoFisico, HashDocumento, MotivoCambio, IdUsuarioSube, FechaSubida, Estatus, IdUsuarioCreacion, ExtensionArchivo, MimeType, TamanoBytes)
    VALUES (@dT6, 1, '/archivos/techcorp/TC-LEG-001_v1.pdf', 'a6789012345678901234567890123456789012345678901234abcdef01020305', N'Versión final para firma', @uDiana, DATEADD(DAY,-20,GETDATE()), 1, @uDiana, '.pdf', 'application/pdf', 768000);

IF NOT EXISTS (SELECT 1 FROM Documento_Version WHERE IdDocumento=@dT7 AND NumeroVersion=1)
    INSERT INTO Documento_Version (IdDocumento, NumeroVersion, RutaArchivoFisico, HashDocumento, MotivoCambio, IdUsuarioSube, FechaSubida, Estatus, IdUsuarioCreacion, ExtensionArchivo, MimeType, TamanoBytes)
    VALUES (@dT7, 1, '/archivos/techcorp/TC-CON-002_v1.pdf', 'b6789012345678901234567890123456789012345678901234abcdef01020306', N'Contrato colectivo 2026 firmado', @uLucia, DATEADD(DAY,-45,GETDATE()), 1, @uLucia, '.pdf', 'application/pdf', 3145728);

IF NOT EXISTS (SELECT 1 FROM Documento_Version WHERE IdDocumento=@dT8 AND NumeroVersion=1)
    INSERT INTO Documento_Version (IdDocumento, NumeroVersion, RutaArchivoFisico, HashDocumento, MotivoCambio, IdUsuarioSube, FechaSubida, Estatus, IdUsuarioCreacion, ExtensionArchivo, MimeType, TamanoBytes)
    VALUES (@dT8, 1, '/archivos/techcorp/TC-ADM-001_v1.pdf', 'c6789012345678901234567890123456789012345678901234abcdef01020307', N'Primera versión para aprobación', @AdminTech, DATEADD(DAY,-50,GETDATE()), 1, @AdminTech, '.pdf', 'application/pdf', 409600);

PRINT '  ✓ Versiones TechCorp creadas.';

-- ================================================================
-- SECCIÓN H: FLUJOS DE APROBACIÓN TECHCORP
-- ================================================================

DECLARE @vT1v1 INT = (SELECT Id FROM Documento_Version WHERE IdDocumento=@dT1 AND NumeroVersion=1);
DECLARE @vT1v2 INT = (SELECT Id FROM Documento_Version WHERE IdDocumento=@dT1 AND NumeroVersion=2);
DECLARE @vT2v1 INT = (SELECT Id FROM Documento_Version WHERE IdDocumento=@dT2 AND NumeroVersion=1);
DECLARE @vT3v1 INT = (SELECT Id FROM Documento_Version WHERE IdDocumento=@dT3 AND NumeroVersion=1);
DECLARE @vT4v1 INT = (SELECT Id FROM Documento_Version WHERE IdDocumento=@dT4 AND NumeroVersion=1);
DECLARE @vT6v1 INT = (SELECT Id FROM Documento_Version WHERE IdDocumento=@dT6 AND NumeroVersion=1);
DECLARE @vT7v1 INT = (SELECT Id FROM Documento_Version WHERE IdDocumento=@dT7 AND NumeroVersion=1);
DECLARE @vT8v1 INT = (SELECT Id FROM Documento_Version WHERE IdDocumento=@dT8 AND NumeroVersion=1);

-- Flujo T1v2: Aprobado (2 pasos: revisión + firma)
IF NOT EXISTS (SELECT 1 FROM Flujo_Aprobacion WHERE IdVersionDocumento=@vT1v2)
BEGIN
    INSERT INTO Flujo_Aprobacion (IdVersionDocumento, IdUsuarioAsignado, TipoAccion, EstadoFirma, Orden, Comentarios, FechaFirma, Estatus, FechaCreacion, IdUsuarioCreacion, TokenFirma, MetodoAutenticacion)
    VALUES
    (@vT1v2, @uSofia, 'Revisión',  'Aprobado', 1, N'Revisado. Comandos verificados en entorno de prueba.', DATEADD(DAY,-55,GETDATE()), 1, DATEADD(DAY,-60,GETDATE()), @uMarco, 'TKN-TC-MT001-V2-01', 'Contraseña'),
    (@vT1v2, @uRaul,  'Aprobación','Aprobado', 2, N'Aprobado para publicación.', DATEADD(DAY,-53,GETDATE()), 1, DATEADD(DAY,-60,GETDATE()), @uMarco, 'TKN-TC-MT001-V2-02', 'Contraseña');
END

-- Flujo T2v1: Aprobado y firmado
IF NOT EXISTS (SELECT 1 FROM Flujo_Aprobacion WHERE IdVersionDocumento=@vT2v1)
BEGIN
    INSERT INTO Flujo_Aprobacion (IdVersionDocumento, IdUsuarioAsignado, TipoAccion, EstadoFirma, Orden, Comentarios, FechaFirma, Estatus, FechaCreacion, IdUsuarioCreacion, TokenFirma, MetodoAutenticacion)
    VALUES
    (@vT2v1, @uSofia,    'Revisión',  'Aprobado', 1, N'Política revisada y alineada con ISO 27001.', DATEADD(DAY,-65,GETDATE()), 1, DATEADD(DAY,-70,GETDATE()), @uMarco, 'TKN-TC-PI001-V1-01', 'Contraseña'),
    (@vT2v1, @uRaul,     'Aprobación','Aprobado', 2, N'Aprobada por Dirección.', DATEADD(DAY,-63,GETDATE()), 1, DATEADD(DAY,-70,GETDATE()), @uMarco, 'TKN-TC-PI001-V1-02', 'Contraseña'),
    (@vT2v1, @AdminTech, 'Firma',     'Aprobado', 3, N'Firmado digitalmente por el Administrador.', DATEADD(DAY,-62,GETDATE()), 1, DATEADD(DAY,-70,GETDATE()), @uMarco, 'TKN-TC-PI001-V1-03', 'Contraseña');
END

-- Flujo T3v1: En revisión (pendiente)
IF NOT EXISTS (SELECT 1 FROM Flujo_Aprobacion WHERE IdVersionDocumento=@vT3v1)
BEGIN
    INSERT INTO Flujo_Aprobacion (IdVersionDocumento, IdUsuarioAsignado, TipoAccion, EstadoFirma, Orden, Estatus, FechaCreacion, IdUsuarioCreacion)
    VALUES
    (@vT3v1, @uRaul,  'Revisión',  'Pendiente', 1, 1, DATEADD(DAY,-30,GETDATE()), @uLucia),
    (@vT3v1, @uSofia, 'Aprobación','Pendiente', 2, 1, DATEADD(DAY,-30,GETDATE()), @uLucia);
END

-- Flujo T6v1: Pendiente Firma
IF NOT EXISTS (SELECT 1 FROM Flujo_Aprobacion WHERE IdVersionDocumento=@vT6v1)
BEGIN
    INSERT INTO Flujo_Aprobacion (IdVersionDocumento, IdUsuarioAsignado, TipoAccion, EstadoFirma, Orden, Comentarios, FechaFirma, Estatus, FechaCreacion, IdUsuarioCreacion, TokenFirma, MetodoAutenticacion)
    VALUES
    (@vT6v1, @uSofia, 'Revisión', 'Aprobado', 1, N'Política revisada por asesor legal.', DATEADD(DAY,-18,GETDATE()), 1, DATEADD(DAY,-20,GETDATE()), @uDiana, 'TKN-TC-LEG001-V1-01', 'Contraseña');
    INSERT INTO Flujo_Aprobacion (IdVersionDocumento, IdUsuarioAsignado, TipoAccion, EstadoFirma, Orden, Estatus, FechaCreacion, IdUsuarioCreacion)
    VALUES (@vT6v1, @AdminTech, 'Firma', 'Pendiente', 2, 1, DATEADD(DAY,-20,GETDATE()), @uDiana);
END

-- Flujo T8v1: Rechazado
IF NOT EXISTS (SELECT 1 FROM Flujo_Aprobacion WHERE IdVersionDocumento=@vT8v1)
BEGIN
    INSERT INTO Flujo_Aprobacion (IdVersionDocumento, IdUsuarioAsignado, TipoAccion, EstadoFirma, Orden, Comentarios, FechaFirma, Estatus, FechaCreacion, IdUsuarioCreacion, TokenFirma, MetodoAutenticacion)
    VALUES
    (@vT8v1, @uSofia, 'Revisión', 'Rechazado', 1, N'El documento no sigue el formato corporativo. Requiere revisión completa de estructura.', DATEADD(DAY,-45,GETDATE()), 1, DATEADD(DAY,-50,GETDATE()), @AdminTech, 'TKN-TC-ADM001-V1-01', 'Contraseña');
END

PRINT '  ✓ Flujos de aprobación TechCorp creados.';

-- ================================================================
-- SECCIÓN I: DOCUMENTOS GRUPO INNOVAR (8 documentos)
-- ================================================================
PRINT '--- Documentos Grupo Innovar ---';

DECLARE @dI1 INT;
IF NOT EXISTS (SELECT 1 FROM Documento WHERE CodigoInterno='GI-RF-001')
BEGIN
    INSERT INTO Documento (CodigoInterno, Titulo, IdDepartamento, EstadoActual, IdUsuarioPropietario,
        FechaCreacion, Estatus, IdUsuarioCreacion, IdTipoDocumento, IdEmpresa)
    VALUES ('GI-RF-001', N'Reporte Financiero Q1 2026', @DFinInnov, 'Vigente',
        @vPatricia, DATEADD(DAY,-80,GETDATE()), 1, @vPatricia, @TRFInnov, @EmpInnov);
END
SET @dI1 = (SELECT Id FROM Documento WHERE CodigoInterno='GI-RF-001');

DECLARE @dI2 INT;
IF NOT EXISTS (SELECT 1 FROM Documento WHERE CodigoInterno='GI-RF-002')
BEGIN
    INSERT INTO Documento (CodigoInterno, Titulo, IdDepartamento, EstadoActual, IdUsuarioPropietario,
        FechaCreacion, Estatus, IdUsuarioCreacion, IdTipoDocumento, IdEmpresa)
    VALUES ('GI-RF-002', N'Reporte Financiero Q2 2026', @DFinInnov, 'En Revisión',
        @vPatricia, DATEADD(DAY,-15,GETDATE()), 1, @vPatricia, @TRFInnov, @EmpInnov);
END
SET @dI2 = (SELECT Id FROM Documento WHERE CodigoInterno='GI-RF-002');

DECLARE @dI3 INT;
IF NOT EXISTS (SELECT 1 FROM Documento WHERE CodigoInterno='GI-AR-001')
BEGIN
    INSERT INTO Documento (CodigoInterno, Titulo, IdDepartamento, EstadoActual, IdUsuarioPropietario,
        FechaCreacion, Estatus, IdUsuarioCreacion, IdTipoDocumento, IdEmpresa)
    VALUES ('GI-AR-001', N'Acta Reunión Consejo Directivo – Enero 2026', @DAdmInnov, 'Vigente',
        @AdminInnov, DATEADD(DAY,-75,GETDATE()), 1, @AdminInnov, @TARInnov, @EmpInnov);
END
SET @dI3 = (SELECT Id FROM Documento WHERE CodigoInterno='GI-AR-001');

DECLARE @dI4 INT;
IF NOT EXISTS (SELECT 1 FROM Documento WHERE CodigoInterno='GI-PO-001')
BEGIN
    INSERT INTO Documento (CodigoInterno, Titulo, IdDepartamento, EstadoActual, IdUsuarioPropietario,
        FechaCreacion, Estatus, IdUsuarioCreacion, IdTipoDocumento, IdEmpresa)
    VALUES ('GI-PO-001', N'Procedimiento de Control de Calidad en Línea de Producción', @DOpsInnov, 'Vigente',
        @vIsabel, DATEADD(DAY,-65,GETDATE()), 1, @vIsabel, @TPOInnov, @EmpInnov);
END
SET @dI4 = (SELECT Id FROM Documento WHERE CodigoInterno='GI-PO-001');

DECLARE @dI5 INT;
IF NOT EXISTS (SELECT 1 FROM Documento WHERE CodigoInterno='GI-PO-002')
BEGIN
    INSERT INTO Documento (CodigoInterno, Titulo, IdDepartamento, EstadoActual, IdUsuarioPropietario,
        FechaCreacion, Estatus, IdUsuarioCreacion, IdTipoDocumento, IdEmpresa)
    VALUES ('GI-PO-002', N'Procedimiento de Gestión de Proveedores', @DOpsInnov, 'Borrador',
        @vIsabel, DATEADD(DAY,-8,GETDATE()), 1, @vIsabel, @TPOInnov, @EmpInnov);
END
SET @dI5 = (SELECT Id FROM Documento WHERE CodigoInterno='GI-PO-002');

DECLARE @dI6 INT;
IF NOT EXISTS (SELECT 1 FROM Documento WHERE CodigoInterno='GI-AR-002')
BEGIN
    INSERT INTO Documento (CodigoInterno, Titulo, IdDepartamento, EstadoActual, IdUsuarioPropietario,
        FechaCreacion, Estatus, IdUsuarioCreacion, IdTipoDocumento, IdEmpresa)
    VALUES ('GI-AR-002', N'Acta Reunión Comercial – Plan de Ventas 2026', @DCompInnov, 'Pendiente Firma',
        @vElena, DATEADD(DAY,-25,GETDATE()), 1, @vElena, @TARInnov, @EmpInnov);
END
SET @dI6 = (SELECT Id FROM Documento WHERE CodigoInterno='GI-AR-002');

DECLARE @dI7 INT;
IF NOT EXISTS (SELECT 1 FROM Documento WHERE CodigoInterno='GI-RF-003')
BEGIN
    INSERT INTO Documento (CodigoInterno, Titulo, IdDepartamento, EstadoActual, IdUsuarioPropietario,
        FechaCreacion, Estatus, IdUsuarioCreacion, IdTipoDocumento, IdEmpresa)
    VALUES ('GI-RF-003', N'Presupuesto Anual 2026 – Proyección vs Real', @DFinInnov, 'Vigente',
        @vPatricia, DATEADD(DAY,-50,GETDATE()), 1, @vPatricia, @TRFInnov, @EmpInnov);
END
SET @dI7 = (SELECT Id FROM Documento WHERE CodigoInterno='GI-RF-003');

DECLARE @dI8 INT;
IF NOT EXISTS (SELECT 1 FROM Documento WHERE CodigoInterno='GI-PO-003')
BEGIN
    INSERT INTO Documento (CodigoInterno, Titulo, IdDepartamento, EstadoActual, IdUsuarioPropietario,
        FechaCreacion, Estatus, IdUsuarioCreacion, IdTipoDocumento, IdEmpresa)
    VALUES ('GI-PO-003', N'Procedimiento de Atención a Quejas y Reclamaciones', @DCompInnov, 'Vigente',
        @vElena, DATEADD(DAY,-40,GETDATE()), 1, @vElena, @TPOInnov, @EmpInnov);
END
SET @dI8 = (SELECT Id FROM Documento WHERE CodigoInterno='GI-PO-003');

PRINT '  ✓ 8 documentos Grupo Innovar creados.';

-- ================================================================
-- SECCIÓN J: VERSIONES DE DOCUMENTOS INNOVAR
-- ================================================================

IF NOT EXISTS (SELECT 1 FROM Documento_Version WHERE IdDocumento=@dI1 AND NumeroVersion=1)
    INSERT INTO Documento_Version (IdDocumento, NumeroVersion, RutaArchivoFisico, HashDocumento, MotivoCambio, IdUsuarioSube, FechaSubida, Estatus, IdUsuarioCreacion, ExtensionArchivo, MimeType, TamanoBytes)
    VALUES (@dI1, 1, '/archivos/innovar/GI-RF-001_v1.xlsx', 'a7891012345678901234567890123456789012345678901234abcdef01020308', N'Reporte Q1 definitivo', @vPatricia, DATEADD(DAY,-80,GETDATE()), 1, @vPatricia, '.xlsx', 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', 2621440);

IF NOT EXISTS (SELECT 1 FROM Documento_Version WHERE IdDocumento=@dI2 AND NumeroVersion=1)
    INSERT INTO Documento_Version (IdDocumento, NumeroVersion, RutaArchivoFisico, HashDocumento, MotivoCambio, IdUsuarioSube, FechaSubida, Estatus, IdUsuarioCreacion, ExtensionArchivo, MimeType, TamanoBytes)
    VALUES (@dI2, 1, '/archivos/innovar/GI-RF-002_v1.xlsx', 'b7891012345678901234567890123456789012345678901234abcdef01020309', N'Primer borrador Q2', @vPatricia, DATEADD(DAY,-15,GETDATE()), 1, @vPatricia, '.xlsx', 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', 2883584);

IF NOT EXISTS (SELECT 1 FROM Documento_Version WHERE IdDocumento=@dI3 AND NumeroVersion=1)
    INSERT INTO Documento_Version (IdDocumento, NumeroVersion, RutaArchivoFisico, HashDocumento, MotivoCambio, IdUsuarioSube, FechaSubida, Estatus, IdUsuarioCreacion, ExtensionArchivo, MimeType, TamanoBytes)
    VALUES (@dI3, 1, '/archivos/innovar/GI-AR-001_v1.pdf', 'c7891012345678901234567890123456789012345678901234abcdef01020310', N'Acta firmada en sesión', @AdminInnov, DATEADD(DAY,-75,GETDATE()), 1, @AdminInnov, '.pdf', 'application/pdf', 614400);

IF NOT EXISTS (SELECT 1 FROM Documento_Version WHERE IdDocumento=@dI4 AND NumeroVersion=1)
BEGIN
    INSERT INTO Documento_Version (IdDocumento, NumeroVersion, RutaArchivoFisico, HashDocumento, MotivoCambio, IdUsuarioSube, FechaSubida, Estatus, IdUsuarioCreacion, ExtensionArchivo, MimeType, TamanoBytes)
    VALUES (@dI4, 1, '/archivos/innovar/GI-PO-001_v1.pdf', 'd7891012345678901234567890123456789012345678901234abcdef01020311', N'Procedimiento inicial', @vIsabel, DATEADD(DAY,-65,GETDATE()), 1, @vIsabel, '.pdf', 'application/pdf', 819200);

    INSERT INTO Documento_Version (IdDocumento, NumeroVersion, RutaArchivoFisico, HashDocumento, MotivoCambio, IdUsuarioSube, FechaSubida, Estatus, IdUsuarioCreacion, ExtensionArchivo, MimeType, TamanoBytes)
    VALUES (@dI4, 2, '/archivos/innovar/GI-PO-001_v2.pdf', 'e7891012345678901234567890123456789012345678901234abcdef01020312', N'Ajuste de tolerancias según auditoría interna', @vIsabel, DATEADD(DAY,-40,GETDATE()), 1, @vIsabel, '.pdf', 'application/pdf', 860160);
END

IF NOT EXISTS (SELECT 1 FROM Documento_Version WHERE IdDocumento=@dI5 AND NumeroVersion=1)
    INSERT INTO Documento_Version (IdDocumento, NumeroVersion, RutaArchivoFisico, HashDocumento, MotivoCambio, IdUsuarioSube, FechaSubida, Estatus, IdUsuarioCreacion, ExtensionArchivo, MimeType, TamanoBytes)
    VALUES (@dI5, 1, '/archivos/innovar/GI-PO-002_v1.docx', 'f7891012345678901234567890123456789012345678901234abcdef01020313', N'Borrador inicial', @vIsabel, DATEADD(DAY,-8,GETDATE()), 1, @vIsabel, '.docx', 'application/vnd.openxmlformats-officedocument.wordprocessingml.document', 163840);

IF NOT EXISTS (SELECT 1 FROM Documento_Version WHERE IdDocumento=@dI6 AND NumeroVersion=1)
    INSERT INTO Documento_Version (IdDocumento, NumeroVersion, RutaArchivoFisico, HashDocumento, MotivoCambio, IdUsuarioSube, FechaSubida, Estatus, IdUsuarioCreacion, ExtensionArchivo, MimeType, TamanoBytes)
    VALUES (@dI6, 1, '/archivos/innovar/GI-AR-002_v1.pdf', 'a8891012345678901234567890123456789012345678901234abcdef01020314', N'Acta de reunión comercial', @vElena, DATEADD(DAY,-25,GETDATE()), 1, @vElena, '.pdf', 'application/pdf', 307200);

IF NOT EXISTS (SELECT 1 FROM Documento_Version WHERE IdDocumento=@dI7 AND NumeroVersion=1)
    INSERT INTO Documento_Version (IdDocumento, NumeroVersion, RutaArchivoFisico, HashDocumento, MotivoCambio, IdUsuarioSube, FechaSubida, Estatus, IdUsuarioCreacion, ExtensionArchivo, MimeType, TamanoBytes)
    VALUES (@dI7, 1, '/archivos/innovar/GI-RF-003_v1.xlsx', 'b8891012345678901234567890123456789012345678901234abcdef01020315', N'Proyección anual vs. gastos reales', @vPatricia, DATEADD(DAY,-50,GETDATE()), 1, @vPatricia, '.xlsx', 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', 3670016);

IF NOT EXISTS (SELECT 1 FROM Documento_Version WHERE IdDocumento=@dI8 AND NumeroVersion=1)
    INSERT INTO Documento_Version (IdDocumento, NumeroVersion, RutaArchivoFisico, HashDocumento, MotivoCambio, IdUsuarioSube, FechaSubida, Estatus, IdUsuarioCreacion, ExtensionArchivo, MimeType, TamanoBytes)
    VALUES (@dI8, 1, '/archivos/innovar/GI-PO-003_v1.pdf', 'c8891012345678901234567890123456789012345678901234abcdef01020316', N'Procedimiento de atención al cliente', @vElena, DATEADD(DAY,-40,GETDATE()), 1, @vElena, '.pdf', 'application/pdf', 512000);

PRINT '  ✓ Versiones Grupo Innovar creadas.';

-- ================================================================
-- SECCIÓN K: FLUJOS DE APROBACIÓN INNOVAR
-- ================================================================
DECLARE @vI1v1 INT = (SELECT Id FROM Documento_Version WHERE IdDocumento=@dI1 AND NumeroVersion=1);
DECLARE @vI2v1 INT = (SELECT Id FROM Documento_Version WHERE IdDocumento=@dI2 AND NumeroVersion=1);
DECLARE @vI3v1 INT = (SELECT Id FROM Documento_Version WHERE IdDocumento=@dI3 AND NumeroVersion=1);
DECLARE @vI4v2 INT = (SELECT Id FROM Documento_Version WHERE IdDocumento=@dI4 AND NumeroVersion=2);
DECLARE @vI6v1 INT = (SELECT Id FROM Documento_Version WHERE IdDocumento=@dI6 AND NumeroVersion=1);
DECLARE @vI7v1 INT = (SELECT Id FROM Documento_Version WHERE IdDocumento=@dI7 AND NumeroVersion=1);
DECLARE @vI8v1 INT = (SELECT Id FROM Documento_Version WHERE IdDocumento=@dI8 AND NumeroVersion=1);

-- Flujo I1v1: Reporte Q1 Aprobado
IF NOT EXISTS (SELECT 1 FROM Flujo_Aprobacion WHERE IdVersionDocumento=@vI1v1)
BEGIN
    INSERT INTO Flujo_Aprobacion (IdVersionDocumento, IdUsuarioAsignado, TipoAccion, EstadoFirma, Orden, Comentarios, FechaFirma, Estatus, FechaCreacion, IdUsuarioCreacion, TokenFirma, MetodoAutenticacion)
    VALUES
    (@vI1v1, @vErnesto,  'Revisión',  'Aprobado', 1, N'Cifras verificadas contra estados de cuenta bancarios.', DATEADD(DAY,-75,GETDATE()), 1, DATEADD(DAY,-80,GETDATE()), @vPatricia, 'TKN-GI-RF001-V1-01', 'Contraseña'),
    (@vI1v1, @AdminInnov,'Aprobación','Aprobado', 2, N'Reporte Q1 aprobado para publicación interna.', DATEADD(DAY,-73,GETDATE()), 1, DATEADD(DAY,-80,GETDATE()), @vPatricia, 'TKN-GI-RF001-V1-02', 'Contraseña');
END

-- Flujo I2v1: En revisión
IF NOT EXISTS (SELECT 1 FROM Flujo_Aprobacion WHERE IdVersionDocumento=@vI2v1)
BEGIN
    INSERT INTO Flujo_Aprobacion (IdVersionDocumento, IdUsuarioAsignado, TipoAccion, EstadoFirma, Orden, Estatus, FechaCreacion, IdUsuarioCreacion)
    VALUES
    (@vI2v1, @vErnesto,  'Revisión',  'Pendiente', 1, 1, DATEADD(DAY,-15,GETDATE()), @vPatricia),
    (@vI2v1, @AdminInnov,'Aprobación','Pendiente', 2, 1, DATEADD(DAY,-15,GETDATE()), @vPatricia);
END

-- Flujo I3v1: Acta Consejo - Aprobada y firmada
IF NOT EXISTS (SELECT 1 FROM Flujo_Aprobacion WHERE IdVersionDocumento=@vI3v1)
BEGIN
    INSERT INTO Flujo_Aprobacion (IdVersionDocumento, IdUsuarioAsignado, TipoAccion, EstadoFirma, Orden, Comentarios, FechaFirma, Estatus, FechaCreacion, IdUsuarioCreacion, TokenFirma, MetodoAutenticacion)
    VALUES
    (@vI3v1, @vMiguel,   'Revisión',  'Aprobado', 1, N'Acta revisada. Quórum verificado.', DATEADD(DAY,-73,GETDATE()), 1, DATEADD(DAY,-75,GETDATE()), @AdminInnov, 'TKN-GI-AR001-V1-01', 'Contraseña'),
    (@vI3v1, @AdminInnov,'Firma',     'Aprobado', 2, N'Firmada por el Administrador General.', DATEADD(DAY,-72,GETDATE()), 1, DATEADD(DAY,-75,GETDATE()), @AdminInnov, 'TKN-GI-AR001-V1-02', 'Contraseña');
END

-- Flujo I4v2: PO Control de Calidad v2 - Aprobado
IF NOT EXISTS (SELECT 1 FROM Flujo_Aprobacion WHERE IdVersionDocumento=@vI4v2)
BEGIN
    INSERT INTO Flujo_Aprobacion (IdVersionDocumento, IdUsuarioAsignado, TipoAccion, EstadoFirma, Orden, Comentarios, FechaFirma, Estatus, FechaCreacion, IdUsuarioCreacion, TokenFirma, MetodoAutenticacion)
    VALUES
    (@vI4v2, @vMiguel,   'Revisión',  'Aprobado', 1, N'Tolerancias correctas según norma ISO 9001:2015.', DATEADD(DAY,-37,GETDATE()), 1, DATEADD(DAY,-40,GETDATE()), @vIsabel, 'TKN-GI-PO001-V2-01', 'Contraseña'),
    (@vI4v2, @AdminInnov,'Aprobación','Aprobado', 2, N'Procedimiento aprobado. Implementar en planta.', DATEADD(DAY,-35,GETDATE()), 1, DATEADD(DAY,-40,GETDATE()), @vIsabel, 'TKN-GI-PO001-V2-02', 'Contraseña');
END

-- Flujo I6v1: Acta Comercial - Pendiente Firma
IF NOT EXISTS (SELECT 1 FROM Flujo_Aprobacion WHERE IdVersionDocumento=@vI6v1)
BEGIN
    INSERT INTO Flujo_Aprobacion (IdVersionDocumento, IdUsuarioAsignado, TipoAccion, EstadoFirma, Orden, Comentarios, FechaFirma, Estatus, FechaCreacion, IdUsuarioCreacion, TokenFirma, MetodoAutenticacion)
    VALUES (@vI6v1, @vMiguel, 'Revisión', 'Aprobado', 1, N'Plan de ventas viable. Metas alineadas con presupuesto.', DATEADD(DAY,-22,GETDATE()), 1, DATEADD(DAY,-25,GETDATE()), @vElena, 'TKN-GI-AR002-V1-01', 'Contraseña');
    INSERT INTO Flujo_Aprobacion (IdVersionDocumento, IdUsuarioAsignado, TipoAccion, EstadoFirma, Orden, Estatus, FechaCreacion, IdUsuarioCreacion)
    VALUES (@vI6v1, @AdminInnov, 'Firma', 'Pendiente', 2, 1, DATEADD(DAY,-25,GETDATE()), @vElena);
END

-- Flujo I7v1: Presupuesto Anual - Aprobado
IF NOT EXISTS (SELECT 1 FROM Flujo_Aprobacion WHERE IdVersionDocumento=@vI7v1)
BEGIN
    INSERT INTO Flujo_Aprobacion (IdVersionDocumento, IdUsuarioAsignado, TipoAccion, EstadoFirma, Orden, Comentarios, FechaFirma, Estatus, FechaCreacion, IdUsuarioCreacion, TokenFirma, MetodoAutenticacion)
    VALUES
    (@vI7v1, @vErnesto,  'Revisión',  'Aprobado', 1, N'Varianza del 3.2% dentro del rango aceptable.', DATEADD(DAY,-46,GETDATE()), 1, DATEADD(DAY,-50,GETDATE()), @vPatricia, 'TKN-GI-RF003-V1-01', 'Contraseña'),
    (@vI7v1, @AdminInnov,'Aprobación','Aprobado', 2, N'Presupuesto aprobado. Ajustes para Q3 autorizados.', DATEADD(DAY,-44,GETDATE()), 1, DATEADD(DAY,-50,GETDATE()), @vPatricia, 'TKN-GI-RF003-V1-02', 'Contraseña');
END

-- Flujo I8v1: PO Atención Quejas - Aprobado
IF NOT EXISTS (SELECT 1 FROM Flujo_Aprobacion WHERE IdVersionDocumento=@vI8v1)
BEGIN
    INSERT INTO Flujo_Aprobacion (IdVersionDocumento, IdUsuarioAsignado, TipoAccion, EstadoFirma, Orden, Comentarios, FechaFirma, Estatus, FechaCreacion, IdUsuarioCreacion, TokenFirma, MetodoAutenticacion)
    VALUES
    (@vI8v1, @vMiguel,   'Revisión',  'Aprobado', 1, N'Tiempos de respuesta ajustados a requerimiento legal.', DATEADD(DAY,-37,GETDATE()), 1, DATEADD(DAY,-40,GETDATE()), @vElena, 'TKN-GI-PO003-V1-01', 'Contraseña'),
    (@vI8v1, @AdminInnov,'Firma',     'Aprobado', 2, N'Aprobado y firmado. Vigente desde hoy.', DATEADD(DAY,-36,GETDATE()), 1, DATEADD(DAY,-40,GETDATE()), @vElena, 'TKN-GI-PO003-V1-02', 'Contraseña');
END

PRINT '  ✓ Flujos Grupo Innovar creados.';

-- ================================================================
-- SECCIÓN L: BITÁCORAS DE ACCESO
-- ================================================================
PRINT '--- Bitácoras de Acceso ---';

IF NOT EXISTS (SELECT 1 FROM BitacoraAcceso WHERE IdUsuario=@uMarco)
BEGIN
    INSERT INTO BitacoraAcceso (IdUsuario, FechaHoraIntento, EstadoIntento, DireccionIP, Estatus)
    VALUES
    (@uMarco,    DATEADD(DAY,-5,GETDATE()),  'Exitoso', '192.168.1.101', 1),
    (@uMarco,    DATEADD(DAY,-3,GETDATE()),  'Exitoso', '192.168.1.101', 1),
    (@uMarco,    DATEADD(DAY,-1,GETDATE()),  'Exitoso', '192.168.1.101', 1),
    (@uSofia,    DATEADD(DAY,-4,GETDATE()),  'Exitoso', '192.168.1.102', 1),
    (@uSofia,    DATEADD(DAY,-2,GETDATE()),  'Exitoso', '192.168.1.102', 1),
    (@uLucia,    DATEADD(DAY,-6,GETDATE()),  'Exitoso', '192.168.1.103', 1),
    (@uLucia,    DATEADD(DAY,-2,GETDATE()),  'Exitoso', '192.168.1.103', 1),
    (@uJorge,    DATEADD(DAY,-3,GETDATE()),  'Exitoso', '192.168.1.104', 1),
    (@uDiana,    DATEADD(DAY,-1,GETDATE()),  'Exitoso', '192.168.1.105', 1),
    (@uRaul,     DATEADD(DAY,-5,GETDATE()),  'Exitoso', '192.168.1.106', 1),
    (@AdminTech, DATEADD(DAY,-1,GETDATE()),  'Exitoso', '192.168.1.100', 1),
    -- Intento fallido (contraseña incorrecta)
    (@uMarco,    DATEADD(HOUR,-3,GETDATE()), 'Fallido', '192.168.1.101', 1),
    -- Innovar
    (@vPatricia, DATEADD(DAY,-4,GETDATE()),  'Exitoso', '10.0.0.101', 1),
    (@vPatricia, DATEADD(DAY,-1,GETDATE()),  'Exitoso', '10.0.0.101', 1),
    (@vErnesto,  DATEADD(DAY,-3,GETDATE()),  'Exitoso', '10.0.0.102', 1),
    (@vIsabel,   DATEADD(DAY,-2,GETDATE()),  'Exitoso', '10.0.0.103', 1),
    (@vRoberto,  DATEADD(DAY,-5,GETDATE()),  'Exitoso', '10.0.0.104', 1),
    (@vElena,    DATEADD(DAY,-1,GETDATE()),  'Exitoso', '10.0.0.105', 1),
    (@vMiguel,   DATEADD(DAY,-3,GETDATE()),  'Exitoso', '10.0.0.106', 1),
    (@AdminInnov,DATEADD(DAY,-1,GETDATE()),  'Exitoso', '10.0.0.100', 1);
END

PRINT '  ✓ Bitácoras de acceso creadas.';

-- ================================================================
-- SECCIÓN M: BITÁCORAS TRANSACCIONALES (actividad de documentos)
-- ================================================================
PRINT '--- Bitácoras Transaccionales ---';

IF NOT EXISTS (SELECT 1 FROM BitacoraTransaccional WHERE IdDocumento=@dT1 AND Accion='CREAR_DOCUMENTO')
BEGIN
    -- TechCorp: creación y actividad de documentos
    INSERT INTO BitacoraTransaccional (IdUsuario, IdDocumento, Accion, FechaHora, Detalle, Estatus, IdUsuarioCreacion)
    VALUES
    (@uMarco, @dT1, 'CREAR_DOCUMENTO', DATEADD(DAY,-85,GETDATE()), N'Documento TC-MT-001 creado', 1, @uMarco),
    (@uMarco, @dT1, 'SUBIR_VERSION',   DATEADD(DAY,-85,GETDATE()), N'Versión v1 subida para TC-MT-001', 1, @uMarco),
    (@uMarco, @dT1, 'SUBIR_VERSION',   DATEADD(DAY,-60,GETDATE()), N'Versión v2 subida con correcciones de red', 1, @uMarco),
    (@uMarco, @dT2, 'CREAR_DOCUMENTO', DATEADD(DAY,-70,GETDATE()), N'Documento TC-PI-001 creado', 1, @uMarco),
    (@uMarco, @dT2, 'SUBIR_VERSION',   DATEADD(DAY,-70,GETDATE()), N'Versión v1 subida para TC-PI-001', 1, @uMarco),
    (@uLucia, @dT3, 'CREAR_DOCUMENTO', DATEADD(DAY,-30,GETDATE()), N'Documento TC-PI-002 creado', 1, @uLucia),
    (@uDiana, @dT4, 'CREAR_DOCUMENTO', DATEADD(DAY,-60,GETDATE()), N'Contrato TC-CON-001 subido', 1, @uDiana),
    (@uDiana, @dT6, 'CREAR_DOCUMENTO', DATEADD(DAY,-20,GETDATE()), N'Política de privacidad lista para firma', 1, @uDiana),
    (@uLucia, @dT7, 'CREAR_DOCUMENTO', DATEADD(DAY,-45,GETDATE()), N'Contrato colectivo 2026 cargado', 1, @uLucia),
    -- TechCorp: cambios de estado
    (@uSofia, @dT1, 'CAMBIO_ESTADO',   DATEADD(DAY,-53,GETDATE()), N'Documento TC-MT-001 aprobado y vigente', 1, @uSofia),
    (@uSofia, @dT2, 'CAMBIO_ESTADO',   DATEADD(DAY,-62,GETDATE()), N'Política TC-PI-001 vigente', 1, @uSofia),
    (@uSofia, @dT8, 'CAMBIO_ESTADO',   DATEADD(DAY,-45,GETDATE()), N'Documento TC-ADM-001 rechazado', 1, @uSofia),
    -- Innovar: creación de documentos
    (@vPatricia, @dI1, 'CREAR_DOCUMENTO', DATEADD(DAY,-80,GETDATE()), N'Reporte Q1 creado', 1, @vPatricia),
    (@vPatricia, @dI1, 'SUBIR_VERSION',   DATEADD(DAY,-80,GETDATE()), N'Versión v1 subida para GI-RF-001', 1, @vPatricia),
    (@AdminInnov,@dI3, 'CREAR_DOCUMENTO', DATEADD(DAY,-75,GETDATE()), N'Acta Consejo Enero creada', 1, @AdminInnov),
    (@vIsabel,   @dI4, 'CREAR_DOCUMENTO', DATEADD(DAY,-65,GETDATE()), N'Procedimiento Control Calidad creado', 1, @vIsabel),
    (@vIsabel,   @dI4, 'SUBIR_VERSION',   DATEADD(DAY,-40,GETDATE()), N'Versión v2 con ajustes de tolerancia', 1, @vIsabel),
    (@vElena,    @dI6, 'CREAR_DOCUMENTO', DATEADD(DAY,-25,GETDATE()), N'Acta reunión comercial creada', 1, @vElena),
    (@vPatricia, @dI7, 'CREAR_DOCUMENTO', DATEADD(DAY,-50,GETDATE()), N'Presupuesto Anual 2026 creado', 1, @vPatricia),
    -- Innovar: cambios de estado
    (@AdminInnov, @dI1, 'CAMBIO_ESTADO', DATEADD(DAY,-73,GETDATE()), N'Reporte Q1 aprobado y vigente', 1, @AdminInnov),
    (@AdminInnov, @dI3, 'CAMBIO_ESTADO', DATEADD(DAY,-72,GETDATE()), N'Acta Consejo firmada y vigente', 1, @AdminInnov),
    (@AdminInnov, @dI4, 'CAMBIO_ESTADO', DATEADD(DAY,-35,GETDATE()), N'PO Control Calidad v2 vigente', 1, @AdminInnov),
    (@AdminInnov, @dI7, 'CAMBIO_ESTADO', DATEADD(DAY,-44,GETDATE()), N'Presupuesto Anual aprobado', 1, @AdminInnov),
    (@AdminInnov, @dI8, 'CAMBIO_ESTADO', DATEADD(DAY,-36,GETDATE()), N'PO Atención Quejas vigente y firmado', 1, @AdminInnov);
END

PRINT '  ✓ Bitácoras transaccionales creadas.';

-- ================================================================
-- SECCIÓN N: BITÁCORAS DE CONTROL DE DOCUMENTOS (audit trail detallado)
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM BitacoraControlDocumento WHERE IdDocumento=@dT1 AND TipoCambio='Estado')
BEGIN
    INSERT INTO BitacoraControlDocumento (IdDocumento, IdUsuarioAccion, TipoCambio, ValorAnterior, ValorNuevo, FechaEvento, Estatus, IdUsuarioCreacion)
    VALUES
    -- TechCorp
    (@dT1, @uMarco, 'Estado', 'Borrador', 'En Revisión',  DATEADD(DAY,-84,GETDATE()), 1, @uMarco),
    (@dT1, @uSofia, 'Estado', 'En Revisión', 'Pendiente Firma', DATEADD(DAY,-55,GETDATE()), 1, @uSofia),
    (@dT1, @uRaul,  'Estado', 'Pendiente Firma', 'Vigente', DATEADD(DAY,-53,GETDATE()), 1, @uRaul),
    (@dT2, @uMarco, 'Estado', 'Borrador', 'En Revisión',  DATEADD(DAY,-69,GETDATE()), 1, @uMarco),
    (@dT2, @uSofia, 'Estado', 'En Revisión', 'Pendiente Firma', DATEADD(DAY,-63,GETDATE()), 1, @uSofia),
    (@dT2, @AdminTech,'Estado','Pendiente Firma','Vigente', DATEADD(DAY,-62,GETDATE()), 1, @AdminTech),
    (@dT3, @uLucia, 'Estado', 'Borrador', 'En Revisión',  DATEADD(DAY,-29,GETDATE()), 1, @uLucia),
    (@dT8, @AdminTech,'Estado','Borrador','En Revisión',   DATEADD(DAY,-49,GETDATE()), 1, @AdminTech),
    (@dT8, @uSofia, 'Estado', 'En Revisión', 'Rechazado', DATEADD(DAY,-45,GETDATE()), 1, @uSofia),
    -- Innovar
    (@dI1, @vPatricia,'Estado','Borrador','En Revisión',   DATEADD(DAY,-79,GETDATE()), 1, @vPatricia),
    (@dI1, @vErnesto, 'Estado','En Revisión','Pendiente Firma', DATEADD(DAY,-75,GETDATE()), 1, @vErnesto),
    (@dI1, @AdminInnov,'Estado','Pendiente Firma','Vigente', DATEADD(DAY,-73,GETDATE()), 1, @AdminInnov),
    (@dI3, @AdminInnov,'Estado','Borrador','Pendiente Firma', DATEADD(DAY,-74,GETDATE()), 1, @AdminInnov),
    (@dI3, @AdminInnov,'Estado','Pendiente Firma','Vigente', DATEADD(DAY,-72,GETDATE()), 1, @AdminInnov),
    (@dI4, @vIsabel, 'Estado','Borrador','En Revisión',    DATEADD(DAY,-64,GETDATE()), 1, @vIsabel),
    (@dI4, @vMiguel, 'Estado','En Revisión','Vigente',     DATEADD(DAY,-35,GETDATE()), 1, @vMiguel),
    (@dI2, @vPatricia,'Estado','Borrador','En Revisión',   DATEADD(DAY,-14,GETDATE()), 1, @vPatricia);
END

PRINT '  ✓ Bitácoras de control de documentos creadas.';

-- ================================================================
-- RESUMEN FINAL
-- ================================================================
PRINT '';
PRINT '============================================================';
PRINT '  SEED COMPLETO FINALIZADO';
PRINT '============================================================';
PRINT '  TECHCORP SOLUTIONS:';
PRINT '    Admin:     admin.tech@techcorp.local       / Admin@Tech2026!';
PRINT '    Editor:    marco.torres@techcorp.local     / Test@2026!';
PRINT '    Aprobador: sofia.ramos@techcorp.local      / Test@2026!';
PRINT '    Editor:    lucia.mendoza@techcorp.local    / Test@2026!';
PRINT '    Auditor:   jorge.vargas@techcorp.local     / Test@2026!';
PRINT '    Editor:    diana.flores@techcorp.local     / Test@2026!';
PRINT '    Superior:  raul.suarez@techcorp.local      / Test@2026!';
PRINT '    Docs: 8 documentos, estados variados (Vigente/En Rev./Borrador/Rechazado/Pend.Firma)';
PRINT '------------------------------------------------------------';
PRINT '  GRUPO INNOVAR:';
PRINT '    Admin:     admin@grupoinnovar.local         / Admin@Innov2026!';
PRINT '    Editor:    patricia.luna@grupoinnovar.local / Test@2026!';
PRINT '    Aprobador: ernesto.medina@grupoinnovar.local/ Test@2026!';
PRINT '    Editor:    isabel.cano@grupoinnovar.local   / Test@2026!';
PRINT '    Auditor:   roberto.solis@grupoinnovar.local / Test@2026!';
PRINT '    Editor:    elena.ruiz@grupoinnovar.local    / Test@2026!';
PRINT '    Superior:  miguel.rojas@grupoinnovar.local  / Test@2026!';
PRINT '    Docs: 8 documentos, estados variados';
PRINT '============================================================';
