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
            var lista = _context.Contratos.OrderByDescending(c => c.FechaCreacion).ToList();
            return View(lista);
        }

        // ==========================================
        // CREAR NUEVO CONTRATO (GET y POST)
        // ==========================================

        // GET: Muestra el formulario
        public IActionResult Crear()
        {
            CargarListasDesplegables();
            return View();
        }

        // POST: Recibe los datos
        [HttpPost]
        public IActionResult Crear(Contrato contrato, string ListaAlumnos, string Curso, string Division)
        {
            try
            {
                // 1. CONFIGURACIÓN BÁSICA
                contrato.Id = Guid.NewGuid();
                contrato.FechaCreacion = DateTime.Now;
                contrato.Estado = "Presupuesto";
                contrato.VendedorAsignado = string.IsNullOrEmpty(contrato.VendedorAsignado) ? "Sin Asignar" : contrato.VendedorAsignado;

                // --- CAMBIO CLAVE: NO GENERAMOS NÚMERO TODAVÍA ---
                // Usamos 0 para indicar que aún es un presupuesto sin firmar
                contrato.Codigo = 0;
                // -------------------------------------------------

                // Completar Nombres (Colegio y Producto)
                var colegioInfo = _context.Colegios.Find(contrato.ColegioId);
                contrato.NombreColegio = (colegioInfo != null) ? colegioInfo.Nombre : "Desconocido";

                var productoInfo = _context.Productos.Find(contrato.ProductoId);
                contrato.NombreProducto = (productoInfo != null) ? productoInfo.Nombre : "Estándar";

                // 2. PROCESAR ALUMNOS CON CÓDIGO TEMPORAL
                var alumnosParaGuardar = new List<Estudiante>();
                if (!string.IsNullOrWhiteSpace(ListaAlumnos))
                {
                    var renglones = ListaAlumnos.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    contrato.CantidadEgresados = renglones.Length;

                    int i = 1;
                    foreach (var nombre in renglones)
                    {
                        var alumno = new Estudiante
                        {
                            NombreCompleto = nombre.Trim().ToUpper(),
                            Curso = Curso ?? "Único",
                            Division = Division ?? "A",
                            ContratoId = contrato.Id,
                            // Código temporal: "TEMP-01", "TEMP-02". Se arregla al firmar.
                            CodigoUnico = $"TEMP-{i:D2}",
                            TalleAbrigo = "S/T",
                            TalleRemera = "S/T"
                        };
                        alumnosParaGuardar.Add(alumno);
                        i++;
                    }
                }
                else
                {
                    if (contrato.CantidadEgresados <= 0) contrato.CantidadEgresados = 0;
                }

                // 3. GUARDAR
                ModelState.Clear();
                _context.Contratos.Add(contrato);
                if (alumnosParaGuardar.Count > 0) _context.Estudiantes.AddRange(alumnosParaGuardar);

                _context.SaveChanges();
                return RedirectToAction("PanelAdmin", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error: " + ex.Message);
                if (ex.InnerException != null) ModelState.AddModelError("", "Detalle: " + ex.InnerException.Message);
            }

            CargarListasDesplegables();
            return View(contrato);
        }
        // ==========================================
        // ACCIONES (Firmar, Cancelar, Editar)
        // ==========================================

        [HttpPost]
        public IActionResult Firmar(Guid id)
        {
            // Traemos el contrato Y sus estudiantes, porque hay que actualizarles el código
            var contrato = _context.Contratos
                                   .Include(c => c.Estudiantes)
                                   .FirstOrDefault(c => c.Id == id);

            if (contrato != null && contrato.Estado == "Presupuesto")
            {
                // 1. GENERACIÓN DEL CÓDIGO REAL (Lógica movida aquí)
                string prefijoFecha = DateTime.Now.ToString("yyMM"); // Ej: 2505
                int baseCodigo = int.Parse(prefijoFecha + "00");

                // Buscamos el último que SÍ tenga número (Codigo > 0)
                var ultimo = _context.Contratos
                                     .Where(c => c.Codigo > baseCodigo && c.Codigo < baseCodigo + 99)
                                     .OrderByDescending(c => c.Codigo)
                                     .FirstOrDefault();

                int nuevoCodigo = (ultimo != null) ? ultimo.Codigo + 1 : baseCodigo + 1;

                // Asignamos el número real al contrato
                contrato.Codigo = nuevoCodigo;
                contrato.Estado = "Firmado"; // O "En Producción"

                // 2. ACTUALIZAR CÓDIGOS DE LOS ALUMNOS
                // Antes eran "TEMP-01", ahora serán "25050101", etc.
                if (contrato.Estudiantes != null)
                {
                    int i = 1;
                    foreach (var alumno in contrato.Estudiantes)
                    {
                        // Genera el código final: CodigoContrato + NumeroAlumno (ej: 25120101)
                        alumno.CodigoUnico = $"{contrato.Codigo}{i:D2}";
                        i++;
                    }
                }

                // 3. AUDITORÍA Y GUARDADO
                string usuarioReal = HttpContext.Session.GetString("UsuarioNombre") ?? "Sistema";
                var log = AuditoriaFactory.Crear(
                    usuarioReal,
                    "FIRMA CONTRATO",
                    $"Contrato activado. Se asignó el N° {contrato.Codigo}."
                );
                _context.Auditorias.Add(log);

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

                //Notificar Gerencia(Opcional)
                // var gestor = new GestorNotificaciones();
                //gestor.AgregarObservador(new NotificadorGerencia(new List<Auditoria>()));
                //gestor.Avisar(contrato);

                _context.SaveChanges();
            }
            return VolverAlPanelCorrecto();
        }

        // ==========================================
        // VISTAS DE DETALLE Y EDICIÓN
        // ==========================================

        public IActionResult Detalle(Guid id)
        {
            var contrato = _context.Contratos
                                   .Include(c => c.Estudiantes) // Traemos los alumnos para verlos
                                   .FirstOrDefault(c => c.Id == id);

            if (contrato == null) return NotFound();
            return View(contrato);
        }

        public IActionResult Editar(Guid id)
        {
            var contrato = _context.Contratos.Find(id);
            if (contrato == null) return NotFound();

            CargarListasDesplegables(contrato.ColegioId, contrato.ProductoId);
            return View(contrato);
        }

        [HttpPost]
        public IActionResult Editar(Contrato contrato)
        {
            try
            {
                // 1. Limpiar Validaciones que bloquean el guardado (Igual que en Crear)
                ModelState.Remove("Colegio");
                ModelState.Remove("Producto");
                ModelState.Remove("Estudiantes");

                // 2. Buscar el contrato ORIGINAL en la base de datos
                // Usamos el ID que viene del formulario para encontrar el real
                var contratoDB = _context.Contratos.Find(contrato.Id);

                if (contratoDB == null)
                {
                    return NotFound();
                }

                // 3. Actualizar SOLO los campos que se ven en el formulario
                // Esto protege los datos como 'Codigo', 'Estado' y 'FechaCreacion' de ser borrados
                contratoDB.ColegioId = contrato.ColegioId;
                contratoDB.ProductoId = contrato.ProductoId;
                contratoDB.CantidadEgresados = contrato.CantidadEgresados;

                // Precios y Pagos
                contratoDB.PrecioUnitario = contrato.PrecioUnitario;
                contratoDB.PrecioContado = contrato.PrecioContado;
                contratoDB.MontoSeña = contrato.MontoSeña;
                contratoDB.CantidadCuotas = contrato.CantidadCuotas;
                contratoDB.MontoPorCuota = contrato.MontoPorCuota;

                // 4. ACTUALIZAR NOMBRES (CRUCIAL para evitar error de "Cannot insert NULL")
                // Si el usuario cambió el colegio o producto, actualizamos el nombre texto
                var colegioInfo = _context.Colegios.Find(contrato.ColegioId);
                contratoDB.NombreColegio = (colegioInfo != null) ? colegioInfo.Nombre : "Desconocido";

                var productoInfo = _context.Productos.Find(contrato.ProductoId);
                contratoDB.NombreProducto = (productoInfo != null) ? productoInfo.Nombre : "Estándar";

                // 5. Guardar Cambios
                _context.SaveChanges();

                return VolverAlPanelCorrecto();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al actualizar: " + ex.Message);
            }

            // Si falla, recargar listas y volver a mostrar la vista
            CargarListasDesplegables(contrato.ColegioId, contrato.ProductoId);
            return View(contrato);
        }

        [HttpPost]
        public IActionResult Eliminar(Guid id)
        {
            var contrato = _context.Contratos.Include(c => c.Estudiantes).FirstOrDefault(c => c.Id == id);
            if (contrato != null)
            {
                // Primero borramos estudiantes para evitar error de Clave Foránea
                if (contrato.Estudiantes != null && contrato.Estudiantes.Any())
                {
                    _context.Estudiantes.RemoveRange(contrato.Estudiantes);
                }

                _context.Contratos.Remove(contrato);
                _context.SaveChanges();
            }
            return VolverAlPanelCorrecto();
        }

        // ==========================================
        // ASIGNACIÓN DE VENDEDORES
        // ==========================================

        public IActionResult AsignarVendedor()
        {
            var contratos = _context.Contratos
                .Where(c => c.Estado != "Perdido")
                .OrderByDescending(c => c.FechaCreacion)
                .ToList();

            var vendedores = _context.Usuarios
                .Where(u => u.EstaActivo && (u.Rol == "Vendedor" || u.Rol == "Administrador"))
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

        // ==========================================
        // MÉTODOS PRIVADOS (Helpers)
        // ==========================================

        private IActionResult VolverAlPanelCorrecto()
        {
            string rol = HttpContext.Session.GetString("UsuarioRol");
            if (rol == "Vendedor")
            {
                return RedirectToAction("PanelVendedor", "Home");
            }
            return RedirectToAction("PanelAdmin", "Home");
        }

        private void CargarListasDesplegables(object colegioSelect = null, object productoSelect = null)
        {
            // Colegios
            var listaColegios = _context.Colegios.OrderBy(c => c.Nombre).ToList();
            ViewBag.ColegiosData = listaColegios; // Para tu script manual si usas
            ViewBag.Colegios = new SelectList(listaColegios, "Id", "Nombre", colegioSelect); // Para asp-items

            // Productos
            ViewBag.Productos = new SelectList(_context.Productos, "Id", "Nombre", productoSelect);

            // Vendedores
            var vendedores = _context.Usuarios
                .Where(u => u.Rol == "Vendedor" || u.Rol == "Administrador")
                .Select(u => new { Nombre = u.Nombre + " " + u.Apellido })
                .ToList();
            ViewBag.Vendedores = new SelectList(vendedores, "Nombre", "Nombre");
        }
        [HttpPost]
        public IActionResult AsignarTaller(Guid contratoId, int tallerId) // <-- Ya no pedimos 'etapa'
        {
            var contrato = _context.Contratos.Find(contratoId);
            var taller = _context.Talleres.Find(tallerId);

            if (contrato != null && taller != null)
            {
                // 1. Asignamos el Taller
                contrato.TallerAsignadoId = taller.Id;
                contrato.NombreTaller = taller.Nombre;

                // 2. AUTOMATIZACIÓN: La etapa es la especialidad del taller
                // Ej: Si el taller es de "Bordado", la etapa será "En Bordado"
                contrato.EtapaProduccion = taller.Especialidad ?? "En Taller";

                // 3. Estado general
                contrato.Estado = "En Producción";

                // 4. Auditoría automática
                var log = AuditoriaFactory.Crear(
                    HttpContext.Session.GetString("UsuarioNombre") ?? "Sistema",
                    "ENVÍO A TALLER",
                    $"Contrato #{contrato.Codigo} enviado a {taller.Nombre} para {contrato.EtapaProduccion}."
                );
                _context.Auditorias.Add(log);

                _context.SaveChanges();
            }

            return RedirectToAction("PanelAdmin", "Home");
        }
    }
}