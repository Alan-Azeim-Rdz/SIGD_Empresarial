using System;
using System.Collections.Generic;

namespace Gestion_de_Documentos.Models;

public partial class EventoIntegracion
{
    public Guid Id { get; set; }

    public string TipoEvento { get; set; } = null!;

    public string PayloadJson { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaProcesado { get; set; }

    public int? Intentos { get; set; }

    public string? MensajeError { get; set; }

    public int? IdUsuarioCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public int? IdUsuarioEliminacion { get; set; }

    public DateTime? FechaEliminacion { get; set; }

    public bool? Estatus { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual Usuario? IdUsuarioCreacionNavigation { get; set; }

    public virtual Usuario? IdUsuarioEliminacionNavigation { get; set; }

    public virtual Usuario? IdUsuarioModificacionNavigation { get; set; }
}
