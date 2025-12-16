using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using InCollege.Datos;
using InCollege.Dominio.Modelos;
using InCollege.Web.Models;
using InCollege.Dominio.Servicios;
using System.Linq;
using System;

namespace InCollege.Web.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Usuarios/Gestionar
        public IActionResult Gestionar()
        {
            // 1. Obtener usuarios pendientes y activos (tu código actual)
            var pendientes = _context.Usuarios.Where(u => !u.EstaActivo).ToList();
            var activos = _context.Usuarios.Where(u => u.EstaActivo).ToList();

            var modelo = new GestionUsuariosViewModel 
            {
                Pendientes = pendientes,
                Activos = activos
            };

            // --- AGREGAR "Disenador" A ESTA LISTA ---
            // Importante: Escríbelo como "Disenador" (sin ñ) si así lo pusimos en el _Layout
            ViewBag.ListaRoles = new List<string> { "Admin", "Vendedor", "Producción", "Gerente", "Disenador" };
            // ----------------------------------------

            return View(modelo);
        }

        // 2. CREAR (GET)
        public IActionResult Crear()
        {
            ViewBag.Roles = new SelectList(new[] { "Vendedor", "Producción", "Gerente", "Administrador" });
            ViewBag.Zonas = new SelectList(new[] { "Norte", "Sur", "Oeste", "Centro", "Alrededores" });
            return View();
        }

        // 3. CREAR (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Usuario usuario)
        {
            if (usuario.Rol == "Vendedor" && string.IsNullOrEmpty(usuario.Zona))
            {
                ModelState.AddModelError("Zona", "Si el rol es Vendedor, debe asignar una Zona.");
            }

            if (_context.Usuarios.Any(u => u.Email == usuario.Email))
            {
                ModelState.AddModelError("Email", "El correo ya existe.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    usuario.FechaAlta = DateTime.Now;
                    usuario.EstaActivo = false;
                    _context.Usuarios.Add(usuario);
                    _context.SaveChanges();

                    try { EmailService.Enviar("", "Bienvenido", $"Hola {usuario.Nombre}, tu cuenta está en revisión."); } catch { }

                    return RedirectToAction("Login", "Home");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            }

            ViewBag.Roles = new SelectList(new[] { "Vendedor", "Producción", "Gerente", "Administrador" });
            ViewBag.Zonas = new SelectList(new[] { "Norte", "Sur", "Oeste", "Centro", "Alrededores" });
            return View(usuario);
        }

        // 4. APROBAR
        [HttpPost]
        [HttpPost]
        public IActionResult AprobarUsuario(Guid userId, string rol, string zona)
        {
            var usuario = _context.Usuarios.Find(userId);
            if (usuario != null)
            {
                usuario.Rol = rol;
                usuario.EstaActivo = true;
                usuario.FechaAlta = DateTime.Now;

                // --- LÓGICA DE ZONA ---
                if (rol == "Vendedor")
                {
                    // Si es vendedor, guardamos la zona que eligieron
                    usuario.Zona = zona;
                }
                else
                {
                    // Si es Admin, Diseñador, etc., forzamos la zona a NULL o vacía
                    // para que no quede basura en la base de datos.
                    usuario.Zona = null;
                }
                // ----------------------

                _context.SaveChanges();
            }
            return RedirectToAction("Gestionar");
        }

        // 5. ELIMINAR
        [HttpPost]
        public IActionResult Eliminar(Guid id)
        {
            var usuario = _context.Usuarios.Find(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
                _context.SaveChanges();
            }
            return RedirectToAction("Gestionar");
        }

        // 6. CAMBIAR ROL
        [HttpPost]
        public IActionResult CambiarRol(Guid id, string nuevoRol, string nuevaZona)
        {
            var usuario = _context.Usuarios.Find(id);
            if (usuario != null)
            {
                usuario.Rol = nuevoRol;
                usuario.Zona = (nuevoRol == "Vendedor") ? nuevaZona : "-";
                _context.SaveChanges();
            }
            return RedirectToAction("Gestionar");
        }
        [HttpPost]
        public IActionResult ActualizarUsuario(Guid userId, string rol, string zona)
        {
            // 1. Buscar al usuario en la base de datos
            var usuario = _context.Usuarios.Find(userId);

            if (usuario != null)
            {
                // 2. Actualizar el Rol
                usuario.Rol = rol;

                // 3. Lógica inteligente de Zona
                if (rol == "Vendedor")
                {
                    // Si es vendedor, guardamos la zona que eligieron
                    usuario.Zona = zona;
                }
                else
                {
                    // Si lo cambiaron a Diseñador, Admin, etc., borramos la zona
                    usuario.Zona = null;
                }

                // 4. Guardar cambios en la Base de Datos
                _context.SaveChanges();
            }

            // 5. Volver a la misma página
            return RedirectToAction("Gestionar");
        }
    }
} 