using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Gestion_de_Documentos.Models;

public partial class DirContext : DbContext
{
    public DirContext()
    {
    }

    public DirContext(DbContextOptions<DirContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BitacoraAcceso> BitacoraAccesos { get; set; }

    public virtual DbSet<BitacoraControlDocumento> BitacoraControlDocumentos { get; set; }

    public virtual DbSet<BitacoraTransaccional> BitacoraTransaccionals { get; set; }

    public virtual DbSet<Departamento> Departamentos { get; set; }

    public virtual DbSet<Empresa> Empresas { get; set; }

    public virtual DbSet<Documento> Documentos { get; set; }

    public virtual DbSet<DocumentoVersion> DocumentoVersions { get; set; }

    public virtual DbSet<EventoIntegracion> EventoIntegracions { get; set; }

    public virtual DbSet<FlujoAprobacion> FlujoAprobacions { get; set; }

    public virtual DbSet<Permiso> Permisos { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<RolPermiso> RolPermisos { get; set; }

    public virtual DbSet<TipoDocumento> TipoDocumentos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<UsuarioRol> UsuarioRols { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Connection string should be configured via dependency injection in Program.cs
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BitacoraAcceso>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Bitacora__3214EC070850188A");

            entity.ToTable("BitacoraAcceso");

            entity.Property(e => e.DireccionIp)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("DireccionIP");
            entity.Property(e => e.EstadoIntento)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FechaEliminacion).HasColumnType("datetime");
            entity.Property(e => e.FechaHoraIntento).HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion).HasColumnType("datetime");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.BitacoraAccesoIdUsuarioNavigations)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bitacora_Usuario");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.BitacoraAccesoIdUsuarioCreacionNavigations)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .HasConstraintName("FK_BitacoraAcceso_UsuCrea");

