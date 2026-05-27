#!/usr/bin/env python3
import json
import os
import subprocess
import sys
from datetime import datetime

# ==============================================================================
# CONFIGURACIÓN Y LECTURA DE VARIABLES DE ENTORNO
# ==============================================================================
# Definir directorios base de manera dinámica
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
ROOT_DIR = os.path.abspath(os.path.join(SCRIPT_DIR, "..", ".."))

def load_env():
    env_vars = {}
    env_path = os.path.join(ROOT_DIR, ".env")
    if os.path.exists(env_path):
        with open(env_path, "r", encoding="utf-8") as f:
            for line in f:
                line = line.strip()
                if not line or line.startswith("#"):
                    continue
                if "=" in line:
                    key, val = line.split("=", 1)
                    env_vars[key.strip()] = val.strip()
    return env_vars

env = load_env()

# Credenciales de base de datos
SQL_DATABASE = env.get("SQL_DATABASE", "SIGD_Central")
SQL_SA_PASSWORD = env.get("SQL_SA_PASSWORD", "TuPasswordSeguro123!")
PG_USER = env.get("PG_USER", "Super_Admin")
PG_PASSWORD = env.get("PG_PASSWORD", "Nintendo64")
PG_DATABASE = env.get("PG_DATABASE", "Postgres_SIGD")
MONGO_USERNAME = env.get("MONGO_USERNAME", "Super_Admin")
MONGO_PASSWORD = env.get("MONGO_PASSWORD", "Nintendo64")

# Rutas de archivos
MONGO_SEED_PATH = os.path.join(SCRIPT_DIR, "mongodb_seed_data.json")
SQLSERVER_OUTPUT_PATH = os.path.join(ROOT_DIR, "scripts", "sqlserver", "seed_demo_json.sql")
POSTGRES_OUTPUT_PATH = os.path.join(ROOT_DIR, "scripts", "postgres", "seed_demo_json.sql")
MONGO_OUTPUT_PATH = os.path.join(ROOT_DIR, "scripts", "mongo", "seed_demo_json.js")

# Mapeo de abreviaturas del JSON a IDs de Departamento
DEPT_MAP = {
    "ADM": 1,
    "RH": 2,
    "PROD": 3,
    "PRD": 3,
    "CAL": 4,
    "MANT": 5,
    "MNT": 5,
    "TI": 6
}

# ==============================================================================
# LECTURA DE DATOS MONGODB
# ==============================================================================
if not os.path.exists(MONGO_SEED_PATH):
    print(f"[ERROR] No se encontró el archivo {MONGO_SEED_PATH}", file=sys.stderr)
    sys.exit(1)

with open(MONGO_SEED_PATH, "r", encoding="utf-8") as f:
    mongo_docs = json.load(f)

print(f"[OK] Cargados {len(mongo_docs)} documentos desde {MONGO_SEED_PATH}")

