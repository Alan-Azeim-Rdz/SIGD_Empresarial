// Configura variables de entorno ANTES de que cualquier módulo sea cargado.
// setupFiles se ejecuta antes de cada archivo de test, antes del primer require().

// Evita el worker de pino-pretty (requiere entorno de red/worker_threads en Jest)
process.env['NODE_ENV'] = 'production';
// Silencia toda salida de pino durante los tests
process.env['LOG_LEVEL'] = 'silent';
// URI ficticia — mongoose.connect() no se llama desde index.ts (se movió a server.ts)
process.env['MONGO_URI'] = 'mongodb://test-host:27017/test';
