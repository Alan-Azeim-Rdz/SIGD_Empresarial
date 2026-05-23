<?php
namespace Models;

use Config\Database;
use PDO;
use Exception;

class Acuse {
    private ?PDO $db;
    private string $table_name = "acuse_lectura";

    /**
     * @param PDO|null $db  Inyección de dependencia para tests (null = usa Database::getConnection())
     */
    public function __construct(?PDO $db = null) {
        if ($db !== null) {
            $this->db = $db;
        } else {
            $database = new Database();
            $this->db = $database->getConnection();
        }
    }

    /**
     * Registra una firma digital de lectura de un operario en planta.
     * Cumple con la trazabilidad exigida por las normas ISO de calidad.
     */
    public function registrarLectura(int $id_documento, int $id_usuario, string $ip, string $user_agent, int $usuario_creador): bool {
        try {
            $query = "INSERT INTO " . $this->table_name . " (
                        id_documento, id_usuario, fecha_lectura, direccion_ip, dispositivo_info,
                        estatus, fecha_creacion, id_usuario_creacion
                      )
                      VALUES (
                        :id_doc, :id_usr, CURRENT_TIMESTAMP, :ip, :user_agent,
                        true, CURRENT_TIMESTAMP, :creador
                      )";

            $stmt = $this->db->prepare($query);

            $stmt->bindValue(':id_doc',     $id_documento,   PDO::PARAM_INT);
            $stmt->bindValue(':id_usr',     $id_usuario,     PDO::PARAM_INT);
            $stmt->bindValue(':ip',         $ip,             PDO::PARAM_STR);
            $stmt->bindValue(':user_agent', $user_agent,     PDO::PARAM_STR);
            $stmt->bindValue(':creador',    $usuario_creador, PDO::PARAM_INT);

            if ($stmt->execute()) {
                return true;
            }
            return false;

        } catch (Exception $e) {
            throw new Exception("Error al registrar acuse en PostgreSQL: " . $e->getMessage());
        }
    }

    /**
     * Obtiene el historial de lecturas de un documento específico (Auditoría de Planta).
     */
    public function obtenerLecturasPorDocumento(int $id_documento): array {
        try {
            $query = "SELECT a.id_acuse, a.fecha_lectura, a.direccion_ip, a.dispositivo_info,
                             u.nombre, u.apellido_p, u.correo, d.nombre as departamento
                      FROM " . $this->table_name . " a
                      INNER JOIN usuario u ON a.id_usuario = u.id_usuario
                      INNER JOIN departamento d ON u.id_departamento = d.id_departamento
                      WHERE a.id_documento = :id_doc AND a.estatus = true
                      ORDER BY a.fecha_lectura DESC";

            $stmt = $this->db->prepare($query);
            $stmt->bindValue(':id_doc', $id_documento, PDO::PARAM_INT);
            $stmt->execute();

            return $stmt->fetchAll();
        } catch (Exception $e) {
            throw new Exception("Error al consultar auditoría de lecturas: " . $e->getMessage());
        }
    }
}
