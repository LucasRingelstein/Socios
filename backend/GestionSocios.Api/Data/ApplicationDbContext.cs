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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // DNI único (recomendado)
            builder.Entity<Socio>()
                .HasIndex(s => s.DNI)
                .IsUnique();

            // Relación opcional Socio -> ApplicationUser
            builder.Entity<Socio>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Recomendación: limitar DNI (opcional)
            builder.Entity<Socio>()
                .Property(s => s.DNI)
                .HasMaxLength(9);
        }
    }
}
