<?php
namespace Controllers;

use Models\Acuse;
use Config\Database;
use PDO;
use Exception;

class ReporteController {
    private Acuse $acuseModel;
    private ?PDO $db;

    public function __construct() {
        $this->acuseModel = new Acuse();
        $database = new Database();
        $this->db = $database->getConnection();
    }

    /**
     * Endpoint para registrar la lectura de un documento por parte de un operario
     * URL: POST /index.php?action=registrar_acuse
     */
    public function registrarAcuse(): void {
        if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
            http_response_code(405);
            echo json_encode(["status" => "error", "message" => "Método no permitido."]);
            return;
        }

        // Leer los datos enviados por el formulario o petición fetch
        $id_documento = $_POST['id_documento'] ?? null;
        $id_usuario = $_POST['id_usuario'] ?? null; // ID del operario firmado

        if (!$id_documento || !$id_usuario) {
            http_response_code(400);
            echo json_encode(["status" => "error", "message" => "Faltan parámetros obligatorios."]);
            return;
        }

        // Captura nativa de metadatos del entorno para auditoría de cumplimiento ISO
        $ip = $_SERVER['REMOTE_ADDR'] ?? '0.0.0.0';
        // En caso de estar detrás de un proxy de Docker, intentar capturar la IP real
        if (!empty($_SERVER['HTTP_X_FORWARDED_FOR'])) {
            $ip = explode(',', $_SERVER['HTTP_X_FORWARDED_FOR'])[0];
        }
        $userAgent = $_SERVER['HTTP_USER_AGENT'] ?? 'Desconocido';

        try {
            // El usuario creador del registro de auditoría aquí es el mismo operario
            $exito = $this->acuseModel->registrarLectura(
                (int)$id_documento, 
                (int)$id_usuario, 
                $ip, 
                $userAgent, 
                (int)$id_usuario
            );

            if ($exito) {
                http_response_code(201);
                echo json_encode([
                    "status" => "success",
                    "message" => "Acuse de lectura registrado correctamente. Métricas actualizadas."
                ]);
            } else {
                throw new Exception("No se pudo completar la inserción.");
            }
        } catch (Exception $e) {
            http_response_code(500);
            echo json_encode(["status" => "error", "message" => $e->getMessage()]);
        }
    }

    /**
     * Endpoint para obtener el porcentaje de cumplimiento por departamento
     * Invoca la función/procedimiento almacenado de PostgreSQL
     * URL: GET /index.php?action=clima_cumplimiento&id_depto=X
     */
    public function obtenerClimaCumplimiento(): void {
        $id_depto = $_GET['id_depto'] ?? null;

        if (!$id_depto) {
            http_response_code(400);
            echo json_encode(["status" => "error", "message" => "ID de departamento requerido."]);
            return;
        }

        try {
            // Invocar la función almacenada sp_reporte_cumplimiento_depto que creamos en Postgres
            $query = "SELECT * FROM sp_reporte_cumplimiento_depto(:id_depto)";
            $stmt = $this->db->prepare($query);
            $stmt->bindValue(':id_depto', (int)$id_depto, PDO::PARAM_INT);
            $stmt->execute();
            
            $resultado = $stmt->fetch();

            echo json_encode([
                "status" => "success",
                "data" => $resultado ?: ["mensaje" => "Sin datos para este departamento"]
            ]);
        } catch (Exception $e) {
            http_response_code(500);
            echo json_encode(["status" => "error", "message" => "Error al ejecutar reporte en Postgres: " . $e->getMessage()]);
        }
    }
}