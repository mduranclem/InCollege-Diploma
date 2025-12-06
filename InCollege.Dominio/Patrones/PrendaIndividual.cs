namespace InCollege.Dominio.Patrones
{
    // LEAF (Hoja)
    // Es el objeto final, no tiene hijos.
    public class PrendaIndividual : ComponenteProducto
    {
        public decimal PrecioBase { get; set; }
        public string Tela { get; set; }

        // --- CORRECCIÓN: CONSTRUCTOR VACÍO OBLIGATORIO PARA EF CORE ---
        public PrendaIndividual() { }
        // --------------------------------------------------------------

        public PrendaIndividual(string nombre, decimal precio, string tela)
        {
            Nombre = nombre;
            PrecioBase = precio;
            Tela = tela;
        }

        public override decimal ObtenerPrecio()
        {
            // La hoja simplemente devuelve su precio
            return PrecioBase;
        }

        public override string MostrarDetalle(int nivel = 0)
        {
            string indentacion = new string('-', nivel * 2);
            return $"{indentacion} PRENDA: {Nombre} ({Tela}) - ${PrecioBase}\n";
        }
    }
}