            entity.HasOne(d => d.IdUsuarioEliminacionNavigation).WithMany(p => p.BitacoraAccesoIdUsuarioEliminacionNavigations)
                .HasForeignKey(d => d.IdUsuarioEliminacion)
                .HasConstraintName("FK_BitacoraAcceso_UsuEli");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.BitacoraAccesoIdUsuarioModificacionNavigations)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("FK_BitacoraAcceso_UsuMod");
        });

        modelBuilder.Entity<BitacoraControlDocumento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Bitacora__3214EC075E17D850");

            entity.ToTable("BitacoraControlDocumento");

            entity.Property(e => e.FechaEliminacion).HasColumnType("datetime");
            entity.Property(e => e.FechaEvento).HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion).HasColumnType("datetime");
            entity.Property(e => e.Observaciones)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.TipoCambio)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ValorAnterior).IsUnicode(false);
            entity.Property(e => e.ValorNuevo).IsUnicode(false);

            entity.HasOne(d => d.IdDocumentoNavigation).WithMany(p => p.BitacoraControlDocumentos)
                .HasForeignKey(d => d.IdDocumento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BitacoraControl_Documento");

            entity.HasOne(d => d.IdUsuarioAccionNavigation).WithMany(p => p.BitacoraControlDocumentoIdUsuarioAccionNavigations)
                .HasForeignKey(d => d.IdUsuarioAccion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BitacoraControl_Usuario");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.BitacoraControlDocumentoIdUsuarioCreacionNavigations)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .HasConstraintName("FK_BitacoraControl_UsuCrea");

            entity.HasOne(d => d.IdUsuarioEliminacionNavigation).WithMany(p => p.BitacoraControlDocumentoIdUsuarioEliminacionNavigations)
                .HasForeignKey(d => d.IdUsuarioEliminacion)
                .HasConstraintName("FK_BitacoraControl_UsuEli");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.BitacoraControlDocumentoIdUsuarioModificacionNavigations)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("FK_BitacoraControl_UsuMod");
        });

        modelBuilder.Entity<BitacoraTransaccional>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Bitacora__3214EC07A55834AA");

            entity.ToTable("BitacoraTransaccional");

            entity.Property(e => e.Accion)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Detalle)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.DireccionIp)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("DireccionIP");
            entity.Property(e => e.FechaEliminacion).HasColumnType("datetime");
            entity.Property(e => e.FechaHora).HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion).HasColumnType("datetime");

            entity.HasOne(d => d.IdDocumentoNavigation).WithMany(p => p.BitacoraTransaccionals)
                .HasForeignKey(d => d.IdDocumento)
                .HasConstraintName("FK_BitacoraTrans_Documento");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.BitacoraTransaccionalIdUsuarioNavigations)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BitacoraTrans_Usuario");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.BitacoraTransaccionalIdUsuarioCreacionNavigations)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .HasConstraintName("FK_BitacoraTrans_UsuCrea_Audit");

            entity.HasOne(d => d.IdUsuarioEliminacionNavigation).WithMany(p => p.BitacoraTransaccionalIdUsuarioEliminacionNavigations)
                .HasForeignKey(d => d.IdUsuarioEliminacion)
                .HasConstraintName("FK_BitacoraTrans_UsuEli_Audit");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.BitacoraTransaccionalIdUsuarioModificacionNavigations)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("FK_BitacoraTrans_UsuMod_Audit");

            entity.HasOne(d => d.IdVersionNavigation).WithMany(p => p.BitacoraTransaccionals)
                .HasForeignKey(d => d.IdVersion)
                .HasConstraintName("FK_BitacoraTrans_Version");
        });

        modelBuilder.Entity<Departamento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Departam__3214EC07785CA430");

            entity.ToTable("Departamento", tb =>
                {
                    tb.HasTrigger("TRG_AutoFechaMod_Departamento");
                    tb.HasTrigger("TRG_SoftDelete_Departamento");
                });

            entity.Property(e => e.Abreviatura)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime");
            entity.Property(e => e.FechaEliminacion).HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion).HasColumnType("datetime");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.DepartamentoIdUsuarioCreacionNavigations)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .HasConstraintName("FK_Depto_UsuCrea");

            entity.HasOne(d => d.IdUsuarioEliminacionNavigation).WithMany(p => p.DepartamentoIdUsuarioEliminacionNavigations)
                .HasForeignKey(d => d.IdUsuarioEliminacion)
                .HasConstraintName("FK_Depto_UsuEli");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.DepartamentoIdUsuarioModificacionNavigations)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("FK_Depto_UsuMod");

            entity.HasOne(d => d.IdEmpresaNavigation).WithMany(p => p.Departamentos)
                .HasForeignKey(d => d.IdEmpresa)
                .HasConstraintName("FK_Departamento_Empresa");
        });

        modelBuilder.Entity<Documento>(entity =>
        {
            entity.ToTable("Documento", tb =>
                {
                    tb.HasTrigger("TRG_Auditoria_CambioEstadoDoc");
                    tb.HasTrigger("TRG_AutoFechaMod_Documento");
                    tb.HasTrigger("TRG_Outbox_DocumentoVigente");
                    tb.HasTrigger("TRG_SoftDelete_Documento");
                });

            entity.HasIndex(e => new { e.EstadoActual, e.IdDepartamento }, "IX_Documento_Estado_Depto");

            entity.HasIndex(e => e.CodigoInterno, "UQ_Documento_CodigoInterno").IsUnique();

            entity.Property(e => e.CodigoInterno)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EstadoActual)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime");
            entity.Property(e => e.FechaEliminacion).HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion).HasColumnType("datetime");
            entity.Property(e => e.Titulo)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.IdDepartamentoNavigation).WithMany(p => p.Documentos)
                .HasForeignKey(d => d.IdDepartamento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Documento__IdDepartamento");

            entity.HasOne(d => d.IdTipoDocumentoNavigation).WithMany(p => p.Documentos)
                .HasForeignKey(d => d.IdTipoDocumento)
                .HasConstraintName("FK_Documento_TipoDoc");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.DocumentoIdUsuarioCreacionNavigations)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .HasConstraintName("FK_Documento_UsuCrea");

            entity.HasOne(d => d.IdUsuarioEliminacionNavigation).WithMany(p => p.DocumentoIdUsuarioEliminacionNavigations)
                .HasForeignKey(d => d.IdUsuarioEliminacion)
                .HasConstraintName("FK_Documento_UsuEli");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.DocumentoIdUsuarioModificacionNavigations)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("FK_Documento_UsuMod");

            entity.HasOne(d => d.IdUsuarioPropietarioNavigation).WithMany(p => p.DocumentoIdUsuarioPropietarioNavigations)
                .HasForeignKey(d => d.IdUsuarioPropietario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Documento__IdUsuario");

            entity.HasOne(d => d.IdEmpresaNavigation).WithMany(p => p.Documentos)
                .HasForeignKey(d => d.IdEmpresa)
                .HasConstraintName("FK_Documento_Empresa");
        });

        modelBuilder.Entity<DocumentoVersion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Document__3214EC07E3ABFDEA");

            entity.ToTable("Documento_Version", tb =>
                {
                    tb.HasTrigger("TRG_AutoFechaMod_Documento_Version");
                    tb.HasTrigger("TRG_SoftDelete_Documento_Version");
                });

            entity.Property(e => e.ExtensionArchivo)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime");
            entity.Property(e => e.FechaEliminacion).HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion).HasColumnType("datetime");
            entity.Property(e => e.FechaSubida).HasColumnType("datetime");
            entity.Property(e => e.HashDocumento)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.MimeType)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.MotivoCambio)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.RutaArchivoFisico)
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.HasOne(d => d.IdDocumentoNavigation).WithMany(p => p.DocumentoVersions)
                .HasForeignKey(d => d.IdDocumento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Documento__IdDoc__0E6E26BF");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.DocumentoVersionIdUsuarioCreacionNavigations)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .HasConstraintName("FK_DocVersion_UsuCrea");

            entity.HasOne(d => d.IdUsuarioEliminacionNavigation).WithMany(p => p.DocumentoVersionIdUsuarioEliminacionNavigations)
                .HasForeignKey(d => d.IdUsuarioEliminacion)
                .HasConstraintName("FK_DocVersion_UsuEli");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.DocumentoVersionIdUsuarioModificacionNavigations)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("FK_DocVersion_UsuMod");

            entity.HasOne(d => d.IdUsuarioSubeNavigation).WithMany(p => p.DocumentoVersionIdUsuarioSubeNavigations)
                .HasForeignKey(d => d.IdUsuarioSube)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Documento__IdUsu__0F624AF8");
        });

        modelBuilder.Entity<EventoIntegracion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Evento_I__3214EC07201089C8");

            entity.ToTable("Evento_Integracion", tb => tb.HasTrigger("TRG_AutoFechaMod_Evento_Integracion"));

            entity.HasIndex(e => e.Estado, "IX_Evento_Pendientes");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime");
            entity.Property(e => e.FechaEliminacion).HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion).HasColumnType("datetime");
            entity.Property(e => e.FechaProcesado).HasColumnType("datetime");
            entity.Property(e => e.MensajeError).IsUnicode(false);
            entity.Property(e => e.PayloadJson).HasColumnName("PayloadJSON");
            entity.Property(e => e.TipoEvento)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.EventoIntegracionIdUsuarioCreacionNavigations)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .HasConstraintName("FK_Evento_UsuCrea");

            entity.HasOne(d => d.IdUsuarioEliminacionNavigation).WithMany(p => p.EventoIntegracionIdUsuarioEliminacionNavigations)
                .HasForeignKey(d => d.IdUsuarioEliminacion)
                .HasConstraintName("FK_Evento_UsuEli");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.EventoIntegracionIdUsuarioModificacionNavigations)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("FK_Evento_UsuMod");
        });

        modelBuilder.Entity<FlujoAprobacion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Flujo_Ap__3214EC0791F8C7BC");

            entity.ToTable("Flujo_Aprobacion", tb =>
                {
                    tb.HasTrigger("TRG_Auditoria_CambioFirma");
                    tb.HasTrigger("TRG_AutoFechaMod_Flujo_Aprobacion");
                    tb.HasTrigger("TRG_SoftDelete_Flujo_Aprobacion");
                });

            entity.Property(e => e.Comentarios)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.EstadoFirma)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime");
            entity.Property(e => e.FechaEliminacion).HasColumnType("datetime");
            entity.Property(e => e.FechaFirma).HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion).HasColumnType("datetime");
            entity.Property(e => e.MetodoAutenticacion)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TipoAccion)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TokenFirma)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.IdUsuarioAsignadoNavigation).WithMany(p => p.FlujoAprobacionIdUsuarioAsignadoNavigations)
                .HasForeignKey(d => d.IdUsuarioAsignado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Flujo_Apr__IdUsu__17F790F9");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.FlujoAprobacionIdUsuarioCreacionNavigations)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .HasConstraintName("FK_Flujo_UsuCrea");

            entity.HasOne(d => d.IdUsuarioEliminacionNavigation).WithMany(p => p.FlujoAprobacionIdUsuarioEliminacionNavigations)
                .HasForeignKey(d => d.IdUsuarioEliminacion)
                .HasConstraintName("FK_Flujo_UsuEli");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.FlujoAprobacionIdUsuarioModificacionNavigations)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("FK_Flujo_UsuMod");

            entity.HasOne(d => d.IdVersionDocumentoNavigation).WithMany(p => p.FlujoAprobacions)
                .HasForeignKey(d => d.IdVersionDocumento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Flujo_Apr__IdVer__18EBB532");
        });

        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Permiso__3214EC0790777122");

            entity.ToTable("Permiso", tb =>
                {
                    tb.HasTrigger("TRG_AutoFechaMod_Permiso");
                    tb.HasTrigger("TRG_SoftDelete_Permiso");
                });

            entity.HasIndex(e => e.Codigo, "UQ__Permiso__06370DAC66EA2A52").IsUnique();

            entity.Property(e => e.Codigo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime");
            entity.Property(e => e.FechaEliminacion).HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion).HasColumnType("datetime");
            entity.Property(e => e.Modulo)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.PermisoIdUsuarioCreacionNavigations)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .HasConstraintName("FK_Permiso_UsuCrea");

            entity.HasOne(d => d.IdUsuarioEliminacionNavigation).WithMany(p => p.PermisoIdUsuarioEliminacionNavigations)
                .HasForeignKey(d => d.IdUsuarioEliminacion)
                .HasConstraintName("FK_Permiso_UsuEli");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.PermisoIdUsuarioModificacionNavigations)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("FK_Permiso_UsuMod");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Rol__3214EC07DAEC4D91");

            entity.ToTable("Rol", tb =>
                {
                    tb.HasTrigger("TRG_AutoFechaMod_Rol");
                    tb.HasTrigger("TRG_SoftDelete_Rol");
                });

            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime");
            entity.Property(e => e.FechaEliminacion).HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion).HasColumnType("datetime");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.RolIdUsuarioCreacionNavigations)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .HasConstraintName("FK_Rol_UsuCrea");

            entity.HasOne(d => d.IdUsuarioEliminacionNavigation).WithMany(p => p.RolIdUsuarioEliminacionNavigations)
                .HasForeignKey(d => d.IdUsuarioEliminacion)
                .HasConstraintName("FK_Rol_UsuEli");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.RolIdUsuarioModificacionNavigations)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("FK_Rol_UsuMod");
        });

        modelBuilder.Entity<RolPermiso>(entity =>
        {
            entity.HasKey(e => new { e.IdRol, e.IdPermiso }).HasName("PK__Rol_Perm__BA9F7EA083D094C5");

            entity.ToTable("Rol_Permiso", tb =>
                {
                    tb.HasTrigger("TRG_AutoFechaMod_Rol_Permiso");
                    tb.HasTrigger("TRG_SoftDelete_Rol_Permiso");
                });

            entity.Property(e => e.FechaCreacion).HasColumnType("datetime");
            entity.Property(e => e.FechaEliminacion).HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion).HasColumnType("datetime");

            entity.HasOne(d => d.IdPermisoNavigation).WithMany(p => p.RolPermisos)
                .HasForeignKey(d => d.IdPermiso)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Rol_Permi__IdPer__245D67DE");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.RolPermisos)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Rol_Permi__IdRol__25518C17");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.RolPermisoIdUsuarioCreacionNavigations)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .HasConstraintName("FK_RolPermiso_UsuCrea");

            entity.HasOne(d => d.IdUsuarioEliminacionNavigation).WithMany(p => p.RolPermisoIdUsuarioEliminacionNavigations)
                .HasForeignKey(d => d.IdUsuarioEliminacion)
                .HasConstraintName("FK_RolPermiso_UsuEli");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.RolPermisoIdUsuarioModificacionNavigations)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("FK_RolPermiso_UsuMod");
        });

        modelBuilder.Entity<TipoDocumento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TipoDocu__3214EC076AA6E40E");

            entity.ToTable("TipoDocumento", tb =>
                {
                    tb.HasTrigger("TRG_AutoFechaMod_TipoDocumento");
                    tb.HasTrigger("TRG_SoftDelete_TipoDocumento");
                });

            entity.Property(e => e.Abreviatura)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime");
            entity.Property(e => e.FechaEliminacion).HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion).HasColumnType("datetime");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.TipoDocumentoIdUsuarioCreacionNavigations)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .HasConstraintName("FK_TipoDoc_UsuCrea");

            entity.HasOne(d => d.IdUsuarioEliminacionNavigation).WithMany(p => p.TipoDocumentoIdUsuarioEliminacionNavigations)
                .HasForeignKey(d => d.IdUsuarioEliminacion)
                .HasConstraintName("FK_TipoDoc_UsuEli");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.TipoDocumentoIdUsuarioModificacionNavigations)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("FK_TipoDoc_UsuMod");

            entity.HasOne(d => d.IdEmpresaNavigation).WithMany(p => p.TipoDocumentos)
                .HasForeignKey(d => d.IdEmpresa)
                .HasConstraintName("FK_TipoDocumento_Empresa");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuario__3214EC07080D5218");

            entity.ToTable("Usuario", tb =>
                {
                    tb.HasTrigger("TRG_AutoFechaMod_Usuario");
                    tb.HasTrigger("TRG_SoftDelete_Usuario");
                });

            entity.HasIndex(e => e.Correo, "UQ__Usuario__60695A19B10478B7").IsUnique();

            entity.Property(e => e.ApellidoM)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ApellidoP)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Contrasena)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Correo)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime");
            entity.Property(e => e.FechaEliminacion).HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion).HasColumnType("datetime");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.IdDepartamentoNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdDepartamento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuario_Departamento");

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.InverseIdUsuarioCreacionNavigation)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .HasConstraintName("FK_Usuario_UsuCrea");

            entity.HasOne(d => d.IdUsuarioEliminacionNavigation).WithMany(p => p.InverseIdUsuarioEliminacionNavigation)
                .HasForeignKey(d => d.IdUsuarioEliminacion)
                .HasConstraintName("FK_Usuario_UsuEli");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.InverseIdUsuarioModificacionNavigation)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("FK_Usuario_UsuMod");

            entity.HasOne(d => d.IdEmpresaNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdEmpresa)
                .HasConstraintName("FK_Usuario_Empresa");
        });

        modelBuilder.Entity<UsuarioRol>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuario___3214EC077FB163C2");

            entity.ToTable("Usuario_Rol", tb =>
                {
                    tb.HasTrigger("TRG_AutoFechaMod_Usuario_Rol");
                    tb.HasTrigger("TRG_SoftDelete_Usuario_Rol");
                });

            entity.Property(e => e.FechaAsignacion).HasColumnType("datetime");
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime");
            entity.Property(e => e.FechaEliminacion).HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion).HasColumnType("datetime");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.UsuarioRols)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UsuarioRol_Rol");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.UsuarioRolIdUsuarioNavigations)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UsuarioRol_Usuario")
                .IsRequired();

            entity.HasOne(d => d.IdUsuarioCreacionNavigation).WithMany(p => p.UsuarioRolIdUsuarioCreacionNavigations)
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .HasConstraintName("FK_UsuRol_UsuCrea");

            entity.HasOne(d => d.IdUsuarioEliminacionNavigation).WithMany(p => p.UsuarioRolIdUsuarioEliminacionNavigations)
                .HasForeignKey(d => d.IdUsuarioEliminacion)
                .HasConstraintName("FK_UsuRol_UsuEli");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation).WithMany(p => p.UsuarioRolIdUsuarioModificacionNavigations)
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("FK_UsuRol_UsuMod");
        });

        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("Empresa", tb =>
            {
                tb.HasTrigger("TRG_AutoFechaMod_Empresa");
            });

            entity.HasIndex(e => e.Slug, "UQ_Empresa_Slug").IsUnique();

            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Slug)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RFC)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CorreoContacto)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.FechaRegistro)
                .HasColumnType("datetime")
                .HasDefaultValueSql("getdate()");
            entity.Property(e => e.Estatus)
                .HasDefaultValueSql("1");
            entity.Property(e => e.CamposPersonalizados)
                .IsUnicode(true);

            // Campos de auditoría
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion).HasColumnType("datetime");
            entity.Property(e => e.FechaEliminacion).HasColumnType("datetime");

            // Relaciones de auditoría
            entity.HasOne(d => d.IdUsuarioCreacionNavigation)
                .WithMany()
                .HasForeignKey(d => d.IdUsuarioCreacion)
                .HasConstraintName("FK_Empresa_UsuCrea");

            entity.HasOne(d => d.IdUsuarioModificacionNavigation)
                .WithMany()
                .HasForeignKey(d => d.IdUsuarioModificacion)
                .HasConstraintName("FK_Empresa_UsuMod");

            entity.HasOne(d => d.IdUsuarioEliminacionNavigation)
                .WithMany()
                .HasForeignKey(d => d.IdUsuarioEliminacion)
                .HasConstraintName("FK_Empresa_UsuEli");
        });
        OnModelCreatingPartial(modelBuilder);
    }

    private void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
    }
}
