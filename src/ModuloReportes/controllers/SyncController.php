<?php
/**
 * ============================================================
 * SIGD Empresarial — Módulo de Reportes
 * Controlador de Sincronización con el Módulo Central (.NET)
 * ============================================================
 * Recibe los metadatos de documentos publicados/actualizados
 * y los persiste en PostgreSQL usando la técnica UPSERT nativa,
 * garantizando idempotencia y consistencia eventual entre módulos.
 * ============================================================
 */

declare(strict_types=1);

namespace Controllers;

use Config\Database;
use PDO;
use Exception;

class SyncController
{
    private ?PDO $db;

    public function __construct()
    {
        $database    = new Database();
        $this->db    = $database->getConnection();
    }

    // ──────────────────────────────────────────────────────────
    // SINCRONIZACIÓN INDIVIDUAL
    // POST /api/sync.php?action=sincronizar
    // Body JSON: { ...campos del documento... }
    // ──────────────────────────────────────────────────────────
    /**
     * Recibe un único documento desde el Módulo Central y lo
     * inserta o actualiza (UPSERT) en PostgreSQL.
     * La idempotencia está garantizada por ON CONFLICT en id_documento.
     */
    public function sincronizarDocumento(): void
    {
        $jsonInput = file_get_contents('php://input');
        $data      = json_decode($jsonInput, true);

        $error = $this->validarPayload($data);
        if ($error) {
            http_response_code(400);
            echo json_encode(['status' => 'error', 'message' => $error]);
            return;
        }

        try {
            $this->upsertDocumento($data);

            // Registrar el evento de sincronización en la bitácora local
            $this->registrarEventoSync($data['id_documento'], 'SYNC_OK', null);

            http_response_code(200);
            echo json_encode([
                'status'  => 'success',
                'message' => 'Documento sincronizado correctamente.',
                'id'      => $data['id_documento'],
            ]);
        } catch (Exception $e) {
            $this->registrarEventoSync($data['id_documento'] ?? 0, 'SYNC_ERROR', $e->getMessage());
            http_response_code(500);
            echo json_encode([
                'status'  => 'error',
                'message' => 'Error al sincronizar documento: ' . $e->getMessage(),
            ]);
        }
    }

    // ──────────────────────────────────────────────────────────
    // SINCRONIZACIÓN EN LOTE
    // POST /api/sync.php?action=sincronizar_batch
    // Body JSON: [ {...doc1...}, {...doc2...}, ... ]
    // ──────────────────────────────────────────────────────────
    /**
     * Procesa un array de documentos dentro de una transacción atómica.
     * Si un documento falla la validación se omite; si la DB falla,
     * se hace rollback de toda la operación.
     */
    public function sincronizarBatch(): void
    {
        $jsonInput = file_get_contents('php://input');
        $lote      = json_decode($jsonInput, true);

        if (!is_array($lote) || count($lote) === 0) {
            http_response_code(400);
            echo json_encode(['status' => 'error', 'message' => 'El cuerpo debe ser un array JSON con al menos un documento.']);
            return;
        }

        $resultados = [];
        $exitosos   = 0;
        $fallidos   = 0;

        // Transacción atómica: si el driver falla en algún UPSERT, todo revierte
        $this->db->beginTransaction();

        try {
            foreach ($lote as $index => $data) {
                $error = $this->validarPayload($data);
                if ($error) {
                    $resultados[] = [
                        'index'   => $index,
                        'id'      => $data['id_documento'] ?? null,
                        'status'  => 'omitido',
                        'message' => $error,
                    ];
                    $fallidos++;
                    continue;
                }

                $this->upsertDocumento($data);
                $resultados[] = [
                    'index'  => $index,
                    'id'     => $data['id_documento'],
                    'status' => 'sincronizado',
                ];
                $exitosos++;
            }

            $this->db->commit();

            http_response_code(200);
            echo json_encode([
                'status'      => 'success',
                'sincronizados' => $exitosos,
                'omitidos'    => $fallidos,
                'detalle'     => $resultados,
            ]);
        } catch (Exception $e) {
            $this->db->rollBack();
            http_response_code(500);
            echo json_encode([
                'status'  => 'error',
                'message' => 'Transacción revertida. Error: ' . $e->getMessage(),
            ]);
        }
    }

    // ──────────────────────────────────────────────────────────
    // MÉTODOS PRIVADOS INTERNOS
    // ──────────────────────────────────────────────────────────

