using InCollege.Datos;
using InCollege.Dominio.Modelos;
using InCollege.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace InCollege.Web.Controllers
{
    public class ColegiosController : Controller
    {
        private readonly AppDbContext _context;

        public ColegiosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Colegios/Index -> Muestra la lista
        public IActionResult Index()
        {
            // 1. Traemos todos los colegios
            var colegios = _context.Colegios.ToList();

            // 2. Preparamos la lista para la vista
            var listaVisual = new List<EscuelaViewModel>();

            foreach (var col in colegios)
            {
                // 3. Buscamos el ÚLTIMO contrato de este colegio
                var ultimoContrato = _context.Contratos
                    .Where(c => c.ColegioId == col.Id)
                    .OrderByDescending(c => c.FechaCreacion) // El más nuevo
                    .FirstOrDefault();

                // 4. Llenamos el modelo visual
                var item = new EscuelaViewModel
                {
                    Id = col.Id,
                    Nombre = col.Nombre,
                    Ciudad = col.Ciudad,
                    Direccion = col.Direccion,
                    ContactoNombre = col.ContactoPrincipal,
                    Telefono = col.Telefono,
                    Email = col.Email,
                };

                // 5. Determinamos el ESTADO según el contrato
                if (ultimoContrato == null)
                {
                    item.EstadoActual = "Nuevo / Sin Gestión";
                    item.Vendedor = "-";
                    item.UltimoProducto = "-";
                }
                else
                {
                    item.EstadoActual = ultimoContrato.Estado; // "Presupuesto", "Firmado", "Perdido"
                    item.Vendedor = ultimoContrato.VendedorAsignado;
                    item.UltimoProducto = ultimoContrato.NombreProducto;
                }

                listaVisual.Add(item);
            }

            return View(listaVisual);
        }

        // GET: /Colegios/Crear -> Muestra el formulario
        public IActionResult Crear()
        {
            return View();
        }

        // POST: Recibe los datos del formulario
        // POST: /Colegios/Crear
        [HttpPost]
        public IActionResult Crear(Colegio colegio)
        {
            // 1. PARCHE DE DATOS:
            // Como borramos el input de Email del formulario, llega nulo.
            // Le asignamos un guion para que la base de datos no se queje.
            if (string.IsNullOrEmpty(colegio.Email)) colegio.Email = "-";
            if (string.IsNullOrEmpty(colegio.Direccion)) colegio.Direccion = "-";
            if (string.IsNullOrEmpty(colegio.Ciudad)) colegio.Ciudad = "-";
            if (string.IsNullOrEmpty(colegio.Telefono)) colegio.Telefono = "-";

            // 2. Limpiamos los errores de validación de esos campos
            ModelState.Remove("Email");
            ModelState.Remove("Direccion");
            ModelState.Remove("Ciudad");
            ModelState.Remove("Telefono");

            // 3. Ahora sí verificamos
            if (ModelState.IsValid)
            {
                _context.Colegios.Add(colegio);
                _context.SaveChanges();

                // Redirigir a Ver Escuelas (Index) o al Dashboard, lo que prefieras
                return RedirectToAction("Index");
            }

            // Si llega acá es porque falló (ej: faltó el Nombre).
            return View(colegio);
        }
        // GET: /Colegios/Editar/5
        public IActionResult Editar(Guid id)
        {
            var colegio = _context.Colegios.Find(id);
            if (colegio == null) return NotFound();
            return View(colegio);
        }

        // POST: /Colegios/Editar
        [HttpPost]
        public IActionResult Editar(Colegio colegio)
        {
            if (ModelState.IsValid)
            {
                _context.Colegios.Update(colegio);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(colegio);
        }

        // POST: /Colegios/Eliminar/5
        [HttpPost]
        public IActionResult Eliminar(Guid id)
        {
            var colegio = _context.Colegios.Find(id);
            if (colegio != null)
            {
                _context.Colegios.Remove(colegio);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}