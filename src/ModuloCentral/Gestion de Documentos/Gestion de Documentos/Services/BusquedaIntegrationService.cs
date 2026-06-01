using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Gestion_de_Documentos.Models;
using Microsoft.EntityFrameworkCore;

namespace Gestion_de_Documentos.Services
{
    public class BusquedaIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly DirContext _context;
        private readonly ILogger<BusquedaIntegrationService> _logger;
        private readonly string _busquedaBaseUrl;

        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public BusquedaIntegrationService(
            HttpClient httpClient,
            DirContext context,
            ILogger<BusquedaIntegrationService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _context = context;
            _logger = logger;
            _busquedaBaseUrl = configuration["BusquedaModule:BaseUrl"] ?? "http://modulo_busqueda:3000";
        }

        public async Task SincronizarDocumentoAsync(int idDocumento, int idUsuarioCreacion, string? ip = null)
        {
            var payload = await ConstruirPayloadAsync(idDocumento, idUsuarioCreacion, ip);
            if (payload is null) return;

            await EnviarPayloadAsync("/indexar", payload);
        }

        public async Task DesindexarDocumentoAsync(int idDocumento)
        {
            await EnviarDeleteAsync($"/documento/{idDocumento}");
        }

        public async Task SincronizarTodosAsync(int idUsuarioCreacion)
        {
            var documentosVigentes = await _context.Documentos
                .Where(d => d.Estatus == true && d.EstadoActual == "Vigente")
                .Select(d => d.Id)
                .ToListAsync();

            foreach (var docId in documentosVigentes)
            {
                await SincronizarDocumentoAsync(docId, idUsuarioCreacion);
            }
        }

        private async Task<object?> ConstruirPayloadAsync(int idDocumento, int idUsuario, string? ip = null)
        {
            var doc = await _context.Documentos
                .Include(d => d.DocumentoVersions)
                .Include(d => d.IdTipoDocumentoNavigation)
                .FirstOrDefaultAsync(d => d.Id == idDocumento && d.Estatus == true);

            if (doc is null) return null;

            var version = doc.DocumentoVersions
                .Where(v => v.Estatus == true && v.VersionMinor == 0)
                .OrderByDescending(v => v.NumeroVersion)
                .FirstOrDefault();

            if (version is null) return null;

            // Obtener la IP si no se pasó
            if (string.IsNullOrEmpty(ip))
            {
                var flujoDoc = await _context.FlujoAprobacions
                    .Where(f => f.IdVersionDocumento == version.Id)
                    .OrderByDescending(f => f.FechaCreacion)
                    .FirstOrDefaultAsync();
                ip = flujoDoc?.IpOrigenRemitente ?? "127.0.0.1";
            }

            // TODO: Extract text from PDF in a real implementation
            var contenidoExtraido = $"Documento {doc.Titulo}. Código: {doc.CodigoInterno}. Tipo: {doc.IdTipoDocumentoNavigation?.Nombre}";

            return new
            {
                id_documento_sql = doc.Id,
                id_empresa = doc.IdEmpresa ?? 0,
                codigo_interno = doc.CodigoInterno,
                titulo = doc.Titulo,
                tags = new string[] { doc.IdTipoDocumentoNavigation?.Nombre ?? "General" },
                contenido_extraido = contenidoExtraido,
                atributos_especificos = new { },
                id_usuario_creacion = idUsuario,
                version = $"{version.NumeroVersion}.{version.VersionMinor}",
                ip_subida = ip
            };
        }

        private async Task<bool> EnviarPayloadAsync(string ruta, object payload)
        {
            try
            {
                var json = JsonSerializer.Serialize(payload, _jsonOpts);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_busquedaBaseUrl}{ruta}")
                {
                    Content = content
                };
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                _logger.LogInformation("[SIGD-Busqueda-Sync] Enviando a {Url}: {Json}", ruta, json);

                var response = await _httpClient.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("[SIGD-Busqueda-Sync] Respuesta OK {Status}: {Body}", (int)response.StatusCode, body);
                    return true;
                }
                else
                {
                    _logger.LogError("[SIGD-Busqueda-Sync] Respuesta de error {Status}: {Body}", (int)response.StatusCode, body);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SIGD-Busqueda-Sync] Error de red al llamar a {Ruta}.", ruta);
                return false;
            }
        }

        private async Task<bool> EnviarDeleteAsync(string ruta)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_busquedaBaseUrl}{ruta}");
                _logger.LogInformation("[SIGD-Busqueda-Sync] Enviando DELETE a {Url}", ruta);

                var response = await _httpClient.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("[SIGD-Busqueda-Sync] Desindexado OK {Status}: {Body}", (int)response.StatusCode, body);
                    return true;
                }
                else
                {
                    _logger.LogError("[SIGD-Busqueda-Sync] Error al desindexar {Status}: {Body}", (int)response.StatusCode, body);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SIGD-Busqueda-Sync] Error de red al desindexar en {Ruta}.", ruta);
                return false;
            }
        }
    }
}
