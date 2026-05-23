<?php
declare(strict_types=1);

namespace Tests;

use Models\Acuse;
use PDO;
use PDOStatement;
use PHPUnit\Framework\TestCase;

class AcuseTest extends TestCase
{
    /** Crea un mock de PDO que simula operaciones exitosas. */
    private function buildMockPdo(array $fetchAllReturn = []): PDO
    {
        $mockStmt = $this->createMock(PDOStatement::class);
        $mockStmt->method('bindValue')->willReturn(true);
        $mockStmt->method('execute')->willReturn(true);
        $mockStmt->method('fetchAll')->willReturn($fetchAllReturn);

        $mockPdo = $this->createMock(PDO::class);
        $mockPdo->method('prepare')->willReturn($mockStmt);

        return $mockPdo;
    }

    // ── Tests de registrarLectura() ───────────────────────────────────────────

    public function testRegistrarLecturaEjecutaInsertYDevuelveTrue(): void
    {
        $mockPdo = $this->buildMockPdo();
        $acuse   = new Acuse($mockPdo);

        $resultado = $acuse->registrarLectura(
            id_documento:    1,
            id_usuario:      101,
            ip:              '192.168.1.50',
            user_agent:      'Mozilla/5.0 (test)',
            usuario_creador: 101
        );

        $this->assertTrue($resultado);
    }

    public function testRegistrarLecturaUsaBindValueCorrectamente(): void
    {
        $mockStmt = $this->createMock(PDOStatement::class);
        $mockStmt->expects($this->exactly(5))
                 ->method('bindValue')
                 ->willReturn(true);
        $mockStmt->method('execute')->willReturn(true);

        $mockPdo = $this->createMock(PDO::class);
        $mockPdo->method('prepare')->willReturn($mockStmt);

        $acuse = new Acuse($mockPdo);
        $acuse->registrarLectura(2, 200, '10.0.0.1', 'TestAgent/1.0', 200);

        // Si llegamos aquí sin excepción, los 5 bindValue() se llamaron
        $this->assertTrue(true);
    }

    public function testRegistrarLecturaPropagaExcepcionDePDO(): void
    {
        $mockStmt = $this->createMock(PDOStatement::class);
        $mockStmt->method('bindValue')->willReturn(true);
        $mockStmt->method('execute')->willThrowException(new \Exception('constraint violation'));

        $mockPdo = $this->createMock(PDO::class);
        $mockPdo->method('prepare')->willReturn($mockStmt);

        $acuse = new Acuse($mockPdo);

        $this->expectException(\Exception::class);
        $this->expectExceptionMessageMatches('/Error al registrar acuse/i');

        $acuse->registrarLectura(1, 1, '127.0.0.1', 'agent', 1);
    }

    // ── Tests de obtenerLecturasPorDocumento() ────────────────────────────────

    public function testObtenerLecturasPorDocumentoDevuelveArray(): void
    {
        $lecturasFicticias = [
            ['id_acuse' => 1, 'fecha_lectura' => '2024-06-01 08:00:00', 'nombre' => 'Juan'],
            ['id_acuse' => 2, 'fecha_lectura' => '2024-06-02 09:30:00', 'nombre' => 'María'],
        ];

        $mockPdo = $this->buildMockPdo($lecturasFicticias);
        $acuse   = new Acuse($mockPdo);

        $resultado = $acuse->obtenerLecturasPorDocumento(1);

        $this->assertIsArray($resultado);
        $this->assertCount(2, $resultado);
        $this->assertSame('Juan', $resultado[0]['nombre']);
    }

    public function testObtenerLecturasSinRegistrosDevuelveArrayVacio(): void
    {
        $mockPdo = $this->buildMockPdo([]);
        $acuse   = new Acuse($mockPdo);

        $resultado = $acuse->obtenerLecturasPorDocumento(999);

        $this->assertIsArray($resultado);
        $this->assertEmpty($resultado);
    }

    public function testObtenerLecturasPropagaExcepcionDePDO(): void
    {
        $mockStmt = $this->createMock(PDOStatement::class);
        $mockStmt->method('bindValue')->willReturn(true);
        $mockStmt->method('execute')->willThrowException(new \Exception('query timeout'));

        $mockPdo = $this->createMock(PDO::class);
        $mockPdo->method('prepare')->willReturn($mockStmt);

        $acuse = new Acuse($mockPdo);

        $this->expectException(\Exception::class);
        $this->expectExceptionMessageMatches('/auditoría de lecturas/i');

        $acuse->obtenerLecturasPorDocumento(1);
    }
}
