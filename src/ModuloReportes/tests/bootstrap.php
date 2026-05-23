<?php
/**
 * Bootstrap de PHPUnit para ModuloReportes.
 * Define variables de entorno ficticias ANTES de cargar cualquier clase,
 * de modo que ningún test requiera una conexión real a PostgreSQL.
 */

// Credenciales de BD ficticias (jamás se usarán para conectar)
putenv('DB_HOST=test_host');
putenv('DB_PORT=5432');
putenv('DB_USER=test_user');
putenv('DB_PASS=test_pass');
putenv('DB_NAME=test_db');

// Clave de API ficticia para el endpoint de sincronización
putenv('SYNC_API_KEY=test_api_key_ficticio_para_pruebas');

// Silencia los logs de Monolog durante los tests
putenv('LOG_LEVEL=ERROR');

// Env de PHP para tests
putenv('APP_ENV=testing');

require_once __DIR__ . '/../vendor/autoload.php';
