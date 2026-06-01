using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Gestion_de_Documentos.Models;
using Gestion_de_Documentos.Services;
using System.Security.Cryptography;
<<<<<<< HEAD
using System.Text.Json;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
=======
>>>>>>> development

namespace Gestion_de_Documentos.Controllers
{
    [Authorize]
    public class DocumentoController : Controller
    {
        private readonly DirContext _context;
        private readonly IMongoGridFsService _gridFsService;
<<<<<<< HEAD

        public DocumentoController(DirContext context, IMongoGridFsService gridFsService)
        {
            _context = context;
            _gridFsService = gridFsService;
=======
        private readonly BusquedaIntegrationService _busquedaService;

        public DocumentoController(DirContext context, IMongoGridFsService gridFsService, BusquedaIntegrationService busquedaService)
        {
            _context = context;
            _gridFsService = gridFsService;
            _busquedaService = busquedaService;
>>>>>>> development
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

<<<<<<< HEAD
        public async Task<IActionResult> Index()
        {
            var empresaId = GetCurrentUserEmpresaId();
            var documentos = await _context.Documentos
=======
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
>>>>>>> development
                .Include(d => d.IdDepartamentoNavigation)
                .Include(d => d.IdTipoDocumentoNavigation)
                .Include(d => d.DocumentoVersions)
                    .ThenInclude(v => v.FlujoAprobacions)
                        .ThenInclude(f => f.IdUsuarioAsignadoNavigation)
<<<<<<< HEAD
                .Where(d => d.Estatus == true && d.IdEmpresa == empresaId)
                .OrderByDescending(d => d.FechaCreacion)
                .ToListAsync();

=======
                .OrderByDescending(d => d.FechaCreacion)
                .Skip((pagina - 1) * porPagina)
                .Take(porPagina)
                .ToListAsync();

            ViewBag.PaginaActual  = pagina;
            ViewBag.TotalPaginas  = (int)Math.Ceiling(total / (double)porPagina);
            ViewBag.TotalDocs     = total;

>>>>>>> development
            return View(documentos);
        }

        public async Task<IActionResult> Crear()
        {
<<<<<<< HEAD
            var empresaId = GetCurrentUserEmpresaId();
            ViewBag.TiposDocumento = await _context.TipoDocumentos.Where(t => t.Estatus == true && t.IdEmpresa == empresaId).ToListAsync();
            ViewBag.Departamentos = await _context.Departamentos.Where(d => d.Estatus == true && d.IdEmpresa == empresaId).ToListAsync();
            
            var empresa = await _context.Empresas.FindAsync(empresaId);
            ViewBag.CamposPersonalizados = empresa?.CamposPersonalizados;

=======
            ViewBag.TiposDocumento = await _context.TipoDocumentos.Where(t => t.Estatus == true).ToListAsync();
            ViewBag.Departamentos = await _context.Departamentos.Where(d => d.Estatus == true).ToListAsync();
>>>>>>> development
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Documento doc, IFormFile archivoPdf)
        {
<<<<<<< HEAD
            var userId = GetCurrentUserId();
            var empresaId = GetCurrentUserEmpresaId();

=======
>>>>>>> development
            if (archivoPdf == null || archivoPdf.Length == 0)
            {
                ModelState.AddModelError("", "Debe seleccionar un archivo PDF.");
            }
            else if (archivoPdf.ContentType != "application/pdf" && !archivoPdf.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Solo se permiten archivos en formato PDF.");
            }

<<<<<<< HEAD
            // Validar y guardar campos personalizados
            var empresa = await _context.Empresas.FindAsync(empresaId);
            var definicionCampos = empresa?.CamposPersonalizados;
            if (!string.IsNullOrEmpty(definicionCampos))
            {
                try
                {
                    using var jsonDoc = JsonDocument.Parse(definicionCampos);
                    var dictValores = new Dictionary<string, string>();
                    
                    foreach (var campo in jsonDoc.RootElement.EnumerateArray())
                    {
                        var nombre = campo.GetProperty("Nombre").GetString() ?? "";
                        var requerido = campo.GetProperty("Requerido").GetBoolean();
                        var valor = Request.Form["CP_" + nombre].ToString() ?? "";
                        
                        if (requerido && string.IsNullOrWhiteSpace(valor))
                        {
                            ModelState.AddModelError("", $"El campo personalizado '{nombre}' es requerido.");
                        }
                        
                        dictValores[nombre] = valor;
                    }
                    
                    doc.CamposPersonalizadosValores = JsonSerializer.Serialize(dictValores);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al procesar campos personalizados: " + ex.Message);
                }
            }

            // Remover validaciones innecesarias del ModelState (Navigation properties)
            ModelState.Remove("IdDepartamentoNavigation");
            ModelState.Remove("IdTipoDocumentoNavigation");
            ModelState.Remove("IdUsuarioCreacionNavigation");
            ModelState.Remove("IdUsuarioModificacionNavigation");
            ModelState.Remove("IdUsuarioEliminacionNavigation");
=======
            ModelState.Remove("IdUsuarioPropietario");
            ModelState.Remove("EstadoActual");
            ModelState.Remove("IdDepartamentoNavigation");
            ModelState.Remove("IdTipoDocumentoNavigation");
            ModelState.Remove("IdUsuarioCreacionNavigation");
>>>>>>> development
            ModelState.Remove("IdUsuarioPropietarioNavigation");
            ModelState.Remove("IdEmpresaNavigation");
            ModelState.Remove("EstadoActual");
            ModelState.Remove("BitacoraControlDocumentos");
            ModelState.Remove("BitacoraTransaccionals");
            ModelState.Remove("DocumentoVersions");

            if (ModelState.IsValid)
            {
<<<<<<< HEAD
=======
                var userId = GetCurrentUserId();
                var empresaId = GetCurrentUserEmpresaId();
                
>>>>>>> development
                // 1. Guardar el archivo en MongoDB GridFS
                using var stream = archivoPdf.OpenReadStream();
                var objectIdStr = await _gridFsService.SubirArchivoAsync(stream, archivoPdf.FileName, archivoPdf.ContentType);

                // Calcular el hash (SHA256) del archivo físico subido
                stream.Position = 0;
                using var sha256 = SHA256.Create();
                var hashBytes = sha256.ComputeHash(stream);
                var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToUpperInvariant();

                // 2. Crear el registro base de Documento
<<<<<<< HEAD
                doc.IdEmpresa = empresaId;
=======
>>>>>>> development
                doc.EstadoActual = "Borrador";
                doc.Estatus = true;
                doc.FechaCreacion = DateTime.Now;
                doc.IdUsuarioCreacion = userId;
                doc.IdUsuarioPropietario = userId;
<<<<<<< HEAD
                
                _context.Documentos.Add(doc);
                await _context.SaveChangesAsync(); // Para obtener el doc.Id

                // 3. Crear la Versión Inicial (V1)
                var version = new DocumentoVersion
                {
                    IdDocumento = doc.Id,
                    NumeroVersion = 1,
=======
                doc.IdEmpresa = empresaId;
                _context.Documentos.Add(doc);
                await _context.SaveChangesAsync(); // Para obtener el doc.Id

                // 3. Crear la Versión Inicial (V0.1)
                var version = new DocumentoVersion
                {
                    IdDocumento = doc.Id,
                    NumeroVersion = 0,
                    VersionMinor = 1,
>>>>>>> development
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

<<<<<<< HEAD
            ViewBag.TiposDocumento = await _context.TipoDocumentos.Where(t => t.Estatus == true && t.IdEmpresa == empresaId).ToListAsync();
            ViewBag.Departamentos = await _context.Departamentos.Where(d => d.Estatus == true && d.IdEmpresa == empresaId).ToListAsync();
            ViewBag.CamposPersonalizados = definicionCampos;
=======
            ViewBag.TiposDocumento = await _context.TipoDocumentos.Where(t => t.Estatus == true).ToListAsync();
            ViewBag.Departamentos = await _context.Departamentos.Where(d => d.Estatus == true).ToListAsync();
>>>>>>> development
            return View(doc);
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
<<<<<<< HEAD
            var empresaId = GetCurrentUserEmpresaId();
            var doc = await _context.Documentos.FirstOrDefaultAsync(d => d.Id == id && d.IdEmpresa == empresaId && d.Estatus == true);
=======
            var userId = GetCurrentUserId();
            var doc = await _context.Documentos.FirstOrDefaultAsync(d => d.Id == id && d.IdUsuarioCreacion == userId && d.Estatus == true);
>>>>>>> development

            if (doc == null || doc.EstadoActual != "Borrador")
                return NotFound("Documento no válido o no se encuentra en estado Borrador.");

<<<<<<< HEAD
            ViewBag.TiposDocumento = await _context.TipoDocumentos.Where(t => t.Estatus == true && t.IdEmpresa == empresaId).ToListAsync();
            ViewBag.Departamentos = await _context.Departamentos.Where(d => d.Estatus == true && d.IdEmpresa == empresaId).ToListAsync();
            
            var empresa = await _context.Empresas.FindAsync(empresaId);
            ViewBag.CamposPersonalizados = empresa?.CamposPersonalizados;

=======
            ViewBag.TiposDocumento = await _context.TipoDocumentos.Where(t => t.Estatus == true).ToListAsync();
            ViewBag.Departamentos = await _context.Departamentos.Where(d => d.Estatus == true).ToListAsync();
>>>>>>> development
            return View(doc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Documento model)
        {
            var userId = GetCurrentUserId();
<<<<<<< HEAD
            var empresaId = GetCurrentUserEmpresaId();
            var doc = await _context.Documentos.FirstOrDefaultAsync(d => d.Id == id && d.IdEmpresa == empresaId && d.Estatus == true);
=======
            var doc = await _context.Documentos.FirstOrDefaultAsync(d => d.Id == id && d.IdUsuarioCreacion == userId && d.Estatus == true);
>>>>>>> development

            if (doc == null || doc.EstadoActual != "Borrador")
                return NotFound("Documento no válido o no se encuentra en estado Borrador.");

<<<<<<< HEAD
            // Validar y guardar campos personalizados
            var empresa = await _context.Empresas.FindAsync(empresaId);
            var definicionCampos = empresa?.CamposPersonalizados;
            if (!string.IsNullOrEmpty(definicionCampos))
            {
                try
                {
                    using var jsonDoc = JsonDocument.Parse(definicionCampos);
                    var dictValores = new Dictionary<string, string>();
                    
                    foreach (var campo in jsonDoc.RootElement.EnumerateArray())
                    {
                        var nombre = campo.GetProperty("Nombre").GetString() ?? "";
                        var requerido = campo.GetProperty("Requerido").GetBoolean();
                        var valor = Request.Form["CP_" + nombre].ToString() ?? "";
                        
                        if (requerido && string.IsNullOrWhiteSpace(valor))
                        {
                            ModelState.AddModelError("", $"El campo personalizado '{nombre}' es requerido.");
                        }
                        
                        dictValores[nombre] = valor;
                    }
                    
                    doc.CamposPersonalizadosValores = JsonSerializer.Serialize(dictValores);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al procesar campos personalizados: " + ex.Message);
                }
            }

            // Remover validaciones innecesarias del ModelState (Navigation properties)
            ModelState.Remove("IdDepartamentoNavigation");
            ModelState.Remove("IdTipoDocumentoNavigation");
            ModelState.Remove("IdUsuarioCreacionNavigation");
            ModelState.Remove("IdUsuarioModificacionNavigation");
            ModelState.Remove("IdUsuarioEliminacionNavigation");
            ModelState.Remove("IdUsuarioPropietarioNavigation");
            ModelState.Remove("IdEmpresaNavigation");
            ModelState.Remove("EstadoActual");
=======
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
>>>>>>> development
            ModelState.Remove("BitacoraControlDocumentos");
            ModelState.Remove("BitacoraTransaccionals");
            ModelState.Remove("DocumentoVersions");

            if (ModelState.IsValid)
            {
                doc.CodigoInterno = model.CodigoInterno;
                doc.Titulo = model.Titulo;
                doc.IdTipoDocumento = model.IdTipoDocumento;
                doc.IdDepartamento = model.IdDepartamento;
<<<<<<< HEAD
                doc.CamposPersonalizadosValores = doc.CamposPersonalizadosValores; // Mantenemos el valor actualizado
=======
>>>>>>> development
                
                doc.FechaModificacion = DateTime.Now;
                doc.IdUsuarioModificacion = userId;

                _context.Update(doc);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Detalle), new { id = doc.Id });
            }

<<<<<<< HEAD
            ViewBag.TiposDocumento = await _context.TipoDocumentos.Where(t => t.Estatus == true && t.IdEmpresa == empresaId).ToListAsync();
            ViewBag.Departamentos = await _context.Departamentos.Where(d => d.Estatus == true && d.IdEmpresa == empresaId).ToListAsync();
            ViewBag.CamposPersonalizados = definicionCampos;
=======
            ViewBag.TiposDocumento = await _context.TipoDocumentos.Where(t => t.Estatus == true).ToListAsync();
            ViewBag.Departamentos = await _context.Departamentos.Where(d => d.Estatus == true).ToListAsync();
>>>>>>> development
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> SubirNuevaVersion(int id)
        {
<<<<<<< HEAD
            var empresaId = GetCurrentUserEmpresaId();
            var doc = await _context.Documentos
                .Include(d => d.DocumentoVersions)
                .FirstOrDefaultAsync(d => d.Id == id && d.IdEmpresa == empresaId && d.Estatus == true);

            if (doc == null || (doc.EstadoActual != "Borrador" && doc.EstadoActual != "Rechazado"))
=======
            var userId = GetCurrentUserId();
            var doc = await _context.Documentos
                .Include(d => d.DocumentoVersions)
                .FirstOrDefaultAsync(d => d.Id == id && d.IdUsuarioCreacion == userId && d.Estatus == true);

            if (doc == null || (doc.EstadoActual != "Borrador" && doc.EstadoActual != "Rechazado" && doc.EstadoActual != "Vigente"))
>>>>>>> development
                return NotFound("Documento no válido o no se puede modificar en su estado actual.");

            return View(doc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
<<<<<<< HEAD
        public async Task<IActionResult> SubirNuevaVersion(int id, IFormFile archivoPdf)
        {
            var userId = GetCurrentUserId();
            var empresaId = GetCurrentUserEmpresaId();
            var doc = await _context.Documentos
                .Include(d => d.DocumentoVersions)
                .FirstOrDefaultAsync(d => d.Id == id && d.IdEmpresa == empresaId && d.Estatus == true);

            if (doc == null || (doc.EstadoActual != "Borrador" && doc.EstadoActual != "Rechazado"))
=======
        public async Task<IActionResult> SubirNuevaVersion(int id, IFormFile archivoPdf, string? motivoCambio)
        {
            var userId = GetCurrentUserId();
            var doc = await _context.Documentos
                .Include(d => d.DocumentoVersions)
                .FirstOrDefaultAsync(d => d.Id == id && d.IdUsuarioCreacion == userId && d.Estatus == true);

            if (doc == null || (doc.EstadoActual != "Borrador" && doc.EstadoActual != "Rechazado" && doc.EstadoActual != "Vigente"))
>>>>>>> development
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
<<<<<<< HEAD
                int nuevaVersionNum = 1;
                int nuevoMinor = 0;
=======
                int nuevaVersionNum = 0;
                int nuevoMinor = 1;
>>>>>>> development
                
                if (doc.DocumentoVersions.Any())
                {
                    var ultimaVersion = doc.DocumentoVersions.OrderByDescending(v => v.NumeroVersion).ThenByDescending(v => v.VersionMinor).First();
<<<<<<< HEAD
                    if (doc.EstadoActual == "Rechazado")
                    {
                        nuevaVersionNum = ultimaVersion.NumeroVersion;
                        nuevoMinor = ultimaVersion.VersionMinor + 1;
                    }
                    else
                    {
                        nuevaVersionNum = ultimaVersion.NumeroVersion + 1;
                        nuevoMinor = 0;
=======
                    nuevaVersionNum = ultimaVersion.NumeroVersion;
                    if (doc.EstadoActual == "Vigente")
                    {
                        nuevoMinor = 1;
                    }
                    else
                    {
                        nuevoMinor = ultimaVersion.VersionMinor + 1;
>>>>>>> development
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
<<<<<<< HEAD
=======
                    MotivoCambio = motivoCambio,
>>>>>>> development
                    FechaCreacion = DateTime.Now
                };

                _context.DocumentoVersions.Add(nuevaVersion);
<<<<<<< HEAD
                
=======

                bool wasVigenteOrRechazado = doc.EstadoActual == "Vigente" || doc.EstadoActual == "Rechazado";
                if (wasVigenteOrRechazado)
                {
                    doc.EstadoActual = "Borrador";
                }

>>>>>>> development
                doc.FechaModificacion = DateTime.Now;
                doc.IdUsuarioModificacion = userId;
                
                await _context.SaveChangesAsync();

<<<<<<< HEAD
=======
                if (wasVigenteOrRechazado)
                {
                    // Desindexar de la búsqueda ya que pasa a Borrador (no vigente)
                    await _busquedaService.DesindexarDocumentoAsync(doc.Id);
                }

>>>>>>> development
                return RedirectToAction(nameof(Detalle), new { id = doc.Id });
            }

            return View(doc);
        }

        [HttpGet]
        public async Task<IActionResult> Historial(int id)
        {
<<<<<<< HEAD
            var empresaId = GetCurrentUserEmpresaId();
=======
            var userId = GetCurrentUserId();
            var esAdminOrAuditor = User.IsInRole("Administrador") || User.IsInRole("Superior") || User.IsInRole("Super Administrador") || User.IsInRole("Auditor");
            var empresaId = GetCurrentUserEmpresaId();

>>>>>>> development
            var doc = await _context.Documentos
                .Include(d => d.IdDepartamentoNavigation)
                .Include(d => d.IdTipoDocumentoNavigation)
                .Include(d => d.DocumentoVersions)
                    .ThenInclude(v => v.FlujoAprobacions)
                        .ThenInclude(f => f.IdUsuarioAsignadoNavigation)
                .FirstOrDefaultAsync(d => d.Id == id && d.IdEmpresa == empresaId && d.Estatus == true);

            if (doc == null)
                return NotFound("Documento no válido.");

<<<<<<< HEAD
            // Ordenamos versiones descendentemente
            doc.DocumentoVersions = doc.DocumentoVersions.OrderByDescending(v => v.NumeroVersion).ToList();
=======
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
>>>>>>> development

            return View(doc);
        }

        public async Task<IActionResult> Detalle(int id)
        {
<<<<<<< HEAD
            var empresaId = GetCurrentUserEmpresaId();
=======
            var userId = GetCurrentUserId();
            var esAdminOrAuditor = User.IsInRole("Administrador") || User.IsInRole("Superior") || User.IsInRole("Super Administrador") || User.IsInRole("Auditor");
            var empresaId = GetCurrentUserEmpresaId();

>>>>>>> development
            var doc = await _context.Documentos
                .Include(d => d.IdDepartamentoNavigation)
                .Include(d => d.IdTipoDocumentoNavigation)
                .Include(d => d.DocumentoVersions)
                    .ThenInclude(v => v.FlujoAprobacions)
                        .ThenInclude(f => f.IdUsuarioAsignadoNavigation)
                .FirstOrDefaultAsync(d => d.Id == id && d.IdEmpresa == empresaId && d.Estatus == true);

            if (doc == null)
                return NotFound();

<<<<<<< HEAD
            // Ordenamos versiones descendentemente
            doc.DocumentoVersions = doc.DocumentoVersions.OrderByDescending(v => v.NumeroVersion).ToList();
=======
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
>>>>>>> development

            return View(doc);
        }

        public async Task<IActionResult> Descargar(int versionId)
        {
<<<<<<< HEAD
            var empresaId = GetCurrentUserEmpresaId();
            var version = await _context.DocumentoVersions
                .Include(v => v.IdDocumentoNavigation)
                .FirstOrDefaultAsync(v => v.Id == versionId && v.IdDocumentoNavigation.IdEmpresa == empresaId);
=======
            var userId = GetCurrentUserId();
            var esAdminOrAuditor = User.IsInRole("Administrador") || User.IsInRole("Superior") || User.IsInRole("Super Administrador") || User.IsInRole("Auditor");
            var empresaId = GetCurrentUserEmpresaId();

            var version = await _context.DocumentoVersions
                .Include(v => v.IdDocumentoNavigation)
                .FirstOrDefaultAsync(v => v.Id == versionId);
>>>>>>> development

            if (version == null || string.IsNullOrEmpty(version.RutaArchivoFisico))
                return NotFound();

<<<<<<< HEAD
=======
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

>>>>>>> development
            try
            {
                var (stream, fileName, contentType) = await _gridFsService.DescargarArchivoAsync(version.RutaArchivoFisico);
                return File(stream, contentType, fileName);
            }
            catch (Exception)
            {
<<<<<<< HEAD
                return NotFound("No se pudo recuperar el archivo desde MongoDB GridFS.");
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> DescargarUltima(int id)
        {
            // Nota: Al descargar de forma anónima (por el portal público sync),
            // requerimos verificar que la descarga ocurra para la empresa que corresponda.
            // Para mantener compatibilidad en descargas públicas directas, buscamos la versión
            // sin obligar claims si no está logueado, pero validamos que esté activo.
            var version = await _context.DocumentoVersions
                .Include(v => v.IdDocumentoNavigation)
=======
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
>>>>>>> development
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
<<<<<<< HEAD
=======

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
>>>>>>> development
    }
}
