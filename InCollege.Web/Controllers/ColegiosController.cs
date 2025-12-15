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
            var colegios = _context.Colegios.ToList();
            var listaVisual = new List<EscuelaViewModel>();

            // Traemos todos los contratos de una sola vez para no hacer mil consultas
            var todosLosContratos = _context.Contratos.ToList();

            foreach (var col in colegios)
            {
                var contratosDelColegio = todosLosContratos.Where(c => c.ColegioId == col.Id).ToList();

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

                // --- LÓGICA DE ESTADO DEL CLIENTE ---
                if (contratosDelColegio.Any(c => c.Estado == "Firmado" || c.Estado == "En Producción" || c.Estado == "Entregado"))
                {
                    // Si tiene AL MENOS UN contrato vigente, es un cliente activo (aunque tenga otros perdidos)
                    item.EstadoActual = "Firmado";

                    // Mostramos el vendedor de ese contrato activo
                    item.Vendedor = contratosDelColegio.First(c => c.Estado == "Firmado" || c.Estado == "En Producción").VendedorAsignado;
                }
                else if (contratosDelColegio.Any(c => c.Estado == "Presupuesto"))
                {
                    // Si no firmó nada pero tiene presupuestos abiertos
                    item.EstadoActual = "Presupuesto";
                    item.Vendedor = contratosDelColegio.First(c => c.Estado == "Presupuesto").VendedorAsignado;
                }
                else if (contratosDelColegio.Any(c => c.Estado == "Perdido"))
                {
                    // Si todo lo que tiene es histórico perdido (Ideal para volver a llamar)
                    item.EstadoActual = "Perdido";
                    item.Vendedor = "-";
                }
                else
                {
                    // Escuela cargada sin contratos aún
                    item.EstadoActual = "Nuevo";
                    item.Vendedor = "-";
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
            // --- 1. PARCHE PARA CAMPOS OPCIONALES ---
            // Si el usuario no escribe dirección, le ponemos un guion "-"
            // para que la Base de Datos no explote.
            if (string.IsNullOrEmpty(colegio.Direccion)) colegio.Direccion = "-";

            // Lo mismo para otros campos que quieras opcionales:
            if (string.IsNullOrEmpty(colegio.Ciudad)) colegio.Ciudad = "-";
            if (string.IsNullOrEmpty(colegio.Telefono)) colegio.Telefono = "-";
            if (string.IsNullOrEmpty(colegio.Email)) colegio.Email = "-";

            // --- 2. QUITAR ERRORES DE VALIDACIÓN ---
            // Le decimos a C#: "Ignora si estos campos están vacíos"
            ModelState.Remove("Direccion");
            ModelState.Remove("Ciudad");
            ModelState.Remove("Telefono");
            ModelState.Remove("Email");
            ModelState.Remove("VendedorResponsable"); // Este se calcula solo

            // --- 3. LÓGICA DE ZONAS (Tu código existente) ---
            if (!string.IsNullOrEmpty(colegio.Zona))
            {
                var vendedorZona = _context.Usuarios
                    .FirstOrDefault(u => u.Rol == "Vendedor" && u.Zona == colegio.Zona && u.EstaActivo);

                if (vendedorZona != null)
                {
                    colegio.VendedorResponsable = $"{vendedorZona.Nombre} {vendedorZona.Apellido}";
                }
                else
                {
                    colegio.VendedorResponsable = "Admin (Zona Libre)";
                }
            }
            else
            {
                colegio.VendedorResponsable = "Sin Asignar";
            }

            // --- 4. GUARDAR ---
            if (ModelState.IsValid)
            {
                _context.Colegios.Add(colegio);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

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