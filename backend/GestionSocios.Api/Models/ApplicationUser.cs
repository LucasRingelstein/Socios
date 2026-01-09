using Microsoft.AspNetCore.Identity;

namespace GestionSocios.Api.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? DisplayName { get; set; }
    }
}
