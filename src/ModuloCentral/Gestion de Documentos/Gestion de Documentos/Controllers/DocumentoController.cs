using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Gestion_de_Documentos.Models;
using Gestion_de_Documentos.Services;
using System.Security.Cryptography;

namespace Gestion_de_Documentos.Controllers
{
    [Authorize]
    public class DocumentoController : Controller
    {
        private readonly DirContext _context;
        private readonly IMongoGridFsService _gridFsService;
        private readonly BusquedaIntegrationService _busquedaService;

        public DocumentoController(DirContext context, IMongoGridFsService gridFsService, BusquedaIntegrationService busquedaService)
        {
            _context = context;
            _gridFsService = gridFsService;
            _busquedaService = busquedaService;
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        private int GetCurrentUserEmpresaId()
        {
            var claim = User.FindFirst("IdEmpresa")?.Value;
            return int.TryParse(claim, out var empId) ? empId : 0;
        }

        public async Task<IActionResult> Index(int pagina = 1)
        {
            var userId = GetCurrentUserId();
            var empresaId = GetCurrentUserEmpresaId();
            var esAdminOrAuditor = User.IsInRole("Administrador") || User.IsInRole("Superior") || User.IsInRole("Super Administrador") || User.IsInRole("Auditor");
            const int porPagina = 10;

            IQueryable<Documento> query;

            if (esAdminOrAuditor)
            {
                query = _context.Documentos
                    .Where(d => d.Estatus == true && d.IdEmpresa == empresaId &&
                        (d.EstadoActual != "En Revision" ||
                         d.IdUsuarioCreacion == userId ||
                         d.DocumentoVersions.Any(v => v.FlujoAprobacions.Any(f => f.IdUsuarioAsignado == userId && (f.EstadoFirma == "Firmado" || f.EstadoFirma == "Rechazado")))
                        ));
            }
            else
            {
                var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == userId);
                var userDeptoId = user?.IdDepartamento;

                var pendingDocIds = await _context.FlujoAprobacions
                    .Where(f => f.IdUsuarioAsignado == userId && f.EstadoFirma == "Pendiente" && f.Estatus == true)
                    .Select(f => f.IdVersionDocumentoNavigation.IdDocumento)
                    .Distinct()
                    .ToListAsync();

                query = _context.Documentos
                    .Where(d => d.Estatus == true && d.IdEmpresa == empresaId && 
                        (d.IdUsuarioCreacion == userId || 
                         (d.IdDepartamento == userDeptoId && d.EstadoActual == "Vigente") ||
                         pendingDocIds.Contains(d.Id)));
            }

            var total = await query.CountAsync();

            var documentos = await query
                .Include(d => d.IdDepartamentoNavigation)
                .Include(d => d.IdTipoDocumentoNavigation)
                .Include(d => d.DocumentoVersions)
                    .ThenInclude(v => v.FlujoAprobacions)
                        .ThenInclude(f => f.IdUsuarioAsignadoNavigation)
                .OrderByDescending(d => d.FechaCreacion)
                .Skip((pagina - 1) * porPagina)
                .Take(porPagina)
                .ToListAsync();

            ViewBag.PaginaActual  = pagina;
            ViewBag.TotalPaginas  = (int)Math.Ceiling(total / (double)porPagina);
            ViewBag.TotalDocs     = total;

            return View(documentos);
        }

