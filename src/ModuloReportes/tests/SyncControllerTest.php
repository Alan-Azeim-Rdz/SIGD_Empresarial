<?php
declare(strict_types=1);

namespace Tests;

use Controllers\SyncController;
use PDO;
use PDOStatement;
use PHPUnit\Framework\TestCase;

class SyncControllerTest extends TestCase
{
    /** Payload mínimo válido para sincronizarDocumento() */
    private function payloadValido(): array
    {
        return [
            'id_documento'         => 1,
            'id_empresa'           => 1,
            'codigo_interno'       => 'CAL-MAN-001',
            'titulo'               => 'Manual de Calidad',
            'id_tipo'              => 2,
            'id_departamento'      => 3,
            'version_actual'       => 4,
            'fecha_publicacion'    => '2024-06-15',
            'ruta_archivo_descarga'=> '/docs/cal-man-001.pdf',
            'id_usuario_creacion'  => 1,
        ];
    }

    /** Crea un mock de PDO + PDOStatement que simula DB funcional. */
    private function buildMockPdo(): PDO
    {
        $mockStmt = $this->createMock(PDOStatement::class);
        $mockStmt->method('bindValue')->willReturn(true);
        $mockStmt->method('execute')->willReturn(true);
        $mockStmt->method('fetchAll')->willReturn([]);

        $mockPdo = $this->createMock(PDO::class);
        $mockPdo->method('prepare')->willReturn($mockStmt);
        $mockPdo->method('beginTransaction')->willReturn(true);
        $mockPdo->method('commit')->willReturn(true);
        $mockPdo->method('rollBack')->willReturn(true);

        return $mockPdo;
    }

    /** Construye un SyncController parcialmente mockeado (leerInput sobreescrito). */
    private function buildController(PDO $pdo, ?array $inputData): SyncController
    {
        $controller = $this->getMockBuilder(SyncController::class)
            ->setConstructorArgs([$pdo])
            ->onlyMethods(['leerInput'])
            ->getMock();

        $controller->method('leerInput')->willReturn($inputData);

        return $controller;
    }

    // ── Tests de sincronizarDocumento() ───────────────────────────────────────

    public function testSincronizarDocumentoConPayloadValidoDevuelveSuccess(): void
    {
        $pdo        = $this->buildMockPdo();
        $controller = $this->buildController($pdo, $this->payloadValido());

        ob_start();
        $controller->sincronizarDocumento();
        $output = ob_get_clean();

        $result = json_decode($output, true);
        $this->assertSame('success', $result['status']);
        $this->assertSame(1, $result['id']);
    }

    public function testSincronizarDocumentoSinIdDocumentoSqlDevuelve400(): void
    {
        $pdo     = $this->buildMockPdo();
        $payload = $this->payloadValido();
        unset($payload['id_documento']);

        $controller = $this->buildController($pdo, $payload);

        ob_start();
        $controller->sincronizarDocumento();
        $output = ob_get_clean();

        $result = json_decode($output, true);
        $this->assertSame('error', $result['status']);
        $this->assertStringContainsString('id_documento', $result['message']);
    }

    public function testSincronizarDocumentoConPayloadVacioDevuelve400(): void
    {
        $pdo        = $this->buildMockPdo();
        $controller = $this->buildController($pdo, null);

        ob_start();
        $controller->sincronizarDocumento();
        $output = ob_get_clean();

        $result = json_decode($output, true);
        $this->assertSame('error', $result['status']);
        $this->assertStringContainsString('vacío', $result['message']);
    }

    public function testSincronizarDocumentoConDbFallaDevuelve500(): void
    {
        $mockStmt = $this->createMock(PDOStatement::class);
        $mockStmt->method('bindValue')->willReturn(true);
        $mockStmt->method('execute')->willThrowException(new \Exception('timeout de PostgreSQL'));

        $mockPdo = $this->createMock(PDO::class);
        $mockPdo->method('prepare')->willReturn($mockStmt);

        $controller = $this->buildController($mockPdo, $this->payloadValido());

        ob_start();
        $controller->sincronizarDocumento();
        $output = ob_get_clean();

        $result = json_decode($output, true);
        $this->assertSame('error', $result['status']);
        $this->assertStringContainsString('Error al sincronizar', $result['message']);
    }

    // ── Tests de sincronizarBatch() ───────────────────────────────────────────

    public function testSincronizarBatchConArrayValidoProcesaTodos(): void
    {
        $pdo  = $this->buildMockPdo();
        $lote = [$this->payloadValido(), array_merge($this->payloadValido(), ['id_documento' => 2, 'codigo_interno' => 'CAL-MAN-002'])];

        $controller = $this->buildController($pdo, $lote);

        ob_start();
        $controller->sincronizarBatch();
        $output = ob_get_clean();

        $result = json_decode($output, true);
        $this->assertSame('success', $result['status']);
        $this->assertSame(2, $result['sincronizados']);
        $this->assertSame(0, $result['omitidos']);
    }

    public function testSincronizarBatchConUnItemInvalidoLoReportaComoOmitido(): void
    {
        $pdo          = $this->buildMockPdo();
        $docValido    = $this->payloadValido();
        $docInvalido  = ['titulo' => 'Sin ID']; // falta id_documento y otros campos

        $controller = $this->buildController($pdo, [$docValido, $docInvalido]);

        ob_start();
        $controller->sincronizarBatch();
        $output = ob_get_clean();

        $result = json_decode($output, true);
        $this->assertSame('success', $result['status']);
        $this->assertSame(1, $result['sincronizados']);
        $this->assertSame(1, $result['omitidos']);

        $detalle = $result['detalle'];
        $omitido = array_filter($detalle, fn($d) => $d['status'] === 'omitido');
        $this->assertCount(1, $omitido);
    }

    public function testSincronizarBatchConArrayVacioDevuelve400(): void
    {
        $pdo        = $this->buildMockPdo();
        $controller = $this->buildController($pdo, []);

        ob_start();
        $controller->sincronizarBatch();
        $output = ob_get_clean();

        $result = json_decode($output, true);
        $this->assertSame('error', $result['status']);
    }
}
