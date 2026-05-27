using System;
using System.Collections.Generic;

namespace Gestion_de_Documentos.Models;

public partial class Empresa
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? RFC { get; set; }

    public string? CorreoContacto { get; set; }

    public DateTime FechaRegistro { get; set; }

    public bool Estatus { get; set; }

    public string? CamposPersonalizados { get; set; }

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();

    public virtual ICollection<Departamento> Departamentos { get; set; } = new List<Departamento>();

    public virtual ICollection<TipoDocumento> TipoDocumentos { get; set; } = new List<TipoDocumento>();

    public virtual ICollection<Documento> Documentos { get; set; } = new List<Documento>();
}
