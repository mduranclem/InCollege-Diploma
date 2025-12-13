using InCollege.Dominio.Modelos;
using System.Collections.Generic;

namespace InCollege.Web.Models
{
    public class GestionUsuariosViewModel
    {
        public List<Usuario> Pendientes { get; set; }
        public List<Usuario> Activos { get; set; }
    }
}