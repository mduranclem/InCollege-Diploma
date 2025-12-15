using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InCollege.Dominio.Modelos
{
    // Esta es la clase PADRE de PrendaIndividual y KitComposite
    public abstract class Producto
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        // Propiedades comunes que usen tus vistas
        public decimal PrecioBase { get; set; }
    }
}