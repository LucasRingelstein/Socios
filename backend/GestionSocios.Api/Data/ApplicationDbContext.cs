using GestionSocios.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GestionSocios.Api.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Socio> Socios => Set<Socio>();

        // --- AGREGA ESTA LÍNEA ---
        public DbSet<Cuota> Cuotas => Set<Cuota>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // --- CONFIGURACIÓN DE SOCIOS ---
            builder.Entity<Socio>()
                .HasIndex(s => s.DNI)
                .IsUnique();

            builder.Entity<Socio>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Socio>()
                .Property(s => s.DNI)
                .HasMaxLength(9);

            // --- (OPCIONAL) CONFIGURACIÓN DE CUOTAS ---
            // Entity Framework ya es inteligente y entiende la relación por el modelo,
            // pero si querés ser explícito, podrías agregar esto para borrar en cascada:
            builder.Entity<Cuota>()
                .HasOne(c => c.Socio)
                .WithMany() // Un socio tiene muchas cuotas
                .HasForeignKey(c => c.SocioId)
                .OnDelete(DeleteBehavior.Cascade); // Si borrás al socio, se borran sus cuotas
           
        }
    }
}