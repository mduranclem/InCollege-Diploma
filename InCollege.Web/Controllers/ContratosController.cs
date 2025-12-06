using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using InCollege.Datos;
using InCollege.Dominio.Modelos;
using System.Linq;
using System;

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

        // GET: /Contratos/Crear
        public IActionResult Crear()
        {
            ViewBag.Colegios = new SelectList(_context.Colegios, "Id", "Nombre");
            ViewBag.Productos = new SelectList(_context.Productos, "Id", "Nombre");
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
                // --- CORRECCIÓN: LÓGICA DE FECHA (YYMM + SECUENCIA) ---
                // 1. Obtenemos prefijo de fecha actual (Ej: "2512" para Dic 2025)
                string prefijoFecha = DateTime.Now.ToString("yyMM");

                // 2. Base numérica: 251200
                int baseCodigo = int.Parse(prefijoFecha + "00");

                // 3. Buscamos el último de ESTE MES
                var ultimoContratoDelMes = _context.Contratos
                    .Where(c => c.Codigo > baseCodigo && c.Codigo < baseCodigo + 99)
                    .OrderByDescending(c => c.Codigo)
                    .FirstOrDefault();

                // 4. Asignamos: Si no hay, es el 01 (251201). Si hay, sumamos 1.
                if (ultimoContratoDelMes == null)
                {
                    contrato.Codigo = baseCodigo + 1; // Resultado: 251201
                }
                else
                {
                    contrato.Codigo = ultimoContratoDelMes.Codigo + 1; // Resultado: 251202...
                }

                // ... Guardado de datos normales ...
                contrato.NombreColegio = colegio.Nombre;
                contrato.NombreProducto = producto.Nombre;
                if (string.IsNullOrEmpty(contrato.VendedorAsignado)) contrato.VendedorAsignado = "Admin";

                _context.Contratos.Add(contrato);
                _context.SaveChanges();

                // --- PROCESAR ALUMNOS CON EL CÓDIGO LARGO ---
                if (!string.IsNullOrWhiteSpace(ListaAlumnos))
                {
                    var renglones = ListaAlumnos.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
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

                            // YA NO EXISTE 'Talle'. AHORA ES:
                            TalleAbrigo = "-",
                            TalleRemera = "-",

                            CodigoUnico = $"{contrato.Codigo}{contadorAlumno.ToString("D2")}",
                            EstaAlDia = true,
                            TotalPagado = 0
                        };

                        _context.Estudiantes.Add(estudiante);
                        contadorAlumno++;
                    }
                   _context.SaveChanges();
                }
                return RedirectToAction("PanelAdmin", "Home");
            }
            // ... recarga de vista si falla ...
            ViewBag.Colegios = new SelectList(_context.Colegios, "Id", "Nombre");
            ViewBag.Productos = new SelectList(_context.Productos, "Id", "Nombre");
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
            var contrato = _context.Contratos.Find(id);
            if (contrato != null) { contrato.Estado = "Firmado"; _context.SaveChanges(); }
            return RedirectToAction("PanelAdmin", "Home");
        }

        [HttpPost]
        public IActionResult Cancelar(Guid id)
        {
            var contrato = _context.Contratos.Find(id);
            if (contrato != null) { contrato.Estado = "Perdido"; _context.SaveChanges(); }
            return RedirectToAction("PanelAdmin", "Home");
        }
    }
}