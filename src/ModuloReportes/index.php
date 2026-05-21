<?php
ini_set('display_errors', 1);
ini_set('display_startup_errors', 1);
error_reporting(E_ALL);

require_once __DIR__ . '/vendor/autoload.php';

use Controllers\SyncController;
use Controllers\ReporteController;
use Controllers\DashboardController;

// Configuración CORS global
header("Access-Control-Allow-Origin: *");
header("Access-Control-Allow-Methods: POST, GET");
header("Access-Control-Allow-Headers: Content-Type, Access-Control-Allow-Headers, Authorization, X-Requested-With");

$action = $_GET['action'] ?? '';

// Identificar si la petición es para una vista HTML (Frontend)
if ($action === 'portal') {
    header("Content-Type: text/html; charset=UTF-8");
    require_once __DIR__ . '/views/portal_operario.php';
    exit;
}

if ($action === 'dashboard') {
    $controller = new DashboardController();
    $controller->mostrarDashboard();
    exit;
}

// Si la ejecución pasa de aquí, es un endpoint de la API. Forzamos respuesta JSON.
header("Content-Type: application/json; charset=UTF-8");

switch ($action) {
    case 'sincronizar':
        $controller = new SyncController();
        $controller->sincronizarDocumento();
        break;

    case 'sincronizar_batch':
        $controller = new SyncController();
        $controller->sincronizarBatch();
        break;

    case 'registrar_acuse':
        $controller = new ReporteController();
        $controller->registrarAcuse();
        break;

    case 'clima_cumplimiento':
        $controller = new ReporteController();
        $controller->obtenerClimaCumplimiento();
        break;

    case 'api_kpis':
        $controller = new DashboardController();
        $controller->obtenerKpis();
        break;

    case 'api_docs_por_depto':
        $controller = new DashboardController();
        $controller->docsPorDepartamento();
        break;

    case 'api_evolucion':
        $controller = new DashboardController();
        $controller->evolucionMensual();
        break;

    case 'api_recientes':
        $controller = new DashboardController();
        $controller->documentosRecientes();
        break;

    default:
        http_response_code(200);
        echo json_encode([
            "modulo"  => "Módulo de Consultas y Reportes Operacionales (PostgreSQL)",
            "estado"  => "Conectado y Operacional",
            "rutas_disponibles" => [
                "Dashboard"       => "/index.php?action=dashboard",
                "Portal Operario" => "/index.php?action=portal",
                "API Sync"        => "/index.php?action=sincronizar",
                "API Acuse"       => "/index.php?action=registrar_acuse",
                "API KPIs"        => "/index.php?action=api_kpis",
                "API Docs/Depto"  => "/index.php?action=api_docs_por_depto",
                "API Evolución"   => "/index.php?action=api_evolucion",
                "API Recientes"   => "/index.php?action=api_recientes",
            ]
        ]);
        break;
}