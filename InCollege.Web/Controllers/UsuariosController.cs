using Microsoft.AspNetCore.Mvc;
using InCollege.Datos;
using InCollege.Dominio.Modelos;
using InCollege.Web.Models; // Necesario para el ViewModel
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

        // GET: Muestra el panel unificado
        public IActionResult Gestionar()
        {
            var viewModel = new GestionUsuariosViewModel
            {
                // Lista de los que pidieron cuenta
                Pendientes = _context.Usuarios
                    .Where(u => !u.EstaActivo)
                    .OrderByDescending(u => u.Apellido)
                    .ToList(),

                // Lista de los que ya trabajan
                Activos = _context.Usuarios
                    .Where(u => u.EstaActivo)
                    .OrderBy(u => u.Apellido)
                    .ToList()
            };

            return View(viewModel);
        }

        // ACCIÓN: APROBAR Y ASIGNAR ROL
        [HttpPost]
        public IActionResult Aprobar(Guid id, string rolAsignado)
        {
            var usuario = _context.Usuarios.Find(id);
            if (usuario != null)
            {
                usuario.EstaActivo = true;
                usuario.Rol = rolAsignado; // Guardamos el rol que eligió el admin
                _context.SaveChanges();
            }
            return RedirectToAction("Gestionar");
        }

        // ACCIÓN: RECHAZAR / ELIMINAR
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

        // ACCIÓN: CAMBIAR ROL (Para usuarios ya activos)
        [HttpPost]
        public IActionResult CambiarRol(Guid id, string nuevoRol)
        {
            var usuario = _context.Usuarios.Find(id);
            if (usuario != null)
            {
                usuario.Rol = nuevoRol;
                _context.SaveChanges();
            }
            return RedirectToAction("Gestionar");
        }
    }
}