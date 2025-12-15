using Microsoft.AspNetCore.Mvc;
using InCollege.Datos;
using System.Linq;

namespace InCollege.Web.Controllers
{
    public class ProductosController : Controller
    {
        private readonly AppDbContext _context;

        public ProductosController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Traemos todos los productos (Kits y Prendas)
            var productos = _context.Productos.ToList();
            return View(productos);
        }
    }
}
