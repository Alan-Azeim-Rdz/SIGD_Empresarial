USE SIGD_Central;

-- Trigger: auditoría de nueva versión de documento
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
        SELECT i.IdUsuarioSube, i.IdDocumento, i.Id, ''SUBIR_VERSION'', GETDATE(),
               ''Nueva version v'' + CAST(i.NumeroVersion AS VARCHAR) + '' para documento Id='' + CAST(i.IdDocumento AS VARCHAR),
               i.IdUsuarioSube, 1
        FROM inserted i;
    END
    ');
    PRINT 'TRG_Auditoria_NuevaVersion creado.';
END
ELSE
    PRINT 'TRG_Auditoria_NuevaVersion ya existe.';

-- Trigger: auditoría de edición de documento
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'TRG_Auditoria_EditarDocumento')
BEGIN
    EXEC('
    CREATE TRIGGER [dbo].[TRG_Auditoria_EditarDocumento]
    ON [dbo].[Documento]
    AFTER UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        IF UPDATE(Titulo)
        BEGIN
            INSERT INTO [dbo].[BitacoraTransaccional] (IdUsuario, IdDocumento, Accion, FechaHora, Detalle, IdUsuarioCreacion, Estatus)
            SELECT ISNULL(i.IdUsuarioModificacion, i.IdUsuarioPropietario), i.Id, ''EDITAR_DOCUMENTO'', GETDATE(),
                   ''Documento Id='' + CAST(i.Id AS VARCHAR) + '' editado: '' + i.Titulo,
                   ISNULL(i.IdUsuarioModificacion, i.IdUsuarioPropietario), 1
            FROM inserted i;
        END
    END
    ');
    PRINT 'TRG_Auditoria_EditarDocumento creado.';
END
ELSE
    PRINT 'TRG_Auditoria_EditarDocumento ya existe.';

-- Trigger: auditoría de eliminación lógica de documento
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'TRG_Auditoria_EliminarDocumento')
BEGIN
    EXEC('
    CREATE TRIGGER [dbo].[TRG_Auditoria_EliminarDocumento]
    ON [dbo].[Documento]
    AFTER UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        IF UPDATE(Estatus)
        BEGIN
            INSERT INTO [dbo].[BitacoraTransaccional] (IdUsuario, IdDocumento, Accion, FechaHora, Detalle, IdUsuarioCreacion, Estatus)
            SELECT ISNULL(i.IdUsuarioEliminacion, i.IdUsuarioPropietario), i.Id, ''ELIMINAR_DOCUMENTO'', GETDATE(),
                   ''Documento Id='' + CAST(i.Id AS VARCHAR) + '' eliminado: '' + i.Titulo,
                   ISNULL(i.IdUsuarioEliminacion, i.IdUsuarioPropietario), 1
            FROM inserted i
            INNER JOIN deleted d ON i.Id = d.Id
            WHERE i.Estatus = 0 AND d.Estatus = 1;
        END
    END
    ');
    PRINT 'TRG_Auditoria_EliminarDocumento creado.';
END
ELSE
    PRINT 'TRG_Auditoria_EliminarDocumento ya existe.';

PRINT 'Triggers de auditoria completados.';
