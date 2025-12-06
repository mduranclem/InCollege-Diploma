using System;
using System.Collections.Generic;

namespace InCollege.Dominio.Modelos
{
    public class Contrato
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ColegioId { get; set; }
        public string NombreColegio { get; set; }

        public int ProductoId { get; set; }
        public string NombreProducto { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public int CantidadEgresados { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal PrecioContado { get; set; }

        public string OrigenContacto { get; set; }

        // PLAN DE PAGOS 
        public decimal MontoSeña { get; set; }      //  plata que ponen para congelar
        public int CantidadCuotas { get; set; }     // Ej: 6 cuotas
        public decimal MontoPorCuota { get; set; }  // Ej: $15.000 c/u
        public int Codigo { get; set; }

        public string Estado { get; set; } = "Presupuesto";
        public string VendedorAsignado { get; set; }
        public virtual ICollection<Estudiante> Estudiantes { get; set; }
    }
}

