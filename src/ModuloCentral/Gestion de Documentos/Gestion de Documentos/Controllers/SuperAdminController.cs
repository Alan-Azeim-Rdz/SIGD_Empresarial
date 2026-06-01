using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Gestion_de_Documentos.Models;
using System.Security.Cryptography;
using System.Text.Json;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Gestion_de_Documentos.Controllers
{
    [Authorize(Roles = "Super Administrador")]
    public class SuperAdminController : Controller
    {
        private readonly DirContext _context;

        public SuperAdminController(DirContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                var builder = new System.Text.StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("X2"));
                }
                return builder.ToString();
            }
        }

        #region PANEL DE CONTROL
        public async Task<IActionResult> Index()
        {
            var empresas = await _context.Empresas
                .Include(e => e.Usuarios)
                .Include(e => e.Documentos)
                .OrderByDescending(e => e.FechaRegistro)
                .ToListAsync();

            var stats = new SuperAdminStatsViewModel
            {
                TotalEmpresas = empresas.Count,
                EmpresasActivas = empresas.Count(e => e.Estatus),
                EmpresasInactivas = empresas.Count(e => !e.Estatus),
                TotalDocumentosGlobal = await _context.Documentos.CountAsync(),
                TotalUsuariosGlobal = await _context.Usuarios.CountAsync(),
                Empresas = empresas
            };

            return View(stats);
        }
        #endregion

        #region CONFIGURACIÓN DE EMPRESAS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleEstatus(int id)
        {
            var empresa = await _context.Empresas.FindAsync(id);
            if (empresa == null)
                return NotFound();

            // Evitar desactivar una empresa demo si es necesario, o permitir todo
            empresa.Estatus = !empresa.Estatus;
            
            // También desactivar/activar usuarios correspondientes para coherencia de seguridad
            var usuarios = await _context.Usuarios.Where(u => u.IdEmpresa == id).ToListAsync();
            foreach (var u in usuarios)
            {
                u.Estatus = empresa.Estatus; // Alinear estatus del usuario con el de la empresa
            }

            _context.Update(empresa);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Estatus de la empresa '{empresa.Nombre}' actualizado con éxito.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult CrearEmpresa()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearEmpresa(CrearEmpresaViewModel model)
        {
            if (string.IsNullOrEmpty(model.Slug))
            {
                model.Slug = await GenerateUniqueSlugAsync(model.Nombre);
            }
            else
            {
                string baseSlug = model.Slug.ToLowerInvariant().Replace(" ", "-");
                string finalSlug = baseSlug;
                int count = 1;
                while (await _context.Empresas.AnyAsync(e => e.Slug == finalSlug))
                {
                    finalSlug = $"{baseSlug}-{count}";
                    count++;
                }
                model.Slug = finalSlug;
            }

            ModelState.Remove("Slug");

            if (ModelState.IsValid)
            {

                bool correoExiste = await _context.Usuarios.AnyAsync(u => u.Correo == model.CorreoAdmin);
                if (correoExiste)
                {
                    ModelState.AddModelError("CorreoAdmin", "El correo electrónico del administrador ya está registrado.");
                    return View(model);
                }

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var nuevaEmpresa = new Empresa
                        {
                            Nombre = model.Nombre,
                            Slug = model.Slug,
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

                        TempData["SuccessMessage"] = $"Empresa '{nuevaEmpresa.Nombre}' creada correctamente. El administrador '{model.NombreAdmin} {model.ApellidoAdminP}' puede iniciar sesión y crear los usuarios de su empresa desde el panel administrativo.";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError("", "Ocurrió un error al crear la empresa: " + ex.Message);
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
            string normalized = name.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder();
            foreach (char c in normalized)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            string slug = sb.ToString().Normalize(System.Text.NormalizationForm.FormC).ToLowerInvariant();
            
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
        #endregion
    }

    // ViewModels para Super Administrador
    public class SuperAdminStatsViewModel
    {
        public int TotalEmpresas { get; set; }
        public int EmpresasActivas { get; set; }
        public int EmpresasInactivas { get; set; }
        public int TotalDocumentosGlobal { get; set; }
        public int TotalUsuariosGlobal { get; set; }
        public List<Empresa> Empresas { get; set; } = new List<Empresa>();
    }

    public class CrearEmpresaViewModel
    {
        [Required(ErrorMessage = "El nombre de la empresa es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre de la empresa no puede exceder los 100 caracteres.")]
        public string Nombre { get; set; } = null!;

        public string? Slug { get; set; }

        [StringLength(20, ErrorMessage = "El RFC no puede exceder los 20 caracteres.")]
        [RegularExpression(@"^[A-Z&Ññ]{3,4}[0-9]{6}[A-Z0-9]{3}$", ErrorMessage = "El formato de RFC no es válido (Ej: AME123456XX9).")]
        public string? RFC { get; set; }

        [EmailAddress(ErrorMessage = "El correo de contacto no tiene un formato válido.")]
        [StringLength(150, ErrorMessage = "El correo de contacto no puede exceder los 150 caracteres.")]
        public string? CorreoContacto { get; set; }

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
