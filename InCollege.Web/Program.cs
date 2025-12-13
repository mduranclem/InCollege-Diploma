using Microsoft.EntityFrameworkCore;
using InCollege.Datos;

var builder = WebApplication.CreateBuilder(args);

// 1. DB
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

// 2. SERVICIOS
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

// --- AGREGAR ESTA LÍNEA PARA ACTIVAR SESIONES ---
builder.Services.AddSession();
// -----------------------------------------------

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

// --- AGREGAR ESTA LÍNEA (ANTES DE MAPCONTROLLER) ---
app.UseSession();
// ---------------------------------------------------

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();