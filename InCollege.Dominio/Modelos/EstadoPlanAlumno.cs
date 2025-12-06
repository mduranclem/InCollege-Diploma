using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InCollege.Dominio.Modelos
{
    public class EstadoPlanAlumno
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } // Ej: "Al Día", "Atrasado", "Plan Caído"

        // Relación inversa
        public virtual ICollection<Alumno> Alumnos { get; set; }
    }
}
