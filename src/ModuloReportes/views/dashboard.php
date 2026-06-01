<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Dashboard de Reportes | SIGD Empresarial</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.0/dist/chart.umd.min.js"></script>
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
            --amethyst-light: #A78BFA;
            --amber:          #F59E0B;
            --crimson:        #EF4444;
            --crimson-light:  #F87171;

            /* Neutros */
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

        * { box-sizing: border-box; }

        body {
            background-color: var(--bg);
            color: var(--text-primary);
            font-family: 'Inter', 'Segoe UI', system-ui, sans-serif;
            min-height: 100vh;
            line-height: 1.6;
        }

        /* ── Sidebar ── */
        .sidebar {
            width: 240px;
            min-height: 100vh;
            background: var(--bg-card);
            border-right: 1px solid var(--border);
            display: flex;
            flex-direction: column;
            padding: 1.5rem 1.2rem;
            position: fixed;
            top: 0; left: 0;
            z-index: 100;
        }
        .sidebar-logo {
            font-size: 1.15rem;
            font-weight: 800;
            color: var(--text-primary);
            margin-bottom: 2rem;
            letter-spacing: -.5px;
        }
        .sidebar-logo span { color: var(--amber); }
        .nav-link-side {
            display: flex;
            align-items: center;
            gap: .7rem;
            color: var(--text-secondary);
            padding: .6rem .8rem;
            border-radius: var(--radius-sm);
            text-decoration: none;
            font-size: .9rem;
            font-weight: 500;
            transition: background .15s, color .15s;
            margin-bottom: .3rem;
        }
        .nav-link-side:hover, .nav-link-side.active {
            background: var(--bg-hover);
            color: var(--sapphire-light);
        }
        .sidebar-divider {
            border-top: 1px solid var(--border);
            margin: 1rem 0;
        }

        /* ── Main content ── */
        .main-content {
            margin-left: 240px;
            padding: 2rem 2.5rem;
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

        .page-hero .badge-live {
            background: rgba(255, 255, 255, .15);
            color: #fff;
            border: 1px solid rgba(255, 255, 255, .3);
            border-radius: 2rem;
            padding: .3rem .9rem;
            font-size: .8rem;
            font-weight: 600;
        }

        body.in-iframe .page-hero {
            display: none !important;
        }

        /* ── KPI Cards ── */
        .kpi-grid {
            display: grid;
            grid-template-columns: repeat(4, 1fr);
            gap: 1.2rem;
            margin-bottom: 2rem;
        }
        .kpi-card {
            background: var(--bg-card);
            border: 1px solid var(--border);
            border-radius: var(--radius-md);
            padding: 1.3rem 1.5rem;
            display: flex;
            flex-direction: column;
            gap: .3rem;
            box-shadow: var(--shadow-sm);
            transition: transform .15s, box-shadow .15s;
        }
        .kpi-card:hover {
            transform: translateY(-2px);
            box-shadow: var(--shadow-md);
        }
        .kpi-card .kpi-icon {
            width: 40px; height: 40px;
            border-radius: .7rem;
            display: flex; align-items: center; justify-content: center;
            font-size: 1.1rem;
            margin-bottom: .3rem;
        }
        .kpi-card .kpi-value {
            font-size: 2rem;
            font-weight: 800;
            line-height: 1;
        }
        .kpi-card .kpi-label { font-size: .8rem; color: var(--text-secondary); font-weight: 500; }

        .kpi-blue  .kpi-icon { background: rgba(59,130,246,.15);  color: var(--sapphire-light); }
        .kpi-green .kpi-icon { background: rgba(6,182,212,.15);   color: var(--emerald); }
        .kpi-red   .kpi-icon { background: rgba(239,68,68,.15); color: var(--crimson); }
        .kpi-purple .kpi-icon { background: rgba(139,92,246,.15); color: var(--amethyst); }
        .kpi-blue  .kpi-value { color: var(--sapphire-light); }
        .kpi-green .kpi-value { color: var(--emerald-light); }
        .kpi-red   .kpi-value { color: var(--crimson); }
        .kpi-purple .kpi-value { color: var(--amethyst); }

        /* ── Chart containers ── */
        .charts-row {
            display: grid;
            grid-template-columns: 1fr 1.5fr;
            gap: 1.2rem;
            margin-bottom: 1.2rem;
        }
        .chart-card {
            background: var(--bg-card);
            border: 1px solid var(--border);
            border-radius: var(--radius-md);
            box-shadow: var(--shadow-sm);
            padding: 1.4rem;
        }
        .chart-card h2 {
            font-size: 1.05rem;
            font-weight: 700;
            color: var(--text-primary);
            margin-bottom: 1rem;
        }
        .chart-card h2 i { color: var(--text-secondary); margin-right: .4rem; }

        /* ── Tabla de actividad ── */
        .activity-table {
            background: var(--bg-card);
            border: 1px solid var(--border);
            border-radius: var(--radius-md);
            box-shadow: var(--shadow-sm);
            padding: 1.4rem;
            margin-bottom: 2rem;
        }
        .activity-table h2 {
            font-size: 1.05rem;
            font-weight: 700;
            color: var(--text-primary);
            margin-bottom: 1rem;
        }
        table.sigd-table { width: 100%; border-collapse: collapse; }
        table.sigd-table th {
            text-align: left;
            font-size: .78rem;
            color: var(--text-secondary);
            padding: .6rem .8rem;
            border-bottom: 1.5px solid var(--border);
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: .05em;
        }
        table.sigd-table td {
            padding: .85rem .8rem;
            font-size: .87rem;
            border-bottom: 1px solid var(--border);
            color: var(--text-primary);
        }
        table.sigd-table tr:last-child td { border-bottom: none; }
        table.sigd-table tr:hover td { background: var(--bg-hover); }

        .badge-version {
            background: rgba(59,130,246,.15);
            color: var(--sapphire-light);
            border: 1px solid rgba(59,130,246,.25);
            border-radius: .4rem;
            padding: .15rem .55rem;
            font-size: .75rem;
            font-weight: 700;
        }
        .badge-depto {
            background: rgba(139,92,246,.15);
            color: var(--amethyst);
            border: 1px solid rgba(139,92,246,.25);
            border-radius: .4rem;
            padding: .15rem .55rem;
            font-size: .73rem;
            font-weight: 600;
        }

        /* ── Spinner & Empty ── */
        .skeleton {
            background: linear-gradient(90deg, var(--bg-hover) 25%, var(--bg-card) 50%, var(--bg-hover) 75%);
            background-size: 200% 100%;
            animation: shimmer 1.5s infinite;
            border-radius: .5rem;
            height: 2rem;
        }
        @keyframes shimmer {
            0% { background-position: 200% 0; }
            100% { background-position: -200% 0; }
        }

        /* ── Portal link ── */
        .portal-btn {
            background: linear-gradient(135deg, var(--sapphire) 0%, var(--emerald) 100%);
            color: #fff !important;
            border: none;
            border-radius: .6rem;
            padding: .55rem 1.2rem;
            font-size: .88rem;
            font-weight: 600;
            box-shadow: 0 2px 8px rgba(59,130,246,.25);
            text-decoration: none;
            display: inline-flex;
            align-items: center;
            gap: .5rem;
            transition: opacity .2s, transform .2s;
        }
        .portal-btn:hover {
            box-shadow: 0 4px 14px rgba(59,130,246,.35);
            transform: translateY(-1px);
        }

        /* Ajustes de Iframe */
        body.in-iframe {
            background-color: transparent !important;
            padding: 0;
        }
        body.in-iframe .sidebar {
            display: none !important;
        }
        body.in-iframe .main-content {
            margin-left: 0 !important;
            padding: 1rem !important;
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
        }
        .form-control:focus {
            border-color: var(--sapphire);
            box-shadow: 0 0 0 3px rgba(59,130,246,.15);
            background-color: #0A0E1A !important;
            color: var(--text-primary);
        }

        @media (max-width: 900px) {
            .sidebar { display: none; }
            .main-content { margin-left: 0; padding: 1rem; }
            .kpi-grid { grid-template-columns: repeat(2, 1fr); }
            .charts-row { grid-template-columns: 1fr; }
        }
    </style>
</head>
<body>

<!-- ══════════════ SIDEBAR ══════════════ -->
<aside class="sidebar">
    <div class="sidebar-logo">SIGD <span>Reportes</span></div>
    <nav>
        <a href="?action=dashboard" class="nav-link-side active">
            <i class="fas fa-chart-line fa-fw"></i> Dashboard
        </a>
        <a href="?action=portal" class="nav-link-side">
            <i class="fas fa-file-alt fa-fw"></i> Portal Operario
        </a>
        <div class="sidebar-divider"></div>
        <a href="http://localhost:5000" target="_blank" class="nav-link-side">
            <i class="fas fa-arrow-up-right-from-square fa-fw"></i> Módulo Central
        </a>
        <a href="http://localhost:5000/Busqueda/Global" target="_blank" class="nav-link-side">
            <i class="fas fa-search fa-fw"></i> Buscador Global
        </a>
        <div class="sidebar-divider"></div>
        <a href="#" class="nav-link-side" data-bs-toggle="modal" data-bs-target="#modalConexiones" id="btn-config-conexiones" style="color: var(--sapphire-light) !important;">
            <i class="fas fa-cog fa-fw"></i> Conexiones
        </a>
    </nav>
</aside>

<!-- ══════════════ MAIN ══════════════ -->
<div class="main-content">

    <!-- Header -->
    <div class="page-hero animate-fade-in">
        <div class="d-flex align-items-center justify-content-between flex-wrap gap-3">
            <div>
                <h1><i class="fas fa-chart-bar me-2"></i>Dashboard de Reportes</h1>
                <p>Monitorea y analiza métricas de publicación y acuses de lectura en tiempo real.</p>
            </div>
            <div class="d-flex gap-2 align-items-center">
                <span class="badge-live"><i class="fas fa-circle me-1" style="font-size:.55rem"></i>En vivo</span>
                <a href="?action=portal" class="portal-btn">
                    <i class="fas fa-users me-1"></i>Portal Operario
                </a>
            </div>
        </div>
    </div>

    <!-- KPI Cards -->
    <div class="kpi-grid" id="kpi-grid">
        <div class="kpi-card kpi-blue">
            <div class="kpi-icon"><i class="fas fa-file-alt"></i></div>
            <div class="kpi-value" id="kpi-docs">—</div>
            <div class="kpi-label">Documentos Vigentes</div>
        </div>
        <div class="kpi-card kpi-green">
            <div class="kpi-icon"><i class="fas fa-building"></i></div>
            <div class="kpi-value" id="kpi-deptos">—</div>
            <div class="kpi-label">Departamentos con Docs</div>
        </div>
        <div class="kpi-card kpi-red">
            <div class="kpi-icon"><i class="fas fa-check-double"></i></div>
            <div class="kpi-value" id="kpi-acuses">—</div>
            <div class="kpi-label">Acuses de Lectura</div>
        </div>
        <div class="kpi-card">
            <div class="kpi-icon" style="background:rgba(210,168,255,.15);color:var(--accent4)">
                <i class="fas fa-calendar-check"></i>
            </div>
            <div class="kpi-value" id="kpi-fecha" style="font-size:1rem;padding-top:.5rem;color:var(--accent4)">—</div>
            <div class="kpi-label">Última Publicación</div>
        </div>
    </div>

    <!-- Charts Row -->
    <div class="charts-row">
        <div class="chart-card">
            <h2><i class="fas fa-chart-pie"></i>Docs por Departamento</h2>
            <canvas id="chartDepto" height="250"></canvas>
        </div>
        <div class="chart-card">
            <h2><i class="fas fa-chart-line"></i>Evolución de Publicaciones (últimos 12 meses)</h2>
            <canvas id="chartEvolucion" height="250"></canvas>
        </div>
    </div>

    <!-- Activity Table -->
    <div class="activity-table">
        <h2><i class="fas fa-history me-2" style="color:var(--text-muted)"></i>Últimos Documentos Publicados</h2>
        <table class="sigd-table">
            <thead>
                <tr>
                    <th>Código</th>
                    <th>Título</th>
                    <th>Versión</th>
                    <th>Departamento</th>
                    <th>Fecha de Publicación</th>
                </tr>
            </thead>
            <tbody id="tabla-recientes">
                <tr><td colspan="5" class="text-center" style="color:var(--text-muted);padding:2rem">
                    <i class="fas fa-spinner fa-spin me-2"></i>Cargando datos...
                </td></tr>
            </tbody>
        </table>
    </div>

</div><!-- /main-content -->

<script>
// Detectar si está en un iframe
if (window.self !== window.top) {
    document.body.classList.add('in-iframe');
}

const BASE = window.location.origin + window.location.pathname.replace(/\?.*/, '');

// ── Paleta de colores Chart.js ──
const COLORS = ['#3B82F6', '#06B6D4', '#8B5CF6', '#F59E0B', '#EF4444', '#60A5FA', '#22D3EE', '#A78BFA'];

// ── 1. KPIs ──
fetch(`${BASE}?action=api_kpis`)
    .then(r => r.json())
    .then(d => {
        document.getElementById('kpi-docs').textContent    = d.total_documentos   ?? '0';
        document.getElementById('kpi-deptos').textContent  = d.total_departamentos ?? '0';
        document.getElementById('kpi-acuses').textContent  = d.total_acuses        ?? '0';
        const fecha = d.ultima_publicacion;
        document.getElementById('kpi-fecha').textContent   = fecha !== 'Sin documentos' && fecha
            ? new Date(fecha).toLocaleDateString('es-MX', { day:'2-digit', month:'short', year:'numeric' })
            : 'Sin datos';
    })
    .catch(() => {
        ['kpi-docs','kpi-deptos','kpi-acuses','kpi-fecha'].forEach(id => {
            document.getElementById(id).textContent = 'N/A';
        });
    });

// ── 2. Gráfica de barras: Docs por Departamento ──
fetch(`${BASE}?action=api_docs_por_depto`)
    .then(r => r.json())
    .then(rows => {
        if (rows.error) return;
        const labels = rows.map(r => `Depto ${r.departamento}`);
        const data   = rows.map(r => parseInt(r.total));
        new Chart(document.getElementById('chartDepto'), {
            type: 'doughnut',
            data: {
                labels,
                datasets: [{ data, backgroundColor: COLORS, borderColor: '#141A2A', borderWidth: 3 }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: { color: '#94A3B8', padding: 12, font: { size: 11 } }
                    },
                    tooltip: {
                        callbacks: {
                            label: ctx => ` ${ctx.label}: ${ctx.parsed} docs`
                        }
                    }
                }
            }
        });
    });

