using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InCollege.Dominio.Modelos
{
    public class Alumno
    {
        [Key]
        public int Id { get; set; } // alumnoId

        // --- Datos Personales ---
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        [Required]
        [MaxLength(100)]
        public string Apellido { get; set; }

        [MaxLength(50)]
        public string Telefono { get; set; }

        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        // --- Datos del Producto ---
        [MaxLength(10)]
        public string TalleCampera { get; set; }

        [MaxLength(10)]
        public string TalleChomba { get; set; }

        [MaxLength(500)]
        public string Observacion { get; set; } // Antes "Apodo", ahora coincide con el diagrama

        // --- RELACIONES ---

        // 1. Relación con Contrato (Muchos alumnos pertenecen a 1 contrato)
        [Required]
        public int ContratoId { get; set; }
        [ForeignKey("ContratoId")]
        public virtual Contrato Contrato { get; set; }

        // 2. Relación con Estado (¡Nueva Clase!)
        // Conecta con la tabla EstadoPlanAlumno (Normal, Atrasado, Plan Caído)
        [Required]
        public int EstadoPlanId { get; set; }
        [ForeignKey("EstadoPlanId")]
        public virtual EstadoPlanAlumno EstadoPlan { get; set; }

        // 3. Lista de Pagos
        // Un alumno tiene muchos pagos individuales
        public virtual ICollection<Pago> Pagos { get; set; } = new HashSet<Pago>();
    }
}