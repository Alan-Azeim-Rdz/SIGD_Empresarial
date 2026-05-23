<?php
declare(strict_types=1);

namespace Tests;

use Config\Logger;
use Monolog\Logger as MonologLogger;
use Monolog\Level;
use PHPUnit\Framework\TestCase;

class LoggerTest extends TestCase
{
    /** Resetea el singleton entre tests para que cada caso empiece limpio. */
    private function resetSingleton(): void
    {
        $reflection = new \ReflectionClass(Logger::class);
        $property   = $reflection->getProperty('instance');
        $property->setAccessible(true);
        $property->setValue(null, null);
    }

    protected function setUp(): void
    {
        $this->resetSingleton();
    }

    protected function tearDown(): void
    {
        $this->resetSingleton();
    }

    public function testGetInstanceDevuelveMonologLogger(): void
    {
        $logger = Logger::getInstance();

        $this->assertInstanceOf(MonologLogger::class, $logger);
    }

    public function testGetInstanceEsSingleton(): void
    {
        $primera  = Logger::getInstance();
        $segunda  = Logger::getInstance();

        $this->assertSame($primera, $segunda, 'getInstance() debe devolver siempre la misma instancia');
    }

    public function testChannelEsModuloReportes(): void
    {
        $logger = Logger::getInstance();

        $this->assertSame('modulo-reportes', $logger->getName());
    }

    public function testRespetaLogLevelDelEnvironment(): void
    {
        // Forzamos ERROR antes de crear la instancia
        putenv('LOG_LEVEL=ERROR');
        $logger = Logger::getInstance();

        $handlers = $logger->getHandlers();
        $this->assertNotEmpty($handlers, 'Debe haber al menos un handler');

        $handler = $handlers[0];
        // Monolog\Level::Error = 400
        $this->assertSame(Level::Error, $handler->getLevel());
    }

    public function testLogLevelDebugSeAplica(): void
    {
        putenv('LOG_LEVEL=DEBUG');
        $logger   = Logger::getInstance();
        $handlers = $logger->getHandlers();

        $this->assertSame(Level::Debug, $handlers[0]->getLevel());
    }
}
