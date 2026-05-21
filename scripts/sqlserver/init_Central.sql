USE [master]
GO
/****** Objeto: Database [SIGD_Central] Fecha de script: 5/10/2026 11:15:53 PM ******/
CREATE DATABASE [SIGD_Central]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'SIGD_Central', FILENAME = N'/var/opt/mssql/data/SIGD_Central.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'SIGD_Central_log', FILENAME = N'/var/opt/mssql/data/SIGD_Central_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO

CREATE LOGIN [usr_sigd] WITH PASSWORD = 'AppSigd_User2026!';
GO
ALTER DATABASE [SIGD_Central] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [SIGD_Central].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [SIGD_Central] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [SIGD_Central] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [SIGD_Central] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [SIGD_Central] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [SIGD_Central] SET ARITHABORT OFF 
GO
ALTER DATABASE [SIGD_Central] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [SIGD_Central] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [SIGD_Central] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [SIGD_Central] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [SIGD_Central] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [SIGD_Central] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [SIGD_Central] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [SIGD_Central] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [SIGD_Central] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [SIGD_Central] SET  DISABLE_BROKER 
GO
ALTER DATABASE [SIGD_Central] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [SIGD_Central] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [SIGD_Central] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [SIGD_Central] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [SIGD_Central] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [SIGD_Central] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [SIGD_Central] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [SIGD_Central] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [SIGD_Central] SET  MULTI_USER 
GO
ALTER DATABASE [SIGD_Central] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [SIGD_Central] SET DB_CHAINING OFF 
GO
ALTER DATABASE [SIGD_Central] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [SIGD_Central] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [SIGD_Central] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [SIGD_Central] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [SIGD_Central] SET QUERY_STORE = ON
GO
ALTER DATABASE [SIGD_Central] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [SIGD_Central]
GO

/****** Objeto: User [usr_sigd] Fecha de script: 5/10/2026 11:15:53 PM ******/
CREATE USER [usr_sigd] FOR LOGIN [usr_sigd] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_datareader] ADD MEMBER [usr_sigd]
GO
ALTER ROLE [db_datawriter] ADD MEMBER [usr_sigd]
GO
/****** Objeto: Table [dbo].[BitacoraAcceso] Fecha de script: 5/10/2026 11:15:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BitacoraAcceso](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IdUsuario] [int] NOT NULL,
	[FechaHoraIntento] [datetime] NULL,
	[DireccionIP] [varchar](50) NULL,
	[EstadoIntento] [varchar](50) NOT NULL,
	[IdUsuarioCreacion] [int] NULL,
	[IdUsuarioModificacion] [int] NULL,
	[FechaModificacion] [datetime] NULL,
	[IdUsuarioEliminacion] [int] NULL,
	[FechaEliminacion] [datetime] NULL,
	[Estatus] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[BitacoraControlDocumento] Fecha de script: 5/10/2026 11:15:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BitacoraControlDocumento](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IdDocumento] [int] NOT NULL,
	[FechaEvento] [datetime] NOT NULL,
	[TipoCambio] [varchar](100) NOT NULL,
	[ValorAnterior] [varchar](max) NULL,
	[ValorNuevo] [varchar](max) NULL,
	[Observaciones] [varchar](500) NULL,
	[IdUsuarioAccion] [int] NOT NULL,
	[Estatus] [bit] NULL,
	[IdUsuarioCreacion] [int] NULL,
	[IdUsuarioModificacion] [int] NULL,
	[FechaModificacion] [datetime] NULL,
	[IdUsuarioEliminacion] [int] NULL,
	[FechaEliminacion] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[BitacoraTransaccional] Fecha de script: 5/10/2026 11:15:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BitacoraTransaccional](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IdUsuario] [int] NOT NULL,
	[IdDocumento] [int] NULL,
	[IdVersion] [int] NULL,
	[Accion] [varchar](100) NOT NULL,
	[FechaHora] [datetime] NOT NULL,
	[DireccionIP] [varchar](50) NULL,
	[Detalle] [varchar](500) NULL,
	[IdUsuarioCreacion] [int] NULL,
	[IdUsuarioModificacion] [int] NULL,
	[FechaModificacion] [datetime] NULL,
	[IdUsuarioEliminacion] [int] NULL,
	[FechaEliminacion] [datetime] NULL,
	[Estatus] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Departamento] Fecha de script: 5/10/2026 11:15:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Departamento](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](100) NOT NULL,
	[Abreviatura] [varchar](20) NOT NULL,
	[IdUsuarioCreacion] [int] NULL,
	[FechaCreacion] [datetime] NULL,
	[IdUsuarioModificacion] [int] NULL,
	[FechaModificacion] [datetime] NULL,
	[IdUsuarioEliminacion] [int] NULL,
	[FechaEliminacion] [datetime] NULL,
	[Estatus] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Documento] Fecha de script: 5/10/2026 11:15:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Documento](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CodigoInterno] [varchar](50) NOT NULL,
	[Titulo] [varchar](255) NOT NULL,
	[IdDepartamento] [int] NOT NULL,
	[EstadoActual] [varchar](50) NOT NULL,
	[IdUsuarioPropietario] [int] NOT NULL,
	[FechaCreacion] [datetime] NULL,
	[Estatus] [bit] NULL,
	[IdUsuarioCreacion] [int] NULL,
	[IdUsuarioModificacion] [int] NULL,
	[FechaModificacion] [datetime] NULL,
	[IdUsuarioEliminacion] [int] NULL,
	[FechaEliminacion] [datetime] NULL,
	[IdTipoDocumento] [int] NULL,
 CONSTRAINT [PK_Documento] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_Documento_CodigoInterno] UNIQUE NONCLUSTERED 
(
	[CodigoInterno] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Documento_Version] Fecha de script: 5/10/2026 11:15:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Documento_Version](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IdDocumento] [int] NOT NULL,
	[NumeroVersion] [int] NOT NULL,
	[RutaArchivoFisico] [varchar](500) NOT NULL,
	[HashDocumento] [varchar](255) NOT NULL,
	[MotivoCambio] [varchar](500) NULL,
	[IdUsuarioSube] [int] NOT NULL,
	[FechaSubida] [datetime] NULL,
	[IdUsuarioCreacion] [int] NULL,
	[FechaCreacion] [datetime] NULL,
	[IdUsuarioModificacion] [int] NULL,
	[FechaModificacion] [datetime] NULL,
	[IdUsuarioEliminacion] [int] NULL,
	[FechaEliminacion] [datetime] NULL,
	[Estatus] [bit] NULL,
	[ExtensionArchivo] [varchar](10) NULL,
	[MimeType] [varchar](100) NULL,
	[TamanoBytes] [bigint] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Evento_Integracion] Fecha de script: 5/10/2026 11:15:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Evento_Integracion](
	[Id] [uniqueidentifier] NOT NULL,
	[TipoEvento] [varchar](100) NOT NULL,
	[PayloadJSON] [nvarchar](max) NOT NULL,
	[Estado] [varchar](20) NOT NULL,
	[FechaCreacion] [datetime] NULL,
	[FechaProcesado] [datetime] NULL,
	[Intentos] [int] NULL,
	[MensajeError] [varchar](max) NULL,
	[IdUsuarioCreacion] [int] NULL,
	[IdUsuarioModificacion] [int] NULL,
	[IdUsuarioEliminacion] [int] NULL,
	[FechaEliminacion] [datetime] NULL,
	[Estatus] [bit] NULL,
	[FechaModificacion] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Flujo_Aprobacion] Fecha de script: 5/10/2026 11:15:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Flujo_Aprobacion](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IdVersionDocumento] [int] NOT NULL,
	[IdUsuarioAsignado] [int] NOT NULL,
	[TipoAccion] [varchar](50) NOT NULL,
	[EstadoFirma] [varchar](50) NOT NULL,
	[Comentarios] [varchar](1000) NULL,
	[FechaFirma] [datetime] NULL,
	[Orden] [int] NOT NULL,
	[IdUsuarioCreacion] [int] NULL,
	[FechaCreacion] [datetime] NULL,
	[IdUsuarioModificacion] [int] NULL,
	[FechaModificacion] [datetime] NULL,
	[IdUsuarioEliminacion] [int] NULL,
	[FechaEliminacion] [datetime] NULL,
	[Estatus] [bit] NULL,
	[TokenFirma] [varchar](255) NULL,
	[MetodoAutenticacion] [varchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Permiso] Fecha de script: 5/10/2026 11:15:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Permiso](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Codigo] [varchar](50) NOT NULL,
	[Descripcion] [varchar](255) NOT NULL,
	[Modulo] [varchar](50) NOT NULL,
	[IdUsuarioCreacion] [int] NULL,
	[FechaCreacion] [datetime] NULL,
	[IdUsuarioModificacion] [int] NULL,
	[FechaModificacion] [datetime] NULL,
	[IdUsuarioEliminacion] [int] NULL,
	[FechaEliminacion] [datetime] NULL,
	[Estatus] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Rol] Fecha de script: 5/10/2026 11:15:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Rol](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](50) NOT NULL,
	[Descripcion] [varchar](255) NULL,
	[IdUsuarioCreacion] [int] NULL,
	[FechaCreacion] [datetime] NULL,
	[IdUsuarioModificacion] [int] NULL,
	[FechaModificacion] [datetime] NULL,
	[IdUsuarioEliminacion] [int] NULL,
	[FechaEliminacion] [datetime] NULL,
	[Estatus] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Rol_Permiso] Fecha de script: 5/10/2026 11:15:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Rol_Permiso](
	[IdRol] [int] NOT NULL,
	[IdPermiso] [int] NOT NULL,
	[IdUsuarioCreacion] [int] NULL,
	[FechaCreacion] [datetime] NULL,
	[IdUsuarioModificacion] [int] NULL,
	[FechaModificacion] [datetime] NULL,
	[IdUsuarioEliminacion] [int] NULL,
	[FechaEliminacion] [datetime] NULL,
	[Estatus] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdRol] ASC,
	[IdPermiso] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[TipoDocumento] Fecha de script: 5/10/2026 11:15:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TipoDocumento](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](50) NOT NULL,
	[Abreviatura] [varchar](10) NOT NULL,
	[TiempoRetencionMeses] [int] NOT NULL,
	[Estatus] [bit] NULL,
	[IdUsuarioCreacion] [int] NULL,
	[FechaCreacion] [datetime] NULL,
	[IdUsuarioModificacion] [int] NULL,
	[FechaModificacion] [datetime] NULL,
	[IdUsuarioEliminacion] [int] NULL,
	[FechaEliminacion] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Usuario] Fecha de script: 5/10/2026 11:15:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Usuario](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IdDepartamento] [int] NOT NULL,
	[Nombre] [varchar](100) NOT NULL,
	[ApellidoP] [varchar](100) NOT NULL,
	[ApellidoM] [varchar](100) NULL,
	[Correo] [varchar](150) NOT NULL,
	[Contrasena] [varchar](255) NOT NULL,
	[IdUsuarioCreacion] [int] NULL,
	[FechaCreacion] [datetime] NULL,
	[IdUsuarioModificacion] [int] NULL,
	[FechaModificacion] [datetime] NULL,
	[IdUsuarioEliminacion] [int] NULL,
	[FechaEliminacion] [datetime] NULL,
	[Estatus] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Correo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Usuario_Rol] Fecha de script: 5/10/2026 11:15:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Usuario_Rol](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IdUsuario] [int] NOT NULL,
	[IdRol] [int] NOT NULL,
	[FechaAsignacion] [datetime] NULL,
	[IdUsuarioCreacion] [int] NULL,
	[FechaCreacion] [datetime] NULL,
	[IdUsuarioModificacion] [int] NULL,
	[FechaModificacion] [datetime] NULL,
	[IdUsuarioEliminacion] [int] NULL,
	[FechaEliminacion] [datetime] NULL,
	[Estatus] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Objeto: Index [IX_Documento_Estado_Depto] Fecha de script: 5/10/2026 11:15:53 PM ******/
