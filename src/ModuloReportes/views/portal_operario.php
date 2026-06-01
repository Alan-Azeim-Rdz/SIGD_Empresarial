<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Portal de Normativas | Planta</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet">
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" rel="stylesheet">
    <style>
        @import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap');

        :root {
            /* Paleta principal — moderno oscuro */
            --sapphire:       #3B82F6;
            --sapphire-light: #60A5FA;
            --sapphire-dark:  #1E40AF;
            --emerald:        #06B6D4;
            --emerald-light:  #22D3EE;
            --emerald-dark:   #0891B2;
            --amethyst:       #8B5CF6;
            --amber:          #F59E0B;
            --crimson:        #EF4444;
            --bg:             #0A0E1A;
            --bg-card:        #141A2A;
            --bg-hover:       #1E2538;
            --text-primary:   #F8FAFC;
            --text-secondary: #94A3B8;
            --text-muted:     #64748B;
            --border:         #1E2538;
            --radius-md:      .85rem;
            --radius-sm:      .5rem;
            --shadow-sm:      0 1px 4px rgba(0,0,0,.4);
            --shadow-md:      0 4px 16px rgba(0,0,0,.4);
        }

        body {
            font-family: 'Inter', 'Segoe UI', system-ui, sans-serif;
            background-color: var(--bg);
            color: var(--text-primary);
            line-height: 1.6;
        }

        h2 {
            font-weight: 800;
            color: var(--text-primary);
            letter-spacing: -.02em;
        }

        .navbar-custom {
            background: linear-gradient(135deg, var(--sapphire-dark) 0%, var(--sapphire) 100%);
            box-shadow: 0 2px 12px rgba(0,0,0,.15);
            padding: .6rem 1rem;
            border-bottom: none;
        }
        .navbar-custom .navbar-brand {
            font-weight: 800;
            color: #fff !important;
        }
        .navbar-custom .navbar-brand span {
            color: var(--amber);
        }
        .navbar-custom .nav-link {
            color: rgba(255,255,255,.85) !important;
            font-weight: 500;
            border-radius: var(--radius-sm);
            transition: background .18s, color .18s;
        }
        .navbar-custom .nav-link:hover,
        .navbar-custom .nav-link.active {
            color: #fff !important;
            background: rgba(255,255,255,.12);
        }

        .card {
            background: var(--bg-card);
            border: 1px solid var(--border);
            border-radius: var(--radius-md);
            box-shadow: var(--shadow-sm);
            transition: box-shadow .2s;
        }
        .card:hover {
            box-shadow: var(--shadow-md);
        }

        .form-label {
            font-weight: 600;
            font-size: .85rem;
            color: var(--text-primary);
        }
        .form-control {
            border: 1px solid var(--border);
            border-radius: var(--radius-sm);
            color: var(--text-primary);
            background-color: #0A0E1A !important;
            padding: .55rem .9rem;
            transition: border-color .18s, box-shadow .18s;
        }
        .form-control:focus {
            border-color: var(--sapphire);
            box-shadow: 0 0 0 3px rgba(59,130,246,.15);
            background-color: #0A0E1A !important;
            color: var(--text-primary);
        }
        .input-group-text {
            background-color: #0A0E1A !important;
            border: 1px solid var(--border);
            color: var(--text-muted);
        }

        /* Tabla custom */
        .table {
            border-collapse: separate;
            border-spacing: 0;
            font-size: .875rem;
            border-radius: var(--radius-md);
            overflow: hidden;
            border: 1px solid var(--border);
        }
        .table thead th {
            background: #1E2538 !important;
            color: #94A3B8 !important;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: .04em;
            padding: .75rem 1rem;
            border: none;
        }
        .table tbody tr {
            background-color: var(--bg-card);
            transition: background .15s;
        }
        .table tbody tr:hover {
            background-color: rgba(59,130,246,.05) !important;
        }
        .table tbody td {
            padding: .75rem 1rem;
            border-bottom: 1px solid var(--border);
            vertical-align: middle;
            color: var(--text-primary);
        }
        .table tr:last-child td {
            border-bottom: none;
        }

        /* Botones custom */
        .btn-jewel-primary {
            background: linear-gradient(135deg, var(--sapphire) 0%, var(--emerald) 100%);
            color: #fff;
            border: none;
            box-shadow: 0 2px 8px rgba(59,130,246,.25);
            font-weight: 600;
        }
        .btn-jewel-primary:hover {
            background: linear-gradient(135deg, var(--sapphire-dark) 0%, var(--sapphire) 100%);
            color: #fff;
            box-shadow: 0 4px 14px rgba(59,130,246,.35);
            transform: translateY(-1px);
        }

        .btn-outline-jewel {
            border: 1.5px solid var(--sapphire);
            color: var(--sapphire-light);
            background: transparent;
            font-weight: 600;
        }
        .btn-outline-jewel:hover {
            background: var(--sapphire);
            color: #fff;
        }

        .badge-version {
            background: rgba(59,130,246,.15);
            color: var(--sapphire-light);
            border: 1px solid rgba(59,130,246,.25);
            font-weight: 600;
            padding: .3em .7em;
            border-radius: 2rem;
            font-size: .72rem;
        }

        .badge-codigo {
            background: rgba(6,182,212,.15);
            color: var(--emerald-light);
            border: 1px solid rgba(6,182,212,.25);
            font-weight: 600;
            padding: .3em .7em;
            border-radius: 2rem;
            font-size: .72rem;
        }

        /* Modales */
        .modal-content {
            border: none;
            border-radius: var(--radius-md);
            box-shadow: var(--shadow-md);
            background: var(--bg-card);
            color: var(--text-primary);
        }
        .modal-header {
            background: linear-gradient(135deg, var(--sapphire) 0%, var(--sapphire-light) 100%);
            color: #fff;
            border-radius: var(--radius-md) var(--radius-md) 0 0;
            padding: 1.1rem 1.5rem;
            border-bottom: none;
        }
        .modal-header .modal-title { font-weight: 700; }
        .modal-header .btn-close { filter: invert(1) brightness(2); }
        .modal-body { padding: 1.5rem; }
        .modal-footer { border-top: 1px solid var(--border); padding: 1rem 1.5rem; }

        /* Alertas */
        .alert {
            border: none;
            border-radius: var(--radius-sm);
            font-size: .9rem;
            padding: 1rem 1.25rem;
        }
        .alert-success { background: rgba(6,182,212,.08); color: var(--emerald-light); border-left: 4px solid var(--emerald); }
        .alert-danger  { background: rgba(239,68,68,.08); color: #F87171; border-left: 4px solid var(--crimson); }

        /* Ajustes de Iframe */
        body.in-iframe {
            background-color: transparent !important;
            padding: 0;
        }
        body.in-iframe .navbar-custom {
            display: none !important;
        }
        body.in-iframe .container {
            padding-top: 0.5rem;
            max-width: 100%;
        }

        /* ── Page header hero ── */
        .page-hero {
            background: linear-gradient(135deg, var(--sapphire-dark) 0%, var(--sapphire) 55%, var(--sapphire-light) 100%);
            border-radius: var(--radius-md);
            padding: 2rem;
            color: #fff;
            margin-bottom: 1.75rem;
            box-shadow: var(--shadow-sm);
            position: relative;
            overflow: hidden;
        }
        .page-hero::after {
            content: '';
            position: absolute;
            right: -60px; top: -60px;
            width: 220px; height: 220px;
            background: rgba(255,255,255,.04);
            border-radius: 50%;
        }
        .page-hero h1 { font-size: 1.7rem; font-weight: 800; color: #fff; margin-bottom: .3rem; }
        .page-hero p  { color: rgba(255,255,255,.78); margin: 0; font-size: .95rem; }

        body.in-iframe .page-hero {
            display: none !important;
        }
    </style>
</head>
<body>
    <!-- Cabecera de Navegación -->
    <nav class="navbar navbar-expand-lg navbar-dark navbar-custom mb-4">
        <div class="container">
            <span class="navbar-brand fw-bold"><i class="fas fa-industry me-2" style="color: var(--amber);"></i>Portal <span>Planta</span></span>
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
                        <a id="lnk-modulo-central" class="btn btn-outline-jewel btn-sm" href="http://localhost:5000" target="_blank">
                            <i class="fas fa-arrow-up-right-from-square me-1"></i> Módulo Central C#
                        </a>
                    </li>
                    <li class="nav-item ms-2">
                        <button class="btn btn-outline-jewel btn-sm" data-bs-toggle="modal" data-bs-target="#modalConexiones" title="Configuración de Conexiones">
                            <i class="fas fa-cog"></i>
                        </button>
                    </li>
                </ul>
            </div>
        </div>
    </nav>

    <div class="container">
        <div class="page-hero animate-fade-in">
            <h1><i class="fas fa-book me-2"></i>Portal de Documentos Vigentes</h1>
            <p>Consulta, descarga y confirma de forma rápida la lectura de normativas vigentes en planta.</p>
        </div>
        
        <div class="card mb-4">
            <div class="card-body">
                <label for="inputBusqueda" class="form-label mb-2">Buscar normativa por título o contenido clave (Full-Text Search):</label>
                <div class="input-group">
                    <span class="input-group-text bg-white border-end-0"><i class="fas fa-search text-muted"></i></span>
                    <input type="text" id="inputBusqueda" class="form-control border-start-0 ps-1" placeholder="Ej. Manual de Calidad, ISO 9001, Soldadura...">
                </div>
            </div>
        </div>

        <div class="table-responsive">
            <table class="table table-hover align-middle">
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
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title fw-bold" id="modalConexionesLabel"><i class="fas fa-network-wired me-2"></i> Configuración de Conexiones</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <p class="small text-muted mb-3">Define las URLs del ecosistema para la navegación del cliente y llamadas de API. De forma predeterminada, se detecta el host actual y cambia el puerto.</p>
                    <div class="mb-3">
                        <label for="cfg_csharp" class="form-label fw-bold">Módulo Central (C# - Puerto 5000)</label>
                        <input type="text" id="cfg_csharp" class="form-control" placeholder="http://localhost:5000" />
                    </div>
                    <div class="mb-3">
                        <label for="cfg_php" class="form-label fw-bold">Módulo de Reportes/Portal (PHP - Puerto 8000)</label>
                        <input type="text" id="cfg_php" class="form-control" placeholder="http://localhost:8000" />
                    </div>
                    <div class="mb-3">
                        <label for="cfg_node" class="form-label fw-bold">Buscador Global (Node.js - Puerto 3000)</label>
                        <input type="text" id="cfg_node" class="form-control" placeholder="http://localhost:3000" />
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-outline-jewel btn-sm" id="btnResetConexiones">Restablecer</button>
                    <button type="button" class="btn btn-jewel-primary btn-sm" id="btnGuardarConexiones">Guardar Cambios</button>
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
                const response = await fetch(`${resolvedUrls.node}/buscar?q=${encodeURIComponent(query)}`);
                const data = await response.json();
                
                renderizarTabla(data.data || []);
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
                        <td><span class="badge badge-codigo">${doc.codigo_interno}</span></td>
                        <td><strong>${doc.titulo}</strong></td>
                        <td><span class="badge badge-version">v${doc.version ?? 1}</span></td>
                        <td class="text-end">
                            <a href="${resolvedUrls.csharp}/Documento/DescargarUltima/${doc.id_documento_sql}" class="btn btn-outline-jewel btn-sm me-2" target="_blank">
                                <i class="fas fa-download me-1"></i> Descargar PDF
                            </a>
                            <button class="btn btn-jewel-primary btn-sm" onclick="firmarLectura(${doc.id_documento_sql})">
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
        // Detectar si está dentro de un iframe
        if (window.self !== window.top) {
            document.body.classList.add('in-iframe');
        }

        async function cargarDocumentosIniciales() {
            try {
                tablaResultados.innerHTML = '<tr><td colspan="4" class="text-center text-muted"><i class="fas fa-spinner fa-spin me-2"></i>Cargando normativas vigentes...</td></tr>';
                const response = await fetch(`${resolvedUrls.node}/buscar?q=PRO`);
                if (!response.ok) throw new Error("Fallo en la respuesta del buscador");
                const data = await response.json();
                renderizarTabla(data.data || []);
            } catch (error) {
                console.error("Error al cargar documentos iniciales:", error);
                tablaResultados.innerHTML = '<tr><td colspan="4" class="text-center text-danger"><i class="fas fa-exclamation-circle me-2"></i>No se pudieron cargar las normativas de forma automática. Verifica que el microservicio de búsqueda (Node.js en puerto 3000) esté activo o intenta buscar manualmente.</td></tr>';
            }
        }

        document.addEventListener('DOMContentLoaded', () => {
            rewriteLinks();
            cargarDocumentosIniciales();
            
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
