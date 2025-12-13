using InCollege.Dominio.Modelos;
using System;

namespace InCollege.Dominio.Servicios
{
    public static class AuditoriaFactory
    {
        public static Auditoria Crear(string usuario, string accion, string detalle)
        {
            return new Auditoria
            {
                Fecha = DateTime.Now,

                // IMPORTANTE: Asignamos a UsuarioEmail
                UsuarioEmail = usuario,

                Accion = accion,
                Detalle = detalle,
                Modulo = "SISTEMA"
            };
        }
    }
}