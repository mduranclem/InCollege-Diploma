using System;
using System.ComponentModel.DataAnnotations;

namespace InCollege.Dominio.Modelos
{
    public class Auditoria
    {
        [Key]
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        // GUARDAMOS SOLO EL EMAIL (TEXTO)
        // Esto evita errores de Foreign Keys y conflictos de ID (Int vs Guid)
        public string UsuarioEmail { get; set; }

        public string Accion { get; set; }
        public string Detalle { get; set; }
        public string Modulo { get; set; }

        public Auditoria() { }
    }
}