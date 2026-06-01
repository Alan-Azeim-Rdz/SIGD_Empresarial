"use strict";
// ── Punto de entrada de producción ───────────────────────────────────────────
// Importa la app ya configurada de index.ts y arranca la conexión a MongoDB
// y el servidor HTTP. Separado de index.ts para que los tests puedan importar
// la app sin conectar a la base de datos ni levantar el servidor.
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const index_1 = require("./index");
const mongoose_1 = __importDefault(require("mongoose"));
const PORT = 3000;
const MONGO_URI = process.env['MONGO_URI'] ?? 'mongodb://localhost:27017/sigd';
const maskUri = (uri) => uri.replace(/(mongodb(?:\+srv)?:\/\/[^:]+:)[^@]+(@)/, '$1***$2');
mongoose_1.default.connect(MONGO_URI)
    .then(() => index_1.logger.info({ uri_masked: maskUri(MONGO_URI) }, 'mongodb_connected'))
    .catch((err) => index_1.logger.error({ err }, 'mongodb_connection_failed'));
index_1.app.listen(PORT, () => {
    index_1.logger.info({
        port: PORT,
        endpoints: ['POST /indexar', 'GET /buscar', 'GET /documento/:id'],
        docs_url: `http://localhost:${PORT}/docs`
    }, 'server_started');
});
//# sourceMappingURL=server.js.map