        public async Task<IActionResult> Crear()
        {
            ViewBag.TiposDocumento = await _context.TipoDocumentos.Where(t => t.Estatus == true).ToListAsync();
            ViewBag.Departamentos = await _context.Departamentos.Where(d => d.Estatus == true).ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Documento doc, IFormFile archivoPdf)
        {
            if (archivoPdf == null || archivoPdf.Length == 0)
            {
                ModelState.AddModelError("", "Debe seleccionar un archivo PDF.");
            }
            else if (archivoPdf.ContentType != "application/pdf" && !archivoPdf.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Solo se permiten archivos en formato PDF.");
            }

            ModelState.Remove("IdUsuarioPropietario");
            ModelState.Remove("EstadoActual");
            ModelState.Remove("IdDepartamentoNavigation");
            ModelState.Remove("IdTipoDocumentoNavigation");
            ModelState.Remove("IdUsuarioCreacionNavigation");
            ModelState.Remove("IdUsuarioPropietarioNavigation");
            ModelState.Remove("IdEmpresaNavigation");
            ModelState.Remove("EstadoActual");
            ModelState.Remove("BitacoraControlDocumentos");
            ModelState.Remove("BitacoraTransaccionals");
            ModelState.Remove("DocumentoVersions");

            if (ModelState.IsValid)
            {
                var userId = GetCurrentUserId();
                var empresaId = GetCurrentUserEmpresaId();
                
                // 1. Guardar el archivo en MongoDB GridFS
                using var stream = archivoPdf.OpenReadStream();
                var objectIdStr = await _gridFsService.SubirArchivoAsync(stream, archivoPdf.FileName, archivoPdf.ContentType);

                // Calcular el hash (SHA256) del archivo físico subido
                stream.Position = 0;
                using var sha256 = SHA256.Create();
                var hashBytes = sha256.ComputeHash(stream);
                var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToUpperInvariant();

                // 2. Crear el registro base de Documento
                doc.EstadoActual = "Borrador";
                doc.Estatus = true;
                doc.FechaCreacion = DateTime.Now;
                doc.IdUsuarioCreacion = userId;
                doc.IdUsuarioPropietario = userId;
                doc.IdEmpresa = empresaId;
                _context.Documentos.Add(doc);
                await _context.SaveChangesAsync(); // Para obtener el doc.Id

                // 3. Crear la Versión Inicial (V0.1)
                var version = new DocumentoVersion
                {
                    IdDocumento = doc.Id,
                    NumeroVersion = 0,
                    VersionMinor = 1,
                    RutaArchivoFisico = objectIdStr, // Aquí guardamos el gridfs:id
                    HashDocumento = hashString,
                    IdUsuarioSube = userId,
                    FechaSubida = DateTime.Now,
                    Estatus = true,
                    ExtensionArchivo = ".pdf",
                    MimeType = "application/pdf",
                    TamanoBytes = archivoPdf.Length,
                    IdUsuarioCreacion = userId,
                    FechaCreacion = DateTime.Now
                };
                
                _context.DocumentoVersions.Add(version);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.TiposDocumento = await _context.TipoDocumentos.Where(t => t.Estatus == true).ToListAsync();
            ViewBag.Departamentos = await _context.Departamentos.Where(d => d.Estatus == true).ToListAsync();
            return View(doc);
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var userId = GetCurrentUserId();
            var doc = await _context.Documentos.FirstOrDefaultAsync(d => d.Id == id && d.IdUsuarioCreacion == userId && d.Estatus == true);

            if (doc == null || doc.EstadoActual != "Borrador")
                return NotFound("Documento no válido o no se encuentra en estado Borrador.");

            ViewBag.TiposDocumento = await _context.TipoDocumentos.Where(t => t.Estatus == true).ToListAsync();
            ViewBag.Departamentos = await _context.Departamentos.Where(d => d.Estatus == true).ToListAsync();
            return View(doc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Documento model)
        {
            var userId = GetCurrentUserId();
            var doc = await _context.Documentos.FirstOrDefaultAsync(d => d.Id == id && d.IdUsuarioCreacion == userId && d.Estatus == true);

            if (doc == null || doc.EstadoActual != "Borrador")
                return NotFound("Documento no válido o no se encuentra en estado Borrador.");

            // Remover validaciones innecesarias del ModelState (Navigation properties)
            ModelState.Remove("IdUsuarioPropietario");
            ModelState.Remove("EstadoActual");
            ModelState.Remove("IdDepartamentoNavigation");
            ModelState.Remove("IdTipoDocumentoNavigation");
            ModelState.Remove("IdUsuarioCreacionNavigation");
            ModelState.Remove("IdUsuarioPropietarioNavigation");
            ModelState.Remove("IdUsuarioModificacionNavigation");
            ModelState.Remove("IdUsuarioEliminacionNavigation");
            ModelState.Remove("IdEmpresaNavigation");
            ModelState.Remove("BitacoraControlDocumentos");
            ModelState.Remove("BitacoraTransaccionals");
            ModelState.Remove("DocumentoVersions");

            if (ModelState.IsValid)
            {
                doc.CodigoInterno = model.CodigoInterno;
                doc.Titulo = model.Titulo;
                doc.IdTipoDocumento = model.IdTipoDocumento;
                doc.IdDepartamento = model.IdDepartamento;
                
                doc.FechaModificacion = DateTime.Now;
                doc.IdUsuarioModificacion = userId;

                _context.Update(doc);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Detalle), new { id = doc.Id });
            }

            ViewBag.TiposDocumento = await _context.TipoDocumentos.Where(t => t.Estatus == true).ToListAsync();
            ViewBag.Departamentos = await _context.Departamentos.Where(d => d.Estatus == true).ToListAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> SubirNuevaVersion(int id)
        {
            var userId = GetCurrentUserId();
            var doc = await _context.Documentos
                .Include(d => d.DocumentoVersions)
                .FirstOrDefaultAsync(d => d.Id == id && d.IdUsuarioCreacion == userId && d.Estatus == true);

            if (doc == null || (doc.EstadoActual != "Borrador" && doc.EstadoActual != "Rechazado" && doc.EstadoActual != "Vigente"))
                return NotFound("Documento no válido o no se puede modificar en su estado actual.");

            return View(doc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubirNuevaVersion(int id, IFormFile archivoPdf, string? motivoCambio)
        {
            var userId = GetCurrentUserId();
            var doc = await _context.Documentos
                .Include(d => d.DocumentoVersions)
                .FirstOrDefaultAsync(d => d.Id == id && d.IdUsuarioCreacion == userId && d.Estatus == true);

            if (doc == null || (doc.EstadoActual != "Borrador" && doc.EstadoActual != "Rechazado" && doc.EstadoActual != "Vigente"))
                return NotFound("Documento no válido o no se puede modificar en su estado actual.");

            if (archivoPdf == null || archivoPdf.Length == 0)
            {
                ModelState.AddModelError("", "Debe seleccionar un archivo PDF.");
            }
            else if (archivoPdf.ContentType != "application/pdf" && !archivoPdf.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Solo se permiten archivos en formato PDF.");
            }

            if (ModelState.IsValid)
            {
                // Determinar el número de versión (Manejo de versiones decimales)
                int nuevaVersionNum = 0;
                int nuevoMinor = 1;
                
                if (doc.DocumentoVersions.Any())
                {
                    var ultimaVersion = doc.DocumentoVersions.OrderByDescending(v => v.NumeroVersion).ThenByDescending(v => v.VersionMinor).First();
                    nuevaVersionNum = ultimaVersion.NumeroVersion;
                    if (doc.EstadoActual == "Vigente")
                    {
                        nuevoMinor = 1;
                    }
                    else
                    {
                        nuevoMinor = ultimaVersion.VersionMinor + 1;
                    }
                }

                // 1. Guardar el archivo en MongoDB GridFS
                using var stream = archivoPdf.OpenReadStream();
                var objectIdStr = await _gridFsService.SubirArchivoAsync(stream, archivoPdf.FileName, archivoPdf.ContentType);

                // Calcular el hash (SHA256) del archivo físico subido
                stream.Position = 0;
                using var sha256 = SHA256.Create();
                var hashBytes = sha256.ComputeHash(stream);
                var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToUpperInvariant();

                // 2. Crear la nueva versión
                var nuevaVersion = new DocumentoVersion
                {
                    IdDocumento = doc.Id,
                    NumeroVersion = nuevaVersionNum,
                    VersionMinor = nuevoMinor,
                    RutaArchivoFisico = objectIdStr,
                    HashDocumento = hashString,
                    IdUsuarioSube = userId,
                    FechaSubida = DateTime.Now,
                    Estatus = true,
                    ExtensionArchivo = ".pdf",
                    MimeType = "application/pdf",
                    TamanoBytes = archivoPdf.Length,
                    IdUsuarioCreacion = userId,
                    MotivoCambio = motivoCambio,
                    FechaCreacion = DateTime.Now
                };

                _context.DocumentoVersions.Add(nuevaVersion);

                bool wasVigenteOrRechazado = doc.EstadoActual == "Vigente" || doc.EstadoActual == "Rechazado";
                if (wasVigenteOrRechazado)
                {
                    doc.EstadoActual = "Borrador";
                }

                doc.FechaModificacion = DateTime.Now;
                doc.IdUsuarioModificacion = userId;
                
                await _context.SaveChangesAsync();

                if (wasVigenteOrRechazado)
                {
                    // Desindexar de la búsqueda ya que pasa a Borrador (no vigente)
                    await _busquedaService.DesindexarDocumentoAsync(doc.Id);
                }

                return RedirectToAction(nameof(Detalle), new { id = doc.Id });
            }

            return View(doc);
        }

        [HttpGet]
        public async Task<IActionResult> Historial(int id)
        {
            var userId = GetCurrentUserId();
            var esAdminOrAuditor = User.IsInRole("Administrador") || User.IsInRole("Superior") || User.IsInRole("Super Administrador") || User.IsInRole("Auditor");
            var empresaId = GetCurrentUserEmpresaId();

            var doc = await _context.Documentos
                .Include(d => d.IdDepartamentoNavigation)
                .Include(d => d.IdTipoDocumentoNavigation)
                .Include(d => d.DocumentoVersions)
                    .ThenInclude(v => v.FlujoAprobacions)
                        .ThenInclude(f => f.IdUsuarioAsignadoNavigation)
                .FirstOrDefaultAsync(d => d.Id == id && d.IdEmpresa == empresaId && d.Estatus == true);

            if (doc == null)
                return NotFound("Documento no válido.");

            if (!esAdminOrAuditor)
            {
                var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == userId);
                var userDeptoId = user?.IdDepartamento;

                bool isCreator = doc.IdUsuarioCreacion == userId;
                bool isDepartmentVigente = doc.IdDepartamento == userDeptoId && doc.EstadoActual == "Vigente";
                bool isAssignedReviewer = doc.DocumentoVersions
                    .SelectMany(v => v.FlujoAprobacions)
                    .Any(f => f.IdUsuarioAsignado == userId && f.EstadoFirma == "Pendiente" && f.Estatus == true);

                if (!isCreator && !isDepartmentVigente && !isAssignedReviewer)
                {
                    return RedirectToAction("AccesoDenegado", "Auth");
                }
            }

            // Ordenamos versiones descendentemente
            doc.DocumentoVersions = doc.DocumentoVersions.OrderByDescending(v => v.NumeroVersion).ThenByDescending(v => v.VersionMinor).ToList();

            return View(doc);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var userId = GetCurrentUserId();
            var esAdminOrAuditor = User.IsInRole("Administrador") || User.IsInRole("Superior") || User.IsInRole("Super Administrador") || User.IsInRole("Auditor");
            var empresaId = GetCurrentUserEmpresaId();

            var doc = await _context.Documentos
                .Include(d => d.IdDepartamentoNavigation)
                .Include(d => d.IdTipoDocumentoNavigation)
                .Include(d => d.DocumentoVersions)
                    .ThenInclude(v => v.FlujoAprobacions)
                        .ThenInclude(f => f.IdUsuarioAsignadoNavigation)
                .FirstOrDefaultAsync(d => d.Id == id && d.IdEmpresa == empresaId && d.Estatus == true);

            if (doc == null)
                return NotFound();

            if (!esAdminOrAuditor)
            {
                var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == userId);
                var userDeptoId = user?.IdDepartamento;

                bool isCreator = doc.IdUsuarioCreacion == userId;
                bool isDepartmentVigente = doc.IdDepartamento == userDeptoId && doc.EstadoActual == "Vigente";
                bool isAssignedReviewer = doc.DocumentoVersions
                    .SelectMany(v => v.FlujoAprobacions)
                    .Any(f => f.IdUsuarioAsignado == userId && f.EstadoFirma == "Pendiente" && f.Estatus == true);

                if (!isCreator && !isDepartmentVigente && !isAssignedReviewer)
                {
                    return RedirectToAction("AccesoDenegado", "Auth");
                }
            }

            // Ordenamos versiones descendentemente
            doc.DocumentoVersions = doc.DocumentoVersions.OrderByDescending(v => v.NumeroVersion).ThenByDescending(v => v.VersionMinor).ToList();

            return View(doc);
        }

        public async Task<IActionResult> Descargar(int versionId)
        {
            var userId = GetCurrentUserId();
            var esAdminOrAuditor = User.IsInRole("Administrador") || User.IsInRole("Superior") || User.IsInRole("Super Administrador") || User.IsInRole("Auditor");
            var empresaId = GetCurrentUserEmpresaId();

            var version = await _context.DocumentoVersions
                .Include(v => v.IdDocumentoNavigation)
                .FirstOrDefaultAsync(v => v.Id == versionId);

            if (version == null || string.IsNullOrEmpty(version.RutaArchivoFisico))
                return NotFound();

            var doc = version.IdDocumentoNavigation;
            if (doc == null || doc.Estatus == false || doc.IdEmpresa != empresaId)
                return NotFound();

            if (!esAdminOrAuditor)
            {
                var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == userId);
                var userDeptoId = user?.IdDepartamento;

                bool isCreator = doc.IdUsuarioCreacion == userId;
                bool isDepartmentVigente = doc.IdDepartamento == userDeptoId && doc.EstadoActual == "Vigente";
                bool isAssignedReviewer = await _context.FlujoAprobacions
                    .AnyAsync(f => f.IdVersionDocumento == version.Id && f.IdUsuarioAsignado == userId && f.EstadoFirma == "Pendiente" && f.Estatus == true);

                if (!isCreator && !isDepartmentVigente && !isAssignedReviewer)
                {
                    return RedirectToAction("AccesoDenegado", "Auth");
                }
            }

            try
            {
                var (stream, fileName, contentType) = await _gridFsService.DescargarArchivoAsync(version.RutaArchivoFisico);
                return File(stream, contentType, fileName);
            }
            catch (Exception)
            {
                // Fallback para datos sembrados o archivos no encontrados en GridFS
                var dummyStream = GetDummyPdfStream(doc.Titulo, doc.CodigoInterno);
                var fallbackName = string.IsNullOrEmpty(doc.CodigoInterno) ? "documento.pdf" : $"{doc.CodigoInterno}.pdf";
                return File(dummyStream, "application/pdf", fallbackName);
            }
        }

