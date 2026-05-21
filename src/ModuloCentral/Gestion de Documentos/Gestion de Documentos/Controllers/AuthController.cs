using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Gestion_de_Documentos.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Security.Cryptography;
using System.Text;

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
            // 1 y 2. Buscamos al usuario y sus roles en una sola consulta optimizada, 
            // usando la propiedad de navegación que el Scaffold mapeó correctamente: UsuarioRolIdUsuarioNavigations
            var usuario = await _context.Usuarios
                .Include(u => u.UsuarioRolIdUsuarioNavigations.Where(ur => ur.Estatus == true))
                    .ThenInclude(ur => ur.IdRolNavigation)
                .FirstOrDefaultAsync(u => u.Correo == username && u.Estatus == true);

            var hashContrasena = HashPassword(contrasena);

            if (usuario != null && string.Equals(usuario.Contrasena.Trim(), hashContrasena.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                var claims = new List<System.Security.Claims.Claim>
                {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, usuario.Correo),
                };

                // Agregar roles desde la base de datos usando la propiedad mapeada
                var rolesActivos = usuario.UsuarioRolIdUsuarioNavigations != null
                    ? usuario.UsuarioRolIdUsuarioNavigations
                        .Select(ur => ur.IdRolNavigation?.Nombre)
                        .Where(r => !string.IsNullOrEmpty(r))
                        .ToList()
                    : new List<string>();

                if (rolesActivos.Any())
                {
                    foreach (var rol in rolesActivos)
                    {
                        claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, rol));
                    }
                }
                else
                {
                    // Si no tiene roles asignados, asignar rol por defecto
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
        [Authorize(Roles = "Administrador,Superior")]
        public IActionResult Registro()
        {
            ViewBag.Departamentos = _context.Departamentos.Where(d => d.Estatus == true).ToList();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Superior")]
        public async Task<IActionResult> Registro(Usuario nuevoUsuario)
        {
            if (ModelState.IsValid)
            {
                bool existe = await _context.Usuarios.AnyAsync(u => u.Correo == nuevoUsuario.Correo);
                if (existe)
                {
                    ViewBag.Error = "Este correo electrónico ya está registrado.";
                    ViewBag.Departamentos = _context.Departamentos.Where(d => d.Estatus == true).ToList();
                    return View(nuevoUsuario);
                }

                var departamentoExiste = await _context.Departamentos.AnyAsync(d => d.Id == nuevoUsuario.IdDepartamento && d.Estatus == true);
                if (!departamentoExiste)
                {
                    ViewBag.Error = "El departamento seleccionado no es válido.";
                    ViewBag.Departamentos = _context.Departamentos.Where(d => d.Estatus == true).ToList();
                    return View(nuevoUsuario);
                }

                nuevoUsuario.Estatus = true;
                nuevoUsuario.FechaCreacion = DateTime.Now;
                nuevoUsuario.IdUsuarioCreacion = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

                nuevoUsuario.Contrasena = HashPassword(nuevoUsuario.Contrasena);

                _context.Usuarios.Add(nuevoUsuario);
                await _context.SaveChangesAsync();

                ViewBag.Exito = "Usuario creado exitosamente. Deberá cambiar su contraseña en el primer acceso.";
                ModelState.Clear();
                ViewBag.Departamentos = _context.Departamentos.Where(d => d.Estatus == true).ToList();
                return View(new Usuario()); // Limpia el formulario
            }

            ViewBag.Departamentos = _context.Departamentos.Where(d => d.Estatus == true).ToList();
            return View(nuevoUsuario);
        }

        // --- GESTIÓN DE USUARIOS (Protegido por Rol) ---
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Usuarios()
        {
            // Usamos la propiedad de navegación correcta aquí también para evitar el error en la vista de lista
            var usuarios = await _context.Usuarios
                .Include(u => u.UsuarioRolIdUsuarioNavigations)
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

        // --- HASHEAR CONTRASEÑA ---
        private string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return string.Empty;
            using (var sha256 = SHA256.Create())
            {
                // NOTA: Usamos Encoding.Unicode (UTF-16LE) para coincidir con
                // HASHBYTES('SHA2_256', N'...') en SQL Server (que usa NVARCHAR)
                var bytes = sha256.ComputeHash(Encoding.Unicode.GetBytes(password));
                var builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("X2"));
                }
                return builder.ToString();
            }
        }
    }
}