CREATE NONCLUSTERED INDEX [IX_Documento_Estado_Depto] ON [dbo].[Documento]
(
	[EstadoActual] ASC,
	[IdDepartamento] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Objeto: Index [IX_Evento_Pendientes] Fecha de script: 5/10/2026 11:15:53 PM ******/
CREATE NONCLUSTERED INDEX [IX_Evento_Pendientes] ON [dbo].[Evento_Integracion]
(
	[Estado] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[BitacoraAcceso] ADD  DEFAULT (getdate()) FOR [FechaHoraIntento]
GO
ALTER TABLE [dbo].[BitacoraAcceso] ADD  DEFAULT ((1)) FOR [Estatus]
GO
ALTER TABLE [dbo].[BitacoraControlDocumento] ADD  DEFAULT (getdate()) FOR [FechaEvento]
GO
ALTER TABLE [dbo].[BitacoraControlDocumento] ADD  DEFAULT ((1)) FOR [Estatus]
GO
ALTER TABLE [dbo].[BitacoraTransaccional] ADD  DEFAULT (getdate()) FOR [FechaHora]
GO
ALTER TABLE [dbo].[BitacoraTransaccional] ADD  DEFAULT ((1)) FOR [Estatus]
GO
ALTER TABLE [dbo].[Departamento] ADD  DEFAULT (getdate()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[Departamento] ADD  DEFAULT ((0)) FOR [Estatus]
GO
ALTER TABLE [dbo].[Documento] ADD  DEFAULT ('Borrador') FOR [EstadoActual]
GO
ALTER TABLE [dbo].[Documento] ADD  DEFAULT (getdate()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[Documento] ADD  DEFAULT ((1)) FOR [Estatus]
GO
ALTER TABLE [dbo].[Documento_Version] ADD  DEFAULT (getdate()) FOR [FechaSubida]
GO
ALTER TABLE [dbo].[Documento_Version] ADD  DEFAULT (getdate()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[Documento_Version] ADD  DEFAULT ((1)) FOR [Estatus]
GO
ALTER TABLE [dbo].[Evento_Integracion] ADD  DEFAULT (newid()) FOR [Id]
GO
ALTER TABLE [dbo].[Evento_Integracion] ADD  DEFAULT ('Pendiente') FOR [Estado]
GO
ALTER TABLE [dbo].[Evento_Integracion] ADD  DEFAULT (getdate()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[Evento_Integracion] ADD  DEFAULT ((0)) FOR [Intentos]
GO
ALTER TABLE [dbo].[Evento_Integracion] ADD  DEFAULT ((1)) FOR [Estatus]
GO
ALTER TABLE [dbo].[Flujo_Aprobacion] ADD  DEFAULT ('Pendiente') FOR [EstadoFirma]
GO
ALTER TABLE [dbo].[Flujo_Aprobacion] ADD  DEFAULT (getdate()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[Flujo_Aprobacion] ADD  DEFAULT ((1)) FOR [Estatus]
GO
ALTER TABLE [dbo].[Permiso] ADD  DEFAULT (getdate()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[Permiso] ADD  DEFAULT ((1)) FOR [Estatus]
GO
ALTER TABLE [dbo].[Rol] ADD  DEFAULT (getdate()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[Rol] ADD  DEFAULT ((0)) FOR [Estatus]
GO
ALTER TABLE [dbo].[Rol_Permiso] ADD  DEFAULT (getdate()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[Rol_Permiso] ADD  DEFAULT ((1)) FOR [Estatus]
GO
ALTER TABLE [dbo].[TipoDocumento] ADD  DEFAULT ((12)) FOR [TiempoRetencionMeses]
GO
ALTER TABLE [dbo].[TipoDocumento] ADD  DEFAULT ((1)) FOR [Estatus]
GO
ALTER TABLE [dbo].[TipoDocumento] ADD  DEFAULT (getdate()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[Usuario] ADD  DEFAULT (getdate()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[Usuario] ADD  DEFAULT ((0)) FOR [Estatus]
GO
ALTER TABLE [dbo].[Usuario_Rol] ADD  DEFAULT (getdate()) FOR [FechaAsignacion]
GO
ALTER TABLE [dbo].[Usuario_Rol] ADD  DEFAULT (getdate()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[Usuario_Rol] ADD  DEFAULT ((0)) FOR [Estatus]
GO
ALTER TABLE [dbo].[BitacoraAcceso]  WITH CHECK ADD  CONSTRAINT [FK_Bitacora_Usuario] FOREIGN KEY([IdUsuario])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[BitacoraAcceso] CHECK CONSTRAINT [FK_Bitacora_Usuario]
GO
ALTER TABLE [dbo].[BitacoraAcceso]  WITH CHECK ADD  CONSTRAINT [FK_BitacoraAcceso_UsuCrea] FOREIGN KEY([IdUsuarioCreacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[BitacoraAcceso] CHECK CONSTRAINT [FK_BitacoraAcceso_UsuCrea]
GO
ALTER TABLE [dbo].[BitacoraAcceso]  WITH CHECK ADD  CONSTRAINT [FK_BitacoraAcceso_UsuEli] FOREIGN KEY([IdUsuarioEliminacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[BitacoraAcceso] CHECK CONSTRAINT [FK_BitacoraAcceso_UsuEli]
GO
ALTER TABLE [dbo].[BitacoraAcceso]  WITH CHECK ADD  CONSTRAINT [FK_BitacoraAcceso_UsuMod] FOREIGN KEY([IdUsuarioModificacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[BitacoraAcceso] CHECK CONSTRAINT [FK_BitacoraAcceso_UsuMod]
GO
ALTER TABLE [dbo].[BitacoraControlDocumento]  WITH CHECK ADD  CONSTRAINT [FK_BitacoraControl_Documento] FOREIGN KEY([IdDocumento])
REFERENCES [dbo].[Documento] ([Id])
GO
ALTER TABLE [dbo].[BitacoraControlDocumento] CHECK CONSTRAINT [FK_BitacoraControl_Documento]
GO
ALTER TABLE [dbo].[BitacoraControlDocumento]  WITH CHECK ADD  CONSTRAINT [FK_BitacoraControl_Usuario] FOREIGN KEY([IdUsuarioAccion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[BitacoraControlDocumento] CHECK CONSTRAINT [FK_BitacoraControl_Usuario]
GO
ALTER TABLE [dbo].[BitacoraControlDocumento]  WITH CHECK ADD  CONSTRAINT [FK_BitacoraControl_UsuCrea] FOREIGN KEY([IdUsuarioCreacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[BitacoraControlDocumento] CHECK CONSTRAINT [FK_BitacoraControl_UsuCrea]
GO
ALTER TABLE [dbo].[BitacoraControlDocumento]  WITH CHECK ADD  CONSTRAINT [FK_BitacoraControl_UsuEli] FOREIGN KEY([IdUsuarioEliminacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[BitacoraControlDocumento] CHECK CONSTRAINT [FK_BitacoraControl_UsuEli]
GO
ALTER TABLE [dbo].[BitacoraControlDocumento]  WITH CHECK ADD  CONSTRAINT [FK_BitacoraControl_UsuMod] FOREIGN KEY([IdUsuarioModificacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[BitacoraControlDocumento] CHECK CONSTRAINT [FK_BitacoraControl_UsuMod]
GO
ALTER TABLE [dbo].[BitacoraTransaccional]  WITH CHECK ADD  CONSTRAINT [FK_BitacoraTrans_Documento] FOREIGN KEY([IdDocumento])
REFERENCES [dbo].[Documento] ([Id])
GO
ALTER TABLE [dbo].[BitacoraTransaccional] CHECK CONSTRAINT [FK_BitacoraTrans_Documento]
GO
ALTER TABLE [dbo].[BitacoraTransaccional]  WITH CHECK ADD  CONSTRAINT [FK_BitacoraTrans_Usuario] FOREIGN KEY([IdUsuario])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[BitacoraTransaccional] CHECK CONSTRAINT [FK_BitacoraTrans_Usuario]
GO
ALTER TABLE [dbo].[BitacoraTransaccional]  WITH CHECK ADD  CONSTRAINT [FK_BitacoraTrans_UsuCrea_Audit] FOREIGN KEY([IdUsuarioCreacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[BitacoraTransaccional] CHECK CONSTRAINT [FK_BitacoraTrans_UsuCrea_Audit]
GO
ALTER TABLE [dbo].[BitacoraTransaccional]  WITH CHECK ADD  CONSTRAINT [FK_BitacoraTrans_UsuEli_Audit] FOREIGN KEY([IdUsuarioEliminacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[BitacoraTransaccional] CHECK CONSTRAINT [FK_BitacoraTrans_UsuEli_Audit]
GO
ALTER TABLE [dbo].[BitacoraTransaccional]  WITH CHECK ADD  CONSTRAINT [FK_BitacoraTrans_UsuMod_Audit] FOREIGN KEY([IdUsuarioModificacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[BitacoraTransaccional] CHECK CONSTRAINT [FK_BitacoraTrans_UsuMod_Audit]
GO
ALTER TABLE [dbo].[BitacoraTransaccional]  WITH CHECK ADD  CONSTRAINT [FK_BitacoraTrans_Version] FOREIGN KEY([IdVersion])
REFERENCES [dbo].[Documento_Version] ([Id])
GO
ALTER TABLE [dbo].[BitacoraTransaccional] CHECK CONSTRAINT [FK_BitacoraTrans_Version]
GO
ALTER TABLE [dbo].[Departamento]  WITH CHECK ADD  CONSTRAINT [FK_Depto_UsuCrea] FOREIGN KEY([IdUsuarioCreacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Departamento] CHECK CONSTRAINT [FK_Depto_UsuCrea]
GO
ALTER TABLE [dbo].[Departamento]  WITH CHECK ADD  CONSTRAINT [FK_Depto_UsuEli] FOREIGN KEY([IdUsuarioEliminacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Departamento] CHECK CONSTRAINT [FK_Depto_UsuEli]
GO
ALTER TABLE [dbo].[Departamento]  WITH CHECK ADD  CONSTRAINT [FK_Depto_UsuMod] FOREIGN KEY([IdUsuarioModificacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Departamento] CHECK CONSTRAINT [FK_Depto_UsuMod]
GO
ALTER TABLE [dbo].[Documento]  WITH CHECK ADD  CONSTRAINT [FK__Documento__IdDepartamento] FOREIGN KEY([IdDepartamento])
REFERENCES [dbo].[Departamento] ([Id])
GO
ALTER TABLE [dbo].[Documento] CHECK CONSTRAINT [FK__Documento__IdDepartamento]
GO
ALTER TABLE [dbo].[Documento]  WITH CHECK ADD  CONSTRAINT [FK__Documento__IdUsuario] FOREIGN KEY([IdUsuarioPropietario])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Documento] CHECK CONSTRAINT [FK__Documento__IdUsuario]
GO
ALTER TABLE [dbo].[Documento]  WITH CHECK ADD  CONSTRAINT [FK_Documento_Departamento] FOREIGN KEY([IdDepartamento])
REFERENCES [dbo].[Departamento] ([Id])
GO
ALTER TABLE [dbo].[Documento] CHECK CONSTRAINT [FK_Documento_Departamento]
GO
ALTER TABLE [dbo].[Documento]  WITH CHECK ADD  CONSTRAINT [FK_Documento_TipoDoc] FOREIGN KEY([IdTipoDocumento])
REFERENCES [dbo].[TipoDocumento] ([Id])
GO
ALTER TABLE [dbo].[Documento] CHECK CONSTRAINT [FK_Documento_TipoDoc]
GO
ALTER TABLE [dbo].[Documento]  WITH CHECK ADD  CONSTRAINT [FK_Documento_UsuarioProp] FOREIGN KEY([IdUsuarioPropietario])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Documento] CHECK CONSTRAINT [FK_Documento_UsuarioProp]
GO
ALTER TABLE [dbo].[Documento]  WITH CHECK ADD  CONSTRAINT [FK_Documento_UsuCrea] FOREIGN KEY([IdUsuarioCreacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Documento] CHECK CONSTRAINT [FK_Documento_UsuCrea]
GO
ALTER TABLE [dbo].[Documento]  WITH CHECK ADD  CONSTRAINT [FK_Documento_UsuEli] FOREIGN KEY([IdUsuarioEliminacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Documento] CHECK CONSTRAINT [FK_Documento_UsuEli]
GO
ALTER TABLE [dbo].[Documento]  WITH CHECK ADD  CONSTRAINT [FK_Documento_UsuMod] FOREIGN KEY([IdUsuarioModificacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Documento] CHECK CONSTRAINT [FK_Documento_UsuMod]
GO
ALTER TABLE [dbo].[Documento_Version]  WITH CHECK ADD FOREIGN KEY([IdDocumento])
REFERENCES [dbo].[Documento] ([Id])
GO
ALTER TABLE [dbo].[Documento_Version]  WITH CHECK ADD FOREIGN KEY([IdUsuarioSube])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Documento_Version]  WITH CHECK ADD  CONSTRAINT [FK_DocVersion_Documento] FOREIGN KEY([IdDocumento])
REFERENCES [dbo].[Documento] ([Id])
GO
ALTER TABLE [dbo].[Documento_Version] CHECK CONSTRAINT [FK_DocVersion_Documento]
GO
ALTER TABLE [dbo].[Documento_Version]  WITH CHECK ADD  CONSTRAINT [FK_DocVersion_UsuarioSube] FOREIGN KEY([IdUsuarioSube])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Documento_Version] CHECK CONSTRAINT [FK_DocVersion_UsuarioSube]
GO
ALTER TABLE [dbo].[Documento_Version]  WITH CHECK ADD  CONSTRAINT [FK_DocVersion_UsuCrea] FOREIGN KEY([IdUsuarioCreacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Documento_Version] CHECK CONSTRAINT [FK_DocVersion_UsuCrea]
GO
ALTER TABLE [dbo].[Documento_Version]  WITH CHECK ADD  CONSTRAINT [FK_DocVersion_UsuEli] FOREIGN KEY([IdUsuarioEliminacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Documento_Version] CHECK CONSTRAINT [FK_DocVersion_UsuEli]
GO
ALTER TABLE [dbo].[Documento_Version]  WITH CHECK ADD  CONSTRAINT [FK_DocVersion_UsuMod] FOREIGN KEY([IdUsuarioModificacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Documento_Version] CHECK CONSTRAINT [FK_DocVersion_UsuMod]
GO
ALTER TABLE [dbo].[Evento_Integracion]  WITH CHECK ADD  CONSTRAINT [FK_Evento_UsuCrea] FOREIGN KEY([IdUsuarioCreacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Evento_Integracion] CHECK CONSTRAINT [FK_Evento_UsuCrea]
GO
ALTER TABLE [dbo].[Evento_Integracion]  WITH CHECK ADD  CONSTRAINT [FK_Evento_UsuEli] FOREIGN KEY([IdUsuarioEliminacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Evento_Integracion] CHECK CONSTRAINT [FK_Evento_UsuEli]
GO
ALTER TABLE [dbo].[Evento_Integracion]  WITH CHECK ADD  CONSTRAINT [FK_Evento_UsuMod] FOREIGN KEY([IdUsuarioModificacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Evento_Integracion] CHECK CONSTRAINT [FK_Evento_UsuMod]
GO
ALTER TABLE [dbo].[Flujo_Aprobacion]  WITH CHECK ADD FOREIGN KEY([IdUsuarioAsignado])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Flujo_Aprobacion]  WITH CHECK ADD FOREIGN KEY([IdVersionDocumento])
REFERENCES [dbo].[Documento_Version] ([Id])
GO
ALTER TABLE [dbo].[Flujo_Aprobacion]  WITH CHECK ADD  CONSTRAINT [FK_Flujo_UsuCrea] FOREIGN KEY([IdUsuarioCreacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Flujo_Aprobacion] CHECK CONSTRAINT [FK_Flujo_UsuCrea]
GO
ALTER TABLE [dbo].[Flujo_Aprobacion]  WITH CHECK ADD  CONSTRAINT [FK_Flujo_UsuEli] FOREIGN KEY([IdUsuarioEliminacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Flujo_Aprobacion] CHECK CONSTRAINT [FK_Flujo_UsuEli]
GO
ALTER TABLE [dbo].[Flujo_Aprobacion]  WITH CHECK ADD  CONSTRAINT [FK_Flujo_UsuMod] FOREIGN KEY([IdUsuarioModificacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Flujo_Aprobacion] CHECK CONSTRAINT [FK_Flujo_UsuMod]
GO
ALTER TABLE [dbo].[Flujo_Aprobacion]  WITH CHECK ADD  CONSTRAINT [FK_FlujoAprob_DocVersion] FOREIGN KEY([IdVersionDocumento])
REFERENCES [dbo].[Documento_Version] ([Id])
GO
ALTER TABLE [dbo].[Flujo_Aprobacion] CHECK CONSTRAINT [FK_FlujoAprob_DocVersion]
GO
ALTER TABLE [dbo].[Flujo_Aprobacion]  WITH CHECK ADD  CONSTRAINT [FK_FlujoAprob_UsuarioAsig] FOREIGN KEY([IdUsuarioAsignado])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Flujo_Aprobacion] CHECK CONSTRAINT [FK_FlujoAprob_UsuarioAsig]
GO
ALTER TABLE [dbo].[Permiso]  WITH CHECK ADD  CONSTRAINT [FK_Permiso_UsuCrea] FOREIGN KEY([IdUsuarioCreacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Permiso] CHECK CONSTRAINT [FK_Permiso_UsuCrea]
GO
ALTER TABLE [dbo].[Permiso]  WITH CHECK ADD  CONSTRAINT [FK_Permiso_UsuEli] FOREIGN KEY([IdUsuarioEliminacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Permiso] CHECK CONSTRAINT [FK_Permiso_UsuEli]
GO
ALTER TABLE [dbo].[Permiso]  WITH CHECK ADD  CONSTRAINT [FK_Permiso_UsuMod] FOREIGN KEY([IdUsuarioModificacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Permiso] CHECK CONSTRAINT [FK_Permiso_UsuMod]
GO
ALTER TABLE [dbo].[Rol]  WITH CHECK ADD  CONSTRAINT [FK_Rol_UsuCrea] FOREIGN KEY([IdUsuarioCreacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Rol] CHECK CONSTRAINT [FK_Rol_UsuCrea]
GO
ALTER TABLE [dbo].[Rol]  WITH CHECK ADD  CONSTRAINT [FK_Rol_UsuEli] FOREIGN KEY([IdUsuarioEliminacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Rol] CHECK CONSTRAINT [FK_Rol_UsuEli]
GO
ALTER TABLE [dbo].[Rol]  WITH CHECK ADD  CONSTRAINT [FK_Rol_UsuMod] FOREIGN KEY([IdUsuarioModificacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Rol] CHECK CONSTRAINT [FK_Rol_UsuMod]
GO
ALTER TABLE [dbo].[Rol_Permiso]  WITH CHECK ADD FOREIGN KEY([IdPermiso])
REFERENCES [dbo].[Permiso] ([Id])
GO
ALTER TABLE [dbo].[Rol_Permiso]  WITH CHECK ADD FOREIGN KEY([IdRol])
REFERENCES [dbo].[Rol] ([Id])
GO
ALTER TABLE [dbo].[Rol_Permiso]  WITH CHECK ADD  CONSTRAINT [FK_RolPermiso_Permiso] FOREIGN KEY([IdPermiso])
REFERENCES [dbo].[Permiso] ([Id])
GO
ALTER TABLE [dbo].[Rol_Permiso] CHECK CONSTRAINT [FK_RolPermiso_Permiso]
GO
ALTER TABLE [dbo].[Rol_Permiso]  WITH CHECK ADD  CONSTRAINT [FK_RolPermiso_Rol] FOREIGN KEY([IdRol])
REFERENCES [dbo].[Rol] ([Id])
GO
ALTER TABLE [dbo].[Rol_Permiso] CHECK CONSTRAINT [FK_RolPermiso_Rol]
GO
ALTER TABLE [dbo].[Rol_Permiso]  WITH CHECK ADD  CONSTRAINT [FK_RolPermiso_UsuCrea] FOREIGN KEY([IdUsuarioCreacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Rol_Permiso] CHECK CONSTRAINT [FK_RolPermiso_UsuCrea]
GO
ALTER TABLE [dbo].[Rol_Permiso]  WITH CHECK ADD  CONSTRAINT [FK_RolPermiso_UsuEli] FOREIGN KEY([IdUsuarioEliminacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Rol_Permiso] CHECK CONSTRAINT [FK_RolPermiso_UsuEli]
GO
ALTER TABLE [dbo].[Rol_Permiso]  WITH CHECK ADD  CONSTRAINT [FK_RolPermiso_UsuMod] FOREIGN KEY([IdUsuarioModificacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Rol_Permiso] CHECK CONSTRAINT [FK_RolPermiso_UsuMod]
GO
ALTER TABLE [dbo].[TipoDocumento]  WITH CHECK ADD  CONSTRAINT [FK_TipoDoc_UsuCrea] FOREIGN KEY([IdUsuarioCreacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[TipoDocumento] CHECK CONSTRAINT [FK_TipoDoc_UsuCrea]
GO
ALTER TABLE [dbo].[TipoDocumento]  WITH CHECK ADD  CONSTRAINT [FK_TipoDoc_UsuEli] FOREIGN KEY([IdUsuarioEliminacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[TipoDocumento] CHECK CONSTRAINT [FK_TipoDoc_UsuEli]
GO
ALTER TABLE [dbo].[TipoDocumento]  WITH CHECK ADD  CONSTRAINT [FK_TipoDoc_UsuMod] FOREIGN KEY([IdUsuarioModificacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[TipoDocumento] CHECK CONSTRAINT [FK_TipoDoc_UsuMod]
GO
ALTER TABLE [dbo].[Usuario]  WITH CHECK ADD  CONSTRAINT [FK_Usuario_Departamento] FOREIGN KEY([IdDepartamento])
REFERENCES [dbo].[Departamento] ([Id])
GO
ALTER TABLE [dbo].[Usuario] CHECK CONSTRAINT [FK_Usuario_Departamento]
GO
ALTER TABLE [dbo].[Usuario]  WITH CHECK ADD  CONSTRAINT [FK_Usuario_UsuCrea] FOREIGN KEY([IdUsuarioCreacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Usuario] CHECK CONSTRAINT [FK_Usuario_UsuCrea]
GO
ALTER TABLE [dbo].[Usuario]  WITH CHECK ADD  CONSTRAINT [FK_Usuario_UsuEli] FOREIGN KEY([IdUsuarioEliminacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Usuario] CHECK CONSTRAINT [FK_Usuario_UsuEli]
GO
ALTER TABLE [dbo].[Usuario]  WITH CHECK ADD  CONSTRAINT [FK_Usuario_UsuMod] FOREIGN KEY([IdUsuarioModificacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Usuario] CHECK CONSTRAINT [FK_Usuario_UsuMod]
GO
ALTER TABLE [dbo].[Usuario_Rol]  WITH CHECK ADD  CONSTRAINT [FK_UsuarioRol_Rol] FOREIGN KEY([IdRol])
REFERENCES [dbo].[Rol] ([Id])
GO
ALTER TABLE [dbo].[Usuario_Rol] CHECK CONSTRAINT [FK_UsuarioRol_Rol]
GO
ALTER TABLE [dbo].[Usuario_Rol]  WITH CHECK ADD  CONSTRAINT [FK_UsuarioRol_Usuario] FOREIGN KEY([IdUsuario])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Usuario_Rol] CHECK CONSTRAINT [FK_UsuarioRol_Usuario]
GO
ALTER TABLE [dbo].[Usuario_Rol]  WITH CHECK ADD  CONSTRAINT [FK_UsuRol_UsuCrea] FOREIGN KEY([IdUsuarioCreacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Usuario_Rol] CHECK CONSTRAINT [FK_UsuRol_UsuCrea]
GO
ALTER TABLE [dbo].[Usuario_Rol]  WITH CHECK ADD  CONSTRAINT [FK_UsuRol_UsuEli] FOREIGN KEY([IdUsuarioEliminacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Usuario_Rol] CHECK CONSTRAINT [FK_UsuRol_UsuEli]
GO
ALTER TABLE [dbo].[Usuario_Rol]  WITH CHECK ADD  CONSTRAINT [FK_UsuRol_UsuMod] FOREIGN KEY([IdUsuarioModificacion])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Usuario_Rol] CHECK CONSTRAINT [FK_UsuRol_UsuMod]
GO
/****** Objeto: StoredProcedure [dbo].[SP_AsignarFlujoAprobacion] Fecha de script: 5/10/2026 11:15:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE   PROCEDURE [dbo].[SP_AsignarFlujoAprobacion]
    @IdVersionDocumento INT,
    @IdUsuarioAsigna INT,
    -- Recibimos los usuarios como un JSON para insertar múltiples firmas de golpe
    @JsonUsuariosFirmantes NVARCHAR(MAX) 
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Formato esperado del JSON: [{"IdUsuario": 3, "Orden": 1, "TipoAccion": "Revisa"}, {"IdUsuario": 7, "Orden": 2, "TipoAccion": "Aprueba"}]

        -- 1. Insertar el flujo de aprobación decodificando el JSON
        INSERT INTO [dbo].[Flujo_Aprobacion] 
            (IdVersionDocumento, IdUsuarioAsignado, TipoAccion, EstadoFirma, Orden, IdUsuarioCreacion)
        SELECT 
            @IdVersionDocumento,
            JSON_VALUE(value, '$.IdUsuario'),
            JSON_VALUE(value, '$.TipoAccion'),
            'Pendiente',
            JSON_VALUE(value, '$.Orden'),
            @IdUsuarioAsigna
        FROM OPENJSON(@JsonUsuariosFirmantes);

        -- 2. Cambiar estado del documento
        DECLARE @IdDocumento INT;
        SELECT @IdDocumento = IdDocumento FROM [dbo].[Documento_Version] WHERE Id = @IdVersionDocumento;

        UPDATE [dbo].[Documento]
        SET EstadoActual = 'En Aprobacion',
            IdUsuarioModificacion = @IdUsuarioAsigna
        WHERE Id = @IdDocumento;

        -- 3. Registrar en bitácora
        INSERT INTO [dbo].[BitacoraTransaccional]
            (IdUsuario, IdDocumento, IdVersion, Accion, Detalle, IdUsuarioCreacion)
        VALUES
            (@IdUsuarioAsigna, @IdDocumento, @IdVersionDocumento, 'ASIGNACION_FLUJO', 'Se asignó flujo de firmas al documento', @IdUsuarioAsigna);

        COMMIT TRANSACTION;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END

GO
/****** Objeto: StoredProcedure [dbo].[SP_ProcesarFirmaDocumento] Fecha de script: 5/10/2026 11:15:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SP_ProcesarFirmaDocumento]
    @IdFlujo int,
    @IdUsuarioFirma int,
    @EstadoFirma varchar(50), -- 'Aprobado' o 'Rechazado'
    @Comentarios varchar(1000)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. Validar que el flujo pertenece al usuario y está pendiente
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Flujo_Aprobacion] WHERE Id = @IdFlujo AND IdUsuarioAsignado = @IdUsuarioFirma AND EstadoFirma = 'Pendiente')
        BEGIN
            RAISERROR('El usuario no tiene permisos para firmar o el documento ya fue procesado.', 16, 1);
            RETURN;
        END

        -- 2. Registrar la firma y AUDITAR LA MODIFICACIÓN
        UPDATE [dbo].[Flujo_Aprobacion]
        SET EstadoFirma = @EstadoFirma, 
            Comentarios = @Comentarios, 
            FechaFirma = GETDATE(),
            IdUsuarioModificacion = @IdUsuarioFirma, -- Auditoría
            FechaModificacion = GETDATE()            -- Auditoría
        WHERE Id = @IdFlujo;

        DECLARE @IdVersion int;
        SELECT @IdVersion = IdVersionDocumento FROM [dbo].[Flujo_Aprobacion] WHERE Id = @IdFlujo;

        DECLARE @IdDocumento int;
        SELECT @IdDocumento = IdDocumento FROM [dbo].[Documento_Version] WHERE Id = @IdVersion;

        -- 3. Lógica de Rechazo: Si alguien rechaza, el documento vuelve a borrador
        IF @EstadoFirma = 'Rechazado'
        BEGIN
            UPDATE [dbo].[Documento] 
            SET EstadoActual = 'Borrador',
                IdUsuarioModificacion = @IdUsuarioFirma, -- Auditoría
                FechaModificacion = GETDATE()            -- Auditoría
            WHERE Id = @IdDocumento;
            
            -- Opcional: Cancelar las firmas pendientes de esta versión
            UPDATE [dbo].[Flujo_Aprobacion] 
            SET EstadoFirma = 'Cancelado',
                IdUsuarioModificacion = @IdUsuarioFirma, -- Auditoría
                FechaModificacion = GETDATE()            -- Auditoría
            WHERE IdVersionDocumento = @IdVersion AND EstadoFirma = 'Pendiente';
        END
        ELSE IF @EstadoFirma = 'Aprobado'
        BEGIN
            -- 4. Lógica de Aprobación: Verificar si faltan firmas pendientes
            DECLARE @FirmasPendientes int;
            SELECT @FirmasPendientes = COUNT(*) FROM [dbo].[Flujo_Aprobacion] 
            WHERE IdVersionDocumento = @IdVersion AND EstadoFirma = 'Pendiente';

            -- Si ya no hay firmas pendientes, el documento se vuelve VIGENTE
            IF @FirmasPendientes = 0
            BEGIN
                UPDATE [dbo].[Documento] 
                SET EstadoActual = 'Vigente',
                    IdUsuarioModificacion = @IdUsuarioFirma, -- Auditoría
                    FechaModificacion = GETDATE()            -- Auditoría
                WHERE Id = @IdDocumento;
                
                -- (Aquí la lógica del backend debería enviar correos de notificación de publicación)
            END
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END

GO
/****** Objeto: StoredProcedure [dbo].[SP_SubirNuevaVersionDocumento] Fecha de script: 5/10/2026 11:15:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[SP_SubirNuevaVersionDocumento]
    @IdDocumento INT,
    @IdUsuarioSube INT,
    @RutaArchivoFisico VARCHAR(500),
    @HashDocumento VARCHAR(255),
    @MotivoCambio VARCHAR(500),
    @ExtensionArchivo VARCHAR(10),
    @MimeType VARCHAR(100),
    @TamanoBytes BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. Obtener el número de la siguiente versión
        DECLARE @SiguienteVersion INT;
        SELECT @SiguienteVersion = ISNULL(MAX(NumeroVersion), 0) + 1 
        FROM [dbo].[Documento_Version] 
        WHERE IdDocumento = @IdDocumento;

        DECLARE @NuevoIdVersion INT;

        -- 2. Insertar la nueva versión
        INSERT INTO [dbo].[Documento_Version]
            (IdDocumento, NumeroVersion, RutaArchivoFisico, HashDocumento, MotivoCambio, IdUsuarioSube, IdUsuarioCreacion, ExtensionArchivo, MimeType, TamanoBytes)
        VALUES
            (@IdDocumento, @SiguienteVersion, @RutaArchivoFisico, @HashDocumento, @MotivoCambio, @IdUsuarioSube, @IdUsuarioSube, @ExtensionArchivo, @MimeType, @TamanoBytes);

        SET @NuevoIdVersion = SCOPE_IDENTITY();

        -- 3. Actualizar el estado del documento principal (vuelve a revisión/borrador)
        UPDATE [dbo].[Documento]
        SET EstadoActual = 'Revision',
            IdUsuarioModificacion = @IdUsuarioSube
        WHERE Id = @IdDocumento;

        -- 4. Registrar en la Bitácora Transaccional
        INSERT INTO [dbo].[BitacoraTransaccional]
            (IdUsuario, IdDocumento, IdVersion, Accion, Detalle, IdUsuarioCreacion)
        VALUES
            (@IdUsuarioSube, @IdDocumento, @NuevoIdVersion, 'NUEVA_VERSION', 'Se subió la versión ' + CAST(@SiguienteVersion AS VARCHAR(10)), @IdUsuarioSube);

        COMMIT TRANSACTION;
        
        -- Retornar el ID de la nueva versión creada
        SELECT @NuevoIdVersion AS IdNuevaVersion;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END

GO
USE [master]
GO
ALTER DATABASE [SIGD_Central] SET  READ_WRITE 
GO

USE [SIGD_Central]
GO

-- TRIGGERS DE BORRADO 

-- 1.1 Documentos y Versiones
CREATE TRIGGER [dbo].[TRG_SoftDelete_Documento] ON [dbo].[Documento] INSTEAD OF DELETE AS
BEGIN
    SET NOCOUNT ON;
    UPDATE t SET t.Estatus = 0, t.EstadoActual = 'Eliminado', t.FechaEliminacion = GETDATE()
    FROM [dbo].[Documento] t INNER JOIN deleted d ON t.Id = d.Id;
END
GO

CREATE TRIGGER [dbo].[TRG_SoftDelete_Documento_Version] ON [dbo].[Documento_Version] INSTEAD OF DELETE AS
BEGIN
    SET NOCOUNT ON;
    UPDATE t SET t.Estatus = 0, t.FechaEliminacion = GETDATE()
    FROM [dbo].[Documento_Version] t INNER JOIN deleted d ON t.Id = d.Id;
END
GO

-- 1.2 Catálogos Principales
CREATE TRIGGER [dbo].[TRG_SoftDelete_Usuario] ON [dbo].[Usuario] INSTEAD OF DELETE AS
BEGIN
    SET NOCOUNT ON;
    UPDATE t SET t.Estatus = 0, t.FechaEliminacion = GETDATE() FROM [dbo].[Usuario] t INNER JOIN deleted d ON t.Id = d.Id;
END
GO

CREATE TRIGGER [dbo].[TRG_SoftDelete_Departamento] ON [dbo].[Departamento] INSTEAD OF DELETE AS
BEGIN
    SET NOCOUNT ON;
    UPDATE t SET t.Estatus = 0, t.FechaEliminacion = GETDATE() FROM [dbo].[Departamento] t INNER JOIN deleted d ON t.Id = d.Id;
END
GO

CREATE TRIGGER [dbo].[TRG_SoftDelete_TipoDocumento] ON [dbo].[TipoDocumento] INSTEAD OF DELETE AS
BEGIN
    SET NOCOUNT ON;
    UPDATE t SET t.Estatus = 0, t.FechaEliminacion = GETDATE() FROM [dbo].[TipoDocumento] t INNER JOIN deleted d ON t.Id = d.Id;
END
GO

-- 1.3 Seguridad y Permisos
CREATE TRIGGER [dbo].[TRG_SoftDelete_Rol] ON [dbo].[Rol] INSTEAD OF DELETE AS
BEGIN
    SET NOCOUNT ON;
    UPDATE t SET t.Estatus = 0, t.FechaEliminacion = GETDATE() FROM [dbo].[Rol] t INNER JOIN deleted d ON t.Id = d.Id;
END
GO

CREATE TRIGGER [dbo].[TRG_SoftDelete_Permiso] ON [dbo].[Permiso] INSTEAD OF DELETE AS
BEGIN
    SET NOCOUNT ON;
    UPDATE t SET t.Estatus = 0, t.FechaEliminacion = GETDATE() FROM [dbo].[Permiso] t INNER JOIN deleted d ON t.Id = d.Id;
END
GO

CREATE TRIGGER [dbo].[TRG_SoftDelete_Usuario_Rol] ON [dbo].[Usuario_Rol] INSTEAD OF DELETE AS
BEGIN
    SET NOCOUNT ON;
    UPDATE t SET t.Estatus = 0, t.FechaEliminacion = GETDATE() FROM [dbo].[Usuario_Rol] t INNER JOIN deleted d ON t.Id = d.Id;
END
GO

-- Nota: Rol_Permiso usa llave compuesta (IdRol, IdPermiso)
CREATE TRIGGER [dbo].[TRG_SoftDelete_Rol_Permiso] ON [dbo].[Rol_Permiso] INSTEAD OF DELETE AS
BEGIN
    SET NOCOUNT ON;
    UPDATE t SET t.Estatus = 0, t.FechaEliminacion = GETDATE() 
    FROM [dbo].[Rol_Permiso] t INNER JOIN deleted d ON t.IdRol = d.IdRol AND t.IdPermiso = d.IdPermiso;
END
GO


-- ==========================================================================================
-- SECCIÓN 2: TRIGGERS
-- ==========================================================================================

-- 2.1 Documentos, Versiones y Flujos
CREATE TRIGGER [dbo].[TRG_AutoFechaMod_Documento] ON [dbo].[Documento] AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF NOT UPDATE(FechaModificacion) BEGIN
        UPDATE t SET t.FechaModificacion = GETDATE() FROM [dbo].[Documento] t INNER JOIN inserted i ON t.Id = i.Id;
    END
END
GO

CREATE TRIGGER [dbo].[TRG_AutoFechaMod_Documento_Version] ON [dbo].[Documento_Version] AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF NOT UPDATE(FechaModificacion) BEGIN
        UPDATE t SET t.FechaModificacion = GETDATE() FROM [dbo].[Documento_Version] t INNER JOIN inserted i ON t.Id = i.Id;
    END
END
GO

CREATE TRIGGER [dbo].[TRG_AutoFechaMod_Flujo_Aprobacion] ON [dbo].[Flujo_Aprobacion] AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF NOT UPDATE(FechaModificacion) BEGIN
        UPDATE t SET t.FechaModificacion = GETDATE() FROM [dbo].[Flujo_Aprobacion] t INNER JOIN inserted i ON t.Id = i.Id;
    END
END
GO

-- 2.2 Catálogos Principales
CREATE TRIGGER [dbo].[TRG_AutoFechaMod_Usuario] ON [dbo].[Usuario] AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF NOT UPDATE(FechaModificacion) BEGIN
        UPDATE t SET t.FechaModificacion = GETDATE() FROM [dbo].[Usuario] t INNER JOIN inserted i ON t.Id = i.Id;
    END
END
GO

CREATE TRIGGER [dbo].[TRG_AutoFechaMod_Departamento] ON [dbo].[Departamento] AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF NOT UPDATE(FechaModificacion) BEGIN
        UPDATE t SET t.FechaModificacion = GETDATE() FROM [dbo].[Departamento] t INNER JOIN inserted i ON t.Id = i.Id;
    END
END
GO

CREATE TRIGGER [dbo].[TRG_AutoFechaMod_TipoDocumento] ON [dbo].[TipoDocumento] AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF NOT UPDATE(FechaModificacion) BEGIN
        UPDATE t SET t.FechaModificacion = GETDATE() FROM [dbo].[TipoDocumento] t INNER JOIN inserted i ON t.Id = i.Id;
    END
END
GO

-- 2.3 Seguridad y Eventos
CREATE TRIGGER [dbo].[TRG_AutoFechaMod_Rol] ON [dbo].[Rol] AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF NOT UPDATE(FechaModificacion) BEGIN
        UPDATE t SET t.FechaModificacion = GETDATE() FROM [dbo].[Rol] t INNER JOIN inserted i ON t.Id = i.Id;
    END
END
GO

CREATE TRIGGER [dbo].[TRG_AutoFechaMod_Permiso] ON [dbo].[Permiso] AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF NOT UPDATE(FechaModificacion) BEGIN
        UPDATE t SET t.FechaModificacion = GETDATE() FROM [dbo].[Permiso] t INNER JOIN inserted i ON t.Id = i.Id;
    END
END
GO

CREATE TRIGGER [dbo].[TRG_AutoFechaMod_Usuario_Rol] ON [dbo].[Usuario_Rol] AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF NOT UPDATE(FechaModificacion) BEGIN
        UPDATE t SET t.FechaModificacion = GETDATE() FROM [dbo].[Usuario_Rol] t INNER JOIN inserted i ON t.Id = i.Id;
    END
END
GO

CREATE TRIGGER [dbo].[TRG_AutoFechaMod_Rol_Permiso] ON [dbo].[Rol_Permiso] AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF NOT UPDATE(FechaModificacion) BEGIN
        UPDATE t SET t.FechaModificacion = GETDATE() FROM [dbo].[Rol_Permiso] t INNER JOIN inserted i ON t.IdRol = i.IdRol AND t.IdPermiso = i.IdPermiso;
    END
END
GO

CREATE TRIGGER [dbo].[TRG_AutoFechaMod_Evento_Integracion] ON [dbo].[Evento_Integracion] AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF NOT UPDATE(FechaModificacion) BEGIN
        UPDATE t SET t.FechaModificacion = GETDATE() FROM [dbo].[Evento_Integracion] t INNER JOIN inserted i ON t.Id = i.Id;
    END
END
GO


-- ==========================================================================================
-- SECCIÓN 3: TRIGGER DE AUDITORÍA AVANZADA
-- Inserta automáticamente en la bitácora cuando un documento cambia de estado.
-- ==========================================================================================

CREATE TRIGGER [dbo].[TRG_Auditoria_CambioEstadoDoc]
ON [dbo].[Documento]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Solo actuamos si el estado realmente cambió
    IF UPDATE(EstadoActual)
    BEGIN
        INSERT INTO [dbo].[BitacoraControlDocumento] (
            IdDocumento, 
            FechaEvento, 
            TipoCambio, 
            ValorAnterior, 
            ValorNuevo, 
            Observaciones, 
            IdUsuarioAccion, 
            IdUsuarioCreacion
        )
        SELECT 
            i.Id,
            GETDATE(),
            'Cambio de Estado',
            d.EstadoActual,
            i.EstadoActual,
            'Cambio detectado automáticamente por Trigger de BD',
            -- Tomamos el usuario que hace la modificación, o el propietario si viene nulo
            ISNULL(i.IdUsuarioModificacion, i.IdUsuarioPropietario),
            ISNULL(i.IdUsuarioModificacion, i.IdUsuarioPropietario)
        FROM inserted i
        INNER JOIN deleted d ON i.Id = d.Id
        WHERE i.EstadoActual <> d.EstadoActual;
    END
END
GO

-- Borrado lógico para Flujo_Aprobacion
CREATE TRIGGER [dbo].[TRG_SoftDelete_Flujo_Aprobacion] ON [dbo].[Flujo_Aprobacion] INSTEAD OF DELETE AS
BEGIN
    SET NOCOUNT ON;
    UPDATE t SET t.Estatus = 0, t.FechaEliminacion = GETDATE()
    FROM [dbo].[Flujo_Aprobacion] t INNER JOIN deleted d ON t.Id = d.Id;
END
GO

-- Auditoría cuando un usuario firma/aprueba/rechaza
CREATE TRIGGER [dbo].[TRG_Auditoria_CambioFirma]
ON [dbo].[Flujo_Aprobacion]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    -- Solo si cambió el EstadoFirma (ej. de 'Pendiente' a 'Aprobado')
    IF UPDATE(EstadoFirma)
    BEGIN
        INSERT INTO [dbo].[BitacoraTransaccional] (
            IdUsuario, IdVersion, Accion, Detalle, IdUsuarioCreacion
        )
        SELECT 
            i.IdUsuarioModificacion, 
            i.IdVersionDocumento,
            'FIRMA_' + UPPER(i.EstadoFirma),
            'El usuario cambió el estado de la firma a: ' + i.EstadoFirma,
            i.IdUsuarioModificacion
        FROM inserted i
        INNER JOIN deleted d ON i.Id = d.Id
        WHERE i.EstadoFirma <> d.EstadoFirma;
    END
END
GO


CREATE TRIGGER [dbo].[TRG_Outbox_DocumentoVigente]
ON [dbo].[Documento]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    -- Generar evento de integración solo cuando el documento pasa a Vigente
    IF UPDATE(EstadoActual)
    BEGIN
        INSERT INTO [dbo].[Evento_Integracion] (
            TipoEvento, PayloadJSON, Estado, IdUsuarioCreacion
        )
        SELECT 
            'DocumentoVigenteEvent',
            '{"IdDocumento": ' + CAST(i.Id AS VARCHAR) + ', "Titulo": "' + i.Titulo + '"}',
            'Pendiente',
            i.IdUsuarioModificacion
        FROM inserted i
        INNER JOIN deleted d ON i.Id = d.Id
        WHERE i.EstadoActual = 'Vigente' AND d.EstadoActual <> 'Vigente';
    END
END
GO


-- ==========================================================================================
-- SECCIÓN 3.5: PROCEDIMIENTOS DE AUTENTICACIÓN (HASHEO EN BD)
-- ==========================================================================================

-- Procedimiento para Crear un Usuario hasheando su contraseña internamente
CREATE PROCEDURE [dbo].[SP_CrearUsuario]
    @IdDepartamento INT,
    @Nombre VARCHAR(100),
    @ApellidoP VARCHAR(100),
    @ApellidoM VARCHAR(100) = NULL,
    @Correo VARCHAR(150),
    @ContrasenaPlana VARCHAR(255),
    @IdUsuarioCreacion INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Aplicamos SHA-256 y lo convertimos a string hexadecimal uppercase
    DECLARE @HashContrasena VARCHAR(255) = CONVERT(VARCHAR(255), HASHBYTES('SHA2_256', @ContrasenaPlana), 2);
    
    INSERT INTO [dbo].[Usuario] (IdDepartamento, Nombre, ApellidoP, ApellidoM, Correo, Contrasena, IdUsuarioCreacion)
    VALUES (@IdDepartamento, @Nombre, @ApellidoP, @ApellidoM, @Correo, @HashContrasena, @IdUsuarioCreacion);
    
    SELECT SCOPE_IDENTITY() AS IdUsuario;
END
GO

-- Procedimiento para Validar el Login de un Usuario
CREATE PROCEDURE [dbo].[SP_ValidarLogin]
    @Correo VARCHAR(150),
    @ContrasenaPlana VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Hashear la contraseña entrante para comparar
    DECLARE @HashIntento VARCHAR(255) = CONVERT(VARCHAR(255), HASHBYTES('SHA2_256', @ContrasenaPlana), 2);
    
    SELECT 
        Id, IdDepartamento, Nombre, ApellidoP, ApellidoM, Correo, Estatus
    FROM [dbo].[Usuario]
    WHERE Correo = @Correo 
      AND Contrasena = @HashIntento
      AND Estatus = 1; -- Solo usuarios activos
END
GO


-- ==========================================================================================
-- SECCIÓN 4: DATOS SEMILLA (SEED DATA)
-- Crea el usuario Super Administrador con máxima autoridad para gestionar el sistema.
-- ==========================================================================================

USE [SIGD_Central]
GO

PRINT '========================================';
PRINT '  INSERTANDO DATOS SEMILLA...';
PRINT '========================================';

-- 4.1 Departamento de Administración
SET IDENTITY_INSERT [dbo].[Departamento] ON;
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[Departamento] WHERE Id = 1)
BEGIN
    INSERT INTO [dbo].[Departamento] (Id, Nombre, Abreviatura, FechaCreacion, Estatus)
    VALUES (1, N'Administración General', N'ADM', GETDATE(), 1);
    PRINT '  ✓ Departamento "Administración General" creado.';
END
ELSE
    PRINT '  → Departamento Id=1 ya existe, se omite.';
GO
SET IDENTITY_INSERT [dbo].[Departamento] OFF;
GO

-- 4.2 Usuario Super Administrador
--     Correo:     admin@sigd.local
--     Contraseña: Admin@SIGD2026!  (hasheada con SHA2_256)
SET IDENTITY_INSERT [dbo].[Usuario] ON;
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[Usuario] WHERE Id = 1)
BEGIN
    INSERT INTO [dbo].[Usuario] (Id, IdDepartamento, Nombre, ApellidoP, ApellidoM, Correo, Contrasena, FechaCreacion, Estatus)
    VALUES (1, 1, N'Super', N'Administrador', N'SIGD', N'admin@sigd.local',
            CONVERT(VARCHAR(255), HASHBYTES('SHA2_256', N'Admin@SIGD2026!'), 2),
            GETDATE(), 1);
    PRINT '  ✓ Usuario Super Admin creado (admin@sigd.local).';
END
ELSE
    PRINT '  → Usuario Id=1 ya existe, se omite.';
GO
SET IDENTITY_INSERT [dbo].[Usuario] OFF;
GO

-- 4.3 Actualizar auditoría del departamento (el admin se creó a sí mismo)
UPDATE [dbo].[Departamento] SET IdUsuarioCreacion = 1 WHERE Id = 1 AND IdUsuarioCreacion IS NULL;
GO

-- 4.4 Rol Super Administrador
SET IDENTITY_INSERT [dbo].[Rol] ON;
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[Rol] WHERE Id = 1)
BEGIN
    INSERT INTO [dbo].[Rol] (Id, Nombre, Descripcion, IdUsuarioCreacion, FechaCreacion, Estatus)
    VALUES (1, N'Super Administrador', N'Acceso total al sistema SIGD. Puede gestionar usuarios, roles, documentos y toda la configuración.', 1, GETDATE(), 1);
    PRINT '  ✓ Rol "Super Administrador" creado.';
END
ELSE
    PRINT '  → Rol Id=1 ya existe, se omite.';
GO
SET IDENTITY_INSERT [dbo].[Rol] OFF;
GO

-- 4.5 Catálogo de Permisos del Sistema
SET IDENTITY_INSERT [dbo].[Permiso] ON;
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[Permiso] WHERE Id = 1)
BEGIN
    INSERT INTO [dbo].[Permiso] (Id, Codigo, Descripcion, Modulo, IdUsuarioCreacion, FechaCreacion, Estatus) VALUES
    (1,  N'DOC_CREAR',       N'Crear documentos nuevos',             N'Documentos', 1, GETDATE(), 1),
    (2,  N'DOC_EDITAR',      N'Editar documentos existentes',        N'Documentos', 1, GETDATE(), 1),
    (3,  N'DOC_ELIMINAR',    N'Eliminar documentos (borrado lógico)',N'Documentos', 1, GETDATE(), 1),
    (4,  N'DOC_VER',         N'Ver documentos y sus versiones',      N'Documentos', 1, GETDATE(), 1),
    (5,  N'DOC_APROBAR',     N'Aprobar documentos en flujo',         N'Documentos', 1, GETDATE(), 1),
    (6,  N'DOC_FIRMAR',      N'Firmar documentos digitalmente',      N'Documentos', 1, GETDATE(), 1),
    (7,  N'DOC_DESCARGAR',   N'Descargar archivos de documentos',    N'Documentos', 1, GETDATE(), 1),
    (8,  N'USU_CREAR',       N'Crear usuarios nuevos',               N'Usuarios',   1, GETDATE(), 1),
    (9,  N'USU_EDITAR',      N'Editar usuarios existentes',          N'Usuarios',   1, GETDATE(), 1),
    (10, N'USU_ELIMINAR',    N'Eliminar usuarios (borrado lógico)',  N'Usuarios',   1, GETDATE(), 1),
    (11, N'USU_VER',         N'Ver lista de usuarios',               N'Usuarios',   1, GETDATE(), 1),
    (12, N'ROL_GESTIONAR',   N'Crear, editar y eliminar roles',      N'Seguridad',  1, GETDATE(), 1),
    (13, N'PERM_ASIGNAR',    N'Asignar permisos a roles',            N'Seguridad',  1, GETDATE(), 1),
    (14, N'DEPTO_GESTIONAR', N'Gestionar departamentos',             N'Catálogos',  1, GETDATE(), 1),
    (15, N'TIPO_GESTIONAR',  N'Gestionar tipos de documento',        N'Catálogos',  1, GETDATE(), 1),
    (16, N'BIT_VER',         N'Ver bitácoras y logs del sistema',    N'Auditoría',  1, GETDATE(), 1),
    (17, N'REP_VER',         N'Ver reportes del módulo de reportes', N'Reportes',   1, GETDATE(), 1),
    (18, N'BUS_VER',         N'Usar el módulo de búsqueda NoSQL',    N'Búsqueda',   1, GETDATE(), 1),
    (19, N'SIS_ADMIN',       N'Administración total del sistema',    N'Sistema',    1, GETDATE(), 1);
    PRINT '  ✓ 19 permisos del sistema creados.';
END
ELSE
    PRINT '  → Permisos ya existen, se omiten.';
GO
SET IDENTITY_INSERT [dbo].[Permiso] OFF;
GO

-- 4.6 Asignar TODOS los permisos al rol Super Administrador
IF NOT EXISTS (SELECT 1 FROM [dbo].[Rol_Permiso] WHERE IdRol = 1)
BEGIN
    INSERT INTO [dbo].[Rol_Permiso] (IdRol, IdPermiso, IdUsuarioCreacion, FechaCreacion, Estatus)
    SELECT 1, Id, 1, GETDATE(), 1 FROM [dbo].[Permiso] WHERE Estatus = 1;
    PRINT '  ✓ Todos los permisos asignados al rol Super Administrador.';
END
ELSE
    PRINT '  → Rol_Permiso para rol Id=1 ya existe, se omite.';
GO

-- 4.7 Asignar el rol Super Administrador al usuario Admin
SET IDENTITY_INSERT [dbo].[Usuario_Rol] ON;
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[Usuario_Rol] WHERE IdUsuario = 1 AND IdRol = 1)
BEGIN
    INSERT INTO [dbo].[Usuario_Rol] (Id, IdUsuario, IdRol, FechaAsignacion, IdUsuarioCreacion, FechaCreacion, Estatus)
    VALUES (1, 1, 1, GETDATE(), 1, GETDATE(), 1);
    PRINT '  ✓ Rol Super Administrador asignado al usuario Admin.';
END
ELSE
    PRINT '  → Usuario_Rol ya existe, se omite.';
GO
SET IDENTITY_INSERT [dbo].[Usuario_Rol] OFF;
GO

PRINT '========================================';
PRINT '  SEED DATA COMPLETADO EXITOSAMENTE';
PRINT '  Usuario: admin@sigd.local';
PRINT '  Contraseña: Admin@SIGD2026!';
PRINT '========================================';