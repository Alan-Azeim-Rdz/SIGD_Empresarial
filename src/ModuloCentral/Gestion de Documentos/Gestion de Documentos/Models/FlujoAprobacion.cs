using System;
using System.Collections.Generic;

namespace Gestion_de_Documentos.Models;

public partial class FlujoAprobacion
{
    public int Id { get; set; }

    public int IdVersionDocumento { get; set; }

    public int IdUsuarioAsignado { get; set; }

    public string TipoAccion { get; set; } = null!;

    public string EstadoFirma { get; set; } = null!;

    public string? Comentarios { get; set; }

    public DateTime? FechaFirma { get; set; }

    public int Orden { get; set; }

    public int? IdUsuarioCreacion { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public int? IdUsuarioEliminacion { get; set; }

    public DateTime? FechaEliminacion { get; set; }

    public bool? Estatus { get; set; }

    public string? TokenFirma { get; set; }

    public string? MetodoAutenticacion { get; set; }
    
    public string? IpOrigenRemitente { get; set; }
    
    public string? IpOrigenFirmante { get; set; }

    public virtual Usuario IdUsuarioAsignadoNavigation { get; set; } = null!;

    public virtual Usuario? IdUsuarioCreacionNavigation { get; set; }

    public virtual Usuario? IdUsuarioEliminacionNavigation { get; set; }

    public virtual Usuario? IdUsuarioModificacionNavigation { get; set; }

    public virtual DocumentoVersion IdVersionDocumentoNavigation { get; set; } = null!;
}