# ==============================================================================
# GENERACIÓN DE SCRIPT PARA SQL SERVER
# ==============================================================================
sqlserver_sql = []
sqlserver_sql.append(f"""-- ==========================================================
-- SCRIPT DE CARGA SEMILLA GENERADO AUTOMATICAMENTE
-- Destino: SQL Server (Base de Datos: {SQL_DATABASE})
-- ==========================================================
USE [{SQL_DATABASE}];
GO

PRINT 'Iniciando limpieza de datos previos...';

-- Deshabilitar temporalmente restricciones de llaves foráneas para evitar errores
EXEC sp_MSforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all";

-- Desactivar triggers de borrado lógico (INSTEAD OF DELETE) para permitir la limpieza física
DISABLE TRIGGER [dbo].[TRG_SoftDelete_Documento] ON [dbo].[Documento];
DISABLE TRIGGER [dbo].[TRG_SoftDelete_Documento_Version] ON [dbo].[Documento_Version];
DISABLE TRIGGER [dbo].[TRG_SoftDelete_Usuario] ON [dbo].[Usuario];
DISABLE TRIGGER [dbo].[TRG_SoftDelete_Departamento] ON [dbo].[Departamento];
DISABLE TRIGGER [dbo].[TRG_SoftDelete_TipoDocumento] ON [dbo].[TipoDocumento];
DISABLE TRIGGER [dbo].[TRG_SoftDelete_Usuario_Rol] ON [dbo].[Usuario_Rol];

-- Limpieza de tablas log/bitácora
DELETE FROM [dbo].[BitacoraAcceso];
DELETE FROM [dbo].[BitacoraControlDocumento];
DELETE FROM [dbo].[BitacoraTransaccional];

-- Limpieza de tablas de negocio
DELETE FROM [dbo].[Documento_Version] WHERE [IdDocumento] >= 101;
DELETE FROM [dbo].[Documento] WHERE [Id] >= 101;
DELETE FROM [dbo].[Usuario_Rol] WHERE [IdUsuario] > 1;
DELETE FROM [dbo].[Usuario] WHERE [Id] > 1;
DELETE FROM [dbo].[Departamento] WHERE [Id] > 1;
DELETE FROM [dbo].[TipoDocumento] WHERE [Id] >= 1;

PRINT 'Limpieza física completada.';

-- Reactivar triggers de borrado lógico
ENABLE TRIGGER [dbo].[TRG_SoftDelete_Documento] ON [dbo].[Documento];
ENABLE TRIGGER [dbo].[TRG_SoftDelete_Documento_Version] ON [dbo].[Documento_Version];
ENABLE TRIGGER [dbo].[TRG_SoftDelete_Usuario] ON [dbo].[Usuario];
ENABLE TRIGGER [dbo].[TRG_SoftDelete_Departamento] ON [dbo].[Departamento];
ENABLE TRIGGER [dbo].[TRG_SoftDelete_TipoDocumento] ON [dbo].[TipoDocumento];
ENABLE TRIGGER [dbo].[TRG_SoftDelete_Usuario_Rol] ON [dbo].[Usuario_Rol];

-- 1. Insertar Departamentos
PRINT 'Insertando departamentos...';
SET IDENTITY_INSERT [dbo].[Departamento] ON;
INSERT INTO [dbo].[Departamento] (Id, Nombre, Abreviatura, Estatus, FechaCreacion, IdUsuarioCreacion) VALUES
(2, 'Recursos Humanos', 'RH', 1, GETDATE(), 1),
(3, 'Producción', 'PRD', 1, GETDATE(), 1),
(4, 'Calidad', 'CAL', 1, GETDATE(), 1),
(5, 'Mantenimiento', 'MNT', 1, GETDATE(), 1),
(6, 'Sistemas e Informática', 'TI', 1, GETDATE(), 1);
SET IDENTITY_INSERT [dbo].[Departamento] OFF;

-- 2. Insertar Tipos de Documento
PRINT 'Insertando tipos de documentos...';
SET IDENTITY_INSERT [dbo].[TipoDocumento] ON;
INSERT INTO [dbo].[TipoDocumento] (Id, Nombre, Abreviatura, TiempoRetencionMeses, Estatus, FechaCreacion, IdUsuarioCreacion) VALUES
(1, 'Procedimiento', 'PROC', 12, 1, GETDATE(), 1),
(2, 'Manual', 'MAN', 12, 1, GETDATE(), 1),
(3, 'Formato', 'FMT', 12, 1, GETDATE(), 1),
(4, 'Instructivo', 'INS', 12, 1, GETDATE(), 1),
(5, 'Política', 'POL', 12, 1, GETDATE(), 1),
(6, 'Especificación', 'ESP', 12, 1, GETDATE(), 1),
(7, 'Registro', 'REG', 12, 1, GETDATE(), 1);
SET IDENTITY_INSERT [dbo].[TipoDocumento] OFF;

-- 3. Insertar Usuarios (IDs del 2 al 10)
PRINT 'Insertando usuarios demo...';
SET IDENTITY_INSERT [dbo].[Usuario] ON;
INSERT INTO [dbo].[Usuario] (Id, IdDepartamento, Nombre, ApellidoP, ApellidoM, Correo, Contrasena, Estatus, FechaCreacion, IdUsuarioCreacion) VALUES
(2, 2, 'María', 'García', 'SIGD', 'maria.garcia@sigd.local', CONVERT(VARCHAR(255), HASHBYTES('SHA2_256', N'Admin@SIGD2026!'), 2), 1, GETDATE(), 1),
(3, 3, 'Carlos', 'Ramírez', 'SIGD', 'carlos.ramirez@sigd.local', CONVERT(VARCHAR(255), HASHBYTES('SHA2_256', N'Admin@SIGD2026!'), 2), 1, GETDATE(), 1),
(4, 4, 'Ana', 'Martínez', 'SIGD', 'ana.martinez@sigd.local', CONVERT(VARCHAR(255), HASHBYTES('SHA2_256', N'Admin@SIGD2026!'), 2), 1, GETDATE(), 1),
(5, 5, 'Jorge', 'López', 'SIGD', 'jorge.lopez@sigd.local', CONVERT(VARCHAR(255), HASHBYTES('SHA2_256', N'Admin@SIGD2026!'), 2), 1, GETDATE(), 1),
(6, 6, 'Laura', 'Hernández', 'SIGD', 'laura.hernandez@sigd.local', CONVERT(VARCHAR(255), HASHBYTES('SHA2_256', N'Admin@SIGD2026!'), 2), 1, GETDATE(), 1),
(7, 4, 'Marcos', 'de León', 'Tapia', 'marcos.deleon@sigd.local', CONVERT(VARCHAR(255), HASHBYTES('SHA2_256', N'Admin@SIGD2026!'), 2), 1, GETDATE(), 1),
(8, 3, 'Aurelio', 'Uribe', 'Santos', 'aurelio.uribe@sigd.local', CONVERT(VARCHAR(255), HASHBYTES('SHA2_256', N'Admin@SIGD2026!'), 2), 1, GETDATE(), 1),
(9, 2, 'Emilio', 'Ybarra', 'Ruiz', 'emilio.ybarra@sigd.local', CONVERT(VARCHAR(255), HASHBYTES('SHA2_256', N'Admin@SIGD2026!'), 2), 1, GETDATE(), 1),
(10, 5, 'Felix', 'Palomo', 'Valencia', 'felix.palomo@sigd.local', CONVERT(VARCHAR(255), HASHBYTES('SHA2_256', N'Admin@SIGD2026!'), 2), 1, GETDATE(), 1);
SET IDENTITY_INSERT [dbo].[Usuario] OFF;

-- 4. Asignar rol Super Administrador (Id=1) a todos para pruebas locales
PRINT 'Asignando roles...';
SET IDENTITY_INSERT [dbo].[Usuario_Rol] ON;
INSERT INTO [dbo].[Usuario_Rol] (Id, IdUsuario, IdRol, FechaAsignacion, Estatus, IdUsuarioCreacion) VALUES
(2, 2, 1, GETDATE(), 1, 1),
(3, 3, 1, GETDATE(), 1, 1),
(4, 4, 1, GETDATE(), 1, 1),
(5, 5, 1, GETDATE(), 1, 1),
(6, 6, 1, GETDATE(), 1, 1),
(7, 7, 1, GETDATE(), 1, 1),
(8, 8, 1, GETDATE(), 1, 1),
(9, 9, 1, GETDATE(), 1, 1),
(10, 10, 1, GETDATE(), 1, 1);
SET IDENTITY_INSERT [dbo].[Usuario_Rol] OFF;
""")

