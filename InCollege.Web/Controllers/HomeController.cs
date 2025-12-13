using Microsoft.AspNetCore.Mvc;
using InCollege.Datos;           // Acceso a la DB
using InCollege.Dominio.Modelos; // Acceso a los Modelos
using InCollege.Dominio.Patrones; // IMPORTANTE: Acceso a PrendaIndividual y Kit
using System.Linq;

namespace InCollege.Web.Controllers
{
    public class HomeController : Controller
    {
        // ESTA ES LA CONEXIÓN A SQL SERVER
        private readonly AppDbContext _context;

        // El constructor recibe la base de datos automáticamente (Inyección de Dependencias)
        public HomeController(AppDbContext context)
        {
            _context = context;

            // 1. SEMILLA DE USUARIOS (Si no existe, crea Admin)
            if (!_context.Usuarios.Any())
            {
                Usuario superAdmin = new Usuario("Admin", "Principal", "admin@incollege.com", "admin123");
                superAdmin.Rol = "Administrador";
                superAdmin.EstaActivo = true;
                _context.Usuarios.Add(superAdmin);
                _context.SaveChanges();
            }

            // 2. SEMILLA DE PRENDAS SUELTAS
            if (!_context.Productos.Any())
            {
                var p1 = new PrendaIndividual("Buzo", 45000, "Algodón Frisado");
                var p2 = new PrendaIndividual("Campera", 48000, "Algodón Frisado");
                var p3 = new PrendaIndividual("Remera", 18000, "Jersey Peinado");
                var p4 = new PrendaIndividual("Chomba", 22000, "Piqué");

                _context.Productos.AddRange(p1, p2, p3, p4);
                _context.SaveChanges();
            }

            // 3. SEMILLA DE KITS (CORREGIDA PARA QUE FUNCIONE SI O SI)
            // Verificamos si no hay Kits cargados.
            if (!_context.Productos.OfType<KitComposite>().Any())
            {
                // CORRECCIÓN: Usamos 'Contains' para ser más flexibles buscando las prendas
                var buzo = _context.Productos.FirstOrDefault(p => p.Nombre.Contains("Buzo"));
                var campera = _context.Productos.FirstOrDefault(p => p.Nombre.Contains("Campera"));
                var remera = _context.Productos.FirstOrDefault(p => p.Nombre.Contains("Remera"));
                var chomba = _context.Productos.FirstOrDefault(p => p.Nombre.Contains("Chomba"));

                // Solo si encontramos las partes, armamos los robots
                if (buzo != null && campera != null && remera != null && chomba != null)
                {
                    var kit1 = new KitComposite("COMBO 1: Buzo + Remera");
                    kit1.Agregar(buzo); kit1.Agregar(remera); kit1.DescuentoPorCombo = 10;

                    var kit2 = new KitComposite("COMBO 2: Buzo + Chomba");
                    kit2.Agregar(buzo); kit2.Agregar(chomba); kit2.DescuentoPorCombo = 10;

                    var kit3 = new KitComposite("COMBO 3: Campera + Remera");
                    kit3.Agregar(campera); kit3.Agregar(remera); kit3.DescuentoPorCombo = 10;

                    var kit4 = new KitComposite("COMBO 4: Campera + Chomba");
                    kit4.Agregar(campera); kit4.Agregar(chomba); kit4.DescuentoPorCombo = 10;

                    _context.Productos.AddRange(kit1, kit2, kit3, kit4);
                    _context.SaveChanges();
                }
            }
        }

        // --- VISTAS (PANTALLAS) ---

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Registro()
        {
            return View();
        }

        // --- ACCIONES (LÓGICA) ---

        [HttpPost]
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var usuario = _context.Usuarios
        .FirstOrDefault(u => u.Email == username && u.Password == password);

            if (usuario != null)
            {
                if (!usuario.EstaActivo)
                {
                    ViewBag.Error = "Acceso denegado. Tu cuenta espera aprobación.";
                    return View("Index");
                }

                // --- NUEVO: GUARDAR DATOS EN MEMORIA (SESSION) ---
                // Esto permite que los otros controladores sepan quién eres
                HttpContext.Session.SetString("UsuarioRol", usuario.Rol);
                HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre);
                // -------------------------------------------------

                if (usuario.Rol == "Administrador")
                {
                    return RedirectToAction("PanelAdmin");
                }
                else if (usuario.Rol == "Vendedor")
                {
                    return RedirectToAction("PanelVendedor");
                }
                else
                {
                    // Por defecto (o Taller/Diseñador en el futuro)
                    return Content($"Hola {usuario.Nombre}. Tu panel está en construcción.");
                }
            }
            else
            {
                ViewBag.Error = "Usuario o contraseña incorrectos.";
                return View("Index");
            }
        }

        [HttpPost]
        public IActionResult RegistrarUsuario(string nombre, string apellido, string email, string password)
        {
            // Validar si el email ya existe en SQL
            if (_context.Usuarios.Any(u => u.Email == email))
            {
                ViewBag.Error = "Ese correo ya está registrado.";
                return View("Registro");
            }

            // Crear nuevo usuario (Nace inactivo)
            Usuario nuevo = new Usuario(nombre, apellido, email, password);

            _context.Usuarios.Add(nuevo);
            _context.SaveChanges(); // INSERT INTO Usuarios...

            ViewBag.Exito = "Solicitud enviada. Espera la aprobación del Admin.";
            return View("Index"); // Volvemos al Login
        }

        // --- DASHBOARD DEL ADMINISTRADOR ---

        public IActionResult PanelAdmin()
        {
            // 1. CONTRATOS ACTIVOS: Son los que NO son Presupuesto Y TAMPOCO son Perdidos
            ViewBag.TotalContratos = _context.Contratos
                .Where(c => c.Estado != "Presupuesto" && c.Estado != "Perdido")
                .Count();

            // 2. NUEVO KPI: CONTRATOS PERDIDOS
            ViewBag.ContratosPerdidos = _context.Contratos
                .Where(c => c.Estado == "Perdido")
                .Count();

            ViewBag.EnProduccion = _context.Contratos.Where(c => c.Estado == "En Producción").Count();
            ViewBag.PendientesAprobacion = _context.Usuarios.Where(u => !u.EstaActivo).Count();

            var listaContratos = _context.Contratos.OrderByDescending(c => c.FechaCreacion).ToList();

            return View(listaContratos);
        }
        public IActionResult PanelVendedor()
        {
            // Cargar datos para el vendedor (Contratos recientes)
            var contratos = _context.Contratos
                .OrderByDescending(c => c.FechaCreacion)
                .Take(10) // Solo los últimos 10 para no saturar
                .ToList();

            ViewBag.TotalContratos = _context.Contratos.Count();

            return View(contratos);
        }
    }
}