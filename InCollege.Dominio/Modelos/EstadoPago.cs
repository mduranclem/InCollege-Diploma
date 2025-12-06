using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InCollege.Dominio.Modelos
{
    public class EstadoPago
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } // Ej: "Pendiente", "Acreditado", "Rechazado"

        public virtual ICollection<Pago> Pagos { get; set; }
    }
}