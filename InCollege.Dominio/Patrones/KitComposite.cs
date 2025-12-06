using System.Collections.Generic;
using System.Linq;

namespace InCollege.Dominio.Patrones
{
    // COMPOSITE (Compuesto)
    // Contiene una lista de Componentes (Hojas u otros Compuestos)
    public class KitComposite : ComponenteProducto
    {
        // Esta lista es la clave del patrón: guarda "Componentes", no solo "Prendas".
        // Nota: Para que EF Core guarde la relación, mapearemos esto más adelante o usaremos esta lista privada.
        private List<ComponenteProducto> _elementos = new List<ComponenteProducto>();

        // Puede tener un descuento por ser combo (ej. 10% off)
        public decimal DescuentoPorCombo { get; set; } = 0;

        // --- CORRECCIÓN: CONSTRUCTOR VACÍO OBLIGATORIO PARA EF CORE ---
        public KitComposite() { }
        // --------------------------------------------------------------

        public KitComposite(string nombre, decimal descuento = 0)
        {
            Nombre = nombre;
            DescuentoPorCombo = descuento;
        }

        public void Agregar(ComponenteProducto componente)
        {
            _elementos.Add(componente);
        }

        public void Quitar(ComponenteProducto componente)
        {
            _elementos.Remove(componente);
        }

        public override decimal ObtenerPrecio()
        {
            decimal total = 0;

            // Magia del patrón: Recorremos la lista sin saber si son prendas o kits.
            // Simplemente les pedimos su precio.
            foreach (var item in _elementos)
            {
                total += item.ObtenerPrecio();
            }

            // Aplicamos descuento si existe
            return total - (total * DescuentoPorCombo / 100);
        }

        public override string MostrarDetalle(int nivel = 0)
        {
            string indentacion = new string('-', nivel * 2);
            string detalle = $"{indentacion} PACK: {Nombre} (Descuento {DescuentoPorCombo}%)\n";

            foreach (var item in _elementos)
            {
                // Delegamos la impresión a los hijos (recursividad)
                detalle += item.MostrarDetalle(nivel + 1);
            }
            return detalle;
        }
    }
}