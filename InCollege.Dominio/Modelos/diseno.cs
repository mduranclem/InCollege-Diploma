using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InCollege.Dominio.Modelos
{
    public class Diseno
    {
        [Key]
        public int Id { get; set; } // disenoId

        [MaxLength(200)]
        public string Descripcion { get; set; }

        // --- IMÁGENES (Guardadas como arreglo de bytes) ---
        // Entity Framework convertirá esto a varbinary(max) en SQL Server
        public byte[] ImagenFrente { get; set; }

        public byte[] ImagenEspalda { get; set; }

        // --- Aprobación ---
        public bool Aprobado { get; set; } = false;

        public DateTime? FechaAprobacion { get; set; } // Nulable (?) porque al crearla no está aprobada

        [MaxLength(50)]
        public string TipoPrenda { get; set; } // Ej: "Campera", "Chomba"

        // --- RELACIONES ---

        // 1. Relación inversa con Pedido (1 a 1)
        public virtual Pedido Pedido { get; set; }

        // 2. Relación con Etapas de Producción
        // Un diseño pasa por varias etapas (Corte, Bordado, etc.)
        // (Aparecerá en ROJO hasta que creemos EtapaProduccion)
        public virtual ICollection<EtapaProduccion> Etapas { get; set; } = new HashSet<EtapaProduccion>();
    }
}
