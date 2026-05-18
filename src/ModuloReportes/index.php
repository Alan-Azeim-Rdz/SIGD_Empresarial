<?php
ini_set('display_errors', 1);
ini_set('display_startup_errors', 1);
error_reporting(E_ALL);

require_once __DIR__ . '/vendor/autoload.php';

use Controllers\SyncController;
use Controllers\ReporteController;

// Configuración CORS global
header("Access-Control-Allow-Origin: *");
header("Access-Control-Allow-Methods: POST, GET");
header("Access-Control-Allow-Headers: Content-Type, Access-Control-Allow-Headers, Authorization, X-Requested-With");

$action = $_GET['action'] ?? '';

// Identificar si la petición es para una vista HTML (Frontend)
if ($action === 'portal') {
    // Si entran al portal, cargamos la vista HTML
    header("Content-Type: text/html; charset=UTF-8");
    require_once __DIR__ . '/views/portal_operario.php';
    exit;
}

// Si la ejecución pasa de aquí, es un endpoint de la API. Forzamos respuesta JSON.
header("Content-Type: application/json; charset=UTF-8");

switch ($action) {
    case 'sincronizar':
        $controller = new SyncController();
        $controller->sincronizarDocumento();
        break;

    case 'registrar_acuse':
        $controller = new ReporteController();
        $controller->registrarAcuse();
        break;

    case 'clima_cumplimiento':
        $controller = new ReporteController();
        $controller->obtenerClimaCumplimiento();
        break;

    default:
        http_response_code(200);
        echo json_encode([
            "modulo" => "Módulo de Consultas y Reportes Operacionales (PostgreSQL)",
            "estado" => "Conectado y Operacional",
            "rutas_disponibles" => [
                "API Sync" => "/index.php?action=sincronizar",
                "API Acuse" => "/index.php?action=registrar_acuse",
                "Portal Operario" => "/index.php?action=portal"
            ]
        ]);
        break;
}