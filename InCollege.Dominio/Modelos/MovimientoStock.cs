using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InCollege.Dominio.Modelos
{
    public class MovimientoStock
    {
        [Key]
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(20)]
        public string Tipo { get; set; } // Ej: "AVANCE", "PROBLEMA", "FINALIZADO"

        [Required]
        public int Cantidad { get; set; }

        [MaxLength(200)]
        public string Detalle { get; set; }

        // --- SOLO DEJAMOS LA RELACIÓN CON CONTRATO ---
        public Guid? ContratoId { get; set; }

        [ForeignKey("ContratoId")]
        public virtual Contrato Contrato { get; set; }

        // AQUÍ BORRAMOS LO DE 'EtapaId' Y 'EtapaProduccion' QUE DABA ERROR
    }
}