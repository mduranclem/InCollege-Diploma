using Microsoft.AspNetCore.Mvc;
using InCollege.Datos;
using InCollege.Dominio.Modelos;
using System.Linq;

namespace InCollege.Web.Controllers
{
    public class AuditoriaController : Controller
    {
        private readonly AppDbContext _context;

        public AuditoriaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Auditoria/Index
        public IActionResult Index()
        {
            // Traemos los últimos 100 eventos, del más nuevo al más viejo
            var logs = _context.Auditorias
                .OrderByDescending(a => a.Fecha)
                .Take(100)
                .ToList();

            return View(logs);
        }
    }
}