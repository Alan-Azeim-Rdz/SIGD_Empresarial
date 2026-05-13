using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Gestion_de_Documentos.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Gestion_de_Documentos.Controllers
{
    public class AuthController : Controller
    {
        private readonly DirContext _context;

        public AuthController(DirContext context)
        {
            _context = context;
        }

        // --- LOGIN ---
        [HttpGet]
        public IActionResult Login()
        {
            // Si ya está logueado, lo mandamos al inicio
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string contrasena)
        {
            // 1. Buscamos al usuario usando 'Correo' (que usamos como username) y 'Estatus'
            var usuario = await _context.Usuarios
                .Include(u => u.UsuarioRols)
                .ThenInclude(ur => ur.IdRolNavigation)
                .FirstOrDefaultAsync(u => u.Correo == username && u.Estatus == true);

            if (usuario != null && usuario.Contrasena == contrasena)
            {
                var claims = new List<System.Security.Claims.Claim>
                {
                    // Usamos 'Id' y 'Correo' exactamente como están en tu tabla
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, usuario.Correo),
                };

                // Agregar roles desde la base de datos
                var rolesActivos = usuario.UsuarioRols
                    .Where(ur => ur.Estatus == true)
                    .Select(ur => ur.IdRolNavigation.Nombre)
                    .ToList();

                if (rolesActivos.Count > 0)
                {
                    foreach (var rol in rolesActivos)
                    {
                        claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, rol));
                    }
                }
                else
                {
                    // Si no tiene roles asignados, asignar rol por defecto (Usuario)
                    claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Usuario"));
                }

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "El Username o la contraseña son incorrectos.";
            return View();
        }

        // --- REGISTRO (Protegido por Rol) ---
        [HttpGet]
        [Authorize(Roles = "Administrador,Superior")] // ¡El RBAC en acción!
        public IActionResult Registro()
        {
            var departamentos = _context.Departamentos.Where(d => d.Estatus == true).ToList();
            ViewBag.Departamentos = departamentos;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Superior")]
        public async Task<IActionResult> Registro(Usuario nuevoUsuario)
        {
            if (ModelState.IsValid)
            {
                // Verificamos que el Correo no exista ya
                bool existe = await _context.Usuarios.AnyAsync(u => u.Correo == nuevoUsuario.Correo);
                if (existe)
                {
                    ViewBag.Error = "Este correo electrónico ya está registrado.";
                    var departamentos = _context.Departamentos.Where(d => d.Estatus == true).ToList();
                    ViewBag.Departamentos = departamentos;
                    return View(nuevoUsuario);
                }

                // Verificamos que el departamento exista
                var departamentoExiste = await _context.Departamentos.AnyAsync(d => d.Id == nuevoUsuario.IdDepartamento && d.Estatus == true);
                if (!departamentoExiste)
                {
                    ViewBag.Error = "El departamento seleccionado no es válido.";
                    var departamentos = _context.Departamentos.Where(d => d.Estatus == true).ToList();
                    ViewBag.Departamentos = departamentos;
                    return View(nuevoUsuario);
                }

                nuevoUsuario.Estatus = true;
                nuevoUsuario.FechaCreacion = DateTime.Now;
                nuevoUsuario.IdUsuarioCreacion = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

                _context.Usuarios.Add(nuevoUsuario);
                await _context.SaveChangesAsync();

                ViewBag.Exito = "Usuario creado exitosamente. Deberá cambiar su contraseña en el primer acceso.";
                ModelState.Clear();
                var departamentosNuevo = _context.Departamentos.Where(d => d.Estatus == true).ToList();
                ViewBag.Departamentos = departamentosNuevo;
                return View();
            }
            var dpts = _context.Departamentos.Where(d => d.Estatus == true).ToList();
            ViewBag.Departamentos = dpts;
            return View(nuevoUsuario);
        }

        // --- GESTIÓN DE USUARIOS (Protegido por Rol) ---
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Usuarios()
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.UsuarioRols)
                .ThenInclude(ur => ur.IdRolNavigation)
                .Where(u => u.Estatus == true)
                .ToListAsync();
            return View(usuarios);
        }

        // --- LOGOUT ---
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // --- ACCESO DENEGADO ---
        public IActionResult AccesoDenegado()
        {
            return View();
        }
    }
}