        /// <summary>
        /// Sirve el PDF con Content-Disposition: inline para visualizarlo en el navegador
        /// sin forzar la descarga. Tiene el mismo control de acceso que Descargar.
        /// </summary>
        public async Task<IActionResult> VerPrevio(int versionId)
        {
            var userId = GetCurrentUserId();
            var esAdminOrAuditor = User.IsInRole("Administrador") || User.IsInRole("Superior") || User.IsInRole("Super Administrador") || User.IsInRole("Auditor");
            var empresaId = GetCurrentUserEmpresaId();

            var version = await _context.DocumentoVersions
                .Include(v => v.IdDocumentoNavigation)
                .FirstOrDefaultAsync(v => v.Id == versionId);

            if (version == null || string.IsNullOrEmpty(version.RutaArchivoFisico))
                return NotFound();

            var doc = version.IdDocumentoNavigation;
            if (doc == null || doc.Estatus == false || doc.IdEmpresa != empresaId)
                return NotFound();

            if (!esAdminOrAuditor)
            {
                var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == userId);
                var userDeptoId = user?.IdDepartamento;

                bool isCreator = doc.IdUsuarioCreacion == userId;
                bool isDepartmentVigente = doc.IdDepartamento == userDeptoId && doc.EstadoActual == "Vigente";
                bool isAssignedReviewer = await _context.FlujoAprobacions
                    .AnyAsync(f => f.IdVersionDocumento == version.Id && f.IdUsuarioAsignado == userId && f.EstadoFirma == "Pendiente" && f.Estatus == true);

                if (!isCreator && !isDepartmentVigente && !isAssignedReviewer)
                    return RedirectToAction("AccesoDenegado", "Auth");
            }

