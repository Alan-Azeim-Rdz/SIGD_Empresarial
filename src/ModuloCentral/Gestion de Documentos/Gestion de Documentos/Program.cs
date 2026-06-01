using Gestion_de_Documentos.Models;
using Gestion_de_Documentos.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// ── MVC ───────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ── Sesiones HTTP (para registrar vista previa antes de firmar) ──
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ── Entity Framework Core → SQL Server ───────────────────────
builder.Services.AddDbContext<DirContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Autenticación por Cookie ──────────────────────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath        = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccesoDenegado";
        options.ExpireTimeSpan   = TimeSpan.FromHours(2);
    });

// ── HttpClient para el Módulo de Reportes ─────────────────────
// Se registra como Typed Client para que ASP.NET Core gestione
// el ciclo de vida del socket (evita socket exhaustion).
var reportesTimeout = int.TryParse(
    builder.Configuration["ReportesModule:TimeoutSeconds"], out var t) ? t : 10;

builder.Services.AddHttpClient<ReportesIntegrationService>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ReportesModule:BaseUrl"] ?? "http://modulo_reportes");
    client.Timeout = TimeSpan.FromSeconds(reportesTimeout);
});

builder.Services.AddHttpClient<BusquedaIntegrationService>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["BusquedaModule:BaseUrl"] ?? "http://modulo_busqueda:3000");
    client.Timeout = TimeSpan.FromSeconds(reportesTimeout);
});

// ── Registrar el servicio de integración en el contenedor DI ─
// AddHttpClient ya registra las clases en el contenedor DI como Transient.

// ── Servicio MongoDB GridFS para Archivos Físicos ─────────────
builder.Services.AddSingleton<IMongoGridFsService, MongoGridFsService>();

var app = builder.Build();

// ── Pipeline HTTP ─────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseAuthentication();
app.UseSession();  // Debe ir después de UseRouting y antes de UseAuthorization
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();
