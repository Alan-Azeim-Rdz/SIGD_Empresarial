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
        :root {
            --bg-dark:   #0d1117;
            --bg-card:   #161b22;
            --bg-card2:  #1c2128;
            --accent1:   #58a6ff;
            --accent2:   #3fb950;
            --accent3:   #f78166;
            --accent4:   #d2a8ff;
            --border:    #30363d;
            --text-main: #e6edf3;
            --text-muted:#8b949e;
        }

        * { box-sizing: border-box; }

        body {
            background: var(--bg-dark);
            color: var(--text-main);
            font-family: 'Segoe UI', system-ui, -apple-system, sans-serif;
            min-height: 100vh;
        }

        /* ── Sidebar ── */
        .sidebar {
            width: 240px;
            min-height: 100vh;
            background: var(--bg-card);
            border-right: 1px solid var(--border);
            display: flex;
            flex-direction: column;
            padding: 1.5rem 1rem;
            position: fixed;
            top: 0; left: 0;
            z-index: 100;
        }
        .sidebar-logo {
            font-size: 1.15rem;
            font-weight: 800;
            color: var(--accent1);
            margin-bottom: 2rem;
            letter-spacing: -.5px;
        }
        .sidebar-logo span { color: var(--text-main); }
        .nav-link-side {
            display: flex;
            align-items: center;
            gap: .7rem;
            color: var(--text-muted);
            padding: .55rem .75rem;
            border-radius: .5rem;
            text-decoration: none;
            font-size: .9rem;
            transition: background .15s, color .15s;
            margin-bottom: .2rem;
        }
        .nav-link-side:hover, .nav-link-side.active {
            background: rgba(88,166,255,.12);
            color: var(--accent1);
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

        /* ── Page header ── */
        .page-header {
            display: flex;
            align-items: center;
            justify-content: space-between;
            margin-bottom: 2rem;
        }
        .page-header h1 {
            font-size: 1.55rem;
            font-weight: 700;
            margin: 0;
        }
        .page-header .badge-live {
            background: rgba(63,185,80,.15);
            color: var(--accent2);
            border: 1px solid rgba(63,185,80,.4);
            border-radius: 2rem;
            padding: .3rem .9rem;
            font-size: .8rem;
            font-weight: 600;
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
            border-radius: 1rem;
            padding: 1.3rem 1.5rem;
            display: flex;
            flex-direction: column;
            gap: .3rem;
            transition: transform .15s, box-shadow .15s;
        }
        .kpi-card:hover { transform: translateY(-2px); box-shadow: 0 6px 24px rgba(0,0,0,.3); }
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
        .kpi-card .kpi-label { font-size: .8rem; color: var(--text-muted); }

        .kpi-blue  .kpi-icon { background: rgba(88,166,255,.15);  color: var(--accent1); }
        .kpi-green .kpi-icon { background: rgba(63,185,80,.15);   color: var(--accent2); }
        .kpi-red   .kpi-icon { background: rgba(247,129,102,.15); color: var(--accent3); }
        .kpi-purple.kpi-icon { background: rgba(210,168,255,.15); color: var(--accent4); }
        .kpi-blue  .kpi-value { color: var(--accent1); }
        .kpi-green .kpi-value { color: var(--accent2); }
        .kpi-red   .kpi-value { color: var(--accent3); }
        .kpi-purple .kpi-value { color: var(--accent4); }

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
            border-radius: 1rem;
            padding: 1.4rem;
        }
        .chart-card h2 {
            font-size: 1rem;
            font-weight: 600;
            color: var(--text-main);
            margin-bottom: 1rem;
        }
        .chart-card h2 i { color: var(--text-muted); margin-right: .4rem; }

        /* ── Tabla de actividad ── */
        .activity-table {
            background: var(--bg-card);
            border: 1px solid var(--border);
            border-radius: 1rem;
            padding: 1.4rem;
            margin-bottom: 2rem;
        }
        .activity-table h2 {
            font-size: 1rem;
            font-weight: 600;
            margin-bottom: 1rem;
        }
        table.sigd-table { width: 100%; border-collapse: collapse; }
        table.sigd-table th {
            text-align: left;
            font-size: .78rem;
            color: var(--text-muted);
            padding: .5rem .8rem;
            border-bottom: 1px solid var(--border);
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: .05em;
        }
        table.sigd-table td {
            padding: .75rem .8rem;
            font-size: .87rem;
            border-bottom: 1px solid rgba(48,54,61,.6);
        }
        table.sigd-table tr:last-child td { border-bottom: none; }
        table.sigd-table tr:hover td { background: rgba(255,255,255,.025); }

        .badge-version {
            background: rgba(88,166,255,.15);
            color: var(--accent1);
            border-radius: .4rem;
            padding: .15rem .55rem;
            font-size: .75rem;
            font-weight: 700;
        }
        .badge-depto {
            background: rgba(210,168,255,.12);
            color: var(--accent4);
            border-radius: .4rem;
            padding: .15rem .55rem;
            font-size: .73rem;
        }

        /* ── Spinner & Empty ── */
        .skeleton {
            background: linear-gradient(90deg, var(--bg-card2) 25%, var(--bg-card) 50%, var(--bg-card2) 75%);
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
            background: linear-gradient(135deg, var(--accent1), var(--accent4));
            color: #fff !important;
            border: none;
            border-radius: .6rem;
            padding: .55rem 1.2rem;
            font-size: .88rem;
            font-weight: 600;
            text-decoration: none;
            display: inline-flex;
            align-items: center;
            gap: .5rem;
            transition: opacity .2s;
        }
        .portal-btn:hover { opacity: .85; }

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
        <a href="http://localhost:3000" target="_blank" class="nav-link-side">
            <i class="fas fa-database fa-fw"></i> API Búsqueda
        </a>
    </nav>
