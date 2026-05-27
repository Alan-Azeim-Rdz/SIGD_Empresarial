using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Gestion_de_Documentos.Controllers
{
    [Authorize]
    public class BusquedaController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<BusquedaController> _logger;

        public BusquedaController(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<BusquedaController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _logger = logger;
        }

        public async Task<IActionResult> Global(string q = "")
        {
            try
            {
                var baseUrl = _config["BusquedaModule:BaseUrl"] ?? "http://modulo_busqueda:3000";
                var client  = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Si no hay búsqueda, traer todos con término vacío usando /buscar?q=a|e|i|o|u
                var termino = string.IsNullOrWhiteSpace(q) ? "a" : q;
                var url      = $"{baseUrl}/buscar?q={Uri.EscapeDataString(termino)}";
                var response = await client.GetAsync(url);
                var body     = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(body);
                    var root = doc.RootElement;
                    ViewBag.Resultados = body;
                    ViewBag.Total = root.TryGetProperty("total", out var total) ? total.GetInt32() : 0;
                }
                else
                {
                    ViewBag.Resultados = null;
                    ViewBag.Total = 0;
                }
            }
            catch
            {
                ViewBag.Resultados = null;
                ViewBag.Total = 0;
            }

            ViewBag.Query = q;
            return View();
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

                var url      = $"{baseUrl}/buscar?q={Uri.EscapeDataString(q)}";
                var response = await client.GetAsync(url);
                var body     = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("[Busqueda] Error {Status}: {Body}", (int)response.StatusCode, body);
                    return Json(new { error = "El servicio de búsqueda no está disponible en este momento." });
                }

                // Parsear y devolver la respuesta al cliente (AJAX)
                using var doc = JsonDocument.Parse(body);
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
