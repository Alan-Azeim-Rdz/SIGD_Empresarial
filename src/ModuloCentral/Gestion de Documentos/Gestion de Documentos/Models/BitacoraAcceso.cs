using System;
using System.Collections.Generic;

namespace Gestion_de_Documentos.Models;

public partial class BitacoraAcceso
{
    public int Id { get; set; }

    public int IdUsuario { get; set; }

    public DateTime? FechaHoraIntento { get; set; }

    public string? DireccionIp { get; set; }

    public string EstadoIntento { get; set; } = null!;

    public int? IdUsuarioCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public int? IdUsuarioEliminacion { get; set; }

    public DateTime? FechaEliminacion { get; set; }

    public bool? Estatus { get; set; }

    public virtual Usuario? IdUsuarioCreacionNavigation { get; set; }

    public virtual Usuario? IdUsuarioEliminacionNavigation { get; set; }

    public virtual Usuario? IdUsuarioModificacionNavigation { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
