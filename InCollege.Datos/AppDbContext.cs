using InCollege.Dominio.Modelos;
using InCollege.Dominio.Patrones;
using Microsoft.EntityFrameworkCore;

namespace InCollege.Datos
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Colegio> Colegios { get; set; }
        public DbSet<Contrato> Contratos { get; set; }
        public DbSet<ComponenteProducto> Productos { get; set; }
        public DbSet<Auditoria> Auditorias { get; set; }
        public DbSet<Estudiante> Estudiantes { get; set; }
        public DbSet<Pago> Pagos { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración previa que ya tenías...
            modelBuilder.Entity<ComponenteProducto>().HasKey(p => p.Id);
            modelBuilder.Entity<PrendaIndividual>();
            modelBuilder.Entity<KitComposite>();

            // ---  BORRADO EN CASCADA ---

            // 1. Si borro un COLEGIO -> Se borran los CONTRATOS
            modelBuilder.Entity<Contrato>()
                .HasOne<Colegio>()           // Un contrato tiene un colegio
                .WithMany()                  // Un colegio tiene muchos contratos
                .HasForeignKey(c => c.ColegioId)
                .OnDelete(DeleteBehavior.Cascade); // <--- LA CLAVE MÁGICA

            // 2. Si se borra un CONTRATO -> Se borran los ESTUDIANTES
            modelBuilder.Entity<Estudiante>()
                .HasOne(e => e.Contrato)
                .WithMany(c => c.Estudiantes) // <--- AQUI ESTA LA CLAVE
                .HasForeignKey(e => e.ContratoId)
                .OnDelete(DeleteBehavior.Cascade);

            // 3. Si se borra un CONTRATO -> Se borran los PAGOS

            modelBuilder.Entity<Pago>()
                .HasOne(p => p.Estudiante) // Relación con Estudiante
                .WithMany()
                .HasForeignKey(p => p.EstudianteId)
                .OnDelete(DeleteBehavior.NoAction); // <--- CAMBIO CLAVE: NO BORRAR EN CASCADA POR ACÁ
        }
    }
}