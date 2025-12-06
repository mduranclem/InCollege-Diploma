using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InCollege.Dominio.Modelos
{
    public class EstadoAsignacion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } // Ej: "Asignada", "En Proceso", "Entregada"

        // Relación inversa (Aparecerá en ROJO hasta crear AsignacionTaller)
        public virtual ICollection<AsignacionTaller> Asignaciones { get; set; }
    }
}