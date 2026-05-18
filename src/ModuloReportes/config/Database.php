<?php
namespace Config;

use PDO;
use PDOException;

class Database {
    private ?PDO $conn = null;

    public function getConnection(): ?PDO {
        // Extraer credenciales directamente de las variables de entorno inyectadas por Docker
        $host = getenv('DB_HOST') ?: 'postgres';
        $port = getenv('DB_PORT') ?: '5432';
        $db_name = getenv('DB_NAME');
        $username = getenv('DB_USER');
        $password = getenv('DB_PASS');

        // Cadena de conexión (DSN) para PostgreSQL
        $dsn = "pgsql:host={$host};port={$port};dbname={$db_name};";

        try {
            if ($this->conn === null) {
                $this->conn = new PDO($dsn, $username, $password, [
                    PDO::ATTR_ERRMODE            => PDO::ERRMODE_EXCEPTION, // Reportar errores como excepciones
                    PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,       // Devolver arreglos asociativos por defecto
                    PDO::ATTR_EMULATE_PREPARES   => false,                  // Usar preparaciones nativas de PostgreSQL
                ]);
            }
        } catch (PDOException $exception) {
            // En entorno de desarrollo imprimimos el error; en producción se debería registrar en un log oculto
            die(json_encode([
                "status" => "error", 
                "message" => "Fallo crítico en la conexión a PostgreSQL: " . $exception->getMessage()
            ]));
        }

        return $this->conn;
    }
}