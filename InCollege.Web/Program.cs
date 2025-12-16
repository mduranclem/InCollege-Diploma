using InCollege.Datos;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using InCollege.Datos.Servicios; // Necesario para encontrar ContratoServicio

var builder = WebApplication.CreateBuilder(args);

// 1. DB
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

// 2. SERVICIOS WEB
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

// 3. CONFIGURACIÓN DE SESIONES
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de vida de la sesión
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// --- ¡ESTA ES LA LÍNEA QUE TE FALTABA! ---
// Registramos el servicio para que el Controlador de Diseños funcione
builder.Services.AddScoped<ContratoServicio>();
// -------------------------------------------

var app = builder.Build();

// CONFIGURACIÓN DEL PIPELINE HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

// --- ACTIVAR EL MOTOR DE SESIONES (ANTES DE RUTAS) ---
app.UseSession();
// -----------------------------------------------------

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();