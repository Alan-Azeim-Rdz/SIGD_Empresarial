using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gestion_de_Documentos.Models;

public partial class Usuario
{
    public int Id { get; set; }

    public int IdDepartamento { get; set; }

    public int? IdEmpresa { get; set; }

    public virtual Empresa? IdEmpresaNavigation { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
    public string Nombre { get; set; } = null!;

    [Required(ErrorMessage = "El primer apellido es obligatorio.")]
    [StringLength(100, ErrorMessage = "El primer apellido no puede exceder los 100 caracteres.")]
    public string ApellidoP { get; set; } = null!;

    [StringLength(100, ErrorMessage = "El segundo apellido no puede exceder los 100 caracteres.")]
    public string? ApellidoM { get; set; }

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
    [StringLength(150, ErrorMessage = "El correo electrónico no puede exceder los 150 caracteres.")]
    public string Correo { get; set; } = null!;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres.")]
    public string Contrasena { get; set; } = null!;

    public int? IdUsuarioCreacion { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public int? IdUsuarioEliminacion { get; set; }

    public DateTime? FechaEliminacion { get; set; }

    public bool? Estatus { get; set; }

    public virtual ICollection<BitacoraAcceso> BitacoraAccesoIdUsuarioCreacionNavigations { get; set; } = new List<BitacoraAcceso>();

    public virtual ICollection<BitacoraAcceso> BitacoraAccesoIdUsuarioEliminacionNavigations { get; set; } = new List<BitacoraAcceso>();

    public virtual ICollection<BitacoraAcceso> BitacoraAccesoIdUsuarioModificacionNavigations { get; set; } = new List<BitacoraAcceso>();

    public virtual ICollection<BitacoraAcceso> BitacoraAccesoIdUsuarioNavigations { get; set; } = new List<BitacoraAcceso>();

    public virtual ICollection<BitacoraControlDocumento> BitacoraControlDocumentoIdUsuarioAccionNavigations { get; set; } = new List<BitacoraControlDocumento>();

    public virtual ICollection<BitacoraControlDocumento> BitacoraControlDocumentoIdUsuarioCreacionNavigations { get; set; } = new List<BitacoraControlDocumento>();

    public virtual ICollection<BitacoraControlDocumento> BitacoraControlDocumentoIdUsuarioEliminacionNavigations { get; set; } = new List<BitacoraControlDocumento>();

    public virtual ICollection<BitacoraControlDocumento> BitacoraControlDocumentoIdUsuarioModificacionNavigations { get; set; } = new List<BitacoraControlDocumento>();

    public virtual ICollection<BitacoraTransaccional> BitacoraTransaccionalIdUsuarioCreacionNavigations { get; set; } = new List<BitacoraTransaccional>();

    public virtual ICollection<BitacoraTransaccional> BitacoraTransaccionalIdUsuarioEliminacionNavigations { get; set; } = new List<BitacoraTransaccional>();

    public virtual ICollection<BitacoraTransaccional> BitacoraTransaccionalIdUsuarioModificacionNavigations { get; set; } = new List<BitacoraTransaccional>();

    public virtual ICollection<BitacoraTransaccional> BitacoraTransaccionalIdUsuarioNavigations { get; set; } = new List<BitacoraTransaccional>();

    public virtual ICollection<Departamento> DepartamentoIdUsuarioCreacionNavigations { get; set; } = new List<Departamento>();

    public virtual ICollection<Departamento> DepartamentoIdUsuarioEliminacionNavigations { get; set; } = new List<Departamento>();

    public virtual ICollection<Departamento> DepartamentoIdUsuarioModificacionNavigations { get; set; } = new List<Departamento>();

    public virtual ICollection<Documento> DocumentoIdUsuarioCreacionNavigations { get; set; } = new List<Documento>();

    public virtual ICollection<Documento> DocumentoIdUsuarioEliminacionNavigations { get; set; } = new List<Documento>();

    public virtual ICollection<Documento> DocumentoIdUsuarioModificacionNavigations { get; set; } = new List<Documento>();

    public virtual ICollection<Documento> DocumentoIdUsuarioPropietarioNavigations { get; set; } = new List<Documento>();

    public virtual ICollection<DocumentoVersion> DocumentoVersionIdUsuarioCreacionNavigations { get; set; } = new List<DocumentoVersion>();

    public virtual ICollection<DocumentoVersion> DocumentoVersionIdUsuarioEliminacionNavigations { get; set; } = new List<DocumentoVersion>();

    public virtual ICollection<DocumentoVersion> DocumentoVersionIdUsuarioModificacionNavigations { get; set; } = new List<DocumentoVersion>();

    public virtual ICollection<DocumentoVersion> DocumentoVersionIdUsuarioSubeNavigations { get; set; } = new List<DocumentoVersion>();

    public virtual ICollection<EventoIntegracion> EventoIntegracionIdUsuarioCreacionNavigations { get; set; } = new List<EventoIntegracion>();

    public virtual ICollection<EventoIntegracion> EventoIntegracionIdUsuarioEliminacionNavigations { get; set; } = new List<EventoIntegracion>();

    public virtual ICollection<EventoIntegracion> EventoIntegracionIdUsuarioModificacionNavigations { get; set; } = new List<EventoIntegracion>();

    public virtual ICollection<FlujoAprobacion> FlujoAprobacionIdUsuarioAsignadoNavigations { get; set; } = new List<FlujoAprobacion>();

    public virtual ICollection<FlujoAprobacion> FlujoAprobacionIdUsuarioCreacionNavigations { get; set; } = new List<FlujoAprobacion>();

    public virtual ICollection<FlujoAprobacion> FlujoAprobacionIdUsuarioEliminacionNavigations { get; set; } = new List<FlujoAprobacion>();

    public virtual ICollection<FlujoAprobacion> FlujoAprobacionIdUsuarioModificacionNavigations { get; set; } = new List<FlujoAprobacion>();

    public virtual Departamento IdDepartamentoNavigation { get; set; } = null!;

    public virtual Usuario? IdUsuarioCreacionNavigation { get; set; }

    public virtual Usuario? IdUsuarioEliminacionNavigation { get; set; }

    public virtual Usuario? IdUsuarioModificacionNavigation { get; set; }

    public virtual ICollection<Usuario> InverseIdUsuarioCreacionNavigation { get; set; } = new List<Usuario>();

    public virtual ICollection<Usuario> InverseIdUsuarioEliminacionNavigation { get; set; } = new List<Usuario>();

    public virtual ICollection<Usuario> InverseIdUsuarioModificacionNavigation { get; set; } = new List<Usuario>();

    public virtual ICollection<Permiso> PermisoIdUsuarioCreacionNavigations { get; set; } = new List<Permiso>();

    public virtual ICollection<Permiso> PermisoIdUsuarioEliminacionNavigations { get; set; } = new List<Permiso>();

    public virtual ICollection<Permiso> PermisoIdUsuarioModificacionNavigations { get; set; } = new List<Permiso>();

    public virtual ICollection<Rol> RolIdUsuarioCreacionNavigations { get; set; } = new List<Rol>();

    public virtual ICollection<Rol> RolIdUsuarioEliminacionNavigations { get; set; } = new List<Rol>();

    public virtual ICollection<Rol> RolIdUsuarioModificacionNavigations { get; set; } = new List<Rol>();

    // 👇 ESTA ES LA LÍNEA MÁGICA QUE ARREGLA EL ERROR 👇
    [NotMapped]
    public virtual ICollection<UsuarioRol> UsuarioRols { get; set; } = new List<UsuarioRol>();

    public virtual ICollection<RolPermiso> RolPermisoIdUsuarioCreacionNavigations { get; set; } = new List<RolPermiso>();

    public virtual ICollection<RolPermiso> RolPermisoIdUsuarioEliminacionNavigations { get; set; } = new List<RolPermiso>();

    public virtual ICollection<RolPermiso> RolPermisoIdUsuarioModificacionNavigations { get; set; } = new List<RolPermiso>();

    public virtual ICollection<TipoDocumento> TipoDocumentoIdUsuarioCreacionNavigations { get; set; } = new List<TipoDocumento>();

    public virtual ICollection<TipoDocumento> TipoDocumentoIdUsuarioEliminacionNavigations { get; set; } = new List<TipoDocumento>();

    public virtual ICollection<TipoDocumento> TipoDocumentoIdUsuarioModificacionNavigations { get; set; } = new List<TipoDocumento>();

    public virtual ICollection<UsuarioRol> UsuarioRolIdUsuarioCreacionNavigations { get; set; } = new List<UsuarioRol>();

    public virtual ICollection<UsuarioRol> UsuarioRolIdUsuarioEliminacionNavigations { get; set; } = new List<UsuarioRol>();

    public virtual ICollection<UsuarioRol> UsuarioRolIdUsuarioModificacionNavigations { get; set; } = new List<UsuarioRol>();

    public virtual ICollection<UsuarioRol> UsuarioRolIdUsuarioNavigations { get; set; } = new List<UsuarioRol>();
}