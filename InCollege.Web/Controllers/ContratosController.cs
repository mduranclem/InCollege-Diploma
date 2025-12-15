using InCollege.Datos;
using InCollege.Dominio.Modelos;
using InCollege.Dominio.Patrones.Observer;
using InCollege.Dominio.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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

        // ==========================================
        // CREAR NUEVO CONTRATO (GET y POST)
        // ==========================================

        // GET: Muestra el formulario vacío
        public IActionResult Crear()
        {
            // Carga de datos para los desplegables
            var listaColegios = _context.Colegios.OrderBy(c => c.Nombre).ToList();
            ViewBag.ColegiosData = listaColegios;
            ViewBag.Productos = new SelectList(_context.Productos, "Id", "Nombre");

            // Carga de vendedores (respaldo por si falla el script)
            ViewBag.Vendedores = new SelectList(_context.Usuarios.Where(u => u.Rol == "Vendedor" || u.Rol == "Administrador"), "Nombre", "Nombre");

            return View();
        }

        // POST: Recibe los datos y GUARDA en la base de datos
        // NOTA: Agregamos 'Curso' y 'Division' como parámetros
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Contrato contrato, string ListaAlumnos, string Curso, string Division)
        {
            if (ModelState.IsValid)
            {
                // 1. Configuración Inicial Automática
                contrato.Id = Guid.NewGuid(); // Generamos ID único
                contrato.FechaCreacion = DateTime.Now;
                contrato.Estado = "Presupuesto"; // Nace como Presupuesto

                // Lógica de Código Numérico (Ej: 251201)
                string prefijoFecha = DateTime.Now.ToString("yyMM");
                int baseCodigo = int.Parse(prefijoFecha + "00");
                var ultimo = _context.Contratos
                                     .Where(c => c.Codigo > baseCodigo && c.Codigo < baseCodigo + 99)
                                     .OrderByDescending(c => c.Codigo)
                                     .FirstOrDefault();
                contrato.Codigo = (ultimo != null) ? ultimo.Codigo + 1 : baseCodigo + 1;

                // 2. Procesar Lista de Alumnos (si escribieron algo)
                if (!string.IsNullOrWhiteSpace(ListaAlumnos))
                {
                    var renglones = ListaAlumnos.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                    // Actualizamos la cantidad real según la lista pegada
                    contrato.CantidadEgresados = renglones.Length;

                    int i = 1;
                    foreach (var nombre in renglones)
                    {
                        var alumno = new Estudiante
                        {
                            NombreCompleto = nombre.Trim().ToUpper(),
                            // AQUI USAMOS LOS DATOS DEL FORMULARIO
                            Curso = Curso ?? "6to",
                            Division = Division ?? "Única",
                            ContratoId = contrato.Id,
                            CodigoUnico = $"{contrato.Codigo}{i:D2}" // Genera 25120101, 25120102...
                        };
                        _context.Estudiantes.Add(alumno);
                        i++;
                    }
                }

                // 3. Guardar Contrato + Alumnos
                _context.Contratos.Add(contrato);
                _context.SaveChanges();

                // 4. Redirigir al éxito
                return VolverAlPanelCorrecto();
            }

            // Si algo falló (validación), recargamos las listas y mostramos el error
            var listaColegios = _context.Colegios.OrderBy(c => c.Nombre).ToList();
            ViewBag.ColegiosData = listaColegios;
            ViewBag.Productos = new SelectList(_context.Productos, "Id", "Nombre");
            return View(contrato);
        }


        // ==========================================
        // ACCIONES (Firmar, Cancelar, Editar)
        // ==========================================

        [HttpPost]
        public IActionResult Firmar(Guid id)
        {
            var contrato = _context.Contratos
                                   .Include(c => c.Estudiantes)
                                   .FirstOrDefault(c => c.Id == id);

            if (contrato != null && contrato.Estado == "Presupuesto")
            {
                // 1. CAMBIO DE ESTADO
                contrato.Estado = "Firmado";

                // Si agregaste FechaFirma, descomenta:
                // contrato.FechaFirma = DateTime.Now; 

                // 2. AUDITORÍA
                string usuarioReal = HttpContext.Session.GetString("UsuarioNombre") ?? "admin@incollege.com";
                var log = AuditoriaFactory.Crear(
                    usuarioReal,
                    "FIRMA CONTRATO",
                    $"Se firmó y activó el contrato #{contrato.Codigo} para {contrato.NombreColegio}."
                );
                _context.Auditorias.Add(log);

                // 3. NOTIFICACIONES
                var gestor = new GestorNotificaciones();
                gestor.AgregarObservador(new NotificadorEmail());
                gestor.Avisar(contrato);

                // 4. GUARDAR
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

                // Notificar a Gerencia
                var gestor = new GestorNotificaciones();
                var logs = new List<Auditoria>();
                gestor.AgregarObservador(new NotificadorGerencia(logs));
                gestor.Avisar(contrato);

                _context.Auditorias.AddRange(logs);
                _context.SaveChanges();
            }
            return VolverAlPanelCorrecto();
        }

        // ==========================================
        // OTROS MÉTODOS (Editar, Detalle, Etc)
        // ==========================================

        public IActionResult Detalle(Guid id)
        {
            var contrato = _context.Contratos.FirstOrDefault(c => c.Id == id);
            if (contrato == null) return NotFound();
            return View(contrato);
        }

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

        // GET: /Contratos/AsignarVendedor
        public IActionResult AsignarVendedor()
        {
            var contratos = _context.Contratos
                .Where(c => c.Estado != "Perdido")
                .OrderByDescending(c => c.FechaCreacion)
                .ToList();

            var vendedores = _context.Usuarios
                .Where(u => u.EstaActivo)
                .Select(u => new { NombreCompleto = u.Nombre + " " + u.Apellido })
                .ToList();

            ViewBag.ListaVendedores = new SelectList(vendedores, "NombreCompleto", "NombreCompleto");

            return View(contratos);
        }

        [HttpPost]
        public IActionResult CambiarVendedor(Guid contratoId, string nuevoVendedor)
        {
            var contrato = _context.Contratos.Find(contratoId);
            if (contrato != null)
            {
                contrato.VendedorAsignado = nuevoVendedor;
                _context.SaveChanges();
            }
            return RedirectToAction("AsignarVendedor");
        }

        // MÉTODO PRIVADO PARA REDIRECCIÓN INTELIGENTE
        private IActionResult VolverAlPanelCorrecto()
        {
            string rol = HttpContext.Session.GetString("UsuarioRol");
            if (rol == "Vendedor")
            {
                return RedirectToAction("PanelVendedor", "Home");
            }
            return RedirectToAction("PanelAdmin", "Home");
        }
    }
}