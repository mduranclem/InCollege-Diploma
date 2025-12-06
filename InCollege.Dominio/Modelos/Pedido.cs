using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InCollege.Dominio.Modelos
{
    public class Pedido
    {
        [Key]
        public int Id { get; set; } // pedidoId

        [Required]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // --- RELACIONES ---

        // 1. Relación con Contrato (Muchos pedidos pertenecen a 1 contrato)
        [Required]
        public int ContratoId { get; set; }
        [ForeignKey("ContratoId")]
        public virtual Contrato Contrato { get; set; }

        // 2. Relación con EstadoPedido (¡Nueva Clase!)
        // Ej: En Preparación, Confirmado, Cancelado
        [Required]
        public int EstadoPedidoId { get; set; }
        [ForeignKey("EstadoPedidoId")]
        public virtual EstadoPedido Estado { get; set; }

        // 3. Relación con Diseño (1 a 1)
        // Un pedido requiere un diseño específico para fabricarse
        // (Aparecerá en ROJO hasta que creemos la clase Diseno)
        public int? DisenoId { get; set; }
        [ForeignKey("DisenoId")]
        public virtual Diseno Diseno { get; set; }
    }
}
