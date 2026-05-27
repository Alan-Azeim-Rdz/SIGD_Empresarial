using System;
using System.Collections.Generic;

namespace Gestion_de_Documentos.Models;

public partial class DocumentoVersion
{
    public int Id { get; set; }

    public int IdDocumento { get; set; }

    public int NumeroVersion { get; set; }

    public int VersionMinor { get; set; } = 0;

    public string RutaArchivoFisico { get; set; } = null!;

    public string HashDocumento { get; set; } = null!;

    public string? MotivoCambio { get; set; }

    public int IdUsuarioSube { get; set; }

    public DateTime? FechaSubida { get; set; }

    public int? IdUsuarioCreacion { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public int? IdUsuarioEliminacion { get; set; }

    public DateTime? FechaEliminacion { get; set; }

    public bool? Estatus { get; set; }

    public string? ExtensionArchivo { get; set; }

    public string? MimeType { get; set; }

    public long? TamanoBytes { get; set; }

    public virtual ICollection<BitacoraTransaccional> BitacoraTransaccionals { get; set; } = new List<BitacoraTransaccional>();

    public virtual ICollection<FlujoAprobacion> FlujoAprobacions { get; set; } = new List<FlujoAprobacion>();

    public virtual Documento IdDocumentoNavigation { get; set; } = null!;

    public virtual Usuario? IdUsuarioCreacionNavigation { get; set; }

    public virtual Usuario? IdUsuarioEliminacionNavigation { get; set; }

    public virtual Usuario? IdUsuarioModificacionNavigation { get; set; }

    public virtual Usuario IdUsuarioSubeNavigation { get; set; } = null!;
}