sqlserver_sql.append("PRINT 'Insertando documentos de mongodb_seed_data.json...';")
sqlserver_sql.append("SET IDENTITY_INSERT [dbo].[Documento] ON;")

# Inserciones para Documento
for doc in mongo_docs:
    id_doc = doc["id_documento_sql"]
    codigo = doc["codigo_interno"]
    titulo = doc["titulo"].replace("'", "''")
    
    # Determinar departamento desde código
    dept_abbr = codigo.split("-")[1]
    dept_id = DEPT_MAP.get(dept_abbr, 1)
    
    # Usuario propietario/creación
    user_id = doc["id_usuario_creacion"]
    
    # Estatus
    estatus_val = 1 if doc["estatus"] else 0
    
    # Fecha
    date_str = doc["fecha_creacion"]["$date"].replace("T", " ").replace("Z", "")
    
    sqlserver_sql.append(
        f"INSERT INTO [dbo].[Documento] (Id, CodigoInterno, Titulo, IdDepartamento, EstadoActual, IdUsuarioPropietario, FechaCreacion, Estatus, IdUsuarioCreacion, IdTipoDocumento) "
        f"VALUES ({id_doc}, '{codigo}', N'{titulo}', {dept_id}, 'Vigente', {user_id}, '{date_str}', {estatus_val}, {user_id}, 1);"
    )

sqlserver_sql.append("SET IDENTITY_INSERT [dbo].[Documento] OFF;")

sqlserver_sql.append("PRINT 'Insertando versiones de documentos...';")

