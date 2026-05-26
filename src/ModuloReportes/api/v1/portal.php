<?php
// ═══════════════════════════════════════════════════════
// SIGD Empresarial — API v1 / Portal de Normativas
// Endpoint REST que devuelve JSON con documentos vigentes.
// Consumido por el ModuloCentral (.NET) via proxy interno.
// ═══════════════════════════════════════════════════════
declare(strict_types=1);

require_once __DIR__ . '/../../vendor/autoload.php';

use Config\Database;
use Config\Logger;

header('Content-Type: application/json; charset=UTF-8');

// ── Clave API compartida ─────────────────────────────────────────────────────
// El ModuloCentral envía la clave en la cabecera X-Api-Key.
// Las peticiones sin clave válida se rechazan con 401.
$expectedKey = getenv('SYNC_API_KEY') ?: 'sigd_sync_secret_2026';
$providedKey = $_SERVER['HTTP_X_API_KEY'] ?? '';

if (!hash_equals($expectedKey, $providedKey)) {
    http_response_code(401);
    echo json_encode(['status' => 'error', 'message' => 'API key inválida o ausente.']);
    exit;
}

// ── Conexión ─────────────────────────────────────────────────────────────────
$db = (new Database())->getConnection();

// ── Parámetros ───────────────────────────────────────────────────────────────
// ── Parámetros ───────────────────────────────────────────────────────────────
$action = $_GET['action'] ?? 'buscar';
$q      = trim($_GET['q'] ?? '');
$id     = (int)($_GET['id'] ?? 0);
$id_empresa = (int)($_GET['id_empresa'] ?? 0);

$logger = Logger::getInstance();

// Para el acuse, leemos la empresa desde el cuerpo JSON si es un POST
if ($action === 'acuse' && $_SERVER['REQUEST_METHOD'] === 'POST') {
    $body = json_decode(file_get_contents('php://input'), true) ?? [];
    $id_documento = (int)($body['id_documento'] ?? 0);
    $id_usuario   = (int)($body['id_usuario'] ?? 0);
    $id_empresa   = (int)($body['id_empresa'] ?? 0);

    if (!$id_documento || !$id_usuario || !$id_empresa) {
        http_response_code(400);
        echo json_encode(['status' => 'error', 'message' => 'Faltan id_documento, id_usuario o id_empresa en el acuse.']);
        exit;
    }

    try {
        // Validar que el documento pertenezca a la empresa
        $checkStmt = $db->prepare("SELECT COUNT(*) FROM documento_vigente WHERE id_documento = :id_doc AND id_empresa = :id_empresa");
        $checkStmt->execute([':id_doc' => $id_documento, ':id_empresa' => $id_empresa]);
        if ((int)$checkStmt->fetchColumn() === 0) {
            http_response_code(403);
            echo json_encode(['status' => 'error', 'message' => 'El documento no pertenece a la empresa especificada.']);
            exit;
        }

        $ip        = $_SERVER['REMOTE_ADDR'] ?? '0.0.0.0';
        $userAgent = $_SERVER['HTTP_USER_AGENT'] ?? 'Desconocido';

        $stmt = $db->prepare("
            INSERT INTO acuse_lectura (id_documento, id_usuario, fecha_lectura, direccion_ip, user_agent, id_usuario_creacion, estatus)
            VALUES (:id_doc, :id_usr, NOW(), :ip, :ua, :id_usr, true)
        ");
        $stmt->execute([
            ':id_doc' => $id_documento,
            ':id_usr' => $id_usuario,
            ':ip'     => $ip,
            ':ua'     => $userAgent,
        ]);

        http_response_code(201);
        echo json_encode(['status' => 'success', 'message' => 'Acuse registrado correctamente.']);
    } catch (Throwable $e) {
        $logger->error('api_v1_acuse_error', ['error' => $e->getMessage()]);
        http_response_code(500);
        echo json_encode(['status' => 'error', 'message' => 'Error al registrar acuse.']);
    }
    exit;
}

// Para otras acciones, validamos id_empresa de forma obligatoria
if (!$id_empresa) {
    http_response_code(400);
    echo json_encode(['status' => 'error', 'message' => 'id_empresa requerido.']);
    exit;
}

// ── Acción: buscar / listar documentos ───────────────────────────────────────
if ($action === 'buscar') {
    try {
        if ($q !== '') {
            $stmt = $db->prepare("
                SELECT id_documento,
                       titulo,
                       codigo_interno,
                       version_actual,
                       TO_CHAR(fecha_publicacion, 'YYYY-MM-DD') AS fecha_publicacion,
                       id_departamento
                FROM documento_vigente
                WHERE estatus = true AND id_empresa = :id_empresa
                  AND titulo ILIKE :q
                ORDER BY titulo
            ");
            $stmt->execute([':q' => "%$q%", ':id_empresa' => $id_empresa]);
        } else {
            $stmt = $db->prepare("
                SELECT id_documento,
                       titulo,
                       codigo_interno,
                       version_actual,
                       TO_CHAR(fecha_publicacion, 'YYYY-MM-DD') AS fecha_publicacion,
                       id_departamento
                FROM documento_vigente
                WHERE estatus = true AND id_empresa = :id_empresa
                ORDER BY titulo
            ");
            $stmt->execute([':id_empresa' => $id_empresa]);
        }

        $documentos = $stmt->fetchAll(PDO::FETCH_ASSOC);

        echo json_encode([
            'status'     => 'ok',
            'query'      => $q,
            'total'      => count($documentos),
            'documentos' => $documentos,
        ]);
    } catch (Throwable $e) {
        $logger->error('api_v1_portal_buscar_error', ['error' => $e->getMessage()]);
        http_response_code(500);
        echo json_encode(['status' => 'error', 'message' => 'Error al consultar documentos.']);
    }
    exit;
}

// ── Acción: metadata de un documento individual ───────────────────────────────
if ($action === 'documento') {
    if (!$id) {
        http_response_code(400);
        echo json_encode(['status' => 'error', 'message' => 'ID de documento requerido.']);
        exit;
    }
    try {
        $stmt = $db->prepare("
            SELECT id_documento,
                   titulo,
                   codigo_interno,
                   version_actual,
                   TO_CHAR(fecha_publicacion, 'YYYY-MM-DD') AS fecha_publicacion,
                   id_departamento
            FROM documento_vigente
            WHERE id_documento = :id AND estatus = true AND id_empresa = :id_empresa
        ");
        $stmt->execute([':id' => $id, ':id_empresa' => $id_empresa]);
        $doc = $stmt->fetch(PDO::FETCH_ASSOC);

        if (!$doc) {
            http_response_code(404);
            echo json_encode(['status' => 'error', 'message' => 'Documento no encontrado o inactivo para la empresa especificada.']);
            exit;
        }

        echo json_encode(['status' => 'ok', 'documento' => $doc]);
    } catch (Throwable $e) {
        $logger->error('api_v1_portal_doc_error', ['id' => $id, 'error' => $e->getMessage()]);
        http_response_code(500);
        echo json_encode(['status' => 'error', 'message' => 'Error al obtener documento.']);
    }
    exit;
}

// ── Fallback ──────────────────────────────────────────────────────────────────
http_response_code(400);
echo json_encode(['status' => 'error', 'message' => "Acción '$action' no reconocida o inválida."]);
