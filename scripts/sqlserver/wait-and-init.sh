#!/bin/bash
# ===========================================================
# Espera a que SQL Server esté listo y ejecuta el script SQL.
# Se ejecuta en segundo plano desde entrypoint.sh.
# Solo inicializa si la BD no existe aún (idempotente).
# ===========================================================

SQLCMD="/opt/mssql-tools18/bin/sqlcmd"
MAX_RETRIES=30
RETRY_INTERVAL=3

echo "[INIT] Esperando a que SQL Server esté listo..."

for i in $(seq 1 $MAX_RETRIES); do
    $SQLCMD -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "SELECT 1" > /dev/null 2>&1
    if [ $? -eq 0 ]; then
        echo "[INIT] SQL Server listo (intento $i/$MAX_RETRIES)."

        # Verificar si la BD ya existe para no ejecutar de nuevo
        DB_EXISTS=$($SQLCMD -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "SELECT COUNT(*) FROM sys.databases WHERE name = 'SIGD_Central'" -h -1 -W 2>/dev/null | head -1 | tr -d '[:space:]')

        if [ "$DB_EXISTS" = "0" ]; then
            echo "[INIT] Base de datos no encontrada. Ejecutando init_Central.sql..."
            $SQLCMD -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -i /scripts/init_Central.sql
            if [ $? -eq 0 ]; then
                echo "[INIT] ✅ Base de datos inicializada exitosamente."
                
                # Ejecutar migración multi-empresa
                echo "[INIT] Ejecutando migration_multi_empresa.sql..."
                $SQLCMD -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -i /scripts/migration_multi_empresa.sql
                if [ $? -eq 0 ]; then
                    echo "[INIT] ✅ Migración multi-empresa aplicada exitosamente."
                else
                    echo "[INIT] ❌ Error al aplicar la migración multi-empresa."
                fi

                # Ejecutar migración empresa auditoria
                echo "[INIT] Ejecutando migration_empresa_auditoria.sql..."
                $SQLCMD -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -i /scripts/migration_empresa_auditoria.sql
                if [ $? -eq 0 ]; then
                    echo "[INIT] ✅ Migración de auditoría de Empresa aplicada exitosamente."
                else
                    echo "[INIT] ❌ Error al aplicar la migración de auditoría de Empresa."
                fi

                # Ejecutar semillas de empresas
                echo "[INIT] Ejecutando seed_empresas.sql..."
                $SQLCMD -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -i /scripts/seed_empresas.sql
                if [ $? -eq 0 ]; then
                    echo "[INIT] ✅ Semilla de empresas cargada exitosamente."
                else
                    echo "[INIT] ❌ Error al cargar semilla de empresas."
                fi

                # Ejecutar semillas completas de datos para ambas empresas
                echo "[INIT] Ejecutando seed_datos_completo.sql..."
                $SQLCMD -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -i /scripts/seed_datos_completo.sql
                if [ $? -eq 0 ]; then
                    echo "[INIT] ✅ Semilla completa de datos cargada exitosamente."
                else
                    echo "[INIT] ❌ Error al cargar semilla completa de datos."
                fi

                # Ejecutar triggers de auditoría
                echo "[INIT] Ejecutando triggers_auditoria.sql..."
                $SQLCMD -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -i /scripts/triggers_auditoria.sql
                if [ $? -eq 0 ]; then
                    echo "[INIT] ✅ Triggers de auditoría creados exitosamente."
                else
                    echo "[INIT] ❌ Error al crear triggers de auditoría."
                fi

                # Ejecutar corrección de trigger de edición
                echo "[INIT] Ejecutando fix_trigger_editar.sql..."
                $SQLCMD -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -i /scripts/fix_trigger_editar.sql
                if [ $? -eq 0 ]; then
                    echo "[INIT] ✅ Corrección de trigger de edición aplicada exitosamente."
                else
                    echo "[INIT] ❌ Error al aplicar corrección de trigger de edición."
                fi

                # Ejecutar trigger de fecha modificacion de empresa
                echo "[INIT] Ejecutando trigger_empresa_fechamod.sql..."
                $SQLCMD -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -i /scripts/trigger_empresa_fechamod.sql
                if [ $? -eq 0 ]; then
                    echo "[INIT] ✅ Trigger de fecha de modificación de empresa creado exitosamente."
                else
                    echo "[INIT] ❌ Error al crear trigger de fecha de modificación de empresa."
                fi

                # Ejecutar migración de funcionalidades (IP y versiones)
                echo "[INIT] Ejecutando migration_funcionalidades.sql..."
                $SQLCMD -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -i /scripts/migration_funcionalidades.sql
                if [ $? -eq 0 ]; then
                    echo "[INIT] ✅ Migración de funcionalidades aplicada exitosamente."
                else
                    echo "[INIT] ❌ Error al aplicar migración de funcionalidades."
                fi

                # Ejecutar script semilla de JSON si existe
                if [ -f /scripts/seed_demo_json.sql ]; then
                    echo "[INIT] Detectado seed_demo_json.sql. Sembrando datos..."
                    $SQLCMD -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -i /scripts/seed_demo_json.sql
                    if [ $? -eq 0 ]; then
                        echo "[INIT] ✅ Datos semilla de SQL Server cargados exitosamente."
                    else
                        echo "[INIT] ❌ Error al cargar los datos semilla de SQL Server."
                    fi
                fi
            else
                echo "[INIT] ❌ Error al ejecutar el script de inicialización."
            fi
        else
            echo "[INIT] ✅ Base de datos SIGD_Central ya existe. No se ejecuta el script."
        fi
        exit 0
    fi
    echo "[INIT] SQL Server no está listo aún... reintentando en ${RETRY_INTERVAL}s ($i/$MAX_RETRIES)"
    sleep $RETRY_INTERVAL
done

echo "[INIT] ❌ SQL Server no respondió después de $MAX_RETRIES intentos."
exit 1
