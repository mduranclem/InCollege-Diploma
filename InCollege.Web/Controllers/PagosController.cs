using Microsoft.AspNetCore.Mvc;
using InCollege.Datos;
using InCollege.Dominio.Modelos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using InCollege.Dominio.Servicios;

namespace InCollege.Web.Controllers
{
    public class PagosController : Controller
    {
        private readonly AppDbContext _context;

        public PagosController(AppDbContext context)
        {
            _context = context;
        }

        // FUNCIÓN DE LIMPIEZA DE ACENTOS
        private string RemoverTildes(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return "";
            var normalizedString = texto.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();
            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLower();
        }

        // 1. PANTALLA PRINCIPAL: SELECCIONAR CONTRATO
        public IActionResult Index(string busqueda)
        {
            var todosLosContratos = _context.Contratos
                .Include(c => c.Estudiantes)
                .Where(c => c.Estado != "Presupuesto" && c.Estado != "Perdido")
                .OrderByDescending(c => c.FechaCreacion)
                .ToList();

            var resultados = todosLosContratos;

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                string termino = RemoverTildes(busqueda.ToLower().Trim());
                resultados = todosLosContratos.Where(c =>
                    RemoverTildes(c.NombreColegio.ToLower()).Contains(termino) ||
                    c.Codigo.ToString().Contains(termino) ||
                    c.Estudiantes.Any(e =>
                        RemoverTildes(e.Nombre).Contains(termino) ||
                        RemoverTildes(e.Apellido).Contains(termino) ||
                        RemoverTildes(e.CodigoUnico).Contains(termino)
                    )
                ).ToList();
            }

            ViewData["BusquedaActual"] = busqueda;
            return View(resultados);
        }

        // 2. MATRIZ DE ALUMNOS (ESTADO SITUACIÓN)
        [HttpGet]
        public IActionResult EstadoSituacion(Guid id, string busqueda = "")
        {
            var contrato = _context.Contratos.Find(id);
            if (contrato == null) return NotFound();

            // A. Traemos todos los alumnos
            var todosLosAlumnos = _context.Estudiantes
                .Where(e => e.ContratoId == id)
                .ToList();

            // B. Filtro de búsqueda
            if (!string.IsNullOrEmpty(busqueda))
            {
                string termino = RemoverTildes(busqueda.ToLower());
                todosLosAlumnos = todosLosAlumnos.Where(e =>
                    RemoverTildes(e.Nombre).Contains(termino) ||
                    RemoverTildes(e.Apellido).Contains(termino) ||
                    RemoverTildes(e.CodigoUnico).Contains(termino)
                ).ToList();
            }

            // Ordenamos
            todosLosAlumnos = todosLosAlumnos.OrderBy(e => e.Apellido).ThenBy(e => e.Nombre).ToList();

            // C. Traemos pagos y calculamos saldos
            var pagos = _context.Pagos.Where(p => p.ContratoId == id).ToList();
            var balance = new Dictionary<Guid, decimal>();

            foreach (var est in todosLosAlumnos)
            {
                decimal pagado = pagos.Where(p => p.EstudianteId == est.Id).Sum(p => p.Monto);
                balance.Add(est.Id, pagado);
            }

            // D. REGLA DE NEGOCIO: CALCULAR RECARGO Y CUOTA ACTUAL
            int dia = DateTime.Now.Day;
            decimal recargo = 0;

            // Regla: Después del día 10 hay recargo de $1.000 (ajustable)
            if (dia > 10) recargo = 1000;

            // --- LÓGICA NUEVA: CALCULAR CUOTA DEL MES ACTUAL ---
            // Calculamos la diferencia en meses entre Hoy y la Fecha de Creación del contrato
            int mesesPasados = ((DateTime.Now.Year - contrato.FechaCreacion.Year) * 12) +
                               (DateTime.Now.Month - contrato.FechaCreacion.Month);

            // La cuota "vigente" es la diferencia + 1. 
            // Ejemplo: Si se creó en Enero y estamos en Marzo -> diferencia 2 -> Cuota 3.
            // Aseguramos que sea al menos 1
            int cuotaVigente = Math.Max(1, mesesPasados + 1);

            // Pasamos todos los datos a la vista
            ViewBag.Contrato = contrato;
            ViewBag.Balance = balance;
            ViewBag.Pagos = pagos;
            ViewBag.RecargoHoy = recargo;
            ViewBag.CuotaVigente = cuotaVigente; // <--- ¡AQUÍ ESTÁ LA MAGIA!

            ViewData["BusquedaActual"] = busqueda;

            return View(todosLosAlumnos);
        }

        // 3. REGISTRAR PAGO
        [HttpPost]
        public IActionResult RegistrarPagoIndividual(Guid contratoId, Guid estudianteId, int nroCuota, decimal monto, string medio)
        {
            // 1. VALIDACIÓN
            bool yaPago = _context.Pagos.Any(p =>
                p.ContratoId == contratoId &&
                p.EstudianteId == estudianteId &&
                p.NumeroCuota == nroCuota);

            if (!yaPago && monto > 0)
            {
                // 2. CREAR EL OBJETO PAGO
                var pago = new Pago
                {
                    ContratoId = contratoId,
                    EstudianteId = estudianteId,
                    NumeroCuota = nroCuota,
                    Monto = monto,
                    MedioDePago = medio ?? "Efectivo",
                    FechaPago = DateTime.Now,
                    Concepto = $"Cuota {nroCuota}",
                    Estado = "Acreditado"
                };

                _context.Pagos.Add(pago);

                // 3. AUDITORÍA
                var alumno = _context.Estudiantes.Find(estudianteId);
                string identificadorAlumno = (alumno != null) ? alumno.CodigoUnico : "Desconocido";

                var log = AuditoriaFactory.Crear(
                    HttpContext.Session.GetString("UsuarioNombre") ?? "Sistema",
                    "COBRO REGISTRADO",
                    $"Se cobró ${monto} al alumno {identificadorAlumno} (Cuota {nroCuota}). Medio: {medio ?? "Efectivo"}"
                );

                _context.Auditorias.Add(log);
                _context.SaveChanges();
            }

            return RedirectToAction("EstadoSituacion", new { id = contratoId });
        }
    }
}