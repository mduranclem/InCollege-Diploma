using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;       // Necesario para [Key]
using System.ComponentModel.DataAnnotations.Schema; // Necesario para [Column]

namespace InCollege.Dominio.Modelos
{
    public class Contrato
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // Código inteligente (Ej: 251201)
        public int Codigo { get; set; }
        public DateTime? FechaFirma { get; set; }

        // DATOS DEL CLIENTE
        public Guid ColegioId { get; set; }
        public string NombreColegio { get; set; }

        // DATOS DEL PRODUCTO
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; }

        // DATOS GENERALES
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public int CantidadEgresados { get; set; }

        // PRECIOS (Configurados para moneda)
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; } // Precio de Lista

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioContado { get; set; } // Precio Oferta

        public string OrigenContacto { get; set; }

        // PLAN DE PAGOS
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoSeña { get; set; } // Plata para congelar

        public int CantidadCuotas { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoPorCuota { get; set; }

        // ESTADO Y GESTIÓN
        public string Estado { get; set; } = "Presupuesto";
        public string VendedorAsignado { get; set; }

        // RELACIÓN CON ALUMNOS (1 Contrato -> Muchos Estudiantes)
        public virtual ICollection<Estudiante> Estudiantes { get; set; }
        public int? TallerAsignadoId { get; set; } // El ID del taller (puede ser nulo si no se envió aun)
        public string? NombreTaller { get; set; } // Guardamos el nombre para mostrarlo fácil
        public string? EtapaProduccion { get; set; }
    }
}
