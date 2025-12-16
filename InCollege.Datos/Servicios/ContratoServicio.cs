using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using InCollege.Dominio.Modelos;

namespace InCollege.Datos.Servicios
{
    public class ContratoServicio
    {
        private readonly AppDbContext _context;

        public ContratoServicio(AppDbContext context)
        {
            _context = context;
        }

        public List<Contrato> ObtenerPendientesDeDiseno()
        {
            return _context.Contratos
                           .Include(c => c.Colegio) // <--- ESTO DA ERROR SI NO HACEMOS EL PASO 3
                           .Where(c => c.EstadoDiseno == EstadoDiseno.Pendiente)
                           .OrderByDescending(c => c.FechaCreacion)
                           .ToList();
        }

        public Contrato ObtenerPorId(int codigo)
        {
            return _context.Contratos
                           .Include(c => c.Colegio)
                           .FirstOrDefault(c => c.Codigo == codigo);
        }

        public void RegistrarDiseno(int codigo, string urlImagen)
        {
            var contrato = _context.Contratos.FirstOrDefault(c => c.Codigo == codigo);
            if (contrato != null)
            {
                contrato.UrlImagenDiseno = urlImagen;
                contrato.EstadoDiseno = EstadoDiseno.Aprobado;
                _context.SaveChanges();
            }
        }
        // Trae TODOS los contratos (pendientes y listos) para el buscador
        public List<Contrato> ObtenerTodos()
        {
            return _context.Contratos
                           .Include(c => c.Colegio)
                           .OrderByDescending(c => c.FechaCreacion)
                           .ToList();
        }
    }
}