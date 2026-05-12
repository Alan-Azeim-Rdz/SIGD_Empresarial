import * as http from 'node:http';

// Creamos un servidor web básico nativo para que el contenedor se mantenga vivo
const server = http.createServer((req: http.IncomingMessage, res: http.ServerResponse) => {
    res.writeHead(200, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify({ mensaje: 'Módulo de Búsqueda (Node.js + TS) funcionando.' }));
});

const PORT = 3000;
server.listen(PORT, () => {
    console.log(`Servidor de búsqueda escuchando en el puerto ${PORT}`);
});