using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InCollege.Dominio.Modelos
{
    public class EtapaProduccion
    {
        [Key]
        public int Id { get; set; } // etapaProduccionId

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } // Ej: "Corte", "Bordado", "Limpieza"

        [Required]
        public int Orden { get; set; } // Para saber qué va primero (1, 2, 3...)

        [Column(TypeName = "decimal(18,2)")]
        public decimal Costo { get; set; } // Costo estimado de esta etapa

        public DateTime? FechaInicio { get; set; } // Nulable (puede no haber empezado)

        public DateTime? FechaFin { get; set; } // Nulable (puede no haber terminado)

        // --- RELACIONES ---

        // 1. Relación con Diseño (A qué diseño pertenece esta etapa)
        [Required]
        public int DisenoId { get; set; }
        [ForeignKey("DisenoId")]
        public virtual Diseno Diseno { get; set; }

        // 2. Relación con EstadoEtapa (¡Nueva Clase que creamos antes!)
        [Required]
        public int EstadoEtapaId { get; set; }
        [ForeignKey("EstadoEtapaId")]
        public virtual EstadoEtapa Estado { get; set; }

        // 3. Relación con Asignaciones a Taller (Aparecerá en ROJO)
        // Una etapa puede tener intentos de asignación (ej. se la mandé a Juan, la rechazó, se la mandé a Pedro)
        public virtual ICollection<AsignacionTaller> Asignaciones { get; set; } = new HashSet<AsignacionTaller>();

        // 4. Relación con Movimientos de Stock (Aparecerá en ROJO)
        // En esta etapa se gastan telas o insumos
        public virtual ICollection<MovimientoStock> MovimientosStock { get; set; } = new HashSet<MovimientoStock>();
    }
}