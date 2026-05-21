<?php
/**
 * ============================================================
 * SIGD Empresarial — Módulo de Reportes
 * Controlador del Dashboard de Estadísticas
 * ============================================================
 * Provee métricas y KPIs consultando la tabla documento_vigente
 * en PostgreSQL. Cada método retorna datos en JSON para ser
 * consumidos por Chart.js en el frontend del dashboard.
 * ============================================================
 */

declare(strict_types=1);

namespace Controllers;

use Config\Database;
use PDO;
use Exception;

class DashboardController
{
    private ?PDO $db;

    public function __construct()
    {
        $database  = new Database();
        $this->db  = $database->getConnection();
    }

    // ──────────────────────────────────────────────────────────
    // VISTA HTML: Renderiza el dashboard completo
    // GET /index.php?action=dashboard
    // ──────────────────────────────────────────────────────────
    public function mostrarDashboard(): void
    {
        header("Content-Type: text/html; charset=UTF-8");
        require_once __DIR__ . '/../views/dashboard.php';
    }

    // ──────────────────────────────────────────────────────────
    // API: Totales rápidos para las tarjetas KPI
    // GET /index.php?action=api_kpis
    // ──────────────────────────────────────────────────────────
    public function obtenerKpis(): void
    {
        try {
            // Total de documentos vigentes activos
            $stmt = $this->db->query("SELECT COUNT(*) AS total FROM documento_vigente WHERE estatus = true");
            $totalDocs = (int)$stmt->fetchColumn();

            // Total de departamentos con al menos un documento
            $stmt2 = $this->db->query("SELECT COUNT(DISTINCT id_departamento) AS total FROM documento_vigente WHERE estatus = true");
            $totalDeptos = (int)$stmt2->fetchColumn();

            // Última fecha de publicación
            $stmt3 = $this->db->query("SELECT MAX(fecha_publicacion) AS ultima FROM documento_vigente WHERE estatus = true");
            $ultimaFecha = $stmt3->fetchColumn() ?? 'Sin documentos';

            // Total de acuses registrados
            $totalAcuses = 0;
            try {
                $stmt4 = $this->db->query("SELECT COUNT(*) AS total FROM acuse_lectura");
                $totalAcuses = (int)$stmt4->fetchColumn();
            } catch (Exception) {
                // La tabla podría no existir aún
            }

            echo json_encode([
                'total_documentos'  => $totalDocs,
                'total_departamentos' => $totalDeptos,
                'ultima_publicacion' => $ultimaFecha,
                'total_acuses'      => $totalAcuses,
            ]);
        } catch (Exception $e) {
            http_response_code(500);
            echo json_encode(['error' => $e->getMessage()]);
        }
    }

    // ──────────────────────────────────────────────────────────
    // API: Documentos por departamento (para gráfica de barras)
    // GET /index.php?action=api_docs_por_depto
    // ──────────────────────────────────────────────────────────
    public function docsPorDepartamento(): void
    {
        try {
            $stmt = $this->db->query("
                SELECT id_departamento AS departamento, COUNT(*) AS total
                FROM documento_vigente
                WHERE estatus = true
                GROUP BY id_departamento
                ORDER BY total DESC
                LIMIT 10
            ");
            $rows = $stmt->fetchAll();
            echo json_encode($rows);
        } catch (Exception $e) {
            http_response_code(500);
            echo json_encode(['error' => $e->getMessage()]);
        }
    }

    // ──────────────────────────────────────────────────────────
    // API: Evolución mensual de publicaciones (gráfica de línea)
    // GET /index.php?action=api_evolucion
    // ──────────────────────────────────────────────────────────
    public function evolucionMensual(): void
    {
        try {
            $stmt = $this->db->query("
                SELECT
                    TO_CHAR(fecha_publicacion, 'YYYY-MM') AS mes,
                    COUNT(*)                               AS total
                FROM documento_vigente
                WHERE estatus = true
                  AND fecha_publicacion >= NOW() - INTERVAL '12 months'
                GROUP BY mes
                ORDER BY mes ASC
            ");
            $rows = $stmt->fetchAll();
            echo json_encode($rows);
        } catch (Exception $e) {
            http_response_code(500);
            echo json_encode(['error' => $e->getMessage()]);
        }
    }

    // ──────────────────────────────────────────────────────────
    // API: Últimos 10 documentos publicados (tabla de actividad)
    // GET /index.php?action=api_recientes
    // ──────────────────────────────────────────────────────────
    public function documentosRecientes(): void
    {
        try {
            $stmt = $this->db->query("
                SELECT
                    id_documento,
                    codigo_interno,
                    titulo,
                    version_actual,
                    TO_CHAR(fecha_publicacion, 'DD/MM/YYYY HH24:MI') AS fecha_formateada,
                    id_departamento
                FROM documento_vigente
                WHERE estatus = true
                ORDER BY fecha_publicacion DESC
                LIMIT 10
            ");
            $rows = $stmt->fetchAll();
            echo json_encode($rows);
        } catch (Exception $e) {
            http_response_code(500);
            echo json_encode(['error' => $e->getMessage()]);
        }
    }
}
