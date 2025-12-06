using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InCollege.Dominio.Modelos
{
    public class AsignacionTaller
    {
        [Key]
        public int Id { get; set; } // asignacionId

        [Required]
        public int Prioridad { get; set; } // Ej: 1 (Alta), 2 (Media), 3 (Baja)

        [Required]
        public DateTime FechaAsignacion { get; set; } = DateTime.Now;

        public DateTime? FechaInicio { get; set; } // Cuándo el taller realmente empezó

        public DateTime? FechaFin { get; set; } // Cuándo el taller entregó el trabajo

        [MaxLength(500)]
        public string Observaciones { get; set; } // Ej: "Entregar planchado y en bolsas"

        // --- RELACIONES ---

        // 1. Relación con la Etapa de Producción (El trabajo a realizar)
        [Required]
        public int EtapaId { get; set; } // Antes etapaProduccionId
        [ForeignKey("EtapaId")]
        public virtual EtapaProduccion Etapa { get; set; }

        // 2. Relación con el Taller (El proveedor que lo hace)
        [Required]
        public int TallerId { get; set; }
        [ForeignKey("TallerId")]
        public virtual Taller Taller { get; set; }

        // 3. Relación con EstadoAsignacion (¡Nueva Clase!)
        // Ej: Asignada, Aceptada, Rechazada, Entregada
        [Required]
        public int EstadoId { get; set; } // Antes estadoAsignacionId
        [ForeignKey("EstadoId")]
        public virtual EstadoAsignacion Estado { get; set; }
    }
}