// ============================================================
// SIGD Empresarial — Módulo Central (.NET 10)
// EJEMPLO DE USO: Cómo llamar a ReportesIntegrationService
// desde cualquier Controller del Módulo Central.
//
// INSTRUCCIÓN: No copies este archivo al proyecto. Es solo
// una guía de referencia de los tres patrones de uso.
// Integra las líneas relevantes en tus acciones existentes.
// ============================================================

// ── 1. INYECCIÓN POR CONSTRUCTOR ─────────────────────────────
//
//   Agrega ReportesIntegrationService al constructor del
//   controller que publique/apruebe documentos:
//
//   private readonly ReportesIntegrationService _reportesSync;
//
//   public MiController(DirContext context,
//                       ReportesIntegrationService reportesSync)
//   {
//       _context      = context;
//       _reportesSync = reportesSync;
//   }

// ── 2. SINCRONIZAR UN DOCUMENTO TRAS APROBARLO ───────────────
//
//   Llámalo al final de cualquier acción que publique un doc:
//
//   [HttpPost]
//   public async Task<IActionResult> PublicarDocumento(int id)
//   {
//       // ... lógica de publicación en SQL Server ...
//       await _context.SaveChangesAsync();
//
//       // Disparar sincronización asíncrona al Módulo de Reportes
//       // Fire-and-forget: la respuesta no bloquea al usuario,
//       // pero el resultado queda en evento_integracion para auditoría.
//       _ = Task.Run(() =>
//           _reportesSync.SincronizarDocumentoAsync(id, GetCurrentUserId()));
//
//       return RedirectToAction("Documentos");
//   }

// ── 3. SINCRONIZACIÓN MASIVA (ENDPOINT DE ADMINISTRACIÓN) ────
//
//   [HttpPost]
//   [Authorize(Roles = "Administrador")]
//   public async Task<IActionResult> SincronizarTodosConReportes()
//   {
//       await _reportesSync.SincronizarTodosAsync(GetCurrentUserId());
//       TempData["Mensaje"] = "Sincronización masiva completada.";
//       return RedirectToAction("Index");
//   }

// ── 4. HEALTH CHECK DESDE UNA VISTA DE DIAGNÓSTICO ───────────
//
//   public async Task<IActionResult> DiagnosticoIntegracion()
//   {
//       bool reportesOk = await _reportesSync.PingReportesAsync();
//       ViewBag.ReportesOnline = reportesOk;
//       return View();
//   }
