using Microsoft.EntityFrameworkCore;
using Socios.Models;

namespace Socios.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<PaymentStatusLog> PaymentStatusLogs => Set<PaymentStatusLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Subscription>()
            .HasIndex(s => s.MercadoPagoPreapprovalId);
    }
}
