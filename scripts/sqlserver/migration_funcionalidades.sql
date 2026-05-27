USE [SIGD_Central];
GO

-- Añadir campos para controlar las versiones menores en Documento_Version
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Documento_Version]') AND name = 'VersionMinor'
)
BEGIN
    ALTER TABLE [dbo].[Documento_Version] ADD [VersionMinor] INT NOT NULL DEFAULT 0;
    PRINT 'Columna VersionMinor añadida a Documento_Version.';
END
ELSE
BEGIN
    PRINT 'La columna VersionMinor ya existe en Documento_Version.';
END
GO

-- Añadir campos para registrar IP en Flujo_Aprobacion
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Flujo_Aprobacion]') AND name = 'IpOrigenRemitente'
)
BEGIN
    ALTER TABLE [dbo].[Flujo_Aprobacion] ADD [IpOrigenRemitente] NVARCHAR(50) NULL;
    PRINT 'Columna IpOrigenRemitente añadida a Flujo_Aprobacion.';
END
ELSE
BEGIN
    PRINT 'La columna IpOrigenRemitente ya existe en Flujo_Aprobacion.';
END
GO

IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Flujo_Aprobacion]') AND name = 'IpOrigenFirmante'
)
BEGIN
    ALTER TABLE [dbo].[Flujo_Aprobacion] ADD [IpOrigenFirmante] NVARCHAR(50) NULL;
    PRINT 'Columna IpOrigenFirmante añadida a Flujo_Aprobacion.';
END
ELSE
BEGIN
    PRINT 'La columna IpOrigenFirmante ya existe en Flujo_Aprobacion.';
END
GO

-- Añadir campos de auditoría faltantes en la tabla Empresa
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Empresa]') AND name = 'FechaCreacion'
)
BEGIN
    ALTER TABLE [dbo].[Empresa] ADD 
        [IdUsuarioCreacion] INT NULL, 
        [FechaCreacion] DATETIME2 NULL, 
        [IdUsuarioModificacion] INT NULL, 
        [FechaModificacion] DATETIME2 NULL, 
        [IdUsuarioEliminacion] INT NULL, 
        [FechaEliminacion] DATETIME2 NULL;
    PRINT 'Columnas de auditoría añadidas a Empresa.';
END
GO

-- Añadir FechaCreacion faltante en Bitacoras
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[BitacoraAcceso]') AND name = 'FechaCreacion'
)
BEGIN
    ALTER TABLE [dbo].[BitacoraAcceso] ADD [FechaCreacion] DATETIME2 NULL;
    ALTER TABLE [dbo].[BitacoraControlDocumento] ADD [FechaCreacion] DATETIME2 NULL;
    ALTER TABLE [dbo].[BitacoraTransaccional] ADD [FechaCreacion] DATETIME2 NULL;
    PRINT 'Columna FechaCreacion añadida a tablas de Bitácora.';
END
GO
