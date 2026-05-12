using System;
using System.Collections.Generic;

namespace Gestion_de_Documentos.Models;

public partial class BitacoraTransaccional
{
    public int Id { get; set; }

    public int IdUsuario { get; set; }

    public int? IdDocumento { get; set; }

    public int? IdVersion { get; set; }

    public string Accion { get; set; } = null!;

    public DateTime FechaHora { get; set; }

    public string? DireccionIp { get; set; }

    public string? Detalle { get; set; }

    public int? IdUsuarioCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public int? IdUsuarioEliminacion { get; set; }

    public DateTime? FechaEliminacion { get; set; }

    public bool? Estatus { get; set; }

    public virtual Documento? IdDocumentoNavigation { get; set; }

    public virtual Usuario? IdUsuarioCreacionNavigation { get; set; }

    public virtual Usuario? IdUsuarioEliminacionNavigation { get; set; }

    public virtual Usuario? IdUsuarioModificacionNavigation { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual DocumentoVersion? IdVersionNavigation { get; set; }
}
