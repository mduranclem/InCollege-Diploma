using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InCollege.Dominio.Modelos
{
    public class Pago
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime FechaPago { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

        [MaxLength(100)]
        public string Concepto { get; set; } // Ej: "Cuota 1"

        public int NumeroCuota { get; set; } // Ej: 1, 2, 3...

        public string MedioDePago { get; set; }
        public string Estado { get; set; } = "Acreditado";

        // RELACIONES
        public Guid ContratoId { get; set; }
        [ForeignKey("ContratoId")]
        public virtual Contrato Contrato { get; set; }

        public Guid? EstudianteId { get; set; } // Puede ser nulo si es seña general
        [ForeignKey("EstudianteId")]
        public virtual Estudiante Estudiante { get; set; }
    }
}