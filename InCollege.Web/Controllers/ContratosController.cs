using InCollege.Datos;
using InCollege.Dominio.Modelos;
using InCollege.Dominio.Patrones.Observer;
using InCollege.Dominio.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InCollege.Dominio.Patrones.Observer; 
using InCollege.Dominio.Servicios;
using System;
using System.Linq;

namespace InCollege.Web.Controllers
{
    public class ContratosController : Controller
    {
        private readonly AppDbContext _context;

        public ContratosController(AppDbContext context)
        {
            _context = context;
        }
        // GET: /Contratos/Index
        public IActionResult Index()
        {
            // Traemos la lista ordenada por fecha (el más nuevo primero)
            var lista = _context.Contratos.OrderByDescending(c => c.FechaCreacion).ToList();
            return View(lista);
        }
        // GET: /Contratos/Crear
        public IActionResult Crear()
        {
            ViewBag.Colegios = new SelectList(_context.Colegios, "Id", "Nombre");
            ViewBag.Productos = new SelectList(_context.Productos, "Id", "Nombre");

            // --- NUEVO: Cargar lista de Vendedores ---
            // Buscamos usuarios activos que sean Vendedores (o Admins si quieres)
            var listaVendedores = _context.Usuarios
                .Where(u => u.EstaActivo == true && (u.Rol == "Vendedor" || u.Rol == "Administrador"))
                .Select(u => new {
                    // Guardamos el Nombre y Apellido como valor para que se vea bien en el contrato
                    NombreCompleto = u.Nombre + " " + u.Apellido
                })
                .ToList();

            // El primer parámetro es el valor que se guarda, el segundo es lo que se ve en la lista
            ViewBag.Vendedores = new SelectList(listaVendedores, "NombreCompleto", "NombreCompleto");
            // -----------------------------------------

            return View();
        }



        // POST: /Contratos/Crear
        [HttpPost]
        public IActionResult Crear(Contrato contrato, string ListaAlumnos)
        {
            var colegio = _context.Colegios.Find(contrato.ColegioId);
            var producto = _context.Productos.Find(contrato.ProductoId);

            if (colegio != null && producto != null)
            {
                // 1. GENERAR CÓDIGO INTELIGENTE (YYMM + Secuencia)
                string prefijoFecha = DateTime.Now.ToString("yyMM");
                int baseCodigo = int.Parse(prefijoFecha + "00");

                var ultimoContrato = _context.Contratos
                    .Where(c => c.Codigo > baseCodigo && c.Codigo < baseCodigo + 99)
                    .OrderByDescending(c => c.Codigo)
                    .FirstOrDefault();

                // Si es el primero del mes, es 01. Si no, el siguiente.
                contrato.Codigo = (ultimoContrato == null) ? baseCodigo + 1 : ultimoContrato.Codigo + 1;

                // 2. COMPLETAR DATOS
                contrato.NombreColegio = colegio.Nombre;
                contrato.NombreProducto = producto.Nombre;

                if (string.IsNullOrEmpty(contrato.VendedorAsignado))
                    contrato.VendedorAsignado = "Admin";

                // 3. GUARDAR EL CONTRATO PRIMERO
                _context.Contratos.Add(contrato);

                // --- AUDITORÍA (Patrón Factory) ---
                // Lo hacemos aquí para que ya tenga el Código generado correcto
                var log = AuditoriaFactory.Crear(
                    "admin@incollege.com",
                    "NUEVO CONTRATO",
                    $"Se creó el contrato #{contrato.Codigo} para {contrato.NombreColegio}."
                );
                _context.Auditorias.Add(log);
                // ----------------------------------

                _context.SaveChanges(); // Guardamos Contrato + Auditoría

                // 4. PROCESAR ALUMNOS
                if (!string.IsNullOrWhiteSpace(ListaAlumnos))
                {
                    var renglones = ListaAlumnos.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                    // Actualizamos la cantidad real en el objeto contrato (aunque ya se guardó, sirve para la lógica visual si la hubiera)
                    contrato.CantidadEgresados = renglones.Length;

                    int contadorAlumno = 1;

                    foreach (var linea in renglones)
                    {
                        var partes = linea.Trim().Split(' ');
                        string apellido = "", nombre = "";

                        if (partes.Length > 1) { apellido = partes[0]; nombre = string.Join(" ", partes.Skip(1)); }
                        else { apellido = linea.Trim(); nombre = "-"; }

                        var estudiante = new Estudiante
                        {
                            ContratoId = contrato.Id,
                            Apellido = apellido.ToUpper(),
                            Nombre = nombre.ToUpper(),

                            // Inicializamos campos opcionales para evitar nulos
                            TalleAbrigo = "-",
                            TalleRemera = "-",
                            Dni = "-",

                            // Código único: 25120101
                            CodigoUnico = $"{contrato.Codigo}{contadorAlumno.ToString("D2")}",

                            EstaAlDia = true,
                            TotalPagado = 0
                        };

                        _context.Estudiantes.Add(estudiante);
                        contadorAlumno++;
                    }

                    // Guardamos los alumnos y la actualización de cantidad del contrato si cambió
                    _context.Contratos.Update(contrato);
                    _context.SaveChanges();
                }

                return RedirectToAction("PanelAdmin", "Home");
            }

            // SI FALLA ALGO, RECARGAMOS LAS LISTAS PARA QUE NO SE ROMPA LA VISTA
            ViewBag.Colegios = new SelectList(_context.Colegios, "Id", "Nombre");
            ViewBag.Productos = new SelectList(_context.Productos, "Id", "Nombre");

            var listaVendedores = _context.Usuarios
                .Where(u => u.EstaActivo == true && (u.Rol == "Vendedor" || u.Rol == "Administrador"))
                .Select(u => new { NombreCompleto = u.Nombre + " " + u.Apellido })
                .ToList();
            ViewBag.Vendedores = new SelectList(listaVendedores, "NombreCompleto", "NombreCompleto");

            return View(contrato);
        }

