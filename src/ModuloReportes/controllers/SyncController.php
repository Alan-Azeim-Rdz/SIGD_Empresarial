<?php
namespace Controllers;

use Config\Database;
use PDO;
use Exception;

class SyncController {
    private ?PDO $db;

    public function __construct() {
        $database = new Database();
        $this->db = $database->getConnection();
    }

    /**
     * Endpoint: POST /index.php?action=sincronizar
     * Recibe los metadatos del documento desde .NET Core 10
     */
    public function sincronizarDocumento(): void {
        // Asegurar que solo se procesen peticiones POST
        if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
            http_response_code(405);
            echo json_encode(["status" => "error", "message" => "Método no permitido. Debe ser POST."]);
            return;
        }

        // Capturar el cuerpo JSON crudo enviado por el HttpClient de .NET
        $jsonInput = file_get_contents('php://input');
        $data = json_decode($jsonInput, true);

        // Validación estructural obligatoria para cumplir la integridad relacional
        if (!$data || empty($data['id_documento']) || empty($data['codigo_interno']) || empty($data['titulo'])) {
            http_response_code(400);
            echo json_encode(["status" => "error", "message" => "Payload JSON inválido o faltan campos obligatorios (id, código, título)."]);
            return;
        }

        try {
            // Consulta SQL con la técnica 'ON CONFLICT' (Upsert nativo de PostgreSQL)
            // Inserta el documento nuevo o actualiza su versión y metadatos si ya existía
            $query = "INSERT INTO documento_vigente (
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
                        titulo = EXCLUDED.titulo,
                        version_actual = EXCLUDED.version_actual,
                        ruta_archivo_descarga = EXCLUDED.ruta_archivo_descarga,
                        hash_verificacion = EXCLUDED.hash_verificacion,
                        fecha_modificacion = CURRENT_TIMESTAMP,
                        id_usuario_modificacion = :usuario_creador;";

            $stmt = $this->db->prepare($query);

            // Sanitización estricta mediante parámetros vinculados para mitigar SQL Injection
            $stmt->bindValue(':id', (int)$data['id_documento'], PDO::PARAM_INT);
            $stmt->bindValue(':codigo', $data['codigo_interno'], PDO::PARAM_STR);
            $stmt->bindValue(':titulo', $data['titulo'], PDO::PARAM_STR);
            $stmt->bindValue(':id_tipo', (int)$data['id_tipo'], PDO::PARAM_INT);
            $stmt->bindValue(':id_depto', (int)$data['id_departamento'], PDO::PARAM_INT);
            $stmt->bindValue(':version', (int)$data['version_actual'], PDO::PARAM_INT);
            $stmt->bindValue(':fecha_pub', $data['fecha_publicacion'], PDO::PARAM_STR);
            $stmt->bindValue(':ruta', $data['ruta_archivo_descarga'], PDO::PARAM_STR);
            $stmt->bindValue(':hash_v', $data['hash_verificacion'] ?? null, PDO::PARAM_STR);
            $stmt->bindValue(':usuario_creador', (int)$data['id_usuario_creacion'], PDO::PARAM_INT);

            $stmt->execute();

            // Responder con éxito rotundo al emisor (.NET Core)
            http_response_code(200);
            echo json_encode([
                "status" => "success",
                "message" => "Documento sincronizado y publicado con éxito en PostgreSQL (ID: " . $data['id_documento'] . ")"
            ]);

        } catch (Exception $e) {
            // En caso de falla relacional, devolvemos un error interno
            http_response_code(500);
            echo json_encode([
                "status" => "error",
                "message" => "Fallo crítico al insertar en PostgreSQL: " . $e->getMessage()
            ]);
        }
    }
}