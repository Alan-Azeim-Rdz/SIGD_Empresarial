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
using System.ComponentModel.DataAnnotations;

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
        public async Task<IActionResult> Login(string username, string contrasena, bool recordarme = false)
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
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.GivenName, usuario.Nombre),
                };

                if (usuario.IdEmpresa.HasValue)
                {
                    claims.Add(new System.Security.Claims.Claim("IdEmpresa", usuario.IdEmpresa.Value.ToString()));
                }

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

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = recordarme,
                    ExpiresUtc = recordarme
                        ? DateTimeOffset.UtcNow.AddDays(30)
                        : DateTimeOffset.UtcNow.AddHours(2),
                    AllowRefresh = true
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

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
            var empresaId = int.TryParse(User.FindFirst("IdEmpresa")?.Value, out var empId) ? empId : 0;
            ViewBag.Departamentos = _context.Departamentos.Where(d => d.Estatus == true && d.IdEmpresa == empresaId).ToList();
            // Excluir roles restringidos del selector
            ViewBag.Roles = _context.Rols.Where(r => r.Estatus == true
                && r.Nombre != "Super Administrador").ToList();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Superior")]
        public async Task<IActionResult> Registro(Usuario nuevoUsuario, int idRol)
        {
            var empresaId = int.TryParse(User.FindFirst("IdEmpresa")?.Value, out var empId) ? empId : 0;

            // Remover propiedades de navegación y campos que el controlador asigna
            // para evitar errores falsos de ModelState
            ModelState.Remove("IdDepartamentoNavigation");
            ModelState.Remove("IdEmpresaNavigation");
            ModelState.Remove("IdUsuarioCreacionNavigation");
            ModelState.Remove("IdUsuarioModificacionNavigation");
            ModelState.Remove("IdUsuarioEliminacionNavigation");
            ModelState.Remove("IdEmpresa");
            ModelState.Remove("Estatus");
            ModelState.Remove("FechaCreacion");
            ModelState.Remove("IdUsuarioCreacion");

            if (ModelState.IsValid)
            {
                bool existe = await _context.Usuarios.AnyAsync(u => u.Correo == nuevoUsuario.Correo);
                if (existe)
                {
                    ViewBag.Error = "Este correo electrónico ya está registrado.";
                    ViewBag.Departamentos = _context.Departamentos.Where(d => d.Estatus == true && d.IdEmpresa == empresaId).ToList();
                    ViewBag.Roles = await _context.Rols.Where(r => r.Estatus == true).ToListAsync();
                    return View(nuevoUsuario);
                }

                var departamentoExiste = await _context.Departamentos.AnyAsync(d => d.Id == nuevoUsuario.IdDepartamento && d.Estatus == true && d.IdEmpresa == empresaId);
                if (!departamentoExiste)
                {
                    ViewBag.Error = "El departamento seleccionado no es válido.";
                    ViewBag.Departamentos = _context.Departamentos.Where(d => d.Estatus == true && d.IdEmpresa == empresaId).ToList();
                    ViewBag.Roles = await _context.Rols.Where(r => r.Estatus == true).ToListAsync();
                    return View(nuevoUsuario);
                }

                var rolExiste = await _context.Rols.AnyAsync(r => r.Id == idRol && r.Estatus == true);
                if (!rolExiste)
                {
                    ViewBag.Error = "El rol seleccionado no es válido.";
                    ViewBag.Departamentos = _context.Departamentos.Where(d => d.Estatus == true && d.IdEmpresa == empresaId).ToList();
                    ViewBag.Roles = await _context.Rols.Where(r => r.Estatus == true).ToListAsync();
                    return View(nuevoUsuario);
                }

                nuevoUsuario.Estatus = true;
                nuevoUsuario.FechaCreacion = DateTime.Now;
                nuevoUsuario.IdUsuarioCreacion = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                nuevoUsuario.IdEmpresa = empresaId;

                nuevoUsuario.Contrasena = HashPassword(nuevoUsuario.Contrasena);

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        _context.Usuarios.Add(nuevoUsuario);
                        await _context.SaveChangesAsync();

                        var usuarioRol = new UsuarioRol
                        {
                            IdUsuario = nuevoUsuario.Id,
                            IdRol = idRol,
                            FechaAsignacion = DateTime.Now,
                            FechaCreacion = DateTime.Now,
                            Estatus = true,
                            IdUsuarioCreacion = nuevoUsuario.IdUsuarioCreacion
                        };
                        _context.UsuarioRols.Add(usuarioRol);
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        ViewBag.Error = "Ocurrió un error al registrar el usuario y su rol: " + ex.Message;
                        ViewBag.Departamentos = _context.Departamentos.Where(d => d.Estatus == true && d.IdEmpresa == empresaId).ToList();
                        ViewBag.Roles = await _context.Rols.Where(r => r.Estatus == true).ToListAsync();
                        return View(nuevoUsuario);
                    }
                }

                ViewBag.Exito = "Usuario creado exitosamente. Deberá cambiar su contraseña en el primer acceso.";
                ModelState.Clear();
                ViewBag.Departamentos = _context.Departamentos.Where(d => d.Estatus == true && d.IdEmpresa == empresaId).ToList();
                ViewBag.Roles = await _context.Rols.Where(r => r.Estatus == true).ToListAsync();
                return View(new Usuario()); // Limpia el formulario
            }

            // Debug: mostrar errores de ModelState en el ViewBag si hay problemas
            var errores = ModelState.Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToList());
            if (errores.Any())
            {
                ViewBag.Error = "Datos inválidos: " + string.Join("; ", errores.SelectMany(e => e.Value));
            }

            ViewBag.Departamentos = _context.Departamentos.Where(d => d.Estatus == true && d.IdEmpresa == empresaId).ToList();
            ViewBag.Roles = await _context.Rols.Where(r => r.Estatus == true).ToListAsync();
            return View(nuevoUsuario);
        }
        // --- GESTIÓN DE USUARIOS (Protegido por Rol) ---
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Usuarios()
        {
            var empresaId = int.TryParse(User.FindFirst("IdEmpresa")?.Value, out var empId) ? empId : 0;
            ViewBag.Departamentos = await _context.Departamentos.Where(d => d.Estatus == true && d.IdEmpresa == empresaId).ToListAsync();
            var usuarios = await _context.Usuarios
                .Include(u => u.IdDepartamentoNavigation)
                .Include(u => u.UsuarioRolIdUsuarioNavigations)
                    .ThenInclude(ur => ur.IdRolNavigation)
                .Where(u => u.Estatus == true && u.IdEmpresa == empresaId)
                .ToListAsync();
            return View(usuarios);
        }

        // --- EDITAR USUARIO ---
        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EditarUsuario(int id)
        {
            var empresaId = int.TryParse(User.FindFirst("IdEmpresa")?.Value, out var empId) ? empId : 0;
            var usuario = await _context.Usuarios
                .Include(u => u.UsuarioRolIdUsuarioNavigations)
                .FirstOrDefaultAsync(u => u.Id == id && u.IdEmpresa == empresaId);
            if (usuario == null) return NotFound();
            
            ViewBag.Departamentos = _context.Departamentos.Where(d => d.Estatus == true && d.IdEmpresa == empresaId).ToList();
            // Excluir roles restringidos del selector
            ViewBag.Roles = _context.Rols.Where(r => r.Estatus == true
                && r.Nombre != "Super Administrador").ToList();

            var rolAsignado = usuario.UsuarioRolIdUsuarioNavigations.FirstOrDefault(ur => ur.Estatus == true);
            ViewBag.IdRolAsignado = rolAsignado?.IdRol ?? 0;

            return View(usuario);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EditarUsuario(int id, string nombre, string apellidoP, string? apellidoM, string correo, int idDepartamento, int idRol, string? nuevaContrasena)
        {
            var empresaId = int.TryParse(User.FindFirst("IdEmpresa")?.Value, out var empId) ? empId : 0;
            var usuario = await _context.Usuarios
                .Include(u => u.UsuarioRolIdUsuarioNavigations)
                .FirstOrDefaultAsync(u => u.Id == id && u.IdEmpresa == empresaId);
            if (usuario == null) return NotFound();

            // Verificar correo único
            bool correoEnUso = await _context.Usuarios.AnyAsync(u => u.Correo == correo && u.Id != id);
            if (correoEnUso)
            {
                ViewBag.Error = "El correo electrónico ya está en uso por otro usuario.";
                ViewBag.Departamentos = _context.Departamentos.Where(d => d.Estatus == true && d.IdEmpresa == empresaId).ToList();
                ViewBag.Roles = await _context.Rols.Where(r => r.Estatus == true).ToListAsync();
                ViewBag.IdRolAsignado = idRol;
                return View(usuario);
            }

            var deptoValido = await _context.Departamentos.AnyAsync(d => d.Id == idDepartamento && d.Estatus == true && d.IdEmpresa == empresaId);
            if (!deptoValido)
            {
                ViewBag.Error = "El departamento seleccionado no es válido.";
                ViewBag.Departamentos = _context.Departamentos.Where(d => d.Estatus == true && d.IdEmpresa == empresaId).ToList();
                ViewBag.Roles = await _context.Rols.Where(r => r.Estatus == true).ToListAsync();
                ViewBag.IdRolAsignado = idRol;
                return View(usuario);
            }

            var rolValido = await _context.Rols.AnyAsync(r => r.Id == idRol && r.Estatus == true);
            if (!rolValido)
            {
                ViewBag.Error = "El rol seleccionado no es válido.";
                ViewBag.Departamentos = _context.Departamentos.Where(d => d.Estatus == true && d.IdEmpresa == empresaId).ToList();
                ViewBag.Roles = await _context.Rols.Where(r => r.Estatus == true).ToListAsync();
                ViewBag.IdRolAsignado = idRol;
                return View(usuario);
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    usuario.Nombre = nombre;
                    usuario.ApellidoP = apellidoP;
                    usuario.ApellidoM = apellidoM;
                    usuario.Correo = correo;
                    usuario.IdDepartamento = idDepartamento;
                    usuario.FechaModificacion = DateTime.Now;
                    usuario.IdUsuarioModificacion = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

                    if (!string.IsNullOrWhiteSpace(nuevaContrasena))
                    {
                        usuario.Contrasena = HashPassword(nuevaContrasena);
                    }

                    await _context.SaveChangesAsync();

                    var rolesActuales = await _context.UsuarioRols
                        .Where(ur => ur.IdUsuario == id && ur.Estatus == true)
                        .ToListAsync();

                    bool rolYaAsignado = false;
                    foreach (var ur in rolesActuales)
                    {
                        if (ur.IdRol == idRol)
                        {
                            rolYaAsignado = true;
                        }
                        else
                        {
                            ur.Estatus = false;
                            ur.FechaEliminacion = DateTime.Now;
                            ur.IdUsuarioEliminacion = usuario.IdUsuarioModificacion;
                            _context.Update(ur);
                        }
                    }

                    if (!rolYaAsignado)
                    {
                        var nuevoUsuarioRol = new UsuarioRol
                        {
                            IdUsuario = usuario.Id,
                            IdRol = idRol,
                            FechaAsignacion = DateTime.Now,
                            FechaCreacion = DateTime.Now,
                            Estatus = true,
                            IdUsuarioCreacion = usuario.IdUsuarioModificacion
                        };
                        _context.UsuarioRols.Add(nuevoUsuarioRol);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ViewBag.Error = "Ocurrió un error al actualizar el usuario y su rol: " + ex.Message;
                    ViewBag.Departamentos = _context.Departamentos.Where(d => d.Estatus == true && d.IdEmpresa == empresaId).ToList();
                    ViewBag.Roles = await _context.Rols.Where(r => r.Estatus == true).ToListAsync();
                    ViewBag.IdRolAsignado = idRol;
                    return View(usuario);
                }
            }

            TempData["Exito"] = "Usuario actualizado correctamente.";
            return RedirectToAction("Usuarios");
        }

        // --- ELIMINAR USUARIO (Soft Delete) ---
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            var empresaId = int.TryParse(User.FindFirst("IdEmpresa")?.Value, out var empId) ? empId : 0;
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id && u.IdEmpresa == empresaId);
            if (usuario != null)
            {
                usuario.Estatus = false;
                usuario.FechaEliminacion = DateTime.Now;
                usuario.IdUsuarioEliminacion = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                await _context.SaveChangesAsync();
                TempData["Exito"] = $"Usuario '{usuario.Nombre} {usuario.ApellidoP}' eliminado correctamente.";
            }
            return RedirectToAction("Usuarios");
        }

        // --- VER USUARIOS ELIMINADOS (Borrado Lógico) ---
        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> UsuariosEliminados()
        {
            var empresaId = int.TryParse(User.FindFirst("IdEmpresa")?.Value, out var empId) ? empId : 0;
            ViewBag.Departamentos = await _context.Departamentos.Where(d => d.Estatus == true && d.IdEmpresa == empresaId).ToListAsync();
            var usuarios = await _context.Usuarios
                .Include(u => u.IdDepartamentoNavigation)
                .Include(u => u.UsuarioRolIdUsuarioNavigations)
                    .ThenInclude(ur => ur.IdRolNavigation)
                .Where(u => (u.Estatus == false || u.Estatus == null) && u.IdEmpresa == empresaId)
                .ToListAsync();
            return View(usuarios);
        }

        // --- REACTIVAR USUARIO ---
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ReactivarUsuario(int id)
        {
            var empresaId = int.TryParse(User.FindFirst("IdEmpresa")?.Value, out var empId) ? empId : 0;
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id && u.IdEmpresa == empresaId);
            if (usuario != null)
            {
                usuario.Estatus = true;
                usuario.FechaEliminacion = null;
                usuario.IdUsuarioEliminacion = null;
                usuario.FechaModificacion = DateTime.Now;
                usuario.IdUsuarioModificacion = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                await _context.SaveChangesAsync();
                TempData["Exito"] = $"Usuario '{usuario.Nombre} {usuario.ApellidoP}' reactivado correctamente.";
            }
            return RedirectToAction("Usuarios");
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

        // --- REGISTRO DE EMPRESA (PÚBLICO) ---
        [HttpGet]
        public IActionResult RegistroEmpresa()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegistroEmpresa(RegistroEmpresaViewModel model)
        {
            if (string.IsNullOrEmpty(model.SlugEmpresa))
            {
                model.SlugEmpresa = await GenerateUniqueSlugAsync(model.NombreEmpresa);
            }
            else
            {
                string baseSlug = model.SlugEmpresa.ToLowerInvariant().Replace(" ", "-");
                string finalSlug = baseSlug;
                int count = 1;
                while (await _context.Empresas.AnyAsync(e => e.Slug == finalSlug))
                {
                    finalSlug = $"{baseSlug}-{count}";
                    count++;
                }
                model.SlugEmpresa = finalSlug;
            }

            ModelState.Remove("SlugEmpresa");

            if (ModelState.IsValid)
            {
                bool correoExiste = await _context.Usuarios.AnyAsync(u => u.Correo == model.CorreoAdmin);
                if (correoExiste)
                {
                    ViewBag.Error = "El correo electrónico del administrador ya está registrado.";
                    return View(model);
                }

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var nuevaEmpresa = new Empresa
                        {
                            Nombre = model.NombreEmpresa,
                            Slug = model.SlugEmpresa,
                            RFC = model.RFC,
                            CorreoContacto = model.CorreoContacto,
                            FechaRegistro = DateTime.Now,
                            Estatus = true
                        };
                        _context.Empresas.Add(nuevaEmpresa);
                        await _context.SaveChangesAsync();

                        var deptoAdm = new Departamento
                        {
                            Nombre = "Administración",
                            Abreviatura = "ADM",
                            Estatus = true,
                            FechaCreacion = DateTime.Now,
                            IdEmpresa = nuevaEmpresa.Id
                        };
                        _context.Departamentos.Add(deptoAdm);
                        await _context.SaveChangesAsync();

                        var nuevoUsuario = new Usuario
                        {
                            IdDepartamento = deptoAdm.Id,
                            IdEmpresa = nuevaEmpresa.Id,
                            Nombre = model.NombreAdmin,
                            ApellidoP = model.ApellidoAdminP,
                            ApellidoM = model.ApellidoAdminM,
                            Correo = model.CorreoAdmin,
                            Contrasena = HashPassword(model.ContrasenaAdmin),
                            FechaCreacion = DateTime.Now,
                            Estatus = true
                        };
                        _context.Usuarios.Add(nuevoUsuario);
                        await _context.SaveChangesAsync();

                        var rolAdmin = await _context.Rols.FirstOrDefaultAsync(r => r.Nombre == "Administrador" && r.Estatus == true);
                        if (rolAdmin == null)
                        {
                            rolAdmin = new Rol
                            {
                                Nombre = "Administrador",
                                Descripcion = "Administrador de la Empresa",
                                Estatus = true,
                                FechaCreacion = DateTime.Now
                            };
                            _context.Rols.Add(rolAdmin);
                            await _context.SaveChangesAsync();
                        }


                        var usuarioRol = new UsuarioRol
                        {
                            IdUsuario = nuevoUsuario.Id,
                            IdRol = rolAdmin.Id,
                            FechaAsignacion = DateTime.Now,
                            FechaCreacion = DateTime.Now,
                            Estatus = true,
                            IdUsuarioCreacion = nuevoUsuario.Id
                        };
                        _context.UsuarioRols.Add(usuarioRol);
                        await _context.SaveChangesAsync();

                        deptoAdm.IdUsuarioCreacion = nuevoUsuario.Id;
                        _context.Update(deptoAdm);
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();

                        TempData["Exito"] = "¡Empresa registrada exitosamente! Ya puedes iniciar sesión como Administrador y comenzar a configurar tu empresa: agrega departamentos, usuarios, auditores y más desde tu panel administrativo.";
                        return RedirectToAction("Login");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        ViewBag.Error = "Ocurrió un error al registrar la empresa: " + ex.Message;
                        return View(model);
                    }
                }
            }

            return View(model);
        }

        private async Task<string> GenerateUniqueSlugAsync(string name)
        {
            if (string.IsNullOrEmpty(name)) return "empresa";
            
            // Normalizar a FormD para separar los caracteres base de sus acentos/diacríticos
            string normalized = name.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (char c in normalized)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                // Filtrar los caracteres que son acentos/diacríticos
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }
            // Volver a normalizar a FormC y pasar a minúsculas
            string slug = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
            
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", " ").Trim();
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s", "-");
            
            if (string.IsNullOrEmpty(slug)) slug = "empresa";

            string finalSlug = slug;
            int count = 1;
            while (await _context.Empresas.AnyAsync(e => e.Slug == finalSlug))
            {
                finalSlug = $"{slug}-{count}";
                count++;
            }
            return finalSlug;
        }
    }

    public class RegistroEmpresaViewModel
    {
        [Required(ErrorMessage = "El nombre de la empresa es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre de la empresa no puede exceder los 100 caracteres.")]
        public string NombreEmpresa { get; set; } = null!;

        public string? SlugEmpresa { get; set; }

        [Required(ErrorMessage = "El RFC es obligatorio.")]
        [StringLength(20, ErrorMessage = "El RFC no puede exceder los 20 caracteres.")]
        [RegularExpression(@"^[A-Z&Ññ]{3,4}[0-9]{6}[A-Z0-9]{3}$", ErrorMessage = "El formato de RFC no es válido (Ej: AME123456XX9).")]
        public string RFC { get; set; } = null!;

        [Required(ErrorMessage = "El correo de contacto es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo de contacto no tiene un formato válido.")]
        [StringLength(150, ErrorMessage = "El correo de contacto no puede exceder los 150 caracteres.")]
        public string CorreoContacto { get; set; } = null!;

        [Required(ErrorMessage = "El nombre del administrador es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre del administrador no puede exceder los 100 caracteres.")]
        public string NombreAdmin { get; set; } = null!;

        [Required(ErrorMessage = "El primer apellido es obligatorio.")]
        [StringLength(100, ErrorMessage = "El primer apellido no puede exceder los 100 caracteres.")]
        public string ApellidoAdminP { get; set; } = null!;

        [StringLength(100, ErrorMessage = "El segundo apellido no puede exceder los 100 caracteres.")]
        public string? ApellidoAdminM { get; set; }

        [Required(ErrorMessage = "El correo del administrador es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo del administrador no tiene un formato válido.")]
        [StringLength(150, ErrorMessage = "El correo del administrador no puede exceder los 150 caracteres.")]
        public string CorreoAdmin { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$", ErrorMessage = "La contraseña debe tener al menos una letra mayúscula, una minúscula y un número.")]
        public string ContrasenaAdmin { get; set; } = null!;
    }
}