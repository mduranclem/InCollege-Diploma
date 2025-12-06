using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InCollege.Dominio.Modelos
{
    public class TipoTrabajo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } // Ej: "Bordado", "Costura"

        [MaxLength(200)]
        public string Descripcion { get; set; }

        // Relación inversa (Aparecerá en ROJO hasta crear Taller)
        public virtual ICollection<Taller> Talleres { get; set; }
    }
}
