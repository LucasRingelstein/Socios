using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GestionSocios.Api.Models;

namespace GestionSocios.Api.Data
{
    public class ApplicationDbContext : IdentityDbContext<Socio>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Socio> Socios { get; set; }
    }

}