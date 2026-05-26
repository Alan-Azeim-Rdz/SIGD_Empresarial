using System;
using System.Collections.Generic;

namespace Gestion_de_Documentos.Models;

public partial class Documento
{
    public int Id { get; set; }

    public string CodigoInterno { get; set; } = null!;

    public string Titulo { get; set; } = null!;

    public int? IdEmpresa { get; set; }

    public string? CamposPersonalizadosValores { get; set; }

    public virtual Empresa? IdEmpresaNavigation { get; set; }

    public int IdDepartamento { get; set; }

    public string EstadoActual { get; set; } = null!;

    public int IdUsuarioPropietario { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public bool? Estatus { get; set; }

    public int? IdUsuarioCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public int? IdUsuarioEliminacion { get; set; }

    public DateTime? FechaEliminacion { get; set; }

    public int? IdTipoDocumento { get; set; }

    public virtual ICollection<BitacoraControlDocumento> BitacoraControlDocumentos { get; set; } = new List<BitacoraControlDocumento>();

    public virtual ICollection<BitacoraTransaccional> BitacoraTransaccionals { get; set; } = new List<BitacoraTransaccional>();

    public virtual ICollection<DocumentoVersion> DocumentoVersions { get; set; } = new List<DocumentoVersion>();

    public virtual Departamento IdDepartamentoNavigation { get; set; } = null!;

    public virtual TipoDocumento? IdTipoDocumentoNavigation { get; set; }

    public virtual Usuario? IdUsuarioCreacionNavigation { get; set; }

    public virtual Usuario? IdUsuarioEliminacionNavigation { get; set; }

    public virtual Usuario? IdUsuarioModificacionNavigation { get; set; }

    public virtual Usuario IdUsuarioPropietarioNavigation { get; set; } = null!;
}
