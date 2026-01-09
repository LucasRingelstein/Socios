namespace GestionSocios.Api.Models
{
    public class Socio
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string DNI { get; set; } = "";

        public DateTime? FechaNacimiento { get; set; }
        public string? Domicilio { get; set; }
        public string? Actividad { get; set; }
        public string? Sexo { get; set; }

        public bool Activo { get; set; } = true;
        public DateTime FechaAlta { get; set; } = DateTime.UtcNow;
        public DateTime? FechaBaja { get; set; }

        // vínculo opcional a cuenta (AspNetUsers)
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