# Inserciones para Documento_Version
for doc in mongo_docs:
    id_doc = doc["id_documento_sql"]
    user_id = doc["id_usuario_creacion"]
    estatus_val = 1 if doc["estatus"] else 0
    date_str = doc["fecha_creacion"]["$date"].replace("T", " ").replace("Z", "")
    
    sqlserver_sql.append(
        f"INSERT INTO [dbo].[Documento_Version] (IdDocumento, NumeroVersion, RutaArchivoFisico, HashDocumento, MotivoCambio, IdUsuarioSube, FechaSubida, IdUsuarioCreacion, FechaCreacion, Estatus, ExtensionArchivo, MimeType, TamanoBytes) "
        f"VALUES ({id_doc}, 1, 'gridfs://mock-id-{id_doc}', 'mock-hash-{id_doc}', 'Carga inicial de datos de prueba', {user_id}, '{date_str}', {user_id}, '{date_str}', {estatus_val}, '.pdf', 'application/pdf', 102400);"
    )

sqlserver_sql.append("""
-- Reactivar todas las restricciones
EXEC sp_MSforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all";
PRINT 'Se han activado las restricciones nuevamente.';
PRINT '========================================';
PRINT '  CARGA SEMILLA CENTRAL (SQL SERVER) LISTA';
PRINT '========================================';
""")

# Unir todo y guardar
sqlserver_content = "\n".join(sqlserver_sql)
os.makedirs(os.path.dirname(SQLSERVER_OUTPUT_PATH), exist_ok=True)
with open(SQLSERVER_OUTPUT_PATH, "w", encoding="utf-8") as f:
    f.write(sqlserver_content)
print(f"[OK] Archivo SQL Server generado en: {SQLSERVER_OUTPUT_PATH}")

# ==============================================================================
# GENERACIÓN DE SCRIPT PARA POSTGRESQL
# ==============================================================================
postgres_sql = []
postgres_sql.append(f"""-- ==========================================================
-- SCRIPT DE CARGA SEMILLA GENERADO AUTOMATICAMENTE
-- Destino: PostgreSQL (Base de Datos: {PG_DATABASE})
-- ==========================================================

-- Limpieza previa
DELETE FROM estadistica_documento WHERE id_documento >= 101;
DELETE FROM documento_vigente WHERE id_documento >= 101;
DELETE FROM usuario WHERE id_usuario > 6;

-- Insertar usuarios faltantes (IDs del 7 al 10)
INSERT INTO usuario (id_usuario, id_departamento, nombre, apellido_p, correo, contrasena, estatus, id_usuario_creacion) VALUES
(7, 4, 'Marcos', 'de León', 'marcos.deleon@sigd.local', UPPER(encode(digest('Admin@SIGD2026!','sha256'),'hex')), TRUE, 1),
(8, 3, 'Aurelio', 'Uribe', 'aurelio.uribe@sigd.local', UPPER(encode(digest('Admin@SIGD2026!','sha256'),'hex')), TRUE, 1),
(9, 2, 'Emilio', 'Ybarra', 'emilio.ybarra@sigd.local', UPPER(encode(digest('Admin@SIGD2026!','sha256'),'hex')), TRUE, 1),
(10, 5, 'Felix', 'Palomo', 'felix.palomo@sigd.local', UPPER(encode(digest('Admin@SIGD2026!','sha256'),'hex')), TRUE, 1)
ON CONFLICT (id_usuario) DO NOTHING;

-- Insertar documentos
""")

# Inserciones para documento_vigente
for doc in mongo_docs:
    id_doc = doc["id_documento_sql"]
    codigo = doc["codigo_interno"]
    titulo = doc["titulo"].replace("'", "''")
    
    # Determinar departamento desde código
    dept_abbr = codigo.split("-")[1]
    dept_id = DEPT_MAP.get(dept_abbr, 1)
    
    user_id = doc["id_usuario_creacion"]
    estatus_val = "TRUE" if doc["estatus"] else "FALSE"
    date_str = doc["fecha_creacion"]["$date"].replace("T", " ").replace("Z", "")
    
    postgres_sql.append(
        f"INSERT INTO documento_vigente (id_documento, codigo_interno, titulo, id_tipo, id_departamento, version_actual, fecha_publicacion, ruta_archivo_descarga, hash_verificacion, estatus, id_usuario_creacion, fecha_creacion) "
        f"VALUES ({id_doc}, '{codigo}', '{titulo}', 1, {dept_id}, 1, '{date_str}', 'gridfs://mock-id-{id_doc}', 'mock-hash-{id_doc}', {estatus_val}, {user_id}, '{date_str}') "
        f"ON CONFLICT (id_documento) DO NOTHING;"
    )

