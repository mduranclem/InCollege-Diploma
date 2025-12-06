using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InCollege.Dominio.Modelos
{
    /// <summary>
    /// Catálogo de estados posibles para un Contrato.
    /// Ej: 1="Borrador", 2="Vigente", 3="Finalizado".
    /// </summary>
    public class EstadoContrato
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; }

        // Nos permite saber qué contratos están en este estado
        public virtual ICollection<Contrato> Contratos { get; set; }
    }
}
