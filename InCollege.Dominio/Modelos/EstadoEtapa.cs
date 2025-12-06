using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InCollege.Dominio.Modelos
{
    public class EstadoEtapa
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } // Ej: "Pendiente", "Finalizada"

        // Relación inversa (Aparecerá en ROJO hasta crear EtapaProduccion)
        public virtual ICollection<EtapaProduccion> Etapas { get; set; }
    }
}