</aside>

<!-- ══════════════ MAIN ══════════════ -->
<div class="main-content">

    <!-- Header -->
    <div class="page-header">
        <div>
            <h1><i class="fas fa-chart-bar me-2" style="color:var(--accent1)"></i>Dashboard de Reportes</h1>
            <small style="color:var(--text-muted)">Métricas en tiempo real — Base de datos PostgreSQL</small>
        </div>
        <div class="d-flex gap-2 align-items-center">
            <span class="badge-live"><i class="fas fa-circle me-1" style="font-size:.55rem"></i>En vivo</span>
            <a href="?action=portal" class="portal-btn">
                <i class="fas fa-users"></i> Portal Operario
            </a>
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
const BASE = window.location.origin + window.location.pathname.replace(/\?.*/, '');

// ── Paleta de colores Chart.js ──
const COLORS = ['#58a6ff','#3fb950','#f78166','#d2a8ff','#ffa657','#79c0ff','#56d364','#ff7b72'];

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
                datasets: [{ data, backgroundColor: COLORS, borderColor: '#161b22', borderWidth: 3 }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: { color: '#8b949e', padding: 12, font: { size: 11 } }
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
                '<p style="color:var(--text-muted);text-align:center;font-size:.88rem">Sin datos en los últimos 12 meses. Los documentos aparecerán conforme sean aprobados.</p>';
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
                    borderColor: '#58a6ff',
                    backgroundColor: 'rgba(88,166,255,.08)',
                    fill: true,
                    tension: .35,
                    pointBackgroundColor: '#58a6ff',
                    pointRadius: 5,
                    pointHoverRadius: 7,
                }]
            },
            options: {
                responsive: true,
                interaction: { mode: 'index', intersect: false },
                scales: {
                    x: { ticks: { color: '#8b949e', font: { size: 11 } }, grid: { color: '#30363d' } },
                    y: { ticks: { color: '#8b949e', font: { size: 11 }, stepSize: 1 }, grid: { color: '#30363d' }, beginAtZero: true }
                },
                plugins: { legend: { labels: { color: '#e6edf3' } } }
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

</body>
</html>