            try
            {
                var (stream, fileName, contentType) = await _gridFsService.DescargarArchivoAsync(version.RutaArchivoFisico);

                // Inline: el navegador muestra el PDF en lugar de descargarlo
                Response.Headers["Content-Disposition"] = $"inline; filename=\"{fileName}\"";
                return File(stream, contentType);
            }
            catch (Exception)
            {
                // Fallback para datos sembrados o archivos no encontrados en GridFS
                var dummyStream = GetDummyPdfStream(doc.Titulo, doc.CodigoInterno);
                var fallbackName = string.IsNullOrEmpty(doc.CodigoInterno) ? "documento.pdf" : $"{doc.CodigoInterno}.pdf";
                Response.Headers["Content-Disposition"] = $"inline; filename=\"{fallbackName}\"";
                return File(dummyStream, "application/pdf");
            }
        }

        /// <summary>
        /// Endpoint AJAX que registra en sesión que el usuario abrió la previsualización del documento.
        /// Requerido antes de poder firmar/aprobar.
        /// </summary>
        [HttpPost]
        public IActionResult RegistrarVista(int versionId)
        {
            var userId = GetCurrentUserId();
            // Guardar en sesión: "visto_{userId}_{versionId}" = true
            HttpContext.Session.SetString($"doc_visto_{userId}_{versionId}", "1");
            return Ok(new { ok = true });
        }

