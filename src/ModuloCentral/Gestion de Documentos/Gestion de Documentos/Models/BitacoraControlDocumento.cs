using System;
using System.Collections.Generic;

namespace Gestion_de_Documentos.Models;

public partial class BitacoraControlDocumento
{
    public int Id { get; set; }

    public int IdDocumento { get; set; }

    public DateTime FechaEvento { get; set; }

    public string TipoCambio { get; set; } = null!;

    public string? ValorAnterior { get; set; }

    public string? ValorNuevo { get; set; }

    public string? Observaciones { get; set; }

    public int IdUsuarioAccion { get; set; }

    public bool? Estatus { get; set; }

    public int? IdUsuarioCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public int? IdUsuarioEliminacion { get; set; }

    public DateTime? FechaEliminacion { get; set; }

    public virtual Documento IdDocumentoNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioAccionNavigation { get; set; } = null!;

    public virtual Usuario? IdUsuarioCreacionNavigation { get; set; }

    public virtual Usuario? IdUsuarioEliminacionNavigation { get; set; }

    public virtual Usuario? IdUsuarioModificacionNavigation { get; set; }
}
