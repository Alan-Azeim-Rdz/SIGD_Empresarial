<?php
declare(strict_types=1);

namespace Tests;

use Config\Database;
use PHPUnit\Framework\TestCase;

/**
 * Tests de Config\Database.
 *
 * NOTA: getConnection() llama a `new PDO(...)` que intenta conectarse realmente.
 * Con credenciales ficticias del bootstrap, ese intento falla y activa die().
 * Por eso estos tests verifican la estructura de la clase (constructor, propiedades,
 * métodos) sin invocar getConnection() directamente contra el die().
 * La cobertura de la ruta de éxito la aportan SyncControllerTest y AcuseTest,
 * que inyectan un PDO mock directamente (sin pasar por Database).
 */
class DatabaseTest extends TestCase
{
    public function testInstanciaSeCreaSinErrores(): void
    {
        // El constructor solo inicializa $conn = null; no conecta.
        $db = new Database();

        $this->assertInstanceOf(Database::class, $db);
    }

    public function testDbErrorEsNullAlInstanciar(): void
    {
        $db = new Database();

        // Sin llamar a getConnection(), $dbError debe ser null
        $this->assertNull($db->dbError);
    }

    public function testGetConnectionMetodoExiste(): void
    {
        $db = new Database();

        $this->assertTrue(
            method_exists($db, 'getConnection'),
            'Database debe tener el método getConnection()'
        );
    }
}
