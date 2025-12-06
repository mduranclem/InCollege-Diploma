using System.ComponentModel.DataAnnotations; // <--- 1. IMPORTANTE: Este using

namespace InCollege.Dominio.Patrones
{
    public abstract class ComponenteProducto
    {
        [Key] // <--- 2. IMPORTANTE: Esta etiqueta
        public int Id { get; set; } // <--- 3. IMPORTANTE: Esta propiedad pública

        public string Nombre { get; set; }

        public abstract decimal ObtenerPrecio();
        public abstract string MostrarDetalle(int nivel = 0);
    }
}