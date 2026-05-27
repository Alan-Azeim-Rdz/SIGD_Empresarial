CREATE TRIGGER [dbo].[TRG_Auditoria_EditarDocumento]
ON [dbo].[Documento]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF UPDATE(Titulo) OR UPDATE(EstadoActual) OR UPDATE(CamposPersonalizadosValores)
    BEGIN
        INSERT INTO [dbo].[BitacoraTransaccional] (IdUsuario, IdDocumento, Accion, FechaHora, Detalle, IdUsuarioCreacion, Estatus)
        SELECT 
            ISNULL(i.IdUsuarioModificacion, i.IdUsuarioPropietario),
            i.Id,
            'EDITAR_DOCUMENTO',
            GETDATE(),
            'Documento Id=' + CAST(i.Id AS VARCHAR) + ' editado: ' + i.Titulo,
            ISNULL(i.IdUsuarioModificacion, i.IdUsuarioPropietario),
            1
        FROM inserted i;
    END
END;
