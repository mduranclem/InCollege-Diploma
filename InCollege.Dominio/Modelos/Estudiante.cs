using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InCollege.Dominio.Modelos
{
    public class Estudiante
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Nombre { get; set; }
        public string Apellido { get; set; }

        // Propiedad calculada (no se guarda en DB, solo sirve para mostrar fácil)
        [NotMapped]
        public string NombreCompleto => $"{Apellido}, {Nombre}";

        public string? Dni { get; set; }

        public bool EstaAlDia { get; set; } = true;

        // AJUSTE: Definimos la precisión del dinero para que SQL no tire warnings
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPagado { get; set; } = 0;

        public string CodigoUnico { get; set; }
        public string? TalleAbrigo { get; set; } // Para Buzo o Campera
        public string? TalleRemera { get; set; } // Para Remera o Chomba

        public Guid ContratoId { get; set; }
        [ForeignKey("ContratoId")]
        public virtual Contrato Contrato { get; set; }
    }
}