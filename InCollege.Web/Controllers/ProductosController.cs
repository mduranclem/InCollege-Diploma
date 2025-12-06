using Microsoft.AspNetCore.Mvc;
using InCollege.Datos;
using InCollege.Dominio.Patrones; // Importante: Acceso a las clases del Patrón
using System.Linq;
using System;

namespace InCollege.Web.Controllers
{
    public class ProductosController : Controller
    {
        private readonly AppDbContext _context;

        public ProductosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Productos/Index
        // Muestra el catálogo completo. Gracias a EF Core y el patrón Composite,
        // esta lista trae tanto 'PrendaIndividual' como 'KitComposite' mezclados.
        public IActionResult Index()
        {
            var productos = _context.Productos.ToList();
            return View(productos);
        }

        // GET: /Productos/CrearPrenda
        // Muestra el formulario para crear una "Hoja" (Leaf)
        public IActionResult CrearPrenda()
        {
            return View();
        }

        // POST: /Productos/CrearPrenda
        // Recibe los datos y guarda una PrendaIndividual en la base de datos
        [HttpPost]
        public IActionResult CrearPrenda(string nombre, decimal precio, string tela)
        {
            if (string.IsNullOrEmpty(nombre) || precio <= 0)
            {
                ModelState.AddModelError("", "Datos inválidos");
                return View();
            }

            // 1. Instanciamos la HOJA del patrón Composite
            var nuevaPrenda = new PrendaIndividual(nombre, precio, tela);

            // 2. Guardamos en la tabla polimórfica 'Productos'
            _context.Productos.Add(nuevaPrenda);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: /Productos/SimularCombo
        // ACCIÓN DEMOSTRATIVA DEL PATRÓN COMPOSITE
        // Muestra cómo se agrupan prendas en un Kit y se calcula el precio total.
        public IActionResult SimularCombo()
        {
            // 1. Buscamos componentes base (Hojas) en la DB
            // Usamos FirstOrDefault para no romper si no existen, pero asumimos que el Seed los creó.
            var buzo = _context.Productos.FirstOrDefault(p => p.Nombre.Contains("Buzo"));
            var remera = _context.Productos.FirstOrDefault(p => p.Nombre.Contains("Remera"));

            if (buzo == null || remera == null)
            {
                return Content("Error: Faltan productos base (Buzo o Remera) para armar el kit. Ejecuta el Seed primero.");
            }

            // 2. Creamos el COMPOSITE (El Kit)
            // Nota: Esto lo hacemos en memoria para probar el cálculo, no lo guardamos en DB todavía.
            KitComposite packEgresado = new KitComposite("Pack Full (Buzo + Remera)");

            // Configuramos un descuento del 10%
            packEgresado.DescuentoPorCombo = 10;

            // 3. Agregamos las hojas al árbol del Composite
            packEgresado.Agregar(buzo);
            packEgresado.Agregar(remera);

            // 4. EJECUTAMOS LA LÓGICA DEL PATRÓN
            // Al llamar a ObtenerPrecio(), el Kit recorre sus hijos, suma sus precios
            // y aplica el descuento, sin que nosotros tengamos que sumar a mano aquí.
            decimal precioCalculado = packEgresado.ObtenerPrecio();
            decimal precioRealSinDescuento = buzo.ObtenerPrecio() + remera.ObtenerPrecio();

            // 5. Mostramos el resultado en pantalla simple
            return Content($"DEMOSTRACIÓN PATRÓN COMPOSITE:\n\n" +
                           $"Producto Compuesto: {packEgresado.Nombre}\n" +
                           $"Incluye: {buzo.Nombre} (${buzo.ObtenerPrecio()}) + {remera.Nombre} (${remera.ObtenerPrecio()})\n" +
                           $"----------------------------------------\n" +
                           $"Precio Suma Real: ${precioRealSinDescuento}\n" +
                           $"Precio Final (Con lógica Composite -10%): ${precioCalculado}");
        }
    }
}
