using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Gestion_de_Documentos.Models;
using Microsoft.EntityFrameworkCore;

namespace Gestion_de_Documentos.Services
{
    /// <summary>
    /// Servicio de integración con el Módulo de Reportes (PHP/PostgreSQL).
    /// Convierte el estado de un documento aprobado en el Módulo Central
    /// en una llamada HTTP al endpoint /api/sync.php del servicio de Reportes,
    /// garantizando que ambas bases de datos mantengan consistencia eventual.
    /// </summary>
    public class ReportesIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly DirContext _context;
        private readonly ILogger<ReportesIntegrationService> _logger;
        private readonly string _reportesBaseUrl;
        private readonly string _syncApiKey;

        // Opciones de serialización JSON compartidas con el endpoint PHP
        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = false
        };

        public ReportesIntegrationService(
            HttpClient httpClient,
            DirContext context,
            ILogger<ReportesIntegrationService> logger,
            IConfiguration configuration)
        {
            _httpClient     = httpClient;
            _context        = context;
            _logger         = logger;
            // Las URLs se leen de appsettings.json / variables de entorno Docker
            _reportesBaseUrl = configuration["ReportesModule:BaseUrl"]
                               ?? "http://modulo_reportes";
            _syncApiKey      = configuration["ReportesModule:SyncApiKey"]
                               ?? "sigd_sync_secret_2026";
        }

        // ─────────────────────────────────────────────────────────────────────
        // SINCRONIZACIÓN DE UN DOCUMENTO INDIVIDUAL
        // Llamado desde AdminController (o cualquier controlador futuro) tras
        // publicar o actualizar la versión vigente de un documento.
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Publica los metadatos de un documento en el Módulo de Reportes.
        /// Si la operación falla, registra el intento como PENDIENTE en la tabla
        /// evento_integracion para que el proceso de reintento lo resuelva.
        /// </summary>
        /// <param name="idDocumento">PK del documento en SQL Server.</param>
        /// <param name="idUsuarioCreacion">ID del usuario que disparó la acción.</param>
        public async Task SincronizarDocumentoAsync(int idDocumento, int idUsuarioCreacion)
        {
            // Construir el payload leyendo el documento y su versión más reciente
            var payload = await ConstruirPayloadAsync(idDocumento, idUsuarioCreacion);
            if (payload is null)
            {
                _logger.LogWarning("[SIGD-Sync] Documento {Id}: no se encontró en BD o no tiene versiones publicadas.", idDocumento);
                return;
            }

            var eventoId = await RegistrarEventoPendienteAsync(payload, idUsuarioCreacion);

            var exito = await EnviarPayloadAsync("/api/sync.php?action=sincronizar", payload);

            await MarcarEventoAsync(eventoId, exito
                ? "PROCESADO"
                : "FALLIDO",
                exito ? null : "El endpoint de Reportes devolvió un código de error.");
        }

        // ─────────────────────────────────────────────────────────────────────
        // SINCRONIZACIÓN EN LOTE
        // Útil para una sincronización inicial o recuperación de fallos masivos.
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Envía todos los documentos vigentes (estado "Publicado") en un solo
        /// batch hacia el endpoint sincronizar_batch del Módulo de Reportes.
        /// </summary>
        public async Task SincronizarTodosAsync(int idUsuarioSolicitante)
        {
            var documentos = await _context.Documentos
                .Include(d => d.DocumentoVersions)
                .Include(d => d.IdTipoDocumentoNavigation)
                .Where(d => d.Estatus == true && d.EstadoActual == "Vigente")
                .ToListAsync();

            if (documentos.Count == 0)
            {
                _logger.LogInformation("[SIGD-Sync] No hay documentos publicados para sincronizar.");
                return;
            }

            var payloads = new List<DocumentoSyncPayload>();
            foreach (var doc in documentos)
            {
                var p = await ConstruirPayloadAsync(doc.Id, idUsuarioSolicitante);
                if (p is not null) payloads.Add(p);
            }

            if (payloads.Count == 0) return;

            await EnviarPayloadAsync("/api/sync.php?action=sincronizar_batch", payloads);
        }

        // ─────────────────────────────────────────────────────────────────────
        // HEALTH CHECK
        // Verifica que el Módulo de Reportes esté disponible antes de sincronizar.
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Llama al endpoint ping del Módulo de Reportes y devuelve true si responde.
        /// </summary>
        public async Task<bool> PingReportesAsync()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post,
                    $"{_reportesBaseUrl}/api/sync.php?action=ping");
                AgregarCabecerasAutenticacion(request);

                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SIGD-Sync] Ping al Módulo de Reportes falló.");
                return false;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // MÉTODOS PRIVADOS
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Construye el payload JSON consultando el documento y su última versión
        /// en SQL Server.
        /// </summary>
        private async Task<DocumentoSyncPayload?> ConstruirPayloadAsync(int idDocumento, int idUsuario)
        {
            var doc = await _context.Documentos
                .Include(d => d.DocumentoVersions)
                .Include(d => d.IdTipoDocumentoNavigation)
                .FirstOrDefaultAsync(d => d.Id == idDocumento && d.Estatus == true);

            if (doc is null) return null;

            // Tomar la versión más reciente (mayor número de versión, estatus activo)
            var version = doc.DocumentoVersions
                .Where(v => v.Estatus == true)
                .OrderByDescending(v => v.NumeroVersion)
                .FirstOrDefault();

            if (version is null) return null;

            return new DocumentoSyncPayload
            {
                IdDocumento          = doc.Id,
                IdEmpresa            = doc.IdEmpresa ?? 0,
                CodigoInterno        = doc.CodigoInterno,
                Titulo               = doc.Titulo,
                IdTipo               = doc.IdTipoDocumento ?? 0,
                IdDepartamento       = doc.IdDepartamento,
                VersionActual        = version.NumeroVersion,
                FechaPublicacion     = (version.FechaSubida ?? DateTime.UtcNow).ToString("yyyy-MM-dd HH:mm:ss"),
                RutaArchivoDescarga  = version.RutaArchivoFisico,
                HashVerificacion     = version.HashDocumento,
                IdUsuarioCreacion    = idUsuario
            };
        }

        /// <summary>
        /// Serializa el payload y lo envía al endpoint PHP indicado.
        /// Retorna true si el servicio respondió con código 2xx.
        /// </summary>
        private async Task<bool> EnviarPayloadAsync<T>(string ruta, T payload)
        {
            try
            {
                var json    = JsonSerializer.Serialize(payload, _jsonOpts);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post,
                    $"{_reportesBaseUrl}{ruta}") { Content = content };
                AgregarCabecerasAutenticacion(request);

                _logger.LogInformation("[SIGD-Sync] Enviando a {Url}: {Json}", ruta, json);

                var response = await _httpClient.SendAsync(request);
                var body     = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("[SIGD-Sync] Respuesta OK {Status}: {Body}", (int)response.StatusCode, body);
                    return true;
                }
                else
                {
                    _logger.LogError("[SIGD-Sync] Respuesta de error {Status}: {Body}", (int)response.StatusCode, body);
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "[SIGD-Sync] Error de red al llamar a {Ruta}.", ruta);
                return false;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "[SIGD-Sync] Timeout al llamar a {Ruta}.", ruta);
                return false;
            }
        }

        /// <summary>
        /// Agrega la clave de API compartida a la cabecera HTTP de la petición.
        /// El endpoint PHP valida esta cabecera antes de procesar la solicitud.
        /// </summary>
        private void AgregarCabecerasAutenticacion(HttpRequestMessage request)
        {
            request.Headers.Add("X-Api-Key", _syncApiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // ─────────────────────────────────────────────────────────────────────
        // BITÁCORA DE EVENTOS (tabla evento_integracion en SQL Server)
        // ─────────────────────────────────────────────────────────────────────

        private async Task<Guid> RegistrarEventoPendienteAsync(DocumentoSyncPayload payload, int idUsuario)
        {
            var evento = new EventoIntegracion
            {
                Id                  = Guid.NewGuid(),
                TipoEvento          = "SYNC_REPORTES",
                PayloadJson         = JsonSerializer.Serialize(payload, _jsonOpts),
                Estado              = "PENDIENTE",
                FechaCreacion       = DateTime.UtcNow,
                Intentos            = 0,
                IdUsuarioCreacion   = idUsuario,
                Estatus             = true
            };
            _context.EventoIntegracions.Add(evento);
            await _context.SaveChangesAsync();
            return evento.Id;
        }

        private async Task MarcarEventoAsync(Guid eventoId, string estado, string? mensajeError)
        {
            var evento = await _context.EventoIntegracions.FindAsync(eventoId);
            if (evento is null) return;

            evento.Estado          = estado;
            evento.FechaProcesado  = DateTime.UtcNow;
            evento.Intentos        = (evento.Intentos ?? 0) + 1;
            evento.MensajeError    = mensajeError;
            await _context.SaveChangesAsync();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DTO — Contrato de datos entre Módulo Central y Módulo de Reportes
    // El nombre de cada propiedad se serializa en snake_case para coincidir
    // con las expectativas del SyncController.php
    // ─────────────────────────────────────────────────────────────────────────
    public record DocumentoSyncPayload
    {
        public int    IdDocumento         { get; init; }
        public int    IdEmpresa           { get; init; }
        public string CodigoInterno       { get; init; } = string.Empty;
        public string Titulo              { get; init; } = string.Empty;
        public int    IdTipo              { get; init; }
        public int    IdDepartamento      { get; init; }
        public int    VersionActual       { get; init; }
        public string FechaPublicacion    { get; init; } = string.Empty;
        public string RutaArchivoDescarga { get; init; } = string.Empty;
        public string? HashVerificacion   { get; init; }
        public int    IdUsuarioCreacion   { get; init; }
    }
}
