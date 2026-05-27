<?php
// ═══════════════════════════════════════════════════════
// MÓDULO DE CONSULTA PÚBLICA — PHP 8 + PostgreSQL
// Proyecto: SIGD Empresarial
// Autor: Josue J.A.V.
// ═══════════════════════════════════════════════════════

// Cargamos las dependencias instaladas con Composer
require_once __DIR__ . '/vendor/autoload.php';

use Config\Logger;
use Dompdf\Dompdf;
use Dompdf\Options;
use Controllers\DashboardController;
use Controllers\ReporteController;

$logger = Logger::getInstance();

// ── 1. CONEXIÓN A POSTGRESQL ──────────────────────────
$host     = getenv('DB_HOST') ?: 'postgres';
$port     = getenv('DB_PORT') ?: '5432';
$dbname   = getenv('DB_NAME') ?: 'sigd_reportes';
$user     = getenv('DB_USER') ?: 'sigd_user';
$password = getenv('DB_PASS') ?: '';

$pdo     = null;
$dbError = null;

try {
    $pdo = new PDO(
        "pgsql:host=$host;port=$port;dbname=$dbname",
        $user,
        $password,
        [PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION]
    );
} catch (PDOException $e) {
    $dbError = $e->getMessage();
    $logger->error('db_connection_failed', [
        'host'  => $host,
        'port'  => $port,
        'db'    => $dbname,
        'error' => $e->getMessage(),
    ]);
}

// ── 2. HELPER: obtener estatus legible ───────────────
function estatusTexto(mixed $val): string
{
    // PostgreSQL devuelve booleans como 't'/'f' o '1'/'0' según el driver
    return ($val === true || $val === 't' || $val === '1' || $val === 'true')
        ? 'Aprobado'
        : 'Inactivo';
}

// ── 3. LÓGICA DE RUTAS ────────────────────────────────
// Acepta tanto ?action= (usado por el iframe .NET y el JS del dashboard)
// como ?accion= (rutas PHP legacy) para compatibilidad total.
$accion = $_GET['action'] ?? $_GET['accion'] ?? 'buscar';

$pagina    = max(1, (int)($_GET['pagina'] ?? 1));
$porPagina = 10;
$offset    = ($pagina - 1) * $porPagina;

