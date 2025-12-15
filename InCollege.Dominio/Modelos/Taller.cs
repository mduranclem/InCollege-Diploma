using System;
using System.ComponentModel.DataAnnotations;

namespace InCollege.Dominio.Modelos
{
    public class Taller
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; } // Ej: "Confecciones Daniel"

        public string Especialidad { get; set; } // Ej: "Costura", "Estampado", "Bordado"

        public string Responsable { get; set; } // Nombre del dueño

        public string Telefono { get; set; }

        public string Direccion { get; set; }

        public bool Activo { get; set; } = true;
    }
}