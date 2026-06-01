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
use Config\Logger;
use PDO;
use Exception;

class SyncController
{
    private ?PDO $db;

    /**
     * @param PDO|null $db  Inyección de dependencia para tests (null = usa Database::getConnection())
     */
    public function __construct(?PDO $db = null)
    {
        if ($db !== null) {
            $this->db = $db;
        } else {
            $database = new Database();
            $this->db = $database->getConnection();
        }
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
        $data  = $this->leerInput();
        $error = $this->validarPayload($data);

        if ($error) {
            http_response_code(400);
            echo json_encode(['status' => 'error', 'message' => $error]);
            return;
        }

        try {
            $this->upsertDocumento($data);
            $this->registrarEventoSync($data['id_documento'], 'SYNC_OK', null);

            Logger::getInstance()->info('document_synced', [
                'id_documento'   => $data['id_documento'],
                'codigo_interno' => $data['codigo_interno'] ?? null,
                'version_actual' => $data['version_actual'] ?? null,
            ]);

            http_response_code(200);
            echo json_encode([
                'status'  => 'success',
                'message' => 'Documento sincronizado correctamente.',
                'id'      => $data['id_documento'],
            ]);
        } catch (Exception $e) {
            $this->registrarEventoSync($data['id_documento'] ?? 0, 'SYNC_ERROR', $e->getMessage());

            Logger::getInstance()->error('document_sync_failed', [
                'id_documento' => $data['id_documento'] ?? null,
                'error'        => $e->getMessage(),
                'file'         => $e->getFile(),
                'line'         => $e->getLine(),
            ]);

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
        $lote = $this->leerInput();

        if (!is_array($lote) || count($lote) === 0) {
            http_response_code(400);
            echo json_encode(['status' => 'error', 'message' => 'El cuerpo debe ser un array JSON con al menos un documento.']);
            return;
        }

        $resultados = [];
        $exitosos   = 0;
        $fallidos   = 0;

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

            Logger::getInstance()->info('batch_synced', [
                'sincronizados' => $exitosos,
                'omitidos'      => $fallidos,
                'total'         => count($lote),
            ]);

            http_response_code(200);
            echo json_encode([
                'status'        => 'success',
                'sincronizados' => $exitosos,
                'omitidos'      => $fallidos,
                'detalle'       => $resultados,
            ]);
        } catch (Exception $e) {
            $this->db->rollBack();

            Logger::getInstance()->error('batch_sync_failed', [
                'error' => $e->getMessage(),
                'file'  => $e->getFile(),
                'line'  => $e->getLine(),
            ]);

            http_response_code(500);
            echo json_encode([
                'status'  => 'error',
                'message' => 'Transacción revertida. Error: ' . $e->getMessage(),
            ]);
        }
    }

    // ──────────────────────────────────────────────────────────
    // MÉTODOS PROTEGIDOS (sobreescribibles en tests)
    // ──────────────────────────────────────────────────────────

    /**
     * Lee y decodifica el cuerpo JSON de la petición HTTP.
     * Extraído para permitir su mock en tests sin necesitar php://input.
     */
    protected function leerInput(): ?array
    {
        return json_decode(file_get_contents('php://input'), true);
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
        $requeridos = ['id_documento', 'id_empresa', 'codigo_interno', 'titulo', 'id_tipo', 'id_departamento',
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
     *
     * @throws Exception Si la ejecución de la sentencia falla.
     */
    private function upsertDocumento(array $data): void
    {
        $query = "
            INSERT INTO documento_vigente (
                id_documento, id_empresa, codigo_interno, titulo, id_tipo, id_departamento,
                version_actual, fecha_publicacion, ruta_archivo_descarga, hash_verificacion,
                estatus, fecha_creacion, id_usuario_creacion
            )
            VALUES (
                :id, :id_empresa, :codigo, :titulo, :id_tipo, :id_depto,
                :version, :fecha_pub, :ruta, :hash_v,
                true, CURRENT_TIMESTAMP, :usuario_creador
            )
            ON CONFLICT (id_documento)
            DO UPDATE SET
                id_empresa              = EXCLUDED.id_empresa,
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
        $stmt->bindValue(':id_empresa',      (int)$data['id_empresa'],           PDO::PARAM_INT);
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
     * Registra cada intento de sincronización en la tabla bitacora_sync.
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
