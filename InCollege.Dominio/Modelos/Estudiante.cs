using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Necesario para [NotMapped]
using System.Linq;

namespace InCollege.Dominio.Modelos
{
    public class Estudiante
    {
        public Guid Id { get; set; }

        [Required]
        public string NombreCompleto { get; set; } // Ahora se puede escribir

        // --- DATOS DEL CONTRATO (Curso y División) ---
        public string Curso { get; set; } = "6to";
        public string Division { get; set; } = "Única";
        public string CodigoUnico { get; set; }

        // --- DATOS DEL PADRÓN (Pagos y Talles) ---
        public decimal TotalPagado { get; set; } = 0;
        public bool EstaAlDia { get; set; } = true;
        public string TalleRemera { get; set; }
        public string TalleAbrigo { get; set; }

        // --- TRUCO DE COMPATIBILIDAD ---
        // Agregamos un "set" vacío para que el controlador no de error al intentar escribir.
        [NotMapped]
        public string Nombre
        {
            get => NombreCompleto?.Split(' ').FirstOrDefault() ?? "";
            set { /* Ignoramos la escritura, usamos NombreCompleto */ }
        }

        [NotMapped]
        public string Apellido
        {
            get => NombreCompleto?.Split(' ').Skip(1).FirstOrDefault() ?? "";
            set { /* Ignoramos la escritura, usamos NombreCompleto */ }
        }

        // Relación
        public Guid ContratoId { get; set; }
        public Contrato Contrato { get; set; }
    }
}