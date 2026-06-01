using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Gestion_de_Documentos.Models;

namespace Gestion_de_Documentos.Controllers
{
    [Authorize]
    public class BusquedaController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<BusquedaController> _logger;
        private readonly DirContext _context;

        public BusquedaController(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<BusquedaController> logger, DirContext context)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _logger = logger;
            _context = context;
        }

        public IActionResult Global()
        {
            return View();
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

        [HttpGet]
        public async Task<IActionResult> Buscar(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Json(new { error = "Ingresa un término de búsqueda." });

            try
            {
                var baseUrl = _config["BusquedaModule:BaseUrl"] ?? "http://modulo_busqueda:3000";
                var client  = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var empresaId = GetCurrentUserEmpresaId();
                var url      = $"{baseUrl}/buscar?q={Uri.EscapeDataString(q)}&id_empresa={empresaId}";
                var response = await client.GetAsync(url);
                var body     = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("[Busqueda] Error {Status}: {Body}", (int)response.StatusCode, body);
                    return Json(new { error = "El servicio de búsqueda no está disponible en este momento." });
                }

                var esAdminOrAuditor = User.IsInRole("Administrador") || User.IsInRole("Superior") || User.IsInRole("Super Administrador") || User.IsInRole("Auditor");
                if (!esAdminOrAuditor)
                {
                    using var doc = JsonDocument.Parse(body);
                    var root = doc.RootElement;
                    
                    if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("data", out var dataProp) && dataProp.ValueKind == JsonValueKind.Array)
                    {
                        var items = new List<JsonElement>();
                        var docIds = new List<int>();
                        foreach (var item in dataProp.EnumerateArray())
                        {
                            if (item.TryGetProperty("id_documento_sql", out var idProp) && idProp.TryGetInt32(out var idDoc))
                            {
                                items.Add(item);
                                docIds.Add(idDoc);
                            }
                        }

                        var userId = GetCurrentUserId();
                        var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == userId);
                        var userDeptoId = user?.IdDepartamento;

                        var allowedDocIds = await _context.Documentos
                            .Where(d => docIds.Contains(d.Id) && d.Estatus == true && (d.IdUsuarioCreacion == userId || d.IdDepartamento == userDeptoId))
                            .Select(d => d.Id)
                            .ToListAsync();

                        var filteredItems = items
                            .Where(item => item.GetProperty("id_documento_sql").GetInt32() is var idDoc && allowedDocIds.Contains(idDoc))
                            .ToList();

                        var filteredResponse = new
                        {
                            success = root.GetProperty("success").GetBoolean(),
                            total = filteredItems.Count,
                            data = filteredItems
                        };

                        var filteredJson = JsonSerializer.Serialize(filteredResponse);
                        return Content(filteredJson, "application/json");
                    }
                }

                return Content(body, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Busqueda] Fallo al conectar con el módulo de búsqueda.");
                return Json(new { error = "Error de conexión con el motor de búsqueda." });
            }
        }
    }
}
