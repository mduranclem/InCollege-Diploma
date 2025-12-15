using System;

namespace InCollege.Dominio.Modelos
{
    public class Colegio
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nombre { get; set; } // Ej: "Instituto San Martín"
        public string Direccion { get; set; }
        public string ContactoPrincipal { get; set; } // Nombre de la madre/padre referente
        public string Email { get; set; }
        public string Zona { get; set; }
        public string VendedorResponsable { get; set; }
        public string Telefono { get; set; }
        public string Ciudad { get; set; }

        // Constructor vacío
        public Colegio() { }

        public Colegio(string nombre, string contacto, string email)
        {
            Nombre = nombre;
            ContactoPrincipal = contacto;
            Email = email;
        }
    }
}
