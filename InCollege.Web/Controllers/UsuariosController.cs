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

        // 1. GESTIÓN
        public IActionResult Gestionar()
        {
            var viewModel = new GestionUsuariosViewModel
            {
                Pendientes = _context.Usuarios.Where(u => !u.EstaActivo).OrderByDescending(u => u.FechaAlta).ToList(),
                Activos = _context.Usuarios.Where(u => u.EstaActivo).OrderBy(u => u.Apellido).ToList()
            };
            return View(viewModel);
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
        public IActionResult Aprobar(Guid id, string rolAsignado, string zonaAsignada)
        {
            var usuario = _context.Usuarios.Find(id);
            if (usuario != null)
            {
                usuario.EstaActivo = true;
                usuario.Rol = rolAsignado;
                usuario.Zona = (rolAsignado == "Vendedor") ? zonaAsignada : "-";
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

    }
} 