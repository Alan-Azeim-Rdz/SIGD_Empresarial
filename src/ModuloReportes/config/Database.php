<?php
namespace Config;

use PDO;
use PDOException;

class Database {
    private ?PDO $conn = null;

    /** Almacena el mensaje de error si la conexión falla (útil en tests y para diagnóstico). */
    public ?string $dbError = null;

    public function getConnection(): ?PDO {
        $host     = getenv('DB_HOST') ?: 'postgres';
        $port     = getenv('DB_PORT') ?: '5432';
        $db_name  = getenv('DB_NAME');
        $username = getenv('DB_USER');
        $password = getenv('DB_PASS');

        $dsn = "pgsql:host={$host};port={$port};dbname={$db_name};";

        try {
            if ($this->conn === null) {
                $this->conn = new PDO($dsn, $username, $password, [
                    PDO::ATTR_ERRMODE            => PDO::ERRMODE_EXCEPTION,
                    PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
                    PDO::ATTR_EMULATE_PREPARES   => false,
                ]);
            }
        } catch (PDOException $exception) {
            $this->dbError = $exception->getMessage();

            Logger::getInstance()->error('db_connection_failed', [
                'host'  => $host,
                'port'  => $port,
                'db'    => $db_name,
                'error' => $exception->getMessage(),
            ]);

            // Responde con JSON de error y termina la ejecución del proceso PHP
            // (comportamiento crítico de producción: sin conexión a DB no hay servicio).
            die(json_encode([
                'status'  => 'error',
                'message' => 'Fallo crítico en la conexión a PostgreSQL: ' . $exception->getMessage()
            ]));
        }

        return $this->conn;
    }
}
