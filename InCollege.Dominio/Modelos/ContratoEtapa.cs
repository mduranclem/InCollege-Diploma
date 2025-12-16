using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InCollege.Dominio.Modelos
{
    public class ContratoEtapa
    {
        [Key]
        public int Id { get; set; }

        // Relaciones con las entidades principales
        public Guid ContratoId { get; set; }
        [ForeignKey("ContratoId")]
        public virtual Contrato Contrato { get; set; }

        public int EtapaProduccionId { get; set; }
        [ForeignKey("EtapaProduccionId")]
        public virtual EtapaProduccion EtapaProduccion { get; set; }

        public Guid? TallerId { get; set; } // Nullable, puede que no esté asignado
        [ForeignKey("TallerId")]
        public virtual Taller Taller { get; set; }

        [Required]
        [MaxLength(20)]
        public string Estado { get; set; } = "PENDIENTE"; // PENDIENTE, EN_CURSO, TERMINADA

        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }
}