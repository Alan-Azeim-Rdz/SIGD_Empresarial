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

        public DocumentoController(DirContext context, IMongoGridFsService gridFsService)
        {
            _context = context;
            _gridFsService = gridFsService;
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

        public async Task<IActionResult> Index()
        {
            var empresaId = GetCurrentUserEmpresaId();
            var documentos = await _context.Documentos
                .Include(d => d.IdDepartamentoNavigation)
                .Include(d => d.IdTipoDocumentoNavigation)
                .Include(d => d.DocumentoVersions)
                    .ThenInclude(v => v.FlujoAprobacions)
                        .ThenInclude(f => f.IdUsuarioAsignadoNavigation)
                .Where(d => d.Estatus == true && d.IdEmpresa == empresaId)
                .OrderByDescending(d => d.FechaCreacion)
                .ToListAsync();

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

                // 3. Crear la Versión Inicial (V1)
                var version = new DocumentoVersion
                {
                    IdDocumento = doc.Id,
                    NumeroVersion = 1,
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

            if (doc == null || (doc.EstadoActual != "Borrador" && doc.EstadoActual != "Rechazado"))
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

            if (doc == null || (doc.EstadoActual != "Borrador" && doc.EstadoActual != "Rechazado"))
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
                int nuevaVersionNum = 1;
                int nuevoMinor = 0;
                
                if (doc.DocumentoVersions.Any())
                {
                    var ultimaVersion = doc.DocumentoVersions.OrderByDescending(v => v.NumeroVersion).ThenByDescending(v => v.VersionMinor).First();
                    if (doc.EstadoActual == "Rechazado")
                    {
                        nuevaVersionNum = ultimaVersion.NumeroVersion;
                        nuevoMinor = ultimaVersion.VersionMinor + 1;
                    }
                    else
                    {
                        nuevaVersionNum = ultimaVersion.NumeroVersion + 1;
                        nuevoMinor = 0;
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

                doc.FechaModificacion = DateTime.Now;
                doc.IdUsuarioModificacion = userId;
                
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Detalle), new { id = doc.Id });
            }

            return View(doc);
        }

        [HttpGet]
        public async Task<IActionResult> Historial(int id)
        {
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

            // Ordenamos versiones descendentemente
            doc.DocumentoVersions = doc.DocumentoVersions.OrderByDescending(v => v.NumeroVersion).ToList();

            return View(doc);
        }

        public async Task<IActionResult> Detalle(int id)
        {
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

            // Ordenamos versiones descendentemente
            doc.DocumentoVersions = doc.DocumentoVersions.OrderByDescending(v => v.NumeroVersion).ToList();

            return View(doc);
        }

        public async Task<IActionResult> Descargar(int versionId)
        {
            var empresaId = GetCurrentUserEmpresaId();

            var version = await _context.DocumentoVersions
                .Include(v => v.IdDocumentoNavigation)
                .FirstOrDefaultAsync(v => v.Id == versionId && v.IdDocumentoNavigation.IdEmpresa == empresaId);

            if (version == null || string.IsNullOrEmpty(version.RutaArchivoFisico))
                return NotFound();

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

        [AllowAnonymous]
        public async Task<IActionResult> DescargarUltima(int id)
        {
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
    }
}
