using System;
using System.Collections.Generic;

namespace Gestion_de_Documentos.Models;

public partial class TipoDocumento
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public int? IdEmpresa { get; set; }

    public virtual Empresa? IdEmpresaNavigation { get; set; }

    public string Abreviatura { get; set; } = null!;

    public int TiempoRetencionMeses { get; set; }

    public bool? Estatus { get; set; }

    public int? IdUsuarioCreacion { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public int? IdUsuarioEliminacion { get; set; }

    public DateTime? FechaEliminacion { get; set; }

    public virtual ICollection<Documento> Documentos { get; set; } = new List<Documento>();

    public virtual Usuario? IdUsuarioCreacionNavigation { get; set; }

    public virtual Usuario? IdUsuarioEliminacionNavigation { get; set; }

    public virtual Usuario? IdUsuarioModificacionNavigation { get; set; }
}