postgres_sql.append("\n-- Insertar estadísticas iniciales")
import random
random.seed(42)  # Para que el número de vistas sea determinista

# Inserciones para estadistica_documento
for doc in mongo_docs:
    id_doc = doc["id_documento_sql"]
    user_id = doc["id_usuario_creacion"]
    estatus_val = "TRUE" if doc["estatus"] else "FALSE"
    date_str = doc["fecha_creacion"]["$date"].replace("T", " ").replace("Z", "")
    vistas = random.randint(10, 150)
    
    postgres_sql.append(
        f"INSERT INTO estadistica_documento (id_documento, total_vistas, ultima_consulta, estatus, id_usuario_creacion, fecha_creacion) "
        f"VALUES ({id_doc}, {vistas}, '{date_str}', {estatus_val}, {user_id}, '{date_str}') "
        f"ON CONFLICT (id_documento) DO NOTHING;"
    )

postgres_sql.append("""
-- Resumen de inserción
DO $$
DECLARE
    total_docs    INT;
    total_users   INT;
BEGIN
    SELECT COUNT(*) INTO total_docs FROM documento_vigente WHERE id_documento >= 101;
    SELECT COUNT(*) INTO total_users FROM usuario;
    RAISE NOTICE '============================================';
    RAISE NOTICE '  SEED DATA POR POSTGRESQL APLICADO';
    RAISE NOTICE '  Documentos semilla >= 101 : %', total_docs;
    RAISE NOTICE '  Total usuarios registrados: %', total_users;
    RAISE NOTICE '============================================';
END $$;
""")

# Unir todo y guardar
postgres_content = "\n".join(postgres_sql)
os.makedirs(os.path.dirname(POSTGRES_OUTPUT_PATH), exist_ok=True)
with open(POSTGRES_OUTPUT_PATH, "w", encoding="utf-8") as f:
    f.write(postgres_content)
print(f"[OK] Archivo PostgreSQL generado en: {POSTGRES_OUTPUT_PATH}")

# ==============================================================================
# GENERACIÓN DE SCRIPT PARA MONGODB
# ==============================================================================
print("Generando script para MongoDB...")

def to_js_val(val):
    if isinstance(val, dict):
        if "$date" in val:
            return f'new Date("{val["$date"]}")'
        # Regular dict
        items = []
        for k, v in val.items():
            items.append(f'"{k}": {to_js_val(v)}')
        return "{" + ", ".join(items) + "}"
    elif isinstance(val, list):
        items = [to_js_val(x) for x in val]
        return "[" + ", ".join(items) + "]"
    elif isinstance(val, bool):
        return "true" if val else "false"
    elif val is None:
        return "null"
    elif isinstance(val, (int, float)):
        return str(val)
    elif isinstance(val, str):
        # Escape string for JS
        escaped = val.replace('\\', '\\\\').replace('"', '\\"').replace('\n', '\\n').replace('\r', '\\r')
        return f'"{escaped}"'
    else:
        return json.dumps(val)

mongo_docs_js = [to_js_val(doc) for doc in mongo_docs]
mongo_array_content = ",\n  ".join(mongo_docs_js)

mongo_js_content = f"""// ==========================================================
// SCRIPT DE CARGA SEMILLA GENERADO AUTOMATICAMENTE
// Destino: MongoDB (Base de Datos: sigd_busqueda)
// ==========================================================
db = db.getSiblingDB('sigd_busqueda');

print('Iniciando limpieza de documentos de prueba previos en MongoDB...');
db.DocumentosMetadata.deleteMany({{ id_documento_sql: {{ $gte: 101 }} }});

print('Insertando 50 documentos de prueba en MongoDB...');
db.DocumentosMetadata.insertMany([
  {mongo_array_content}
]);

print('========================================');
print('  SEED DATA PARA MONGODB LISTO');
print('  Total documentos >= 101: ' + db.DocumentosMetadata.countDocuments({{ id_documento_sql: {{ $gte: 101 }} }}));
print('========================================');
"""

os.makedirs(os.path.dirname(MONGO_OUTPUT_PATH), exist_ok=True)
with open(MONGO_OUTPUT_PATH, "w", encoding="utf-8") as f:
    f.write(mongo_js_content)
print(f"[OK] Archivo MongoDB generado en: {MONGO_OUTPUT_PATH}")

# ==============================================================================
# EJECUCIÓN VÍA DOCKER CLI
# ==============================================================================
print("\nDetectando contenedores de bases de datos de Docker...")

