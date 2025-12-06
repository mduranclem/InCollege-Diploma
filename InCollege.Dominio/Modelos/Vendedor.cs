using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InCollege.Dominio.Modelos
{
    public class Vendedor
    {
        [Key]
        public int Id { get; set; } // En el diagrama es "vendedorId"

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        [MaxLength(100)]
        public string Zona { get; set; } // Ej: "Zona Norte", "Córdoba Capital"

        [MaxLength(50)]
        public string Telefono { get; set; }

        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        public bool Activo { get; set; } = true; // Para "borrado lógico" (no eliminar de la BD, solo desactivar)

        // --- Relaciones ---
        // Un vendedor gestiona muchos contratos
        public virtual ICollection<Contrato> Contratos { get; set; } = new HashSet<Contrato>();
    }
}
