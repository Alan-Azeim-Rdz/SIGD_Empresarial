<!DOCTYPE html>
<html lang="es" data-bs-theme="dark">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Portal de Normativas | Planta</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet">
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" rel="stylesheet">
    <style>
        body { background-color: #121212; color: #e0e0e0; }
        .card { background-color: #1e1e1e; border: 1px solid #333; }
        .table-dark { --bs-table-bg: #1e1e1e; }
        .navbar-custom { background-color: #1a1a1a; border-bottom: 1px solid #333; }
    </style>
</head>
<body>
    <!-- Cabecera de Navegación -->
    <nav class="navbar navbar-expand-lg navbar-dark navbar-custom mb-4">
        <div class="container">
            <span class="navbar-brand fw-bold text-primary"><i class="fas fa-industry me-2"></i>Portal Planta</span>
            <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav">
                <span class="navbar-toggler-icon"></span>
            </button>
            <div class="collapse navbar-collapse" id="navbarNav">
                <ul class="navbar-nav me-auto">
                    <li class="nav-item">
                        <a class="nav-link active" href="?action=portal"><i class="fas fa-book me-1"></i>Portal de Normativas</a>
                    </li>
                    <li class="nav-item">
                        <a class="nav-link" href="?action=dashboard"><i class="fas fa-chart-bar me-1"></i>Dashboard de Reportes</a>
                    </li>
                </ul>
                <ul class="navbar-nav">
                    <li class="nav-item">
                        <a id="lnk-modulo-central" class="btn btn-outline-info btn-sm" href="http://localhost:5000" target="_blank">
                            <i class="fas fa-arrow-up-right-from-square me-1"></i> Módulo Central C#
                        </a>
                    </li>
                    <li class="nav-item ms-2">
                        <button class="btn btn-outline-secondary btn-sm text-light" data-bs-toggle="modal" data-bs-target="#modalConexiones" title="Configuración de Conexiones">
                            <i class="fas fa-cog"></i>
                        </button>
                    </li>
                </ul>
            </div>
        </div>
    </nav>

    <div class="container">
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
                        <th class="text-end">Acciones</th>
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

    <!-- Modal Configuración de Conexiones -->
    <div class="modal fade" id="modalConexiones" tabindex="-1" aria-labelledby="modalConexionesLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content bg-dark text-white border-secondary" style="border-radius: 12px;">
                <div class="modal-header border-secondary" style="background-color: #1a1a1a;">
                    <h5 class="modal-title fw-bold text-primary" id="modalConexionesLabel"><i class="fas fa-network-wired me-2"></i> Configuración de Conexiones</h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <p class="small text-muted mb-3">Define las URLs del ecosistema para la navegación del cliente y llamadas de API. De forma predeterminada, se detecta el host actual y cambia el puerto.</p>
                    <div class="mb-3">
                        <label for="cfg_csharp" class="form-label fw-bold text-light">Módulo Central (C# - Puerto 5000)</label>
                        <input type="text" id="cfg_csharp" class="form-control bg-secondary text-white border-dark text-white" placeholder="http://localhost:5000" />
                    </div>
                    <div class="mb-3">
                        <label for="cfg_php" class="form-label fw-bold text-light">Módulo de Reportes/Portal (PHP - Puerto 8000)</label>
                        <input type="text" id="cfg_php" class="form-control bg-secondary text-white border-dark text-white" placeholder="http://localhost:8000" />
                    </div>
                    <div class="mb-3">
                        <label for="cfg_node" class="form-label fw-bold text-light">Buscador Global (Node.js - Puerto 3000)</label>
                        <input type="text" id="cfg_node" class="form-control bg-secondary text-white border-dark text-white" placeholder="http://localhost:3000" />
                    </div>
                </div>
                <div class="modal-footer border-secondary" style="background-color: #1a1a1a;">
                    <button type="button" class="btn btn-outline-light btn-sm" id="btnResetConexiones">Restablecer</button>
                    <button type="button" class="btn btn-primary btn-sm" id="btnGuardarConexiones">Guardar Cambios</button>
                </div>
            </div>
        </div>
    </div>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js"></script>
    <script>
        // Configuración y resolución de conexiones
        const currentHost = window.location.hostname;
        const defaultCSharp = `http://${currentHost}:5000`;
        const defaultPHP = window.location.origin;
        const defaultNode = `http://${currentHost}:3000`;

        const resolvedUrls = {
            csharp: localStorage.getItem('cfg_url_csharp') || defaultCSharp,
            php: localStorage.getItem('cfg_url_php') || defaultPHP,
            node: localStorage.getItem('cfg_url_node') || defaultNode
        };

        function rewriteLinks() {
            // Actualizar enlace en navbar
            const lnkCentral = document.getElementById('lnk-modulo-central');
            if (lnkCentral) {
                lnkCentral.href = resolvedUrls.csharp;
            }
            
            document.querySelectorAll('a[href]').forEach(el => {
                let href = el.getAttribute('href');
                if (href) {
                    if (href.startsWith('http://localhost:5000')) {
                        el.href = href.replace('http://localhost:5000', resolvedUrls.csharp);
                    } else if (href.startsWith('http://localhost:8000')) {
                        el.href = href.replace('http://localhost:8000', resolvedUrls.php);
                    } else if (href.startsWith('http://localhost:3000')) {
                        el.href = href.replace('http://localhost:3000', resolvedUrls.node);
                    }
                }
            });
        }

        rewriteLinks();

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
                // Aquí llamamos a tu microservicio de MongoDB usando la URL dinámica resuelta
                const response = await fetch(`${resolvedUrls.node}/api/busqueda/buscar?q=${query}`);
                const data = await response.json();
                
                renderizarTabla(data);
            } catch (error) {
                console.error("Error contactando a Node.js:", error);
            }
        });

        function renderizarTabla(documentos) {
            tablaResultados.innerHTML = '';
            if (documentos.length === 0) {
                tablaResultados.innerHTML = '<tr><td colspan="4" class="text-center text-muted">No se encontraron documentos vigentes.</td></tr>';
                return;
            }

            documentos.forEach(doc => {
                const fila = `
                    <tr>
                        <td><span class="badge bg-secondary">${doc.codigo_interno}</span></td>
                        <td><strong>${doc.titulo}</strong></td>
                        <td><span class="badge bg-info text-dark">v${doc.version ?? 1}</span></td>
                        <td class="text-end">
                            <a href="${resolvedUrls.csharp}/Documento/DescargarUltima/${doc.id_documento_sql}" class="btn btn-outline-info btn-sm me-2" target="_blank">
                                <i class="fas fa-download me-1"></i> Descargar PDF
                            </a>
                            <button class="btn btn-success btn-sm" onclick="firmarLectura(${doc.id_documento_sql})">
                                <i class="fas fa-check me-1"></i> Confirmar Lectura
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
                // Llamamos al controlador de PHP usando la URL dinámica resuelta
                const response = await fetch(`${resolvedUrls.php}/index.php?action=registrar_acuse`, {
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

        // Configuración modal
        document.addEventListener('DOMContentLoaded', () => {
            rewriteLinks();
            
            const inputCSharp = document.getElementById('cfg_csharp');
            const inputPHP = document.getElementById('cfg_php');
            const inputNode = document.getElementById('cfg_node');
            
            if (inputCSharp) inputCSharp.value = resolvedUrls.csharp;
            if (inputPHP) inputPHP.value = resolvedUrls.php;
            if (inputNode) inputNode.value = resolvedUrls.node;

            const btnGuardar = document.getElementById('btnGuardarConexiones');
            if (btnGuardar) {
                btnGuardar.addEventListener('click', () => {
                    localStorage.setItem('cfg_url_csharp', inputCSharp.value.trim());
                    localStorage.setItem('cfg_url_php', inputPHP.value.trim());
                    localStorage.setItem('cfg_url_node', inputNode.value.trim());
                    location.reload();
                });
            }

            const btnReset = document.getElementById('btnResetConexiones');
            if (btnReset) {
                btnReset.addEventListener('click', () => {
                    localStorage.removeItem('cfg_url_csharp');
                    localStorage.removeItem('cfg_url_php');
                    localStorage.removeItem('cfg_url_node');
                    location.reload();
                });
            }
        });
    </script>
</body>
</html>
