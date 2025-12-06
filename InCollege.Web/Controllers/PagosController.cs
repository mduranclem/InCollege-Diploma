using Microsoft.AspNetCore.Mvc;
using InCollege.Datos;
using InCollege.Dominio.Modelos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization; // NECESARIO PARA LOS ACENTOS
using System.Text;          // NECESARIO PARA EL TEXTO

namespace InCollege.Web.Controllers
{
    public class PagosController : Controller
    {
        private readonly AppDbContext _context;

        public PagosController(AppDbContext context)
        {
            _context = context;
        }

        // --- FUNCIÓN MÁGICA PARA QUITAR TILDES ---
        // Transforma "Mártínez" en "martinez"
        private string LimpiarTexto(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return "";

            // 1. Normalizar (separar letras de tildes)
            var normalizedString = texto.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            // 2. Eliminar los tildes
            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            // 3. Volver a unir y pasar a minúsculas
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLower();
        }

        // 1. PANTALLA PRINCIPAL CON BUSCADOR BLINDADO
        public IActionResult Index(string busqueda)
        {
            // A. Traemos TODOS los contratos a memoria primero
            // (Esto es necesario para poder usar nuestra función LimpiarTexto en C#)
            var todosLosContratos = _context.Contratos
                .Include(c => c.Estudiantes) // Traemos los alumnos
                .Where(c => c.Estado != "Presupuesto" && c.Estado != "Perdido")
                .OrderByDescending(c => c.FechaCreacion)
                .ToList();

            var resultados = todosLosContratos;

            // B. Filtramos en memoria usando la limpieza de acentos
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                string termino = LimpiarTexto(busqueda); // Lo que escribió el usuario (limpio)

                resultados = todosLosContratos.Where(c =>
                    // 1. Buscar por Colegio
                    LimpiarTexto(c.NombreColegio).Contains(termino) ||

                    // 2. Buscar por Código
                    c.Codigo.ToString().Contains(termino) ||

                    // 3. Buscar por Alumno (Cualquier parte del nombre)
                    c.Estudiantes.Any(e =>
                        LimpiarTexto(e.Nombre).Contains(termino) ||
                        LimpiarTexto(e.Apellido).Contains(termino) ||
                        LimpiarTexto(e.CodigoUnico).Contains(termino) ||
                        LimpiarTexto(e.Nombre + " " + e.Apellido).Contains(termino) ||
                        LimpiarTexto(e.Apellido + " " + e.Nombre).Contains(termino)
                    )
                ).ToList();
            }

            ViewData["BusquedaActual"] = busqueda;
            return View(resultados);
        }

        // 2. MATRIZ DE ALUMNOS (ESTADO SITUACIÓN)
        [HttpGet]
        public IActionResult EstadoSituacion(Guid id)
        {
            var contrato = _context.Contratos.Find(id);
            if (contrato == null) return NotFound();

            // 1. Traemos los alumnos ordenados
            var estudiantes = _context.Estudiantes
                .Where(e => e.ContratoId == id)
                .OrderBy(e => e.CodigoUnico)
                .ToList();

            // 2. Traemos TODOS los pagos históricos de este curso
            // Esto es lo que nos permite saber qué "casilleros" pintar de verde
            var pagosHechos = _context.Pagos.Where(p => p.ContratoId == id).ToList();

            // 3. LÓGICA DE RECARGOS (Para mostrar el precio actualizado hoy)
            int diaHoy = DateTime.Now.Day;
            decimal recargo = 0;

            if (diaHoy >= 10 && diaHoy <= 19) recargo = 1000;       // Del 10 al 19
            else if (diaHoy >= 20 && diaHoy <= 31) recargo = 2000;  // Del 20 a fin de mes

            // (Nota: La lógica de "mes vencido" requiere comparar meses, por ahora usamos la del día corriente)

            ViewBag.Contrato = contrato;
            ViewBag.Pagos = pagosHechos; // Lista de pagos para pintar la matriz
            ViewBag.RecargoHoy = recargo;

            return View(estudiantes);
        }

        // 3. REGISTRAR PAGO
        [HttpPost]
        [HttpPost]
        public IActionResult RegistrarPagoIndividual(Guid contratoId, Guid estudianteId, int nroCuota, decimal monto, string medio)
        {
            // Validación para no cobrar doble
            bool yaPago = _context.Pagos.Any(p => p.ContratoId == contratoId && p.EstudianteId == estudianteId && p.NumeroCuota == nroCuota);

            if (!yaPago && monto > 0)
            {
                var pago = new Pago
                {
                    ContratoId = contratoId,
                    EstudianteId = estudianteId,
                    NumeroCuota = nroCuota, // Guardamos qué cuota es
                    Monto = monto,
                    MedioDePago = medio ?? "Chequera",
                    FechaPago = DateTime.Now,
                    Concepto = $"Cuota {nroCuota}",
                    Estado = "Acreditado"
                };

                _context.Pagos.Add(pago);
                _context.SaveChanges();
            }

            return RedirectToAction("EstadoSituacion", new { id = contratoId }); // Ojo: nombre de la acción corregido a EstadoSituacion o Matriz según lo que uses
        }
    }
}