// ── 3. Gráfica de línea: Evolución mensual ──
fetch(`${BASE}?action=api_evolucion`)
    .then(r => r.json())
    .then(rows => {
        if (rows.error || rows.length === 0) {
            document.getElementById('chartEvolucion').parentElement.innerHTML +=
                '<p style="color:var(--text-secondary);text-align:center;font-size:.88rem">Sin datos en los últimos 12 meses. Los documentos aparecerán conforme sean aprobados.</p>';
            return;
        }
        const labels = rows.map(r => r.mes);
        const data   = rows.map(r => parseInt(r.total));
        new Chart(document.getElementById('chartEvolucion'), {
            type: 'line',
            data: {
                labels,
                datasets: [{
                    label: 'Documentos publicados',
                    data,
                    borderColor: '#3B82F6',
                    backgroundColor: 'rgba(59,130,246,.08)',
                    fill: true,
                    tension: .35,
                    pointBackgroundColor: '#3B82F6',
                    pointRadius: 5,
                    pointHoverRadius: 7,
                }]
            },
            options: {
                responsive: true,
                interaction: { mode: 'index', intersect: false },
                scales: {
                    x: { ticks: { color: '#94A3B8', font: { size: 11 } }, grid: { color: 'rgba(30,37,56,0.5)' } },
                    y: { ticks: { color: '#94A3B8', font: { size: 11 }, stepSize: 1 }, grid: { color: 'rgba(30,37,56,0.5)' }, beginAtZero: true }
                },
                plugins: { legend: { labels: { color: '#F8FAFC' } } }
            }
        });
    });

// ── 4. Tabla de documentos recientes ──
fetch(`${BASE}?action=api_recientes`)
    .then(r => r.json())
    .then(rows => {
        const tbody = document.getElementById('tabla-recientes');
        if (!rows.length) {
            tbody.innerHTML = '<tr><td colspan="5" class="text-center" style="color:var(--text-muted);padding:2rem">Aún no hay documentos sincronizados. Aprueba un documento en el Módulo Central para verlo aquí.</td></tr>';
            return;
        }
        tbody.innerHTML = rows.map(doc => `
            <tr>
                <td><span class="badge-version">${doc.codigo_interno}</span></td>
                <td>${doc.titulo}</td>
                <td><span class="badge-version">v${doc.version_actual}</span></td>
                <td><span class="badge-depto">Depto ${doc.id_departamento}</span></td>
                <td style="color:var(--text-muted)">${doc.fecha_formateada}</td>
            </tr>
        `).join('');
    })
    .catch(() => {
        document.getElementById('tabla-recientes').innerHTML =
            '<tr><td colspan="5" class="text-center" style="color:var(--text-muted)">Error cargando datos</td></tr>';
    });
</script>

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
        (function() {
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
        })();
    </script>
</body>
</html>