    /**
     * Valida los campos obligatorios del payload entrante.
     * Devuelve un string con el mensaje de error o null si es válido.
     */
    private function validarPayload(?array $data): ?string
    {
        if (empty($data)) {
            return 'Payload JSON vacío o mal formado.';
        }
        $requeridos = ['id_documento', 'codigo_interno', 'titulo', 'id_tipo', 'id_departamento',
                       'version_actual', 'fecha_publicacion', 'ruta_archivo_descarga', 'id_usuario_creacion'];

        foreach ($requeridos as $campo) {
            if (!isset($data[$campo]) || $data[$campo] === '' || $data[$campo] === null) {
                return "Campo obligatorio ausente o vacío: '{$campo}'.";
            }
        }
        return null;
    }

    /**
     * Ejecuta el UPSERT (INSERT … ON CONFLICT DO UPDATE) de un documento
     * en la tabla documento_vigente de PostgreSQL.
     * Esta operación es idempotente: envíar el mismo documento dos veces
     * produce el mismo estado final en la base de datos.
     *
     * @throws Exception Si la ejecución de la sentencia falla.
     */
    private function upsertDocumento(array $data): void
    {
        $query = "
            INSERT INTO documento_vigente (
                id_documento, codigo_interno, titulo, id_tipo, id_departamento,
                version_actual, fecha_publicacion, ruta_archivo_descarga, hash_verificacion,
                estatus, fecha_creacion, id_usuario_creacion
            )
            VALUES (
                :id, :codigo, :titulo, :id_tipo, :id_depto,
                :version, :fecha_pub, :ruta, :hash_v,
                true, CURRENT_TIMESTAMP, :usuario_creador
            )
            ON CONFLICT (id_documento)
            DO UPDATE SET
                titulo                  = EXCLUDED.titulo,
                codigo_interno          = EXCLUDED.codigo_interno,
                id_tipo                 = EXCLUDED.id_tipo,
                id_departamento         = EXCLUDED.id_departamento,
                version_actual          = EXCLUDED.version_actual,
                fecha_publicacion       = EXCLUDED.fecha_publicacion,
                ruta_archivo_descarga   = EXCLUDED.ruta_archivo_descarga,
                hash_verificacion       = EXCLUDED.hash_verificacion,
                fecha_modificacion      = CURRENT_TIMESTAMP,
                id_usuario_modificacion = :usuario_creador
        ";

        $stmt = $this->db->prepare($query);

        $stmt->bindValue(':id',              (int)$data['id_documento'],         PDO::PARAM_INT);
        $stmt->bindValue(':codigo',          $data['codigo_interno'],             PDO::PARAM_STR);
        $stmt->bindValue(':titulo',          $data['titulo'],                     PDO::PARAM_STR);
        $stmt->bindValue(':id_tipo',         (int)$data['id_tipo'],               PDO::PARAM_INT);
        $stmt->bindValue(':id_depto',        (int)$data['id_departamento'],       PDO::PARAM_INT);
        $stmt->bindValue(':version',         (int)$data['version_actual'],        PDO::PARAM_INT);
        $stmt->bindValue(':fecha_pub',       $data['fecha_publicacion'],          PDO::PARAM_STR);
        $stmt->bindValue(':ruta',            $data['ruta_archivo_descarga'],      PDO::PARAM_STR);
        $stmt->bindValue(':hash_v',          $data['hash_verificacion'] ?? null,  PDO::PARAM_STR);
        $stmt->bindValue(':usuario_creador', (int)$data['id_usuario_creacion'],   PDO::PARAM_INT);

        if (!$stmt->execute()) {
            throw new Exception('La ejecución del UPSERT no retornó éxito.');
        }
    }

    /**
     * Registra cada intento de sincronización en la tabla bitacora_sync
     * para trazabilidad y diagnóstico de errores de integración.
     * Si la tabla no existe o el INSERT falla, no interrumpe el flujo principal.
     */
    private function registrarEventoSync(int $idDocumento, string $estado, ?string $mensajeError): void
    {
        try {
            $query = "
                INSERT INTO bitacora_sync (id_documento, estado, mensaje_error, fecha_evento)
                VALUES (:id_doc, :estado, :error, CURRENT_TIMESTAMP)
            ";
            $stmt = $this->db->prepare($query);
            $stmt->bindValue(':id_doc', $idDocumento,   PDO::PARAM_INT);
            $stmt->bindValue(':estado', $estado,         PDO::PARAM_STR);
            $stmt->bindValue(':error',  $mensajeError,   PDO::PARAM_STR);
            $stmt->execute();
        } catch (Exception) {
            // Silencioso: la bitácora es secundaria; no debe interrumpir la sincronización
        }
    }
}