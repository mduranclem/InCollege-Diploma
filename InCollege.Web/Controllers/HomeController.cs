using Microsoft.AspNetCore.Mvc;
using InCollege.Datos;            // Mis accesos a la DB
using InCollege.Dominio.Modelos;  // Mis accesos a los Modelos
using InCollege.Dominio.Patrones; // Mis accesos a PrendaIndividual y Kit
using System.Linq;
using Microsoft.AspNetCore.Http;
using System;

namespace InCollege.Web.Controllers
{
    public class HomeController : Controller
    {
        // Mi conexión a la base de datos SQL Server
        private readonly AppDbContext _context;

        // Inyecto las dependencias en mi constructor
        public HomeController(AppDbContext context)
        {
            _context = context;

            // --- MI SECCIÓN DE SEMILLA (SEEDING) ---
            // Ejecuto esta carga inicial de datos básicos si mis tablas están vacías.

            // 1. Creo el usuario Administrador principal por defecto
            if (!_context.Usuarios.Any())
            {
                Usuario superAdmin = new Usuario("Admin", "Principal", "admin@incollege.com", "admin123");
                superAdmin.Rol = "Administrador";
                superAdmin.EstaActivo = true;
                _context.Usuarios.Add(superAdmin);
                _context.SaveChanges();
            }

            // 2. Cargo mi catálogo de prendas sueltas
            if (!_context.Productos.Any())
            {
                var p1 = new PrendaIndividual("Buzo", 45000, "Algodón Frisado");
                var p2 = new PrendaIndividual("Campera", 48000, "Algodón Frisado");
                var p3 = new PrendaIndividual("Remera", 18000, "Jersey Peinado");
                var p4 = new PrendaIndividual("Chomba", 22000, "Piqué");

                _context.Productos.AddRange(p1, p2, p3, p4);
                _context.SaveChanges();
            }

            // 3. Genero los Combos usando mi Patrón Composite
            if (!_context.Productos.OfType<KitComposite>().Any())
            {
                // Busco las prendas base para armar mis kits
                var buzo = _context.Productos.FirstOrDefault(p => p.Nombre.Contains("Buzo"));
                var campera = _context.Productos.FirstOrDefault(p => p.Nombre.Contains("Campera"));
                var remera = _context.Productos.FirstOrDefault(p => p.Nombre.Contains("Remera"));
                var chomba = _context.Productos.FirstOrDefault(p => p.Nombre.Contains("Chomba"));

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

        // --- MIS VISTAS (PANTALLAS PRINCIPALES) ---

        public IActionResult Index()
        {
            // Verifico si el usuario ya tiene sesión activa para enviarlo a su panel
            var rol = HttpContext.Session.GetString("UsuarioRol");

            if (rol == "Administrador" || rol == "Admin") return RedirectToAction("PanelAdmin");
            if (rol == "Vendedor") return RedirectToAction("PanelVendedor");
            if (rol == "Diseñador" || rol == "Disenador") return RedirectToAction("Panel", "Disenos");

            return View();
        }

        public IActionResult Registro()
        {
            return View();
        }

        // --- MI LÓGICA DE ACCESO ---

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.Email == username && u.Password == password);

            if (usuario != null)
            {
                // Verifico si la cuenta fue aprobada
                if (!usuario.EstaActivo)
                {
                    ViewBag.Error = "Acceso denegado. Tu cuenta espera aprobación.";
                    return View("Index");
                }

                // Guardo los datos del usuario en la memoria de la sesión
                HttpContext.Session.SetString("UsuarioRol", usuario.Rol);
                HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre);

                // Redirijo según el rol que detecté
                if (usuario.Rol == "Administrador" || usuario.Rol == "Admin")
                {
                    return RedirectToAction("PanelAdmin");
                }
                else if (usuario.Rol == "Vendedor")
                {
                    return RedirectToAction("PanelVendedor");
                }
                else if (usuario.Rol == "Diseñador" || usuario.Rol == "Disenador")
                {
                    return RedirectToAction("Panel", "Disenos");
                }
                else
                {
                    return Content($"Hola {usuario.Nombre}. Tu rol '{usuario.Rol}' no tiene panel asignado.");
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
            // Valido que no haya duplicados en mi base de datos
            if (_context.Usuarios.Any(u => u.Email == email))
            {
                ViewBag.Error = "Ese correo ya está registrado.";
                return View("Registro");
            }

            Usuario nuevo = new Usuario(nombre, apellido, email, password);
            nuevo.Zona = "Sin Asignar";
            nuevo.EstaActivo = false; // Requiere aprobación manual mía (como Admin)
            nuevo.Rol = "Sin Asignar";

            _context.Usuarios.Add(nuevo);
            _context.SaveChanges();

            ViewBag.Exito = "Solicitud enviada. Espera la aprobación del Admin.";
            return View("Index");
        }

        public IActionResult CerrarSesion()
        {
            // Limpio la memoria
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        // --- MIS DASHBOARDS ---

        public IActionResult PanelAdmin()
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");

            // Mi filtro de seguridad para que solo ingresen administradores
            if (rol != "Administrador" && rol != "Admin")
                return RedirectToAction("Index");

            // 1. Calculo mis métricas principales para las tarjetas (KPIs)
            ViewBag.TotalContratos = _context.Contratos.Where(c => c.Estado != "Presupuesto" && c.Estado != "Perdido").Count();
            ViewBag.ContratosPerdidos = _context.Contratos.Where(c => c.Estado == "Perdido").Count();
            ViewBag.EnProduccion = _context.Contratos.Where(c => c.Estado == "En Producción").Count();
            ViewBag.PendientesAprobacion = _context.Usuarios.Where(u => !u.EstaActivo).Count();
            ViewBag.ListaTalleres = _context.Talleres.Where(t => t.Activo).ToList();

            // 2. Preparo los datos en tiempo real para mi Gráfico de Torta (Estado Global)
            ViewBag.CantFirmados = _context.Contratos.Count(c => c.Estado == "Firmado" || c.Estado == "En Producción" || c.Estado == "Entregado");
            ViewBag.CantPerdidos = _context.Contratos.Count(c => c.Estado == "Perdido");
            ViewBag.CantPresupuestos = _context.Contratos.Count(c => c.Estado == "Presupuesto");

            // 3. Preparo los datos para mi Gráfico de Avance de Producción (Solo los confirmados)
            ViewBag.CantSoloFirmados = _context.Contratos.Count(c => c.Estado == "Firmado");
            ViewBag.CantEnProduccion = _context.Contratos.Count(c => c.Estado == "En Producción");
            ViewBag.CantEntregados = _context.Contratos.Count(c => c.Estado == "Entregado");

            // 4. Preparo los datos para mi Gráfico de Barras (Rendimiento por vendedor activo)
            var ventasPorVendedor = _context.Contratos
                .Where(c => c.Estado != "Perdido" && c.Estado != "Presupuesto")
                .GroupBy(c => c.VendedorAsignado)
                .Select(g => new { Vendedor = g.Key ?? "Sin Asignar", Cantidad = g.Count() })
                .ToList();

            // Paso las listas separadas para que Chart.js las lea fácilmente en mi vista
            ViewBag.NombresVendedores = ventasPorVendedor.Select(v => v.Vendedor).ToList();
            ViewBag.CantidadesVentas = ventasPorVendedor.Select(v => v.Cantidad).ToList();

            // Cargo la lista de contratos para mi tabla inferior
            var contratos = _context.Contratos.OrderByDescending(c => c.FechaCreacion).ToList();

            return View(contratos);
        }

        public IActionResult PanelVendedor()
        {
            if (HttpContext.Session.GetString("UsuarioRol") != "Vendedor")
                return RedirectToAction("Index");

            var contratos = _context.Contratos
                .OrderByDescending(c => c.FechaCreacion)
                .Take(10)
                .ToList();

            ViewBag.TotalContratos = _context.Contratos.Count();

            return View(contratos);
        }

        // ==========================================
        // MI RECUPERACIÓN DE CONTRASEÑA
        // ==========================================

        public IActionResult OlvidePassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult OlvidePassword(string email)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == email);

