using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // <--- NUEVO USING
using Microsoft.EntityFrameworkCore;
using GestionSocios.Api.Models;

namespace GestionSocios.Api.Data
{
 
    public class ApplicationDbContext : IdentityDbContext
    {
    
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Socio> Socios { get; set; }
    }
}