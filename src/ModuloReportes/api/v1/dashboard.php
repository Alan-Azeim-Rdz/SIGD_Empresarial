<?php
// ═══════════════════════════════════════════════════════
// SIGD Empresarial — API v1 / Dashboard de Reportes
// Endpoint REST que devuelve JSON con KPIs y estadísticas.
// Consumido por el ModuloCentral (.NET) via proxy interno.
// ═══════════════════════════════════════════════════════
declare(strict_types=1);

require_once __DIR__ . '/../../vendor/autoload.php';

use Config\Database;
use Config\Logger;

header('Content-Type: application/json; charset=UTF-8');

// ── Clave API compartida ─────────────────────────────────────────────────────
$expectedKey = getenv('SYNC_API_KEY') ?: 'sigd_sync_secret_2026';
$providedKey = $_SERVER['HTTP_X_API_KEY'] ?? '';

if (!hash_equals($expectedKey, $providedKey)) {
    http_response_code(401);
    echo json_encode(['status' => 'error', 'message' => 'API key inválida o ausente.']);
    exit;
}

// ── Conexión ─────────────────────────────────────────────────────────────────
$db     = (new Database())->getConnection();
$logger = Logger::getInstance();
$action = $_GET['action'] ?? 'resumen';
$id_empresa = (int)($_GET['id_empresa'] ?? 0);

if (!$id_empresa) {
    http_response_code(400);
    echo json_encode(['status' => 'error', 'message' => 'id_empresa requerido.']);
    exit;
}

// ── Acción: resumen completo (KPIs + tablas + gráficas en una sola llamada) ──
if ($action === 'resumen') {
    try {
        // KPIs principales
        $kpis = [];

        $stmt = $db->prepare("SELECT COUNT(*) AS total FROM documento_vigente WHERE estatus = true AND id_empresa = :id_empresa");
        $stmt->execute([':id_empresa' => $id_empresa]);
        $kpis['total_documentos'] = (int)$stmt->fetchColumn();

        $stmt = $db->prepare("SELECT COUNT(DISTINCT id_departamento) AS total FROM documento_vigente WHERE estatus = true AND id_empresa = :id_empresa");
        $stmt->execute([':id_empresa' => $id_empresa]);
        $kpis['total_departamentos'] = (int)$stmt->fetchColumn();

        $stmt = $db->prepare("SELECT MAX(fecha_publicacion) AS ultima FROM documento_vigente WHERE estatus = true AND id_empresa = :id_empresa");
        $stmt->execute([':id_empresa' => $id_empresa]);
        $kpis['ultima_publicacion'] = $stmt->fetchColumn() ?: null;

        try {
            $stmt = $db->prepare("SELECT COUNT(*) AS total FROM acuse_lectura a JOIN documento_vigente d ON a.id_documento = d.id_documento WHERE d.id_empresa = :id_empresa");
            $stmt->execute([':id_empresa' => $id_empresa]);
            $kpis['total_acuses'] = (int)$stmt->fetchColumn();
        } catch (Throwable) {
            $kpis['total_acuses'] = 0;
        }

        // Documentos por departamento (gráfica de barras)
        $stmtDepto = $db->prepare("
            SELECT id_departamento AS departamento, COUNT(*) AS total
            FROM documento_vigente
            WHERE estatus = true AND id_empresa = :id_empresa
            GROUP BY id_departamento
            ORDER BY total DESC
            LIMIT 10
        ");
        $stmtDepto->execute([':id_empresa' => $id_empresa]);
        $porDepartamento = $stmtDepto->fetchAll(PDO::FETCH_ASSOC);

        // Evolución mensual de publicaciones (gráfica de línea — últimos 12 meses)
        $stmtEvo = $db->prepare("
            SELECT
                TO_CHAR(fecha_publicacion, 'YYYY-MM') AS mes,
                COUNT(*)                               AS total
            FROM documento_vigente
            WHERE estatus = true AND id_empresa = :id_empresa
              AND fecha_publicacion >= NOW() - INTERVAL '12 months'
            GROUP BY mes
            ORDER BY mes ASC
        ");
        $stmtEvo->execute([':id_empresa' => $id_empresa]);
        $evolucion = $stmtEvo->fetchAll(PDO::FETCH_ASSOC);

        // Últimos 10 documentos publicados (tabla de actividad reciente)
        $stmtRecientes = $db->prepare("
            SELECT
                id_documento,
                codigo_interno,
                titulo,
                version_actual,
                TO_CHAR(fecha_publicacion, 'DD/MM/YYYY HH24:MI') AS fecha_formateada,
                id_departamento
            FROM documento_vigente
            WHERE estatus = true AND id_empresa = :id_empresa
            ORDER BY fecha_publicacion DESC
            LIMIT 10
        ");
        $stmtRecientes->execute([':id_empresa' => $id_empresa]);
        $recientes = $stmtRecientes->fetchAll(PDO::FETCH_ASSOC);

        echo json_encode([
            'status'          => 'ok',
            'kpis'            => $kpis,
            'por_departamento' => $porDepartamento,
            'evolucion'       => $evolucion,
            'recientes'       => $recientes,
        ]);
    } catch (Throwable $e) {
        $logger->error('api_v1_dashboard_resumen_error', ['error' => $e->getMessage()]);
        http_response_code(500);
        echo json_encode(['status' => 'error', 'message' => 'Error al obtener datos del dashboard.']);
    }
    exit;
}

// ── Acción: clima de cumplimiento por departamento ────────────────────────────
if ($action === 'cumplimiento') {
    $id_depto = (int)($_GET['id_depto'] ?? 0);

    if (!$id_depto) {
        http_response_code(400);
        echo json_encode(['status' => 'error', 'message' => 'id_depto requerido.']);
        exit;
    }

    try {
        // Validar que el departamento pertenece a la empresa
        // (En la DB postgres_sigd de reportes, asumimos que id_departamento se relaciona al documento)
        // Hacemos una verificación de consistencia de tenant en documento_vigente
        $checkStmt = $db->prepare("SELECT COUNT(*) FROM documento_vigente WHERE id_departamento = :id_depto AND id_empresa = :id_empresa");
        $checkStmt->execute([':id_depto' => $id_depto, ':id_empresa' => $id_empresa]);
        if ((int)$checkStmt->fetchColumn() === 0) {
            http_response_code(403);
            echo json_encode(['status' => 'error', 'message' => 'El departamento no pertenece a la empresa especificada o no tiene documentos.']);
            exit;
        }

        $stmt = $db->prepare("SELECT * FROM sp_reporte_cumplimiento_depto(:id_depto)");
        $stmt->bindValue(':id_depto', $id_depto, PDO::PARAM_INT);
        $stmt->execute();
        $resultado = $stmt->fetch(PDO::FETCH_ASSOC);

        echo json_encode([
            'status' => 'ok',
            'data'   => $resultado ?: ['mensaje' => 'Sin datos para este departamento'],
        ]);
    } catch (Throwable $e) {
        $logger->error('api_v1_cumplimiento_error', ['id_depto' => $id_depto, 'error' => $e->getMessage()]);
        http_response_code(500);
        echo json_encode(['status' => 'error', 'message' => 'Error al ejecutar reporte de cumplimiento.']);
    }
    exit;
}

// ── Fallback ──────────────────────────────────────────────────────────────────
http_response_code(400);
echo json_encode(['status' => 'error', 'message' => "Acción '$action' no reconocida."]);
