<!DOCTYPE html>
<html lang="es" data-bs-theme="dark">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Portal de Normativas | Planta</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet">
    <style>
        body { background-color: #121212; color: #e0e0e0; }
        .card { background-color: #1e1e1e; border: 1px solid #333; }
        .table-dark { --bs-table-bg: #1e1e1e; }
    </style>
</head>
<body>
    <div class="container mt-5">
        <h2 class="mb-4 text-primary">📚 Portal de Documentos Vigentes</h2>
        
        <div class="card mb-4">
            <div class="card-body">
                <label for="inputBusqueda" class="form-label">Buscar normativa por título o contenido clave (Full-Text Search):</label>
                <input type="text" id="inputBusqueda" class="form-control bg-dark text-white border-secondary" placeholder="Ej. Manual de Calidad, ISO 9001, Soldadura...">
            </div>
        </div>

        <div class="table-responsive">
            <table class="table table-dark table-hover align-middle">
                <thead>
                    <tr>
                        <th>Código</th>
                        <th>Título del Documento</th>
                        <th>Versión</th>
                        <th>Acción</th>
                    </tr>
                </thead>
                <tbody id="tablaResultados">
                    <tr>
                        <td colspan="4" class="text-center text-muted">Escribe en el buscador para consultar normativas...</td>
                    </tr>
                </tbody>
            </table>
        </div>
        
        <div id="alertaNotificacion" class="alert d-none mt-3" role="alert"></div>
    </div>

    <script>
        // Simulamos que el operario logueado tiene el ID 105 (En producción esto vendría de la sesión)
        const ID_USUARIO_ACTUAL = 105;

        const inputBusqueda = document.getElementById('inputBusqueda');
        const tablaResultados = document.getElementById('tablaResultados');
        const alertaNotificacion = document.getElementById('alertaNotificacion');

        // 1. EVENTO DE BÚSQUEDA -> Consulta a Node.js (Puerto 3000)
        inputBusqueda.addEventListener('keyup', async (e) => {
            const query = e.target.value;
            if (query.length < 3) return; // Esperar a que escriba al menos 3 letras

            try {
                // Aquí llamamos a tu microservicio de MongoDB
                const response = await fetch(`http://localhost:3000/api/documentos/buscar?q=${query}`);
                const data = await response.json();
                
                renderizarTabla(data);
            } catch (error) {
                console.error("Error contactando a Node.js:", error);
            }
        });

        function renderizarTabla(documentos) {
            tablaResultados.innerHTML = '';
            if (documentos.length === 0) {
                tablaResultados.innerHTML = '<tr><td colspan="4" class="text-center">No se encontraron documentos vigentes.</td></tr>';
                return;
            }

            documentos.forEach(doc => {
                const fila = `
                    <tr>
                        <td><span class="badge bg-secondary">${doc.codigo_interno}</span></td>
                        <td><strong>${doc.titulo}</strong></td>
                        <td>v${doc.version}</td>
                        <td>
                            <button class="btn btn-success btn-sm" onclick="firmarLectura(${doc.id_documento})">
                                ✓ Confirmar Lectura
                            </button>
                        </td>
                    </tr>
                `;
                tablaResultados.innerHTML += fila;
            });
        }

        // 2. EVENTO DE FIRMA -> Consulta a PHP/PostgreSQL (Puerto 8000)
        async function firmarLectura(idDocumento) {
            const formData = new FormData();
            formData.append('id_documento', idDocumento);
            formData.append('id_usuario', ID_USUARIO_ACTUAL);

            try {
                // Llamamos al controlador de PHP que acabamos de crear
                const response = await fetch('http://localhost:8000/index.php?action=registrar_acuse', {
                    method: 'POST',
                    body: formData
                });
                
                const result = await response.json();

                alertaNotificacion.classList.remove('d-none', 'alert-danger', 'alert-success');
                if (response.ok) {
                    alertaNotificacion.classList.add('alert-success');
                    alertaNotificacion.innerText = result.message; // "Acuse registrado..."
                } else {
                    alertaNotificacion.classList.add('alert-danger');
                    alertaNotificacion.innerText = result.message || "Error al registrar.";
                }
                
                // Ocultar la alerta después de 4 segundos
                setTimeout(() => alertaNotificacion.classList.add('d-none'), 4000);

            } catch (error) {
                console.error("Error contactando a PHP:", error);
            }
        }
    </script>
</body>
</html>