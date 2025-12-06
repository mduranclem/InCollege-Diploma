using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InCollege.Dominio.Modelos
{
    public class EstadoPedido
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } // Ej: "En Preparación", "Confirmado"

        // Relación inversa
        public virtual ICollection<Pedido> Pedidos { get; set; }
    }
}
