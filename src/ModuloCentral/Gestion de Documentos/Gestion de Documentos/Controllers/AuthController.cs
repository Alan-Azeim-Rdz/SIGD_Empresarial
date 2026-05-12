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
                                        .FirstOrDefaultAsync(u => u.Correo == username && u.Estatus == true);

            if (usuario != null && usuario.Contrasena == contrasena)
            {
                var claims = new List<Claim>
        {
            // Usamos 'Id' y 'Correo' exactamente como están en tu tabla
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Correo),
            
            // OJO: Como tu tabla no tiene una columna 'Rol', de momento 
            // le asignaremos el rol de Administrador temporalmente para que 
            // el botón de registro funcione y puedas probar el sistema.
            // (Más adelante podemos arreglar esto usando el IdDepartamento).
            new Claim(ClaimTypes.Role, "Administrador")
        };

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
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Superior")]
        public async Task<IActionResult> Registro(Usuario nuevoUsuario)
        {
            if (ModelState.IsValid)
            {
                // Verificamos que el Nombre no exista ya
                bool existe = await _context.Usuarios.AnyAsync(u => u.Nombre == nuevoUsuario.Nombre);
                if (existe)
                {
                    ViewBag.Error = "Este Nombre de usuario ya está en uso.";
                    return View(nuevoUsuario);
                }

                nuevoUsuario.Estatus = true;
                nuevoUsuario.FechaCreacion = DateTime.Now;

                _context.Usuarios.Add(nuevoUsuario);
                await _context.SaveChangesAsync();

                ViewBag.Exito = "Usuario creado exitosamente en la red.";
                ModelState.Clear();
                return View();
            }
            return View(nuevoUsuario);
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