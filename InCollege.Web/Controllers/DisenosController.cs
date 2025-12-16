using Microsoft.AspNetCore.Mvc;
using InCollege.Datos.Servicios;
using InCollege.Dominio.Modelos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Text;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace InCollege.Web.Controllers
{
    public class DisenosController : Controller
    {
        private readonly ContratoServicio _servicio;
        private readonly IWebHostEnvironment _entorno;

        public DisenosController(ContratoServicio servicio, IWebHostEnvironment entorno)
        {
            _servicio = servicio;
            _entorno = entorno;
        }

        // =======================================================
        // 1. PANEL PRINCIPAL (MENÚ DE 3 BOTONES)
        // =======================================================
        public IActionResult Panel()
        {
            // Verifica que el usuario tenga rol de diseñador
            var rol = HttpContext.Session.GetString("UsuarioRol");
            // Puedes descomentar esta línea para activar seguridad estricta:
            // if (rol != "Diseñador" && rol != "Disenador") return RedirectToAction("Index", "Home");

            return View(); // Busca Views/Disenos/Panel.cshtml
        }

        // =======================================================
        // 2. LISTADO DE CONTRATOS (TABLA DE TRABAJO)
        // =======================================================
        public IActionResult Index(string busqueda)
        {
            List<Contrato> lista;

            if (string.IsNullOrWhiteSpace(busqueda))
            {
                // MODO POR DEFECTO: Solo muestra lo que tienes pendiente de trabajo
                lista = _servicio.ObtenerPendientesDeDiseno();
            }
            else if (busqueda == " ") // Truco: Si viene un espacio (desde el botón "Ver Contratos") mostramos todo
            {
                lista = _servicio.ObtenerTodos();
            }
            else
            {
                // MODO BÚSQUEDA: Busca en TODO por texto
                var todos = _servicio.ObtenerTodos();
                string termino = RemoveDiacritics(busqueda.ToLower());

                lista = todos.Where(c =>
                    c.Codigo.ToString().Contains(termino) ||
                    (c.NombreColegio != null && RemoveDiacritics(c.NombreColegio.ToLower()).Contains(termino)) ||
                    (c.NombreProducto != null && RemoveDiacritics(c.NombreProducto.ToLower()).Contains(termino))
                ).ToList();

                ViewData["BusquedaActual"] = busqueda;
            }

            return View(lista); // Busca Views/Disenos/Index.cshtml
        }

        // =======================================================
        // 3. LISTADO DE ESCUELAS (SOLO LECTURA)
        // =======================================================
        public IActionResult Escuelas()
        {
            // Traemos los nombres de colegios únicos para la lista
            var lista = _servicio.ObtenerTodos()
                                 .Select(c => c.NombreColegio)
                                 .Distinct()
                                 .OrderBy(n => n)
                                 .ToList();
            return View(lista); // Busca Views/Disenos/Escuelas.cshtml
        }

        // =======================================================
        // 4. CARGA DE IMAGEN (GET Y POST)
        // =======================================================
        [HttpGet]
        public IActionResult Cargar(int id)
        {
            var contrato = _servicio.ObtenerPorId(id);
            if (contrato == null) return NotFound();
            return View(contrato); // Busca Views/Disenos/Cargar.cshtml
        }

        [HttpPost]
        public async Task<IActionResult> Cargar(int codigoContrato, IFormFile archivo)
        {
            if (archivo != null && archivo.Length > 0)
            {
                // 1. Definir ruta (wwwroot/disenos_confirmados)
                string carpeta = Path.Combine(_entorno.WebRootPath, "disenos_confirmados");
                if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);

                // 2. Crear nombre único
                string nombreArchivo = $"{codigoContrato}_{Guid.NewGuid()}.jpg";
                string rutaCompleta = Path.Combine(carpeta, nombreArchivo);

                // 3. Guardar archivo en disco
                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }

                // 4. Guardar en Base de Datos usando el Servicio
                _servicio.RegistrarDiseno(codigoContrato, $"/disenos_confirmados/{nombreArchivo}");

                return RedirectToAction("Index");
            }

            // Si falló, volver a mostrar la vista
            return View(_servicio.ObtenerPorId(codigoContrato));
        }

        // =======================================================
        // UTILIDADES (Ayuda interna)
        // =======================================================
        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}