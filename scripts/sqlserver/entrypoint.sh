#!/bin/bash
# ===========================================================
# Entrypoint personalizado para SQL Server en Docker
# Inicia SQL Server y ejecuta el script de inicialización
# en paralelo (esperando a que esté listo).
# ===========================================================

# Lanzar el script de inicialización en segundo plano
# (espera a que SQL Server acepte conexiones antes de ejecutar)
(/scripts/wait-and-init.sh &)

# Iniciar SQL Server en primer plano (proceso principal del contenedor)
exec /opt/mssql/bin/sqlservr
