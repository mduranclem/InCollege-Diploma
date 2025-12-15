using Microsoft.AspNetCore.Mvc;
using InCollege.Datos;
using InCollege.Dominio.Modelos;
using System.Linq;

namespace InCollege.Web.Controllers
{
    public class TalleresController : Controller
    {
        private readonly AppDbContext _context;

        public TalleresController(AppDbContext context)
        {
            _context = context;
        }

        // 1. LISTA DE TALLERES
        public IActionResult Index()
        {
            var talleres = _context.Talleres.Where(t => t.Activo).ToList();
            return View(talleres);
        }

        // 2. CREAR (VISTA)
        public IActionResult Crear()
        {
            return View();
        }

        // 3. CREAR (LÓGICA)
        [HttpPost]
        public IActionResult Crear(Taller taller)
        {
            if (ModelState.IsValid)
            {
                _context.Talleres.Add(taller);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(taller);
        }

        // 4. ELIMINAR (LÓGICA)
        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            var taller = _context.Talleres.Find(id);
            if (taller != null)
            {
                taller.Activo = false; // Borrado lógico para no romper historial
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        // VER DETALLE Y TRABAJOS EN CURSO
        public IActionResult Detalle(int id)
        {
            var taller = _context.Talleres.Find(id);
            if (taller == null) return NotFound();

            // Buscamos los contratos que están asignados a este taller
            // Y que NO estén terminados ni perdidos (solo lo activo)
            var trabajosEnCurso = _context.Contratos
                                          .Where(c => c.TallerAsignadoId == id &&
                                                      c.Estado == "En Producción")
                                          .OrderByDescending(c => c.FechaCreacion)
                                          .ToList();

            // Usamos ViewBag para pasar la lista extra
            ViewBag.Trabajos = trabajosEnCurso;

            return View(taller);
        }
    }
}