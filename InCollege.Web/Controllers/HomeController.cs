using Microsoft.AspNetCore.Mvc;
using InCollege.Datos;            // Acceso a la DB
using InCollege.Dominio.Modelos;  // Acceso a los Modelos
using InCollege.Dominio.Patrones; // Acceso a PrendaIndividual y Kit
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

            // --- SECCIÓN DE SEMILLA (SEEDING) ---
            // Se ejecuta cada vez que se llama al controlador si los datos no existen.

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

            // 3. SEMILLA DE KITS
            // Verificamos si no hay Kits cargados.
            if (!_context.Productos.OfType<KitComposite>().Any())
            {
                // Buscamos las prendas base usando 'Contains' para ser más flexibles
                var buzo = _context.Productos.FirstOrDefault(p => p.Nombre.Contains("Buzo"));
                var campera = _context.Productos.FirstOrDefault(p => p.Nombre.Contains("Campera"));
                var remera = _context.Productos.FirstOrDefault(p => p.Nombre.Contains("Remera"));
                var chomba = _context.Productos.FirstOrDefault(p => p.Nombre.Contains("Chomba"));

                // Solo si encontramos las partes, armamos los robots (Kits)
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
            // Si el usuario ya está logueado, redirigirlo a su panel correspondiente
            var rol = HttpContext.Session.GetString("UsuarioRol");

            // CORRECCIÓN 1: Aceptamos ambos nombres para el admin en la redirección inicial
            if (rol == "Administrador" || rol == "Admin") return RedirectToAction("PanelAdmin");

            if (rol == "Vendedor") return RedirectToAction("PanelVendedor");

            // Redirige al nuevo controlador 'Disenos'
            if (rol == "Diseñador" || rol == "Disenador") return RedirectToAction("Index", "Disenos");

            return View();
        }

        public IActionResult Registro()
        {
            return View();
        }

        // --- ACCIONES (LÓGICA) ---

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

                // --- GUARDAR DATOS EN MEMORIA (SESSION) ---
                HttpContext.Session.SetString("UsuarioRol", usuario.Rol);
                HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre);
                // ------------------------------------------

                // CORRECCIÓN 2: Aceptamos ambos nombres en el Login
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
                    // CAMBIO IMPORTANTE: Redirigir a "Panel", no a "Index"
                    return RedirectToAction("Panel", "Disenos");
                }
                else
                {
                    // Por defecto si el rol no coincide con nada conocido
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
            // Validar si el email ya existe
            if (_context.Usuarios.Any(u => u.Email == email))
            {
                ViewBag.Error = "Ese correo ya está registrado.";
                return View("Registro");
            }

            // Crear nuevo usuario
            Usuario nuevo = new Usuario(nombre, apellido, email, password);

            // Como la DB obliga a tener una Zona, le ponemos una por defecto.
            nuevo.Zona = "Sin Asignar";

            // Aseguramos también que nazca inactivo y sin rol definido
            nuevo.EstaActivo = false;
            nuevo.Rol = "Sin Asignar";

            _context.Usuarios.Add(nuevo);
            _context.SaveChanges();

            ViewBag.Exito = "Solicitud enviada. Espera la aprobación del Admin.";
            return View("Index");
        }

        // Acción para Cerrar Sesión (Conectada al botón del Layout)
        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear(); // Limpia la memoria
            return RedirectToAction("Index");
        }

        // --- DASHBOARD DEL ADMINISTRADOR ---

        public IActionResult PanelAdmin()
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");

            // CORRECCIÓN 3: Seguridad blindada para ambos nombres
            // Si NO es Administrador Y TAMPOCO es Admin, entonces sácalo.
            if (rol != "Administrador" && rol != "Admin")
                return RedirectToAction("Index");

            // 1. CONTRATOS ACTIVOS (Ni Presupuesto, ni Perdidos)
            ViewBag.TotalContratos = _context.Contratos
                .Where(c => c.Estado != "Presupuesto" && c.Estado != "Perdido")
                .Count();

            // 2. CONTRATOS PERDIDOS
            ViewBag.ContratosPerdidos = _context.Contratos
                .Where(c => c.Estado == "Perdido")
                .Count();

            var contratos = _context.Contratos.OrderByDescending(c => c.FechaCreacion).ToList();

            ViewBag.EnProduccion = _context.Contratos.Where(c => c.Estado == "En Producción").Count();
            ViewBag.PendientesAprobacion = _context.Usuarios.Where(u => !u.EstaActivo).Count();
            ViewBag.ListaTalleres = _context.Talleres.Where(t => t.Activo).ToList();

            return View(contratos);
        }

        // --- DASHBOARD DEL VENDEDOR ---

        public IActionResult PanelVendedor()
        {
            // Seguridad básica: Verificar Rol
            if (HttpContext.Session.GetString("UsuarioRol") != "Vendedor")
                return RedirectToAction("Index");

            // Cargar datos para el vendedor (Contratos recientes)
            var contratos = _context.Contratos
                .OrderByDescending(c => c.FechaCreacion)
                .Take(10)
                .ToList();

            ViewBag.TotalContratos = _context.Contratos.Count();

            return View(contratos);
        }
        // ==========================================
        // RECUPERAR CONTRASEÑA
        // ==========================================

        // 1. VISTA: Pide el email
        public IActionResult OlvidePassword()
        {
            return View();
        }

        // 2. PROCESO: Genera token y envía mail
        [HttpPost]
        public IActionResult OlvidePassword(string email)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == email);

            // Por seguridad, no decimos si el email existe o no, pero aquí actuamos si existe.
            if (usuario != null)
            {
                // Generar Token único y fecha de expiración (1 hora)
                string token = Guid.NewGuid().ToString();
                usuario.TokenRecuperacion = token;
                usuario.TokenExpiracion = DateTime.Now.AddHours(1);
                _context.SaveChanges();

                // Crear el link de recuperación
                // Esto genera algo como: https://localhost:7048/Home/Restablecer?token=abc-123...
                var link = Url.Action("Restablecer", "Home", new { token = token }, Request.Scheme);

                // Enviar Email
                string asunto = "Recuperar Contraseña - InCollege";
                string cuerpo = $@"
                    <h2>Hola {usuario.Nombre},</h2>
                    <p>Recibimos una solicitud para restablecer tu contraseña.</p>
                    <p>Haz clic en el siguiente enlace para crear una nueva clave:</p>
                    <a href='{link}' style='background:#1e6f42; color:white; padding:10px 20px; text-decoration:none; border-radius:5px;'>RESTABLECER AHORA</a>
                    <p>Este enlace expira en 1 hora.</p>";

                try
                {
                    // Usamos tu servicio de Email existente (asegúrate de tener el using InCollege.Dominio.Servicios;)
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

        // 3. VISTA: Formulario para poner la nueva clave (Valida el token)
        public IActionResult Restablecer(string token)
        {
            // Buscar usuario con ese token y que no haya expirado
            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.TokenRecuperacion == token && u.TokenExpiracion > DateTime.Now);

            if (usuario == null)
            {
                ViewBag.Error = "El enlace ha expirado o no es válido.";
                return View("Login"); // O una vista de error
            }

            ViewBag.Token = token; // Pasamos el token a la vista para enviarlo después
            return View();
        }

        // 4. PROCESO: Guarda la nueva clave
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
                usuario.Password = password; // Guardamos la nueva clave

                // Limpiamos el token para que no se pueda usar de nuevo
                usuario.TokenRecuperacion = null;
                usuario.TokenExpiracion = null;

                _context.SaveChanges();

                ViewBag.Exito = "Contraseña actualizada. Ya puedes iniciar sesión.";
                return View("Index"); // Volvemos al Login
            }

            ViewBag.Error = "Error al restablecer. Intenta solicitar un nuevo enlace.";
            return View("Index");
        }
    }
}