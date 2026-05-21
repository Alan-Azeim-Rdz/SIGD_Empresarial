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
