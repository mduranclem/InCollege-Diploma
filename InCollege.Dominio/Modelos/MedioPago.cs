using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InCollege.Dominio.Modelos
{
    public class MedioPago
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } // Ej: "Efectivo", "Transferencia", "MercadoPago"

        // Relación inversa
        public virtual ICollection<Pago> Pagos { get; set; }
    }
}
