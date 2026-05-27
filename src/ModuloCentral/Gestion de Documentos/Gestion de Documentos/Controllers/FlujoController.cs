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

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        [HttpGet]
        public async Task<IActionResult> EnviarARevision(int idDocumento)
        {
            var doc = await _context.Documentos
                .FirstOrDefaultAsync(d => d.Id == idDocumento && d.IdUsuarioCreacion == GetCurrentUserId());

            if (doc == null || doc.EstadoActual != "Borrador")
                return NotFound("Documento no válido o no se encuentra en estado Borrador.");

            // Obtener lista de usuarios que son 'Administrador' o 'Superior'
            var revisores = await _context.UsuarioRols
                .Include(ur => ur.IdUsuarioNavigation)
                .Include(ur => ur.IdRolNavigation)
                .Where(ur => (ur.IdRolNavigation.Nombre == "Administrador" || ur.IdRolNavigation.Nombre == "Superior") && ur.Estatus == true && ur.IdUsuarioNavigation.Estatus == true)
                .Select(ur => new { ur.IdUsuarioNavigation.Id, ur.IdUsuarioNavigation.Nombre, ApellidoP = ur.IdUsuarioNavigation.ApellidoP, ApellidoM = ur.IdUsuarioNavigation.ApellidoM, Rol = ur.IdRolNavigation.Nombre })
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

            // Validar que el revisor elegido exista y tenga rol válido
            var revisorValido = await _context.UsuarioRols
                .Include(ur => ur.IdRolNavigation)
                .AnyAsync(ur => ur.IdUsuario == idRevisor && (ur.IdRolNavigation.Nombre == "Administrador" || ur.IdRolNavigation.Nombre == "Superior") && ur.Estatus == true);

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
                Estatus = true
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
                .Include(f => f.IdUsuarioCreacionNavigation)
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

            // Modificar el registro de flujo actual en lugar de crear un detalle
            flujo.EstadoFirma = respuesta == "Aprobar" ? "Firmado" : "Rechazado";
            flujo.Comentarios = comentarios;
            flujo.FechaFirma = DateTime.Now;
            flujo.IdUsuarioModificacion = userId;
            flujo.FechaModificacion = DateTime.Now;

            if (respuesta == "Aprobar")
            {
                // El documento pasa a Vigente
                flujo.IdVersionDocumentoNavigation.IdDocumentoNavigation.EstadoActual = "Vigente";
                await _context.SaveChangesAsync();
                
                var idDoc = flujo.IdVersionDocumentoNavigation.IdDocumento;

                // Sincronizar con Módulo de Reportes (PostgreSQL/PHP)
                await _reportesService.SincronizarDocumentoAsync(idDoc, userId);

                // Sincronizar con Módulo de Búsqueda (MongoDB/Node.js)
                await _busquedaService.SincronizarDocumentoAsync(idDoc, userId);
            }
            else
            {
                // El documento vuelve a borrador para ser corregido
                flujo.IdVersionDocumentoNavigation.IdDocumentoNavigation.EstadoActual = "Borrador";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Pendientes));
        }
    }
}
