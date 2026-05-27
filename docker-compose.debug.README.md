# Modo Debug — SIGD Empresarial

## ¿Para qué sirve?
Activa logs detallados y mensajes de error completos en los 3 módulos.
Útil para desarrollo y pruebas. **NUNCA usar en producción.**

## ¿Cómo usarlo?
```bash
docker compose -f docker-compose.yml -f docker-compose.debug.yml up -d
```

## ¿Cómo volver al modo normal?
```bash
docker compose down
docker compose up -d
```

## ¿Qué activa?
| Módulo | Cambio |
|---|---|
| .NET | ASPNETCORE_ENVIRONMENT=Development (muestra stack trace completo) |
| Node.js | LOG_LEVEL=debug + DEBUG=* (logs verbose) |
| PHP | display_errors=On + error_reporting=E_ALL |
| SQL Server | Auditoría SQL activada |
