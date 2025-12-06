using Microsoft.EntityFrameworkCore;
using InCollege.Datos;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURACIÓN SQL ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// --- 2. SERVICIOS MVC ---
// Agregamos RuntimeCompilation para que si cambias una vista, se actualice sin reiniciar
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

var app = builder.Build();

// --- 3. PIPELINE HTTP ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// IMPORTANTE: Para .NET 8 usamos UseStaticFiles, no MapStaticAssets
// Esto permite que carguen tus imágenes de fondo y CSS
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
