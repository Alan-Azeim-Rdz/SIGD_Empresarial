-- PASO 1: COLUMNAS Y FKs (sin trigger aún)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('dbo.Empresa') AND name='IdUsuarioCreacion')
    ALTER TABLE [dbo].[Empresa] ADD [IdUsuarioCreacion] INT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('dbo.Empresa') AND name='FechaCreacion')
    ALTER TABLE [dbo].[Empresa] ADD [FechaCreacion] DATETIME NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('dbo.Empresa') AND name='IdUsuarioModificacion')
    ALTER TABLE [dbo].[Empresa] ADD [IdUsuarioModificacion] INT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('dbo.Empresa') AND name='FechaModificacion')
    ALTER TABLE [dbo].[Empresa] ADD [FechaModificacion] DATETIME NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('dbo.Empresa') AND name='IdUsuarioEliminacion')
    ALTER TABLE [dbo].[Empresa] ADD [IdUsuarioEliminacion] INT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('dbo.Empresa') AND name='FechaEliminacion')
    ALTER TABLE [dbo].[Empresa] ADD [FechaEliminacion] DATETIME NULL;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_Empresa_UsuCrea')
    ALTER TABLE [dbo].[Empresa] ADD CONSTRAINT [FK_Empresa_UsuCrea] FOREIGN KEY ([IdUsuarioCreacion]) REFERENCES [dbo].[Usuario]([Id]);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_Empresa_UsuMod')
    ALTER TABLE [dbo].[Empresa] ADD CONSTRAINT [FK_Empresa_UsuMod] FOREIGN KEY ([IdUsuarioModificacion]) REFERENCES [dbo].[Usuario]([Id]);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_Empresa_UsuEli')
    ALTER TABLE [dbo].[Empresa] ADD CONSTRAINT [FK_Empresa_UsuEli] FOREIGN KEY ([IdUsuarioEliminacion]) REFERENCES [dbo].[Usuario]([Id]);

-- Retroactivo
UPDATE [dbo].[Empresa] SET FechaCreacion = FechaRegistro, IdUsuarioCreacion = 1 WHERE FechaCreacion IS NULL;

PRINT 'Columnas y FKs de auditoria aplicadas a Empresa.';