            if (usuario != null)
            {
                string token = Guid.NewGuid().ToString();
                usuario.TokenRecuperacion = token;
                usuario.TokenExpiracion = DateTime.Now.AddHours(1);
                _context.SaveChanges();

                var link = Url.Action("Restablecer", "Home", new { token = token }, Request.Scheme);

                string asunto = "Recuperar Contraseña - InCollege";
                string cuerpo = $@"
                    <h2>Hola {usuario.Nombre},</h2>
                    <p>Recibimos una solicitud para restablecer tu contraseña.</p>
                    <p>Haz clic en el siguiente enlace para crear una nueva clave:</p>
                    <a href='{link}' style='background:#1e6f42; color:white; padding:10px 20px; text-decoration:none; border-radius:5px;'>RESTABLECER AHORA</a>
                    <p>Este enlace expira en 1 hora.</p>";

                try
                {
                    InCollege.Dominio.Servicios.EmailService.Enviar(usuario.Email, asunto, cuerpo);
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error al enviar correo: " + ex.Message;
                    return View();
                }
            }

            ViewBag.Mensaje = "Si el correo existe, te hemos enviado las instrucciones.";
            return View();
        }

        public IActionResult Restablecer(string token)
        {
            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.TokenRecuperacion == token && u.TokenExpiracion > DateTime.Now);

            if (usuario == null)
            {
                ViewBag.Error = "El enlace ha expirado o no es válido.";
                return View("Login");
            }

            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public IActionResult Restablecer(string token, string password, string confirmarPassword)
        {
            if (password != confirmarPassword)
            {
                ViewBag.Error = "Las contraseñas no coinciden.";
                ViewBag.Token = token;
                return View();
            }

            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.TokenRecuperacion == token && u.TokenExpiracion > DateTime.Now);

            if (usuario != null)
            {
                usuario.Password = password;
                usuario.TokenRecuperacion = null;
                usuario.TokenExpiracion = null;

                _context.SaveChanges();

                ViewBag.Exito = "Contraseña actualizada. Ya puedes iniciar sesión.";
                return View("Index");
            }

            ViewBag.Error = "Error al restablecer. Intenta solicitar un nuevo enlace.";
            return View("Index");
        }
    }
}