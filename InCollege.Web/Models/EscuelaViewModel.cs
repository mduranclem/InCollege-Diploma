using System;

namespace InCollege.Web.Models
{
    public class EscuelaViewModel
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Ciudad { get; set; }
        public string Direccion { get; set; }

        // Datos de Contacto
        public string ContactoNombre { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }

        // Datos calculados (vienen del Contrato)
        public string EstadoActual { get; set; } // "Nuevo", "Firmado", "Perdido"
        public string Vendedor { get; set; }
        public string UltimoProducto { get; set; } // Qué les ofrecimos
    }
}