// ── 4. ACCIÓN: GENERAR REPORTE PDF ───────────────────
if ($accion === 'reporte') {
    if (!$pdo) {
        http_response_code(503);
        header('Content-Type: text/plain; charset=UTF-8');
        echo "No se puede generar el reporte: sin conexión a la base de datos.\n$dbError";
        exit;
    }

    $stmt = $pdo->query("
        SELECT id_documento,
               titulo,
               codigo_interno,
               version_actual,
               TO_CHAR(fecha_publicacion, 'YYYY-MM-DD') AS fecha_publicacion,
               estatus
        FROM documento_vigente
        WHERE estatus = true
        ORDER BY titulo
    ");
    $documentos = $stmt->fetchAll(PDO::FETCH_ASSOC);

    $fecha_hoy = date('d/m/Y H:i');
    $total     = count($documentos);
    $filas     = '';

    foreach ($documentos as $doc) {
        $estado = estatusTexto($doc['estatus']);
        $filas .= "
        <tr>
            <td>{$doc['id_documento']}</td>
            <td>{$doc['titulo']}</td>
            <td>{$doc['codigo_interno']}</td>
            <td>v{$doc['version_actual']}</td>
            <td>{$doc['fecha_publicacion']}</td>
            <td><span style='color: green; font-weight: bold;'>{$estado}</span></td>
        </tr>";
    }

    $html = "
    <html>
    <head>
        <style>
            body { font-family: Arial, sans-serif; font-size: 12px; color: #333; }
            h1   { color: #1a56db; text-align: center; font-size: 18px; }
            h3   { color: #555; text-align: center; font-size: 13px; }
            table { width: 100%; border-collapse: collapse; margin-top: 20px; }
            th   { background: #1a56db; color: white; padding: 8px; text-align: left; }
            td   { padding: 7px; border-bottom: 1px solid #ddd; }
            tr:nth-child(even) { background: #f5f5f5; }
            .footer { margin-top: 30px; text-align: center; color: #888; font-size: 10px; }
            .total  { margin-top: 15px; font-weight: bold; color: #1a56db; }
        </style>
    </head>
    <body>
        <h1>Reporte de Cumplimiento de Normativas</h1>
        <h3>Sistema Integral de Gestión Documental — SIGD Empresarial</h3>
        <p>Fecha de generación: <strong>$fecha_hoy</strong></p>
        <table>
            <thead>
                <tr>
                    <th>ID</th>
                    <th>Título del Documento</th>
                    <th>Código Interno</th>
                    <th>Versión</th>
                    <th>Fecha Publicación</th>
                    <th>Estado</th>
                </tr>
            </thead>
            <tbody>$filas</tbody>
        </table>
        <p class='total'>Total de documentos vigentes: $total</p>
        <div class='footer'>
            Generado por SIGD Empresarial — Módulo de Consulta Pública
        </div>
    </body>
    </html>";

    $options = new Options();
    $options->set('isHtml5ParserEnabled', true);

    $dompdf = new Dompdf($options);
    $dompdf->loadHtml($html);
    $dompdf->setPaper('A4', 'landscape');
    $dompdf->render();

    $dompdf->stream("Reporte_Cumplimiento_$fecha_hoy.pdf", ['Attachment' => true]);
    exit;
}

// ── 5. ACCIÓN: DESCARGAR ARCHIVO ─────────────────────
if ($accion === 'descargar') {
    $id = (int)($_GET['id'] ?? 0);

    if (!$id) {
        http_response_code(400);
        die('ID de documento no especificado.');
    }

    if (!$pdo) {
        http_response_code(503);
        die('Sin conexión a la base de datos. No se puede descargar el documento.');
    }

    try {
        $stmt = $pdo->prepare("
            SELECT id_documento,
                   titulo,
                   codigo_interno,
                   version_actual,
                   TO_CHAR(fecha_publicacion, 'YYYY-MM-DD') AS fecha_publicacion
            FROM documento_vigente
            WHERE id_documento = :id AND estatus = true
        ");
        $stmt->execute([':id' => $id]);
        $doc = $stmt->fetch(PDO::FETCH_ASSOC);

        if (!$doc) {
            http_response_code(404);
            die('Documento no encontrado o no está vigente.');
        }

        $options = new Options();
        $options->set('isHtml5ParserEnabled', true);
        $dompdf = new Dompdf($options);

        $htmlDoc = "
        <html><head><style>
            body { font-family: Arial, sans-serif; padding: 40px; color: #333; }
            h1 { color: #1a56db; font-size: 20px; border-bottom: 2px solid #1a56db; padding-bottom: 10px; }
            table { width: 100%; margin-top: 20px; border-collapse: collapse; }
            td { padding: 10px 14px; border-bottom: 1px solid #e2e8f0; font-size: 13px; }
            .label { font-weight: bold; color: #555; width: 40%; background: #f8fafc; }
            .footer { margin-top: 40px; color: #aaa; font-size: 10px; text-align: center; }
        </style></head><body>
            <h1>Documento: {$doc['titulo']}</h1>
            <table>
                <tr>
                    <td class='label'>Código Interno</td>
                    <td>{$doc['codigo_interno']}</td>
                </tr>
                <tr>
                    <td class='label'>Versión</td>
                    <td>v{$doc['version_actual']}</td>
                </tr>
                <tr>
                    <td class='label'>Fecha de Publicación</td>
                    <td>{$doc['fecha_publicacion']}</td>
                </tr>
                <tr>
                    <td class='label'>ID Documento</td>
                    <td>{$doc['id_documento']}</td>
                </tr>
            </table>
            <div class='footer'>Generado por SIGD Empresarial — Módulo de Consulta Pública</div>
        </body></html>";

        $dompdf->loadHtml($htmlDoc);
        $dompdf->setPaper('A4', 'portrait');
        $dompdf->render();

        $filename = 'Documento_' . $doc['codigo_interno'] . '_v' . $doc['version_actual'] . '.pdf';
        $dompdf->stream($filename, ['Attachment' => true]);
    } catch (\Throwable $e) {
        $logger->error('download_failed', [
            'id_documento' => $id,
            'error'        => $e->getMessage(),
            'file'         => $e->getFile(),
            'line'         => $e->getLine(),
        ]);
        http_response_code(500);
        die('Error interno al generar el documento.');
    }
    exit;
}

// ── 5.1. ACCIÓN: PORTAL DE OPERARIOS Y DASHBOARD DE REPORTES ───────────
if ($accion === 'portal') {
    require_once __DIR__ . '/views/portal_operario.php';
    exit;
}

if ($accion === 'dashboard') {
    $controller = new DashboardController();
    $controller->mostrarDashboard();
    exit;
}

if ($accion === 'api_kpis') {
    $controller = new DashboardController();
    $controller->obtenerKpis();
    exit;
}

if ($accion === 'api_docs_por_depto') {
    $controller = new DashboardController();
    $controller->docsPorDepartamento();
    exit;
}

if ($accion === 'api_evolucion') {
    $controller = new DashboardController();
    $controller->evolucionMensual();
    exit;
}

if ($accion === 'api_recientes') {
    $controller = new DashboardController();
    $controller->documentosRecientes();
    exit;
}

if ($accion === 'registrar_acuse') {
    $controller = new ReporteController();
    $controller->registrarAcuse();
    exit;
}

if ($accion === 'clima_cumplimiento') {
    $controller = new ReporteController();
    $controller->obtenerClimaCumplimiento();
    exit;
}

// ── 6. ACCIÓN: BUSCAR DOCUMENTOS (página principal) ──
$busqueda   = $_GET['q'] ?? '';
$documentos = [];

if ($pdo) {
    try {
        if ($busqueda !== '') {
            $stmt = $pdo->prepare("
                SELECT id_documento,
                       titulo,
                       codigo_interno,
                       version_actual,
                       TO_CHAR(fecha_publicacion, 'YYYY-MM-DD') AS fecha_publicacion,
                       estatus
                FROM documento_vigente
                WHERE titulo ILIKE :q
                  AND estatus = true
                ORDER BY titulo
            ");
            $stmt->execute([':q' => "%$busqueda%"]);
            $documentos = $stmt->fetchAll(PDO::FETCH_ASSOC);
        } else {
            // Total para paginado
            $stmtTotal = $pdo->query("SELECT COUNT(*) FROM documento_vigente WHERE estatus = true");
            $totalDocs = (int)$stmtTotal->fetchColumn();
            $totalPaginas = (int)ceil($totalDocs / $porPagina);

            $stmt = $pdo->prepare("
                SELECT id_documento,
                       titulo,
                       codigo_interno,
                       version_actual,
                       TO_CHAR(fecha_publicacion, 'YYYY-MM-DD') AS fecha_publicacion,
                       estatus
                FROM documento_vigente
                WHERE estatus = true
                ORDER BY titulo
                LIMIT :limite OFFSET :offset
            ");
            $stmt->bindValue(':limite', $porPagina, PDO::PARAM_INT);
            $stmt->bindValue(':offset', $offset,    PDO::PARAM_INT);
            $stmt->execute();
            $documentos = $stmt->fetchAll(PDO::FETCH_ASSOC);
        }
    } catch (\Throwable $e) {
        $logger->error('search_failed', [
            'q'     => $busqueda,
            'error' => $e->getMessage(),
            'file'  => $e->getFile(),
            'line'  => $e->getLine(),
        ]);
        $documentos = [];
    }
}
?>
<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>SIGD — Consulta Pública de Documentos</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }

        body {
            font-family: 'Segoe UI', sans-serif;
            background: #f0f4f8;
            color: #333;
        }

        /* ── HEADER ── */
        header {
            background: linear-gradient(135deg, #1a56db, #0e3a8a);
            color: white;
            padding: 20px 40px;
            display: flex;
            justify-content: space-between;
            align-items: center;
            box-shadow: 0 2px 10px rgba(0,0,0,0.2);
        }
        header h1 { font-size: 22px; }
        header p  { font-size: 13px; opacity: 0.85; margin-top: 4px; }

        /* ── CONTENIDO PRINCIPAL ── */
        main {
            max-width: 1100px;
            margin: 40px auto;
            padding: 0 20px;
        }

        /* ── AVISO DE ERROR DE BD ── */
        .db-error {
            background: #fef2f2;
            border: 1px solid #fca5a5;
            border-radius: 8px;
            padding: 16px 20px;
            margin-bottom: 24px;
            color: #991b1b;
            font-size: 14px;
        }
        .db-error strong { display: block; margin-bottom: 4px; }

        /* ── BARRA DE BÚSQUEDA ── */
        .search-section {
            background: white;
            padding: 25px 30px;
            border-radius: 12px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.08);
            margin-bottom: 30px;
        }
        .search-section h2 {
            font-size: 16px;
            color: #1a56db;
            margin-bottom: 15px;
        }
        .search-form {
            display: flex;
            gap: 12px;
        }
        .search-form input {
            flex: 1;
            padding: 12px 16px;
            border: 2px solid #e2e8f0;
            border-radius: 8px;
            font-size: 15px;
            transition: border-color 0.2s;
        }
        .search-form input:focus {
            outline: none;
            border-color: #1a56db;
        }
        .btn {
            padding: 12px 24px;
            border: none;
            border-radius: 8px;
            font-size: 14px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.2s;
            text-decoration: none;
            display: inline-flex;
            align-items: center;
            gap: 6px;
        }
        .btn-primary {
            background: #1a56db;
            color: white;
        }
        .btn-primary:hover { background: #1648c0; transform: translateY(-1px); }

        .btn-success {
            background: #0e9f6e;
            color: white;
        }
        .btn-success:hover { background: #057a55; transform: translateY(-1px); }

        .btn-report {
            background: #e3a008;
            color: white;
        }
        .btn-report:hover { background: #c27803; transform: translateY(-1px); }

        /* ── TABLA DE DOCUMENTOS ── */
        .table-section {
            background: white;
            border-radius: 12px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.08);
            overflow: hidden;
        }
        .table-header {
            padding: 20px 25px;
            display: flex;
            justify-content: space-between;
            align-items: center;
            border-bottom: 1px solid #e2e8f0;
        }
        .table-header h2 { font-size: 16px; color: #333; }

        table {
            width: 100%;
            border-collapse: collapse;
        }
        thead th {
            background: #f8fafc;
            padding: 14px 20px;
            text-align: left;
            font-size: 13px;
            color: #666;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            border-bottom: 2px solid #e2e8f0;
        }
        tbody td {
            padding: 14px 20px;
            border-bottom: 1px solid #f1f5f9;
            font-size: 14px;
        }
        tbody tr:hover { background: #f8fafc; }

        .badge {
            padding: 4px 12px;
            border-radius: 20px;
            font-size: 12px;
            font-weight: 600;
            background: #d1fae5;
            color: #065f46;
        }
        .badge-inactive {
            background: #f1f5f9;
            color: #64748b;
        }

        .no-results {
            text-align: center;
            padding: 50px;
            color: #888;
        }
        .no-results p { font-size: 16px; margin-top: 10px; }

        /* ── FOOTER ── */
        footer {
            text-align: center;
            padding: 30px;
            color: #888;
            font-size: 13px;
        }
    </style>
</head>
<body>

<header>
    <div>
        <h1>SIGD Empresarial</h1>
        <p>Sistema Integral de Gestión Documental — Consulta Pública</p>
    </div>
    <a href="?accion=reporte" class="btn btn-report">
        Generar Reporte PDF
    </a>
</header>

<main>
    <?php if ($dbError): ?>
    <div class="db-error">
        <strong>Error de conexión a la base de datos</strong>
        <?= htmlspecialchars($dbError) ?>
    </div>
    <?php endif; ?>

    <!-- Barra de búsqueda -->
    <div class="search-section">
        <h2>Buscar Documentos Vigentes</h2>
        <form class="search-form" method="GET" action="">
            <input
                type="text"
                name="q"
                placeholder="Escribe el nombre del documento..."
                value="<?= htmlspecialchars($busqueda) ?>"
            >
            <button type="submit" class="btn btn-primary">Buscar</button>
            <?php if ($busqueda): ?>
                <a href="?" class="btn" style="background:#e2e8f0; color:#333;">
                    Limpiar
                </a>
            <?php endif; ?>
        </form>
    </div>

    <!-- Tabla de documentos -->
    <div class="table-section">
        <div class="table-header">
            <h2>
                <?= $busqueda
                    ? "Resultados para: \"" . htmlspecialchars($busqueda) . "\""
                    : "Todos los Documentos Vigentes" ?>
            </h2>
            <span style="color:#888; font-size:13px;">
                <?= count($documentos) ?> documento(s) encontrado(s)
            </span>
        </div>

        <?php if (empty($documentos)): ?>
            <div class="no-results">
                <?php if (!$pdo): ?>
                    <p>Sin conexión a la base de datos.</p>
                <?php elseif ($busqueda): ?>
                    <p>No se encontraron documentos para "<?= htmlspecialchars($busqueda) ?>".</p>
                <?php else: ?>
                    <p>No hay documentos vigentes registrados.</p>
                <?php endif; ?>
            </div>
        <?php else: ?>
            <table>
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Título</th>
                        <th>Código Interno</th>
                        <th>Versión</th>
                        <th>Fecha Publicación</th>
                        <th>Estado</th>
                        <th>Acciones</th>
                    </tr>
                </thead>
                <tbody>
                    <?php foreach ($documentos as $doc):
                        $estadoTexto  = estatusTexto($doc['estatus']);
                        $badgeClass   = ($estadoTexto === 'Aprobado') ? 'badge' : 'badge badge-inactive';
                    ?>
                    <tr>
                        <td><?= (int)$doc['id_documento'] ?></td>
                        <td><strong><?= htmlspecialchars($doc['titulo']) ?></strong></td>
                        <td><?= htmlspecialchars($doc['codigo_interno']) ?></td>
                        <td>v<?= (int)$doc['version_actual'] ?></td>
                        <td><?= htmlspecialchars($doc['fecha_publicacion']) ?></td>
                        <td><span class="<?= $badgeClass ?>"><?= $estadoTexto ?></span></td>
                        <td>
                            <a href="?accion=descargar&id=<?= (int)$doc['id_documento'] ?>"
                               class="btn btn-success"
                               style="padding: 7px 14px; font-size: 13px;">
                                Descargar
                            </a>
                        </td>
                    </tr>
                    <?php endforeach; ?>
                </tbody>
            </table>
        <?php endif; ?>

        <?php if (isset($totalPaginas) && $totalPaginas > 1): ?>
        <div style="display:flex; justify-content:space-between; align-items:center; margin-top:20px; padding:0 4px;">
            <span style="color:#888; font-size:13px;">
                Página <?= $pagina ?> de <?= $totalPaginas ?>
                (<?= $totalDocs ?> documentos en total)
            </span>
            <div style="display:flex; gap:6px;">
                <?php if ($pagina > 1): ?>
                    <a href="?<?= http_build_query(array_merge($_GET, ['pagina' => $pagina - 1])) ?>"
                       class="btn" style="background:#e2e8f0; color:#333; padding:8px 16px;">
                        ← Anterior
                    </a>
                <?php endif; ?>

                <?php for ($i = 1; $i <= $totalPaginas; $i++): ?>
                    <a href="?<?= http_build_query(array_merge($_GET, ['pagina' => $i])) ?>"
                       class="btn"
                       style="padding:8px 14px; <?= $i === $pagina ? 'background:#1a56db; color:white;' : 'background:#e2e8f0; color:#333;' ?>">
                        <?= $i ?>
                    </a>
                <?php endfor; ?>

                <?php if ($pagina < $totalPaginas): ?>
                    <a href="?<?= http_build_query(array_merge($_GET, ['pagina' => $pagina + 1])) ?>"
                       class="btn" style="background:#e2e8f0; color:#333; padding:8px 16px;">
                        Siguiente →
                    </a>
                <?php endif; ?>
            </div>
        </div>
        <?php endif; ?>
    </div>
</main>

<footer>
    <p>SIGD Empresarial © 2024 — Módulo de Consulta Pública | PHP 8 + PostgreSQL</p>
</footer>

</body>
</html>
