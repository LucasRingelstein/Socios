using Microsoft.AspNetCore.Identity;

namespace GestionSocios.Api.Models
{
    // Heredar de IdentityUser permite que Socio sea el usuario del sistema
    public class Socio : IdentityUser
    {
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public DateTime? FechaDeNacimiento { get; set; }
        public int Edad { get; set; }
        public string? DNI { get; set; }
        public string? Domicilio { get; set; }
        public string? Actividad { get; set; }
        public string? Sexo { get; set; }
        public bool Activo { get; set; }
        public DateTime? FechaAlta { get; set; }
        public DateTime? FechaBaja { get; set; }
    }
}