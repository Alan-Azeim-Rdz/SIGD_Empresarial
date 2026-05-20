<?php
/**
 * ============================================================
 * SIGD Empresarial — Módulo de Reportes
 * Endpoint dedicado de sincronización con el Módulo Central
 * Ruta docker: POST http://modulo_reportes/api/sync.php
 * ============================================================
 * Este archivo es el punto de entrada exclusivo para las
 * peticiones HTTP que llegan desde el Módulo Central (.NET).
 * Verifica la clave API compartida y delega la lógica al
 * SyncController, manteniendo la separación de responsabilidades.
 * ============================================================
 */

declare(strict_types=1);

// ── Carga del autoloader de Composer ──────────────────────────
require_once __DIR__ . '/../vendor/autoload.php';

use Controllers\SyncController;

// ── Cabeceras de respuesta ─────────────────────────────────────
header('Content-Type: application/json; charset=UTF-8');

// ── CORS: Solo el Módulo Central (misma red Docker) ───────────
// En producción sustituir '*' por la IP/hostname exacta del contenedor central
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: POST, OPTIONS');
header('Access-Control-Allow-Headers: Content-Type, X-Api-Key');

// Respuesta anticipada al preflight de CORS
if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') {
    http_response_code(204);
    exit;
}

// ── Validación de la clave API compartida ─────────────────────
// El Módulo Central la inyecta en cada petición como cabecera HTTP.
// El valor debe coincidir con la variable de entorno SYNC_API_KEY
// definida en el docker-compose.yml (inyectada en ambos módulos).
$apiKey          = $_SERVER['HTTP_X_API_KEY'] ?? '';
$expectedApiKey  = getenv('SYNC_API_KEY') ?: 'sigd_sync_secret_2026';   // fallback solo para desarrollo local

if (empty($apiKey) || !hash_equals($expectedApiKey, $apiKey)) {
    http_response_code(401);
    echo json_encode([
        'status'  => 'error',
        'message' => 'Acceso denegado: clave de API inválida o ausente.'
    ]);
    exit;
}

// ── Solo se permiten peticiones POST ──────────────────────────
if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    http_response_code(405);
    echo json_encode([
        'status'  => 'error',
        'message' => 'Método no permitido. Usa POST para sincronizar documentos.'
    ]);
    exit;
}

// ── Detectar sub-acción en el query string ────────────────────
// Ejemplo: POST /api/sync.php?action=sincronizar
//          POST /api/sync.php?action=sincronizar_batch
$action = $_GET['action'] ?? 'sincronizar';

try {
    $controller = new SyncController();

    switch ($action) {
        case 'sincronizar':
            // Sincroniza un único documento publicado/actualizado
            $controller->sincronizarDocumento();
            break;

        case 'sincronizar_batch':
            // Sincroniza múltiples documentos en una sola llamada
            $controller->sincronizarBatch();
            break;

        case 'ping':
            // Health-check: el Módulo Central lo usa para verificar
            // que el servicio de Reportes está disponible antes de sincronizar
            http_response_code(200);
            echo json_encode([
                'status'    => 'ok',
                'modulo'    => 'ModuloReportes',
                'timestamp' => date('c'),
            ]);
            break;

        default:
            http_response_code(400);
            echo json_encode([
                'status'  => 'error',
                'message' => "Acción desconocida: '{$action}'. Acciones válidas: sincronizar, sincronizar_batch, ping."
            ]);
    }
} catch (Throwable $e) {
    // Captura cualquier error inesperado para no exponer trazas al exterior
    http_response_code(500);
    echo json_encode([
        'status'  => 'error',
        'message' => 'Error interno del servidor. Consulta los logs del contenedor PHP.'
    ]);
    // Registrar el error real en el log de PHP (visible en `docker logs app_reportes_php`)
    error_log('[SIGD-Sync] Error no controlado: ' . $e->getMessage() . ' en ' . $e->getFile() . ':' . $e->getLine());
}