        /// <summary>
        /// Verifica si el usuario ya visualizó el documento (usado por JS antes de habilitar botones de firma).
        /// </summary>
        [HttpGet]
        public IActionResult VerificaVista(int versionId)
        {
            var userId = GetCurrentUserId();
            var visto = HttpContext.Session.GetString($"doc_visto_{userId}_{versionId}") == "1";
            return Ok(new { visto });
        }

        [AllowAnonymous]
        public async Task<IActionResult> DescargarUltima(int id)
        {
            var doc = await _context.Documentos.FirstOrDefaultAsync(d => d.Id == id && d.Estatus == true);
            if (doc == null || doc.EstadoActual != "Vigente")
                return NotFound("No se encontró el documento o no está en estado Vigente.");

            var version = await _context.DocumentoVersions
                .Where(v => v.IdDocumento == id && v.Estatus == true)
                .OrderByDescending(v => v.NumeroVersion)
                .FirstOrDefaultAsync();

            if (version == null || string.IsNullOrEmpty(version.RutaArchivoFisico))
                return NotFound("No se encontró una versión activa para este documento.");

            try
            {
                var (stream, fileName, contentType) = await _gridFsService.DescargarArchivoAsync(version.RutaArchivoFisico);
                return File(stream, contentType, fileName);
            }
            catch (Exception)
            {
                return NotFound("No se pudo recuperar el archivo desde MongoDB GridFS.");
            }
        }

