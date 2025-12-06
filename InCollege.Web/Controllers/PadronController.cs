using Microsoft.AspNetCore.Mvc;
using InCollege.Datos;
using InCollege.Dominio.Modelos;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.AccessControl;

namespace InCollege.Web.Controllers
{
    public class PadronController : Controller
    {
        private readonly AppDbContext _context;

        public PadronController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Muestra la lista de alumnos de un contrato
        public IActionResult Index(Guid contratoId)
        {
            var contrato = _context.Contratos.Find(contratoId);
            if (contrato == null) return NotFound();

            var alumnos = _context.Estudiantes
                .Where(e => e.ContratoId == contratoId)
                // CAMBIO AQUÍ: Ordenamos por CÓDIGO para que salgan 01, 02, 03...
                .OrderBy(e => e.CodigoUnico)
                .ToList();

            ViewBag.Contrato = contrato;
            return View(alumnos);
        }

        // POST: Carga Masiva (Pegar lista)
        [HttpPost]
        public IActionResult CargaMasiva(Guid contratoId, string listaNombres)
        {
            if (!string.IsNullOrWhiteSpace(listaNombres))
            {
                // Separamos por renglón (Enter)
                var renglones = listaNombres.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var linea in renglones)
                {
                    // LÓGICA DE SEPARACIÓN: APELLIDO NOMBRE
                    // Ejemplo entrada: "PEREZ JUAN MANUEL"
                    var partes = linea.Trim().Split(' ');
                    string apellido = "";
                    string nombre = "";

                    if (partes.Length > 1)
                    {
                        // Tomamos la primera palabra como Apellido
                        apellido = partes[0];
                        // El resto lo juntamos como Nombre
                        nombre = string.Join(" ", partes.Skip(1));
                    }
                    else
                    {
                        // Si escribió solo una palabra, la ponemos como Apellido
                        apellido = linea.Trim();
                        nombre = "-";
                    }

                    var alumno = new Estudiante
                    {
                        ContratoId = contratoId,
                        Apellido = apellido.ToUpper(),
                        Nombre = nombre.ToUpper(),
                        EstaAlDia = true, // Nace al día
                        TotalPagado = 0
                    };

                    _context.Estudiantes.Add(alumno);
                }
                _context.SaveChanges();
            }

            // Recargamos la página para ver la lista nueva
            return RedirectToAction("Index", new { contratoId = contratoId });
        }
        [HttpPost]
        public IActionResult ActualizarTalles(Guid id, string talleAbrigo, string talleRemera)
        {
            // 1. Buscamos al alumno REAL en la base de datos
            var alumno = _context.Estudiantes.Find(id);

            if (alumno != null)
            {
                // 2. Modificamos SOLO lo que cambió
                // (Usamos el operador ?? para no borrar datos si vienen vacíos por error)
                alumno.TalleAbrigo = talleAbrigo ?? "-";
                alumno.TalleRemera = talleRemera ?? "-";

                // 3. Guardamos (EF Core detecta que solo cambiaron esos campos)
                _context.SaveChanges();

                return RedirectToAction("Index", new { contratoId = alumno.ContratoId });
            }

            return NotFound();
        }
    }
} 