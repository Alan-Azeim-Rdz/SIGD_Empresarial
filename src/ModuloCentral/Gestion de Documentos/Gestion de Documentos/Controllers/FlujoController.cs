using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Gestion_de_Documentos.Models;
using System.Security.Claims;
using Gestion_de_Documentos.Services;

namespace Gestion_de_Documentos.Controllers
{
    [Authorize]
    public class FlujoController : Controller
    {
        private readonly DirContext _context;
        private readonly ReportesIntegrationService _reportesService;
        private readonly BusquedaIntegrationService _busquedaService;

        public FlujoController(DirContext context, ReportesIntegrationService reportesService, BusquedaIntegrationService busquedaService)
        {
            _context = context;
            _reportesService = reportesService;
            _busquedaService = busquedaService;
        }

        public class RevisorDto
        {
            public int Id { get; set; }
            public string Nombre { get; set; }
            public string ApellidoP { get; set; }
            public string ApellidoM { get; set; }
            public string Rol { get; set; }
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        private int GetCurrentUserEmpresaId()
        {
            var claim = User.FindFirst("IdEmpresa")?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }

        [HttpGet]
        public async Task<IActionResult> EnviarARevision(int idDocumento)
        {
            var doc = await _context.Documentos
                .FirstOrDefaultAsync(d => d.Id == idDocumento && d.IdUsuarioCreacion == GetCurrentUserId());

            if (doc == null || doc.EstadoActual != "Borrador")
                return NotFound("Documento no válido o no se encuentra en estado Borrador.");

            var empresaId = doc.IdEmpresa ?? GetCurrentUserEmpresaId();

            var revisores = await _context.UsuarioRols
                .Include(ur => ur.IdUsuarioNavigation)
                .Include(ur => ur.IdRolNavigation)
                .Where(ur => ur.Estatus != false 
                          && ur.IdUsuarioNavigation.Estatus != false
                          && (empresaId == 0 
                              ? ur.IdUsuarioNavigation.IdEmpresa == null 
                              : ur.IdUsuarioNavigation.IdEmpresa == empresaId)
                          && (
                              ur.IdRolNavigation.Nombre == "Administrador"
                              || (ur.IdRolNavigation.Nombre == "Superior" && ur.IdUsuarioNavigation.IdDepartamento == doc.IdDepartamento)
                             ))
                .Select(ur => new RevisorDto
                {
                    Id = ur.IdUsuarioNavigation.Id,
                    Nombre = ur.IdUsuarioNavigation.Nombre,
                    ApellidoP = ur.IdUsuarioNavigation.ApellidoP,
                    ApellidoM = ur.IdUsuarioNavigation.ApellidoM,
                    Rol = ur.IdRolNavigation.Nombre
                })
                .ToListAsync();

            ViewBag.Revisores = revisores;
            return View(doc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarARevision(int idDocumento, int idRevisor)
        {
            var doc = await _context.Documentos
                .Include(d => d.DocumentoVersions)
                .FirstOrDefaultAsync(d => d.Id == idDocumento && d.IdUsuarioCreacion == GetCurrentUserId());

            if (doc == null || doc.EstadoActual != "Borrador")
                return NotFound("Documento no válido o no se encuentra en estado Borrador.");

            var versionActual = doc.DocumentoVersions.OrderByDescending(v => v.NumeroVersion).FirstOrDefault();
            if (versionActual == null)
                return BadRequest("El documento no tiene versiones válidas.");

            var empresaId = doc.IdEmpresa ?? GetCurrentUserEmpresaId();

            // Validar que el revisor elegido exista y tenga rol válido (Administrador de la misma empresa o Superior del mismo departamento/empresa)
            var revisorValido = await _context.UsuarioRols
                .Include(ur => ur.IdUsuarioNavigation)
                .Include(ur => ur.IdRolNavigation)
                .AnyAsync(ur => ur.IdUsuario == idRevisor 
                             && ur.Estatus != false 
                             && ur.IdUsuarioNavigation.Estatus != false
                             && (empresaId == 0 
                                 ? ur.IdUsuarioNavigation.IdEmpresa == null 
                                 : ur.IdUsuarioNavigation.IdEmpresa == empresaId)
                             && (
                                 ur.IdRolNavigation.Nombre == "Administrador"
                                 || (ur.IdRolNavigation.Nombre == "Superior" && ur.IdUsuarioNavigation.IdDepartamento == doc.IdDepartamento)
                                ));

            if (!revisorValido) return BadRequest("El usuario seleccionado no es un revisor válido.");

            // Crear el registro de Flujo
            var flujo = new FlujoAprobacion
            {
                IdVersionDocumento = versionActual.Id,
                IdUsuarioAsignado = idRevisor,
                TipoAccion = "Revisión",
                EstadoFirma = "Pendiente",
                Orden = 1,
                IdUsuarioCreacion = GetCurrentUserId(),
                FechaCreacion = DateTime.Now,
                Estatus = true,
                IpOrigenRemitente = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            doc.EstadoActual = "En Revision";

            _context.FlujoAprobacions.Add(flujo);
            await _context.SaveChangesAsync();

            return RedirectToAction("Detalle", "Documento", new { id = doc.Id });
        }

        [Authorize(Roles = "Administrador, Superior")]
        public async Task<IActionResult> Pendientes()
        {
            var userId = GetCurrentUserId();

            var flujosPendientes = await _context.FlujoAprobacions
                .Include(f => f.IdVersionDocumentoNavigation)
                    .ThenInclude(v => v.IdDocumentoNavigation)
                .Where(f => f.IdUsuarioAsignado == userId && f.EstadoFirma == "Pendiente" && f.Estatus == true)
                .OrderBy(f => f.FechaCreacion)
                .ToListAsync();

            return View(flujosPendientes);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador, Superior")]
        public async Task<IActionResult> Responder(int idFlujo, string respuesta, string comentarios)
        {
            var flujo = await _context.FlujoAprobacions
                .Include(f => f.IdVersionDocumentoNavigation)
                    .ThenInclude(v => v.IdDocumentoNavigation)
                .FirstOrDefaultAsync(f => f.Id == idFlujo);

            if (flujo == null || flujo.EstadoFirma != "Pendiente")
                return NotFound("Flujo no válido.");

            var userId = GetCurrentUserId();

            if (flujo.IdUsuarioAsignado != userId)
                return RedirectToAction("AccesoDenegado", "Auth");

            if (string.IsNullOrWhiteSpace(comentarios))
            {
                return BadRequest("Los comentarios son obligatorios para aprobar o rechazar el documento.");
            }

            // Validar que haya previsualizado el archivo antes de firmar/responder
            var visto = HttpContext.Session.GetString($"doc_visto_{userId}_{flujo.IdVersionDocumento}") == "1";
            if (!visto)
            {
                return BadRequest("Debe previsualizar el documento completo en el visor antes de poder tomar una decisión.");
            }

            // Modificar el registro de flujo actual en lugar de crear un detalle
            flujo.EstadoFirma = respuesta == "Aprobar" ? "Firmado" : "Rechazado";
            flujo.Comentarios = comentarios;
            flujo.FechaFirma = DateTime.Now;
            flujo.IdUsuarioModificacion = userId;
            flujo.FechaModificacion = DateTime.Now;
            flujo.IpOrigenFirmante = HttpContext.Connection.RemoteIpAddress?.ToString();

            if (respuesta == "Aprobar")
            {
                var idDoc = flujo.IdVersionDocumentoNavigation.IdDocumento;

                // Calcular el siguiente NumeroVersion mayor
                var maxAprobada = await _context.DocumentoVersions
                    .Where(v => v.IdDocumento == idDoc && v.VersionMinor == 0 && v.Id != flujo.IdVersionDocumento)
                    .Select(v => (int?)v.NumeroVersion)
                    .MaxAsync() ?? 0;

                var siguienteMajor = maxAprobada + 1;

                flujo.IdVersionDocumentoNavigation.NumeroVersion = siguienteMajor;
                flujo.IdVersionDocumentoNavigation.VersionMinor = 0;

                // El documento pasa a Vigente
                flujo.IdVersionDocumentoNavigation.IdDocumentoNavigation.EstadoActual = "Vigente";
                await _context.SaveChangesAsync();
                
                // Sincronizar con Módulo de Reportes (PostgreSQL/PHP)
                await _reportesService.SincronizarDocumentoAsync(idDoc, userId);

                // Obtener IP del firmante
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

                // Sincronizar con Módulo de Búsqueda (MongoDB/Node.js)
                await _busquedaService.SincronizarDocumentoAsync(idDoc, userId, ip);
            }
            else
            {
                // El documento vuelve a estado Rechazado
                flujo.IdVersionDocumentoNavigation.IdDocumentoNavigation.EstadoActual = "Rechazado";
                await _context.SaveChangesAsync();

                // Desindexar de la búsqueda ya que no está Vigente
                var idDoc = flujo.IdVersionDocumentoNavigation.IdDocumento;
                await _busquedaService.DesindexarDocumentoAsync(idDoc);
            }

            return RedirectToAction(nameof(Pendientes));
        }

        [HttpPost]
        [Authorize(Roles = "Administrador, Superior")]
        public async Task<IActionResult> DeshacerRespuesta(int idFlujo)
        {
            var flujo = await _context.FlujoAprobacions
                .Include(f => f.IdVersionDocumentoNavigation)
                    .ThenInclude(v => v.IdDocumentoNavigation)
                .FirstOrDefaultAsync(f => f.Id == idFlujo);

            if (flujo == null || (flujo.EstadoFirma != "Firmado" && flujo.EstadoFirma != "Rechazado"))
                return NotFound("Flujo no válido o no se puede deshacer.");

            var userId = GetCurrentUserId();

            if (flujo.IdUsuarioAsignado != userId)
                return RedirectToAction("AccesoDenegado", "Auth");

            // Revertir a Pendiente
            flujo.EstadoFirma = "Pendiente";
            flujo.Comentarios = null;
            flujo.FechaFirma = null;
            flujo.IpOrigenFirmante = null;
            flujo.IdUsuarioModificacion = userId;
            flujo.FechaModificacion = DateTime.Now;

            // El documento vuelve a En Revision
            flujo.IdVersionDocumentoNavigation.IdDocumentoNavigation.EstadoActual = "En Revision";
            await _context.SaveChangesAsync();

            return RedirectToAction("Detalle", "Documento", new { id = flujo.IdVersionDocumentoNavigation.IdDocumento });
        }
    }
}
