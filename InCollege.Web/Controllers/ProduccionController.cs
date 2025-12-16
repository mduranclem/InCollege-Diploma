using Microsoft.AspNetCore.Mvc;
using InCollege.Datos;
using InCollege.Dominio.Modelos;
using InCollege.Dominio.Servicios;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace InCollege.Web.Controllers
{
    public class ProduccionController : Controller
    {
        private readonly AppDbContext _context;

        public ProduccionController(AppDbContext context)
        {
            _context = context;
        }

        // 1. LISTA DE CONTRATOS EN PRODUCCIÓN
        public IActionResult Index()
        {
            var enProduccion = _context.Contratos
                .Where(c => c.Estado == "En Producción" || c.Estado == "Firmado") // Incluimos firmados para iniciarlos
                .OrderByDescending(c => c.FechaCreacion)
                .ToList();

            return View(enProduccion);
        }

        // 2. PANTALLA DE SEGUIMIENTO (BITÁCORA)
        public IActionResult Seguimiento(Guid id)
        {
            var contrato = _context.Contratos
                .Include(c => c.Colegio) // Para mostrar nombre del colegio
                .FirstOrDefault(c => c.Id == id);

            if (contrato == null) return NotFound();

            // --- CORRECCIÓN AQUÍ ---
            // Usamos Set<MovimientoStock>() para ir directo a la tabla correcta
            // en lugar de intentar filtrarla desde Productos.
            var historial = _context.Set<MovimientoStock>()
                                    .Where(m => m.ContratoId == id)
                                    .OrderByDescending(m => m.Fecha)
                                    .ToList();

            ViewBag.Historial = historial;

            return View(contrato);
        }

        // 3. REGISTRAR UN AVANCE (POST)
        [HttpPost]
        public IActionResult RegistrarAvance(Guid contratoId, string detalle, string tipo, int cantidad)
        {
            var contrato = _context.Contratos.Find(contratoId);
            if (contrato != null)
            {
                var movimiento = new MovimientoStock
                {
                    ContratoId = contratoId,
                    Fecha = DateTime.Now,
                    Tipo = tipo, // "AVANCE", "PROBLEMA", "TERMINADO"
                    Detalle = detalle,
                    Cantidad = cantidad,
                    // Dejamos EtapaId null por ahora o lo vinculamos si quieres complejidad
                };

                _context.Add(movimiento);

                // Si marcan "TERMINADO", cambiamos el estado del contrato
                if (tipo == "FINALIZADO")
                {
                    contrato.Estado = "Entregado"; // O "Listo para Entrega"

                    // Auditoría automática
                    _context.Auditorias.Add(AuditoriaFactory.Crear(
                        HttpContext.Session.GetString("UsuarioNombre") ?? "Sistema",
                        "FIN PRODUCCIÓN",
                        $"El contrato #{contrato.Codigo} fue finalizado."
                    ));
                }

                _context.SaveChanges();
            }

            return RedirectToAction("Seguimiento", new { id = contratoId });
        }
    }
}