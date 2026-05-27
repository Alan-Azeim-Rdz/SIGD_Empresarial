using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Gestion_de_Documentos.Models;
using Microsoft.EntityFrameworkCore;

namespace Gestion_de_Documentos.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly DirContext _context;

        public AdminController(DirContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        #region PANEL PRINCIPAL
        public IActionResult Index()
        {
            var stats = new AdminDashboardViewModel
            {
                TotalUsuarios = _context.Usuarios.Where(u => u.Estatus == true).Count(),
                TotalRoles = _context.Rols.Where(r => r.Estatus == true).Count(),
                TotalDepartamentos = _context.Departamentos.Where(d => d.Estatus == true).Count(),
                TotalTiposDocumento = _context.TipoDocumentos.Where(t => t.Estatus == true).Count()
            };
            return View(stats);
        }
        #endregion

        #region DEPARTAMENTOS
        public async Task<IActionResult> Departamentos()
        {
            var departamentos = await _context.Departamentos
                .Where(d => d.Estatus == true)
                .ToListAsync();
            return View(departamentos);
        }

        public IActionResult CrearDepartamento()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CrearDepartamento(Departamento departamento)
        {
            if (ModelState.IsValid)
            {
                // Verificar que no exista
                var existe = await _context.Departamentos
                    .AnyAsync(d => d.Nombre == departamento.Nombre && d.Estatus == true);

                if (existe)
                {
                    ModelState.AddModelError("Nombre", "Este departamento ya existe.");
                    return View(departamento);
                }

                departamento.Estatus = true;
                departamento.FechaCreacion = DateTime.Now;
                departamento.IdUsuarioCreacion = GetCurrentUserId();

                _context.Departamentos.Add(departamento);
                await _context.SaveChangesAsync();

                return RedirectToAction("Departamentos");
            }
            return View(departamento);
        }

        public async Task<IActionResult> EditarDepartamento(int id)
        {
            var departamento = await _context.Departamentos.FindAsync(id);
            if (departamento == null)
                return NotFound();
            return View(departamento);
        }

        [HttpPost]
        public async Task<IActionResult> EditarDepartamento(Departamento departamento)
        {
            if (ModelState.IsValid)
            {
                var existe = await _context.Departamentos
                    .AnyAsync(d => d.Nombre == departamento.Nombre && d.Id != departamento.Id && d.Estatus == true);

                if (existe)
                {
                    ModelState.AddModelError("Nombre", "Este nombre de departamento ya está en uso.");
                    return View(departamento);
                }

                var dptoActual = await _context.Departamentos.FindAsync(departamento.Id);
                dptoActual.Nombre = departamento.Nombre;
                dptoActual.Abreviatura = departamento.Abreviatura;
                dptoActual.FechaModificacion = DateTime.Now;
                dptoActual.IdUsuarioModificacion = GetCurrentUserId();

                await _context.SaveChangesAsync();
                return RedirectToAction("Departamentos");
            }
            return View(departamento);
        }

        [HttpPost]
        public async Task<IActionResult> EliminarDepartamento(int id)
        {
            var departamento = await _context.Departamentos.FindAsync(id);
            if (departamento != null)
            {
                departamento.Estatus = false;
                departamento.FechaEliminacion = DateTime.Now;
                departamento.IdUsuarioEliminacion = GetCurrentUserId();
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Departamentos");
        }
        #endregion

        #region ROLES
        public async Task<IActionResult> Roles()
        {
            var roles = await _context.Rols
                .Where(r => r.Estatus == true)
                .ToListAsync();
            return View(roles);
        }

        public IActionResult CrearRol()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CrearRol(Rol rol)
        {
            if (ModelState.IsValid)
            {
                var existe = await _context.Rols
                    .AnyAsync(r => r.Nombre == rol.Nombre && r.Estatus == true);

                if (existe)
                {
                    ModelState.AddModelError("Nombre", "Este rol ya existe.");
                    return View(rol);
                }

                rol.Estatus = true;
                rol.FechaCreacion = DateTime.Now;
                rol.IdUsuarioCreacion = GetCurrentUserId();

                _context.Rols.Add(rol);
                await _context.SaveChangesAsync();

                return RedirectToAction("Roles");
            }
            return View(rol);
        }

        public async Task<IActionResult> EditarRol(int id)
        {
            var rol = await _context.Rols.FindAsync(id);
            if (rol == null)
                return NotFound();
            return View(rol);
        }

        [HttpPost]
        public async Task<IActionResult> EditarRol(Rol rol)
        {
            if (ModelState.IsValid)
            {
                var existe = await _context.Rols
                    .AnyAsync(r => r.Nombre == rol.Nombre && r.Id != rol.Id && r.Estatus == true);

                if (existe)
                {
                    ModelState.AddModelError("Nombre", "Este nombre de rol ya está en uso.");
                    return View(rol);
                }

                var rolActual = await _context.Rols.FindAsync(rol.Id);
                rolActual.Nombre = rol.Nombre;
                rolActual.Descripcion = rol.Descripcion;
                rolActual.FechaModificacion = DateTime.Now;
                rolActual.IdUsuarioModificacion = GetCurrentUserId();

                await _context.SaveChangesAsync();
                return RedirectToAction("Roles");
            }
            return View(rol);
        }

        [HttpPost]
        public async Task<IActionResult> EliminarRol(int id)
        {
            var rol = await _context.Rols.FindAsync(id);
            if (rol != null)
            {
                rol.Estatus = false;
                rol.FechaEliminacion = DateTime.Now;
                rol.IdUsuarioEliminacion = GetCurrentUserId();
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Roles");
        }
        #endregion

        #region PERMISOS
        public async Task<IActionResult> Permisos()
        {
            var permisos = await _context.Permisos
                .Where(p => p.Estatus == true)
                .ToListAsync();
            return View(permisos);
        }

        public IActionResult CrearPermiso()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CrearPermiso(Permiso permiso)
        {
            if (ModelState.IsValid)
            {
                var existe = await _context.Permisos.AnyAsync(p => p.Codigo == permiso.Codigo && p.Estatus == true);

                if (existe)
                {
                    ModelState.AddModelError("Codigo", "Este código de permiso ya existe.");
                    return View(permiso);
                }

                permiso.Estatus = true;
                permiso.FechaCreacion = DateTime.Now;
                permiso.IdUsuarioCreacion = GetCurrentUserId();

                _context.Permisos.Add(permiso);
                await _context.SaveChangesAsync();

                return RedirectToAction("Permisos");
            }
            return View(permiso);
        }

        public async Task<IActionResult> EditarPermiso(int id)
        {
            var permiso = await _context.Permisos.FindAsync(id);
            if (permiso == null)
                return NotFound();
            return View(permiso);
        }

        [HttpPost]
        public async Task<IActionResult> EditarPermiso(Permiso permiso)
        {
            if (ModelState.IsValid)
            {
                var existe = await _context.Permisos
                    .AnyAsync(p => p.Codigo == permiso.Codigo && p.Id != permiso.Id && p.Estatus == true);

                if (existe)
                {
                    ModelState.AddModelError("Codigo", "Este código de permiso ya está en uso.");
                    return View(permiso);
                }

                var permisoActual = await _context.Permisos.FindAsync(permiso.Id);
                permisoActual.Codigo = permiso.Codigo;
                permisoActual.Descripcion = permiso.Descripcion;
                permisoActual.Modulo = permiso.Modulo;
                permisoActual.FechaModificacion = DateTime.Now;
                permisoActual.IdUsuarioModificacion = GetCurrentUserId();

                await _context.SaveChangesAsync();
                return RedirectToAction("Permisos");
            }
            return View(permiso);
        }

        [HttpPost]
        public async Task<IActionResult> EliminarPermiso(int id)
        {
            var permiso = await _context.Permisos.FindAsync(id);
            if (permiso != null)
            {
                permiso.Estatus = false;
                permiso.FechaEliminacion = DateTime.Now;
                permiso.IdUsuarioEliminacion = GetCurrentUserId();
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Permisos");
        }
        #endregion

        #region TIPOS DE DOCUMENTO
        public async Task<IActionResult> TiposDocumento()
        {
            var tipos = await _context.TipoDocumentos
                .Where(t => t.Estatus == true)
                .ToListAsync();
            return View(tipos);
        }

        public IActionResult CrearTipoDocumento()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CrearTipoDocumento(TipoDocumento tipoDocumento)
        {
            if (ModelState.IsValid)
            {
                var existe = await _context.TipoDocumentos
                    .AnyAsync(t => t.Nombre == tipoDocumento.Nombre && t.Estatus == true);

                if (existe)
                {
                    ModelState.AddModelError("Nombre", "Este tipo de documento ya existe.");
                    return View(tipoDocumento);
                }

                tipoDocumento.Estatus = true;
                tipoDocumento.FechaCreacion = DateTime.Now;
                tipoDocumento.IdUsuarioCreacion = GetCurrentUserId();

                _context.TipoDocumentos.Add(tipoDocumento);
                await _context.SaveChangesAsync();

                return RedirectToAction("TiposDocumento");
            }
            return View(tipoDocumento);
        }

        public async Task<IActionResult> EditarTipoDocumento(int id)
        {
            var tipoDocumento = await _context.TipoDocumentos.FindAsync(id);
            if (tipoDocumento == null)
                return NotFound();
            return View(tipoDocumento);
        }

        [HttpPost]
        public async Task<IActionResult> EditarTipoDocumento(TipoDocumento tipoDocumento)
        {
            if (ModelState.IsValid)
            {
                var existe = await _context.TipoDocumentos
                    .AnyAsync(t => t.Nombre == tipoDocumento.Nombre && t.Id != tipoDocumento.Id && t.Estatus == true);

                if (existe)
                {
                    ModelState.AddModelError("Nombre", "Este nombre de tipo documento ya está en uso.");
                    return View(tipoDocumento);
                }

                var tipoActual = await _context.TipoDocumentos.FindAsync(tipoDocumento.Id);
                tipoActual.Nombre = tipoDocumento.Nombre;
                tipoActual.Abreviatura = tipoDocumento.Abreviatura;
                tipoActual.TiempoRetencionMeses = tipoDocumento.TiempoRetencionMeses;
                tipoActual.FechaModificacion = DateTime.Now;
                tipoActual.IdUsuarioModificacion = GetCurrentUserId();

                await _context.SaveChangesAsync();
                return RedirectToAction("TiposDocumento");
            }
            return View(tipoDocumento);
        }

        [HttpPost]
        public async Task<IActionResult> EliminarTipoDocumento(int id)
        {
            var tipoDocumento = await _context.TipoDocumentos.FindAsync(id);
            if (tipoDocumento != null)
            {
                tipoDocumento.Estatus = false;
                tipoDocumento.FechaEliminacion = DateTime.Now;
                tipoDocumento.IdUsuarioEliminacion = GetCurrentUserId();
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("TiposDocumento");
        }
        #endregion

        #region GESTIÓN DE ROLES PARA USUARIOS
        public async Task<IActionResult> AsignarRolesUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.UsuarioRolIdUsuarioNavigations)
                .ThenInclude(ur => ur.IdRolNavigation)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
                return NotFound();

            var rolesDisponibles = await _context.Rols
                .Where(r => r.Estatus == true)
                .ToListAsync();

            var viewModel = new AsignarRolesViewModel
            {
                Usuario = usuario,
                RolesDisponibles = rolesDisponibles,
                RolesAsignados = usuario.UsuarioRolIdUsuarioNavigations
                    .Where(ur => ur.Estatus == true)
                    .Select(ur => ur.IdRol)
                    .ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AsignarRolesUsuario(int idUsuario, List<int> rolesSeleccionados)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.UsuarioRolIdUsuarioNavigations)
                .FirstOrDefaultAsync(u => u.Id == idUsuario);

            if (usuario == null)
                return NotFound();

            // Eliminar roles anteriores
            var rolesActuales = usuario.UsuarioRolIdUsuarioNavigations.Where(ur => ur.Estatus == true).ToList();
            foreach (var rol in rolesActuales)
            {
                rol.Estatus = false;
                rol.FechaEliminacion = DateTime.Now;
                rol.IdUsuarioEliminacion = GetCurrentUserId();
            }

            // Agregar nuevos roles
            if (rolesSeleccionados != null && rolesSeleccionados.Count > 0)
            {
                foreach (var idRol in rolesSeleccionados)
                {
                    var nuevoRol = new UsuarioRol
                    {
                        IdUsuario = idUsuario,
                        IdRol = idRol,
                        FechaAsignacion = DateTime.Now,
                        FechaCreacion = DateTime.Now,
                        IdUsuarioCreacion = GetCurrentUserId(),
                        Estatus = true
                    };
                    _context.UsuarioRols.Add(nuevoRol);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Usuarios", "Auth");
        }
        #endregion

        #region GESTIÓN DE PERMISOS PARA ROLES
        public async Task<IActionResult> AsignarPermisosRol(int id)
        {
            var rol = await _context.Rols
                .Include(r => r.RolPermisos)
                .ThenInclude(rp => rp.IdPermisoNavigation)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rol == null)
                return NotFound();

            var permisosDisponibles = await _context.Permisos
                .Where(p => p.Estatus == true)
                .ToListAsync();

            var viewModel = new AsignarPermisosViewModel
            {
                Rol = rol,
                PermisosDisponibles = permisosDisponibles,
                PermisosAsignados = rol.RolPermisos
                    .Where(rp => rp.Estatus == true)
                    .Select(rp => rp.IdPermiso)
                    .ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AsignarPermisosRol(int idRol, List<int> permisosSeleccionados)
        {
            var rol = await _context.Rols
                .Include(r => r.RolPermisos)
                .FirstOrDefaultAsync(r => r.Id == idRol);

            if (rol == null)
                return NotFound();

            // Eliminar permisos anteriores
            var permisosActuales = rol.RolPermisos.Where(rp => rp.Estatus == true).ToList();
            foreach (var permiso in permisosActuales)
            {
                permiso.Estatus = false;
                permiso.FechaEliminacion = DateTime.Now;
                permiso.IdUsuarioEliminacion = GetCurrentUserId();
            }

            // Agregar nuevos permisos
            if (permisosSeleccionados != null && permisosSeleccionados.Count > 0)
            {
                foreach (var idPermiso in permisosSeleccionados)
                {
                    var nuevoPermiso = new RolPermiso
                    {
                        IdRol = idRol,
                        IdPermiso = idPermiso,
                        FechaCreacion = DateTime.Now,
                        IdUsuarioCreacion = GetCurrentUserId(),
                        Estatus = true
                    };
                    _context.RolPermisos.Add(nuevoPermiso);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Roles");
        }
        #endregion
    }

    // ViewModels
    public class AdminDashboardViewModel
    {
        public int TotalUsuarios { get; set; }
        public int TotalRoles { get; set; }
        public int TotalDepartamentos { get; set; }
        public int TotalTiposDocumento { get; set; }
    }

    public class AsignarRolesViewModel
    {
        public Usuario Usuario { get; set; }
        public List<Rol> RolesDisponibles { get; set; }
        public List<int> RolesAsignados { get; set; }
    }

    public class AsignarPermisosViewModel
    {
        public Rol Rol { get; set; }
        public List<Permiso> PermisosDisponibles { get; set; }
        public List<int> PermisosAsignados { get; set; }
    }
}