        private Stream GetDummyPdfStream(string title, string code)
        {
            title = (title ?? "Documento sin titulo").Replace("(", "[").Replace(")", "]");
            code = (code ?? "CODIGO").Replace("(", "[").Replace(")", "]");

            string pdfContent = 
                "%PDF-1.4\n" +
                "1 0 obj\n" +
                "<< /Type /Catalog /Pages 2 0 R >>\n" +
                "endobj\n" +
                "2 0 obj\n" +
                "<< /Type /Pages /Kids [3 0 R] /Count 1 >>\n" +
                "endobj\n" +
                "3 0 obj\n" +
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 << /Type /Font /Subtype /Type1 /BaseFont /Helvetica >> >> >> /Contents 4 0 R >>\n" +
                "endobj\n" +
                "4 0 obj\n" +
                "<< /Length 150 >>\n" +
                "stream\n" +
                "BT\n" +
                "/F1 18 Tf\n" +
                "50 750 Td\n" +
                $"({title}) Tj\n" +
                "0 -30 Td\n" +
                "/F1 12 Tf\n" +
                $"({code}) Tj\n" +
                "0 -40 Td\n" +
                "(Documento de muestra para desarrollo y pruebas.) Tj\n" +
                "0 -20 Td\n" +
                "(Este archivo se genero dinamicamente como fallback.) Tj\n" +
                "ET\n" +
                "endstream\n" +
                "endobj\n" +
                "xref\n" +
                "0 5\n" +
                "0000000000 65535 f \n" +
                "0000000009 00000 n \n" +
                "0000000058 00000 n \n" +
                "0000000115 00000 n \n" +
                "0000000242 00000 n \n" +
                "trailer\n" +
                "<< /Size 5 /Root 1 0 R >>\n" +
                "startxref\n" +
                "450\n" +
                "%%EOF";

            var bytes = System.Text.Encoding.UTF8.GetBytes(pdfContent);
            return new MemoryStream(bytes);
        }
    }
}
