using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InCollege.Dominio.Modelos
{
    public class MovimientoStock
    {
        [Key]
        public int Id { get; set; } // movimientoId

        [Required]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(20)]
        public string Tipo { get; set; } // Ej: "ENTRADA" o "SALIDA"

        [Required]
        public int Cantidad { get; set; } // Cantidad de insumo/prenda

        [MaxLength(200)]
        public string Detalle { get; set; } // Ej: "Metros de tela Jersey", "Botones"

        // --- RELACIONES ---

        // 1. Relación con EtapaProduccion
        // Nos permite saber en qué etapa se gastó este material
        [Required]
        public int EtapaId { get; set; }
        [ForeignKey("EtapaId")]
        public virtual EtapaProduccion Etapa { get; set; }
    }
}