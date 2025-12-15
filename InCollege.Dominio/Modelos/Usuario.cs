using System;

namespace InCollege.Dominio.Modelos
{
    public class Usuario
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        // CORRECCIÓN: Agregamos '?' para permitir que venga vacío de la BD
        public string? Zona { get; set; }

        // CORRECCIÓN: Agregamos '?' para evitar el error de SqlNullValueException
        public DateTime? FechaAlta { get; set; } = DateTime.Now;

        public string Rol { get; set; } = "SinAsignar";

        public bool EstaActivo { get; set; } = false;

        public Usuario() { }

        public Usuario(string nombre, string apellido, string email, string password)
        {
            Nombre = nombre;
            Apellido = apellido;
            Email = email;
            Password = password;
            // Valores por defecto
            Rol = "SinAsignar";
            EstaActivo = false;
            FechaAlta = DateTime.Now;
        }
    }
}