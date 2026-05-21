<?php
// ═══════════════════════════════════════════════════════
// MÓDULO DE CONSULTA PÚBLICA — PHP 8 + PostgreSQL
// Proyecto: SIGD Empresarial
// Autor: Josue J.A.V.
// ═══════════════════════════════════════════════════════

// Cargamos las dependencias instaladas con Composer
require_once __DIR__ . '/vendor/autoload.php';

use Dompdf\Dompdf;
use Dompdf\Options;

// ── 1. CONEXIÓN A POSTGRESQL ──────────────────────────
// Tomamos los datos de variables de entorno (más seguro)
// Si no hay variable, usamos valores por defecto
$host     = getenv('POSTGRES_HOST')     ?: 'localhost';
$port     = getenv('POSTGRES_PORT')     ?: '5432';
$dbname   = getenv('POSTGRES_DB')       ?: 'sigd_consulta';
$user     = getenv('POSTGRES_USER')     ?: 'postgres';
$password = getenv('POSTGRES_PASSWORD') ?: 'postgres';

// Intentamos conectar a PostgreSQL
try {
    $pdo = new PDO(
        "pgsql:host=$host;port=$port;dbname=$dbname",
        $user,
        $password,
        [PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION]
    );
} catch (PDOException $e) {
    // Si no hay conexión, seguimos con $pdo = null
    // El sistema mostrará datos de ejemplo
    $pdo = null;
}

// ── 2. LÓGICA DE RUTAS ────────────────────────────────
// Leemos qué acción pidió el usuario desde la URL
// Ejemplo: index.php?accion=reporte
$accion = $_GET['accion'] ?? 'buscar';

