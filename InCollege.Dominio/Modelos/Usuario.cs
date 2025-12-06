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

        // NUEVO: Define qué permisos tiene (Admin, Estudiante, Invitado)
        public string Rol { get; set; } = "SinAsignar";

        // NUEVO: Define si puede entrar o no.
        // false = Esperando aprobación. true = Puede entrar.
        public bool EstaActivo { get; set; } = false;

        public Usuario() { }

        public Usuario(string nombre, string apellido, string email, string password)
        {
            Nombre = nombre;
            Apellido = apellido;
            Email = email;
            Password = password;
            // Por defecto, todo nuevo usuario nace inactivo y sin rol
            Rol = "SinAsignar";
            EstaActivo = false;
        }
    }
}
