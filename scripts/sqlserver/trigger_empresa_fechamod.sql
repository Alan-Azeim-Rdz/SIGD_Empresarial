CREATE TRIGGER [dbo].[TRG_AutoFechaMod_Empresa] ON [dbo].[Empresa] AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF NOT UPDATE(FechaModificacion)
        UPDATE t SET t.FechaModificacion = GETDATE()
        FROM [dbo].[Empresa] t INNER JOIN inserted i ON t.Id = i.Id;
END;
