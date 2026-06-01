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

        // URL interna Docker del módulo de reportes PHP
        private string ReportesBaseUrl =>
            _config["ReportesModule:BaseUrl"] ?? "http://modulo_reportes";

        // Clave API compartida entre .NET y PHP
        private string SyncApiKey =>
            _config["ReportesModule:SyncApiKey"] ?? "sigd_sync_secret_2026";

        public ModulosController(IConfiguration config, IHttpClientFactory httpClientFactory, ILogger<ModulosController> logger)
        {
            _config            = config;
            _httpClientFactory = httpClientFactory;
            _logger            = logger;
        }

        // ─── Portal de Normativas ─────────────────────────────────────────────
        /// <summary>
        /// Devuelve la vista SPA del Portal de Normativas.
        /// El contenido real se carga mediante fetch() al proxy ApiPortal.
        /// </summary>
        public IActionResult Portal()
        {
            ViewData["Title"] = "Portal de Normativas";
            return View();
        }

        // ─── Dashboard de Reportes ────────────────────────────────────────────
        /// <summary>
        /// Devuelve la vista SPA del Dashboard de Reportes.
        /// El contenido real se carga mediante fetch() al proxy ApiDashboard.
        /// </summary>
        public IActionResult Dashboard()
        {
            ViewData["Title"] = "Dashboard de Reportes";
            return View();
        }

        private int GetCurrentUserEmpresaId()
        {
            var claim = User.FindFirst("IdEmpresa")?.Value;
            return int.TryParse(claim, out var empId) ? empId : 0;
        }

        // ─── Proxy: Portal API ────────────────────────────────────────────────
        /// <summary>
        /// Reenvía peticiones del browser al endpoint PHP /api/v1/portal.php.
        /// Agrega la clave API internamente — el browser nunca la ve.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ApiPortal([FromQuery] string action = "buscar", [FromQuery] string q = "")
        {
            var empresaId = GetCurrentUserEmpresaId();
            return await ProxyGetAsync($"/api/v1/portal.php?action={Uri.EscapeDataString(action)}&q={Uri.EscapeDataString(q)}&id_empresa={empresaId}");
        }

        // ─── Proxy: Portal API — documento individual ─────────────────────────
        [HttpGet]
        public async Task<IActionResult> ApiPortalDocumento([FromQuery] int id)
        {
            var empresaId = GetCurrentUserEmpresaId();
            return await ProxyGetAsync($"/api/v1/portal.php?action=documento&id={id}&id_empresa={empresaId}");
        }

        // ─── Proxy: Dashboard API ─────────────────────────────────────────────
        /// <summary>
        /// Reenvía peticiones del browser al endpoint PHP /api/v1/dashboard.php.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ApiDashboard([FromQuery] string action = "resumen")
        {
            var empresaId = GetCurrentUserEmpresaId();
            return await ProxyGetAsync($"/api/v1/dashboard.php?action={Uri.EscapeDataString(action)}&id_empresa={empresaId}");
        }

        // ─── Proxy: Dashboard cumplimiento por depto ─────────────────────────
        [HttpGet]
        public async Task<IActionResult> ApiCumplimiento([FromQuery] int id_depto)
        {
            var empresaId = GetCurrentUserEmpresaId();
            return await ProxyGetAsync($"/api/v1/dashboard.php?action=cumplimiento&id_depto={id_depto}&id_empresa={empresaId}");
        }

        // ─── Proxy: Acuse de lectura (POST) ──────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> ApiAcuse([FromBody] System.Text.Json.JsonElement payload)
        {
            var empresaId = GetCurrentUserEmpresaId();
            var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(payload.GetRawText()) 
                       ?? new Dictionary<string, object>();
            dict["id_empresa"] = empresaId;
            return await ProxyPostAsync("/api/v1/portal.php?action=acuse", dict);
        }

        // ─── Proxy POST /Modulos/Indexar → Node.js POST /indexar ─────────────
        /// <summary>
        /// Permite que el módulo central envíe documentos al índice NoSQL de búsqueda.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrador,Superior")]
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

        // ─── Helper: GET proxy ────────────────────────────────────────────────
        private async Task<IActionResult> ProxyGetAsync(string ruta)
        {
            try
            {
                var client  = _httpClientFactory.CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Get, $"{ReportesBaseUrl}{ruta}");
                AgregarCabeceras(request);

                var response = await client.SendAsync(request);
                var body     = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("[Modulos/Proxy] GET {Ruta} → {Status}: {Body}", ruta, (int)response.StatusCode, body);
                    return StatusCode((int)response.StatusCode, body);
                }

                return Content(body, "application/json");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "[Modulos/Proxy] Error de red al llamar a {Ruta}.", ruta);
                return StatusCode(503, "{\"status\":\"error\",\"message\":\"Módulo de Reportes no disponible.\"}");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "[Modulos/Proxy] Timeout al llamar a {Ruta}.", ruta);
                return StatusCode(504, "{\"status\":\"error\",\"message\":\"Timeout al contactar Módulo de Reportes.\"}");
            }
        }

        // ─── Helper: POST proxy ───────────────────────────────────────────────
        private async Task<IActionResult> ProxyPostAsync(string ruta, object payload)
        {
            try
            {
                var client  = _httpClientFactory.CreateClient();
                var json    = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, $"{ReportesBaseUrl}{ruta}")
                {
                    Content = content
                };
                AgregarCabeceras(request);

                var response = await client.SendAsync(request);
                var body     = await response.Content.ReadAsStringAsync();

                return StatusCode((int)response.StatusCode, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Modulos/Proxy] Error POST a {Ruta}.", ruta);
                return StatusCode(503, "{\"status\":\"error\",\"message\":\"Módulo de Reportes no disponible.\"}");
            }
        }

        // ─── Helper: agregar cabeceras de autenticación ───────────────────────
        private void AgregarCabeceras(HttpRequestMessage request)
        {
            request.Headers.Add("X-Api-Key", SyncApiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}
