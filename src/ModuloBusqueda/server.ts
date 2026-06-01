// ── Punto de entrada de producción ───────────────────────────────────────────
// Importa la app ya configurada de index.ts y arranca la conexión a MongoDB
// y el servidor HTTP. Separado de index.ts para que los tests puedan importar
// la app sin conectar a la base de datos ni levantar el servidor.

import { app, logger } from './index';
import mongoose from 'mongoose';

const PORT      = 3000;
const MONGO_URI = process.env['MONGO_URI'] ?? 'mongodb://localhost:27017/sigd';

const maskUri = (uri: string): string =>
  uri.replace(/(mongodb(?:\+srv)?:\/\/[^:]+:)[^@]+(@)/, '$1***$2');

mongoose.connect(MONGO_URI)
  .then(() => logger.info({ uri_masked: maskUri(MONGO_URI) }, 'mongodb_connected'))
  .catch((err: unknown) => logger.error({ err }, 'mongodb_connection_failed'));

app.listen(PORT, () => {
  logger.info({
    port:      PORT,
    endpoints: ['POST /indexar', 'GET /buscar', 'GET /documento/:id'],
    docs_url:  `http://localhost:${PORT}/docs`
  }, 'server_started');
});
