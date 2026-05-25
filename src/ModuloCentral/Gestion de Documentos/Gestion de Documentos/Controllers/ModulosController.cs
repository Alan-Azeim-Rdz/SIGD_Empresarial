using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Gestion_de_Documentos.Controllers
{
    [Authorize]
    public class ModulosController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ModulosController> _logger;

        public ModulosController(IConfiguration config, IHttpClientFactory httpClientFactory, ILogger<ModulosController> logger)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // ─── Portal de Normativas (PHP) ───────────────────────────────────────────
        public IActionResult Portal()
        {
            // La URL del iframe debe ser resolvible desde el navegador del cliente,
            // no desde el servidor .NET. Usamos el mismo host con puerto 8000.
            var clientHost = $"{Request.Scheme}://{Request.Host.Host}:8000";
            ViewData["Title"]       = "Portal de Normativas";
            ViewData["IframeUrl"]   = $"{clientHost}/index.php?action=portal";
            ViewData["IconClass"]   = "fas fa-book";
            ViewData["SubTitle"]    = "Consulta y firma normativas y procedimientos vigentes en planta.";
            return View("Iframe");
        }

        // ─── Dashboard de Reportes (PHP) ─────────────────────────────────────────
        // Sin restricción de rol: la vista muestra el iframe a cualquier usuario
        // autenticado. El acceso visual ya se controla en el navbar y Home/Index.
        public IActionResult Dashboard()
        {
            var clientHost = $"{Request.Scheme}://{Request.Host.Host}:8000";
            ViewData["Title"]    = "Dashboard de Reportes";
            ViewData["IframeUrl"] = $"{clientHost}/index.php?action=dashboard";
            ViewData["IconClass"] = "fas fa-chart-bar";
            ViewData["SubTitle"]  = "KPIs y reportes de auditoría en tiempo real.";
            return View("Iframe");
        }

        // ─── Proxy POST /Modulos/Indexar → Node.js POST /indexar ─────────────────
        /// <summary>
        /// Permite que el módulo central envíe documentos al índice NoSQL de búsqueda.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrador,Editor")]
        public async Task<IActionResult> Indexar([FromBody] object payload)
        {
            try
            {
                var nodeBase = _config["BusquedaModule:BaseUrl"] ?? "http://modulo_busqueda:3000";
                var client   = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var json     = JsonSerializer.Serialize(payload);
                var content  = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{nodeBase}/indexar", content);
                var body     = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("[Modulos/Indexar] Error {Status}: {Body}", (int)response.StatusCode, body);
                    return StatusCode((int)response.StatusCode, new { error = "El servicio de búsqueda rechazó la solicitud.", detalle = body });
                }

                return Content(body, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Modulos/Indexar] Fallo al conectar con el módulo de búsqueda.");
                return StatusCode(503, new { error = "Servicio de búsqueda no disponible." });
            }
        }
    }
}