def is_container_running(name):
    try:
        out = subprocess.check_output(["docker", "ps", "--filter", f"name={name}", "--format", "{{.Names}}"], text=True)
        return name in out.strip().split("\n")
    except Exception:
        return False

sqlserver_running = is_container_running("sigd_sqlserver")
postgres_running = is_container_running("sigd_postgres")
mongodb_running = is_container_running("sigd_mongodb")

# Aplicar a SQL Server
if sqlserver_running:
    print("--> Aplicando script a SQL Server en el contenedor 'sigd_sqlserver'...")
    try:
        cmd = [
            "docker", "exec", "-i", "sigd_sqlserver",
            "/opt/mssql-tools18/bin/sqlcmd",
            "-S", "localhost",
            "-U", "sa",
            "-P", SQL_SA_PASSWORD,
            "-C"
        ]
        # Pasar como bytes codificados en utf-8, capturar salidas como bytes
        res = subprocess.run(cmd, input=sqlserver_content.encode("utf-8"), capture_output=True)
        if res.returncode == 0:
            print("[OK] SQL Server poblado exitosamente.")
            print(res.stdout.decode("utf-8", errors="replace"))
        else:
            print(f"[ERROR] Al ejecutar en SQL Server:\n{res.stderr.decode('utf-8', errors='replace')}\n{res.stdout.decode('utf-8', errors='replace')}", file=sys.stderr)
    except Exception as e:
        print(f"[ERROR] Al comunicar con SQL Server: {e}", file=sys.stderr)
else:
    print("[WARN] Contenedor 'sigd_sqlserver' no detectado o inactivo. Puedes aplicar manualmente 'scripts/sqlserver/seed_demo_json.sql'.")

# Aplicar a PostgreSQL
if postgres_running:
    print("--> Aplicando script a PostgreSQL en el contenedor 'sigd_postgres'...")
    try:
        cmd = [
            "docker", "exec", "-i",
            "-e", f"PGPASSWORD={PG_PASSWORD}",
            "sigd_postgres",
            "psql",
            "-U", PG_USER,
            "-d", PG_DATABASE
        ]
        # Pasar como bytes codificados en utf-8, capturar salidas como bytes
        res = subprocess.run(cmd, input=postgres_content.encode("utf-8"), capture_output=True)
        if res.returncode == 0:
            print("[OK] PostgreSQL poblado exitosamente.")
            print(res.stdout.decode("utf-8", errors="replace"))
            print(res.stderr.decode("utf-8", errors="replace"))  # Las notificaciones van a stderr
        else:
            print(f"[ERROR] Al ejecutar en PostgreSQL:\n{res.stderr.decode('utf-8', errors='replace')}\n{res.stdout.decode('utf-8', errors='replace')}", file=sys.stderr)
    except Exception as e:
        print(f"[ERROR] Al comunicar con PostgreSQL: {e}", file=sys.stderr)
else:
    print("[WARN] Contenedor 'sigd_postgres' no detectado o inactivo. Puedes aplicar manualmente 'scripts/postgres/seed_demo_json.sql'.")

# Aplicar a MongoDB
if mongodb_running:
    print("--> Aplicando script a MongoDB en el contenedor 'sigd_mongodb'...")
    try:
        cmd = [
            "docker", "exec", "-i", "sigd_mongodb",
            "mongosh",
            "-u", MONGO_USERNAME,
            "-p", MONGO_PASSWORD,
            "--authenticationDatabase", "admin",
            "sigd_busqueda"
        ]
        # Pasar como bytes codificados en utf-8, capturar salidas como bytes
        res = subprocess.run(cmd, input=mongo_js_content.encode("utf-8"), capture_output=True)
        if res.returncode == 0:
            print("[OK] MongoDB poblado exitosamente.")
            print(res.stdout.decode("utf-8", errors="replace"))
        else:
            print(f"[ERROR] Al ejecutar en MongoDB:\n{res.stderr.decode('utf-8', errors='replace')}\n{res.stdout.decode('utf-8', errors='replace')}", file=sys.stderr)
    except Exception as e:
        print(f"[ERROR] Al comunicar con MongoDB: {e}", file=sys.stderr)
else:
    print("[WARN] Contenedor 'sigd_mongodb' no detectado o inactivo. Puedes aplicar manualmente 'scripts/mongo/seed_demo_json.js'.")

print("\nProceso finalizado. Los scripts de semilla se han guardado para su uso futuro o respaldo.")
