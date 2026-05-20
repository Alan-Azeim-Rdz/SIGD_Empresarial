-- ============================================================
-- SIGD Empresarial — Módulo de Reportes (PostgreSQL)
-- Script de inicialización: tabla bitacora_sync
-- Se ejecuta automáticamente al crear el contenedor postgres
-- gracias al volumen: ./scripts/postgres:/docker-entrypoint-initdb.d
-- ============================================================

-- Tabla de bitácora para registrar cada intento de sincronización
-- recibido desde el Módulo Central. Permite diagnóstico de fallos
-- de integración sin afectar el flujo principal.
CREATE TABLE IF NOT EXISTS bitacora_sync (
    id            BIGSERIAL    PRIMARY KEY,
    id_documento  INTEGER      NOT NULL,
    estado        VARCHAR(20)  NOT NULL,    -- SYNC_OK | SYNC_ERROR
    mensaje_error TEXT,
    fecha_evento  TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Índice para consultas por documento (diagnóstico rápido)
CREATE INDEX IF NOT EXISTS idx_bitacora_sync_id_doc
    ON bitacora_sync (id_documento);

-- Índice para filtrar por estado (monitoreo operacional)
CREATE INDEX IF NOT EXISTS idx_bitacora_sync_estado
    ON bitacora_sync (estado);

-- ============================================================
-- NOTA: La tabla documento_vigente ya debe existir previamente
-- (creada por el script principal del esquema de Reportes).
-- Si aún no existe, puedes crearla con la siguiente sentencia:
-- ============================================================
CREATE TABLE IF NOT EXISTS documento_vigente (
    id_documento          INTEGER      PRIMARY KEY,
    codigo_interno        VARCHAR(50)  NOT NULL UNIQUE,
    titulo                VARCHAR(255) NOT NULL,
    id_tipo               INTEGER      NOT NULL,
    id_departamento       INTEGER      NOT NULL,
    version_actual        INTEGER      NOT NULL DEFAULT 1,
    fecha_publicacion     TIMESTAMP,
    ruta_archivo_descarga TEXT         NOT NULL,
    hash_verificacion     VARCHAR(128),
    estatus               BOOLEAN      NOT NULL DEFAULT TRUE,
    fecha_creacion        TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion    TIMESTAMP,
    id_usuario_creacion   INTEGER      NOT NULL,
    id_usuario_modificacion INTEGER
);

-- Índice de búsqueda por departamento (usado por sp_reporte_cumplimiento_depto)
CREATE INDEX IF NOT EXISTS idx_doc_vigente_depto
    ON documento_vigente (id_departamento);

-- ============================================================
-- FUNCIÓN: sp_reporte_cumplimiento_depto
-- Devuelve el porcentaje de empleados que han confirmado la
-- lectura de los documentos vigentes de su departamento.
-- Invocada desde ReporteController::obtenerClimaCumplimiento()
-- ============================================================
CREATE OR REPLACE FUNCTION sp_reporte_cumplimiento_depto(p_id_depto INTEGER)
RETURNS TABLE (
    id_departamento      INTEGER,
    total_documentos     BIGINT,
    total_lecturas       BIGINT,
    porcentaje_cumplimiento NUMERIC(5,2)
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT
        p_id_depto                            AS id_departamento,
        COUNT(DISTINCT dv.id_documento)       AS total_documentos,
        COUNT(DISTINCT al.id_acuse)           AS total_lecturas,
        CASE
            WHEN COUNT(DISTINCT dv.id_documento) = 0 THEN 0.00
            ELSE ROUND(
                COUNT(DISTINCT al.id_acuse)::NUMERIC
                / COUNT(DISTINCT dv.id_documento)::NUMERIC * 100,
                2
            )
        END                                   AS porcentaje_cumplimiento
    FROM documento_vigente dv
    LEFT JOIN acuse_lectura al
           ON al.id_documento = dv.id_documento
          AND al.estatus = true
    WHERE dv.id_departamento = p_id_depto
      AND dv.estatus         = true;
END;
$$;
