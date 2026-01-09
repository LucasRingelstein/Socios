using System.ComponentModel.DataAnnotations;

namespace GestionSocios.DTOs
{
    public class CrearSocioDto
    {
        [Required] public string Nombre { get; set; } = "";
        [Required] public string Apellido { get; set; } = "";
        [Required, RegularExpression(@"^[0-9]{7,9}$")] public string DNI { get; set; } = "";

        public DateTime? FechaNacimiento { get; set; }
        public string? Domicilio { get; set; }
        public string? Actividad { get; set; }
        public string? Sexo { get; set; }
        public bool Activo { get; set; } = true;
    }
}
