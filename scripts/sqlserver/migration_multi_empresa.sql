USE [SIGD_Central];
GO

PRINT '--- 1. Creando tabla Empresa ---';
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Empresa')
BEGIN
    CREATE TABLE [dbo].[Empresa] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Nombre] VARCHAR(100) NOT NULL,
        [Slug] VARCHAR(50) NOT NULL,
        [RFC] VARCHAR(20) NULL,
        [CorreoContacto] VARCHAR(150) NULL,
        [FechaRegistro] DATETIME NOT NULL DEFAULT(GETDATE()),
        [Estatus] BIT NOT NULL DEFAULT(1),
        [CamposPersonalizados] NVARCHAR(MAX) NULL, -- JSON definición de campos
        PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_Empresa_Slug] UNIQUE NONCLUSTERED ([Slug] ASC)
    );

    SET IDENTITY_INSERT [dbo].[Empresa] ON;
    INSERT INTO [dbo].[Empresa] (Id, Nombre, Slug, RFC, CorreoContacto, Estatus)
    VALUES (1, N'Empresa Demo', N'demo', N'DEMO123456XX9', N'contacto@demo.local', 1);
    SET IDENTITY_INSERT [dbo].[Empresa] OFF;
    PRINT '  ✓ Tabla Empresa creada y Empresa Demo (Id=1) insertada.';
END
ELSE
    PRINT '  → Tabla Empresa ya existe, se omite.';
GO

PRINT '--- 2. Modificando tabla Departamento ---';
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Departamento') AND name = 'IdEmpresa')
BEGIN
    ALTER TABLE [dbo].[Departamento] ADD [IdEmpresa] INT NULL;
    ALTER TABLE [dbo].[Departamento] ADD CONSTRAINT [FK_Departamento_Empresa] FOREIGN KEY ([IdEmpresa]) REFERENCES [dbo].[Empresa] ([Id]);
    
    -- Usamos SQL dinámico para evitar errores de compilación de columnas no existentes
    EXEC('UPDATE [dbo].[Departamento] SET [IdEmpresa] = 1 WHERE [Id] <> 1;');
    PRINT '  ✓ Columna IdEmpresa agregada a Departamento.';
END
ELSE
    PRINT '  → Columna IdEmpresa ya existe en Departamento.';
GO

PRINT '--- 3. Modificando tabla TipoDocumento ---';
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TipoDocumento') AND name = 'IdEmpresa')
BEGIN
    ALTER TABLE [dbo].[TipoDocumento] ADD [IdEmpresa] INT NULL;
    ALTER TABLE [dbo].[TipoDocumento] ADD CONSTRAINT [FK_TipoDocumento_Empresa] FOREIGN KEY ([IdEmpresa]) REFERENCES [dbo].[Empresa] ([Id]);
    
    EXEC('UPDATE [dbo].[TipoDocumento] SET [IdEmpresa] = 1;');
    PRINT '  ✓ Columna IdEmpresa agregada a TipoDocumento.';
END
ELSE
    PRINT '  → Columna IdEmpresa ya existe en TipoDocumento.';
GO

PRINT '--- 4. Modificando tabla Documento ---';
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Documento') AND name = 'IdEmpresa')
BEGIN
    ALTER TABLE [dbo].[Documento] ADD [IdEmpresa] INT NULL;
    ALTER TABLE [dbo].[Documento] ADD CONSTRAINT [FK_Documento_Empresa] FOREIGN KEY ([IdEmpresa]) REFERENCES [dbo].[Empresa] ([Id]);
    ALTER TABLE [dbo].[Documento] ADD [CamposPersonalizadosValores] NVARCHAR(MAX) NULL;

    EXEC('UPDATE [dbo].[Documento] SET [IdEmpresa] = 1;');
    PRINT '  ✓ Columnas IdEmpresa y CamposPersonalizadosValores agregadas a Documento.';
END
ELSE
    PRINT '  → Columnas ya existen en Documento.';
GO

PRINT '--- 5. Modificando tabla Usuario ---';
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Usuario') AND name = 'IdEmpresa')
BEGIN
    ALTER TABLE [dbo].[Usuario] ADD [IdEmpresa] INT NULL;
    ALTER TABLE [dbo].[Usuario] ADD CONSTRAINT [FK_Usuario_Empresa] FOREIGN KEY ([IdEmpresa]) REFERENCES [dbo].[Empresa] ([Id]);
    
    EXEC('UPDATE [dbo].[Usuario] SET [IdEmpresa] = 1 WHERE [Id] <> 1;');
    PRINT '  ✓ Columna IdEmpresa agregada a Usuario y registros actualizados.';
END
ELSE
    PRINT '  → Columna IdEmpresa ya existe en Usuario.';
GO

PRINT '--- 6. Creación de Rol Super Administrador y Permisos si no existen ---';
IF NOT EXISTS (SELECT 1 FROM [dbo].[Rol] WHERE Nombre = 'Super Administrador')
BEGIN
    INSERT INTO [dbo].[Rol] (Nombre, Descripcion, IdUsuarioCreacion, Estatus)
    VALUES ('Super Administrador', 'Acceso total y global para administrar todas las empresas.', 1, 1);
    PRINT '  ✓ Rol Super Administrador creado.';
END
GO