        // GET: Detalle
        public IActionResult Detalle(Guid id)
        {
            var contrato = _context.Contratos.FirstOrDefault(c => c.Id == id);
            if (contrato == null) return NotFound();
            return View(contrato);
        }

        // Acciones extra (Editar, Eliminar, Firmar, Cancelar)
        public IActionResult Editar(Guid id)
        {
            var contrato = _context.Contratos.Find(id);
            if (contrato == null) return NotFound();
            ViewBag.Colegios = new SelectList(_context.Colegios, "Id", "Nombre", contrato.ColegioId);
            ViewBag.Productos = new SelectList(_context.Productos, "Id", "Nombre", contrato.ProductoId);
            return View(contrato);
        }

        [HttpPost]
        public IActionResult Editar(Contrato contrato)
        {
            _context.Contratos.Update(contrato);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Eliminar(Guid id)
        {
            var contrato = _context.Contratos.Find(id);
            if (contrato != null) { _context.Contratos.Remove(contrato); _context.SaveChanges(); }
            return RedirectToAction("Index");
        }


        [HttpPost]
        public IActionResult Firmar(Guid id)
        {
            var contrato = _context.Contratos
                                   .Include(c => c.Estudiantes)
                                   .FirstOrDefault(c => c.Id == id);

            if (contrato != null && contrato.Estado == "Presupuesto")
            {
                // 1. LÓGICA DE NEGOCIO (Generar código y cambiar estado)
                string prefijoFecha = DateTime.Now.ToString("yyMM");
                int baseCodigo = int.Parse(prefijoFecha + "00");
                var ultimo = _context.Contratos.Where(c => c.Codigo > baseCodigo && c.Codigo < baseCodigo + 99).OrderByDescending(c => c.Codigo).FirstOrDefault();
                contrato.Codigo = (ultimo != null) ? ultimo.Codigo + 1 : baseCodigo + 1;

                contrato.Estado = "Firmado"; // <--- CAMBIO DE ESTADO

                // Actualizar alumnos
                int i = 1;
                foreach (var est in contrato.Estudiantes.OrderBy(e => e.Apellido))
                {
                    est.CodigoUnico = $"{contrato.Codigo}{i.ToString("D2")}";
                    i++;
                }

                // 2. AUDITORÍA (Aquí registramos al usuario REAL)
                // Recuperamos el nombre del usuario de la sesión (o "Admin" si es null)
                string usuarioReal = HttpContext.Session.GetString("UsuarioNombre") ?? "admin@incollege.com";

                var log = AuditoriaFactory.Crear(
                    usuarioReal,
                    "FIRMA CONTRATO",
                    $"Se firmó y activó el contrato #{contrato.Codigo} para {contrato.NombreColegio}."
                );
                _context.Auditorias.Add(log);

                // 3. PATRÓN OBSERVER (Solo notificaciones externas/mails)
                var gestor = new GestorNotificaciones();
                gestor.AgregarObservador(new NotificadorEmail()); // <--- Este manda el mail
                gestor.Avisar(contrato);

                // 4. GUARDAR TODO
                _context.SaveChanges();
            }

            return VolverAlPanelCorrecto();
        }
        [HttpPost]
        public IActionResult Cancelar(Guid id)
        {
            var contrato = _context.Contratos.Find(id);
            if (contrato != null)
            {
                contrato.Estado = "Perdido";

                // --- PATRÓN OBSERVER ---
                var gestor = new GestorNotificaciones();
                var logs = new List<Auditoria>();

                // Solo avisamos a Gerencia (Producción no necesita saber de ventas caídas)
                gestor.AgregarObservador(new NotificadorGerencia(logs));

                gestor.Avisar(contrato);
                _context.Auditorias.AddRange(logs);
                // -----------------------

                _context.SaveChanges();
            }
            return VolverAlPanelCorrecto();
        }
        // MÉTODO PRIVADO PARA DECIDIR A DÓNDE VOLVER
        private IActionResult VolverAlPanelCorrecto()
        {
            // Leemos la memoria para ver qué rol tiene el usuario actual
            string rol = HttpContext.Session.GetString("UsuarioRol");

            if (rol == "Vendedor")
            {
                return RedirectToAction("PanelVendedor", "Home");
            }

            // Si es Admin o cualquier otro, va al panel principal
            return RedirectToAction("PanelAdmin", "Home");
        }
        // GET: /Contratos/AsignarVendedor
        public IActionResult AsignarVendedor()
        {
            // 1. Traemos los contratos que están vigentes o en presupuesto (ignoramos los perdidos)
            var contratos = _context.Contratos
                .Where(c => c.Estado != "Perdido")
                .OrderByDescending(c => c.FechaCreacion)
                .ToList();

            // 2. Traemos la lista de usuarios que son Vendedores (o Admins si también venden)
            var vendedores = _context.Usuarios
                .Where(u => u.EstaActivo) // Solo usuarios activos
                .Select(u => new {
                    NombreCompleto = u.Nombre + " " + u.Apellido
                })
                .ToList();

            // Lo guardamos en ViewBag para usarlo en el desplegable
            ViewBag.ListaVendedores = new SelectList(vendedores, "NombreCompleto", "NombreCompleto");

            return View(contratos);
        }

        // POST: Cambiar el vendedor
        [HttpPost]
        public IActionResult CambiarVendedor(Guid contratoId, string nuevoVendedor)
        {
            var contrato = _context.Contratos.Find(contratoId);

            if (contrato != null)
            {
                // Auditoría del cambio (Opcional pero recomendado)
                // var log = AuditoriaFactory.Crear("Admin", "REASIGNACIÓN", $"Contrato {contrato.Codigo} pasó de {contrato.VendedorAsignado} a {nuevoVendedor}");
                // _context.Auditorias.Add(log);

                contrato.VendedorAsignado = nuevoVendedor;
                _context.SaveChanges();
            }

            // Recargamos la misma página para seguir asignando
            return RedirectToAction("AsignarVendedor");
        }
    }
}