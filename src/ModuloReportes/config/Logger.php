<?php
declare(strict_types=1);

namespace Config;

use Monolog\Level;
use Monolog\Logger as MonologLogger;
use Monolog\Handler\StreamHandler;
use Monolog\Formatter\JsonFormatter;

/**
 * Singleton que provee un logger Monolog con salida JSON estructurada a stderr.
 * Apache/PHP-FPM redirige stderr a `docker logs`, por lo que cada log
 * aparece en `docker logs app_reportes_php` como una línea JSON.
 *
 * Uso:  $logger = \Config\Logger::getInstance();
 *       $logger->info('evento_clave', ['campo' => 'valor']);
 */
class Logger
{
    private static ?MonologLogger $instance = null;

    private function __construct() {}
    private function __clone() {}

    public static function getInstance(): MonologLogger
    {
        if (self::$instance === null) {
            $level = match (strtoupper((string)(getenv('LOG_LEVEL') ?: 'INFO'))) {
                'DEBUG'              => Level::Debug,
                'NOTICE'             => Level::Notice,
                'WARNING', 'WARN'    => Level::Warning,
                'ERROR'              => Level::Error,
                'CRITICAL'           => Level::Critical,
                'ALERT'              => Level::Alert,
                'EMERGENCY'          => Level::Emergency,
                default              => Level::Info,
            };

            // StreamHandler a php://stderr: visible en docker logs sin tocar stdout
            $handler = new StreamHandler('php://stderr', $level);
            $handler->setFormatter(new JsonFormatter());

            $monolog = new MonologLogger('modulo-reportes');
            $monolog->pushHandler($handler);

            self::$instance = $monolog;
        }

        return self::$instance;
    }
}
