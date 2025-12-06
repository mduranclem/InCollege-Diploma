using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InCollege.Dominio.Modelos
{
    public class Taller
    {
        [Key]
        public int Id { get; set; } // tallerId

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } // Ej: "Confecciones Juana"

        [MaxLength(100)]
        public string Contacto { get; set; } // Nombre de la persona de contacto

        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        public bool Activo { get; set; } = true; // Para no borrarlo si deja de trabajar con nosotros

        // --- RELACIONES ---

        // 1. Relación con TipoTrabajo (Especialidad)
        // Define qué sabe hacer este taller (Costura, Bordado, etc.)
        [Required]
        public int TipoTrabajoId { get; set; }
        [ForeignKey("TipoTrabajoId")]
        public virtual TipoTrabajo TipoTrabajo { get; set; }

        // 2. Relación con Asignaciones (Trabajos recibidos)
        // (Aparecerá en ROJO hasta que creemos AsignacionTaller)
        public virtual ICollection<AsignacionTaller> Asignaciones { get; set; } = new HashSet<AsignacionTaller>();
    }
}