// ── 3. ACCIÓN: GENERAR REPORTE PDF ───────────────────
if ($accion === 'reporte') {
    // Obtenemos documentos aprobados para el reporte
    $documentos = [];
    if ($pdo) {
        $stmt = $pdo->query("SELECT * FROM DocumentosVigentes ORDER BY titulo");
        $documentos = $stmt->fetchAll(PDO::FETCH_ASSOC);
    } else {
        // Datos de ejemplo si no hay BD
        $documentos = [
            ['id' => 1, 'titulo' => 'Manual de Calidad ISO 9001',
             'version' => '2.0', 'fecha_aprobacion' => '2024-01-15', 'estado' => 'Aprobado'],
            ['id' => 2, 'titulo' => 'Procedimiento de Auditoría',
             'version' => '1.5', 'fecha_aprobacion' => '2024-02-20', 'estado' => 'Aprobado'],
            ['id' => 3, 'titulo' => 'Política de Calidad',
             'version' => '3.0', 'fecha_aprobacion' => '2024-03-10', 'estado' => 'Aprobado'],
        ];
    }

    // Construimos el HTML del reporte
    $fecha_hoy = date('d/m/Y H:i');
    $total     = count($documentos);
    $filas     = '';

    foreach ($documentos as $doc) {
        $filas .= "
        <tr>
            <td>{$doc['id']}</td>
            <td>{$doc['titulo']}</td>
            <td>{$doc['version']}</td>
            <td>{$doc['fecha_aprobacion']}</td>
            <td><span style='color: green; font-weight: bold;'>{$doc['estado']}</span></td>
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
        <h1>📋 Reporte de Cumplimiento de Normativas</h1>
        <h3>Sistema Integral de Gestión Documental — SIGD Empresarial</h3>
        <p>Fecha de generación: <strong>$fecha_hoy</strong></p>
        <table>
            <thead>
                <tr>
                    <th>ID</th>
                    <th>Título del Documento</th>
                    <th>Versión</th>
                    <th>Fecha Aprobación</th>
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

    // Generamos el PDF con dompdf
    $options = new Options();
    $options->set('isHtml5ParserEnabled', true);

    $dompdf = new Dompdf($options);
    $dompdf->loadHtml($html);
    $dompdf->setPaper('A4', 'landscape');
    $dompdf->render();

    // Descargamos el PDF automáticamente
    $dompdf->stream("Reporte_Cumplimiento_$fecha_hoy.pdf", [
        "Attachment" => true
    ]);
    exit;
}

// ── 4. ACCIÓN: DESCARGAR ARCHIVO ─────────────────────
if ($accion === 'descargar') {
    $id = $_GET['id'] ?? null;

    if (!$id) {
        die('ID de documento no especificado.');
    }

    // En producción buscarías la ruta real en la BD
    // Por ahora simulamos la descarga
    header('Content-Type: application/octet-stream');
    header("Content-Disposition: attachment; filename=\"Documento_$id.pdf\"");
    echo "Contenido del documento $id";
    exit;
}

// ── 5. ACCIÓN: BUSCAR DOCUMENTOS (página principal) ──
$busqueda    = $_GET['q'] ?? '';
$documentos  = [];

if ($pdo && $busqueda !== '') {
    // Búsqueda en PostgreSQL con ILIKE (no distingue mayúsculas)
    $stmt = $pdo->prepare(
        "SELECT * FROM DocumentosVigentes
         WHERE titulo ILIKE :q
         ORDER BY titulo"
    );
    $stmt->execute([':q' => "%$busqueda%"]);
    $documentos = $stmt->fetchAll(PDO::FETCH_ASSOC);

} elseif ($busqueda === '') {
    // Sin búsqueda — mostrar todos
    if ($pdo) {
        $stmt = $pdo->query("SELECT * FROM DocumentosVigentes ORDER BY titulo");
        $documentos = $stmt->fetchAll(PDO::FETCH_ASSOC);
    } else {
        // Datos de ejemplo si no hay BD conectada
        $documentos = [
            ['id' => 1, 'titulo' => 'Manual de Calidad ISO 9001',
             'version' => '2.0', 'fecha_aprobacion' => '2024-01-15', 'estado' => 'Aprobado'],
            ['id' => 2, 'titulo' => 'Procedimiento de Auditoría',
             'version' => '1.5', 'fecha_aprobacion' => '2024-02-20', 'estado' => 'Aprobado'],
            ['id' => 3, 'titulo' => 'Política de Calidad',
             'version' => '3.0', 'fecha_aprobacion' => '2024-03-10', 'estado' => 'Aprobado'],
        ];
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
        <h1>📋 SIGD Empresarial</h1>
        <p>Sistema Integral de Gestión Documental — Consulta Pública</p>
    </div>
    <a href="?accion=reporte" class="btn btn-report">
        📄 Generar Reporte PDF
    </a>
</header>

<main>
    <!-- Barra de búsqueda -->
    <div class="search-section">
        <h2>🔍 Buscar Documentos Vigentes</h2>
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
                    ✕ Limpiar
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
                <p>😕 No se encontraron documentos.</p>
                <p>Intenta con otro término de búsqueda.</p>
            </div>
        <?php else: ?>
            <table>
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Título</th>
                        <th>Versión</th>
                        <th>Fecha Aprobación</th>
                        <th>Estado</th>
                        <th>Acciones</th>
                    </tr>
                </thead>
                <tbody>
                    <?php foreach ($documentos as $doc): ?>
                    <tr>
                        <td><?= $doc['id'] ?></td>
                        <td><strong><?= htmlspecialchars($doc['titulo']) ?></strong></td>
                        <td>v<?= $doc['version'] ?></td>
                        <td><?= $doc['fecha_aprobacion'] ?></td>
                        <td><span class="badge">✅ <?= $doc['estado'] ?></span></td>
                        <td>
                            <a href="?accion=descargar&id=<?= $doc['id'] ?>"
                               class="btn btn-success"
                               style="padding: 7px 14px; font-size: 13px;">
                                📥 Descargar
                            </a>
                        </td>
                    </tr>
                    <?php endforeach; ?>
                </tbody>
            </table>
        <?php endif; ?>
    </div>
</main>

<footer>
    <p>SIGD Empresarial © 2024 — Módulo de Consulta Pública | PHP 8 + PostgreSQL</p>
</footer>

</body>
</html>