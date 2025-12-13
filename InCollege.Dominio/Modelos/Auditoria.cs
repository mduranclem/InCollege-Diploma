using System;
using System.ComponentModel.DataAnnotations;

namespace InCollege.Dominio.Modelos
{
    public class Auditoria
    {
        [Key]
        public int Id { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;

        // CORRECCIÓN: Debe llamarse igual que en la base de datos
        public string UsuarioEmail { get; set; }

        public string Accion { get; set; }
        public string Detalle { get; set; }

        // Agregamos Modulo por si la base de datos lo tiene de antes
        public string Modulo { get; set; }
    }
}